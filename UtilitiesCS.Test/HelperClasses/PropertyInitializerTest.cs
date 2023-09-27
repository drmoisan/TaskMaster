using Deedle.Internal;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using UtilitiesCS;
using System.Collections.Generic;

namespace UtilitiesCS.Test.HelperClasses
{
    [TestClass]
    public class PropertyInitializerTest
    {
        [TestMethod]
        public void DependenciesNotNull_ExpectedState_True()
        {
            bool strict = false;
            string variable1 = "dummy";
            List<bool> variable2 = new List<bool>();
            int variable3 = 1;

            var test = Initializer.DependenciesNotNull(strict, variable1, variable2, variable3);
            Assert.IsTrue(test);
        }
        [TestMethod]
        public void DependenciesNotNull_ExpectedState_False_ParamsIsNull()
        {
            bool strict = false;
            var test = Initializer.DependenciesNotNull(strict, null);
            Assert.IsFalse(test);
        }

        [TestMethod]
        public void DependenciesNotNull_ExpectedState_False_ParamsIsEmpty()
        {
            bool strict = false;
            var test = Initializer.DependenciesNotNull(strict);
            Assert.IsFalse(test);
        }

        [TestMethod]
        public void DependenciesNotNull_ExpectedState_False_MultipleParamsNull()
        {
            bool strict = false;
            string variable1 = null;
            List<bool> variable2 = null;
            int variable3 = 1;
            var test = Initializer.DependenciesNotNull(strict, null);
            Assert.IsFalse(test);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void DependenciesNotNull_ExpectedState_Exception_ParamsIsNull()
        {
            bool strict = true;
            var test = Initializer.DependenciesNotNull(strict, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void DependenciesNotNull_ExpectedState_Exception_ParamsIsEmpty()
        {
            bool strict = true;
            var test = Initializer.DependenciesNotNull(strict);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void DependenciesNotNull_ExpectedState_Exception_MultipleParamsNull()
        {
            bool strict = true;
            string variable1 = null;
            List<bool> variable2 = null;
            int variable3 = 1;
            var test = Initializer.DependenciesNotNull(strict, null);
            Assert.IsFalse(test);
        }
    }
}
