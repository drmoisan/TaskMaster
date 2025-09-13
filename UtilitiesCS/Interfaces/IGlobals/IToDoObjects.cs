using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UtilitiesCS;
using UtilitiesCS.Interfaces;
using UtilitiesCS.ReusableTypeClasses;

namespace UtilitiesCS
{

    public interface IToDoObjects
    {
        //IPeopleScoDictionary DictPPL { get; }
        IPeopleScoDictionaryNew People { get; }
        //Dictionary<string, string> DictPPL { get; }
        //string DictPPL_Filename { get; }
        //void DictPPL_Save();
        Task LoadAsync(bool parallel);
        IScoDictionary<string, string> DictRemap { get; }
        ISerializableList<string> CategoryFilters { get; }
        IIDList IDList { get; }
        IApplicationGlobals Parent { get; }
        IProjectData ProjInfo { get; }
        ScDictionary<string, string> ProgramInfo { get; }
        ScoCollection<IPrefix> PrefixList { get; }
        ScoCollection<IPrefix> LoadPrefixList();
        ScoDictionary<string, int> FilteredFolderScraping { get; }
        ScoDictionary<string, string> FolderRemap { get; }
        string ProjInfo_Filename { get; }
        string FnameDictRemap { get; }
        string FnameIDList { get; }
        Func<IEnumerable<string>, IPrefix, string, string, string> FindMatchingTag { get; }
        public Func<IEnumerable<string>, List<string>> SelectFromList { get; }
        IFlagChangeTrainingQueue FlagChangeTrainingQueue { get; }

    }
}