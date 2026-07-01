using System;
using Microsoft.Office.Interop.Outlook;

namespace QuickFiler.Interfaces
{
    /// <summary>
    /// Monitors Outlook <see cref="MailItem"/> objects for moves and invokes a registered
    /// action when a hooked item is moved. All Outlook COM member access performed by
    /// implementations is marshaled to the captured Outlook STA thread, so callers may invoke
    /// these members from any thread (including ThreadPool/background threads) without raising
    /// cross-thread <see cref="System.Runtime.InteropServices.COMException"/>.
    /// </summary>
    internal interface IEmailMoveMonitor
    {
        /// <summary>
        /// Registers <paramref name="mail"/> so that <paramref name="moveAction"/> runs when the
        /// item is moved. Subscribes to the parent folder's BeforeItemMove event for the first
        /// hooked item of that folder. Outlook COM access is marshaled to the captured STA thread.
        /// </summary>
        /// <param name="mail">The mail item to monitor.</param>
        /// <param name="moveAction">The action to invoke when the item is moved.</param>
        void HookItem(MailItem mail, Action<MailItem> moveAction);

        /// <summary>
        /// Removes <paramref name="mail"/> from monitoring. Unsubscribes the parent folder's
        /// BeforeItemMove event only when the removed item was the last hooked item for that
        /// folder. A null argument is a no-op. Outlook COM access is marshaled to the captured
        /// STA thread.
        /// </summary>
        /// <param name="mail">The mail item to stop monitoring; null is ignored.</param>
        void UnhookItem(MailItem mail);

        /// <summary>
        /// Removes all monitored items and unsubscribes every hooked folder's BeforeItemMove
        /// event. Outlook COM access is marshaled to the captured STA thread.
        /// </summary>
        void UnhookAll();
    }
}
