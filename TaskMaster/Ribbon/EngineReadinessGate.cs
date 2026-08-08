using System;
using UtilitiesCS;

namespace TaskMaster
{
    /// <summary>
    /// The per-engine-key readiness signal for issue #503, computed live from the existing
    /// <see cref="IAppItemEngines.InboxEngines"/> member.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <c>AppItemEngines.InboxEngines</c> is an empty <c>ConcurrentDictionary</c> from field-
    /// initializer time until <c>InitAsync()</c> assigns the fully-built dictionary in a single
    /// terminal reference assignment. A per-key probe is therefore precise and race-free: it never
    /// observes a partially populated map.
    /// </para>
    /// <para>
    /// Readiness is deliberately per-key rather than a coarse "initialization complete" flag.
    /// <c>InitAsync()</c> filters on <c>config.Value.Engine</c> and drops null factory results, so
    /// an engine that is configured off never enters the dictionary; a global flag would report
    /// ready for a command that will never work. A per-key probe also handles
    /// <c>RestartEngineAsync</c> re-assigning a single key, because nothing is cached here.
    /// </para>
    /// <para>
    /// This type is deliberately NOT marked <c>[ExcludeFromCodeCoverage]</c>: it is host-neutral
    /// decision logic with no COM and no <c>Microsoft.Office.*</c> reference, and is fully
    /// unit-tested. It follows the <c>HookReadinessCoordinator</c> precedent.
    /// </para>
    /// </remarks>
    internal sealed class EngineReadinessGate
    {
        private readonly Func<IAppItemEngines> _enginesAccessor;

        /// <summary>
        /// Creates a gate over an engines accessor.
        /// </summary>
        /// <param name="enginesAccessor">
        /// Supplies the current engines container. Must not be null, but is expected to return
        /// null before the ribbon controller has been given its globals, which the gate treats as
        /// "not ready" rather than as an error.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="enginesAccessor"/> is null.
        /// </exception>
        internal EngineReadinessGate(Func<IAppItemEngines> enginesAccessor)
        {
            _enginesAccessor =
                enginesAccessor ?? throw new ArgumentNullException(nameof(enginesAccessor));
        }

        /// <summary>
        /// Reports whether the engine registered under <paramref name="engineName"/> is available.
        /// </summary>
        /// <param name="engineName">
        /// The <c>InboxEngines</c> key. Comparison is the <c>ConcurrentDictionary</c> default —
        /// ordinal and case-sensitive — so <c>"spam"</c> is not <c>"Spam"</c>.
        /// </param>
        /// <returns>
        /// <see langword="true"/> only when the accessor returns a non-null container, its
        /// <c>InboxEngines</c> is non-null, the key is present, and the stored engine is non-null.
        /// A null or whitespace <paramref name="engineName"/> yields <see langword="false"/>.
        /// The result is recomputed on every call and is never cached.
        /// </returns>
        internal bool IsEngineReady(string engineName)
        {
            return TryGetEngine(engineName, out _);
        }

        /// <summary>
        /// Resolves the engine registered under <paramref name="engineName"/>, applying the same
        /// readiness predicate as <see cref="IsEngineReady"/>.
        /// </summary>
        /// <param name="engineName">The <c>InboxEngines</c> key; ordinal, case-sensitive.</param>
        /// <param name="engine">
        /// The registered engine when the gate is open; otherwise <see langword="null"/>.
        /// </param>
        /// <returns><see langword="true"/> when the engine is available.</returns>
        internal bool TryGetEngine(string engineName, out IConditionalEngine<MailItemHelper> engine)
        {
            engine = null!;

            if (string.IsNullOrWhiteSpace(engineName))
            {
                return false;
            }

            var engines = _enginesAccessor();
            var inboxEngines = engines?.InboxEngines;
            if (inboxEngines is null)
            {
                return false;
            }

            if (!inboxEngines.TryGetValue(engineName, out var candidate) || candidate is null)
            {
                return false;
            }

            engine = candidate;
            return true;
        }
    }
}
