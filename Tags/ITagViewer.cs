using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using UtilitiesCS;
using UtilitiesCS.Interfaces.IWinForm;

namespace Tags
{
    /// <summary>
    /// Testability seam for the tag-selection dialog. Derives from <see cref="IForm"/> and
    /// exposes the WinForms surface consumed by <see cref="TagController"/> as intent-named
    /// events, properties, and methods so the controller never depends on the concrete
    /// <see cref="TagViewer"/> form or its designer controls. The concrete viewer maps every
    /// member 1:1 onto its existing controls; unit tests supply an in-memory fake or a Moq mock.
    /// </summary>
    public interface ITagViewer : IForm
    {
        // Command intent events (replace raw Button/TextBox/CheckBox event subscriptions).
        event EventHandler OkClicked;
        event EventHandler CancelClicked;
        event EventHandler NewClicked;
        event EventHandler AutoAssignClicked;
        event EventHandler SearchTextChanged;
        event KeyEventHandler SearchKeyDown;
        event KeyEventHandler SearchKeyUp;
        event EventHandler HideArchiveChanged;

        // Maps to the form's KeyDown event, which IForm does not expose.
        event KeyEventHandler ViewKeyDown;

        event PreviewKeyDownEventHandler OptionsPreviewKeyDown;
        event KeyEventHandler OptionsKeyDown;

        // State intent properties.
        bool HideArchiveChecked { get; }
        bool AutoAssignVisible { get; set; }
        bool AutoAssignEnabled { get; set; }
        bool ButtonNewVisible { get; set; }
        string SearchTextValue { get; set; }
        int SearchSelectionStart { get; }

        // The form caption; IForm does not expose Text, so Caption is declared explicitly.
        string Caption { get; set; }

        // Intent methods and option-panel abstraction.
        ControlPosition CaptureAndRemoveTemplate();
        void FocusSearch();
        void AddOptionControl(CheckBox control);
        void RemoveOptionControl(CheckBox control);
        IReadOnlyList<CheckBox> OptionControls { get; }
        int OptionsPanelHeight { get; }
        int OptionsScrollMaximum { get; }
        Point OptionsAutoScrollPosition { get; set; }

        // Retained so TagLauncher and CheckBoxController._parent stay bound to the concrete controller.
        void SetController(TagController controller);
    }
}
