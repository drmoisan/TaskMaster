using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilitiesCS
{
    public class SubjectMapSL<SubjectMapEntry> : SerializableList<SubjectMapEntry>
    {
        public SubjectMapSL() : base() { }
        public SubjectMapSL(List<SubjectMapEntry> listOfT) : base(listOfT) { }
        public SubjectMapSL(IEnumerable<SubjectMapEntry> IEnumerableOfT) : base(IEnumerableOfT) { }
        public SubjectMapSL(string filename, string folderpath) : base(filename, folderpath) { }


        public void Add(string subject, string folderName)
        {
            int idx = base.FindIndex(entry => ((entry.EmailSubject == subject) && (entry.EmailFolder == folderName)));

            // If it doesn't exist, add an entry. If it does exist, increase the count
            if (idx == -1)
            {
                SubjectMapEntries.Add(
                    new SubjectMapEntry(emailFolder: folderName, emailSubject: subject, emailSubjectCount: 1));
            }
            else
            {
                SubjectMapEntries[idx].EmailSubjectCount += 1;
            }
        }
    }
}
