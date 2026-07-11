using System;
using System.Collections.Generic;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using TaskVisualization;
using UtilitiesCS;
using UtilitiesCS.ReusableTypeClasses.Concurrent.Observable.Collection;

namespace TaskVisualization.Test
{
    /// <summary>
    /// Unit tests for <see cref="ManageFiltersController"/>. The controller depends
    /// only on <see cref="IManageFiltersViewer"/>, <see cref="IApplicationGlobals"/>,
    /// and an injectable edit-filter factory seam, so all orchestration is exercised
    /// against a mocked viewer with no live form, popup, or disk I/O. The persisted
    /// filter set uses a real empty <see cref="ScoCollection{FilterEntry}"/> whose
    /// <c>Serialize()</c> is a no-op while its <c>FilePath</c> is empty.
    /// </summary>
    [TestClass]
    public class ManageFiltersControllerTests
    {
        private static Mock<IApplicationGlobals> BuildGlobals(
            ConcurrentObservableCollection<FilterEntry> filters
        )
        {
            var af = new Mock<IAppAutoFileObjects>();
            af.Setup(x => x.Filters).Returns(filters);

            var globals = new Mock<IApplicationGlobals>();
            globals.Setup(x => x.AF).Returns(af.Object);
            return globals;
        }

        [TestMethod]
        public void LoadFilters_BindsFilterSetIntoViewer()
        {
            var viewer = new Mock<IManageFiltersViewer>();
            var filters = new ConcurrentObservableCollection<FilterEntry>();
            var globals = BuildGlobals(filters);
            var controller = new ManageFiltersController(viewer.Object, globals.Object);

            controller.LoadFilters();

            viewer.Verify(v => v.SetFilters(filters), Times.Once);
        }

        [TestMethod]
        public void EditSelected_ReadsSelectedFilter_InvokesFactoryWithThatEntry()
        {
            var viewer = new Mock<IManageFiltersViewer>();
            var selected = new FilterEntry { Name = "Selected" };
            viewer.Setup(v => v.SelectedFilter).Returns(selected);

            var filters = new ConcurrentObservableCollection<FilterEntry>();
            var globals = BuildGlobals(filters);

            IApplicationGlobals factoryGlobals = null;
            FilterEntry factoryEntry = null;
            var factoryCalls = 0;
            var controller = new ManageFiltersController(
                viewer.Object,
                globals.Object,
                (g, fe) =>
                {
                    factoryCalls++;
                    factoryGlobals = g;
                    factoryEntry = fe;
                    return null;
                }
            );

            controller.EditSelected();

            factoryCalls.Should().Be(1);
            factoryGlobals.Should().BeSameAs(globals.Object);
            factoryEntry.Should().BeSameAs(selected);
            viewer.VerifyGet(v => v.SelectedFilter, Times.Once);
        }

        [TestMethod]
        public void AddFilter_InvokesFactoryWithNull_ThenSetFiltersAndRebuild()
        {
            var viewer = new Mock<IManageFiltersViewer>();
            var filters = new ConcurrentObservableCollection<FilterEntry>();
            var globals = BuildGlobals(filters);

            var passedEntries = new List<FilterEntry>();
            var controller = new ManageFiltersController(
                viewer.Object,
                globals.Object,
                (g, fe) =>
                {
                    passedEntries.Add(fe);
                    return null;
                }
            );

            controller.AddFilter();

            passedEntries.Should().ContainSingle().Which.Should().BeNull();
            viewer.Verify(v => v.SetFilters(filters), Times.Once);
            viewer.Verify(v => v.RebuildList(), Times.Once);
        }

        [TestMethod]
        public void EditFilterCallback_CommitsEntryToFilterSet_AndRebuilds()
        {
            var viewer = new Mock<IManageFiltersViewer>();
            var filters = new ConcurrentObservableCollection<FilterEntry>();
            var globals = BuildGlobals(filters);
            var controller = new ManageFiltersController(viewer.Object, globals.Object);
            var entry = new FilterEntry { Name = "New" };

            controller.EditFilterCallback(null, entry);

            filters.Should().Contain(entry);
            viewer.Verify(v => v.RebuildList(), Times.Once);
        }

        [TestMethod]
        public void DeleteSelected_ReadsSelectedFilter_WithoutSideEffects()
        {
            var viewer = new Mock<IManageFiltersViewer>();
            viewer.Setup(v => v.SelectedFilter).Returns(new FilterEntry());
            var filters = new ConcurrentObservableCollection<FilterEntry>();
            var globals = BuildGlobals(filters);
            var controller = new ManageFiltersController(viewer.Object, globals.Object);

            controller.DeleteSelected();

            viewer.VerifyGet(v => v.SelectedFilter, Times.Once);
            viewer.Verify(v => v.RebuildList(), Times.Never);
            viewer.Verify(v => v.SetFilters(It.IsAny<IEnumerable<FilterEntry>>()), Times.Never);
        }
    }
}
