using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Office.Interop.Outlook;
using QuickFiler.Interfaces;

namespace QuickFiler.Controllers
{
    public interface IQfcQueue : INotifyCollectionChanged, INotifyPropertyChanged
    {
        int Count { get; }
        int JobsRunning { get; }
        TlpCellStates TlpStates { get; set; }
        TableLayoutPanel TlpTemplate { get; set; }

        Task ChangeIterationSize(
            (TableLayoutPanel Tlp, List<QfcItemGroup> ItemGroups) entry,
            int newRowCount,
            RowStyle rowStyleTemplate
        );
        Task CompleteAddingAsync(CancellationToken token, int timeout);
        (TableLayoutPanel Tlp, List<QfcItemGroup> ItemGroups) Dequeue();

        /// <summary>
        /// Enqueues a background page. Issue #678 adds <paramref name="preScored"/>, the carriers
        /// holding the folder search handler the dequeue-time gate already initialised for each
        /// accepted item. It is a required parameter rather than optional because an optional
        /// parameter cannot be omitted inside a Moq setup or verification expression tree (CS0854);
        /// callers outside high-confidence mode pass <see langword="null"/>.
        /// </summary>
        Task EnqueueAsync(
            IList<MailItem> items,
            IQfcCollectionController qfcCollectionController,
            IList<QfcPreScoredItem> preScored
        );
        void GrowEntry(
            ref (TableLayoutPanel Tlp, List<QfcItemGroup> ItemGroups) target,
            ref (TableLayoutPanel Tlp, List<QfcItemGroup> ItemGroups) source,
            int newRowCount,
            RowStyle rowStyleTemplate
        );
        Task JobsToFinish(int pollInterval, CancellationToken token);
        Task RemoveItem(MailItem mailItem);
        void RenumberGroups(List<QfcItemGroup> itemGroups);
        Task<(TableLayoutPanel Tlp, List<QfcItemGroup> ItemGroups)> TryDequeueAsync(
            CancellationToken token,
            int timeout
        );
    }
}
