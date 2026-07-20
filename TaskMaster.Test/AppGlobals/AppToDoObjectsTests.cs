using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Proxies;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Office.Interop.Outlook;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using TaskMaster;
using ToDoModel.Data_Model.People;
using UtilitiesCS;
using UtilitiesCS.EmailIntelligence;
using UtilitiesCS.ReusableTypeClasses;
using UtilitiesCS.ReusableTypeClasses.Concurrent.Observable.Dictionary;

namespace TaskMaster.Test.AppGlobals
{
    [TestClass]
    public class AppToDoObjectsTests
    {
        [TestInitialize]
        public void TestInitialize()
        {
            Console.SetOut(new DebugTextWriter());
        }

        #region Helper Classes and Variables

        private MockRepository mockRepository = null!;
        private Mock<IApplicationGlobals> mockApplicationGlobals = null!;
        private Mock<IntelligenceConfig> mockIntelligenceConfig = null!;
        private Mock<ISmartSerializableNonTyped> mockSmartSerializable = null!;

        // This file has no project-level <Nullable> and no whole-file #nullable pragma; these
        // pre-existing `?` annotations need an explicit annotations context to avoid CS8632.
        // Scoping narrowly to annotations-only avoids introducing new CS86xx diagnostics
        // elsewhere in this file (no behavior change per AC7).
#nullable enable annotations
        private static void ConfigureIdListLoader(
            AppToDoObjects appToDoObjects,
            string fileName,
            Func<string, bool>? fileExists = null,
            Func<string, string>? readAllText = null
        )
#nullable restore annotations
        {
            var settings = new TaskMaster.Properties.Settings();
            var propertyValue = settings.PropertyValues["FileName_IDList"];
            if (propertyValue is null)
            {
                var property = settings.Properties["FileName_IDList"]!;
                propertyValue = new SettingsPropertyValue(property) { PropertyValue = fileName };
                settings.PropertyValues.Add(propertyValue);
            }
            else
            {
                propertyValue.PropertyValue = fileName;
            }

            AppToDoObjectsTestUtilities.SetReadonlyField(appToDoObjects, "_defaults", settings);

            if (fileExists is not null)
            {
                appToDoObjects.FileExists = fileExists;
            }

            if (readAllText is not null)
            {
                appToDoObjects.ReadAllText = readAllText;
            }
        }

        private Mock<ISmartSerializableNonTyped> GetMockSS()
        {
            var mockSS = this.mockRepository.Create<ISmartSerializableNonTyped>();
            mockSS
                .Setup(m =>
                    m.DeserializeAsync(
                        It.IsAny<SmartSerializableLoader>(),
                        true,
                        It.IsAny<Func<PeopleScoDictionaryNew>>()
                    )
                )
                .ReturnsAsync(new PeopleScoDictionaryNew(mockApplicationGlobals.Object));

            return mockSS;
        }

        private Mock<IntelligenceConfig> SetUpMockIntelRes(Mock<IApplicationGlobals> mockGlobals)
        {
            var intel = this.mockRepository.Create<IntelligenceConfig>(mockGlobals.Object);
            var config = new Dictionary<string, SmartSerializableLoader>
            {
                { "People", new SmartSerializableLoader() },
            }.ToConcurrentDictionary();
            intel.SetupGet(x => x.Config).Returns(config);
            mockGlobals.SetupGet(x => x.IntelRes).Returns(intel.Object);

            return intel;
        }

        #endregion Helper Classes and Variables

        [TestMethod]
        public async Task LoadPeopleAsync_CanLoadProperly()
        {
            // Arrange
            this.mockRepository = new MockRepository(MockBehavior.Strict);
            this.mockApplicationGlobals = this.mockRepository.Create<IApplicationGlobals>();
            this.mockApplicationGlobals.SetupGet(x => x.AF.CancelToken)
                .Returns(CancellationToken.None);
            this.mockIntelligenceConfig = SetUpMockIntelRes(mockApplicationGlobals);
            var appToDoObjects = new AppToDoObjects(mockApplicationGlobals.Object);
            this.mockSmartSerializable = GetMockSS();
            appToDoObjects.SmartSerializable = mockSmartSerializable.Object;
            var expectedHandler = typeof(AppToDoObjects).GetMethod(
                "People_CollectionChanged",
                [typeof(object), typeof(DictionaryChangedEventArgs<string, string>)]
            );

            // Act
            await appToDoObjects.LoadPeopleAsync();

            // Assert

            // that SmartSerializable.DeserializeAsync was called once,
            // the return value was properly assigned to the People property,
            // and the CollectionChanged event was properly assigned

            mockSmartSerializable.Verify(
                m =>
                    m.DeserializeAsync(
                        It.IsAny<SmartSerializableLoader>(),
                        true,
                        It.IsAny<Func<PeopleScoDictionaryNew>>()
                    ),
                Times.Once
            );
            Assert.IsNotNull(appToDoObjects.People);
            var assignedHandlers = EventHelper.GetEventInvocationList(
                appToDoObjects.People,
                "CollectionChanged"
            );
            Assert.IsTrue(
                assignedHandlers.Any(d => d.Method == expectedHandler),
                "CollectionChanged event does not contain the expected handler"
            );
        }

        [TestMethod]
        public async Task LoadIdListAsync_DoesNotReadOutlookApplicationFromWorkerThread()
        {
            // Arrange
            var callerThreadId = Environment.CurrentManagedThreadId;
            var accessedThreadIds = new ConcurrentQueue<int>();

            var fileSystem = new StubFileSystemFolderPaths();
            fileSystem.SpecialFolders["AppData"] = "virtual-app-data";

            var olObjects = OlObjectsProxy.Create(() =>
            {
                var currentThreadId = Environment.CurrentManagedThreadId;
                accessedThreadIds.Enqueue(currentThreadId);

                if (currentThreadId != callerThreadId)
                {
                    throw new InvalidOperationException(
                        "Outlook Application getter ran off the caller thread."
                    );
                }

                return null!;
            });

            var appToDoObjects = new AppToDoObjects(
                new StubApplicationGlobals(fileSystem, olObjects)
            );
            ConfigureIdListLoader(
                appToDoObjects,
                "ids.json",
                fileExists: _ => true,
                readAllText: _ => "[]"
            );

            // Act
            await AppToDoObjectsTestUtilities.InvokePrivateAsync(appToDoObjects, "LoadIdListAsync");

            // Assert
            accessedThreadIds.Should().NotBeEmpty();
            accessedThreadIds.Should().OnlyContain(threadId => threadId == callerThreadId);
        }

        [TestMethod]
        public async Task LoadIdListAsync_ReturnsEmptyWhenAppDataDirectoryMissing()
        {
            // Arrange
            var olObjects = OlObjectsProxy.Create(() =>
            {
                throw new InvalidOperationException(
                    "The Outlook application should not be accessed when AppData is unavailable."
                );
            });
            var appToDoObjects = new AppToDoObjects(
                new StubApplicationGlobals(new StubFileSystemFolderPaths(), olObjects)
            );

            // Act
            await AppToDoObjectsTestUtilities.InvokePrivateAsync(appToDoObjects, "LoadIdListAsync");

            // Assert
            appToDoObjects.IDList.Should().BeNull();
        }

        [TestMethod]
        public void LoadIdListFromDisk_ReturnsEmptyWhenJsonDeserializationFails()
        {
            // Arrange
            var fileSystem = new StubFileSystemFolderPaths();
            var appToDoObjects = new AppToDoObjects(
                new StubApplicationGlobals(fileSystem, OlObjectsProxy.Create(() => null!))
            );
            ConfigureIdListLoader(
                appToDoObjects,
                "ids.json",
                fileExists: _ => true,
                readAllText: _ => "not-json"
            );
            var method = typeof(AppToDoObjects).GetMethod(
                "LoadIdListFromDisk",
                BindingFlags.Instance | BindingFlags.NonPublic
            );

            // Act
            var idList = method!.Invoke(appToDoObjects, ["virtual-app-data"]) as IIDList;

            // Assert
            idList.Should().NotBeNull();
            idList!.Count.Should().Be(0);
        }

        [TestMethod]
        public void LoadIdListFromDisk_ReturnsEmptyWhenPersistedJsonIsCorrupted()
        {
            // Arrange
            var fileSystem = new StubFileSystemFolderPaths();
            var appToDoObjects = new AppToDoObjects(
                new StubApplicationGlobals(fileSystem, OlObjectsProxy.Create(() => null!))
            );
            ConfigureIdListLoader(
                appToDoObjects,
                "ids.json",
                fileExists: _ => true,
                readAllText: _ => "{"
            );
            var method = typeof(AppToDoObjects).GetMethod(
                "LoadIdListFromDisk",
                BindingFlags.Instance | BindingFlags.NonPublic
            );

            // Act
            var idList = method!.Invoke(appToDoObjects, ["virtual-app-data"]) as IIDList;

            // Assert
            idList.Should().NotBeNull();
            idList!.Count.Should().Be(0);
        }

        [TestMethod]
        public void LoadIdListFromDisk_ReturnsEmptyWhenReadThrowsIOException()
        {
            // Arrange
            var fileSystem = new StubFileSystemFolderPaths();
            var appToDoObjects = new AppToDoObjects(
                new StubApplicationGlobals(fileSystem, OlObjectsProxy.Create(() => null!))
            );
            ConfigureIdListLoader(
                appToDoObjects,
                "ids.json",
                fileExists: _ => true,
                readAllText: _ => throw new IOException("Simulated read failure.")
            );
            var method = typeof(AppToDoObjects).GetMethod(
                "LoadIdListFromDisk",
                BindingFlags.Instance | BindingFlags.NonPublic
            );

            // Act
            var idList = method!.Invoke(appToDoObjects, ["virtual-app-data"]) as IIDList;

            // Assert
            idList.Should().NotBeNull();
            idList!.Count.Should().Be(0);
        }

        [TestMethod]
        public void LoadIdListFromDisk_ReturnsEmptyWhenIdListFileIsMissing()
        {
            // Arrange
            var fileSystem = new StubFileSystemFolderPaths();
            var appToDoObjects = new AppToDoObjects(
                new StubApplicationGlobals(fileSystem, OlObjectsProxy.Create(() => null!))
            );
            ConfigureIdListLoader(appToDoObjects, "missing-id-list.json", fileExists: _ => false);
            var method = typeof(AppToDoObjects).GetMethod(
                "LoadIdListFromDisk",
                BindingFlags.Instance | BindingFlags.NonPublic
            );

            // Act
            var idList = method!.Invoke(appToDoObjects, ["virtual-app-data"]) as IIDList;

            // Assert
            idList.Should().NotBeNull();
            idList!.Count.Should().Be(0);
        }

        [TestMethod]
        public void LoadIdListFromDisk_ReturnsPersistedIdsWhenJsonIsValid()
        {
            // Arrange
            var fileSystem = new StubFileSystemFolderPaths();
            var appToDoObjects = new AppToDoObjects(
                new StubApplicationGlobals(fileSystem, OlObjectsProxy.Create(() => null!))
            );
            ConfigureIdListLoader(
                appToDoObjects,
                "ids.json",
                fileExists: _ => true,
                readAllText: _ => "[\"TD-1\"]"
            );
            var method = typeof(AppToDoObjects).GetMethod(
                "LoadIdListFromDisk",
                BindingFlags.Instance | BindingFlags.NonPublic
            );

            // Act
            var idList = method!.Invoke(appToDoObjects, ["virtual-app-data"]) as IIDList;

            // Assert
            idList.Should().NotBeNull();
            idList.Should().ContainSingle().Which.Should().Be("TD-1");
        }

        [TestMethod]
        public async Task LoadIdListAsync_RefreshesFromOutlookOnlyWhenDiskListIsEmpty()
        {
            // Arrange
            var throwingApplication = (Application)
                new ReflectionRealProxy(
                    typeof(Application),
                    (method, _) =>
                        method.Name switch
                        {
                            "get_Session" => throw new InvalidOperationException(
                                "RefreshIDList attempted to access the Outlook session."
                            ),
                            _ => throw new NotSupportedException(method.Name),
                        }
                ).GetTransparentProxy();

            var nonEmptyFileSystem = new StubFileSystemFolderPaths();
            nonEmptyFileSystem.SpecialFolders["AppData"] = "virtual-app-data";
            var nonEmptyAppToDoObjects = new AppToDoObjects(
                new StubApplicationGlobals(
                    nonEmptyFileSystem,
                    OlObjectsProxy.Create(() => throwingApplication)
                )
            );
            ConfigureIdListLoader(
                nonEmptyAppToDoObjects,
                "ids.json",
                fileExists: _ => true,
                readAllText: _ => "[\"TD-1\"]"
            );

            var emptyFileSystem = new StubFileSystemFolderPaths();
            emptyFileSystem.SpecialFolders["AppData"] = "virtual-app-data";
            var emptyAppToDoObjects = new AppToDoObjects(
                new StubApplicationGlobals(
                    emptyFileSystem,
                    OlObjectsProxy.Create(() => throwingApplication)
                )
            );
            ConfigureIdListLoader(emptyAppToDoObjects, "missing-serializable-list.json");

            // Act
            Func<Task> nonEmptyAct = () =>
                AppToDoObjectsTestUtilities.InvokePrivateAsync(
                    nonEmptyAppToDoObjects,
                    "LoadIdListAsync"
                );
            Func<Task> emptyAct = () =>
                AppToDoObjectsTestUtilities.InvokePrivateAsync(
                    emptyAppToDoObjects,
                    "LoadIdListAsync"
                );

            // Assert
            await nonEmptyAct.Should().NotThrowAsync();
            nonEmptyAppToDoObjects.IDList.Should().NotBeNull();
            nonEmptyAppToDoObjects.IDList!.Count.Should().Be(1);

            await emptyAct
                .Should()
                .ThrowAsync<InvalidOperationException>()
                .WithMessage("RefreshIDList attempted to access the Outlook session.");
        }

        [TestMethod]
        public async Task LoadIdListAsync_SkipsOutlookRefreshWhenDiskListAlreadyContainsEntries()
        {
            // Arrange
            var throwingApplication = (Application)
                new ReflectionRealProxy(
                    typeof(Application),
                    (method, _) =>
                        method.Name switch
                        {
                            "get_Session" => throw new InvalidOperationException(
                                "RefreshIDList attempted to access the Outlook session."
                            ),
                            _ => throw new NotSupportedException(method.Name),
                        }
                ).GetTransparentProxy();

            var fileSystem = new StubFileSystemFolderPaths();
            fileSystem.SpecialFolders["AppData"] = "virtual-app-data";
            var appToDoObjects = new AppToDoObjects(
                new StubApplicationGlobals(
                    fileSystem,
                    OlObjectsProxy.Create(() => throwingApplication)
                )
            );
            ConfigureIdListLoader(
                appToDoObjects,
                "ids.json",
                fileExists: _ => true,
                readAllText: _ => "[\"TD-1\"]"
            );

            // Act
            await AppToDoObjectsTestUtilities.InvokePrivateAsync(appToDoObjects, "LoadIdListAsync");

            // Assert
            appToDoObjects.IDList.Should().NotBeNull();
            appToDoObjects.IDList!.Count.Should().Be(1);
        }

        [TestMethod]
        public async Task LoadIdListAsync_SkipsOutlookRefreshWhenParentAppIsNull()
        {
            // Arrange
            var fileSystem = new StubFileSystemFolderPaths();
            fileSystem.SpecialFolders["AppData"] = "virtual-app-data";
            var appToDoObjects = new AppToDoObjects(
                new StubApplicationGlobals(fileSystem, OlObjectsProxy.Create(() => null!))
            );
            ConfigureIdListLoader(appToDoObjects, "missing-serializable-list.json");

            // Act
            await AppToDoObjectsTestUtilities.InvokePrivateAsync(appToDoObjects, "LoadIdListAsync");

            // Assert
            appToDoObjects.IDList.Should().NotBeNull();
            appToDoObjects.IDList!.Count.Should().Be(0);
        }

        [TestMethod]
        public async Task LoadProjInfoAsync_SkipsRebuildWhenOutlookApplicationIsNull()
        {
            // Arrange
            var fileSystem = new StubFileSystemFolderPaths();
            fileSystem.SpecialFolders["AppData"] = "virtual-app-data";
            using var serializableListScope = new ProjectDataSerializableListScope();
            var appToDoObjects = new AppToDoObjects(
                new StubApplicationGlobals(fileSystem, OlObjectsProxy.Create(() => null!))
            );
            var settings = new TaskMaster.Properties.Settings();
            var propertyValue = settings.PropertyValues["FileName_ProjInfo"];
            if (propertyValue is null)
            {
                var property = settings.Properties["FileName_ProjInfo"]!;
                propertyValue = new SettingsPropertyValue(property)
                {
                    PropertyValue = "ProjInfo.json",
                };
                settings.PropertyValues.Add(propertyValue);
            }
            else
            {
                propertyValue.PropertyValue = "ProjInfo.json";
            }
            AppToDoObjectsTestUtilities.SetReadonlyField(appToDoObjects, "_defaults", settings);

            // Act
            await AppToDoObjectsTestUtilities.InvokePrivateAsync(
                appToDoObjects,
                "LoadProjInfoAsync"
            );

            // Assert
            appToDoObjects.ProjInfo.Should().NotBeNull();
            appToDoObjects.ProjInfo.Count.Should().Be(0);
        }

        [TestMethod]
        public async Task LoadProjInfoAsync_SkipsRebuildWhenProjectCountIsNonZero()
        {
            // Arrange
            var fileSystem = new StubFileSystemFolderPaths();
            fileSystem.SpecialFolders["AppData"] = "virtual-app-data";
            var throwingApplication = (Application)
                new ReflectionRealProxy(
                    typeof(Application),
                    (method, _) =>
                        method.Name switch
                        {
                            "get_Session" => throw new InvalidOperationException(
                                "ProjectData.Rebuild attempted to access the Outlook session."
                            ),
                            _ => throw new NotSupportedException(method.Name),
                        }
                ).GetTransparentProxy();
            using var serializableListScope = new ProjectDataSerializableListScope(
                new ToDoModel.ProjectEntry("Project A", "1234", "Program A", "PRG1")
            );
            var appToDoObjects = new AppToDoObjects(
                new StubApplicationGlobals(
                    fileSystem,
                    OlObjectsProxy.Create(() => throwingApplication)
                )
            );
            var settings = new TaskMaster.Properties.Settings();
            var propertyValue = settings.PropertyValues["FileName_ProjInfo"];
            if (propertyValue is null)
            {
                var property = settings.Properties["FileName_ProjInfo"]!;
                propertyValue = new SettingsPropertyValue(property)
                {
                    PropertyValue = "ProjInfo.json",
                };
                settings.PropertyValues.Add(propertyValue);
            }
            else
            {
                propertyValue.PropertyValue = "ProjInfo.json";
            }
            AppToDoObjectsTestUtilities.SetReadonlyField(appToDoObjects, "_defaults", settings);

            // Act
            await AppToDoObjectsTestUtilities.InvokePrivateAsync(
                appToDoObjects,
                "LoadProjInfoAsync"
            );

            // Assert
            appToDoObjects.ProjInfo.Should().NotBeNull();
            appToDoObjects.ProjInfo.Count.Should().Be(1);
            appToDoObjects.ProjInfo[0].ProjectID.Should().Be("1234");
        }

        [TestMethod]
        public async Task LoadProjInfoAsync_RebuildsWhenProjectCountIsZeroAndOutlookApplicationIsAvailable()
        {
            // Arrange
            var fileSystem = new StubFileSystemFolderPaths();
            fileSystem.SpecialFolders["AppData"] = "virtual-app-data";
            var throwingApplication = (Application)
                new ReflectionRealProxy(
                    typeof(Application),
                    (method, _) =>
                        method.Name switch
                        {
                            "get_Session" => throw new InvalidOperationException(
                                "ProjectData.Rebuild attempted to access the Outlook session."
                            ),
                            _ => throw new NotSupportedException(method.Name),
                        }
                ).GetTransparentProxy();
            using var serializableListScope = new ProjectDataSerializableListScope();
            var appToDoObjects = new AppToDoObjects(
                new StubApplicationGlobals(
                    fileSystem,
                    OlObjectsProxy.Create(() => throwingApplication)
                )
            );
            var settings = new TaskMaster.Properties.Settings();
            var propertyValue = settings.PropertyValues["FileName_ProjInfo"];
            if (propertyValue is null)
            {
                var property = settings.Properties["FileName_ProjInfo"]!;
                propertyValue = new SettingsPropertyValue(property)
                {
                    PropertyValue = "ProjInfo.json",
                };
                settings.PropertyValues.Add(propertyValue);
            }
            else
            {
                propertyValue.PropertyValue = "ProjInfo.json";
            }
            AppToDoObjectsTestUtilities.SetReadonlyField(appToDoObjects, "_defaults", settings);

            // Act
            Func<Task> act = () =>
                AppToDoObjectsTestUtilities.InvokePrivateAsync(appToDoObjects, "LoadProjInfoAsync");

            // Assert
            await act.Should()
                .ThrowAsync<InvalidOperationException>()
                .WithMessage("ProjectData.Rebuild attempted to access the Outlook session.");
        }

        [TestMethod]
        public async Task LoadProjInfoAsync_DoesNotReadOutlookApplicationFromWorkerThread()
        {
            // Arrange
            var callerThreadId = Environment.CurrentManagedThreadId;
            var accessedThreadIds = new ConcurrentQueue<int>();

            var fileSystem = new StubFileSystemFolderPaths();
            fileSystem.SpecialFolders["AppData"] = "virtual-app-data";

            var olObjects = OlObjectsProxy.Create(() =>
            {
                var currentThreadId = Environment.CurrentManagedThreadId;
                accessedThreadIds.Enqueue(currentThreadId);

                if (currentThreadId != callerThreadId)
                {
                    throw new InvalidOperationException(
                        "Outlook Application getter ran off the caller thread."
                    );
                }

                return null!;
            });

            using var serializableListScope = new ProjectDataSerializableListScope();
            var appToDoObjects = new AppToDoObjects(
                new StubApplicationGlobals(fileSystem, olObjects)
            );
            var settings = TaskMaster.Properties.Settings.Default;
            var propertyValue = settings.PropertyValues["FileName_ProjInfo"];
            if (propertyValue is null)
            {
                var property = settings.Properties["FileName_ProjInfo"]!;
                propertyValue = new SettingsPropertyValue(property)
                {
                    PropertyValue = "ProjInfo.json",
                };
                settings.PropertyValues.Add(propertyValue);
            }
            else
            {
                propertyValue.PropertyValue = "ProjInfo.json";
            }
            AppToDoObjectsTestUtilities.SetReadonlyField(appToDoObjects, "_defaults", settings);

            // Act
            await AppToDoObjectsTestUtilities.InvokePrivateAsync(
                appToDoObjects,
                "LoadProjInfoAsync"
            );

            // Assert
            accessedThreadIds.Should().NotBeEmpty();
            accessedThreadIds.Should().OnlyContain(threadId => threadId == callerThreadId);
        }
    }
}
