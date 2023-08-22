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
        void KeyboardHandler_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e);
        void KeyboardHandler_KeyDown(object sender, KeyEventArgs e);
        Dictionary<char, Action<char>> KdCharActions { get; set; }
        Dictionary<Keys, Action<Keys>> KdKeyActions { get; set; }
        void CboFolders_KeyDown(object sender, KeyEventArgs e);
    }
}