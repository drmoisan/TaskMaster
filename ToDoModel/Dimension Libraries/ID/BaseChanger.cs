using System;
using System.Diagnostics;
using System.Numerics;
using Microsoft.VisualBasic;

namespace ToDoModel
{

    public static class BaseChanger
    {
        public static string ConvertToBase(int nbase, BigInteger num, int intMinDigits = 2)
        {
            string ConvertToBaseRet = default;
            string chars;
            long r;
            string newNumber;
            int maxBase;
            int i;

            chars = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyzÀÁÂÃÄÅÆÇÈÉÊËÌÍÎÏÐÑÒÓÔÕÖØÙÚÛÜÝÞßàáâãäåæçèéêëìíîïðñòóôõöøùúûüýþÿŒœŠšŸŽžƒ";
            maxBase = Strings.Len(chars);

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
                    r = (long)(num % nbase);
                    newNumber = Strings.Mid(chars, (int)(r + 1L), 1) + newNumber;
                    num /= nbase;
                }

                newNumber = Strings.Mid(chars, (int)(num + 1), 1) + newNumber;

                var loopTo = Strings.Len(newNumber) % intMinDigits;
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
            long i;
            long lngLoc;
            var lngTmp = default(long);
            var bigint = new BigInteger();

            chars = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyzÀÁÂÃÄÅÆÇÈÉÊËÌÍÎÏÐÑÒÓÔÕÖØÙÚÛÜÝÞßàáâãäåæçèéêëìíîïðñòóôõöøùúûüýþÿŒœŠšŸŽžƒ";
            bool unused = bigint.Equals(0L);

            try
            {
                var loopTo = (long)Strings.Len(strBase);
                for (i = 1L; i <= loopTo; i++)
                {
                    bigint *= nbase;
                    lngLoc = Strings.InStr(chars, Strings.Mid(strBase, (int)i, 1));
                    bigint += lngLoc - 1L;
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