using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UtilitiesCS.Test.OutlookObjects.Folder
{
    /// <summary>
    /// Tests for <see cref="UtilitiesCS.FolderNodeViewModel"/>: the INV4 glyph bijection and the
    /// null-probability empty-percentage behavior of the derived <c>FormattedPercentage</c> accessor.
    /// </summary>
    [TestClass]
    public class FolderNodeViewModelTests
    {
        private static UtilitiesCS.FolderNodeViewModel Node(
            double? probability,
            bool hasChildren,
            bool expanded
        )
        {
            var vm = new UtilitiesCS.FolderNodeViewModel(
                "Archive\\Finance",
                "Finance",
                probability,
                depth: 1,
                hasChildren: hasChildren
            )
            {
                Expanded = expanded,
            };
            return vm;
        }

        [TestMethod]
        public void Glyph_CollapsedParent_IsPlus()
        {
            // INV4: HasChildren && !Expanded => '+'
            Node(null, hasChildren: true, expanded: false).Glyph.Should().Be('+');
        }

        [TestMethod]
        public void Glyph_ExpandedParent_IsMinus()
        {
            // INV4: HasChildren && Expanded => '-'
            Node(null, hasChildren: true, expanded: true).Glyph.Should().Be('-');
        }

        [TestMethod]
        public void Glyph_Leaf_HasNoGlyph()
        {
            // INV4: !HasChildren => no glyph
            Node(0.5, hasChildren: false, expanded: false).Glyph.Should().BeNull();
        }

        [TestMethod]
        public void FormattedPercentage_NullProbability_IsEmpty()
        {
            Node(null, hasChildren: true, expanded: false).FormattedPercentage.Should().BeEmpty();
        }

        [TestMethod]
        public void FormattedPercentage_WithProbability_DelegatesToFormatter()
        {
            // Sourced from a FolderScore.Probability value; delegates to PercentageFormatter.
            Node(0.4267, hasChildren: false, expanded: false)
                .FormattedPercentage.Should()
                .Be("43%");
        }
    }
}
