using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UtilitiesCS;

namespace ToDoModel
{
    public class SuggestionsLeg2
    {
        #region constructors and private variables

        public SuggestionsLeg2() { }

        private const int maxSuggestions = 5;

        private IList<KeyValuePair<string, int>> _folderNameScores = new List<KeyValuePair<string, int>>(); 
        private static char[] _wordChars = { '&' };
        private Regex _tokenizerRegex = Tokenizer.GetRegex(_wordChars.AsTokenPattern());

        #endregion

        #region public methods
        
        
        
        /// <summary>
        /// 
        /// Add a folderpath to the list of suggestions 
        /// </summary>
        /// <param name="folderPath"></param>
        /// <param name="score"></param>
        /// <param name="max"></param>
        public void Add(string folderPath, int score, int max = maxSuggestions)
        {
            int idx = _folderNameScores.FindIndex(x => x.Key == folderPath);
            if (idx == -1) { AddMissing(folderPath, score, max); }
            else { AddToExisting(score, idx); }
        }

        internal void AddToExisting(int score, int idx)
        {
            _folderNameScores[idx] = new KeyValuePair<string, int>(
                                _folderNameScores[idx].Key, _folderNameScores[idx].Value + score);
            _folderNameScores = _folderNameScores.OrderByDescending(x => x.Value).ToList();
        }

        internal void AddMissing(string folderName, int score, int max)
        {
            if (_folderNameScores.Count < max)
                _folderNameScores.Add(new KeyValuePair<string, int>(folderName, score));
            else
            {
                var min = _folderNameScores.Min(x => x.Value);
                if (score > min)
                {
                    _folderNameScores.Remove(_folderNameScores.First(x => x.Value == min));
                    _folderNameScores.Add(new KeyValuePair<string, int>(folderName, score));

                    // Sort the list by score with the highest score first                        
                    _folderNameScores = _folderNameScores.OrderByDescending(x => x.Value).ToList();
                }
            }
        }

        #endregion

    }
}
