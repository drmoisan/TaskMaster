using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Office.Interop.Outlook;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UtilitiesCS;

namespace Tags.Test
{
    /// <summary>
    /// Unit tests for the extracted <see cref="LauncherAutoAssign"/> delegate-wiring type. Each
    /// pass-through forwards to its injected delegate; no live form or Outlook host is used.
    /// </summary>
    [TestClass]
    public class LauncherAutoAssignTests
    {
        [TestMethod]
        public void AddChoicesToDict_ForwardsMailItemToDelegate()
        {
            var mail = new Mock<MailItem>().Object;
            Func<MailItem, IList<string>> addChoices = m => new List<string> { "Alpha", "Beta" };
            var autoAssign = new LauncherAutoAssign(null, addChoices, null, null);

            autoAssign
                .AddChoicesToDict(mail, new List<IPrefix>(), "Program", "user@example.test")
                .Should()
                .Equal("Alpha", "Beta");
        }

        [TestMethod]
        public void AddColorCategory_ForwardsPrefixAndNameToDelegate()
        {
            var category = new Mock<Category>().Object;
            var prefix = new Mock<IPrefix>().Object;
            Func<IPrefix, string, Category> addColor = (p, n) => category;
            var autoAssign = new LauncherAutoAssign(null, null, addColor, null);

            autoAssign.AddColorCategory(prefix, "New Category").Should().BeSameAs(category);
        }

        [TestMethod]
        public void AutoFind_ForwardsItemToDelegate()
        {
            Func<object, IList<string>> autoFind = o => new List<string> { "Result" };
            var autoAssign = new LauncherAutoAssign(null, null, null, autoFind);

            autoAssign.AutoFind("item").Should().Equal("Result");
        }

        [TestMethod]
        public async Task AutoFindAsync_RunsSyncDelegateAndReturnsResult()
        {
            Func<object, IList<string>> autoFind = o => new List<string> { "Async Result" };
            var autoAssign = new LauncherAutoAssign(null, null, null, autoFind);

            var result = await autoAssign.AutoFindAsync("item");

            result.Should().Equal("Async Result");
        }

        [TestMethod]
        public void Properties_RoundTripFilterListAndDelegates()
        {
            var autoAssign = new LauncherAutoAssign();
            var filterList = new List<string> { "Filtered" };
            Func<MailItem, IList<string>> addChoices = m => null;
            Func<IPrefix, string, Category> addColor = (p, n) => null;
            Func<object, IList<string>> autoFind = o => null;

            autoAssign.FilterList = filterList;
            autoAssign.AddChoicesToDictDelegate = addChoices;
            autoAssign.AddColorCategoryDelegate = addColor;
            autoAssign.AutoFindDelegate = autoFind;

            autoAssign.FilterList.Should().BeSameAs(filterList);
            autoAssign.AddChoicesToDictDelegate.Should().BeSameAs(addChoices);
            autoAssign.AddColorCategoryDelegate.Should().BeSameAs(addColor);
            autoAssign.AutoFindDelegate.Should().BeSameAs(autoFind);
        }

        [TestMethod]
        public void GetAutoAssign_BuildsConfiguredInstance()
        {
            var filterList = new List<string> { "Filtered" };

            var autoAssign = LauncherAutoAssign.GetAutoAssign(
                filterList,
                m => null,
                (p, n) => null,
                o => null
            );

            autoAssign.Should().BeOfType<LauncherAutoAssign>();
            autoAssign.FilterList.Should().BeSameAs(filterList);
        }
    }
}
