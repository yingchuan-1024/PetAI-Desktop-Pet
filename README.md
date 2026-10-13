<div align="center">

# 🐾 PetAI 桌宠

**AI Desktop Pet · 让桌面多一位会聊天、会查信息的小伙伴**

![版本](https://img.shields.io/badge/版本-1.1.1-e8a0bf?style=flat-square)
![平台](https://img.shields.io/badge/平台-Windows-0078D4?style=flat-square)
![Electron](https://img.shields.io/badge/Electron-33-47848F?style=flat-square)
![Spine](https://img.shields.io/badge/Spine-4.1%20%2F%204.2-FF9900?style=flat-square)

透明悬浮 · 角色动画 · AI 多模态对话 · 可配置 Agent 工具

[📦 项目仓库](https://github.com/yingchuan-1024/PetAI-Desktop-Pet) · [🐛 问题反馈](https://github.com/yingchuan-1024/PetAI-Desktop-Pet/issues)

</div>

---

基于 Electron + Spine 的 AI 桌面宠物，支持透明悬浮窗、角色动画、AI 多模态对话、本地会话历史，以及可通过界面添加的 Agent 工具。

本项目基于 [jinyuwan/PetAI-Desktop-Pet](https://github.com/jinyuwan/PetAI-Desktop-Pet) 改造，保留原有作者信息与许可。仓库不包含第三方角色皮肤素材，运行前需要自行准备并确认素材使用授权。

## ✨ 功能

| 功能 | 说明 |
| --- | --- |
| 🐾 桌宠本体 | 透明无边框窗口、置顶、拖拽、自主动作调度、姿态切换、静默模式和单实例运行 |
| 💬 AI 对话 | OpenAI 兼容接口、流式回复、模型配置切换、自定义人设、多会话管理及本地历史导入导出 |
| 🖼️ 多模态 | 图片上传和屏幕识别，需要模型支持视觉输入；当前图片请求不走 Agent 工具循环 |
| 🤖 Agent | 模型判断是否调用工具，将返回的 JSON 回填后结合人设回答；支持原生 function calling 和提示词 JSON 协议回退 |
| 🌤️ 内置工具 | IP 近似定位、天气查询、油价查询 |
| 🧩 自定义工具 | 在设置面板新增、编辑、删除、启用或停用 HTTP JSON 工具，无需修改代码 |
| 📐 显示设置 | 角色显示区域宽度、高度可修改并保存；头顶预留气泡空间，长提示最多显示两行 |
| 🎭 Spine 模型 | 本地加载 4.1 / 4.2 runtime，支持版本分流及皮肤独立透明混合配置 |
| ⏰ 提醒与陪伴 | 番茄钟、每日闹钟、系统通知、陪伴统计 |

## 🚀 快速开始

建议使用 Node.js 20 或更新版本及 npm。Windows 是当前主要运行与打包平台。

```powershell
git clone git@github.com:yingchuan-1024/PetAI-Desktop-Pet.git
cd PetAI-Desktop-Pet
npm ci
npm start
```

启动前请准备皮肤，参见下方说明。退出时可使用系统托盘菜单中的退出选项；开发运行也可在启动终端按 Ctrl+C。

## ⚙️ AI 配置

打开设置 → AI，填写 OpenAI 兼容 API 地址、API Key、模型名称与人设 Prompt，保存后测试连接。

| 字段 | 说明 |
| --- | --- |
| API 地址 | 模型平台的 OpenAI 兼容接口地址 |
| API Key | 自己的模型平台密钥 |
| 模型名 | 平台提供的模型或端点 ID |
| 人设 Prompt | 角色回答风格，可使用 `{petName}` 占位符 |

原生工具调用需要模型和接口支持 `tools` / `tool_calls`。接口明确拒绝工具参数时会尝试提示词 JSON 协议回退；最终效果仍取决于模型能否遵循协议。模型连接测试成功并不代表所有工具调用都已验证。

## 🤖 Agent 与内置工具

打开设置 → AI → 智能工具，启用 Agent 并保存配置。

| 工具 | 配置与说明 |
| --- | --- |
| 定位 | 手动所在地优先；留空时通过网络出口 IP 获取近似省市，代理可能导致定位偏差 |
| 天气 | 配置天气 API 的 ID 和 Key；默认公共测试值可能限流，建议使用自己的账号 |
| 油价 | 配置起零数据共享 Token，查询省市的 98 / 95 / 92 号汽油和 0 号柴油价格及涨跌 |

所在地建议填写“省份 城市”，例如 `四川 成都`。油价等接口的 Token 输入框只填凭证本身，不要包含 `token=` 或 `&keyword=`。

### 💬 对话示例

- “今天天气怎么样？”：模型可先定位，再查询天气并生成回答。
- “成都 92 号汽油多少钱？”：模型可直接使用指定地区查询。
- “我这里的油价怎么样？”：模型可结合定位结果查询油价。

油价接口：`https://api.istero.com/resource/v2/oilprice`。价格返回的数据日期、更新时间和下次调价日期可能不同，应以服务返回为准，不能仅凭查询时间认为价格已更新。

第三方服务的授权、额度、网络限制及 HTTP 403 等错误需根据具体响应排查，不能仅凭 HTTP 状态码认定 Token 无效。

## 🧩 通过界面添加工具

进入设置 → AI → 智能工具 → 自定义 HTTP JSON 工具，点击添加，填写以下信息后保存。

| 字段 | 说明 |
| --- | --- |
| 稳定工具名 | 唯一英文标识，不与内置工具重名；创建后编辑时锁定 |
| 中文显示名称 | 用于列表及聊天中的工具调用状态 |
| 模型说明 | 说明何时使用工具、需要什么输入、返回内容的含义 |
| HTTPS URL | 接口地址，可包含固定查询参数 |
| 请求方法 | GET 或 POST；GET 使用查询参数，POST 使用 JSON 请求体 |
| 鉴权方式 | 无鉴权、Query 参数或 Header |
| 鉴权字段名 | 例如 `token`、`key`、`Authorization`，按接口文档填写 |
| 鉴权值来源 | 独立凭证或共享起零 Token；Bearer 鉴权需填写完整 `Bearer ...` 值 |
| 请求超时 | 1–60 秒，默认 15 秒 |

### 📝 参数来源

每个参数支持填写名称、说明、类型、必填状态及来源：

- 模型提供：由模型根据问题生成字符串、数字或布尔值。
- 固定值：请求时使用已配置的值。
- 定位城市 / 定位省份：自动获取位置后注入，只支持字符串类型。

同一网站的接口只有在其文档明确支持共用凭证时，才应选择共享 Token。共享起零 Token 与内置油价工具使用同一个配置项。

### ⛽ 配置示例

以下为油价接口配置示例（已有内置油价工具，无需重复创建）：

| 字段 | 示例 |
| --- | --- |
| 稳定工具名 | `query_oil_price` |
| 显示名称 | 油价查询 |
| 模型说明 | 查询指定省市油价及涨跌，回答注明数据日期 |
| URL | `https://api.istero.com/resource/v2/oilprice` |
| 方法 | GET |
| 鉴权 | Query，字段名 `token`，值来源为共享起零 Token |
| 参数 | `keyword`，字符串，来源为模型提供或定位城市 |

工具返回必须是 JSON。目前业务错误识别采用常见 `ok` / `success` / `code` 约定，`code` 为 `0` 或 `200` 视为成功；不同服务约定可能需要进一步适配。暂不支持文件上传、非 JSON 响应和复杂嵌套参数编辑。

## 🎭 皮肤与模型

应用按以下顺序查找皮肤：

1. `%APPDATA%/desktop-pet/skins/` 用户皮肤目录。
2. 安装目录旁的 `skins/`。
3. 应用内置 `skins/`。

每个皮肤需在直接子目录中提供 `skin.json`。骨架、图集及贴图可以位于下级目录，路径相对于皮肤目录。

```text
skins/<皮肤名>/
├─ skin.json
├─ character.skel
├─ character.atlas
└─ character.png
```

骨架可使用兼容版本的 JSON 或二进制 `.skel`。以下为 4.1 模型的配置示例，动画名称需对应自己的模型：

```json
{
  "name": "角色名称",
  "petName": "角色称呼",
  "spine": {
    "skeleton": "character.skel",
    "atlas": "character.atlas",
    "png": "character.png",
    "premultipliedAlpha": false
  },
  "states": {
    "idle": "idle",
    "stand": "stand"
  },
  "extraStates": {
    "skill": "skill01_1"
  }
}
```

`states` 用于常规动作，`extraStates` 用于手动触发的大幅动作。`premultipliedAlpha` 需与贴图实际编码一致：straight alpha 贴图设为 `false`，不要对所有模型统一关闭。当前已验证过 4.1 模型的待机与站立显示；其他模型和技能动作需自行验证。

更多皮肤配置参考 [skins/README.md](skins/README.md)。Spine runtime 使用须遵守文件中的官方许可条款。

## 📦 打包与验证

```powershell
npm start
npm run dev
npm run dist
```

默认输出到 `dist/`：

- `PetAI Setup <版本>.exe`：Windows 安装包。
- `win-unpacked/PetAI.exe`：免安装启动程序，必须保留整个 `win-unpacked` 目录。

可指定独立目录，避免旧产物被运行中的程序占用：

```powershell
npm run dist -- --publish never --config.directories.output=dist-tools
```

自定义工具模拟验证：

```powershell
node tools/verify-custom-tools.js
```

已完成工具模拟请求、Agent 结果回填、语法检查及 Windows 打包验证。模拟验证不代表真实第三方授权或真实模型端到端调用成功。当前程序版本仍为 `1.1.1`；本次源码更新未创建新的 GitHub Release。

## 🔐 数据与隐私

配置和聊天记录保存在 Electron `userData` 目录，常规 Windows 路径为 `%APPDATA%/desktop-pet/`：

| 文件 | 内容 |
| --- | --- |
| `ai-config.json` | 模型配置及 API Key |
| `prefs.json` | 用户偏好、角色尺寸、工具配置及工具凭证 |
| `chat-history.json` | 本地对话历史 |
| `reminders.json` | 提醒配置 |
| `companion.json` | 陪伴记录 |

凭证保存在本机配置文件中，不随源码仓库分发；目前并非操作系统凭据库加密存储，请勿公开这些文件。调用服务时，相关凭证会发送到所配置的服务地址用于鉴权。

AI 对话、图片及工具结果会发送到所配置的模型服务。启用 IP 定位时会访问第三方定位服务；自定义工具会向填写的接口发送其参数。请只添加可信接口，并注意发送的数据范围。

## 📜 版权与来源

- 原项目：[jinyuwan/PetAI-Desktop-Pet](https://github.com/jinyuwan/PetAI-Desktop-Pet)。保留作者信息，代码许可见 [LICENSE](LICENSE)。
- 第三方角色素材不入仓库；请自行获取合法使用授权，不要将无授权素材作为公共分发内容。
- Spine runtime 许可见本地 vendor 文件中的许可声明。

技术栈：Electron 33、Spine Player 4.1 / 4.2、原生 JavaScript、electron-builder。
