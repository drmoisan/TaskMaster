using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Microsoft.Office.Interop.Outlook;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using TaskVisualization;
using UtilitiesCS;

namespace TaskVisualization.Test
{
    /// <summary>
    /// Unit tests for the host-neutral <see cref="FlagChangeGroup.TryEnqueue"/> seam.
    /// The Outlook-bound members are never exercised; the group is built with Moq
    /// stubs so no live MailItem is constructed. No form, popup, sleep, timer, or
    /// temp file is used.
    /// </summary>
    [TestClass]
    public class FlagChangeGroupTests
    {
        private static FlagChangeGroup CreateGroup()
        {
            var globals = new Mock<IApplicationGlobals>().Object;
            var mail = new Mock<MailItem>();
            mail.Setup(x => x.Subject).Returns("test-subject");
            return new FlagChangeGroup(globals, mail.Object);
        }

        [TestMethod]
        public void TryEnqueue_NoDifference_ReturnsFalse_AndEnqueuesNothing()
        {
            var group = CreateGroup();

            var result = group.TryEnqueue("People", new[] { "x", "y" }, new[] { "x", "y" });

            result.Should().BeFalse();
            group.FlagChangeItems.Should().BeEmpty();
        }

        [TestMethod]
        public void TryEnqueue_AdditionsAndRemovals_ReturnsTrue_WithCorrectFlags()
        {
            var group = CreateGroup();

            var result = group.TryEnqueue(
                "People",
                new[] { "keep", "remove" },
                new[] { "keep", "add" }
            );

            result.Should().BeTrue();
            group.FlagChangeItems.Should().ContainSingle();
            var item = group.FlagChangeItems.Single();
            item.ClassifierName.Should().Be("People");
            item.UntrainFlags.Should().BeEquivalentTo(new[] { "remove" });
            item.TrainFlags.Should().BeEquivalentTo(new[] { "add" });
        }

        [TestMethod]
        public void TryEnqueue_OnlyAdditions_ReturnsTrue_WithTrainFlagsOnly()
        {
            var group = CreateGroup();

            var result = group.TryEnqueue("Context", new string[0], new[] { "added" });

            result.Should().BeTrue();
            var item = group.FlagChangeItems.Single();
            item.TrainFlags.Should().BeEquivalentTo(new[] { "added" });
            item.UntrainFlags.Should().BeEmpty();
        }
    }
}
