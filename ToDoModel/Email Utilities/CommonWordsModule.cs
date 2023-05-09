using System.Collections.Generic;
using System.Text;

namespace ToDoModel
{

    public static class CommonWordsModule
    {
        public static string StripCommonWords(string seedString, IList<string> commonWords)
        {
            var input = new StringBuilder(seedString);
            foreach (string word in commonWords)
                input.Replace(word, "");
            return input.ToString();
        }


    }
}