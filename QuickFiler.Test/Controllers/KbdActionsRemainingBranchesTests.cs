using System;
using System.Linq;
using System.Windows.Forms;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QuickFiler.Controllers;

namespace QuickFiler.Controllers.Tests
{
    /// <summary>
    /// Covers the <see cref="KbdActions{TKey, UClass, VDelegate}"/> registry branches not already
    /// exercised by KbdActionsTests: Find / FindIndex (no-match, single-match, and ambiguous
    /// multiple-match throwing paths), the Add(UClass) overload and its duplicate guard, Remove
    /// (present and absent), the indexer getter/setter, enumeration, the Keys projection, and the
    /// empty-registry state. KaKey (Action&lt;Keys&gt;) is used as the concrete element type; the
    /// registry is pure collection management with no Outlook dependency.
    /// </summary>
    [TestClass]
    public class KbdActionsRemainingBranchesTests
    {
        private static KbdActions<Keys, KaKey, Action<Keys>> NewRegistry() =>
            new KbdActions<Keys, KaKey, Action<Keys>>();

        [TestMethod]
        public void EmptyRegistry_HasNoKeysAndFindReturnsDefault()
        {
            // Arrange
            var registry = NewRegistry();

            // Act / Assert
            registry.Keys.Should().BeEmpty("a new registry holds no actions");
            registry.Find(Keys.A).Should().BeNull("Find returns default(UClass) when no match");
            registry.FindIndex(Keys.A).Should().Be(-1, "FindIndex returns -1 when no match");
            registry.ContainsKey(Keys.A).Should().BeFalse();
        }

        [TestMethod]
        public void AddInstance_ThenFind_ReturnsTheRegisteredInstance()
        {
            // Arrange
            var registry = NewRegistry();
            var instance = new KaKey("src", Keys.Enter, _ => { });

            // Act
            registry.Add(instance);
            var found = registry.Find(Keys.Enter);

            // Assert
            found.Should().BeSameAs(instance, "the single registered instance is returned");
            registry.FindIndex(Keys.Enter).Should().Be(0);
        }

        [TestMethod]
        public void AddInstance_ExactDuplicate_ThrowsArgumentException()
        {
            // Arrange
            var registry = NewRegistry();
            registry.Add(new KaKey("src", Keys.Enter, _ => { }));

            // Act
            Action act = () => registry.Add(new KaKey("src", Keys.Enter, _ => { }));

            // Assert
            act.Should()
                .Throw<ArgumentException>("a duplicate source+key registration is rejected")
                .WithMessage("*already exists*");
        }

        [TestMethod]
        public void Find_WhenMultipleSourcesShareKey_ThrowsInvalidOperationException()
        {
            // Arrange: two distinct sources register the same key, creating an ambiguous lookup.
            var registry = NewRegistry();
            registry.Add("sourceA", Keys.Enter, _ => { });
            registry.Add("sourceB", Keys.Enter, _ => { });

            // Act
            Action act = () => registry.Find(Keys.Enter);

            // Assert
            act.Should()
                .Throw<InvalidOperationException>(
                    "an ambiguous key shared by multiple sources cannot resolve to one action"
                );
        }

        [TestMethod]
        public void FindIndex_WhenMultipleSourcesShareKey_ThrowsInvalidOperationException()
        {
            // Arrange
            var registry = NewRegistry();
            registry.Add("sourceA", Keys.Enter, _ => { });
            registry.Add("sourceB", Keys.Enter, _ => { });

            // Act
            Action act = () => registry.FindIndex(Keys.Enter);

            // Assert
            act.Should().Throw<InvalidOperationException>();
        }

        [TestMethod]
        public void Remove_PresentKey_RemovesAndReturnsTrue()
        {
            // Arrange
            var registry = NewRegistry();
            registry.Add("src", Keys.Enter, _ => { });

            // Act
            var removed = registry.Remove("src", Keys.Enter);

            // Assert
            removed.Should().BeTrue("an existing source+key is removed");
            registry.ContainsKey(Keys.Enter).Should().BeFalse();
        }

        [TestMethod]
        public void Remove_AbsentKey_ReturnsFalse()
        {
            // Arrange
            var registry = NewRegistry();

            // Act
            var removed = registry.Remove("src", Keys.Enter);

            // Assert
            removed
                .Should()
                .BeFalse("removing a non-existent source+key is a no-op returning false");
        }

        [TestMethod]
        public void Indexer_Get_ReturnsRegisteredDelegate_Set_ReplacesIt()
        {
            // Arrange
            Action<Keys> first = _ => { };
            Action<Keys> second = _ => { };
            var registry = NewRegistry();
            registry.Add(new KaKey("src", Keys.Enter, first));

            // Act
            var fetched = registry[Keys.Enter];
            registry[Keys.Enter] = second;

            // Assert
            fetched.Should().BeSameAs(first, "the indexer getter returns the stored delegate");
            registry[Keys.Enter].Should().BeSameAs(second, "the indexer setter replaces it");
        }

        [TestMethod]
        public void Enumeration_YieldsAllRegisteredInstancesAndKeysProjection()
        {
            // Arrange
            var registry = NewRegistry();
            registry.Add("src", Keys.Enter, _ => { });
            registry.Add("src", Keys.Escape, _ => { });

            // Act
            var enumerated = registry.ToArray();

            // Assert
            enumerated.Should().HaveCount(2, "the enumerator yields every registered instance");
            registry.Keys.Should().BeEquivalentTo(new[] { Keys.Enter, Keys.Escape });
        }

        [TestMethod]
        public void FilterKeys_ReturnsOnlyMatchingInstances()
        {
            // Arrange
            var registry = NewRegistry();
            registry.Add("src", Keys.Enter, _ => { });
            registry.Add("src", Keys.Escape, _ => { });

            // Act
            var matches = registry.FilterKeys(Keys.Enter);

            // Assert
            matches.Should().ContainSingle().Which.Key.Should().Be(Keys.Enter);
        }
    }
}
