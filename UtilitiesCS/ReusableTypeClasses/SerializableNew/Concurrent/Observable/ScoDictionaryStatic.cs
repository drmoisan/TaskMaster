#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilitiesCS.ReusableTypeClasses
{
    public static class ScoDictionaryStatic
    {
        public static bool IsDerivedFrom_ScoDictionaryNew(this Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            Type baseType = typeof(ScoDictionaryNew<,>);
            while (type != null && type != typeof(object))
            {
                if (type.IsGenericType && type.GetGenericTypeDefinition() == baseType)
                {
                    return true;
                }
                type = type.BaseType;
            }
            return false;
        }

        public static Type[] GetScoDictionaryNewGenerics(this Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            Type baseType = typeof(ScoDictionaryNew<,>);
            while (type != null && type != typeof(object))
            {
                if (type.IsGenericType && type.GetGenericTypeDefinition() == baseType)
                {
                    return type.GetGenericArguments();
                }
                type = type.BaseType;
            }
            // why: the Type[] return contract is preserved (return type unchanged) because the sole
            // caller, ScoDictionaryConverter, only invokes this after CanConvert (i.e.
            // IsDerivedFrom_ScoDictionaryNew is true), so this branch is unreachable for valid
            // callers; keeping Type[] (not Type[]?) avoids cascading a nullable into that
            // constraint-only #367 waiver file. The prior null return value is preserved exactly.
            return null!;
        }
    }
}
