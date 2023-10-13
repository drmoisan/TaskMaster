using System;
using System.IO;
using System.Threading.Tasks;
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

        private AppFileSystemFolderPaths(bool async){}

        async public static Task<AppFileSystemFolderPaths> LoadAsync()
        {
            var fs = new AppFileSystemFolderPaths(true);
            await fs.LoadFoldersAsync();
            fs._filenames = new AppStagingFilenames();
            return fs;
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

        async private Task CreateMissingPathsAsync(string filepath)
        {
            if (!Directory.Exists(filepath))
            {
                await Task.Run(()=> Directory.CreateDirectory(filepath));
            }
        }

        private void LoadFolders()
        {
            _appStaging = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "TaskMaster");
            _ = Task.Run(() => CreateMissingPaths(_appStaging));
            //_ = Task.Factory.StartNew(() => CreateMissingPaths(_appStaging),
            //                          default,
            //                          TaskCreationOptions.None,
            //                          PriorityScheduler.BelowNormal);

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

        //TODO: Cleanup Staging Files so that they are in one or two directories and not all over the place
        async private Task LoadFoldersAsync()
        {
            _appStaging = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "TaskMaster");
            Task a = CreateMissingPathsAsync(_appStaging);  

            _stagingPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            _myD = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            _oneDrive = Environment.GetEnvironmentVariable("OneDriveCommercial");
            _flow = Path.Combine(_oneDrive, "Email attachments from Flow");
            Task b = CreateMissingPathsAsync(_flow);

            _prereads = Path.Combine(_oneDrive, "_  Workflow", "_ Pre-Reads");
            Task c = CreateMissingPathsAsync(_prereads);

            _remap = Path.Combine(_stagingPath, "dictRemap.csv");
            _fldrPythonStaging = Path.Combine(_flow, "Combined", "data");

            await Task.WhenAll(a, b, c);
        }

        public string FldrAppData { get => _appStaging; }

        public string FldrStaging { get => _stagingPath; }
        
        public string FldrMyD { get => _myD; }

        public string FldrRoot { get => _oneDrive; }
        
        public string FldrFlow { get => _flow; }

        public string FldrPreReads { get => _prereads; }
        
        public string FldrPythonStaging { get => _fldrPythonStaging; set => _fldrPythonStaging = value; }
        
        public IAppStagingFilenames Filenames { get => _filenames; }

    }
}