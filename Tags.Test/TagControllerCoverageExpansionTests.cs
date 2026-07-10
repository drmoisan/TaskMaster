using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using FluentAssertions;
using Microsoft.Office.Interop.Outlook;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Tags.Test.Fakes;
using UtilitiesCS;

namespace Tags.Test
{
    [TestClass]
    public class TagControllerCoverageExpansionTests
    {
        [TestMethod]
        public void AddOption_WhenNewDuplicateAndEmptyInputs_UpdatesSelectionState()
        {
            using (var fixture = CreateFixture())
            {
                fixture.Controller.AddOption("TagProgram Alpha", blClickTrue: true);
                fixture.Controller.GetSelections().Should().Equal("TagProgram Alpha");

                fixture.Controller.AddOption("TagProgram Alpha", blClickTrue: false);
                fixture.Controller.GetSelections().Should().BeEmpty();

                fixture.Controller.AddOption("", blClickTrue: true);
                fixture.Controller.GetSelections().Should().Equal("");
            }
        }

        [TestMethod]
        public void ToggleMethods_WhenOptionExists_AddRemoveAndUpdateSelectionState()
        {
            var options = new SortedDictionary<string, bool>
            {
                ["TagProgram Alpha"] = false,
                ["TagProgram Beta"] = true,
            };

            using (var fixture = CreateFixture(options))
            {
                fixture.Controller.SelectionAsString().Should().Be("TagProgram Beta");

                fixture.Controller.ToggleOn("TagProgram Alpha");
                fixture.Controller.ToggleOff("TagProgram Beta");
                fixture.Controller.GetSelections().Should().Equal("TagProgram Alpha");

                fixture.Controller.ToggleChoice("TagProgram Alpha");
                fixture.Controller.GetSelections().Should().BeEmpty();
            }
        }

        [TestMethod]
        public void SearchAndParse_WhenInputIsEmptyMissingOrWildcard_ReturnsExpectedMatches()
        {
            var options = new SortedDictionary<string, bool>
            {
                ["TagProgram Alpha"] = true,
                ["TagProgram Beta"] = false,
                ["Topic Gamma"] = true,
            };

            using (var fixture = CreateFixture(options))
            {
                fixture.Controller.ParseSearchStrings("   ").Should().BeEmpty();
                fixture
                    .Controller.ParseSearchStrings("Alpha*Gamma")
                    .Should()
                    .Equal("Alpha", "Gamma");

                fixture.Controller.Search(options, new List<string>()).Should().Equal(options);
                fixture
                    .Controller.Search(options, new List<string> { "tagprogram" })
                    .Keys.Should()
                    .Equal("TagProgram Alpha", "TagProgram Beta");
                fixture
                    .Controller.Search(options, new List<string> { "missing" })
                    .Should()
                    .BeEmpty();
            }
        }

        [TestMethod]
        public void FilterArchive_WhenAutoAssignerHasExclusions_RemovesMatchesCaseInsensitively()
        {
            var options = new SortedDictionary<string, bool>
            {
                ["Archive Choice"] = true,
                ["Current Choice"] = true,
            };
            var autoAssigner = new Mock<IAutoAssign>(MockBehavior.Loose);
            autoAssigner.SetupGet(x => x.FilterList).Returns(new List<string> { "archive choice" });

            var viewer = new FakeTagViewer();
            var prompt = new Mock<IUserPrompt>(MockBehavior.Loose);
            var controller = new TagController(
                viewer.Object,
                options,
                autoAssigner.Object,
                new List<IPrefix> { CreatePrefix() },
                "current@example.test",
                prompt: prompt.Object,
                drawFocus: _ => { }
            );

            try
            {
                controller.FilterArchive(options).Keys.Should().Equal("Current Choice");
                controller.ButtonAutoAssignActive.Should().BeFalse();
            }
            finally
            {
                DisposeOptions(viewer);
            }
        }

        [TestMethod]
        public void ResolvePrefix_WhenMissingOrInvalid_UsesDefaultOrThrows()
        {
            using (var fixture = CreateFixture())
            {
                fixture.Controller.IsPrefixMissing(CreatePrefix(), "Alpha").Should().BeTrue();
                fixture
                    .Controller.IsPrefixMissing(CreatePrefix(), "TagProgram Alpha")
                    .Should()
                    .BeFalse();
                fixture.Controller.IsPrefixMissing(CreatePrefix(), null).Should().BeTrue();

                fixture.Controller.ResolvePrefix(new List<IPrefix> { CreatePrefix() }, "Program");

                System.Action act = () =>
                    fixture.Controller.ResolvePrefix(
                        new List<IPrefix> { CreatePrefix() },
                        "Missing"
                    );
                act.Should().Throw<ArgumentException>();
            }
        }

        [TestMethod]
        public void FilterToSelected_AfterStateTransitions_ReloadsOnlySelectedControls()
        {
            var options = new SortedDictionary<string, bool>
            {
                ["TagProgram Alpha"] = true,
                ["TagProgram Beta"] = false,
            };

            using (var fixture = CreateFixture(options))
            {
                fixture.Controller.AddOption("TagProgram Gamma", blClickTrue: true);
                fixture.Controller.FilterToSelected();

                fixture
                    .Controller.GetSelections()
                    .Should()
                    .Equal("TagProgram Alpha", "TagProgram Gamma");
                fixture
                    .Controller.SelectionAsList()
                    .Should()
                    .Equal("TagProgram Alpha", "TagProgram Gamma");
            }
        }

        [TestMethod]
        public void LoadSelections_WhenExistingSelectionsUseBothForms_TogglesMatchingOptions()
        {
            var unprefixedOptions = new SortedDictionary<string, bool>
            {
                ["TagProgram Alpha"] = false,
            };
            using (var fixture = CreateFixture(unprefixedOptions, new List<string> { "Alpha" }))
            {
                fixture.Controller.GetSelections().Should().Equal("TagProgram Alpha");
            }

            var prefixedOptions = new SortedDictionary<string, bool>
            {
                ["TagProgram Alpha"] = true,
            };
            using (
                var fixture = CreateFixture(
                    prefixedOptions,
                    new List<string> { "TagProgram Alpha" }
                )
            )
            {
                fixture.Controller.GetSelections().Should().BeEmpty();
            }
        }

        [TestMethod]
        public void SearchAndReload_WhenFilterChanges_ReplacesVisibleCheckboxes()
        {
            var options = new SortedDictionary<string, bool>
            {
                ["TagProgram Alpha"] = true,
                ["TagProgram Beta"] = false,
                ["Topic Gamma"] = true,
            };

            using (var fixture = CreateFixture(options))
            {
                fixture.Viewer.SearchTextValue = "Beta";
                fixture.Controller.SearchAndReload();

                fixture
                    .Viewer.OptionControls.Select(control => control.Tag as string)
                    .Should()
                    .Equal("TagProgram Beta");
            }
        }

        [TestMethod]
        public void UpdateSelections_AfterFiltering_SynchronizesPrivateSelectionLists()
        {
            var options = new SortedDictionary<string, bool>
            {
                ["TagProgram Alpha"] = true,
                ["TagProgram Beta"] = false,
                ["TagProgram Gamma"] = true,
            };

            using (var fixture = CreateFixture(options))
            {
                fixture.Controller.FilterToSelected();
                fixture.Controller.UpdateSelections();

                // Selection state now lives on the controller's TagSelectionModel instance
                // (fields relocated from TagController in P3-T3), so read it from the model.
                var model = GetPrivateField<TagSelectionModel>(fixture.Controller, "_model");
                model.Selections.Should().Equal("TagProgram Alpha", "TagProgram Gamma");
                model.FilteredSelections.Should().Equal("TagProgram Alpha", "TagProgram Gamma");
            }
        }

        [TestMethod]
        public void SelectControlMethods_WhenPositionsChange_UpdateFocusIndexOrThrow()
        {
            var options = new SortedDictionary<string, bool>
            {
                ["TagProgram Alpha"] = false,
                ["TagProgram Beta"] = false,
            };

            using (var fixture = CreateFixture(options))
            {
                fixture.Controller.Select_First_Control();
                GetPrivateField<int>(fixture.Controller, "intFocus").Should().Be(0);

                fixture.Controller.Select_Last_Control();
                GetPrivateField<int>(fixture.Controller, "intFocus").Should().Be(1);

                fixture.Controller.Select_Ctrl_By_Position(-1);
                GetPrivateField<int>(fixture.Controller, "intFocus").Should().Be(-1);

                System.Action act = () => fixture.Controller.Select_Ctrl_By_Position(2);
                act.Should().Throw<ArgumentOutOfRangeException>();
            }
        }

        [TestMethod]
        public void HideArchive_WhenToggled_ReloadsFilteredAndOriginalOptions()
        {
            var options = new SortedDictionary<string, bool>
            {
                ["Archive Choice"] = true,
                ["Current Choice"] = true,
            };
            var autoAssigner = NewAutoAssigner(new List<string> { "Archive Choice" });

            using (
                var fixture = CreateAutoAssignFixture(
                    options,
                    autoAssigner.Object,
                    NewMailItem("mail")
                )
            )
            {
                fixture
                    .Viewer.OptionControls.Select(control => control.Tag as string)
                    .Should()
                    .Equal("Current Choice");

                fixture.Viewer.SetHideArchive(false);
                fixture
                    .Viewer.OptionControls.Select(control => control.Tag as string)
                    .Should()
                    .Equal("Archive Choice", "Current Choice");

                fixture.Viewer.SetHideArchive(true);
                fixture
                    .Viewer.OptionControls.Select(control => control.Tag as string)
                    .Should()
                    .Equal("Current Choice");
            }
        }

        [TestMethod]
        public async Task AutoAssignAction_WhenExistingAndNewAssignmentsReturned_UpdatesSelections()
        {
            var options = new SortedDictionary<string, bool> { ["TagProgram Existing"] = false };
            var autoAssigner = NewAutoAssigner(new List<string>());
            autoAssigner
                .Setup(x => x.AutoFindAsync(It.IsAny<object>()))
                .Returns(
                    Task.FromResult<IList<string>>(
                        new List<string> { "TagProgram Existing", "TagProgram New" }
                    )
                );

            using (
                var fixture = CreateAutoAssignFixture(
                    options,
                    autoAssigner.Object,
                    NewMailItem("auto")
                )
            )
            {
                // Await the extracted Task-returning action directly (no banned timer/delay wait).
                await fixture.Controller.ButtonAutoAssign_Action();

                fixture
                    .Controller.GetSelections()
                    .Should()
                    .Equal("TagProgram Existing", "TagProgram New");
                autoAssigner.Verify(x => x.AutoFindAsync(It.IsAny<object>()), Times.Once);
            }
        }

        private static ControllerFixture CreateFixture(
            SortedDictionary<string, bool> options = null,
            IList<string> selections = null
        )
        {
            var viewer = new FakeTagViewer();
            var prompt = new Mock<IUserPrompt>(MockBehavior.Loose);
            var controller = new TagController(
                viewer.Object,
                options ?? new SortedDictionary<string, bool>(),
                selections,
                CreatePrefix(),
                prompt.Object,
                _ => { }
            );

            return new ControllerFixture(viewer, controller);
        }

        private static ControllerFixture CreateAutoAssignFixture(
            SortedDictionary<string, bool> options,
            IAutoAssign autoAssigner,
            MailItem mailItem
        )
        {
            var viewer = new FakeTagViewer();
            var prompt = new Mock<IUserPrompt>(MockBehavior.Loose);
            var controller = new TagController(
                viewer.Object,
                options,
                autoAssigner,
                new List<IPrefix> { CreatePrefix() },
                "current@example.test",
                prefixKey: "Program",
                objItemObject: mailItem,
                prompt: prompt.Object,
                drawFocus: _ => { }
            );
            SetPrivateField(controller, "_isMail", true);
            controller.SetAutoAssignState(autoAssigner);

            return new ControllerFixture(viewer, controller);
        }

        private static IPrefix CreatePrefix() =>
            new TestPrefix
            {
                Key = "Program",
                Value = "TagProgram ",
                Color = OlCategoryColor.olCategoryColorNone,
                PrefixType = PrefixTypeEnum.Program,
                OlUserFieldName = "TagProgram",
            };

        private static Mock<IAutoAssign> NewAutoAssigner(IList<string> filterList)
        {
            var autoAssigner = new Mock<IAutoAssign>(MockBehavior.Loose);
            autoAssigner.SetupGet(x => x.FilterList).Returns(filterList);
            return autoAssigner;
        }

        private static MailItem NewMailItem(string entryId)
        {
            var mailItem = new Mock<MailItem>();
            mailItem.Setup(x => x.EntryID).Returns(entryId);
            return mailItem.Object;
        }

        private static void DisposeOptions(FakeTagViewer viewer)
        {
            foreach (var control in viewer.OptionControls.ToList())
            {
                control.Dispose();
            }
        }

        private static T GetPrivateField<T>(object target, string fieldName)
        {
            var field = target
                .GetType()
                .GetField(
                    fieldName,
                    BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public
                );
            field.Should().NotBeNull();
            return (T)field.GetValue(target);
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            var field = target
                .GetType()
                .GetField(
                    fieldName,
                    BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public
                );
            field.Should().NotBeNull();
            field.SetValue(target, value);
        }

        private sealed class ControllerFixture : IDisposable
        {
            public ControllerFixture(FakeTagViewer viewer, TagController controller)
            {
                Viewer = viewer;
                Controller = controller;
            }

            public FakeTagViewer Viewer { get; }

            public TagController Controller { get; }

            public void Dispose()
            {
                DisposeOptions(Viewer);
            }
        }

        private sealed class TestPrefix : IPrefix
        {
            public string Key { get; set; }

            public string Value { get; set; }

            public OlCategoryColor Color { get; set; }

            public PrefixTypeEnum PrefixType { get; set; }

            public string OlUserFieldName { get; set; }
        }
    }
}
