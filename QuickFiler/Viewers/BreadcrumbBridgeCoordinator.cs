#nullable enable
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UtilitiesCS;
using UtilitiesCS.OutlookObjects.Folder;

namespace QuickFiler.Viewers
{
    /// <summary>
    /// Host-neutral coordinator for the QuickFiler breadcrumb (#351 P4-T4, NOT coverage-exempt):
    /// wires <see cref="IWebViewMessenger.MessageReceived"/> -&gt;
    /// <see cref="BreadcrumbBridgeRouter"/> -&gt; <see cref="IWebViewMessenger.PostJson"/>, exposes
    /// the population/selection surface the viewer glue delegates to, and raises the .NET events
    /// (<see cref="SelectionChanged"/>, <see cref="UnhandledArrow"/>,
    /// <see cref="FolderArrowKeyDown"/>) that preserve the existing controller seams. The
    /// synthetic <see cref="FolderArrowKeyDown"/> carries the arrow direction; the
    /// coverage-exempt viewer partial adapts it to the WinForms <c>KeyEventHandler</c> shape of
    /// <c>IItemViewer.FolderKeyDown</c>, keeping this type free of WinForms/WebView2/COM usage.
    /// Router responses are awaited directly — no timers.
    /// </summary>
    public sealed class BreadcrumbBridgeCoordinator
    {
        private readonly IWebViewMessenger _messenger;
        private readonly BreadcrumbBridgeRouter _router;

        /// <summary>
        /// Creates the coordinator and subscribes to inbound page messages.
        /// </summary>
        /// <param name="messenger">The post-init WebView2 messaging seam. Required.</param>
        /// <param name="provider">The merged 9101 hierarchy provider. Required.</param>
        /// <exception cref="ArgumentNullException">Either argument is null.</exception>
        public BreadcrumbBridgeCoordinator(
            IWebViewMessenger messenger,
            IFolderHierarchyProvider provider
        )
        {
            _messenger = messenger ?? throw new ArgumentNullException(nameof(messenger));
            _router = new BreadcrumbBridgeRouter(provider);
            _messenger.MessageReceived += OnMessageReceived;
        }

        /// <summary>Raised when the page selection changes (backs <c>FolderSelectionChanged</c>).</summary>
        public event EventHandler? SelectionChanged;

        /// <summary>Raised when an arrow was not consumed, for the legacy fall-through (FR-6).</summary>
        public event EventHandler<BreadcrumbArrowDirection>? UnhandledArrow;

        /// <summary>Synthetic key event for every inbound arrow message (handled or not).</summary>
        public event EventHandler<BreadcrumbArrowDirection>? FolderArrowKeyDown;

        /// <summary>The dispatch task of the most recent inbound message (awaitable by tests/glue).</summary>
        public Task LastDispatch { get; private set; } = Task.CompletedTask;

        /// <summary>
        /// Populates Path A suggestion rows through the router/provider and posts the render
        /// payload to the page (FR-1/FR-4).
        /// </summary>
        public async Task SetSuggestionsAsync(
            IReadOnlyList<FolderRow> rows,
            CancellationToken cancellationToken
        )
        {
            var renderJson = await _router
                .SetSuggestionsAsync(rows, cancellationToken)
                .ConfigureAwait(false);
            _messenger.PostJson(renderJson);
        }

        /// <summary>
        /// Synchronous population facade for the void <c>IItemViewer.SetFolderSuggestions</c>
        /// contract: rows are populated immediately as plain full-path rows so the selection
        /// contract (FolderContains/SetFolderSelectedItem/GetSelectedFolder readback) holds
        /// without awaiting the provider, then the ancestor-chain upgrade runs asynchronously
        /// (<see cref="SuggestionsUpgrade"/>) preserving the selected index (FR-1/G10).
        /// </summary>
        public void SetSuggestions(IReadOnlyList<FolderRow> rows)
        {
            if (rows == null)
            {
                throw new ArgumentNullException(nameof(rows));
            }

            var immediate = new string[rows.Count];
            for (int i = 0; i < rows.Count; i++)
            {
                immediate[i] = rows[i].Score.HasValue
                    ? rows[i].Score.Value.FolderPath
                    : rows[i].Text;
            }
            _messenger.PostJson(_router.SetItems(immediate));
            SuggestionsUpgrade = UpgradeSuggestionsAsync(rows);
        }

        /// <summary>The in-flight ancestor-chain upgrade of the latest <see cref="SetSuggestions"/> call.</summary>
        public Task SuggestionsUpgrade { get; private set; } = Task.CompletedTask;

        private async Task UpgradeSuggestionsAsync(IReadOnlyList<FolderRow> rows)
        {
            int selected = _router.Model.SelectedIndex;
            var renderJson = await _router
                .SetSuggestionsAsync(rows, CancellationToken.None)
                .ConfigureAwait(false);
            if (selected >= 0 && selected < _router.Model.Rows.Count)
            {
                // Row order and count are preserved by the rebuild, so index selection carries over.
                renderJson = _router.SelectRow(selected);
            }
            _messenger.PostJson(renderJson);
        }

        /// <summary>Appends Path B plain rows verbatim and re-renders (legacy AddRange semantics).</summary>
        public void AddItems(IReadOnlyList<string> items)
        {
            _messenger.PostJson(_router.AddItems(items));
        }

        /// <summary>Clears all rows and the selection, emptying the page (backs <c>ClearFolderItems</c>).</summary>
        public void Clear()
        {
            _messenger.PostJson(_router.Clear());
        }

        /// <summary>Selects the row at <paramref name="index"/> and re-renders (backs <c>SetFolderSelectedIndex</c>).</summary>
        public void SelectRow(int index)
        {
            _messenger.PostJson(_router.SelectRow(index));
            SelectionChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Selects the first row whose output string equals <paramref name="item"/> (backs
        /// <c>SetFolderSelectedItem</c>); unknown items are a no-op per the legacy contract.
        /// </summary>
        public void SelectItem(string item)
        {
            if (BreadcrumbSelectionMap.TrySelectItem(_router.Model, item))
            {
                _messenger.PostJson(_router.RenderJson());
                SelectionChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>The selection output string (backs <c>GetSelectedFolder</c>; FR-7/G10).</summary>
        public string? GetSelectedFolder()
        {
            return BreadcrumbSelectionMap.GetSelectedFolder(_router.Model);
        }

        /// <summary>The per-row output strings (backs <c>GetFolderItems</c>).</summary>
        public string[] GetFolderItems()
        {
            return BreadcrumbSelectionMap.GetFolderItems(_router.Model);
        }

        /// <summary>True when a row's output string equals <paramref name="item"/> (backs <c>FolderContains</c>).</summary>
        public bool Contains(string item)
        {
            return BreadcrumbSelectionMap.FolderContains(_router.Model, item);
        }

        /// <summary>Posts a theme switch to the page ("dark"/"light"; FR-5 theming).</summary>
        public void SetTheme(string theme)
        {
            _messenger.PostJson(
                BreadcrumbBridgeSerializer.Serialize(new ThemeChangeMessage(theme))
            );
        }

        private async void OnMessageReceived(object? sender, string json)
        {
            // Event-handler boundary: the dispatch task is tracked so callers/tests can observe
            // completion; RouteAsync converts all routing/provider failures into explicit error
            // responses, so this await only propagates cancellation.
            var dispatch = DispatchAsync(json);
            LastDispatch = dispatch;
            await dispatch.ConfigureAwait(false);
        }

        private async Task DispatchAsync(string json)
        {
            RaiseSyntheticArrowKey(json);
            var outputs = await _router
                .RouteAsync(json, CancellationToken.None)
                .ConfigureAwait(false);
            foreach (var output in outputs)
            {
                var message = BreadcrumbBridgeSerializer.Parse(output);
                if (message is UnhandledArrowMessage unhandled)
                {
                    // Not posted back to the page: the host owns the legacy fall-through (FR-6).
                    UnhandledArrow?.Invoke(this, unhandled.Direction);
                    continue;
                }
                _messenger.PostJson(output);
                if (message is SelectionChangeMessage)
                {
                    SelectionChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        private void RaiseSyntheticArrowKey(string json)
        {
            BreadcrumbBridgeMessage message;
            try
            {
                message = BreadcrumbBridgeSerializer.Parse(json);
            }
            catch (FormatException)
            {
                // Malformed inbound JSON is routed anyway and surfaces as an error response.
                return;
            }

            if (message is ArrowKeyMessage arrow)
            {
                FolderArrowKeyDown?.Invoke(this, arrow.Direction);
            }
            else if (message is UnhandledArrowMessage report)
            {
                FolderArrowKeyDown?.Invoke(this, report.Direction);
            }
        }
    }
}
