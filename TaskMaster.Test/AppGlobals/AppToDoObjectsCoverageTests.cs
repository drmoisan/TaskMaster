using System.Reflection;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using TaskMaster;
using UtilitiesCS;

namespace TaskMaster.Test.AppGlobals
{
    [TestClass]
    public class AppToDoObjectsCoverageTests
    {
        [TestMethod]
        public void LoadProgramInfo_ReturnsNullWhenPythonStagingMissing()
        {
            var appToDoObjects = new AppToDoObjects(
                new StubApplicationGlobals(
                    new StubFileSystemFolderPaths(),
                    OlObjectsProxy.Create(() => null!)
                )
            );
            var method = typeof(AppToDoObjects).GetMethod(
                "LoadProgramInfo",
                BindingFlags.Instance | BindingFlags.NonPublic
            );

            var programInfo = method!.Invoke(appToDoObjects, null);

            programInfo.Should().BeNull();
        }

        [TestMethod]
        public void People_CollectionChanged_SerializesPeopleDictionary()
        {
            var appToDoObjects = new AppToDoObjects(
                new StubApplicationGlobals(
                    new StubFileSystemFolderPaths(),
                    OlObjectsProxy.Create(() => null!)
                )
            );
            var people = new Mock<IPeopleScoDictionaryNew>(MockBehavior.Strict);

            people.Setup(x => x.Serialize());
            AppToDoObjectsTestUtilities.SetReadonlyField(
                appToDoObjects,
                "<People>k__BackingField",
                people.Object
            );

            appToDoObjects.People_CollectionChanged(people.Object, null!);

            people.Verify(x => x.Serialize(), Times.Once);
        }

        [TestMethod]
        public void LoadIDList_ReturnsNullWhenAppDataMissing()
        {
            var appToDoObjects = new AppToDoObjects(
                new StubApplicationGlobals(
                    new StubFileSystemFolderPaths(),
                    OlObjectsProxy.Create(() => null!)
                )
            );
            var method = typeof(AppToDoObjects).GetMethod(
                "LoadIDList",
                BindingFlags.Instance | BindingFlags.NonPublic
            );

            var idList = method!.Invoke(appToDoObjects, null);

            idList.Should().BeNull();
        }

        [TestMethod]
        public void LoadProjInfo_ReturnsNullWhenAppDataMissing()
        {
            var appToDoObjects = new AppToDoObjects(
                new StubApplicationGlobals(
                    new StubFileSystemFolderPaths(),
                    OlObjectsProxy.Create(() => null!)
                )
            );
            var method = typeof(AppToDoObjects).GetMethod(
                "LoadProjInfo",
                BindingFlags.Instance | BindingFlags.NonPublic
            );

            var projectInfo = method!.Invoke(appToDoObjects, null);

            projectInfo.Should().BeNull();
        }
    }
}
