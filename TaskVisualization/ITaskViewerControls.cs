using System.Windows.Forms;

namespace TaskVisualization
{
    /// <summary>
    /// Control-identity companion surface for <see cref="TaskController"/>'s two
    /// WinForms-bound partials (<c>TaskController.ControlMaps.cs</c> and
    /// <c>TaskController.Accelerator.cs</c>). It exposes, as their real
    /// <see cref="System.Windows.Forms"/> <see cref="Label"/> / <see cref="Control"/>
    /// types (deliberately NOT primitives), exactly the accelerator / navigation
    /// control-identity members those partials read off the concrete
    /// <see cref="TaskViewer"/>.
    /// </summary>
    /// <remarks>
    /// Rationale (STA last-resort refinement, condition a): these members' real
    /// object identity and live parenting ARE the logic under test — the lookup
    /// dictionaries key on control object-identity, <c>TipsController</c> throws
    /// without a real parented <see cref="TableLayoutPanel"/>/<see cref="Panel"/>,
    /// and <c>.BackColor</c>/<c>Button.PerformClick</c> require real
    /// <see cref="Control"/> instances. They therefore cannot be represented as
    /// mockable primitives. Isolating them on this companion interface (rather than
    /// on the primitives-only <see cref="ITaskViewer"/>) lets the dedicated STA
    /// tests supply real, never-shown, in-memory controls while keeping
    /// <see cref="ITaskViewer"/> clean and Form-free. All members are get-only: the
    /// control-map / accelerator regions read control identity and then operate on
    /// the returned control instances directly.
    /// </remarks>
    public interface ITaskViewerControls
    {
        // Accelerator sector labels (nav group 0).
        Label XlSector1 { get; }
        Label XlSector2 { get; }
        Label XlSector3 { get; }
        Label XlSector4 { get; }

        // Navigation cell labels (used by NavTips / TipsController).
        Label C1S1 { get; }
        Label C3S1 { get; }
        Label C4S1 { get; }
        Label C2S2 { get; }
        Label C3S2 { get; }
        Label C4S2 { get; }
        Label C2S3 { get; }
        Label C3S3 { get; }
        Label C4S3 { get; }
        Label C2S4 { get; }
        Label C3S4 { get; }

        // Accelerator labels for the field groups.
        Label XlTopic { get; }
        Label XlProject { get; }
        Label XlPeople { get; }
        Label XlContext { get; }
        Label XlTaskname { get; }
        Label XlImportance { get; }
        Label XlKanban { get; }
        Label XlWorktime { get; }
        Label XlReminder { get; }
        Label XlDuedate { get; }
        Label XlOk { get; }
        Label XlCancel { get; }
        Label XlAutotag { get; }

        // Accelerator labels for the shortcut group.
        Label XlScWaiting { get; }
        Label XlScUnprocessed { get; }
        Label XlScNews { get; }
        Label XlScEmail { get; }
        Label XlScReadingbusiness { get; }
        Label XlScCalls { get; }
        Label XlScInternet { get; }
        Label XlScPreread { get; }
        Label XlScMeeting { get; }
        Label XlScPersonal { get; }
        Label XlScBullpin { get; }
        Label XlScToday { get; }

        // Caption labels.
        Label LblTopic { get; }
        Label LblProject { get; }
        Label LblPeople { get; }
        Label LblContext { get; }
        Label LblTaskname { get; }
        Label LblPriority { get; }
        Label LblKbf { get; }
        Label LblDuration { get; }
        Label LblReminder { get; }
        Label LblDuedate { get; }

        // Data-bearing selection labels (used by OptionsGroups).
        Label CategorySelection { get; }
        Label PeopleSelection { get; }
        Label ProjectSelection { get; }
        Label TopicSelection { get; }

        // Data-bearing control targets referenced by GetControlRelationships.
        TextBox TaskName { get; }
        ComboBox PriorityBox { get; }
        ComboBox KbSelector { get; }
        TextBox Duration { get; }
        DateTimePicker DtReminder { get; }
        DateTimePicker DtDuedate { get; }

        // Buttons.
        Button OKButton { get; }
        Button Cancel_Button { get; }
        Button AutoTagButton { get; }
        Button ShortcutWaitingFor { get; }
        Button ShortcutUnprocessed { get; }
        Button ShortcutNews { get; }
        Button ShortcutEmail { get; }
        Button ShortcutReadingBusiness { get; }
        Button ShortcutCalls { get; }
        Button ShortcutInternet { get; }
        Button ShortcutPreRead { get; }
        Button ShortcutMeeting { get; }
        Button ShortcutPersonal { get; }

        // Check boxes referenced by the shortcut group.
        CheckBox CbxBullpin { get; }
        CheckBox CbxToday { get; }
    }
}
