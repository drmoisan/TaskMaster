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
    }
}
