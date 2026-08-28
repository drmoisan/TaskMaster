using System;
using System.Reflection;
using System.Windows.Forms;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace QuickFiler.Test.Viewers
{
    /// <summary>
    /// Issue #680 additive-contract tests for the search-dismissal seam, mirroring the reflection
    /// style of <c>ItemViewerBreadcrumbDropDownContractTests</c> (which stays byte-unmodified per
    /// spec AC-5) and the additive-only discipline of #438's AC-10: the two new
    /// <see cref="IItemViewer"/> members must be additive, and every existing search / drop-down
    /// member shape must be unchanged.
    /// </summary>
    [TestClass]
    public sealed class ItemViewerSearchDismissalContractTests
    {
        /// <summary>
        /// The new leave intent is a plain <see cref="EventHandler"/>, not a WinForms-specific
        /// delegate, so the controller seam stays host-neutral and mockable.
        /// </summary>
        [TestMethod]
        public void IItemViewer_DeclaresSearchLeaveAsPlainEventHandler()
        {
            // Act
            EventInfo declared = typeof(IItemViewer).GetEvent("SearchLeave");

            // Assert
            declared.Should().NotBeNull("issue #680 adds a search-leave dismissal intent");
            declared.EventHandlerType.Should().Be(typeof(EventHandler));
        }

        /// <summary>
        /// The dismissal guard is read-only: the controller reads the drop-down state and never
        /// writes it through this member.
        /// </summary>
        [TestMethod]
        public void IItemViewer_DeclaresIsFolderDropDownOpenAsReadOnlyBool()
        {
            // Act
            PropertyInfo declared = typeof(IItemViewer).GetProperty("IsFolderDropDownOpen");

            // Assert
            declared.Should().NotBeNull("issue #680 adds a drop-down open-state guard");
            declared.PropertyType.Should().Be(typeof(bool));
            declared.CanRead.Should().BeTrue();
            declared.CanWrite.Should().BeFalse("the guard is a read-only state query");
        }

        /// <summary>
        /// Additive-only discipline: the pre-existing search and drop-down member shapes are
        /// unchanged by the #680 seam.
        /// </summary>
        [TestMethod]
        public void IItemViewer_ExistingSearchAndDropDownMemberShapes_AreUnchanged()
        {
            // Act
            EventInfo searchKeyDown = typeof(IItemViewer).GetEvent("SearchKeyDown");
            EventInfo searchTextChanged = typeof(IItemViewer).GetEvent("SearchTextChanged");
            MethodInfo setDroppedDown = typeof(IItemViewer).GetMethod(
                "SetFolderDroppedDown",
                new[] { typeof(bool) }
            );

            // Assert
            searchKeyDown.Should().NotBeNull();
            searchKeyDown.EventHandlerType.Should().Be(typeof(KeyEventHandler));
            searchTextChanged.Should().NotBeNull();
            searchTextChanged.EventHandlerType.Should().Be(typeof(EventHandler));
            setDroppedDown.Should().NotBeNull();
            setDroppedDown.ReturnType.Should().Be(typeof(void));
        }

        /// <summary>
        /// The concrete viewer implements both additive members, so the interface addition is not
        /// satisfied only by some other implementer.
        /// </summary>
        [TestMethod]
        public void ItemViewer_ImplementsSearchLeaveAndIsFolderDropDownOpen()
        {
            // Act
            EventInfo declaredEvent = typeof(QuickFiler.ItemViewer).GetEvent("SearchLeave");
            PropertyInfo declaredProperty = typeof(QuickFiler.ItemViewer).GetProperty(
                "IsFolderDropDownOpen"
            );

            // Assert
            declaredEvent.Should().NotBeNull();
            declaredEvent.EventHandlerType.Should().Be(typeof(EventHandler));
            declaredProperty.Should().NotBeNull();
            declaredProperty.PropertyType.Should().Be(typeof(bool));
        }
    }
}
