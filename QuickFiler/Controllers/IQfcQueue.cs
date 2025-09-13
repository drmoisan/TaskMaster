using Microsoft.Office.Interop.Outlook;
using QuickFiler.Interfaces;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QuickFiler.Controllers
{
    public interface IQfcQueue: INotifyCollectionChanged, INotifyPropertyChanged
    {
        int Count { get; }
        int JobsRunning { get; }
        TlpCellStates TlpStates { get; set; }
        TableLayoutPanel TlpTemplate { get; set; }

        Task ChangeIterationSize((TableLayoutPanel Tlp, List<QfcItemGroup> ItemGroups) entry, int newRowCount, RowStyle rowStyleTemplate);
        Task CompleteAddingAsync(CancellationToken token, int timeout);
        (TableLayoutPanel Tlp, List<QfcItemGroup> ItemGroups) Dequeue();
        Task EnqueueAsync(IList<MailItem> items, IQfcCollectionController qfcCollectionController);
        void GrowEntry(ref (TableLayoutPanel Tlp, List<QfcItemGroup> ItemGroups) target, ref (TableLayoutPanel Tlp, List<QfcItemGroup> ItemGroups) source, int newRowCount, RowStyle rowStyleTemplate);
        Task JobsToFinish(int pollInterval, CancellationToken token);
        Task RemoveItem(MailItem mailItem);
        void RenumberGroups(List<QfcItemGroup> itemGroups);
        Task<(TableLayoutPanel Tlp, List<QfcItemGroup> ItemGroups)> TryDequeueAsync(CancellationToken token, int timeout);
    }
}