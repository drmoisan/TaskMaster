using Microsoft.TeamFoundation.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
//using ToDoModel;


namespace ToDoModel.Test
{
    [TestClass]
    public class FlagParser_Test
    {
        [TestInitialize] public void Init() 
        {
            MakeSearchPattern = TempClass.MakeSearchPattern;
            MakeReplacePattern = TempClass.MakeReplacePattern;
        }

        private delegate string PatternDelegate(string searchString);
        private PatternDelegate MakeSearchPattern;
        private PatternDelegate MakeReplacePattern;

        
        [TestMethod]
        public void MakeSearchPattern_Simple()
        {
            string input = "abc*123*def";
            string expected = "^abc(.*)123(.*)def$";
            string test = MakeSearchPattern(input);
            Assert.AreEqual(expected, test);
        }
        
        [TestMethod]
        public void MakeSearchPattern_AltWC()
        {
            string input = "abc%123%def";
            string expected = "^abc(.*)123(.*)def$";
            string test = MakeSearchPattern(input);
            Assert.AreEqual(expected, test);
        }

        [TestMethod]
        public void MakeSearchPattern_Bookend()
        {
            string input = "*abc*123*def*";
            string expected = "^(.*)abc(.*)123(.*)def(.*)$";
            string test = MakeSearchPattern(input);
            Assert.AreEqual(expected, test);
        }

        [TestMethod]
        public void MakeSearchPattern_BookendOnly()
        {
            string input = "*abc123def*";
            string expected = "^(.*)abc123def(.*)$";
            string test = MakeSearchPattern(input);
            Assert.AreEqual(expected, test);
        }

        [TestMethod]
        public void MakeReplacePattern_Simple()
        {
            string input = "^abc(.*)123(.*)def$";
            string expected = "$1$2";
            string test = MakeReplacePattern(input);
            Assert.AreEqual(expected, test);
        }

        [TestMethod]
        public void MakeReplacePattern_Bookend()
        {
            string input = "^(.*)abc(.*)123(.*)def(.*)$";
            string expected = "$1$2$3$4";
            string test = MakeReplacePattern(input);
            Assert.AreEqual(expected, test);
        }

        [TestMethod]
        public void MakeReplacePattern_BookendOnly()
        {
            string input = "^(.*)abc123def(.*)$";
            string expected = "$1$2";
            string test = MakeReplacePattern(input);
            Assert.AreEqual(expected, test);
        }

        


    }
}
