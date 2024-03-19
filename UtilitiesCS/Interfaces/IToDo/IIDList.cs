using Microsoft.Office.Interop.Outlook;

namespace UtilitiesCS
{
    public interface IIDList: ISerializableList<string>
    {
        int MaxLengthOfID { get; }
        void CompressToDoIDs(IApplicationGlobals appGlobals);
        string GetNextToDoID();
        string GetNextToDoID(string strSeed);
        void RefreshIDList();
        void RefreshIDList(Application Application);
        void SetOlApp(Application olApp);
        void SubstituteIdRoot(string oldPrefix, string newPrefix);
    }
}