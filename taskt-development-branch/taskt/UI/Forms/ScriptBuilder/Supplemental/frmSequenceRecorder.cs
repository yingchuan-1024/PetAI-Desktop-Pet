using System;
using System.Collections.Generic;
using taskt.Core.Automation.Commands;
using static taskt.Core.Automation.User32.User32Functions;

namespace taskt.UI.Forms.ScriptBuilder.Supplemental
{
    public partial class frmSequenceRecorder : UIForm
    {
        public static List<Core.Automation.Commands.ScriptCommand> commandList;
        public ScriptBuilder.frmScriptBuilder callBackForm { get; set; }
        private bool isRecording;
        private readonly Action startHook;
        private readonly Action stopHook;
        private readonly Action finishRecording;
        private readonly int optionsHeight;
        private readonly int recordButtonTop;
        private System.Drawing.Point optionsLocation;

        public frmSequenceRecorder() : this(null, null, null)
        {
        }

        private frmSequenceRecorder(Action start, Action stop, Action finish)
        {
            InitializeComponent();
            startHook = start ?? StartRecordingHook;
            stopHook = stop ?? GlobalHook.StopHook;
            finishRecording = finish ?? FinalizeRecording;
            optionsHeight = Height;
            recordButtonTop = uiBtnRecord.Top;
        }

        private void btnStartRecording_Click(object sender, EventArgs e)
        {
        }

        void OnHookStopped(object sender, EventArgs e)
        {
            GlobalHook.HookStopped -= OnHookStopped;
            if (!isRecording) return;
            isRecording = false;
            RestoreRecordingOptions();
            finishRecording();
        }

        private void RestoreRecordingOptions()
        {
            Height = optionsHeight;
            Location = optionsLocation;
            TopMost = false;
            uiBtnRecord.Top = recordButtonTop;
            uiBtnRecord.DisplayText = ChineseUI.Translate("Start");
            uiBtnRecord.Image = taskt.Properties.Resources.various_record_button;
            pnlOptions.Show();
            lblRecording.Hide();
        }

        private void FinalizeRecording()
        {
            var commandList = GlobalHook.generatedCommands;
            var outputList = new List<Core.Automation.Commands.ScriptCommand>();
            if (chkGroupIntoSequence.Checked)
            {
                var newSequence = new Core.Automation.Commands.SequenceCommand();
                foreach (Core.Automation.Commands.ScriptCommand cmd in commandList)
                {
                    newSequence.v_scriptActions.Add(cmd);
                }
                if (newSequence.v_scriptActions.Count > 0)
                {
                    outputList.Add(newSequence);
                }
            }
            else if (chkGroupMovesIntoSequences.Checked)
            {
                var newSequence = new Core.Automation.Commands.SequenceCommand();
                foreach (Core.Automation.Commands.ScriptCommand cmd in commandList)
                {
                    if (cmd is Core.Automation.Commands.MoveMouseCommand)
                    {
                        var sendMouseCmd = (Core.Automation.Commands.MoveMouseCommand)cmd;
                        if (sendMouseCmd.v_MouseClick != "None")
                        {
                            outputList.Add(newSequence);
                            newSequence = new Core.Automation.Commands.SequenceCommand();
                            outputList.Add(cmd);
                        }
                        else
                        {
                            newSequence.v_scriptActions.Add(cmd);
                        }
                    }
                    else if (cmd is EnterKeysCommand)
                    {
                        outputList.Add(newSequence);
                        newSequence = new Core.Automation.Commands.SequenceCommand();
                        outputList.Add(cmd);
                    }
                    else
                    {
                        newSequence.v_scriptActions.Add(cmd);
                    }
                }
                if (newSequence.v_scriptActions.Count > 0)
                {
                    outputList.Add(newSequence);
                }
            }
            else
            {
                outputList = commandList;
            }
            var commentCommand = new Core.Automation.Commands.CommentCommand();
            commentCommand.v_Comment = "Sequence Recorded " + DateTime.Now.ToString();
            outputList.Insert(0, commentCommand);
            foreach (var cmd in outputList)
            {
                callBackForm.AddCommandToListView(cmd);
            }
            this.Close();
        }

        private void btnStopRecording_Click(object sender, EventArgs e)
        {
            stopHook();
        }

        private void frmSequenceRecorder_Load(object sender, EventArgs e)
        {
        }

        private void chkGroupIntoSequences_CheckedChanged(object sender, EventArgs e)
        {
        }

        private void chkCaptureMouse_CheckedChanged(object sender, EventArgs e)
        {
            chkGroupMovesIntoSequences.Checked = chkCaptureMouse.Checked;
            chkGroupMovesIntoSequences.Enabled = chkGroupMovesIntoSequences.Checked;
        }

        private void StartRecordingHook()
        {
            this.BringToFront();
            MoveFormToBottomRight(this);
            int.TryParse(txtHookResolution.Text, out int samplingResolution);
            GlobalHook.StartScreenRecordingHook(chkCaptureClicks.Checked, chkCaptureMouse.Checked, chkGroupMovesIntoSequences.Checked, chkCaptureKeyboard.Checked, chkCaptureWindowEvents.Checked, chkActivateTopLeft.Checked, chkTrackWindowSize.Checked, chkTrackWindowsOpenLocation.Checked, samplingResolution, txtHookStop.Text);
        }

        private void uiBtnRecord_Click(object sender, EventArgs e)
        {
            if (isRecording)
            {
                stopHook();
                return;
            }

            optionsLocation = Location;
            isRecording = true;
            GlobalHook.HookStopped += OnHookStopped;
            try
            {
                this.Height = 150;
                this.TopMost = true;
                uiBtnRecord.Top = lblRecording.Top + 50;
                pnlOptions.Hide();
                lblRecording.Text = ChineseUI.RecordingHint(txtHookStop.Text);
                lblRecording.Show();
                uiBtnRecord.DisplayText = ChineseUI.Translate("Stop");
                uiBtnRecord.Image = taskt.Properties.Resources.various_stop_button;
                commandList = new List<Core.Automation.Commands.ScriptCommand>();
                startHook();
            }
            catch
            {
                GlobalHook.HookStopped -= OnHookStopped;
                isRecording = false;
                RestoreRecordingOptions();
                stopHook();
                throw;
            }
        }

        internal static void CheckRecordingState()
        {
            foreach (var caption in new[] { "开始", "Start" })
            {
                int starts = 0, stops = 0, finishes = 0;
                using (var recorder = new frmSequenceRecorder(() => starts++, () => stops++, () => finishes++))
                {
                    ChineseUI.Apply(recorder);
                    recorder.uiBtnRecord.DisplayText = caption;
                    recorder.txtHookStop.Text = "F3";
                    recorder.uiBtnRecord_Click(null, EventArgs.Empty);
                    if (!recorder.isRecording || starts != 1 || stops != 0 || recorder.uiBtnRecord.DisplayText != "停止" || recorder.lblRecording.Text != "按 F3 键停止录制！")
                        throw new InvalidOperationException("Recorder start branch failed: " + caption);
                    recorder.uiBtnRecord.DisplayText = "Start";
                    recorder.uiBtnRecord_Click(null, EventArgs.Empty);
                    if (starts != 1 || stops != 1 || !recorder.isRecording)
                        throw new InvalidOperationException("Recorder stop branch depended on caption");
                    recorder.OnHookStopped(null, EventArgs.Empty);
                    recorder.OnHookStopped(null, EventArgs.Empty);
                    if (recorder.isRecording || finishes != 1 || recorder.uiBtnRecord.DisplayText != "开始" || recorder.Height != recorder.optionsHeight || recorder.TopMost)
                        throw new InvalidOperationException("Recorder stopped callback did not restore idle state");
                    recorder.uiBtnRecord_Click(null, EventArgs.Empty);
                    recorder.OnHookStopped(null, EventArgs.Empty);
                    if (starts != 2 || finishes != 2)
                        throw new InvalidOperationException("Recorder could not restart after callback");
                }
            }
            int cleanups = 0;
            using (var recorder = new frmSequenceRecorder(() => { throw new InvalidOperationException("fake start failure"); }, () => cleanups++, () => { throw new InvalidOperationException("Failed start finalized"); }))
            {
                try { recorder.uiBtnRecord_Click(null, EventArgs.Empty); }
                catch (InvalidOperationException ex) { if (ex.Message != "fake start failure") throw; }
                recorder.OnHookStopped(null, EventArgs.Empty);
                if (recorder.isRecording || cleanups != 1 || recorder.uiBtnRecord.DisplayText != "开始" || recorder.Height != recorder.optionsHeight)
                    throw new InvalidOperationException("Recorder start failure rollback failed");
            }
            Console.WriteLine("Recorder state self-check passed: Chinese/English start, caption-independent stop, stopped callback/restart, start failure rollback; fake hooks only");
        }

        private void chkActivateTopLeft_CheckedChanged(object sender, EventArgs e)
        {
            if (chkActivateTopLeft.Checked)
            {
                chkTrackWindowsOpenLocation.Checked = false;
            }
        }

        private void chkTrackWindowsOpenLocation_CheckedChanged(object sender, EventArgs e)
        {
            if (chkTrackWindowsOpenLocation.Checked)
            {
                chkActivateTopLeft.Checked = false;
            }
        }
    }
}
