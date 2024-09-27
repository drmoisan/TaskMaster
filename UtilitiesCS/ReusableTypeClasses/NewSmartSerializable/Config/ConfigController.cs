using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UtilitiesCS.ReusableTypeClasses.NewSmartSerializable.Config
{
    public class ConfigController
    {

        #region ctor

        public ConfigController(IApplicationGlobals globals, INewSmartSerializableConfig config) 
        { 
            Config = config;
            Globals = globals;           
        }

        public ConfigController Init() 
        {
            FilePathConverter = new FilePathHelperConverter(Globals.FS);
            SpecialFolderList = [.. Globals.FS.SpecialFolders.Keys];
            SpecialFolderList.Insert(0, "None");

            Viewer = new ConfigViewer();            
            Viewer.ComboSpecialFolder.DataSource = Globals.FS.SpecialFolders.Keys;
            var (specialFolder, relativePath) = FilePathConverter.GetSerializablePath(Config.Disk.FolderPath);
            var specialFolderSelection = specialFolder == "Not Found" ? "None" : specialFolder;
            Viewer.ComboSpecialFolder.SelectedItem = specialFolderSelection;
            Viewer.RelativePath.Text = relativePath;
            Viewer.FileName.Text = Config.Disk.FileName;
            Viewer.SetController(this);
            return this;
        }

        public static ConfigController Show(IApplicationGlobals globals, INewSmartSerializableConfig config)
        {            
            var controller = new ConfigController(globals, config).Init();            
            controller.Viewer.Show();
            return controller;
        }

        #endregion ctor

        #region Properties
        internal INewSmartSerializableConfig Config { get; set; }
        internal ConfigViewer Viewer { get; set; }
        internal IApplicationGlobals Globals { get; set; }
        internal FilePathHelperConverter FilePathConverter { get; set; }
        internal List<string> SpecialFolderList { get; set; }
        #endregion Properties

        #region Events

        internal void Cancel()
        {
            Viewer.Close();
        }

        internal async Task ChangeSpecialFolderAsync()
        {
            throw new NotImplementedException();
        }

        internal async Task OpenFileChooserAsync()
        {
            throw new NotImplementedException();
        }

        internal async Task SaveAsync()
        {
            throw new NotImplementedException();
        }

        #endregion Events

        
    }
}
