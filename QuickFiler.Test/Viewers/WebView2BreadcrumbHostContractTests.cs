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
    }
}
