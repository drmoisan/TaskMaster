using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilitiesCS.Extensions;


namespace UtilitiesCS.HelperClasses
{
    public class ParamArray
    {
        public ParamArray() { }
        public ParamArray(params object[] args) => _args = args;

        private object[] _args; 

        public static bool AnyNull(params object[] args) => args.Any(arg => arg is null);
        public bool AnyNull() => _args.Any(arg => arg is null);

    }

    public class ParamArray<T>
    {
        public ParamArray() { }
        public ParamArray(params T[] args) => _args = args;

        private T[] _args;

        public static bool AnyNull(params T[] args) => args.Any(arg => arg is null);
        public bool AnyNull() => _args.Any(arg => arg is null);

        public static bool AnyNullOrEmpty(params T[] args) => args.IsNullOrEmpty();
        public bool AnyNullOrEmpty() => _args.IsNullOrEmpty();
    }
}
