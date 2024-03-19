using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using UtilitiesCS.EmailIntelligence;

namespace UtilitiesCS.Test.EmailIntelligence
{
    [TestClass]
    public class EmailTokenizerTests
    {
        [TestMethod]
        public void commonprefix_Test()
        {
            // arrange
            string[] strings = new string[] { "abc", "abcd", "abcde" };
            var expected = "abc";
            var tokenizer = new EmailTokenizer();
            
            // act
            var actual = tokenizer.commonprefix(strings);

            // assert
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void commonsuffix_Test()
        {
            // arrange
            string[] strings = new string[] { "cba", "dcba", "edcba" };
            var expected = "cba";
            var tokenizer = new EmailTokenizer();

            // act
            var actual = tokenizer.commonsuffix(strings);

            // assert
            Assert.AreEqual(expected, actual);
        }
    }
}
