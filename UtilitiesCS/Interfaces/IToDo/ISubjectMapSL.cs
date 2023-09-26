using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text.RegularExpressions;

namespace UtilitiesCS
{
    

    public interface ISubjectMapSL: ISerializableList<SubjectMapEntry>
    {
        void Add(string subject, string folderName);
        IList<SubjectMapEntry> Find(string key, Enums.FindBy findBy);
        SubjectMapEntry Find(string subject, string folderName);
        void SetTokenizerRegex(Regex tokenizerRegex);
        void EncodeAll(ISubjectMapEncoder encoder);
        void EncodeAll(ISubjectMapEncoder encoder, Regex tokenizerRegex);
    }
}