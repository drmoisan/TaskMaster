using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using UtilitiesCS.ReusableTypeClasses.Concurrent.Observable.Collection;

namespace UtilitiesCS.Test.ReusableTypeClasses.Concurrent.Observable.Collection
{
    /// <summary>
    /// Unit tests for the Swordfish-free <see cref="ConcurrentObservableCollection{T}"/>. Covers the
    /// list-search surface, indexer/mutation members, observer subscription, native
    /// CollectionChanged, list conversion, and the serialize/deserialize paths driven through an
    /// injected filesystem seam (no temp files).
    /// </summary>
    [TestClass]
    public class ConcurrentObservableCollection_Tests
    {
        private static readonly JsonSerializerSettings AutoSettings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto,
            Formatting = Formatting.Indented,
        };

        #region search surface

        [TestMethod]
        public void FindIndex_ReturnsIndexOfFirstMatch()
        {
            var sut = new ConcurrentObservableCollection<string> { "a", "bb", "ccc" };

            sut.FindIndex(x => x.Length == 2).Should().Be(1);
        }

        [TestMethod]
        public void FindIndex_NoMatch_ReturnsMinusOne()
        {
            var sut = new ConcurrentObservableCollection<int> { 1, 2, 3 };

            sut.FindIndex(x => x == 99).Should().Be(-1);
        }

        [TestMethod]
        public void FindIndex_WithStartIndex_SkipsEarlierMatches()
        {
            var sut = new ConcurrentObservableCollection<int> { 5, 5, 5 };

            sut.FindIndex(1, x => x == 5).Should().Be(1);
        }

        [TestMethod]
        public void FindIndex_WithStartIndexAndCount_RestrictsRange()
        {
            var sut = new ConcurrentObservableCollection<int> { 1, 2, 3, 4 };

            sut.FindIndex(0, 2, x => x == 3).Should().Be(-1);
        }

        [TestMethod]
        public void FindIndices_ReturnsAllMatchingIndices()
        {
            var sut = new ConcurrentObservableCollection<int> { 1, 0, 1, 0, 1 };

            sut.FindIndices(x => x == 1).Should().Equal(new[] { 0, 2, 4 });
        }

        [TestMethod]
        public void FindIndices_WithStartIndexAndCount_RestrictsRange()
        {
            var sut = new ConcurrentObservableCollection<int> { 1, 1, 1, 0 };

            sut.FindIndices(1, 2, x => x == 1).Should().Equal(new[] { 1, 2 });
        }

        [TestMethod]
        public void FindIndices_WithNonZeroStartIndexOnFullList_Throws()
        {
            // Preserves the legacy Swordfish FindIndices(startIndex, match) semantics, where the
            // two-argument overload uses Count (not Count - startIndex) as the span length, so a
            // non-zero start index on a full-length list overflows the range guard.
            var sut = new ConcurrentObservableCollection<int> { 1, 1, 1 };

            Action act = () => sut.FindIndices(1, x => x == 1);

            act.Should().Throw<ArgumentOutOfRangeException>();
        }

        [TestMethod]
        public void Find_ReturnsFirstMatch()
        {
            var sut = new ConcurrentObservableCollection<string> { "a", "bb", "cc" };

            sut.Find(x => x.Length == 2).Should().Be("bb");
        }

        [TestMethod]
        public void Find_NoMatch_ReturnsDefault()
        {
            var sut = new ConcurrentObservableCollection<string> { "a" };

            sut.Find(x => x == "z").Should().BeNull();
        }

        [TestMethod]
        public void Exists_TrueWhenMatchPresent_FalseOtherwise()
        {
            var sut = new ConcurrentObservableCollection<int> { 2, 4, 6 };

            sut.Exists(x => x == 4).Should().BeTrue();
            sut.Exists(x => x == 5).Should().BeFalse();
        }

        #endregion search surface

        #region indexer and mutation surface

        [TestMethod]
        public void Indexer_GetAndSet_Work()
        {
            var sut = new ConcurrentObservableCollection<string> { "a", "b" };

            sut[1].Should().Be("b");

            sut[1] = "z";
            sut[1].Should().Be("z");
        }

        [TestMethod]
        public void Add_Insert_RemoveAt_Remove_Contains_IndexOf_Count_Behave()
        {
            var sut = new ConcurrentObservableCollection<string>();

            sut.Add("a");
            sut.Insert(0, "z");
            sut.Should().Equal(new[] { "z", "a" });

            sut.IndexOf("a").Should().Be(1);
            sut.Contains("z").Should().BeTrue();
            sut.Count.Should().Be(2);

            sut.RemoveAt(0);
            sut.Should().Equal(new[] { "a" });

            sut.Remove("a").Should().BeTrue();
            sut.Count.Should().Be(0);
        }

        #endregion indexer and mutation surface

        #region observer and CollectionChanged

        [TestMethod]
        public void Subscribe_ReplaysExistingItems_AndForwardsSubsequentChanges()
        {
            var sut = new ConcurrentObservableCollection<string> { "existing" };
            var observer = new Mock<IObserver<NotifyCollectionChangedEventArgs>>();

            sut.Subscribe(observer.Object);

            // Replay of the one existing item on subscribe.
            observer.Verify(
                o =>
                    o.OnNext(
                        It.Is<NotifyCollectionChangedEventArgs>(e =>
                            e.Action == NotifyCollectionChangedAction.Add
                        )
                    ),
                Times.Once
            );

            sut.Add("new");

            // One more notification after the add.
            observer.Verify(
                o => o.OnNext(It.IsAny<NotifyCollectionChangedEventArgs>()),
                Times.Exactly(2)
            );
        }

        [TestMethod]
        public void Subscribe_NullObserver_Throws()
        {
            var sut = new ConcurrentObservableCollection<int>();

            Action act = () => sut.Subscribe(null);

            act.Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void Subscribe_Dispose_StopsFurtherNotifications()
        {
            var sut = new ConcurrentObservableCollection<int>();
            var observer = new Mock<IObserver<NotifyCollectionChangedEventArgs>>();

            var token = sut.Subscribe(observer.Object);
            token.Dispose();

            sut.Add(1);

            observer.Verify(
                o => o.OnNext(It.IsAny<NotifyCollectionChangedEventArgs>()),
                Times.Never
            );
        }

        [TestMethod]
        public void CollectionChanged_RaisedOnAdd_WithWrapperSender()
        {
            var sut = new ConcurrentObservableCollection<int>();
            object capturedSender = null;
            NotifyCollectionChangedAction? action = null;

            sut.CollectionChanged += (sender, e) =>
            {
                capturedSender = sender;
                action = e.Action;
            };

            sut.Add(7);

            capturedSender.Should().BeSameAs(sut);
            action.Should().Be(NotifyCollectionChangedAction.Add);
        }

        #endregion observer and CollectionChanged

        #region list conversion

        [TestMethod]
        public void ToList_ReturnsSnapshotCopy()
        {
            var sut = new ConcurrentObservableCollection<int> { 1, 2, 3 };

            var list = sut.ToList();
            sut.Add(4);

            list.Should().Equal(new[] { 1, 2, 3 });
        }

        [TestMethod]
        public void FromList_ReplacesContents()
        {
            var sut = new ConcurrentObservableCollection<int> { 9 };

            sut.FromList(new[] { 1, 2 });

            sut.Should().Equal(new[] { 1, 2 });
        }

        [TestMethod]
        public void FromList_Null_ClearsCollection()
        {
            var sut = new ConcurrentObservableCollection<int> { 1, 2 };

            sut.FromList(null);

            sut.Should().BeEmpty();
        }

        #endregion list conversion

        #region serialization via injected seam (no temp files)

        [TestMethod]
        public void SerializeThreadSafe_WritesBareJsonArray_ThroughInjectedFileSystemSeam()
        {
            using var stream = new MemoryStream();
            var fsMock = BuildWriteFileSystem(stream);

            using (SeamScope<string>.Install(fsMock.Object))
            {
                var sut = new ConcurrentObservableCollection<string> { "x", "y" };

                sut.SerializeThreadSafe(@"C:\any\path.json");
            }

            var json = Encoding.UTF8.GetString(stream.ToArray()).TrimStart('﻿').Trim();
            json.Should().StartWith("[");
            var roundTrip = JsonConvert.DeserializeObject<ConcurrentObservableCollection<string>>(
                json,
                AutoSettings
            );
            roundTrip.Should().Equal(new[] { "x", "y" });
        }

        [TestMethod]
        public async Task SerializeAsync_WithNoConfiguredPath_IsNoOp()
        {
            var sut = new ConcurrentObservableCollection<int> { 1 };

            await sut.SerializeAsync();

            sut.Should().Equal(new[] { 1 });
        }

        [TestMethod]
        public async Task SerializeAsync_WithExplicitPath_SetsFilePath()
        {
            var sut = new ConcurrentObservableCollection<int> { 1 };

            await sut.SerializeAsync(@"C:\any\file.json");

            sut.FilePath.Should().Be(@"C:\any\file.json");
        }

        [TestMethod]
        public void FileConstructor_DeserializesItems_ThroughInjectedFileSystemSeam()
        {
            var fsMock = new Mock<IConcurrentObservableCollectionFileSystem>(MockBehavior.Strict);
            fsMock.Setup(f => f.ReadAllText(It.IsAny<string>())).Returns("[\"a\",\"b\",\"c\"]");

            using (SeamScope<string>.Install(fsMock.Object))
            {
                var sut = new ConcurrentObservableCollection<string>(
                    "file.json",
                    @"C:\folder",
                    askUserOnError: false
                );

                sut.Should().Equal(new[] { "a", "b", "c" });
            }
        }

        #endregion serialization via injected seam

        #region bare-array serialization guardrail (P1-T5)

        [TestMethod]
        public void JsonSerialize_ProducesBareArray_AndRoundTripsElementOrder()
        {
            // Arrange
            var sut = new ConcurrentObservableCollection<string> { "one", "two", "three" };

            // Act
            var json = JsonConvert.SerializeObject(sut, AutoSettings);

            // Assert — no [JsonObject] wrapper: the on-disk shape is a bare JSON array.
            json.TrimStart().Should().StartWith("[");
            json.Should().NotContain("$type");

            var roundTrip = JsonConvert.DeserializeObject<ConcurrentObservableCollection<string>>(
                json,
                AutoSettings
            );
            roundTrip.Should().Equal(new[] { "one", "two", "three" });
        }

        #endregion bare-array serialization guardrail

        #region helpers

        private static Mock<IConcurrentObservableCollectionFileSystem> BuildWriteFileSystem(
            MemoryStream stream
        )
        {
            var fsMock = new Mock<IConcurrentObservableCollectionFileSystem>(MockBehavior.Strict);
            // AutoFlush ensures the serializer's writes reach the MemoryStream before Close;
            // MemoryStream.ToArray() remains readable after the writer closes it.
            fsMock
                .Setup(f => f.CreateText(It.IsAny<string>()))
                .Returns(() =>
                    new StreamWriter(stream, new UTF8Encoding(false)) { AutoFlush = true }
                );
            return fsMock;
        }

        /// <summary>
        /// Installs a filesystem seam on the static <see cref="ConcurrentObservableCollection{T}"/>
        /// hook and restores the original on dispose so tests stay independent.
        /// </summary>
        private sealed class SeamScope<T> : IDisposable
        {
            private readonly IConcurrentObservableCollectionFileSystem _originalFs;

            private SeamScope(IConcurrentObservableCollectionFileSystem fs)
            {
                _originalFs = ConcurrentObservableCollection<T>.FileSystem;
                ConcurrentObservableCollection<T>.FileSystem = fs;
            }

            public static SeamScope<T> Install(IConcurrentObservableCollectionFileSystem fs) =>
                new SeamScope<T>(fs);

            public void Dispose()
            {
                ConcurrentObservableCollection<T>.FileSystem = _originalFs;
            }
        }

        #endregion helpers
    }
}
