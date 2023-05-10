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
        public static string StripCommonWords(this string sentence, IList<string> commonWords)
        {
            var sentenceWords = sentence.Tokenize();
            return string.Join(" ", from word in sentenceWords 
                                    where !commonWords.Contains(word) 
                                    select word.StripAccents());
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
