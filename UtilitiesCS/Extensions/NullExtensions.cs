using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace UtilitiesCS.Extensions
{
#nullable enable
    
    public static class NullExtensions
    {
        public static T ThrowIfNull<T>(
            this T? argument,
            string? message = default,
            [CallerMemberName] string callerName = ""
        ) where T : notnull
        {
            if (argument is null)
            {
                var paramName = new StackTrace().GetCallerByName(callerName).GetParameterName(0);
                throw new ArgumentNullException(paramName, message);
            }
            else
            {
                return argument;
            }
        }

        public static bool IsNullOrEmpty<T>(
            this IEnumerable<T> argument)
        {
            if (argument is null || argument.Count() == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static IEnumerable<T> ThrowIfNullOrEmpty<T>(
            this IEnumerable<T> argument,
            string? message = default,
            [CallerMemberName] string callerName = ""
        ) 
        {
            if (argument is null || argument.Count() == 0)
            {
                var paramName = new StackTrace().GetCallerByName(callerName).GetParameterName(0);
                throw new ArgumentNullException(paramName, message);
            }
            else
            {
                return argument;
            }
        }

        public static T ThrowIfNullOrEmpty<T>(
            this T? argument,
            string? message = default,
            [CallerMemberName] string callerName = ""
        ) where T : System.Collections.ICollection
        {
            if (argument is null || argument.Count == 0)
            {
                var paramName = new StackTrace().GetCallerByName(callerName).GetParameterName(0);
                throw new ArgumentNullException(paramName, message);
            }
            else
            {
                return argument;
            }
        }

        public static string ThrowIfNullOrEmpty(
            this string argument,
            string? message = default,
            [CallerMemberName] string callerName = ""
        )
        {
            if (string.IsNullOrEmpty(argument))
            {
                var paramName = new StackTrace().GetCallerByName(callerName).GetParameterName(0);
                throw new ArgumentNullException(paramName, message);
            }
            else
            {
                return argument;
            }
        }




    }
}
