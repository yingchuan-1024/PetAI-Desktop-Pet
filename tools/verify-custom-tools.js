'use strict';
const assert = require('node:assert/strict');
const fs = require('node:fs');
const path = require('node:path');
const vm = require('node:vm');
const agent = require('../agent');
const root = path.resolve(__dirname, '..');
const tool = { name: 'query_demo', label: '测试查询', description: '查询测试数据', enabled: true, url: 'https://example.com/json?fixed=yes', method: 'GET', authMode: 'query', authName: 'token', authSource: 'shared', authValue: '', timeoutMs: 1000, parameters: [
  { name: 'city', description: '', type: 'string', required: true, source: 'city' },
  { name: 'province', description: '', type: 'string', required: true, source: 'province' },
  { name: 'count', description: '数量', type: 'number', required: true, source: 'model' },
  { name: 'active', description: '', type: 'boolean', required: true, source: 'fixed', value: 'false' }
] };
const config = agent.normalizeToolsConfig({ manualCity: '四川 绵阳', oilToken: 'fake-shared-secret', customTools: [tool] });
let request;
const ctx = { config, fetch: async (url, opts) => { request = { url: new URL(url), opts }; return { ok: true, text: async () => JSON.stringify({ code: 200, data: { price: 8, echo: config.oilToken, token: config.oilToken } }) }; } };
async function agentChecks() {
  const result = await agent.executeTool(ctx, tool.name, { count: 2, city: '错误城市', token: '错误凭证' });
  assert.equal(result.ok, true);
  assert.equal(request.url.searchParams.get('fixed'), 'yes');
  assert.equal(request.url.searchParams.get('token'), config.oilToken);
  assert.equal(request.url.searchParams.get('city'), '绵阳');
  assert.equal(request.url.searchParams.get('province'), '四川');
  assert.equal(request.url.searchParams.get('active'), 'false');
  assert.equal(request.url.searchParams.get('count'), '2');
  assert(!JSON.stringify(result).includes(config.oilToken));
  const post = { ...tool, method: 'POST', authMode: 'header', authName: 'Authorization', authSource: 'independent', authValue: 'Bearer fake-private-secret' };
  await agent.executeTool({ ...ctx, config: { ...config, customTools: [post] } }, tool.name, { count: 3 });
  assert.equal(request.opts.headers.Authorization, post.authValue);
  assert.equal(request.url.searchParams.get('fixed'), 'yes');
  assert.deepEqual(JSON.parse(request.opts.body), { city: '绵阳', province: '四川', count: 3, active: false });
  assert.equal(request.opts.redirect, 'error');
  assert.equal((await agent.executeTool(ctx, tool.name, { count: '2' })).ok, false);
  assert.equal((await agent.executeTool(ctx, tool.name, {})).ok, false);
  for (const invalid of [{ name: 'get_weather' }, { name: '非法' }, { url: 'http://example.com' }, { timeoutMs: -1 }, { parameters: [{ name: '__proto__' }] }, { method: 'DELETE' }]) {
    const c = { ...config, customTools: [{ ...tool, ...invalid }] };
    assert.equal(agent.normalizeToolsConfig(c).customTools.length, 0);
    assert.equal((await agent.executeTool({ ...ctx, config: c }, tool.name, { count: 2 })).ok, false);
  }
  assert.equal((await agent.executeTool({ ...ctx, config: { ...config, enabled: false } }, 'get_location', {})).ok, false);
  assert.equal((await agent.executeTool({ ...ctx, config: { ...config, customTools: [{ ...tool, enabled: false }] } }, tool.name, { count: 2 })).ok, false);
  assert.equal((await agent.executeTool({ ...ctx, fetch: async () => ({ ok: false, status: 403 }) }, tool.name, { count: 2 })).message, '工具服务 HTTP 403');
  const business = await agent.executeTool({ ...ctx, fetch: async () => ({ ok: true, text: async () => JSON.stringify({ code: 403, msg: config.oilToken }) }) }, tool.name, { count: 2 });
  assert.equal(business.ok, false); assert(!JSON.stringify(business).includes(config.oilToken));
  assert.equal((await agent.executeTool({ ...ctx, fetch: async () => ({ ok: true, text: async () => 'not json' }) }, tool.name, { count: 2 })).ok, false);
  assert.match((await agent.executeTool({ ...ctx, fetch: () => new Promise(() => {}) }, tool.name, { count: 2 })).message, /超时/);
  const specs = agent.getToolSpecs(config); assert.equal(specs.length, 4); assert.deepEqual(Object.keys(specs[3].function.parameters.properties), ['count']);
  for (const prompt of [false, true]) {
    let step = 0; const events = [];
    const full = await agent.runAgent(ctx, { streamFn: async (messages, delta, opts) => {
      if (prompt && opts.tools) throw new Error('tools unsupported');
      if (step++ === 0) {
        if (prompt) { assert(messages[0].content.includes(tool.name)); return { content: JSON.stringify({ tool: tool.name, args: { count: 2 } }) }; }
        assert(opts.tools.some((s) => s.function.name === tool.name));
        return { toolCalls: [{ id: 'call-1', function: { name: tool.name, arguments: '{"count":2}' } }] };
      }
      const backfill = messages.at(-1); assert.equal(backfill.role, prompt ? 'user' : 'tool'); assert(backfill.content.includes('price')); assert(!backfill.content.includes(config.oilToken)); return { content: '完成' };
    } }, [{ role: 'user', content: '查询' }], () => {}, (ev) => events.push(ev));
    assert.equal(full, '完成'); assert.equal(events[0].label, tool.label); assert.equal(events.length, 2);
  }
  console.log('PASS 模拟 GET/POST、共享/独立鉴权、定位、非法/禁用拒绝、类型、403、业务错误、JSON、超时、脱敏及两种 Agent 循环');
}
async function domChecks() {
  const { app, BrowserWindow, ipcMain } = require('electron');
  await app.whenReady();
  let prefs = { tools: config }; let saved = 0;
  const main = fs.readFileSync(path.join(root, 'main.js'), 'utf8');
  const handlers = main.slice(main.indexOf("ipcMain.handle('tools:get-config'"), main.indexOf('/* ---------- 屏幕识别'));
  vm.runInNewContext(handlers, { ipcMain, prefs, normalizeToolsConfig: agent.normalizeToolsConfig, savePrefs: () => saved++ });
  const fallbacks = { 'settings:get-state': { petSize: { w: 240, h: 336 } }, 'ai:get-config': {}, 'prefs:get': {}, 'skins:list': [], 'skins:active': '', 'pet:get-name': '测试宠物', 'companion:get': {}, 'theme:get': '#e8a0bf', 'app:get-info': {}, 'chat:get-history': {}, 'reminder:get-state': {} };
  Object.entries(fallbacks).forEach(([channel, value]) => ipcMain.handle(channel, () => value));
  const win = new BrowserWindow({ show: false, webPreferences: { preload: path.join(root, 'preload.js'), contextIsolation: true, nodeIntegration: false } });
  await win.loadFile(path.join(root, 'renderer/settings.html'));
  await win.webContents.executeJavaScript(`new Promise(resolve => setTimeout(resolve, 200))`);
  const dom = await win.webContents.executeJavaScript(`(async () => {
    const el = id => document.getElementById(id); const click = id => el(id).click();
    const wait = () => new Promise(resolve => setTimeout(resolve, 80));
    if (!el('custom-tool-list').textContent.includes('测试查询')) throw Error('加载列表失败');
    click('custom-tool-add');
    const values = { 'ct-name':'ui_demo', 'ct-label':'界面工具', 'ct-desc':'测试界面工具', 'ct-url':'https://example.com/post?mode=json', 'ct-method':'POST', 'ct-auth-mode':'header', 'ct-auth-name':'X-Token', 'ct-auth-source':'shared', 'ct-timeout':'2000' };
    Object.entries(values).forEach(([id, value]) => { el(id).value = value; el(id).dispatchEvent(new Event('change')); });
    click('ct-param-add'); const row = el('ct-params').querySelector('.ct-param');
    for (const [key, value] of Object.entries({name:'city',description:'城市',type:'string',source:'city'})) row.querySelector('[data-field="'+key+'"]').value=value;
    row.querySelector('[data-field="required"]').checked=true;
    click('ct-param-add'); el('ct-params').lastElementChild.querySelector('button').click();
    if (el('ct-params').children.length !== 1 || !el('ct-auth-value').disabled) throw Error('参数或鉴权联动错误');
    click('ct-save'); await wait(); let cfg = await window.pet.getToolsConfig();
    const added = cfg.customTools.find(t => t.name === 'ui_demo');
    if (!added || added.method !== 'POST' || added.parameters[0].source !== 'city') throw Error('保存绑定失败');
    const buttons = action => el('custom-tool-list').querySelectorAll('[data-action="'+action+'"]');
    buttons('edit')[1].click(); el('ct-label').value='编辑成功'; click('ct-save'); await wait();
    buttons('toggle')[1].click(); await wait(); cfg=await window.pet.getToolsConfig();
    if (cfg.customTools[1].enabled || cfg.customTools[1].label !== '编辑成功') throw Error('编辑启停失败');
    const invalid=await window.pet.saveToolsConfig({...cfg,customTools:[{...added,name:'get_weather'}]});
    if (invalid.ok) throw Error('IPC 未拒绝冲突名称');
    window.confirm=()=>true; buttons('delete')[1].click(); await wait(); cfg=await window.pet.getToolsConfig();
    if(cfg.customTools.length!==1) throw Error('删除失败');
    return { tools:cfg.customTools.length, status:el('tools-status').textContent };
  })()`);
  assert.equal(saved, 4); assert.equal(dom.tools, 1);
  console.log('PASS Electron 实际 settings DOM + preload invoke + 主进程原配置 handlers：加载、新增、参数行增删、字段保存、编辑、启停、删除、非法配置拒绝');
  win.destroy(); app.quit();
}
agentChecks().then(() => process.argv.includes('--dom') ? domChecks() : undefined).catch((err) => { console.error(err); process.exit(1); });
