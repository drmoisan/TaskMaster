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
using OutlookMailItem = Microsoft.Office.Interop.Outlook.MailItem;

namespace QuickFiler.Controllers.Tests
{
    /// <summary>
    /// Issue #644 regression coverage for the navigation-key ledger on
    /// <see cref="QfcCollectionController"/>.
    ///
    /// Before the ledger, <c>UnregisterNavigation</c> bounded its removal loop with the
    /// <em>current</em> <c>_itemGroups.Count</c>, while several production paths mutate
    /// <c>_itemGroups</c> with no unregister/register bracket around the mutation. When a group was
    /// removed through one of those paths, the loop iterated fewer times than the registration had,
    /// leaving orphaned <c>KbdActions</c> registrations behind. Because every production call site
    /// discards <c>KbdActions.Remove</c>'s <c>bool</c> result, nothing reported the divergence at the
    /// point it happened; it surfaced later as an <see cref="ArgumentException"/> from a duplicate
    /// <c>Add</c>, or an <see cref="InvalidOperationException"/> from a <c>Find</c> resolving against
    /// a multi-element match set.
    ///
    /// The ledger records the exact <c>(SourceId, Key)</c> pairs registration added and replays that
    /// recorded set on unregistration, so unregistration is total for every interleaving of
    /// <c>_itemGroups</c> mutations. The invariant these tests pin is:
    ///
    /// <para>
    /// After any <c>RegisterNavigation()</c> / <c>UnregisterNavigation()</c> pair, the
    /// <c>"Collection"</c>-sourced key set in <c>IQfcKeyboardHandler.StringActionsAsync</c> is
    /// exactly what it was before the <c>RegisterNavigation()</c> call.
    /// </para>
    ///
    /// <see cref="QfcCollectionController"/> requires WinForms components in its constructor, so
    /// instances are built with <see cref="FormatterServices.GetUninitializedObject(Type)"/> and the
    /// private fields each test needs are injected by reflection. This file is deliberately
    /// self-contained: it carries its own field setter, item-group builder, and controller factory
    /// rather than depending on any other test file, so it introduces no cross-feature coupling.
    ///
    /// Every test here is host-free. There is no live Outlook process, no COM object, no WinForms
    /// handle, no STA apartment, no temporary file, no wall-clock wait, and no mutable static state.
    /// </summary>
    [TestClass]
    public class QfcCollectionControllerNavigationLedgerTests
    {
        /// <summary>The <c>SourceId</c> under which navigation keys are registered.</summary>
        private const string CollectionSourceId = "Collection";

        /// <summary>Sets a private instance field of <see cref="QfcCollectionController"/>.</summary>
        private static void SetControllerField(object target, string name, object value)
        {
            FieldInfo field = typeof(QfcCollectionController).GetField(
                name,
                BindingFlags.NonPublic | BindingFlags.Instance
            );
            field.Should().NotBeNull($"QfcCollectionController must declare the field {name}");
            field.SetValue(target, value);
        }

        /// <summary>
        /// Builds one minimal item group carrying a mocked mail item with the distinct entry
        /// identifier <c>entry-{index}</c> and a mocked item controller whose
        /// <c>TopFolderScore</c> is <paramref name="topFolderScore"/>. <c>ItemViewer</c> is left
        /// null; no test in this file lets execution reach a path that dereferences it.
        /// </summary>
        private static QfcItemGroup MakeGroup(int index, long topFolderScore)
        {
            var mail = new Mock<OutlookMailItem>(MockBehavior.Loose);
            mail.SetupGet(x => x.EntryID).Returns($"entry-{index}");

            var itemController = new Mock<IQfcItemController>(MockBehavior.Loose);
            itemController.SetupGet(x => x.TopFolderScore).Returns(topFolderScore);

            return new QfcItemGroup
            {
                MailItem = mail.Object,
                ItemController = itemController.Object,
            };
        }

        /// <summary>
        /// Builds <paramref name="count"/> item groups, every one of them scoring
        /// <paramref name="topFolderScore"/>.
        /// </summary>
        private static List<QfcItemGroup> MakeGroups(int count, long topFolderScore = 1000L)
        {
            var groups = new List<QfcItemGroup>();
            for (int i = 0; i < count; i++)
            {
                groups.Add(MakeGroup(i, topFolderScore));
            }
            return groups;
        }

        /// <summary>
        /// Builds an uninitialized controller wired for the navigation register/unregister pair: a
        /// real parameterless <c>KbdActions</c> behind a Loose <see cref="IQfcKeyboardHandler"/>
        /// whose <c>StringActionsAsync</c> getter returns it, the injected item-group page, and
        /// <c>_digits</c> pre-set to <paramref name="digits"/>.
        ///
        /// <c>_digits</c> must equal the width the page already needs. The <c>Digits</c> getter
        /// computes <c>_itemGroups.Count &gt;= 10 ? 2 : 1</c> and sets <c>_digitRefreshNeeded</c>
        /// only when that differs from <c>_digits</c>; keeping them equal means
        /// <c>RegisterNavigation</c> never routes into <c>SetVisualDigits</c>, which requires
        /// WinForms.
        /// </summary>
        private static QfcCollectionController CreateLedgerController(
            List<QfcItemGroup> groups,
            int digits,
            out KbdActions<string, KaStringAsync, Func<string, Task>> registry
        )
        {
            var controller = (QfcCollectionController)
                FormatterServices.GetUninitializedObject(typeof(QfcCollectionController));

            var kbdActions = new KbdActions<string, KaStringAsync, Func<string, Task>>();
            registry = kbdActions;
            var kbdHandler = new Mock<IQfcKeyboardHandler>(MockBehavior.Loose);
            kbdHandler.SetupGet(x => x.StringActionsAsync).Returns(() => kbdActions);

            SetControllerField(controller, "_kbdHandler", kbdHandler.Object);
            SetControllerField(controller, "_digits", digits);
            SetControllerField(controller, "_itemGroups", groups);

            return controller;
        }

        /// <summary>Returns every <c>"Collection"</c>-sourced key currently registered.</summary>
        private static string[] CollectionKeys(
            KbdActions<string, KaStringAsync, Func<string, Task>> registry
        ) => registry.Where(a => a.SourceId == CollectionSourceId).Select(a => a.Key).ToArray();

        /// <summary>
        /// T1 / AC-2. Models the <c>RemoveBelowThresholdAsync</c> reach: a ten-group page registers
        /// keys "01".."10" at width 2, then one group is removed through the
        /// <c>_removeGroupByEntryId</c> seam with no intervening unregister, and the page is
        /// unregistered.
        ///
        /// Before the ledger the removal loop was bounded by the now-nine <c>_itemGroups.Count</c>,
        /// so "10" was never visited and survived as an orphan. After the ledger the recorded set
        /// is replayed verbatim and every registered key is removed.
        /// </summary>
        [TestMethod]
        public async Task UnregisterNavigation_AfterGroupRemovedThroughRemoveGroupByEntryIdSeam_RemovesEveryRegisteredKey()
        {
            // Arrange: ten groups, only entry-0 scoring below the 0.9 threshold.
            var groups = new List<QfcItemGroup>();
            for (int i = 0; i < 10; i++)
            {
                groups.Add(MakeGroup(i, i == 0 ? 100L : 1000L));
            }

            var controller = CreateLedgerController(groups, digits: 2, out var registry);

            // The seam performs only the list mutation, which is the part of
            // RemoveSpecificControlGroup(int) that is reachable without WinForms and COM state.
            Func<string, Task> removeGroupByEntryId = entryId =>
            {
                groups.RemoveAll(group => group.MailItem.EntryID == entryId);
                return Task.CompletedTask;
            };
            SetControllerField(controller, "_removeGroupByEntryId", removeGroupByEntryId);

            controller.RegisterNavigation();
            CollectionKeys(registry)
                .Should()
                .HaveCount(10, "a ten-group page registers one key per group at width 2");

            // Act
            await controller.RemoveBelowThresholdAsync(0.9);
            groups.Should().HaveCount(9, "entry-0 scores below the threshold and is removed");
            controller.UnregisterNavigation();

            // Assert
            CollectionKeys(registry)
                .Should()
                .BeEmpty(
                    "issue #644 requires unregistration to replay the recorded registration set, "
                        + "so an unbracketed removal through the RemoveGroupByEntryId seam cannot "
                        + "orphan the tail key"
                );
        }

        /// <summary>
        /// T2 / AC-3. Models the unbracketed <c>RemoveSpecificControlGroup(int)</c> reach shared by
        /// the synchronous 'R' char action and <c>PopOutControlGroup(int)</c>: a five-group page
        /// registers keys "1".."5" at width 1, one group is removed directly from the injected list
        /// with no intervening unregister, the page is unregistered, and the page is then restored
        /// to five groups before a second registration — which is what a subsequent page rebuild
        /// does in production.
        ///
        /// Before the ledger the removal loop was bounded by the now-four count, so "5" survived as
        /// an orphan and the second registration collided with it, throwing
        /// <see cref="ArgumentException"/>. Restoring the page to five groups is what makes that
        /// collision reachable: a bare shrink-then-re-register would re-add only "1".."4", which do
        /// not collide with the orphaned "5".
        /// </summary>
        [TestMethod]
        public void UnregisterNavigation_AfterUnbracketedItemGroupsRemoval_ThenReRegister_DoesNotThrow()
        {
            // Arrange
            var groups = MakeGroups(5);
            var controller = CreateLedgerController(groups, digits: 1, out var registry);
            controller.RegisterNavigation();

            // Act: unbracketed removal, then unregister, then restore the page to five groups.
            groups.RemoveAt(0);
            controller.UnregisterNavigation();
            groups.Add(MakeGroup(5, 1000L));
            System.Action secondRegister = () => controller.RegisterNavigation();

            // Assert
            secondRegister
                .Should()
                .NotThrow(
                    "issue #644 requires the first unregistration to have been total, so no "
                        + "orphaned key remains for the second registration to collide with"
                );

            var remaining = CollectionKeys(registry);
            remaining
                .Should()
                .BeEquivalentTo(
                    new[] { "1", "2", "3", "4", "5" },
                    "the re-registered five-group page owns exactly its own keys"
                );
            remaining
                .Should()
                .OnlyHaveUniqueItems(
                    "a navigation keypress must resolve against exactly one handler"
                );
        }

        /// <summary>
        /// T3 / AC-5. State-transition coverage: register, unregister, register, unregister with no
        /// mutation in between. Each unregistration must drain the ledger completely so the next
        /// registration starts from an empty registry.
        /// </summary>
        [TestMethod]
        public void RegisterAndUnregisterNavigation_RepeatedCycles_LeaveRegistryEmpty()
        {
            // Arrange
            var controller = CreateLedgerController(MakeGroups(3), digits: 1, out var registry);

            // Act
            System.Action cycles = () =>
            {
                controller.RegisterNavigation();
                controller.UnregisterNavigation();
                controller.RegisterNavigation();
                controller.UnregisterNavigation();
            };

            // Assert
            cycles
                .Should()
                .NotThrow(
                    "each unregistration clears the ledger, so the following registration re-adds "
                        + "keys that are no longer present"
                );
            CollectionKeys(registry)
                .Should()
                .BeEmpty("the populated state returns to empty on every unregistration");
        }

        /// <summary>
        /// T4 / AC-6. Empty-ledger negative case: unregistration on a controller that never
        /// registered must throw nothing and must not disturb registry entries owned by another
        /// source. The lazy ledger accessor is what keeps this safe on a reflection-built instance,
        /// where <see cref="FormatterServices.GetUninitializedObject(Type)"/> has bypassed the field
        /// initialisers and left the ledger field null.
        /// </summary>
        [TestMethod]
        public void UnregisterNavigation_WithNoPriorRegistration_DoesNotThrowAndLeavesRegistryUnchanged()
        {
            // Arrange: one unrelated entry, owned by a different source.
            var controller = CreateLedgerController(MakeGroups(3), digits: 1, out var registry);
            registry.Add("Other", "1", _ => Task.CompletedTask);

            // Act
            System.Action act = () => controller.UnregisterNavigation();

            // Assert
            act.Should()
                .NotThrow("an empty ledger yields a no-op unregistration rather than a throw");
            registry
                .Count()
                .Should()
                .Be(1, "unregistration must not touch an entry this controller never registered");
            registry.Single().SourceId.Should().Be("Other");
            registry.Single().Key.Should().Be("1");
            CollectionKeys(registry)
                .Should()
                .BeEmpty("the controller registered no Collection-sourced key");
        }

        /// <summary>
        /// T5 / AC-7. Structural proof that <c>UnregisterNavigation</c> no longer reads
        /// <c>_itemGroups</c> at all. Models the post-<c>Cleanup</c> state, where <c>_itemGroups</c>
        /// has been set to null: before the ledger the loop bound dereferenced that null field and
        /// raised <see cref="NullReferenceException"/>; after it, the ledger is replayed and drained
        /// without consulting <c>_itemGroups</c>.
        ///
        /// This is the regression guard named in the spec's Risks section: a future change that
        /// reintroduces an <c>_itemGroups</c>-derived bound fails this test.
        /// </summary>
        [TestMethod]
        public void UnregisterNavigation_AfterItemGroupsSetToNull_DoesNotThrow()
        {
            // Arrange
            var controller = CreateLedgerController(MakeGroups(5), digits: 1, out var registry);
            controller.RegisterNavigation();

            // Act
            SetControllerField(controller, "_itemGroups", null);
            System.Action act = () => controller.UnregisterNavigation();

            // Assert
            act.Should()
                .NotThrow(
                    "issue #644 removes _itemGroups from the unregistration path entirely, so a "
                        + "null field is no longer dereferenced"
                );
            CollectionKeys(registry)
                .Should()
                .BeEmpty("the recorded set is replayed from the ledger, not from _itemGroups");
        }

        /// <summary>
        /// T6 / AC-4. The #644-side companion to the #472 width test. A ten-group page registers
        /// keys "01".."10" at width 2 and the page is then shrunk to nine with no intervening
        /// unregister, so the orphan is exactly the tail key "10" — which is the residual the #472
        /// width-fidelity test pinned explicitly and attributed to this follow-up issue.
        /// </summary>
        [TestMethod]
        public void UnregisterNavigation_AfterTwoDigitRegistrationAndShrinkToNine_LeavesNoCollectionKeys()
        {
            // Arrange
            var groups = MakeGroups(10);
            var controller = CreateLedgerController(groups, digits: 2, out var registry);
            controller.RegisterNavigation();

            // Act
            groups.RemoveAt(0);
            controller.UnregisterNavigation();

            // Assert
            CollectionKeys(registry)
                .Should()
                .BeEmpty(
                    "the ledger replays all ten recorded keys, so the width-crossing tail key '10' "
                        + "is no longer left behind"
                );
        }
    }
}
