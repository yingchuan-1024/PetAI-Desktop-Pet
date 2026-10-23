# AI 编程助手与硬件状态接入教程

本文说明如何将支持 Hook／插件／会话事件的 AI 编程助手状态推送给 PetAI，以及如何规划外接状态屏或设备。**PetAI 当前提供的是本机 HTTP 状态接收器；各 Agent 的 Hook 安装位置、事件名称和输入格式并不相同，不能把一个 Agent 的配置直接复制给另一个。**本教程不会假定尚未验证的 Agent 配置已经适配。

## 1. 当前支持范围

| 类别 | 当前状态 | 说明 |
| --- | --- | --- |
| Trae | 可接入 | 本项目已有 Trae 全局 Hook 转发脚本配置示例，需在 Trae 中确认 Hooks 已启用。 |
| 其他 Agent（Codex、Cursor、Windsurf、Claude、Gemini、Copilot、CodeBuddy、Kimi、Qwen、OpenClaw、opencode、Qoder、Hermes、Reasonix、WorkBuddy 等） | 可按协议适配，未逐一验证 | 需要在对应工具配置 Hook／插件，将其事件映射为下文的 JSON，再 POST 至 PetAI。是否支持及具体步骤取决于该工具版本和 Hook 能力。 |
| Vibe Pet 等硬件 | 尚未实现硬件通信适配 | PetAI 当前没有 BLE/Wi-Fi 设备发现、配对、传输或固件协议；不能直接将本机 HTTP 桥接视作硬件支持。 |

PetAI 当前的头顶胶囊显示状态或标题；任务完成时显示气泡。完成状态由 Hook 推送推断，不代表 Agent 的业务结果一定成功。完成判定使用全局前一条事件，尚未按 `agentId`／`sessionId` 隔离；同时运行多个 Agent／会话时请谨慎测试。

## 2. 启用 PetAI 状态接入

1. 启动包含“AI 状态接入”功能的 PetAI 版本。
2. 打开「设置 → 状态接入」，默认端口为 `17384`，打开「启用状态接入」。服务只绑定本机 `127.0.0.1`。
3. 到「对话」页设置称呼，完成提醒会使用该称呼；未设置时显示“主人”。
4. 保持 PetAI 运行。关闭开关或退出应用后，接收服务不可用。

若启用失败，先检查端口是否被其他程序占用。可以在设置中改端口；之后发送请求的脚本也必须使用相同端口。

## 3. 通用 HTTP 协议

### 端点

```text
POST http://127.0.0.1:17384/api/hook
Content-Type: application/json; charset=utf-8
```

桥接服务只接受此 POST 路径；请求体上限为 64 KB。成功返回 HTTP 200 和 `{"ok":true}`。无效 JSON 或状态返回 400；路径／方法错误返回 404；超过上限返回 413。当前接口没有 Token 鉴权，且只绑定回环地址，**不要将它改成监听公网地址或通过端口转发暴露到局域网／互联网**。

### 请求体

```json
{
  "agentId": "cursor",
  "sessionId": "session-001",
  "state": "working",
  "event": "PreToolUse",
  "title": "编辑文件",
  "output": "正在执行",
  "activityUpdatedAt": 1781500000000
}
```

| 字段 | 规则 |
| --- | --- |
| `state` | 必填；只接受 `idle`、`thinking`、`working`、`attention`、`error`，大小写不敏感。 |
| `agentId` | 可选；建议填写工具稳定标识，如 `trae`、`codex`。 |
| `sessionId` | 可选；建议填写会话标识，便于后续扩展与排错。当前完成判定不按会话区分。 |
| `event` | 可选；工具原始事件名或自定义事件名，最长 64 字符。 |
| `title` | 可选；短任务／工具标题，最长 120 字符；有标题时胶囊优先显示标题。 |
| `output` | 可选，最长 500 字符；当前宠物界面不显示此字段。 |
| `activityUpdatedAt` | 可选；Unix 毫秒时间戳，缺失或非数值时由桌宠使用接收时间。 |

### 胶囊与事件文字显示

胶囊优先显示 `title`；如果标题为空，则显示中文状态，例如“思考中…”、“工作中…”。PetAI 会将部分常见 Hook 事件名和工具名翻译为中文：

| 英文名称（大小写不敏感） | 显示文字 |
| --- | --- |
| `Read` | 读取文件 |
| `Grep` | 搜索内容 |
| `Glob` | 查找文件 |
| `Edit` | 编辑文件 |
| `Write` | 写入文件 |
| `RunCommand` | 运行命令 |
| `CheckCommandStatus` | 检查命令状态 |
| `StopCommand` | 停止命令 |
| `WebSearch` | 搜索网页 |
| `WebFetch` | 读取网页 |
| `UserPromptSubmit` | 收到新任务 |
| `PreToolUse` | 准备调用工具 |
| `PostToolUse` | 工具执行完成 |
| `Stop` | 本轮已结束 |
| `Notification` | 等待确认 |

这是内置名称映射，不是对 Agent 返回文本的机器翻译。未列出的工具名／事件名保留原文；自定义中文 `title` 会直接显示。设置页最近事件会另外显示翻译后的 Hook 事件和工具标题，方便排错。

状态建议映射：

| Agent 生命周期 | 建议状态 | 用途 |
| --- | --- | --- |
| 收到用户任务、开始思考 | `thinking` | 显示思考中 |
| 开始调用工具或执行代码 | `working` | 显示工作中 |
| 等待用户确认／授权 | `attention` | 显示等待确认；不要将所有 attention 当作成功 |
| 任务结束 | `idle` 或明确完成事件 | 收起为闲置状态或发送完成事件 |
| 发生错误 | `error` | 显示错误状态，不应触发完成提醒 |

目前会触发完成气泡的情况：事件名为 `Stop`、`TaskComplete`、`Task_Complete`、`Complete`、`Completed`（大小写不敏感），或上一条事件处于 `thinking`／`working` 且本次转为 `attention`，但确认类事件 `Notification`、`Confirm`、`ConfirmRequest`、`Wait`、`Pause` 除外。不同工具的结束语义不同，接入时应先验证真实事件，不要将“轮次停止”误当成“任务成功”。

## 4. PowerShell 手动发送测试

PetAI 启动且状态接入已开启后，在 PowerShell 中运行以下测试。它只发送一条 `working` 状态；随后可将状态改为 `idle`。

```powershell
$payload = @{
  agentId = 'manual-test'
  sessionId = 'test-001'
  state = 'working'
  event = 'ManualTest'
  title = '状态接入测试'
  activityUpdatedAt = [DateTimeOffset]::Now.ToUnixTimeMilliseconds()
} | ConvertTo-Json -Compress

Invoke-RestMethod `
  -Uri 'http://127.0.0.1:17384/api/hook' `
  -Method Post `
  -Body ([System.Text.Encoding]::UTF8.GetBytes($payload)) `
  -ContentType 'application/json; charset=utf-8'
```

收到 `{"ok":true}` 且桌宠胶囊显示标题，说明 PetAI HTTP 接收端可用。中文请求体通过 UTF-8 字节发送，避免旧版 Windows PowerShell 的默认编码差异。

## 5. Trae Hooks 接入

本项目开发环境的 Trae 全局配置使用转发脚本将 Hook stdin JSON 转换为 PetAI HTTP 请求。示例目录为：

```text
%USERPROFILE%\.trae-cn\hooks.json
%USERPROFILE%\.trae-cn\hook_pusher.ps1
```

确认 `hooks.json` 中命令的脚本路径与实际文件位置一致，并在 Trae「设置 → Hooks」确认全局 Hooks 已加载。项目外部的 `hooks.json` 和脚本是本机配置，不会随 PetAI 安装包或源码仓库分发。若没有该脚本，可先按第 4 节手动验证接收端，再根据本机 Trae 版本的 Hooks 配置格式安装转发脚本。

已配置的事件映射示例：

| Trae Hook 事件 | PetAI 状态 | 说明 |
| --- | --- | --- |
| `UserPromptSubmit` | `thinking` | 用户提交提示词 |
| `PreToolUse` | `working` | 工具调用前 |
| `Stop` | `idle`，事件名保留为 `Stop` | 当前轮次结束；当前 PetAI 规则会触发完成气泡，但不等于任务成功 |

`PreToolUse` 在每次工具调用时可能触发，因此胶囊可能多次更新。实际配置字段以你安装的 Trae 版本为准。要测试时先确认桌宠开关和端口正确，再在 Trae 发起一个简单任务，观察设置页最近事件和宠物胶囊。

## 6. 接入其他 Agent 的通用步骤

不同 Agent 的 Hook 可能通过命令行参数、环境变量、标准输入 JSON、日志 JSONL 或插件回调传递事件。请查阅该 Agent 当前版本的官方 Hook 文档，并按以下步骤适配：

1. 确认工具是否支持任务开始、工具调用、等待确认、结束和错误等生命周期事件。
2. 写一个小型转发器，将该工具输入转换为第 3 节约定的 JSON。保留会话 ID；不要把提示词、文件内容、凭证或完整工具输出放入 `title`／`output`。
3. 使用本机 HTTP POST，URL 指向 `127.0.0.1` 和 PetAI 当前配置端口。
4. 对事件做映射：开始思考→`thinking`，运行中→`working`，等待用户确认→`attention`，明确错误→`error`。只有能确认任务结束的事件才映射为完成事件；不要把每次通知、审批或工具结束都标成完成。
5. 先用无敏感内容的小任务验证开始、运行、确认、结束、错误各阶段。查看 PetAI「状态接入」面板的最近事件；如无事件，先检查服务是否启用、端口和脚本错误。
6. 为不同 Agent 使用不同 `agentId`，为不同会话使用真实 `sessionId`。当前版本虽接收这些字段，但不会按会话隔离状态或完成判定。

不要直接复制另一款 Agent 的 Hook 配置：事件名、配置位置、stdin 格式和退出码约定可能完全不同。插件只要能在本机运行并发起 HTTP 请求，也可实现协议转换；本教程未提供任何特定插件的现成实现。

## 7. 硬件接入现状与建议架构

### 当前限制

PetAI 当前仅实现桌面进程内的 HTTP 接收和桌面 UI 显示。尚未实现：BLE 扫描／配对、Wi-Fi 设备发现、硬件协议编码、设备认证、屏幕布局同步、断线恢复或固件。SenseCAP Indicator、Wio Terminal、Vibe Pet 等设备不能仅靠启用本地桥接直接显示状态。

### 自行扩展时的分层

```text
AI Agent Hook／插件
        ↓ 本机 HTTP JSON
PetAI 状态桥接（127.0.0.1）
        ↓ 需自行实现的设备适配层
BLE 或 Wi-Fi
        ↓ 需自行实现的设备固件／显示逻辑
外接屏幕、状态灯或开发板
```

实现设备适配时，建议先定义与显示硬件无关的精简事件（设备 ID、状态、短标题、更新时间），由独立适配层负责将其转换为 BLE／Wi-Fi 数据包。固件端只处理有限状态和短文本，避免传送提示词、模型输出全文、API Key 或本地文件内容。还需设计配对／认证、断线与重连、消息长度、版本兼容、状态过期及用户隐私策略。

PetAI 当前没有公开的设备协议或现成 BLE SDK，因此无法在本教程中给出声称可直接烧录的固件、引脚定义或 SenseCAP／Wio／Vibe Pet 专用操作步骤。开始硬件开发前，需要取得目标设备的官方 SDK、协议与开发环境，并在独立设备适配层实现和验证。

## 8. 排错

| 现象 | 检查项 |
| --- | --- |
| 请求连接失败 | PetAI 是否正在运行；设置里的状态接入是否开启；端口是否一致、是否被占用。 |
| HTTP 404 | 使用 `POST` 且路径准确为 `/api/hook`。 |
| HTTP 400 | JSON 格式正确，`state` 是五种允许值之一。 |
| HTTP 413 | 请求体超过 64 KB；缩短内容，不要上传完整日志或提示词。 |
| 桌宠无胶囊 | 查看设置页最近事件；确认宠物窗口未关闭，且运行的是包含状态接入功能的版本。 |
| Stop 后出现完成气泡但任务实际未成功 | 这是结束事件语义不等于业务成功；需按具体 Agent 的可靠成功事件重新映射，或避免发送完成事件。 |
| 同时运行多个 Agent 时显示不准确 | 当前实现只有全局最近事件，未做会话隔离；串行测试或停止并发来源。 |

## 9. 安全与隐私

接收器仅监听本机回环地址，但当前接口没有 Token 鉴权。仅允许可信的本地 Hook／转发器访问，不要将服务转发到公网。事件标题与输出可能包含项目名、文件名或其他敏感内容；发送最少必要信息，不要记录或转发 API Key、密码、完整提示词、源文件内容或私人数据。
