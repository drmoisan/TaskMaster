using Microsoft.Data.Analysis;
using Microsoft.Office.Interop.Outlook;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using UtilitiesCS;

namespace QuickFiler.Helper_Classes
{
    public interface IConversationResolver
    {
        Pair<List<MailItemInfo>> ConversationInfo { get; set; }
        Pair<IList<MailItem>> ConversationItems { get; set; }
        Pair<int> Count { get; }
        Pair<DataFrame> Df { get; }
        Action<List<MailItemInfo>> UpdateUI { get; set; }

        event PropertyChangedEventHandler PropertyChanged;

        Task BackgroundInitInfoItemsAsync(CancellationToken token);
        void Handler_PropertyChanged(object sender, PropertyChangedEventArgs e);
        Task<Pair<List<MailItemInfo>>> LoadConversationInfoAsync(CancellationToken token, bool backgroundLoad);
        Task LoadConversationItemsAsync(CancellationToken token, bool backgroundLoad);
        Task LoadDfAsync(CancellationToken token, bool backgroundLoad);
    }
}