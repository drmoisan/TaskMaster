using Microsoft.Office.Interop.Outlook;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace UtilitiesCS
{
    public interface IIDList: ISerializableList<string>
    {
        int MaxLengthOfID { get; }
        void CompressToDoIDs(IApplicationGlobals appGlobals);
        string GetNextToDoID();
        string GetNextToDoID(string strSeed);
        IAsyncEnumerable<IToDoItem> GetItemsWithRootIdAsync(string rootId);
        void RefreshIDList();
        void RefreshIDList(Application Application);
        void SetOlApp(Application olApp);
        void SubstituteIdRoot(string oldPrefix, string newPrefix);
        Task<string> SubstituteIdRootAsync(string oldId, string newRoot, string oldRoot);
    }
}