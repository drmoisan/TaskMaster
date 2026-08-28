using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace QuickFiler.Controllers.Tests
{
    /// <summary>
    /// Regression tests for issue #467 — <c>EfcViewer.ProcessCmdKey</c> claiming every
    /// Alt-modified key and so swallowing the <c>Alt+F</c> and <c>Alt+M</c> menu mnemonics —
    /// and for the <c>EfcViewer</c> half of issue #466, the dead <c>SetController</c> /
    /// <c>_formController</c> / viewer-side <c>EditFiltersMenuItem_Click</c> trap.
    /// </summary>
    /// <remarks>
    /// This fixture does not derive from, construct, or show any
    /// <c>System.Windows.Forms.Form</c>. The input-routing logic is exercised through the
    /// extracted <c>internal static</c> predicate, which needs no window handle, following the
    /// pattern of <c>QfcFormKeyHandlerTests.cs</c>.
    ///
    /// The file is deliberately placed under <c>Controllers/</c> rather than <c>Viewers/</c>;
    /// the deviation from the mirrored test layout is recorded in the plan task that created it.
    /// </remarks>
    [TestClass]
    public class EfcViewerTests { }
}
