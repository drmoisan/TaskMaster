using System.Collections.Generic;
using Outlook = Microsoft.Office.Interop.Outlook;

namespace TaskMaster
{
    /// <summary>
    /// Partial of <see cref="AppEvents"/> holding the per-store inbox-item-subscribe primitive and
    /// its StoreID-keyed idempotency tracking (issue #263, epic #260). Extracted so that both the
    /// startup <c>PerformReadinessHookup</c> loop and the runtime rehook coordinator share one
    /// implementation of "subscribe one store's inbox <c>ItemAdd</c>", and so a second call for a
    /// StoreID already hooked performs zero additional subscribes. Mirrors the
    /// <c>AppEvents.ReadinessHookup.cs</c> split to keep <c>AppEvents.cs</c> within the file-size
    /// ceiling.
    /// </summary>
    public partial class AppEvents
    {
        /// <summary>
        /// Tracks the inbox <c>Items</c> collection whose <c>ItemAdd</c> handler has been
        /// subscribed, keyed by Outlook <c>StoreID</c>. The presence of a StoreID means that
        /// store's inbox is already hooked; a repeat subscribe for the same StoreID is skipped.
        /// Mutations are performed under <c>lock (OlInboxes)</c> so the presence check and the
        /// <see cref="UtilitiesCS.ReusableTypeClasses.LockingLinkedList{T}.AddLast(T, System.Action{T})"/>
        /// are atomic with respect to each other (<c>LockingLinkedList</c> also locks on its own
        /// instance, and <c>lock</c> is reentrant on the same thread).
        /// </summary>
        private readonly Dictionary<string, Outlook.Items> _hookedInboxItemsByStoreId =
            new Dictionary<string, Outlook.Items>();

        /// <summary>
        /// Idempotently subscribes the <c>ItemAdd</c> handler for one store's inbox, keyed by the
        /// store's <c>StoreID</c>. On the first call for a StoreID the handler is subscribed exactly
        /// once and the StoreID is recorded; a second call for the same StoreID performs zero
        /// additional subscribes. A null store or inbox is a no-op. This is the single per-store
        /// inbox-subscribe implementation reused by startup hookup and runtime rehook.
        /// </summary>
        /// <param name="store">The store whose inbox to subscribe. Supplies the StoreID key.</param>
        /// <param name="inbox">The store's inbox folder; its <c>Items</c> collection is subscribed.</param>
        internal void SubscribeInboxForStore(Outlook.Store store, Outlook.Folder inbox)
        {
            if (store is null || inbox is null)
            {
                return;
            }

            var storeId = store.StoreID;
            var inboxes = OlInboxes;

            lock (inboxes)
            {
                if (storeId != null && _hookedInboxItemsByStoreId.ContainsKey(storeId))
                {
                    // Already hooked: zero additional subscribes (closes the double-subscribe risk).
                    return;
                }

                var items = inbox.Items;
                inboxes.AddLast(items, i => i.ItemAdd += OlInboxItems_ItemAdd);

                if (storeId != null)
                {
                    _hookedInboxItemsByStoreId[storeId] = items;
                }
            }
        }

        /// <summary>
        /// Returns <c>true</c> when the inbox for <paramref name="storeId"/> has already been
        /// subscribed via <see cref="SubscribeInboxForStore"/>. A pure, non-COM predicate over the
        /// idempotency tracker, used by the rehook coordinator's already-hooked check.
        /// </summary>
        /// <param name="storeId">The Outlook <c>StoreID</c> to test.</param>
        internal bool IsInboxHooked(string storeId)
        {
            if (storeId is null)
            {
                return false;
            }

            var inboxes = OlInboxes;
            lock (inboxes)
            {
                return _hookedInboxItemsByStoreId.ContainsKey(storeId);
            }
        }
    }
}
