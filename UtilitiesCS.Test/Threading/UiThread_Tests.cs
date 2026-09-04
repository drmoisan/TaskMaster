using System;
using System.Reflection;
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
        public void IsCompleted_WhenContextMatchesCurrent_ReturnsTrue()
        {
            // Arrange: set the thread's synchronization context to the same instance captured
            // by the awaiter so that the equality check (_context == Current) evaluates true
            var context = new SynchronizationContext();
            SynchronizationContext.SetSynchronizationContext(context);
            try
            {
                var awaiter = new UiThread.SynchronizationContextAwaiter(context);

                // Act
                var result = awaiter.IsCompleted;

                // Assert
                result.Should().BeTrue();
            }
            finally
            {
                // Restore the context so this test does not influence other test-thread tests
                SynchronizationContext.SetSynchronizationContext(null);
            }
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

    /// <summary>
    /// Regression coverage for issue #584: the accessor contract of
    /// <c>UiThread.Dispatcher</c>.
    ///
    /// Purpose:
    ///     Reflection is used to write the private static <c>UiThread._dispatcher</c> backing
    ///     field directly. The property has a private setter whose only production writer is
    ///     <c>UiThread.Initialize()</c>, which shows a real hidden WinForms window, so the
    ///     backing field is the one seam that lets a unit test place the accessor in each of
    ///     its two states. Driving the contract through that seam makes both tests
    ///     deterministic without any timing construct.
    ///
    ///     Both tests capture the prior field value and put it back in a finally block, so the
    ///     process-global state is left exactly as it was found.
    /// </summary>
    [TestClass]
    [DoNotParallelize]
    public class UiThread_Dispatcher_Tests
    {
        private static FieldInfo DispatcherField()
        {
            return typeof(UiThread).GetField(
                "_dispatcher",
                BindingFlags.NonPublic | BindingFlags.Static
            );
        }

        [TestMethod]
        public void Dispatcher_WhenBackingFieldIsNull_ThrowsInvalidOperationExceptionNamingInitialize()
        {
            // Arrange
            var field = DispatcherField();
            field.Should().NotBeNull();
            var prior = field.GetValue(null);
            field.SetValue(null, null);
            try
            {
                // Act
                Action act = () =>
                {
                    _ = UiThread.Dispatcher;
                };

                // Assert
                act.Should()
                    .Throw<InvalidOperationException>()
                    .WithMessage("*UiThread.Initialize()*");
            }
            finally
            {
                field.SetValue(null, prior);
            }
        }

        [TestMethod]
        public void Dispatcher_WhenBackingFieldIsPopulated_ReturnsThatSameInstance()
        {
            // Arrange
            var field = DispatcherField();
            var prior = field.GetValue(null);
            var expected = System.Windows.Threading.Dispatcher.CurrentDispatcher;
            field.SetValue(null, expected);
            try
            {
                // Act / Assert
                UiThread.Dispatcher.Should().BeSameAs(expected);
            }
            finally
            {
                field.SetValue(null, prior);
            }
        }
    }
}
