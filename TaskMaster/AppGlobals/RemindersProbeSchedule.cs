using System;

namespace TaskMaster
{
    /// <summary>
    /// Pure, deterministic decision/scheduling seam for the Issue #207 increment-3
    /// <c>OlReminders</c> first-access latency probe. Given the configured
    /// <c>RemindersProbeDelaySeconds</c> user setting, it resolves whether the first
    /// <c>Globals.Ol.OlReminders</c> access should be deferred and, if so, by how long.
    /// </summary>
    /// <remarks>
    /// This is the unit-tested decision seam for the increment-3 probe. It contains no COM,
    /// no timer, no clock, and no I/O, so it is fully deterministic and is covered by
    /// <c>TaskMaster.Test/AppGlobals/RemindersProbeScheduleTests.cs</c>. The COM access and the
    /// <see cref="System.Windows.Threading.DispatcherTimer"/> wiring that consume this decision
    /// live in <c>AppEvents.Hook()</c> and are COM/VSTO-exempt per the <c>CLAUDE.md</c> coverage
    /// exemption. Implemented as a <see langword="readonly"/> <see langword="struct"/> with an
    /// explicit constructor (not a positional <c>record struct</c>) because the net48 target lacks
    /// <c>System.Runtime.CompilerServices.IsExternalInit</c> (CS0518).
    /// </remarks>
    internal readonly struct RemindersProbeSchedule
    {
        /// <summary>
        /// Creates a schedule decision from the configured probe delay in seconds.
        /// A value strictly greater than zero defers the first access by that many seconds;
        /// zero or negative values resolve to no deferral.
        /// </summary>
        /// <param name="configuredSeconds">
        /// The <c>RemindersProbeDelaySeconds</c> user-setting value.
        /// </param>
        public RemindersProbeSchedule(int configuredSeconds)
        {
            if (configuredSeconds > 0)
            {
                ShouldDefer = true;
                Delay = TimeSpan.FromSeconds(configuredSeconds);
            }
            else
            {
                ShouldDefer = false;
                Delay = TimeSpan.Zero;
            }
        }

        /// <summary>
        /// <see langword="true"/> when the first <c>OlReminders</c> access should be deferred
        /// (configured seconds strictly greater than zero); otherwise <see langword="false"/>.
        /// </summary>
        public bool ShouldDefer { get; }

        /// <summary>
        /// The resolved deferral interval. Equal to <see cref="TimeSpan.FromSeconds(double)"/>
        /// of the configured seconds when deferring; otherwise <see cref="TimeSpan.Zero"/>.
        /// </summary>
        public TimeSpan Delay { get; }
    }
}
