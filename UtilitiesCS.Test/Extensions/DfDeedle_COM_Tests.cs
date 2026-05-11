using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Deedle;
using FluentAssertions;
using Microsoft.Office.Interop.Outlook;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UtilitiesCS;
using Outlook = Microsoft.Office.Interop.Outlook;
using WinForms = System.Windows.Forms;

namespace UtilitiesCS.Test.Extensions
{
    /// <summary>
    /// COM-mocked and Deedle-extension tests for <see cref="DfDeedle"/>.
    ///
    /// Purpose:
    ///     Covers the COM-interface paths (HasUserDefinedProperty, EnsureTriageColumnExists,
    ///     AddQfcColumns), the testability-seam paths (GetEmailDataFromTable,
    ///     GetEmailDataInView via TableEtlInvoker, AddQfcColumnsAsync), the storage-folder
    ///     factory paths (FromDefaultFolder), and the pure Deedle extension methods
    ///     (PrintToLog, DropFirstN, Exclude, GetDuplicateEntriesByColumn).
    ///
    /// Usage:
    ///     Every test that modifies a static seam saves/restores the original value to
    ///     prevent side effects across the test run.
    /// </summary>
    [TestClass]
    public class DfDeedle_COM_Tests
    {
        // ----------------------------------------------------------------
        // Shared helpers
        // ----------------------------------------------------------------

        /// <summary>
        /// Builds a <see cref="Mock{MAPIFolder}"/> that reports one user-defined property
        /// named <paramref name="udpName"/>, so that <c>HasUserDefinedProperty</c> returns
        /// <c>true</c> for that name without a live COM session.
        /// </summary>
        private static Mock<MAPIFolder> BuildFolderWithUdp(string udpName)
        {
            var mockUDP = new Mock<UserDefinedProperty>(MockBehavior.Loose);
            mockUDP.SetupGet(p => p.Name).Returns(udpName);

            // Make the UserDefinedProperties collection enumerate a single entry;
            // the foreach in HasUserDefinedProperty relies on GetEnumerator.
            var udpList = new List<UserDefinedProperty> { mockUDP.Object };
            var mockUDPs = new Mock<UserDefinedProperties>(MockBehavior.Loose);
            mockUDPs.Setup(u => u.GetEnumerator()).Returns(udpList.GetEnumerator());

            var mockFolder = new Mock<MAPIFolder>(MockBehavior.Loose);
            mockFolder.SetupGet(f => f.UserDefinedProperties).Returns(mockUDPs.Object);
            return mockFolder;
        }

        /// <summary>
        /// Builds a <see cref="Mock{MAPIFolder}"/> whose UserDefinedProperties collection
        /// is empty so that <c>HasUserDefinedProperty</c> returns <c>false</c>.
        /// </summary>
        private static Mock<MAPIFolder> BuildFolderWithNoUdps()
        {
            var emptyList = new List<UserDefinedProperty>();
            var mockUDPs = new Mock<UserDefinedProperties>(MockBehavior.Loose);
            mockUDPs.Setup(u => u.GetEnumerator()).Returns(emptyList.GetEnumerator());

            var mockFolder = new Mock<MAPIFolder>(MockBehavior.Loose);
            mockFolder.SetupGet(f => f.UserDefinedProperties).Returns(mockUDPs.Object);
            return mockFolder;
        }

        /// <summary>
        /// Builds a <see cref="Mock{Table}"/> with a fully stubbed <see cref="Columns"/>
        /// so that column Add and Remove calls succeed as no-ops.
        /// </summary>
        private static (Mock<Table>, Mock<Columns>) BuildTableMock()
        {
            var mockColumns = new Mock<Columns>(MockBehavior.Loose);
            var mockTable = new Mock<Table>(MockBehavior.Loose);
            mockTable.SetupGet(t => t.Columns).Returns(mockColumns.Object);
            return (mockTable, mockColumns);
        }

        // ----------------------------------------------------------------
        // HasUserDefinedProperty (private via reflection)
        // ----------------------------------------------------------------

        private static MethodInfo GetHasUserDefinedPropertyMethod() =>
            typeof(DfDeedle).GetMethod(
                "HasUserDefinedProperty",
                BindingFlags.NonPublic | BindingFlags.Static
            );

        [TestMethod]
        public void HasUserDefinedProperty_NullFolder_ReturnsFalse()
        {
            // Arrange: null folder short-circuits at the guard expression.
            var method = GetHasUserDefinedPropertyMethod();

            // Act
            var result = (bool)method!.Invoke(null, new object[] { null!, "Triage" });

            // Assert
            result.Should().BeFalse();
        }

        [TestMethod]
        public void HasUserDefinedProperty_WhitespaceName_ReturnsFalse()
        {
            // Arrange: blank property name is treated as missing → same guard fires.
            var method = GetHasUserDefinedPropertyMethod();
            var folder = BuildFolderWithNoUdps();

            // Act
            var result = (bool)method!.Invoke(null, new object[] { folder.Object, "   " });

            // Assert
            result.Should().BeFalse();
        }

        [TestMethod]
        public void HasUserDefinedProperty_MatchingProperty_ReturnsTrue()
        {
            // Arrange: a folder that has exactly one UDP named "Triage".
            var method = GetHasUserDefinedPropertyMethod();
            var folder = BuildFolderWithUdp("Triage");

            // Act
            var result = (bool)method!.Invoke(null, new object[] { folder.Object, "Triage" });

            // Assert
            result.Should().BeTrue();
        }

        [TestMethod]
        public void HasUserDefinedProperty_NoMatchingProperty_ReturnsFalse()
        {
            // Arrange: a folder whose UDPs don't contain the requested name.
            var method = GetHasUserDefinedPropertyMethod();
            var folder = BuildFolderWithNoUdps();

            // Act
            var result = (bool)method!.Invoke(null, new object[] { folder.Object, "Triage" });

            // Assert: the loop completes without finding the property.
            result.Should().BeFalse();
        }

        // ----------------------------------------------------------------
        // EnsureTriageColumnExists (private via reflection)
        // ----------------------------------------------------------------

        private static MethodInfo GetEnsureTriageColumnExistsMethod() =>
            typeof(DfDeedle).GetMethod(
                "EnsureTriageColumnExists",
                BindingFlags.NonPublic | BindingFlags.Static
            );

        [TestMethod]
        public void EnsureTriageColumnExists_NullFolder_ReturnsFalse()
        {
            // Arrange: null folder → immediate false; no COM or dialog needed.
            var method = GetEnsureTriageColumnExistsMethod();

            // Act
            var result = (bool)method!.Invoke(null, new object[] { null! });

            // Assert
            result.Should().BeFalse();
        }

        [TestMethod]
        public void EnsureTriageColumnExists_TriagePropertyAlreadyExists_ReturnsTrue()
        {
            // Arrange: the folder already has "Triage" → no dialog, returns true immediately.
            var method = GetEnsureTriageColumnExistsMethod();
            var folder = BuildFolderWithUdp("Triage");

            // Act
            var result = (bool)method!.Invoke(null, new object[] { folder.Object });

            // Assert
            result.Should().BeTrue();
        }

        [TestMethod]
        public void EnsureTriageColumnExists_UserDeclinesCreate_ReturnsFalse()
        {
            // Arrange: folder has no Triage UDP; user click "No" in the dialog.
            var method = GetEnsureTriageColumnExistsMethod();
            var folder = BuildFolderWithNoUdps();
            var original = DfDeedle.MessageBoxInvoker;
            DfDeedle.MessageBoxInvoker = (_, __, ___, ____) => WinForms.DialogResult.No;

            try
            {
                // Act
                var result = (bool)method!.Invoke(null, new object[] { folder.Object });

                // Assert: user declined, so the column was not created.
                result.Should().BeFalse();
            }
            finally
            {
                DfDeedle.MessageBoxInvoker = original;
            }
        }

        [TestMethod]
        public void EnsureTriageColumnExists_UserAcceptsCreate_ReturnsTrue()
        {
            // Arrange: folder has no Triage UDP; user clicks "Yes"; Add succeeds.
            var method = GetEnsureTriageColumnExistsMethod();
            var folder = BuildFolderWithNoUdps();
            // Setup: Accept the creation dialog and allow UserDefinedProperties.Add to succeed.
            folder.Object.UserDefinedProperties.As<object>(); // ensure non-null return
            var original = DfDeedle.MessageBoxInvoker;
            DfDeedle.MessageBoxInvoker = (_, __, ___, ____) => WinForms.DialogResult.Yes;

            try
            {
                // Act: Loose mock means Add is a no-op; the method should return true.
                var result = (bool)method!.Invoke(null, new object[] { folder.Object });

                // Assert
                result.Should().BeTrue();
            }
            finally
            {
                DfDeedle.MessageBoxInvoker = original;
            }
        }

        [TestMethod]
        public void EnsureTriageColumnExists_CreateThrows_ReturnsFalse()
        {
            // Arrange: folder has no Triage UDP; user clicks "Yes"; Add throws.
            var method = GetEnsureTriageColumnExistsMethod();
            var mockUDPs = new Mock<UserDefinedProperties>(MockBehavior.Loose);
            var emptyList = new List<UserDefinedProperty>();
            mockUDPs.Setup(u => u.GetEnumerator()).Returns(emptyList.GetEnumerator());
            mockUDPs
                .Setup(u =>
                    u.Add(
                        It.IsAny<string>(),
                        It.IsAny<OlUserPropertyType>(),
                        It.IsAny<object>(),
                        It.IsAny<object>()
                    )
                )
                .Throws(new InvalidOperationException("COM error"));

            var mockFolder = new Mock<MAPIFolder>(MockBehavior.Loose);
            mockFolder.SetupGet(f => f.UserDefinedProperties).Returns(mockUDPs.Object);

            var original = DfDeedle.MessageBoxInvoker;
            // First call (ask to create) returns Yes; second call (error notice) returns OK.
            int callCount = 0;
            DfDeedle.MessageBoxInvoker = (_, __, ___, ____) =>
            {
                callCount++;
                return callCount == 1 ? WinForms.DialogResult.Yes : WinForms.DialogResult.OK;
            };

            try
            {
                // Act
                var result = (bool)method!.Invoke(null, new object[] { mockFolder.Object });

                // Assert: Add threw, so the column was not created and method returns false.
                result.Should().BeFalse();
            }
            finally
            {
                DfDeedle.MessageBoxInvoker = original;
            }
        }

        // ----------------------------------------------------------------
        // AddQfcColumns (private via reflection)
        // ----------------------------------------------------------------

        private static MethodInfo GetAddQfcColumnsMethod() =>
            typeof(DfDeedle).GetMethod(
                "AddQfcColumns",
                BindingFlags.NonPublic | BindingFlags.Static
            );

        [TestMethod]
        public void AddQfcColumns_TriageExists_AddsAndRemovesExpectedColumns()
        {
            // Arrange: a table mock that records column operations, and a folder
            // with a Triage UDP so EnsureTriageColumnExists returns true.
            var method = GetAddQfcColumnsMethod();
            var (mockTable, mockColumns) = BuildTableMock();
            var folder = BuildFolderWithUdp("Triage");

            // Act: should complete without throwing.
            method!.Invoke(null, new object[] { mockTable.Object, folder.Object });

            // Assert: the expected columns were requested to be added and removed.
            mockColumns.Verify(c => c.Add("SentOn"), Times.Once);
            mockColumns.Verify(c => c.Remove("Subject"), Times.Once);
        }

        [TestMethod]
        public void AddQfcColumns_TriageMissing_ThrowsInvalidOperationException()
        {
            // Arrange: null folder → EnsureTriageColumnExists returns false → error path fires.
            var method = GetAddQfcColumnsMethod();
            var (mockTable, _) = BuildTableMock();
            var original = DfDeedle.MessageBoxInvoker;
            DfDeedle.MessageBoxInvoker = (_, __, ___, ____) => WinForms.DialogResult.OK;

            try
            {
                // Act: must throw because the required column doesn't exist.
                System.Action act = () =>
                    method!.Invoke(null, new object[] { mockTable.Object, null! });

                // Assert: TargetInvocationException wraps the real InvalidOperationException.
                act.Should()
                    .Throw<TargetInvocationException>()
                    .WithInnerException<InvalidOperationException>();
            }
            finally
            {
                DfDeedle.MessageBoxInvoker = original;
            }
        }

        // ----------------------------------------------------------------
        // GetEmailDataFromTable (internal)
        // ----------------------------------------------------------------

        [TestMethod]
        public void GetEmailDataFromTable_OneRow_ReturnsFrameWithExpectedFields()
        {
            // Arrange: a single email row with all required fields as object values.
            object[,] data =
            {
                { "id-1", "IPM.Note", "2024-06-01", "conv-A", "B", "store-X" },
            };
            var columnInfo = new Dictionary<string, int>
            {
                ["EntryID"] = 0,
                ["MessageClass"] = 1,
                ["SentOn"] = 2,
                ["ConversationId"] = 3,
                ["Triage"] = 4,
            };

            // Act
            Frame<int, string> df = DfDeedle.GetEmailDataFromTable("store-X", data, columnInfo);

            // Assert
            df.Should().NotBeNull();
            df.RowCount.Should().Be(1);
        }

        // ----------------------------------------------------------------
        // GetEmailDataInView (using TableEtlInvoker seam)
        // ----------------------------------------------------------------

        [TestMethod]
        public void GetEmailDataInView_WithInjectedEtlResult_ReturnsPopulatedFrame()
        {
            // Arrange: inject pre-built ETL data so no live Outlook Table is needed;
            // mock Explorer to supply a Table reference for the AddQfcColumns call.
            object[,] injectedData =
            {
                { "id-1", "IPM.Note", "2024-01-01", "conv-1", "A", "store-1" },
            };
            var injectedColInfo = new Dictionary<string, int>
            {
                ["EntryID"] = 0,
                ["MessageClass"] = 1,
                ["SentOn"] = 2,
                ["ConversationId"] = 3,
                ["Triage"] = 4,
            };

            var folderWithTriage = BuildFolderWithUdp("Triage");
            folderWithTriage.SetupGet(f => f.StoreID).Returns("store-1");

            var (mockTable, _) = BuildTableMock();
            var mockTableView = new Mock<TableView>(MockBehavior.Loose);
            mockTableView.Setup(tv => tv.GetTable()).Returns(mockTable.Object);

            var mockExplorer = new Mock<Outlook.Explorer>(MockBehavior.Loose);
            mockExplorer.SetupGet(e => e.CurrentView).Returns(mockTableView.Object);
            mockExplorer.SetupGet(e => e.CurrentFolder).Returns(folderWithTriage.Object);

            var originalEtl = DfDeedle.TableEtlInvoker;
            DfDeedle.TableEtlInvoker = _ => (injectedData, injectedColInfo);

            try
            {
                // Act
                Frame<int, string> df = DfDeedle.GetEmailDataInView(mockExplorer.Object);

                // Assert
                df.Should().NotBeNull();
                df.RowCount.Should().Be(1);
            }
            finally
            {
                DfDeedle.TableEtlInvoker = originalEtl;
            }
        }

        /// <summary>
        /// Locks in the follow-up async-boundary contract for dataframe loading from the
        /// current Outlook table view. `GetEmailDataInViewAsync` should capture the table
        /// snapshot before it starts the background dataframe transform.
        /// </summary>
        [TestMethod]
        public async Task GetEmailDataInViewAsync_SeparatesTableSnapshotFromDataFrameTransform()
        {
            var data = new object[,]
            {
                { "entry-1", "IPM.Note", "2024-01-01", "conv-1", "A" },
            };
            var row = new Mock<Row>(MockBehavior.Strict);
            row.Setup(x => x.GetValues())
                .Returns(new object[] { "entry-1", "IPM.Note", "2024-01-01", "conv-raw", "A" });
            row.Setup(x => x.BinaryToString(4)).Returns("conv-1");

            var folder = BuildFolderWithUdp("Triage");
            folder.SetupGet(x => x.StoreID).Returns("store-1");
            folder.SetupGet(x => x.Name).Returns("Inbox");

            var mockColumns = new Mock<Columns>(MockBehavior.Loose);
            var mockTable = new Mock<Table>(MockBehavior.Strict);
            mockTable.SetupGet(x => x.Columns).Returns(mockColumns.Object);
            mockTable.Setup(x => x.MoveToStart());
            mockTable.Setup(x => x.GetRowCount()).Returns(1);
            var currentRow = 0;
            mockTable.Setup(x => x.EndOfTable).Returns(() => currentRow >= 1);
            mockTable
                .Setup(x => x.GetNextRow())
                .Returns(() =>
                {
                    currentRow++;
                    return row.Object;
                });

            var columnNames = new[]
            {
                "EntryID",
                "MessageClass",
                "SentOn",
                "ConversationId",
                "Triage",
            };
            mockColumns.Setup(x => x.Count).Returns(columnNames.Length);
            for (var index = 0; index < columnNames.Length; index++)
            {
                var column = new Mock<Column>(MockBehavior.Strict);
                column.SetupGet(x => x.Name).Returns(columnNames[index]);
                mockColumns.Setup(x => x[index + 1]).Returns(column.Object);
            }

            var tableView = new Mock<TableView>(MockBehavior.Strict);
            tableView.Setup(x => x.GetTable()).Returns(mockTable.Object);

            var explorer = new Mock<Outlook.Explorer>(MockBehavior.Strict);
            explorer.SetupGet(x => x.CurrentView).Returns(tableView.Object);
            explorer.SetupGet(x => x.CurrentFolder).Returns(folder.Object);

            var progress = CreateProgressTracker();
            var result = await DfDeedle.GetEmailDataInViewAsync(
                explorer.Object,
                CancellationToken.None,
                new CancellationTokenSource(),
                progress
            );

            result.RowCount.Should().Be(1);
            result
                .ColumnKeys.Should()
                .Contain(new[] { "EntryId", "MessageClass", "ConversationId" });
            mockTable.Verify(x => x.GetNextRow(), Times.Once);
        }

        // ----------------------------------------------------------------
        // AddQfcColumnsAsync (private via reflection)
        // ----------------------------------------------------------------

        private static MethodInfo GetAddQfcColumnsAsyncMethod() =>
            typeof(DfDeedle).GetMethod(
                "AddQfcColumnsAsync",
                BindingFlags.NonPublic | BindingFlags.Static
            );

        [TestMethod]
        public void AddQfcColumnsAsync_HappyPath_CompletesWithoutThrowing()
        {
            // Arrange: table + folder arranged so AddQfcColumns succeeds.
            var method = GetAddQfcColumnsAsyncMethod();
            var (mockTable, _) = BuildTableMock();
            var folder = BuildFolderWithUdp("Triage");
            var cts = new CancellationTokenSource();

            // Act: call the async private method and await the returned Task.
            var task = (Task)
                method!.Invoke(
                    null,
                    new object[] { mockTable.Object, folder.Object, cts.Token, 0 }
                );
            System.Action act = () => task.GetAwaiter().GetResult();

            // Assert: completes without exception.
            act.Should().NotThrow();
        }

        [TestMethod]
        public void AddQfcColumnsAsync_PreCancelledToken_CompletesGracefully()
        {
            // Arrange: a pre-cancelled token causes the inner Task.Run to be cancelled
            // immediately; the method's catch block must handle it without re-throwing.
            var method = GetAddQfcColumnsAsyncMethod();
            var (mockTable, _) = BuildTableMock();
            var folder = BuildFolderWithUdp("Triage");
            var cts = new CancellationTokenSource();
            cts.Cancel();

            // Act
            var task = (Task)
                method!.Invoke(
                    null,
                    new object[] { mockTable.Object, folder.Object, cts.Token, 0 }
                );

            // Assert: must not propagate the cancellation as an unhandled exception.
            System.Action act = () => task.GetAwaiter().GetResult();
            act.Should().NotThrow();
        }

        // ----------------------------------------------------------------
        // FromDefaultFolder(Store) — null-table branch
        // ----------------------------------------------------------------

        [TestMethod]
        public void FromDefaultFolder_Store_WhenGetTableReturnsNull_ReturnsNull()
        {
            // Arrange: a Store whose GetDefaultFolder throws so the inner GetTable call
            // is safely short-circuited and the outer FromDefaultFolder sees null.
            var mockStore = new Mock<Outlook.Store>(MockBehavior.Loose);
            mockStore
                .Setup(s => s.GetDefaultFolder(It.IsAny<OlDefaultFolders>()))
                .Throws<InvalidOperationException>();

            // Act
            var result = DfDeedle.FromDefaultFolder(
                mockStore.Object,
                OlDefaultFolders.olFolderInbox,
                removeColumns: null,
                addColumns: null
            );

            // Assert: null table → null frame returned.
            result.Should().BeNull();
        }

        // ----------------------------------------------------------------
        // FromDefaultFolder(Stores) — empty and single-store branches
        // ----------------------------------------------------------------

        [TestMethod]
        public void FromDefaultFolder_EmptyStores_ReturnsEmptyFrame()
        {
            // Arrange: a Stores collection with no entries; loop body never executes.
            var mockStores = new Mock<Outlook.Stores>(MockBehavior.Loose);
            mockStores
                .As<IEnumerable>()
                .Setup(e => e.GetEnumerator())
                .Returns(new List<Outlook.Store>().GetEnumerator());

            // Act
            Frame<int, string> result = DfDeedle.FromDefaultFolder(
                mockStores.Object,
                OlDefaultFolders.olFolderInbox,
                removeColumns: null,
                addColumns: null
            );

            // Assert: the empty-frame fallback is returned.
            result.Should().NotBeNull();
            result.RowCount.Should().Be(0);
        }

        [TestMethod]
        public void FromDefaultFolder_StoresWithOneStoreThatHasNoData_ReturnsEmptyFrame()
        {
            // Arrange: one store that throws during GetDefaultFolder; FromDefaultFolder(Store)
            // returns null → the loop's continue-on-null guard fires.
            var mockStore = new Mock<Outlook.Store>(MockBehavior.Loose);
            mockStore
                .Setup(s => s.GetDefaultFolder(It.IsAny<OlDefaultFolders>()))
                .Throws<InvalidOperationException>();

            var mockStores = new Mock<Outlook.Stores>(MockBehavior.Loose);
            mockStores
                .As<IEnumerable>()
                .Setup(e => e.GetEnumerator())
                .Returns(new List<Outlook.Store> { mockStore.Object }.GetEnumerator());

            // Act
            Frame<int, string> result = DfDeedle.FromDefaultFolder(
                mockStores.Object,
                OlDefaultFolders.olFolderInbox,
                removeColumns: null,
                addColumns: null
            );

            // Assert: all stores produced null frames → same empty fallback returned.
            result.Should().NotBeNull();
            result.RowCount.Should().Be(0);
        }

        // ----------------------------------------------------------------
        // Deedle extension methods (pure functional)
        // ----------------------------------------------------------------

        [TestMethod]
        public void PrintToLog_WithPopulatedFrame_LogsWithoutThrowing()
        {
            // Arrange: use a simple integer-keyed, string-columned frame; mock the logger.
            var logger = new Mock<log4net.ILog>(MockBehavior.Loose);
            var data = new Dictionary<string, int> { ["A"] = 0, ["B"] = 1 };
            var dataArr = new object[,]
            {
                { "x", "y" },
            };
            Frame<int, string> df = DfDeedle.FromArray2D(dataArr, data);

            // Act: should not throw and should invoke logger.Debug at least once.
            System.Action act = () => df.PrintToLog(logger.Object);

            // Assert
            act.Should().NotThrow();
            logger.Verify(l => l.Debug(It.IsAny<object>()), Times.AtLeastOnce);
        }

        [TestMethod]
        public void DropFirstN_DropsFirstNRows()
        {
            // Arrange: a 3-row frame built from a 2-D array; dropping 2 rows leaves 1.
            var colInfo = new Dictionary<string, int> { ["Val"] = 0 };
            var data = new object[,]
            {
                { "a" },
                { "b" },
                { "c" },
            };
            var df = DfDeedle.FromArray2D(data, colInfo);

            // Act
            var result = df.DropFirstN(2);

            // Assert
            result.RowCount.Should().Be(1);
        }

        [TestMethod]
        public void Exclude_EmptyOtherFrame_ReturnsSameRowCount()
        {
            // Arrange: a 2-row base frame; empty other → nothing excluded.
            var colInfo = new Dictionary<string, int> { ["Val"] = 0 };
            var data = new object[,]
            {
                { "a" },
                { "b" },
            };
            var df = DfDeedle.FromArray2D(data, colInfo);
            var emptyOther = DfDeedle.FromArray2D(new object[0, 1], colInfo);

            // Act
            var result = df.Exclude(emptyOther);

            // Assert: nothing excluded.
            result.RowCount.Should().Be(2);
        }

        [TestMethod]
        public void Exclude_NonEmptyOtherFrame_RemovesMatchingRows()
        {
            // Arrange: 3-row base frame; other contains row with key 0 → row 0 is removed.
            var colInfo = new Dictionary<string, int> { ["Val"] = 0 };
            var data = new object[,]
            {
                { "a" },
                { "b" },
                { "c" },
            };
            var df = DfDeedle.FromArray2D(data, colInfo);
            // Create a 1-row "other" frame from the same 2-D data (row key 0).
            var otherData = new object[,]
            {
                { "a" },
            };
            var other = DfDeedle.FromArray2D(otherData, colInfo);

            // Act: row 0 in df shares key 0 with other → it gets excluded.
            var result = df.Exclude(other);

            // Assert: 2 rows remain after excluding row 0.
            result.RowCount.Should().Be(2);
        }

        [TestMethod]
        public void GetDuplicateEntriesByColumn_ReturnsDuplicateValues()
        {
            // Arrange: a 3-row frame where column 0 has values [a, b, a]; "a" is the duplicate.
            var data = new object[,]
            {
                { "a" },
                { "b" },
                { "a" },
            };
            var colInfo = new Dictionary<string, int> { ["Key"] = 0 };
            Frame<int, string> df = DfDeedle.FromArray2D(data, colInfo);

            // Act
            string[] dups = df.GetDuplicateEntriesByColumn<int, string, string>("Key");

            // Assert: "a" appears twice, so it should be in the duplicates array.
            dups.Should().ContainSingle().Which.Should().Be("a");
        }

        // ----------------------------------------------------------------
        // FromDefaultFolder(Store) — non-null table path (StoreTableEtlInvoker seam)
        // ----------------------------------------------------------------

        [TestMethod]
        public void FromDefaultFolder_Store_WithInjectedEtlResult_ReturnsPopulatedFrame()
        {
            // Arrange: mock Store.GetDefaultFolder to return a mock MAPIFolder,
            // and mock folder.GetTable() (the COM method on MAPIFolder) to return a mock Table.
            // GetTable(Store,...) is a static extension method that Moq cannot intercept directly;
            // the correct seam is the underlying COM interface chain.
            // StoreTableEtlInvoker is replaced to supply known data without live COM calls.
            var mockColumns = new Mock<Outlook.Columns>(MockBehavior.Loose);
            var mockTable = new Mock<Outlook.Table>(MockBehavior.Loose);
            mockTable.SetupGet(t => t.Columns).Returns(mockColumns.Object);
            var mockFolder = new Mock<Outlook.MAPIFolder>(MockBehavior.Loose);
            mockFolder
                .Setup(f => f.GetTable(It.IsAny<object>(), It.IsAny<object>()))
                .Returns(mockTable.Object);
            var mockStore = new Mock<Outlook.Store>(MockBehavior.Loose);
            mockStore
                .Setup(s => s.GetDefaultFolder(It.IsAny<OlDefaultFolders>()))
                .Returns(mockFolder.Object);

            var injectedData = new object[,]
            {
                { "entryId-1", "IPM.Note", "2024-01-01", "conv-1", "A", "store-1" },
            };
            var injectedColInfo = new Dictionary<string, int>
            {
                ["EntryID"] = 0,
                ["MessageClass"] = 1,
                ["SentOn"] = 2,
                ["ConversationId"] = 3,
                ["Triage"] = 4,
                ["StoreId"] = 5,
            };

            var originalSeam = DfDeedle.StoreTableEtlInvoker;
            DfDeedle.StoreTableEtlInvoker = _ => (injectedData, injectedColInfo);

            try
            {
                // Act
                Frame<int, string> df = DfDeedle.FromDefaultFolder(
                    mockStore.Object,
                    OlDefaultFolders.olFolderInbox,
                    Array.Empty<string>(),
                    Array.Empty<string>()
                );

                // Assert: frame is non-null and has data from the injected ETL result.
                df.Should().NotBeNull();
                df.RowCount.Should().Be(1);
            }
            finally
            {
                DfDeedle.StoreTableEtlInvoker = originalSeam;
            }
        }

        // ----------------------------------------------------------------
        // FromDefaultFolder(Stores) — store with non-null data path
        // ----------------------------------------------------------------

        [TestMethod]
        public void FromDefaultFolder_Stores_FirstStoreHasData_ReturnsNonEmptyFrame()
        {
            // Arrange: mock Store.GetDefaultFolder to return a mock MAPIFolder,
            // and mock folder.GetTable() (the COM method on MAPIFolder) to return a mock Table.
            // GetTable(Store,...) is a static extension method that Moq cannot intercept directly;
            // the correct seam is the underlying COM interface chain.
            // StoreTableEtlInvoker supplies known data with an EntryID column so that
            // the IndexRowsWith and frame-assembly paths inside the method are exercised.
            var mockColumns = new Mock<Outlook.Columns>(MockBehavior.Loose);
            var mockTable = new Mock<Outlook.Table>(MockBehavior.Loose);
            mockTable.SetupGet(t => t.Columns).Returns(mockColumns.Object);
            var mockFolder = new Mock<Outlook.MAPIFolder>(MockBehavior.Loose);
            mockFolder
                .Setup(f => f.GetTable(It.IsAny<object>(), It.IsAny<object>()))
                .Returns(mockTable.Object);
            var mockStore = new Mock<Outlook.Store>(MockBehavior.Loose);
            mockStore
                .Setup(s => s.GetDefaultFolder(It.IsAny<OlDefaultFolders>()))
                .Returns(mockFolder.Object);

            var injectedData = new object[,]
            {
                { "entryId-1", "IPM.Note", "2024-01-01", "conv-1", "A" },
            };
            var injectedColInfo = new Dictionary<string, int>
            {
                ["EntryID"] = 0,
                ["MessageClass"] = 1,
                ["SentOn"] = 2,
                ["ConversationId"] = 3,
                ["Triage"] = 4,
            };

            var singleStoreList = new List<Outlook.Store> { mockStore.Object };
            var mockStores = new Mock<Outlook.Stores>(MockBehavior.Loose);
            mockStores.Setup(s => s.GetEnumerator()).Returns(singleStoreList.GetEnumerator());

            var originalSeam = DfDeedle.StoreTableEtlInvoker;
            DfDeedle.StoreTableEtlInvoker = _ => (injectedData, injectedColInfo);

            try
            {
                // Act
                Frame<int, string> df = DfDeedle.FromDefaultFolder(
                    mockStores.Object,
                    OlDefaultFolders.olFolderInbox,
                    Array.Empty<string>(),
                    Array.Empty<string>()
                );

                // Assert: frame is non-null and contains data from the single store.
                df.Should().NotBeNull();
                df.RowCount.Should().Be(1);
            }
            finally
            {
                DfDeedle.StoreTableEtlInvoker = originalSeam;
            }
        }

        private static ProgressTracker CreateProgressTracker()
        {
            return new ProgressTracker(new SilentProgressTracker(), allocation: 100, startingAt: 0);
        }

        public TestContext TestContext { get; set; } = null!;

        private sealed class SilentProgressTracker : ProgressTracker
        {
            public SilentProgressTracker()
                : base(new CancellationTokenSource()) { }

            public override void Report((int Value, string JobName) report) { }

            public override void Report(double value) { }

            public override void Report(double value, string jobName) { }
        }
    }
}
