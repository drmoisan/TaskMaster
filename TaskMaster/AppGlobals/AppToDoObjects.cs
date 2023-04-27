using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using Microsoft.VisualBasic;
using Newtonsoft.Json;
using ToDoModel;
using UtilitiesCS;
using UtilitiesVB;

namespace TaskMaster
{

    public class AppToDoObjects : IToDoObjects
    {

        private ProjectInfo _projInfo;
        private Dictionary<string, string> _dictPPL;
        private ListOfIDs _IDList;
        private readonly ApplicationGlobals _parent;
        private Dictionary<string, string> _dictRemap;
        private SerializableList<string> _catFilters;

        public AppToDoObjects(ApplicationGlobals ParentInstance)
        {
            _parent = ParentInstance;
        }

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
                return Properties.Settings.Default.FileName_ProjInfo;
            }
        }

        public IProjectInfo ProjInfo
        {
            get
            {
                if (_projInfo is null)
                {
                    _projInfo = ToDoProjectInfoUtilities.LoadToDoProjectInfo(Path.Combine(Parent.FS.FldrAppData, Properties.Settings.Default.FileName_ProjInfo));
                }
                return _projInfo;
            }
        }


        public string DictPPL_Filename
        {
            get
            {
                return Properties.Settings.Default.FilenameDictPpl;
            }
        }

        public Dictionary<string, string> DictPPL
        {
            get
            {
                if (_dictPPL is null)
                {
                    _dictPPL = LoadDictJSON(Parent.FS.FldrStaging, DictPPL_Filename);
                }
                return _dictPPL;
            }
        }

        public void DictPPL_Save()
        {
            File.WriteAllText(Path.Combine(Parent.FS.FldrStaging, DictPPL_Filename), JsonConvert.SerializeObject(_dictPPL, Formatting.Indented));
        }

        public string FnameIDList
        {
            get
            {
                return Properties.Settings.Default.FileName_IDList;
            }
        }

        public IListOfIDs IDList
        {
            get
            {
                if (_IDList is null)
                {
                    _IDList = new ListOfIDs(Path.Combine(Parent.FS.FldrAppData, Properties.Settings.Default.FileName_IDList), _parent.Ol.App);
                }
                return _IDList;
            }
        }

        public string FnameDictRemap
        {
            get
            {
                return Properties.Settings.Default.FileName_DictRemap;
            }
        }

        public Dictionary<string, string> DictRemap
        {
            get
            {
                if (_dictRemap is null)
                {
                    _dictRemap = LoadDictCSV(Parent.FS.FldrStaging, Properties.Settings.Default.FileName_DictRemap);
                }
                return _dictRemap;
            }
        }

        public ISerializableList<string> CategoryFilters
        {
            get
            {
                if (_catFilters is null)
                {
                    var _catFilters = new SerializableList<string>();
                    {
                        ref var withBlock = ref _catFilters;
                        withBlock.Filename = Properties.Settings.Default.FileName_CategoryFilters;
                        withBlock.Folderpath = Parent.FS.FldrAppData;
                        if (File.Exists(withBlock.Folderpath))
                        {
                            withBlock.Deserialize();
                        }
                        else
                        {
                            var tempList = new SerializableList<string>(Load_CCO_Categories.CCOCatList_Load());
                            tempList.Folderpath = withBlock.Folderpath;
                            _catFilters = tempList;
                            withBlock.Serialize();
                        }
                    }
                }
                return _catFilters;
            }
        }

        private Dictionary<string, string> LoadDictCSV(string fpath, string filename)
        {
            var dict = CSVDictUtilities.LoadDictCSV(fpath, filename.Split('.')[0] + ".csv");
            if (dict != null)
                WriteDictJSON(dict, Path.Combine(fpath, filename));
            return dict;
        }

        private Dictionary<string, string> LoadDictJSON(string fpath, string filename)
        {

            string filepath = Path.Combine(fpath, filename);
            Dictionary<string, string> dict = null;
            var response = DialogResult.Ignore;

            try
            {
                dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(Path.Combine(Parent.FS.FldrStaging, DictPPL_Filename)));
            }
            catch (FileNotFoundException ex)
            {
                response = MessageBox.Show($"{filepath} not found. Load from CSV?", "Error", MessageBoxButtons.YesNo);
            }
            catch (Exception ex)
            {
                response = MessageBox.Show($"{filepath} encountered a problem. {ex.Message} Load from CSV?", "Error", MessageBoxButtons.YesNo);
            }
            finally
            {
                if (response == DialogResult.Yes)
                {
                    dict = LoadDictCSV(fpath, filename);
                }
                else if (response == DialogResult.No)
                {
                    response = MessageBox.Show("Start a new blank dictionary?", "Error",MessageBoxButtons.YesNo);
                    if (response == DialogResult.Yes)
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