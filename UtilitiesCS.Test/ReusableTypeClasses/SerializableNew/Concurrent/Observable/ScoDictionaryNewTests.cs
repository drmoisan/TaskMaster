using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Threading.Tasks;
using UtilitiesCS.ReusableTypeClasses;

namespace UtilitiesCS.Test.ReusableTypeClasses.SerializableNew.Concurrent.Observable
{
    [TestClass]
    public class ScoDictionaryNewTests
    {
        private MockRepository mockRepository;



        [TestInitialize]
        public void TestInitialize()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);


        }

        public class MyDerivedClass : ScoDictionaryNew<string, string>
        {
            // Class implementation
        }

        #region Commented out tests
        //private ScoDictionaryNew CreateScoDictionaryNew()
        //{
        //    return new ScoDictionaryNew();
        //}

        //[TestMethod]
        //public void Serialize_StateUnderTest_ExpectedBehavior()
        //{
        //    // Arrange
        //    var scoDictionaryNew = this.CreateScoDictionaryNew();

        //    // Act
        //    scoDictionaryNew.Serialize();

        //    // Assert
        //    Assert.Fail();
        //    this.mockRepository.VerifyAll();
        //}

        //[TestMethod]
        //public void Serialize_StateUnderTest_ExpectedBehavior1()
        //{
        //    // Arrange
        //    var scoDictionaryNew = this.CreateScoDictionaryNew();
        //    string filePath = null;

        //    // Act
        //    scoDictionaryNew.Serialize(
        //        filePath);

        //    // Assert
        //    Assert.Fail();
        //    this.mockRepository.VerifyAll();
        //}

        //[TestMethod]
        //public void SerializeThreadSafe_StateUnderTest_ExpectedBehavior()
        //{
        //    // Arrange
        //    var scoDictionaryNew = this.CreateScoDictionaryNew();
        //    string filePath = null;

        //    // Act
        //    scoDictionaryNew.SerializeThreadSafe(
        //        filePath);

        //    // Assert
        //    Assert.Fail();
        //    this.mockRepository.VerifyAll();
        //}

        //[TestMethod]
        //public void SerializeToString_StateUnderTest_ExpectedBehavior()
        //{
        //    // Arrange
        //    var scoDictionaryNew = this.CreateScoDictionaryNew();

        //    // Act
        //    var result = scoDictionaryNew.SerializeToString();

        //    // Assert
        //    Assert.Fail();
        //    this.mockRepository.VerifyAll();
        //}

        //[TestMethod]
        //public void SerializeToStream_StateUnderTest_ExpectedBehavior()
        //{
        //    // Arrange
        //    var scoDictionaryNew = this.CreateScoDictionaryNew();
        //    StreamWriter sw = null;

        //    // Act
        //    scoDictionaryNew.SerializeToStream(
        //        sw);

        //    // Assert
        //    Assert.Fail();
        //    this.mockRepository.VerifyAll();
        //}

        //[TestMethod]
        //public void DeserializeObject_StateUnderTest_ExpectedBehavior()
        //{
        //    // Arrange
        //    var scoDictionaryNew = this.CreateScoDictionaryNew();
        //    string json = null;
        //    JsonSerializerSettings settings = null;

        //    // Act
        //    var result = scoDictionaryNew.DeserializeObject(
        //        json,
        //        settings);

        //    // Assert
        //    Assert.Fail();
        //    this.mockRepository.VerifyAll();
        //}

        //[TestMethod]
        //public void Deserialize_StateUnderTest_ExpectedBehavior()
        //{
        //    // Arrange
        //    var scoDictionaryNew = this.CreateScoDictionaryNew();
        //    string fileName = null;
        //    string folderPath = null;

        //    // Act
        //    var result = scoDictionaryNew.Deserialize(
        //        fileName,
        //        folderPath);

        //    // Assert
        //    Assert.Fail();
        //    this.mockRepository.VerifyAll();
        //}

        //[TestMethod]
        //public void Deserialize_StateUnderTest_ExpectedBehavior1()
        //{
        //    // Arrange
        //    var scoDictionaryNew = this.CreateScoDictionaryNew();
        //    string fileName = null;
        //    string folderPath = null;
        //    bool askUserOnError = false;

        //    // Act
        //    var result = scoDictionaryNew.Deserialize(
        //        fileName,
        //        folderPath,
        //        askUserOnError);

        //    // Assert
        //    Assert.Fail();
        //    this.mockRepository.VerifyAll();
        //}

        //[TestMethod]
        //public void Deserialize_StateUnderTest_ExpectedBehavior2()
        //{
        //    // Arrange
        //    var scoDictionaryNew = this.CreateScoDictionaryNew();
        //    string fileName = null;
        //    string folderPath = null;
        //    bool askUserOnError = false;
        //    JsonSerializerSettings settings = null;

        //    // Act
        //    var result = scoDictionaryNew.Deserialize(
        //        fileName,
        //        folderPath,
        //        askUserOnError,
        //        settings);

        //    // Assert
        //    Assert.Fail();
        //    this.mockRepository.VerifyAll();
        //}

        //[TestMethod]
        //public async Task DeserializeAsync_StateUnderTest_ExpectedBehavior()
        //{
        //    // Arrange
        //    var scoDictionaryNew = this.CreateScoDictionaryNew();
        //    SmartSerializable config = null;

        //    // Act
        //    var result = await scoDictionaryNew.DeserializeAsync(
        //        config);

        //    // Assert
        //    Assert.Fail();
        //    this.mockRepository.VerifyAll();
        //}

        //[TestMethod]
        //public async Task DeserializeAsync_StateUnderTest_ExpectedBehavior1()
        //{
        //    // Arrange
        //    var scoDictionaryNew = this.CreateScoDictionaryNew();
        //    SmartSerializable config = null;
        //    bool askUserOnError = false;

        //    // Act
        //    var result = await scoDictionaryNew.DeserializeAsync(
        //        config,
        //        askUserOnError);

        //    // Assert
        //    Assert.Fail();
        //    this.mockRepository.VerifyAll();
        //}
        
        #endregion Commented out tests

        [TestMethod]
        public void IsDerivedFromScoDictionaryNew_StronglyTypedDerivative_True()
        {
            // Arrange
            Type derivedType = typeof(MyDerivedClass);

            // Act
            var result = derivedType.IsDerivedFrom_ScoDictionaryNew();

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void IsDerivedFromScoDictionaryNew_ObjectTypeDerivative_True()
        {
            // Arrange
            var myDerived = new MyDerivedClass();
            var myObject = (object)myDerived;
            var myObjectType = myObject.GetType();

            // Act
            var result = myObjectType.IsDerivedFrom_ScoDictionaryNew();

            // Assert
            Assert.IsTrue(result);
        }

        //[TestMethod]
        //public void Notify_StateUnderTest_ExpectedBehavior()
        //{
        //    // Arrange
        //    var scoDictionaryNew = this.CreateScoDictionaryNew();
        //    string propertyName = null;

        //    // Act
        //    scoDictionaryNew.Notify(
        //        propertyName);

        //    // Assert
        //    Assert.Fail();
        //    this.mockRepository.VerifyAll();
        //}

        //[TestMethod]
        //public void GetSettingsJson_StateUnderTest_ExpectedBehavior()
        //{
        //    // Arrange
        //    var scoDictionaryNew = this.CreateScoDictionaryNew();
        //    IApplicationGlobals globals = null;

        //    // Act
        //    var result = scoDictionaryNew.GetSettingsJson(
        //        globals);

        //    // Assert
        //    Assert.Fail();
        //    this.mockRepository.VerifyAll();
        //}
    }
}
