using System.Collections.Generic;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS;
using UtilitiesCS.ReusableTypeClasses.SerializableNew.Concurrent.Observable;

namespace UtilitiesCS.Test.ReusableTypeClasses.SerializableNew.Concurrent.Observable
{
    /// <summary>
    /// Regression tests for the positional undo-loop contract that both
    /// <c>SortEmail.UndoAsync</c> and <c>QfcFormController.UndoDialog</c> depend on. Each loop uses a
    /// forward index <c>i</c>, reads <c>stack[i]</c>, and on confirmation calls <c>stack.Pop(i)</c>
    /// (removing and returning the element at ordinal <c>i</c>) <b>without advancing <c>i</c></b>, so
    /// the next element shifts down into index <c>i</c> and is reprocessed. These tests assert that
    /// <see cref="SloStack{T}"/> reproduces the legacy <c>ScoStack</c> shift-and-reprocess ordinal
    /// semantics exactly. No temp files are used.
    /// </summary>
    [TestClass]
    public class SloStackUndoContract_Tests
    {
        private static MovedMailInfo Info(string entryId) =>
            new MovedMailInfo
            {
                FolderPathOld = "Inbox",
                FolderPathNew = "Archive",
                EntryId = entryId,
                StoreId = "store",
            };

        private static SloStack<IMovedMailInfo> BuildStack(params string[] topToBottom)
        {
            // topToBottom[0] is the intended top (index 0). Push in reverse so it ends up at front.
            var stack = new SloStack<IMovedMailInfo>();
            for (int i = topToBottom.Length - 1; i >= 0; i--)
            {
                stack.Push(Info(topToBottom[i]));
            }
            return stack;
        }

        [TestMethod]
        public void UndoLoop_ConfirmAll_ProcessesEveryElementTopToBottom_AndDrainsStack()
        {
            // Arrange — replicate the undo loop with "undo yes" for every element.
            var stack = BuildStack("A", "B", "C", "D");
            var processed = new List<string>();

            // Act — forward index, positional Pop(i) without advancing i.
            int i = 0;
            while (i < stack.Count)
            {
                var item = stack[i];
                processed.Add(item.EntryId);
                var popped = stack.Pop(i);
                // Pop(i) returns the same element that was read at ordinal i.
                popped.EntryId.Should().Be(item.EntryId);
                // i intentionally not advanced: the next element shifts into index i.
            }

            // Assert — processed top-to-bottom in order; stack fully drained.
            processed.Should().Equal(new[] { "A", "B", "C", "D" });
            stack.Count.Should().Be(0);
        }

        [TestMethod]
        public void UndoLoop_MixedConfirmAndSkip_ShiftsAndReprocessesCorrectly()
        {
            // Arrange — undo A and C, skip B. Skips advance i; confirmations Pop(i) and hold i.
            var stack = BuildStack("A", "B", "C");
            var processedUndo = new List<string>();

            // Act
            int i = 0;
            while (i < stack.Count)
            {
                var item = stack[i];
                if (item.EntryId == "B")
                {
                    // skip → advance index (matches the "message is null" / "No" branches)
                    i++;
                }
                else
                {
                    processedUndo.Add(item.EntryId);
                    stack.Pop(i); // remove at ordinal i, hold i so the next element reprocesses
                }
            }

            // Assert — A and C undone in order; B remains as the only survivor.
            processedUndo.Should().Equal(new[] { "A", "C" });
            stack.Count.Should().Be(1);
            stack[0].EntryId.Should().Be("B");
        }

        [TestMethod]
        public void PopAtOrdinal_ShiftsHigherIndicesDown_SoNextElementOccupiesSameIndex()
        {
            // Arrange
            var stack = BuildStack("A", "B", "C");

            // Act — remove the middle element at ordinal 1.
            var removed = stack.Pop(1);

            // Assert — "B" removed; "C" shifts down into index 1.
            removed.EntryId.Should().Be("B");
            stack[0].EntryId.Should().Be("A");
            stack[1].EntryId.Should().Be("C");
            stack.Count.Should().Be(2);
        }
    }
}
