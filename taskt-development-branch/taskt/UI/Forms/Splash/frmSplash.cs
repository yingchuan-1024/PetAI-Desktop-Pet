using System;
using System.Windows.Forms;
using System.IO;
using taskt.Core.IO;

namespace taskt.UI.Forms.Splash
{
    public partial class frmSplash : Form
    {
        public frmSplash()
        {
            InitializeComponent();
        }

        private void frmSplash_Load(object sender, EventArgs e)
        {
            // check exist or create AutoSave, RunWithoutSaving, BeforeConverted folders
            CreateFolderProcess(Folders.GetAutoSaveFolderPath());

            CreateFolderProcess(Folders.GetRunWithoutSavingFolderPath());

            CreateFolderProcess(Folders.GetBeforeConvertedFolderPath());

            CreateFolderProcess(Folders.GetTasktTemporaryFolderPath());

            CreateFolderProcess(Folders.GetUserTemporaryFolderPath());
        }

        /// <summary>
        /// create folder process when not exists
        /// </summary>
        /// <param name="path"></param>
        private static void CreateFolderProcess(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }
    }
}
