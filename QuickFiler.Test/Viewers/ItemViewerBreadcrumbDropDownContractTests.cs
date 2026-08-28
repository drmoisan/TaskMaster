using System;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Web.WebView2.Core;
using QuickFiler.Viewers;

namespace QuickFiler.Test.Viewers
{
    /// <summary>Failure-first ItemViewer surface and compatibility contracts for issue #400.</summary>
    [TestClass]
    public sealed class ItemViewerBreadcrumbDropDownContractTests
    {
        [TestMethod]
        public void ExistingAnchor_RemainsTheDesignerWebViewClosedSurface()
        {
            // Arrange
            PropertyInfo property = typeof(QuickFiler.ItemViewer).GetProperty(
                "L0vhBreadcrumb_WebView2"
            );

            // Assert
            property.Should().NotBeNull();
            property.PropertyType.Should().Be(typeof(Microsoft.Web.WebView2.WinForms.WebView2));
        }

        [TestMethod]
        public void ProductionConfiguration_AcceptsExistingEnvironmentAndInitializer()
        {
            // Act
            MethodInfo method = typeof(QuickFiler.ItemViewer).GetMethod(
                "ConfigureBreadcrumbDropDown",
                BindingFlags.Instance | BindingFlags.NonPublic,
                null,
                new[] { typeof(CoreWebView2Environment), typeof(IWebViewCoreInitializer) },
                null
            );

            // Assert
            method
                .Should()
                .NotBeNull(
                    "the controller must pass its existing environment to lazy popup configuration"
                );
        }

        [TestMethod]
        public void InjectedConfiguration_AcceptsHostAndScreenGeometryProviders()
        {
            // Act
            MethodInfo method = typeof(QuickFiler.ItemViewer).GetMethod(
                "ConfigureBreadcrumbDropDown",
                BindingFlags.Instance | BindingFlags.NonPublic,
                null,
                new[]
                {
                    typeof(IBreadcrumbDropDownHost),
                    typeof(Func<Rectangle>),
                    typeof(Func<Rectangle>),
                },
                null
            );

            // Assert
            method
                .Should()
                .NotBeNull(
                    "screen bounds and the active monitor working area require deterministic seams"
                );
        }

        [TestMethod]
        public void ExistingFolderEventsAndDropDownIntentSignatures_AreUnchanged()
        {
            // Arrange
            Type interfaceType = typeof(IItemViewer);
            EventInfo selection = interfaceType.GetEvent("FolderSelectionChanged");
            EventInfo key = interfaceType.GetEvent("FolderKeyDown");
            MethodInfo dropDown = interfaceType.GetMethod(
                "SetFolderDroppedDown",
                new[] { typeof(bool) }
            );

            // Assert
            selection.EventHandlerType.Should().Be(typeof(EventHandler));
            key.EventHandlerType.Should().Be(typeof(KeyEventHandler));
            dropDown.ReturnType.Should().Be(typeof(void));
            typeof(QuickFiler.ItemViewer)
                .GetEvent("FolderSelectionChanged")
                .EventHandlerType.Should()
                .Be(typeof(EventHandler));
            typeof(QuickFiler.ItemViewer)
                .GetEvent("FolderKeyDown")
                .EventHandlerType.Should()
                .Be(typeof(KeyEventHandler));
        }

        [TestMethod]
        public void HostNeutralPopupOpenOrchestration_IsOwnedByInstrumentedCoordinator()
        {
            // Arrange
            Type coordinatorType = typeof(QuickFiler.ItemViewer).Assembly.GetType(
                "QuickFiler.Viewers.BreadcrumbDropDownOpenCoordinator",
                false
            );
            MethodInfo itemViewerOpenMethod = typeof(QuickFiler.ItemViewer).GetMethod(
                "OpenBreadcrumbDropDownAsync",
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly
            );

            // Assert
            using (new AssertionScope())
            {
                coordinatorType
                    .Should()
                    .NotBeNull("host-neutral popup-open orchestration must be instrumented");
                coordinatorType?.IsNotPublic.Should().BeTrue("the coordinator is internal");
                coordinatorType
                    ?.GetCustomAttribute<ExcludeFromCodeCoverageAttribute>()
                    .Should()
                    .BeNull("the coordinator must remain measurable");
                itemViewerOpenMethod
                    .Should()
                    .BeNull("ItemViewer must delegate host-neutral popup-open orchestration");
            }
        }

        [TestMethod]
        public void ItemViewer_DeclaresNoMenuItemCheckedChangedMembers()
        {
            // Arrange
            const BindingFlags Flags =
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

            // Act
            MethodInfo eventHandlerForm = typeof(QuickFiler.ItemViewer).GetMethod(
                "MenuItem_CheckedChanged",
                Flags,
                null,
                new[] { typeof(object), typeof(EventArgs) },
                null
            );
            MethodInfo typedForm = typeof(QuickFiler.ItemViewer).GetMethod(
                "MenuItem_CheckedChanged",
                Flags,
                null,
                new[] { typeof(ToolStripMenuItem) },
                null
            );

            // Assert
            using (new AssertionScope())
            {
                eventHandlerForm
                    .Should()
                    .BeNull("the EventHandler-shaped overload is dead code on ItemViewer");
                typedForm.Should().BeNull("the typed overload is dead code on ItemViewer");
            }
        }

        [TestMethod]
        public void ItemViewer_DeclaresNoMoveOptionsMenuClickHandler()
        {
            // Arrange
            const BindingFlags Flags =
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

            // Act
            MethodInfo handler = typeof(QuickFiler.ItemViewer).GetMethod(
                "MoveOptionsMenu_Click",
                Flags,
                null,
                new[] { typeof(object), typeof(EventArgs) },
                null
            );

            // Assert
            handler
                .Should()
                .BeNull(
                    "the empty MoveOptionsMenu_Click body has no caller and no designer wiring"
                );
        }
    }
}
