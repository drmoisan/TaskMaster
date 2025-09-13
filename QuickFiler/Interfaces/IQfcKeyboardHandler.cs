using System.Collections.Generic;
using System;
using System.Windows.Forms;
using QuickFiler.Controllers;
using System.Threading.Tasks;

namespace QuickFiler.Interfaces
{
    public interface IQfcKeyboardHandler
    {
        bool KbdActive { get; set; }
        void ToggleKeyboardDialog();
        void ToggleKeyboardDialog(object sender, KeyEventArgs e); 
        Task ToggleKeyboardDialogAsync();
        void ToggleKeyboardDialogAsync(object sender, KeyEventArgs e);
        void KeyboardHandler_PreviewKeyDownAsync(object sender, PreviewKeyDownEventArgs e);
        void KeyboardHandler_KeyDown(object sender, KeyEventArgs e);
        void KeyboardHandler_KeyDownAsync(object sender, KeyEventArgs e);
        //Dictionary<char, Action<char>> CharActions { get; set; }
        KbdActions<char, KaChar, Action<char>> CharActions { get; set; }
        KbdActions<char, KaCharAsync, Func<char, Task>> CharActionsAsync { get; set; }
        KbdActions<Keys, KaKey, Action<Keys>> KeyActions { get; set; }
        KbdActions<Keys, KaKeyAsync, Func<Keys, Task>> KeyActionsAsync { get; set; }
        KbdActions<Keys, KaKeyAsync, Func<Keys, Task>> AlwaysOnKeyActionsAsync { get; set; }
        KbdActions<string, KaStringAsync, Func<string, Task>> StringActionsAsync { get; set; }

        void CboFolders_KeyDown(object sender, KeyEventArgs e);
        void CboFolders_KeyDownAsync(object sender, KeyEventArgs e);
    }
}