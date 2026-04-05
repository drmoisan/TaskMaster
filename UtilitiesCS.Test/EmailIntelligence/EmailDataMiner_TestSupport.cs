using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Office.Interop.Outlook;
using Moq;
using Newtonsoft.Json;
using TaskMaster;
using UtilitiesCS;
using UtilitiesCS.EmailIntelligence;
using UtilitiesCS.EmailIntelligence.Bayesian;
using UtilitiesCS.HelperClasses;
using UtilitiesCS.Interfaces;
using UtilitiesCS.ReusableTypeClasses;

namespace UtilitiesCS.Test.EmailIntelligence
{
    public partial class EmailDataMiner_Tests
    {
        private static ConcurrentDictionary<string, string> CreateAppDataMap(string appDataRoot) =>
            new() { ["AppData"] = appDataRoot };

        private static string GetGuaranteedMissingPath(string scenario) =>
            Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "EmailDataMinerCoverageMissingPaths",
                scenario,
                "missing-root"
            );

        private sealed class StubGlobalsWithEmptySpecialFolders : StubGlobals
        {
            public StubGlobalsWithEmptySpecialFolders()
                : base(specialFolders: new ConcurrentDictionary<string, string>()) { }
        }

        private class StubGlobals : IApplicationGlobals
        {
            public StubGlobals(
                ConcurrentDictionary<string, string> specialFolders = null,
                IToDoObjects toDoObjects = null
            )
            {
                FS = new StubFileSystemFolderPaths(
                    specialFolders ?? new ConcurrentDictionary<string, string>()
                );
                TD = toDoObjects ?? new StubToDoObjects();
            }

            public IFileSystemFolderPaths FS { get; }

            public Task LoadAsync(bool parallel) => throw new NotImplementedException();

            public IOlObjects Ol => throw new NotImplementedException();

            public IToDoObjects TD { get; }

            public IAppAutoFileObjects AF => throw new NotImplementedException();

            public IAppEvents Events => throw new NotImplementedException();

            public IAppQuickFilerSettings QfSettings => throw new NotImplementedException();

            public IAppItemEngines Engines => throw new NotImplementedException();

            public IntelligenceConfig IntelRes => throw new NotImplementedException();
        }

        private sealed class StubFileSystemFolderPaths : IFileSystemFolderPaths
        {
            public StubFileSystemFolderPaths(ConcurrentDictionary<string, string> specialFolders)
            {
                SpecialFolders = specialFolders;
            }

            public ConcurrentDictionary<string, string> SpecialFolders { get; }

            public void Reload() => throw new NotImplementedException();

            public IAppStagingFilenames Filenames => throw new NotImplementedException();

            public string MatchBestSpecialFolder(string path) =>
                throw new NotImplementedException();
        }

        private sealed class StubToDoObjects : IToDoObjects
        {
            public StubToDoObjects(
                ScoDictionary<string, int> filteredFolderScraping = null,
                ScoDictionary<string, string> folderRemap = null
            )
            {
                FilteredFolderScraping = filteredFolderScraping ?? new ScoDictionary<string, int>();
                FolderRemap = folderRemap ?? new ScoDictionary<string, string>();
            }

            public Task LoadAsync(bool parallel) => throw new NotImplementedException();

            public IPeopleScoDictionaryNew People => throw new NotImplementedException();

            public IScoDictionary<string, string> DictRemap => throw new NotImplementedException();

            public ISerializableList<string> CategoryFilters => throw new NotImplementedException();

            public IIDList IDList => throw new NotImplementedException();

            public IApplicationGlobals Parent => throw new NotImplementedException();

            public IProjectData ProjInfo => throw new NotImplementedException();

            public ScDictionary<string, string> ProgramInfo => throw new NotImplementedException();

            public ScoCollection<IPrefix> PrefixList => throw new NotImplementedException();

            public ScoCollection<IPrefix> LoadPrefixList() => throw new NotImplementedException();

            public ScoDictionary<string, int> FilteredFolderScraping { get; }

            public ScoDictionary<string, string> FolderRemap { get; }

            public string ProjInfo_Filename => throw new NotImplementedException();

            public string FnameDictRemap => throw new NotImplementedException();

            public string FnameIDList => throw new NotImplementedException();

            public Func<
                System.Collections.Generic.IEnumerable<string>,
                IPrefix,
                string,
                string,
                string
            > FindMatchingTag => throw new NotImplementedException();

            public Func<
                System.Collections.Generic.IEnumerable<string>,
                System.Collections.Generic.List<string>
            > SelectFromList => throw new NotImplementedException();

            public IFlagChangeTrainingQueue FlagChangeTrainingQueue =>
                throw new NotImplementedException();
        }

        private sealed class TestableEmailDataMiner : EmailDataMiner
        {
            public TestableEmailDataMiner(IApplicationGlobals globals)
                : base(globals) { }

            public object LoaderResult { get; set; }

            public long LoaderSize { get; set; }

            public object ValidationDeserializeResult { get; set; }

            public System.Exception ValidationDeserializeException { get; set; }

            public string CapturedFolderPath { get; private set; }

            public string CapturedFileName { get; private set; }

            public int SerializeMailInfoCalls { get; private set; }

            internal override void SerializeAndSave<T>(
                T obj,
                JsonSerializer serializer,
                FilePathHelper disk
            )
            {
                CapturedFolderPath = disk.FolderPath;
                CapturedFileName = disk.FileName;
            }

            internal override (T Object, long Size) TryLoadObjectAndGetMemorySize<T>(
                Func<T> loader,
                int copiesToLoad = 1
            )
            {
                return (LoaderResult is null ? default : (T)LoaderResult, LoaderSize);
            }

            internal override void SerializeMailInfo(
                Microsoft.Office.Interop.Outlook.MailItem mailItem
            )
            {
                SerializeMailInfoCalls++;
            }

            internal override Task<T> DeserializeForValidation<T>(
                string folderPath,
                string fileNameSeed,
                string fileNameSuffix = ""
            )
            {
                if (ValidationDeserializeException is not null)
                {
                    return Task.FromException<T>(ValidationDeserializeException);
                }

                return Task.FromResult(
                    ValidationDeserializeResult is T typedValue ? typedValue : default(T)
                );
            }

            internal override void LogSizeComparison(
                string m1,
                long s1,
                string m2,
                long s2,
                string objectName
            ) { }
        }

        private sealed class FolderTreeBackedEmailDataMiner : EmailDataMiner
        {
            public FolderTreeBackedEmailDataMiner(IApplicationGlobals globals)
                : base(globals)
            {
                typeof(EmailDataMiner)
                    .GetField("_sw", BindingFlags.Instance | BindingFlags.NonPublic)
                    .SetValue(this, new SegmentStopWatch().Start());
            }

            public FolderTree FolderTree { get; set; }

            public object DeserializedValue { get; set; }

            public IEnumerable<FolderWrapper> FolderInfos { get; set; }

            public IEnumerable<Microsoft.Office.Interop.Outlook.MAPIFolder> OutlookFolders { get; set; }

            public IEnumerable<Microsoft.Office.Interop.Outlook.MailItem> MailItems { get; set; }

            public bool UseBaseTryResolveMapiHandles { get; set; } = true;

            public bool TryResolveMapiHandlesResult { get; set; }

            public List<string> SavedSeeds { get; } = [];

            internal override FolderTree GetOlFolderTree() => FolderTree;

            internal override FolderTree GetOlFolderTree(ProgressTracker progress) => FolderTree;

            internal override IEnumerable<FolderWrapper> QueryOlFolderInfo(FolderTree tree) =>
                FolderInfos ?? base.QueryOlFolderInfo(tree);

            internal override IEnumerable<Microsoft.Office.Interop.Outlook.MAPIFolder> QueryOlFolders(
                FolderTree tree
            ) => OutlookFolders ?? base.QueryOlFolders(tree);

            internal override IEnumerable<Microsoft.Office.Interop.Outlook.MailItem> QueryMailItems(
                IEnumerable<Microsoft.Office.Interop.Outlook.MAPIFolder> folders
            ) => MailItems ?? base.QueryMailItems(folders);

            internal override T Deserialize<T>(string fileNameSeed, string fileNameSuffix = "") =>
                DeserializedValue is T typedValue ? typedValue : default;

            internal override async Task<bool> TryResolveMapiHandles(FolderWrapper[] folders)
            {
                if (UseBaseTryResolveMapiHandles)
                {
                    return await base.TryResolveMapiHandles(folders);
                }

                return await Task.FromResult(TryResolveMapiHandlesResult);
            }

            internal override void SerializeAndSave<T>(
                T obj,
                string fileNameSeed,
                string fileNameSuffix = ""
            )
            {
                SavedSeeds.Add(
                    string.IsNullOrEmpty(fileNameSuffix)
                        ? fileNameSeed
                        : $"{fileNameSeed}_{fileNameSuffix}"
                );
            }
        }

        private static FolderTree CreateFolderTree(params FolderWrapper[] folders)
        {
            var tree = new FolderTree();
            var roots = folders.Select(folder => new TreeNode<FolderWrapper>(folder)).ToList();
            typeof(FolderTree)
                .GetField("_roots", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(tree, roots);
            return tree;
        }

        private static Mock<Items> CreateOutlookItems(int count, params object[] items)
        {
            var outlookItems = new Mock<Items>(MockBehavior.Strict);
            var collection = new ArrayList(items ?? Array.Empty<object>());
            outlookItems.SetupGet(x => x.Count).Returns(count);
            outlookItems.Setup(x => x.GetEnumerator()).Returns(() => collection.GetEnumerator());
            return outlookItems;
        }

        private static Mock<MAPIFolder> CreateOutlookFolder(int count, params object[] items)
        {
            var folder = new Mock<MAPIFolder>(MockBehavior.Strict);
            folder.SetupGet(x => x.Items).Returns(CreateOutlookItems(count, items).Object);
            return folder;
        }

        private static async Task<object[]> InvokeEnumerableTask(
            object target,
            string methodName,
            params object[] args
        )
        {
            var method = ResolveMethod(target, methodName, args);
            var task = (Task)method.Invoke(target, args);
            await task;
            var result = task.GetType().GetProperty("Result").GetValue(task);
            return ((IEnumerable)result).Cast<object>().ToArray();
        }

        private static object[] InvokeEnumerable(
            object target,
            string methodName,
            params object[] args
        )
        {
            var method = ResolveMethod(target, methodName, args);
            var result = method.Invoke(target, args);
            return ((IEnumerable)result).Cast<object>().ToArray();
        }

        private static object GetTupleField(object tuple, string fieldName)
        {
            return tuple.GetType().GetField(fieldName).GetValue(tuple);
        }

        private static MethodInfo ResolveMethod(object target, string methodName, object[] args)
        {
            for (
                var currentType = target.GetType();
                currentType is not null;
                currentType = currentType.BaseType
            )
            {
                var match = currentType
                    .GetMethods(
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
                    )
                    .SingleOrDefault(method =>
                    {
                        if (method.Name != methodName)
                        {
                            return false;
                        }

                        var parameters = method.GetParameters();
                        if (parameters.Length != args.Length)
                        {
                            return false;
                        }

                        for (var i = 0; i < parameters.Length; i++)
                        {
                            if (
                                args[i] is not null
                                && !parameters[i].ParameterType.IsAssignableFrom(args[i].GetType())
                            )
                            {
                                return false;
                            }
                        }

                        return true;
                    });
                if (match is not null)
                {
                    return match;
                }
            }

            throw new InvalidOperationException($"No overload found for {methodName}.");
        }

        private sealed class NoOpProgressTracker : ProgressTracker
        {
            public NoOpProgressTracker()
                : base(new CancellationTokenSource()) { }

            public override void Report((int Value, string JobName) report) { }

            public override void Report(double value, string jobName) { }

            public override void Report(double value) { }
        }
    }
}
