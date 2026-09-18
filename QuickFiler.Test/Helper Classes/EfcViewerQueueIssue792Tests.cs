using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QuickFiler;

namespace QuickFiler.Test.HelperClasses
{
    /// <summary>
    /// Regression test for issue #792 on <see cref="EfcViewerQueue"/>: the UI-thread half of the
    /// pop-out contract is verifiable only as a scheduler-delegate identity assertion. The test
    /// reads the delegate and writes nothing, because the only writer of these statics is a
    /// serialized test class and a concurrent write from the parallel bucket would race it.
    /// </summary>
    [TestClass]
    public sealed class EfcViewerQueueIssue792Tests
    {
        /// <summary>
        /// The production blocking scheduler must be the named static method
        /// <c>InvokeOnUiDispatcher</c>, not an inline lambda. The delegate is never invoked here:
        /// the named method reaches <c>UiThread.Dispatcher</c>, which throws when no UI thread has
        /// been initialized. Before the fix the default is a compiler-generated lambda whose
        /// method name is not <c>InvokeOnUiDispatcher</c>.
        /// </summary>
        [TestMethod]
        public void ProductionBlockingPriorityScheduler_DefaultIsTheNamedUiDispatcherInvoke()
        {
            // Arrange / Act - a read only; no static is written.
            var scheduler = EfcViewerQueue.ProductionBlockingPriorityScheduler;

            // Assert
            scheduler.Should().NotBeNull("the production blocking scheduler must have a default");
            scheduler
                .Method.Name.Should()
                .Be(
                    "InvokeOnUiDispatcher",
                    "the blocking scheduler must be the named UI-dispatcher invoke, not a lambda"
                );
            scheduler.Target.Should().BeNull("a static method group carries no target instance");
        }
    }
}
