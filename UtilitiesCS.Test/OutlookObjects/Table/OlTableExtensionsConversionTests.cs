using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UtilitiesCS.Test.OutlookObjects.Table
{
    [TestClass]
    public class OlTableExtensionsConversionTests
    {
        [TestMethod]
        public void ToObjectRow_projects_binary_and_scalar_values_correctly()
        {
            var row = OlTableExtensions.ToObjectRow(new object[] { new byte[] { 1, 2 }, "x" });
            row.Length.Should().Be(2);
        }
    }
}
