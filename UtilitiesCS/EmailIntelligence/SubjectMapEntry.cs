using System.Linq;
using UtilitiesCS.EmailIntelligence;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Diagnostics;
using Newtonsoft.Json;

namespace UtilitiesCS
{
    /// <summary>
    /// Subject Map Entry holds information regarding email folders and the subject line of 
    /// the emails within the folder. Each entry contains a unique combination of a folder 
    /// name and an email subject. Class is to be used in conjunction with <see cref="SubjectMapEncoder"/> and 
    /// <see cref="SubjectMapSL"/>
    /// </summary>
    public class SubjectMapEntry : ISubjectMapEntry
    {
        public SubjectMapEntry() { _tokenizerRegex = Tokenizer.GetRegex(); }
        public SubjectMapEntry(Regex tokenizerRegex) { _tokenizerRegex = tokenizerRegex; }
        public SubjectMapEntry(string emailFolder, string emailSubject, int emailSubjectCount, IList<string> commonWords, Regex tokenizerRegex)
        {
            _tokenizerRegex = tokenizerRegex;
            Init(emailFolder: emailFolder,
                 emailSubject: emailSubject.StripCommonWords(commonWords),
                 emailSubjectCount: emailSubjectCount,
                 commonWords: commonWords);
        }
        public SubjectMapEntry(string emailFolder, string emailSubject, int emailSubjectCount, IList<string> commonWords)
        {
            _tokenizerRegex = Tokenizer.GetRegex(_wordChars.AsTokenPattern());
            Init(emailFolder: emailFolder,
                 emailSubject: emailSubject.StripCommonWords(commonWords, _tokenizerRegex),
                 emailSubjectCount: emailSubjectCount,
                 commonWords: commonWords);
        }
        public SubjectMapEntry(string emailSubject, int emailSubjectCount, IList<string> commonWords, Regex tokenizerRegex, ISubjectMapEncoder encoder)
        {
            _encoder = encoder;
            _tokenizerRegex = tokenizerRegex;
            Init(emailSubject: emailSubject,
                 emailSubjectCount: emailSubjectCount,
                 commonWords: commonWords);
        }
        public SubjectMapEntry(string emailSubject, int emailSubjectCount, IList<string> commonWords)
        {
            _tokenizerRegex = Tokenizer.GetRegex();
            Init(emailSubject: emailSubject.StripCommonWords(commonWords),
                 emailSubjectCount: emailSubjectCount,
                 commonWords: commonWords);
        }
        public SubjectMapEntry(string emailSubject, int emailSubjectCount, Regex tokenizerRegex)
        {
            _tokenizerRegex = tokenizerRegex;
            Init(emailSubject: emailSubject,
                 emailSubjectCount: emailSubjectCount);
        }
        public SubjectMapEntry(string emailSubject, int emailSubjectCount)
        {
            _tokenizerRegex = Tokenizer.GetRegex();
            Init(emailSubject: emailSubject,
                 emailSubjectCount: emailSubjectCount);
        }

        internal void Init(string emailSubject, int emailSubjectCount, IList<string> commonWords)
        {
            _commonWords = commonWords;
            Init(emailSubject: emailSubject,
                 emailSubjectCount: emailSubjectCount);
        }
        internal void Init(string emailSubject, int emailSubjectCount)
        {
            _subjectTokens = emailSubject.Tokenize(_tokenizerRegex);
            _subjectTokens = _subjectTokens.StripCommonWords(_commonWords);
            if (_subjectTokens.Count() == 0) 
            { 
                throw new System.InvalidOperationException($"{nameof(emailSubject)} {emailSubject} has no valid tokens"); 
            }
            _subjectText = string.Join(" ",_subjectTokens);
            _subjectWordLengths = _subjectTokens.Select(x => x.Length).ToArray();
            _subjectEmailCount = emailSubjectCount;
            if (ReadyToEncode(_subjectTokens, false))
            {
                _subjectEncoded = _encoder.Encode(_subjectTokens);
            }
        }
        internal void Init(string emailFolder, string emailSubject, int emailSubjectCount, IList<string> commonWords)
        {
            _commonWords = commonWords;
            
            Init(emailFolder: emailFolder, 
                 emailSubject: emailSubject, 
                 emailSubjectCount: emailSubjectCount);
        }
        internal void Init(string emailFolder, string emailSubject, int emailSubjectCount)
        {
            _folderPath = emailFolder;
            if (_folderPath is null) { throw new System.ArgumentNullException(emailFolder, $"{nameof(emailFolder)} is null");}
            
            _folderName = emailFolder.Split("\\").Last();
            _folderTokens = _folderName.Tokenize(_tokenizerRegex);
            _folderWordLengths = _folderTokens.Select(x => x.Length).ToArray();

            if (_folderTokens.Count() == 0)
            {
                throw new System.InvalidOperationException($"{nameof(emailFolder)} {emailFolder} has no valid tokens"); 
            }

            _subjectText = emailSubject;
            _subjectTokens = _subjectText.Tokenize(_tokenizerRegex);
            if ((_commonWords is not null)&(_subjectTokens.Length > 0)) 
            { 
                _subjectTokens.StripCommonWords(_commonWords); 
            }
            if (_subjectTokens.Count() == 0)
            {
                throw new System.InvalidOperationException($"{nameof(emailSubject)} {emailSubject} has no valid tokens");
            }
            _subjectWordLengths = _subjectTokens.Select(x => x.Length).ToArray();
            _subjectEmailCount = emailSubjectCount;
        }

        private string _folderPath;
        private string _folderName;
        private string[] _folderTokens;
        private int[] _folderEncoded;
        private int[] _folderWordLengths;
        private string _subjectText;
        private int _subjectEmailCount;
        private string[] _subjectTokens;
        private int[] _subjectEncoded;
        private int[] _subjectWordLengths;  
        private int _score = 0;
        private IList<string> _commonWords;
        private ISubjectMapEncoder _encoder;
        private Regex _tokenizerRegex;
        private char[] _wordChars = { '&' };

        public string Folderpath 
        { 
            get => _folderPath; 
            set 
            { 
                _folderPath = value; 
                _folderName = _folderPath.Split("\\").Last();
                _folderTokens = _folderName.Tokenize(_tokenizerRegex);
                _folderWordLengths = _folderTokens.Select(x => x.Length).ToArray();
                if (ReadyToEncode(_folderTokens, false))
                {
                    _folderEncoded = _encoder.Encode(_folderTokens);
                }
            }
        }
        
        public string Foldername { get => _folderName; }
        
        public string EmailSubject 
        { 
            get => _subjectText;
            set 
            {
                if (value is null)
                {
                    _subjectTokens = new string[] { };
                    _subjectWordLengths = new int[] { };
                    _subjectText = "";
                }
                else
                {
                    _subjectTokens = value.Tokenize(_tokenizerRegex);
                    if (_commonWords is not null) { _subjectTokens = _subjectTokens.StripCommonWords(_commonWords); }

                    _subjectText = string.Join(" ", _subjectTokens);
                    _subjectWordLengths = _subjectTokens.Select(x => x.Length).ToArray();

                    if (ReadyToEncode(_subjectTokens, false))
                    {
                        _subjectEncoded = _encoder.Encode(_subjectTokens);
                    }
                }
            } 
        }
        
        public int EmailSubjectCount { get => _subjectEmailCount; set => _subjectEmailCount = value; }
        
        public int[] FolderWordLengths { get => _folderWordLengths; set => _folderWordLengths = value; }

        public int[] FolderEncoded 
        {
            get
            {
                // Encode folder only if it is null, we have an active encoder, and we are ready to encode
                if (_folderEncoded is null && _encoder is not null && ReadyToEncode(_folderTokens, false))
                {
                    _folderEncoded = _encoder.Encode(_folderTokens);
                }
                return _folderEncoded;
            }
            set
            {
                if (value is null)
                {
                    Debug.WriteLine("SubjectEncoded set to null");
                }
                _folderEncoded = value;
            }
        }

        public int[] SubjectEncoded 
        {
            get 
            { 
                // Encode subject only if it is null, we have an active encoder, and we are ready to encode
                if (_subjectEncoded is null && _encoder is not null && ReadyToEncode(_subjectTokens, false))
                {
                    _subjectEncoded = _encoder.Encode(_subjectTokens);
                }
                return _subjectEncoded; 
            }
            set 
            { 
                if (value is null) 
                { 
                    Debug.WriteLine("SubjectEncoded set to null");
                }
                _subjectEncoded = value; 
            } 
        }
        
        public int[] SubjectWordLengths { get => _subjectWordLengths; set => _subjectWordLengths = value; }

        [JsonIgnore]
        public int Score { get => _score; set => _score = value; }

        [JsonIgnore]
        public ISubjectMapEncoder Encoder 
        { 
            get => _encoder;
            set 
            { 
                _encoder = value;
                if (ReadyToEncode(throwEx: false))
                    this.Encode();
            }
        }

        public bool ReadyToEncode(ISubjectMapEncoder encoder)
        {
            _encoder = encoder;
            return ReadyToEncode(true);
        }

        public bool ReadyToEncode(bool throwEx)
        {
            if (IsNull(_encoder, nameof(_encoder), throwEx)) return false;
            
            string[] tokens = TokensToEncode(throwEx);

            if (IsNull(tokens, nameof(tokens), throwEx) || tokens.Length == 0) return false;
            
            _encoder.AugmentTokenDict(tokens); 
            
            return true;
        }

        public bool ReadyToEncode(string[] tokens, bool throwEx)
        {
            if (IsNull(_encoder, nameof(_encoder), throwEx)) { return false; }

            //if (IsNull(tokens, nameof(tokens), throwEx) || tokens.Length == 0) { return false; }
            if (IsNull(tokens, nameof(tokens), throwEx)) { return false; }

            _encoder.AugmentTokenDict(tokens);

            return true;
        }

        public void SetCommonWords(IList<string> commonWords) => _commonWords = commonWords;

        internal bool IsNull(object value, string name, bool throwEx)
        {
            if (value is null)
            {
                if (!throwEx) { return true; }
                else { throw new System.ArgumentNullException(name, $"{name} is null or has not been initialized"); }
            }
            return false;
        }

        internal string[] TokensToEncode(bool throwEx)
        {
            if ((_folderTokens is null) || (_folderTokens.Length == 0)) 
            {
                if (throwEx)
                {
                    throw new System.ArgumentNullException(
                        nameof(_folderTokens), $"{nameof(_folderTokens)} is null or empty");
                }
                else 
                { 
                    return null; 
                }
            }
            else if ((_subjectTokens is null) || (_subjectTokens.Length == 0)) { return _folderTokens; }
            else { return _folderTokens.Union(_subjectTokens).ToArray(); }
        }

        internal void Encode()
        {
            _folderEncoded = _encoder.Encode(_folderTokens);
            _subjectEncoded = _encoder.Encode(_subjectTokens);

            //if (_folderTokens is null)
            //{
            //    if (_folderName is not null) { _folderTokens = _folderName.Tokenize(_tokenizerRegex); }
            //    else { _folderEncoded = null; }
            //}
            //else if (_folderTokens.Length == 0) { _folderEncoded = new int[] { }; }
            //else { _folderEncoded = _encoder.Encode(_folderTokens); }

            //if (_subjectTokens is null)
            //{
            //    if (_subjectText is not null) { _subjectTokens = _subjectText.Tokenize(_tokenizerRegex); }
            //    else { _subjectEncoded = null; }
            //}
            //else if (_subjectTokens.Length == 0) { _subjectEncoded = new int[] { }; }
            //else { _subjectEncoded = _encoder.Encode(_subjectTokens); }
        }

        public void Encode(ISubjectMapEncoder encoder, Regex tokenizerRegex)
        {
            _tokenizerRegex = tokenizerRegex;
            Init(emailFolder: _folderPath, emailSubject: _subjectText, emailSubjectCount: _subjectEmailCount);
            if (ReadyToEncode(encoder)) { Encode(); }
        }
        
        public void Encode(ISubjectMapEncoder encoder)
        {
            if (ReadyToEncode(encoder)) { Encode(); }
        }

        public int[] Encode(ISubjectMapEncoder encoder, string[] tokens)
        {
            _encoder = encoder;
            if (ReadyToEncode(tokens, true))
            {
                return _encoder.Encode(tokens);
            }
            else 
            { 
                return null; 
            }
        }
    
        internal int[] Encode(ISubjectMapEncoder encoder, string text)
        {
            if (text is null) { return null; }
            else if (_tokenizerRegex is null) { return null; }
            else
            {
                string[] tokens = text.Tokenize(_tokenizerRegex);
                if (ReadyToEncode(tokens, true))
                {
                    return _encoder.Encode(tokens);
                }
                else
                {
                    return null;
                }
            }
        }
    }
}