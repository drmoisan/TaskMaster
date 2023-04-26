using System.Collections.Generic;
using Microsoft.Office.Interop.Outlook;
using UtilitiesCS;


namespace QuickFiler.Interfaces
{
    public interface IQfcDatamodel
    {
        IList<object> DequeueNextItemGroup(int quantity);
        void UndoMove();
        StackObjectCS<object> StackMovedItems { get; set; }
        bool MoveItems(ref StackObjectCS<object> StackMovedItems);
    }
}