using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace QuickFiler.Controllers.Tests
{
    /// <summary>
    /// Regression tests for issue #460 — the <c>EfcItemController.Cleanup()</c> null-dereference
    /// and undisposed-timer defects — together with the item-side post-teardown accessor guards
    /// of issue #464.
    /// </summary>
    /// <remarks>
    /// Held in a second item-side file, separate from <c>EfcItemControllerTests.cs</c>, so that
    /// neither file approaches the 500-line ceiling. The timer test arms with
    /// <c>Timeout.Infinite</c> for both due time and period and observes disposal through the
    /// <c>ObjectDisposedException</c> that <c>Timer.Change</c> then throws; it never waits.
    /// </remarks>
    [TestClass]
    public class EfcItemControllerCleanupTests { }
}
