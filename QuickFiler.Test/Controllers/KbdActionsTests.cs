using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QuickFiler.Controllers;

namespace QuickFiler.Controllers.Tests
{
    [TestClass]
    public class KbdActionsTests
    {
        [TestMethod]
        public void Add_WhenSourceAndStoredKeysAreDistinct_DoesNotTreatSubstringAsDuplicate()
        {
            // Arrange
            var actions = new KbdActions<string, KaStringAsync, Func<string, Task>>();
            actions.Add("Collection", "10", _ => Task.CompletedTask);

            // Act
            Action act = () => actions.Add("Collection", "1", _ => Task.CompletedTask);

            // Assert
            act.Should()
                .NotThrow(
                    because: "storage identity must distinguish distinct registered keys even when runtime keyboard matching uses substring semantics"
                );
            actions.Keys.Should().Equal("10", "1");
        }

        [TestMethod]
        public void Add_WhenSourceAndStoredKeyAreExactDuplicate_ThrowsArgumentException()
        {
            // Arrange
            var actions = new KbdActions<string, KaStringAsync, Func<string, Task>>();
            actions.Add("Collection", "1", _ => Task.CompletedTask);

            // Act
            Action act = () => actions.Add("Collection", "1", _ => Task.CompletedTask);

            // Assert
            act.Should()
                .Throw<ArgumentException>(
                    because: "exact duplicate storage registrations must still be rejected"
                )
                .WithMessage("*already exists*");
        }

        [TestMethod]
        public void FilterKeys_WhenDistinctStoredKeysCoexist_PreservesKeyboardMatchingSemantics()
        {
            // Arrange
            var actions = new KbdActions<string, KaStringAsync, Func<string, Task>>();
            actions.Add("Collection", "10", _ => Task.CompletedTask);
            actions.Add("Collection", "1", _ => Task.CompletedTask);

            // Act
            var singleDigitMatches = actions.FilterKeys("1").Select(action => action.Key).ToArray();
            var exactTwoDigitMatches = actions
                .FilterKeys("10")
                .Select(action => action.Key)
                .ToArray();

            // Assert
            actions
                .ContainsKey("1")
                .Should()
                .BeTrue(
                    because: "the first digit should still participate in live keyboard filtering"
                );
            singleDigitMatches
                .Should()
                .BeEquivalentTo(
                    new[] { "1", "10" },
                    because: "KaStringAsync.KeyEquals substring matching must remain available for keyboard filtering"
                );
            exactTwoDigitMatches
                .Should()
                .ContainSingle()
                .Which.Should()
                .Be(
                    "10",
                    because: "the full key should still narrow the lookup to the exact registered action"
                );
            actions["10"].Should().NotBeNull();
        }
    }
}
