using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Office.Interop.Outlook;
using Tags;
using ToDoModel;
using UtilitiesCS;
using UtilitiesCS.OutlookExtensions;

namespace TaskVisualization
{
    public partial class TaskController
    {
        /// <summary>
        /// Loads a TagViewer with categories relevant to People for assigment
        /// </summary>
        public void AssignPeople()
        {
            var prefix = _defaults.PrefixList.Find(x => x.Key == "People");

            var filtered_cats = (
                from x in _dict_categories
                where x.Key.Contains(prefix.Value)
                select x
            ).ToSortedDictionary();

            IList<string> selections = _active.People.AsListWithPrefix;

            selections.Remove("");

            var result = _tagPromptService.Prompt(
                new TagPromptRequest(
                    options: filtered_cats,
                    autoAssigner: _autoAssign,
                    prefixes: _defaults.PrefixList,
                    selections: selections,
                    prefixKey: prefix.Key,
                    objItemObject: _active.OlItem,
                    userEmailAddress: _userEmailAddress,
                    caption: string.Empty
                )
            );
            if (!result.Cancelled)
            {
                _active.People.AsStringNoPrefix = result.Selection;
                _viewer.PeopleText = _active.People.AsStringNoPrefix;
            }
        }

        /// <summary>
        /// Loads a TagViewer with categories relevant to Context for assigment
        /// </summary>
        public void AssignContext()
        {
            var prefix = _defaults.PrefixList.Find(x => x.Key == "Context");

            var filtered_cats = (
                from x in _dict_categories
                where x.Key.Contains(prefix.Value)
                select x
            ).ToSortedDictionary();

            IList<string> selections = _active.Context.AsListNoPrefix;
            bool unused1 = selections.Remove("");

            var result = _tagPromptService.Prompt(
                new TagPromptRequest(
                    options: filtered_cats,
                    autoAssigner: _autoAssign,
                    prefixes: _defaults.PrefixList,
                    selections: selections,
                    prefixKey: prefix.Key,
                    objItemObject: _active.OlItem,
                    userEmailAddress: _userEmailAddress,
                    caption: string.Empty
                )
            );
            if (!result.Cancelled)
            {
                _active.Context.AsStringNoPrefix = result.Selection;
                _viewer.ContextText = _active.Context.AsStringNoPrefix;
            }
        }

        public void AssignProject()
        {
            var prefix = _defaults.PrefixList.Find(x => x.Key == "Project");

            var filtered_cats = (
                from x in _dict_categories
                where x.Key.Contains(prefix.Value)
                select x
            ).ToSortedDictionary();

            IList<string> selections = _active.Projects.AsListNoPrefix;
            bool unused1 = selections.Remove("");

            var result = _tagPromptService.Prompt(
                new TagPromptRequest(
                    options: filtered_cats,
                    autoAssigner: ProjectAssign,
                    prefixes: _defaults.PrefixList,
                    selections: selections,
                    prefixKey: prefix.Key,
                    objItemObject: _active.OlItem,
                    userEmailAddress: _userEmailAddress,
                    caption: "Assign Project"
                )
            );
            if (!result.Cancelled)
            {
                _active.Projects.AsStringNoPrefix = result.Selection;
                _viewer.ProjectText = _active.Projects.AsStringNoPrefix;
                _active.Program.AsStringNoPrefix = ProjectsToPrograms(
                    _active.Projects.AsStringNoPrefix
                );
            }
        }

        /// <summary>
        /// Loads a TagViewer with categories relevant to Topics for assigment
        /// </summary>
        public void AssignTopic()
        {
            var prefix = _defaults.PrefixList.Find(x => x.Key == "Topic");

            var filtered_cats = (
                from x in _dict_categories
                where x.Key.Contains(prefix.Value)
                select x
            ).ToSortedDictionary();

            IList<string> selections = _active.Topics.AsListNoPrefix;
            bool unused1 = selections.Remove("");

            var result = _tagPromptService.Prompt(
                new TagPromptRequest(
                    options: filtered_cats,
                    autoAssigner: _autoAssign,
                    prefixes: _defaults.PrefixList,
                    selections: selections,
                    prefixKey: prefix.Key,
                    objItemObject: _active.OlItem,
                    userEmailAddress: _userEmailAddress,
                    caption: string.Empty
                )
            );
            if (!result.Cancelled)
            {
                _active.Topics.AsStringNoPrefix = result.Selection;
                _viewer.TopicText = _active.Topics.AsStringNoPrefix;
            }
        }

        /// <summary> Ensures ToDoItem model is in sync with changes in the viewer </summary>
        public void Assign_KB()
        {
            _active.KB.AsStringNoPrefix = _viewer.KbSelectedItem.ToString();
        }

        /// <summary> Ensures ToDoItem model is in sync with changes in the viewer </summary>
        public void Assign_Priority()
        {
            _active.Priority = TaskPriorityMapper.FromDisplayString(
                _viewer.PrioritySelectedItem.ToString()
            );
        }

        /// <summary> Ensures ToDoItem model is in sync with changes in the viewer </summary>
        public void Today_Change()
        {
            _active.Today = _viewer.TodayChecked;
        }

        /// <summary> Ensures ToDoItem model is in sync with changes in the viewer </summary>
        public void Bullpin_Change()
        {
            _active.Bullpin = _viewer.BullpinChecked;
        }

        /// <summary> Ensures ToDoItem model is in sync with changes in the viewer </summary>
        public void FlagAsTask_Change()
        {
            _active.FlagAsTask = _viewer.FlagAsTaskChecked;
        }

        /// <summary>
        /// Method determines if any category has been selected and copies the flags from the
        /// sample _active item to all members of _todo_list based on flags set in _options
        /// </summary>
        public async Task OK_Action()
        {
            if (_viewer.InvokeRequired)
            {
                // Fire-and-forget re-marshal onto the UI thread via the synchronous
                // Control.Invoke API. This is a pre-existing recursive UI-thread-marshal
                // pattern; adding `await` here would change the WinForms message-pump
                // re-entrancy behavior (a real behavior change forbidden by AC7). The
                // pragma bracket suppresses CS4014 without altering the fire-and-forget
                // semantics.
#pragma warning disable CS4014
                _viewer.Invoke((System.Action)(() => OK_Action()));
#pragma warning restore CS4014
                return;
            }
            if (AnyCategorySelected)
            {
                // Capture whether the task was flagged as a task
                _active.FlagAsTask = _viewer.FlagAsTaskChecked;

                // Capture the value of the task subject and if not empty write to ToDoItem
                if (_options.HasFlag(Enums.FlagsToSet.Taskname))
                {
                    if (!string.IsNullOrEmpty(_viewer.TaskNameText))
                        _active.TaskSubject = _viewer.TaskNameText;
                }

                // Capture the worktime, validate and write to ToDoItem
                CaptureDuration();

                _viewer.Hide();

                // Apply values captured in _active to each member of _todo_list for flags in _options
                await Task.Run(ApplyChanges);

                _viewer.DialogResult = DialogResult.OK;

                _viewer.Dispose();
            }
        }

        /// <summary>
        /// Handles cancel button click. Sets the controller exit type to
        /// "Cancel" and disposes of the viewer
        /// </summary>
        public void Cancel_Action()
        {
            _viewer.Hide();
            //_exit_type = "Cancel";
            _viewer.DialogResult = DialogResult.Cancel;
            _viewer.Dispose();
        }

        /// <summary> Sets values to specific fields based on shortcut button </summary>
        public void Shortcut_Personal()
        {
            var prefix = _defaults.PrefixList.Find(x => x.Key == "Context");
            _viewer.ContextText = prefix.Value + "Personal";
            _active.Context.AsStringNoPrefix = prefix.Value + "Personal";

            prefix = _defaults.PrefixList.Find(x => x.Key == "Project");
            _viewer.ProjectText = prefix.Value + "Personal - Other";
            _active.Projects.AsStringNoPrefix = prefix.Value + "Personal - Other";
        }

        /// <summary> Sets values to specific fields based on shortcut button </summary>
        public void Shortcut_Meeting()
        {
            SetFlag("Meeting", Enums.FlagsToSet.Context);
        }

        /// <summary> Sets values to specific fields based on shortcut button </summary>
        public void Shortcut_Email()
        {
            SetFlag("Email", Enums.FlagsToSet.Context);
        }

        /// <summary> Sets values to specific fields based on shortcut button </summary>
        public void Shortcut_Calls()
        {
            SetFlag("Calls", Enums.FlagsToSet.Context);
        }

        /// <summary> Sets values to specific fields based on shortcut button </summary>
        public void Shortcut_PreRead()
        {
            SetFlag("PreRead", Enums.FlagsToSet.Context);
        }

        /// <summary> Sets values to specific fields based on shortcut button </summary>
        public void Shortcut_WaitingFor()
        {
            SetFlag("Waiting For", Enums.FlagsToSet.Context);
        }

        /// <summary> Sets values to specific fields based on shortcut button </summary>
        public void Shortcut_Unprocessed()
        {
            SetFlag("Reading - .Unprocessed > 2 Minutes", Enums.FlagsToSet.Context);
        }

        /// <summary> Sets values to specific fields based on shortcut button </summary>
        public void Shortcut_ReadingBusiness()
        {
            SetFlag("Reading - Business", Enums.FlagsToSet.Context);
        }

        /// <summary> Sets values to specific fields based on shortcut button </summary>
        public void Shortcut_ReadingNews()
        {
            SetFlag("Reading - News | Articles | Other", Enums.FlagsToSet.Context);
            SetFlag("Routine - Reading", Enums.FlagsToSet.Projects);
            SetFlag("READ: " + _viewer.TaskNameText, Enums.FlagsToSet.Taskname);
            SetFlag("15", Enums.FlagsToSet.Worktime);
            _viewer.FocusDuration();
        }

        internal async Task AutoAssignAllAsync()
        {
            if (_active?.OlItem?.InnerObject is not MailItem mailItem)
            {
                return;
            }
            var helper = await _mailItemHelperFactory(mailItem).ConfigureAwait(true);

            var projects = await ProjectAssign.AutoFindAsync(helper).ConfigureAwait(true);
            if (projects?.Count > 0)
            {
                MergeFlag(projects, Enums.FlagsToSet.Projects);
            }

            var context = await ContextAssign.AutoFindAsync(helper).ConfigureAwait(true);
            if (context?.Count > 0)
            {
                MergeFlag(context, Enums.FlagsToSet.Context);
            }

            var people = await _autoAssign.AutoFindAsync(mailItem).ConfigureAwait(true);
            if (people?.Count > 0)
            {
                MergeFlag(people, Enums.FlagsToSet.People);
            }
        }

        /// <summary>
        /// Property determines whether any category contains a value
        /// </summary>
        /// <returns>True if any value set in Context, People, Project or Topic. Else returns False</returns>
        internal bool AnyCategorySelected
        {
            //TODO: Rewrite AnyCategorySelected property to be more stable
            get
            {
                return _viewer.ContextText != "[Category Label]"
                    | _viewer.PeopleText != "[Assigned People Flagged]"
                    | _viewer.ProjectText != "[ Projects Flagged ]"
                    | _viewer.TopicText != "[Other Topics Tagged]";
            }
        }

        /// <summary>
        /// Sets value based on the flag type and value
        /// </summary>
        /// <param name="value">Comma separated list of tags</param>
        /// <param name="flagType">Used to identify field names and tag Prefix</param>
        internal void SetFlag(string value, Enums.FlagsToSet flagType)
        {
            switch (flagType)
            {
                case Enums.FlagsToSet.Context:
                {
                    _active.Context.AsStringNoPrefix = value;
                    _viewer.ContextText = _active.Context.AsStringNoPrefix;
                    break;
                }
                case Enums.FlagsToSet.People:
                {
                    _active.People.AsStringNoPrefix = value;
                    _viewer.PeopleText = _active.People.AsStringNoPrefix;
                    break;
                }
                case Enums.FlagsToSet.Projects:
                {
                    _active.Projects.AsStringNoPrefix = value;
                    _viewer.ProjectText = _active.Projects.AsStringNoPrefix;
                    break;
                }
                case Enums.FlagsToSet.Topics:
                {
                    _active.Topics.AsStringNoPrefix = value;
                    _viewer.TopicText = _active.Topics.AsStringNoPrefix;
                    break;
                }
                case Enums.FlagsToSet.Taskname:
                {
                    _setActiveTaskSubject(value);
                    _viewer.TaskNameText = value;
                    break;
                }
                case Enums.FlagsToSet.Worktime:
                {
                    _viewer.DurationText = value;
                    break;
                }
                // Note that _active is set after OK click
            }
        }

        internal ObservableCollection<string> MergeToCollection(
            ObservableCollection<string> original,
            IList<string> toMerge
        )
        {
            var hash = new HashSet<string>(toMerge);
            original.ForEach(x => hash.Add(x));
            return new ObservableCollection<string>(hash);
        }

        /// <summary>
        /// Sets value based on the flag type and value
        /// </summary>
        /// <param name="value">Comma separated list of tags</param>
        /// <param name="flagType">Used to identify field names and tag Prefix</param>
        internal void MergeFlag(IList<string> value, Enums.FlagsToSet flagType)
        {
            if (_viewer.InvokeRequired)
            {
                _viewer.Invoke((System.Action)(() => MergeFlag(value, flagType)));
                return;
            }

            if (value.IsNullOrEmpty())
            {
                return;
            }
            switch (flagType)
            {
                case Enums.FlagsToSet.Context:
                {
                    _active.Context.AsListWithPrefix = MergeToCollection(
                        _active.Context.AsListWithPrefix,
                        value
                    );
                    _viewer.ContextText = _active.Context.AsStringNoPrefix;
                    break;
                }
                case Enums.FlagsToSet.People:
                {
                    _active.People.AsListWithPrefix = MergeToCollection(
                        _active.People.AsListWithPrefix,
                        value
                    );
                    _viewer.PeopleText = _active.People.AsStringNoPrefix;
                    break;
                }
                case Enums.FlagsToSet.Projects:
                {
                    _active.Projects.AsListWithPrefix = MergeToCollection(
                        _active.Projects.AsListWithPrefix,
                        value
                    );
                    _viewer.ProjectText = _active.Projects.AsStringNoPrefix;
                    break;
                }
                case Enums.FlagsToSet.Topics:
                {
                    _active.Topics.AsListWithPrefix = MergeToCollection(
                        _active.Topics.AsListWithPrefix,
                        value
                    );
                    _viewer.TopicText = _active.Topics.AsStringNoPrefix;
                    break;
                } // Note that _active is set after OK click
            }
        }

        /// <summary>
        /// Grabs the work duration text from the viewer, parses it via
        /// <see cref="TaskDurationParser"/>, and sets TotalWork on the ToDoItem.
        /// Behavior is preserved exactly: a negative integer invokes the injected warning
        /// notifier (production default <c>MessageBox.Show</c>) and leaves TotalWork
        /// unchanged; a non-integer, empty, or whitespace input lets the
        /// <see cref="FormatException"/> from the parser propagate uncaught (as before).
        /// </summary>
        /// <exception cref="FormatException">
        /// Propagated from the parser for a non-integer / empty / whitespace input.
        /// </exception>
        internal void CaptureDuration()
        {
            var (ok, minutes, error) = TaskDurationParser.Parse(_viewer.DurationText);
            if (!ok)
            {
                _showWarning(error);
                return;
            }

            _active.TotalWork = minutes;
        }
    }
}
