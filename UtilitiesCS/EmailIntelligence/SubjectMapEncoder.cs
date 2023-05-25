using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UtilitiesCS.ReusableTypeClasses;

namespace UtilitiesCS
{
    public class SubjectMapEncoder : ISubjectMapEncoder
    {
        //TODO: Make this class inherit from Serializable Dictionary
        public SubjectMapEncoder() { }

        public SubjectMapEncoder(string filename, string folderpath)
        {
            _filename = filename;
            _folderpath = folderpath;
            _encoder = new SerializableDictionary<string, int>(filename, folderpath);
        }

        private string _filename;
        private string _folderpath;
        private ISerializableDictionary<string, int> _encoder;
        private Dictionary<int, string> _decoder;
        private Regex _tokenizerRegex = Tokenizer.GetRegex(new char[] { '&' }.AsTokenPattern());

        public Dictionary<int, string> Decoder 
        { 
            get 
            { 
                if (_decoder is null)
                {
                    if (_encoder is null)
                    {
                        _encoder.Deserialize();
                    }
                    _decoder = _encoder.ToDictionary().Select(x => new KeyValuePair<int, string>(x.Value, x.Key)).ToDictionary();
                }
                return _decoder; 
            } 
        }
        public ISerializableDictionary<string, int> Encoder
        {
            get
            {
                if (_encoder is null)
                    _encoder = new SerializableDictionary<string, int>(filename: _filename,
                                                                               folderpath: _folderpath);
                return _encoder;
            }
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

            _encoder = new SerializableDictionary<string, int>(dictionary: words,
                                                               filename: _filename,
                                                               folderpath: _folderpath);

            _encoder.Serialize();
            _decoder = _encoder.ToDictionary().Select(x => new KeyValuePair<int, string>(x.Value, x.Key)).ToDictionary();

            foreach (var entry in map)
            {
                entry.Encode(this, _tokenizerRegex);
            }
            map.Serialize();
        }

        public void AugmentTokenDict(string[] tokens)
        {
            bool changed = false;
            foreach (var token in tokens)
            {
                if (!Encoder.ContainsKey(token))
                {
                    int code = Encoder.Values.Max() + 1;
                    Decoder.Add(code, token);
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
