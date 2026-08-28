using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using QuickFiler.Interfaces;
using UtilitiesCS;

namespace QuickFiler.Controllers.Tests
{
    /// <summary>
    /// Regression tests for Issue #251: <see cref="QfcCollectionController"/> subscribes
    /// <c>DarkMode_CheckedChanged</c> to <c>_globals.Ol.PropertyChanged</c> in its constructor
    /// (via <c>SetupLightDark</c>) but, before the fix, never unsubscribed in
    /// <c>Cleanup()</c>/<c>CleanupAsync()</c>. A dark-mode toggle firing after cleanup dereferenced
    /// the now-null <c>_globals</c> field, throwing a <see cref="NullReferenceException"/>.
    /// </summary>
    [TestClass]
    public class QfcCollectionControllerDarkModeTests
    {
        /// <summary>
        /// Constructs a <see cref="QfcCollectionController"/> via its real constructor with every
        /// collaborator mocked or supplied as a real, side-effect-free value. Only <c>Ol</c> exposes a
        /// real, raisable <see cref="INotifyPropertyChanged.PropertyChanged"/> event so the test can
        /// simulate a dark-mode toggle without touching Outlook/COM/WinForms.
        /// </summary>
        private static QfcCollectionController CreateController(out Mock<IOlObjects> mockOl)
        {
            mockOl = new Mock<IOlObjects>();
            mockOl.SetupGet(o => o.DarkMode).Returns(true);

            var mockGlobals = new Mock<IApplicationGlobals>();
            mockGlobals.SetupGet(g => g.Ol).Returns(mockOl.Object);

            var mockFormViewer = new Mock<IQfcFormViewer>();

            var mockKeyboardHandler = new Mock<IQfcKeyboardHandler>();
            var mockHomeController = new Mock<IFilerHomeController>();
            mockHomeController.SetupGet(h => h.KeyboardHandler).Returns(mockKeyboardHandler.Object);

            var mockParent = new Mock<IQfcFormController>();

            var tokenSource = new CancellationTokenSource();
            var token = tokenSource.Token;

            return new QfcCollectionController(
                mockGlobals.Object,
                mockFormViewer.Object,
                QfEnums.InitTypeEnum.Sort,
                mockHomeController.Object,
                mockParent.Object,
                tokenSource,
                token,
                new TlpCellStates()
            );
        }

        /// <summary>
        /// Injects a single <see cref="QfcItemGroup"/> carrying a mocked <see cref="IQfcItemController"/>
        /// into the controller's private <c>_itemGroups</c> field, purely for observing whether
        /// <c>SetThemeDark</c>/<c>SetThemeLight</c> are invoked. This is done after cleanup so it does
        /// not interfere with the cleanup path itself (which must take the null-<c>_itemGroups</c>
        /// early-exit inside <c>RemoveControls</c>/<c>RemoveControlsAsync</c>).
        /// </summary>
        private static Mock<IQfcItemController> InjectObservableItemGroup(
            QfcCollectionController controller
        )
        {
            var mockItemController = new Mock<IQfcItemController>();
            var group = new QfcItemGroup { ItemController = mockItemController.Object };

            typeof(QfcCollectionController)
                .GetField("_itemGroups", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(controller, new List<QfcItemGroup> { group });

            return mockItemController;
        }

        /// <summary>
        /// Asserts that raising a dark-mode <c>PropertyChanged</c> notification after cleanup never
        /// results in a theme-change call, satisfying AC5. Called after the exception assertion so a
        /// pre-fix throw is captured first.
        /// </summary>
        private static void AssertNoThemeChangeInvoked(Mock<IQfcItemController> mockItemController)
        {
            mockItemController.Verify(c => c.SetThemeDark(It.IsAny<bool>()), Times.Never);
            mockItemController.Verify(c => c.SetThemeLight(It.IsAny<bool>()), Times.Never);
        }

        /// <summary>
        /// Regression test for Issue #251 (synchronous cleanup path). Before the fix,
        /// <c>Cleanup()</c> nulls <c>_globals</c> without unsubscribing <c>DarkMode_CheckedChanged</c>
        /// from <c>_globals.Ol.PropertyChanged</c>; the next dark-mode toggle re-enters the handler and
        /// dereferences the null <c>_globals</c>, throwing <see cref="NullReferenceException"/>. After
        /// the fix, the unsubscribe (AC2) and the defensive guard in the handler (AC4) mean the raised
        /// event produces no exception and no theme-change call (AC5).
        /// </summary>
        [TestMethod]
        public void Cleanup_ThenDarkModePropertyChanged_DoesNotThrow()
        {
            // Arrange
            var controller = CreateController(out var mockOl);
            controller.Cleanup();
            var mockItemController = InjectObservableItemGroup(controller);

            // Act
            Action act = () =>
                mockOl.Raise(
                    o => o.PropertyChanged += null,
                    mockOl.Object,
                    new PropertyChangedEventArgs("DarkMode")
                );

            // Assert
            act.Should()
                .NotThrow(
                    "DarkMode_CheckedChanged must not dereference cleaned-up state after Cleanup()"
                );
            AssertNoThemeChangeInvoked(mockItemController);
        }

        /// <summary>
        /// Regression test for Issue #251 (asynchronous cleanup path). Identical arrangement to
        /// <see cref="Cleanup_ThenDarkModePropertyChanged_DoesNotThrow"/> but exercises
        /// <c>CleanupAsync()</c> instead of the synchronous <c>Cleanup()</c>.
        /// </summary>
        [TestMethod]
        public async Task CleanupAsync_ThenDarkModePropertyChanged_DoesNotThrow()
        {
            // Arrange
            var controller = CreateController(out var mockOl);
            await controller.CleanupAsync();
            var mockItemController = InjectObservableItemGroup(controller);

            // Act
            Action act = () =>
                mockOl.Raise(
                    o => o.PropertyChanged += null,
                    mockOl.Object,
                    new PropertyChangedEventArgs("DarkMode")
                );

            // Assert
            act.Should()
                .NotThrow(
                    "DarkMode_CheckedChanged must not dereference cleaned-up state after CleanupAsync()"
                );
            AssertNoThemeChangeInvoked(mockItemController);
        }
    }
}
