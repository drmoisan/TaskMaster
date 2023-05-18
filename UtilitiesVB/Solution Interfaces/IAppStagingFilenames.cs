
namespace UtilitiesVB
{
    public interface IAppStagingFilenames
    {
        string CommonWords { get; set; }
        string ConditionalReminders { get; set; }
        string CtfInc { get; set; }
        string CtfMap { get; set; }
        string EmailMoves { get; set; }
        string EmailSession { get; set; }
        string EmailSessionTemp { get; set; }
        string RecentsFile { get; set; }
        string SubjectMap { get; set; }
    }
}