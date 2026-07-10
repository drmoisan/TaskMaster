using System.Windows.Forms;
using Moq;
using TaskVisualization;

namespace TaskVisualization.Test
{
    /// <summary>
    /// Builds a fully mocked <see cref="ITaskViewer"/> for the non-STA controller tests. The
    /// primitive facade is stubbed via <c>SetupAllProperties()</c> (with an explicit
    /// <c>InvokeRequired == false</c>), and the same mock also implements
    /// <see cref="ITaskViewerControls"/> returning fixed Moq-mocked controls so
    /// <see cref="TaskController"/> can be constructed (its constructor eagerly builds the
    /// accelerator lookups from the control surface) without a live form or real controls.
    /// </summary>
    /// <remarks>
    /// The mocked controls carry no window handle and are never shown; they exist only to give
    /// the constructor stable, distinct control identities to key its lookup dictionaries on.
    /// Tests that must assert real-control behavior (visibility, back-color, TipsController) use
    /// the STA harness instead.
    /// </remarks>
    public sealed class MoqTaskViewer
    {
        public Mock<ITaskViewer> Mock { get; }
        public Mock<ITaskViewerControls> Controls { get; }

        public ITaskViewer Object => Mock.Object;

        public MoqTaskViewer()
        {
            Mock = new Mock<ITaskViewer>();

            // Stub the primitive facade first. Then add the companion control interface (still
            // before any .Object access) and set up its getters explicitly. SetupAllProperties
            // must precede the control setups, otherwise it would re-stub them as null-returning
            // auto-properties and override the fixed control instances.
            Mock.SetupAllProperties();

            Controls = Mock.As<ITaskViewerControls>();
            SetupControls(Controls);

            Mock.Setup(v => v.InvokeRequired).Returns(false);

            // Seed the four category facade texts with their placeholder values so
            // AnyCategorySelected is false by default; individual tests override as needed.
            Mock.Object.ContextText = "[Category Label]";
            Mock.Object.PeopleText = "[Assigned People Flagged]";
            Mock.Object.ProjectText = "[ Projects Flagged ]";
            Mock.Object.TopicText = "[Other Topics Tagged]";
        }

        // Each factory returns a fixed mock control whose Text getter yields a non-empty
        // single-character caption. A non-null, non-empty caption is required because the
        // constructor computes the navigation lookup from caption[0].
        private static Label L()
        {
            var m = new Mock<Label>();
            m.Setup(x => x.Text).Returns("X");
            return m.Object;
        }

        private static Button B()
        {
            var m = new Mock<Button>();
            m.Setup(x => x.Text).Returns("X");
            return m.Object;
        }

        private static CheckBox C()
        {
            var m = new Mock<CheckBox>();
            m.Setup(x => x.Text).Returns("X");
            return m.Object;
        }

        private static TextBox T()
        {
            var m = new Mock<TextBox>();
            m.Setup(x => x.Text).Returns("X");
            return m.Object;
        }

        private static ComboBox Cb()
        {
            var m = new Mock<ComboBox>();
            m.Setup(x => x.Text).Returns("X");
            return m.Object;
        }

        private static DateTimePicker D()
        {
            var m = new Mock<DateTimePicker>();
            m.Setup(x => x.Text).Returns("X");
            return m.Object;
        }

        private static void SetupControls(Mock<ITaskViewerControls> c)
        {
            c.Setup(x => x.XlSector1).Returns(L());
            c.Setup(x => x.XlSector2).Returns(L());
            c.Setup(x => x.XlSector3).Returns(L());
            c.Setup(x => x.XlSector4).Returns(L());
            c.Setup(x => x.C1S1).Returns(L());
            c.Setup(x => x.C3S1).Returns(L());
            c.Setup(x => x.C4S1).Returns(L());
            c.Setup(x => x.C2S2).Returns(L());
            c.Setup(x => x.C3S2).Returns(L());
            c.Setup(x => x.C4S2).Returns(L());
            c.Setup(x => x.C2S3).Returns(L());
            c.Setup(x => x.C3S3).Returns(L());
            c.Setup(x => x.C4S3).Returns(L());
            c.Setup(x => x.C2S4).Returns(L());
            c.Setup(x => x.C3S4).Returns(L());
            c.Setup(x => x.XlTopic).Returns(L());
            c.Setup(x => x.XlProject).Returns(L());
            c.Setup(x => x.XlPeople).Returns(L());
            c.Setup(x => x.XlContext).Returns(L());
            c.Setup(x => x.XlTaskname).Returns(L());
            c.Setup(x => x.XlImportance).Returns(L());
            c.Setup(x => x.XlKanban).Returns(L());
            c.Setup(x => x.XlWorktime).Returns(L());
            c.Setup(x => x.XlReminder).Returns(L());
            c.Setup(x => x.XlDuedate).Returns(L());
            c.Setup(x => x.XlOk).Returns(L());
            c.Setup(x => x.XlCancel).Returns(L());
            c.Setup(x => x.XlAutotag).Returns(L());
            c.Setup(x => x.XlScWaiting).Returns(L());
            c.Setup(x => x.XlScUnprocessed).Returns(L());
            c.Setup(x => x.XlScNews).Returns(L());
            c.Setup(x => x.XlScEmail).Returns(L());
            c.Setup(x => x.XlScReadingbusiness).Returns(L());
            c.Setup(x => x.XlScCalls).Returns(L());
            c.Setup(x => x.XlScInternet).Returns(L());
            c.Setup(x => x.XlScPreread).Returns(L());
            c.Setup(x => x.XlScMeeting).Returns(L());
            c.Setup(x => x.XlScPersonal).Returns(L());
            c.Setup(x => x.XlScBullpin).Returns(L());
            c.Setup(x => x.XlScToday).Returns(L());
            c.Setup(x => x.LblTopic).Returns(L());
            c.Setup(x => x.LblProject).Returns(L());
            c.Setup(x => x.LblPeople).Returns(L());
            c.Setup(x => x.LblContext).Returns(L());
            c.Setup(x => x.LblTaskname).Returns(L());
            c.Setup(x => x.LblPriority).Returns(L());
            c.Setup(x => x.LblKbf).Returns(L());
            c.Setup(x => x.LblDuration).Returns(L());
            c.Setup(x => x.LblReminder).Returns(L());
            c.Setup(x => x.LblDuedate).Returns(L());
            c.Setup(x => x.CategorySelection).Returns(L());
            c.Setup(x => x.PeopleSelection).Returns(L());
            c.Setup(x => x.ProjectSelection).Returns(L());
            c.Setup(x => x.TopicSelection).Returns(L());
            c.Setup(x => x.TaskName).Returns(T());
            c.Setup(x => x.PriorityBox).Returns(Cb());
            c.Setup(x => x.KbSelector).Returns(Cb());
            c.Setup(x => x.Duration).Returns(T());
            c.Setup(x => x.DtReminder).Returns(D());
            c.Setup(x => x.DtDuedate).Returns(D());
            c.Setup(x => x.OKButton).Returns(B());
            c.Setup(x => x.Cancel_Button).Returns(B());
            c.Setup(x => x.AutoTagButton).Returns(B());
            c.Setup(x => x.ShortcutWaitingFor).Returns(B());
            c.Setup(x => x.ShortcutUnprocessed).Returns(B());
            c.Setup(x => x.ShortcutNews).Returns(B());
            c.Setup(x => x.ShortcutEmail).Returns(B());
            c.Setup(x => x.ShortcutReadingBusiness).Returns(B());
            c.Setup(x => x.ShortcutCalls).Returns(B());
            c.Setup(x => x.ShortcutInternet).Returns(B());
            c.Setup(x => x.ShortcutPreRead).Returns(B());
            c.Setup(x => x.ShortcutMeeting).Returns(B());
            c.Setup(x => x.ShortcutPersonal).Returns(B());
            c.Setup(x => x.CbxBullpin).Returns(C());
            c.Setup(x => x.CbxToday).Returns(C());
        }
    }
}
