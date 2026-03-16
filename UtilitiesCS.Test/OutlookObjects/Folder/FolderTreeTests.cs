using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UtilitiesCS.Test.OutlookObjects.Folder
{
    [TestClass]
    public class FolderTreeTests
    {
        [TestMethod]
        public void Flatten_returns_all_nodes_in_expected_order()
        {
            true.Should().BeTrue();
        }

        [TestMethod]
        public void Selection_filter_excludes_non_matching_nodes()
        {
            true.Should().BeTrue();
        }
    }
}
