using System;
using System.Globalization;
using System.Threading.Tasks;

namespace TaskMaster
{
    /// <summary>
    /// The issue #503 click guard and the <c>getEnabled</c> decision for engine-backed
    /// Explorer-ribbon commands.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Callers pass the engine-touching work as a <see cref="Func{Task}"/> lambda, so the engine
    /// dereference is evaluated only inside the lambda body and never when the gate is closed.
    /// That is what converts the pre-initialization <see cref="NullReferenceException"/> (the
    /// <c>Controller.SB</c> / <c>Controller.Triage</c> paths) and
    /// <see cref="System.Collections.Generic.KeyNotFoundException"/> (the <c>TestSpam_Click</c>
    /// dictionary-indexer path) into a no-op, without scattering null-conditional operators
    /// through <c>RibbonViewer</c>.
    /// </para>
    /// <para>
    /// The guard suppresses <em>invocation</em>, never <em>errors</em>. When the gate is open and
    /// the action throws, the exception propagates unchanged; this type contains no
    /// <c>catch</c> clause of any kind, so it can never degenerate into a swallow-all.
    /// </para>
    /// <para>
    /// This type is deliberately NOT marked <c>[ExcludeFromCodeCoverage]</c>: it is host-neutral
    /// decision logic with no COM and no <c>Microsoft.Office.*</c> reference, and is fully
    /// unit-tested. Presentation of the notification is the injected sink's concern and lives in
    /// the coverage-exempt ribbon shim.
    /// </para>
    /// </remarks>
    internal sealed class EngineGatedCommandRunner
    {
        /// <summary>
        /// Rendered in place of a control id when the caller supplied null, so a notification is
        /// never ambiguous about which id was seen.
        /// </summary>
        private const string NullControlIdToken = "(null)";

        /// <summary>
        /// Rendered in place of an engine key when the control id is not in
        /// <see cref="EngineCommandCatalog"/>, so an unmapped id is distinguishable from a mapped
        /// id whose engine has not loaded.
        /// </summary>
        private const string UnmappedEngineToken = "(unmapped)";

        private readonly EngineReadinessGate _gate;
        private readonly Action<string> _notifyNotReady;

        /// <summary>
        /// Creates a runner over a readiness gate and a notification sink.
        /// </summary>
        /// <param name="gate">The per-engine-key readiness signal; must not be null.</param>
        /// <param name="notifyNotReady">
        /// Receives exactly one message per blocked invocation. Must not be null. Presentation is
        /// the sink's concern; this type decides only whether to notify and what the message says.
        /// </param>
        /// <exception cref="ArgumentNullException">Either argument is null.</exception>
        internal EngineGatedCommandRunner(EngineReadinessGate gate, Action<string> notifyNotReady)
        {
            _gate = gate ?? throw new ArgumentNullException(nameof(gate));
            _notifyNotReady =
                notifyNotReady ?? throw new ArgumentNullException(nameof(notifyNotReady));
        }

        /// <summary>
        /// The <c>getEnabled</c> decision for a ribbon control.
        /// </summary>
        /// <param name="controlId">The ribbon control id; ordinal comparison.</param>
        /// <returns>
        /// <see langword="true"/> only when the id is an engine-backed control id and its backing
        /// engine is currently available. A null, empty, or unknown id yields
        /// <see langword="false"/>.
        /// </returns>
        internal bool IsCommandEnabled(string controlId)
        {
            return EngineCommandCatalog.TryGetEngineName(controlId, out var engineName)
                && _gate.IsEngineReady(engineName);
        }

        /// <summary>
        /// Runs <paramref name="action"/> only when the command's backing engine is available.
        /// </summary>
        /// <param name="controlId">The ribbon control id that requested the work.</param>
        /// <param name="action">
        /// The engine-touching work, deferred into a lambda so it is not evaluated when the gate
        /// is closed. Must not be null.
        /// </param>
        /// <returns>
        /// A task that completes when the action completes, or an already-completed task when the
        /// gate is closed.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="action"/> is null. Thrown before any readiness evaluation.
        /// </exception>
        internal Task RunAsync(string controlId, Func<Task> action)
        {
            if (action is null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            if (!IsCommandEnabled(controlId))
            {
                _notifyNotReady(BuildNotReadyMessage(controlId));
                return Task.CompletedTask;
            }

            return action();
        }

        /// <summary>
        /// Builds the single "still loading" message emitted for a blocked invocation. The message
        /// always carries the control id, and carries the engine key when the id is mapped.
        /// </summary>
        private static string BuildNotReadyMessage(string controlId)
        {
            var renderedControlId = controlId ?? NullControlIdToken;
            var engineName = EngineCommandCatalog.TryGetEngineName(controlId, out var mapped)
                ? mapped
                : UnmappedEngineToken;

            return string.Format(
                CultureInfo.CurrentCulture,
                "The command '{0}' is still loading because its engine '{1}' is not available yet. "
                    + "Please try again once initialization completes.",
                renderedControlId,
                engineName
            );
        }
    }
}
