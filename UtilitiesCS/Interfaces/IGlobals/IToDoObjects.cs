using System.Collections.Generic;
using UtilitiesCS;
using UtilitiesCS.ReusableTypeClasses;

namespace UtilitiesCS
{

    public interface IToDoObjects
    {
        IPeopleScoDictionary DictPPL { get; }
        //Dictionary<string, string> DictPPL { get; }
        //string DictPPL_Filename { get; }
        //void DictPPL_Save();
        IScoDictionary<string, string> DictRemap { get; }
        ISerializableList<string> CategoryFilters { get; }
        IIDList IDList { get; }
        IApplicationGlobals Parent { get; }
        IProjectInfo ProjInfo { get; }
        ScoCollection<IPrefix> PrefixList { get; }
        ScoCollection<IPrefix> LoadPrefixList();
        ScoDictionary<string, int> FilteredFolderScraping { get; }
        ScoDictionary<string, string> FolderRemap { get; }
        string ProjInfo_Filename { get; }
        string FnameDictRemap { get; }
        string FnameIDList { get; }

    }
}