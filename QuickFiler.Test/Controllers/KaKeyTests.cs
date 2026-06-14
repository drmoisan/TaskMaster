using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QuickFiler.Controllers;

namespace QuickFiler.Controllers.Tests
{
    /// <summary>
    /// Unit tests for the keyboard-action value objects <see cref="KaKey"/> and
    /// <see cref="KaKeyAsync"/>, which store a <see cref="Keys"/> enum key and an
    /// <c>Action&lt;Keys&gt;</c> / <c>Func&lt;Keys, Task&gt;</c> delegate respectively. Keys is an
    /// enum, so no WinForms message loop is required. Tests cover construction, delegate dispatch
    /// (synchronous and awaited synchronously-completing), null-delegate no-guard behavior, and
    /// key-equality edges. No timing dependency is introduced.
    /// </summary>
    [TestClass]
    public class KaKeyTests
    {
        // ---- KaKey (synchronous Action<Keys>) ----

        [TestMethod]
        public void KaKey_Constructor_StoresSourceIdKeyAndDelegate()
        {
            // Arrange
            Action<Keys> action = _ => { };

            // Act
            var ka = new KaKey("src", Keys.Enter, action);

            // Assert
            ka.SourceId.Should().Be("src");
            ka.Key.Should().Be(Keys.Enter);
            ka.Delegate.Should().BeSameAs(action);
        }

        [TestMethod]
        public void KaKey_Delegate_DispatchesToSuppliedAction()
        {
            // Arrange
            Keys received = Keys.None;
            var ka = new KaKey("src", Keys.Escape, k => received = k);

            // Act
            ka.Delegate(Keys.Escape);

            // Assert
            received.Should().Be(Keys.Escape);
        }

        [TestMethod]
        public void KaKey_KeyEquals_MatchesSameKeyAndRejectsOther()
        {
            // Arrange
            var ka = new KaKey("src", Keys.A, _ => { });

            // Act / Assert
            ka.KeyEquals(Keys.A).Should().BeTrue();
            ka.KeyEquals(Keys.B).Should().BeFalse();
        }

        [TestMethod]
        public void KaKey_ParameterlessConstructor_LeavesNullDelegateAndNoneKey()
        {
            // Arrange / Act
            var ka = new KaKey();

            // Assert
            ka.Delegate.Should().BeNull();
            ka.Key.Should().Be(Keys.None, "the default Keys value is None (0)");
        }

        [TestMethod]
        public void KaKey_Constructor_NullDelegate_IsStoredNotRejected()
        {
            // Arrange / Act
            var ka = new KaKey("src", Keys.A, null);

            // Assert
            ka.Delegate.Should().BeNull("a null delegate is stored, not rejected");
        }

        // ---- KaKeyAsync (awaited Func<Keys, Task>) ----

        [TestMethod]
        public void KaKeyAsync_Constructor_StoresSourceIdKeyAndDelegate()
        {
            // Arrange
            Func<Keys, Task> func = _ => Task.CompletedTask;

            // Act
            var ka = new KaKeyAsync("src", Keys.Tab, func);

            // Assert
            ka.SourceId.Should().Be("src");
            ka.Key.Should().Be(Keys.Tab);
            ka.Delegate.Should().BeSameAs(func);
        }

        [TestMethod]
        public async Task KaKeyAsync_Delegate_AwaitsAndCompletesSynchronously()
        {
            // Arrange: synchronously-completing delegate (no Task.Delay / Sleep).
            Keys received = Keys.None;
            var ka = new KaKeyAsync(
                "src",
                Keys.Space,
                k =>
                {
                    received = k;
                    return Task.CompletedTask;
                }
            );

            // Act
            await ka.Delegate(Keys.Space);

            // Assert
            received.Should().Be(Keys.Space);
        }

        [TestMethod]
        public void KaKeyAsync_KeyEquals_MatchesSameKeyAndRejectsOther()
        {
            // Arrange
            var ka = new KaKeyAsync("src", Keys.F1, _ => Task.CompletedTask);

            // Act / Assert
            ka.KeyEquals(Keys.F1).Should().BeTrue();
            ka.KeyEquals(Keys.F2).Should().BeFalse();
        }

        [TestMethod]
        public void KaKeyAsync_Constructor_NullDelegate_IsStoredNotRejected()
        {
            // Arrange / Act
            var ka = new KaKeyAsync("src", Keys.A, null);

            // Assert
            ka.Delegate.Should().BeNull("a null async delegate is stored, not rejected");
        }
    }
}
