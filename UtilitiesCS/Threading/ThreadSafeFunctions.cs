using Deedle.Internal;
using Microsoft.Office.Interop.Outlook;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace UtilitiesCS.Threading
{
    public static class ThreadSafeFunctions
    {
        public static void AddThreadSafe(this ref double source, double amount, int maxAttempts = 100)
        {
            if (maxAttempts < 1)
                throw new ArgumentException("maxAttempts must be greater than 0");
            int attempts = 0;
            double exchangeValue = 0;
            double startingValue = -1;
            while (startingValue != exchangeValue)
            {
                if (++attempts > maxAttempts)
                    throw new InvalidOperationException($"Attempted to add {attempts-1} times without success");
                startingValue = source;
                var temp = startingValue + amount;
                exchangeValue = Interlocked.CompareExchange(ref source, temp, startingValue);
            }
        }

        internal static void AddThreadSafe(this ref double source, double amount, double limit, int maxAttempts)
        {
            if (maxAttempts < 1)
                throw new ArgumentException("maxAttempts must be greater than 0");
            int attempts = 0;
            double exchangeValue = 0;
            double startingValue = -1;
            while (startingValue != exchangeValue)
            {
                if (++attempts > maxAttempts)
                    throw new InvalidOperationException($"Attempted to add {attempts - 1} times without success");
                startingValue = source;
                var temp = startingValue + amount;
                if (temp > limit) { exchangeValue = Interlocked.CompareExchange(ref source, limit, startingValue); }
                else { exchangeValue = Interlocked.CompareExchange(ref source, temp, startingValue); }
            }
        }

        internal static void SubtractThreadSafe(this ref double source, double amount, double limit, int maxAttempts)
        {
            if (maxAttempts < 1)
                throw new ArgumentException("maxAttempts must be greater than 0");
            int attempts = 0;
            double exchangeValue = 0;
            double startingValue = -1;
            while (startingValue != exchangeValue)
            {
                if (++attempts > maxAttempts)
                    throw new InvalidOperationException($"Attempted to add {attempts - 1} times without success");
                startingValue = source;
                var temp = startingValue - amount;
                if (temp < limit) { exchangeValue = Interlocked.CompareExchange(ref source, limit, startingValue); }
                else { exchangeValue = Interlocked.CompareExchange(ref source, temp, startingValue); }
            }
        }

        public static void IncrementThreadSafe(this ref double source)
        {
            source.AddThreadSafe(1);
        }

        public static void IncrementThreadSafe(this ref double source, double maxValue)
        {
            source.AddThreadSafe(1, maxValue, 100);
        }

        public static void IncrementThreadSafe(this ref double source, double maxValue, int maxAttempts)
        {
            source.AddThreadSafe(1, maxValue, maxAttempts);
        }

        public static void DecrementThreadSafe(this ref double source)
        {
            source.AddThreadSafe(-1);
        }

        public static void DecrementThreadSafe(this ref double source, double minValue)
        {
            source.SubtractThreadSafe(1,minValue,100);
        }

        public static void DecrementThreadSafe(this ref double source, double minValue, int maxAttempts)
        {
            source.SubtractThreadSafe(1, minValue, maxAttempts);
        }

        public static void AdjustThreadSafe(this ref double source, Func<double, double> adjustmentFactory, Func<double, double> limitFactory)
        {
            source.AdjustThreadSafe(adjustmentFactory, limitFactory, 100);
        }

        public static void AdjustThreadSafe(this ref double source, Func<double,double> adjustmentFactory, Func<double,double> limitFactory, int maxAttempts) 
        {
            if (maxAttempts < 1)
                throw new ArgumentException("maxAttempts must be greater than 0");
            
            int attempts = 0;
            double exchangeValue = 0;
            double startingValue = -1;
            
            while (startingValue != exchangeValue)
            {
                if (++attempts > maxAttempts)
                    throw new InvalidOperationException($"Attempted to add {attempts - 1} times without success");
                startingValue = source;
                var temp = limitFactory(adjustmentFactory(startingValue));                
                exchangeValue = Interlocked.CompareExchange(ref source, temp, startingValue); 
            }
        }

    }
}
