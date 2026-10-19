namespace taskt.Core.Automation.Commands
{
    /// <summary>
    /// copy folder commands properties
    /// </summary>
    public interface IFolderCopyFolderProperties : ILExpandableProperties
    {
        /// <summary>
        /// copy sub folder or not
        /// </summary>
        string v_CopySubFolder { get; set; }
    }
}
