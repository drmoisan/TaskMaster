using System.Collections.Generic;
using System;
using System.Windows.Forms;

namespace QuickFiler.Interfaces
{
    public interface IQfcKeyboardHandler
    {
        bool KbdActive { get; set; }
        void ToggleKeyboardDialog();
        void ToggleKeyboardDialog(object sender, KeyEventArgs e); 
        void ToggleRemoteMouseLabels(); // Not supported yet
        bool ToggleOffActiveItem(bool parentBlExpanded);
        void KeyboardDialog_Change();
        void KeyboardDialog_KeyDown(object sender, KeyEventArgs e);
        void KeyboardDialog_KeyUp(object sender, KeyEventArgs e);
        void ResetAcceleratorSilently();
        void KeyboardHandler_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e);
        void KeyboardHandler_KeyDown(object sender, KeyEventArgs e);
        void KeyboardHandler_KeyPress(object sender, KeyPressEventArgs e);
        void KeyboardHandler_KeyUp(object sender, KeyEventArgs e);
        void PanelMain_KeyDown(object sender, KeyEventArgs e);
        void PanelMain_KeyPress(object sender, KeyPressEventArgs e);
        void PanelMain_KeyUp(object sender, KeyEventArgs e);
        Dictionary<char, Action<char>> KdCharActions { get; set; }
        Dictionary<Keys, Action<Keys>> KdKeyActions { get; set; }
        Dictionary<char, System.Action> KuCharActions { get; set; }
        Dictionary<Keys, System.Action> KuKeyActions { get; set; }
        Dictionary<char, System.Action> KprsCharActions { get; set; }
        Dictionary<Keys, System.Action> KprsKeyActions { get; set; }
    }
}