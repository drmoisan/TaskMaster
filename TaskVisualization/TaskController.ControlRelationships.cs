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
        private List<ControlRelationship> GetControlRelationships()
        {
            var list = new List<ControlRelationship>
            {
                new ControlRelationship(
                    0,
                    ViewerControls.XlSector1,
                    true,
                    ViewerControls.XlSector1.Text,
                    ViewerControls.XlSector1
                ),
                new ControlRelationship(
                    0,
                    ViewerControls.XlSector2,
                    true,
                    ViewerControls.XlSector2.Text,
                    ViewerControls.XlSector2
                ),
                new ControlRelationship(
                    0,
                    ViewerControls.XlSector3,
                    _options.HasFlag(Enums.FlagsToSet.All),
                    ViewerControls.XlSector3.Text,
                    ViewerControls.XlSector3
                ),
                new ControlRelationship(
                    0,
                    ViewerControls.XlSector4,
                    true,
                    ViewerControls.XlSector4.Text,
                    ViewerControls.XlSector4
                ),
                new ControlRelationship(
                    2,
                    ViewerControls.XlTopic,
                    _options.HasFlag(Enums.FlagsToSet.Topics),
                    ViewerControls.LblTopic.Text,
                    ViewerControls.LblTopic
                ),
                new ControlRelationship(
                    2,
                    ViewerControls.XlProject,
                    _options.HasFlag(Enums.FlagsToSet.Projects),
                    ViewerControls.LblProject.Text,
                    ViewerControls.LblProject
                ),
                new ControlRelationship(
                    2,
                    ViewerControls.XlPeople,
                    _options.HasFlag(Enums.FlagsToSet.People),
                    ViewerControls.LblPeople.Text,
                    ViewerControls.LblPeople
                ),
                new ControlRelationship(
                    2,
                    ViewerControls.XlContext,
                    _options.HasFlag(Enums.FlagsToSet.Context),
                    ViewerControls.LblContext.Text,
                    ViewerControls.LblContext
                ),
                new ControlRelationship(
                    1,
                    ViewerControls.XlTaskname,
                    _options.HasFlag(Enums.FlagsToSet.Taskname),
                    ViewerControls.LblTaskname.Text,
                    ViewerControls.TaskName
                ),
                new ControlRelationship(
                    1,
                    ViewerControls.XlImportance,
                    _options.HasFlag(Enums.FlagsToSet.Priority),
                    ViewerControls.LblPriority.Text,
                    ViewerControls.PriorityBox
                ),
                new ControlRelationship(
                    1,
                    ViewerControls.XlKanban,
                    _options.HasFlag(Enums.FlagsToSet.Kbf),
                    ViewerControls.LblKbf.Text,
                    ViewerControls.KbSelector
                ),
                new ControlRelationship(
                    1,
                    ViewerControls.XlWorktime,
                    _options.HasFlag(Enums.FlagsToSet.Worktime),
                    ViewerControls.LblDuration.Text,
                    ViewerControls.Duration
                ),
                new ControlRelationship(
                    4,
                    ViewerControls.XlOk,
                    true,
                    ViewerControls.OKButton.Text,
                    ViewerControls.OKButton
                ),
                new ControlRelationship(
                    4,
                    ViewerControls.XlCancel,
                    true,
                    ViewerControls.Cancel_Button.Text,
                    ViewerControls.Cancel_Button
                ),
                new ControlRelationship(
                    4,
                    ViewerControls.XlAutotag,
                    true,
                    ViewerControls.AutoTagButton.Text,
                    ViewerControls.AutoTagButton
                ),
                new ControlRelationship(
                    1,
                    ViewerControls.XlReminder,
                    _options.HasFlag(Enums.FlagsToSet.Reminder),
                    ViewerControls.LblReminder.Text,
                    ViewerControls.DtReminder
                ),
                new ControlRelationship(
                    1,
                    ViewerControls.XlDuedate,
                    _options.HasFlag(Enums.FlagsToSet.DueDate),
                    ViewerControls.LblDuedate.Text,
                    ViewerControls.DtDuedate
                ),
                new ControlRelationship(
                    3,
                    ViewerControls.XlScWaiting,
                    _options.HasFlag(Enums.FlagsToSet.All),
                    ViewerControls.ShortcutWaitingFor.Text,
                    ViewerControls.ShortcutWaitingFor
                ),
                new ControlRelationship(
                    3,
                    ViewerControls.XlScUnprocessed,
                    _options.HasFlag(Enums.FlagsToSet.All),
                    ViewerControls.ShortcutUnprocessed.Text,
                    ViewerControls.ShortcutUnprocessed
                ),
                new ControlRelationship(
                    3,
                    ViewerControls.XlScNews,
                    _options.HasFlag(Enums.FlagsToSet.All),
                    ViewerControls.ShortcutNews.Text,
                    ViewerControls.ShortcutNews
                ),
                new ControlRelationship(
                    3,
                    ViewerControls.XlScEmail,
                    _options.HasFlag(Enums.FlagsToSet.All),
                    ViewerControls.ShortcutEmail.Text,
                    ViewerControls.ShortcutEmail
                ),
                new ControlRelationship(
                    3,
                    ViewerControls.XlScReadingbusiness,
                    _options.HasFlag(Enums.FlagsToSet.All),
                    ViewerControls.ShortcutReadingBusiness.Text,
                    ViewerControls.ShortcutReadingBusiness
                ),
                new ControlRelationship(
                    3,
                    ViewerControls.XlScCalls,
                    _options.HasFlag(Enums.FlagsToSet.All),
                    ViewerControls.ShortcutCalls.Text,
                    ViewerControls.ShortcutCalls
                ),
                new ControlRelationship(
                    3,
                    ViewerControls.XlScInternet,
                    _options.HasFlag(Enums.FlagsToSet.All),
                    ViewerControls.ShortcutInternet.Text,
                    ViewerControls.ShortcutInternet
                ),
                new ControlRelationship(
                    3,
                    ViewerControls.XlScPreread,
                    _options.HasFlag(Enums.FlagsToSet.All),
                    ViewerControls.ShortcutPreRead.Text,
                    ViewerControls.ShortcutPreRead
                ),
                new ControlRelationship(
                    3,
                    ViewerControls.XlScMeeting,
                    _options.HasFlag(Enums.FlagsToSet.All),
                    ViewerControls.ShortcutMeeting.Text,
                    ViewerControls.ShortcutMeeting
                ),
                new ControlRelationship(
                    3,
                    ViewerControls.XlScPersonal,
                    _options.HasFlag(Enums.FlagsToSet.All),
                    ViewerControls.ShortcutPersonal.Text,
                    ViewerControls.ShortcutPersonal
                ),
                new ControlRelationship(
                    3,
                    ViewerControls.XlScBullpin,
                    _options.HasFlag(Enums.FlagsToSet.All),
                    ViewerControls.CbxBullpin.Text,
                    ViewerControls.CbxBullpin
                ),
                new ControlRelationship(
                    3,
                    ViewerControls.XlScToday,
                    _options.HasFlag(Enums.FlagsToSet.All),
                    ViewerControls.CbxToday.Text,
                    ViewerControls.CbxToday
                ),
            };
            return list;
        }

        private struct ControlRelationship
        {
            public ControlRelationship() { }

            public ControlRelationship(
                int group,
                Label accelerator,
                bool active,
                string caption,
                Control control
            )
            {
                Group = group;
                Accelerator = accelerator;
                Active = active;
                Caption = caption;
                Control = control;
            }

            public int Group;
            public Label Accelerator;
            public bool Active;
            public string Caption;
            public Control Control;
        }
    }
}
