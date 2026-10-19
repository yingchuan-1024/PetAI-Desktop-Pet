'use strict';

const fs = require('fs');
const path = require('path');
const { spawn } = require('child_process');
const { randomUUID } = require('crypto');
const readline = require('readline');

const ID = /^[0-9a-f]{8}-[0-9a-f]{4}-4[0-9a-f]{3}-[89ab][0-9a-f]{3}-[0-9a-f]{12}$/;
const fail = message => ({ ok: false, message });
const text = (value, max) => typeof value === 'string' ? value.trim().slice(0, max) : '';

// Whitelist the persisted protocol: never persist helper diagnostics or input values.
function cleanSteps(steps) {
  if (!Array.isArray(steps) || !steps.length || steps.length > 10000) throw new Error('录制步骤为空或超过限制');
  return steps.map(step => {
    if (!step || !['click', 'key', 'wheel', 'input'].includes(step.type)) throw new Error('无效动作');
    const t = step.target;
    if (!t || !t.process || !t.title || !Number.isInteger(t.controlType) || t.controlType < 50000 || t.controlType > 50100 || (!t.automationId && !t.name)) throw new Error('控件无法可靠识别');
    const target = { process: text(t.process, 256), title: text(t.title, 1024), automationId: text(t.automationId, 512), name: text(t.name, 1024), controlType: t.controlType, password: t.password === true };
    if (!target.process || !target.title || (!target.automationId && !target.name)) throw new Error('控件无法可靠识别');
    if (t.parents !== undefined) {
      if (!Array.isArray(t.parents) || t.parents.length > 64) throw new Error('无效 UIA 父链');
      target.parents = t.parents.map(p => {
        if (!p || !Number.isInteger(p.controlType) || p.controlType < 50000 || p.controlType > 50100) throw new Error('无效 UIA 父链');
        return { automationId: text(p.automationId, 512), name: text(p.name, 1024), controlType: p.controlType };
      });
    }
    const result = { type: step.type, target };
    if (step.type === 'input') {
      if (!/^input_[1-9][0-9]{0,3}$/.test(step.parameter)) throw new Error('无效输入参数');
      result.parameter = step.parameter;
    } else if (step.type === 'key') {
      if (target.password) throw new Error('禁止保存密码按键');
      if (!Number.isInteger(step.key) || step.key < 1 || step.key > 254 || typeof step.down !== 'boolean') throw new Error('无效按键');
      result.key = step.key;
      result.down = step.down;
    } else if (step.type === 'wheel') {
      if (!Number.isInteger(step.delta) || !step.delta || Math.abs(step.delta) > 12000) throw new Error('无效滚轮');
      result.delta = step.delta;
    } else {
      if (![step.x, step.y].every(v => typeof v === 'number' && Number.isFinite(v) && v >= 0 && v <= 1)) throw new Error('无效控件相对坐标');
      if (![0, 1, 2].includes(step.button)) throw new Error('无效鼠标按钮');
      result.x = step.x; result.y = step.y; result.button = step.button;
    }
    return result;
  });
}

function createAutomationService({ userData, onState = () => {}, runnerFactory } = {}) {
  if (typeof userData !== 'string' || !path.isAbsolute(userData)) throw new Error('userData 必须是绝对路径');
  const file = path.join(userData, 'automation-tasks.json');
  const cleared = { ok: null, reasonCode: null, failedStep: null, completedSteps: 0, totalSteps: 0, progress: null, retainedSteps: 0 };
  let tasks = [], state = { ...cleared, mode: 'idle', message: '', taskId: null }, runner, recording, disposed = false;
  let persistence = Promise.resolve(), activeOperation = null, stopping = null, runnerGeneration = 0;
  const ready = fs.promises.readFile(file, 'utf8').then(raw => {
    const data = JSON.parse(raw);
    if (!Array.isArray(data)) throw new Error('任务文件格式无效');
    tasks = data.map(t => {
      if (!t || !ID.test(t.id) || !text(t.name, 120) || !Number.isFinite(Date.parse(t.createdAt))) throw new Error('任务文件包含无效任务');
      return { id: t.id, name: text(t.name, 120), description: text(t.description, 2000), createdAt: t.createdAt, partial: t.partial === true, steps: cleanSteps(t.steps) };
    });
    if (new Set(tasks.map(t => t.id)).size !== tasks.length) throw new Error('任务ID重复');
  }).catch(error => { if (error.code !== 'ENOENT') { state.message = '任务文件读取失败：' + error.message; state.ok = false; throw error; } });
  ready.catch(() => {});
  function setState(next) { state = { ...state, ...next }; try { onState({ ...state }); } catch (_) {} }
  function current(token) { return !disposed && activeOperation === token; }
  function resultState(result) {
    const next = { ok: result.ok === true, message: result.message || (result.ok ? '操作完成' : '自动化操作失败'), reasonCode: result.reasonCode || null, failedStep: null };
    for (const field of ['failedStep', 'completedSteps', 'totalSteps']) if (Number.isInteger(result[field]) && result[field] >= 0) next[field] = result[field];
    return next;
  }
  function complete(token, result) {
    if (current(token)) { activeOperation = null; recording = null; setState({ mode: 'idle', taskId: null, ...resultState(result) }); }
    return result;
  }
  function failure(error) { return { ok: false, message: error.message || String(error), reasonCode: error.reasonCode || 'helper_error' }; }
  // Compute each update from the last committed task set, even for simultaneous deletes.
  function persist(update, allowed = () => !disposed) {
    const pending = persistence.then(async () => {
      if (!allowed()) throw new Error('操作已取消');
      const next = update(tasks);
      await fs.promises.mkdir(userData, { recursive: true });
      const temp = file + '.' + randomUUID() + '.tmp';
      try {
        await fs.promises.writeFile(temp, JSON.stringify(next, null, 2), { encoding: 'utf8', mode: 0o600 });
        if (!allowed()) throw new Error('操作已取消');
        await fs.promises.rename(temp, file); tasks = next;
      } finally { await fs.promises.unlink(temp).catch(() => {}); }
    });
    persistence = pending.catch(() => {});
    return pending;
  }
  function finishRecording(result, session = recording) {
    if (!session || !current(session.token)) return Promise.resolve(fail('录制已取消'));
    if (session.finishing) return session.finishing;
    setState({ mode: 'stopping', message: '正在保存录制' });
    session.finishing = (async () => {
      try {
        if (!result.ok) return complete(session.token, result);
        const task = { ...session.metadata, partial: result.partial === true, steps: cleanSteps(result.steps) };
        await persist(list => [...list, task], () => current(session.token));
        return complete(session.token, { ok: true, message: result.partial ? '仅保存暂停前的有效步骤；流程不完整，请确认或重新录制' : '录制已保存', partial: result.partial === true, taskId: task.id, stepsCount: task.steps.length });
      } catch (error) { return complete(session.token, failure(error)); }
    })();
    return session.finishing;
  }
  function retireRunner(active) {
    if (runner !== active) return;
    runner = null; runnerGeneration++;
    if (typeof active.kill === 'function') active.kill();
    else if (typeof active.dispose === 'function') Promise.resolve(active.dispose()).catch(() => {});
  }
  function getRunner() {
    if (runner) return runner;
    if (disposed) throw new Error('自动化服务已关闭');
    const generation = ++runnerGeneration;
    const event = data => {
      if (generation !== runnerGeneration || disposed) return;
      if (data.event === 'recordingPaused' && recording && current(recording.token) && state.mode === 'recording') {
        setState({ ok: false, message: data.message, reasonCode: data.reasonCode || 'recording_paused', retainedSteps: data.retainedSteps });
      } else if (data.event === 'recordingFinished' && recording) {
        finishRecording(data);
      } else if (data.event === 'progress' && activeOperation && activeOperation.kind === 'run' && state.mode === 'running') {
        setState({ ok: null, message: data.message || '正在运行', completedSteps: data.completedSteps, totalSteps: data.totalSteps, progress: { completedSteps: data.completedSteps, totalSteps: data.totalSteps, message: data.message || '' } });
      } else if (data.event === 'error') {
        runner = null; runnerGeneration++;
        if (activeOperation && activeOperation.kind !== 'stop') complete(activeOperation, { ...data, ok: false, message: data.message || '辅助进程退出', reasonCode: data.reasonCode || 'helper_error' });
      }
    };
    const exited = () => { if (generation === runnerGeneration) { runner = null; runnerGeneration++; } };
    runner = runnerFactory ? runnerFactory(event, exited) : createRunner(event, exited);
    return runner;
  }
  async function guarded(action) { try { await ready; if (disposed) return fail('自动化服务已关闭'); return await action(); } catch (error) { return failure(error); } }
  return {
    list: () => guarded(async () => ({ ok: true, tasks: tasks.map(({ steps, ...t }) => ({ ...t, stepsCount: steps.length, stepsSummary: steps.map(({ type, target }) => ({ type, process: target.process, title: target.title, name: target.name || target.automationId })), parameters: [...new Set(steps.filter(s => s.type === 'input').map(s => s.parameter))].map(name => ({ name, label: '输入参数 ' + name.replace('input_', ''), password: steps.some(s => s.type === 'input' && s.parameter === name && s.target.password) })) })), state: { ...state } })),
    getState: () => ({ ...state }),
    startRecording: options => guarded(async () => {
      if (state.mode !== 'idle') return fail('已有录制或运行中的任务');
      const name = text(options && options.name, 120);
      if (!name) return fail('请填写任务名称');
      const token = activeOperation = { kind: 'recording' };
      const session = recording = { token, metadata: { id: randomUUID(), name, description: text(options.description, 2000), createdAt: new Date().toISOString() } };
      setState({ ...cleared, mode: 'starting', taskId: session.metadata.id, message: '正在启动录制' });
      let active;
      try {
        active = getRunner();
        const result = await active.request('startRecording', { excludedProcessIds: [process.pid] });
        if (!current(token)) return fail('录制已取消');
        if (!result.ok) complete(token, result);
        else setState({ mode: 'recording', message: '正在录制；F8完成，F9取消' });
        return result;
      } catch (error) { if (active) retireRunner(active); return complete(token, failure(error)); }
    }),
    stopRecording: () => guarded(async () => {
      const session = recording;
      if (session && session.stopPromise) return session.stopPromise;
      if (session && session.finishing) return session.finishing;
      if (state.mode !== 'recording' || !session) return fail('没有正在录制的任务');
      setState({ mode: 'stopping', message: '正在结束录制' });
      session.stopPromise = (async () => {
        let active;
        try { active = getRunner(); return await finishRecording(await active.request('stopRecording', {}), session); }
        catch (error) { if (active) retireRunner(active); return complete(session.token, failure(error)); }
      })();
      return session.stopPromise;
    }),
    run: options => guarded(async () => {
      if (state.mode !== 'idle') return fail('已有录制或运行中的任务');
      if (!options || typeof options.taskId !== 'string' || !ID.test(options.taskId)) return fail('无效任务ID');
      const task = tasks.find(t => t.id === options.taskId);
      if (!task) return fail('任务不存在');
      const inputs = Object.create(null);
      for (const step of task.steps.filter(s => s.type === 'input')) {
        const value = options.inputs && Object.prototype.hasOwnProperty.call(options.inputs, step.parameter) ? options.inputs[step.parameter] : undefined;
        if (typeof value !== 'string' || value.length > 4096 || value.includes('\0')) return fail('缺少或无效的运行时参数：' + step.parameter);
        inputs[step.parameter] = value;
      }
      const token = activeOperation = { kind: 'run' };
      setState({ ...cleared, mode: 'running', taskId: task.id, totalSteps: task.steps.length, message: '正在运行；F9紧急停止' });
      let active;
      try { active = getRunner(); return complete(token, await active.request('run', { steps: task.steps, inputs })); }
      catch (error) { if (active) retireRunner(active); return complete(token, failure(error)); }
    }),
    stop: () => guarded(async () => {
      if (stopping) return stopping;
      if (state.mode === 'idle') return { ok: true, message: '当前没有运行中的任务' };
      const token = activeOperation = { kind: 'stop' };
      recording = null;
      setState({ mode: 'stopping', ok: null, message: '正在停止' });
      stopping = (async () => {
        const active = runner;
        let result;
        try { result = active ? await active.request('stop', {}) : { ok: true }; }
        catch (error) { result = failure(error); }
        finally { if (active) retireRunner(active); }
        return complete(token, result.ok ? { ...result, message: '已停止', reasonCode: 'cancelled' } : result);
      })();
      const pending = stopping;
      pending.finally(() => { if (stopping === pending) stopping = null; }).catch(() => {});
      return stopping;
    }),
    remove: taskId => guarded(async () => {
      if (state.mode !== 'idle') return fail('请先停止自动化');
      if (typeof taskId !== 'string' || !ID.test(taskId)) return fail('无效任务ID');
      if (!tasks.some(t => t.id === taskId)) return fail('任务不存在');
      await persist(list => list.filter(t => t.id !== taskId)); return { ok: true, message: '任务已删除' };
    }),
    dispose: async () => {
      disposed = true; recording = null; activeOperation = null;
      const active = runner; runner = null; runnerGeneration++;
      if (active) await active.dispose();
      await persistence;
      setState({ ...cleared, mode: 'idle', taskId: null, message: '自动化服务已关闭' });
    }
  };
}

function createRunner(onEvent, onExit = () => {}, { spawnProcess = spawn, setTimer = setTimeout, clearTimer = clearTimeout } = {}) {
  if (process.platform !== 'win32') throw new Error('全局自动化仅支持Windows');
  const base = __dirname.includes('app.asar') ? __dirname.replace(/app\.asar(?=[\\/]|$)/, 'app.asar.unpacked') : __dirname;
  const script = path.join(base, 'tools', 'automation-helper.ps1');
  const executable = path.join(process.env.SystemRoot || 'C:\\Windows', 'System32', 'WindowsPowerShell', 'v1.0', 'powershell.exe');
  const child = spawnProcess(executable, ['-NoLogo', '-NoProfile', '-NonInteractive', '-ExecutionPolicy', 'Bypass', '-File', script], { windowsHide: true, shell: false, stdio: ['pipe', 'pipe', 'pipe'] });
  let sequence = 0, closed = false;
  const pending = new Map();
  const rejectAll = (message = '自动化辅助进程已退出', reasonCode = 'helper_error') => {
    if (closed) return;
    closed = true;
    const error = Object.assign(new Error(message), { reasonCode });
    for (const p of pending.values()) { clearTimer(p.timer); p.reject(error); }
    pending.clear(); onEvent({ event: 'error', message, reasonCode }); onExit();
  };
  const transportError = () => { rejectAll('自动化辅助进程通信失败'); child.kill(); };
  child.on('error', () => rejectAll('自动化辅助进程无法启动'));
  child.on('exit', () => rejectAll());
  child.stdin.on('error', transportError); child.stdout.on('error', transportError);
  // Do not surface stderr: provider failures may contain sensitive UI text.
  child.stderr.resume();
  const lines = readline.createInterface({ input: child.stdout });
  lines.on('line', line => {
    try {
      const data = JSON.parse(line);
      if (closed) return;
      if (!data || typeof data !== 'object') throw new Error('无效辅助进程消息');
      if (data.event) {
        // Runs may last minutes. Only an absence of step progress expires them.
        if (data.event === 'progress') for (const p of pending.values()) if (p.command === 'run') p.arm();
        onEvent(data);
      } else if (pending.has(data.id)) { const p = pending.get(data.id); pending.delete(data.id); clearTimer(p.timer); p.resolve(data); }
    } catch (_) { rejectAll(); child.kill(); }
  });
  function request(command, args) {
    if (closed) return Promise.reject(new Error('自动化辅助进程不可用'));
    const id = ++sequence;
    return new Promise((resolve, reject) => {
      const timeout = command === 'run' || command === 'startRecording' ? 30000 : command === 'stopRecording' ? 20000 : 5000;
      const request = { resolve, reject, command, timer: null, arm() { clearTimer(this.timer); this.timer = setTimer(() => { rejectAll('自动化辅助进程响应超时，已强制回收', 'helper_timeout'); child.kill(); }, timeout); } };
      pending.set(id, request); request.arm();
      child.stdin.write(JSON.stringify({ id, command, ...args }) + '\n', error => { if (error) transportError(); });
    });
  }
  return { request, kill: () => { child.kill(); rejectAll('自动化辅助进程已重置'); }, dispose: async () => {
    if (!closed) {
      let timer;
      try { await Promise.race([request('stop', {}), new Promise(resolve => { timer = setTimer(resolve, 1000); })]); }
      catch (_) {}
      finally { clearTimer(timer); child.kill(); rejectAll('自动化辅助进程已关闭'); }
    }
    lines.close();
  } };
}

module.exports = { createAutomationService, createRunner };
