using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Office.Interop.Outlook;
using UtilitiesCS;

namespace Tags
{
    /// <summary>
    /// Host-neutral selection/search/filter/prefix logic for the tag dialog. Owns the option and
    /// selection dictionaries and the active prefix, and provides the pure decision logic the
    /// controller orchestrates. Contains no <c>System.Windows.Forms</c> references and no live
    /// Outlook COM dependency; the only Outlook Interop reference is the compile-time
    /// <see cref="OlCategoryColor"/> enum constant used to build a default <see cref="IPrefix"/>,
    /// plus the <see cref="IAutoAssign"/>/<see cref="IPrefix"/> interface parameters. All of these
    /// are testable with Moq and pure inputs.
    /// </summary>
    public class TagSelectionModel
    {
        private readonly SortedDictionary<string, bool> _dictOriginal;
        private SortedDictionary<string, bool> _dictOptions;
        private SortedDictionary<string, bool> _filteredOptions;
        private IList<string> _selections;
        private IList<string> _filteredSelections;
        private IPrefix _prefix;
        private readonly IAutoAssign _autoAssigner;

        public TagSelectionModel(
            SortedDictionary<string, bool> dictOptions,
            IAutoAssign autoAssigner,
            IList<string> selections
        )
        {
            _dictOriginal = dictOptions;
            _dictOptions = dictOptions;
            _autoAssigner = autoAssigner;
            _selections = selections;
        }

        public SortedDictionary<string, bool> DictOriginal => _dictOriginal;

        public SortedDictionary<string, bool> DictOptions => _dictOptions;

        public SortedDictionary<string, bool> FilteredOptions
        {
            get => _filteredOptions;
            set => _filteredOptions = value;
        }

        public IList<string> Selections => _selections;

        public IList<string> FilteredSelections => _filteredSelections;

        public IPrefix Prefix
        {
            get => _prefix;
            set => _prefix = value;
        }

        public void SetDictOptions(SortedDictionary<string, bool> dictOptions) =>
            _dictOptions = dictOptions;

        public bool ContainsOption(string choice) => _dictOptions.Keys.Contains(choice);

        public IPrefix GetDefaultPrefix() =>
            new PrefixItem(PrefixTypeEnum.Other, "", "", OlCategoryColor.olCategoryColorNone);

        public void ResolvePrefix(IList<IPrefix> prefixes, string prefixKey)
        {
            // Set default prefix if none exists
            if (prefixes is null || string.IsNullOrEmpty(prefixKey))
            {
                _prefix = GetDefaultPrefix();
            }
            // Else if it exists, set the IPrefix based on the prefixKey
            else if (prefixes.Exists(x => x.Key == prefixKey))
            {
                _prefix = prefixes.Find(x => (x.Key) == prefixKey);
            }
            // Else throw an error
            else
            {
                throw new ArgumentException(
                    nameof(prefixes) + " must contain " + nameof(prefixKey) + " value " + prefixKey
                );
            }
        }

        public bool IsPrefixMissing(IPrefix prefix, string sample)
        {
            bool addPrefix = false;
            int prefixLength = prefix.Value.Length;
            if (prefixLength > 0)
            {
                if ((sample != null) && (sample.Length > prefixLength))
                {
                    if (sample.Substring(0, prefixLength) != prefix.Value)
                    {
                        addPrefix = true;
                    }
                }
                else
                {
                    addPrefix = true;
                }
            }

            return addPrefix;
        }

        public SortedDictionary<string, bool> FilterArchive(
            SortedDictionary<string, bool> sourceDict
        )
        {
            if (_autoAssigner is not null)
            {
                var exclude = _autoAssigner.FilterList;
                var filteredDict = (
                    from x in sourceDict
                    where !exclude.Contains(x.Key, StringComparison.OrdinalIgnoreCase)
                    select x
                ).ToSortedDictionary();
                return filteredDict;
            }
            else
            {
                return sourceDict;
            }
        }

        public void ToggleChoice(string strChoice) =>
            _dictOptions[strChoice] = !_dictOptions[strChoice];

        public void ToggleOn(string strChoice) => _dictOptions[strChoice] = true;

        public void ToggleOff(string strChoice) => _dictOptions[strChoice] = false;

        public void UpdateSelections()
        {
            _selections = _dictOptions.Where(x => x.Value).Select(x => x.Key).ToList();
            _filteredSelections = _filteredOptions.Where(x => x.Value).Select(x => x.Key).ToList();
        }

        public SortedDictionary<string, bool> Search(
            SortedDictionary<string, bool> source,
            List<string> searchStrings
        )
        {
            // If there are no search strings, then the filtered dictionary is the original dictionary
            if (searchStrings.Count == 0)
            {
                return source;
            }

            // Else, filter the original dictionary based on the search strings
            return searchStrings
                .Select(search =>
                    source.Where(x =>
                        x.Key.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0
                    )
                )
                .SelectMany(x => x)
                .Distinct()
                .ToSortedDictionary();
        }

        public List<string> ParseSearchStrings(string searchText)
        {
            searchText = searchText.Trim();
            if (searchText.IsNullOrEmpty())
                return new List<string>();
            return searchText
                .Split(new char[] { '*' }, StringSplitOptions.RemoveEmptyEntries)
                .ToList();
        }

        public string SelectionAsString() => string.Join(", ", SelectionAsList());

        public List<string> SelectionAsList() =>
            _dictOptions.Where(item => item.Value).Select(item => item.Key).ToList();

        public List<string> GetSelections() =>
            (from x in _dictOptions where x.Value == true select x.Key).ToList();

        public void AddOption(string option, bool blClickTrue = false)
        {
            if (!_dictOptions.ContainsKey(option))
            {
                _dictOptions.Add(option, blClickTrue);
            }
            else
            {
                _dictOptions[option] = blClickTrue;
            }

            if (!_dictOptions.Equals(_filteredOptions))
            {
                _filteredOptions ??= new SortedDictionary<string, bool>();
                if (!_filteredOptions.ContainsKey(option))
                {
                    _filteredOptions.Add(option, blClickTrue);
                }
                else
                {
                    _filteredOptions[option] = blClickTrue;
                }
            }
        }

        /// <summary>
        /// Computes the selected-only filtered set used by <c>FilterToSelected</c>: the subset of
        /// options whose value is <c>true</c>. Sets <see cref="FilteredOptions"/> and returns it.
        /// </summary>
        public SortedDictionary<string, bool> FilterToSelectedSet()
        {
            var tmp = (from x in _dictOptions where x.Value select x).ToDictionary(
                x => x.Key,
                x => x.Value
            );
            _filteredOptions = new SortedDictionary<string, bool>(tmp);
            return _filteredOptions;
        }
    }
}
