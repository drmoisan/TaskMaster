using QuickFiler.Controllers;
using QuickFiler;
using System;
using System.Windows.Forms;

namespace QuickFiler.Interfaces
{
    public interface IQfcTipsDetails
    {
        int ColumnNumber { get; }
        float ColumnWidth { get; }
        Label LabelControl { get; }
        TableLayoutPanel TLP { get; }
        Type ResolveParentType();
        void Toggle();
        void Toggle(Enums.ToggleState desiredState);
        void Toggle(bool shareColumn);
        void Toggle(Enums.ToggleState desiredState, bool shareColumn);
    }
}