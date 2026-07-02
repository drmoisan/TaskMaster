using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS.Threading;

namespace QuickFiler.Controllers.Tests
{
    /// <summary>
    /// Construction smoke test (cycle-2 Phase 6, P6-T1/P6-T12) for the production
    /// <see cref="WpfUiDispatcher"/>. The adapter forwards to the static <see cref="UtilitiesCS.UiThread.Dispatcher"/>,
    /// which requires a live WPF message pump; its forwarding bodies are therefore exempt and only the
    /// construction/contract is asserted here.
    /// </summary>
    [TestClass]
    public class WpfUiDispatcherTests
    {
        [TestMethod]
        public void Construction_YieldsAnIUiDispatcher()
        {
            IUiDispatcher dispatcher = new WpfUiDispatcher();

            dispatcher.Should().NotBeNull();
            dispatcher.Should().BeAssignableTo<IUiDispatcher>();
        }
    }
}
