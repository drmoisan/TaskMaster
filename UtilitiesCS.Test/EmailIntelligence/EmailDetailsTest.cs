using Microsoft.FSharp.Data.UnitSystems.SI.UnitNames;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using UtilitiesCS;
using FluentAssertions;

namespace UtilitiesCS.Test.EmailIntelligence
{
    [TestClass]
    public class EmailDetailsTest
    {
        [TestMethod]
        public void ExtractNameFromAddress_Test()
        {
            // Arrange
            string[] addresses = ["first.last@domain.com", "\"a@b\"@example.com", "first.last.contractor@domain.com", "other@domain.com", "first.i.last@domain.com"];
            string[][] expected = [["first", "last", "domain.com"], ["a@b", null, "example.com"], ["first", "last", "domain.com"], ["other", null, "domain.com"], ["first","last","domain.com"]];
            string[][] actual;

            // Act
            actual = addresses.Select(address => EmailDetails.ExtractNameFromAddress(address)).Select(x => new string[] { x.FirstName, x.LastName, x.DomainName }).ToArray();

            Console.WriteLine($"Addresses:\n{string.Join("\n", addresses)}\n");
            Console.WriteLine($"\n{expected.ToFormattedText(["First Name", "Last Name", "Domain"],"EXPECTED")}");
            Console.WriteLine($"\n{actual.ToFormattedText(["First Name", "Last Name", "Domain"], "ACTUAL")}");

            // Assert

            actual.Should().BeEquivalentTo(expected);
            
        }
    }
}
