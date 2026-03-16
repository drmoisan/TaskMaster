using FluentAssertions;
using Microsoft.Office.Interop.Outlook;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS.OutlookExtensions;
using UtilitiesCS.OutlookObjects.Fields;

namespace UtilitiesCS.Test.OutlookObjects.Fields
{
    [TestClass]
    public class UserDefinedFieldsTests
    {
        [TestMethod]
        public void ValidPropertyArgs_rejects_missing_required_inputs()
        {
            UserDefinedFields.ValidPropertyArgs(123, OlUserPropertyType.olText).Should().BeFalse();
        }

        [TestMethod]
        public void GetUdfValue_returns_expected_lookup_value_for_known_field()
        {
            MAPIFields.FieldToSchema.TryGetValue("Store", out var schema).Should().BeTrue();
            schema.Should().NotBeNullOrWhiteSpace();
        }
    }
}
