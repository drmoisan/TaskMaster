using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using QuickFiler.Controllers;
using QuickFiler.Viewers;
using UtilitiesCS;
using UtilitiesCS.OutlookObjects.Folder;

namespace QuickFiler.Controllers.Tests
{
    [TestClass]
    public class EfcFormControllerTests
    {
        /// <summary>
        /// Creates an EfcFormController via the private no-arg constructor, which allocates
        /// the object without initializing any sub-components, leaving all fields null.
        /// Used to exercise method-level guards without a live Outlook COM context.
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

        // Regression test for issue #145. A minimally constructed controller now returns
        // before touching `_dataModel` when `_formViewer` has already been cleared, which is
        // the safety contract that prevents the post-await null race from surfacing as a UI
        // thread crash.
        [TestMethod]
        public async Task PopulateFolderCombobox_WhenFormViewerIsNull_ReturnsWithoutTouchingDataModel()
        {
            // Arrange
            // Both `_dataModel` and `_formViewer` are null in a minimally constructed
            // controller. The current contract is that `PopulateFolderCombobox` should exit
            // immediately when the viewer has already been cleaned up, which also avoids
            // dereferencing `_dataModel` in this COM-free test path.
            var controller = CreateMinimalController();

            // Act
            Func<Task> act = () => controller.PopulateFolderCombobox();

            // Assert
            await act.Should()
                .NotThrowAsync(
                    "PopulateFolderCombobox should return immediately when Cleanup has already"
                        + " cleared the form viewer, instead of dereferencing downstream state"
                );
        }

        [TestMethod]
        public async Task Issue439BindBreadcrumbRowsAsync_SubmitsArchiveRootToRealRouter()
        {
            // Arrange: a private no-argument construction and strict interface seams keep this
            // binding-boundary test independent of WinForms, WebView2, Outlook COM, and a UI pump.
            const string archiveRoot = @"\Archive";
            const string presentedTarget = @"Clients\North";
            const string hierarchyTarget = @"\Archive\Clients\North";
            var provider = new Mock<IFolderHierarchyProvider>(MockBehavior.Strict);
            var host = new Mock<IBreadcrumbWebHost>(MockBehavior.Strict);
            var ol = new Mock<IOlObjects>(MockBehavior.Strict);
            var globals = new Mock<IApplicationGlobals>(MockBehavior.Strict);
            var key = new FolderTreeNodeKey("archive", hierarchyTarget, hierarchyTarget);

            host.SetupGet(value => value.IsCoreInitialized).Returns(true);
            host.SetupAdd(value => value.MessageReceived += It.IsAny<EventHandler<string>>());
            host.Setup(value => value.NavigateToString(It.IsAny<string>()));
            host.Setup(value => value.PostMessageJson(It.IsAny<string>()));
            ol.SetupGet(value => value.ArchiveRootPath).Returns(archiveRoot);
            globals.SetupGet(value => value.Ol).Returns(ol.Object);
            provider
                .Setup(value =>
                    value.ResolveLeafKeyAsync(
                        hierarchyTarget,
                        It.IsAny<System.Threading.CancellationToken>()
                    )
                )
                .ReturnsAsync(key);
            provider
                .Setup(value =>
                    value.GetAncestorChainAsync(key, It.IsAny<System.Threading.CancellationToken>())
                )
                .ReturnsAsync(
                    new[]
                    {
                        new FolderBreadcrumbSegment(
                            new FolderTreeNodeKey("archive", archiveRoot, archiveRoot),
                            "Archive",
                            archiveRoot,
                            true
                        ),
                        new FolderBreadcrumbSegment(
                            new FolderTreeNodeKey(
                                "archive",
                                @"\Archive\Clients",
                                @"\Archive\Clients"
                            ),
                            "Clients",
                            @"\Archive\Clients",
                            true
                        ),
                        new FolderBreadcrumbSegment(key, "North", hierarchyTarget, false),
                    }
                );
            var router = new BreadcrumbBridgeRouter(
                provider.Object,
                host.Object,
                new BreadcrumbMessageCodec(),
                new BreadcrumbHtmlRenderer(),
                new BreadcrumbOutboundQueue(host.Object)
            );
            var controller = CreateMinimalController();
            SetPrivateField(controller, "_globals", globals.Object);
            SetPrivateField(controller, "_router", router);

            // Act
            await controller.BindBreadcrumbRowsAsync(new[] { presentedTarget });

            // Assert
            provider.Verify(
                value =>
                    value.ResolveLeafKeyAsync(
                        hierarchyTarget,
                        It.IsAny<System.Threading.CancellationToken>()
                    ),
                Times.Once
            );
            provider.Verify(
                value =>
                    value.GetAncestorChainAsync(
                        key,
                        It.IsAny<System.Threading.CancellationToken>()
                    ),
                Times.Once
            );
            host.Verify(
                value => value.NavigateToString(It.Is<string>(html => html.Contains("North"))),
                Times.Once
            );
            host.VerifyAdd(
                value => value.MessageReceived += It.IsAny<EventHandler<string>>(),
                Times.Once
            );
            host.VerifyGet(value => value.IsCoreInitialized, Times.Once);
            provider.VerifyNoOtherCalls();
            host.VerifyNoOtherCalls();
            ol.VerifyGet(value => value.ArchiveRootPath, Times.Once);
            globals.VerifyGet(value => value.Ol, Times.Once);
        }

        // RC1 (issues #460 A/C, #464 A, #465 A): the three theme/dark-mode accessors must be
        // readable on a post-Cleanup() controller, whose fields are all null. CreateMinimalController
        // reproduces exactly that state without a live Outlook COM context.
        [TestMethod]
        public void FormDarkMode_OnAllFieldsNullController_ReturnsFalseAndDoesNotThrow()
        {
            // Arrange
            var controller = CreateMinimalController();

            // Act
            Func<bool> act = () => controller.DarkMode;

            // Assert
            act.Should()
                .NotThrow(
                    "DarkMode must be readable after Cleanup has nulled _globals, instead of"
                        + " eagerly materialising a dependency array over a null reference"
                );
            controller.DarkMode.Should().BeFalse("the _darkMode backing field defaults to false");
        }

        [TestMethod]
        public void FormActiveTheme_OnAllFieldsNullController_ReturnsBackingFieldAndDoesNotThrow()
        {
            // Arrange
            var controller = CreateMinimalController();
            var backingField = (string)GetPrivateField(controller, "_activeTheme");

            // Act
            Func<string> act = () => controller.ActiveTheme;

            // Assert
            act.Should()
                .NotThrow(
                    "ActiveTheme must be readable after Cleanup has nulled _themes, instead of"
                        + " failing the strict dependency check with ArgumentNullException"
                );
            controller
                .ActiveTheme.Should()
                .Be(
                    backingField,
                    "the guarded getter returns the _activeTheme backing field verbatim when"
                        + " _themes is null"
                );
        }

        [TestMethod]
        public void FormLoadTheme_OnAllFieldsNullController_DoesNotThrow()
        {
            // Arrange
            var controller = CreateMinimalController();

            // Act
            Func<string> act = () => controller.LoadTheme();

            // Assert
            act.Should()
                .NotThrow(
                    "LoadTheme must compute and return a theme name without applying it when"
                        + " _themes is null, so a torn-down controller cannot fault"
                );
        }

        // RC1: Cleanup() must be idempotent and must not double-invoke the parent callback.
        [TestMethod]
        public void FormCleanup_CalledTwice_DoesNotThrow()
        {
            // Arrange
            var controller = CreateMinimalController();

            // Act
            Action act = () =>
            {
                controller.Cleanup();
                controller.Cleanup();
            };

            // Assert
            act.Should()
                .NotThrow(
                    "Cleanup must be callable on a partially constructed controller and must be"
                        + " idempotent, so a second teardown pass cannot fault"
                );
        }

        [TestMethod]
        public void FormCleanup_InvokesParentCleanupExactlyOnce()
        {
            // Arrange
            var controller = CreateMinimalController();
            var parentCleanup = new Mock<Action>();
            SetPrivateField(controller, "_parentCleanup", parentCleanup.Object);

            // Act
            controller.Cleanup();
            controller.Cleanup();

            // Assert
            parentCleanup.Verify(
                callback => callback(),
                Times.Once(),
                "the parent teardown callback must run exactly once no matter how many times"
                    + " Cleanup is called"
            );
            GetPrivateField(controller, "_parentCleanup")
                .Should()
                .BeNull(
                    "Cleanup nulls the field before invoking the captured local, which is what"
                        + " makes the single invocation structural rather than incidental"
                );
        }

        // #464 B (RC3). Each of the five extracted boundary members must log through the
        // injectable sink and contain the fault, instead of rethrowing it out of an async void
        // rim where it becomes an unhandled UI-thread crash. Under decision D9 each [DataRow] is
        // a distinct named test result with its own name and outcome in the TRX.
        [DataTestMethod]
        [DataRow("ButtonCancelClickAsync")]
        [DataRow("ButtonOkClickAsync")]
        [DataRow("ButtonRefreshClickAsync")]
        [DataRow("ButtonCreateClickAsync")]
        [DataRow("ButtonDeleteClickAsync")]
        public async Task AsyncVoidBoundary_WhenFaulted_LogsOnceAndDoesNotThrow(string memberName)
        {
            // Arrange
            // The fault is injected by the all-fields-null state itself: the first statement each
            // extracted member reaches dereferences the null _formViewer, or awaits its null
            // UiSyncContext, which throws because _formViewer is itself null.
            SynchronizationContext previousContext = SynchronizationContext.Current;
            SynchronizationContext.SetSynchronizationContext(null);
            try
            {
                var controller = CreateMinimalController();
                var sinkCallCount = 0;
                controller.BoundaryErrorSink = (message, exception) => sinkCallCount++;
                MethodInfo member = typeof(EfcFormController).GetMethod(
                    memberName,
                    BindingFlags.Instance | BindingFlags.NonPublic
                );
                member
                    .Should()
                    .NotBeNull(
                        $"{memberName} must exist as an extracted internal async Task member"
                    );

                // Act
                Func<Task> act = () => (Task)member.Invoke(controller, Array.Empty<object>());

                // Assert
                await act.Should()
                    .NotThrowAsync(
                        $"{memberName} is the fault boundary for an async void rim, so it must"
                            + " contain the fault rather than rethrow it into the UI message loop"
                    );
                sinkCallCount
                    .Should()
                    .Be(
                        1,
                        $"{memberName} must report the contained fault through the boundary error"
                            + " sink exactly once"
                    );
            }
            finally
            {
                SynchronizationContext.SetSynchronizationContext(previousContext);
            }
        }

        [TestMethod]
        public void BoundaryErrorSink_DefaultDelegate_InvokesWithoutThrowing()
        {
            // Arrange
            // The default delegate is left in place, so this covers its body: a single
            // logger.Error(message, exception) call on the pre-existing static logger.
            var controller = CreateMinimalController();

            // Act
            Action act = () =>
                controller.BoundaryErrorSink(
                    "boundary sink smoke",
                    new InvalidOperationException()
                );

            // Assert
            act.Should()
                .NotThrow(
                    "the default sink delegate must be safe to invoke on a controller whose"
                        + " collaborators have all been released"
                );
        }

        // #464 C (RC3). PopulateFolderCombobox is invoked fire-and-forget from two call sites, so
        // a fault inside it becomes an unobserved faulted Task with no catch anywhere on the path.
        // The uninitialized EfcViewer gets past the pre-existing null-viewer early return without
        // running a constructor, so it has no handle, no controls and no message pump; _dataModel
        // is left null so the first collaborator call inside the method faults.
        [TestMethod]
        public async Task PopulateFolderCombobox_WhenDataModelFaults_LogsOnceAndDoesNotFault()
        {
            // Arrange
            var controller = CreateMinimalController();
            var viewer = (EfcViewer)
                System.Runtime.Serialization.FormatterServices.GetUninitializedObject(
                    typeof(EfcViewer)
                );
            SetPrivateField(controller, "_formViewer", viewer);
            var sinkCallCount = 0;
            controller.BoundaryErrorSink = (message, exception) => sinkCallCount++;

            // Act
            Func<Task> act = () => controller.PopulateFolderCombobox();

            // Assert
            await act.Should()
                .NotThrowAsync(
                    "a fire-and-forget call site cannot observe a faulted Task, so the method must"
                        + " contain its own fault instead of returning one"
                );
            sinkCallCount
                .Should()
                .Be(
                    1,
                    "the contained fault must be reported through the boundary error sink exactly"
                        + " once"
                );
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            var field = target
                .GetType()
                .GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            field.Should().NotBeNull($"{fieldName} must remain available for this headless seam");
            field.SetValue(target, value);
        }

        private static object GetPrivateField(object target, string fieldName)
        {
            var field = target
                .GetType()
                .GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            field.Should().NotBeNull($"{fieldName} must remain available for this headless seam");
            return field.GetValue(target);
        }
    }
}
