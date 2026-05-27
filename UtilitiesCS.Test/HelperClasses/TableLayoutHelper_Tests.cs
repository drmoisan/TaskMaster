using System;
using System.Linq;
using System.Windows.Forms;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS;

namespace UtilitiesCS.Test.HelperClasses
{
    [STATestClass]
    public class TableLayoutHelper_Additional_Tests
    {
        [TestMethod]
        public void InsertSpecificRow_WithExistingControls_ShiftsRowsAndClonesStyles()
        {
            var panel = CreatePanel(rowCount: 2, columnCount: 1);
            var top = new Label { Name = "top" };
            var bottom = new Label { Name = "bottom" };
            panel.Controls.Add(top, 0, 0);
            panel.Controls.Add(bottom, 0, 1);
            var insertedStyle = new RowStyle(SizeType.Absolute, 33);

            panel.InsertSpecificRow(1, insertedStyle, insertCount: 2);

            panel.RowCount.Should().Be(4);
            panel
                .RowStyles.Cast<RowStyle>()
                .Select(style => style.Height)
                .Should()
                .Equal(20, 33, 33, 21);
            panel.GetRow(top).Should().Be(0);
            panel.GetRow(bottom).Should().Be(3);
        }

        [TestMethod]
        public void RemoveSpecificRow_WhenIndexIsOutOfRange_LeavesPanelUnchanged()
        {
            var panel = CreatePanel(rowCount: 2, columnCount: 1);

            panel.RemoveSpecificRow(rowIndex: 2);

            panel.RowCount.Should().Be(2);
            panel.RowStyles.Count.Should().Be(2);
        }

        [TestMethod]
        public void RemoveSpecificRow_RemovesTargetedControlsAndShiftsRemainingRows()
        {
            var panel = CreatePanel(rowCount: 3, columnCount: 2);
            var rowZero = new Label { Name = "rowZero" };
            var removed = new Label { Name = "removed" };
            var shifted = new Label { Name = "shifted" };
            panel.Controls.Add(rowZero, 0, 0);
            panel.Controls.Add(removed, 1, 1);
            panel.Controls.Add(shifted, 0, 2);

            panel.RemoveSpecificRow(rowIndex: 1);

            panel.RowCount.Should().Be(2);
            panel.RowStyles.Count.Should().Be(2);
            panel.Controls.Contains(removed).Should().BeFalse();
            panel.GetRow(rowZero).Should().Be(0);
            panel.GetRow(shifted).Should().Be(1);
        }

        [TestMethod]
        public void RemoveSpecificColumn_WhenIndexIsOutOfRange_LeavesPanelUnchanged()
        {
            var panel = CreatePanel(rowCount: 1, columnCount: 2);

            panel.RemoveSpecificColumn(colIndex: 2);

            panel.ColumnCount.Should().Be(2);
            panel.ColumnStyles.Count.Should().Be(2);
        }

        [TestMethod]
        public void RemoveSpecificColumn_RemovesTargetedControlsAndShiftsRemainingColumns()
        {
            var panel = CreatePanel(rowCount: 2, columnCount: 3);
            var kept = new Label { Name = "kept" };
            var removed = new Label { Name = "removed" };
            var shifted = new Label { Name = "shifted" };
            panel.Controls.Add(kept, 0, 0);
            panel.Controls.Add(removed, 1, 1);
            panel.Controls.Add(shifted, 2, 0);

            panel.RemoveSpecificColumn(colIndex: 1);

            panel.ColumnCount.Should().Be(2);
            panel.ColumnStyles.Count.Should().Be(2);
            panel.Controls.Contains(removed).Should().BeFalse();
            panel.GetColumn(kept).Should().Be(0);
            panel.GetColumn(shifted).Should().Be(1);
        }

        private static TableLayoutPanel CreatePanel(int rowCount, int columnCount)
        {
            var panel = new TableLayoutPanel { RowCount = rowCount, ColumnCount = columnCount };

            for (var row = 0; row < rowCount; row++)
            {
                panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 20 + row));
            }

            for (var column = 0; column < columnCount; column++)
            {
                panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 40 + column));
            }

            return panel;
        }
    }
}
