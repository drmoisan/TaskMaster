using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

[assembly: InternalsVisibleTo("UtilitiesCS.Test")]
namespace UtilitiesCS
{
    public static class Tokenizer
    {
        /// <summary>
        /// Converts a string into array of tokens. 
        /// Words must be at least 2 characters to be a token
        /// </summary>
        /// <param name="doc">text to be tokenized</param>
        /// <returns>array of word tokens</returns>
        public static string[] Tokenize(this string doc)
        {
            Regex regex = new Regex(@"\b\w\w+\b");
            return doc.Tokenize(regex);
        }

        /// <summary>
        /// Converts a string into array of tokens. 
        /// Minimum token size passed as parameter
        /// </summary>
        /// <param name="doc">text to be tokenized</param>
        /// <param name="min">minimum token size in characters</param>
        /// <returns>array of word tokens</returns>
        public static string[] Tokenize(this string doc, int min) 
        { 
            return doc.Tokenize(GetRegex(new char[] { }.AsTokenPattern(min))); 
        }

        /// <summary>
        /// Converts a string into array of tokens. 
        /// Minimum token size passed as parameter
        /// </summary>
        /// <param name="doc">text to be tokenized</param>
        /// <param name="chars">array of whitespace characters to 
        /// be interpreted as string literals</param>
        /// <returns>array of word tokens</returns>
        public static string[] Tokenize(this string doc, char[] chars) 
        { 
            return doc.Tokenize(GetRegex(chars.AsTokenPattern())); 
        }

        /// <summary>
        /// Converts a string into array of tokens. 
        /// Minimum token size passed as parameter
        /// </summary>
        /// <param name="doc">text to be tokenized</param>
        /// <param name="regex">Regex object with preloaded pattern</param>
        /// <returns>array of word tokens</returns>
        public static string[] Tokenize(this string doc, Regex regex)
        {
            if (doc.IsNullOrEmpty() || regex == null) { return []; }
            return regex.Matches(doc)
                        .Cast<Match>()
                        .Select(x => x.Value
                        .ToLower())
                        .ToArray();
        }
        
        /// <summary>
        /// Create a regex for tokenization with expanded definition of a word
        /// </summary>
        /// <param name="chars">Array of whitespace characters to be interpreted as string literals</param>
        /// <param name="minCharsPerWord">Minimum token size in characters</param>
        /// <returns>tokenPattern for regex</returns>
        public static string AsTokenPattern(this char[] chars, int minCharsPerWord = 2)
        {
            string wordPattern = chars.AsRegexWord();
            return GetTokenPattern(wordPattern, minCharsPerWord);  
        }

        public static Regex GetRegex(string tokenPattern) => new Regex(tokenPattern);
        public static Regex GetRegex() => new Regex(@"\b\w\w+\b");

        /// <summary>
        /// Defines regex pattern for tokenizer based on expanded definition of a 
        /// word character and a minimum token size
        /// </summary>
        /// <param name="wordPattern">Expanded Regex pattern to define a word character</param>
        /// <param name="minCharsPerWord">Minimum token size in characters</param>
        /// <returns>Regex pattern for tokenizer</returns>
        internal static string GetTokenPattern(string wordPattern, int minCharsPerWord)
        {
            string wordInsertion = wordPattern;
            for (int i = 2; i <= minCharsPerWord; i++)
            {
                wordInsertion += wordPattern;
            }
            return @"\b" + wordInsertion + @"+\b";
        }

        /// <summary>
        /// Expands the definition of a regex word character to 
        /// include the whitespace characters in the parameter
        /// </summary>
        /// <param name="chars">array of whitespace characters to 
        /// be interpreted as string literals</param>
        /// <returns>Expanded Regex pattern for word</returns>
        public static string AsRegexWord(this char[] chars)
        {
            if ((chars is null)||(chars.Length == 0)) { return @"\w"; }
            else { return @"[\w" + new string(chars) + @"]"; }
        }
    }
}
