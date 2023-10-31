using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using ToDoModel;
using UtilitiesCS;
using UtilitiesCS.EmailIntelligence;
using UtilitiesCS.ReusableTypeClasses;
using UtilitiesCS.Threading;

namespace TaskMaster
{

    public class AppToDoObjects : IToDoObjects
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public AppToDoObjects(ApplicationGlobals ParentInstance)
        {
            _parent = ParentInstance;
        }

        async public Task LoadAsync()
        {
            var tasks = new List<Task> 
            {
                LoadDictPPLAsync(),
                LoadDictRemapAsync(),
                LoadProjInfoAsync(),
                LoadIdListAsync(),
                LoadCategoryFiltersAsync(),
                LoadPrefixListAsync()
            };
            await Task.WhenAll(tasks);
            //logger.Debug($"{nameof(AppToDoObjects)}.{nameof(LoadAsync)} is complete.");
        }

        private Properties.Settings _defaults = Properties.Settings.Default;
        
        private T Initialized<T>(T obj, Func<T> initializer)
        {
            if (obj is null)
            {
                obj = initializer.Invoke();
            }
            return obj;
        }

        private readonly ApplicationGlobals _parent;
        public IApplicationGlobals Parent
        {
            get
            {
                return _parent;
            }
        }

        private string _projInfo_Filename;
        public string ProjInfo_Filename => Initialized(_projInfo_Filename, () => _projInfo_Filename = _defaults.FileName_ProjInfo);
        private ProjectInfo _projInfo;
        public IProjectInfo ProjInfo => Initialized(_projInfo, () => LoadProjInfo());
        async private Task LoadProjInfoAsync()
        {
            _projInfo = await Task.Run(() => new ProjectInfo(filename: _defaults.FileName_ProjInfo, folderpath: Parent.FS.FldrAppData));
            //_projInfo = await Task.Factory.StartNew(
            //                  () => new ProjectInfo(filename: _defaults.FileName_ProjInfo,
            //                                        folderpath: Parent.FS.FldrAppData), 
            //                  default,
            //                  TaskCreationOptions.None,
            //                  PriorityScheduler.BelowNormal);
            if (_projInfo.Count == 0) 
            {
                await Task.Run(() => _projInfo.Rebuild(Parent.Ol.App));
                //await Task.Factory.StartNew(
                //      () => _projInfo.Rebuild(Parent.Ol.App),
                //      default,
                //      TaskCreationOptions.None,
                //      PriorityScheduler.BelowNormal);
            }
        }
        private IProjectInfo LoadProjInfo()
        {
            var projectInfo = new ProjectInfo(filename: _defaults.FileName_ProjInfo,
                                              folderpath: Parent.FS.FldrAppData);
            if (projectInfo.Count == 0) { projectInfo.Rebuild(Parent.Ol.App); }
            return projectInfo;
        }

        //private string _dictPPL_Filename;
        //public string DictPPL_Filename { get => Initialized(_dictPPL_Filename, () => _defaults.FilenameDictPpl);}
        ////{
        ////    get
        ////    {
        ////        if (_dictPPL_Filename is null)
        ////            _dictPPL_Filename = _defaults.FilenameDictPpl;
        ////        return _dictPPL_Filename;
        ////    }
        ////}
        

        ////TODO: Convert DictPPL to SCODictionary
        //private Dictionary<string, string> _dictPPL;
        //public Dictionary<string, string> DictPPL => Initialized(_dictPPL, () => LoadDictJSON(Parent.FS.FldrStaging, DictPPL_Filename));
        ////{
        ////    get
        ////    {
        ////        if (_dictPPL is null)
        ////            _dictPPL = LoadDictJSON(Parent.FS.FldrStaging, DictPPL_Filename);
        ////        return _dictPPL;
        ////    }
        ////}
        //async private Task LoadDictPPLAsync() 
        //{ _dictPPL = await LoadDictJSONAsync(Parent.FS.FldrStaging, DictPPL_Filename); }
        //public void DictPPL_Save()
        //{
        //    File.WriteAllText(Path.Combine(Parent.FS.FldrStaging, DictPPL_Filename), 
        //                      JsonConvert.SerializeObject(_dictPPL, Formatting.Indented));
        //}

        private PeopleScoDictionary _dictPPL;
        public IPeopleScoDictionary DictPPL => Initialized(_dictPPL, () => LoadDictPPL());
        private PeopleScoDictionary LoadDictPPL()
        {
            var dictPPL = new PeopleScoDictionary(filename: _defaults.FilenameDictPpl,
                                                  folderpath: Parent.FS.FldrPythonStaging,
                                                  appGlobals: Parent,
                                                  prefix: PrefixList.Find(x => x.PrefixType == PrefixTypeEnum.People));
            
            return dictPPL;
        }
        async private Task LoadDictPPLAsync() => _dictPPL = await Task.Factory.StartNew(
            () => LoadDictPPL(), default, TaskCreationOptions.LongRunning, TaskScheduler.Current);
        async private Task LoadPrefixAndDictPeopleAsync()
        {
            await LoadPrefixListAsync();
            await LoadDictPPLAsync();
        }

        public string FnameIDList => _defaults.FileName_IDList;
        
        private IIDList _idList;
        //TODO: Convert IDList to ScoCollection
        public IIDList IDList => Initialized(_idList, () => LoadIDList());
        async private Task LoadIdListAsync() => _idList = await Task.Run(() => LoadIDList());
        
        private IIDList LoadIDList()
        {
            var idList = new IDList(FnameIDList,
                                    Parent.FS.FldrAppData,
                                    Parent.Ol.App);
            if (idList.Count == 0) { idList.RefreshIDList(); }
            return idList;
        }

        private string _fnameDictRemap;
        public string FnameDictRemap => Initialized(_fnameDictRemap, () => _fnameDictRemap = _defaults.FileName_DictRemap);

        private ScoDictionary<string, string> _dictRemap;
        public IScoDictionary<string, string> DictRemap => Initialized(_dictRemap, () => LoadDictRemap());
        private ScoDictionary<string, string> LoadDictRemap()
        {
            var dictRemap = new ScoDictionary<string, string>(filename: FnameDictRemap,
                                                              folderpath: Parent.FS.FldrPythonStaging);
            return dictRemap;
        }
        async private Task LoadDictRemapAsync() => _dictRemap = await Task.Factory.StartNew(
            () => LoadDictRemap(), default, TaskCreationOptions.LongRunning, TaskScheduler.Current);

        //private Dictionary<string, string> _dictRemap;
        //public Dictionary<string, string> DictRemap => Initialized(_dictRemap, () => LoadDictJSON(Parent.FS.FldrStaging, FnameDictRemap));
        //async private Task LoadDictRemapAsync() => _dictRemap = await LoadDictJSONAsync(Parent.FS.FldrStaging, FnameDictRemap);

        //TODO: Convert CategoryFilters to ScoCollection
        private ISerializableList<string> _catFilters;
        public ISerializableList<string> CategoryFilters
        {
            get => Initialized(_catFilters, () => new SerializableList<string>(filename: _defaults.FileName_CategoryFilters,
                                                                               folderpath: _parent.FS.FldrPythonStaging));
            set
            {
                _catFilters = value;
                if (_catFilters.Folderpath == "")
                {
                    _catFilters.Folderpath = _parent.FS.FldrPythonStaging;
                    _catFilters.Filename = _defaults.FileName_CategoryFilters;
                }
                _catFilters.Serialize();
            }
        }
        async private Task LoadCategoryFiltersAsync()
        {
            _catFilters = await Task.Factory.StartNew(
                () => new SerializableList<string>(filename: _defaults.FileName_CategoryFilters,
                                                   folderpath: _parent.FS.FldrPythonStaging),
                default(CancellationToken));
                //default,
                //TaskCreationOptions.None,
                //PriorityScheduler.BelowNormal);
        }

        // Prefix List
        private ScoCollection<IPrefix> _prefixList;
        public ScoCollection<IPrefix> PrefixList => Initialized(_prefixList, () => LoadPrefixList());
        public ScoCollection<IPrefix> LoadPrefixList()
        {
            var prefixList = new ScoCollection<IPrefix>(fileName: _defaults.FileName_PrefixList,
                                                        folderPath: Parent.FS.FldrPythonStaging);
            if (prefixList.Count == 0) 
            { 
                var tdDefaults = new ToDoDefaults();
                foreach (var prefix in tdDefaults.PrefixList) { prefixList.Add(prefix); }
                prefixList.Serialize();
            }
            return prefixList;
        }
        async private Task LoadPrefixListAsync()
        {
            _prefixList = await Task.Factory.StartNew(
                              () => LoadPrefixList(),
                              default(CancellationToken));
                              //default,
                              //TaskCreationOptions.None,
                              //PriorityScheduler.BelowNormal);
        }


        //private Dictionary<string, string> LoadDictCSV(string fpath, string filename)
        //{
        //    var dict = CSVDictUtilities.LoadDictCSV(fpath, filename.Split('.')[0] + ".csv");
        //    if (dict is not null)
        //        WriteDictJSON(dict, Path.Combine(fpath, filename));
        //    return dict;
        //}

        //async private Task<Dictionary<string, string>> LoadDictCSVAsync(string fpath, string filename)
        //{
        //    var dict = await Task<Dictionary<string,string>>.Factory.StartNew(
        //                     () => CSVDictUtilities.LoadDictCSV(fpath, filename.Split('.')[0] + ".csv"),
        //                     default,
        //                     TaskCreationOptions.None,
        //                     PriorityScheduler.BelowNormal);

        //    if (dict is not null)
        //        _ = Task.Factory.StartNew(
        //            () => WriteDictJSON(dict, Path.Combine(fpath, filename)),
        //            default,
        //            TaskCreationOptions.None,
        //            PriorityScheduler.BelowNormal);
        //    return dict;
        //}

        ////TODO: Deprecate LoadDictJSON
        //private Dictionary<string, string> LoadDictJSON(string fpath, string filename)
        //{

        //    string filepath = Path.Combine(fpath, filename);
        //    Dictionary<string, string> dict = null;
        //    var response = DialogResult.Ignore;

        //    try
        //    {                
        //        dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(Path.Combine(Parent.FS.FldrStaging, DictPPL_Filename)));
        //    }
        //    catch (FileNotFoundException ex)
        //    {
        //        response = MessageBox.Show("Error", filepath + "not found. Load from CSV?", MessageBoxButtons.YesNo,MessageBoxIcon.Error);
        //    }
        //    catch (Exception ex)
        //    {
        //        response = MessageBox.Show("Error", filepath + "encountered a problem. " + ex.Message + "Load from CSV?", MessageBoxButtons.YesNo, MessageBoxIcon.Error);
        //    }
        //    finally
        //    {
        //        if (response == DialogResult.Yes)
        //        {
        //            dict = LoadDictCSV(fpath, filename);
        //        }
        //        else if (response == DialogResult.No)
        //        {
        //            response = MessageBox.Show("Error", "Start a new blank dictionary?", MessageBoxButtons.YesNo, MessageBoxIcon.Error);
        //            if (response == DialogResult.Yes)
        //            {
        //                dict = new Dictionary<string, string>();
        //            }
        //            else
        //            {
        //                throw new ArgumentNullException("Cannot proceed without dictionary: " + filename);
        //            }
        //        }
        //    }
        //    return dict;
        //}

        //async private Task<Dictionary<string, string>> LoadDictJSONAsync(string fpath, string filename)
        //{

        //    string filepath = Path.Combine(fpath, filename);
        //    Dictionary<string, string> dict = null;
        //    var response = DialogResult.Ignore;

        //    try
        //    {
        //        dict = await Task<Dictionary<string, string>>.Factory.StartNew(
        //                     () => JsonConvert.DeserializeObject<Dictionary<string, string>>(
        //                         File.ReadAllText(
        //                            Path.Combine(
        //                                Parent.FS.FldrStaging, 
        //                                DictPPL_Filename))),
        //                     default,
        //                     TaskCreationOptions.None,
        //                     PriorityScheduler.BelowNormal);
        //    }
        //    catch (FileNotFoundException)
        //    {
                
        //        response = MessageBox.Show("Error", filepath + "not found. Load from CSV?", MessageBoxButtons.YesNo, MessageBoxIcon.Error);
        //    }
        //    catch (Exception ex)
        //    {
        //        response = MessageBox.Show("Error", filepath + "encountered a problem. " + ex.Message + "Load from CSV?", MessageBoxButtons.YesNo, MessageBoxIcon.Error);
        //    }
        //    finally
        //    {
        //        if (response == DialogResult.Yes)
        //        {
        //            dict = await LoadDictCSVAsync(fpath, filename);
        //        }
        //        else if (response == DialogResult.No)
        //        {
        //            response = MessageBox.Show("Error", "Start a new blank dictionary?", MessageBoxButtons.YesNo, MessageBoxIcon.Error);
        //            if (response == DialogResult.Yes)
        //            {
        //                dict = new Dictionary<string, string>();
        //            }
        //            else
        //            {
        //                throw new ArgumentNullException("Cannot proceed without dictionary: " + filename);
        //            }
        //        }
        //    }
        //    return dict;
        //}

        ////TODO: Deprecate WriteDictJSON
        //public void WriteDictJSON(Dictionary<string, string> dict, string filepath)
        //{
        //    File.WriteAllText(filepath, JsonConvert.SerializeObject(dict, Formatting.Indented));
        //}

    }
}