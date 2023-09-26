using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UtilitiesCS.EmailIntelligence;


namespace UtilitiesCS
{
    /// <summary>
    /// A serializable list of ISubjectMapEntry. See <see cref="ISubjectMapEntry"/>.
    /// </summary>
    public class SubjectMapSL : SerializableList<SubjectMapEntry>, ISubjectMapSL
    {
        public SubjectMapSL(ISerializableList<string> commonWords) : base() { _commonWords = commonWords; }

        public SubjectMapSL(List<SubjectMapEntry> listOfT,
                            ISerializableList<string> commonWords) : base(listOfT) { _commonWords = commonWords; }

        public SubjectMapSL(IEnumerable<SubjectMapEntry> IEnumerableOfT,
                            ISerializableList<string> commonWords) : base(IEnumerableOfT) { _commonWords = commonWords; }

        public SubjectMapSL(string filename,
                            string folderpath,
                            ISerializableList<string> commonWords) : base(filename, folderpath) { _commonWords = commonWords; }

        /// <summary>
        /// Constructor that takes the filename and folderpath for the primary file as well as a backup loader and backup location. 
        /// </summary>
        /// <param name="filename">Filename of the primary json serialized object</param>
        /// <param name="folderpath">Location of the serialized object</param>
        /// <param name="backupLoader">Delegate function <see cref="CSVLoader{T}"/> that 
        /// returns an <seealso cref="IList{T}"/> where T : <see cref="ISubjectMapEntry"/></param>
        /// <param name="backupFilepath">Fully qualified filepath to backup file</param>
        /// <param name="askUserOnError">Determines whether to ask the user for direction if initial load fails. If false, 
        /// procedure will automatically use the backup loader if the primary laoder fails</param>
        public SubjectMapSL(string filename,
                            string folderpath,
                            CSVLoader<SubjectMapEntry> backupLoader,
                            string backupFilepath,
                            bool askUserOnError,
                            ISerializableList<string> commonWords) : base(filename,
                                                                          folderpath,
                                                                          backupLoader,
                                                                          backupFilepath,
                                                                          askUserOnError)
        { _commonWords = commonWords; }

        private ISerializableList<string> _commonWords;
        private Regex _tokenizerRegex = Tokenizer.GetRegex();

        public void SetTokenizerRegex(Regex tokenizerRegex) => _tokenizerRegex = tokenizerRegex;

        public void EncodeAll(ISubjectMapEncoder encoder, Regex tokenizerRegex)
        {
            _tokenizerRegex = tokenizerRegex;
            EncodeAll(encoder);
        }

        public void EncodeAll(ISubjectMapEncoder encoder)
        {
            base.ToList().AsParallel().Select(entry => { entry.Encode(encoder, _tokenizerRegex); return entry; });
        }

        /// <summary>
        /// Adds a Subject Map Entry to the list. If it already exists, the count is increased
        /// </summary>
        /// <param name="subject"></param>
        /// <param name="folderName"></param>
        public void Add(string subject, string folderName)
        {
            int idx = base.FindIndex(entry => (entry.EmailSubject == subject) && (entry.Folderpath == folderName));

            // If it doesn't exist, add an entry. If it does exist, increase the count
            if (idx == -1)
            {
                base.Add(new SubjectMapEntry(emailFolder: folderName, 
                                             emailSubject: subject, 
                                             emailSubjectCount: 1, 
                                             commonWords: _commonWords));
            }
            else
            {
                base[idx].EmailSubjectCount += 1;
            }
        }

        /// <summary>
        /// Finds a subject map entry by the subject
        /// </summary>
        /// <param name="subject"></param>
        /// <param name="folderName"></param>
        /// <returns></returns>
        public SubjectMapEntry Find(string subject, string folderName)
        {
            int idx = base.FindIndex(entry => (entry.EmailSubject == subject) && (entry.Folderpath == folderName));
            if (idx != -1) { return base[idx]; }
            return null;
        }

        /// <summary>
        /// Find elements in the list that match the given key. 
        /// </summary>
        /// <param name="key">String to match. For EmailSubject, key is standardized. For Folderpath, key is matched literally</param>
        /// <param name="findBy"><inheritdoc cref="FindBy"/></param>
        /// <returns>List of matching subject map entries</returns>
        public IList<SubjectMapEntry> Find(string key, Enums.FindBy findBy)
        {
            switch (findBy)
            {
                case Enums.FindBy.Subject:
                    key = key.StripCommonWords(_commonWords).ToLower();
                    return base.ToList().Where(entry => entry.EmailSubject == key).ToList();

                default:
                    return base.ToList().Where(entry => entry.Folderpath == key).ToList();
            }
        }
    }
}
