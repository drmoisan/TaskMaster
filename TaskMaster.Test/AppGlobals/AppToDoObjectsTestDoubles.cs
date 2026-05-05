#nullable enable

using System;
using System.Collections.Concurrent;
using System.IO;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Proxies;
using System.Threading.Tasks;
using Microsoft.Office.Interop.Outlook;
using Newtonsoft.Json;
using ToDoModel.Data_Model.People;
using UtilitiesCS;
using UtilitiesCS.EmailIntelligence;
using UtilitiesCS.ReusableTypeClasses.SerializableNew.Concurrent.Observable;

namespace TaskMaster.Test.AppGlobals
{
    internal static class EventHelper
    {
        internal static Delegate[] GetEventInvocationList(object target, string eventName)
        {
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            if (string.IsNullOrEmpty(eventName))
            {
                throw new ArgumentNullException(nameof(eventName));
            }

            var targetType = target.GetType();
            var eventInfo =
                targetType.GetEvent(
                    eventName,
                    BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public
                )
                ?? throw new ArgumentException(
                    $"Event '{eventName}' not found on type '{targetType}'."
                );

            var declaringType =
                eventInfo.DeclaringType
                ?? throw new ArgumentException(
                    $"Event '{eventName}' does not have a declaring type."
                );

            var eventField =
                (
                    declaringType.GetField(
                        eventName,
                        BindingFlags.Instance
                            | BindingFlags.NonPublic
                            | BindingFlags.Public
                            | BindingFlags.Static
                            | BindingFlags.FlattenHierarchy
                    ) ?? FindEventFieldInBaseClasses(declaringType, eventName)
                )
                ?? throw new ArgumentException(
                    $"Event field '{eventName}' not found on type '{declaringType}'."
                );
            var eventFieldValue = eventField.GetValue(target);
            if (eventFieldValue is Delegate eventDelegate)
            {
                return eventDelegate.GetInvocationList();
            }

            return [];
        }

        private static FieldInfo? FindEventFieldInBaseClasses(Type type, string eventName)
        {
            while (type != null)
            {
                var field = type.GetField(
                    eventName,
                    BindingFlags.Instance
                        | BindingFlags.NonPublic
                        | BindingFlags.Public
                        | BindingFlags.Static
                        | BindingFlags.FlattenHierarchy
                );
                if (field != null)
                {
                    return field;
                }

                type = type.BaseType!;
            }

            return null;
        }
    }

    internal sealed class StubFileSystemFolderPaths : IFileSystemFolderPaths
    {
        public ConcurrentDictionary<string, string> SpecialFolders { get; } = new();

        public void Reload() { }

        public IAppStagingFilenames Filenames => throw new NotSupportedException();

        public string MatchBestSpecialFolder(string path) => path;
    }

    internal sealed class OlObjectsProxy : RealProxy
    {
        private readonly Func<Application> appAccessor;

        private OlObjectsProxy(Func<Application> appAccessor)
            : base(typeof(IOlObjects))
        {
            this.appAccessor = appAccessor;
        }

        internal static IOlObjects Create(Func<Application> appAccessor)
        {
            return (IOlObjects)new OlObjectsProxy(appAccessor).GetTransparentProxy();
        }

        public override IMessage Invoke(IMessage msg)
        {
            var call = (IMethodCallMessage)msg;

            if (call.MethodName == "get_App")
            {
                return new ReturnMessage(appAccessor(), null, 0, call.LogicalCallContext, call);
            }

            return new ReturnMessage(
                new NotSupportedException(
                    $"Member '{call.MethodName}' is not used by this test proxy."
                ),
                call
            );
        }
    }

    internal sealed class StubApplicationGlobals : IApplicationGlobals
    {
        public StubApplicationGlobals(IFileSystemFolderPaths fs, IOlObjects ol)
        {
            FS = fs;
            Ol = ol;
        }

        public IFileSystemFolderPaths FS { get; }

        public IOlObjects Ol { get; }

        public Task LoadAsync(bool parallel) => throw new NotSupportedException();

        public IToDoObjects TD => throw new NotSupportedException();

        public IAppAutoFileObjects AF => throw new NotSupportedException();

        public IAppEvents Events => throw new NotSupportedException();

        public IAppQuickFilerSettings QfSettings => throw new NotSupportedException();

        public IAppItemEngines Engines => throw new NotSupportedException();

        public IntelligenceConfig IntelRes => throw new NotSupportedException();
    }

    internal sealed class ReflectionRealProxy : RealProxy
    {
        private readonly Func<MethodInfo, object?[]?, object?> handler;

        internal ReflectionRealProxy(
            Type interfaceType,
            Func<MethodInfo, object?[]?, object?> handler
        )
            : base(interfaceType)
        {
            this.handler = handler;
        }

        public override IMessage Invoke(IMessage msg)
        {
            var call = (IMethodCallMessage)msg;

            try
            {
                var result = handler((MethodInfo)call.MethodBase, call.Args);
                return new ReturnMessage(result, null, 0, call.LogicalCallContext, call);
            }
            catch (System.Exception ex)
            {
                return new ReturnMessage(ex, call);
            }
        }
    }

    internal sealed class ProjectDataSerializableListScope : IDisposable
    {
        private readonly PropertyInfo fileSystemProperty;
        private readonly PropertyInfo promptProperty;
        private readonly object originalFileSystem;
        private readonly object originalPrompt;
        private readonly string serializedPayload;

        internal ProjectDataSerializableListScope(params IProjectEntry[] seedEntries)
        {
            var serializableListType = typeof(SerializableList<>).MakeGenericType(
                typeof(IProjectEntry)
            );
            fileSystemProperty = serializableListType.GetProperty(
                "FileSystem",
                BindingFlags.Static | BindingFlags.NonPublic
            )!;
            promptProperty = serializableListType.GetProperty(
                "Prompt",
                BindingFlags.Static | BindingFlags.NonPublic
            )!;
            originalFileSystem = fileSystemProperty.GetValue(null)!;
            originalPrompt = promptProperty.GetValue(null)!;
            serializedPayload = JsonConvert.SerializeObject(
                seedEntries,
                new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.Auto,
                    Formatting = Formatting.Indented,
                }
            );

            fileSystemProperty.SetValue(
                null,
                CreateProxy(
                    fileSystemProperty.PropertyType,
                    (method, _) =>
                        method.Name switch
                        {
                            "ReadAllText" => serializedPayload,
                            "CreateText" => new StreamWriter(Stream.Null),
                            _ => throw new NotSupportedException(method.Name),
                        }
                )
            );
            promptProperty.SetValue(
                null,
                CreateProxy(
                    promptProperty.PropertyType,
                    (_, _) => System.Windows.Forms.DialogResult.Yes
                )
            );
        }

        public void Dispose()
        {
            fileSystemProperty.SetValue(null, originalFileSystem);
            promptProperty.SetValue(null, originalPrompt);
        }

        private static object CreateProxy(
            Type interfaceType,
            Func<MethodInfo, object?[]?, object?> handler
        )
        {
            return new ReflectionRealProxy(interfaceType, handler).GetTransparentProxy();
        }
    }
}
