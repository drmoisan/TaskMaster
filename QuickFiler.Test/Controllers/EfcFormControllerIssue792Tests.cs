using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QuickFiler.Viewers;

namespace QuickFiler.Controllers.Tests
{
    /// <summary>
    /// Regression tests for issue #792 on <see cref="EfcFormController"/>. Every test builds a
    /// minimal controller through the private no-arg constructor, so no Outlook COM context and no
    /// WinForms window is required. The user-facing surface is observed through
    /// <see cref="EfcFormController.UserFaultNotifier"/>, which is per-async-flow storage, so a
    /// capture installed here is invisible to tests running in parallel.
    /// </summary>
    [TestClass]
    public sealed class EfcFormControllerIssue792Tests
    {
        /// <summary>
        /// Creates an EfcFormController via the private no-arg constructor, which allocates the
        /// object without initializing any sub-components, leaving all fields null.
        /// </summary>
        private static EfcFormController CreateMinimalController()
        {
            var ctor = typeof(EfcFormController).GetConstructor(
                BindingFlags.NonPublic | BindingFlags.Instance,
                null,
                Type.EmptyTypes,
                null
            );
            ctor.Should().NotBeNull("private no-arg constructor must exist on EfcFormController");
            return (EfcFormController)ctor.Invoke(Array.Empty<object>());
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            var field = target
                .GetType()
                .GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            field.Should().NotBeNull($"{fieldName} must remain available for this headless seam");
            field.SetValue(target, value);
        }

        /// <summary>
        /// Routes <see cref="EfcFormController.UserFaultNotifier"/> for the current async flow into
        /// <paramref name="captured"/> and returns a scope that restores the previous value.
        /// </summary>
        private static IDisposable CaptureUserFaults(List<string> captured)
        {
            var previous = EfcFormController.UserFaultNotifier;
            EfcFormController.UserFaultNotifier = captured.Add;
            return new NotifierScope(previous);
        }

        private sealed class NotifierScope : IDisposable
        {
            private readonly Action<string> _previous;

            internal NotifierScope(Action<string> previous)
            {
                _previous = previous;
            }

            public void Dispose()
            {
                EfcFormController.UserFaultNotifier = _previous;
            }
        }

        /// <summary>
        /// AC-U4, PopulateFolderCombobox half (strengthened user-surface test). The sink is left
        /// at its default so the fault must reach the user through the notifier; the existing
        /// call-count test cannot see whether the default sink notifies anyone. This test passes
        /// before the fix and is mutation-proven in Phase 5.
        /// </summary>
        [TestMethod]
        public async Task PopulateFolderCombobox_WhenDataModelFaults_NotifiesTheUserThroughTheDefaultSink()
        {
            // Arrange
            var controller = CreateMinimalController();
            var viewer = (EfcViewer)FormatterServices.GetUninitializedObject(typeof(EfcViewer));
            SetPrivateField(controller, "_formViewer", viewer);
            var captured = new List<string>();
            using (CaptureUserFaults(captured))
            {
                // Act
                Func<Task> act = () => controller.PopulateFolderCombobox();

                // Assert
                await act.Should()
                    .NotThrowAsync(
                        "a fire-and-forget call site cannot observe a faulted Task, so the method"
                            + " must contain its own fault"
                    );
                captured
                    .Should()
                    .ContainSingle(
                        "the default boundary sink must surface the contained fault to the user"
                            + " exactly once"
                    );
            }
        }

        /// <summary>
        /// AC-U4, InitializeBreadcrumbHostAsync half. With no host, no router and no viewer, every
        /// attempt faults; after the attempt limit the failure must be reported through the
        /// boundary sink to the user, naming the attempt count. Before the fix the single attempt
        /// raises NullReferenceException, which the catch only logs, so nothing reaches the user.
        /// </summary>
        [TestMethod]
        public async Task InitializeBreadcrumbHostAsync_WhenHostIsNull_ReportsThroughTheBoundarySinkToTheUser()
        {
            // Arrange
            var controller = CreateMinimalController();
            var method = typeof(EfcFormController).GetMethod(
                "InitializeBreadcrumbHostAsync",
                BindingFlags.Instance | BindingFlags.NonPublic
            );
            method.Should().NotBeNull("InitializeBreadcrumbHostAsync must remain available");
            var captured = new List<string>();
            using (CaptureUserFaults(captured))
            {
                // Act
                Func<Task> act = () => (Task)method.Invoke(controller, Array.Empty<object>());

                // Assert
                await act.Should()
                    .NotThrowAsync(
                        "the host initializer is fire-and-forget, so it must contain its own fault"
                    );
                captured
                    .Should()
                    .ContainSingle(
                        "the final initialization failure must be reported to the user exactly"
                            + " once"
                    )
                    .Which.Should()
                    .Contain(
                        "after 3 attempts",
                        "the report must name the exhausted attempt limit"
                    );
            }
        }
    }
}
