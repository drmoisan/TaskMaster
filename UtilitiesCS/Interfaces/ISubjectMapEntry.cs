namespace UtilitiesCS
{
    public interface ISubjectMapEntry
    {
        string EmailFolder { get; set; }
        string EmailSubject { get; set; }
        int EmailSubjectCount { get; set; }
    }
}