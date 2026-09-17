using System;
using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using Moq;
using QuickFiler.Test.TestSupport;
using QuickFiler.Viewers;

namespace QuickFiler.Test.Viewers
{
    /// <summary>
    /// Regression tests for issue #792 on the WebView2 breadcrumb host. Every test constructs its
    /// own <see cref="WebView2"/> control on <see cref="WinFormsPumpHost"/>, so the process-wide
    /// per-control owner registry cannot couple one test to another. No test drives
    /// <c>EnsureCoreWebView2Async</c> or <c>CoreWebView2Environment.CreateAsync</c> to completion,
    /// so no Evergreen WebView2 runtime is required.
    /// </summary>
    [TestClass]
    public sealed class WebView2BreadcrumbHostIssue792Tests
    {
        private const int PumpTimeoutMs = 60000;

        /// <summary>
        /// #792 site 1: the host must hand the shared cache folder and the shared
        /// <c>--incognito </c> browser argument to the <see cref="IWebViewCoreInitializer"/> seam.
        /// Before the fix the host builds a parameterless options object, so the captured
        /// <c>AdditionalBrowserArguments</c> is null and the second assertion fails.
        /// </summary>
        [TestMethod]
        [Timeout(PumpTimeoutMs)]
        public async Task InitializeAsync_PassesTheSharedFolderAndIncognitoArgumentToTheSeam()
        {
            // Arrange
            using (var pump = new WinFormsPumpHost())
            {
                WebView2 control = await pump.InvokeAsync(() => new WebView2())
                    .ConfigureAwait(false);
                try
                {
                    string capturedFolder = null;
                    CoreWebView2EnvironmentOptions capturedOptions = null;
                    var initializer = new Mock<IWebViewCoreInitializer>();
                    initializer
                        .Setup(seam =>
                            seam.CreateEnvironmentAsync(
                                It.IsAny<string>(),
                                It.IsAny<CoreWebView2EnvironmentOptions>()
                            )
                        )
                        .Callback<string, CoreWebView2EnvironmentOptions>(
                            (folder, options) =>
                            {
                                capturedFolder = folder;
                                capturedOptions = options;
                            }
                        )
                        .Returns(Task.FromResult<CoreWebView2Environment>(null));
                    initializer
                        .Setup(seam =>
                            seam.EnsureCoreWebView2Async(
                                It.IsAny<WebView2>(),
                                It.IsAny<CoreWebView2Environment>()
                            )
                        )
                        .Returns(Task.CompletedTask);
                    WebView2BreadcrumbHost subject = await pump.InvokeAsync(() =>
                            new WebView2BreadcrumbHost(control, initializer.Object, null)
                        )
                        .ConfigureAwait(false);
                    string expectedFolder = Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                        "WindowsFormsWebView2"
                    );

                    // Act
                    await subject.InitializeAsync(pump.SyncContext).ConfigureAwait(false);

                    // Assert
                    capturedFolder
                        .Should()
                        .Be(
                            expectedFolder,
                            because: "the host must use the shared WindowsFormsWebView2 cache folder under LocalApplicationData"
                        );
                    capturedOptions
                        .Should()
                        .NotBeNull(
                            because: "the host must pass an options object through the seam"
                        );
                    capturedOptions
                        .AdditionalBrowserArguments.Should()
                        .Be(
                            "--incognito ",
                            because: "every WebView2 site must share the same incognito browser argument"
                        );
                }
                finally
                {
                    await pump.InvokeAsync(() => control.Dispose()).ConfigureAwait(false);
                }
            }
        }

        /// <summary>
        /// #792: a document handed to the host before its CoreWebView2 exists must be logged and
        /// dropped, not forwarded. Before the fix the inline forward reaches
        /// <c>WebView2.NavigateToString</c> on a control with no core, which throws
        /// <see cref="InvalidOperationException"/>.
        /// </summary>
        [TestMethod]
        [Timeout(PumpTimeoutMs)]
        public async Task NavigateToString_BeforeCoreInitialization_WithNoDispatcher_DropsTheDocumentWithoutThrowing()
        {
            // Arrange
            using (var pump = new WinFormsPumpHost())
            {
                WebView2 control = await pump.InvokeAsync(() => new WebView2())
                    .ConfigureAwait(false);
                try
                {
                    WebView2BreadcrumbHost subject = await pump.InvokeAsync(() =>
                            new WebView2BreadcrumbHost(
                                control,
                                Mock.Of<IWebViewCoreInitializer>(),
                                null
                            )
                        )
                        .ConfigureAwait(false);

                    // Act - called from the MSTest thread; InitializeAsync never ran, so no
                    // dispatcher exists and no core exists.
                    Action act = () => subject.NavigateToString("<html></html>");

                    // Assert
                    act.Should()
                        .NotThrow(
                            because: "a document navigated before core initialization must be dropped, not forwarded to a control with no core"
                        );
                    subject
                        .IsCoreInitialized.Should()
                        .BeFalse(because: "nothing in this test initializes the core");
                }
                finally
                {
                    await pump.InvokeAsync(() => control.Dispose()).ConfigureAwait(false);
                }
            }
        }
    }
}
