using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Threading;
using Microsoft.Office.Interop.Outlook;
using Microsoft.Web.WebView2.Core;
using QuickFiler.Helper_Classes;
using QuickFiler.Interfaces;
using QuickFiler.Viewers;
using TaskVisualization;
using ToDoModel;
using UtilitiesCS;
using UtilitiesCS.EmailIntelligence.EmailParsingSorting;
using UtilitiesCS.Extensions;

namespace QuickFiler.Controllers
{
    internal partial class QfcItemController
    {
        /// <summary>
        /// Gets the Outlook.Conversation from the underlying MailItem
        /// embedded in the class. Conversation details are loaded to
        /// a Dataframe. Count is inferred from the df row count
        /// </summary>
        public void PopulateConversation()
        {
            ConversationResolver = _conversationResolverFactory(Mail);

            PopulateConversation(ConversationResolver.Count.SameFolder);
            //PopulateConversation(_mailItem.GetConversationDf());
        }

        public void PopulateConversation(ConversationResolver resolver)
        {
            ConversationResolver = resolver;
            PopulateConversation(ConversationResolver.Count.SameFolder);
        }

        public async Task LoadConversationResolverAsync(
            CancellationTokenSource tokenSource,
            CancellationToken token,
            bool loadAll
        )
        {
            //TraceUtility.LogMethodCall(tokenSource, token, loadAll);
            token.ThrowIfCancellationRequested();

            try
            {
                ConversationResolver = await DoLoadConversationResolverCoreAsync(
                    tokenSource,
                    token,
                    loadAll
                );
            }
            catch (OperationCanceledException)
            {
                // Cancellation is an expected flow; propagate so callers can observe it.
                throw;
            }
            catch (System.Exception e)
            {
                logger.Error($"Error in PopulateConversationAsync: {e.Message}", e);
                logger.Debug($"Skipping Populate Conversation");
            }
        }

        /// <summary>
        /// Seam for the static ConversationResolver.LoadAsync call. Override in tests to
        /// inject controlled behaviour without requiring WinForms infrastructure.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
        protected virtual Task<ConversationResolver> DoLoadConversationResolverCoreAsync(
            CancellationTokenSource tokenSource,
            CancellationToken token,
            bool loadAll
        ) =>
            ConversationResolver.LoadAsync(
                _globals,
                ItemHelper,
                tokenSource,
                token,
                loadAll,
                SetTopicThread
            );

        public async Task PopulateConversationAsync(
            CancellationTokenSource tokenSource,
            CancellationToken token,
            bool loadAll
        )
        {
            await LoadConversationResolverAsync(tokenSource, token, loadAll);
            token.ThrowIfCancellationRequested();
            if (ConversationResolver is null)
                return;
            await RenderConversationCountAsync(
                ConversationResolver.Count.SameFolder,
                token,
                loadAll
            );

            if (!loadAll)
            {
                // Issue #255: in the deferred (loadAll == false) path, ConversationResolver.LoadAsync
                // does not run LoadConversationInfoAsync, and the deferred Df-change handler cannot
                // fire (Df is assigned inside LoadDfAsync before the PropertyChanged handler is
                // subscribed), so the resolver never publishes the conversation to the fast list.
                // Publish it here so the TopicThread is populated instead of rendering
                // "The fast list is empty". The genuinely-empty case is preserved:
                // ConversationResolver.LoadConversationInfo returns a single-item fallback (the
                // current mail item) when Count.Expanded <= 0 (e.g. the Junk E-mail path).
                token.ThrowIfCancellationRequested();
                SetTopicThread(ConversationResolver.ConversationInfo.Expanded);
            }
        }

        public async Task PopulateConversationAsync(
            ConversationResolver resolver,
            CancellationToken token,
            bool loadAll
        )
        {
            token.ThrowIfCancellationRequested();

            ConversationResolver = resolver;
            await RenderConversationCountAsync(
                ConversationResolver.Count.SameFolder,
                token,
                loadAll
            );
        }

        /// <summary>
        /// TBD if this overload will be of use. Depends on whether _dfConversation
        /// is needed by any individual element when expanded
        /// </summary>
        /// <param name="df"></param>
        //public void PopulateConversation(DataFrame df)
        //{
        //    DfConversationExpanded = df.FilterConversation(((Folder)Mail.Parent).FolderPath, false, true);
        //    DfConversation = DfConversationExpanded.FilterConversation(((Folder)Mail.Parent).Name, true, true);
        //    int count = DfConversation.Rows.Count();
        //    PopulateConversation(count);
        //}

        /// <summary>
        /// Sets the conversation count of the visual without altering the
        /// _dfConversation. Useful when expanding or collapsing the
        /// conversation to show how many items will be moved
        /// </summary>
        /// <param name="count"></param>
        public void PopulateConversation(int count)
        {
            //_itemViewer.LblConvCt.BeginInvoke(new System.Action(() =>
            _uiDispatcher.BeginInvoke(() =>
            {
                _itemViewer.ConversationCountText = count.ToString();
                if (count == 0)
                {
                    _itemViewer.ConversationCountBackColor = Color.Red;
                }
            });
        }

        public void RenderConversationCount()
        {
            int count = ConversationResolver?.Count.SameFolder ?? 0;
            RenderConversationCount(count);
        }

        public void RenderConversationCount(int count)
        {
            if (_itemViewer.InvokeRequired)
            {
                _itemViewer.Invoke(() => RenderConversationCount(count));
                return;
            }

            _itemViewer.ConversationCountText = count.ToString();
            if (count == 0)
            {
                _itemViewer.ConversationCountBackColor = Color.Red;
            }
        }

        public async Task RenderConversationCountAsync(
            int count,
            CancellationToken token,
            bool backgroundLoad
        )
        {
            //TraceUtility.LogMethodCall(count, token, backgroundLoad);
            token.ThrowIfCancellationRequested();

            DispatcherPriority priority = backgroundLoad
                ? DispatcherPriority.Background
                : DispatcherPriority.Normal;

            await _uiDispatcher.InvokeAsync(
                () =>
                {
                    _itemViewer.ConversationCountText = count.ToString();
                    if (count == 0)
                    {
                        _itemViewer.ConversationCountBackColor = Color.Red;
                    }
                },
                priority,
                token
            );
        }

        public void SetTopicThread(List<MailItemHelper> conversationInfo)
        {
            // Run on the UI Thread if necessary
            if (_itemViewer.InvokeRequired)
            {
                _itemViewer.Invoke(() => SetTopicThread(conversationInfo));
                return;
            }

            // Set the TopicThread to the ConversationInfo list
            _itemViewer.SetConversationItems(conversationInfo);
            _itemViewer.SortConversationByDate(SortOrder.Descending);
        }
    }
}
