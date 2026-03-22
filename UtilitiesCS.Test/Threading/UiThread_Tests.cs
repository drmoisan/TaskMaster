using System;
using System.Threading;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UtilitiesCS.Test.Threading
{
    [TestClass]
    public class SynchronizationContextAwaiter_Tests
    {
        [TestMethod]
        public void Constructor_NullContext_ThrowsArgumentNullException()
        {
            // Act
            Action act = () => new UiThread.SynchronizationContextAwaiter(null);

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void IsCompleted_WhenContextIsNotCurrent_ReturnsFalse()
        {
            // Arrange
            var context = new SynchronizationContext();
            var awaiter = new UiThread.SynchronizationContextAwaiter(context);

            // Act
            var result = awaiter.IsCompleted;

            // Assert
            result.Should().BeFalse();
        }

        [TestMethod]
        public void GetResult_DoesNotThrow()
        {
            // Arrange
            var context = new SynchronizationContext();
            var awaiter = new UiThread.SynchronizationContextAwaiter(context);

            // Act
            Action act = () => awaiter.GetResult();

            // Assert
            act.Should().NotThrow();
        }

        [TestMethod]
        public void OnCompleted_PostsCallbackToContext()
        {
            // Arrange
            Action postedCallback = null;
            var mockContext = new TestSynchronizationContext(cb => postedCallback = cb);
            var awaiter = new UiThread.SynchronizationContextAwaiter(mockContext);
            Action continuation = () => { };

            // Act
            awaiter.OnCompleted(continuation);

            // Assert
            postedCallback.Should().NotBeNull();
        }

        private class TestSynchronizationContext : SynchronizationContext
        {
            private readonly Action<Action> _onPost;

            public TestSynchronizationContext(Action<Action> onPost)
            {
                _onPost = onPost;
            }

            public override void Post(SendOrPostCallback d, object state)
            {
                _onPost?.Invoke((Action)state);
            }
        }
    }
}
