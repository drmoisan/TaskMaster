using System;
using System.Globalization;
using System.Threading.Tasks;
using UtilitiesCS;

namespace TaskMaster
{
    /// <summary>
    /// The availability guard for the Explorer-ribbon Clear Spam Manager command (issue #735,
    /// finding 2), computed live from the auto-file objects container and the engines facade.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The globals chain this command depends on — the globals object, its auto-file objects, that
    /// container's classifier manager, and the engines facade — is genuinely null during the window
    /// between ribbon construction and the completion of add-in initialization. Each link is
    /// assigned only inside the load paths that run after the ribbon exists. Dereferencing the
    /// chain unguarded from a click handler in that window raises an unhandled
    /// <see cref="NullReferenceException"/> from a user-interface event.
    /// </para>
    /// <para>
    /// Callers pass the engine-touching work as a lambda, so the manager and engines dereferences
    /// are evaluated only inside the lambda body and never when the gate is closed. That is what
    /// converts the pre-initialization crash into a single explanatory notice, without scattering
    /// null-conditional operators through the coverage-exempt ribbon shim.
    /// </para>
    /// <para>
    /// This gate is deliberately separate from <see cref="EngineGatedCommandRunner"/>. That
    /// runner's predicate is inbox-engine readiness, which is the wrong question here: Clear Spam
    /// Manager is not a member of <see cref="EngineCommandCatalog"/> and correctly declares no
    /// enabled-state callback, because its real dependency is the availability of the classifier
    /// manager rather than the readiness of an engine.
    /// </para>
    /// <para>
    /// The guard suppresses <em>invocation</em>, never <em>errors</em>. When the gate is open and
    /// the deferred work throws, the exception propagates unchanged; this type contains no
    /// <c>catch</c> clause of any kind, so it can never degenerate into a swallow-all.
    /// </para>
    /// <para>
    /// This type is deliberately NOT marked <c>[ExcludeFromCodeCoverage]</c>: it is host-neutral
    /// decision logic with no COM, no <c>Microsoft.Office.*</c> reference, no
    /// <c>System.Windows.Forms</c> reference and no logger field, and is fully unit-tested. It
    /// follows the <see cref="EngineReadinessGate"/> precedent. Presentation of the notification is
    /// the injected sink's concern and lives in the coverage-exempt ribbon shim.
    /// </para>
    /// </remarks>
    internal sealed class SpamManagerResetGate
    {
        private readonly Func<IAppAutoFileObjects> _autoFileAccessor;
        private readonly Func<IAppItemEngines> _enginesAccessor;
        private readonly Action<string> _notifyNotReady;

        /// <summary>
        /// Creates a gate over an auto-file-objects accessor, an engines accessor and a
        /// notification sink.
        /// </summary>
        /// <param name="autoFileAccessor">
        /// Supplies the current auto-file objects container, which owns the classifier manager.
        /// Must not be null, but is expected to return null before the ribbon controller has been
        /// given its globals and before the basic load has run, which the gate treats as "not
        /// ready" rather than as an error.
        /// </param>
        /// <param name="enginesAccessor">
        /// Supplies the current engines facade. Must not be null, but is expected to return null in
        /// the same pre-initialization window, with the same "not ready" treatment.
        /// </param>
        /// <param name="notifyNotReady">
        /// Receives exactly one message per blocked invocation. Must not be null. Presentation is
        /// the sink's concern; this type decides only whether to notify and what the message says.
        /// </param>
        /// <exception cref="ArgumentNullException">Any argument is null.</exception>
        internal SpamManagerResetGate(
            Func<IAppAutoFileObjects> autoFileAccessor,
            Func<IAppItemEngines> enginesAccessor,
            Action<string> notifyNotReady
        )
        {
            _autoFileAccessor =
                autoFileAccessor ?? throw new ArgumentNullException(nameof(autoFileAccessor));
            _enginesAccessor =
                enginesAccessor ?? throw new ArgumentNullException(nameof(enginesAccessor));
            _notifyNotReady =
                notifyNotReady ?? throw new ArgumentNullException(nameof(notifyNotReady));
        }

        /// <summary>
        /// Runs <paramref name="reset"/> only when both the classifier manager and the engines
        /// facade have been resolved.
        /// </summary>
        /// <param name="reset">
        /// The engine-touching reset work, deferred into a lambda so it is not evaluated when the
        /// gate is closed. It receives the resolved manager and engines rather than reading the
        /// globals chain itself. Must not be null.
        /// </param>
        /// <returns>
        /// The task returned by <paramref name="reset"/> when the gate is open, or an
        /// already-completed task when the gate is closed. The reset task is returned directly and
        /// is not awaited here, so a fault from the deferred work propagates unchanged.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="reset"/> is null. Thrown before either accessor is invoked, so a caller
        /// error is never masked by a "not ready" notice.
        /// </exception>
        internal Task RunAsync(Func<ManagerAsyncLazy, IAppItemEngines, Task> reset)
        {
            if (reset is null)
            {
                throw new ArgumentNullException(nameof(reset));
            }

            var autoFile = _autoFileAccessor();
            var manager = autoFile?.Manager;
            var engines = _enginesAccessor();

            if (manager is null || engines is null)
            {
                _notifyNotReady(BuildNotReadyMessage());
                return Task.CompletedTask;
            }

            return reset(manager, engines);
        }

        /// <summary>
        /// Builds the single "still loading" message emitted for a blocked invocation.
        /// </summary>
        /// <remarks>
        /// The message names no control id. Unlike <see cref="EngineGatedCommandRunner"/>, which
        /// serves fourteen commands and must say which one was blocked, this gate serves exactly
        /// one command, so a control id would add no information.
        /// </remarks>
        private static string BuildNotReadyMessage()
        {
            return string.Format(
                CultureInfo.CurrentCulture,
                "The Spam Manager cannot be cleared yet because the classifier manager is still "
                    + "loading. Please try again once initialization completes."
            );
        }
    }
}
