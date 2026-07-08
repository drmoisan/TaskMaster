#nullable enable
using System;
using System.Runtime.InteropServices;

namespace TaskMaster.Test.AppGlobals
{
    /// <summary>
    /// Host-neutral seam that runs the live-Outlook harness in two explicit phases and classifies
    /// the outcome. The classification decision — "no Outlook to exercise, so skip" versus "a real
    /// failure, so capture" — is scoped to the phase in which the exception is thrown rather than to
    /// a narrow HRESULT whitelist.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Phase 1 (construction): the caller supplies a <see cref="Func{T}"/> that constructs the
    /// resource (in production, <c>new Outlook.Application()</c>). Any
    /// <see cref="COMException"/> thrown here is a pure environment/launch failure — no
    /// code-under-test has run yet — so it is reported as a skip regardless of HRESULT (specifically
    /// including <c>0x80010100</c> RPC_E_SYS_CALL_FAILED, which a narrow whitelist would miss). A
    /// NON-COM construction failure is NOT a skip: it is captured as a failure, because it does not
    /// represent "Outlook is unavailable".
    /// </para>
    /// <para>
    /// Phase 2 (exercise): the caller supplies an <see cref="Action{T}"/> that exercises the
    /// constructed resource (in production, the readiness gate, coordinator tick, and hookup
    /// callback). Any exception thrown here — INCLUDING a <see cref="COMException"/> — is captured as
    /// a failure and never converted to a skip, preserving strict failure semantics for the
    /// code-under-test.
    /// </para>
    /// <para>
    /// The delegate parameters are the injected seam, which makes this classification unit-testable
    /// without a live Outlook, without temporary files, and without a mock.
    /// </para>
    /// </remarks>
    internal static class LiveOutlookHarnessRunner
    {
        /// <summary>
        /// Immutable result of a harness run. Exactly one of the two properties is meaningful:
        /// a non-null <see cref="SkipReason"/> indicates a construction-phase COMException (skip);
        /// a non-null <see cref="Captured"/> indicates a captured failure; both null indicates the
        /// exercise phase completed successfully.
        /// </summary>
        /// <remarks>
        /// Declared as a plain <c>readonly struct</c> with a constructor and get-only auto-properties
        /// because the target framework is net481, which has no <c>IsExternalInit</c> and therefore
        /// cannot use <c>init</c> accessors, <c>record</c>, or <c>record struct</c> (CS0518).
        /// </remarks>
        internal readonly struct HarnessOutcome
        {
            /// <summary>
            /// Initializes a new <see cref="HarnessOutcome"/>.
            /// </summary>
            /// <param name="captured">The captured failure exception, or <c>null</c>.</param>
            /// <param name="skipReason">The skip reason (construction-phase COMException), or <c>null</c>.</param>
            public HarnessOutcome(Exception? captured, string? skipReason)
            {
                Captured = captured;
                SkipReason = skipReason;
            }

            /// <summary>
            /// The exception captured as a failure, or <c>null</c> when no failure was captured.
            /// </summary>
            public Exception? Captured { get; }

            /// <summary>
            /// A human-readable reason the run was skipped (a construction-phase COMException), or
            /// <c>null</c> when the run was not skipped.
            /// </summary>
            public string? SkipReason { get; }
        }

        /// <summary>
        /// Runs the harness in two phases and classifies the outcome.
        /// </summary>
        /// <typeparam name="T">The type of the constructed resource.</typeparam>
        /// <param name="construct">
        /// Constructs the resource (construction phase). A <see cref="COMException"/> thrown here is
        /// classified as a skip regardless of HRESULT; any other exception is captured as a failure.
        /// </param>
        /// <param name="exercise">
        /// Exercises the constructed resource (exercise phase). Any exception thrown here, including a
        /// <see cref="COMException"/>, is captured as a failure.
        /// </param>
        /// <returns>
        /// A <see cref="HarnessOutcome"/> whose <see cref="HarnessOutcome.SkipReason"/> is non-null on
        /// a construction-phase COMException, whose <see cref="HarnessOutcome.Captured"/> is non-null
        /// on any captured failure, and whose properties are both null on a successful exercise.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="construct"/> or <paramref name="exercise"/> is <c>null</c>.
        /// </exception>
        internal static HarnessOutcome Run<T>(Func<T> construct, Action<T> exercise)
        {
            if (construct is null)
            {
                throw new ArgumentNullException(nameof(construct));
            }

            if (exercise is null)
            {
                throw new ArgumentNullException(nameof(exercise));
            }

            T resource;

            // Phase 1 — construction. A COMException here is a pure environment/launch failure
            // (Outlook not registered/available/launchable); skip regardless of HRESULT. A non-COM
            // failure here is a real failure and is captured, not skipped.
            try
            {
                resource = construct();
            }
            catch (COMException comEx)
            {
                string skipReason =
                    $"Outlook could not be constructed in this environment "
                    + $"(HRESULT 0x{comEx.ErrorCode:X8}: {comEx.Message}).";
                return new HarnessOutcome(null, skipReason);
            }
            catch (Exception ex)
            {
                return new HarnessOutcome(ex, null);
            }

            // Phase 2 — exercise. Any exception here (including a COMException) is a captured
            // failure: the code-under-test ran, so a fault is a genuine defect, never a skip.
            try
            {
                exercise(resource);
                return new HarnessOutcome(null, null);
            }
            catch (Exception ex)
            {
                return new HarnessOutcome(ex, null);
            }
        }
    }
}
