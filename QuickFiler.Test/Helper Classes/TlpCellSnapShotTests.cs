using System;
using System.Collections.Generic;
using System.Windows.Forms;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using QuickFiler;

namespace QuickFiler.Test.HelperClasses
{
    /// <summary>
    /// Cycle-5 (R2, de-exempted): <c>TlpCellSnapShot</c>/<c>TlpCellSnapShotList.ApplyState</c> now
    /// accept <c>IContainerControlLocal</c> instead of a concrete <c>Control</c>. These tests prove
    /// <c>ApplyState</c> genuinely restores previously-snapshotted <c>Enabled</c>/<c>Visible</c>/
    /// accelerator-text state (not merely a no-op replay), using a bare <see cref="Control"/> host and
    /// a <see cref="Mock{IItemViewer}"/> whose <c>Controls</c> getter returns the host's
    /// <see cref="Control.ControlCollection"/>. No real <c>ItemViewer</c> or Designer control tree is
    /// required for this seam.
    /// </summary>
    [TestClass]
    public class TlpCellSnapShotTests
    {
        [TestMethod]
        public void ApplyState_OnInstance_RestoresSnapshottedEnabledVisibleAndAcceleratorText()
        {
            // Arrange — snapshot the label while enabled/visible/original text, then mutate it live.
            var host = new Control();
            var tlp = new TableLayoutPanel
            {
                Name = "Compressed",
                ColumnCount = 1,
                RowCount = 1,
            };
            tlp.RowStyles.Add(new RowStyle());
            tlp.ColumnStyles.Add(new ColumnStyle());
            var label = new Label
            {
                Name = "LblAcOpen",
                Text = "Original",
                Enabled = true,
                Visible = true,
            };
            tlp.Controls.Add(label, 0, 0);
            host.Controls.Add(tlp);

            var snapshot = new TlpCellSnapShot();
            snapshot.SnapCell(tlp, label);

            label.Enabled = false;
            label.Visible = false;
            label.Text = "Changed";

            var mockViewer = new Mock<IItemViewer>();
            mockViewer.Setup(v => v.Controls).Returns(host.Controls);

            // Act
            snapshot.ApplyState(mockViewer.Object);

            // Assert — the snapshotted state (not the mutated live state) is restored.
            label.Enabled.Should().BeTrue();
            label.Visible.Should().BeTrue();
            label.Text.Should().Be("Original");
        }

        [TestMethod]
        public void ApplyState_OnList_AppliesEveryEntry()
        {
            // Arrange — two distinct named controls, each independently snapshotted, then mutated.
            var host = new Control();
            var tlp = new TableLayoutPanel
            {
                Name = "Compressed",
                ColumnCount = 1,
                RowCount = 2,
            };
            tlp.RowStyles.Add(new RowStyle());
            tlp.RowStyles.Add(new RowStyle());
            tlp.ColumnStyles.Add(new ColumnStyle());
            var labelOne = new Label
            {
                Name = "LblOne",
                Enabled = true,
                Visible = true,
            };
            var labelTwo = new Label
            {
                Name = "LblTwo",
                Enabled = true,
                Visible = true,
            };
            tlp.Controls.Add(labelOne, 0, 0);
            tlp.Controls.Add(labelTwo, 0, 1);
            host.Controls.Add(tlp);

            var snapshotOne = new TlpCellSnapShot();
            snapshotOne.SnapCell(tlp, labelOne);
            var snapshotTwo = new TlpCellSnapShot();
            snapshotTwo.SnapCell(tlp, labelTwo);

            labelOne.Enabled = false;
            labelOne.Visible = false;
            labelTwo.Enabled = false;
            labelTwo.Visible = false;

            var snapshotList = new TlpCellSnapShotList(
                new List<TlpCellSnapShot> { snapshotOne, snapshotTwo }
            );

            var mockViewer = new Mock<IItemViewer>();
            mockViewer.Setup(v => v.Controls).Returns(host.Controls);

            // Act
            snapshotList.ApplyState(mockViewer.Object);

            // Assert — every entry in the list is restored.
            labelOne.Enabled.Should().BeTrue();
            labelOne.Visible.Should().BeTrue();
            labelTwo.Enabled.Should().BeTrue();
            labelTwo.Visible.Should().BeTrue();
        }
    }
}
