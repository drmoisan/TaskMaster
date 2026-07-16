using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Deedle;
using FluentAssertions;
using Microsoft.Office.Interop.Outlook;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ToDoModel;
using UtilitiesCS;
using UtilitiesCS.OutlookObjects.Store;
using OutlookStore = Microsoft.Office.Interop.Outlook.Store;

namespace ToDoModel.Test
{
    [TestClass]
    public class ProjectDataCoverageExpansionTests
    {
        [TestMethod]
        public void Constructors_WithDefaultListAndEnumerableInputs_LoadEntriesWithoutFileSystem()
        {
            // Arrange
            var entryA = NewEntry("Alpha", "A001", "Program A");
            var entryB = NewEntry("Beta", "B001", "Program B");
            var list = new List<IProjectEntry> { entryA, entryB };
            IEnumerable<IProjectEntry> enumerable = list.Select(entry => entry);

            // Act
            var empty = new ProjectData();
            var fromList = new ProjectData(list);
            var fromEnumerable = new ProjectData(enumerable);

            // Assert
            empty.Should().BeEmpty();
            fromList.Should().Equal(entryA, entryB);
            fromEnumerable.Should().Equal(entryA, entryB);
        }

        [TestMethod]
        public void SetIdUpdateAction_WithEntries_PropagatesActionToEveryEntry()
        {
            // Arrange
            var entryA = new Mock<IProjectEntry>(MockBehavior.Loose);
            var entryB = new Mock<IProjectEntry>(MockBehavior.Loose);
            var data = new ProjectData(new[] { entryA.Object, entryB.Object });
            Action<string, string> action = (oldId, newId) => { };

            // Act
            data.SetIdUpdateAction(action);

            // Assert
            entryA.Verify(entry => entry.SetIdUpdateAction(action), Times.Once);
            entryB.Verify(entry => entry.SetIdUpdateAction(action), Times.Once);
        }

        [TestMethod]
        public void IsCorrupt_WithEmptyValidAndNullEntry_ReturnsExpectedIndices()
        {
            // Arrange
            var empty = new ProjectData();
            var valid = new ProjectData(new[] { NewEntry("Alpha", "A001", "Program A") });
            var corrupt = new ProjectData(
                new[]
                {
                    NewEntry("Alpha", "A001", "Program A"),
                    NewEntry(null, "B001", "Program B"),
                    NewEntry("Gamma", null, "Program C"),
                }
            );

            // Act
            var emptyResult = empty.IsCorrupt();
            var validResult = valid.IsCorrupt();
            var corruptResult = corrupt.IsCorrupt();

            // Assert
            emptyResult.Any.Should().BeFalse();
            emptyResult.Indices.Should().Equal(-1);
            validResult.Any.Should().BeFalse();
            validResult.Indices.Should().BeEmpty();
            corruptResult.Any.Should().BeTrue();
            corruptResult.Indices.Should().Equal(1, 2);
        }

        [TestMethod]
        public void Queries_WithDuplicateMissingAndCaseVariants_ReturnExpectedMatches()
        {
            // Arrange
            var data = new ProjectData(
                new[]
                {
                    NewEntry("Alpha", "A001", "Program A"),
                    NewEntry("Alpha", "A002", "Program B"),
                    NewEntry("Beta", "B001", "Program B"),
                }
            );

            // Act and Assert
            data.Contains_ProjectName("alpha").Should().BeTrue();
            data.Contains_ProjectName("Missing").Should().BeFalse();
            data.Find_ByProjectName("ALPHA")
                .Select(entry => entry.ProjectID)
                .Should()
                .Equal("A001", "A002");
            data.Contains_ProjectID("B001").Should().BeTrue();
            data.Contains_ProjectID("Z999").Should().BeFalse();
            data.Find_ByProjectID("Z999").Should().BeEmpty();
            data.Contains_ProgramName("program b").Should().BeTrue();
            data.Contains_ProgramName("Unknown").Should().BeFalse();
            data.Find_ByProgramName("PROGRAM B")
                .Select(entry => entry.ProjectName)
                .Should()
                .Equal("Alpha", "Beta");
            data.Programs_ByProjectNames("Alpha, Missing, Beta").Should().Be("Program A,Program B");
        }

        [TestMethod]
        public void ProgramsByProjectNames_WithNullInput_ReturnsEmptyString()
        {
            // Arrange
            var data = new ProjectData(new[] { NewEntry("Alpha", "A001", "Program A") });

            // Act
            var result = data.Programs_ByProjectNames(null);

            // Assert
            result.Should().BeEmpty();
        }

        [TestMethod]
        public void UpdateProjectID_WithDuplicateAndNewIds_ReturnsExpectedValues()
        {
            // Arrange
            var data = new ProjectData(new[] { NewEntry("Alpha", "A001", "Program A") });

            // Act
            var duplicate = InvokeInternal<bool>(data, "UpdateProjectID", "A001");
            var available = InvokeInternal<bool>(data, "UpdateProjectID", "B001");

            // Assert
            duplicate.Should().BeFalse();
            available.Should().BeTrue();
        }

        [TestMethod]
        public void FilterToProjectIDs_WithNullAndMixedRowKeys_ReturnsOnlyFourCharacterRows()
        {
            // Arrange
            var data = new ProjectData();
            var frame = NewProjectFrame(
                "A001",
                "Tag PROJECT Alpha",
                "LONG01",
                "Tag PROJECT Long",
                "B002",
                "Tag PROJECT Beta"
            );

            // Act
            var nullResult = InvokeInternal<Frame<string, string>>(
                data,
                "FilterToProjectIDs",
                (object)null
            );
            var filtered = InvokeInternal<Frame<string, string>>(data, "FilterToProjectIDs", frame);

            // Assert
            nullResult.Should().BeNull();
            filtered.RowIndex.Keys.Should().Equal("A001", "B002");
            filtered.ColumnKeys.Should().NotContain("IdLength");
        }

        [TestMethod]
        public void DfToListEntries_WithProjectCategories_ParsesProjectAndProgramNames()
        {
            // Arrange
            var data = new ProjectData();
            var frame = NewProjectFrame(
                "A001",
                "Tag PROJECT Program A-Alpha",
                "B002",
                "Tag PROJECT Beta"
            );

            // Act
            var entries = InvokeInternal<List<IProjectEntry>>(data, "DfToListEntries", frame);

            // Assert
            entries.Select(entry => entry.ProjectID).Should().Equal("A001", "B002");
            entries.Select(entry => entry.ProjectName).Should().Equal("Program A-Alpha", "Beta");
            entries.Select(entry => entry.ProgramName).Should().Equal("Program A", "Beta");
        }

        [TestMethod]
        public void Rebuild_WhenStoreIdExcluded_DoesNotProcessExcludedStore()
        {
            // Arrange
            var keep = CreateStoreMock("KEEP-STORE-ID");
            var drop = CreateStoreMock("DROP-STORE-ID");
            var app = CreateAppWithStores(keep.Object, drop.Object);
            var wrapper = new StoresWrapper
            {
                ExcludedStoreIds = new List<string> { "DROP-STORE-ID" },
                ExcludedStoreNameContains = new List<string>(),
                ExcludedStoreFilePathContains = new List<string>(),
                GwsoFilePathContains = new List<string>(),
                ExcludeGwsoStores = false,
                ExcludePublicFolderStores = false,
            };
            var data = new ProjectData();

            // Act: Rebuild routes stores through the shared filter before GetDfToDo. This test
            // asserts the routing side effect (which stores are visited); the downstream
            // empty-frame failure after the included store yields no ToDo data is intentionally
            // swallowed because Rebuild completion is out of scope for this routing assertion.
            try
            {
                data.Rebuild(app.Object, wrapper);
            }
            catch
            {
                // routing-only assertion below.
            }

            // Assert
            keep.Verify(
                x => x.GetDefaultFolder(OlDefaultFolders.olFolderToDo),
                Times.Once(),
                "the included store must be processed by Rebuild"
            );
            drop.Verify(
                x => x.GetDefaultFolder(It.IsAny<OlDefaultFolders>()),
                Times.Never(),
                "the StoreID-excluded store must be skipped before GetDfToDo"
            );
        }

        private static Mock<OutlookStore> CreateStoreMock(string storeId)
        {
            var store = new Mock<OutlookStore>();
            store.SetupGet(x => x.StoreID).Returns(storeId);
            store.SetupGet(x => x.DisplayName).Returns(storeId + " Mailbox");
            store
                .SetupGet(x => x.ExchangeStoreType)
                .Returns(OlExchangeStoreType.olPrimaryExchangeMailbox);
            store.SetupGet(x => x.FilePath).Returns(@"C:\Data\" + storeId + ".ost");
            // why: GetToDoTable() swallows a GetDefaultFolder failure and returns null, so throwing
            // here keeps the included-store path deterministic (no partial COM table mock needed)
            // while still recording that the folder was resolved.
            store
                .Setup(x => x.GetDefaultFolder(OlDefaultFolders.olFolderToDo))
                .Throws(new InvalidOperationException("ToDo folder unavailable in test"));
            return store;
        }

        private static Mock<Application> CreateAppWithStores(params OutlookStore[] stores)
        {
            var storesCollection = new Mock<Stores>();
            storesCollection
                .As<IEnumerable>()
                .Setup(x => x.GetEnumerator())
                .Returns(() => stores.Cast<object>().GetEnumerator());

            var nameSpace = new Mock<NameSpace>();
            nameSpace.SetupGet(x => x.Stores).Returns(storesCollection.Object);

            var app = new Mock<Application>();
            app.SetupGet(x => x.Session).Returns(nameSpace.Object);
            return app;
        }

        private static IProjectEntry NewEntry(
            string projectName,
            string projectId,
            string programName
        )
        {
            return new ProjectEntry(projectName, projectId, programName);
        }

        private static Frame<string, string> NewProjectFrame(params string[] rowValues)
        {
            if (rowValues.Length % 2 != 0)
            {
                throw new ArgumentException("Project frame rows must provide ID/category pairs.");
            }

            var data = new object[rowValues.Length / 2, 2];
            for (var index = 0; index < rowValues.Length; index += 2)
            {
                data[index / 2, 0] = rowValues[index];
                data[index / 2, 1] = rowValues[index + 1];
            }

            return DfDeedle
                .FromArray2D(
                    data,
                    new Dictionary<string, int> { ["ToDoID"] = 0, ["Categories"] = 1 }
                )
                .IndexRows<string>("ToDoID");
        }

        private static T InvokeInternal<T>(
            ProjectData data,
            string methodName,
            params object[] arguments
        )
        {
            var method = typeof(ProjectData).GetMethod(
                methodName,
                BindingFlags.Instance | BindingFlags.NonPublic
            );

            method.Should().NotBeNull();
            return (T)method.Invoke(data, arguments);
        }
    }
}
