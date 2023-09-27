using System;
using System.Windows.Forms;
using System.Threading.Tasks;


namespace UtilitiesCS
{
    public interface IQfcTipsDetails
    {
        int ColumnNumber { get; }
        float ColumnWidth { get; }
        Label LabelControl { get; }
        TableLayoutPanel TLP { get; }
        Type ResolveParentType();
        void Toggle();
        void Toggle(bool shareColumn);
        void Toggle(Enums.ToggleState desiredState);
        void Toggle(Enums.ToggleState desiredState, bool shareColumn);
        Task ToggleAsync(Enums.ToggleState desiredState);
        Task ToggleAsync(Enums.ToggleState desiredState, bool shareColumn);
    }
}