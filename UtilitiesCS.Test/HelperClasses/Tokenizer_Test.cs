using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;

namespace UtilitiesCS.Test
{
    [TestClass]
    public class Tokenizer_Test
    {
        [TestMethod]
        public void AsRegexWord_CharsNull()
        {
            char[] chars = null;
            string target = @"\w";
            string test = chars.AsRegexWord();
            Assert.AreEqual(target, test);
        }

        [TestMethod]
        public void AsRegexWord_CharsEmpty()
        {
            char[] chars = { };
            string target = @"\w";
            string test = chars.AsRegexWord();
            Assert.AreEqual(target, test);
        }

        [TestMethod]
        public void AsRegexWord_CharsEscaped()
        {
            char[] chars = { '&', '!' };
            string target = @"[\w&!]";
            string test = chars.AsRegexWord();
            Assert.AreEqual(target, test);
        }

        [TestMethod]
        public void GetTokenPattern_StandardWord()
        {
            string test = Tokenizer.GetTokenPattern(@"\w", 2);
            string target = @"\b\w\w+\b";
            Assert.AreEqual(target, test);
        }

        [TestMethod]
        public void GetTokenPattern_ExpandedWord()
        {
            string test = Tokenizer.GetTokenPattern(@"[\w&!]", 2);
            string target = @"\b[\w&!][\w&!]+\b";
            Assert.AreEqual(target, test);
        }

        [TestMethod]
        public void GetTokenPattern_Min0()
        {
            string test = Tokenizer.GetTokenPattern(@"[\w&!]", 0);
            string target = @"\b[\w&!]+\b";
            Assert.AreEqual(target, test);
        }

        [TestMethod]
        public void GetTokenPattern_Min1()
        {
            string test = Tokenizer.GetTokenPattern(@"[\w&!]", 1);
            string target = @"\b[\w&!]+\b";
            Assert.AreEqual(target, test);
        }

        [TestMethod]
        public void GetTokenPattern_Min4()
        {
            string test = Tokenizer.GetTokenPattern(@"[\w&!]", 4);
            string target = @"\b[\w&!][\w&!][\w&!][\w&!]+\b";
            Assert.AreEqual(target, test);
        }
                
        [TestMethod]
        public void Tokenize_SpecialChar()
        {
            string doc = @" T&E and o.ther";
            var regex = new Regex(@"\b[\w&][\w&]+\b");
            string[] test = doc.Tokenize(regex);
            string[] target = { "t&e", "and", "ther" };
            Assert.IsTrue(test.SequenceEqual(target));
        }

        [TestMethod]
        public void Tokenize_Standard()
        {
            string doc = @" T&E and o.ther";
            var regex = new Regex(@"\b\w\w+\b");
            string[] test = doc.Tokenize(regex);
            string[] target = { "and", "ther" };
            Assert.IsTrue(test.SequenceEqual(target));
        }

        [TestMethod]
        public void Tokenize_StandardEmpty()
        {
            string doc = @"T&E";
            var regex = new Regex(@"\b\w\w+\b");
            string[] test = doc.Tokenize(regex);
            string[] target = {};
            Assert.IsTrue(test.SequenceEqual(target));
        }

        [TestMethod]
        public void Tokenize_SpecialCharOneMatch()
        {
            string doc = @"T&E";
            var regex = new Regex(@"\b[\w&][\w&]+\b");
            string[] test = doc.Tokenize(regex);
            string[] target = { "t&e"};
            Assert.IsTrue(test.SequenceEqual(target));
        }
    }
}
