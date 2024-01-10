using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace UtilitiesCS.Test.HelperClasses
{
    [TestClass]
    public class PrettyPrintTest
    {
        [TestInitialize]
        public void TestInitialize()
        {
            Console.SetOut(new DebugTextWriter());
        }

        [TestMethod]
        public void ToJustifiedText_AddSpacing_Test()
        {
            // Arrange
            string input = "   Test of the ToJustifiedText method.   ";
            string expected = "T e s t     o f     t h e     T o J u s t i f i e d T e x t     m e t h o d .   ";
            
            // Act
            string actual = input.ToJustifiedText(80);

            Console.WriteLine($"Expected: {expected.Length} {expected}");
            Console.WriteLine($"Actual:   {actual.Length} {actual}");


            // Assert
            Assert.AreEqual(expected, actual);

        }

        [TestMethod]
        public void ToJustifiedText_Truncate_Test()
        {
            // Arrange
            string input = "   Test of the ToJustifiedText method.    ";
            string expected = "Test of the ToJustifiedText meth";

            // Act
            string actual = input.ToJustifiedText(32);

            Console.WriteLine($"Expected: {expected.Length} {expected}");
            Console.WriteLine($"Actual:   {actual.Length} {actual}");


            // Assert
            Assert.AreEqual(expected, actual);

        }
    }
}
