using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace UtilitiesCS.EmailIntelligence
{
    public static class CommonWords
    {
        public static string StripCommonWords(this string sentence, ISerializableList<string> commonWords)
        {
            return sentence.StripCommonWords((IList<string>)commonWords);
        }
        public static string[] StripCommonWords(this string[] tokens, IList<string> commonWords)
        {
            if (tokens.Length == 0)
            {
                return tokens;
            }
            else
            {
                return (from word in tokens where !commonWords.Contains(word) select word.StripAccents()).ToArray();
            }
        }       
        public static string StripCommonWords(this string sentence, IList<string> commonWords)
        {
            Regex tokenizer = Tokenizer.GetRegex();
            return sentence.StripCommonWords(commonWords, tokenizer);
        }
        public static string StripCommonWords(this string sentence, IList<string> commonWords, Regex tokenizer)
        {
            var tokens = sentence.Tokenize(tokenizer);
            return string.Join(" ", tokens.StripCommonWords(commonWords));
        }

        public static string StripAccents(this string s)
        {
            StringBuilder sb = new StringBuilder(s.Normalize(NormalizationForm.FormKD));
            for (int i = sb.Length -1; i >= 0; i--)
            {
                if (sb[i] > 127)
                {
                    sb.Remove(i, 1);
                } 
            }
            
            return sb.ToString();
        }

        public static string StripAccents2(this string s)
        {
            StringBuilder sb = new StringBuilder();
            var snorm = s.Normalize(NormalizationForm.FormKD);
            foreach (char c in s.Normalize(NormalizationForm.FormKD))
                switch (CharUnicodeInfo.GetUnicodeCategory(c))
                {
                    case UnicodeCategory.NonSpacingMark:
                    case UnicodeCategory.SpacingCombiningMark:
                    case UnicodeCategory.EnclosingMark:
                        break;

                    default:
                        sb.Append(c);
                        break;
                }
            return sb.ToString();
        }

    }
}
