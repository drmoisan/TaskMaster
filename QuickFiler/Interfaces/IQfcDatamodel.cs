using System.Collections.Generic;
using Microsoft.Office.Interop.Outlook;
using UtilitiesCS;


namespace QuickFiler
{
    public interface IQfcDatamodel
    {
        IList<MailItem> DequeueNextEmailGroup(int quantity);
        void UndoMove();
        StackObjectCS<MailItem> MovedMails { get; set; }
        bool MoveEmails(ref StackObjectCS<MailItem> MovedMails);
        void CountMailsInConv(int ct = 0); //From item controller
    }
}