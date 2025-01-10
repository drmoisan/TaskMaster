using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using TaskMaster;
using UtilitiesCS;
using UtilitiesCS.EmailIntelligence;
using UtilitiesCS.ReusableTypeClasses;
using ToDoModel.Data_Model.People;
using System.Threading;

namespace TaskMaster.Test.AppGlobals
{
    [TestClass]
    public class AppToDoObjectsTests
    {
        private MockRepository mockRepository;
        private Mock<ApplicationGlobals> mockApplicationGlobals;
        private Mock<AppAutoFileObjects> mockAutoFileObjects;
        private AppFileSystemFolderPaths appFP; 
        private Mock<IntelligenceConfig> mockIntelligenceConfig;

        [TestInitialize]
        public void TestInitialize()
        {
            //this.mockRepository = new MockRepository(MockBehavior.Strict);
            //this.mockApplicationGlobals = this.mockRepository.Create<ApplicationGlobals>();
            //this.mockAutoFileObjects = this.mockRepository.Create<AppAutoFileObjects>();
            //this.mockAutoFileObjects.SetupGet(x => x.CancelToken).Returns(new System.Threading.CancellationToken());
            //this.mockApplicationGlobals.SetupGet(x => x.AF).Returns(this.mockAutoFileObjects.Object);
            //appFP = new AppFileSystemFolderPaths();
            //this.mockApplicationGlobals.SetupGet(x => x.FS).Returns(appFP);
            
            //this.mockApplicationGlobals.SetupGet(x => x.IntelRes).Returns(this.mockIntelligenceConfig.Object);

        }

        private IntelligenceConfig CreateMockIntelRes()
        {
            this.mockIntelligenceConfig = this.mockRepository.Create<IntelligenceConfig>();
            var config = new ConcurrentDictionary<string, SmartSerializableLoader>();

            return this.mockIntelligenceConfig.Object;
        }

        private AppToDoObjects CreateAppToDoObjects()
        {
            return new AppToDoObjects(
                this.mockApplicationGlobals.Object);
        }

        [TestMethod]
        public async Task LoadPeopleAsync_CanLoadProperly()
        {
            // Arrange
            //var appToDoObjects = this.CreateAppToDoObjects();
            var mockParent = new Mock<IApplicationGlobals>();
            var mockIntelRes = new Mock<IntelligenceConfig>(mockParent.Object);
            var mockConfig = new Dictionary<string, SmartSerializableLoader>
            {
                { "People", new SmartSerializableLoader()   }
            }.ToConcurrentDictionary();

            mockIntelRes.SetupGet(x => x.Config).Returns(mockConfig);
            mockParent.SetupGet(x => x.IntelRes).Returns(mockIntelRes.Object);
            mockParent.SetupGet(x => x.AF.CancelToken).Returns(CancellationToken.None);

            var mockSmartSerializable = new Mock<ISmartSerializableNonTyped>();
            mockSmartSerializable
                .Setup(m => m.DeserializeAsync(It.IsAny<SmartSerializableLoader>(), true, It.IsAny<Func<IPeopleScoDictionaryNew>>()))
                .ReturnsAsync(new PeopleScoDictionaryNew());

            // Act
            //await appToDoObjects.LoadPeopleAsync();

            // Assert


            await Task.CompletedTask;
        }

        //[TestMethod]
        //public async Task LoadAsync_StateUnderTest_ExpectedBehavior()
        //{
        //    // Arrange
        //    var appToDoObjects = this.CreateAppToDoObjects();
        //    bool parallel = false;

        //    // Act
        //    await appToDoObjects.LoadAsync(
        //        parallel);

        //    // Assert
        //    Assert.Fail();
        //    this.mockRepository.VerifyAll();
        //}

        //[TestMethod]
        //public async Task LoadParallelAsync_StateUnderTest_ExpectedBehavior()
        //{
        //    // Arrange
        //    var appToDoObjects = this.CreateAppToDoObjects();

        //    // Act
        //    await appToDoObjects.LoadParallelAsync();

        //    // Assert
        //    Assert.Fail();
        //    this.mockRepository.VerifyAll();
        //}

        //[TestMethod]
        //public async Task LoadSequentialAsync_StateUnderTest_ExpectedBehavior()
        //{
        //    // Arrange
        //    var appToDoObjects = this.CreateAppToDoObjects();

        //    // Act
        //    await appToDoObjects.LoadSequentialAsync();

        //    // Assert
        //    Assert.Fail();
        //    this.mockRepository.VerifyAll();
        //}

        //[TestMethod]
        //public void People_CollectionChanged_StateUnderTest_ExpectedBehavior()
        //{
        //    // Arrange
        //    var appToDoObjects = this.CreateAppToDoObjects();
        //    object Sender = null;
        //    DictionaryChangedEventArgs args = null;

        //    // Act
        //    appToDoObjects.People_CollectionChanged(
        //        Sender,
        //        args);

        //    // Assert
        //    Assert.Fail();
        //    this.mockRepository.VerifyAll();
        //}

        //[TestMethod]
        //public void LoadPrefixList_StateUnderTest_ExpectedBehavior()
        //{
        //    // Arrange
        //    var appToDoObjects = this.CreateAppToDoObjects();

        //    // Act
        //    var result = appToDoObjects.LoadPrefixList();

        //    // Assert
        //    Assert.Fail();
        //    this.mockRepository.VerifyAll();
        //}

        //[TestMethod]
        //public void LoadFilteredFolderScraping_StateUnderTest_ExpectedBehavior()
        //{
        //    // Arrange
        //    var appToDoObjects = this.CreateAppToDoObjects();

        //    // Act
        //    var result = appToDoObjects.LoadFilteredFolderScraping();

        //    // Assert
        //    Assert.Fail();
        //    this.mockRepository.VerifyAll();
        //}

        //[TestMethod]
        //public void LoadFolderRemap_StateUnderTest_ExpectedBehavior()
        //{
        //    // Arrange
        //    var appToDoObjects = this.CreateAppToDoObjects();

        //    // Act
        //    var result = appToDoObjects.LoadFolderRemap();

        //    // Assert
        //    Assert.Fail();
        //    this.mockRepository.VerifyAll();
        //}
    }
}
