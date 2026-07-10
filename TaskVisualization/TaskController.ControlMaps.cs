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
        //private Dictionary<Label, bool> CreateOptionsLookup()
        //{
        //    var xlCtrlOptions = new Dictionary<Label, bool>();
        //    {
        //        xlCtrlOptions.Add(ViewerControls.XlTopic, _options.HasFlag(Enums.FlagsToSet.topics));
        //        xlCtrlOptions.Add(ViewerControls.XlProject, _options.HasFlag(Enums.FlagsToSet.projects));
        //        xlCtrlOptions.Add(ViewerControls.XlPeople, _options.HasFlag(Enums.FlagsToSet.people));
        //        xlCtrlOptions.Add(ViewerControls.XlContext, _options.HasFlag(Enums.FlagsToSet.context));
        //        xlCtrlOptions.Add(ViewerControls.XlTaskname, _options.HasFlag(Enums.FlagsToSet.taskname));
        //        xlCtrlOptions.Add(ViewerControls.XlImportance, _options.HasFlag(Enums.FlagsToSet.priority));
        //        xlCtrlOptions.Add(ViewerControls.XlKanban, _options.HasFlag(Enums.FlagsToSet.kbf));
        //        xlCtrlOptions.Add(ViewerControls.XlWorktime, _options.HasFlag(Enums.FlagsToSet.worktime));
        //        xlCtrlOptions.Add(ViewerControls.XlOk, true);
        //        xlCtrlOptions.Add(ViewerControls.XlCancel, true);
        //        xlCtrlOptions.Add(ViewerControls.XlReminder, _options.HasFlag(Enums.FlagsToSet.reminder));
        //        xlCtrlOptions.Add(ViewerControls.XlDuedate, _options.HasFlag(Enums.FlagsToSet.duedate));
        //        xlCtrlOptions.Add(ViewerControls.XlScWaiting, _options.HasFlag(Enums.FlagsToSet.all));
        //        xlCtrlOptions.Add(ViewerControls.XlScUnprocessed, _options.HasFlag(Enums.FlagsToSet.all));
        //        xlCtrlOptions.Add(ViewerControls.XlScNews, _options.HasFlag(Enums.FlagsToSet.all));
        //        xlCtrlOptions.Add(ViewerControls.XlScEmail, _options.HasFlag(Enums.FlagsToSet.all));
        //        xlCtrlOptions.Add(ViewerControls.XlScReadingbusiness, _options.HasFlag(Enums.FlagsToSet.all));
        //        xlCtrlOptions.Add(ViewerControls.XlScCalls, _options.HasFlag(Enums.FlagsToSet.all));
        //        xlCtrlOptions.Add(ViewerControls.XlScInternet, _options.HasFlag(Enums.FlagsToSet.all));
        //        xlCtrlOptions.Add(ViewerControls.XlScPreread, _options.HasFlag(Enums.FlagsToSet.all));
        //        xlCtrlOptions.Add(ViewerControls.XlScMeeting, _options.HasFlag(Enums.FlagsToSet.all));
        //        xlCtrlOptions.Add(ViewerControls.XlScPersonal, _options.HasFlag(Enums.FlagsToSet.all));
        //        xlCtrlOptions.Add(ViewerControls.XlScBullpin, _options.HasFlag(Enums.FlagsToSet.all));
        //        xlCtrlOptions.Add(ViewerControls.XlScToday, _options.HasFlag(Enums.FlagsToSet.all));
        //    }
        //    return xlCtrlOptions;
        //}

        internal Dictionary<Label, bool> GetOptionsLookup(int group)
        {
            return GetControlRelationships()
                .Where(x => x.Group == group)
                .Select(x => new KeyValuePair<Label, bool>(x.Accelerator, x.Active))
                .ToDictionary();
        }

        internal Dictionary<Label, bool> GetOptionsLookup()
        {
            return GetControlRelationships()
                .Select(x => new KeyValuePair<Label, bool>(x.Accelerator, x.Active))
                .ToDictionary();
        }

        //internal Dictionary<Label, string> CreateCaptionLookup()
        //{
        //    var xlCtrlCaptions = new Dictionary<Label, string>();
        //    {
        //        xlCtrlCaptions.Add(ViewerControls.XlTopic, ViewerControls.LblTopic.Text);
        //        xlCtrlCaptions.Add(ViewerControls.XlProject, ViewerControls.LblProject.Text);
        //        xlCtrlCaptions.Add(ViewerControls.XlPeople, ViewerControls.LblPeople.Text);
        //        xlCtrlCaptions.Add(ViewerControls.XlContext, ViewerControls.LblContext.Text);
        //        xlCtrlCaptions.Add(ViewerControls.XlTaskname, ViewerControls.LblTaskname.Text);
        //        xlCtrlCaptions.Add(ViewerControls.XlImportance, ViewerControls.LblPriority.Text);
        //        xlCtrlCaptions.Add(ViewerControls.XlKanban, ViewerControls.LblKbf.Text);
        //        xlCtrlCaptions.Add(ViewerControls.XlWorktime, ViewerControls.LblDuration.Text);
        //        xlCtrlCaptions.Add(ViewerControls.XlOk, ViewerControls.OKButton.Text);
        //        xlCtrlCaptions.Add(ViewerControls.XlCancel, ViewerControls.Cancel_Button.Text);
        //        xlCtrlCaptions.Add(ViewerControls.XlReminder, ViewerControls.LblReminder.Text);
        //        xlCtrlCaptions.Add(ViewerControls.XlDuedate, ViewerControls.LblDuedate.Text);

        //        xlCtrlCaptions.Add(ViewerControls.XlScWaiting, ViewerControls.ShortcutWaitingFor.Text);
        //        xlCtrlCaptions.Add(ViewerControls.XlScUnprocessed, ViewerControls.ShortcutUnprocessed.Text);
        //        xlCtrlCaptions.Add(ViewerControls.XlScNews, ViewerControls.ShortcutNews.Text);
        //        xlCtrlCaptions.Add(ViewerControls.XlScEmail, ViewerControls.ShortcutEmail.Text);
        //        xlCtrlCaptions.Add(ViewerControls.XlScReadingbusiness, ViewerControls.ShortcutReadingBusiness.Text);
        //        xlCtrlCaptions.Add(ViewerControls.XlScCalls, ViewerControls.ShortcutCalls.Text);
        //        xlCtrlCaptions.Add(ViewerControls.XlScInternet, ViewerControls.ShortcutInternet.Text);
        //        xlCtrlCaptions.Add(ViewerControls.XlScPreread, ViewerControls.ShortcutPreRead.Text);
        //        xlCtrlCaptions.Add(ViewerControls.XlScMeeting, ViewerControls.ShortcutMeeting.Text);
        //        xlCtrlCaptions.Add(ViewerControls.XlScPersonal, ViewerControls.ShortcutPersonal.Text);
        //        xlCtrlCaptions.Add(ViewerControls.XlScBullpin, ViewerControls.CbxBullpin.Text);
        //        xlCtrlCaptions.Add(ViewerControls.XlScToday, ViewerControls.CbxToday.Text);
        //    }
        //    return xlCtrlCaptions;
        //}

        internal Dictionary<Label, string> GetCaptionLookup(int group)
        {
            return GetControlRelationships()
                .Where(x => x.Group == group)
                .Select(x => new KeyValuePair<Label, string>(x.Accelerator, x.Caption))
                .ToDictionary();
        }

        internal Dictionary<Label, string> GetCaptionLookup()
        {
            return GetControlRelationships()
                .Select(x => new KeyValuePair<Label, string>(x.Accelerator, x.Caption))
                .ToDictionary();
        }

        //internal Dictionary<Label, Control> CreateControlLookup()
        //{
        //    var xlCtrlLookup = new Dictionary<Label, Control>();
        //    {
        //        xlCtrlLookup.Add(ViewerControls.XlTopic, ViewerControls.LblTopic);
        //        xlCtrlLookup.Add(ViewerControls.XlProject, ViewerControls.LblProject);
        //        xlCtrlLookup.Add(ViewerControls.XlPeople, ViewerControls.LblPeople);
        //        xlCtrlLookup.Add(ViewerControls.XlContext, ViewerControls.LblContext);
        //        xlCtrlLookup.Add(ViewerControls.XlTaskname, ViewerControls.TaskName);
        //        xlCtrlLookup.Add(ViewerControls.XlImportance, ViewerControls.PriorityBox);
        //        xlCtrlLookup.Add(ViewerControls.XlKanban, ViewerControls.KbSelector);
        //        xlCtrlLookup.Add(ViewerControls.XlWorktime, ViewerControls.Duration);
        //        xlCtrlLookup.Add(ViewerControls.XlOk, ViewerControls.OKButton);
        //        xlCtrlLookup.Add(ViewerControls.XlCancel, ViewerControls.Cancel_Button);
        //        xlCtrlLookup.Add(ViewerControls.XlReminder, ViewerControls.DtReminder);
        //        xlCtrlLookup.Add(ViewerControls.XlDuedate, ViewerControls.DtDuedate);

        //        xlCtrlLookup.Add(ViewerControls.XlScWaiting, ViewerControls.ShortcutWaitingFor);
        //        xlCtrlLookup.Add(ViewerControls.XlScUnprocessed, ViewerControls.ShortcutUnprocessed);
        //        xlCtrlLookup.Add(ViewerControls.XlScNews, ViewerControls.ShortcutNews);
        //        xlCtrlLookup.Add(ViewerControls.XlScEmail, ViewerControls.ShortcutEmail);
        //        xlCtrlLookup.Add(ViewerControls.XlScReadingbusiness, ViewerControls.ShortcutReadingBusiness);
        //        xlCtrlLookup.Add(ViewerControls.XlScCalls, ViewerControls.ShortcutCalls);
        //        xlCtrlLookup.Add(ViewerControls.XlScInternet, ViewerControls.ShortcutInternet);
        //        xlCtrlLookup.Add(ViewerControls.XlScPreread, ViewerControls.ShortcutPreRead);
        //        xlCtrlLookup.Add(ViewerControls.XlScMeeting, ViewerControls.ShortcutMeeting);
        //        xlCtrlLookup.Add(ViewerControls.XlScPersonal, ViewerControls.ShortcutPersonal);
        //        xlCtrlLookup.Add(ViewerControls.XlScBullpin, ViewerControls.CbxBullpin);
        //        xlCtrlLookup.Add(ViewerControls.XlScToday, ViewerControls.CbxToday);
        //    }
        //    return xlCtrlLookup;
        //}

        internal Dictionary<Label, Control> GetControlLookup(int group)
        {
            return GetControlRelationships()
                .Where(x => x.Group == group)
                .Select(x => new KeyValuePair<Label, Control>(x.Accelerator, x.Control))
                .ToDictionary();
        }

        internal Dictionary<Label, Control> GetControlLookup()
        {
            return GetControlRelationships()
                .Select(x => new KeyValuePair<Label, Control>(x.Accelerator, x.Control))
                .ToDictionary();
        }

        private Dictionary<Enums.FlagsToSet, List<Control>> _optionsGroups;
        internal Dictionary<Enums.FlagsToSet, List<Control>> OptionsGroups
        {
            get
            {
                if (_optionsGroups is null)
                {
                    _optionsGroups = new()
                    {
                        {
                            Enums.FlagsToSet.Context,
                            new List<Control>
                            {
                                ViewerControls.CategorySelection,
                                ViewerControls.LblContext,
                            }
                        },
                        {
                            Enums.FlagsToSet.Topics,
                            new List<Control>
                            {
                                ViewerControls.TopicSelection,
                                ViewerControls.LblTopic,
                            }
                        },
                        {
                            Enums.FlagsToSet.Projects,
                            new List<Control>
                            {
                                ViewerControls.ProjectSelection,
                                ViewerControls.LblProject,
                            }
                        },
                        {
                            Enums.FlagsToSet.People,
                            new List<Control>
                            {
                                ViewerControls.PeopleSelection,
                                ViewerControls.LblPeople,
                            }
                        },
                        {
                            Enums.FlagsToSet.Taskname,
                            new List<Control>
                            {
                                ViewerControls.TaskName,
                                ViewerControls.LblTaskname,
                            }
                        },
                        {
                            Enums.FlagsToSet.Priority,
                            new List<Control>
                            {
                                ViewerControls.PriorityBox,
                                ViewerControls.LblPriority,
                            }
                        },
                        {
                            Enums.FlagsToSet.Kbf,
                            new List<Control> { ViewerControls.KbSelector, ViewerControls.LblKbf }
                        },
                        {
                            Enums.FlagsToSet.Worktime,
                            new List<Control>
                            {
                                ViewerControls.Duration,
                                ViewerControls.LblDuration,
                            }
                        },
                        {
                            Enums.FlagsToSet.Reminder,
                            new List<Control>
                            {
                                ViewerControls.DtReminder,
                                ViewerControls.LblReminder,
                            }
                        },
                        {
                            Enums.FlagsToSet.DueDate,
                            new List<Control>
                            {
                                ViewerControls.DtDuedate,
                                ViewerControls.LblDuedate,
                            }
                        },
                        {
                            Enums.FlagsToSet.All,
                            new List<Control>
                            {
                                ViewerControls.ShortcutMeeting,
                                ViewerControls.ShortcutCalls,
                                ViewerControls.ShortcutPersonal,
                                ViewerControls.ShortcutEmail,
                                ViewerControls.ShortcutInternet,
                                ViewerControls.ShortcutReadingBusiness,
                                ViewerControls.ShortcutNews,
                                ViewerControls.ShortcutUnprocessed,
                                ViewerControls.ShortcutWaitingFor,
                                ViewerControls.ShortcutPreRead,
                            }
                        },
                    };
                }
                return _optionsGroups;
            }
        }

        private IEnumerable<TipsController> _navTips;
        internal IEnumerable<TipsController> NavTips
        {
            get =>
                _navTips ??= new List<TipsController>
                {
                    new TipsController(ViewerControls.XlSector1, 0),
                    new TipsController(ViewerControls.XlSector2, 0),
                    new TipsController(ViewerControls.XlSector3, 0),
                    new TipsController(ViewerControls.XlSector4, 0),
                    new TipsController(ViewerControls.C1S1, 1),
                    new TipsController(ViewerControls.C3S1, 1),
                    new TipsController(ViewerControls.C4S1, 1),
                    new TipsController(ViewerControls.C2S2, 2),
                    new TipsController(ViewerControls.C3S2, 2),
                    new TipsController(ViewerControls.C4S2, 2),
                    new TipsController(ViewerControls.C2S3, 3),
                    new TipsController(ViewerControls.C3S3, 3),
                    new TipsController(ViewerControls.C4S3, 3),
                    new TipsController(ViewerControls.C2S4, 4),
                    new TipsController(ViewerControls.C3S4, 4),
                };
        }
    }
}
