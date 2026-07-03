using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QuickFiler.Viewers;

namespace QuickFiler.Controllers.Tests
{
    /// <summary>
    /// Construction smoke test (cycle-2 Phase 6, P6-T4/P6-T12) for the production
    /// <see cref="WebView2CoreInitializer"/>. Its two members forward to the WebView2 SDK
    /// (CoreWebView2Environment/WebView2), which requires the WebView2 runtime; the forwarding bodies
    /// are therefore exempt and only the construction/contract is asserted here.
    /// </summary>
    [TestClass]
    public class WebView2CoreInitializerTests
    {
        [TestMethod]
        public void Construction_YieldsAnIWebViewCoreInitializer()
        {
            IWebViewCoreInitializer initializer = new WebView2CoreInitializer();

            initializer.Should().NotBeNull();
            initializer.Should().BeAssignableTo<IWebViewCoreInitializer>();
        }
    }
}
