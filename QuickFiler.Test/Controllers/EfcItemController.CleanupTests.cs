using System;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using QuickFiler.Controllers;
using QuickFiler.Interfaces;
using UtilitiesCS;

namespace QuickFiler.Controllers.Tests
{
    /// <summary>
    /// Regression tests for issue #460 — the <c>EfcItemController.Cleanup()</c> null-dereference
    /// and undisposed-timer defects — together with the item-side post-teardown accessor guards
    /// of issue #464.
    /// </summary>
    /// <remarks>
    /// Held in a second item-side file, separate from <c>EfcItemControllerTests.cs</c>, so that
    /// neither file approaches the 500-line ceiling. The timer test arms with
    /// <c>Timeout.Infinite</c> for both due time and period and observes disposal through the
    /// <c>ObjectDisposedException</c> that <c>Timer.Change</c> then throws; it never waits.
    /// </remarks>
    [TestClass]
    public class EfcItemControllerCleanupTests
    {
        private const BindingFlags DeclaredInstance =
            BindingFlags.Instance | BindingFlags.NonPublic;

        /// <summary>
        /// Builds an <c>EfcItemController</c> through the five-parameter constructor using
        /// interface seams and a headless <c>ItemViewer</c>. <c>Initialize</c> is never called,
        /// so the controller is left in the partially constructed state <c>Cleanup()</c> must
        /// survive.
        /// </summary>
        private static EfcItemController CreateFiveArgumentController()
        {
            var globals = new Mock<IApplicationGlobals>();
            var homeController = new Mock<IFilerHomeController>();
            var viewer = new QuickFiler.ItemViewer();

            // The window handle of the viewer is already created by the time its constructor
            // returns, because the WebView2 children force it. Substituting a real, parentless
            // FastObjectListView keeps the control type identical while leaving its native
            // handle uncreated, which a host with no message pump could never complete.
            viewer.TopicThread = new BrightIdeasSoftware.FastObjectListView();

            return new EfcItemController(
                globals.Object,
                homeController.Object,
                null,
                viewer,
                default(CancellationToken)
            );
        }

        private static EfcItemController CreateUninitializedController() =>
            (EfcItemController)FormatterServices.GetUninitializedObject(typeof(EfcItemController));

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            FieldInfo field = target.GetType().GetField(fieldName, DeclaredInstance);
            field.Should().NotBeNull($"{fieldName} must remain available for this headless seam");
            field.SetValue(target, value);
        }

        private static object GetPrivateField(object target, string fieldName)
        {
            FieldInfo field = target.GetType().GetField(fieldName, DeclaredInstance);
            field.Should().NotBeNull($"{fieldName} must remain available for this headless seam");
            return field.GetValue(target);
        }

        [TestMethod]
        public void Cleanup_OnFiveArgumentConstructedController_DoesNotThrow()
        {
            // Arrange
            EfcItemController controller = CreateFiveArgumentController();

            // Act
            Action act = () => controller.Cleanup();

            // Assert
            act.Should()
                .NotThrow(
                    "Cleanup must remain callable on a partially constructed controller whose"
                        + " Initialize was never run, so no field it touches may be dereferenced"
                        + " unguarded"
                );
        }

        [TestMethod]
        public void Cleanup_CalledTwice_DoesNotThrow()
        {
            // Arrange
            EfcItemController controller = CreateFiveArgumentController();

            // Act
            Action act = () =>
            {
                controller.Cleanup();
                controller.Cleanup();
            };

            // Assert
            act.Should()
                .NotThrow("Cleanup must be idempotent, so a second teardown pass cannot fault");
        }

        [TestMethod]
        public void Cleanup_NullsButtonsField()
        {
            // Arrange
            EfcItemController controller = CreateFiveArgumentController();

            // Act
            controller.Cleanup();

            // Assert
            GetPrivateField(controller, "_buttons")
                .Should()
                .BeNull(
                    "Cleanup detaches both mouse handlers from the button list and then releases"
                        + " the list itself, so a torn-down controller retains no control reference"
                );
        }

        [TestMethod]
        public void Cleanup_DisposesTimerBeforeNullingIt()
        {
            // Arrange
            // Timeout.Infinite for both the due time and the period means the callback can never
            // run, so disposal is observed purely as state and never as a race.
            EfcItemController controller = CreateFiveArgumentController();
            var timer = new Timer(_ => { }, null, Timeout.Infinite, Timeout.Infinite);
            SetPrivateField(controller, "_timer", timer);

            // Act
            controller.Cleanup();

            // Assert
            GetPrivateField(controller, "_timer")
                .Should()
                .BeNull("Cleanup releases the timer field after disposing it");
            Action rearm = () => timer.Change(0, Timeout.Infinite);
            rearm
                .Should()
                .Throw<ObjectDisposedException>(
                    "the timer must be disposed before the field is nulled, otherwise the thread"
                        + " pool retains a callback into a torn-down controller"
                );
        }

        [TestMethod]
        public void ApplyReadEmailFormat_AfterCleanup_DoesNotThrow()
        {
            // Arrange
            EfcItemController controller = CreateFiveArgumentController();
            controller.Cleanup();

            // Act
            Action act = () => controller.ApplyReadEmailFormat(null);

            // Assert
            act.Should()
                .NotThrow(
                    "a post-teardown thread-pool timer callback must early-return without side"
                        + " effect, which is the expected steady state rather than an error"
                );
        }

        [TestMethod]
        public void SubjectSenderAndTo_ReadFromItemInfo_AndAreInertAfterCleanup()
        {
            // Arrange
            const string subject = "cached subject";
            const string sender = "cached sender";
            const string recipients = "cached recipients";
            EfcItemController controller = CreateFiveArgumentController();
            var itemInfo = new Mock<MailItemHelper>();
            itemInfo.SetupGet(info => info.Subject).Returns(subject);
            itemInfo.SetupGet(info => info.SenderName).Returns(sender);
            itemInfo.SetupGet(info => info.ToRecipientsName).Returns(recipients);
            SetPrivateField(controller, "_itemInfo", itemInfo.Object);

            // Act
            string observedSubject = controller.Subject;
            string observedSender = controller.Sender;
            string observedTo = controller.To;

            // Assert
            observedSubject
                .Should()
                .Be(
                    subject,
                    "Subject must read the cached mail-item model like Sender and To, not the"
                        + " label text of a control the teardown path has already released"
                );
            observedSender.Should().Be(sender);
            observedTo.Should().Be(recipients);

            controller.Cleanup();
            Action readAfterCleanup = () =>
            {
                _ = controller.Subject;
                _ = controller.Sender;
                _ = controller.To;
            };
            readAfterCleanup
                .Should()
                .NotThrow(
                    "all three getters must be null-safe once Cleanup has released _itemInfo"
                );
        }

        [TestMethod]
        public void ItemDarkMode_OnNullGlobalsController_ReturnsFalseAndDoesNotThrow()
        {
            // Arrange
            EfcItemController controller = CreateUninitializedController();

            // Act
            Func<bool> act = () => controller.DarkMode;

            // Assert
            act.Should()
                .NotThrow(
                    "DarkMode must be readable once _globals is null, instead of eagerly"
                        + " materialising a dependency array over a null reference"
                );
            controller.DarkMode.Should().BeFalse("the _darkMode backing field defaults to false");
        }

        [TestMethod]
        public void ItemActiveThemeAndLoadTheme_OnNullThemesController_DoNotThrow()
        {
            // Arrange
            EfcItemController controller = CreateUninitializedController();

            // Act
            Func<string> readActiveTheme = () => controller.ActiveTheme;
            Func<string> invokeLoadTheme = () => controller.LoadTheme();

            // Assert
            readActiveTheme
                .Should()
                .NotThrow(
                    "ActiveTheme must return its backing field once _themes is null, instead of"
                        + " failing the strict dependency check with ArgumentNullException"
                );
            invokeLoadTheme
                .Should()
                .NotThrow(
                    "LoadTheme must compute and return a theme name without applying it when"
                        + " _themes is null, so a torn-down controller cannot fault"
                );
        }
    }
}
