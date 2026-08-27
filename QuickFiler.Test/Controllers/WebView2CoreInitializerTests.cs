using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
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

        /// <summary>
        /// #477: the coverage exemption must fall only on the two SDK forwards. The argument guards
        /// are pure validation with no SDK dependency, so under the repository rule they are a
        /// testable seam and must be measured.
        /// </summary>
        [TestMethod]
        public void WebView2CoreInitializer_ExemptsOnlyTheSdkForwards()
        {
            // Arrange
            Type subject = typeof(WebView2CoreInitializer);
            const BindingFlags AllDeclared =
                BindingFlags.Instance
                | BindingFlags.Static
                | BindingFlags.Public
                | BindingFlags.NonPublic
                | BindingFlags.DeclaredOnly;

            // Act
            ExcludeFromCodeCoverageAttribute classLevel =
                subject.GetCustomAttribute<ExcludeFromCodeCoverageAttribute>(inherit: false);
            MethodInfo createForward = subject.GetMethod(
                "ForwardCreateEnvironmentAsync",
                AllDeclared
            );
            MethodInfo ensureForward = subject.GetMethod(
                "ForwardEnsureCoreWebView2Async",
                AllDeclared
            );
            MethodInfo createGuarded = subject.GetMethod("CreateEnvironmentAsync", AllDeclared);
            MethodInfo ensureGuarded = subject.GetMethod("EnsureCoreWebView2Async", AllDeclared);

            // Assert
            classLevel
                .Should()
                .BeNull(
                    because: "a class-level exemption would suppress measurement of the argument guards as well as the forwards"
                );
            createForward
                .Should()
                .NotBeNull(because: "the environment SDK call must be extracted into its own method");
            ensureForward
                .Should()
                .NotBeNull(because: "the ensure SDK call must be extracted into its own method");
            createForward
                .GetCustomAttribute<ExcludeFromCodeCoverageAttribute>(inherit: false)
                .Should()
                .NotBeNull(
                    because: "the environment forward needs the Evergreen runtime and creates a user-data folder on disk"
                );
            ensureForward
                .GetCustomAttribute<ExcludeFromCodeCoverageAttribute>(inherit: false)
                .Should()
                .NotBeNull(because: "the ensure forward needs the Evergreen runtime");
            createGuarded
                .GetCustomAttribute<ExcludeFromCodeCoverageAttribute>(inherit: false)
                .Should()
                .BeNull(because: "the cacheFolder guards are measured, not exempt");
            ensureGuarded
                .GetCustomAttribute<ExcludeFromCodeCoverageAttribute>(inherit: false)
                .Should()
                .BeNull(because: "the control guard is measured, not exempt");
        }
    }
}
