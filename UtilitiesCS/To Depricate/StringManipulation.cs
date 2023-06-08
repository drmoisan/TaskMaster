using System;
using System.Reflection;
using UtilitiesCS;

namespace UtilitiesCS
{

    public static class StringManipulation
    {
        public static string GetStrippedText(string strTmp)
        {
            if (NotImplementedDialog.StopAtNotImplemented(MethodBase.GetCurrentMethod().Name))
            {
                throw new NotImplementedException();
            }
            return strTmp;
        }
    }
}