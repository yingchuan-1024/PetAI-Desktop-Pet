using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace taskt.UI
{
    // PetAI display-only localization. Never change command IDs, Tag, input values or XML.
    internal static class ChineseUI
    {
        private static readonly ConditionalWeakTable<Control, object> attached = new ConditionalWeakTable<Control, object>();
        private static readonly ConditionalWeakTable<ToolStripItem, object> menuAttached = new ConditionalWeakTable<ToolStripItem, object>();
        private static readonly Dictionary<string, string> text = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private static readonly Dictionary<string, string> words = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        public static int TranslatedCount { get; private set; }

        static ChineseUI()
        {
            Add(text, @"File Actions|文件操作
Edit|编辑
Options|选项
Script Actions|脚本操作
Save and Run|保存并运行
Help|帮助
New File|新建文件
Open File|打开文件
Import File|导入文件
Sample/Templete (M)|示例与模板
Save|保存
Save As|另存为
Restart Application|重启应用
Close Application (X)|退出应用
Undo|撤销
Redo|重做
Edit This Action|编辑此操作
Help This Command (Web)|命令帮助（网页）
Where This Command|定位此命令
Enable Selected Actions|启用所选操作
Disable Selected Actions|禁用所选操作
Enable Selected Action|启用所选操作
Disable Selected Action|禁用所选操作
Pause Before Execution|执行前暂停
Don't Pause Before Execution|取消执行前暂停
Select All|全选
Cut (X)|剪切
Copy|复制
Paste (V)|粘贴
Delete Selected Actions|删除所选操作
Search|搜索
Replace|替换
Highlight This Command|高亮此命令
Clear Highlights (L)|清除高亮
Clear Highlight (L)|清除高亮
Variable Manager|变量管理
Script Informations|脚本信息
Settings Manager (Classic)|设置管理（经典）
Settings Manager (new)|设置管理（新版）
Show Script Folder|打开脚本文件夹
Show Log Folder|打开日志文件夹
Show Search Bar|显示搜索栏
GUI Inspect Tool|界面元素检查工具
JSONPath Helper|JSONPath 助手
Format Checker|格式检查器
Record|录制
Schedule|计划任务
Run w/o Saving|直接运行
Run Script without Saving|不保存直接运行脚本
Search Keyword|搜索关键词（中文或英文）
taskt Project Page|taskt 项目主页
taskt Wiki|taskt 使用文档
taskt Gitter Chat|taskt 交流社区
Release|版本发布
Issue|问题反馈
About|关于
View Gitter/Chat|查看交流社区
Start Edit Script|开始编辑脚本
there were no script files found in your script directory.|脚本目录中没有找到脚本文件。
No Recent Files Found|没有最近使用的文件
Recent Files|最近使用的文件
View Documentation/Wiki|查看使用文档
Request Enhancement or Report a bug|提出建议或反馈问题
View Latest Releases|查看最新版本
View Project on GitHub|查看 GitHub 项目
Welcome to taskt!|欢迎使用 taskt！
Start building automation by double-clicking a command from the list to the left.|双击左侧列表中的命令，开始创建自动化脚本。
Script Commands|脚本命令
Save and Close|保存并关闭
Import|导入
Keep|保留
Close|关闭
New|新建
Open|打开
Record and Run|录制与运行
Run|运行
Variables and Settings|变量与设置
Clear|清空
Settings|设置
Variables|变量
Multi Enter Keys Edit|批量编辑按键
Cut Selected Action(s) (X)|剪切所选操作
Copy Selected Action(s)|复制所选操作
Paste Selected Action(s) (V)|粘贴所选操作
Delete Selected Action(s)|删除所选操作
Insert Comment|插入注释
Above Here|在上方
Below Here|在下方
Move Out To Parent|移到上一级
View Code (I)|查看代码
Show Script Informations|显示脚本信息
Expand|展开
Collapse|折叠
Clear Filter (L)|清除筛选
Add This Action|添加此操作
Sample This Command|此命令的示例
Cancel|取消
OK|确定
Yes|是
No|否
Add|添加
Update|更新
Delete|删除
Remove|移除
Help Command|命令帮助
Add New Command|添加命令
Edit Command|编辑命令
Command|命令
Command Name|命令名称
Command Description|命令说明
Description|说明
Comment|注释
Name|名称
Value|值
Type|类型
Output|输出
Input|输入
Browse|浏览
Browse...|浏览…
Select|选择
Enabled|启用
Advanced|高级
General|常规
Ready|就绪
Ready!|就绪
Stopped|已停止
Running|正在运行
Paused|已暂停
Pause|暂停
Resume|继续
Stop|停止
Exit|退出
Start|开始
Finish|完成
Next|下一步
Back|上一步
Execution Log|执行日志
Script Engine|脚本执行器
Script Execution|脚本执行
Show Variables|显示变量
Hide Variables|隐藏变量
screen recorder|屏幕录制器
Screen Recorder|屏幕录制器
Capture Options|捕获选项
Stop Capture HotKey|停止录制快捷键
Other Options|其他选项
Command Grouping Options|命令分组选项
Track Activated Window Position|记录激活窗口的位置
Track Activated Window Sizes|记录激活窗口的大小
Open Activated Windows Top Left|将激活窗口移到左上角
Mouse Move Sampling (ms)|鼠标移动采样间隔（毫秒）
Capture Window Events|捕获窗口事件
Capture Mouse Clicks|捕获鼠标点击
Capture Mouse Movements|捕获鼠标移动
Capture Keyboard Events|捕获键盘事件
Capture Keyboard|捕获键盘
Group Into Sequence|合并为序列
Group Mouse Moves Into Sequences|将鼠标移动合并为序列
Press 'F2' Key to stop recording!|按 F2 键停止录制！
nothing :-(|未找到匹配命令
Window Name|窗口名称
Window Title|窗口标题
Window Handle|窗口句柄
File Path|文件路径
Folder Path|文件夹路径
Source File|源文件
Destination File|目标文件
Source Folder|源文件夹
Destination Folder|目标文件夹
Timeout|超时
Timeout (ms)|超时（毫秒）
X Position|X 坐标
Y Position|Y 坐标
Mouse Button|鼠标按钮
Click Type|点击类型
Enter Text|输入文本
Enter Keys|输入按键
Variable Name|变量名称
Please select a command|请选择命令
Element Name|元素名称
Element ID|元素 ID
Automation ID|自动化 ID
Control Type|控件类型
Selector|选择器
URL|网址
Browser Name|浏览器名称
Browser Instance Name|浏览器实例名称
XPath|XPath
Delay|延迟
Milliseconds|毫秒
Seconds|秒");
            Add(text, @"Group Mouse Moves as Sequences (Less Clutter)|将鼠标移动合并为序列（更简洁）
Capture Mouse Moves|捕获鼠标移动
Capture Keyboard Input|捕获键盘输入
Group All Into Sequence|将全部操作合并为序列
Use AutomationId Attribute|使用 AutomationId 属性
Use Name Attribute|使用 Name 属性
Copied!!|已复制！
Auto Reload (5s)|自动刷新（5秒）
XPath is Relative to the checked element|XPath 相对于勾选的元素
Hide Check|隐藏勾选标记
Variable Name to Store Process Name|保存进程名称的变量
Variable Name to Store Window Name|保存窗口名称的变量
When Window Not Found|未找到窗口时
Variable Name to Recieve the Window Position X|接收窗口 X 坐标的变量
Variable Name to Recieve the Window Position Y|接收窗口 Y 坐标的变量
Variable Name to Recieve the Window Width|接收窗口宽度的变量
Variable Name to Recieve the Window Height|接收窗口高度的变量
Base position|基准位置
State of the Window|窗口状态
Welcome! Press 'Start Edit Script' to get started!|欢迎！点击“开始编辑脚本”即可开始。
No code was selected!|尚未选择操作！
Error! can not paste commands(s).|无法粘贴所选命令。
Error Parsing File: Commands not found!|解析文件失败：未找到命令。
Script Loaded Successfully!|脚本加载成功！
Script Imported Successfully!|脚本导入成功！
You must have at least 1 automation command to save.|至少添加一条自动化命令才能保存。
Please verify the ordering of your try/catch blocks.|请检查 try/catch 异常处理块的顺序。
Please verify the ordering of your loops.|请检查循环块的顺序。
Please verify the ordering of your ifs.|请检查条件判断块的顺序。
File has been saved successfully!|文件保存成功！
You must first build the script by adding commands!|请先添加命令来创建脚本！
You must first save your script before you can run it! Or use 'Run w/o Saving'.|请先保存脚本，或选择“直接运行”。
Running Script.|正在运行脚本。
Sample|示例
Compile|编译
Accept|接受
Clear Result Close|清除结果并关闭
Load DLL|加载 DLL
Launch Task Scheduler|打开任务计划程序
Capture Mouse Click Events|捕获鼠标点击事件
Capture Mouse Move Events|捕获鼠标移动事件
Group Keyboard Commands into Sequences|将键盘命令合并为序列
Group Mouse Move Commands into Sequences|将鼠标移动合并为序列
taskt is executing|taskt 正在执行脚本
taskt is paused|taskt 已暂停
Bot Engine|脚本执行器
Press the 'Pause/Break' key to stop automation.|按 Pause/Break 键停止自动化。
Action...|正在执行操作…
Status Message|状态消息
Capture Mouse Position|捕获鼠标坐标
Keys Builder|按键构建器
Encrypt Text|加密文本
Text or Keys to Send.|要发送的文本或按键
Text is Encrypted or Not|文本是否已加密
Use Paste from Clipboard|通过剪贴板粘贴
Clear Clipboard After Paste|粘贴后清空剪贴板
Try Activate Window, when Specifiy Current Window Variable|使用当前窗口变量时尝试激活窗口
X Position to Move the Mouse to|鼠标目标 X 坐标
Y Position to Move the Mouse to|鼠标目标 Y 坐标
Ignore Wait Time When Click Type is 'None'|点击类型为 None 时忽略等待时间
Shortcut Key to Enter|要输入的快捷键
Window to Send Keys to|接收按键的窗口
Window to Activate|要激活的窗口
Window to Close|要关闭的窗口
Window to Resize|要调整大小的窗口
Window to Move|要移动的窗口
File to Copy|要复制的文件
File to Move|要移动的文件
File to Delete|要删除的文件
Folder to Create|要创建的文件夹
Folder to Delete|要删除的文件夹
Destination Path|目标路径
Source Path|源路径
Overwrite Existing File|覆盖已有文件
Wait Time (ms)|等待时间（毫秒）
Capture Mouse|捕获鼠标移动
Capture Clicks|捕获鼠标点击
Capture Keyboard Events|捕获键盘事件
Group Mouse Movements into Sequences|将鼠标移动合并为序列
Group Commands into Sequence|将命令合并为序列
Select Window|选择窗口
Refresh Windows|刷新窗口列表
Inspect Element|检查元素
Start Recording|开始录制
Stop Recording|停止录制
Record Sequence|录制操作序列
Sequence Recorder|操作录制器
Command Editor|命令编辑器
Key/Mouse|键盘鼠标
Application/Script|应用与脚本
File/Folder|文件与文件夹
Wait For Window|等待窗口
Click Mouse|点击鼠标
Move Mouse|移动鼠标
Enter Keys|输入按键
Enter Shortcut Key|输入快捷键");
            Add(text, @"Application Path or Application Name|应用程序路径或名称
Arguments|启动参数
Wait for the Application to Complete|等待应用程序运行结束
Variable Name to Store Application Process Name|保存应用程序进程名称的变量
Wait Time until Application Starts (ms)|等待应用程序启动的时间（毫秒）
Wait Time before Executing Next Command (ms)|执行下一条命令前的等待时间（毫秒）
Wait Time for the Appliaction to Exist (sec)|等待应用程序出现的时间（秒）
Select a File|选择文件
Select a Folder|选择文件夹
Insert Variable|插入变量
Insert System Variable|插入系统变量
Select Variable|选择变量
Select Window Name|选择窗口名称
Variable Name to Receive the Output|接收输出的变量名称
Variable Name to Store Result|保存结果的变量名称
Variable Name to Store Window Handle|保存窗口句柄的变量名称
Script Code|脚本代码
Script File Path|脚本文件路径
Expand taskt Variables In Code|展开代码中的 taskt 变量
Delete Script File After Execute|执行后删除脚本文件
Folder to Save Temporary Script File|保存临时脚本的文件夹
Variable Name to Store Temporary Script File Path|保存临时脚本路径的变量
Text to Send|要发送的文本
Text or Keys to Send|要发送的文本或按键
Text to Write|要写入的文本
Text to Append|要追加的文本
Text to Find|要查找的文本
Text to Replace|要替换的文本
Replacement Text|替换后的文本
Text to Split|要分割的文本
Delimiter|分隔符
Instance Name|实例名称
Browser Instance|浏览器实例
Process Name|进程名称
File Name|文件名称
Folder Name|文件夹名称
Destination File Path|目标文件路径
Source File Path|源文件路径
Wait Time|等待时间
Wait Time (sec)|等待时间（秒）
Pause Length (ms)|暂停时长（毫秒）
Number of Times to Loop|循环次数
Loop Type|循环类型
Condition Type|条件类型
Comparison Type|比较类型
Variable Name to Store Text|保存文本的变量
Variable Name to Store List|保存列表的变量
Variable Name to Store DataTable|保存数据表的变量
List Variable Name|列表变量名称
DataTable Variable Name|数据表变量名称
Dictionary Variable Name|字典变量名称
Window Name to Activate|要激活的窗口名称
Window Name to Close|要关闭的窗口名称
Window Name to Send Keys to|接收按键的窗口名称
Manage settings used by the application|管理应用程序使用的设置
Application|应用程序
Application Settings|应用程序设置
Automation Engine|自动化引擎
Editor|编辑器
Debug|调试
Folder|文件夹
Other|其他
Script File|脚本文件
Script Metric|脚本统计
Settings File|设置文件
Start Up|启动
Engine|引擎
Keyword|关键字
Log|日志
Parser|解析器
UIElement Inspect|界面元素检查
UIElement Inspcet (beta)|界面元素检查（测试版）
Variable|变量
Documents|文档
Command Reference|命令参考
Command List|命令列表
Indent|缩进
Instance|实例
Insert Command|插入命令
Menu Bar|菜单栏
Mini Map|脚本缩略图
Status Bar|状态栏
Validation|校验
Network|网络
Local Listener|本地监听
Local Listener (Beta)|本地监听（测试版）
Local Listener Settings (BETA)|本地监听设置（测试版）
Server|服务器
Server Settings|服务器设置
Check Update|检查更新
Check For Updates|检查更新
Display Manager|显示管理器
Display Manager for VMs|虚拟机显示管理器
VM|虚拟机
Anti-Idle (while app is open)|应用打开时防止系统空闲
Load Commands at Startup (Reduces Flicker)|启动时加载命令（减少闪烁）
Start Mode|启动模式
Launch Attended Mode|打开有人值守模式
taskt Root Folder|taskt 根文件夹
Attended Tasks Folder|有人值守任务文件夹
Attended Tasks Folder (Default Folder for saving Script Files)|有人值守任务文件夹（保存脚本文件的默认目录）
Show Debug Window|显示调试窗口
Show Debug Window when Script Execute|脚本执行时显示调试窗口
Automatically Close Debug Window|自动关闭调试窗口
Show Advanced Debug Logs During Execution|执行时显示高级调试日志
Track Execution Metrics|记录执行统计
Script Execution Metrics (Last 10 per Script)|脚本执行统计（每个脚本最近 10 次）
Getting Metrics...|正在读取统计…
Clear Metrics|清空统计
No Metrics Found|没有统计记录
Metrics Unavailable - Metrics are only available after running tasks which will generate metrics logs|暂无统计；运行脚本并生成统计日志后可查看
Minimize to System Tray|最小化到系统托盘
Resources Folder|资源文件夹
If you want to update the WebDriver (chromedriver.exe etc), open the Resources folder and overwrite the file.|如需更新 WebDriver（chromedriver.exe 等），请打开 Resources 文件夹并替换对应文件。
Open 'Resources' Folder|打开 Resources 文件夹
Check WebDrivers|检查 WebDriver
Chrome Driver|Chrome 驱动
Edge Driver|Edge 驱动
IE Driver|IE 驱动
Intermediate Script File|中间脚本文件
Export Intermediate Script File|导出中间脚本文件
Auto Save Script File|自动保存脚本文件
Enable Auto Save Script File|启用脚本自动保存
Auto Save Interval|自动保存间隔
minute(s) [1-120]|分钟 [1-120]
Delete Auto Saved Script Files that are more than|删除自动保存脚本，保留天数：
days old|天
Show 'AutoSave' Folder|打开 AutoSave 文件夹
Run Without Saving Script File|不保存直接运行的脚本文件
Delete 'Run without Saving' Script Files that are more than|删除直接运行的临时脚本，保留天数：
Show 'RunWithoutSaving' Folder|打开 RunWithoutSaving 文件夹
Before Converted Script File|转换前的脚本文件
Delete 'Before Converted' Script Files that are more than|删除转换前的脚本备份，保留天数：
Show 'BeforeConverted' Folder|打开 BeforeConverted 文件夹
Import Settings|导入设置
Export Settings|导出设置
Export|导出
Load Default Settings|加载默认设置
Load Default|加载默认值
Start Marker|起始标记
End Marker|结束标记
If Start Maker and End Marker are the same, the variable may not expand properly.|起始和结束标记相同时，变量可能无法正常展开。
Calculate Automatically|自动计算
Calculate Variables Automatically|自动计算变量
Use New Parser (beta)|使用新版解析器（测试版）
Use New Parser (New Parser is beta)|使用新版解析器（测试版）
Ignore First Variable Marker In Output Parameter (Check is strongly recommended)|忽略输出参数的首个变量标记（强烈建议勾选）
Depath of Nodes|节点深度
Number of Sibling Nodes|同级节点数量
IMPORTANT|重要提示
These keywords are no longer supported. Please use System Variables from now on.|这些关键字已不再受支持，请改用系统变量。
Window Keyword|窗口关键字
Current window keyword|当前窗口关键字
Current Window Keyword : Currently Window.CurrentWindowName|当前窗口关键字：现为 Window.CurrentWindowName
Excel Keyword|Excel 关键字
Current Worksheet Keyword : Currently Excel.CurrentWorksheet|当前工作表关键字：现为 Excel.CurrentWorksheet
Next Worksheet Keyword : Currently Excel.NextWorksheet|下一工作表关键字：现为 Excel.NextWorksheet
Previous Worksheet Keyword : Currently Excel.PreviousWorksheet|上一工作表关键字：现为 Excel.PreviousWorksheet
Override App Instances|覆盖已有应用实例
Default delay between executing commands (ms)|命令之间的默认延迟（毫秒）
End Script Hotkey|停止脚本快捷键
Create Missing Variables During Execution|执行时创建缺失变量
Enable Diagnostic Logging|启用诊断日志
Create Command Reference|生成命令参考
Generate Documentation|生成文档
Use Slim Menu Bar (required restart)|使用精简菜单栏（需重启）
Use Slim Action Bar (Restart required)|使用精简操作栏（需重启）
Show Command Search Box when taskt is started (required restart)|启动时显示命令搜索框（需重启）
Gruoping by subgroup (Restart required)|按子组分组（需重启）
Support IE Commands (Restart required)|支持 IE 命令（需重启）
Command Search|命令搜索
Command Search Settings|命令搜索设置
Make Group Name a Search Target|搜索范围包含分组名称
Show All Commands if Group Name Matches|分组名称匹配时显示组内全部命令
Make SubGroup Name a Search Target|搜索范围包含子组名称
Show All Commands if SubGroup Name Matches|子组名称匹配时显示组内全部命令
Instance Sort|实例排序
Instance Name Sort Order|实例名称排序方式
Default Instance Name|默认实例名称
Default Database Instance Name|默认数据库实例名称
Default Excel Instance Name|默认 Excel 实例名称
Default NLG Instance Name|默认 NLG 实例名称
Default StopWatch Instance Name|默认秒表实例名称
Default Stopwach instance name|默认秒表实例名称
Default WebBrowser Instance Name|默认浏览器实例名称
Default browser instance name|默认浏览器实例名称
Default Word Instance Name|默认 Word 实例名称
Don't Show Default Instance When Multiple Instance Exists (partial support)|存在多个实例时隐藏默认实例（部分支持）
New Commands Insert Below Selected Command|在所选命令下方插入新命令
Allow Drag and Drop into Sequence Commands|允许拖放到序列命令中
Insert Else when BeginIf command inserted|插入 BeginIf 时自动插入 Else
Insert Comment above If, Loop, Try|在 If、Loop、Try 上方插入注释
Show Script Mini Map (beta)|显示脚本缩略图（测试版）
Show Indent Line|显示缩进参考线
Indent Width (1 to 32)|缩进宽度（1 至 32）
Insert variable at cursor position(Textbox / Combobox)|在光标位置插入变量（文本框／下拉框）
Insert variable at cursor position (Textbox/Combobox)|在光标位置插入变量（文本框／下拉框）
Don't show Script Command Validation Message|隐藏脚本命令校验提示
Hide Status Bar Automatically (reqired restart)|自动隐藏状态栏（需重启）
Remember Command Editor Size and Position|记住命令编辑器大小和位置
Remember the Position of the Supplemental Forms for Command Editor|记住命令编辑器辅助窗口的位置
Try Polite Text In Description (beta)|字段说明使用引导语（测试版）
Show Sample Usage In Description|字段说明显示使用示例
Show Default Value In Description If Optional|可选字段说明显示默认值
Change Items with Mouse Wheel when NOT Forcused|未聚焦时允许滚轮切换选项
Display Parameter Number Before Description|字段说明前显示参数序号
Enable this functionality to allow this computer to accept script execution requests from other taskt or REST-capable clients.|启用后，此计算机可接收其他 taskt 或支持 REST 的客户端发来的脚本执行请求。
Start Listening on Startup|启动时开始监听
Local Listening Enabled|启用本地监听
Listening Port|监听端口
Require Authentication Key|需要身份验证密钥
Authentication Key|身份验证密钥
Regenerate|重新生成
Enable IP Verification (Seperate with comma)|启用 IP 验证（以逗号分隔）
Start Listening|开始监听
Stop Listening|停止监听
Listening on {}|监听地址：{}
Server Connection Enabled|启用服务器连接
Check In On Startup|启动时连接服务器
Retry If Connection Fails|连接失败时重试
Connect|连接
Connect Key|连接密钥
Bypass SSL Validation|跳过 SSL 证书验证
HTTPS Server URL|HTTPS 服务器网址
Enter the location of the taskt server (ex. https://localhost:60281|输入 taskt 服务器地址（例：https://localhost:60281
Enter the location of the taskt server ex. https://localhost:60281|输入 taskt 服务器地址，例：https://localhost:60281
Server URL ex. ws://localhost:port/ws)|服务器网址，例：ws://localhost:port/ws)
Client GUID|客户端 GUID
Indicates the GUID the client will use when connecting to taskt server|客户端连接 taskt 服务器时使用的 GUID
Test Connection|测试连接
Publish Task|发布任务
Socket Status|连接状态
Socket Exception|连接异常
Check for update at startup|启动时检查更新
Skip Beta version|跳过测试版本
Launch Display Manager|打开显示管理器
Clear Commands/Actions Filter|清除命令／操作筛选
Filter Commands/Actions|筛选命令／操作
Filter Commands/Actions keyword|筛选命令／操作的关键词
Save Script and Close|保存脚本并关闭
Close without save changes|关闭且不保存更改
Add Variable|添加变量
Edit Variables|编辑变量
Clear All Commands|清空所有命令");
            Add(text, @"Variable Name to Store Window Handle Result|保存窗口句柄结果的变量
Comment Field|注释内容
Variable Pattern|变量格式
Indicate the start and end markers for variables.  When the engine runs, it will replace the variable name and markers with the variable value.|指定变量的起始和结束标记。脚本运行时，变量名称及标记会被替换为变量值。
Imported|设置导入成功
Fail import|设置导入失败
Exported|设置导出成功
Fail export|设置导出失败
Are you sure to Load Default Settings?|确定要加载默认设置吗？
Connected Successfully!|连接成功！
Unable To Connect!|无法连接！
Change Default Root Folder|更改默认根文件夹
Data Migration Complete|数据迁移完成
Migrate Data?|迁移数据？
Gecko Driver|Gecko 驱动
geckodriver|Gecko 驱动
Help Command (Web)|命令帮助（网页）
This command allows you to start a program or a process.|此命令用于启动应用程序或进程。
Application Path or Name|应用程序路径或名称
Enable this functionality to connect to a local instance of taskt server for workforce management. After testing the connection, the client will be assigned a new GUID which must be approved by an administrator in the server.|启用后，可连接本地 taskt 服务器进行任务管理。测试连接后，客户端会获得新的 GUID，需由服务器管理员批准。
geckodriver (Firefox)|Gecko 驱动（Firefox）
Connected Successfully! GUID will be reloaded automatically the next time settings is loaded!|连接成功！下次加载设置时将自动重新加载 GUID。
Please open the task in order to publish it.|请先打开要发布的任务。
Close Settings form to launch Attended Mode. If you have changed the settings, click the 'OK' button to save the changes. Launch Attended Mode now ?|将关闭设置窗口并启动有人值守模式。如已修改设置，请先点击“确定”保存更改。现在启动有人值守模式吗？
Close Settings form to launch Display Manager. If you have changed the settings, click the 'OK' button to save the changes. Launch Display Manager now ?|将关闭设置窗口并启动显示管理器。如已修改设置，请先点击“确定”保存更改。现在启动显示管理器吗？
Would you like to change the default root folder that taskt uses to store tasks and information?|要更改 taskt 存储任务和信息的默认根文件夹吗？
Current Root Folder:|当前根文件夹：
Please confirm the changes below:|请确认以下更改：
Old Root Folder:|原根文件夹：
New Root Folder:|新根文件夹：
Would you like to attempt to move the data from the old folder to the new folder? Please note, depending on how many files you have, this could take a few minutes.|要将数据从原文件夹迁移到新文件夹吗？根据文件数量，此操作可能需要几分钟。");
            Add(words, @"API|API
Application Script|应用脚本
Color|颜色
Data|数据
Database|数据库
DataTable|数据表
DateTime|日期时间
Dialog Message|消息对话框
Dictionary|字典
Email|邮件
EMail|邮件
Engine|执行引擎
Error Handling|错误处理
Excel|Excel
File Operation|文件操作
Folder Operation|文件夹操作
If|条件判断
Image|图像
JSON|JSON
Key Mouse|键盘鼠标
List|列表
LocalListener|本地监听
Loop|循环
Math|数学
Misc|其他
Network|网络
Regex|正则表达式
Script|脚本
Text|文本
UI Automation|界面自动化
UIAutomation|界面自动化
Window|窗口
Web Browser|网页浏览器
Selenium Browser|Selenium 浏览器
IE Browser|IE 浏览器
Variable|变量
Word|Word
Commands|命令
Get|获取
Set|设置
Create|创建
Delete|删除
Remove|移除
Add|添加
Copy|复制
Move|移动
Open|打开
Close|关闭
Read|读取
Write|写入
Append|追加
Clear|清空
Check|检查
Exists|是否存在
Start|启动
Stop|停止
Run|运行
Execute|执行
Send|发送
Receive|接收
Recieve|接收
Load|加载
Unload|卸载
Save|保存
Convert|转换
To|为
From|从
As|为
By|按
With|使用
For|针对
In|在
Of|的
And|和
Or|或
Not|不
Begin|开始
End|结束
Try|尝试
Catch|捕获异常
Finally|最终处理
Throw|抛出
Exception|异常
Pause|暂停
Wait|等待
Sleep|等待
Comment|注释
Sequence|序列
Group|分组
Click|点击
Double|双击
Right|右键
Left|左键
Mouse|鼠标
Keyboard|键盘
Key|按键
Keys|按键
Keystrokes|按键
Press|按下
Release|释放
Position|位置
Coordinates|坐标
Cursor|光标
Find|查找
Search|搜索
Activate|激活
Resize|调整大小
Minimize|最小化
Maximize|最大化
Restore|还原
Focus|聚焦
Element|元素
Elements|元素
Attribute|属性
Attributes|属性
Browser|浏览器
Navigate|导航
Go|转到
Refresh|刷新
Switch|切换
Frame|框架
Tab|标签页
Tabs|标签页
File|文件
Files|文件
Folder|文件夹
Folders|文件夹
Directory|目录
Path|路径
Name|名称
Title|标题
Handle|句柄
Process|进程
Application|应用程序
Script|脚本
Task|任务
Current|当前
New|新建
Existing|已有
Text|文本
String|字符串
Number|数字
Value|值
Values|值
Item|项
Items|项
Index|索引
Count|数量
Length|长度
Size|大小
Time|时间
Date|日期
Format|格式
Replace|替换
Split|分割
Join|合并
Filter|筛选
Sort|排序
Reverse|反转
Trim|去除空白
Compare|比较
Contains|包含
Empty|为空
Null|空值
Output|输出
Input|输入
Result|结果
Results|结果
Message|消息
Dialog|对话框
Prompt|提示
Show|显示
Hide|隐藏
Log|日志
Data|数据
Table|表
Row|行
Rows|行
Column|列
Columns|列
Cell|单元格
Range|区域
Sheet|工作表
Worksheet|工作表
Workbook|工作簿
Connection|连接
Query|查询
Define|定义
List|列表
Dictionary|字典
Key|键
Image|图像
Screenshot|截图
Capture|捕获
Screen|屏幕
Image|图像
Delay|延迟
Timeout|超时
Error|错误
Handling|处理
Loop|循环
Next|下一个
Exit|退出
While|当
Else|否则
If|如果
Condition|条件
Statement|语句
Boolean|布尔值
True|真
False|假
Email|邮件
Attachment|附件
Attachments|附件
Address|地址
Addresses|地址
Subject|主题
Body|正文
Password|密码
User|用户
Username|用户名
Server|服务器
Port|端口
Request|请求
Response|响应
HTTP|HTTP
REST|REST
API|API
JSON|JSON
XML|XML
PDF|PDF
HTML|HTML
XPath|XPath
JavaScript|JavaScript
PowerShell|PowerShell
Custom|自定义
Code|代码
Command|命令
Select|选择
Selected|所选
Enable|启用
Disable|禁用
Match|匹配
Matches|匹配项
Regex|正则表达式
Random|随机
Calculate|计算
Clipboard|剪贴板
Hotkey|快捷键
HotKey|快捷键
Milliseconds|毫秒
Seconds|秒
Wait|等待
Until|直到
Property|属性
Properties|属性
Control|控件
Inspect|检查
Desktop|桌面
Remote|远程
Session|会话
Instance|实例
Preference|首选项
Context|上下文
Information|信息
Informations|信息");
        }

        private static void Add(Dictionary<string, string> target, string entries)
        {
            foreach (var line in entries.Split('\n'))
            {
                var pair = line.Trim().Split('|');
                if (pair.Length == 2) target[Regex.Replace(pair[0], @"\s+", " ")] = pair[1];
            }
        }

        public static string Translate(string value)
        {
            if (string.IsNullOrEmpty(value)) return value;
            var key = Regex.Replace(value.Replace("&", "").Trim(), @"\s+", " ");
            string translated;
            if (text.TryGetValue(key, out translated)) return translated;
            if (key.EndsWith(":"))
            {
                var bare = key.Substring(0, key.Length - 1).Trim();
                if (text.TryGetValue(bare, out translated)) return translated + "：";
            }
            var description = Regex.Match(key, @"^(?<number>\d+\s+)?(?<optional>Optional\s*-\s*)?(?:Please\s+(?:Specify|Select|Enter|Indicate|Input)\s+(?:the\s+)?)?(?<body>.+?)(?<suffix>\s*\((?:ex\.|Default is).*)?$", RegexOptions.IgnoreCase);
            if (description.Success && text.TryGetValue(description.Groups["body"].Value.Trim(), out translated))
            {
                var suffix = description.Groups["suffix"].Value;
                suffix = Regex.Replace(suffix, @"\(ex\.\s*([^)]*)\)", m => "（例：" + m.Groups[1].Value + "）", RegexOptions.IgnoreCase);
                suffix = Regex.Replace(suffix, @"\(Default is\s+([^)]*)\)", m => "（默认值：" + Translate(m.Groups[1].Value) + "）", RegexOptions.IgnoreCase);
                return description.Groups["number"].Value + (description.Groups["optional"].Success ? "可选 - " : "") + translated + suffix;
            }
            var driverVersion = Regex.Match(key, @"^(Chrome Driver|Edge Driver|geckodriver|IE Driver) Version Result: (.*)$");
            if (driverVersion.Success) return Translate(driverVersion.Groups[1].Value) + "版本：" + driverVersion.Groups[2].Value;
            if (key == "Client is Not Listening!") return "客户端尚未开始监听！";
            if (key.StartsWith("Client is Listening at Endpoint '", StringComparison.Ordinal))
                return "客户端正在监听：" + key.Substring(33);
            if (key.StartsWith("Start building automation by double-clicking a command from the list to the left.", StringComparison.Ordinal))
                return "双击左侧列表中的命令，开始创建自动化脚本。";
            return value;
        }

        public static string RecordingHint(string stopKey)
        {
            return "按 " + stopKey + " 键停止录制！";
        }

        public static string CommandDisplay(string value)
        {
            var direct = Translate(value);
            if (direct != value) return direct;
            string translated;
            if (words.TryGetValue(value, out translated)) return translated;
            if (value.EndsWith(" Commands", StringComparison.Ordinal))
                return CommandDisplay(value.Substring(0, value.Length - 9)) + "命令";
            return Regex.Replace(value, @"[A-Za-z]+", m => words.TryGetValue(m.Value, out translated) ? translated : m.Value);
        }

        public static bool Matches(string value, string keyword)
        {
            return value.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0 ||
                CommandDisplay(value).IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        public static void Install()
        {
            Application.Idle += delegate
            {
                foreach (Form form in Application.OpenForms.Cast<Form>().ToArray()) Apply(form);
            };
        }

        public static void Apply(Control control)
        {
            object marker;
            if (!attached.TryGetValue(control, out marker))
            {
                attached.Add(control, new object());
                if (control is Label || control is ButtonBase || control is GroupBox || control is Form || control is TabPage)
                {
                    control.TextChanged += delegate { TranslateControl(control); };
                    TranslateControl(control);
                }
                var picture = control as CustomControls.UIPictureButton;
                if (picture != null) picture.DisplayText = Translate(picture.DisplayText);
                control.ControlAdded += (sender, e) => Apply(e.Control);
                var commandChoice = control as ComboBox;
                if (commandChoice != null && commandChoice.Name == "cboSelectedCommand")
                {
                    commandChoice.DrawMode = DrawMode.OwnerDrawFixed;
                    commandChoice.DrawItem += (sender, e) =>
                    {
                        e.DrawBackground();
                        if (e.Index >= 0)
                            TextRenderer.DrawText(e.Graphics, CommandDisplay(commandChoice.GetItemText(commandChoice.Items[e.Index])), e.Font, e.Bounds, e.ForeColor, TextFormatFlags.NoPrefix | TextFormatFlags.VerticalCenter);
                        e.DrawFocusRectangle();
                    };
                }
                var tree = control as TreeView;
                if (tree != null && (tree.Name == "tvCommands" || tree.Name == "tvSettingsMenu"))
                {
                    tree.DrawMode = TreeViewDrawMode.OwnerDrawText;
                    tree.ShowNodeToolTips = true;
                    tree.DrawNode += DrawCommandNode;
                }
            }
            var form = control as Form;
            if (form != null)
            {
                var field = form.GetType().GetField("components", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                var components = field == null ? null : field.GetValue(form) as System.ComponentModel.IContainer;
                if (components != null)
                    foreach (var tooltip in components.Components.OfType<ToolTip>())
                        foreach (var child in Descendants(form))
                        {
                            var original = tooltip.GetToolTip(child);
                            var localized = Translate(original);
                            if (localized != original) tooltip.SetToolTip(child, localized);
                        }
            }
            var pictureButton = control as CustomControls.UIPictureButton;
            if (pictureButton != null) pictureButton.DisplayText = Translate(pictureButton.DisplayText);
            var strip = control as ToolStrip;
            if (strip != null) foreach (ToolStripItem item in strip.Items) ApplyMenu(item);
            if (control.ContextMenuStrip != null)
                foreach (ToolStripItem item in control.ContextMenuStrip.Items) ApplyMenu(item);
            var list = control as ListView;
            if (list != null) foreach (ColumnHeader column in list.Columns) column.Text = Translate(column.Text);
            foreach (Control child in control.Controls) Apply(child);
        }

        private static void TranslateControl(Control control)
        {
            var localized = Translate(control.Text);
            if (localized == control.Text) return;
            control.Text = localized;
            TranslatedCount++;
            var label = control as Label;
            if (label != null && !label.AutoSize) label.AutoEllipsis = true;
        }

        private static void ApplyMenu(ToolStripItem item)
        {
            if (item is ToolStripTextBox || item is ToolStripComboBox) return;
            object marker;
            if (!menuAttached.TryGetValue(item, out marker))
            {
                menuAttached.Add(item, new object());
                item.TextChanged += delegate { TranslateMenuText(item); };
                TranslateMenuText(item);
            }
            item.ToolTipText = Translate(item.ToolTipText);
            var dropdown = item as ToolStripDropDownItem;
            if (dropdown != null) foreach (ToolStripItem child in dropdown.DropDownItems) ApplyMenu(child);
        }

        private static void TranslateMenuText(ToolStripItem item)
        {
            var localized = Translate(item.Text);
            if (localized != item.Text) { item.Text = localized; TranslatedCount++; }
        }

        private static void DrawCommandNode(object sender, DrawTreeNodeEventArgs e)
        {
            if (e.Bounds.Width == 0) return;
            var tree = (TreeView)sender;
            var selected = (e.State & TreeNodeStates.Selected) != 0;
            var background = selected ? SystemColors.Highlight : tree.BackColor;
            var foreground = selected ? SystemColors.HighlightText : e.Node.ForeColor.IsEmpty ? tree.ForeColor : e.Node.ForeColor;
            var display = CommandDisplay(e.Node.Text);
            var width = Math.Max(e.Bounds.Width, TextRenderer.MeasureText(display, tree.Font).Width);
            var bounds = new Rectangle(e.Bounds.X, e.Bounds.Y, width, e.Bounds.Height);
            using (var brush = new SolidBrush(background)) e.Graphics.FillRectangle(brush, bounds);
            TextRenderer.DrawText(e.Graphics, display, tree.Font, bounds, foreground, TextFormatFlags.NoPrefix | TextFormatFlags.VerticalCenter);
        }

        private static IEnumerable<Control> Descendants(Control parent)
        {
            foreach (Control child in parent.Controls)
            {
                yield return child;
                foreach (var descendant in Descendants(child)) yield return descendant;
            }
        }

        private static void CheckEditorAndSettings(Forms.ScriptBuilder.frmScriptBuilder builder, Forms.ScriptBuilder.CommandEditor.frmCommandEditor editor)
        {
            typeof(App).GetProperty("Taskt_Settings").GetSetMethod(true).Invoke(null, new object[] { new Core.SafeApplicationSettings(new Core.ApplicationSettings()) });
            editor.appSettings = App.Taskt_Settings;
            editor.scriptVariables = new List<Core.Script.ScriptVariable>();
            CustomControls.CommandControls.CurrentEditor = editor;
            var command = new Core.Automation.Commands.StartApplicationCommand();
            var automation = new CustomControls.AutomationCommand { Command = command, CommandClass = command.GetType() };
            automation.RenderUIComponents(editor);
            var panel = (FlowLayoutPanel)editor.Controls.Find("flw_InputVariables", true).First();
            panel.Controls.AddRange(automation.UIControls.ToArray());
            Apply(editor);
            editor.PerformLayout(); panel.PerformLayout();
            var labels = Descendants(panel).OfType<Label>().ToArray();
            foreach (var expected in new[] { "应用程序路径或名称", "启动参数", "等待应用程序运行结束", "保存应用程序进程名称的变量" })
                if (!labels.Any(l => l.Text.Contains(expected))) throw new InvalidOperationException("Missing dynamic description: " + expected);
            if (labels.Any(l => l.Text.Contains("Please Specify") || l.Text.Contains("Please Select") || l.Text.Contains("Optional -") || l.Text.Contains("Default is")))
                throw new InvalidOperationException("English dynamic description remained: " + string.Join("; ", labels.Select(l => l.Text)));
            var wait = Descendants(panel).OfType<ComboBox>().First(c => c.Name == "v_WaitForExit");
            if (!wait.Items.Cast<object>().Select(Convert.ToString).SequenceEqual(new[] { "Yes", "No" }))
                throw new InvalidOperationException("Application wait business values changed");
            var helpers = Descendants(panel).OfType<CustomControls.CommandItemControl>().ToArray();
            if (!helpers.Any(h => h.CommandDisplay == "Select a File" && Translate(h.CommandDisplay) == "选择文件") ||
                !helpers.Any(h => h.CommandDisplay == "Insert Variable" && Translate(h.CommandDisplay) == "插入变量"))
                throw new InvalidOperationException("Dynamic helper display failed");
            foreach (var label in labels.Where(l => l.Name.StartsWith("lbl_")))
                if (label.Parent.Controls.Cast<Control>().Any(c => c != label && c.Bounds.IntersectsWith(label.Bounds)))
                    throw new InvalidOperationException("Dynamic label overlaps input: " + label.Name);
            using (var classic = new Forms.ScriptBuilder.Supplemental.frmSettings(builder))
            using (var settings = new Forms.ScriptBuilder.Supplemental.frmNewSettings(builder))
            {
                Apply(classic); Apply(settings);
                if (classic.Text != "设置" || settings.Text != "设置" || Descendants(classic).OfType<TabPage>().Any(t => Translate(t.Text) != t.Text))
                    throw new InvalidOperationException("Settings captions failed");
                var type = settings.GetType();
                type.GetField("newAppSettings", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).SetValue(settings, new Core.ApplicationSettings());
                foreach (var method in new[] { "showApplicationStartUpSetting", "showApplicationDebugSettings", "showApplicationFolderSettings", "showApplicationScriptFileSettings", "showApplicationSettingsFile", "showEditorCommandEditorSettings", "showAutomationEngineEngineSettings", "showAutomationEngineParserSettings", "showNetworkServerSettings" })
                {
                    type.GetMethod(method, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).Invoke(settings, null);
                    Apply(settings); settings.PerformLayout();
                    var flow = (FlowLayoutPanel)settings.Controls.Find("flowLayoutSettings", true).First();
                    flow.PerformLayout();
                    var captions = Descendants(flow).Where(c => c is Label || c is Button || c is CheckBox).Select(c => c.Text).ToArray();
                    if (!captions.Any(c => Regex.IsMatch(c, @"[\u4e00-\u9fff]")) || captions.Any(c => Translate(c) != c))
                        throw new InvalidOperationException("Settings page translation failed: " + method);
                    if (method == "showApplicationStartUpSetting")
                    {
                        var modes = (ComboBox)flow.Controls.Find("cmbStartMode", true).First();
                        if (!modes.Items.Cast<object>().Select(Convert.ToString).SequenceEqual(new[] { "Builder Mode", "Attended Task Mode" }))
                            throw new InvalidOperationException("Startup business values changed");
                    }
                    foreach (Control child in flow.Controls.Cast<Control>().Where(c => c.Name != "chkPreLoadCommands"))
                        if (flow.Controls.Cast<Control>().Any(c => c != child && c.Name != "chkPreLoadCommands" && c.Bounds.IntersectsWith(child.Bounds)))
                            throw new InvalidOperationException("Settings controls overlap: " + method + "/" + child.Name);
                }
                var tree = (TreeView)settings.Controls.Find("tvSettingsMenu", true).First();
                if (tree.DrawMode != TreeViewDrawMode.OwnerDrawText || tree.Nodes[0].Text != "Application")
                    throw new InvalidOperationException("Settings navigation business values changed");
            }
            Console.WriteLine("Editor/settings self-check passed: real StartApplication fields/helpers; Yes/No and startup values unchanged; classic tabs and nine dynamic settings pages; layout overlap checks passed");
        }

        public static void SelfCheck()
        {
            Application.SetCompatibleTextRenderingDefault(false);
            Forms.ScriptBuilder.Supplemental.frmSequenceRecorder.CheckRecordingState();
            using (var form = new Form())
            {
                var label = new Label { Text = "Welcome to taskt!" };
                var input = new TextBox { Text = "Save" };
                var options = new ComboBox(); options.Items.Add("Left");
                var button = new CustomControls.UIPictureButton { DisplayText = "Cancel", Tag = "business-id" };
                var menu = new MenuStrip(); menu.Items.Add("&File Actions");
                var tree = new TreeView { Name = "tvCommands" }; tree.Nodes.Add("Key Mouse Commands").Nodes.Add("Send Keystrokes");
                form.Controls.AddRange(new Control[] { label, input, options, button, menu, tree });
                Apply(form);
                if (label.Text != "欢迎使用 taskt！" || input.Text != "Save" || (string)options.Items[0] != "Left" ||
                    (string)button.Tag != "business-id" || button.DisplayText != "取消" || menu.Items[0].Text != "文件操作" ||
                    tree.Nodes[0].Text != "Key Mouse Commands" || !Matches("Send Keystrokes", "按键") || !Matches("Send Keystrokes", "keystrokes"))
                    throw new InvalidOperationException("Chinese display invariant failed");
                label.Text = "Ready";
                if (label.Text != "就绪") throw new InvalidOperationException("Dynamic label localization failed");
                using (var builder = new Forms.ScriptBuilder.frmScriptBuilder())
                using (var recorder = new Forms.ScriptBuilder.Supplemental.frmSequenceRecorder())
                using (var editor = new Forms.ScriptBuilder.CommandEditor.frmCommandEditor(new List<CustomControls.AutomationCommand>(), new List<Core.Automation.Commands.ScriptCommand>()))
                {
                    Apply(builder); Apply(recorder); Apply(editor);
                    CheckEditorAndSettings(builder, editor);
                    recorder.Width = recorder.MinimumSize.Width;
                    recorder.PerformLayout();
                    var panel = recorder.Controls.Find("pnlOptions", true).First();
                    panel.PerformLayout();
                    var samplingLabel = (Label)panel.Controls.Find("label1", false).First();
                    var samplingInput = (TextBox)panel.Controls.Find("txtHookResolution", false).First();
                    var stopLabel = (Label)panel.Controls.Find("label6", false).First();
                    var stopInput = (TextBox)panel.Controls.Find("txtHookStop", false).First();
                    var measured = TextRenderer.MeasureText(samplingLabel.Text, samplingLabel.Font);
                    if (samplingLabel.Bounds.IntersectsWith(samplingInput.Bounds) || stopLabel.Bounds.IntersectsWith(stopInput.Bounds) ||
                        samplingLabel.Left + measured.Width + 12 > samplingInput.Left || samplingInput.Left != stopInput.Left ||
                        samplingInput.Width < 80 || !panel.ClientRectangle.Contains(samplingInput.Bounds) ||
                        !recorder.ClientRectangle.Contains(panel.Bounds) || samplingInput.Text != "0" || stopInput.Text != "F2" ||
                        samplingInput.ForeColor.ToArgb() == samplingInput.BackColor.ToArgb())
                        throw new InvalidOperationException("Recorder sampling layout/input visibility failed");
                    Console.WriteLine("Recorder layout self-check passed: label=" + samplingLabel.Bounds + "; measured=" + measured +
                        "; input=" + samplingInput.Bounds + "; text=" + samplingInput.Text + "; fore=" + samplingInput.ForeColor + "; back=" + samplingInput.BackColor);
                    var welcome = builder.Controls.Find("label1", true).FirstOrDefault();
                    if (welcome == null || welcome.Text != "欢迎使用 taskt！" || recorder.Text != "屏幕录制器" || editor.Text != "添加命令")
                        throw new InvalidOperationException("Real form construction localization failed");
                    if (TranslatedCount < 80) throw new InvalidOperationException("Insufficient translated UI controls");
                }
                Console.WriteLine("Chinese UI self-check passed; translated controls: " + TranslatedCount + "; input/enum/Tag/tree business values unchanged; bilingual search passed; real builder/recorder/editor constructed without showing windows");
            }
        }
    }
}
