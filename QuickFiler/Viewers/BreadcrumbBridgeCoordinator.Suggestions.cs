#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UtilitiesCS;
using UtilitiesCS.OutlookObjects.Folder;

namespace QuickFiler.Viewers
{
    /// <summary>
    /// Issue #501 (SR-1): the suggestion-population surface of the breadcrumb coordinator.
    /// <para>
    /// Held on a third partial-class part so <c>BreadcrumbBridgeCoordinator.cs</c> stays clear of the
    /// repository's 500-line ceiling (<c>.claude/rules/general-code-change.md</c>). The primary file
    /// sat at 487 of 500 lines and the #502 call-site change adds 13 to 17 more, which would have
    /// breached the cap. This is the same split, on the same file, for the same reason as
    /// <c>BreadcrumbBridgeCoordinator.Search.cs</c>.
    /// </para>
    /// </summary>
    public sealed partial class BreadcrumbBridgeCoordinator
    {
        /// <summary>Publishes scored fallbacks immediately, then resolves current hierarchy chains.</summary>
        public void SetSuggestions(IReadOnlyList<FolderRow> rows)
        {
            _ = rows ?? throw new ArgumentNullException(nameof(rows));

            BreadcrumbUpgradeLease lease = _upgradeLifetime.BeginPopulation();
            SetSuggestionsCore(rows, lease);
        }

        /// <summary>
        /// The population body, separated from <see cref="SetSuggestions"/> so a superseded lease can
        /// be injected between <c>BeginPopulation</c> and the guarded run (SR-5).
        /// </summary>
        /// <remarks>
        /// Issue #502 (I-502.2): when <c>RunSynchronous</c> reports that the guarded action was skipped
        /// because <paramref name="lease"/> was already superseded, the previous call's
        /// <see cref="SuggestionsUpgrade"/> task may still be incomplete. Leaving it in place is the
        /// defect, so it is replaced with an already-completed task. <c>Task.CompletedTask</c> is used
        /// rather than <c>Task.FromCanceled</c> because eleven existing tests call
        /// <c>SuggestionsUpgrade.GetAwaiter().GetResult()</c>, which would throw on a cancelled task,
        /// and a completed task matches the property's declared initial value. <c>Abandon</c> settles
        /// the unused lease (I-502.3); it is safe here because it bumps the generation only while the
        /// lease is still current, which a superseded lease is not.
        /// </remarks>
        internal void SetSuggestionsCore(
            IReadOnlyList<FolderRow> rows,
            BreadcrumbUpgradeLease lease
        )
        {
            bool ran = _upgradeLifetime.RunSynchronous(
                lease,
                () =>
                {
                    string renderJson = _router.SetSuggestionFallbacks(rows);
                    BreadcrumbSelectorState selectorState = _router.GetSelectorState();
                    _ = PostRenderAndSelectorAsync(renderJson, selectorState, lease);
                    SuggestionsUpgrade = PopulateSuggestionsAsync(rows, lease);
                }
            );
            if (!ran)
            {
                SuggestionsUpgrade = Task.CompletedTask;
                _upgradeLifetime.Abandon(lease);
            }
        }

        /// <summary>The in-flight ancestor-chain upgrade of the latest <see cref="SetSuggestions"/> call.</summary>
        public Task SuggestionsUpgrade { get; private set; } = Task.CompletedTask;

        private Task PopulateSuggestionsAsync(
            IReadOnlyList<FolderRow> rows,
            BreadcrumbUpgradeLease lease
        ) =>
            _upgradeLifetime.RunAsync(
                lease,
                token => _router.SetSuggestionsAsync(rows, token),
                render => PostRenderAndSelectorAsync(render, _router.GetSelectorState(), lease)
            );

        /// <summary>Appends Path B plain rows verbatim and re-renders (legacy AddRange semantics).</summary>
        /// <remarks>
        /// Issue #502 (I-502.4): a superseded <c>AddItems</c> exposes NO handle to replace — unlike
        /// <see cref="SetSuggestions"/>, its dispatch task is deliberately discarded and nothing on the
        /// public surface reflects it. The skip is therefore observable only through the settled lease,
        /// which is why the <c>false</c> branch calls <c>Abandon</c> and nothing else. The discard is
        /// intentional rather than accidental: recording it here is what distinguishes the two.
        /// </remarks>
        public void AddItems(IReadOnlyList<string> items)
        {
            _ = items ?? throw new ArgumentNullException(nameof(items));
            BreadcrumbUpgradeLease lease = _upgradeLifetime.BeginPopulation();
            bool ran = _upgradeLifetime.RunSynchronous(
                lease,
                () =>
                {
                    string renderJson = _router.AddItems(items);
                    BreadcrumbSelectorState selectorState = _router.GetSelectorState();
                    _ = _upgradeLifetime.RunAsync(
                        lease,
                        _ => PostRenderAndSelectorAsync(renderJson, selectorState, lease)
                    );
                }
            );
            if (!ran)
            {
                _upgradeLifetime.Abandon(lease);
            }
        }
    }
}
