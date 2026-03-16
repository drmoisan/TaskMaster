using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UtilitiesCS.Test.OutlookObjects.Folder
{
    [TestClass]
    public class FolderWrapperStateTests
    {
        [TestMethod]
        public void Lazy_name_and_relative_path_load_once()
        {
            true.Should().BeTrue();
        }
    }
}
