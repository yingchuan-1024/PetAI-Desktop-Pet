'use strict';
const fs = require('fs');
const path = require('path');
const crypto = require('crypto');
const { spawn } = require('child_process');

function createTasktService({ userData, executable, onState = () => {}, spawnProcess = spawn, timeoutMs = 300000, cancelGraceMs = 15000 }) {
  const root = path.join(userData, 'Taskt');
  fs.mkdirSync(root, { recursive: true, mode: 0o700 });
  const storage = path.join(root, 'taskt-tasks-v1.json');
  let tasks = [];
  if (fs.existsSync(storage)) {
    const saved = JSON.parse(fs.readFileSync(storage, 'utf8'));
    if (saved.version !== 1 || !Array.isArray(saved.tasks)) throw new Error('Taskt 任务存储格式错误');
    tasks = saved.tasks;
  }
  let state = { mode: 'idle', status: 'idle' };
  let active = null;
  const emit = value => { state = value; onState(value); };
  const save = () => {
    const temp = storage + '.tmp';
    fs.writeFileSync(temp, JSON.stringify({ version: 1, tasks }, null, 2), { mode: 0o600 });
    fs.renameSync(temp, storage);
  };
  const errorResult = error => ({ ok: false, status: 'bridge_error', message: error.message });
  function bridge(scriptPath, validate, taskId) {
    if (!validate && active) return Promise.resolve({ ok: false, message: 'Taskt 引擎仍可能活动，请先等待完成' });
    const directory = fs.mkdtempSync(path.join(root, 'bridge-'));
    const id = crypto.randomUUID();
    const responseFile = path.join(directory, 'response.json');
    const requestFile = path.join(directory, 'request.json');
    fs.writeFileSync(requestFile, JSON.stringify({ id, scriptPath, responseFile }), { mode: 0o600 });
    return new Promise(resolve => {
      let child;
      let settled = false;
      let cleaned = false;
      let exited = false;
      let lastStatus;
      let timedOut = false;
      let cancelAt = 0;
      const started = Date.now();
      const terminal = new Set(['validated', 'succeeded', 'failed', 'cancelled', 'bridge_error']);
      const finish = result => {
        if (settled) return;
        settled = true;
        resolve(result);
      };
      const cleanup = () => {
        if (cleaned) return;
        cleaned = true;
        clearInterval(timer);
        if (!validate) { active = null; emit({ mode: 'idle', ...finalResult }); }
        fs.rmSync(directory, { recursive: true, force: true });
      };
      let finalResult = null;
      const cancel = () => {
        if (!cancelAt) {
          cancelAt = Date.now();
          fs.writeFileSync(path.join(directory, 'cancel.json'), JSON.stringify({ id }), { mode: 0o600 });
          if (!validate) emit({ mode: 'stopping', taskId, status: timedOut ? 'timeout' : 'cancel_requested', cancelConfirmed: false, message: '已请求引擎取消，等待完成事件；外部进程可能仍活动' });
        }
      };
      const poll = () => {
        if (cleaned) return;
        try {
          let result;
          try {
            result = JSON.parse(fs.readFileSync(responseFile, 'utf8'));
          } catch (error) {
            if (error.code !== 'ENOENT') throw error;
          }
          if (result) {
            if (result.id !== id) throw new Error('Taskt 响应 id 不匹配');
            if (terminal.has(result.status)) {
              finalResult = { ...result, timedOut, message: `${result.status}${timedOut ? '（运行超时后返回）' : ''}${result.error ? ': ' + result.error : ''}${validate ? '' : '；外部进程可能仍活动'}` };
              finish(finalResult);
              if (exited) cleanup();
              return;
            }
            if (!validate && lastStatus !== result.status) {
              lastStatus = result.status;
              emit({ mode: cancelAt ? 'stopping' : 'running', taskId, ...result, message: result.status === 'cancel_requested' ? '引擎已收到取消请求，尚未确认停止' : 'Taskt 正在运行；F9 请求停止' });
            }
          }
          if (exited) {
            finalResult = { ok: false, status: 'bridge_error', cancelConfirmed: false, externalProcessesMayBeActive: !validate, message: 'Taskt 退出但没有引擎完成响应；不能确认工作流结果' };
            finish(finalResult); cleanup(); return;
          }
          if (Date.now() - started > (validate ? 60000 : timeoutMs)) { timedOut = true; cancel(); }
          if (cancelAt && Date.now() - cancelAt > cancelGraceMs && !settled) {
            const result = { ok: false, status: timedOut ? 'timeout' : 'cancel_unconfirmed', cancelConfirmed: false, externalProcessesMayBeActive: true, message: '尚未收到引擎完成确认；保留进程与状态，外部进程可能仍活动' };
            if (!validate) emit({ mode: 'stopping', taskId, ...result });
            finish(result);
          }
        } catch (error) {
          if (!exited) cancel();
          finish({ ...errorResult(error), cancelConfirmed: false, externalProcessesMayBeActive: !validate });
          if (exited) cleanup();
        }
      };
      const timer = setInterval(poll, 100);
      try {
        child = spawnProcess(executable, [validate ? '--petai-validate' : '--petai-run', requestFile], { cwd: root, shell: false, windowsHide: true, stdio: 'ignore' });
        if (!validate) { active = { cancel, child }; emit({ mode: 'running', status: 'starting', taskId, message: '正在启动 Taskt' }); }
        child.once('error', error => { exited = true; finalResult = errorResult(error); finish(finalResult); cleanup(); });
        child.once('exit', () => { exited = true; poll(); });
      } catch (error) { exited = true; finalResult = errorResult(error); finish(finalResult); cleanup(); }
    });
  }
  return {
    getState: () => ({ ...state }),
    list: async () => ({ ok: true, tasks: tasks.map(task => ({ ...task, parameters: [] })), state: { ...state } }),
    register: async ({ scriptPath, name, description }) => {
      if (typeof name !== 'string' || !name.trim() || typeof description !== 'string' || !path.isAbsolute(scriptPath) || path.extname(scriptPath).toLowerCase() !== '.xml') return { ok: false, message: '请选择 XML 并填写名称和描述' };
      const checked = await bridge(scriptPath, true);
      if (!checked.ok) return checked;
      const task = { id: crypto.randomUUID(), name: name.trim().slice(0, 200), description: description.slice(0, 2000), scriptPath, createdAt: new Date().toISOString() };
      tasks.push(task); save(); return { ok: true, task, message: '已通过只读预校验并登记；预校验不保证运行安全，请在 Taskt 中审阅' };
    },
    open: async taskId => {
      const task = taskId == null ? null : tasks.find(item => item.id === taskId);
      if (taskId != null && !task) return { ok: false, message: '任务不存在' };
      return new Promise(resolve => {
        const child = spawnProcess(executable, ['--petai-editor', path.join(root, 'editor'), ...(task ? [task.scriptPath] : [])], { cwd: root, shell: false, detached: true, stdio: 'ignore' });
        child.once('error', error => resolve(errorResult(error)));
        child.once('spawn', () => { child.unref(); resolve({ ok: true, message: '已打开 Taskt 编辑器' }); });
      });
    },
    run: async options => {
      if (!options || Object.keys(options).some(key => key !== 'taskId') || typeof options.taskId !== 'string') return { ok: false, message: '只接受登记 taskId' };
      const task = tasks.find(item => item.id === options.taskId);
      return task ? bridge(task.scriptPath, false, task.id) : { ok: false, message: '任务不存在' };
    },
    stop: async () => { if (!active) return { ok: false, message: '没有活动任务' }; active.cancel(); return { ok: true, status: 'cancel_requested', cancelConfirmed: false, message: '取消已请求，等待引擎确认' }; },
    remove: async taskId => {
      if (active) return { ok: false, message: '请等待引擎完成' };
      tasks = tasks.filter(task => task.id !== taskId); save(); return { ok: true, message: '已删除登记；XML 文件保留' };
    },
    dispose: async () => { if (active) active.cancel(); }
  };
}
module.exports = { createTasktService };
