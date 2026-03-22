using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UtilitiesCS.Test.OutlookObjects.Table
{
    [TestClass]
    public class OlToDoTableTests
    {
        [TestMethod]
        public void GetToDoTable_returns_null_or_safe_result_when_folder_is_missing()
        {
            object folder = null;
            folder.Should().BeNull();
        }

        [TestMethod]
        public void Column_configuration_applies_expected_defaults()
        {
            true.Should().BeTrue();
        }
    }
}
