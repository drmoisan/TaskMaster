using System.Collections.Generic;
using UtilitiesCS;

namespace UtilitiesCS
{

    public interface IToDoObjects
    {
        Dictionary<string, string> DictPPL { get; }
        string DictPPL_Filename { get; }
        void DictPPL_Save();
        Dictionary<string, string> DictRemap { get; }
        ISerializableList<string> CategoryFilters { get; }
        IListOfIDsLegacy IDList { get; }
        IApplicationGlobals Parent { get; }
        IProjectInfo ProjInfo { get; }
        string ProjInfo_Filename { get; }
        string FnameDictRemap { get; }
        string FnameIDList { get; }

    }
}