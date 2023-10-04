using System.Collections.Generic;
using System;
using System.Windows.Forms;
using QuickFiler.Controllers;

namespace QuickFiler.Interfaces
{
    public interface IQfcKeyboardHandler
    {
        bool KbdActive { get; set; }
        void ToggleKeyboardDialog();
        void ToggleKeyboardDialog(object sender, KeyEventArgs e); 
        void KeyboardHandler_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e);
        void KeyboardHandler_KeyDown(object sender, KeyEventArgs e);
        KbdActions<char, KaChar, Action<char>> CharActions { get; set; }
        //Dictionary<char, Action<char>> CharActions { get; set; }
        KbdActions<Keys, KaKey, Action<Keys>> KeyActions { get; set; }
        void CboFolders_KeyDown(object sender, KeyEventArgs e);
    }
}