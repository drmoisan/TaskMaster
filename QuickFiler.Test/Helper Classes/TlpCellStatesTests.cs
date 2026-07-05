using System;
using System.Collections.Generic;
using System.Windows.Forms;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using QuickFiler;

namespace QuickFiler.Test.HelperClasses
{
    [TestClass]
    public class TlpCellStatesTests
    {
        [TestMethod]
        public void EmptyConstructor_CreatesEmptyStateDictionary()
        {
            var states = new TlpCellStates();

            states.Should().BeEmpty();
        }

        [TestMethod]
        public void TypedCollectionConstructor_PreservesSnapshotListsByKey()
        {
            var expandedSnapshots = new TlpCellSnapShotList { CreateSnapshot("Expanded") };
            var compressedSnapshots = new TlpCellSnapShotList { CreateSnapshot("Compressed") };
            var source = new[]
            {
                new KeyValuePair<string, TlpCellSnapShotList>("expanded", expandedSnapshots),
                new KeyValuePair<string, TlpCellSnapShotList>("compressed", compressedSnapshots),
            };

            var states = new TlpCellStates(source);

            states.Should().ContainKey("expanded");
            states["expanded"].Should().BeSameAs(expandedSnapshots);
            states["compressed"].Should().BeSameAs(compressedSnapshots);
        }

        [TestMethod]
        public void RawCollectionConstructor_ConvertsListsToTlpCellSnapShotLists()
        {
            var rawSnapshots = new List<TlpCellSnapShot> { CreateSnapshot("Raw") };
            var source = new[]
            {
                new KeyValuePair<string, List<TlpCellSnapShot>>("raw", rawSnapshots),
            };

            var states = new TlpCellStates(source);

            states["raw"].Should().BeOfType<TlpCellSnapShotList>();
            states["raw"].Should().ContainSingle().Which.ControlName.Should().Be("Raw");
            states["raw"].Should().NotBeSameAs(rawSnapshots);
        }

        [TestMethod]
        public void CollectionConstructors_WithEmptyInputs_CreateEmptyStateDictionary()
        {
            var typedStates = new TlpCellStates(
                Array.Empty<KeyValuePair<string, TlpCellSnapShotList>>()
            );
            var rawStates = new TlpCellStates(
                Array.Empty<KeyValuePair<string, List<TlpCellSnapShot>>>()
            );

            typedStates.Should().BeEmpty();
            rawStates.Should().BeEmpty();
        }

        [TestMethod]
        public void TypedCollectionConstructor_WithDuplicateKeys_ThrowsArgumentException()
        {
            var source = new[]
            {
                new KeyValuePair<string, TlpCellSnapShotList>(
                    "duplicate",
                    new TlpCellSnapShotList()
                ),
                new KeyValuePair<string, TlpCellSnapShotList>(
                    "duplicate",
                    new TlpCellSnapShotList()
                ),
            };

            Action act = () => _ = new TlpCellStates(source);

            act.Should().Throw<ArgumentException>();
        }

        [TestMethod]
        public void TryAddState_WithoutSnapshots_AddsOnlyMissingState()
        {
            var states = new TlpCellStates();

            bool firstAdd = states.TryAddState("normal");
            bool secondAdd = states.TryAddState("normal");

            firstAdd.Should().BeTrue();
            secondAdd.Should().BeFalse();
            states["normal"].Should().BeEmpty();
        }

        [TestMethod]
        public void TryAddState_WithSnapshots_AddsConvertedListOnlyForMissingState()
        {
            var snapshots = new List<TlpCellSnapShot> { CreateSnapshot("Button") };
            var states = new TlpCellStates();

            bool firstAdd = states.TryAddState("expanded", snapshots);
            bool secondAdd = states.TryAddState(
                "expanded",
                new List<TlpCellSnapShot> { CreateSnapshot("Other") }
            );

            firstAdd.Should().BeTrue();
            secondAdd.Should().BeFalse();
            states["expanded"].Should().ContainSingle().Which.ControlName.Should().Be("Button");
        }

        [TestMethod]
        public void TypedCollectionConstructor_WithNullInput_ThrowsArgumentNullException()
        {
            Action act = () =>
                _ = new TlpCellStates((IEnumerable<KeyValuePair<string, TlpCellSnapShotList>>)null);

            act.Should().Throw<ArgumentNullException>().WithParameterName("collection");
        }

        [TestMethod]
        public void RawCollectionConstructor_WithNullInput_ThrowsArgumentNullException()
        {
            Action act = () =>
                _ = new TlpCellStates(
                    (IEnumerable<KeyValuePair<string, List<TlpCellSnapShot>>>)null
                );

            act.Should().Throw<ArgumentNullException>().WithParameterName("collection");
        }

        [TestMethod]
        public void SnapshotConstructor_CapturesControlCellState()
        {
            var tlp = CreateTableLayoutPanel(rowCount: 2, columnCount: 2);
            var label = new Label
            {
                Name = "LblAcOpen",
                Text = "Open",
                Enabled = true,
                Visible = true,
            };
            tlp.Controls.Add(label, 1, 1);
            tlp.SetRowSpan(label, 1);
            tlp.SetColumnSpan(label, 1);

            var snapshot = new TlpCellSnapShot(tlp, label);

            snapshot.TlpName.Should().Be("StatePanel");
            snapshot.ControlName.Should().Be("LblAcOpen");
            snapshot.Row.Should().Be(1);
            snapshot.Column.Should().Be(1);
            snapshot.AcceleratorText.Should().Be("Open");
        }

        [TestMethod]
        public void RowAndColumnAccessors_UpdateCellPosition()
        {
            var snapshot = new TlpCellSnapShot();

            snapshot.Row = 2;
            snapshot.Column = 3;

            snapshot.Cell.Row.Should().Be(2);
            snapshot.Cell.Column.Should().Be(3);
        }

        [TestMethod]
        public void ApplyState_WhenControlHasDifferentParent_ReparentsAndRestoresCell()
        {
            var host = new Control();
            var tlp = CreateTableLayoutPanel(rowCount: 2, columnCount: 2);
            var originalParent = new Panel();
            var label = new Label
            {
                Name = "LblAcOpen",
                Text = "Changed",
                Enabled = false,
                Visible = false,
            };
            originalParent.Controls.Add(label);
            host.Controls.Add(tlp);
            host.Controls.Add(originalParent);

            var snapshot = new TlpCellSnapShot
            {
                TlpName = tlp.Name,
                ControlName = label.Name,
                AcceleratorText = "Open",
                Cell = new TableLayoutPanelCellPosition(1, 1),
                RowSpan = 1,
                ColumnSpan = 1,
                RowStyles = new List<RowStyle> { new RowStyle(SizeType.Absolute, 33) },
                ColumnStyles = new List<ColumnStyle> { new ColumnStyle(SizeType.Absolute, 44) },
                Enabled = true,
                Visible = true,
            };
            var viewer = new Mock<IItemViewer>();
            viewer.Setup(v => v.Controls).Returns(host.Controls);

            snapshot.ApplyState(viewer.Object);

            label.Parent.Should().BeSameAs(tlp);
            tlp.GetCellPosition(label).Should().Be(new TableLayoutPanelCellPosition(1, 1));
            label.Enabled.Should().BeTrue();
            label.Visible.Should().BeTrue();
            label.Text.Should().Be("Open");
            tlp.RowStyles[1].Height.Should().Be(33);
            tlp.ColumnStyles[1].Width.Should().Be(44);
        }

        private static TlpCellSnapShot CreateSnapshot(string controlName)
        {
            return new TlpCellSnapShot { ControlName = controlName };
        }

        private static TableLayoutPanel CreateTableLayoutPanel(int rowCount, int columnCount)
        {
            var tlp = new TableLayoutPanel
            {
                Name = "StatePanel",
                RowCount = rowCount,
                ColumnCount = columnCount,
            };

            for (var row = 0; row < rowCount; row++)
            {
                tlp.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
            }

            for (var column = 0; column < columnCount; column++)
            {
                tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            }

            return tlp;
        }
    }
}
