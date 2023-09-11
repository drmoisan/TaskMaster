using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System;
using UtilitiesCS.EmailIntelligence;
using System.Collections;
using System.Collections.Generic;

namespace UtilitiesCS.Test
{
    [TestClass]
    public class CommonWords_Test
    {
        [TestMethod]
        public void Tokenize_Test()
        {
            string original = "Dale ... Estás añadiendo una prueba?";
            string[] target = { "dale", "estás", "añadiendo", "una", "prueba" };
            string[] test = original.Tokenize();
            CollectionAssert.AreEqual(test, target);
        }

        [TestMethod]
        public void StripAccents_Test()
        {
            string[] original = { "dale", "estás", "añadiendo", "una", "prueba" };
            string[] target = { "dale","estas","anadiendo","una","prueba" };
            string[] test = (from string word in original select word.StripAccents()).ToArray();
            CollectionAssert.AreEqual(test, target);
        }

        [TestMethod]
        public void StripCommonWords_Test()
        {
            string original = "Fwd: Dale ... Estás añadiendo una prueba?";
            string target = "dale anadiendo prueba";
            IList<string> stopWords = new List<string>{ "re", "el", "ella", "fwd", "estás", "una" };
            string test = original.StripCommonWords(stopWords);
            Assert.AreEqual(test, target);
        }
    }
}
