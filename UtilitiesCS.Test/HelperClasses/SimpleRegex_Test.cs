using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace UtilitiesCS.Test
{
    [TestClass]
    public class SimpleRegex_Test
    {
        [TestMethod]
        public void MakeSearchPattern_Simple()
        {
            string input = "abc*123*def";
            string expected = "^abc(.*)123(.*)def$";
            string test = SimpleRegex.MakeSearchPattern(input);
            Assert.AreEqual(expected, test);
        }

        [TestMethod]
        public void MakeSearchPattern_AltWC()
        {
            string input = "abc%123%def";
            string expected = "^abc(.*)123(.*)def$";
            string test = SimpleRegex.MakeSearchPattern(input);
            Assert.AreEqual(expected, test);
        }

        [TestMethod]
        public void MakeSearchPattern_Bookend()
        {
            string input = "*abc*123*def*";
            string expected = "^(.*)abc(.*)123(.*)def(.*)$";
            string test = SimpleRegex.MakeSearchPattern(input);
            Assert.AreEqual(expected, test);
        }

        [TestMethod]
        public void MakeSearchPattern_BookendOnly()
        {
            string input = "*abc123def*";
            string expected = "^(.*)abc123def(.*)$";
            string test = SimpleRegex.MakeSearchPattern(input);
            Assert.AreEqual(expected, test);
        }

        [TestMethod]
        public void MakeReplacePattern_Simple()
        {
            string input = "^abc(.*)123(.*)def$";
            string expected = "$1$2";
            string test = SimpleRegex.MakeReplacePattern(input);
            Assert.AreEqual(expected, test);
        }

        [TestMethod]
        public void MakeReplacePattern_Bookend()
        {
            string input = "^(.*)abc(.*)123(.*)def(.*)$";
            string expected = "$1$2$3$4";
            string test = SimpleRegex.MakeReplacePattern(input);
            Assert.AreEqual(expected, test);
        }

        [TestMethod]
        public void MakeReplacePattern_BookendOnly()
        {
            string input = "^(.*)abc123def(.*)$";
            string expected = "$1$2";
            string test = SimpleRegex.MakeReplacePattern(input);
            Assert.AreEqual(expected, test);
        }
    }
}
