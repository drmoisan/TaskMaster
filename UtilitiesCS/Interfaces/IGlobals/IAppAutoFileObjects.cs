using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using UtilitiesCS;
using UtilitiesCS.EmailIntelligence.Bayesian;
using UtilitiesCS.ReusableTypeClasses;
using UtilitiesCS.ReusableTypeClasses.SerializableNew.Concurrent.Observable;

namespace UtilitiesCS
{

    public interface IAppAutoFileObjects
    {
        Task LoadAsync(bool parallel);
        CtfMap LoadCtfMap();
        int Conversation_Weight { get; set; }
        int LngConvCtPwr { get; set; }
        int MaxRecents { get; set; }
        //RecentsList<string> RecentsList { get; set; }
        SloLinkedList<string> RecentsList { get;}
        CtfMap CtfMap { get; set; }
        ISerializableList<string> CommonWords { get; set; }
        bool SuggestionFilesLoaded { get; set; }
        int SmithWatterman_MatchScore { get; set; }
        int SmithWatterman_MismatchScore { get; set; }
        int SmithWatterman_GapPenalty { get; set; }
        public SubjectMapSco SubjectMap { get;  }
        ISubjectMapEncoder Encoder { get; }
        System.Action MaximizeQuickFileWindow { get; set; }
        ScoStack<IMovedMailInfo> MovedMails { get; }
        ScoCollection<FilterEntry> Filters { get; }

        //AsyncLazy<ConcurrentDictionary<string, NewSmartSerializableLoader>> ManagerConfiguration { get; }
        //ConcurrentDictionary<string, AsyncLazy<BayesianClassifierGroup>> Manager { get; }
        ManagerAsyncLazy Manager { get; }

        //[Obsolete]
        //ScDictionary<string, BayesianClassifierGroup> Manager { get; }
        ProgressTrackerPane ProgressTracker { get; }
        Microsoft.Office.Tools.CustomTaskPane ProgressPane { get; }
        CancellationToken CancelToken { get; }
        CancellationTokenSource CancelSource { get; }
    }
}