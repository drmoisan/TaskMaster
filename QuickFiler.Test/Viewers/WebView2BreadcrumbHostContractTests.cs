using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QuickFiler.Viewers;

namespace QuickFiler.Test.Viewers
{
    /// <summary>
    /// Structural contract assertions over <see cref="WebView2BreadcrumbHost"/> for issues #476 and
    /// #477. These tests use reflection over the type's declared members rather than driving
    /// behaviour, so they require no WebView2 control and no Evergreen runtime.
    /// </summary>
    [TestClass]
    public sealed class WebView2BreadcrumbHostContractTests
    {
        private const string BackingFieldName = "_isCoreInitialized";
        private const string CompilerBackingFieldName = "<IsCoreInitialized>k__BackingField";

        /// <summary>
        /// Asserts that the initialization flag is held in an explicit, hand-written private field
        /// rather than in a compiler-generated auto-property backing field, which is the structural
        /// precondition for reading and writing it through
        /// <c>Volatile.Read</c> / <c>Volatile.Write</c> (#476 defect 2).
        /// </summary>
        /// <remarks>
        /// This assertion is a STRUCTURAL PROXY for the memory-ordering fix and is explicitly NOT a
        /// proof that the race is eliminated. A memory-ordering defect cannot be made to fail
        /// deterministically by a unit test: on x86/x64 the missing barrier is very unlikely to
        /// produce an observable reordering, and a test that spun threads hoping to catch one would
        /// violate the determinism requirement in <c>.claude/rules/general-unit-test.md</c> and
        /// CLAUDE.md UT1. What this test does establish is that the auto-property is gone and that an
        /// explicit field exists for the volatile accessors to operate on; the ordering of the
        /// release store relative to the preceding event subscription is evidenced separately by the
        /// publication-order record in this feature's evidence folder.
        /// </remarks>
        [TestMethod]
        public void IsCoreInitialized_HasAnExplicitBackingField()
        {
            // Arrange
            FieldInfo[] declaredFields = typeof(WebView2BreadcrumbHost).GetFields(
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public
            );

            // Act
            FieldInfo explicitField = declaredFields.FirstOrDefault(field =>
                field.Name == BackingFieldName
            );
            FieldInfo compilerField = declaredFields.FirstOrDefault(field =>
                field.Name == CompilerBackingFieldName
            );

            // Assert
            explicitField
                .Should()
                .NotBeNull(
                    because: "IsCoreInitialized must be backed by an explicit private field so Volatile.Read and Volatile.Write can be applied to it"
                );
            explicitField.IsPublic.Should()
                .BeFalse(because: "the backing field is an implementation detail and must stay non-public");
            explicitField.FieldType.Should()
                .Be(typeof(bool), because: "the initialization flag is a boolean state");
            explicitField
                .GetCustomAttribute<CompilerGeneratedAttribute>()
                .Should()
                .BeNull(
                    because: "a field carrying CompilerGeneratedAttribute would be an auto-property backing field, which is exactly the non-volatile shape #476 defect 2 reports"
                );
            compilerField
                .Should()
                .BeNull(
                    because: "the presence of <IsCoreInitialized>k__BackingField would prove IsCoreInitialized is still an auto-property"
                );
        }

        /// <summary>
        /// #477: the class-level coverage exemption became false once the internal constructor, the
        /// dispatcher-routing decisions, the registry detach path and the state accessor became
        /// reachable from tests. Keeping it would recreate the exact false-rationale defect #477
        /// reports against the sibling initializer.
        /// </summary>
        [TestMethod]
        public void WebView2BreadcrumbHost_CarriesNoClassLevelCoverageExemption()
        {
            // Arrange
            Type subject = typeof(WebView2BreadcrumbHost);

            // Act
            ExcludeFromCodeCoverageAttribute exemption =
                subject.GetCustomAttribute<ExcludeFromCodeCoverageAttribute>(inherit: false);

            // Assert
            exemption
                .Should()
                .BeNull(
                    because: "a class-level exemption would suppress measurement of the whole type, including the seams this feature makes testable"
                );
        }

        /// <summary>
        /// #477: member-level coverage exemptions must fall only on the genuinely host-bound members.
        /// The two SDK event handlers cannot be invoked with a valid argument because their
        /// event-argument types have no public constructor, and the two extracted forwards reach the
        /// SDK directly. Everything else — including <c>InitializeAsync</c>, whose only SDK-reaching
        /// statements go through the mockable seam — must be measured.
        /// </summary>
        [TestMethod]
        public void WebView2BreadcrumbHost_ExemptsOnlyHostBoundMembers()
        {
            // Arrange
            Type subject = typeof(WebView2BreadcrumbHost);
            string[] expectedExempt = new[]
            {
                "OnCoreInitializationCompleted",
                "OnWebMessageReceived",
                "ForwardNavigateToString",
                "ForwardWebMessage",
            };
            string[] expectedMeasured = new[]
            {
                "IsAttached",
                "HasUiDispatcher",
                "IsCoreInitialized",
                "NavigateToString",
                "PostMessageJson",
                "InitializeAsync",
                "DetachCore",
            };

            // Act
            string[] actualExempt = subject
                .GetMethods(
                    BindingFlags.Instance
                        | BindingFlags.Static
                        | BindingFlags.Public
                        | BindingFlags.NonPublic
                        | BindingFlags.DeclaredOnly
                )
                .Where(method =>
                    method.GetCustomAttribute<ExcludeFromCodeCoverageAttribute>(inherit: false)
                    != null
                )
                .Select(method => method.Name)
                .Where(name => !name.StartsWith("<", StringComparison.Ordinal))
                .Distinct()
                .OrderBy(name => name, StringComparer.Ordinal)
                .ToArray();

            // Assert
            actualExempt
                .Should()
                .BeEquivalentTo(
                    expectedExempt,
                    because: "exactly the four genuinely host-bound members may carry the exemption"
                );

            foreach (string name in expectedMeasured)
            {
                MemberInfo[] members = subject.GetMember(
                    name,
                    BindingFlags.Instance
                        | BindingFlags.Static
                        | BindingFlags.Public
                        | BindingFlags.NonPublic
                        | BindingFlags.DeclaredOnly
                );
                members
                    .Should()
                    .NotBeEmpty(because: $"{name} must exist to be asserted measured");
                foreach (MemberInfo member in members)
                {
                    member
                        .GetCustomAttribute<ExcludeFromCodeCoverageAttribute>(inherit: false)
                        .Should()
                        .BeNull(
                            because: $"{name} is reachable from unit tests and must therefore be measured"
                        );
                }
            }

            foreach (
                ConstructorInfo constructor in subject.GetConstructors(
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
                )
            )
            {
                constructor
                    .GetCustomAttribute<ExcludeFromCodeCoverageAttribute>(inherit: false)
                    .Should()
                    .BeNull(
                        because: "both constructors are exercised by the regression tests and must be measured"
                    );
            }
        }
    }
}
