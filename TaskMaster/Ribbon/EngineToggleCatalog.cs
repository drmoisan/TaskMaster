using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace TaskMaster
{
    /// <summary>
    /// The single source of truth binding an engine key to the id of the Explorer-ribbon
    /// <c>checkBox</c> that displays and flips that engine's activation setting.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This map is deliberately kept <b>separate from</b> <see cref="EngineCommandCatalog"/>, and
    /// the two toggle checkboxes must never join that catalog. Two independent reasons:
    /// </para>
    /// <para>
    /// First, semantics. <c>EngineCommandCatalog</c> membership means "gated on engine readiness",
    /// and readiness is computed from <c>InboxEngines</c>. An engine that is configured off never
    /// enters <c>InboxEngines</c>, so a readiness-gated toggle could never be used to re-enable a
    /// disabled engine — the gate would be permanently closed on exactly the control that exists
    /// to open it. Toggle state is backed by engine <em>configuration</em>, not by readiness.
    /// </para>
    /// <para>
    /// Second, schema. Catalog membership drives the <c>getEnabled</c> set-equality assertions and
    /// the "every catalog id resolves to a <c>button</c> element" assertion in
    /// <c>RibbonExplorerXmlTests</c>. The toggles are <c>checkBox</c> elements, so adding them to
    /// the command catalog would fail that test by construction.
    /// </para>
    /// <para>
    /// This type is deliberately NOT marked <c>[ExcludeFromCodeCoverage]</c>: it is host-neutral
    /// data with no COM and no <c>Microsoft.Office.*</c> reference, and is fully unit-tested. It
    /// follows the <see cref="EngineCommandCatalog"/> precedent.
    /// </para>
    /// </remarks>
    internal static class EngineToggleCatalog
    {
        /// <summary>
        /// Engine key to toggle control id. Ordinal comparison, matching the
        /// <see cref="System.Collections.Concurrent.ConcurrentDictionary{TKey, TValue}"/> default
        /// used by <c>AppItemEngines.InboxEngines</c>, so <c>"spam"</c> is not <c>"Spam"</c>.
        /// </summary>
        /// <remarks>
        /// The <c>"Spam"</c> key is <c>SpamBayes.GroupName</c>; the <c>"Triage"</c> key is the
        /// literal used by the Triage engine's own registration.
        /// </remarks>
        private static readonly IReadOnlyDictionary<string, string> Map = new Dictionary<
            string,
            string
        >(StringComparer.Ordinal)
        {
            ["Spam"] = "SpamBayesEnabledToggle",
            ["Triage"] = "TriageEnabledToggle",
        };

        private static readonly IReadOnlyCollection<string> EngineNameList =
            new ReadOnlyCollection<string>(new List<string>(Map.Keys));

        /// <summary>
        /// The engine keys that have a toggle checkbox, without duplicates.
        /// </summary>
        internal static IReadOnlyCollection<string> EngineNames => EngineNameList;

        /// <summary>
        /// Resolves the ribbon control id of the toggle checkbox that displays the supplied
        /// engine's activation state.
        /// </summary>
        /// <param name="engineName">
        /// The engine key, compared ordinally. May be null, empty, or unrecognized.
        /// </param>
        /// <param name="controlId">
        /// The toggle control id when the lookup succeeds; otherwise <see langword="null"/>.
        /// </param>
        /// <returns>
        /// <see langword="true"/> when <paramref name="engineName"/> has a toggle checkbox;
        /// <see langword="false"/> for a null, empty, or unrecognized key. Callers must not read
        /// <paramref name="controlId"/> when the result is <see langword="false"/>.
        /// </returns>
        internal static bool TryGetControlId(string engineName, out string controlId)
        {
            if (string.IsNullOrEmpty(engineName))
            {
                // null-forgiving: the pinned signature uses a non-nullable `out string`, and the
                // documented contract is that callers must not read the out value when the method
                // returns false. This matches EngineCommandCatalog.TryGetEngineName.
                controlId = null!;
                return false;
            }

            return Map.TryGetValue(engineName, out controlId);
        }
    }
}
