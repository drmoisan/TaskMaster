using System;
using System.Reflection;
using System.Text.RegularExpressions;
using UtilitiesCS;

namespace UtilitiesCS
{

    public static class StringManipulation
    {
        public static string GetStrippedText(string text)
        {
            var regex = new Regex("[^\u0020-\u007D]");
            return regex.Replace(text, "");
            
            //if (NotImplementedDialog.StopAtNotImplemented(MethodBase.GetCurrentMethod().Name))
            //{
            //    throw new NotImplementedException();
            //}
            //return strTmp;
        }
    }
}