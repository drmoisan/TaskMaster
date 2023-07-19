using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text.RegularExpressions;

namespace UtilitiesCS
{
    

    public interface ISubjectMapSL: ISerializableList<ISubjectMapEntry>
    {
        void Add(string subject, string folderName);
        IList<ISubjectMapEntry> Find(string key, UtilitiesCS.EmailIntelligence.SubjectMap.FindBy findBy);
        ISubjectMapEntry Find(string subject, string folderName);
        void SetTokenizerRegex(Regex tokenizerRegex);
        void EncodeAll(ISubjectMapEncoder encoder);
        void EncodeAll(ISubjectMapEncoder encoder, Regex tokenizerRegex);
    }
}