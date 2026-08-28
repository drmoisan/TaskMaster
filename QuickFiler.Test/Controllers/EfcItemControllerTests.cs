using System;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using QuickFiler.Controllers;
using QuickFiler.Interfaces;

namespace QuickFiler.Controllers.Tests
{
    /// <summary>
    /// Regression tests for the <c>EfcItemController</c> surface defects closed by issues
    /// #459, #461, #463, #464 D and E, and the <c>EfcItemController</c> half of #466.
    /// </summary>
    /// <remarks>
    /// Several of the defects covered here are latent: the members they sit on have no
    /// reachable call path, so the regression test pins the post-change contract by direct
    /// invocation or by a type-metadata assertion rather than reproducing a user-visible
    /// failure. Cleanup and timer behaviour for #460 lives in
    /// <c>EfcItemController.CleanupTests.cs</c>, not here.
    /// </remarks>
    [TestClass]
    public class EfcItemControllerTests
    {
        private const BindingFlags DeclaredInstance =
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

        /// <summary>
        /// #459 A. <c>RegisterActions</c> misused the <c>KbdActions&lt;&gt;</c> indexer setter,
        /// which is assign-if-present and never inserts, so its <c>overwriteDuplicates: false</c>
        /// path registered nothing. It had zero call sites, so the remedy is removal; this test
        /// pins that the member is gone rather than repaired.
        /// </summary>
        [TestMethod]
        public void RegisterActions_IsAbsentFromEfcItemControllerMetadata()
        {
            // Arrange / Act
            MethodInfo registerActions = typeof(EfcItemController).GetMethod(
                "RegisterActions",
                BindingFlags.NonPublic | BindingFlags.Instance
            );

            // Assert
            registerActions
                .Should()
                .BeNull(
                    "RegisterActions is dead code that misuses the KbdActions<> indexer setter and #459 A closes it by removal"
                );
        }

        /// <summary>
        /// #459 B and C, and #464 D. Both synchronous expansion overloads are removed: they were
        /// the sole writers of the <c>'B'</c> and <c>'D'</c> <c>CharActions</c> entries and the
        /// sole home of the two genuine <c>async void</c> lambdas. The surviving asynchronous
        /// expansion path must remain declared, which is what makes the deletion behaviour-neutral.
        /// </summary>
        [TestMethod]
        public void ToggleExpansion_IsAbsentAtEveryArity()
        {
            // Arrange
            MethodInfo[] declared = typeof(EfcItemController).GetMethods(DeclaredInstance);

            // Act
            MethodInfo[] synchronousOverloads = declared
                .Where(candidate => candidate.Name == "ToggleExpansion")
                .ToArray();
            MethodInfo[] asynchronousOverloads = declared
                .Where(candidate => candidate.Name == "ToggleExpansionAsync")
                .ToArray();

            // Assert
            synchronousOverloads
                .Should()
                .BeEmpty(
                    "the dead synchronous expansion path is removed at every arity by #459 B/C, which also closes #464 D"
                );
            asynchronousOverloads
                .Should()
                .HaveCount(
                    2,
                    "the live asynchronous expansion path keeps both ToggleExpansionAsync overloads"
                );
        }

        /// <summary>
        /// #466 B, and the dead third site of #463. <c>InitializeWebView()</c> had zero call sites,
        /// so the EN DASH incognito literal it contained is removed with its container rather than
        /// edited in place.
        /// </summary>
        [TestMethod]
        public void InitializeWebView_IsAbsentFromEfcItemControllerMetadata()
        {
            // Arrange / Act
            MethodInfo initializeWebView = typeof(EfcItemController).GetMethod(
                "InitializeWebView",
                BindingFlags.NonPublic | BindingFlags.Instance
            );

            // Assert
            initializeWebView
                .Should()
                .BeNull(
                    "InitializeWebView has zero call sites and #466 B closes it by removal, taking the dead EN DASH literal with it"
                );
        }

        /// <summary>
        /// #466 C. The seven-parameter constructor had zero call sites. The two overloads that are
        /// actually constructed, from <c>EfcFormController</c>, must survive.
        /// </summary>
        [TestMethod]
        public void SevenParameterConstructor_IsAbsentFromEfcItemControllerMetadata()
        {
            // Arrange
            ConstructorInfo[] constructors = typeof(EfcItemController).GetConstructors(
                DeclaredInstance
            );

            // Act
            int[] parameterCounts = constructors
                .Select(candidate => candidate.GetParameters().Length)
                .ToArray();

            // Assert
            parameterCounts
                .Should()
                .NotContain(
                    7,
                    "the seven-parameter constructor has zero call sites and #466 C removes it"
                );
            parameterCounts
                .Should()
                .Contain(
                    5,
                    "the five-parameter constructor is called from EfcFormController and is retained"
                );
            parameterCounts
                .Should()
                .Contain(
                    6,
                    "the six-parameter constructor is called from EfcFormController and is retained"
                );
        }

        /// <summary>
        /// #466 B. <c>_selectorsCtrls</c> was initialised to null, never assigned, and passed to
        /// <c>SetupThemes</c> at two call sites. Removing it and passing an explicit null is
        /// behaviour-identical and makes the contract visible instead of concealed.
        /// </summary>
        [TestMethod]
        public void SelectorsCtrlsField_IsAbsentFromEfcItemControllerMetadata()
        {
            // Arrange / Act
            FieldInfo selectorsCtrls = typeof(EfcItemController).GetField(
                "_selectorsCtrls",
                BindingFlags.NonPublic | BindingFlags.Instance
            );

            // Assert
            selectorsCtrls
                .Should()
                .BeNull(
                    "_selectorsCtrls is never assigned and #466 B removes it in favour of an explicit null at both SetupThemes call sites"
                );
        }

        /// <summary>
        /// #459 C. Pins the post-change contract of the surviving asynchronous expansion path:
        /// driving it On, Off, On must neither throw nor touch the <c>CharActions</c> registry.
        /// The deleted synchronous path added <c>'B'</c> and <c>'D'</c> on expand and removed them
        /// on collapse, which is what made a sync-On / async-Off / sync-On sequence leave stale
        /// entries behind.
        /// </summary>
        /// <remarks>
        /// <c>ToggleExpansionAsync(ToggleState)</c> itself is not awaited. It marshals through
        /// <c>ItemViewer.UiDispatcher</c>, a WPF <c>Dispatcher</c> that queues rather than runs on
        /// an unpumped thread, so an await of it can never complete under a test policy that
        /// forbids a message loop. The two dispatched bodies are therefore invoked directly.
        ///
        /// Two mechanisms keep the registry assertion falsifiable rather than tautological:
        /// <c>MockBehavior.Strict</c> throws on any member invoked without a set-up, and
        /// <c>VerifyNoOtherCalls()</c> fails on any unverified invocation. A dispatched body that
        /// touched the keyboard handler at all therefore fails this test. The registry is read
        /// from a local, never through the mock, so no arrange-time invocation is charged against
        /// <c>VerifyNoOtherCalls()</c>.
        /// </remarks>
        [TestMethod]
        public void AsyncExpansionPath_OnOffOn_LeavesCharActionsKeysUnchanged()
        {
            // Arrange
            SynchronizationContext previousContext = SynchronizationContext.Current;
            SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());
            try
            {
                var controller = (EfcItemController)
                    FormatterServices.GetUninitializedObject(typeof(EfcItemController));
                var viewer = new QuickFiler.ItemViewer();

                // The viewer's own window handle is already created by the time its constructor
                // returns, because the WebView2 children force it. A visibility write against a
                // child that is still parented to it therefore triggers CreateControl(), and
                // creating a FastObjectListView handle never completes in a host with no message
                // pump. Substituting a real, parentless FastObjectListView keeps the control type
                // and the observable writes identical while leaving the native handle uncreated.
                viewer.TopicThread = new BrightIdeasSoftware.FastObjectListView();

                Action<char> jumpToWebView = _ => { };
                Action<char> jumpToTopicThread = _ => { };
                var registry = new KbdActions<char, KaChar, Action<char>>();
                registry.Add("Item", 'B', jumpToWebView);
                registry.Add("Item", 'D', jumpToTopicThread);

                var mockKbd = new Mock<IQfcKeyboardHandler>(MockBehavior.Strict);
                mockKbd.SetupGet(handler => handler.CharActions).Returns(registry);

                SetPrivateField(controller, "_itemViewer", viewer);
                SetPrivateField(controller, "_keyboardHandler", mockKbd.Object);

                // Read the pre-state from the local, never through the mock.
                int countBefore = registry.Count();
                char[] keysBefore = registry.Keys.ToArray();

                MethodInfo toggleOn = typeof(EfcItemController).GetMethod(
                    "ToggleExpansionOn",
                    BindingFlags.NonPublic | BindingFlags.Instance
                );
                MethodInfo toggleOff = typeof(EfcItemController).GetMethod(
                    "ToggleExpansionOff",
                    BindingFlags.NonPublic | BindingFlags.Instance
                );
                toggleOn
                    .Should()
                    .NotBeNull("the live async expansion path dispatches to ToggleExpansionOn");
                toggleOff
                    .Should()
                    .NotBeNull("the live async expansion path dispatches to ToggleExpansionOff");

                // Act
                Action driveOnOffOn = () =>
                {
                    toggleOn.Invoke(controller, Array.Empty<object>());
                    toggleOff.Invoke(controller, Array.Empty<object>());
                    toggleOn.Invoke(controller, Array.Empty<object>());
                };

                // Assert
                driveOnOffOn
                    .Should()
                    .NotThrow(
                        "the surviving asynchronous expansion path must run headlessly without a keyboard registry"
                    );
                registry
                    .Count()
                    .Should()
                    .Be(countBefore, "the async expansion path registers and unregisters nothing");
                registry
                    .Keys.Should()
                    .Equal(keysBefore, "no CharActions key is added or removed by the async path");
                registry['B']
                    .Should()
                    .BeSameAs(jumpToWebView, "the seeded 'B' delegate is not replaced");
                registry['D']
                    .Should()
                    .BeSameAs(jumpToTopicThread, "the seeded 'D' delegate is not replaced");
                mockKbd.VerifyNoOtherCalls();
            }
            finally
            {
                SynchronizationContext.SetSynchronizationContext(previousContext);
            }
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            FieldInfo field = target
                .GetType()
                .GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            field.Should().NotBeNull($"{fieldName} must remain available for this headless seam");
            field.SetValue(target, value);
        }
    }
}
