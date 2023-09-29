using Microsoft.Data.Analysis;
using Microsoft.Office.Interop.Outlook;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace QuickFiler.Helper_Classes
{
    public interface IConversationResolver
    {
        (List<MailItemInfo> SameFolder, List<MailItemInfo> Expanded) ConversationInfo { get; set; }
        (IList<MailItem> SameFolder, IList<MailItem> Expanded) ConversationItems { get; set; }
        (int SameFolder, int Expanded) Count { get; }
        (DataFrame SameFolder, DataFrame Expanded) Df { get; }
        bool IsDarkMode { get; set; }
        Action<List<MailItemInfo>> UpdateUI { get; set; }

        event PropertyChangedEventHandler PropertyChanged;

        void Handler_PropertyChanged(object sender, PropertyChangedEventArgs e);
        
        Task<(List<MailItemInfo> SameFolder, List<MailItemInfo> Expanded)> LoadConversationInfoAsync(CancellationToken token, bool backgroundLoad);
        Task LoadDfAsync(CancellationToken token, bool backgroundLoad);
    }
}