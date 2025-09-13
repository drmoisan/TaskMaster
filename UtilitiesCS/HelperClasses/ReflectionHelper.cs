using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace UtilitiesCS.HelperClasses
{
    public static class ReflectionHelper
    {
        public static List<Type> GetAllClassesInSolution()
        {
            List<Type> allClasses = new List<Type>();

            // Get all assemblies loaded in the current AppDomain
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

            foreach (var assembly in assemblies)
            {
                try
                {
                    // Get all types in the assembly
                    Type[] types = assembly.GetTypes();

                    // Filter out only the classes and exclude anonymous and lambda classes
                    var classes = types.Where(t => t.IsClass && IsMyAssembly(t.Assembly) && !IsAnonymousOrLambdaType(t))
                        .Where(t => !t.IsNestedPrivate).ToList();
                    allClasses.AddRange(classes);
                }
                catch (ReflectionTypeLoadException ex)
                {
                    // Handle the exception if some types cannot be loaded
                    var types = ex.Types.Where(t => t != null && t.IsClass && IsMyAssembly(t.Assembly) && !IsAnonymousOrLambdaType(t))
                        .Where(t => !t.IsNestedPrivate).ToList();
                    allClasses.AddRange(types);
                }
            }

            return allClasses;
        }

        private static bool IsMyAssembly(Assembly assembly)
        {
            return TraceUtility.ProjectNames.Contains(assembly.GetName().Name);
        }

        private static bool IsAnonymousOrLambdaType(Type type)
        {
            //return (type.Name.Contains("AnonymousType") || type.Name.Contains("<>")) &&
            return type.IsDefined(typeof(System.Runtime.CompilerServices.CompilerGeneratedAttribute), false);
        }

        public static List<Type> GetAllContainedTypes(object obj)
        {
            var types = new HashSet<Type>();
            if (obj == null) return types.ToList();

            var visited = new HashSet<object>();
            CollectTypes(obj, types, visited);
            return types.ToList();
        }

        private static void CollectTypes(object obj, HashSet<Type> types, HashSet<object> visited)
        {
            if (obj == null || visited.Contains(obj)) return;

            visited.Add(obj);
            var type = obj.GetType();
            types.Add(type);

            // Collect types from properties
            foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                if (property.CanRead)
                {
                    var value = property.GetValue(obj);
                    CollectTypes(value, types, visited);
                }
            }

            // Collect types from fields
            foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                var value = field.GetValue(obj);
                CollectTypes(value, types, visited);
            }

            // If the object is a collection, collect types from its elements
            if (obj is IEnumerable<object> enumerable)
            {
                foreach (var item in enumerable)
                {
                    CollectTypes(item, types, visited);
                }
            }
        }

        public static List<FieldInfo> GetAllFields(this Type type)
        {
            var fields = new List<FieldInfo>();
            while (type != null)
            {
                fields.AddRange(type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.DeclaredOnly));
                type = type.BaseType;
            }
            return fields;
        }

        public static List<FieldInfo> GetAllDerivedFields(this Type type, Type baseType)
        {
            var fields = new List<FieldInfo>();
            while (type != null && type != baseType)
            {
                fields.AddRange(type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.DeclaredOnly));
                type = type.BaseType;
            }
            return fields;
        }

    }
}
