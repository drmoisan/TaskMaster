using System;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;


namespace ToDoModel
{
    public static class BaseChanger
    {
        public const string ConverterString = "0123456789abcdefghijklmnopqrstuvwxyz";
        private static int _maxBase = ConverterString.Length;
        public static int MaxBase { get => _maxBase; }   

        internal static void ValidateParams(int nbase)
        {
            if (nbase < 1)
            {
                throw new ArgumentOutOfRangeException(
                    $"Cannot convert base {nbase} because it must be a positive number");
            }
            if (nbase > MaxBase)
            {
                throw new ArgumentOutOfRangeException(
                    $"Cannot convert base {nbase} because {nameof(ConverterString)} " +
                    $"only supports conversions up to base {MaxBase}");
            }
        }
        
        internal static void ValidateParams(int nbase, BigInteger num)
        {
            if (num < 0)
            {
                throw new ArgumentOutOfRangeException(
                    $"Cannot convert number {num} because it must be 0 or greater");
            }
            ValidateParams(nbase);
        }

        public static string ToBase(this BigInteger num, int nbase, int intMinDigits = 2)
        {
            BigInteger r;
            StringBuilder baseBuilder = new StringBuilder(10);
            
            ValidateParams(nbase, num);

            // in r we have the offset of the char that was converted to the new base
            while (num >= nbase)
            {
                r = num % nbase;
                baseBuilder.Append(ConverterString[(int)r]);
                num /= nbase;
            }

            baseBuilder.Append(ConverterString[(int)num]);

            var prependCount = baseBuilder.Length % intMinDigits;
            switch (prependCount)
            {
                case 1:
                    baseBuilder.Insert(0, "0");
                    return baseBuilder.ToString();
                case 2:
                    baseBuilder.Insert(0, "00");
                    return baseBuilder.ToString();
                default: 
                    return baseBuilder.ToString();
            }
        }
                
        public static int ToBase10(this char c, int nbase)
        {
            ValidateParams(nbase);

            int idx = ConverterString.IndexOf(c);
            if (idx == -1)
            {
                throw new ArgumentOutOfRangeException(nameof(c), $"Character {c} is not part of this " +
                $"implementation of base {nbase}");
            }
            return idx;
        }
                
        public static BigInteger ToBase10(this string strBase, int nbase)
        {
            string converter;
            var bigint = new BigInteger();

            converter = ConverterString;
            bigint.Equals(0L);
                        
            var loopTo = strBase.Length;
            for (int i = 0; i < loopTo; i++)
            {
                bigint *= nbase;
                int idx = converter.IndexOf(strBase[i]);
                bigint += idx;
            }

            return bigint;  
        }

    }
}