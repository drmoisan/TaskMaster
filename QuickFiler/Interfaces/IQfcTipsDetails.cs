using QuickFiler.Controllers;
using System;
using System.Windows.Forms;

namespace QuickFiler.Interfaces
{
    internal interface IQfcTipsDetails
    {
        public enum ToggleState { Off = 0, On = 1 }
        int ColumnNumber { get; }
        float ColumnWidth { get; }
        Label LabelControl { get; }
        TableLayoutPanel TLP { get; }
        Type ResolveParentType();
        void Toggle();
        void Toggle(ToggleState desiredState);
    }
}