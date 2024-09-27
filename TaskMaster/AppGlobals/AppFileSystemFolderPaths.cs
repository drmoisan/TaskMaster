using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading.Tasks;
using UtilitiesCS;

namespace TaskMaster
{

    public class AppFileSystemFolderPaths : IFileSystemFolderPaths
    {       
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
            FldrAppData = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "TaskMaster");
            _ = Task.Run(() => CreateMissingPaths(_appData));
            FldrMyDocuments = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            FldrOneDrive = Environment.GetEnvironmentVariable("OneDriveCommercial");
            FldrFlow = Path.Combine(_oneDrive, "Email attachments from Flow");
            CreateMissingPaths(_flow);
            FldrPreReads = Path.Combine(_oneDrive, "_  Workflow", "_ Pre-Reads");
            CreateMissingPaths(_prereads);
            _remap = Path.Combine(_myDocuments, "dictRemap.csv");
            _fldrPythonStaging = Path.Combine(_flow, "Combined", "data");
            SpecialFolders = new ConcurrentDictionary<string, string>
            {
                ["AppData"] = FldrAppData,
                ["MyDocuments"] = FldrMyDocuments,
                ["OneDrive"] = FldrOneDrive,
                ["Flow"] = FldrFlow,
                ["PreReads"] = FldrPreReads,
                ["PythonStaging"] = FldrPythonStaging
            };
        }

        //TODO: Cleanup Staging Files so that they are in one or two directories and not all over the place
        async private Task LoadFoldersAsync()
        {
            FldrAppData = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "TaskMaster");
            Task a = CreateMissingPathsAsync(_appData);

            FldrMyDocuments = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            FldrOneDrive = Environment.GetEnvironmentVariable("OneDriveCommercial");
            FldrFlow = Path.Combine(_oneDrive, "Email attachments from Flow");
            FldrPreReads = Path.Combine(_oneDrive, "_  Workflow", "_ Pre-Reads");
            Task b = CreateMissingPathsAsync(_flow);

            _remap = Path.Combine(_myDocuments, "dictRemap.csv");
            FldrPythonStaging = Path.Combine(_flow, "Combined", "data");

            await Task.WhenAll(a, b);

            SpecialFolders = new ConcurrentDictionary<string, string>
            {
                ["AppData"] = FldrAppData,
                ["MyDocuments"] = FldrMyDocuments,
                ["OneDrive"] = FldrOneDrive,
                ["Flow"] = FldrFlow,
                ["PreReads"] = FldrPreReads,
                ["PythonStaging"] = FldrPythonStaging
            };
        }

        private string _appData;
        public string FldrAppData { get => _appData; protected set => _appData = value; }

        private string _myDocuments;
        public string FldrMyDocuments { get => _myDocuments; protected set => _myDocuments = value; }
        
        private string _oneDrive;
        public string FldrOneDrive { get => _oneDrive; protected set => _oneDrive = value; }
        
        private string _flow;
        public string FldrFlow { get => _flow; protected set => _flow = value; }

        private string _prereads;
        public string FldrPreReads { get => _prereads; protected set => _prereads = value; }
        
        private string _fldrPythonStaging;
        public string FldrPythonStaging { get => _fldrPythonStaging; protected set => _fldrPythonStaging = value; }
        
        private IAppStagingFilenames _filenames;
        public IAppStagingFilenames Filenames { get => _filenames; protected set => _filenames = value; }

        private ConcurrentDictionary<string, string> _specialFolders;
        public ConcurrentDictionary<string, string> SpecialFolders { get => _specialFolders; protected set => _specialFolders = value; }

        private string _remap;
    }
}