using System.Linq;
using UtilitiesCS.EmailIntelligence;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Diagnostics;
using Newtonsoft.Json;
using log4net.Repository.Hierarchy;
using System;

namespace UtilitiesCS
{
    /// <summary>
    /// Subject Map Entry holds information regarding email folders and the subject line of 
    /// the emails within the folder. Each entry contains a unique combination of a folder 
    /// name and an email subject. Class is to be used in conjunction with <see cref="SubjectMapEncoder"/> and 
    /// <see cref="SubjectMapSco"/>
    /// </summary>
    public class SubjectMapEntry : ISubjectMapEntry
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #region Constructors and Initializers

        public SubjectMapEntry() { _tokenizerRegex = Tokenizer.GetRegex(_wordChars.AsTokenPattern()); }
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
            _tokenizerRegex = Tokenizer.GetRegex(_wordChars.AsTokenPattern());
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
            _tokenizerRegex = Tokenizer.GetRegex(_wordChars.AsTokenPattern());
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
                try
                {
                    _subjectEncoded = _encoder.Encode(_subjectTokens);
                    if (_subjectEncoded is not null && _subjectEncoded.Length != _subjectWordLengths.Length)
                    {
                        throw new System.InvalidOperationException($"{nameof(_subjectEncoded)} length {_subjectEncoded.Length} does not match {nameof(_subjectWordLengths)} length {_subjectWordLengths.Length}");
                    }
                }
                catch (System.Exception e)
                {
                    throw e;
                }
                

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

        #endregion Constructors and Initializers
                
        #region Public Properties

        [JsonIgnore]
        public IList<string> CommonWords { get => _commonWords; set => _commonWords = value; }
        private IList<string> _commonWords;

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
                    try
                    {
                        _folderEncoded = _encoder.Encode(_folderTokens);
                        if (_folderEncoded.Length != _folderWordLengths.Length)
                        {
                            throw new System.InvalidOperationException($"{nameof(_folderEncoded)} length {_folderEncoded.Length} does not match {nameof(_folderWordLengths)} length {_folderWordLengths.Length}");
                        }
                    }
                    catch (System.Exception e)
                    {
                        throw e;
                    }
                }
            }
        }
        private string _folderPath;
        
        public string Foldername { get => _folderName; }
        private string _folderName;
        
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

                    try
                    {
                        _subjectEncoded = _encoder?.Encode(_subjectTokens);
                        if (_subjectEncoded is not null && _subjectEncoded.Length != _subjectWordLengths.Length)
                        {
                            throw new System.InvalidOperationException($"{nameof(_subjectEncoded)} length {_subjectEncoded.Length} does not match {nameof(_subjectWordLengths)} length {_subjectWordLengths.Length}");
                        }
                    }
                    catch (System.Exception e)
                    {
                        throw e;
                    }
                }
            } 
        }
        private string _subjectText;
        
        public int EmailSubjectCount { get => _subjectEmailCount; set => _subjectEmailCount = value; }
        private int _subjectEmailCount;

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
        private ISubjectMapEncoder _encoder;
        
        public int[] FolderWordLengths { get => _folderWordLengths; set => _folderWordLengths = value; }
        private int[] _folderWordLengths;

        public int[] FolderEncoded 
        {
            get
            {
                // Encode folder only if it is null, we have an active encoder, and we are ready to encode
                if (_folderEncoded is null && _encoder is not null && ReadyToEncode(_folderTokens, false))
                {
                    try
                    {
                        _folderEncoded = _encoder.Encode(_folderTokens);
                        if (_folderEncoded.Length != _folderWordLengths.Length)
                        {
                            throw new System.InvalidOperationException($"{nameof(_folderEncoded)} length {_folderEncoded.Length} does not match {nameof(_folderWordLengths)} length {_folderWordLengths.Length}");
                        }
                    }
                    catch (System.Exception e)
                    {
                        throw e;
                    }
                }
                return _folderEncoded;
            }
            set
            {
                if (value is null)
                {
                    var stack = TraceUtility.GetMyMethods(new StackTrace());
                    //logger.Debug($"SubjectEncoded set to null. See stack:\n" +
                    //    $"{string.Join(" -> ",stack.Select(x => x.Name).ToArray())}");
                }
                _folderEncoded = value;
            }
        }
        private int[] _folderEncoded;

        [JsonIgnore]
        public int Score { get => _score; set => _score = value; }
        private int _score = 0;

        public int[] SubjectEncoded 
        {
            get 
            { 
                // Encode subject only if it is null, we have an active encoder, and we are ready to encode
                if (_subjectEncoded is null && _encoder is not null && ReadyToEncode(_subjectTokens, false))
                {
                    try
                    {
                        _subjectEncoded = _encoder.Encode(_subjectTokens);
                        if (_subjectEncoded is not null && _subjectEncoded.Length != _subjectWordLengths.Length)
                        {
                            throw new System.InvalidOperationException($"{nameof(_subjectEncoded)} length {_subjectEncoded.Length} does not match {nameof(_subjectWordLengths)} length {_subjectWordLengths.Length}");
                        }
                    }
                    catch (System.Exception e)
                    {
                        throw e;
                    }
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
        private int[] _subjectEncoded;
        
        public int[] SubjectWordLengths { get => _subjectWordLengths; set => _subjectWordLengths = value; }
        private int[] _subjectWordLengths;  
                        
        public Regex TokenizerRegex { get => _tokenizerRegex; set => _tokenizerRegex = value; }
        private Regex _tokenizerRegex;

        #endregion Public Properties

        #region Private Properties

        private string[] _folderTokens;
        private string[] _subjectTokens;
        private char[] _wordChars = { '&' };

        #endregion Private Properties

        #region Public and Internal Methods

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

        public bool Equals(ISubjectMapEntry other)
        {
            return this.EmailSubject == other.EmailSubject && 
                this.Folderpath == other.Folderpath;
        }

        public void LogObjectState() => logger.Debug(JsonConvert.SerializeObject(this));
        
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

        public bool TryRepair(bool encode)
        {
            try
            {
                Init(_folderPath, _subjectText, _subjectEmailCount);
            }
            catch (System.Exception e)
            {
                logger.Error(e.Message);
                return false;
            }
            if (!encode)
                return true;

            if (this.ReadyToEncode(false))
            {
                this.Encode();
                return true;
            }
            else { return false; }
        }

        public bool Validate()
        {
            if (_folderTokens.Length != _folderWordLengths.Length ||
                _subjectTokens.Length != _subjectTokens.Length)
            {
                return TryRepair(true);
            }
            else
            {
                return true;
            }
        }

        #endregion Public and Internal Methods

    }
}