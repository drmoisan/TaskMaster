using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.VisualBasic;
using Newtonsoft.Json;
using ToDoModel;
using UtilitiesCS;
using UtilitiesCS.EmailIntelligence;
using UtilitiesVB;

namespace TaskMaster
{

    public class AppToDoObjects : IToDoObjects
    {
        public AppToDoObjects(ApplicationGlobals ParentInstance)
        {
            _parent = ParentInstance;
        }

        private ProjectInfo _projInfo;
        private Dictionary<string, string> _dictPPL;
        private ListOfIDs _IDList;
        private readonly ApplicationGlobals _parent;
        private Dictionary<string, string> _dictRemap;
        private ISerializableList<string> _catFilters;
        private Properties.Settings _defaults = Properties.Settings.Default;
                        
        public IApplicationGlobals Parent
        {
            get
            {
                return _parent;
            }
        }

        public string ProjInfo_Filename
        {
            get
            {
                return _defaults.FileName_ProjInfo;
            }
        }

        public IProjectInfo ProjInfo
        {
            get
            {
                if (_projInfo is null)
                {
                    _projInfo = ToDoProjectInfoUtilities.LoadToDoProjectInfo(
                        Path.Combine(Parent.FS.FldrAppData, _defaults.FileName_ProjInfo));
                }
                return _projInfo;
            }
        }

        public string DictPPL_Filename
        {
            get
            {
                return _defaults.FilenameDictPpl;
            }
        }

        public Dictionary<string, string> DictPPL
        {
            get
            {
                if (_dictPPL is null)
                    _dictPPL = LoadDictJSON(Parent.FS.FldrStaging, DictPPL_Filename);
                return _dictPPL;
            }
        }

        public void DictPPL_Save()
        {
            File.WriteAllText(Path.Combine(Parent.FS.FldrStaging, DictPPL_Filename), 
                              JsonConvert.SerializeObject(_dictPPL, Formatting.Indented));
        }

        public string FnameIDList
        {
            get
            {
                return _defaults.FileName_IDList;
            }
        }

        public IListOfIDs IDList
        {
            get
            {
                if (_IDList is null)
                {
                    _IDList = new ListOfIDs(Path.Combine(Parent.FS.FldrAppData,
                                                         _defaults.FileName_IDList), _parent.Ol.App);
                }
                return _IDList;
            }
        }

        public string FnameDictRemap
        {
            get
            {
                return _defaults.FileName_DictRemap;
            }
        }

        public Dictionary<string, string> DictRemap
        {
            get
            {
                if (_dictRemap is null)
                {
                    _dictRemap = LoadDictCSV(Parent.FS.FldrStaging, _defaults.FileName_DictRemap);
                }
                return _dictRemap;
            }
        }

        public ISerializableList<string> CategoryFilters
        {
            get
            {
                if (_catFilters is null)
                    _catFilters = new SerializableList<string>(filename: _defaults.FileName_CategoryFilters,
                                                                folderpath: _parent.FS.FldrPythonStaging);
                return _catFilters;
            }
            set
            {
                _catFilters = value;
                if (_catFilters.Folderpath == "")
                {
                    _catFilters.Folderpath = _parent.FS.FldrFlow;
                    _catFilters.Filename = _defaults.FileName_Recents;
                }
                _catFilters.Serialize();
            }
        }

        private Dictionary<string, string> LoadDictCSV(string fpath, string filename)
        {
            var dict = CSVDictUtilities.LoadDictCSV(fpath, filename.Split('.')[0] + ".csv");
            if (dict is not null)
                WriteDictJSON(dict, Path.Combine(fpath, filename));
            return dict;
        }

        private Dictionary<string, string> LoadDictJSON(string fpath, string filename)
        {

            string filepath = Path.Combine(fpath, filename);
            Dictionary<string, string> dict = null;
            var response = MsgBoxResult.Ignore;

            try
            {
                dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(Path.Combine(Parent.FS.FldrStaging, DictPPL_Filename)));
            }
            catch (FileNotFoundException ex)
            {
                response = Interaction.MsgBox(filepath + "not found. Load from CSV?", Constants.vbYesNo);
            }
            catch (Exception ex)
            {
                response = Interaction.MsgBox(filepath + "encountered a problem. " + ex.Message + "Load from CSV?", Constants.vbYesNo);
            }
            finally
            {
                if (response == Constants.vbYes)
                {
                    dict = LoadDictCSV(fpath, filename);
                }
                else if (response == Constants.vbNo)
                {
                    response = Interaction.MsgBox("Start a new blank dictionary?", Constants.vbYesNo);
                    if (response == Constants.vbYes)
                    {
                        dict = new Dictionary<string, string>();
                    }
                    else
                    {
                        throw new ArgumentNullException("Cannot proceed without dictionary: " + filename);
                    }
                }
            }
            return dict;
        }

        public void WriteDictJSON(Dictionary<string, string> dict, string filepath)
        {
            File.WriteAllText(filepath, JsonConvert.SerializeObject(dict, Formatting.Indented));
        }
    }
}