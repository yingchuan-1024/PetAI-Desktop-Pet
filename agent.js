/**
 * agent.js — 轻量 Agent 工具箱（意图识别 → 工具调用 → 结果回填）
 *
 * 由主进程调用：
 *   1. 把工具清单（tools）随对话请求发给模型，由模型自行判断该不该调用；
 *   2. 模型返回 tool_calls 后，本地执行对应工具（定位 / 天气）；
 *   3. 把工具结果作为 tool 消息回填，让模型继续推理，直到给出最终回答。
 * 若服务商/模型不支持原生 function calling，会自动回退到「提示词 JSON 协议」。
 *
 * fetch 由外部注入（Electron 的 net.fetch，可跟随系统代理），便于复用与单独测试。
 */
'use strict';

/** 工具箱默认配置（持久化于 prefs.json 的 tools 字段） */
const DEFAULT_TOOLS_CONFIG = {
  enabled: true,          // 是否启用 Agent 工具调用
  manualCity: '',         // 手动所在地（留空 = IP 自动定位），如「四川 绵阳」
  weatherId: '88888888',  // apihz API 账号 id
  weatherKey: '88888888', // apihz API 账号 key
  oilToken: '',           // 起零数据共享 Token（油价和自定义工具）
  customTools: [],
};

/** 实时天气接口（apihz） */
const WEATHER_API = 'https://cn.apihz.cn/api/tianqi/tqyb.php';
const OIL_PRICE_API = 'https://api.istero.com/resource/v2/oilprice';

/** 一次提问内最多执行的「模型推理 + 工具调用」轮数 */
const MAX_ROUNDS = 5;

/** 直辖市：省 = 市 */
const MUNICIPALITIES = ['北京', '上海', '天津', '重庆'];

/** 自治区 / 特别行政区全称 → 简称（天气接口偏好简称） */
const REGION_ALIAS = {
  '内蒙古自治区': '内蒙古',
  '广西壮族自治区': '广西',
  '宁夏回族自治区': '宁夏',
  '新疆维吾尔自治区': '新疆',
  '西藏自治区': '西藏',
  '香港特别行政区': '香港',
  '澳门特别行政区': '澳门',
};

/** 提供给模型的工具清单（OpenAI function calling 格式） */
const TOOL_SPECS = [
  {
    type: 'function',
    function: {
      name: 'get_location',
      description: '获取用户当前所在的地理位置（省份 + 城市）。当需要知道用户在哪里（如查询天气、本地信息）时，先调用本工具获取省市，再把结果用于其它工具。',
      parameters: { type: 'object', properties: {}, required: [] },
    },
  },
  {
    type: 'function',
    function: {
      name: 'get_weather',
      description: '查询中国某地区的实时天气（温度、湿度、风向、风力、降水等）。若不确定用户所在地区，应先调用 get_location 拿到省市后再调用本工具。',
      parameters: {
        type: 'object',
        properties: {
          sheng: { type: 'string', description: '省份名称，如「四川」「广东」，不含「省」字' },
          place: { type: 'string', description: '城市名称，如「绵阳」「深圳」，不含「市」字' },
        },
        required: [],
      },
    },
  },
  {
    type: 'function',
    function: {
      name: 'get_oil_price',
      description: '查询中国省份或城市的98号、95号、92号汽油和0号柴油价格及涨跌。用户指定地区时使用该地区；查询本地油价而未指定地区时，先调用 get_location，再使用城市或省份。回答须说明数据日期，保留更新时间和下次调价日期，不得把旧数据说成今日实时价格，也不要猜测接口未注明的单位。',
      parameters: {
        type: 'object',
        properties: {
          keyword: { type: 'string', description: '省份或城市名称，如「四川」「成都」；留空时获取用户所在地' },
        },
        required: [],
      },
    },
  },
];

/* ---------- 配置与地名规范化 ---------- */

/** 规范化配置：脏数据回退默认值 */
function normalizeToolsConfig(raw) {
  const src = raw && typeof raw === 'object' ? raw : {};
  return {
    enabled: src.enabled !== false,
    manualCity: String(src.manualCity || '').trim().slice(0, 40),
    weatherId: String(src.weatherId || '').trim().slice(0, 32) || DEFAULT_TOOLS_CONFIG.weatherId,
    weatherKey: String(src.weatherKey || '').trim().slice(0, 64) || DEFAULT_TOOLS_CONFIG.weatherKey,
    oilToken: String(src.oilToken || '').trim().slice(0, 256),
    customTools: normalizeCustomTools(src.customTools),
  };
}

const SAFE_NAME = /^[A-Za-z][A-Za-z0-9_]{0,63}$/;
const BLOCKED_NAMES = ['__proto__', 'prototype', 'constructor'];

function parameterValue(type, value) {
  if (type === 'string' && typeof value === 'string') return value;
  if (type === 'number' && value !== '' && (typeof value === 'number' || typeof value === 'string') && Number.isFinite(Number(value))) return Number(value);
  if (type === 'boolean' && [true, false, 'true', 'false'].includes(value)) return value === true || value === 'true';
  throw new Error('参数类型不匹配');
}

function normalizeCustomTools(raw) {
  if (!Array.isArray(raw)) return [];
  const seen = new Set(TOOL_SPECS.map((s) => s.function.name));
  return raw.slice(0, 50).flatMap((t) => {
    try {
      if (!t || typeof t !== 'object' || !SAFE_NAME.test(t.name) || BLOCKED_NAMES.includes(t.name) || seen.has(t.name)) return [];
      const url = new URL(t.url);
      if (url.protocol !== 'https:' || url.username || url.password || url.hash) return [];
      if (!['GET', 'POST'].includes(t.method) || !['none', 'query', 'header'].includes(t.authMode)) return [];
      if (!String(t.label || '').trim() || !String(t.description || '').trim()) return [];
      const authName = String(t.authName || '').trim();
      if (t.authMode !== 'none' && (!/^[A-Za-z][A-Za-z0-9_-]{0,63}$/.test(authName) || BLOCKED_NAMES.includes(authName))) return [];
      if (t.authMode === 'header' && /^(host|content-length|connection|cookie|content-type)$/i.test(authName)) return [];
      if (!['independent', 'shared'].includes(t.authSource)) return [];
      const authValue = String(t.authValue || '').trim().slice(0, 2048);
      if (/[\r\n]/.test(authValue)) return [];
      if (!Array.isArray(t.parameters) || t.parameters.length > 30) return [];
      const names = new Set();
      const parameters = t.parameters.map((p) => {
        if (!p || !SAFE_NAME.test(p.name) || BLOCKED_NAMES.includes(p.name) || names.has(p.name) || (t.authMode !== 'none' && p.name.toLowerCase() === authName.toLowerCase())) throw new Error('非法参数');
        names.add(p.name);
        if (!['string', 'number', 'boolean'].includes(p.type) || !['model', 'fixed', 'city', 'province'].includes(p.source)) throw new Error('非法参数');
        if (['city', 'province'].includes(p.source) && p.type !== 'string') throw new Error('定位参数应为字符串');
        const value = p.source === 'fixed' ? parameterValue(p.type, p.value) : '';
        return { name: p.name, description: String(p.description || '').slice(0, 500), type: p.type, required: p.required === true, source: p.source, value };
      });
      const timeoutMs = t.timeoutMs == null ? 15000 : Number(t.timeoutMs);
      if (!Number.isInteger(timeoutMs) || timeoutMs < 1000 || timeoutMs > 60000) return [];
      seen.add(t.name);
      return [{ name: t.name, label: String(t.label).trim().slice(0, 80), description: String(t.description).trim().slice(0, 2000), enabled: t.enabled !== false, url: url.href, method: t.method, authMode: t.authMode, authName, authSource: t.authSource, authValue, parameters, timeoutMs }];
    } catch (e) { return []; }
  });
}

function getToolSpecs(config) {
  const cfg = normalizeToolsConfig(config);
  if (!cfg.enabled) return [];
  return TOOL_SPECS.concat(cfg.customTools.filter((t) => t.enabled).map((t) => {
    const properties = {};
    const required = [];
    t.parameters.filter((p) => p.source === 'model').forEach((p) => {
      properties[p.name] = { type: p.type, description: p.description };
      if (p.required) required.push(p.name);
    });
    return { type: 'function', function: { name: t.name, description: t.description, parameters: { type: 'object', properties, required, additionalProperties: false } } };
  }));
}

function getPromptProtocol(config) {
  return ['【工具调用协议】', '你可以调用以下工具获取真实数据：',
    ...getToolSpecs(config).map((s) => '- ' + s.function.name + '：' + s.function.description + '\n参数 JSON Schema：' + JSON.stringify(s.function.parameters)),
    '需要工具时只输出一行 JSON：{"tool":"工具名","args":{}}。收到工具结果后继续调用或用自然语言回答。价格数据须注明数据日期，保留更新时间和下次调价日期，不得把旧数据称为今日实时价格，不猜测接口未注明的单位。'].join('\n');
}

function redactToolData(config, data) {
  const cfg = normalizeToolsConfig(config);
  const secrets = [cfg.oilToken, cfg.weatherKey, ...cfg.customTools.flatMap((t) => [t.authValue, t.authValue.replace(/^Bearer\s+/i, '')])];
  cfg.customTools.forEach((t) => {
    new URL(t.url).searchParams.forEach((v, k) => { if (/token|key|secret|auth|password/i.test(k) || k === t.authName) secrets.push(v); });
  });
  const values = [...new Set(secrets.filter(Boolean).flatMap((s) => [s, encodeURIComponent(s)]))].sort((a, b) => b.length - a.length);
  const sensitiveNames = new Set(cfg.customTools.filter((t) => t.authMode !== 'none').map((t) => t.authName.toLowerCase()));
  function clean(v) {
    if (typeof v === 'string') return values.reduce((s, secret) => s.split(secret).join('[已隐藏]'), v);
    if (Array.isArray(v)) return v.map(clean);
    if (v && typeof v === 'object') return Object.fromEntries(Object.entries(v).map(([k, val]) => [clean(k), /token|api.?key|secret|password|authorization/i.test(k) || sensitiveNames.has(k.toLowerCase()) ? '[已隐藏]' : clean(val)]));
    if (typeof v === 'number' && values.includes(String(v))) return '[已隐藏]';
    return v;
  }
  return clean(data);
}

async function executeCustomTool(ctx, tool, args) {
  const a = args && typeof args === 'object' && !Array.isArray(args) ? args : {};
  const payload = {};
  let location;
  for (const p of tool.parameters) {
    let value;
    if (p.source === 'fixed') value = p.value;
    else if (p.source === 'model') value = Object.prototype.hasOwnProperty.call(a, p.name) ? a[p.name] : undefined;
    else {
      if (!location) location = await getLocation(ctx);
      if (!location.ok) return location;
      value = location[p.source];
    }
    if (value == null || value === '') {
      if (p.required) return { ok: false, message: '缺少必填参数：' + p.name };
      if (value == null) continue;
    }
    try {
      if (p.source === 'model' && typeof value !== p.type) throw new Error('类型不匹配');
      payload[p.name] = parameterValue(p.type, value);
    } catch (e) { return { ok: false, message: '参数类型错误：' + p.name + ' 应为 ' + p.type }; }
  }
  const url = new URL(tool.url);
  const headers = { Accept: 'application/json' };
  if (tool.method === 'GET') Object.entries(payload).forEach(([k, v]) => url.searchParams.set(k, String(v)));
  if (tool.authMode !== 'none') {
    const secret = tool.authSource === 'shared' ? ctx.config.oilToken : tool.authValue;
    if (!secret) return { ok: false, message: '请填写工具鉴权值或共享起零 Token 并保存' };
    if (tool.authMode === 'query') url.searchParams.set(tool.authName, secret);
    else headers[tool.authName] = secret;
  }
  if (tool.method === 'POST') headers['Content-Type'] = 'application/json';
  const controller = new AbortController();
  let timer;
  try {
    const request = (async () => {
      const resp = await ctx.fetch(url.href, { method: tool.method, headers, redirect: 'error', signal: controller.signal, ...(tool.method === 'POST' ? { body: JSON.stringify(payload) } : {}) });
      if (!resp.ok) return { ok: false, message: '工具服务 HTTP ' + resp.status };
      let data;
      try { data = JSON.parse(await readText(resp)); } catch (e) { return { ok: false, message: '工具服务未返回有效 JSON' }; }
      const failed = data && typeof data === 'object' && (data.ok === false || data.success === false || (data.code != null && ![0, 200, '0', '200'].includes(data.code)));
      return failed ? { ok: false, message: '工具服务业务错误：' + String(data.message || data.msg || data.error || data.code), data } : { ok: true, data };
    })();
    return await Promise.race([request, new Promise((resolve) => {
      timer = setTimeout(() => { controller.abort(); resolve({ ok: false, message: '工具请求超时，请稍后重试' }); }, tool.timeoutMs);
    })]);
  } catch (e) { return { ok: false, message: '工具请求失败，请检查地址、网络及鉴权配置' }; }
  finally { clearTimeout(timer); }
}

/** 省份名规范化：「四川省」→「四川」 */
function normalizeProvince(name) {
  const s = String(name || '').trim();
  if (!s) return '';
  if (REGION_ALIAS[s]) return REGION_ALIAS[s];
  const muni = MUNICIPALITIES.find((m) => s.startsWith(m));
  if (muni) return muni;
  return s.replace(/(省|自治区|特别行政区)$/, '');
}

/** 城市名规范化：「绵阳市」→「绵阳」 */
function normalizeCity(name) {
  const s = String(name || '').trim();
  if (!s) return '';
  const muni = MUNICIPALITIES.find((m) => s.startsWith(m));
  if (muni) return muni;
  return s.replace(/(市|地区|盟)$/, '');
}

/** 解析手动所在地：「四川 绵阳」/「四川,绵阳」/「绵阳」 */
function parseManualCity(text) {
  const s = String(text || '').trim();
  if (!s) return { province: '', city: '' };
  const parts = s.split(/[\s,，、/|]+/).filter(Boolean);
  if (parts.length >= 2) {
    return { province: normalizeProvince(parts[0]), city: normalizeCity(parts[1]) };
  }
  const only = parts[0];
  const muni = MUNICIPALITIES.find((m) => only.startsWith(m));
  if (muni) return { province: muni, city: muni };
  return { province: '', city: normalizeCity(only) };
}

/* ---------- 工具：位置 ---------- */

/** 读取响应体文本；encoding 用于 GBK 等非 UTF-8 数据源 */
async function readText(resp, encoding) {
  if (!encoding && typeof resp.text === 'function') return resp.text();
  const buf = Buffer.from(await resp.arrayBuffer());
  if (!encoding) return buf.toString('utf8');
  try { return new TextDecoder(encoding).decode(buf); } catch (e) { return buf.toString('utf8'); }
}

/** IP 定位源：按顺序回退，任一成功即返回 */
const LOCATION_SOURCES = [
  {
    name: 'useragentinfo',
    async resolve(fetchImpl) {
      const resp = await fetchImpl('https://ip.useragentinfo.com/json');
      if (!resp.ok) throw new Error('HTTP ' + resp.status);
      const j = JSON.parse(await readText(resp));
      return { ip: j.ip, province: j.province, city: j.city };
    },
  },
  {
    name: 'pconline',
    async resolve(fetchImpl) {
      const resp = await fetchImpl('https://whois.pconline.com.cn/ipJson.jsp?json=true');
      if (!resp.ok) throw new Error('HTTP ' + resp.status);
      const j = JSON.parse(await readText(resp, 'gbk'));
      if (!j || j.err) throw new Error('定位服务返回异常');
      return { ip: j.ip, province: j.pro, city: j.city };
    },
  },
];

/** 获取用户位置：手动配置优先，其次 IP 定位 */
async function getLocation(ctx) {
  const cfg = ctx.config || DEFAULT_TOOLS_CONFIG;
  if (cfg.manualCity) {
    const m = parseManualCity(cfg.manualCity);
    if (m.city) return { ok: true, source: 'manual', province: m.province, city: m.city };
  }
  const errors = [];
  for (const src of LOCATION_SOURCES) {
    try {
      const r = await src.resolve(ctx.fetch);
      const city = normalizeCity(r && r.city);
      if (!city) { errors.push(src.name + '：未返回城市'); continue; }
      return { ok: true, source: src.name, ip: r.ip, province: normalizeProvince(r.province), city };
    } catch (e) {
      errors.push(src.name + '：' + (e && e.message ? e.message : String(e)));
    }
  }
  return {
    ok: false,
    message: '自动定位失败，请在「设置 → AI → 智能工具」中手动填写所在地',
    errors,
  };
}

/* ---------- 工具：天气 ---------- */

/** 查询实时天气；未给出地区时先自动定位 */
async function getWeather(ctx, args) {
  const cfg = ctx.config || DEFAULT_TOOLS_CONFIG;
  const a = args && typeof args === 'object' ? args : {};
  let sheng = normalizeProvince(a.sheng);
  let place = normalizeCity(a.place);
  if (!place) {
    const loc = await getLocation(ctx);
    if (!loc.ok) return loc;
    sheng = sheng || loc.province || '';
    place = loc.city;
  }
  const url = WEATHER_API +
    '?id=' + encodeURIComponent(cfg.weatherId || DEFAULT_TOOLS_CONFIG.weatherId) +
    '&key=' + encodeURIComponent(cfg.weatherKey || DEFAULT_TOOLS_CONFIG.weatherKey) +
    '&sheng=' + encodeURIComponent(sheng) +
    '&place=' + encodeURIComponent(place);
  const resp = await ctx.fetch(url);
  if (!resp.ok) return { ok: false, message: '天气服务 HTTP ' + resp.status };
  let data;
  try {
    data = JSON.parse(await readText(resp));
  } catch (e) {
    return { ok: false, message: '天气服务返回解析失败' };
  }
  if (String(data.code) !== '200') {
    return { ok: false, message: data.msg || ('天气服务返回 code=' + data.code), query: { sheng, place } };
  }
  return Object.assign({ ok: true, query: { sheng, place } }, data);
}

/** 查询油价；Token 仅用于请求，不放入模型的工具结果 */
async function getOilPrice(ctx, args) {
  const cfg = ctx.config || DEFAULT_TOOLS_CONFIG;
  const token = String(cfg.oilToken || '').trim();
  if (!token) return { ok: false, message: '请在「设置 → AI → 智能工具」中填写油价 API Token 并保存' };
  const a = args && typeof args === 'object' ? args : {};
  if (a.keyword != null && typeof a.keyword !== 'string') {
    return { ok: false, message: '油价查询的 keyword 必须是省份或城市名称字符串' };
  }
  let keyword = (a.keyword || '').trim();
  if (!keyword) {
    const loc = await getLocation(ctx);
    if (!loc.ok) return loc;
    keyword = loc.city || loc.province;
  }
  if (keyword.length > 40) return { ok: false, message: '请提供不超过40个字符的省份或城市名称' };
  let resp;
  try {
    resp = await ctx.fetch(OIL_PRICE_API + '?token=' + encodeURIComponent(token) +
      '&keyword=' + encodeURIComponent(keyword), { signal: AbortSignal.timeout(15000) });
  } catch (e) {
    return { ok: false, message: '油价服务请求失败或超时，请稍后重试', query: { keyword } };
  }
  if (!resp.ok) return { ok: false, message: '油价服务 HTTP ' + resp.status, query: { keyword } };
  let data;
  try {
    data = JSON.parse(await readText(resp));
  } catch (e) {
    return { ok: false, message: '油价服务返回解析失败', query: { keyword } };
  }
  if (!data || String(data.code) !== '200') {
    return { ok: false, message: '油价服务查询失败，请检查 Token、额度及地区名称', code: data && data.code, query: { keyword } };
  }
  if (!data.data || typeof data.data !== 'object' || Array.isArray(data.data)) {
    return { ok: false, message: '油价服务未返回有效的价格数据', query: { keyword } };
  }
  return { ok: true, query: { keyword }, data: data.data };
}

/** 按名称执行工具，返回可 JSON 序列化的结果 */
async function executeTool(ctx, name, args) {
  const config = normalizeToolsConfig(ctx.config);
  const safeCtx = { ...ctx, config };
  if (!getToolSpecs(config).some((s) => s.function.name === name)) return { ok: false, message: '工具未注册或已停用' };
  let result;
  try {
    if (name === 'get_location') result = await getLocation(safeCtx);
    else if (name === 'get_weather') result = await getWeather(safeCtx, args);
    else if (name === 'get_oil_price') result = await getOilPrice(safeCtx, args);
    else result = await executeCustomTool(safeCtx, config.customTools.find((t) => t.name === name), args);
  } catch (e) { result = { ok: false, message: '工具请求失败，请检查网络及配置' }; }
  return redactToolData(config, result);
}

/* ---------- 提示词协议回退 ---------- */

/** 判断错误是否「不支持 tools」，用于回退到提示词协议 */
function isToolUnsupported(err) {
  const msg = (err && err.message ? err.message : String(err)) || '';
  return /tool|function|unsupported|not support|不支持|invalid.*(param|tool|function)/i.test(msg);
}

/** 从模型文本中解析工具调用（提示词协议回退用） */
function parsePromptToolCall(text) {
  if (!text) return null;
  const candidates = [];
  const fence = text.match(/```(?:json)?\s*([\s\S]*?)```/i);
  if (fence) candidates.push(fence[1]);
  const start = text.indexOf('{');
  if (start >= 0) {
    let depth = 0;
    for (let i = start; i < text.length; i += 1) {
      const ch = text[i];
      if (ch === '{') depth += 1;
      else if (ch === '}') {
        depth -= 1;
        if (depth === 0) { candidates.push(text.slice(start, i + 1)); break; }
      }
    }
  }
  for (const c of candidates) {
    try {
      const obj = JSON.parse(String(c).trim());
      if (obj && typeof obj.tool === 'string') return { tool: obj.tool, args: obj.args || {} };
    } catch (e) { /* 尝试下一个候选 */ }
  }
  return null;
}

/* ---------- Agent 主循环 ---------- */

/**
 * 执行 Agent 对话。
 * @param {object} ctx       { fetch, config }
 * @param {object} deps      { streamFn(messages, onDelta, {tools}) -> { content, toolCalls } }
 * @param {Array}  messages  已组装好的对话消息（含 system）
 * @param {Function} onDelta 文本增量回调
 * @param {Function} onEvent 工具状态回调：{ phase:'start'|'end', name, args, result }
 * @returns {Promise<string>} 最终回复文本
 */
async function runAgent(ctx, deps, messages, onDelta, onEvent) {
  const streamFn = deps.streamFn;
  const convo = messages.slice();
  ctx = { ...ctx, config: normalizeToolsConfig(ctx.config) };
  const notify = (ev) => {
    if (typeof onEvent === 'function') onEvent(redactToolData(ctx.config, { ...ev, label: ctx.config.customTools.find((t) => t.name === ev.name)?.label }));
  };

  for (let round = 0; round < MAX_ROUNDS; round += 1) {
    let res;
    try {
      res = await streamFn(convo, onDelta, { tools: getToolSpecs(ctx.config) });
    } catch (err) {
      // 服务商不支持原生 function calling → 回退提示词协议
      if (isToolUnsupported(err)) return runAgentPrompt(ctx, deps, messages, onDelta, notify);
      throw err;
    }
    const calls = (res && res.toolCalls) || [];
    if (!calls.length) return (res && res.content) || '';

    convo.push({ role: 'assistant', content: (res && res.content) || '', tool_calls: calls });
    for (const call of calls) {
      const fn = call.function || {};
      const name = fn.name || 'unknown';
      let args = {};
      try { args = JSON.parse(fn.arguments || '{}') || {}; } catch (e) { args = {}; }
      notify({ phase: 'start', name, label: (ctx.config.customTools || []).find((t) => t.name === name)?.label, args });
      let result;
      try { result = await executeTool(ctx, name, args); }
      catch (e) { result = { ok: false, message: (e && e.message) || String(e) }; }
      notify({ phase: 'end', name, result });
      convo.push({ role: 'tool', tool_call_id: call.id || name, name, content: JSON.stringify(result) });
    }
  }
  // 轮次用尽：关闭工具再请求一次，确保拿到最终答复
  const fin = await streamFn(convo, onDelta, { tools: null });
  return (fin && fin.content) || '';
}

/** 提示词协议版 Agent（兼容不支持 function calling 的模型） */
async function runAgentPrompt(ctx, deps, messages, onDelta, onEvent) {
  const streamFn = deps.streamFn;
  const convo = messages.slice();
  const sysIdx = convo.findIndex((m) => m.role === 'system');
  if (sysIdx >= 0) {
    convo[sysIdx] = { role: 'system', content: String(convo[sysIdx].content) + '\n\n' + getPromptProtocol(ctx.config) };
  } else {
    convo.unshift({ role: 'system', content: getPromptProtocol(ctx.config) });
  }

  for (let round = 0; round < MAX_ROUNDS; round += 1) {
    const res = await streamFn(convo, onDelta, { tools: null });
    const content = (res && res.content) || '';
    const call = parsePromptToolCall(content);
    if (!call) return content;
    onEvent({ phase: 'start', name: call.tool, args: call.args });
    let result;
    try { result = await executeTool(ctx, call.tool, call.args); }
    catch (e) { result = { ok: false, message: (e && e.message) || String(e) }; }
    onEvent({ phase: 'end', name: call.tool, result });
    convo.push({ role: 'assistant', content });
    convo.push({ role: 'user', content: '工具结果：' + JSON.stringify(result) });
  }
  const fin = await streamFn(convo, onDelta, { tools: null });
  return (fin && fin.content) || '';
}

module.exports = {
  DEFAULT_TOOLS_CONFIG,
  TOOL_SPECS,
  normalizeToolsConfig,
  normalizeCustomTools,
  getToolSpecs,
  getPromptProtocol,
  redactToolData,
  normalizeProvince,
  normalizeCity,
  parseManualCity,
  getLocation,
  getWeather,
  getOilPrice,
  executeTool,
  runAgent,
  parsePromptToolCall,
};
