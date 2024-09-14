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
    public class WrapperScDictionary<TDerived, TKey, TValue> where TDerived : ConcurrentDictionary<TKey, TValue>
    {
        public ConcurrentDictionary<TKey, TValue> ConcurrentDictionary { get; set; }
        public object RemainingObject { get; set; }

        public WrapperScDictionary() 
        {
            ConcurrentDictionary = new ConcurrentDictionary<TKey, TValue>();
        }

        public TDerived ToDerived(WrapperScDictionary<TDerived, TKey, TValue> wrapper)
        {
            ConcurrentDictionary = wrapper.ConcurrentDictionary;
            RemainingObject = wrapper.RemainingObject;
            return ToDerived();
        }

        public TDerived ToDerived()
        {            
            ConcurrentDictionary.ThrowIfNull();
            RemainingObject.ThrowIfNull();

            // Create an instance using reflection
            var derivedInstance = (TDerived)Activator.CreateInstance(typeof(TDerived), true);

            // Copy dictionary entries
            foreach (var kvp in ConcurrentDictionary)
            {
                derivedInstance.TryAdd(kvp.Key, kvp.Value);
            }

            // Set additional fields
            var derivedType = typeof(TDerived);

            var additionalFields = RemainingObject.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).ToArray();
            
            foreach (var field in additionalFields)
            {
                var fieldInfo = derivedType.GetField(field.Name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                
                if (fieldInfo != null)
                {
                    fieldInfo.SetValue(derivedInstance, field.GetValue(RemainingObject));
                }
            }

            return derivedInstance;
        }

        public WrapperScDictionary<TDerived, TKey, TValue> ToComposition(TDerived derivedInstance)
        {
            derivedInstance.ThrowIfNull();
            ConcurrentDictionary = derivedInstance;

            Type objectType = CompileType();
            RemainingObject = CopyTo(derivedInstance, objectType);

            return this;
        }

        public Type CompileType()
        {
            var derivedType = typeof(TDerived);
            var baseType = typeof(ConcurrentDictionary<TKey, TValue>);

            var derivedProperties = derivedType
                .GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(property => property.DeclaringType != baseType)
                .ToArray();

            TypeBuilder tb = GetTypeBuilder();
            ConstructorBuilder constructor = tb.DefineDefaultConstructor(
                MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName);

            var capturedNames = new List<string>();
            foreach (var property in derivedProperties)
            {
                FieldInfo existingField = GetBackingField(property);
                ReplicateProperty(tb, property, existingField);
                capturedNames.Add(existingField.Name);
            }

            var derivedFields = derivedType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                                           .Where(field => field.DeclaringType != baseType);

            var fieldsToCreate = derivedFields.Where(field => !capturedNames.Contains(field.Name)).ToArray();
            
            foreach (var field in fieldsToCreate)
            {
                tb.DefineField(field.Name, field.FieldType, field.Attributes);
            }

            return tb.CreateType();
        }

        public object CopyTo(TDerived instance, Type objectType)
        {
            var myObject = Activator.CreateInstance(objectType);
            var derivedType = typeof(TDerived);
            var derivedFields = derivedType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (var field in derivedFields)
            {
                var fieldValue = field.GetValue(instance);
                var fieldInfo = objectType.GetField(field.Name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (fieldInfo != null)
                {
                    fieldInfo.SetValue(myObject, fieldValue);
                }
            }
            return myObject;
        }

        private TypeBuilder GetTypeBuilder()
        {
            var typeSignature = $"{typeof(TDerived).Name}_ExDictionary";
            var assemblyName = new AssemblyName(typeSignature);
            AssemblyBuilder assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(
                assemblyName, AssemblyBuilderAccess.Run);
            
            ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule("MainModule");
            
            TypeBuilder tb = moduleBuilder.DefineType(typeSignature,
                    TypeAttributes.Public |
                    TypeAttributes.Class |
                    TypeAttributes.AutoClass |
                    TypeAttributes.AnsiClass |
                    TypeAttributes.BeforeFieldInit |
                    TypeAttributes.AutoLayout,
                    null);
            return tb;
        }

        public void ReplicateProperty(TypeBuilder tb, PropertyInfo property, FieldInfo existingField)
        {
            //FieldBuilder fieldBuilder = tb.DefineField("_" + propertyName, propertyType, FieldAttributes.Private);
            var fieldBuilder = tb.DefineField(existingField.Name, existingField.FieldType, existingField.Attributes);
            var getAttributes = property.GetGetMethod().Attributes;
            var setAttributes = property.GetSetMethod().Attributes;

            PropertyBuilder propertyBuilder = tb.DefineProperty(property.Name, property.Attributes, property.PropertyType, null);
            MethodBuilder getPropMthdBldr = tb.DefineMethod("get_" + property.Name, getAttributes, property.PropertyType, Type.EmptyTypes);
            ILGenerator getIl = getPropMthdBldr.GetILGenerator();

            getIl.Emit(OpCodes.Ldarg_0);
            getIl.Emit(OpCodes.Ldfld, fieldBuilder);
            getIl.Emit(OpCodes.Ret);

            MethodBuilder setPropMthdBldr =
                tb.DefineMethod("set_" + property.Name,
                  setAttributes,
                  null, new[] { property.PropertyType });

            ILGenerator setIl = setPropMthdBldr.GetILGenerator();
            Label modifyProperty = setIl.DefineLabel();
            Label exitSet = setIl.DefineLabel();

            setIl.MarkLabel(modifyProperty);
            setIl.Emit(OpCodes.Ldarg_0);
            setIl.Emit(OpCodes.Ldarg_1);
            setIl.Emit(OpCodes.Stfld, fieldBuilder);

            setIl.Emit(OpCodes.Nop);
            setIl.MarkLabel(exitSet);
            setIl.Emit(OpCodes.Ret);

            propertyBuilder.SetGetMethod(getPropMthdBldr);
            propertyBuilder.SetSetMethod(setPropMthdBldr);
        }

        public FieldInfo GetBackingField(PropertyInfo property)
        {
            var getMethod = property.GetGetMethod(true);
            if (getMethod == null)
            {
                throw new InvalidOperationException("Property does not have a getter.");
            }

            var instructions = getMethod.GetMethodBody().GetILAsByteArray();
            for (int i = 0; i < instructions.Length; i++)
            {
                // Look for the "ldfld" or "stfld" opcode, which is used to load or store a field
                if (instructions[i] == OpCodes.Ldfld.Value || instructions[i] == OpCodes.Stfld.Value)
                {
                    // The next bytes represent the metadata token for the field
                    int metadataToken = BitConverter.ToInt32(instructions, i + 1);
                    return getMethod.Module.ResolveField(metadataToken);
                }
            }

            throw new InvalidOperationException("Backing field not found.");
        }
    }
}
