using System;
using System.ComponentModel;
using System.Configuration;
using System.Threading;

namespace TaskMaster.Test.AppGlobals
{
    /// <summary>
    /// Test-only probe that detects whether an action persists an
    /// <see cref="ApplicationSettingsBase"/> instance on the calling test thread.
    /// </summary>
    internal static class SettingsSaveProbe
    {
        /// <summary>
        /// Runs <paramref name="act"/> and counts the <see cref="ApplicationSettingsBase.SettingsSaving"/>
        /// events that <paramref name="settings"/> raises on the calling thread while it runs.
        /// </summary>
        /// <param name="settings">The settings instance whose saves are observed.</param>
        /// <param name="act">The action under observation.</param>
        /// <returns>The number of saves attempted on the calling thread.</returns>
        /// <remarks>
        /// The handler is thread-attributed: a save raised by another thread (for example a test of
        /// another class running in parallel against the same settings instance) is neither counted
        /// nor cancelled. Every counted save is cancelled through <see cref="CancelEventArgs.Cancel"/>,
        /// so <see cref="ApplicationSettingsBase.Save"/> skips the provider and nothing is written to
        /// the per-host settings file. The handler is always removed in a <c>finally</c> block.
        /// </remarks>
        internal static int CountSavesOnCurrentThread(ApplicationSettingsBase settings, Action act)
        {
            if (settings is null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            if (act is null)
            {
                throw new ArgumentNullException(nameof(act));
            }

            var threadId = Environment.CurrentManagedThreadId;
            var count = 0;
            SettingsSavingEventHandler handler = (sender, e) =>
            {
                if (Environment.CurrentManagedThreadId == threadId)
                {
                    Interlocked.Increment(ref count);
                    e.Cancel = true;
                }
            };

            // SettingsSaving uses explicit add and remove accessors over a plain delegate field,
            // which are not atomic; locking on the settings instance keeps two concurrent probes
            // from losing a subscription. Only the subscribe and unsubscribe statements are locked,
            // so the observed action itself never runs serialized.
            lock (settings)
            {
                settings.SettingsSaving += handler;
            }

            try
            {
                act();
            }
            finally
            {
                lock (settings)
                {
                    settings.SettingsSaving -= handler;
                }
            }

            return Volatile.Read(ref count);
        }
    }
}
