using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Moq;
using Newtonsoft.Json;
using UtilitiesCS.EmailIntelligence.Bayesian;
using UtilitiesCS.EmailIntelligence.ClassifierGroups;
using UtilitiesCS.HelperClasses;

namespace UtilitiesCS.Test.EmailIntelligence.ClassifierGroups
{
    internal static class ClassifierGroupUtilitiesTestSupport
    {
        internal static Mock<IApplicationGlobals> CreateGlobalsWithAppData(string appDataRoot)
        {
            var globals = new Mock<IApplicationGlobals>();
            var mockFs = new Mock<IFileSystemFolderPaths>();
            var specialFolders = new ConcurrentDictionary<string, string>
            {
                ["AppData"] = appDataRoot,
            };
            mockFs.SetupGet(x => x.SpecialFolders).Returns(specialFolders);
            globals.SetupGet(x => x.FS).Returns(mockFs.Object);
            return globals;
        }
    }

    internal sealed class StubClassifierGroupUtilities(
        IApplicationGlobals globals,
        BayesianClassifierGroup stubbedResult
    ) : ClassifierGroupUtilities(globals)
    {
        internal override T Deserialize<T>(string fileNameSeed, string fileNameSuffix = "")
        {
            if (stubbedResult is T result)
            {
                return result;
            }

            return default;
        }
    }

    internal sealed class RecordingClassifierGroupUtilities(IApplicationGlobals globals)
        : ClassifierGroupUtilities(globals)
    {
        private readonly Queue<object> _loaderResults = new();
        private readonly Queue<long> _loaderSizes = new();
        private readonly Dictionary<string, string> _storedTexts = new(
            StringComparer.OrdinalIgnoreCase
        );
        private IEnumerable<object> _loaderResultsSource;
        private IEnumerable<long> _loaderSizesSource;

        public IEnumerable<object> LoaderResults
        {
            get => _loaderResultsSource;
            set
            {
                _loaderResultsSource = value;
                _loaderResults.Clear();
                if (value is null)
                {
                    return;
                }

                foreach (var item in value)
                {
                    _loaderResults.Enqueue(item);
                }
            }
        }

        public IEnumerable<long> LoaderSizes
        {
            get => _loaderSizesSource;
            set
            {
                _loaderSizesSource = value;
                _loaderSizes.Clear();
                if (value is null)
                {
                    return;
                }

                foreach (var item in value)
                {
                    _loaderSizes.Enqueue(item);
                }
            }
        }

        public object ValidationResult { get; set; }

        public Exception ValidationException { get; set; }

        public bool InvokeBaseDeserializeAsync { get; set; }

        public bool InvokeBaseSerializeAndSaveCore { get; set; }

        public bool InvokeBaseSerializeFsSave { get; set; }

        public string CapturedFolderPath { get; private set; }

        public string CapturedFileName { get; private set; }

        public int SerializeMailInfoCalls { get; private set; }

        public bool InvokeBaseSerializeMailInfo { get; set; } = true;

        public List<string> SavedExampleNames { get; } = new();

        public List<string> LoggedObjectNames { get; } = new();

        public void StoreText(string filePath, string text)
        {
            _storedTexts[filePath] = text;
        }

        public string ReadStoredText(string filePath)
        {
            return _storedTexts[filePath];
        }

        internal override void SerializeAndSave<T>(
            T obj,
            JsonSerializer serializer,
            FilePathHelper disk
        )
        {
            CapturedFolderPath = disk.FolderPath;
            CapturedFileName = disk.FileName;
            if (InvokeBaseSerializeAndSaveCore)
            {
                base.SerializeAndSave(obj, serializer, disk);
            }
        }

        internal override void SerializeFsSave<T>(
            T obj,
            string objName,
            JsonSerializer serializer,
            FilePathHelper disk
        )
        {
            SavedExampleNames.Add(objName);
            CapturedFolderPath = disk.FolderPath;
            CapturedFileName = disk.FileName;
            if (InvokeBaseSerializeFsSave)
            {
                base.SerializeFsSave(obj, objName, serializer, disk);
            }
        }

        internal override void SerializeMailInfo(Microsoft.Office.Interop.Outlook.MailItem mailItem)
        {
            SerializeMailInfoCalls++;
            if (InvokeBaseSerializeMailInfo)
            {
                base.SerializeMailInfo(mailItem);
            }
        }

        internal override bool FileExists(string filePath)
        {
            return _storedTexts.ContainsKey(filePath);
        }

        internal override string ReadAllText(string filePath)
        {
            return _storedTexts[filePath];
        }

        internal override Task<string> ReadAllTextAsync(string filePath)
        {
            return Task.FromResult(_storedTexts[filePath]);
        }

        internal override void EnsureDirectoryExists(string folderPath) { }

        internal override TextWriter CreateTextWriter(string filePath)
        {
            return new CapturingStringWriter(text => _storedTexts[filePath] = text);
        }

        internal override (T Object, long Size) TryLoadObjectAndGetMemorySize<T>(
            Func<T> loader,
            int copiesToLoad = 1
        )
        {
            var nextObject = _loaderResults.Count > 0 ? _loaderResults.Dequeue() : default;
            var nextSize = _loaderSizes.Count > 0 ? _loaderSizes.Dequeue() : 0;
            return (nextObject is T typed ? typed : default, nextSize);
        }

        internal override Task<T> DeserializeAsync<T>(
            string fileNameSeed,
            string fileNameSuffix = ""
        )
        {
            if (InvokeBaseDeserializeAsync)
            {
                return base.DeserializeAsync<T>(fileNameSeed, fileNameSuffix);
            }

            if (ValidationException is not null)
            {
                return Task.FromException<T>(ValidationException);
            }

            return Task.FromResult(ValidationResult is T typed ? typed : default);
        }

        internal override void LogSizeComparison(
            string m1,
            long s1,
            string m2,
            long s2,
            string objectName
        )
        {
            LoggedObjectNames.Add(objectName);
        }

        private sealed class CapturingStringWriter(Action<string> onDispose)
            : StringWriter(new StringBuilder())
        {
            protected override void Dispose(bool disposing)
            {
                if (disposing)
                {
                    onDispose(ToString());
                }

                base.Dispose(disposing);
            }
        }
    }
}
