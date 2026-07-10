using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using TaskVisualization;
using UtilitiesCS;

namespace TaskVisualization.Test
{
    /// <summary>
    /// Unit tests for the host-neutral members of <see cref="AutoAssignContext"/>.
    /// The MailItemHelper seam is stubbed so no live Outlook process, classifier
    /// engine, popup, or temp file is used.
    /// </summary>
    [TestClass]
    public class AutoAssignContextTests
    {
        private static Mock<IApplicationGlobals> BuildGlobals(IList<string> categoryFilters = null)
        {
            var td = new Mock<IToDoObjects>();
            var cfList = categoryFilters ?? new List<string> { "a", "b" };
            td.Setup(x => x.CategoryFilters).Returns(BuildFilterList(cfList));

            var globals = new Mock<IApplicationGlobals>();
            globals.Setup(x => x.TD).Returns(td.Object);
            return globals;
        }

        private static ISerializableList<string> BuildFilterList(IList<string> items)
        {
            var cf = new Mock<ISerializableList<string>>();
            cf.Setup(x => x.Count).Returns(() => items.Count);
            cf.Setup(x => x[It.IsAny<int>()]).Returns((int i) => items[i]);
            cf.Setup(x => x.CopyTo(It.IsAny<string[]>(), It.IsAny<int>()))
                .Callback((string[] arr, int i) => items.CopyTo(arr, i));
            cf.As<IEnumerable<string>>()
                .Setup(x => x.GetEnumerator())
                .Returns(() => items.GetEnumerator());
            return cf.Object;
        }

        [TestMethod]
        public void FilterList_ReturnsCategoryFilters()
        {
            var sut = new AutoAssignContext(BuildGlobals(new List<string> { "x", "y" }).Object);
            sut.FilterList.Should().BeEquivalentTo(new[] { "x", "y" });
        }

        [TestMethod]
        public void AddChoicesToDict_Throws_NotImplemented()
        {
            var sut = new AutoAssignContext(BuildGlobals().Object);
            Action act = () => sut.AddChoicesToDict(null, null, null, null);
            act.Should().Throw<NotImplementedException>();
        }

        [TestMethod]
        public void AddColorCategory_Throws_NotImplemented()
        {
            var sut = new AutoAssignContext(BuildGlobals().Object);
            Action act = () => sut.AddColorCategory(null, null);
            act.Should().Throw<NotImplementedException>();
        }

        [TestMethod]
        public void AutoFind_Throws_NotImplemented()
        {
            var sut = new AutoAssignContext(BuildGlobals().Object);
            Action act = () => sut.AutoFind(null);
            act.Should().Throw<NotImplementedException>();
        }

        [TestMethod]
        public async Task AutoFindAsync_HelperSeamReturnsNull_ReturnsEmpty()
        {
            var sut = new AutoAssignContext(
                BuildGlobals().Object,
                toHelper: _ => Task.FromResult<MailItemHelper>(null)
            );

            var result = await sut.AutoFindAsync(new object());

            result.Should().BeEmpty();
        }
    }
}
