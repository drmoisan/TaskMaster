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
        private static Mock<IApplicationGlobals> BuildGlobals(
            IList<string> categoryFilters = null,
            Mock<IPeopleScoDictionaryNew> people = null
        )
        {
            var td = new Mock<IToDoObjects>();
            var cfList = categoryFilters ?? new List<string> { "a", "b" };
            td.Setup(x => x.CategoryFilters).Returns(BuildFilterList(cfList));
            if (people != null)
            {
                td.Setup(x => x.People).Returns(people.Object);
            }

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

        [TestMethod]
        public void AutoFind_OutlookItemMailBranch_RoutesThroughToHelperSeam()
        {
            // The IOutlookItem-wrapped-mail branch (AutoAssignPeople.cs:70-76) is the branch the
            // corrected People argument (Active.OlItem, the wrapper) reaches after the issue #322
            // fix. It constructs the helper via the synchronous _toHelper seam (called with
            // olItem.InnerObject) before reaching the exempt classifier. A throwing stub proves
            // the seam is on the AutoFind execution path for this wrapper shape, without invoking
            // the COM/dialog-bound recipient matcher.
            var seamInvoked = false;
            object seamArg = null;
            MailItemHelper ThrowingSeam(object o)
            {
                seamInvoked = true;
                seamArg = o;
                throw new InvalidOperationException("seam-invoked");
            }

            var sut = new AutoAssignPeople(BuildGlobals().Object, toHelper: ThrowingSeam);
            var mail = new Mock<MailItem>().Object;
            var outlookItem = new Mock<IOutlookItem>();
            outlookItem.SetupGet(x => x.InnerObject).Returns(mail);

            System.Action act = () => sut.AutoFind(outlookItem.Object);

            act.Should().Throw<InvalidOperationException>().WithMessage("seam-invoked");
            seamInvoked.Should().BeTrue();
            seamArg.Should().BeSameAs(mail);
        }

        [TestMethod]
        public void AddChoicesToDict_PassesMailItemThrough_ReturnsPeopleDictionaryResult()
        {
            // AddChoicesToDict forwards the live MailItem to the injected People
            // dictionary and returns its result verbatim. A Moq IPeopleScoDictionaryNew
            // proves the pass-through without a live Outlook process or recipient data.
            var cannedList = new List<string> { "alice", "bob" };
            var mail = new Mock<MailItem>();
            var people = new Mock<IPeopleScoDictionaryNew>();
            people.Setup(p => p.AddMissingEntries(It.IsAny<MailItem>())).Returns(cannedList);

            var sut = new AutoAssignPeople(BuildGlobals(people: people).Object);

            var result = sut.AddChoicesToDict(mail.Object, null, null, null);

            result.Should().BeEquivalentTo(cannedList);
            people.Verify(p => p.AddMissingEntries(mail.Object), Times.Once);
        }

        [TestMethod]
        public void AddColorCategory_ForwardsPrefixAndName_ReturnsSeamCategory()
        {
            // AddColorCategory delegates to the injected category-creation seam,
            // forwarding the prefix and category name and returning the seam's
            // Category. A stub delegate proves the forwarding without a live MAPI call.
            var prefix = new Mock<IPrefix>().Object;
            var categoryName = "TagAlpha";
            var canned = new Mock<Category>().Object;
            IPrefix receivedPrefix = null;
            string receivedName = null;
            Category StubCreateCategory(IPrefix p, string n)
            {
                receivedPrefix = p;
                receivedName = n;
                return canned;
            }

            var sut = new AutoAssignPeople(
                BuildGlobals().Object,
                createCategory: StubCreateCategory
            );

            var result = sut.AddColorCategory(prefix, categoryName);

            result.Should().BeSameAs(canned);
            receivedPrefix.Should().BeSameAs(prefix);
            receivedName.Should().Be(categoryName);
        }
    }
}
