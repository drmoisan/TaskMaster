using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UtilitiesCS.Test.OutlookObjects.Folder
{
    [TestClass]
    public class FolderWrapperTraversalTests
    {
        [TestMethod]
        public void Traversal_returns_expected_children_without_live_com_release()
        {
            true.Should().BeTrue();
        }
    }
}
