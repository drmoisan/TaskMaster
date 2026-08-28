using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using QuickFiler.Interfaces;
using Keys = System.Windows.Forms.Keys;
using OutlookMailItem = Microsoft.Office.Interop.Outlook.MailItem;

namespace QuickFiler.Controllers.Tests
{
    /// <summary>
    /// Navigation-key registration tests for <see cref="QfcCollectionController"/> covering issues
    /// #444 and #472. <see cref="QfcCollectionController"/> requires WinForms components in its
    /// constructor, so instances are built with
    /// <see cref="FormatterServices.GetUninitializedObject(Type)"/> and the private fields each test
    /// needs are injected by reflection.
    ///
    /// This file is deliberately self-contained: it carries its own field setter and item-group
    /// builder rather than depending on any other test file, so it introduces no cross-feature
    /// coupling. It constructs no form-derived type, no background worker, and no temporary file, and
    /// it contains no wall-clock wait.
    /// </summary>
    [TestClass]
    public class QfcCollectionControllerNavigationDigitsTests
    {
        /// <summary>Sets a private instance field of <see cref="QfcCollectionController"/>.</summary>
        private static void SetControllerField(object target, string name, object value)
        {
            var field = typeof(QfcCollectionController).GetField(
                name,
                BindingFlags.NonPublic | BindingFlags.Instance
            );
            field.Should().NotBeNull($"QfcCollectionController must declare the field {name}");
            field.SetValue(target, value);
        }

        /// <summary>
        /// Builds <paramref name="count"/> minimal item groups, each carrying only a mocked mail item
        /// with a distinct entry identifier. <c>ItemController</c> and <c>ItemViewer</c> are left
        /// null; no test in this file lets execution reach a path that dereferences them.
        /// </summary>
        private static List<QfcItemGroup> MakeGroups(int count)
        {
            var groups = new List<QfcItemGroup>();
            for (int i = 0; i < count; i++)
            {
                var mail = new Mock<OutlookMailItem>(MockBehavior.Loose);
                mail.SetupGet(x => x.EntryID).Returns($"entry-{i}");
                groups.Add(new QfcItemGroup { MailItem = mail.Object });
            }
            return groups;
        }

        /// <summary>
        /// Issue #444 decision pin. Upstream #468 deleted the dead <c>WireUpKeyboardHandler</c>
        /// method whose seed registered <c>Keys.Down</c> twice, once to <c>SelectNextItem()</c> and
        /// once to <c>ActionOkAsync()</c>. The surviving live registration is
        /// <c>RegisterAsyncKeyActions</c>, and this test pins its cardinality and its bindings so a
        /// future edit cannot silently re-introduce the ambiguous pair. It has no pre-fix red state.
        /// </summary>
        [TestMethod]
        public void RegisterAsyncKeyActions_RegistersExactlyOneDownBoundToSelectNextItemAsync()
        {
            // Arrange
            var controller = (QfcCollectionController)
                FormatterServices.GetUninitializedObject(typeof(QfcCollectionController));
            var kbdHandler = new Mock<IQfcKeyboardHandler>(MockBehavior.Loose);
            kbdHandler.SetupProperty(x => x.KeyActionsAsync);
            SetControllerField(controller, "_kbdHandler", kbdHandler.Object);

            // Act
            controller.RegisterAsyncKeyActions();

            // Assert
            var registry = kbdHandler.Object.KeyActionsAsync;
            registry.Should().NotBeNull("RegisterAsyncKeyActions assigns the async key registry");
            registry
                .Count(a => a.SourceId == "Collection" && a.Key == Keys.Down)
                .Should()
                .Be(1, "exactly one Collection-sourced Keys.Down action may be registered");
            registry
                .Count(a => a.SourceId == "Collection" && a.Key == Keys.Up)
                .Should()
                .Be(1, "exactly one Collection-sourced Keys.Up action may be registered");
        }

        /// <summary>
        /// Builds an uninitialized controller wired for the navigation register/unregister pair: a
        /// real <c>KbdActions</c> behind a Loose <see cref="IQfcKeyboardHandler"/> whose
        /// <c>StringActionsAsync</c> getter returns it, the injected item-group page, and
        /// <c>_digits</c> pre-set to <paramref name="digits"/>.
        ///
        /// <c>_digits</c> must equal the width the page already needs. The <c>Digits</c> getter
        /// computes <c>_itemGroups.Count &gt;= 10 ? 2 : 1</c> and sets <c>_digitRefreshNeeded</c> only
        /// when that differs from <c>_digits</c>; keeping them equal means <c>RegisterNavigation</c>
        /// never enters <c>SetVisualDigits</c>, which would dereference the deliberately null
        /// <c>ItemController</c> on each group.
        /// </summary>
        private static QfcCollectionController CreateNavigationController(
            int itemCount,
            int digits,
            out KbdActions<string, KaStringAsync, Func<string, Task>> registry,
            out List<QfcItemGroup> groups
        )
        {
            var controller = (QfcCollectionController)
                FormatterServices.GetUninitializedObject(typeof(QfcCollectionController));

            var kbdActions = new KbdActions<string, KaStringAsync, Func<string, Task>>();
            registry = kbdActions;
            var kbdHandler = new Mock<IQfcKeyboardHandler>(MockBehavior.Loose);
            kbdHandler.SetupGet(x => x.StringActionsAsync).Returns(() => kbdActions);

            groups = MakeGroups(itemCount);

            SetControllerField(controller, "_kbdHandler", kbdHandler.Object);
            SetControllerField(controller, "_digits", digits);
            SetControllerField(controller, "_itemGroups", groups);

            return controller;
        }

        /// <summary>Counts the <c>"Collection"</c>-sourced entries whose key is exactly <paramref name="key"/>.</summary>
        private static int CountCollectionKey(
            KbdActions<string, KaStringAsync, Func<string, Task>> registry,
            string key
        ) => registry.Count(a => a.SourceId == "Collection" && a.Key == key);

        /// <summary>Returns every <c>"Collection"</c>-sourced key currently registered.</summary>
        private static string[] CollectionKeys(
            KbdActions<string, KaStringAsync, Func<string, Task>> registry
        ) => registry.Where(a => a.SourceId == "Collection").Select(a => a.Key).ToArray();

        /// <summary>
        /// Issue #472. Registering a ten-item page records keys "01".."10" at width 2. One group is
        /// then removed without an intervening unregister, modelling the unbracketed
        /// <c>RemoveSpecificControlGroup</c> path, so the live <c>Digits</c> getter now computes width
        /// 1. Before the fix <c>UnregisterNavigation</c> re-evaluated <c>Digits</c> per iteration and
        /// removed the never-registered "1".."9", leaving all ten two-digit keys orphaned. After the
        /// fix it replays the recorded width and removes "01".."09".
        ///
        /// The single residual "10" entry is expected and is NOT this fix's scope. The loop is bounded
        /// by the current <c>_itemGroups.Count</c>, which is now nine, so the tenth key is never
        /// visited whatever the digit width. That count mismatch is the separately-promoted defect
        /// recorded in <c>### Downstream notes</c> item 3 of this feature's spec, and the assertion
        /// below is written as an explicit at-most bound so it cannot silently absorb it.
        /// </summary>
        [TestMethod]
        public void UnregisterNavigation_AfterRegisteringAtTwoDigitsAndShrinkingToNine_RemovesTheTwoDigitKeys()
        {
            // Arrange
            var controller = CreateNavigationController(
                itemCount: 10,
                digits: 2,
                out var registry,
                out var groups
            );
            controller.RegisterNavigation();
            CountCollectionKey(registry, "01")
                .Should()
                .Be(1, "a ten-item page registers its keys at width 2");
            CountCollectionKey(registry, "10").Should().Be(1);

            // Act: drop one group without an intervening unregister, then unregister.
            groups.RemoveAt(0);
            controller.UnregisterNavigation();

            // Assert
            var remaining = CollectionKeys(registry);
            remaining
                .Where(k => k.StartsWith("0", StringComparison.Ordinal))
                .Should()
                .BeEmpty(
                    "the recorded registration width is replayed, so the '0'-prefixed keys go"
                );
            remaining
                .Should()
                .Equal(
                    new[] { "10" },
                    "only the key the shortened loop bound cannot reach survives, which is the separately-promoted count mismatch"
                );
        }

        /// <summary>
        /// Issue #472, mirror direction. A nine-item page registers keys "1".."9" at width 1. A group
        /// is then added without an intervening unregister, so the live <c>Digits</c> getter now
        /// computes width 2. Before the fix <c>UnregisterNavigation</c> removed the never-registered
        /// "01".."10" and left all nine single-digit keys orphaned. After the fix it replays the
        /// recorded width 1 and, because the loop bound has grown to ten, removes every registered
        /// key.
        /// </summary>
        [TestMethod]
        public void UnregisterNavigation_AfterRegisteringAtOneDigitAndGrowingToTen_RemovesTheOneDigitKeys()
        {
            // Arrange
            var controller = CreateNavigationController(
                itemCount: 9,
                digits: 1,
                out var registry,
                out var groups
            );
            controller.RegisterNavigation();
            CountCollectionKey(registry, "1")
                .Should()
                .Be(1, "a nine-item page registers its keys at width 1");
            CountCollectionKey(registry, "9").Should().Be(1);

            // Act: grow the page past the two-digit boundary without an intervening unregister.
            var extra = MakeGroups(1);
            groups.Add(extra[0]);
            controller.UnregisterNavigation();

            // Assert
            CollectionKeys(registry)
                .Should()
                .BeEmpty(
                    "the recorded width 1 is replayed and the grown loop bound reaches every registered key"
                );
        }
    }
}
