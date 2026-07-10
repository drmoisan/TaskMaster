using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Office.Interop.Outlook;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using TaskVisualization;
using UtilitiesCS;

namespace TaskVisualization.Test
{
    /// <summary>
    /// Unit tests for the host-neutral branches of <see cref="AutoAssignPeople"/>.
    /// The recipient-matching classifier (RunPeopleClassifier / AutoFindPeople) is
    /// COM/dialog-bound and exempt; the synchronous MailItemHelper seam is exercised
    /// with a stub so no live Outlook process, popup, or temp file is used.
    /// </summary>
    [TestClass]
    public class AutoAssignPeopleTests
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
            var sut = new AutoAssignPeople(BuildGlobals(new List<string> { "p", "q" }).Object);
            sut.FilterList.Should().BeEquivalentTo(new[] { "p", "q" });
        }

        [TestMethod]
        public void AutoFind_Null_ReturnsEmpty()
        {
            var sut = new AutoAssignPeople(BuildGlobals().Object);
            sut.AutoFind(null).Should().BeEmpty();
        }

        [TestMethod]
        public void AutoFind_UnknownType_ReturnsEmpty()
        {
            var sut = new AutoAssignPeople(BuildGlobals().Object);
            sut.AutoFind("an unrecognized object").Should().BeEmpty();
        }

        [TestMethod]
        public async Task AutoFindAsync_Null_ReturnsEmpty()
        {
            var sut = new AutoAssignPeople(BuildGlobals().Object);
            (await sut.AutoFindAsync(null)).Should().BeEmpty();
        }

        [TestMethod]
        public void AutoFind_MailItemBranch_RoutesThroughToHelperSeam()
        {
            // The MailItem branch constructs the helper via the synchronous _toHelper
            // seam before reaching the exempt classifier. A throwing stub proves the
            // seam is on the AutoFind execution path and controls its behavior,
            // without invoking the COM/dialog-bound recipient matcher.
            var seamInvoked = false;
            MailItemHelper ThrowingSeam(object o)
            {
                seamInvoked = true;
                throw new InvalidOperationException("seam-invoked");
            }

            var sut = new AutoAssignPeople(BuildGlobals().Object, toHelper: ThrowingSeam);
            var mail = new Mock<MailItem>().Object;

            System.Action act = () => sut.AutoFind(mail);

            act.Should().Throw<InvalidOperationException>().WithMessage("seam-invoked");
            seamInvoked.Should().BeTrue();
        }
    }
}
