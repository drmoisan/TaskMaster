using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using UtilitiesCS.ReusableTypeClasses;

namespace UtilitiesCS.Test.ReusableTypeClasses
{
    public partial class SCODictionary_Tests
    {
        [TestMethod]
        public void Filepath_WhenExistingDirectoryPathProvided_ThrowsArgumentException()
        {
            var dict = new RecordingScoDictionary { DirectoryExistsResult = true };

            Action act = () => dict.Filepath = "directoryOnly";

            act.Should().Throw<ArgumentException>().WithMessage("*Folder Path*");
        }

        [TestMethod]
        public void Filename_WhenFolderpathAlreadySet_UpdatesFilepath()
        {
            var dict = new RecordingScoDictionary { Folderpath = @"C:\data" };

            dict.Filename = "items.json";

            dict.Filepath.Should().Be(@"C:\data\items.json");
        }

        [TestMethod]
        public void Serialize_WithExplicitPath_UpdatesFilepathAndWritesJson()
        {
            var dict = new RecordingScoDictionary();
            dict["alpha"] = 1;

            dict.Serialize("memory.json");

            SpinWait
                .SpinUntil(() => dict.HasWrittenPath("memory.json"), TimeSpan.FromSeconds(1))
                .Should()
                .BeTrue();
            dict.Filepath.Should().Be("memory.json");
            dict.ReadWrittenText("memory.json").Should().Contain("alpha");
        }

        [TestMethod]
        public async Task SerializeAsync_WithNoConfiguredPath_CompletesWithoutWriting()
        {
            var dict = new RecordingScoDictionary();
            dict["alpha"] = 1;

            await dict.SerializeAsync();

            dict.WrittenPathCount.Should().Be(0);
        }

        [TestMethod]
        public async Task SerializeAsync_WithExplicitPath_WritesJson()
        {
            var dict = new RecordingScoDictionary();
            dict["beta"] = 2;

            await dict.SerializeAsync("memory-async.json");

            dict.Filepath.Should().Be("memory-async.json");
            dict.ReadWrittenText("memory-async.json").Should().Contain("beta");
        }

        [TestMethod]
        public void SerializeThreadSafe_WhenWriterThrows_DoesNotThrow()
        {
            var dict = new RecordingScoDictionary { ThrowOnCreateText = true };
            dict["alpha"] = 1;

            Action act = () => dict.SerializeThreadSafe("broken.json");

            act.Should().NotThrow();
        }

        [TestMethod]
        public async Task WriteTextAsync_WritesUnicodeContentToAsyncStream()
        {
            var dict = new RecordingScoDictionary();

            await dict.InvokeWriteTextAsync("async.txt", "hello");

            dict.LastAsyncWritePath.Should().Be("async.txt");
            dict.ReadAsyncText().Should().Be("hello");
        }

        [TestMethod]
        public void Deserialize_WithValidJsonAndBackupLoaderOverload_LoadsEntriesWithoutFallback()
        {
            var dict = new RecordingScoDictionary();
            dict.StoreText(
                "valid.json",
                JsonConvert.SerializeObject(
                    new Dictionary<string, int> { ["alpha"] = 1, ["beta"] = 2 }
                )
            );
            var loaderCalled = false;

            dict.Deserialize(
                "valid.json",
                _ =>
                {
                    loaderCalled = true;
                    return new Dictionary<string, int> { ["fallback"] = 99 };
                },
                askUserOnError: false
            );

            loaderCalled.Should().BeFalse();
            dict.Should().ContainKey("alpha").WhoseValue.Should().Be(1);
            dict.Should().ContainKey("beta").WhoseValue.Should().Be(2);
        }

        [TestMethod]
        public void Deserialize_WhenMissingFile_UsesDerivedCsvPathAndSerializesFallback()
        {
            var dict = new RecordingScoDictionary();
            var observedPath = string.Empty;
            var primaryPath = @"C:\store\items.json";

            dict.Deserialize(
                primaryPath,
                path =>
                {
                    observedPath = path;
                    return new Dictionary<string, int> { ["gamma"] = 3 };
                },
                askUserOnError: false
            );

            observedPath.Should().Be(@"C:\store\items.csv");
            dict.Should().ContainKey("gamma").WhoseValue.Should().Be(3);
            SpinWait
                .SpinUntil(() => dict.HasWrittenPath(primaryPath), TimeSpan.FromSeconds(1))
                .Should()
                .BeTrue();
            dict.ReadWrittenText(primaryPath).Should().Contain("gamma");
        }

        [TestMethod]
        public void Deserialize_WhenInvalidJsonAndPromptDisabled_UsesBackupLoader()
        {
            var dict = new RecordingScoDictionary();
            var observedPath = string.Empty;
            dict.StoreText("broken.json", "{ not valid json }");

            dict.Deserialize(
                "broken.json",
                path =>
                {
                    observedPath = path;
                    return new Dictionary<string, int> { ["delta"] = 4 };
                },
                askUserOnError: false
            );

            observedPath.Should().Be("broken.csv");
            dict.Should().ContainKey("delta").WhoseValue.Should().Be(4);
            SpinWait
                .SpinUntil(() => dict.HasWrittenPath("broken.json"), TimeSpan.FromSeconds(1))
                .Should()
                .BeTrue();
            dict.ReadWrittenText("broken.json").Should().Contain("delta");
        }

        [TestMethod]
        public void Deserialize_WhenPromptDeclinedTwice_ThrowsArgumentNullException()
        {
            var dict = new RecordingScoDictionary();
            dict.QueueMessageResponse(DialogResult.No);
            dict.QueueMessageResponse(DialogResult.No);

            Action act = () =>
                dict.Deserialize(
                    "missing.json",
                    _ => new Dictionary<string, int> { ["unused"] = 1 },
                    askUserOnError: true
                );

            act.Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void Deserialize_WithValidJsonSimpleOverload_LoadsEntries()
        {
            var dict = new RecordingScoDictionary();
            dict.StoreText(
                "simple.json",
                JsonConvert.SerializeObject(new Dictionary<string, int> { ["zeta"] = 6 })
            );

            dict.Deserialize("simple.json", askUserOnError: false);

            dict.Should().ContainKey("zeta").WhoseValue.Should().Be(6);
        }

        [TestMethod]
        public void Deserialize_Overloads_WithConfiguredFilepath_LoadEntries()
        {
            var defaultPromptDict = new RecordingScoDictionary { Filepath = "configured.json" };
            defaultPromptDict.StoreText(
                "configured.json",
                JsonConvert.SerializeObject(new Dictionary<string, int> { ["one"] = 1 })
            );

            defaultPromptDict.Deserialize();

            defaultPromptDict.Should().ContainKey("one").WhoseValue.Should().Be(1);

            var promptDisabledDict = new RecordingScoDictionary
            {
                Filepath = "configured-no-ui.json",
            };
            promptDisabledDict.StoreText(
                "configured-no-ui.json",
                JsonConvert.SerializeObject(new Dictionary<string, int> { ["two"] = 2 })
            );

            promptDisabledDict.Deserialize(askUserOnError: false);

            promptDisabledDict.Should().ContainKey("two").WhoseValue.Should().Be(2);
        }

        [TestMethod]
        public void Deserialize_WhenInvalidJsonSimpleOverloadAndPromptDisabled_SerializesCurrentState()
        {
            var dict = new RecordingScoDictionary();
            dict.StoreText("simple-broken.json", "{ bad json }");

            dict.Deserialize("simple-broken.json", askUserOnError: false);

            dict.Should().BeEmpty();
            dict.HasWrittenPath("simple-broken.json").Should().BeTrue();
        }

        [TestMethod]
        public void Deserialize_WhenPromptDeclinedInSimpleOverload_ThrowsArgumentNullException()
        {
            var dict = new RecordingScoDictionary();
            dict.QueueMyBoxResponse(DialogResult.No);

            Action act = () => dict.Deserialize("missing-simple.json", askUserOnError: true);

            act.Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void ExplicitInterfaceToDictionary_ReturnsDictionaryCopy()
        {
            var dict = new RecordingScoDictionary();
            dict["alpha"] = 1;
            dict["beta"] = 2;

            var copy = ((IScoDictionary<string, int>)dict).ToDictionary();

            copy.Should().ContainKey("alpha").WhoseValue.Should().Be(1);
            copy.Should().ContainKey("beta").WhoseValue.Should().Be(2);
            copy.Should().NotBeSameAs(dict);
        }

        private sealed class RecordingScoDictionary : ScoDictionary<string, int>
        {
            private readonly Dictionary<string, string> _storedTexts = new(
                StringComparer.OrdinalIgnoreCase
            );
            private readonly Queue<DialogResult> _messageResponses = new();
            private readonly Queue<DialogResult> _myBoxResponses = new();
            private readonly Dictionary<string, string> _writtenTexts = new(
                StringComparer.OrdinalIgnoreCase
            );
            private MemoryStream _asyncWriteStream;

            internal bool DirectoryExistsResult { get; set; }

            internal bool ThrowOnCreateText { get; set; }

            internal string LastAsyncWritePath { get; private set; }

            internal int WrittenPathCount => _writtenTexts.Count;

            internal void StoreText(string path, string text) => _storedTexts[path] = text;

            internal void QueueMessageResponse(DialogResult result) =>
                _messageResponses.Enqueue(result);

            internal void QueueMyBoxResponse(DialogResult result) =>
                _myBoxResponses.Enqueue(result);

            internal bool HasWrittenPath(string path) => _writtenTexts.ContainsKey(path);

            internal string ReadWrittenText(string path) => _writtenTexts[path];

            internal string ReadAsyncText() =>
                _asyncWriteStream is null
                    ? string.Empty
                    : Encoding.Unicode.GetString(_asyncWriteStream.ToArray());

            internal Task InvokeWriteTextAsync(string path, string text) =>
                WriteTextAsync(path, text);

            protected override bool DirectoryExists(string path) => DirectoryExistsResult;

            protected override string ReadAllText(string path, Encoding encoding)
            {
                if (_storedTexts.TryGetValue(path, out var text))
                {
                    return text;
                }

                throw new FileNotFoundException("missing", path);
            }

            protected override TextWriter CreateText(string path)
            {
                if (ThrowOnCreateText)
                {
                    throw new IOException("simulated create failure");
                }

                return new CapturingStringWriter(text => _writtenTexts[path] = text);
            }

            protected override Stream CreateAsyncWriteStream(string path)
            {
                LastAsyncWritePath = path;
                _asyncWriteStream = new MemoryStream();
                return _asyncWriteStream;
            }

            protected override DialogResult ShowMessageBox(
                string text,
                string caption,
                MessageBoxButtons buttons,
                MessageBoxIcon icon
            ) => _messageResponses.Count > 0 ? _messageResponses.Dequeue() : DialogResult.Yes;

            protected override DialogResult ShowMyBoxDialog(
                string text,
                string caption,
                MessageBoxButtons buttons,
                MessageBoxIcon icon
            ) => _myBoxResponses.Count > 0 ? _myBoxResponses.Dequeue() : DialogResult.Yes;

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
}
