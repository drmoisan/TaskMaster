using System;
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
        public void KeyEquals_MultiCharNonMatch_InvokesUpdateWithFirstCharAndReturnsFalse()
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
    }
}
