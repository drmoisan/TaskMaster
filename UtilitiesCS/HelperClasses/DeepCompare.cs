using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace UtilitiesCS.HelperClasses
{
    public static class Deep
    {
        public static List<(string, object, object)> DeepDifferences<T>(T obj1, T obj2)
        {
            List<(string, object, object)> differences = new List<(string, object, object)>();
            
            var properties = DispatchUtility.GetType(obj1, true).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            //var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var property in properties)
            {
                var value1 = property.GetValue(obj1);
                var value2 = property.GetValue(obj2);
                if (value1 != value2)
                {
                    differences.Add(($"{property.Name}",(object)value1,(object)value2));
                }
            }
            return differences;
        }
    }
}
