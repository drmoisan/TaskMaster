using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Moq;
using TaskVisualization;

namespace TaskVisualization.Test
{
    /// <summary>
    /// STA-only support fixture for the control-identity tests. It constructs real, never-shown,
    /// in-memory WinForms controls and exposes them as an <see cref="ITaskViewerControls"/> surface
    /// (via a <see cref="Mock{T}"/> whose getters return the real controls) layered on the same
    /// mock that provides the primitive <see cref="ITaskViewer"/> facade.
    /// </summary>
    /// <remarks>
    /// Seam-infeasibility rationale (epic refinement condition a): the control-map and accelerator
    /// regions key their lookup dictionaries on real control object identity, and
    /// <see cref="UtilitiesCS.HelperClasses.ToolTips.TipsController"/> throws unless its label has a
    /// real parented <see cref="TableLayoutPanel"/>/<see cref="Panel"/> parent, so no mock/stub can
    /// substitute the logic under test. Every accelerator label is parented in a real
    /// <see cref="TableLayoutPanel"/> (each in its own styled column) so <c>NavTips</c> construction
    /// and the column-toggle paths run with no shown window and no message pump. No
    /// <see cref="Form"/>-derived type is constructed (condition d); nothing is shown and no pump is
    /// used (condition c); all controls are disposed via <see cref="Dispose"/>.
    /// </remarks>
    public sealed class StaControlHarness : IDisposable
    {
        public Mock<ITaskViewer> Viewer { get; }
        public Mock<ITaskViewerControls> Controls { get; }
        public ITaskViewer Object => Viewer.Object;

        private readonly TableLayoutPanel _tlp = new TableLayoutPanel();
        private readonly List<Control> _created = new List<Control>();
        private readonly Dictionary<string, Control> _byName = new Dictionary<string, Control>();
        private int _nextColumn;

        // Typed accessors the tests assert against.
        public Label XlProject => (Label)_byName[nameof(ITaskViewerControls.XlProject)];
        public Label XlOk => (Label)_byName[nameof(ITaskViewerControls.XlOk)];
        public Label XlScBullpin => (Label)_byName[nameof(ITaskViewerControls.XlScBullpin)];
        public Label C2S3 => (Label)_byName[nameof(ITaskViewerControls.C2S3)];
        public CheckBox CbxBullpin => (CheckBox)_byName[nameof(ITaskViewerControls.CbxBullpin)];
        public Button OKButton => (Button)_byName[nameof(ITaskViewerControls.OKButton)];

        /// <summary>The shared parent panel; its BackColor is toggled by the group-nav paths.</summary>
        public Color TlpBackColor => _tlp.BackColor;

        /// <summary>Resolves a control by its <see cref="ITaskViewerControls"/> member name.</summary>
        public Control ControlNamed(string name) => _byName[name];

        public StaControlHarness()
        {
            _tlp.ColumnCount = 80;
            for (int i = 0; i < _tlp.ColumnCount; i++)
            {
                _tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 20f));
            }

            Viewer = new Mock<ITaskViewer>();
            Viewer.SetupAllProperties();
            Controls = Viewer.As<ITaskViewerControls>();
            SetupControls();
            Viewer.Setup(v => v.InvokeRequired).Returns(false);

            Viewer.Object.ContextText = "[Category Label]";
            Viewer.Object.PeopleText = "[Assigned People Flagged]";
            Viewer.Object.ProjectText = "[ Projects Flagged ]";
            Viewer.Object.TopicText = "[Other Topics Tagged]";
        }

        private T Add<T>(string name)
            where T : Control, new()
        {
            var control = new T();
            // DateTimePicker.Text must parse as a date; its text is never used as a caption, so it
            // is left at its default. All other controls carry their member name as a caption.
            if (!(control is DateTimePicker))
            {
                control.Text = name;
            }
            int column = _nextColumn++;
            _tlp.Controls.Add(control, column, 0);
            _tlp.SetColumn(control, column);
            _created.Add(control);
            _byName[name] = control;
            return control;
        }

        private void L(string name) =>
            Controls.Setup(GetterFor<Label>(name)).Returns(Add<Label>(name));

        private void B(string name) =>
            Controls.Setup(GetterFor<Button>(name)).Returns(Add<Button>(name));

        private void C(string name) =>
            Controls.Setup(GetterFor<CheckBox>(name)).Returns(Add<CheckBox>(name));

        private void T(string name) =>
            Controls.Setup(GetterFor<TextBox>(name)).Returns(Add<TextBox>(name));

        private void Cb(string name) =>
            Controls.Setup(GetterFor<ComboBox>(name)).Returns(Add<ComboBox>(name));

        private void D(string name) =>
            Controls.Setup(GetterFor<DateTimePicker>(name)).Returns(Add<DateTimePicker>(name));

        // Builds a getter expression for the named ITaskViewerControls member.
        private static System.Linq.Expressions.Expression<
            Func<ITaskViewerControls, TReturn>
        > GetterFor<TReturn>(string name)
        {
            var param = System.Linq.Expressions.Expression.Parameter(
                typeof(ITaskViewerControls),
                "c"
            );
            var property = System.Linq.Expressions.Expression.Property(param, name);
            return System.Linq.Expressions.Expression.Lambda<Func<ITaskViewerControls, TReturn>>(
                property,
                param
            );
        }

        private void SetupControls()
        {
            foreach (
                var n in new[]
                {
                    "XlSector1",
                    "XlSector2",
                    "XlSector3",
                    "XlSector4",
                    "C1S1",
                    "C3S1",
                    "C4S1",
                    "C2S2",
                    "C3S2",
                    "C4S2",
                    "C2S3",
                    "C3S3",
                    "C4S3",
                    "C2S4",
                    "C3S4",
                    "XlTopic",
                    "XlProject",
                    "XlPeople",
                    "XlContext",
                    "XlTaskname",
                    "XlImportance",
                    "XlKanban",
                    "XlWorktime",
                    "XlReminder",
                    "XlDuedate",
                    "XlOk",
                    "XlCancel",
                    "XlAutotag",
                    "XlScWaiting",
                    "XlScUnprocessed",
                    "XlScNews",
                    "XlScEmail",
                    "XlScReadingbusiness",
                    "XlScCalls",
                    "XlScInternet",
                    "XlScPreread",
                    "XlScMeeting",
                    "XlScPersonal",
                    "XlScBullpin",
                    "XlScToday",
                    "LblTopic",
                    "LblProject",
                    "LblPeople",
                    "LblContext",
                    "LblTaskname",
                    "LblPriority",
                    "LblKbf",
                    "LblDuration",
                    "LblReminder",
                    "LblDuedate",
                    "CategorySelection",
                    "PeopleSelection",
                    "ProjectSelection",
                    "TopicSelection",
                }
            )
            {
                L(n);
            }

            T("TaskName");
            Cb("PriorityBox");
            Cb("KbSelector");
            T("Duration");
            D("DtReminder");
            D("DtDuedate");
            B("OKButton");
            B("Cancel_Button");
            B("AutoTagButton");
            B("ShortcutWaitingFor");
            B("ShortcutUnprocessed");
            B("ShortcutNews");
            B("ShortcutEmail");
            B("ShortcutReadingBusiness");
            B("ShortcutCalls");
            B("ShortcutInternet");
            B("ShortcutPreRead");
            B("ShortcutMeeting");
            B("ShortcutPersonal");
            C("CbxBullpin");
            C("CbxToday");
        }

        public void Dispose()
        {
            foreach (var control in _created)
            {
                control.Dispose();
            }
            _tlp.Dispose();
        }
    }
}
