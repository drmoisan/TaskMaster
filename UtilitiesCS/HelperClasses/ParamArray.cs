#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;
using UtilitiesCS.Extensions;

namespace UtilitiesCS.HelperClasses
{
    public class ParamArray
    {
        public ParamArray() { }

        public ParamArray(params object[] args) => _args = args;

        // Nullable: left null by the parameterless ctor; the instance AnyNull() below
        // assumes the params ctor was used (behavior unchanged).
        private object[]? _args;

        public static bool AnyNull(params object[] args) => args.Any(arg => arg is null);

        // Behavior-preserving: dereferencing a null _args throws (as before) when the
        // parameterless ctor was used without setting args.
        public bool AnyNull() => _args!.Any(arg => arg is null);
    }

    public class ParamArray<T>
    {
        public ParamArray() { }

        public ParamArray(params T[] args) => _args = args;

        // Nullable: left null by the parameterless ctor (behavior unchanged).
        private T[]? _args;

        public static bool AnyNull(params T[] args) => args.Any(arg => arg is null);

        // Behavior-preserving: null _args dereference throws as before.
        public bool AnyNull() => _args!.Any(arg => arg is null);

        public static bool AnyNullOrEmpty(params T[] args) => args.IsNullOrEmpty();

        public bool AnyNullOrEmpty() => _args!.IsNullOrEmpty();
    }
}
