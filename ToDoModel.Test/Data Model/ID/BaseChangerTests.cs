using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using ToDoModel;
using System.Numerics;


namespace ToDoModel.Test
{
	[TestClass]
    public class BaseChangerTests
    {
        private MockRepository mockRepository;



        [TestInitialize]
        public void TestInitialize()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);


        }

        [TestMethod]
        public void ToBase10_StateUnderTest_ExpectedBehavior_10()
        {
            // Arrange
            string input = "10";
            BigInteger expected = 36;
            int nbase = 36;

            // Act
            var actual = BaseChanger.ToBase10(
                input,
                nbase);

            // Assert
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void ToBase10_StateUnderTest_ExpectedBehavior_11()
        {
            // Arrange
            string input = "11";
            BigInteger expected = 37;
            int nbase = 36;

            // Act
            var actual = BaseChanger.ToBase10(
                input,
                nbase);

            // Assert
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void ToBase10_StateUnderTest_ExpectedBehavior_12()
        {
            // Arrange
            string input = "12";
            BigInteger expected = 38;
            int nbase = 36;

            // Act
            var actual = BaseChanger.ToBase10(
                input,
                nbase);

            // Assert
            Assert.AreEqual(expected, actual);
        }
        
        [TestMethod]
        public void ToBase10_StateUnderTest_ExpectedBehavior_1Z()
        {
            // Arrange
            string input = "1Z";
            BigInteger expected = 71;
            int nbase = 36;

            // Act
            var actual = BaseChanger.ToBase10(
                input,
                nbase);

            // Assert
            Assert.AreEqual(expected, actual);
        }


        [TestMethod]
        public void ToBase10_StateUnderTest_ExpectedBehavior_131Z()
        {
            // Arrange
            string input = "131Z";
            BigInteger expected = 50615;
            int nbase = 36;

            // Act
            var actual = BaseChanger.ToBase10(
                input,
                nbase);

            // Assert
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void ToBase36_StateUnderTest_ExpectedBehavior_50615()
        {
            // Arrange

            BigInteger input = 50615;
            int nbase = 36;
            int intMinDigits = 2;
            string expected = "131Z";

            // Act
            var actual = input.ToBase(
                nbase,
                intMinDigits);

            // Assert
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void ToBase36_StateUnderTest_ExpectedBehavior_37()
        {
            // Arrange

            BigInteger input = 37;
            int nbase = 36;
            int intMinDigits = 2;
            string expected = "11";

            // Act
            var actual = input.ToBase(
                nbase,
                intMinDigits);

            // Assert
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void ToBase36_StateUnderTest_ExpectedBehavior_36()
        {
            // Arrange

            BigInteger input = 36;
            int nbase = 36;
            int intMinDigits = 2;
            string expected = "10";

            // Act
            var actual = input.ToBase(
                nbase,
                intMinDigits);

            // Assert
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void ToBase36_StateUnderTest_ExpectedBehavior_38()
        {
            // Arrange

            BigInteger input = 38;
            int nbase = 36;
            int intMinDigits = 2;
            string expected = "12";

            // Act
            var actual = input.ToBase(
                nbase,
                intMinDigits);

            // Assert
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void ToBase36_StateUnderTest_ExpectedBehavior_71()
        {
            // Arrange

            BigInteger input = 71;
            int nbase = 36;
            int intMinDigits = 2;
            string expected = "1Z";

            // Act
            var actual = input.ToBase(
                nbase,
                intMinDigits);

            // Assert
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void ToBase36_StateUnderTest_ExpectedBehavior_73()
        {
            // Arrange

            BigInteger input = 73;
            int nbase = 36;
            int intMinDigits = 2;
            string expected = "21";

            // Act
            var actual = input.ToBase(
                nbase,
                intMinDigits);

            // Assert
            Assert.AreEqual(expected, actual);
        }

        //[TestMethod]
        //public void ToBase10_StateUnderTest_ExpectedBehavior()
        //{
        //    // Arrange
        //    var baseChanger = this.CreateBaseChanger();
        //    char c = default(global::System.Char);
        //    int nbase = 0;

        //    // Act
        //    var result = baseChanger.ToBase10(
        //        c,
        //        nbase);

        //    // Assert
        //    Assert.Fail();
        //    this.mockRepository.VerifyAll();
        //}

        //[TestMethod]
        //public void ToBase10_StateUnderTest_ExpectedBehavior1()
        //{
        //    // Arrange
        //    var baseChanger = this.CreateBaseChanger();
        //    string strBase = null;
        //    int nbase = 0;

        //    // Act
        //    var result = baseChanger.ToBase10(
        //        strBase,
        //        nbase);

        //    // Assert
        //    Assert.Fail();
        //    this.mockRepository.VerifyAll();
        //}
    }
}
