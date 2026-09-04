using System;
using System.Linq;
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
    public partial class EfcFormControllerTests
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

        // RC1 (#460 A/C, #464 A, #465 A): the theme/dark-mode accessors must be readable on the
        // all-fields-null post-Cleanup() state that CreateMinimalController reproduces.
        [TestMethod]
        public void FormDarkMode_OnAllFieldsNullController_ReturnsFalseAndDoesNotThrow()
        {
            // Arrange
            var controller = CreateMinimalController();

            // Act
            Func<bool> act = () => controller.DarkMode;

            // Assert
            act.Should().NotThrow("DarkMode must be readable after Cleanup nulled _globals");
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
            act.Should().NotThrow("ActiveTheme must be readable after Cleanup nulled _themes");
            controller.ActiveTheme.Should().Be(backingField, "the getter returns the field");
        }

        [TestMethod]
        public void FormLoadTheme_OnAllFieldsNullController_DoesNotThrow()
        {
            // Arrange
            var controller = CreateMinimalController();

            // Act
            Func<string> act = () => controller.LoadTheme();

            // Assert
            act.Should().NotThrow("LoadTheme must not apply a theme when _themes is null");
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
            act.Should().NotThrow("Cleanup must be idempotent on a partial controller");
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
            parentCleanup.Verify(c => c(), Times.Once(), "the parent runs exactly once");
            GetPrivateField(controller, "_parentCleanup")
                .Should()
                .BeNull("the field is cleared before the captured local is invoked");
        }

        // #464 B (RC3). Each extracted boundary member must log through the sink and contain the
        // fault rather than rethrow it out of an async void rim. Per D9 each [DataRow] is a
        // distinct named result.
        [DataTestMethod]
        [DataRow("ButtonCancelClickAsync")]
        [DataRow("ButtonOkClickAsync")]
        [DataRow("ButtonRefreshClickAsync")]
        [DataRow("ButtonCreateClickAsync")]
        [DataRow("ButtonDeleteClickAsync")]
        public async Task AsyncVoidBoundary_WhenFaulted_LogsOnceAndDoesNotThrow(string memberName)
        {
            // Arrange: the all-fields-null state is the fault injection. The first statement each
            // member reaches dereferences the null _formViewer or awaits its null UiSyncContext.
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
                member.Should().NotBeNull($"{memberName} must exist as an internal async Task");

                // Act
                Func<Task> act = () => (Task)member.Invoke(controller, Array.Empty<object>());

                // Assert
                await act.Should()
                    .NotThrowAsync($"{memberName} must contain the fault, not rethrow it");
                sinkCallCount.Should().Be(1, $"{memberName} must report the fault exactly once");
            }
            finally
            {
                SynchronizationContext.SetSynchronizationContext(previousContext);
            }
        }

        [TestMethod]
        public void BoundaryErrorSink_DefaultDelegate_InvokesWithoutThrowing()
        {
            // Arrange: the default delegate is left in place, so this covers its body.
            var controller = CreateMinimalController();

            // Act
            Action act = () =>
                controller.BoundaryErrorSink("smoke", new InvalidOperationException());

            // Assert
            act.Should().NotThrow("the default sink must be safe on a released controller");
        }

        // #464 C (RC3). Both call sites discard the result, so a fault becomes an unobserved
        // faulted Task. The uninitialized EfcViewer clears the null-viewer early return while
        // running no constructor; _dataModel stays null so the first collaborator call faults.
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

        // #465 B (RC8). The extracted pure matching helper is exercised with no EfcViewer and no
        // controller, which is what makes the relocated read testable at all.
        [TestMethod]
        public void MatchesForSearchText_WithRepresentativeInput_ReturnsExpectedMatches()
        {
            // Arrange
            const string searchText = "north";
            var expected = new[] { @"Clients\North", @"Clients\Northeast", @"Archive\Northwind" };
            string observedArgument = null;
            Func<string, string[]> findMatches = text =>
            {
                observedArgument = text;
                return text == searchText ? expected : Array.Empty<string>();
            };

            // Act / Assert
            EfcFormController
                .MatchesForSearchText(findMatches, searchText)
                .Should()
                .Equal(expected, "the helper returns the delegate result verbatim");
            EfcFormController
                .MatchesForSearchText(null, searchText)
                .Should()
                .BeEmpty("a null delegate yields an empty array rather than a null dereference");
            EfcFormController
                .MatchesForSearchText(findMatches, null)
                .Should()
                .BeEmpty("a null search text yields no matches");
            observedArgument
                .Should()
                .BeEmpty("a null search text is passed through to the delegate as an empty string");
        }

        // #465 C (RC9). WithTrashRow is the pure half of the delete gesture.
        [TestMethod]
        public void WithTrashRow_AppliedTwice_YieldsExactlyOneTrashRow()
        {
            // Arrange
            var rows = new[] { @"Clients\North", @"Clients\South" };

            // Act
            string[] once = EfcFormController.WithTrashRow(rows);
            string[] twice = EfcFormController.WithTrashRow(once);

            // Assert
            twice
                .Where(row => row == EfcFormController.TrashRowText)
                .Should()
                .HaveCount(1, "the trash row must not accumulate on a repeated delete gesture");
            twice
                .Should()
                .BeSameAs(once, "an input already carrying the trash row is returned as-is");
            twice.Should().Contain(rows, "the presented rows survive the gesture");
        }

        // #465 C (RC9). Drives the criterion's literal instrument, ActionDeleteAsync itself.
        // Injecting any non-null SynchronizationContext satisfies the SynchronizationContextAwaiter
        // null guard, so the awaited UI-thread marshal completes headlessly; the uninitialized
        // EfcViewer runs no constructor so it has no handle, no controls and no message pump; and
        // with _router left null BindFolderRows returns at its guard without touching the
        // breadcrumb host, while ApplyDeleteGesture has already assigned _folderRows.
        [TestMethod]
        public async Task ActionDeleteAsync_AwaitedTwice_LeavesExactlyOneTrashRowInFolderRows()
        {
            // Arrange
            var rows = new[] { @"Clients\North", @"Clients\South" };
            var controller = CreateMinimalController();
            var viewer = (EfcViewer)
                System.Runtime.Serialization.FormatterServices.GetUninitializedObject(
                    typeof(EfcViewer)
                );
            SetPrivateField(viewer, "_context", new SynchronizationContext());
            SetPrivateField(controller, "_formViewer", viewer);
            SetPrivateField(controller, "_folderRows", rows);

            // Act
            await controller.ActionDeleteAsync();
            await controller.ActionDeleteAsync();

            // Assert
            var folderRows = (string[])GetPrivateField(controller, "_folderRows");
            folderRows
                .Where(row => row == EfcFormController.TrashRowText)
                .Should()
                .HaveCount(1, "a repeated delete gesture must not accumulate trash rows");
            folderRows.Should().Contain(rows, "both original rows survive the gesture");
        }

        // #465 D (RC7). The producers emit a four-character banner prefix
        // (BreadcrumbRowBuilder.BannerPrefix), so a three-equals row is shorter than the prefix.
        [TestMethod]
        public void IsBannerRow_ClassifiesByTheFourCharacterPrefix()
        {
            // Act / Assert
            EfcFormController
                .IsBannerRow("===")
                .Should()
                .BeFalse("a three-equals row is shorter than the producer prefix");
            EfcFormController
                .IsBannerRow("====")
                .Should()
                .BeTrue("a four-equals row carries the producer prefix");
            BreadcrumbRowBuilder
                .BannerPrefix.Should()
                .Be("====", "IsBannerRow classifies by the prefix the row producers emit");
        }

        [TestMethod]
        public void IsBannerRow_NullOrShortRow_ReturnsFalseWithoutThrowing()
        {
            // Act / Assert
            foreach (var row in new[] { null, string.Empty, "=" })
            {
                Func<bool> act = () => EfcFormController.IsBannerRow(row);
                act.Should().NotThrow($"a row of length {row?.Length ?? -1} must not throw");
                act().Should().BeFalse("a row shorter than the prefix is not a banner");
            }
        }

        // Both EFC classification sites must agree. The creation path is IsValidSelection, which is
        // IsSelectableFolder; the filing path is the guard expression ActionOkAsync composes. The
        // guard is reproduced rather than driven, because ActionOkAsync shows a MessageBox.
        [TestMethod]
        public void IsSelectableFolder_AndIsBannerRow_ClassifyThreeAndFourEqualsRowsIdentically()
        {
            // Act / Assert
            foreach (var row in new[] { "===", "====" })
            {
                bool creationPath = EfcFormController.IsSelectableFolder(row);
                bool filingPath =
                    !EfcFormController.IsBannerRow(row)
                    && EfcSelectionGuard.IsValidFilingSelection(row);
                creationPath.Should().Be(filingPath, $"both sites must classify {row} alike");
                creationPath.Should().BeFalse($"{row} is rejected at both sites");
            }
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
