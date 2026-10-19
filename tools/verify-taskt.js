'use strict';
const assert = require('assert');
const fs = require('fs');
const os = require('os');
const path = require('path');
const { EventEmitter } = require('events');
const { spawnSync } = require('child_process');
const { createTasktService } = require('../taskt');
const root = fs.mkdtempSync(path.join(os.tmpdir(), 'petai-taskt-test-'));
const executable = path.resolve(__dirname, '../taskt-runtime/taskt.exe');
const xml = command => `<?xml version="1.0"?><Script xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"><Commands>${command ? `<ScriptAction><ScriptCommand xsi:type="${command.type}" CommandName="${command.type}" CommandEnabled="true" LineNumber="1" ${command.attributes || ''}/></ScriptAction>` : ''}</Commands><Variables/><Info/></Script>`;
const wait = ms => new Promise(resolve => setTimeout(resolve, ms));
async function actual() {
  const check = spawnSync(executable, ['--petai-check'], { shell: false, cwd: root, encoding: 'utf8' });
  assert.strictEqual(check.status, 0, check.stderr);
  console.log('--petai-check passed');
  const service = createTasktService({ userData: root, executable });
  for (const [name, command, expected] of [
    ['success', null, 'succeeded'],
    ['failure', { type: 'ThrowExceptionCommand', attributes: 'v_Message="intentional-petai-test-failure"' }, 'failed'],
    ['cancel', { type: 'PauseScriptCommand', attributes: 'v_PauseLength="2500"' }, 'cancelled']
  ]) {
    const scriptPath = path.join(root, name + '.xml');
    fs.writeFileSync(scriptPath, xml(command));
    const before = fs.readFileSync(scriptPath, 'utf8');
    const registered = await service.register({ scriptPath, name, description: 'No desktop input' });
    assert.strictEqual(registered.ok, true, JSON.stringify(registered));
    assert.strictEqual(fs.readFileSync(scriptPath, 'utf8'), before, 'validation modified XML');
    const running = service.run({ taskId: registered.task.id });
    if (name === 'cancel') {
      await wait(1200);
      const stop = await service.stop();
      assert.strictEqual(stop.cancelConfirmed, false);
    }
    const result = await running;
    console.log(name, JSON.stringify(result));
    assert.strictEqual(result.status, expected);
    if (name === 'cancel') assert.strictEqual(result.cancelConfirmed, true);
    if (name === 'failure') assert.match(result.error, /intentional-petai-test-failure/);
    while (service.getState().mode !== 'idle') await wait(100);
  }
  const invalid = path.join(root, 'invalid.xml');
  fs.writeFileSync(invalid, '<!DOCTYPE Script [<!ENTITY x SYSTEM "file:///C:/secret">]><Script/>');
  assert.strictEqual((await service.register({ scriptPath: invalid, name: 'invalid', description: '' })).ok, false);
  assert.strictEqual((await service.run({ taskId: 'unknown', scriptPath: invalid })).ok, false);
  console.log('read-only validation / DTD rejection / taskId restriction passed');
}
async function mocks() {
  let mode = 'success';
  let lastChild;
  function fakeSpawn(file, args, options) {
    assert.strictEqual(options.shell, false);
    assert.ok(options.cwd.startsWith(root));
    const request = JSON.parse(fs.readFileSync(args[1], 'utf8'));
    const child = new EventEmitter();
    lastChild = child;
    const write = result => fs.writeFileSync(request.responseFile, JSON.stringify({ id: request.id, ...result }));
    if (args[0] === '--petai-validate') setTimeout(() => { write({ ok: true, status: 'validated' }); child.emit('exit', 0); }, 10);
    else if (mode === 'success') setTimeout(() => { write({ ok: true, status: 'succeeded', engineResult: 'Successful' }); child.emit('exit', 0); }, 10);
    else if (mode === 'exit') setTimeout(() => child.emit('exit', 0), 10);
    else {
      const timer = setInterval(() => {
        if (!fs.existsSync(path.join(path.dirname(args[1]), 'cancel.json'))) return;
        clearInterval(timer);
        setTimeout(() => { write({ ok: false, status: 'cancelled', cancelConfirmed: true }); child.emit('exit', 0); }, mode === 'delayed' ? 250 : 10);
      }, 10);
    }
    return child;
  }
  const states = [];
  const service = createTasktService({ userData: path.join(root, 'mock'), executable, spawnProcess: fakeSpawn, timeoutMs: 150, cancelGraceMs: 50, onState: state => states.push(state) });
  const registered = await service.register({ scriptPath: path.join(root, 'success.xml'), name: 'mock', description: '' });
  const taskId = registered.task.id;
  assert.strictEqual((await service.run({ taskId })).status, 'succeeded');
  mode = 'exit';
  assert.strictEqual((await service.run({ taskId })).status, 'bridge_error');
  mode = 'delayed';
  const result = await service.run({ taskId });
  assert.strictEqual(result.status, 'timeout');
  assert.strictEqual(result.cancelConfirmed, false);
  assert.strictEqual(service.getState().mode, 'stopping');
  assert.strictEqual((await service.run({ taskId })).ok, false);
  await wait(400);
  assert.strictEqual(service.getState().mode, 'idle');
  assert.strictEqual(service.getState().cancelConfirmed, true);
  assert.ok(states.some(state => state.status === 'timeout'));
  assert.ok(lastChild);
  console.log('Node mocks: completion / zero-exit without result / timeout cancel handshake / late confirmation passed');
}
async function mainRunner() {
  const source = fs.readFileSync(path.join(__dirname, '../main.js'), 'utf8');
  const start = source.indexOf('async function runAutomationTask(options) {');
  const end = source.indexOf("\nipcMain.handle('automation:list'", start);
  assert.ok(start >= 0 && end > start, 'main runner not found');
  const window = visible => ({
    visible, hidden: 0, restored: 0,
    isDestroyed() { return false; },
    isVisible() { return this.visible; },
    hide() { this.visible = false; this.hidden++; },
    showInactive() { this.visible = true; this.restored++; }
  });
  const settings = window(true), chat = window(true), pet = window(true);
  let releaseList, releaseRun, notifyRun, runCalls = 0;
  const started = new Promise(resolve => { notifyRun = resolve; });
  const context = {
    automationDisposed: false, automationRunning: false,
    settingsWin: settings, chatWin: chat, win: pet,
    dialog: { showMessageBox() { assert.fail('Taskt run must not request confirmation'); } },
    automation: {
      getState: () => ({ mode: 'idle' }),
      list: () => new Promise(resolve => { releaseList = resolve; }),
      run: options => {
        assert.deepStrictEqual(Object.keys(options), ['taskId']);
        assert.strictEqual(options.taskId, 'registered');
        runCalls++;
        return new Promise(resolve => { releaseRun = resolve; notifyRun(); });
      }
    }
  };
  require('vm').createContext(context);
  require('vm').runInContext(source.slice(start, end), context);
  const run = context.runAutomationTask;
  assert.strictEqual((await run({ taskId: 'registered', scriptPath: 'other.xml' })).ok, false);
  const pending = run({ taskId: 'registered' });
  assert.strictEqual(context.automationRunning, true);
  assert.strictEqual((await run({ taskId: 'registered' })).ok, false);
  releaseList({ ok: true, tasks: [{ id: 'registered' }] });
  await started;
  assert.strictEqual(runCalls, 1);
  assert.strictEqual(settings.hidden, 1);
  assert.strictEqual(chat.hidden, 1);
  assert.strictEqual(pet.hidden, 0);
  assert.strictEqual((await run({ taskId: 'registered' })).ok, false);
  releaseRun({ ok: true });
  assert.strictEqual((await pending).ok, true);
  assert.strictEqual(settings.restored, 1);
  assert.strictEqual(chat.restored, 1);
  assert.strictEqual(context.automationRunning, false);
  context.automation.list = async () => ({ ok: true, tasks: [{ id: 'registered' }] });
  assert.strictEqual((await run({ taskId: 'unknown' })).message, '任务不存在');
  assert.strictEqual(runCalls, 1);
  assert.strictEqual(context.automationRunning, false);
  context.automation.run = async () => { throw new Error('run failed'); };
  await assert.rejects(run({ taskId: 'registered' }), /run failed/);
  assert.strictEqual(settings.restored, 2);
  assert.strictEqual(chat.restored, 2);
  assert.strictEqual(context.automationRunning, false);
  context.automation.getState = () => ({ mode: 'stopping' });
  assert.strictEqual((await run({ taskId: 'registered' })).ok, false);
  console.log('Main runner: direct execution / registered task validation / concurrency / window restoration passed');
}
(async () => {
  try { await mainRunner(); await actual(); await mocks(); console.log('Taskt verification passed'); }
  finally { fs.rmSync(root, { recursive: true, force: true }); }
})().catch(error => { console.error(error); process.exitCode = 1; });
