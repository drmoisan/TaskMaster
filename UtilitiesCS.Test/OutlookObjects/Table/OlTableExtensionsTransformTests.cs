using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UtilitiesCS.Test.OutlookObjects.Table
{
    [TestClass]
    public class OlTableExtensionsTransformTests
    {
        [TestMethod]
        public void GetColumnDictionary_returns_expected_name_value_pairs()
        {
            var dict = OlTableExtensions.GetColumnDictionary(new[] { "A" }, new object[] { 1 });
            dict["A"].Should().Be(1);
        }
    }
}
