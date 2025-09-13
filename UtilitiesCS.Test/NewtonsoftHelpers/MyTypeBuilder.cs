using System;
using System.Collections;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.IO;
using System.Linq;

namespace TypeBuilderNamespace
{
    public static class MyTypeBuilder
    {
        public static void CreateNewObject()
        {            
            var myType = CompileResultType([new FieldReduced() { Name = "TestField", Type = typeof(string)}]);
            var myObject = Activator.CreateInstance(myType);
        }

        public struct FieldReduced
        {
            public string Name;
            public Type Type;
        }

        public static Type CompileResultType(IEnumerable<FieldReduced> yourListOfFields)
        {
            TypeBuilder tb = GetTypeBuilder();
            ConstructorBuilder constructor = tb.DefineDefaultConstructor(MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName);

            // NOTE: assuming your list contains Field objects with fields FieldName(string) and FieldType(Type)
            foreach (var field in yourListOfFields)
            {
                CreateProperty(tb, field.Name, field.Type);
            }

            Type objectType = tb.CreateType();
            return objectType;
        }

        public static object CreateReplica<T>(T instance)
        {
            var myType = CompileResultType(instance);
            //var myObject = Activator.CreateInstance(myType);
            var myObject = CopyTo(instance, myType);
            return myObject;
        }

        public static object CopyTo<T>(T instance, Type objectType)
        {
            var myObject = Activator.CreateInstance(objectType);
            var derivedType = typeof(T);
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

        public static Type CompileResultType<T>(T instance) 
        {
            TypeBuilder tb = GetTypeBuilder();
            ConstructorBuilder constructor = tb.DefineDefaultConstructor(MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName);

            var existingProperties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            
            foreach (var property in existingProperties)
            {
                ReplicateProperty(tb, property);
            }
            
            var existingFields = typeof(T).GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
            var capturedFields = tb.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
            var fieldsToCreate = existingFields.Except(capturedFields);
            foreach (var field in fieldsToCreate)
            {
                tb.DefineField(field.Name, field.FieldType, field.Attributes);
            }

            Type objectType = tb.CreateType();
            return objectType;
        }

        private static TypeBuilder GetTypeBuilder()
        {
            var typeSignature = "MyDynamicType";
            var an = new AssemblyName(typeSignature);
            AssemblyBuilder assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(an, AssemblyBuilderAccess.Run);
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

        private static void ReplicateProperty(TypeBuilder tb, PropertyInfo property)
        {
            //FieldBuilder fieldBuilder = tb.DefineField("_" + propertyName, propertyType, FieldAttributes.Private);
            FieldInfo existingField = GetBackingField(property);
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


        private static void CreateProperty(TypeBuilder tb, string propertyName, Type propertyType)
        {
            FieldBuilder fieldBuilder = tb.DefineField("_" + propertyName, propertyType, FieldAttributes.Private);

            PropertyBuilder propertyBuilder = tb.DefineProperty(propertyName, PropertyAttributes.HasDefault, propertyType, null);
            MethodBuilder getPropMthdBldr = tb.DefineMethod("get_" + propertyName, MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig, propertyType, Type.EmptyTypes);
            ILGenerator getIl = getPropMthdBldr.GetILGenerator();

            getIl.Emit(OpCodes.Ldarg_0);
            getIl.Emit(OpCodes.Ldfld, fieldBuilder);
            getIl.Emit(OpCodes.Ret);

            MethodBuilder setPropMthdBldr =
                tb.DefineMethod("set_" + propertyName,
                  MethodAttributes.Public |
                  MethodAttributes.SpecialName |
                  MethodAttributes.HideBySig,
                  null, new[] { propertyType });

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

        public static FieldInfo GetBackingField(PropertyInfo property)
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
