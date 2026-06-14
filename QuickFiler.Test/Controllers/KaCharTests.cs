using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QuickFiler.Controllers;

namespace QuickFiler.Controllers.Tests
{
    /// <summary>
    /// Unit tests for the keyboard-action value objects <see cref="KaChar"/> and
    /// <see cref="KaCharAsync"/>. Both are pure value objects with no Outlook dependency: they
    /// store a source id, a char key, and a delegate, and expose <c>KeyEquals</c>. Tests cover
    /// construction/positive flows, delegate dispatch (synchronous for KaChar, awaited but
    /// synchronously-completing for KaCharAsync), the documented no-guard behavior for null
    /// delegates, and key-equality edge cases. No timing dependency is introduced.
    /// </summary>
    [TestClass]
    public class KaCharTests
    {
        // ---- KaChar (synchronous Action<char>) ----

        [TestMethod]
        public void KaChar_Constructor_StoresSourceIdKeyAndDelegate()
        {
            // Arrange
            Action<char> action = _ => { };

            // Act
            var ka = new KaChar("src", 'a', action);

            // Assert
            ka.SourceId.Should().Be("src");
            ka.Key.Should().Be('a');
            ka.Delegate.Should().BeSameAs(action);
        }

        [TestMethod]
        public void KaChar_Delegate_DispatchesToSuppliedAction()
        {
            // Arrange
            char received = '\0';
            var ka = new KaChar("src", 'x', c => received = c);

            // Act
            ka.Delegate('x');

            // Assert
            received.Should().Be('x', "invoking the stored delegate dispatches with the argument");
        }

        [TestMethod]
        public void KaChar_KeyEquals_MatchesSameCharAndRejectsOther()
        {
            // Arrange
            var ka = new KaChar("src", 'q', _ => { });

            // Act / Assert
            ka.KeyEquals('q').Should().BeTrue();
            ka.KeyEquals('r').Should().BeFalse();
        }

        [TestMethod]
        public void KaChar_ParameterlessConstructor_LeavesNullDelegate()
        {
            // Arrange / Act: the parameterless constructor performs no initialization.
            var ka = new KaChar();

            // Assert
            ka.Delegate.Should().BeNull("the parameterless ctor does not assign a delegate");
            ka.SourceId.Should().BeNull();
        }

        [TestMethod]
        public void KaChar_Constructor_NullDelegate_IsStoredNotRejected()
        {
            // Arrange / Act: the constructor does not guard against a null delegate.
            var ka = new KaChar("src", 'a', null);

            // Assert
            ka.Delegate.Should().BeNull("a null delegate is stored, not rejected");
        }

        [TestMethod]
        public void KaChar_DefaultCharKey_IsSupported()
        {
            // Arrange / Act
            var ka = new KaChar("src", default(char), _ => { });

            // Assert
            ka.KeyEquals('\0').Should().BeTrue("the default char is a valid boundary key");
        }

        // ---- KaCharAsync (awaited Func<char, Task>) ----

        [TestMethod]
        public void KaCharAsync_Constructor_StoresSourceIdKeyAndDelegate()
        {
            // Arrange
            Func<char, Task> func = _ => Task.CompletedTask;

            // Act
            var ka = new KaCharAsync("src", 'b', func);

            // Assert
            ka.SourceId.Should().Be("src");
            ka.Key.Should().Be('b');
            ka.Delegate.Should().BeSameAs(func);
        }

        [TestMethod]
        public async Task KaCharAsync_Delegate_AwaitsAndCompletesSynchronously()
        {
            // Arrange: the delegate completes synchronously (no Task.Delay / Sleep).
            char received = '\0';
            var ka = new KaCharAsync(
                "src",
                'z',
                c =>
                {
                    received = c;
                    return Task.CompletedTask;
                }
            );

            // Act
            await ka.Delegate('z');

            // Assert
            received
                .Should()
                .Be('z', "awaiting the stored async delegate dispatches the argument");
        }

        [TestMethod]
        public void KaCharAsync_KeyEquals_MatchesSameCharAndRejectsOther()
        {
            // Arrange
            var ka = new KaCharAsync("src", 'm', _ => Task.CompletedTask);

            // Act / Assert
            ka.KeyEquals('m').Should().BeTrue();
            ka.KeyEquals('n').Should().BeFalse();
        }

        [TestMethod]
        public void KaCharAsync_Constructor_NullDelegate_IsStoredNotRejected()
        {
            // Arrange / Act
            var ka = new KaCharAsync("src", 'a', null);

            // Assert
            ka.Delegate.Should().BeNull("a null async delegate is stored, not rejected");
        }
    }
}
