using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QuickFiler.Controllers;

namespace QuickFiler.Controllers.Tests
{
    /// <summary>
    /// Unit tests for <see cref="KaStringAsync"/>, a pure keyboard-action value object whose key is
    /// a lower-cased string and whose delegate is an awaited <c>Func&lt;string, Task&gt;</c>. The
    /// type also exposes <c>Update</c>, <c>ToggleControl</c>, and an <c>Activated</c> flag that
    /// drive the branching in <c>KeyEquals</c>. Tests cover construction/normalization, awaited
    /// (synchronously-completing) delegate dispatch, and each <c>KeyEquals</c> branch. No timing
    /// dependency is introduced.
    /// </summary>
    [TestClass]
    public class KaStringAsyncTests
    {
        private static KaStringAsync NewKa(
            string key,
            Func<string, Task> func = null,
            Action<string> update = null,
            Action toggle = null
        ) => new KaStringAsync("src", key, func ?? (_ => Task.CompletedTask), update, toggle);

        [TestMethod]
        public void Constructor_LowercasesKeyAndStoresMembers()
        {
            // Arrange
            Func<string, Task> func = _ => Task.CompletedTask;

            // Act
            var ka = new KaStringAsync("src", "ABC", func, null, null);

            // Assert
            ka.SourceId.Should().Be("src");
            ka.Key.Should().Be("abc", "the constructor lower-cases the key");
            ka.Delegate.Should().BeSameAs(func);
        }

        [TestMethod]
        public void KeySetter_LowercasesValue()
        {
            // Arrange
            var ka = new KaStringAsync();

            // Act
            ka.Key = "XyZ";

            // Assert
            ka.Key.Should().Be("xyz", "the Key setter normalizes to lower case");
        }

        [TestMethod]
        public async Task Delegate_AwaitsAndCompletesSynchronously()
        {
            // Arrange
            string received = null;
            var ka = NewKa(
                "abc",
                s =>
                {
                    received = s;
                    return Task.CompletedTask;
                }
            );

            // Act
            await ka.Delegate("abc");

            // Assert
            received.Should().Be("abc");
        }

        [TestMethod]
        public void KeyEquals_ContainsMatchWhileActivated_InvokesUpdateAndReturnsTrue()
        {
            // Arrange: Key "abc" contains "ab"; Activated drives the Update side effect.
            string updateArg = null;
            var ka = NewKa("abc", update: s => updateArg = s);
            ka.Activated = true;

            // Act
            var result = ka.KeyEquals("ab");

            // Assert
            result.Should().BeTrue("a substring match returns true");
            updateArg
                .Should()
                .Be("b", "Update receives Key.Substring(other.Length - 1, 1) => index 1 => \"b\"");
            ka.Activated.Should()
                .BeTrue(
                    "the contains-match branch returns before the trailing Activated = false reset"
                );
        }

        [TestMethod]
        public void KeyEquals_ContainsMatchWhileNotActivated_ReturnsTrueWithoutUpdate()
        {
            // Arrange
            bool updateCalled = false;
            var ka = NewKa("abc", update: _ => updateCalled = true);
            ka.Activated = false;

            // Act
            var result = ka.KeyEquals("ab");

            // Assert
            result.Should().BeTrue();
            updateCalled
                .Should()
                .BeFalse("Update is gated behind Activated for the contains branch");
        }

        [TestMethod]
        public void KeyEquals_SingleCharNonMatchWhileActivated_InvokesToggleControlAndReturnsFalse()
        {
            // Arrange: "z" is a single char not contained in "abc"; the else-if length==1 branch
            // invokes ToggleControl when Activated.
            bool toggled = false;
            var ka = NewKa("abc", toggle: () => toggled = true);
            ka.Activated = true;

            // Act
            var result = ka.KeyEquals("z");

            // Assert
            result.Should().BeFalse("a non-matching key returns false");
            toggled.Should().BeTrue("the single-char non-match branch toggles when Activated");
        }

        [TestMethod]
        public void KeyEquals_MultiCharNonMatchWhileActivated_InvokesUpdateWithFirstCharAndReturnsFalse()
        {
            // Arrange: "zz" is multi-char and not contained in "abc"; the else-if length>1 branch
            // invokes Update(Key.Substring(0,1)).
            string updateArg = null;
            bool toggled = false;
            var ka = NewKa("abc", update: s => updateArg = s, toggle: () => toggled = true);
            ka.Activated = true;

            // Act
            var result = ka.KeyEquals("zz");

            // Assert
            result.Should().BeFalse();
            updateArg.Should().Be("a", "Update receives the first char of the key");
            toggled
                .Should()
                .BeTrue("ToggleControl is invoked when Activated in the multi-char branch");
        }

        [TestMethod]
        public void KeyEquals_NullDelegatesAreToleratedInNonMatchBranches()
        {
            // Arrange: Update and ToggleControl are null; the guarded branches must not throw.
            var ka = NewKa("abc", update: null, toggle: null);
            ka.Activated = true;

            // Act
            Action act = () => ka.KeyEquals("zz");

            // Assert
            act.Should().NotThrow("null Update/ToggleControl are guarded against");
        }

        [TestMethod]
        public void KeyEquals_MultiCharNonMatchWhileNotActivated_DoesNotInvokeUpdateAndReturnsFalse()
        {
            // Intent: defect-1 regression. Branch 3 (other.Length > 1, non-matching) must gate its
            // Update side effect on Activated exactly as branches 1 and 2 do. Before the fix the
            // branch-3 guard read "if (Update is not null)" with no Activated conjunct, so Update
            // fired with "a" even though Activated was false.

            // Arrange: Activated is left at its false default. Update is non-null so that an
            // ungated invocation is observable rather than swallowed by the null check.
            var updates = new List<string>();
            var ka = NewKa("abc", update: s => updates.Add(s));

            // Act
            var result = ka.KeyEquals("zz");

            // Assert
            updates.Should().BeEmpty("no KeyEquals side effect may fire while Activated is false");
            result.Should().BeFalse("a multi-character non-match returns false");
        }

        [TestMethod]
        public void KeyEquals_LatchSurvivesMatchThenNonMatchTransition_StillResetsToFirstChar()
        {
            // Intent: pins the branch-1 early return described in the Hard Anti-Regression
            // Constraint. Branch 1 deliberately does NOT clear Activated and returns early, so a row
            // that matches at depth 1 and then fails at depth 2 still receives its Key[0] reset. If
            // branch 1's early return were removed for symmetry, the first probe would consume the
            // latch and the second probe would produce neither the Update nor the ToggleControl.

            // Arrange
            var updates = new List<string>();
            bool toggled = false;
            var ka = NewKa("abc", update: s => updates.Add(s), toggle: () => toggled = true);
            ka.Activated = true;

            // Act: a matching probe, then a non-matching probe on the same instance.
            ka.KeyEquals("ab");
            ka.KeyEquals("zz");

            // Assert: one collection assertion, deliberately not per-element Be(...) calls, so the
            // AC19 retention gate on the pre-existing single-character assertion stays at one hit.
            updates.Should().Equal(new[] { "b", "a" });
            toggled.Should().BeTrue("the surviving latch lets branch 3 toggle on the second probe");
            ka.Activated.Should().BeFalse("the non-matching branch clears the latch");
        }

        [TestMethod]
        public void KeyEquals_EmptyProbe_ThrowsArgumentExceptionNamingOther()
        {
            // Intent: defect-2 regression. AC6 requires the rejection to hold for EVERY combination
            // of instance state, so both variants live in one test method. ThrowExactly is
            // load-bearing here: ArgumentOutOfRangeException derives from ArgumentException, so a
            // plain Throw<ArgumentException> would already pass today for variant 2 and would gate
            // nothing.

            // Arrange, variant 1: default instance, Activated false and Update null. Before the fix
            // this returned true without throwing, because Key.Contains(string.Empty) is true for
            // every key, so an empty probe silently matched every registered action.
            var defaultKa = NewKa("abc");

            // Act
            Action actDefault = () => defaultKa.KeyEquals("");

            // Assert
            actDefault
                .Should()
                .ThrowExactly<ArgumentException>(
                    "an empty probe would otherwise match every registered action"
                )
                .WithParameterName("other");

            // Arrange, variant 2: Activated true with a non-null Update. Before the fix this threw
            // ArgumentOutOfRangeException, because the offset arithmetic evaluated Substring(-1, 1).
            var activatedKa = NewKa("abc", update: _ => { });
            activatedKa.Activated = true;

            // Act
            Action actActivated = () => activatedKa.KeyEquals("");

            // Assert
            actActivated
                .Should()
                .ThrowExactly<ArgumentException>(
                    "the guard clause runs before any offset arithmetic"
                )
                .WithParameterName("other");
        }

        [TestMethod]
        public void KeyEquals_NullProbe_ThrowsArgumentNullExceptionNamingOther()
        {
            // Intent: defect-2 regression for the null case. The parameter-name clause is what
            // distinguishes the explicit guard from today's behaviour: before the fix the throw
            // originates inside string.Contains, whose parameter is named "value", not "other". The
            // guard changes the exception's origin rather than its type, so a type-only assertion
            // would pass unchanged and would gate nothing.

            // Arrange
            var ka = NewKa("abc");

            // Act
            Action act = () => ka.KeyEquals(null);

            // Assert
            act.Should()
                .ThrowExactly<ArgumentNullException>("a null probe is a caller error")
                .WithParameterName("other");
        }
    }
}
