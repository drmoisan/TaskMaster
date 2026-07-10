using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using TaskVisualization;
using UtilitiesCS;
using UtilitiesCS.ReusableTypeClasses;

namespace TaskVisualization.Test
{
    /// <summary>
    /// Unit tests for the host-neutral members of <see cref="AutoCreateProject"/>.
    /// Interop seams are injected/stubbed so no live Outlook process, popup, or temp
    /// file is used.
    /// </summary>
    [TestClass]
    public class AutoCreateProjectTests
    {
        private static Mock<IApplicationGlobals> BuildGlobals(
            IEnumerable<IProjectEntry> projects = null,
            ScDictionary<string, string> programInfo = null,
            IList<string> categoryFilters = null,
            Mock<IIDList> idList = null,
            Mock<IProjectData> projInfoMock = null
        )
        {
            var td = new Mock<IToDoObjects>();

            var projInfo = projInfoMock ?? new Mock<IProjectData>();
            var projList = (projects ?? Enumerable.Empty<IProjectEntry>()).ToList();
            projInfo
                .As<IEnumerable<IProjectEntry>>()
                .Setup(x => x.GetEnumerator())
                .Returns(() => projList.GetEnumerator());
            td.Setup(x => x.ProjInfo).Returns(projInfo.Object);

            td.Setup(x => x.ProgramInfo).Returns(programInfo ?? new ScDictionary<string, string>());

            var cfList = categoryFilters ?? new List<string>();
            td.Setup(x => x.CategoryFilters).Returns(BuildFilterList(cfList));

            td.Setup(x => x.IDList).Returns((idList ?? new Mock<IIDList>()).Object);

            var globals = new Mock<IApplicationGlobals>();
            globals.Setup(x => x.TD).Returns(td.Object);
            return globals;
        }

        private static ISerializableList<string> BuildFilterList(IList<string> items)
        {
            var cf = new Mock<ISerializableList<string>>();
            cf.Setup(x => x.Count).Returns(() => items.Count);
            cf.Setup(x => x[It.IsAny<int>()]).Returns((int i) => items[i]);
            cf.Setup(x => x.CopyTo(It.IsAny<string[]>(), It.IsAny<int>()))
                .Callback((string[] arr, int i) => items.CopyTo(arr, i));
            cf.As<IEnumerable<string>>()
                .Setup(x => x.GetEnumerator())
                .Returns(() => items.GetEnumerator());
            return cf.Object;
        }

        private static IProjectEntry ProjectEntry(string programId, string projectId)
        {
            var entry = new Mock<IProjectEntry>();
            entry.Setup(x => x.ProgramID).Returns(programId);
            entry.Setup(x => x.ProjectID).Returns(projectId);
            return entry.Object;
        }

        [TestMethod]
        public void StripPrefix_PrefixPresent_RemovesPrefix()
        {
            var sut = new AutoCreateProject(BuildGlobals().Object);
            sut.StripPrefix("Tag ", "Tag PROJECT").Should().Be("PROJECT");
        }

        [TestMethod]
        public void StripPrefix_EmptyPrefix_ReturnsCategoryUnchanged()
        {
            var sut = new AutoCreateProject(BuildGlobals().Object);
            sut.StripPrefix("", "Category").Should().Be("Category");
        }

        [TestMethod]
        public void StripPrefix_NullPrefix_ReturnsCategoryUnchanged()
        {
            var sut = new AutoCreateProject(BuildGlobals().Object);
            sut.StripPrefix(null, "Category").Should().Be("Category");
        }

        [TestMethod]
        public void StripPrefix_EmptyCategory_ReturnsEmpty()
        {
            var sut = new AutoCreateProject(BuildGlobals().Object);
            sut.StripPrefix("Tag ", "").Should().Be("");
        }

        [TestMethod]
        public void FilterList_ReturnsCategoryFilters()
        {
            var sut = new AutoCreateProject(
                BuildGlobals(categoryFilters: new List<string> { "a", "b" }).Object
            );
            sut.FilterList.Should().BeEquivalentTo(new[] { "a", "b" });
        }

        [TestMethod]
        public void AddChoicesToDict_Throws_NotImplemented()
        {
            var sut = new AutoCreateProject(BuildGlobals().Object);
            Action act = () => sut.AddChoicesToDict(null, null, null, null);
            act.Should().Throw<NotImplementedException>();
        }

        [TestMethod]
        public void AutoFind_Throws_NotImplemented()
        {
            var sut = new AutoCreateProject(BuildGlobals().Object);
            Action act = () => sut.AutoFind(null);
            act.Should().Throw<NotImplementedException>();
        }

        [TestMethod]
        public void TryAutoExtractProgram_MatchesLongestProgramFirst()
        {
            var programInfo = new ScDictionary<string, string>();
            programInfo["Alpha"] = "P1";
            programInfo["AlphaBeta"] = "P2";
            var sut = new AutoCreateProject(BuildGlobals(programInfo: programInfo).Object);

            sut.TryAutoExtractProgram("ZAlphaBetaZ", out var programName).Should().BeTrue();
            programName.Should().Be("AlphaBeta");
        }

        [TestMethod]
        public void TryAutoExtractProgram_NoMatch_ReturnsFalse()
        {
            var programInfo = new ScDictionary<string, string>();
            programInfo["Alpha"] = "P1";
            var sut = new AutoCreateProject(BuildGlobals(programInfo: programInfo).Object);

            sut.TryAutoExtractProgram("Nothing", out var programName).Should().BeFalse();
            programName.Should().BeNull();
        }

        [TestMethod]
        public void GetNextProjectID_WithMatchingProject_SeedsFromHighestProjectId()
        {
            var idList = new Mock<IIDList>();
            idList.Setup(x => x.GetNextToDoID("P105")).Returns("P106");
            var sut = new AutoCreateProject(
                BuildGlobals(
                    projects: new[] { ProjectEntry("P1", "P103"), ProjectEntry("P1", "P105") },
                    idList: idList
                ).Object
            );

            sut.GetNextProjectID("P1").Should().Be("P106");
            idList.Verify(x => x.GetNextToDoID("P105"), Times.Once);
        }

        [TestMethod]
        public void GetNextProjectID_NoMatchingProject_SeedsFromProgramIdZeroZero()
        {
            var idList = new Mock<IIDList>();
            idList.Setup(x => x.GetNextToDoID("P100")).Returns("P101");
            var sut = new AutoCreateProject(BuildGlobals(idList: idList).Object);

            sut.GetNextProjectID("P1").Should().Be("P101");
            idList.Verify(x => x.GetNextToDoID("P100"), Times.Once);
        }

        [TestMethod]
        public void AddColorCategory_ExistingProject_ReturnsNull()
        {
            var projInfo = new Mock<IProjectData>();
            projInfo.Setup(x => x.Contains_ProjectName(It.IsAny<string>())).Returns(true);
            var globals = BuildGlobals(projInfoMock: projInfo);
            var sut = new AutoCreateProject(globals.Object);

            var prefix = new Mock<IPrefix>();
            prefix.Setup(x => x.Value).Returns("Tag ");

            sut.AddColorCategory(prefix.Object, "Tag Existing").Should().BeNull();
        }

        [TestMethod]
        public void AddColorCategory_NewProject_NoProgramChosen_ReturnsNull()
        {
            var projInfo = new Mock<IProjectData>();
            projInfo.Setup(x => x.Contains_ProjectName(It.IsAny<string>())).Returns(false);
            // Empty ProgramInfo so TryAutoExtractProgram fails; chooseProgram returns
            // null so ChooseOrCreateProgramName short-circuits before any Serialize.
            var globals = BuildGlobals(projInfoMock: projInfo);
            var sut = new AutoCreateProject(globals.Object, chooseProgram: keys => null);

            var prefix = new Mock<IPrefix>();
            prefix.Setup(x => x.Value).Returns("Tag ");

            sut.AddColorCategory(prefix.Object, "Tag BrandNew").Should().BeNull();
        }
    }
}
