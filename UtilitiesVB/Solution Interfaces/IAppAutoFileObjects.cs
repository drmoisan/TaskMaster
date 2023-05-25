using UtilitiesCS;
using UtilitiesCS.ReusableTypeClasses;

namespace UtilitiesVB
{

    public interface IAppAutoFileObjects
    {
        int Conversation_Weight { get; set; }
        int LngConvCtPwr { get; set; }
        int MaxRecents { get; set; }
        IRecentsList<string> RecentsList { get; set; }
        CtfIncidenceList CTFList { get; set; }
        ISerializableList<string> CommonWords { get; set; }
        bool SuggestionFilesLoaded { get; set; }
        int SmithWatterman_MatchScore { get; set; }
        int SmithWatterman_MismatchScore { get; set; }
        int SmithWatterman_GapPenalty { get; set; }
        public ISubjectMapSL SubjectMap { get;  }
        ISubjectMapEncoder Encoder { get; }
    }
}