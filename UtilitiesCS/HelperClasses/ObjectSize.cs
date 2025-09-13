using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilitiesCS.HelperClasses
{
    public static class ObjectSize
    {
        public static long GetObjectSize(object obj)
        {
            return GetObjectSize(obj, new HashSet<object>());
        }

        private static long GetObjectSize(object obj, HashSet<object> visited)
        {
            if (obj == null)
                return 0;

            long size = 0;

            var type = obj.GetType();

            if (type.IsValueType)
            {
                size += System.Runtime.InteropServices.Marshal.SizeOf(obj);
            }
            else if (visited.Add(obj))
            {
                if (obj is string)
                {
                    size += ((string)obj).Length * sizeof(char);
                }
                else if (obj is System.Collections.ICollection)
                {
                    foreach (var child in (System.Collections.ICollection)obj)
                        size += GetObjectSize(child, visited);
                }
                else
                {
                    foreach (var field in type.GetFields(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic))
                    {
                        var childObj = field.GetValue(obj);
                        size += GetObjectSize(childObj, visited);
                    }
                }
            }

            return size;
        }
    }
}
