using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using UtilitiesCS.Extensions;

namespace UtilitiesCS.NewtonsoftHelpers
{
    public class DerivedCompositionConverter_ConcurrentDictionary<TDerived, TKey, TValue>
        where TDerived : ConcurrentDictionary<TKey, TValue>
    {
        public ConcurrentDictionary<TKey, TValue> ConcurrentDictionary { get; set; }
        public object RemainingObject { get; set; }
        public Dictionary<string, object> AdditionalFields { get; private set; }
        public Dictionary<string, object> AdditionalProperties { get; private set; }

        public DerivedCompositionConverter_ConcurrentDictionary() { }

        public DerivedCompositionConverter_ConcurrentDictionary(TDerived derivedInstance) =>
            ToCompositionOld(derivedInstance);

        public DerivedCompositionConverter_ConcurrentDictionary<
            TDerived,
            TKey,
            TValue
        > ToCompositionOld(TDerived derivedInstance)
        {
            derivedInstance.ThrowIfNull();
            ConcurrentDictionary = derivedInstance;
            AdditionalFields = [];
            AdditionalProperties = [];

            var derivedType = typeof(TDerived);
            var baseType = typeof(ConcurrentDictionary<TKey, TValue>);

            //var derivedFields = derivedType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            //                               .Where(field => field.DeclaringType != baseType);
            var derivedFields = derivedType
                .GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(field => field.DeclaringType != baseType);

            var derivedProperties = derivedType
                .GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(property => property.DeclaringType != baseType);

            foreach (var field in derivedFields)
            {
                var fieldValue = field.GetValue(derivedInstance);
                AdditionalFields.Add(field.Name, fieldValue);
            }

            foreach (var property in derivedProperties)
            {
                var propertyValue = property.GetValue(derivedInstance);
                AdditionalProperties.Add(property.Name, propertyValue);
            }
            return this;
        }

        public TDerived ToDerivedOld()
        {
            // Create an instance using reflection
            var derivedInstance = (TDerived)Activator.CreateInstance(typeof(TDerived), true);

            // Copy dictionary entries
            foreach (var kvp in ConcurrentDictionary)
            {
                derivedInstance.TryAdd(kvp.Key, kvp.Value);
            }

            // Set additional fields
            var derivedType = typeof(TDerived);
            foreach (var field in AdditionalFields)
            {
                var fieldInfo = derivedType.GetField(
                    field.Key,
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
                );
                if (fieldInfo != null)
                {
                    fieldInfo.SetValue(derivedInstance, field.Value);
                }
            }

            return derivedInstance;
        }

        public Type EmitNewClass()
        {
            var derivedType = typeof(TDerived);
            var baseType = typeof(ConcurrentDictionary<TKey, TValue>);

            var assemblyName = new AssemblyName("DynamicAssembly");
            var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(
                assemblyName,
                AssemblyBuilderAccess.Run
            );
            var moduleBuilder = assemblyBuilder.DefineDynamicModule("MainModule");

            var typeBuilder = moduleBuilder.DefineType(
                $"{derivedType.Name}_WithoutBase",
                TypeAttributes.Public | TypeAttributes.Class
            );

            var derivedFields = derivedType
                .GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(field => field.DeclaringType != baseType);

            var derivedProperties = derivedType
                .GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(property => property.DeclaringType != baseType);

            foreach (var field in derivedFields)
            {
                typeBuilder.DefineField(field.Name, field.FieldType, FieldAttributes.Public);
            }

            foreach (var property in derivedProperties)
            {
                var backingField = typeBuilder.DefineField(
                    $"_{property.Name}",
                    property.PropertyType,
                    FieldAttributes.Private
                );
                var propertyBuilder = typeBuilder.DefineProperty(
                    property.Name,
                    property.Attributes,
                    property.PropertyType,
                    null
                );
                var getMethodBuilder = typeBuilder.DefineMethod(
                    $"get_{property.Name}",
                    MethodAttributes.Public
                        | MethodAttributes.SpecialName
                        | MethodAttributes.HideBySig,
                    property.PropertyType,
                    Type.EmptyTypes
                );
                var getIL = getMethodBuilder.GetILGenerator();
                getIL.Emit(OpCodes.Ldarg_0);
                getIL.Emit(OpCodes.Ldfld, backingField);
                getIL.Emit(OpCodes.Ret);
                propertyBuilder.SetGetMethod(getMethodBuilder);

                if (property.CanWrite)
                {
                    var setMethodBuilder = typeBuilder.DefineMethod(
                        $"set_{property.Name}",
                        MethodAttributes.Public
                            | MethodAttributes.SpecialName
                            | MethodAttributes.HideBySig,
                        null,
                        new Type[] { property.PropertyType }
                    );
                    var setIL = setMethodBuilder.GetILGenerator();
                    setIL.Emit(OpCodes.Ldarg_0);
                    setIL.Emit(OpCodes.Ldarg_1);
                    setIL.Emit(OpCodes.Stfld, backingField);
                    setIL.Emit(OpCodes.Ret);
                    propertyBuilder.SetSetMethod(setMethodBuilder);
                }
            }

            return typeBuilder.CreateTypeInfo().AsType();
        }

        public object ConvertToNewClassInstance(TDerived derivedInstance)
        {
            var newClassType = EmitNewClass();
            var newClassInstance = Activator.CreateInstance(newClassType);

            var derivedType = typeof(TDerived);
            var baseType = typeof(ConcurrentDictionary<TKey, TValue>);

            var derivedFields = derivedType
                .GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(field => field.DeclaringType != baseType);

            var derivedProperties = derivedType
                .GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(property => property.DeclaringType != baseType);

            foreach (var field in derivedFields)
            {
                var fieldValue = field.GetValue(derivedInstance);
                var newField = newClassType.GetField(
                    field.Name,
                    BindingFlags.Instance | BindingFlags.Public
                );
                if (newField != null)
                {
                    newField.SetValue(newClassInstance, fieldValue);
                }
            }

            foreach (var property in derivedProperties)
            {
                var propertyValue = property.GetValue(derivedInstance);
                var newProperty = newClassType.GetProperty(
                    property.Name,
                    BindingFlags.Instance | BindingFlags.Public
                );
                if (newProperty != null && newProperty.CanWrite)
                {
                    newProperty.SetValue(newClassInstance, propertyValue);
                }
            }

            return newClassInstance;
        }

        public DerivedCompositionConverter_ConcurrentDictionary<
            TDerived,
            TKey,
            TValue
        > ToComposition(TDerived derivedInstance)
        {
            derivedInstance.ThrowIfNull();
            ConcurrentDictionary = derivedInstance;
            RemainingObject = ConvertToNewClassInstance(derivedInstance);

            return this;
        }
    }
}
