using QuickFiler.Controllers;
using System;
using System.Windows.Forms;

namespace QuickFiler.Interfaces
{
    public interface IQfcTipsDetails
    {
        public enum ToggleState { Off = 0, On = 1 }
        int ColumnNumber { get; }
        float ColumnWidth { get; }
        Label LabelControl { get; }
        TableLayoutPanel TLP { get; }
        Type ResolveParentType();
        void Toggle();
        void Toggle(ToggleState desiredState);
        void Toggle(bool shareColumn);
        void Toggle(ToggleState desiredState, bool shareColumn);
    }
}