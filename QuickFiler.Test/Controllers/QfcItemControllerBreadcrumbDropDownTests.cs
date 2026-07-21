using System;
using System.Drawing;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Web.WebView2.Core;
using Moq;
using QuickFiler.Interfaces;
using QuickFiler.Viewers;
using UtilitiesCS;
using UtilitiesCS.OutlookObjects.Folder;

namespace QuickFiler.Controllers.Tests
{
    /// <summary>Failure-first controller environment/theme and cleanup integration for issue #400.</summary>
    [TestClass]
    public sealed class QfcItemControllerBreadcrumbDropDownTests
    {
        [TestMethod]
        public void ConfigureBreadcrumbDropDown_PassesExistingEnvironmentAndDarkThemeLazily()
        {
            // Arrange
            using (ViewerScope scope = new ViewerScope())
            {
                var environment = (CoreWebView2Environment)
                    FormatterServices.GetUninitializedObject(typeof(CoreWebView2Environment));
                var initializer = new Mock<IWebViewCoreInitializer>(MockBehavior.Strict);
                var ol = new Mock<IOlObjects>();
                ol.SetupGet(value => value.DarkMode).Returns(true);
                var globals = new Mock<IApplicationGlobals>();
                globals.SetupGet(value => value.Ol).Returns(ol.Object);
                var controller = new HarnessController();
                QfcItemControllerTestSupport.SetField(
                    controller,
                    "_webViewInitializer",
                    initializer.Object
                );
                QfcItemControllerTestSupport.SetField(controller, "_globals", globals.Object);

                // Act
                InvokeConfigure(controller, scope.Viewer, environment);

                // Assert
                object host = Host(scope.Viewer);
                Property<CoreWebView2Environment>(host, "Environment")
                    .Should()
                    .BeSameAs(environment);
                Property<string>(host, "Theme").Should().Be("dark");
                Property<object>(host, "ControlHost").Should().BeNull("popup creation stays lazy");
                initializer.VerifyNoOtherCalls();
            }
        }

        [TestMethod]
        public void ConfigureBreadcrumbDropDown_LightThemeUsesSameControllerSetupSeam()
        {
            // Arrange
            using (ViewerScope scope = new ViewerScope())
            {
                var environment = (CoreWebView2Environment)
                    FormatterServices.GetUninitializedObject(typeof(CoreWebView2Environment));
                var initializer = new Mock<IWebViewCoreInitializer>(MockBehavior.Strict);
                var ol = new Mock<IOlObjects>();
                ol.SetupGet(value => value.DarkMode).Returns(false);
                var globals = new Mock<IApplicationGlobals>();
                globals.SetupGet(value => value.Ol).Returns(ol.Object);
                var controller = new HarnessController();
                QfcItemControllerTestSupport.SetField(
                    controller,
                    "_webViewInitializer",
                    initializer.Object
                );
                QfcItemControllerTestSupport.SetField(controller, "_globals", globals.Object);

                // Act
                InvokeConfigure(controller, scope.Viewer, environment);

                // Assert
                Property<string>(Host(scope.Viewer), "Theme").Should().Be("light");
                initializer.VerifyNoOtherCalls();
            }
        }

        [TestMethod]
        public void Cleanup_ResetsInjectedHostForPooledViewerReuse()
        {
            // Arrange
            using (ViewerScope scope = new ViewerScope())
            {
                var provider = new Mock<IFolderHierarchyProvider>(MockBehavior.Strict);
                scope.Viewer.InitializeBreadcrumbPipeline(provider.Object);
                var host = new Mock<IBreadcrumbDropDownHost>();
                ConfigureInjected(scope.Viewer, host.Object);
                var controller = new HarnessController();
                QfcItemControllerTestSupport.SetField(controller, "_itemViewer", scope.Viewer);
                QfcItemControllerTestSupport.SetField(
                    controller,
                    "_breadcrumbViewer",
                    scope.Viewer
                );

                // Act
                controller.Cleanup();

                // Assert
                host.Verify(value => value.Reset(), Times.Once());
                QfcItemControllerTestSupport.GetField(controller, "_itemViewer").Should().BeNull();
                QfcItemControllerTestSupport
                    .GetField(controller, "_breadcrumbViewer")
                    .Should()
                    .BeNull();
            }
        }

        [TestMethod]
        public void OnBreadcrumbUnhandledArrow_ForViewer_RoutesOnceToKeyboardHandler()
        {
            // Arrange
            using (ViewerScope scope = new ViewerScope())
            {
                var keyboard = new Mock<IQfcKeyboardHandler>(MockBehavior.Strict);
                keyboard.Setup(handler =>
                    handler.BreadcrumbArrowFallThrough(scope.Viewer, BreadcrumbArrowDirection.Right)
                );
                var controller = new HarnessController();
                QfcItemControllerTestSupport.SetField(controller, "_kbdHandler", keyboard.Object);
                MethodInfo method = typeof(QfcItemController).GetMethod(
                    "OnBreadcrumbUnhandledArrow",
                    BindingFlags.Instance | BindingFlags.NonPublic
                );

                // Act
                method.Invoke(
                    controller,
                    new object[] { new object(), BreadcrumbArrowDirection.Left }
                );
                method.Invoke(
                    controller,
                    new object[] { scope.Viewer, BreadcrumbArrowDirection.Right }
                );

                // Assert
                keyboard.VerifyAll();
            }
        }

        private static void InvokeConfigure(
            HarnessController controller,
            QuickFiler.ItemViewer viewer,
            CoreWebView2Environment environment
        )
        {
            MethodInfo method = typeof(QfcItemController).GetMethod(
                "ConfigureBreadcrumbDropDown",
                BindingFlags.Instance | BindingFlags.NonPublic,
                null,
                new[] { typeof(QuickFiler.ItemViewer), typeof(CoreWebView2Environment) },
                null
            );
            method
                .Should()
                .NotBeNull("ViewerSetup must pass the controller's existing WebView2 environment");
            method.Invoke(controller, new object[] { viewer, environment });
        }

        private static void ConfigureInjected(
            QuickFiler.ItemViewer viewer,
            IBreadcrumbDropDownHost host
        )
        {
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
            method.Should().NotBeNull();
            method.Invoke(
                viewer,
                new object[]
                {
                    host,
                    new Func<Rectangle>(() => new Rectangle(0, 0, 300, 25)),
                    new Func<Rectangle>(() => new Rectangle(0, 0, 1920, 1040)),
                }
            );
        }

        private static object Host(QuickFiler.ItemViewer viewer)
        {
            PropertyInfo property = typeof(QuickFiler.ItemViewer).GetProperty(
                "BreadcrumbDropDownHost",
                BindingFlags.Instance | BindingFlags.NonPublic
            );
            property
                .Should()
                .NotBeNull("controller setup requires an inspectable host ownership seam");
            return property.GetValue(viewer);
        }

        private static T Property<T>(object target, string property) =>
            (T)target.GetType().GetProperty(property).GetValue(target);

        private sealed class ViewerScope : IDisposable
        {
            private readonly SynchronizationContext _previous;

            public ViewerScope()
            {
                _previous = SynchronizationContext.Current;
                SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());
                Viewer = new QuickFiler.ItemViewer();
            }

            public QuickFiler.ItemViewer Viewer { get; }

            public void Dispose()
            {
                Viewer.Dispose();
                SynchronizationContext.SetSynchronizationContext(_previous);
            }
        }
    }
}
