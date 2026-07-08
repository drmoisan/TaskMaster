using System;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS.Threading;

namespace UtilitiesCS.Test.Threading
{
    /// <summary>
    /// Deterministic unit tests for <see cref="CurrentStoreContext"/> (issue #264). The type is a
    /// plain static, volatile-backed ambient holder, so these tests require no COM, no threads, and
    /// no waits. Each test fully opens and disposes its scopes, returning
    /// <see cref="CurrentStoreContext.Current"/> to <see langword="null"/> so the process-global
    /// state does not leak between tests.
    /// </summary>
    [TestClass]
    [DoNotParallelize]
    public class CurrentStoreContextTests
    {
        [TestMethod]
        public void Begin_SetsCurrent_ReadableInsideScope()
        {
            // Arrange
            CurrentStoreContext.Current.Should().BeNull("no scope is open before the test");

            // Act & Assert
            using (CurrentStoreContext.Begin("Mailbox A"))
            {
                CurrentStoreContext.Current.Should().Be("Mailbox A");
            }
        }

        [TestMethod]
        public void Dispose_RestoresPreviousValue()
        {
            // Arrange & Act
            using (CurrentStoreContext.Begin("Mailbox A"))
            {
                CurrentStoreContext.Current.Should().Be("Mailbox A");
            }

            // Assert: after the scope disposes, the previous (null) value is restored.
            CurrentStoreContext.Current.Should().BeNull();
        }

        [TestMethod]
        public void NestedScopes_RestoreInnerThenOuter()
        {
            // Arrange & Act & Assert
            using (CurrentStoreContext.Begin("Outer"))
            {
                CurrentStoreContext.Current.Should().Be("Outer");

                using (CurrentStoreContext.Begin("Inner"))
                {
                    CurrentStoreContext.Current.Should().Be("Inner");
                }

                // Inner disposed: the outer value is restored, not null.
                CurrentStoreContext.Current.Should().Be("Outer");
            }

            CurrentStoreContext.Current.Should().BeNull();
        }

        [TestMethod]
        public void SequentialScopes_EachRestoreToNull()
        {
            // Arrange & Act & Assert
            using (CurrentStoreContext.Begin("First"))
            {
                CurrentStoreContext.Current.Should().Be("First");
            }

            CurrentStoreContext.Current.Should().BeNull();

            using (CurrentStoreContext.Begin("Second"))
            {
                CurrentStoreContext.Current.Should().Be("Second");
            }

            CurrentStoreContext.Current.Should().BeNull();
        }

        [DataTestMethod]
        [DataRow(null)]
        [DataRow("")]
        [DataRow("   ")]
        [DataRow("<unavailable>")]
        public void Begin_NormalizesUnavailableIdentity_ToNoContext(string identity)
        {
            // Act & Assert: an unresolved/unavailable identity normalizes to "no context".
            using (CurrentStoreContext.Begin(identity))
            {
                CurrentStoreContext.Current.Should().BeNull();
            }

            CurrentStoreContext.Current.Should().BeNull();
        }

        [TestMethod]
        public void Begin_NormalizedInnerScope_RestoresRealOuterValue()
        {
            // Arrange & Act: a normalized (null) inner scope must still restore the real outer value.
            using (CurrentStoreContext.Begin("Outer"))
            {
                using (CurrentStoreContext.Begin("<unavailable>"))
                {
                    CurrentStoreContext
                        .Current.Should()
                        .BeNull("the inner identity normalizes to no context");
                }

                CurrentStoreContext.Current.Should().Be("Outer");
            }

            CurrentStoreContext.Current.Should().BeNull();
        }
    }
}
