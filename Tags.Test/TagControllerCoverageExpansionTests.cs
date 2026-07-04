using System;
using System.Collections.Generic;
using FluentAssertions;
using Microsoft.Office.Interop.Outlook;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UtilitiesCS;

namespace Tags.Test
{
    [TestClass]
    public class TagControllerCoverageExpansionTests
    {
        [TestMethod]
        [STAThread]
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
        [STAThread]
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
        [STAThread]
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
        [STAThread]
        public void FilterArchive_WhenAutoAssignerHasExclusions_RemovesMatchesCaseInsensitively()
        {
            var options = new SortedDictionary<string, bool>
            {
                ["Archive Choice"] = true,
                ["Current Choice"] = true,
            };
            var autoAssigner = new Mock<IAutoAssign>(MockBehavior.Loose);
            autoAssigner.SetupGet(x => x.FilterList).Returns(new List<string> { "archive choice" });

            using (var viewer = new TagViewer())
            {
                var controller = new TagController(
                    viewer,
                    options,
                    autoAssigner.Object,
                    new List<IPrefix> { CreatePrefix() },
                    "current@example.test"
                );

                controller.FilterArchive(options).Keys.Should().Equal("Current Choice");
                controller.ButtonAutoAssignActive.Should().BeFalse();
            }
        }

        [TestMethod]
        [STAThread]
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
        [STAThread]
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

        private static ControllerFixture CreateFixture(
            SortedDictionary<string, bool> options = null,
            IList<string> selections = null
        )
        {
            var viewer = new TagViewer();
            var controller = new TagController(
                viewer,
                options ?? new SortedDictionary<string, bool>(),
                selections,
                CreatePrefix()
            );

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

        private sealed class ControllerFixture : IDisposable
        {
            public ControllerFixture(TagViewer viewer, TagController controller)
            {
                Viewer = viewer;
                Controller = controller;
            }

            public TagViewer Viewer { get; }

            public TagController Controller { get; }

            public void Dispose()
            {
                Viewer.Dispose();
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
