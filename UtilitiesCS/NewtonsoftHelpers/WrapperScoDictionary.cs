#nullable enable
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Mono.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UtilitiesCS.Extensions;
using UtilitiesCS.NewtonsoftHelpers.MonoExtension;
using UtilitiesCS.ReusableTypeClasses;
using UtilitiesCS.ReusableTypeClasses.Concurrent.Observable.Dictionary;

namespace UtilitiesCS.NewtonsoftHelpers
{
    public class WrapperScoDictionary<TDerived, TKey, TValue>
        where TDerived : ScoDictionaryNew<TKey, TValue>
    {
        [JsonProperty("CoDictionary")]
        public ConcurrentObservableDictionary<TKey, TValue> CoDictionary { get; set; }

        // [JsonProperty] deserialization target; annotated null! to preserve the non-null
        // contract consumers rely on (guarded with RemainingObject.ThrowIfNull() before use).
        [JsonProperty("RemainingObject")]
        public object RemainingObject { get; set; } = null!;

        public WrapperScoDictionary()
        {
            CoDictionary = new ConcurrentObservableDictionary<TKey, TValue>();
        }

        public TDerived ToDerived(WrapperScoDictionary<TDerived, TKey, TValue> wrapper)
        {
            CoDictionary = wrapper.CoDictionary;
            RemainingObject = wrapper.RemainingObject;
            return ToDerived();
        }

        public TDerived ToDerived()
        {
            CoDictionary.ThrowIfNull();
            RemainingObject.ThrowIfNull();

            // Create an instance using reflection
            var derivedInstance = (TDerived)Activator.CreateInstance(typeof(TDerived), true);

            // Copy dictionary entries
            foreach (var kvp in CoDictionary)
            {
                derivedInstance.TryAdd(kvp.Key, kvp.Value);
            }

            // Set up the config field
            var remainingObjectType = RemainingObject.GetType();
            var configField =
                remainingObjectType.GetField(
                    "<Config>k__BackingField",
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
                )
                ?? remainingObjectType.GetField(
                    "_Config",
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
                );
            var configValue = configField?.GetValue(RemainingObject) as NewSmartSerializableConfig;
            if (configValue is null)
            {
                var configProperty = remainingObjectType.GetProperty(
                    "Config",
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
                );
                configValue =
                    configProperty?.GetValue(RemainingObject) as NewSmartSerializableConfig;
            }

            // When deserialization runs under TypeNameHandling.None there is no $type discriminator,
            // so Newtonsoft binds the untyped RemainingObject to a JObject. A JObject exposes no
            // <Config>k__BackingField / _Config field and no CLR Config property, so the reflective
            // lookups above return null and Config would otherwise be left at its empty default.
            // Read the Config token directly out of the JObject and materialize a typed
            // NewSmartSerializableConfig. The typed-RemainingObject reflective path above is
            // preserved: this branch only runs as a fallback when configValue is still null.
            if (configValue is null && RemainingObject is JObject remainingObjectJson)
            {
                var configToken = remainingObjectJson["Config"];
                if (configToken is not null && configToken.Type != JTokenType.Null)
                {
                    configValue = configToken.ToObject<NewSmartSerializableConfig>();
                    NormalizeEmptyDiskFilePaths(configValue);
                }
            }

            if (configValue is not null)
            {
                derivedInstance.Config = configValue;
            }
            // Set additional fields
            var derivedType = typeof(TDerived);

            var additionalFields = RemainingObject
                .GetType()
                .GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .ToArray();
            var drvdFields = derivedType
                .GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .ToArray();
            foreach (var field in additionalFields)
            {
                var fieldInfo = derivedType.GetField(
                    field.Name,
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
                );

                if (fieldInfo != null)
                {
                    var fieldValue = field.GetValue(RemainingObject);
                    fieldInfo.SetValue(derivedInstance, fieldValue);
                }
            }

            var additionalProperties = remainingObjectType
                .GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(p => p.Name != nameof(ScoDictionaryNew<TKey, TValue>.Config));

            foreach (var property in additionalProperties)
            {
                if (!property.CanRead || property.GetIndexParameters().Length > 0)
                {
                    continue;
                }

                var targetProperty = derivedType.GetProperty(
                    property.Name,
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
                );
                if (targetProperty is null || !targetProperty.CanWrite)
                {
                    continue;
                }

                var propertyValue = property.GetValue(RemainingObject);
                targetProperty.SetValue(derivedInstance, propertyValue);
            }

            // new 2/16/2025
            //var baseType = typeof(ConcurrentObservableDictionary<TKey, TValue>);

            //var derivedProperties = derivedType
            //    .GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            //    .Where(property => (property.DeclaringType != baseType) && (property.Name != "Config"))
            //    .ToArray();

            //foreach (var property in derivedProperties)
            //{
            //    var obj = property.GetValue(RemainingObject);
            //    property.SetValue(derivedInstance, obj);
            //}

            return derivedInstance;
        }

        // A default-constructed FilePathHelper reports FilePath == "" (empty string), but when a
        // FilePathHelper carrying no FileName is materialized from JSON its FilePath setter routes
        // through the PropertyChanged handler, which leaves the backing field at null. That null vs
        // empty-string divergence is invisible to callers that only read FileName (e.g. the People
        // pplkey deserialization) but breaks full-graph equivalence checks against a default Config.
        // For each empty disk (no FileName) on a freshly reconstructed Config, restore the
        // default empty-string FilePath so reconstructed-but-empty disks match default disks.
        private static void NormalizeEmptyDiskFilePaths(NewSmartSerializableConfig? config)
        {
            if (config is null)
            {
                return;
            }

            NormalizeEmptyDiskFilePath(config.Disk);
            NormalizeEmptyDiskFilePath(config.LocalDisk);
            NormalizeEmptyDiskFilePath(config.NetDisk);
        }

        private static void NormalizeEmptyDiskFilePath(FilePathHelper disk)
        {
            if (disk is not null && disk.FileName.IsNullOrEmpty() && disk.FilePath is null)
            {
                disk.FilePath = "";
            }
        }

        public WrapperScoDictionary<TDerived, TKey, TValue> ToComposition(TDerived derivedInstance)
        {
            derivedInstance.ThrowIfNull();
            CoDictionary = new ConcurrentObservableDictionary<TKey, TValue>(derivedInstance);

            Type objectType = CompileType();
            var instance = CopyTo(derivedInstance, objectType);
            RemainingObject = instance;

            return this;
        }

        public Type CompileType()
        {
            var derivedType = typeof(TDerived);
            var baseType = typeof(ConcurrentObservableDictionary<TKey, TValue>);

            var derivedProperties = derivedType
                .GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(property =>
                    (property.DeclaringType != baseType) && (property.Name != "Config")
                )
                .ToArray();

            var derivedFields = derivedType
                .GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(field => (field.DeclaringType != baseType) && (field.Name != "ism"));

            TypeBuilder tb = GetTypeBuilder();
            ConstructorBuilder constructor = tb.DefineDefaultConstructor(
                MethodAttributes.Public
                    | MethodAttributes.SpecialName
                    | MethodAttributes.RTSpecialName
            );

            CreateConfigProperty(tb);

            var capturedFields = new Dictionary<string, FieldBuilder>();
            foreach (var property in derivedProperties)
            {
                ReplicateProperty(tb, property, ref capturedFields);
            }

            var fieldsToCreate = derivedFields
                .Where(field => !capturedFields.ContainsKey(field.Name))
                .ToArray();

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

            // Set up the config field
            objectType
                .GetField(
                    "_Config",
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
                )
                ?.SetValue(myObject, instance.Config);

            // Get all other fields in the derived type except for ism which was captured by the _Config field
            var derivedFields = derivedType
                .GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(field => field.Name != "ism")
                .ToArray();

            foreach (var field in derivedFields)
            {
                var fieldValue = field.GetValue(instance);
                var fieldInfo = objectType.GetField(
                    field.Name,
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
                );
                fieldInfo?.SetValue(myObject, fieldValue);
            }
            return myObject;
        }

        private TypeBuilder GetTypeBuilder()
        {
            var typeSignature = $"{typeof(TDerived).Name}_ExDictionary";
            var assemblyName = new AssemblyName(typeSignature);
            AssemblyBuilder assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(
                assemblyName,
                AssemblyBuilderAccess.Run
            );

            ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule("MainModule");

            TypeBuilder tb = moduleBuilder.DefineType(
                typeSignature,
                TypeAttributes.Public
                    | TypeAttributes.Class
                    | TypeAttributes.AutoClass
                    | TypeAttributes.AnsiClass
                    | TypeAttributes.BeforeFieldInit
                    | TypeAttributes.AutoLayout,
                null
            );
            return tb;
        }

        public void CreateConfigProperty(TypeBuilder tb)
        {
            var propertyBuilder = tb.DefineProperty(
                "Config",
                PropertyAttributes.None,
                typeof(NewSmartSerializableConfig),
                null
            );
            var fieldBuilder = tb.DefineField(
                "_Config",
                typeof(NewSmartSerializableConfig),
                FieldAttributes.Private
            );

            var getMethod = tb.DefineMethod(
                "get_Config",
                MethodAttributes.Public,
                typeof(NewSmartSerializableConfig),
                Type.EmptyTypes
            );
            var getIl = getMethod.GetILGenerator();
            getIl.Emit(OpCodes.Ldarg_0);
            getIl.Emit(OpCodes.Ldfld, fieldBuilder);
            getIl.Emit(OpCodes.Ret);
            propertyBuilder.SetGetMethod(getMethod);

            var setMethod = tb.DefineMethod(
                "set_Config",
                MethodAttributes.Public,
                null,
                new[] { typeof(NewSmartSerializableConfig) }
            );
            var setIl = setMethod.GetILGenerator();
            Label modifyProperty = setIl.DefineLabel();
            Label exitSet = setIl.DefineLabel();
            setIl.MarkLabel(modifyProperty);
            setIl.Emit(OpCodes.Ldarg_0);
            setIl.Emit(OpCodes.Ldarg_1);
            setIl.Emit(OpCodes.Stfld, fieldBuilder);
            setIl.Emit(OpCodes.Nop);
            setIl.MarkLabel(exitSet);
            setIl.Emit(OpCodes.Ret);
        }

        public void ReplicateProperty(
            TypeBuilder tb,
            PropertyInfo property,
            ref Dictionary<string, FieldBuilder> capturedFields
        )
        {
            PropertyBuilder propertyBuilder = tb.DefineProperty(
                property.Name,
                property.Attributes,
                property.PropertyType,
                property.DeclaringType.GetGenericArguments()
            );
            var getMethod = ModifyGetMethod(tb, property, ref capturedFields);
            if (getMethod is not null)
            {
                propertyBuilder.SetGetMethod(getMethod);
            }

            var setMethod = ModifySetMethod(tb, property, ref capturedFields);
            if (setMethod is not null)
            {
                propertyBuilder.SetSetMethod(setMethod);
            }
            ;
        }

        public void ReplicateProperty(
            TypeBuilder tb,
            PropertyInfo property,
            FieldInfo existingField
        )
        {
            //FieldBuilder fieldBuilder = tb.DefineField("_" + propertyName, propertyType, FieldAttributes.Private);
            var fieldBuilder = tb.DefineField(
                existingField.Name,
                existingField.FieldType,
                existingField.Attributes
            );
            var getAttributes = property.GetGetMethod().Attributes;
            var setAttributes = property.GetSetMethod().Attributes;

            PropertyBuilder propertyBuilder = tb.DefineProperty(
                property.Name,
                property.Attributes,
                property.PropertyType,
                null
            );
            MethodBuilder getPropMthdBldr = GenerateGetMethod(
                tb,
                property,
                fieldBuilder,
                getAttributes
            );
            MethodBuilder setPropMthdBldr = GenerateSetMethod(
                tb,
                property,
                fieldBuilder,
                setAttributes
            );

            propertyBuilder.SetGetMethod(getPropMthdBldr);
            propertyBuilder.SetSetMethod(setPropMthdBldr);
        }

        private static MethodBuilder GenerateSetMethod(
            TypeBuilder tb,
            PropertyInfo property,
            FieldBuilder fieldBuilder,
            MethodAttributes setAttributes
        )
        {
            MethodBuilder setPropMthdBldr = tb.DefineMethod(
                "set_" + property.Name,
                setAttributes,
                null,
                new[] { property.PropertyType }
            );

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
            return setPropMthdBldr;
        }

        // Per-file behavior preserved: this wrapper's ModifySetMethod returns null when the
        // property has no setter (nullable return), unlike WrapperScDictionary which throws.
        private MethodBuilder? ModifySetMethod(
            TypeBuilder tb,
            PropertyInfo property,
            ref Dictionary<string, FieldBuilder> backingFields
        )
        {
            //Type[] method_arguments = null;
            Type[]? type_arguments = null;
            var oldSetMethod = property.GetSetMethod(true);
            if (oldSetMethod == null)
            {
                return null;
            }

            //if (!(oldGetMethod is ConstructorInfo))
            //    method_arguments = oldGetMethod.GetGenericArguments();

            if (oldSetMethod.DeclaringType != null)
                type_arguments = oldSetMethod.DeclaringType.GetGenericArguments();

            var oldInstructions = Disassembler.GetInstructions(oldSetMethod);
            //var newInstructions = new List<Instruction>();

            MethodBuilder setPropMthdBldr = tb.DefineMethod(
                "set_" + property.Name,
                oldSetMethod.Attributes,
                property.PropertyType,
                type_arguments
            );
            ILGenerator setIl = setPropMthdBldr.GetILGenerator();

            foreach (var instruction in oldInstructions)
            {
                if (instruction.OpCode == OpCodes.Ldfld || instruction.OpCode == OpCodes.Stfld)
                {
                    var bf = (FieldInfo)instruction.Operand!;
                    //FieldBuilder fieldBuilder;
                    if (!backingFields.TryGetValue(bf.Name, out var fieldBuilder))
                    {
                        fieldBuilder = tb.DefineField(bf.Name, bf.FieldType, bf.Attributes);
                        backingFields[bf.Name] = fieldBuilder;
                    }

                    setIl.Emit(instruction.OpCode, fieldBuilder);
                    //setIl.Emit(OpCodes.Ldfld, fieldBuilder);
                }
                else if (instruction.OpCode == OpCodes.Callvirt)
                {
                    var method = (MethodInfo)instruction.Operand!;
                    setIl.Emit(instruction.OpCode, method);
                }
                else if (instruction.Operand is not null)
                {
                    instruction.EmitOperand(setIl, setPropMthdBldr);
                    //getIl.Emit(instruction.OpCode, instruction.Operand);
                }
                else
                {
                    setIl.Emit(instruction.OpCode);
                }
                //newInstructions.Add(instruction);
            }

            return setPropMthdBldr;
        }

        private MethodBuilder ModifyGetMethod(
            TypeBuilder tb,
            PropertyInfo property,
            ref Dictionary<string, FieldBuilder> backingFields
        )
        {
            //Type[] method_arguments = null;
            Type[]? type_arguments = null;
            var oldGetMethod = property.GetGetMethod(true);
            if (oldGetMethod == null)
            {
                throw new InvalidOperationException("Property does not have a getter.");
            }

            //if (!(oldGetMethod is ConstructorInfo))
            //    method_arguments = oldGetMethod.GetGenericArguments();

            if (oldGetMethod.DeclaringType != null)
                type_arguments = oldGetMethod.DeclaringType.GetGenericArguments();

            var oldInstructions = Disassembler.GetInstructions(oldGetMethod);
            //var newInstructions = new List<Instruction>();

            MethodBuilder getPropMthdBldr = tb.DefineMethod(
                "get_" + property.Name,
                oldGetMethod.Attributes,
                property.PropertyType,
                type_arguments
            );
            ILGenerator getIl = getPropMthdBldr.GetILGenerator();

            foreach (var instruction in oldInstructions)
            {
                if (instruction.OpCode == OpCodes.Ldfld || instruction.OpCode == OpCodes.Stfld)
                {
                    var bf = (FieldInfo)instruction.Operand!;
                    //FieldBuilder fieldBuilder;
                    if (!backingFields.TryGetValue(bf.Name, out var fieldBuilder))
                    {
                        fieldBuilder = tb.DefineField(bf.Name, bf.FieldType, bf.Attributes);
                        backingFields[bf.Name] = fieldBuilder;
                    }

                    getIl.Emit(instruction.OpCode, fieldBuilder);
                    //getIl.Emit(OpCodes.Ldfld, fieldBuilder);
                }
                else if (instruction.OpCode == OpCodes.Callvirt)
                {
                    var method = (MethodInfo)instruction.Operand!;
                    getIl.Emit(instruction.OpCode, method);
                }
                else if (instruction.Operand is not null)
                {
                    instruction.EmitOperand(getIl, getPropMthdBldr);
                    //getIl.Emit(instruction.OpCode, instruction.Operand);
                }
                else
                {
                    getIl.Emit(instruction.OpCode);
                }
                //newInstructions.Add(instruction);
            }

            return getPropMthdBldr;
        }

        private static MethodBuilder GenerateGetMethod(
            TypeBuilder tb,
            PropertyInfo property,
            FieldBuilder fieldBuilder,
            MethodAttributes getAttributes
        )
        {
            MethodBuilder getPropMthdBldr = tb.DefineMethod(
                "get_" + property.Name,
                getAttributes,
                property.PropertyType,
                Type.EmptyTypes
            );
            ILGenerator getIl = getPropMthdBldr.GetILGenerator();

            getIl.Emit(OpCodes.Ldarg_0);
            getIl.Emit(OpCodes.Ldfld, fieldBuilder);
            getIl.Emit(OpCodes.Ret);
            return getPropMthdBldr;
        }

        public FieldInfo GetBackingField(PropertyInfo property)
        {
            var getMethod = property.GetGetMethod(true);
            if (getMethod == null)
            {
                throw new InvalidOperationException("Property does not have a getter.");
            }

            // Try naming conventions first — these are immune to coverage-tool IL instrumentation,
            // which can rewrite method bodies and invalidate IL-derived metadata tokens.
            var declaringType =
                property.DeclaringType
                ?? throw new InvalidOperationException("Property has no declaring type.");

            var allFields = declaringType.GetFields(
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public
            );

            // Convention 1: underscore-prefixed camelCase (_propertyName)
            var underscoreName =
                $"_{char.ToLowerInvariant(property.Name[0])}{property.Name.Substring(1)}";
            var byConvention = Array.Find(allFields, f => f.Name == underscoreName);
            if (byConvention != null)
            {
                return byConvention;
            }

            // Convention 2: compiler-generated auto-property backing field (<PropertyName>k__BackingField)
            var autoName = $"<{property.Name}>k__BackingField";
            var byAutoName = Array.Find(allFields, f => f.Name == autoName);
            if (byAutoName != null)
            {
                return byAutoName;
            }

            // Fall back to IL parsing for non-standard backing field names.
            // NOTE: This path can fail when IL has been instrumented by a code coverage tool.
            var instructions = getMethod.GetMethodBody().GetILAsByteArray();
            for (int i = 0; i < instructions.Length; i++)
            {
                // Look for the "ldfld" or "stfld" opcode, which is used to load or store a field
                if (
                    instructions[i] == OpCodes.Ldfld.Value
                    || instructions[i] == OpCodes.Stfld.Value
                )
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
