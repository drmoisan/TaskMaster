using log4net.Repository.Hierarchy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using UtilitiesCS.OutlookObjects.Folder;

namespace UtilitiesCS.OutlookObjects.Store
{
    public class StoreWrapperController
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public StoreWrapperController(IApplicationGlobals globals)
        {
            Globals = globals;            
            FsConverter = new FilePathHelperConverter(globals.FS).GetSerializablePath;
        }

        internal IApplicationGlobals Globals { get; set; }
        
        public IStoreWrapperViewer Viewer { get; internal set; }

        public StoresWrapper Model { get; internal set; }

        public StoreWrapper Current { get; internal set; }

        internal FolderMinimalWrapper ArchiveOutlook { get; set; }
        internal FilePathHelper ArchiveFS { get; set; }
        internal FolderMinimalWrapper JunkEmail { get; set; }
        internal FolderMinimalWrapper JunkPotential { get; set; }       
        internal Func<string, (string, string)> FsConverter { get; set; }
        

        #region Events

        public void Launch() 
        { 
            Viewer = new StoreWrapperViewer(this);
            Viewer.DisplayName.DataSource = Model.Stores.Select(store => store.DisplayName).ToList();
            
            Viewer.Show();
        }

        public void ButtonOk_Click()
        {
            if (AnyChanges()) { SaveChanges(); }
            Viewer.Close();
        }

        public void ButtonCancel_Click()
        {
            Viewer.Close();
        }

        public void DisplayName_SelectedValueChanged(object sender, EventArgs e)
        {
            if (AnyChanges())
            {
                var response = MyBox.ShowDialog("Save changes?", "Save Changes", 
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (response == DialogResult.Yes) { SaveChanges(); }
            }
            var displayName = Viewer.DisplayName.SelectedValue.ToString();
            Current = Model.Stores.Find(store => store.DisplayName == displayName);
            PopulateWithCurrent();
        }

        public void ArchiveFS_Click()
        {
            if (Viewer.InvokeRequired)
            {
                Viewer.Invoke(() => ArchiveFS_Click());
                return;
            }
            throw new NotImplementedException();
        }

        public void ArchiveOutlook_Click()
        {
            if (Viewer.InvokeRequired)
            {
                Viewer.Invoke(() => ArchiveOutlook_Click());
                return;
            }
            ArchiveOutlook = SelectFolder();
            Viewer.ArchiveOutlook.Text = ArchiveOutlook?.RelativePath;
        }

        public void JunkEmail_Click()
        {
            if (Viewer.InvokeRequired)
            {
                Viewer.Invoke(() => JunkEmail_Click());
                return;
            }
            JunkEmail = SelectFolder();
            Viewer.JunkEmail.Text = JunkEmail?.RelativePath;
        }

        public void JunkPotential_Click()
        {
            if (Viewer.InvokeRequired)
            {
                Viewer.Invoke(() => JunkPotential_Click());
                return;
            }
            JunkPotential = SelectFolder();
            Viewer.JunkPotential.Text = JunkPotential?.RelativePath;
        }

        #endregion Events

        #region Methods
        
        internal bool AnyChanges()
        {
            return !ArchiveOutlook.Equals(Current.ArchiveRoot) || 
                !JunkEmail.Equals(Current.JunkCertain) || 
                !JunkPotential.Equals(Current.JunkPotential) ||
                !ArchiveFS.Equals(Current.ArchiveFsRoot);
        }

        internal void PopulateWithCurrent()
        {
            if (Viewer.InvokeRequired) { Viewer.Invoke(() => PopulateWithCurrent()); return; }
            ArchiveOutlook = Current.ArchiveRoot;
            ArchiveFS = Current.ArchiveFsRoot;
            JunkEmail = Current.JunkCertain;
            JunkPotential = Current.JunkPotential;
            Viewer.ArchiveOutlook.Text = ArchiveOutlook?.RelativePath ?? "Please select an archive";
            if (ArchiveFS is not null && !ArchiveFS.FilePath.IsNullOrEmpty())
            {
                var (specialFolder, relativePath) = FsConverter(ArchiveFS.FilePath);
                if (specialFolder.IsNullOrEmpty() & relativePath.IsNullOrEmpty())
                {
                    Viewer.ArchiveFS.Text = "Please select an archive";
                }
                else
                {
                    Viewer.ArchiveFS.Text = $"{string.Join(" -> ", [specialFolder,relativePath])}";
                }
            }
            Viewer.JunkEmail.Text = JunkEmail?.RelativePath ?? "Please select a folder";
            Viewer.JunkPotential.Text = JunkPotential?.RelativePath ?? "Please select a folder";
        }

        internal void SaveChanges()
        {            
            Current.ArchiveRoot = ArchiveOutlook;
            Current.JunkCertain = JunkEmail;
            Current.JunkPotential = JunkPotential;
            Current.ArchiveFsRoot = ArchiveFS;
            Model.Serialize();
        }

        internal virtual FolderMinimalWrapper SelectFolder()
        {
            try
            {
                var folder = Globals.Ol.NamespaceMAPI.PickFolder();
                if (folder is null) { return null; }
                return new FolderMinimalWrapper(folder, Current.RootFolder);
            }
            catch (Exception e)
            {
                logger.Error($"Error selecting folder. {e.Message}", e);
                return null;
            }                   
        }
        
        #endregion Methods

    }
}
