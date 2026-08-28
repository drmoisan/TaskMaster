using System;
using System.Reflection;
using System.Runtime.Serialization;
using FluentAssertions;
using QuickFiler.Controllers;

namespace QuickFiler.Controllers.Tests
{
    /// <summary>
    /// Shared reflection helpers and builders for the issue #468 defect-family test files.
    /// <para>
    /// <see cref="QfcCollectionController"/>'s only constructor requires WinForms UI components, so
    /// instances are allocated with <see cref="FormatterServices.GetUninitializedObject(Type)"/> to
    /// bypass it and the required private fields are then injected by reflection. This mirrors the
    /// technique already established in <c>QfcCollectionControllerTests.cs</c>.
    /// </para>
    /// <para>
    /// The helpers below follow the <em>asserting</em> form used by
    /// <c>QfcItemControllerTestSupport</c> (assert the member was found before touching it) rather
    /// than the silently-no-op <c>?.SetValue(...)</c> form used by the older
    /// <c>QfcCollectionControllerTests.SetControllerField</c>. A typo in a member name must fail the
    /// test loudly instead of leaving the field at its default and producing a misleading result.
    /// </para>
    /// </summary>
    internal static class QfcCollectionControllerTestSupport
    {
        private const BindingFlags NonPublicInstance =
            BindingFlags.NonPublic | BindingFlags.Instance;

        private const BindingFlags NonPublicStatic = BindingFlags.NonPublic | BindingFlags.Static;

        /// <summary>
        /// Sets a non-public instance field on <paramref name="controller"/>, asserting first that
        /// the field exists.
        /// </summary>
        internal static void SetField(QfcCollectionController controller, string name, object value)
        {
            FieldInfo field = typeof(QfcCollectionController).GetField(name, NonPublicInstance);
            field
                .Should()
                .NotBeNull(because: "field '" + name + "' must exist on QfcCollectionController");
            field.SetValue(controller, value);
        }

        /// <summary>
        /// Reads a non-public instance field from <paramref name="controller"/>, asserting first
        /// that the field exists.
        /// </summary>
        internal static object GetField(QfcCollectionController controller, string name)
        {
            FieldInfo field = typeof(QfcCollectionController).GetField(name, NonPublicInstance);
            field
                .Should()
                .NotBeNull(because: "field '" + name + "' must exist on QfcCollectionController");
            return field.GetValue(controller);
        }

        /// <summary>
        /// Returns the <see cref="FieldInfo"/> for a non-public instance field, asserting first that
        /// it exists. Used by structural tests that assert on a field's declared
        /// <see cref="FieldInfo.FieldType"/> rather than on its value.
        /// </summary>
        internal static FieldInfo GetFieldInfo(string name)
        {
            FieldInfo field = typeof(QfcCollectionController).GetField(name, NonPublicInstance);
            field
                .Should()
                .NotBeNull(because: "field '" + name + "' must exist on QfcCollectionController");
            return field;
        }

        /// <summary>
        /// Sets a non-public <em>static</em> field on <see cref="QfcCollectionController"/>,
        /// asserting first that the field exists. Static state is process-wide, so every test that
        /// touches it must reset it in <c>[TestInitialize]</c> and <c>[TestCleanup]</c> to keep the
        /// suite order-independent.
        /// </summary>
        internal static void SetStaticField(string name, object value)
        {
            FieldInfo field = typeof(QfcCollectionController).GetField(name, NonPublicStatic);
            field
                .Should()
                .NotBeNull(
                    because: "static field '" + name + "' must exist on QfcCollectionController"
                );
            field.SetValue(null, value);
        }

        /// <summary>
        /// Reads a non-public <em>static</em> field from <see cref="QfcCollectionController"/>,
        /// asserting first that the field exists.
        /// </summary>
        internal static object GetStaticField(string name)
        {
            FieldInfo field = typeof(QfcCollectionController).GetField(name, NonPublicStatic);
            field
                .Should()
                .NotBeNull(
                    because: "static field '" + name + "' must exist on QfcCollectionController"
                );
            return field.GetValue(null);
        }

        /// <summary>
        /// Invokes a non-public instance method by name, asserting first that the method exists.
        /// </summary>
        /// <remarks>
        /// Reflection wraps any exception thrown by the target in a
        /// <see cref="TargetInvocationException"/>, so a caller asserting on the underlying failure
        /// must assert on the inner exception.
        /// </remarks>
        internal static object InvokeNonPublic(
            QfcCollectionController controller,
            string name,
            params object[] args
        )
        {
            MethodInfo method = typeof(QfcCollectionController).GetMethod(name, NonPublicInstance);
            method
                .Should()
                .NotBeNull(because: "method '" + name + "' must exist on QfcCollectionController");
            return method.Invoke(controller, args);
        }

        /// <summary>
        /// Allocates a <see cref="QfcCollectionController"/> without running its
        /// WinForms-dependent constructor, and injects the one field that must never be left at its
        /// uninitialized default.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <see cref="FormatterServices.GetUninitializedObject(Type)"/> bypasses field
        /// initializers, so <c>_digits</c> is <c>0</c> rather than its declared <c>1</c>. The
        /// <c>Digits</c> getter then sets <c>_digitRefreshNeeded = true</c>, which routes
        /// <c>RegisterNavigation</c> into the WinForms-bound <c>SetVisualDigits</c> path. Every
        /// builder that can reach <c>RegisterNavigation</c>, <c>UnregisterNavigation</c>, or
        /// <c>RemoveSpecificControlGroupAsync</c> therefore injects <c>_digits = 1</c>, unless the
        /// test specifically wants that path.
        /// </para>
        /// <para>
        /// Note that <c>_moveMonitor</c> and <c>BackgroundLoadingTasks</c> are also field
        /// initializers and are therefore <c>null</c> on the returned instance. Tests that need
        /// them must inject them explicitly.
        /// </para>
        /// </remarks>
        internal static QfcCollectionController CreateUninitializedController()
        {
            QfcCollectionController controller = (QfcCollectionController)
                FormatterServices.GetUninitializedObject(typeof(QfcCollectionController));
            SetField(controller, "_digits", 1);
            return controller;
        }
    }
}
