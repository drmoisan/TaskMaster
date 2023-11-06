using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Threading.Tasks;
using UtilitiesCS;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;

namespace UtilitiesCS.Test.ReusableTypeClasses
{
    [TestClass]
    public class ScoCollectionTests
    {
        private MockRepository mockRepository;

        [TestInitialize]
        public void TestInitialize()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);


        }

        private ScoCollection<string> CreateScoCollection()
        {
            return new ScoCollection<string>();
        }
        private List<string> _receivedEvents;

        public void CollectionChangedHandler(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            _receivedEvents.Add(e.Action.ToString());
        }

        [TestMethod]
        public void CollectionChanged_StateUnderTest_AddRaisesEvent()
        {
            // Arrange
            var scoCollection = this.CreateScoCollection();
            
            //var receivedEvents = new List<string>();
            //scoCollection.CollectionChanged += delegate (object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
            //{
            //    receivedEvents.Add(e.Action.ToString());
            //};

            _receivedEvents = new List<string>();
            scoCollection.CollectionChanged += CollectionChangedHandler;

            // Act
            scoCollection.Add("One");
            scoCollection.Add("Two");

            // Assert
            Assert.AreEqual(2, _receivedEvents.Count);
            Assert.AreEqual("One", _receivedEvents[0]);
            Assert.AreEqual("Two", _receivedEvents[1]);
        }

        [TestMethod]
        public void ToList_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var scoCollection = this.CreateScoCollection();

            // Act
            var result = scoCollection.ToList();

            // Assert
            Assert.Fail();
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        public void FromList_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var scoCollection = this.CreateScoCollection();
            IList<string> value = null;

            // Act
            scoCollection.FromList(
                value);

            // Assert
            Assert.Fail();
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        public void Serialize_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var scoCollection = this.CreateScoCollection();

            // Act
            scoCollection.Serialize();

            // Assert
            Assert.Fail();
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        public void Serialize_StateUnderTest_ExpectedBehavior1()
        {
            // Arrange
            var scoCollection = this.CreateScoCollection();
            string filePath = null;

            // Act
            scoCollection.Serialize(
                filePath);

            // Assert
            Assert.Fail();
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        public async Task SerializeAsync_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var scoCollection = this.CreateScoCollection();

            // Act
            await scoCollection.SerializeAsync();

            // Assert
            Assert.Fail();
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        public async Task SerializeAsync_StateUnderTest_ExpectedBehavior1()
        {
            // Arrange
            var scoCollection = this.CreateScoCollection();
            string filePath = null;

            // Act
            await scoCollection.SerializeAsync(
                filePath);

            // Assert
            Assert.Fail();
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        public void SerializeThreadSafe_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var scoCollection = this.CreateScoCollection();
            string filePath = null;

            // Act
            scoCollection.SerializeThreadSafe(
                filePath);

            // Assert
            Assert.Fail();
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        public void Deserialize_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var scoCollection = this.CreateScoCollection();

            // Act
            scoCollection.Deserialize();

            // Assert
            Assert.Fail();
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        public void Deserialize_StateUnderTest_ExpectedBehavior1()
        {
            // Arrange
            var scoCollection = this.CreateScoCollection();
            bool askUserOnError = false;

            // Act
            scoCollection.Deserialize(
                askUserOnError);

            // Assert
            Assert.Fail();
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        public void Deserialize_StateUnderTest_ExpectedBehavior2()
        {
            // Arrange
            var scoCollection = this.CreateScoCollection();
            string fileName = null;
            string folderPath = null;
            bool askUserOnError = false;

            // Act
            scoCollection.Deserialize(
                fileName,
                folderPath,
                askUserOnError);

            // Assert
            Assert.Fail();
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        public void Deserialize_StateUnderTest_ExpectedBehavior3()
        {
            // Arrange
            var scoCollection = this.CreateScoCollection();
            string fileName = null;
            string folderPath = null;
            ScoCollection<string>.AltListLoader backupLoader = null;
            string backupFilepath = null;
            bool askUserOnError = false;

            // Act
            scoCollection.Deserialize(
                fileName,
                folderPath,
                backupLoader,
                backupFilepath,
                askUserOnError);

            // Assert
            Assert.Fail();
            this.mockRepository.VerifyAll();
        }
    }
}
