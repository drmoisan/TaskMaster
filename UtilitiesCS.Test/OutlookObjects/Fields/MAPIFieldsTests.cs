using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS.OutlookObjects.Fields;

namespace UtilitiesCS.Test.OutlookObjects.Fields
{
    [TestClass]
    public class MAPIFieldsTests
    {
        [TestMethod]
        public void Known_property_tag_returns_expected_constant()
        {
            MAPIFields.FieldToSchema["Store"].Should().Contain("0x0FFB");
        }

        [TestMethod]
        public void Unknown_property_tag_returns_safe_result()
        {
            MAPIFields.FieldToSchema.TryGetValue("__missing__", out var value).Should().BeFalse();
            value.Should().BeNull();
        }
    }
}
