﻿using log4net.Repository.Hierarchy;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UtilitiesCS;
using UtilitiesCS.Extensions;

namespace TaskMaster
{

    public class AppFileSystemFolderPaths : IFileSystemFolderPaths
    {       
        public AppFileSystemFolderPaths()
        {
            LoadFolders();
            _filenames = new AppStagingFilenames();
        }

        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

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

        private bool TryAddSpecialFolder(string name, string[] pathParts)
        {
            if (name.IsNullOrEmpty()) { return false; }
            
            else if (pathParts.IsNullOrEmpty())
            {
                logger.Debug($"Error in {nameof(TryAddSpecialFolder)} for key {nameof(name)} because {nameof(pathParts)} is null or empty. {TraceUtility.GetMyTraceString(new System.Diagnostics.StackTrace())}");
                return false;
            }
            
            else if (pathParts.Any(x => x is null)) 
            {
                var locations = Enumerable.Range(0, pathParts.Length).Where(i => pathParts[i] is null).Select(i => i.ToString()).SentenceJoin();
                logger.Debug($"Error in {nameof(TryAddSpecialFolder)} for key {nameof(name)} because {nameof(pathParts)} has null elements at {locations}. {TraceUtility.GetMyTraceString(new System.Diagnostics.StackTrace())}");
                return false;
            }

            SpecialFolders ??= [];
            
            try
            {
                SpecialFolders[name] = Path.Combine(pathParts);
                CreateMissingPaths(SpecialFolders[name]);
                return true;
            }
            
            catch (Exception e)
            {
                logger.Error(e.Message, e);
                return false;
            }

        }

        private void LoadFolders()
        {
            SpecialFolders = [];
            TryAddSpecialFolder("AppData", [Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), nameof(TaskMaster)]);
            TryAddSpecialFolder("MyDocuments", [Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)]);

            if (!TryAddSpecialFolder("OneDrive", [Environment.GetEnvironmentVariable("OneDriveCommercial")])) 
            {
                if (!TryAddSpecialFolder("OneDrive", [Environment.GetEnvironmentVariable("OneDrive")])) 
                {
                    if (!TryAddSpecialFolder("OneDrive", [Environment.GetEnvironmentVariable("OneDrivePersonal")])) 
                    {
                        if(SpecialFolders.Count > 0) 
                        {
                            if(SpecialFolders.TryGetValue("AppData", out var appData))
                            {
                                TryAddSpecialFolder("OneDrive", [appData]);
                            }
                            else
                            {
                                TryAddSpecialFolder("OneDrive", [SpecialFolders.First().Value]);
                            }
                        }
                        else { throw new InvalidOperationException("No know network or local folders set in environment variables"); }
                    }
                }
            }
            SpecialFolders.TryGetValue("OneDrive", out var oneDrive);

            TryAddSpecialFolder("Flow", [oneDrive, "Email attachments from Flow"]);
            SpecialFolders.TryGetValue("Flow", out var flow);
            TryAddSpecialFolder("PreReads", [oneDrive, "_  Workflow", "_ Pre-Reads"]);
            
            if (SpecialFolders.TryGetValue("MyDocuments", out var myDocuments))
            {
                _remap = Path.Combine(myDocuments, "dictRemap.csv");
            }

            TryAddSpecialFolder("PythonStaging", [flow, "Combined", "data"]);
            
        }

        //TODO: Cleanup Staging Files so that they are in one or two directories and not all over the place
        async private Task LoadFoldersAsync()
        {
            await Task.Run(LoadFolders);
        }

        //private string _appData;
        //public string FldrAppData { get => _appData; protected set => _appData = value; }

        //private string _myDocuments;
        //public string FldrMyDocuments { get => _myDocuments; protected set => _myDocuments = value; }
        
        //private string _oneDrive;
        //public string FldrOneDrive { get => _oneDrive; protected set => _oneDrive = value; }
        
        //private string _flow;
        //public string FldrFlow { get => _flow; protected set => _flow = value; }

        //private string _prereads;
        //public string FldrPreReads { get => _prereads; protected set => _prereads = value; }
        
        //private string _fldrPythonStaging;
        //public string FldrPythonStaging { get => _fldrPythonStaging; protected set => _fldrPythonStaging = value; }
        
        private IAppStagingFilenames _filenames;
        public IAppStagingFilenames Filenames { get => _filenames; protected set => _filenames = value; }

        private ConcurrentDictionary<string, string> _specialFolders;
        public ConcurrentDictionary<string, string> SpecialFolders { get => _specialFolders; protected set => _specialFolders = value; }

        private string _remap;
    }
}