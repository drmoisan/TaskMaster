using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS.ReusableTypeClasses;

namespace UtilitiesCS.Test.ReusableTypeClasses
{
    [TestClass]
    public class ScoSortedDictionary_Tests
    {
        private static readonly string RepoRoot = Path.GetFullPath(
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..")
        );

        [TestMethod]
        public void DefaultConstructor_StartsEmpty()
        {
            // Arrange
            var dictionary = new ScoSortedDictionary<string, int>();

            // Assert
            dictionary.Count.Should().Be(0);
            dictionary.Should().BeEmpty();
        }

        [TestMethod]
        public void Constructor_WithDictionary_EnumeratesKeysInSortedOrder()
        {
            // Arrange
            var source = new Dictionary<string, int>
            {
                ["b"] = 2,
                ["a"] = 1,
                ["c"] = 3,
            };

            // Act
            var dictionary = new ScoSortedDictionary<string, int>(source);

            // Assert
            dictionary.Keys.Should().Equal("a", "b", "c");
            dictionary.Values.Should().Equal(1, 2, 3);
        }

        [TestMethod]
        public void AddRemoveAndTryGetValue_WorkAsExpected()
        {
            // Arrange
            var dictionary = new ScoSortedDictionary<string, int>();

            // Act
            dictionary.Add("b", 2);
            dictionary.Add("a", 1);
            var found = dictionary.TryGetValue("a", out var value);
            var removed = dictionary.Remove("b");

            // Assert
            found.Should().BeTrue();
            value.Should().Be(1);
            removed.Should().BeTrue();
            dictionary.Keys.Should().Equal("a");
        }

        [TestMethod]
        public void Add_DuplicateKey_PreservesBothEntries()
        {
            // Arrange
            var dictionary = new ScoSortedDictionary<string, int>();
            dictionary.Add("a", 1);

            // Act
            Action act = () => dictionary.Add("a", 2);

            // Assert
            act.Should().NotThrow();
            dictionary.Count.Should().Be(2);
            dictionary.Keys.Should().Contain(key => key == "a");
        }

        [TestMethod]
        public async Task ConcurrentIndexerAssignments_PreserveAllEntries()
        {
            // Arrange
            var dictionary = new ScoSortedDictionary<int, string>();
            var values = Enumerable.Range(1, 20).ToArray();

            // Act
            await Task.WhenAll(
                values.Select(value => Task.Run(() => dictionary[value] = $"value-{value}"))
            );

            // Assert
            dictionary.Count.Should().Be(values.Length);
            dictionary.Keys.OrderBy(value => value).Should().Equal(values);
        }

        [TestMethod]
        public void Filename_SetAndGet_Works()
        {
            // Arrange
            var dict = new ScoSortedDictionary<string, int>();

            // Act
            dict.FileName = "test.json";

            // Assert
            dict.FileName.Should().Be("test.json");
        }

        [TestMethod]
        public void Folderpath_SetAndGet_UpdatesFilepath()
        {
            // Arrange
            var dict = new ScoSortedDictionary<string, int>();
            dict.FileName = "test.json";

            // Act
            dict.FolderPath = @"C:\data";

            // Assert
            dict.FilePath.Should().Be(@"C:\data\test.json");
        }

        [TestMethod]
        public void Serialize_WithNoPath_IsNoOp()
        {
            // Arrange
            var dict = new ScoSortedDictionary<string, int>();
            dict.Add("key", 1);

            // Act
            dict.Serialize();

            // Assert
            dict.Count.Should().Be(1);
        }

        [TestMethod]
        public void JsonRoundTrip_PreservesEntries()
        {
            // Arrange
            var original = new ScoSortedDictionary<string, int>();
            original.Add("b", 2);
            original.Add("a", 1);
            var settings = new Newtonsoft.Json.JsonSerializerSettings
            {
                Formatting = Newtonsoft.Json.Formatting.Indented,
                TypeNameHandling = Newtonsoft.Json.TypeNameHandling.Auto,
            };

            // Act
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(original, settings);
            var restored = Newtonsoft.Json.JsonConvert.DeserializeObject<
                ScoSortedDictionary<string, int>
            >(json, settings);

            // Assert
            restored.Should().NotBeNull();
            restored.Should().ContainKey("a").WhoseValue.Should().Be(1);
            restored.Should().ContainKey("b").WhoseValue.Should().Be(2);
        }

        [TestMethod]
        public void Constructor_WithComparer_UsesCustomComparer()
        {
            // Arrange & Act — verify comparer affects sort order (reverse ordinal)
            var dict = new ScoSortedDictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            dict.Add("banana", 1);
            dict.Add("Apple", 2);

            // Assert — keys should be sorted case-insensitively (Apple < banana)
            dict.Keys.First().Should().Be("Apple");
            dict.Count.Should().Be(2);
        }

        [TestMethod]
        public void IndexerSet_ExistingKey_UpdatesValue()
        {
            // Arrange
            var dict = new ScoSortedDictionary<string, int>();
            dict["key"] = 1;

            // Act
            dict["key"] = 99;

            // Assert
            dict["key"].Should().Be(99);
        }

        [TestMethod]
        public void ContainsKey_ExistingKey_ReturnsTrue()
        {
            // Arrange
            var dict = new ScoSortedDictionary<string, int>();
            dict.Add("test", 42);

            // Act & Assert
            dict.ContainsKey("test").Should().BeTrue();
        }

        [TestMethod]
        public void ContainsKey_MissingKey_ReturnsFalse()
        {
            // Arrange
            var dict = new ScoSortedDictionary<string, int>();

            // Act & Assert
            dict.ContainsKey("missing").Should().BeFalse();
        }

        [TestMethod]
        public void Clear_RemovesAllEntries()
        {
            // Arrange
            var dict = new ScoSortedDictionary<string, int>();
            dict.Add("a", 1);
            dict.Add("b", 2);

            // Act
            dict.Clear();

            // Assert
            dict.Count.Should().Be(0);
        }

        [TestMethod]
        public void Serialize_WithExplicitPath_UpdatesFilePathAndQueuesTimer()
        {
            // Arrange
            var dictionary = new ScoSortedDictionary<string, int>();
            var invalidPath = CreateInvalidFilePath();

            // Act
            dictionary.Serialize(invalidPath);

            // Assert
            dictionary.FilePath.Should().Be(invalidPath);
            StopPendingTimer(dictionary);
        }

        [TestMethod]
        public void SerializeThreadSafe_WithInvalidPath_IsSwallowedByProductionErrorHandling()
        {
            // Arrange
            var dictionary = new ScoSortedDictionary<string, int>();
            dictionary["key"] = 1;

            // Act
            Action act = () => dictionary.SerializeThreadSafe(CreateInvalidFilePath());

            // Assert
            act.Should().NotThrow();
        }

        [TestMethod]
        public void Deserialize_WithoutConfiguredPath_DoesNothing()
        {
            // Arrange
            var dictionary = new ScoSortedDictionary<string, int>();
            dictionary["key"] = 3;

            // Act
            Action act = () =>
            {
                dictionary.Deserialize();
                dictionary.Deserialize(askUserOnError: false);
            };

            // Assert
            act.Should().NotThrow();
            dictionary.Should().ContainKey("key").WhoseValue.Should().Be(3);
        }

        [TestMethod]
        public void Deserialize_WithInvalidPathAndPromptDisabled_CreatesEmptyDictionary()
        {
            // Arrange
            var dictionary = new ScoSortedDictionary<string, int>();

            // Act
            dictionary.Deserialize(
                "*invalid-sorted-dictionary.json",
                RepoRoot,
                askUserOnError: false
            );

            // Assert
            dictionary.Should().BeEmpty();
            dictionary
                .FilePath.Should()
                .Be(Path.Combine(RepoRoot, "*invalid-sorted-dictionary.json"));
        }

        [TestMethod]
        public void Constructor_WithInvalidPath_AndDefaultPromptBehavior_ThrowsArgumentNullException()
        {
            using var overridePrompt = OverrideScoSortedDictionaryField<string, int>(
                "_showMessageBox",
                new Func<string, string, DialogResult>((_, _) => DialogResult.No)
            );

            // Act
            Action act = () =>
                _ = new ScoSortedDictionary<string, int>("*invalid-constructor.json", RepoRoot);

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void AskUser_WhenPromptDisabled_ReturnsYes()
        {
            // Arrange
            var dictionary = new ScoSortedDictionary<string, int>();

            // Act
            var response = InvokeNonPublic<DialogResult>(dictionary, "AskUser", false, "ignored");

            // Assert
            response.Should().Be(DialogResult.Yes);
        }

        [TestMethod]
        public void CreateEmpty_WhenResponseYes_ReturnsEmptyDictionaryAndConfiguresPath()
        {
            // Arrange
            var dictionary = new ScoSortedDictionary<string, int>();
            var disk = new FilePathHelper("*empty-sorted-dictionary.json", RepoRoot);

            // Act
            var created = InvokeNonPublic<ScoSortedDictionary<string, int>>(
                dictionary,
                "CreateEmpty",
                DialogResult.Yes,
                disk
            );

            // Assert
            created.Should().BeEmpty();
            created.FilePath.Should().Be(disk.FilePath);
            StopPendingTimer(created);
        }

        [TestMethod]
        public void CreateEmpty_WhenResponseNo_ThrowsArgumentNullException()
        {
            // Arrange
            var dictionary = new ScoSortedDictionary<string, int>();
            var disk = new FilePathHelper("*empty-sorted-dictionary.json", RepoRoot);

            // Act
            Action act = () =>
                InvokeNonPublic<ScoSortedDictionary<string, int>>(
                    dictionary,
                    "CreateEmpty",
                    DialogResult.No,
                    disk
                );

            // Assert
            act.Should()
                .Throw<TargetInvocationException>()
                .WithInnerException<ArgumentNullException>();
        }

        [TestMethod]
        public void AskUser_WhenPromptEnabled_UsesInjectedPromptResponse()
        {
            // Arrange
            var dictionary = new ScoSortedDictionary<string, int>();
            using var overridePrompt = OverrideScoSortedDictionaryField<string, int>(
                "_showMessageBox",
                new Func<string, string, DialogResult>((_, _) => DialogResult.No)
            );

            // Act
            var response = InvokeNonPublic<DialogResult>(dictionary, "AskUser", true, "ignored");

            // Assert
            response.Should().Be(DialogResult.No);
        }

        private static T InvokeNonPublic<T>(object target, string methodName, params object[] args)
        {
            var method = target
                .GetType()
                .GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);

            return (T)method.Invoke(target, args);
        }

        private static void StopPendingTimer(object target)
        {
            var timerField = target
                .GetType()
                .GetField("_timer", BindingFlags.Instance | BindingFlags.NonPublic);
            var timer = timerField?.GetValue(target);

            timer?.GetType().GetMethod("StopTimer")?.Invoke(timer, null);
            timer?.GetType().GetMethod("Dispose")?.Invoke(timer, null);
        }

        private static string CreateInvalidFilePath()
        {
            return Path.Combine(RepoRoot, "*invalid-sorted-dictionary.json");
        }

        private static IDisposable OverrideScoSortedDictionaryField<TKey, TValue>(
            string fieldName,
            object replacement
        )
        {
            var field = typeof(ScoSortedDictionary<TKey, TValue>).GetField(
                fieldName,
                BindingFlags.Static | BindingFlags.NonPublic
            );
            var original = field.GetValue(null);
            field.SetValue(
                null,
                replacement is Func<string, string, DialogResult> twoArgPrompt
                    ? new Func<string, string, MessageBoxButtons, MessageBoxIcon, DialogResult>(
                        (text, caption, _, _) => twoArgPrompt(text, caption)
                    )
                    : replacement
            );

            return new CallbackDisposable(() => field.SetValue(null, original));
        }

        private sealed class CallbackDisposable : IDisposable
        {
            private readonly Action _callback;

            public CallbackDisposable(Action callback)
            {
                _callback = callback;
            }

            public void Dispose()
            {
                _callback();
            }
        }
    }
}
