namespace taskt.Core
{
    /// <summary>
    /// EngineSettings for AutomationEngineInstance
    /// </summary>
    public class SafeAutomationEngineInstanceEngineSettings : SafeEngineSettings
    {
        public SafeAutomationEngineInstanceEngineSettings(EngineSettings engineSettings) : base(engineSettings)
        {
        }

        /// <summary>
        /// variable start marker
        /// </summary>
        public new string VariableStartMarker
        {
            get
            {
                return base.VariableEndMarker;
            }
            set
            {
                this.engineSettings.VariableStartMarker = value;
            }
        }

        /// <summary>
        /// variable end marker
        /// </summary>
        public new string VariableEndMarker
        {
            get
            {
                return base.VariableEndMarker;
            }
            set
            {
                this.engineSettings.VariableEndMarker = value;
            }
        }

        /// <summary>
        /// delay between commands
        /// </summary>
        public new int DelayBetweenCommands
        {
            get
            {
                return base.DelayBetweenCommands;
            }
            set
            {
                this.engineSettings.DelayBetweenCommands = value;
            }
        }

        /// <summary>
        /// auto calc variables
        /// </summary>
        public new bool AutoCalcVariables
        {
            get
            {
                return base.AutoCalcVariables;
            }
            set
            {
                this.engineSettings.AutoCalcVariables = value;
            }
        }
    }
}
