using System;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QuickFiler.Viewers;

namespace QuickFiler.Controllers.Tests
{
    /// <summary>
    /// Construction smoke test (cycle-2 Phase 6, P6-T4/P6-T12) for the production
    /// <see cref="WebView2CoreInitializer"/>, plus the argument-guard regression tests for issue
    /// #477 defect 2. The two SDK forwards require the external Evergreen WebView2 runtime and are
    /// therefore exempt; the argument guards are pure validation that never reaches the SDK, so they
    /// are measured and are asserted directly here.
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

        /// <summary>
        /// #477 defect 2: a null <c>cacheFolder</c> must fail fast with the parameter name rather
        /// than being forwarded to the SDK. The returned task is never awaited or observed, so the
        /// guard is the only code that runs and no WebView2 runtime is involved.
        /// </summary>
        [TestMethod]
        public void CreateEnvironmentAsync_NullCacheFolder_ThrowsArgumentNullException()
        {
            // Arrange
            var initializer = new WebView2CoreInitializer();

            // Act
            Action act = () =>
            {
                _ = initializer.CreateEnvironmentAsync(null, null);
            };

            // Assert
            act.Should()
                .Throw<ArgumentNullException>(
                    because: "a null cacheFolder is a caller defect and must surface with its parameter name instead of a less specific SDK failure"
                )
                .And.ParamName.Should()
                .Be("cacheFolder");
        }

        /// <summary>
        /// #477 defect 2: a whitespace <c>cacheFolder</c> must throw <see cref="ArgumentException"/>
        /// exactly. <c>ThrowExactly</c> is required because
        /// <see cref="ArgumentNullException"/> derives from <see cref="ArgumentException"/> and
        /// would otherwise satisfy a non-exact assertion.
        /// </summary>
        [TestMethod]
        public void CreateEnvironmentAsync_WhitespaceCacheFolder_ThrowsArgumentException()
        {
            // Arrange
            var initializer = new WebView2CoreInitializer();

            // Act
            Action act = () =>
            {
                _ = initializer.CreateEnvironmentAsync("   ", null);
            };

            // Assert
            act.Should()
                .ThrowExactly<ArgumentException>(
                    because: "a whitespace cacheFolder cannot name a user-data folder, and ThrowExactly is required because ArgumentNullException derives from ArgumentException"
                )
                .And.ParamName.Should()
                .Be("cacheFolder");
        }

        /// <summary>
        /// #477 defect 2: a null <c>control</c> must throw <see cref="ArgumentNullException"/> with
        /// the parameter name rather than producing a bare <see cref="NullReferenceException"/> from
        /// the SDK forward.
        /// </summary>
        [TestMethod]
        public void EnsureCoreWebView2Async_NullControl_ThrowsArgumentNullException()
        {
            // Arrange
            var initializer = new WebView2CoreInitializer();

            // Act
            Action act = () =>
            {
                _ = initializer.EnsureCoreWebView2Async(null, null);
            };

            // Assert
            act.Should()
                .Throw<ArgumentNullException>(
                    because: "a null control previously produced a bare NullReferenceException with no parameter name, against the convention of every sibling seam"
                )
                .And.ParamName.Should()
                .Be("control");
        }
    }
}
