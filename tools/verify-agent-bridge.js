/**
 * verify-agent-bridge — AI 状态桥接模块回归
 * 覆盖：启动/停止、合法事件、未知 state、坏 JSON、错误路径、超限请求体、字段容错、端口冲突。
 * 用法：node tools/verify-agent-bridge.js
 */
const http = require('http');
const { createAgentBridge } = require('../agent-bridge');

function post(port, body, pathname = '/api/hook') {
  return new Promise((resolve, reject) => {
    const data = typeof body === 'string' ? body : JSON.stringify(body);
    const req = http.request({
      host: '127.0.0.1',
      port,
      path: pathname,
      method: 'POST',
      headers: { 'Content-Type': 'application/json', 'Content-Length': Buffer.byteLength(data) },
    }, (res) => {
      const chunks = [];
      res.on('data', (c) => chunks.push(c));
      res.on('end', () => resolve({ code: res.statusCode, body: Buffer.concat(chunks).toString('utf8') }));
    });
    req.on('error', reject);
    req.end(data);
  });
}

function get(port, pathname) {
  return new Promise((resolve, reject) => {
    const req = http.get({ host: '127.0.0.1', port, path: pathname }, (res) => {
      res.resume();
      res.on('end', () => resolve({ code: res.statusCode }));
    });
    req.on('error', reject);
  });
}

const assert = (cond, name) => {
  if (!cond) throw new Error('断言失败：' + name);
  console.log('  ✓ ' + name);
};

(async () => {
  const events = [];
  const bridge = createAgentBridge({ onEvent: (e) => events.push(e) });
  const started = await bridge.start(17480);
  assert(started.port === 17480 && bridge.status().listening, '启动并使用指定端口');

  let res = await post(17480, { agentId: 'trae', sessionId: 'task-1', state: 'working', event: 'PreToolUse', title: '修改 a.js' });
  assert(res.code === 200, '合法事件返回 200');
  assert(events.length === 1 && events[0].state === 'working' && events[0].title === '修改 a.js', '回调收到归一化事件');
  assert(bridge.status().lastEvent && bridge.status().lastEvent.agentId === 'trae', 'status 记录最近事件');

  res = await post(17480, { state: 'dancing' });
  assert(res.code === 400, '未知 state 返回 400');

  res = await post(17480, '{not-json');
  assert(res.code === 400, '坏 JSON 返回 400');

  res = await get(17480, '/api/hook');
  assert(res.code === 404, 'GET 返回 404');

  res = await post(17480, 'x'.repeat(70 * 1024));
  assert(res.code === 413, '超过 64KB 返回 413');

  res = await post(17480, { agentId: 'trae', state: 'ATTENTION', extra: { nested: true }, activityUpdatedAt: 'bad' });
  assert(res.code === 200, '字段容错：state 大写归一、忽略多余/非法字段');
  const last = events[events.length - 1];
  assert(last.state === 'attention' && typeof last.activityUpdatedAt === 'number', 'state 转小写、非法时间戳回退为当前时间');

  await bridge.stop();
  assert(bridge.status().listening === false, '停止后不再监听');

  const second = createAgentBridge({});
  await second.start(17481);
  let conflict = false;
  try { await bridge.start(17481); } catch (e) { conflict = true; }
  assert(conflict, '端口被占用时启动失败');
  assert(bridge.status().listening === false, '启动失败后处于停止状态');

  await second.stop();
  console.log('\nagent-bridge 回归全部通过');
  process.exit(0);
})().catch((e) => {
  console.error('\n验证失败：' + (e && e.message));
  process.exit(1);
});
