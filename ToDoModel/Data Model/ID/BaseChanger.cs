using System;
using System.Diagnostics;
using System.Numerics;


namespace ToDoModel
{

    public static class BaseChanger
    {
        public static string ConvertToBase(int nbase, BigInteger num, int intMinDigits = 2)
        {
            string ConvertToBaseRet = default;
            string chars;
            int r;
            string newNumber;
            int maxBase;
            int i;

            chars = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyzÀÁÂÃÄÅÆÇÈÉÊËÌÍÎÏÐÑÒÓÔÕÖØÙÚÛÜÝÞßàáâãäåæçèéêëìíîïðñòóôõöøùúûüýþÿŒœŠšŸŽžƒ";
            maxBase = chars.Length;

            // check if we can convert to this base
            if (nbase > maxBase)
            {
                ConvertToBaseRet = "";
            }
            else
            {

                // in r we have the offset of the char that was converted to the new base
                newNumber = "";
                while (num >= nbase)
                {
                    r = (int)(num % nbase);
                    newNumber = chars.Substring((r + 1), 1) + newNumber;
                    num /= nbase;
                }

                newNumber = chars.Substring((int)(num + 1), 1) + newNumber;

                var loopTo = newNumber.Length % intMinDigits;
                for (i = 1; i <= loopTo; i++)
                    newNumber = 0 + newNumber;

                ConvertToBaseRet = newNumber;
            }

            return ConvertToBaseRet;
        }

        public static BigInteger ConvertToDecimal(int nbase, string strBase)
        {
            BigInteger ConvertToDecimalRet = default;
            string chars;
            int i;
            long lngLoc;
            var lngTmp = default(long);
            var bigint = new BigInteger();

            chars = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyzÀÁÂÃÄÅÆÇÈÉÊËÌÍÎÏÐÑÒÓÔÕÖØÙÚÛÜÝÞßàáâãäåæçèéêëìíîïðñòóôõöøùúûüýþÿŒœŠšŸŽžƒ";
            bigint.Equals(0L);

            try
            {
                var loopTo = strBase.Length;
                for (i = 1; i <= loopTo; i++)
                {
                    bigint *= nbase;
                    lngLoc = chars.IndexOf(strBase.Substring(i,1));
                    bigint += lngLoc - 1;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Debug.WriteLine(ex.Source);
                Debug.WriteLine(ex.StackTrace);
                Debug.WriteLine("");

            }

            ConvertToDecimalRet = lngTmp;
            return ConvertToDecimalRet;
        }

    }
}