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
using System.Collections.Specialized;
using FluentAssertions;
using System.Linq;
using ConcurrentObservableCollections.ConcurrentObservableDictionary;
using System.Reflection;

namespace TaskMaster.Test.AppGlobals
{
    [TestClass]
    public class AppToDoObjectsTests
    {
        [TestInitialize]
        public void TestInitialize()
        {
            Console.SetOut(new DebugTextWriter());
            this.mockRepository = new MockRepository(MockBehavior.Strict);
            this.mockApplicationGlobals = this.mockRepository.Create<IApplicationGlobals>();
            this.mockApplicationGlobals.SetupGet(x => x.AF.CancelToken).Returns(CancellationToken.None);            
        }

        #region Helper Classes and Variables

        private MockRepository mockRepository;
        private Mock<IApplicationGlobals> mockApplicationGlobals;
        //private Mock<AppAutoFileObjects> mockAutoFileObjects;
        //private AppFileSystemFolderPaths appFP;
        private Mock<IntelligenceConfig> mockIntelligenceConfig;
        private Mock<ISmartSerializableNonTyped> mockSmartSerializable;

        private Mock<ISmartSerializableNonTyped> GetMockSS()
        {
            var mockSS = this.mockRepository.Create<ISmartSerializableNonTyped>();
            mockSS
                .Setup(m => m.DeserializeAsync(It.IsAny<SmartSerializableLoader>(), true, It.IsAny<Func<PeopleScoDictionaryNew>>()))
                .ReturnsAsync(new PeopleScoDictionaryNew());

            return mockSS;
        }

        private Mock<IntelligenceConfig> SetUpMockIntelRes(Mock<IApplicationGlobals> mockGlobals)
        {
            var intel = this.mockRepository.Create<IntelligenceConfig>(mockGlobals.Object);
            var config = new Dictionary<string, SmartSerializableLoader>
            {
                { "People", new SmartSerializableLoader()   }
            }.ToConcurrentDictionary();
            intel.SetupGet(x => x.Config).Returns(config);
            mockGlobals.SetupGet(x => x.IntelRes).Returns(intel.Object);
            
            return intel;
        }

        public static class EventHelper
        {
            public static Delegate[] GetEventInvocationList(object target, string eventName)
            {
                if (target == null) throw new ArgumentNullException(nameof(target));
                if (string.IsNullOrEmpty(eventName)) throw new ArgumentNullException(nameof(eventName));

                Type targetType = target.GetType();
                EventInfo eventInfo = targetType.GetEvent(eventName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                if (eventInfo == null) throw new ArgumentException($"Event '{eventName}' not found on type '{targetType}'.");

                // Get the method that adds the event handler
                MethodInfo addMethod = eventInfo.GetAddMethod(true);
                if (addMethod == null) throw new ArgumentException($"Event '{eventName}' does not have an accessible add method.");

                // Get the declaring type of the event
                Type declaringType = eventInfo.DeclaringType;
                if (declaringType == null) throw new ArgumentException($"Event '{eventName}' does not have a declaring type.");

                // Get the field that stores the event handlers
                FieldInfo eventField = declaringType.GetField(eventName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
                if (eventField == null)
                {
                    // Try to find the field that stores the event handlers in the base class
                    eventField = FindEventFieldInBaseClasses(declaringType, eventName);
                }

                if (eventField == null) throw new ArgumentException($"Event field '{eventName}' not found on type '{declaringType}'.");

                object eventFieldValue = eventField.GetValue(target);
                if (eventFieldValue is Delegate eventDelegate)
                {
                    return eventDelegate.GetInvocationList();
                }

                return Array.Empty<Delegate>();
            }

            private static FieldInfo FindEventFieldInBaseClasses(Type type, string eventName)
            {
                while (type != null)
                {
                    FieldInfo field = type.GetField(eventName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
                    if (field != null)
                    {
                        return field;
                    }
                    type = type.BaseType;
                }
                return null;
            }
        }

        #endregion Helper Classes and Variables

        [TestMethod]
        public async Task LoadPeopleAsync_CanLoadProperly()
        {
            // Arrange
            this.mockIntelligenceConfig = SetUpMockIntelRes(mockApplicationGlobals);
            var appToDoObjects = new AppToDoObjects(mockApplicationGlobals.Object);            
            this.mockSmartSerializable = GetMockSS();
            appToDoObjects.SmartSerializable = mockSmartSerializable.Object;
            var expectedHandler = typeof(AppToDoObjects).GetMethod("People_CollectionChanged", [typeof(object), typeof(DictionaryChangedEventArgs<string, string>)]);

            // Act
            await appToDoObjects.LoadPeopleAsync();

            // Assert
            
            // that SmartSerializable.DeserializeAsync was called once,
            // the return value was properly assigned to the People property,
            // and the CollectionChanged event was properly assigned
           
            mockSmartSerializable.Verify(m => m.DeserializeAsync(It.IsAny<SmartSerializableLoader>(), true, It.IsAny<Func<PeopleScoDictionaryNew>>()), Times.Once);
            Assert.IsNotNull(appToDoObjects.People);
            var assignedHandlers = EventHelper.GetEventInvocationList(appToDoObjects.People, "CollectionChanged");
            Assert.IsTrue(assignedHandlers.Any(d => d.Method == expectedHandler), "CollectionChanged event does not contain the expected handler");

        }

        //[TestMethod]
        //public async Task IntegrationTest_LoadPeopleAsync_CanLoadProperly()
        //{
        //    // Arrange
        //    mockApplicationGlobals.SetupGet(x => x.FS).Returns(new AppFileSystemFolderPaths());
        //    var intelRes = await IntelligenceConfig.LoadAsync(mockApplicationGlobals.Object);
        //    mockApplicationGlobals.SetupGet(x => x.IntelRes).Returns(intelRes);
        //    var appToDoObjects = new AppToDoObjects(mockApplicationGlobals.Object);                       

        //    // Act
        //    await appToDoObjects.LoadPeopleAsync();            
            
        //    // Assert

        //    // the return value was properly assigned to the People property,
        //    // and the CollectionChanged event was properly assigned

        //    Assert.IsNotNull(appToDoObjects.People);            

        //}


        #region Commented Tests

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

        #endregion Commented Tests
    }
}
