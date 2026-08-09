using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace TaskMaster
{
    /// <summary>
    /// The single source of truth binding an Explorer-ribbon control id to the
    /// <c>InboxEngines</c> key of the engine that backs it.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This binding is shared by four consumers: the <c>getEnabled</c> attribute wiring in
    /// <c>RibbonExplorer.xml</c>, the <c>EngineCommand_GetEnabled</c> callback, the click guards
    /// in <see cref="EngineGatedCommandRunner"/>, and the post-initialization refresh planned by
    /// <see cref="EngineCommandRefreshPlanner"/>. Centralizing it here is what allows a single
    /// unit test to assert that the XML and the code agree.
    /// </para>
    /// <para>
    /// This type is deliberately NOT marked <c>[ExcludeFromCodeCoverage]</c>: it is host-neutral
    /// decision logic, contains no COM and no <c>Microsoft.Office.*</c> reference, and is fully
    /// unit-tested. It follows the <c>HookReadinessCoordinator</c> precedent.
    /// </para>
    /// <para>
    /// The map is intentionally extensible: adding a future <c>Project</c>, <c>Context</c>, or
    /// <c>Actionable</c> ribbon command is a single entry in <see cref="Map"/>.
    /// </para>
    /// </remarks>
    internal static class EngineCommandCatalog
    {
        /// <summary>
        /// Control id to engine key. Ordinal comparison, matching the
        /// <see cref="System.Collections.Concurrent.ConcurrentDictionary{TKey, TValue}"/> default
        /// used by <c>AppItemEngines.InboxEngines</c>.
        /// </summary>
        private static readonly IReadOnlyDictionary<string, string> Map = new Dictionary<
            string,
            string
        >(StringComparer.Ordinal)
        {
            ["TrainSpam"] = "Spam",
            ["TrainHam"] = "Spam",
            ["TestSpam"] = "Spam",
            ["TriageSetA"] = "Triage",
            ["TriageSetB"] = "Triage",
            ["TriageSetC"] = "Triage",
            ["ClearTriage"] = "Triage",
            ["FilterTriageGroup"] = "Triage",
            // Issue #518: the six save/info commands. Their engine work reduces to an
            // InboxEngines lookup that no-ops when the key is absent, so engine readiness is the
            // semantically exact predicate for them — unlike the two activation toggles, which are
            // backed by configuration and live in EngineToggleCatalog instead. Note the id and
            // handler names diverge for the "current location" buttons: the control ids are
            // GetSaveState / TriageGetSaveState while their onAction handlers remain
            // GetSaveLocation_Click / TriageGetSaveLocation_Click, as pinned by the ribbon XML.
            ["SpamSaveNetwork"] = "Spam",
            ["SpamSaveLocal"] = "Spam",
            ["GetSaveState"] = "Spam",
            ["TriageSaveNetwork"] = "Triage",
            ["TriageSaveLocal"] = "Triage",
            ["TriageGetSaveState"] = "Triage",
        };

        private static readonly IReadOnlyCollection<string> ControlIdList =
            new ReadOnlyCollection<string>(new List<string>(Map.Keys));

        /// <summary>
        /// The engine-backed Explorer-ribbon control ids, without duplicates.
        /// </summary>
        internal static IReadOnlyCollection<string> ControlIds => ControlIdList;

        /// <summary>
        /// Resolves the <c>InboxEngines</c> key that backs the supplied ribbon control id.
        /// </summary>
        /// <param name="controlId">
        /// The ribbon control id, compared ordinally. May be null or empty.
        /// </param>
        /// <param name="engineName">
        /// The engine key when the lookup succeeds; otherwise <see langword="null"/>.
        /// </param>
        /// <returns>
        /// <see langword="true"/> when <paramref name="controlId"/> is an engine-backed control id;
        /// <see langword="false"/> for a null, empty, or unrecognized id. A control this catalog
        /// does not own must never be disabled by the readiness wiring, so the false result is the
        /// safe default only in combination with the caller's own semantics.
        /// </returns>
        internal static bool TryGetEngineName(string controlId, out string engineName)
        {
            if (string.IsNullOrEmpty(controlId))
            {
                // null-forgiving: the pinned signature uses a non-nullable `out string`, and the
                // documented contract is that callers must not read the out value when the method
                // returns false. This keeps the file clean under /p:Nullable=enable.
                engineName = null!;
                return false;
            }

            return Map.TryGetValue(controlId, out engineName);
        }
    }
}
