using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using UtilitiesCS.Extensions;
using FluentAssertions;
using C;

namespace UtilitiesCS.Test.Extensions
{
    [TestClass]
    public class Frexp_Test
    {
        [TestInitialize]
        public void TestInitialize()
        {
            Console.SetOut(new DebugTextWriter());
        }

        [TestMethod]
        public void FrexpTest()
        {
            var inputs = new double[] { 0.00001234, 1234};
            //var actual = inputs.Select(FrexpClass.Frexp).ToList();
            var actual = inputs.Select(input =>
            {
                int exponent = 0;
                double mantissa = math.frexp(input, ref exponent);
                return (mantissa, exponent);
            }).ToList();

            var expected = new (double,int)[] { (0.80871424,-16), (0.6025390625, 11)};
            
            Console.WriteLine($"Actual: {actual}");
            Console.WriteLine($"Expected: {expected}");

            actual.Should().BeEquivalentTo(expected);
        }

    }
}
