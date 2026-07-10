using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Office.Interop.Outlook;
using Moq;
using Tags;
using TaskVisualization;
using ToDoModel;
using UtilitiesCS;

namespace TaskVisualization.Test
{
    /// <summary>
    /// Deterministic builders for the doubles the <see cref="TaskController"/> tests need:
    /// seeded <see cref="ToDoDefaults"/>, an in-memory <see cref="ToDoItem"/> backed by a
    /// <see cref="MoqOlToDo"/> mail mock, mock <see cref="Categories"/>, and the injected seams
    /// (mock <see cref="ITagPromptService"/>, a capturing warning notifier, a mail-helper factory
    /// stub, and a mock <see cref="IAutoAssign"/>). No live forms, popups, or temp files are used;
    /// no <c>DateTime.Now</c>-derived value is asserted.
    /// </summary>
    public static class TaskControllerFixtures
    {
        private static readonly DateTime FixedDate = new DateTime(2020, 1, 1);

        public static ToDoDefaults Defaults() => new ToDoDefaults();

        public static ToDoItem BuildTodo(string categories = "")
        {
            var moq = new MoqOlToDo();
            var mail = moq.MailItemMock(
                "Test Subject",
                OlImportance.olImportanceNormal,
                FixedDate,
                FixedDate,
                OlFlagStatus.olFlagMarked,
                categories
            );
            return new ToDoItem(new OutlookItem(mail.Object));
        }

        public static List<ToDoItem> TodoList(int count = 1) =>
            Enumerable.Range(0, count).Select(_ => BuildTodo()).ToList();

        /// <summary>
        /// A deterministic mock <see cref="MailItem"/> (no live Outlook) for the tests that must
        /// supply a mail item to a <see cref="FlagChangeGroup"/> or double.
        /// </summary>
        public static MailItem MailMock() =>
            new MoqOlToDo()
                .MailItemMock(
                    "Test Subject",
                    OlImportance.olImportanceNormal,
                    FixedDate,
                    FixedDate,
                    OlFlagStatus.olFlagMarked,
                    ""
                )
                .Object;

        public static Categories MockCategories() =>
            new MoqOlToDo().MockCategories("Tag PROJECT TestProject");

        public static Mock<IAutoAssign> AutoAssign(IList<string> found = null)
        {
            var mock = new Mock<IAutoAssign>();
            mock.Setup(x => x.AutoFindAsync(It.IsAny<object>()))
                .ReturnsAsync(found ?? new List<string>());
            mock.Setup(x => x.AutoFind(It.IsAny<object>())).Returns(found ?? new List<string>());
            return mock;
        }

        public static Mock<ITagPromptService> TagPrompt(bool cancelled, string selection = "")
        {
            var mock = new Mock<ITagPromptService>();
            mock.Setup(x => x.Prompt(It.IsAny<TagPromptRequest>()))
                .Returns(new TagPromptResult(cancelled, selection));
            return mock;
        }

        /// <summary>
        /// A capturing warning notifier. Appends every message to <paramref name="captured"/>.
        /// </summary>
        public static Action<string> CapturingNotifier(List<string> captured) =>
            m => captured.Add(m);

        /// <summary>
        /// A mail-helper factory stub that never touches a live Outlook process. It returns a
        /// completed task whose result is produced by <paramref name="factory"/> (default null).
        /// </summary>
        public static Func<MailItem, Task<MailItemHelper>> MailHelperFactory(
            Func<MailItem, MailItemHelper> factory = null
        ) => m => Task.FromResult(factory != null ? factory(m) : null);

        /// <summary>
        /// A deterministic, fully-mocked <see cref="IApplicationGlobals"/> for the tests whose
        /// path constructs a <see cref="FlagChangeGroup"/> (which requires a non-null globals).
        /// </summary>
        public static IApplicationGlobals MockGlobals() => new MoqOlToDo().MockGlobals();

        /// <summary>
        /// Builds a <see cref="TaskController"/> over the supplied mocked viewer using the
        /// 11-parameter constructor, wiring the injected seams. Every dependency is a
        /// deterministic in-memory double; no live form, popup, Outlook process, or temp file is
        /// created. Callers pass distinct auto-assigners when a test must observe which assigner
        /// contributed a merge.
        /// </summary>
        public static TaskController BuildController(
            MoqTaskViewer view,
            Enums.FlagsToSet options = Enums.FlagsToSet.All,
            Mock<ITagPromptService> tagPrompt = null,
            List<string> warnings = null,
            Mock<IAutoAssign> autoAssign = null,
            Mock<IAutoAssign> projectAssign = null,
            Mock<IAutoAssign> contextAssign = null,
            IApplicationGlobals globals = null,
            Func<MailItem, Task<MailItemHelper>> mailHelperFactory = null,
            Action<string> setActiveTaskSubject = null
        ) =>
            BuildControllerOver(
                view.Object,
                options,
                tagPrompt,
                warnings,
                autoAssign,
                projectAssign,
                contextAssign,
                globals,
                mailHelperFactory,
                setActiveTaskSubject
            );

        /// <summary>
        /// Builds a <see cref="TaskController"/> via the 7-parameter constructor overload (the
        /// overload not used by <c>FlagTasks</c>), exercising that construction path.
        /// </summary>
        public static TaskController BuildController7(
            MoqTaskViewer view,
            Enums.FlagsToSet options = Enums.FlagsToSet.All
        )
        {
            var autoAssign = AutoAssign();
            return new TaskController(
                view.Object,
                MockCategories(),
                TodoList(1),
                Defaults(),
                autoAssign.Object,
                "user@test.com",
                options,
                TagPrompt(true).Object,
                CapturingNotifier(new List<string>()),
                MailHelperFactory()
            );
        }

        /// <summary>
        /// Builds a <see cref="TaskController"/> directly over any <see cref="ITaskViewer"/>
        /// implementation (used by the STA tests, which supply a viewer whose companion
        /// <see cref="ITaskViewerControls"/> surface returns real in-memory controls).
        /// </summary>
        public static TaskController BuildControllerOver(
            ITaskViewer viewer,
            Enums.FlagsToSet options = Enums.FlagsToSet.All,
            Mock<ITagPromptService> tagPrompt = null,
            List<string> warnings = null,
            Mock<IAutoAssign> autoAssign = null,
            Mock<IAutoAssign> projectAssign = null,
            Mock<IAutoAssign> contextAssign = null,
            IApplicationGlobals globals = null,
            Func<MailItem, Task<MailItemHelper>> mailHelperFactory = null,
            Action<string> setActiveTaskSubject = null
        )
        {
            var people = autoAssign ?? AutoAssign();
            return new TaskController(
                viewer,
                MockCategories(),
                TodoList(1),
                Defaults(),
                people.Object,
                (projectAssign ?? AutoAssign()).Object,
                (contextAssign ?? AutoAssign()).Object,
                s => s,
                "user@test.com",
                globals,
                options,
                (tagPrompt ?? TagPrompt(true)).Object,
                CapturingNotifier(warnings ?? new List<string>()),
                mailHelperFactory ?? MailHelperFactory(),
                setActiveTaskSubject
            );
        }
    }
}
