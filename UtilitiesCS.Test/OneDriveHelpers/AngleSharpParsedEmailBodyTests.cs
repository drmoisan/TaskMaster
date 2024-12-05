using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using UtilitiesCS.OneDriveHelpers;
using FluentAssertions;
using System.Linq;
using UtilitiesCS.Extensions.Lazy;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace UtilitiesCS.Test.OneDriveHelpers
{
    [TestClass]
    public class AngleSharpParsedEmailBodyTests
    {
        private AngleSharpParsedEmailBodyDerived parsed; 

        [TestInitialize]
        public void TestInitialize()
        {
            Console.SetOut(new DebugTextWriter());
            //this.mockRepository = new MockRepository(MockBehavior.Loose);
        }

        public class AngleSharpParsedEmailBodyDerived : AngleSharpParsedEmailBody
        {
            public AngleSharpParsedEmailBodyDerived() : base("") { }
            public void SetLinks(IEnumerable<(string, string)> links) => Links = links;
        }

        [TestMethod]
        public void ExtractLinks_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var htmlBody = Properties.Resources.HtmlBodyLinks;
            var body = new AngleSharpParsedEmailBody(htmlBody).ExtractLinks();
            var expected = new (string, string)[] 
            { 
                (" 10z Week 38 Rollback CID.xlsx","https://sabradipping.sharepoint.com/:x:/s/Sales-LargeFormatTeam2/EVpXvJ8dfZ9DmaoSjGJ16uIBuc7kN7LG9esDj9pciuoriA"), 
                (" Snackers Week 37 Rollback CID.xlsx","https://sabradipping.sharepoint.com/:x:/s/Sales-LargeFormatTeam2/EZckiLtanGtMjumaLHLwsCsBwQsHSfAuMpzsMyfcIPUTdg?email=dmoisan%40sabra.com&e=YRrQJk"),
                ("trindels@sabra.com","mailto:chollingsworth@sabra.com")
            };

            // Act
            var actual = body.Links;

            // Assert
            Console.WriteLine("\nExpected:");
            expected.ForEach(x => Console.WriteLine(x));
            Console.WriteLine("\nActual:");
            actual.ForEach(x => Console.WriteLine(x));

            actual.Should().BeEquivalentTo(expected);
        }

        [TestMethod]
        public void FilterLinksByDomain_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var source = new (string, string)[]
            {
                (" 10z Week 38 Rollback CID.xlsx","https://sabradipping.sharepoint.com/:x:/s/Sales-LargeFormatTeam2/EVpXvJ8dfZ9DmaoSjGJ16uIBuc7kN7LG9esDj9pciuoriA"),
                (" Snackers Week 37 Rollback CID.xlsx","https://sabradipping.sharepoint.com/:x:/s/Sales-LargeFormatTeam2/EZckiLtanGtMjumaLHLwsCsBwQsHSfAuMpzsMyfcIPUTdg?email=dmoisan%40sabra.com&e=YRrQJk"),
                ("trindels@sabra.com","mailto:chollingsworth@sabra.com")
            };
            var expected = new (string, string)[]
            {
                (" 10z Week 38 Rollback CID.xlsx","https://sabradipping.sharepoint.com/:x:/s/Sales-LargeFormatTeam2/EVpXvJ8dfZ9DmaoSjGJ16uIBuc7kN7LG9esDj9pciuoriA"),
                (" Snackers Week 37 Rollback CID.xlsx","https://sabradipping.sharepoint.com/:x:/s/Sales-LargeFormatTeam2/EZckiLtanGtMjumaLHLwsCsBwQsHSfAuMpzsMyfcIPUTdg?email=dmoisan%40sabra.com&e=YRrQJk")
            };
            
            string domain = "sabradipping.sharepoint.com";

            // Act
            parsed = new();
            parsed.SetLinks(source);
            var actual = parsed.FilterLinksByDomain(domain).FilteredLinks;

            //// Assert
            Console.WriteLine("\nExpected:");
            expected.ForEach(x => Console.WriteLine(x));
            Console.WriteLine("\nActual:");
            actual.ForEach(x => Console.WriteLine(x));

            actual.Should().BeEquivalentTo(expected);
            //this.mockRepository.VerifyAll();
        }

        
    }
}
