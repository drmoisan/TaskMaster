using System;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using FluentAssertions;
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
    }
}
