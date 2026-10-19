'use strict';

const assert = require('assert');
const fs = require('fs');
const os = require('os');
const path = require('path');
const { EventEmitter } = require('events');
const { PassThrough, Writable } = require('stream');
const { randomUUID } = require('crypto');
const { createAutomationService, createRunner } = require('../automation');

const click = { type: 'click', x: 0.5, y: 0.5, button: 0, target: { process: 'fixture', title: 'Fixture', automationId: 'button', name: 'Button', controlType: 50000 } };
const tick = () => new Promise(resolve => setImmediate(resolve));
function deferred() { let resolve, reject; const promise = new Promise((yes, no) => { resolve = yes; reject = no; }); return { promise, resolve, reject }; }
async function taskFixture(dir, count = 1) {
  const tasks = Array.from({ length: count }, (_, i) => ({ id: randomUUID(), name: 'Fixture ' + i, description: '', createdAt: new Date().toISOString(), steps: [click] }));
  await fs.promises.mkdir(dir, { recursive: true });
  await fs.promises.writeFile(path.join(dir, 'automation-tasks.json'), JSON.stringify(tasks));
  return tasks;
}

async function verifyRaces(dir) {
  const tasks = await taskFixture(path.join(dir, 'races'), 3);
  const runs = [deferred(), deferred()];
  const stopAck = deferred();
  const events = [];
  let runners = 0, killed = 0;
  const service = createAutomationService({ userData: path.join(dir, 'races'), runnerFactory: notify => {
    events.push(notify);
    const index = runners++;
    return { request: command => command === 'run' ? runs[index].promise : stopAck.promise, kill: () => { killed++; }, dispose: async () => {} };
  } });
  try {
    // Updating from a stale snapshot used to resurrect the first deleted task.
    const deleted = await Promise.all([service.remove(tasks[1].id), service.remove(tasks[2].id)]);
    assert(deleted.every(result => result.ok));
    assert.deepStrictEqual((await service.list()).tasks.map(t => t.id), [tasks[0].id]);
    const first = service.run({ taskId: tasks[0].id });
    await tick();
    events[0]({ event: 'progress', completedSteps: 0, totalSteps: 1, message: 'waiting' });
    const stop = service.stop();
    await tick();
    assert.strictEqual(killed, 0, 'must wait for helper to release pressed keys before killing');
    assert.strictEqual(service.getState().mode, 'stopping');
    stopAck.resolve({ ok: true });
    assert.strictEqual((await stop).ok, true);
    assert.strictEqual(killed, 1);
    const second = service.run({ taskId: tasks[0].id });
    await tick();
    assert.strictEqual(runners, 2, 'a stopped helper must never be reused');
    assert.strictEqual(service.getState().progress, null);
    runs[0].resolve({ ok: false, message: 'old failure', reasonCode: 'old' });
    await first;
    events[0]({ event: 'progress', completedSteps: 99, totalSteps: 99 });
    events[0]({ event: 'error', message: 'old exit' });
    assert.strictEqual(service.getState().mode, 'running', 'stale completion must not idle a later run');
    assert.strictEqual(service.getState().message, '正在运行；F9紧急停止');
    runs[1].resolve({ ok: false, message: 'target missing', reasonCode: 'target_not_found', failedStep: 1, completedSteps: 0, totalSteps: 1 });
    await second;
    assert.strictEqual(service.getState().ok, false);
    assert.strictEqual(service.getState().reasonCode, 'target_not_found');
    assert.strictEqual(service.getState().failedStep, 1);
    assert.strictEqual(service.getState().totalSteps, 1);
  } finally { await service.dispose(); }

  const starting = deferred();
  const cancelled = createAutomationService({ userData: path.join(dir, 'start-race'), runnerFactory: () => ({ request: command => command === 'startRecording' ? starting.promise : Promise.resolve({ ok: true }), kill: () => {}, dispose: async () => {} }) });
  try {
    const start = cancelled.startRecording({ name: 'cancel during startup' });
    await cancelled.list();
    assert.strictEqual(cancelled.getState().mode, 'starting');
    await cancelled.stop();
    starting.resolve({ ok: true });
    assert.strictEqual((await start).ok, false);
    assert.strictEqual(cancelled.getState().mode, 'idle', 'late startup must not resume a cancelled recording');
    assert.strictEqual((await cancelled.list()).tasks.length, 0);
  } finally { await cancelled.dispose(); }

  let finishEvent;
  const recordingService = createAutomationService({ userData: path.join(dir, 'hotkey'), runnerFactory: notify => { finishEvent = notify; return { request: async () => ({ ok: true }), dispose: async () => {} }; } });
  try {
    await recordingService.startRecording({ name: 'hotkey save' });
    const finished = { event: 'recordingFinished', ok: true, steps: [click] };
    finishEvent(finished); finishEvent(finished);
    const saved = await recordingService.stopRecording();
    assert.strictEqual(saved.ok, true);
    assert.strictEqual((await recordingService.list()).tasks.length, 1, 'duplicate finish events must save only one task');
    assert.deepStrictEqual((await recordingService.list()).tasks[0].stepsSummary, [{ type: 'click', process: 'fixture', title: 'Fixture', name: 'Button' }]);
  } finally { await recordingService.dispose(); }
}

async function verifyRunnerProtocol() {
  if (process.platform !== 'win32') return;
  const timers = new Set(), messages = [], events = [];
  const child = new EventEmitter();
  child.stdout = new PassThrough(); child.stderr = new PassThrough();
  child.stdin = new Writable({ write(chunk, encoding, callback) { messages.push(JSON.parse(String(chunk))); callback(); } });
  let kills = 0;
  child.kill = () => { kills++; };
  const runner = createRunner(event => events.push(event), () => {}, {
    spawnProcess: () => child,
    setTimer: (callback, milliseconds) => { const timer = { callback, milliseconds }; timers.add(timer); return timer; },
    clearTimer: timer => timers.delete(timer)
  });
  try {
    const run = runner.request('run', { steps: [click], inputs: {} });
    const firstTimer = [...timers][0];
    assert.strictEqual(firstTimer.milliseconds, 30000, 'normal replay must not use the 5-second command deadline');
    child.stdout.write(JSON.stringify({ event: 'progress', completedSteps: 1, totalSteps: 10 }) + '\n');
    assert.strictEqual(timers.has(firstTimer), false, 'each completed step must extend the inactivity deadline');
    assert.strictEqual(timers.size, 1);
    child.stdout.write(JSON.stringify({ id: messages[0].id, ok: true, completedSteps: 10 }) + '\n');
    assert.strictEqual((await run).ok, true);
    assert.strictEqual(timers.size, 0);
    assert.strictEqual(kills, 0);
    const stalled = runner.request('run', { steps: [click], inputs: {} });
    const rejected = assert.rejects(stalled, error => error.reasonCode === 'helper_timeout');
    [...timers][0].callback();
    await rejected;
    assert.strictEqual(kills, 1);
    assert.strictEqual(events.at(-1).reasonCode, 'helper_timeout');
    assert.strictEqual(timers.size, 0);
  } finally { await runner.dispose(); child.stdin.destroy(); child.stdout.destroy(); child.stderr.destroy(); }
}

async function main() {
  const dir = await fs.promises.mkdtemp(path.join(os.tmpdir(), 'pet-automation-'));
  let runnerCalls = [];
  let stateEvents = [];
  const runnerFactory = () => ({
    request: async (command, args) => {
        runnerCalls.push(command);
      if (command === 'startRecording') return { ok: true };
      if (command === 'stopRecording') return { ok: true, steps: [{ type: 'input', parameter: 'input_1', target: { process: 'app', title: 'Window', automationId: 'secret-field', name: '', controlType: 50004, password: true } }] };
      if (command === 'run') return { ok: true, message: 'done', completedSteps: 1 };
      if (command === 'stop') return { ok: true };
      return { ok: false, message: 'unknown' };
    },
    dispose: async () => {}
  });
  try {
    const service = createAutomationService({ userData: dir, runnerFactory, onState: value => stateEvents.push(value) });
    const started = await service.startRecording({ name: 'Login', description: 'test' });
    assert.strictEqual(started.ok, true);
    assert.deepStrictEqual(runnerCalls, ['startRecording']);
    const stopped = await service.stopRecording();
    assert.strictEqual(stopped.ok, true);
    const listed = await service.list();
    assert.strictEqual(listed.tasks.length, 1);
    assert.strictEqual(listed.tasks[0].stepsCount, 1);
    const saved = await fs.promises.readFile(path.join(dir, 'automation-tasks.json'), 'utf8');
    assert(!saved.includes('pwdvalue'));
    const missing = await service.run({ taskId: listed.tasks[0].id, inputs: {} });
    assert.strictEqual(missing.ok, false);
    assert.strictEqual(runnerCalls.includes('run'), false);
    const executed = await service.run({ taskId: listed.tasks[0].id, inputs: { input_1: 'pwdvalue' } });
    assert.strictEqual(executed.ok, true);
    assert.strictEqual(stateEvents.some(event => event.progress), false);
    assert(!fs.readFileSync(path.join(dir, 'automation-tasks.json'), 'utf8').includes('pwdvalue'));
    const removed = await service.remove(listed.tasks[0].id);
    assert.strictEqual(removed.ok, true);
    await service.dispose();

    const timeoutService = createAutomationService({ userData: path.join(dir, 'timeout'), runnerFactory: () => ({
      request: async command => command === 'startRecording' ? { ok: true } : Promise.reject(new Error('request timed out')),
      dispose: async () => {}
    }) });
    await timeoutService.startRecording({ name: 'timeout' });
    const timeoutResult = await timeoutService.stopRecording();
    assert.strictEqual(timeoutResult.ok, false);
    assert.match(timeoutResult.message, /timed out/i);
    assert.strictEqual(timeoutService.getState().mode, 'idle');
    await timeoutService.dispose();

    const cancelled = createAutomationService({ userData: path.join(dir, 'cancel'), runnerFactory: () => ({
      request: async command => command === 'startRecording' ? { ok: true } : { ok: false, message: 'cancelled' }, dispose: async () => {}
    }) });
    await cancelled.startRecording({ name: 'cancel' });
    const cancelResult = await cancelled.stop();
    assert.strictEqual(cancelResult.ok, false);
    assert.strictEqual(cancelled.getState().mode, 'idle');
    assert.strictEqual(cancelled.getState().ok, false);
    assert.strictEqual(cancelled.getState().message, 'cancelled', 'stop failure must not be overwritten by a success message');
    await cancelled.dispose();
    let notify;
    const parent = { automationId: '', name: 'Fixture parent', controlType: 50026 };
    const partialService = createAutomationService({ userData: path.join(dir, 'partial'), runnerFactory: event => {
      notify = event;
      return { request: async command => command === 'startRecording' ? { ok: true } : { ok: true, partial: true, steps: [{ type: 'click', x: 0.5, y: 0.5, button: 0, target: { process: 'fixture', title: 'Fixture', automationId: '', name: 'Duplicate', controlType: 50000, parents: [parent] } }] }, dispose: async () => {} };
    } });
    await partialService.startRecording({ name: 'partial' });
    notify({ event: 'recordingPaused', reasonCode: 'element_unavailable', retainedSteps: 1, message: '此动作未录入；F8仅保存此前步骤' });
    assert.strictEqual(partialService.getState().mode, 'recording');
    assert.strictEqual(partialService.getState().reasonCode, 'element_unavailable');
    assert.strictEqual((await partialService.list()).tasks.length, 0);
    const partialResult = await partialService.stopRecording();
    assert.strictEqual(partialResult.partial, true);
    assert.match(partialResult.message, /流程不完整/);
    const partialSaved = JSON.parse(await fs.promises.readFile(path.join(dir, 'partial', 'automation-tasks.json'), 'utf8'));
    assert.deepStrictEqual(partialSaved[0].steps[0].target.parents, [parent]);
    await partialService.dispose();
    await verifyRaces(dir);
    await verifyRunnerProtocol();
    console.log('automation service verification passed');
  } finally { await fs.promises.rm(dir, { recursive: true, force: true }); }
}

main().catch(error => { console.error(error); process.exitCode = 1; });
