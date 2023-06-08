using System;
using System.IO;
using UtilitiesCS;

namespace TaskMaster
{

    public class AppFileSystemFolderPaths : IFileSystemFolderPaths
    {

        private string _appStaging;
        private string _stagingPath;
        private string _myD;
        private string _oneDrive;
        private string _flow;
        private string _prereads;
        private string _remap;
        private string _fldrPythonStaging;
        private IAppStagingFilenames _filenames;

        public AppFileSystemFolderPaths()
        {
            LoadFolders();
            _filenames = new AppStagingFilenames();
        }

        public void Reload()
        {
            LoadFolders();
        }

        private void CreateMissingPaths(string filepath)
        {
            if (!Directory.Exists(filepath))
            {
                Directory.CreateDirectory(filepath);
            }
        }

        private void LoadFolders()
        {
            _appStaging = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "TaskMaster");
            CreateMissingPaths(_appStaging);

            _stagingPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            _myD = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            _oneDrive = Environment.GetEnvironmentVariable("OneDriveCommercial");
            _flow = Path.Combine(_oneDrive, "Email attachments from Flow");
            CreateMissingPaths(_flow);

            _prereads = Path.Combine(_oneDrive, "_  Workflow", "_ Pre-Reads");
            CreateMissingPaths(_prereads);

            _remap = Path.Combine(_stagingPath, "dictRemap.csv");
            _fldrPythonStaging = Path.Combine(_flow, "Combined", "data");
        }

        public string FldrAppData
        {
            get
            {
                return _appStaging;
            }
        }

        public string FldrStaging
        {
            get
            {
                return _stagingPath;
            }
        }

        public string FldrMyD
        {
            get
            {
                return _myD;
            }
        }

        public string FldrRoot
        {
            get
            {
                return _oneDrive;
            }
        }

        public string FldrFlow
        {
            get
            {
                return _flow;
            }
        }

        public string FldrPreReads
        {
            get
            {
                return _prereads;
            }
        }

        public string FldrPythonStaging
        {
            get
            {
                return _fldrPythonStaging;
            }
            set
            {
                _fldrPythonStaging = value;
            }
        }

        public IAppStagingFilenames Filenames
        {
            get
            {
                return _filenames;
            }
        }

    }
}