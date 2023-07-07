using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using UtilitiesCS.ReusableTypeClasses;

namespace UtilitiesCS
{
    public class SubjectMapEncoder : ISubjectMapEncoder
    {
        //TODO: Make this class inherit from Serializable Dictionary
        public SubjectMapEncoder() { }

        public SubjectMapEncoder(string filename, string folderpath, ISubjectMapSL subjectMap)
        {
            _filename = filename;
            _folderpath = folderpath;
            _subjectMap = subjectMap;
            _encoder = new SCODictionary<string, int>(filename, folderpath);
        }

        private string _filename;
        private string _folderpath;
        private ISCODictionary<string, int> _encoder;
        private ISCODictionary<int, string> _decoder;
        private ISubjectMapSL _subjectMap;
        private Regex _tokenizerRegex = Tokenizer.GetRegex(new char[] { '&' }.AsTokenPattern());

        public ISCODictionary<int, string> Decoder 
        { 
            get 
            { 
                if (_decoder is null)
                {
                    if (_encoder is null)
                    {
                        _encoder.Deserialize();
                    }
                    // _decoder = new SCODictionary<int, string>(_encoder.ToDictionary().Select(x => new KeyValuePair<int, string>(x.Value, x.Key)).ToDictionary());
                    var iEnumerableOfKVPs = _encoder.Select(x => new KeyValuePair<int, string>(x.Value, x.Key));
                    try
                    {
                        _decoder = new SCODictionary<int, string>(iEnumerableOfKVPs.ToDictionary());
                    }
                    catch (InvalidOperationException)
                    {
                        if (iEnumerableOfKVPs.GroupBy(kvp => kvp.Key).Where(g => g.Count() > 1).Any())
                        {
                            var response = MessageBox.Show("Encoder is corrupt. "+
                                "Duplicate keys found in decoder. "+
                                "Would you like to rebuild the encoder / decoder?", 
                                "Duplicate Keys", MessageBoxButtons.YesNo);
                            
                            if (response == DialogResult.Yes) { RebuildEncoding(); }
                            else { throw; }
                        }
                        else { throw; }
                    }
                }
                return _decoder; 
            } 
        }
        public ISCODictionary<string, int> Encoder
        {
            get
            {
                if (_encoder is null)
                    _encoder = new SCODictionary<string, int>(filename: _filename,
                                                              folderpath: _folderpath);
                return _encoder;
            }
        }

        public void RebuildEncoding()
        {
            if(_subjectMap is null) { throw new NullReferenceException(
                $"{nameof(_subjectMap)} is null within class {nameof(SubjectMapEncoder)}"); }
            RebuildEncoding(_subjectMap);
        }

        public void RebuildEncoding(ISubjectMapSL map)
        {
            var words = map.ToList()
                           .Select(x => string.Concat(x.EmailSubject,
                                                      " ",
                                                      x.Folderpath.Split("\\").Last())
                           .Tokenize(_tokenizerRegex))
                           .SelectMany(x => x)
                           .Distinct()
                           .Select((input, index) => new { input, index })
                           .ToDictionary(x => x.input, x => x.index);

            _encoder = new SCODictionary<string, int>(dictionary: words,
                                                      filename: _filename,
                                                      folderpath: _folderpath);

            _encoder.Serialize();
            _decoder = new SCODictionary<int, string>(
                _encoder.ToDictionary()
                .Select(x => new KeyValuePair<int, string>(x.Value, x.Key))
                .ToDictionary());

            foreach (var entry in map)
            {
                entry.Encode(this, _tokenizerRegex);
            }
            map.Serialize();
        }

        public void AugmentTokenDict(string[] tokens)
        {
            bool changed = false;
            if (tokens is null) { throw new ArgumentNullException(nameof(tokens));}
            foreach (var token in tokens)
            {
                if (!Encoder.ContainsKey(token))
                {
                    bool tryAgain = true;
                    int code = -1;
                    while (tryAgain)
                    {
                        code = Encoder.Values.Max() + 1;
                        if (Decoder.TryAdd(code, token)) { tryAgain = false; }
                    }
                    Encoder.Add(token, code);
                    changed = true;
                }
            }
            if (changed) { _encoder.Serialize(); }
        }
        
        public void AugmentTokenDict(string text)
        {
            AugmentTokenDict(text.Tokenize().Distinct().ToArray());
        }

        public int[] Encode(string[] words)
        {
            return words.Select(x => Encoder[x]).ToArray();
        }

        public int[] Encode(string text)
        {
            return text.Tokenize().Select(x => _encoder[x]).ToArray();
        }

        public string Decode(int[] encodedWords)
        {
            return string.Join(" ", encodedWords.Select(value => Decoder[value]).ToArray());
        }


    }
}
