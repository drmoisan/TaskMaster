using System;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TaskMaster
{
    /// <summary>
    /// Issue #503 engine-command wiring for <see cref="RibbonController"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This is thin COM/VSTO glue: every decision it makes is delegated to the host-neutral,
    /// unit-tested <see cref="EngineGatedCommandRunner"/> / <see cref="EngineReadinessGate"/>
    /// pair. The type-level <c>[ExcludeFromCodeCoverage]</c> declared on the
    /// <c>RibbonController.cs</c> partial already applies to this file, so no second attribute is
    /// declared here.
    /// </para>
    /// </remarks>
    public partial class RibbonController
    {
        private EngineGatedCommandRunner _engineCommandRunner;

        /// <summary>
        /// The lazily-built gated runner for engine-backed ribbon commands.
        /// </summary>
        /// <remarks>
        /// The readiness accessor is <c>() =&gt; Globals?.Engines</c>. The null-conditional is
        /// load-bearing: ribbon callbacks are reachable before <c>SetGlobals</c> has run, and the
        /// gate must report "not ready" rather than throw. The accessor reads <c>Globals</c>
        /// directly rather than going through the <c>RibbonController.Engines</c> property so that
        /// the readiness decision stays independent of that property's implementation. (As of
        /// issue #507 that property is itself null-safe — <c>Globals?.Engines</c> — so it would no
        /// longer throw here; keeping the accessor direct is a deliberate decoupling, not a
        /// workaround.) The readiness decision is likewise never routed through <c>SB</c> /
        /// <c>Triage</c> / <c>TriageAsync</c>, whose getters install a real
        /// <c>WindowsFormsSynchronizationContext</c> on the calling thread as a side effect.
        /// </remarks>
        private EngineGatedCommandRunner EngineCommands =>
            _engineCommandRunner ??= new EngineGatedCommandRunner(
                // The null-forgiving operator records that this accessor may legitimately return
                // null before SetGlobals has run. EngineReadinessGate treats a null result as
                // "not ready" by contract, so null is a supported value rather than a defect.
                new EngineReadinessGate(() => Globals?.Engines!),
                NotifyEngineCommandNotReady
            );

        /// <summary>
        /// The <c>getEnabled</c> decision for an engine-backed ribbon control.
        /// </summary>
        /// <param name="controlId">The ribbon control id.</param>
        /// <returns>
        /// <see langword="true"/> only when the control is engine-backed and its engine is loaded.
        /// </returns>
        internal bool IsEngineCommandEnabled(string controlId)
        {
            return EngineCommands.IsCommandEnabled(controlId);
        }

        /// <summary>
        /// Runs an engine-backed ribbon command, suppressing invocation while its engine is
        /// still loading.
        /// </summary>
        /// <param name="controlId">The ribbon control id that requested the work.</param>
        /// <param name="action">
        /// The engine-touching work, supplied as a lambda so the engine dereference is deferred
        /// and never evaluated when the gate is closed.
        /// </param>
        /// <returns>A task that completes when the action completes, or immediately when the
        /// gate is closed.</returns>
        internal Task RunEngineCommandAsync(string controlId, Func<Task> action)
        {
            return EngineCommands.RunAsync(controlId, action);
        }

        /// <summary>
        /// Asks the ribbon to re-query <c>getEnabled</c> for every engine-backed control.
        /// </summary>
        /// <remarks>
        /// No-ops when the viewer has not been attached yet (<c>Ribbon_Load</c> has not run), so
        /// an early refresh is harmless.
        /// </remarks>
        internal void RefreshEngineCommands()
        {
            _viewer?.InvalidateEngineCommands();
        }

        /// <summary>
        /// Presents the single "still loading" notice emitted for a blocked engine command.
        /// </summary>
        /// <remarks>
        /// Presentation only. The decision to notify and the message content are made by
        /// <see cref="EngineGatedCommandRunner"/> and are unit-tested through its injected sink;
        /// no test constructs a <see cref="MessageBox"/>. The repository has no non-modal notice
        /// surface, so this uses the established <c>logger.Warn</c> plus
        /// <c>MessageBox.Show</c> mechanism already used elsewhere in the ribbon layer.
        /// </remarks>
        private void NotifyEngineCommandNotReady(string message)
        {
            logger.Warn(message);
            MessageBox.Show(message);
        }
    }
}
