using System;
using System.IO;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Web.WebView2.Core;
using QuickFiler.Viewers;

namespace QuickFiler.Test.Viewers
{
    /// <summary>
    /// Contract tests for <see cref="WebView2EnvironmentContract"/> (#792 AC-U6), the single owner
    /// of the WebView2 environment values shared by every host in QuickFiler. All three pass
    /// before the fix because the contract type is declared in Phase 2; their non-vacuity is
    /// proven by mutation in Phase 5. No test touches the filesystem: the folder assertion
    /// compares two computed strings.
    /// </summary>
    [TestClass]
    public sealed class WebView2EnvironmentContractTests
    {
        private const string SharedLeafName = "WindowsFormsWebView2";

        /// <summary>
        /// The shared browser argument is the ASCII double-hyphen incognito switch with its
        /// trailing space, mirroring the #463 pin on <c>EfcItemController.IncognitoArgument</c>.
        /// </summary>
        [TestMethod]
        public void AdditionalBrowserArguments_IsAsciiDoubleHyphenIncognitoWithTrailingSpace()
        {
            // Arrange
            const string expected = "--incognito ";

            // Act
            string actual = WebView2EnvironmentContract.AdditionalBrowserArguments;

            // Assert
            actual
                .Should()
                .Be(
                    expected,
                    "Chromium command-line switches are introduced by two ASCII hyphen-minus characters"
                );
            actual
                .ToCharArray()
                .Should()
                .OnlyContain(
                    character => character <= 0x7F,
                    "a non-ASCII character in a machine-parsed switch is silently ignored"
                );
            actual[0].Should().Be('-', "the first character must be ASCII HYPHEN-MINUS");
            actual[1].Should().Be('-', "the second character must be ASCII HYPHEN-MINUS");
        }

        /// <summary>
        /// The user-data folder is LocalApplicationData joined with the shared leaf name. The
        /// resolver is pure, so the expected value is computed the same way and compared.
        /// </summary>
        [TestMethod]
        public void ResolveUserDataFolder_CombinesLocalApplicationDataWithTheSharedLeafName()
        {
            // Arrange
            string expected = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                SharedLeafName
            );

            // Act
            string actual = WebView2EnvironmentContract.ResolveUserDataFolder();

            // Assert
            actual
                .Should()
                .Be(
                    expected,
                    "every WebView2 host must resolve the same user-data folder or the shared browser"
                        + " process rejects the second environment"
                );
        }

        /// <summary>
        /// Each call returns a distinct options instance (the SDK type is mutable) and every
        /// instance carries the shared browser arguments.
        /// </summary>
        [TestMethod]
        public void CreateOptions_CarriesTheSharedArgumentsOnAFreshInstance()
        {
            // Act
            CoreWebView2EnvironmentOptions first = WebView2EnvironmentContract.CreateOptions();
            CoreWebView2EnvironmentOptions second = WebView2EnvironmentContract.CreateOptions();

            // Assert
            first
                .Should()
                .NotBeSameAs(second, "the options type is mutable, so callers must not share one");
            first
                .AdditionalBrowserArguments.Should()
                .Be(
                    WebView2EnvironmentContract.AdditionalBrowserArguments,
                    "the first instance must carry the shared arguments"
                );
            second
                .AdditionalBrowserArguments.Should()
                .Be(
                    WebView2EnvironmentContract.AdditionalBrowserArguments,
                    "the second instance must carry the shared arguments"
                );
        }
    }
}
