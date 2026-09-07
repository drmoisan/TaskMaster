using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TaskMaster
{
    /// <summary>
    /// Boundary through which an Explorer-ribbon command runs its engine-touching work.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A ribbon callback is an <c>async void</c> method, so an exception that escapes it reaches
    /// Outlook as an unhandled exception rather than a diagnosable message. <see cref="RunAsync"/>
    /// therefore catches every failure, forwards it to both injected sinks, and never rethrows.
    /// Containment is total: a sink that itself fails is caught as well, because a broken dialog
    /// must not convert a handled command failure back into an unhandled exception.
    /// </para>
    /// <para>
    /// This type deliberately carries no coverage-exemption attribute: it is host-neutral
    /// decision logic with no COM and no <c>Microsoft.Office.*</c> reference, and is unit-tested.
    /// Presentation of the failure is the injected sink's concern and lives in the
    /// coverage-exempt ribbon shim.
    /// </para>
    /// </remarks>
    internal sealed class RibbonCommandBoundary
    {
        private readonly Action<string, System.Exception> _logFailure;
        private readonly Action<string> _presentFailure;

        /// <summary>
        /// Creates a boundary over the supplied failure sinks.
        /// </summary>
        /// <param name="logFailure">
        /// Receives the command name and the exception when a command fails.
        /// </param>
        /// <param name="presentFailure">Receives the message shown to the user.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when either sink is null. Both sinks are required at construction so a boundary
        /// can never be built in a state where a failure would go unreported.
        /// </exception>
        internal RibbonCommandBoundary(
            Action<string, System.Exception> logFailure,
            Action<string> presentFailure
        )
        {
            _logFailure = logFailure ?? throw new ArgumentNullException(nameof(logFailure));
            _presentFailure =
                presentFailure ?? throw new ArgumentNullException(nameof(presentFailure));
        }

        /// <summary>
        /// Runs <paramref name="action"/> under the boundary, containing any failure it raises.
        /// </summary>
        /// <param name="commandName">Name of the ribbon command, reported on failure.</param>
        /// <param name="action">The engine-touching work.</param>
        /// <returns>
        /// A task that completes when the action completes or when its failure has been reported.
        /// The returned task never faults: the caller is an <c>async void</c> ribbon callback and
        /// has no way to observe a propagated failure.
        /// </returns>
        internal async Task RunAsync(string commandName, Func<Task> action)
        {
            try
            {
                await action();
            }
            catch (System.Exception exception)
            {
                ReportFailure(commandName, exception);
            }
        }

        /// <summary>
        /// Forwards a command failure to both sinks, containing a failure in either one.
        /// </summary>
        /// <remarks>
        /// The log sink runs first so that a failure inside the presentation sink is recorded
        /// after the failure that caused it, preserving causal order in the log.
        /// </remarks>
        private void ReportFailure(string commandName, System.Exception exception)
        {
            SafeLog(commandName, exception);

            try
            {
                _presentFailure(BuildFailureMessage(commandName, exception));
            }
            catch (System.Exception presentationException)
            {
                // The user cannot be told the dialog itself failed, so the log is the only
                // remaining channel. Failing here would re-raise on the async void callback.
                SafeLog(commandName, presentationException);
            }
        }

        /// <summary>
        /// Invokes the log sink, containing a failure in the sink itself.
        /// </summary>
        /// <remarks>
        /// This is the last reporting channel. If it throws there is nowhere left to report to,
        /// so the failure is discarded deliberately rather than allowed to escape the boundary.
        /// </remarks>
        private void SafeLog(string commandName, System.Exception exception)
        {
            try
            {
                _logFailure(commandName, exception);
            }
            catch (System.Exception)
            {
                // Intentionally discarded: see remarks.
            }
        }

        /// <summary>
        /// Builds the message shown to the user for a failed command.
        /// </summary>
        /// <remarks>
        /// The message renders the whole exception chain rather than the outermost message alone.
        /// The timeout helper's result marshalling wraps upstream of the rethrow in the QuickFiler
        /// data model, so the exception this boundary observes is routinely an
        /// <see cref="AggregateException"/>. A rethrow with <c>throw;</c> restores the original
        /// stack but does not unwrap, and the wrapper's own message is
        /// "One or more errors occurred.", which carries no actionable content. Rendering the
        /// inner detail is what makes the dialog diagnosable.
        /// </remarks>
        private static string BuildFailureMessage(string commandName, System.Exception exception)
        {
            var details = new List<string>();
            CollectDetail(exception, details);
            return $"{commandName} failed: {string.Join(" -> ", details)}";
        }

        /// <summary>
        /// Appends a description of <paramref name="exception"/> and of every exception nested
        /// inside it, outermost first.
        /// </summary>
        /// <remarks>
        /// An <see cref="AggregateException"/> contributes its inner exceptions rather than its
        /// own summary message, which says only that one or more errors occurred. It contributes
        /// its own description only when it wraps nothing, so the message can never come back
        /// empty.
        /// </remarks>
        private static void CollectDetail(System.Exception exception, ICollection<string> details)
        {
            if (exception is null)
            {
                return;
            }

            if (exception is AggregateException aggregate)
            {
                var inner = aggregate.Flatten().InnerExceptions;
                if (inner.Count == 0)
                {
                    details.Add(Describe(aggregate));
                    return;
                }

                foreach (var innerException in inner)
                {
                    CollectDetail(innerException, details);
                }

                return;
            }

            details.Add(Describe(exception));
            CollectDetail(exception.InnerException, details);
        }

        /// <summary>Describes a single exception by type name and message.</summary>
        private static string Describe(System.Exception exception) =>
            $"{exception.GetType().Name}: {exception.Message}";
    }
}
