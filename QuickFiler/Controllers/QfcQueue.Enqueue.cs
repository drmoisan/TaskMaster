using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Office.Interop.Outlook;
using QuickFiler.Interfaces;
using UtilitiesCS;

namespace QuickFiler.Controllers
{
    /// <summary>
    /// Background enqueue path for <see cref="QfcQueue"/>: leg B of the high-confidence display
    /// path, which builds every page after the first. This part exists because the base file
    /// <c>QfcQueue.cs</c> stood at 610 lines, already past the 500-line limit, and issue #678 adds a
    /// parameter to both members below. Each gains its parameter or argument on its own line under
    /// CSharpier, and the <c>new QfcItemController(</c> construction sits inside a lambda in
    /// <see cref="LoadControllersViewersAsync"/>'s body, so it is not a relocatable unit on its own
    /// and the whole enclosing member had to move. The primary constructor stays on the base part.
    /// </summary>
    public partial class QfcQueue
    {
        /// <summary>
        /// Issue #678 injectable-delegate seam (form 2 of <c>.claude/rules/csharp.md</c>, mirroring
        /// the existing <c>QfcDatamodel.ScoringServiceFactory</c> pattern) for the per-row item
        /// controller this queue constructs. The production default reproduces the previous
        /// construction expression exactly, argument for argument, with the carried folder search
        /// handler appended; tests assign a factory that captures its arguments so the carry can be
        /// asserted without a live WinForms viewer or Outlook COM. No new interface is introduced.
        /// </summary>
        internal Func<
            IApplicationGlobals,
            IFilerHomeController,
            IQfcCollectionController,
            QfcItemGroup,
            int,
            int,
            TlpCellStates,
            IFolderSearchHandler,
            IQfcItemController
        > ItemControllerFactory { get; set; } =
            (globals, home, parent, grp, viewerPosition, digits, tlpStates, carriedHandler) =>
                new QfcItemController(
                    appGlobals: globals,
                    homeController: home,
                    parent: parent,
                    itemViewer: grp.ItemViewer,
                    viewerPosition: viewerPosition,
                    itemNumberDigits: digits,
                    grp.MailItem,
                    tlpStates,
                    carriedFolderHandler: carriedHandler
                );

        /// <summary>
        /// Enqueues a background page. <paramref name="preScored"/> carries the folder search
        /// handler the dequeue-time confidence gate already initialised for each accepted item
        /// (issue #678); it is null or empty outside high-confidence mode, in which case every row
        /// is constructed exactly as before and the item controller performs its own scoring pass.
        /// Carriers are matched to items by <c>EntryID</c> rather than by position, because
        /// <c>UnhookDequeuedNodes</c> can replace an element of the item list in place.
        /// The parameter is required rather than optional so that a Moq setup or verification can
        /// name it in an expression tree, which C# forbids for an omitted optional argument.
        /// </summary>
        public async Task EnqueueAsync(
            IList<MailItem> items,
            IQfcCollectionController qfcCollectionController,
            IList<QfcPreScoredItem> preScored
        )
        {
            //TraceUtility.LogMethodCall(items, qfcCollectionController);

            if (items is null)
            {
                throw new ArgumentNullException(nameof(items));
            }
            if (items.Count == 0)
            {
                throw new ArgumentException("items is empty");
            }

            _qfcCollectionController = qfcCollectionController;

            await Task.Run(() =>
                items.ForEach(item => _moveMonitor.HookItem(item, async (x) => await RemoveItem(x)))
            );

            Interlocked.Increment(ref _jobsRunning);
            //logger.Debug($"{nameof(EnqueueAsync)} called and jobsRunning increased to {_jobsRunning}");

            var tlp = await UiIdleCallAsync(() =>
                _tlpTemplate.Clone(name: "BackgroundTableLayout")
            );

            //ActivateTlpTemplate(tlp);

            try
            {
                var itemGroups = await UiIdleAsyncCallAsync(async () =>
                    await LoadControllersViewersAsync(
                        items,
                        _globals,
                        _homeController,
                        qfcCollectionController,
                        tlp,
                        0,
                        preScored
                    )
                );
                _queue.Add((tlp, itemGroups));
            }
            catch (OperationCanceledException)
            {
                //logger.Debug($"{nameof(EnqueueAsync)} was canceled by the user");
            }
            catch (System.Exception e)
            {
                logger.Error(
                    $"{nameof(EnqueueAsync)} failed to load controllers and viewers. \n {e.Message}\n{e.StackTrace}"
                );
            }
            finally
            {
                Interlocked.Decrement(ref _jobsRunning);
                //logger.Debug($"{nameof(EnqueueAsync)} completed and jobsRunning decreased to {_jobsRunning}");

                CollectionChanged?.Invoke(
                    this,
                    new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, _queue)
                );
            }
        }

        /// <summary>
        /// Resolves the folder search handler carried for <paramref name="mailItem"/>, or null when
        /// no carrier list was supplied or none of its entries matches. Matching is by
        /// <c>EntryID</c>: a null or empty carrier list, a null mail item, and a mail item absent
        /// from the list all yield null, which is the pre-#678 behaviour for every row.
        /// </summary>
        internal static IFolderSearchHandler ResolveCarriedHandler(
            IList<QfcPreScoredItem> preScored,
            MailItem mailItem
        )
        {
            if (preScored is null || preScored.Count == 0 || mailItem is null)
            {
                return null;
            }

            string entryId = mailItem.EntryID;
            if (string.IsNullOrEmpty(entryId))
            {
                return null;
            }

            foreach (var carrier in preScored)
            {
                if (carrier.MailItem is not null && carrier.MailItem.EntryID == entryId)
                {
                    return carrier.FolderHandler;
                }
            }

            return null;
        }

        private ValueTask<List<QfcItemGroup>> LoadControllersViewersAsync(
            IList<MailItem> items,
            IApplicationGlobals appGlobals,
            IFilerHomeController homeController,
            IQfcCollectionController qfcCollectionController,
            TableLayoutPanel tlp,
            int start,
            IList<QfcPreScoredItem> preScored = null
        )
        {
            //TraceUtility.LogMethodCall(items, appGlobals, homeController, qfcCollectionController, tlp, start);

            var digits = start + items.Count >= 10 ? 2 : 1;

            // SelectAwait (System.Linq.Async) is obsolete (CS0618) per the framework's migration
            // guidance ("Use Select... the SelectAwait functionality now exists as overloads of
            // Select"), but migrating to the new overload signature is a call-shape change to
            // production code, not an annotation-only edit. Suppressing narrowly preserves the
            // exact pre-existing behavior (no behavior change per AC7).
#pragma warning disable CS0618
            var itemTasks = Enumerable
                .Range(start, items.Count)
                .ToAsyncEnumerable()
                .SelectAwait(async i => (i: i, grp: await AddAsync(tlp, items[i - start], i)))
                .SelectAwait(async x =>
                {
                    x.grp.CarriedFolderHandler = ResolveCarriedHandler(preScored, x.grp.MailItem);
                    x.grp.ItemController = ItemControllerFactory(
                        appGlobals,
                        homeController,
                        qfcCollectionController,
                        x.grp,
                        x.i + 1,
                        digits,
                        TlpStates,
                        x.grp.CarriedFolderHandler
                    );
                    await x.grp.ItemController.InitializeAsync();
                    return x.grp;
                })
                .ToListAsync();
#pragma warning restore CS0618
            return itemTasks;
        }
    }
}
