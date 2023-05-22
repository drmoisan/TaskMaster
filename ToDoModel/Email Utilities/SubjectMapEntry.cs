using UtilitiesCS;

namespace ToDoModel
{
    public class SubjectMapEntry : ISubjectMapEntry
    {
        public SubjectMapEntry() { }
        public SubjectMapEntry(string emailFolder, string emailSubject, int emailSubjectCount)
        {
            EmailFolder = emailFolder;
            EmailSubject = emailSubject;
            EmailSubjectCount = emailSubjectCount;
        }

        private string _emailFolder;
        private string _emailSubject;
        private int _emailSubjectCount;

        public string EmailFolder { get => _emailFolder; set => _emailFolder = value; }
        public string EmailSubject { get => _emailSubject; set => _emailSubject = value; }
        public int EmailSubjectCount { get => _emailSubjectCount; set => _emailSubjectCount = value; }
    }
}