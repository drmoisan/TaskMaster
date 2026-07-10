using System;
using UtilitiesCS.Interfaces.IWinForm;

namespace TaskVisualization
{
    /// <summary>
    /// Intent-named primitive facade over the data-bearing controls that
    /// <see cref="TaskController"/> reads from and writes to on the concrete
    /// <see cref="TaskViewer"/> form. Derives from <see cref="IForm"/> so the
    /// Form-level surface the controller uses (AcceptButton, CancelButton,
    /// DialogResult, ShowDialog, and via the IControl chain InvokeRequired,
    /// Invoke, Hide, Dispose, Focus, Controls) resolves through the base.
    /// </summary>
    /// <remarks>
    /// This interface is primitives-only: no <c>System.Windows.Forms</c> control
    /// types appear in any member signature. The ~50 accelerator / navigation
    /// control-identity members are exposed separately on
    /// <see cref="ITaskViewerControls"/>, because their real object identity and
    /// live parenting are the logic under test and cannot be represented as
    /// mockable primitives.
    /// </remarks>
    public interface ITaskViewer : IForm
    {
        /// <summary>Task subject text (backed by the TaskName text box).</summary>
        string TaskNameText { get; set; }

        /// <summary>Context / category selection text.</summary>
        string ContextText { get; set; }

        /// <summary>Assigned-people selection text.</summary>
        string PeopleText { get; set; }

        /// <summary>Project selection text.</summary>
        string ProjectText { get; set; }

        /// <summary>Topic selection text.</summary>
        string TopicText { get; set; }

        /// <summary>Duration (work-time) text.</summary>
        string DurationText { get; set; }

        /// <summary>Currently selected priority display item.</summary>
        object PrioritySelectedItem { get; set; }

        /// <summary>Currently selected Kanban / backlog display item.</summary>
        object KbSelectedItem { get; set; }

        /// <summary>Whether the "Today" flag check box is checked.</summary>
        bool TodayChecked { get; set; }

        /// <summary>Whether the "Bullpin" flag check box is checked.</summary>
        bool BullpinChecked { get; set; }

        /// <summary>Whether the "Flag as task" check box is checked.</summary>
        bool FlagAsTaskChecked { get; set; }

        /// <summary>Reminder date/time value.</summary>
        DateTime ReminderValue { get; set; }

        /// <summary>Whether the reminder picker is checked (active).</summary>
        bool ReminderChecked { get; set; }

        /// <summary>Due-date value.</summary>
        DateTime DueDateValue { get; set; }

        /// <summary>Whether the due-date picker is checked (active).</summary>
        bool DueDateChecked { get; set; }

        /// <summary>Moves input focus to the duration control.</summary>
        void FocusDuration();

        /// <summary>Registers the controller with the viewer.</summary>
        void SetController(TaskController controller);
    }
}
