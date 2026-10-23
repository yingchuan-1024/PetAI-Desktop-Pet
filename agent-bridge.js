/**
 * agent-bridge — AI 状态本地桥接（协议参考 Vibe Pet）
 * 在本机回环地址监听 HTTP，接收 Trae / Cursor 等工具 Hook 推送的状态事件，
 * 统一归一化为 { agentId, sessionId, state, event, title, output, activityUpdatedAt, receivedAt }。
 * 仅绑定 127.0.0.1，不对外网开放；请求体限制 64KB。
 */
const http = require('http');

const VALID_AGENT_STATES = ['idle', 'thinking', 'working', 'attention', 'error'];
const MAX_BODY_BYTES = 64 * 1024;
const DEFAULT_PORT = 17384;

function clampInt(value, min, max, fallback) {
  const n = Math.floor(Number(value));
  return Number.isFinite(n) ? Math.min(max, Math.max(min, n)) : fallback;
}

/** 校验并归一化一次 Hook 推送；不合法时返回 { ok:false, message } */
function normalizeAgentEvent(raw) {
  if (!raw || typeof raw !== 'object' || Array.isArray(raw)) {
    return { ok: false, message: '请求体必须是 JSON 对象' };
  }
  const state = typeof raw.state === 'string' ? raw.state.trim().toLowerCase() : '';
  if (!VALID_AGENT_STATES.includes(state)) {
    return { ok: false, message: `未知 state：${state || '(空)'}，允许值：${VALID_AGENT_STATES.join(' / ')}` };
  }
  const text = (v, max) => (typeof v === 'string' ? v.slice(0, max) : '');
  return {
    ok: true,
    event: {
      agentId: text(raw.agentId, 64) || 'unknown',
      sessionId: text(raw.sessionId, 128),
      state,
      event: text(raw.event, 64),
      title: text(raw.title, 120),
      output: text(raw.output, 500),
      activityUpdatedAt: Number.isFinite(raw.activityUpdatedAt) ? raw.activityUpdatedAt : Date.now(),
      receivedAt: Date.now(),
    },
  };
}

/**
 * 创建桥接服务。
 * @param {object} options
 * @param {number}   [options.port]     默认 17384
 * @param {function} [options.onEvent]  收到合法状态事件时回调 (event)
 * @param {function} [options.onError]  服务层错误回调（如端口被占用）(error)
 */
function createAgentBridge({ port = DEFAULT_PORT, onEvent = () => {}, onError = () => {} } = {}) {
  let server = null;
  let listeningPort = 0; // 0 = 未监听
  let lastEvent = null;

  function requestHandler(req, res) {
    const finish = (code, body) => {
      try {
        res.writeHead(code, { 'Content-Type': 'application/json; charset=utf-8' });
        res.end(JSON.stringify(body || {}));
      } catch (e) { /* 客户端可能已断开 */ }
    };
    if (req.method !== 'POST' || req.url.split('?')[0] !== '/api/hook') {
      return finish(404, { ok: false, message: '仅支持 POST /api/hook' });
    }
    const chunks = [];
    let size = 0;
    let tooLarge = false;
    req.on('data', (chunk) => {
      if (tooLarge) return;
      size += chunk.length;
      if (size > MAX_BODY_BYTES) {
        tooLarge = true;
        return finish(413, { ok: false, message: '请求体超过 64KB 限制' });
      }
      chunks.push(chunk);
    });
    req.on('end', () => {
      if (tooLarge) return;
      let raw;
      try {
        raw = JSON.parse(Buffer.concat(chunks).toString('utf8'));
      } catch (e) {
        return finish(400, { ok: false, message: 'JSON 解析失败：' + e.message });
      }
      const parsed = normalizeAgentEvent(raw);
      if (!parsed.ok) return finish(400, { ok: false, message: parsed.message });
      lastEvent = parsed.event;
      try {
        onEvent(parsed.event);
      } catch (e) {
        return finish(500, { ok: false, message: '事件处理失败：' + e.message });
      }
      finish(200, { ok: true });
    });
    req.on('error', () => { /* 忽略客户端中断 */ });
  }

  /** 启动监听；已监听时先停止旧实例。返回 Promise<{ port }> */
  function start(nextPort) {
    return stop().then(() => new Promise((resolve, reject) => {
      const target = clampInt(nextPort !== undefined ? nextPort : port, 1024, 65535, DEFAULT_PORT);
      server = http.createServer(requestHandler);
      server.on('error', (err) => {
        server = null;
        listeningPort = 0;
        onError(err);
        reject(err);
      });
      server.listen(target, '127.0.0.1', () => {
        listeningPort = target;
        resolve({ port: target });
      });
    }));
  }

  /** 停止监听（未启动时直接完成）。返回 Promise */
  function stop() {
    return new Promise((resolve) => {
      if (!server) {
        listeningPort = 0;
        return resolve();
      }
      const closing = server;
      server = null;
      listeningPort = 0;
      closing.close(() => resolve());
    });
  }

  function status() {
    return { listening: listeningPort > 0, port: listeningPort, lastEvent };
  }

  return {
    start,
    stop,
    status,
    dispose: stop,
    /** 仅供测试：手动注入事件（与 HTTP 入口走同一归一化路径，不记录 lastEvent） */
    _emit: normalizeAgentEvent,
  };
}

module.exports = { createAgentBridge, normalizeAgentEvent, VALID_AGENT_STATES, DEFAULT_PORT };
