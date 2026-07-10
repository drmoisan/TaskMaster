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
        /// Iterates through _todo_list and applies the values in _active for the fields in _options.
        /// </summary>
        /// <remarks>
        /// COM-iteration wiring (method-level exemption per the coverage-exemption inventory): this
        /// method constructs <see cref="FlagChangeGroup"/> from a live <c>MailItem</c>, mutates the
        /// static <c>ToDoEvents.Editing</c> edit-count queue, drives <c>WriteFlagsBatchAsync</c>
        /// (live UDF/category writes), and enqueues onto <c>Globals.TD.FlagChangeTrainingQueue</c>.
        /// These Outlook-Interop side effects do not terminate deterministically over Moq doubles
        /// (they depend on live COM semantics), so the iteration wiring is exempt. The extractable,
        /// host-neutral units it calls — both <c>ApplyChange</c> overloads and
        /// <see cref="AreCollectionsEqual"/> — remain measured (P6-T5).
        /// </remarks>
        [ExcludeFromCodeCoverage]
        internal async Task ApplyChanges()
        {
            foreach (ToDoItem c in _todo_list)
            {
                ToDoEvents.Editing.AddOrUpdate(
                    c.OlItem.EntryID,
                    1,
                    (key, existing) => existing + 1
                );

                FlagChangeGroup fcg =
                    (c.OlItem.GetOlItemType() == OlItemType.olMailItem)
                        ? new(Globals, c.OlItem.InnerObject as MailItem)
                        : null;

                if (c.FlagAsTask != _active.FlagAsTask)
                {
                    c.FlagAsTask = _active.FlagAsTask;
                }

                c.ReadOnly = true;
                ApplyChange(fcg, "Context", Enums.FlagsToSet.Context, c.Context, _active.Context);
                ApplyChange(Enums.FlagsToSet.People, c.People, _active.People);
                ApplyChange(
                    fcg,
                    "Project",
                    Enums.FlagsToSet.Projects,
                    c.Projects,
                    _active.Projects
                );
                ApplyChange(Enums.FlagsToSet.Program, c.Program, _active.Program);
                ApplyChange(Enums.FlagsToSet.Topics, c.Topics, _active.Topics);
                ApplyChange(Enums.FlagsToSet.Kbf, c.KB, _active.KB);
                if (_options.HasFlag(Enums.FlagsToSet.Today) && c.Today != _active.Today)
                {
                    c.Today = _active.Today;
                    ChangedFlags |= Enums.FlagsToSet.Today;
                }
                if (_options.HasFlag(Enums.FlagsToSet.Bullpin) && c.Bullpin != _active.Bullpin)
                {
                    c.Bullpin = _active.Bullpin;
                    ChangedFlags |= Enums.FlagsToSet.Bullpin;
                }

                //if (_options.HasFlag(Enums.FlagsToSet.Context))
                //    c.Context.AsListNoPrefix = _active.Context.AsListNoPrefix;
                //if (_options.HasFlag(Enums.FlagsToSet.People))
                //    c.People.AsListNoPrefix = _active.People.AsListNoPrefix;
                //if (_options.HasFlag(Enums.FlagsToSet.Projects))
                //    c.Projects.AsListNoPrefix = _active.Projects.AsListNoPrefix;
                //if (_options.HasFlag(Enums.FlagsToSet.Program))
                //    c.Program.AsListNoPrefix = _active.Program.AsListNoPrefix;
                //if (_options.HasFlag(Enums.FlagsToSet.Topics))
                //    c.Topics.AsListNoPrefix = _active.Topics.AsListNoPrefix;
                //if (_options.HasFlag(Enums.FlagsToSet.Kbf))
                //    c.KB.AsStringNoPrefix = _active.KB.AsStringNoPrefix;

                await c.WriteFlagsBatchAsync(ChangedFlags);
                ChangedFlags = Enums.FlagsToSet.None;
                c.ReadOnly = false;

                if (_options.HasFlag(Enums.FlagsToSet.Priority) && c.Priority != _active.Priority)
                    c.Priority = _active.Priority;
                if (
                    _options.HasFlag(Enums.FlagsToSet.Taskname)
                    && c.TaskSubject != _active.TaskSubject
                )
                    c.TaskSubject = _active.TaskSubject;
                if (_options.HasFlag(Enums.FlagsToSet.Worktime) && c.TotalWork != _active.TotalWork)
                    c.TotalWork = _active.TotalWork;
                if (_options.HasFlag(Enums.FlagsToSet.DueDate) && c.DueDate != _active.DueDate)
                    c.DueDate = _active.DueDate;
                if (
                    _options.HasFlag(Enums.FlagsToSet.Reminder)
                    && c.ReminderTime != _active.ReminderTime
                )
                    c.ReminderTime = _active.ReminderTime;
                if (_options == Enums.FlagsToSet.All && c.ActiveBranch != true)
                    c.ActiveBranch = true;

                if (fcg?.FlagChangeItems.Count > 0)
                {
                    Globals.TD.FlagChangeTrainingQueue.Enqueue(fcg);
                }

                ToDoEvents.Editing.UpdateOrRemove(
                    c.OlItem.EntryID,
                    (key, existing) => existing == 1,
                    (key, existing) => existing - 1,
                    out _
                );
            }
        }

        internal void ApplyChange(
            Enums.FlagsToSet flag,
            FlagTranslator current,
            FlagTranslator revised
        )
        {
            if (Options.HasFlag(flag))
            {
                if (!AreCollectionsEqual(current.AsListNoPrefix, revised.AsListNoPrefix))
                {
                    current.AsListNoPrefix = revised.AsListNoPrefix;
                    ChangedFlags |= flag;
                }
            }
        }

        internal void ApplyChange(
            FlagChangeGroup fcg,
            string classifierName,
            Enums.FlagsToSet flag,
            FlagTranslator current,
            FlagTranslator revised
        )
        {
            if (Options.HasFlag(flag))
            {
                if (
                    fcg?.TryEnqueue(classifierName, current.AsListNoPrefix, revised.AsListNoPrefix)
                    ?? false || !AreCollectionsEqual(current.AsListNoPrefix, revised.AsListNoPrefix)
                )
                {
                    current.AsListNoPrefix = revised.AsListNoPrefix;
                    ChangedFlags |= flag;
                }
            }
        }

        internal bool AreCollectionsEqual(
            ObservableCollection<string> collectionA,
            ObservableCollection<string> collectionB
        )
        {
            if (collectionA == null || collectionB == null)
                return collectionA == collectionB;

            var setA = new HashSet<string>(collectionA);
            var setB = new HashSet<string>(collectionB);

            return setA.SetEquals(setB);
        }
    }
}
