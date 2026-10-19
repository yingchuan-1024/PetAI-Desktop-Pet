using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Web.Script.Serialization;
using System.Windows.Automation;
using System.Windows;

public static class PetAutomation {
    static readonly object Gate = new object(), Output = new object();
    static readonly JavaScriptSerializer Json = new JavaScriptSerializer { MaxJsonLength = 8000000 };
    static readonly ConcurrentQueue<Raw> Queue = new ConcurrentQueue<Raw>();
    static readonly AutoResetEvent Wake = new AutoResetEvent(false);
    static readonly List<object> Steps = new List<object>();
    static readonly HashSet<int> Held = new HashSet<int>();
    static readonly HashSet<int> PhysicalHeld = new HashSet<int>();
    static int ParameterCount;
    static string ActiveInputIdentity;
    static volatile bool Recording, Running, Cancel, Closing, RecordingPaused;
    static readonly Dictionary<int,object> ClickSnapshots=new Dictionary<int,object>();
    static readonly Dictionary<int,IntPtr> MouseDownWindows=new Dictionary<int,IntPtr>();
    sealed class TargetException : Exception {
        public readonly string ReasonCode;
        public TargetException(string code,string message) : base(message) { ReasonCode=code; }
    }
    static string Reason(Exception error) { return error is TargetException ? ((TargetException)error).ReasonCode : error is ElementNotAvailableException ? "element_unavailable" : error is System.Runtime.InteropServices.COMException ? "provider_unavailable" : "uia_operation_failed"; }
    static string FailureMessage(Exception error) {
        if(error is TargetException) return error.Message;
        if(error is ElementNotAvailableException) return "页面或控件已重建，请等待页面加载完成后重试";
        if(error is System.Runtime.InteropServices.COMException) return "应用的 UI Automation 服务暂不可用，请确认应用权限一致后重试";
        return "UI Automation 操作失败；请检查目标控件是否支持此操作";
    }
    static void DpiContext() { try { SetThreadDpiAwarenessContext(new IntPtr(-4)); } catch(EntryPointNotFoundException) {} }
    static Thread HookThread, Worker;
    static uint HookThreadId;
    static IntPtr MouseHook, KeyHook;
    static HookProc MouseCallback = Mouse, KeyCallback = Keyboard;
    static readonly ManualResetEvent HooksReady = new ManualResetEvent(false);
    static readonly ManualResetEvent RunFinished = new ManualResetEvent(true);
    static string HookError;
    static HashSet<int> ExcludedProcessIds = new HashSet<int>();
    static readonly Dictionary<int,Dictionary<string,object>> KeyTargets = new Dictionary<int,Dictionary<string,object>>();
    class Raw { public string Type; public int Key, Delta, Button, X, Y; public bool Down; public object Id; public IntPtr Window; public Dictionary<string,object> Target; }
    delegate IntPtr HookProc(int code, IntPtr w, IntPtr l);
    [StructLayout(LayoutKind.Sequential)] struct POINT { public int X,Y; }
    [StructLayout(LayoutKind.Sequential)] struct MSG { public IntPtr hwnd; public uint message; public UIntPtr wParam; public IntPtr lParam; public uint time; public POINT pt; public uint priv; }
    [StructLayout(LayoutKind.Sequential)] struct MS { public POINT pt; public uint data,flags,time; public UIntPtr extra; }
    [StructLayout(LayoutKind.Sequential)] struct KB { public uint key,scan,flags,time; public UIntPtr extra; }
    [StructLayout(LayoutKind.Sequential)] struct MI { public int dx,dy; public uint data,flags,time; public UIntPtr extra; }
    [StructLayout(LayoutKind.Sequential)] struct KI { public ushort key,scan; public uint flags,time; public UIntPtr extra; }
    [StructLayout(LayoutKind.Explicit)] struct UNION { [FieldOffset(0)] public MI mouse; [FieldOffset(0)] public KI keyboard; }
    [StructLayout(LayoutKind.Sequential)] struct INPUT { public uint type; public UNION value; }
    [DllImport("user32.dll", SetLastError=true)] static extern IntPtr SetWindowsHookEx(int id, HookProc proc, IntPtr module, uint thread);
    [DllImport("user32.dll")] static extern bool UnhookWindowsHookEx(IntPtr hook);
    [DllImport("user32.dll")] static extern IntPtr CallNextHookEx(IntPtr hook,int code,IntPtr w,IntPtr l);
    [DllImport("user32.dll")] static extern int GetMessage(out MSG msg,IntPtr hwnd,uint min,uint max);
    [DllImport("user32.dll")] static extern bool PeekMessage(out MSG msg,IntPtr hwnd,uint min,uint max,uint remove);
    [DllImport("user32.dll")] static extern bool TranslateMessage(ref MSG msg);
    [DllImport("user32.dll")] static extern IntPtr DispatchMessage(ref MSG msg);
    [DllImport("user32.dll")] static extern bool PostThreadMessage(uint thread,uint msg,UIntPtr w,IntPtr l);
    [DllImport("kernel32.dll")] static extern uint GetCurrentThreadId();
    [DllImport("kernel32.dll", CharSet=CharSet.Unicode)] static extern IntPtr GetModuleHandle(string name);
    [DllImport("user32.dll")] static extern IntPtr GetForegroundWindow();
    [DllImport("user32.dll")] static extern IntPtr WindowFromPoint(POINT point);
    [DllImport("user32.dll")] static extern IntPtr SetThreadDpiAwarenessContext(IntPtr context);
    [DllImport("user32.dll")] static extern uint GetDpiForWindow(IntPtr hwnd);
    [DllImport("user32.dll")] static extern uint GetWindowThreadProcessId(IntPtr h,out uint pid);
    [DllImport("user32.dll")] static extern IntPtr GetAncestor(IntPtr h,uint flag);
    [DllImport("user32.dll")] static extern bool SetForegroundWindow(IntPtr h);
    [DllImport("user32.dll")] static extern bool IsWindow(IntPtr h);
    [DllImport("user32.dll")] static extern short GetAsyncKeyState(int key);
    [DllImport("user32.dll", SetLastError=true)] static extern uint SendInput(uint count,INPUT[] inputs,int size);
    [DllImport("user32.dll")] static extern bool SetCursorPos(int x,int y);
    static void Emit(object value) { lock(Output) { Console.Out.WriteLine(Json.Serialize(value)); Console.Out.Flush(); } }
    static Dictionary<string,object> D(object value) { return (Dictionary<string,object>)value; }
    static string S(Dictionary<string,object> d,string k) { return d.ContainsKey(k) ? Convert.ToString(d[k]) : ""; }
    static int N(Dictionary<string,object> d,string k) { return Convert.ToInt32(d[k]); }
    static bool B(Dictionary<string,object> d,string k) { return d.ContainsKey(k) && Convert.ToBoolean(d[k]); }
    static void Reply(object id,bool ok,string message) { Emit(new { id=id,ok=ok,message=message }); }
    static void Reply(object id,bool ok,string message,int completedSteps) { Emit(new { id=id,ok=ok,message=message,completedSteps=completedSteps }); }
    static IntPtr Mouse(int code,IntPtr w,IntPtr l) {
        if(code>=0 && Recording) {
            MS m=(MS)Marshal.PtrToStructure(l,typeof(MS)); int msg=w.ToInt32();
            IntPtr hit=WindowFromPoint(m.pt), root=GetAncestor(hit,2);
            if(!RecordingPaused && (m.flags&1)==0 && (msg==0x201 || msg==0x202 || msg==0x204 || msg==0x205 || msg==0x207 || msg==0x208 || msg==0x20A)) {
                bool down=msg==0x201 || msg==0x204 || msg==0x207;
                int button=(msg==0x204 || msg==0x205) ? 1 : (msg==0x207 || msg==0x208) ? 2 : 0;
                if(down || msg==0x20A) {
                    if(Excluded(hit) || Excluded(root)) return CallNextHookEx(MouseHook,code,w,l);
                    if(down) MouseDownWindows[button]=root;
                } else {
                    if(!MouseDownWindows.TryGetValue(button,out root)) return CallNextHookEx(MouseHook,code,w,l);
                    MouseDownWindows.Remove(button);
                }
                Queue.Enqueue(new Raw { Type=msg==0x20A ? "wheel" : down ? "mouseDown":"click", Delta=(short)(m.data>>16), Button=button, X=m.pt.X,Y=m.pt.Y, Window=root }); Wake.Set();
            }
        }
        return CallNextHookEx(MouseHook,code,w,l);
    }
    static IntPtr Keyboard(int code,IntPtr w,IntPtr l) {
        if(code>=0) {
            KB k=(KB)Marshal.PtrToStructure(l,typeof(KB));
            if((k.flags&0x10)==0) {
                bool down=w.ToInt32()==0x100 || w.ToInt32()==0x104;
                lock(PhysicalHeld) { if(down) PhysicalHeld.Add((int)k.key); else PhysicalHeld.Remove((int)k.key); }
                if(k.key==120 && (Recording || Running)) { if(down) { Cancel=true; Queue.Enqueue(new Raw { Type="cancel" }); Wake.Set(); } return new IntPtr(1); }
                if(k.key==119 && Recording) { if(down) { Recording=false; Queue.Enqueue(new Raw { Type="finish" }); Wake.Set(); } return new IntPtr(1); }
                if(Recording && !RecordingPaused) {
                    IntPtr foreground=GetForegroundWindow();
                    if(!Excluded(foreground)) {
                        Dictionary<string,object> target=null;
                        lock(KeyTargets) { if(down) KeyTargets.Remove((int)k.key); else KeyTargets.TryGetValue((int)k.key,out target); }
                        Queue.Enqueue(new Raw { Type="key", Key=(int)k.key,Down=down,Window=foreground,Target=target }); Wake.Set();
                    }
                }
            }
        }
        return CallNextHookEx(KeyHook,code,w,l);
    }
    static void Hooks() {
        DpiContext(); HookThreadId=GetCurrentThreadId(); MSG msg; PeekMessage(out msg,IntPtr.Zero,0,0,0);
        MouseHook=SetWindowsHookEx(14,MouseCallback,GetModuleHandle(null),0);
        KeyHook=SetWindowsHookEx(13,KeyCallback,GetModuleHandle(null),0);
        if(MouseHook==IntPtr.Zero || KeyHook==IntPtr.Zero) HookError="无法安装全局输入监听";
        HooksReady.Set();
        try { while(GetMessage(out msg,IntPtr.Zero,0,0)>0) { TranslateMessage(ref msg); DispatchMessage(ref msg); } }
        finally { if(MouseHook!=IntPtr.Zero) UnhookWindowsHookEx(MouseHook); if(KeyHook!=IntPtr.Zero) UnhookWindowsHookEx(KeyHook); }
    }
    static AutomationElement WindowOf(AutomationElement element) {
        var current=element;
        for(int i=0;i<64 && current!=null;i++) {
            int h=current.Current.NativeWindowHandle;
            if(h!=0) { IntPtr root=GetAncestor(new IntPtr(h),2); if(root!=IntPtr.Zero) return AutomationElement.FromHandle(root); }
            current=TreeWalker.RawViewWalker.GetParent(current);
        }
        throw new Exception("控件没有可靠所属窗口");
    }
    static string ProcessName(AutomationElement window) { return Process.GetProcessById(window.Current.ProcessId).ProcessName; }
    static bool Editable(AutomationElement element) {
        object pattern;
        if(element.Current.IsPassword || element.Current.ControlType==ControlType.Edit) return true;
        return element.TryGetCurrentPattern(ValuePattern.Pattern,out pattern) && !((ValuePattern)pattern).Current.IsReadOnly;
    }
    static bool Excluded(IntPtr hwnd) {
        uint pid; GetWindowThreadProcessId(hwnd,out pid); return ExcludedProcessIds.Contains((int)pid);
    }
    static bool Actionable(AutomationElement element) {
        object pattern;
        return Editable(element) || element.TryGetCurrentPattern(InvokePattern.Pattern,out pattern) || element.TryGetCurrentPattern(SelectionItemPattern.Pattern,out pattern) || element.TryGetCurrentPattern(TogglePattern.Pattern,out pattern) || element.TryGetCurrentPattern(ExpandCollapsePattern.Pattern,out pattern) || element.TryGetCurrentPattern(ScrollPattern.Pattern,out pattern);
    }
    static AutomationElement SemanticTarget(AutomationElement element,AutomationElement window) {
        for(int i=0;i<64 && element!=null && !Automation.Compare(element,window);i++) {
            if(Actionable(element)) return element;
            element=TreeWalker.RawViewWalker.GetParent(element);
        }
        throw new TargetException("unsupported_semantics","目标没有可验证的 UIA 操作语义");
    }
    static Dictionary<string,object> Identity(AutomationElement element) {
        var c=element.Current;
        return new Dictionary<string,object> { {"automationId",c.AutomationId}, {"name",Editable(element) ? "" : c.Name}, {"controlType",c.ControlType.Id} };
    }
    static object[] ParentChain(AutomationElement element,AutomationElement window) {
        var chain=new List<object>();
        for(int i=0;i<64;i++) {
            element=TreeWalker.RawViewWalker.GetParent(element);
            if(element==null) throw new TargetException("root_mismatch","控件不属于预期 UIA 窗口");
            if(Automation.Compare(element,window)) return chain.ToArray();
            chain.Add(Identity(element));
        }
        throw new TargetException("tree_depth","UIA 父链超过安全限制");
    }
    // A browser document is a stable semantic boundary. Chromium's native proxy
    // windows above it contain session-specific handles and change after refresh.
    // Normalize both old recordings and current observations for compatibility.
    static object[] StableParents(Dictionary<string,object> target,object[] chain) {
        if(N(target,"controlType")==ControlType.Document.Id) return new object[0];
        for(int i=0;i<chain.Length;i++) if(N(D(chain[i]),"controlType")==ControlType.Document.Id) return chain.Take(i+1).ToArray();
        return chain;
    }
    static Dictionary<string,object> Describe(AutomationElement element,AutomationElement window) {
        var c=element.Current;
        var target=Identity(element);
        if(String.IsNullOrWhiteSpace(S(target,"automationId")) && String.IsNullOrWhiteSpace(S(target,"name"))) throw new TargetException("missing_identity","目标缺少稳定的 UIA 标识");
        if(!Actionable(element)) throw new TargetException("unsupported_semantics","目标没有可验证的 UIA 操作语义");
        target["process"]=ProcessName(window); target["title"]=window.Current.Name; target["password"]=c.IsPassword;
        target["parents"]=StableParents(target,ParentChain(element,window));
        return target;
    }
    static bool SameIdentity(Dictionary<string,object> expected, Dictionary<string,object> actual) {
        return N(expected,"controlType")==N(actual,"controlType") && S(expected,"automationId")==S(actual,"automationId") && S(expected,"name")==S(actual,"name");
    }
    static List<AutomationElement> Matches(AutomationElement window,Dictionary<string,object> target) {
        var conditions=new List<Condition> { new PropertyCondition(AutomationElement.ControlTypeProperty,ControlType.LookupById(N(target,"controlType"))) };
        if(S(target,"automationId")!="") conditions.Add(new PropertyCondition(AutomationElement.AutomationIdProperty,S(target,"automationId")));
        if(S(target,"name")!="") conditions.Add(new PropertyCondition(AutomationElement.NameProperty,S(target,"name")));
        var result=new List<AutomationElement>();
        Condition condition=conditions.Count==1 ? conditions[0] : new AndCondition(conditions.ToArray());
        foreach(AutomationElement candidate in window.FindAll(TreeScope.Descendants,condition)) {
            if(target.ContainsKey("parents")) {
                var expected=StableParents(target,((IEnumerable)target["parents"]).Cast<object>().ToArray());
                var actual=StableParents(target,ParentChain(candidate,window));
                if(expected.Length!=actual.Length || expected.Where((value,index)=>!SameIdentity(D(value),D(actual[index]))).Any()) continue;
            }
            result.Add(candidate);
        }
        return result;
    }
    static void Record(Raw raw) {
        if(raw.Type=="click") {
            object snapshot;
            if(!ClickSnapshots.TryGetValue(raw.Button,out snapshot)) throw new TargetException("snapshot_missing","按下时未能捕获可验证的目标");
            ClickSnapshots.Remove(raw.Button); Steps.Add(snapshot); return;
        }
        if(raw.Window==IntPtr.Zero || (raw.Type=="key" && raw.Window!=GetForegroundWindow())) throw new TargetException("window_changed","录制目标窗口发生变化");
        if(raw.Type!="key" && GetAncestor(WindowFromPoint(new POINT { X=raw.X,Y=raw.Y }),2)!=raw.Window) throw new TargetException("window_changed","按下目标已改变，无法安全捕获 UIA 快照");
        AutomationElement element=raw.Type=="key" && !raw.Down && raw.Target!=null ? null : raw.Type=="key" ? AutomationElement.FocusedElement : AutomationElement.FromPoint(new Point(raw.X,raw.Y));
        Dictionary<string,object> target=raw.Target;
        AutomationElement window;
        if(target!=null) { window=FindWindow(target); element=Locate(window,target,raw.Window); }
        else {
            if(element==null) throw new Exception("无法可靠识别输入目标");
            window=AutomationElement.FromHandle(raw.Window);
            if(WindowOf(element).Current.NativeWindowHandle!=window.Current.NativeWindowHandle) throw new TargetException("root_mismatch","UIA 根窗口与输入窗口不一致");
            if(raw.Type!="key") element=SemanticTarget(element,window);
            target=Describe(element,window);
        }
        if(new IntPtr(window.Current.NativeWindowHandle)!=raw.Window) throw new Exception("控件不属于当前窗口");
        if(raw.Type=="key" && raw.Down) lock(KeyTargets) KeyTargets[raw.Key]=target;
        if(Matches(window,target).Count!=1) throw new TargetException("ambiguous_target","UIA 控件及父链标识仍不唯一");
        if(raw.Type=="key" && Editable(element)) {
            if(raw.Key==16 || raw.Key==160 || raw.Key==161) return;
            if(raw.Key==17 || raw.Key==162 || raw.Key==163 || raw.Key==18 || raw.Key==164 || raw.Key==165 || raw.Key==91 || raw.Key==92)
                throw new TargetException("unsupported_shortcut","编辑框中的 Ctrl、Alt 或 Win 组合键暂不支持录制，请使用普通文本输入参数");
            if(raw.Key!=9 && raw.Key!=13 && raw.Key!=27) {
                if(raw.Down) {
                    string identity=Json.Serialize(target);
                    if(identity!=ActiveInputIdentity) {
                        ActiveInputIdentity=identity;
                        Steps.Add(new { type="input",target=target,parameter="input_"+(++ParameterCount) });
                    }
                }
                return;
            }
            ActiveInputIdentity=null;
            if(B(target,"password")) return;
        } else if(raw.Type=="key" && raw.Down) ActiveInputIdentity=null;
        if(raw.Type=="key") Steps.Add(new { type="key",target=target,key=raw.Key,down=raw.Down });
        else if(raw.Type=="wheel") Steps.Add(new { type="wheel",target=target,delta=raw.Delta });
        else {
            CaptureClick(raw,element,target);
        }
        if(Steps.Count>10000) throw new Exception("录制超过步骤限制");
    }
    static void CaptureClick(Raw raw,AutomationElement element,Dictionary<string,object> target) {
        Rect r=element.Current.BoundingRectangle;
        if(r.IsEmpty || r.Width<=0 || r.Height<=0 || !r.Contains(new Point(raw.X,raw.Y))) throw new TargetException("invalid_bounds","UIA 控件边界无法验证");
        ClickSnapshots[raw.Button]=new { type="click",target=target,x=(raw.X-r.X)/r.Width,y=(raw.Y-r.Y)/r.Height,button=raw.Button };
    }
    static void RecordingLoop() {
        DpiContext();
        while(!Closing) {
            Wake.WaitOne(100); Raw raw;
            while(Queue.TryDequeue(out raw)) {
                {
                    if(raw.Type=="cancel") { bool wasRecording=Recording; Recording=false; Steps.Clear(); ParameterCount=0; ActiveInputIdentity=null; lock(KeyTargets) KeyTargets.Clear(); if(wasRecording) Emit(new { @event="recordingFinished",ok=false,message="已紧急取消录制" }); if(raw.Id!=null) Reply(raw.Id,true,"停止信号已发送"); continue; }
                    if(raw.Type=="finish") { Finish(null,true); continue; }
                    if(raw.Type=="finishRequest") { try { FinishRecording(raw.Id); } catch { Steps.Clear(); ParameterCount=0; ActiveInputIdentity=null; lock(KeyTargets) KeyTargets.Clear(); Reply(raw.Id,false,"输入目标无法验证，录制未保存"); } continue; }
                    if(Cancel || RecordingPaused) continue;
                    try { Record(raw); } catch(Exception error) {
                        RecordingPaused=true; ClickSnapshots.Clear();
                        Emit(new { @event="recordingPaused",ok=false,reasonCode=Reason(error),retainedSteps=Steps.Count,message="录制已暂停："+FailureMessage(error)+"；此动作未录入。F8仅保存之前的有效步骤，F9取消后可重新录制" });
                    }
                }
            }
        }
    }
    static void FinishRecording(object id) {
        Finish(id,false);
    }
    static void Finish(object id,bool notification) {
        Recording=false;
        string message=Steps.Count==0 ? "录制为空" : RecordingPaused ? "仅保存暂停前的有效步骤；流程不完整，请确认或重新录制" : "录制结束";
        if(notification) Emit(new { @event="recordingFinished",ok=Steps.Count>0,partial=RecordingPaused,message=message,steps=Steps.ToArray() });
        else Emit(new { id=id,ok=Steps.Count>0,partial=RecordingPaused,message=message,steps=Steps.ToArray() });
        RecordingPaused=false; ClickSnapshots.Clear();
        Steps.Clear(); ParameterCount=0; ActiveInputIdentity=null; lock(KeyTargets) KeyTargets.Clear();
    }
    static void Check(IntPtr hwnd) {
        if(Cancel || Closing) throw new OperationCanceledException();
        if(!IsWindow(hwnd)) throw new TargetException("window_closed","目标窗口已关闭，请重新打开目标应用");
        if(GetForegroundWindow()!=hwnd) throw new TargetException("foreground_changed","前台窗口已变化，已停止；运行时请保持目标窗口在前台");
    }
    static void Pause(int milliseconds) { for(int i=0;i<milliseconds;i+=20) { if(Cancel || Closing) throw new OperationCanceledException(); Thread.Sleep(20); } }
    static bool BrowserTitleMatches(string process,string title,Dictionary<string,object> target) {
        if(!new[]{"msedge","chrome","brave","vivaldi","opera"}.Contains(process)) return false;
        string document=N(target,"controlType")==ControlType.Document.Id ? S(target,"name") : "";
        if(document=="" && target.ContainsKey("parents")) foreach(object parent in (IEnumerable)target["parents"]) {
            var p=D(parent); if(N(p,"controlType")==ControlType.Document.Id) { document=S(p,"name"); break; }
        }
        if(String.IsNullOrWhiteSpace(document)) return false;
        return title==document || title.StartsWith(document+" - ",StringComparison.Ordinal) || title.StartsWith(document+" — ",StringComparison.Ordinal) || title.StartsWith(document+" 和另外 ",StringComparison.Ordinal) || title.StartsWith(document+" and ",StringComparison.Ordinal);
    }
    static AutomationElement FindWindow(Dictionary<string,object> target) {
        for(int attempt=0;attempt<10;attempt++) {
            var windows=AutomationElement.RootElement.FindAll(TreeScope.Children,Condition.TrueCondition);
            var exact=new List<AutomationElement>(); var browser=new List<AutomationElement>();
            foreach(AutomationElement w in windows) {
                if(Cancel) throw new OperationCanceledException();
                try {
                    string process=ProcessName(w); if(process!=S(target,"process")) continue;
                    string title=w.Current.Name;
                    if(title==S(target,"title")) exact.Add(w);
                    else if(BrowserTitleMatches(process,title,target)) browser.Add(w);
                } catch(ElementNotAvailableException) {} catch(ArgumentException) {}
            }
            var matches=exact.Count>0 ? exact : browser;
            if(matches.Count==1) return matches[0];
            if(matches.Count>1) throw new TargetException("ambiguous_window","存在多个匹配窗口，请只保留一个目标窗口后重试");
            Pause(250);
        }
        throw new TargetException("window_not_found","未找到录制时的目标窗口，请先打开相同应用及页面");
    }
    static AutomationElement Locate(AutomationElement window,Dictionary<string,object> target,IntPtr hwnd) {
        int scrolls=0;
        for(int attempt=0;attempt<13;attempt++) {
            Check(hwnd);
            List<AutomationElement> matches;
            try { matches=Matches(window,target); } catch(ElementNotAvailableException) { Pause(200); continue; }
            if(matches.Count>1) throw new TargetException("ambiguous_target","存在多个相同控件，请重新录制具有唯一标识的目标");
            if(matches.Count==1 && !matches[0].Current.IsOffscreen && matches[0].Current.IsEnabled) {
                var element=matches[0];
                if(new IntPtr(WindowOf(element).Current.NativeWindowHandle)!=hwnd) throw new TargetException("root_mismatch","目标不属于录制时的窗口");
                if(element.Current.IsPassword!=B(target,"password")) throw new TargetException("input_changed","输入控件的密码属性已变化，请重新录制");
                return element;
            }
            // Wait for page transitions before attempting bounded scrolling.
            if(attempt<5) { Pause(200); continue; }
            if(matches.Count==1) {
                object item;
                if(scrolls<3 && matches[0].Current.IsOffscreen && matches[0].TryGetCurrentPattern(ScrollItemPattern.Pattern,out item)) {
                    Check(hwnd); ((ScrollItemPattern)item).ScrollIntoView(); scrolls++; Pause(200); continue;
                }
                if(!matches[0].Current.IsEnabled) { Pause(200); continue; }
            }
            if(attempt==12) break;
            if(scrolls>=3) { Pause(200); continue; }
            var containers=window.FindAll(TreeScope.Descendants,new PropertyCondition(AutomationElement.IsScrollPatternAvailableProperty,true));
            if(containers.Count!=1) { Pause(200); continue; }
            var scroll=(ScrollPattern)containers[0].GetCurrentPattern(ScrollPattern.Pattern);
            if(!scroll.Current.VerticallyScrollable) { Pause(200); continue; }
            Check(hwnd); scroll.Scroll(ScrollAmount.NoAmount,ScrollAmount.LargeIncrement); scrolls++; Pause(200);
        }
        throw new TargetException("target_not_found","等待并有限滚动后仍未找到可用控件；请恢复录制时的页面，若页面结构已变请重新录制");
    }
    static bool HitWithin(AutomationElement hit,AutomationElement element) {
        for(int i=0;i<64 && hit!=null;i++) {
            if(Automation.Compare(hit,element)) return true;
            hit=TreeWalker.RawViewWalker.GetParent(hit);
        }
        return false;
    }
    static void Send(INPUT input) { if(SendInput(1,new INPUT[]{input},Marshal.SizeOf(typeof(INPUT)))!=1) throw new TargetException("input_rejected","Windows 拒绝输入，请确认桌宠与目标应用以相同权限运行"); }
    static void Key(int key,bool down) {
        Send(new INPUT { type=1,value=new UNION { keyboard=new KI { key=(ushort)key,flags=down ? 0u:2u } } });
        if(down) Held.Add(key); else Held.Remove(key);
    }
    static void Release() { foreach(int key in Held.ToArray()) { try { Key(key,false); } catch {} } Held.Clear(); }
    static void Execute(Dictionary<string,object> command) {
        DpiContext();
        object id=command["id"];
        int completed=0, total=0;
        object response;
        try {
            var steps=((IEnumerable)command["steps"]).Cast<object>().ToArray();
            total=steps.Length;
            if(steps.Length==0 || steps.Length>10000) throw new Exception("无效任务步骤");
            var inputs=D(command["inputs"]);
            foreach(object entry in steps) { var s=D(entry); if(S(s,"type")=="input" && (!inputs.ContainsKey(S(s,"parameter")) || !(inputs[S(s,"parameter")] is string))) throw new Exception("缺少运行时输入参数"); }
            foreach(object entry in steps) {
                if(Cancel) throw new OperationCanceledException();
                var step=D(entry); var target=D(step["target"]);
                var window=FindWindow(target); IntPtr hwnd=new IntPtr(window.Current.NativeWindowHandle);
                if(GetForegroundWindow()!=hwnd) { if(!SetForegroundWindow(hwnd)) throw new TargetException("activation_failed","无法激活目标窗口，请先将目标应用恢复到桌面后重试"); Pause(120); }
                Check(hwnd); var element=Locate(window,target,hwnd); string type=S(step,"type"); object pattern;
                if(type=="input") {
                    if(!Editable(element) || !element.TryGetCurrentPattern(ValuePattern.Pattern,out pattern)) throw new TargetException("unsupported_input","输入框不支持 UIA Value，当前无法可靠填写；请改为手动输入");
                    var value=(ValuePattern)pattern;
                    if(value.Current.IsReadOnly) throw new TargetException("readonly_input","目标输入框只读，无法填写");
                    element.SetFocus(); Check(hwnd); value.SetValue((string)inputs[S(step,"parameter")]);
                } else if(type=="key") {
                    if(element.Current.IsPassword) throw new TargetException("password_keys","密码框禁止按键回放，请使用运行时输入参数");
                    element.SetFocus(); Check(hwnd);
                    var focus=AutomationElement.FocusedElement;
                    if(focus==null || !Automation.Compare(focus,element)) throw new TargetException("focus_failed","无法确认键盘焦点，已停止发送按键");
                    int key=N(step,"key"); if(key<1 || key>254 || key==119 || key==120) throw new Exception("无效按键");
                    Key(key,B(step,"down"));
                } else if(type=="click") {
                    int button=N(step,"button");
                    if(button==0 && element.TryGetCurrentPattern(InvokePattern.Pattern,out pattern)) { Check(hwnd); ((InvokePattern)pattern).Invoke(); }
                    else if(button==0 && Editable(element)) { element.SetFocus(); Check(hwnd); }
                    else if(button==0 && element.TryGetCurrentPattern(TogglePattern.Pattern,out pattern)) { Check(hwnd); ((TogglePattern)pattern).Toggle(); }
                    else if(button==0 && element.TryGetCurrentPattern(SelectionItemPattern.Pattern,out pattern)) { Check(hwnd); ((SelectionItemPattern)pattern).Select(); }
                    else {
                        Rect r=element.Current.BoundingRectangle; double x=Convert.ToDouble(step["x"]),y=Convert.ToDouble(step["y"]);
                        if(r.IsEmpty || r.Width<=0 || r.Height<=0 || x<0 || x>1 || y<0 || y>1 || button<0 || button>2) throw new Exception("无效控件坐标");
                        Point p=new Point(r.X+r.Width*x,r.Y+r.Height*y);
                        var hit=AutomationElement.FromPoint(p);
                        if(!HitWithin(hit,element)) throw new TargetException("target_obscured","目标被其他窗口遮挡，请移开遮挡窗口后重试");
                        Check(hwnd); if(!SetCursorPos((int)p.X,(int)p.Y)) throw new Exception("无法定位鼠标");
                        uint down=button==0 ? 2u:button==1 ? 8u:32u; uint up=down*2;
                        Send(new INPUT { type=0,value=new UNION { mouse=new MI { flags=down } } });
                        Send(new INPUT { type=0,value=new UNION { mouse=new MI { flags=up } } });
                    }
                } else if(type=="wheel") {
                    Rect r=element.Current.BoundingRectangle; if(r.IsEmpty || r.Width<=0 || r.Height<=0) throw new Exception("滚轮目标不可见");
                    Point p=new Point(r.X+r.Width/2,r.Y+r.Height/2);
                    if(!HitWithin(AutomationElement.FromPoint(p),element)) throw new TargetException("target_obscured","滚轮目标被其他窗口遮挡，请移开遮挡窗口后重试");
                    Check(hwnd); if(!SetCursorPos((int)p.X,(int)p.Y)) throw new Exception("无法定位滚轮");
                    Send(new INPUT { type=0,value=new UNION { mouse=new MI { flags=0x800,data=unchecked((uint)N(step,"delta")) } } });
                } else throw new Exception("不支持的动作");
                completed++;
                Emit(new { @event="progress",completedSteps=completed,totalSteps=steps.Length,message="已完成 "+completed+" / "+steps.Length+" 步" });
                Pause(100);
            }
            response=new { id=id,ok=true,message="任务完成（"+completed+" / "+total+" 步）",completedSteps=completed,totalSteps=total };
        } catch(OperationCanceledException) { response=new { id=id,ok=false,reasonCode="cancelled",message="任务已停止（已完成 "+completed+" / "+total+" 步）",completedSteps=completed,totalSteps=total }; }
        catch(Exception error) { response=new { id=id,ok=false,reasonCode=Reason(error),failedStep=completed+1,message="第 "+(completed+1)+" / "+total+" 步失败："+FailureMessage(error)+"。已停止后续步骤",completedSteps=completed,totalSteps=total }; }
        finally { Release(); }
        lock(Gate) {
            try { Emit(response); }
            finally { Running=false; RunFinished.Set(); }
        }
    }
    // This mode only inspects controls owned by its dedicated test window; no hooks or input injection.
    public static void SelfTest() {
        Console.OutputEncoding=new UTF8Encoding(false); DpiContext();
        var ready=new ManualResetEvent(false);
        System.Windows.Forms.Form form=null;
        IntPtr handle=IntPtr.Zero, buttonHandle=IntPtr.Zero, textHandle=IntPtr.Zero;
        var ui=new Thread(()=> {
            DpiContext();
            form=new System.Windows.Forms.Form { Text="UIA controlled fixture", Width=640, Height=360, ShowInTaskbar=false };
            var left=new System.Windows.Forms.GroupBox { Text="Left fixture", Left=10, Top=10, Width=280, Height=250 };
            var right=new System.Windows.Forms.GroupBox { Text="Right fixture", Left=310, Top=10, Width=280, Height=250 };
            var button=new System.Windows.Forms.Button { Text="Duplicate fixture", Left=20, Top=30, Width=200, Height=100 };
            var label=new System.Windows.Forms.Label { Text="Fixture child", Left=10, Top=10, Width=140, Height=30 };
            button.Controls.Add(label); left.Controls.Add(button);
            right.Controls.Add(new System.Windows.Forms.Button { Text="Duplicate fixture", Left=20, Top=30, Width=200, Height=100 });
            form.Controls.Add(left); form.Controls.Add(right);
            form.Shown+=(s,e)=> { handle=form.Handle; buttonHandle=button.Handle; textHandle=label.Handle; ready.Set(); };
            System.Windows.Forms.Application.Run(form);
        });
        ui.SetApartmentState(ApartmentState.STA); ui.Start();
        if(!ready.WaitOne(10000)) throw new Exception("fixture_start_timeout");
        try {
            var window=AutomationElement.FromHandle(handle);
            Emit(new { test="dpi", dpi=GetDpiForWindow(handle), scale125Verified=GetDpiForWindow(handle)==120 });
            foreach(var item in new[] { new { kind="text_descendant", h=textHandle }, new { kind="button", h=buttonHandle } }) {
                try {
                    var element=SemanticTarget(AutomationElement.FromHandle(item.h),window);
                    var target=Describe(element,window);
                    if(element.Current.ControlType!=ControlType.Button) throw new Exception("semantic_target_failed");
                    if(item.kind=="text_descendant") {
                        if(!HitWithin(AutomationElement.FromHandle(textHandle),element)) throw new Exception("descendant_hit_failed");
                        var other=window.FindAll(TreeScope.Descendants,new PropertyCondition(AutomationElement.ControlTypeProperty,ControlType.Button)).Cast<AutomationElement>().First(e=>!Automation.Compare(e,element));
                        if(HitWithin(other,element)) throw new Exception("unrelated_hit_accepted");
                        Emit(new { test="descendant_hit_and_occlusion",ok=true });
                    }
                    Emit(new { test=item.kind, phase="observation", matchCount=Matches(window,target).Count, rootConsistent=WindowOf(element).Current.NativeWindowHandle==handle.ToInt32(), reasonCode="observed" });
                    target["automationId"]="";
                    Emit(new { test=item.kind, phase="name_only_provider", matchCount=Matches(window,target).Count, reasonCode=Matches(window,target).Count==1 ? "unique":"ambiguous" });
                    if(Matches(window,target).Count!=1) throw new Exception("parent_match_failed");
                    var ambiguous=new Dictionary<string,object>(target); ambiguous.Remove("parents");
                    if(Matches(window,ambiguous).Count!=2) throw new Exception("ambiguity_guard_failed");
                    if(item.kind=="button") {
                        Rect bounds=element.Current.BoundingRectangle;
                        int x=(int)(bounds.X+bounds.Width/2), y=(int)(bounds.Y+bounds.Height/2);
                        // Direct UIA fixture handle avoids inspecting any other desktop window.
                        CaptureClick(new Raw { Type="mouseDown", X=x,Y=y,Window=handle },element,Describe(element,window));
                        form.Invoke(new Action(()=>form.Controls[0].Controls[0].Dispose()));
                        Record(new Raw { Type="click", X=x,Y=y,Window=handle });
                        if(Steps.Count!=1) throw new Exception("snapshot_failed");
                        Emit(new { test="mouseup_after_dispose", reasonCode="snapshot_recorded", steps=Steps.Count });
                        try { Describe(element,window); Emit(new { test="after_action", reasonCode="still_available" }); }
                        catch(Exception error) { Emit(new { test="after_action", reasonCode=error.GetType().Name }); }
                    }
                } catch(Exception error) { Emit(new { test=item.kind, phase="observation", reasonCode=Reason(error) }); throw; }
            }
            var document=new Dictionary<string,object> { {"automationId","RootWebArea"},{"name","Fixture page"},{"controlType",ControlType.Document.Id} };
            var buttonTarget=new Dictionary<string,object> { {"controlType",ControlType.Button.Id},{"parents",new object[]{document}},{"process","msedge"} };
            var proxy=new Dictionary<string,object> { {"automationId","12345"},{"name","Chrome Legacy Window"},{"controlType",ControlType.Pane.Id} };
            if(StableParents(buttonTarget,new object[]{document,proxy}).Length!=1 || StableParents(document,new object[]{proxy}).Length!=0) throw new Exception("browser_parent_normalization_failed");
            if(!BrowserTitleMatches("msedge","Fixture page 和另外 13 个页面 - Microsoft Edge",buttonTarget) || BrowserTitleMatches("msedge","Fixture other page - Microsoft Edge",buttonTarget)) throw new Exception("browser_title_match_failed");
            Emit(new { test="legacy_browser_parents_and_titles",ok=true });
        } finally { form.BeginInvoke(new Action(()=>form.Close())); ui.Join(5000); }
    }
    // Inspect and invoke only the dedicated local Chromium fixture supplied by
    // verify-automation-browser.js. No hooks or personal browser pages are used.
    public static void BrowserSelfTest(int processId) {
        Console.OutputEncoding=new UTF8Encoding(false); DpiContext();
        AutomationElement window=null;
        for(int i=0;i<30 && window==null;i++) {
            foreach(AutomationElement candidate in AutomationElement.RootElement.FindAll(TreeScope.Children,new PropertyCondition(AutomationElement.ProcessIdProperty,processId))) {
                if(candidate.Current.Name=="PetAI automation Chromium fixture") { window=candidate; break; }
            }
            if(window==null) Thread.Sleep(100);
        }
        if(window==null) throw new Exception("owned_chromium_fixture_missing");
        IntPtr hwnd=new IntPtr(window.Current.NativeWindowHandle);
        var reload=window.FindFirst(TreeScope.Descendants,new PropertyCondition(AutomationElement.NameProperty,"Reload fixture page"));
        var delayed=window.FindFirst(TreeScope.Descendants,new PropertyCondition(AutomationElement.NameProperty,"Delayed fixture action"));
        if(reload==null || delayed==null) throw new Exception("chromium_accessibility_unavailable");
        var reloadTarget=Describe(SemanticTarget(reload,window),window);
        var delayedTarget=Describe(SemanticTarget(delayed,window),window);
        // Simulate a legacy saved task with the obsolete native proxy chain.
        var legacy=((IEnumerable)reloadTarget["parents"]).Cast<object>().Concat(new object[]{new Dictionary<string,object>{{"controlType",50033},{"automationId","obsolete-session-handle"},{"name","Chrome Legacy Window"}}}).ToArray();
        reloadTarget["parents"]=legacy;
        if(Matches(window,reloadTarget).Count!=1) throw new Exception("legacy_chromium_match_failed");
        var steps=new object[]{new {type="click",target=reloadTarget,x=0.5,y=0.5,button=0},new {type="click",target=delayedTarget,x=0.5,y=0.5,button=0}};
        var serialized=Json.Serialize(steps);
        RunFinished.Reset(); Running=true;
        Execute(new Dictionary<string,object>{{"id","chromium-fixture"},{"steps",Json.DeserializeObject(serialized)},{"inputs",new Dictionary<string,object>()}});
        AutomationElement done=null;
        for(int attempt=0;attempt<10 && done==null;attempt++) {
            done=window.FindFirst(TreeScope.Descendants,new PropertyCondition(AutomationElement.NameProperty,"Fixture action complete"));
            if(done==null) Thread.Sleep(100);
        }
        if(done==null || Held.Count!=0 || !RunFinished.WaitOne(0)) throw new Exception("chromium_replay_failed");
        Emit(new { test="chromium_reload_legacy_task_and_delayed_target",ok=true,steps=2 });
        Cancel=false;
        var missing=new Dictionary<string,object>(delayedTarget); missing["name"]="Missing owned fixture action";
        var failureSteps=Json.DeserializeObject(Json.Serialize(new object[]{new {type="click",target=missing,x=0.5,y=0.5,button=0}}));
        RunFinished.Reset(); Running=true;
        Execute(new Dictionary<string,object>{{"id","chromium-failure-fixture"},{"steps",failureSteps},{"inputs",new Dictionary<string,object>()}});
        Emit(new { test="chromium_missing_target_diagnostics",ok=true });
    }
    public static void MainLoop(bool validateOnly) {
        if(validateOnly) { Emit(new { ok=true,message="UIA与PInvoke编译验证通过；未安装hook或操作桌面" }); return; }
        Console.InputEncoding=new UTF8Encoding(false); Console.OutputEncoding=new UTF8Encoding(false);
        HookThread=new Thread(Hooks); HookThread.SetApartmentState(ApartmentState.STA); HookThread.IsBackground=true; HookThread.Start();
        if(!HooksReady.WaitOne(5000) || HookError!=null) { Emit(new { @event="error",message="全局监听启动失败" }); return; }
        Worker=new Thread(RecordingLoop); Worker.SetApartmentState(ApartmentState.MTA); Worker.IsBackground=true; Worker.Start();
        try {
            string line;
            while((line=Console.ReadLine())!=null) {
                Dictionary<string,object> command;
                try { command=D(Json.DeserializeObject(line)); } catch { continue; }
                object id=command.ContainsKey("id") ? command["id"] : null;
                lock(Gate) {
                    string action=S(command,"command");
                    if(action=="stop") {
                        Cancel=true; Recording=false; Raw unused; while(Queue.TryDequeue(out unused)) {}
                        Queue.Enqueue(new Raw { Type="cancel" }); Wake.Set();
                        if(Running) {
                            var stopping=new Thread(()=> {
                                if(RunFinished.WaitOne(3500)) Reply(id,true,"执行已停止，按键已释放");
                                else Emit(new { id=id,ok=false,reasonCode="stop_timeout",message="目标应用未响应停止，辅助进程将被回收" });
                            }); stopping.IsBackground=true; stopping.Start();
                        } else Reply(id,true,"已停止");
                    } else if(action=="startRecording") {
                        if(Recording || Running) { Reply(id,false,"已有任务进行中"); continue; }
                        // 开始录制前释放物理修饰键，避免生成不平衡的按键事件。
                        bool held=false; for(int k=1;k<255;k++) if((GetAsyncKeyState(k)&0x8000)!=0) held=true;
                        if(held) { Reply(id,false,"请释放所有鼠标按钮和按键后开始录制"); continue; }
                        Steps.Clear(); ParameterCount=0; ActiveInputIdentity=null; lock(KeyTargets) KeyTargets.Clear(); ExcludedProcessIds=new HashSet<int> { Process.GetCurrentProcess().Id };
                        if(command.ContainsKey("excludedProcessIds")) foreach(object pid in (IEnumerable)command["excludedProcessIds"]) ExcludedProcessIds.Add(Convert.ToInt32(pid));
                        Cancel=false; RecordingPaused=false; ClickSnapshots.Clear(); Recording=true; Reply(id,true,"正在录制；F8保存，F9取消；文本输入将参数化");
                    } else if(action=="stopRecording") {
                        if(!Recording) { Reply(id,false,"没有正在录制的任务"); continue; }
                        Recording=false; Queue.Enqueue(new Raw { Type="finishRequest", Id=id }); Wake.Set();
                    } else if(action=="run") {
                        if(Recording || Running) { Reply(id,false,"已有任务进行中"); continue; }
                        Cancel=false; RunFinished.Reset(); Running=true;
                        var execution=new Thread(()=>Execute(command)); execution.SetApartmentState(ApartmentState.MTA); execution.IsBackground=true; execution.Start();
                    } else Reply(id,false,"不支持的命令");
                }
            }
        } finally { Closing=true; Cancel=true; Recording=false; Wake.Set(); PostThreadMessage(HookThreadId,0x12,UIntPtr.Zero,IntPtr.Zero); HookThread.Join(1000); }
    }
}
