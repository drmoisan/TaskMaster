using System.Windows.Forms;

namespace QuickFiler
{
    public interface IQfcKeyboardHandler
    {
        void ToggleKeyboardDialog(); // Need to rewrite
        void ToggleRemoteMouseLabels(); // Not supported yet
        bool ToggleOffActiveItem(bool parentBlExpanded);
        void KeyboardDialog_Change();
        void KeyboardDialog_KeyDown(object sender, KeyEventArgs e);
        void KeyboardDialog_KeyUp(object sender, KeyEventArgs e);
        void ResetAcceleratorSilently();
        void KeyboardHandler_KeyDown(object sender, KeyEventArgs e);
        void KeyboardHandler_KeyPress(object sender, KeyPressEventArgs e);
        void KeyboardHandler_KeyUp(object sender, KeyEventArgs e);
        void PanelMain_KeyDown(object sender, KeyEventArgs e);
        void PanelMain_KeyPress(object sender, KeyPressEventArgs e);
        void PanelMain_KeyUp(object sender, KeyEventArgs e);
    }
}