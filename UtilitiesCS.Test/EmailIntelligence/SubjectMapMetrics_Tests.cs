#nullable enable

using System;
using System.Linq;
using System.Reflection;
using BrightIdeasSoftware;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS.EmailIntelligence.SubjectMap;

namespace UtilitiesCS.Test.EmailIntelligence
{
    [TestClass]
    public class SubjectMapMetrics_Tests
    {
        private static DataListView GetMetricsListView(SubjectMapMetrics viewer) =>
            (DataListView)
                typeof(SubjectMapMetrics)
                    .GetField("DlvMetrics", BindingFlags.NonPublic | BindingFlags.Instance)
                    .GetValue(viewer);

        [STAThread]
        [TestMethod]
        public void Constructor_WithMetrics_PopulatesDlvMetricsWithExpectedNumericValues()
        {
            var metric = new SubjectMapSco.SummaryMetric
            {
                FolderName = "Reports",
                FolderPath = @"Inbox\Reports",
                SubjectCount = 5,
                EmailCount = 8,
            };
            var viewer = new SubjectMapMetrics(new[] { metric });

            var metricsListView = GetMetricsListView(viewer);
            var boundObjects = metricsListView.Objects.Cast<object>().ToList();
            var boundMetric = (SubjectMapSco.SummaryMetric)boundObjects[0];

            boundObjects.Should().HaveCount(1);
            boundMetric.SubjectCount.Should().Be(5);
            boundMetric.EmailCount.Should().Be(8);
            metricsListView
                .AllColumns.Should()
                .Contain(column => column.AspectName == "SubjectCount");
            metricsListView
                .AllColumns.Should()
                .Contain(column => column.AspectName == "EmailCount");
        }

        [STAThread]
        [TestMethod]
        public void Constructors_WithEquivalentEmptyInputs_ProduceEquivalentDlvMetricsState()
        {
            var defaultViewer = new SubjectMapMetrics();
            var emptyMetricsViewer = new SubjectMapMetrics(
                System.Array.Empty<SubjectMapSco.SummaryMetric>()
            );

            var defaultListView = GetMetricsListView(defaultViewer);
            var emptyMetricsListView = GetMetricsListView(emptyMetricsViewer);

            defaultListView.Items.Count.Should().Be(0);
            emptyMetricsListView.Items.Count.Should().Be(0);
            defaultListView.Columns.Count.Should().Be(emptyMetricsListView.Columns.Count);
            defaultListView
                .AllColumns.Select(column => column.AspectName)
                .Should()
                .Equal(emptyMetricsListView.AllColumns.Select(column => column.AspectName));
            defaultListView.View.Should().Be(emptyMetricsListView.View);
            defaultListView.ShowGroups.Should().Be(emptyMetricsListView.ShowGroups);
        }
    }
}
