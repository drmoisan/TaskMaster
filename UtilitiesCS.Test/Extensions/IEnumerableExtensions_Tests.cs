using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS;

namespace UtilitiesCS.Test.Extensions
{
    [TestClass]
    public class IEnumerableExtensions_Tests
    {
        [TestMethod]
        public void CastNullSafe_ReturnsTypedSequenceAndConvertsNullElementsToDefault()
        {
            // Arrange
            IEnumerable<string> typed = new[] { "alpha", "beta" };
            IEnumerable sourceWithNull = new object[] { "alpha", null, "beta" };

            // Act
            var typedResult = typed.CastNullSafe<string>();
            var nullSafeResult = sourceWithNull.CastNullSafe<string>().ToArray();

            // Assert
            typedResult.Should().BeSameAs(typed);
            nullSafeResult.Should().Equal("alpha", null, "beta");
        }

        [TestMethod]
        public void CastNullSafe_WhenSourceIsNull_ThrowsArgumentNullException()
        {
            // Arrange
            IEnumerable source = null;

            // Act
            Action action = () => source.CastNullSafe<string>().ToArray();

            // Assert
            action.Should().Throw<ArgumentNullException>().Which.ParamName.Should().Be("source");
        }

        [TestMethod]
        public void CompareTo_HandlesNullEmptyAndDifferenceScenarios()
        {
            // Arrange
            IEnumerable<int> left = null;
            IEnumerable<int> empty = Array.Empty<int>();
            var source = new[] { 1, 2, 3 };
            var other = new[] { 2, 3, 4 };

            // Act
            var nullVsEmpty = left.CompareTo(empty);
            var sourceVsNull = source.CompareTo(left);
            var difference = source.CompareTo(other);
            Action bothNullAction = () => left.CompareTo(left);

            // Assert
            nullVsEmpty.DifferenceCount.Should().Be(0);
            nullVsEmpty.OnlyThis.Should().BeEmpty();
            nullVsEmpty.OnlyOther.Should().BeEmpty();

            sourceVsNull.DifferenceCount.Should().Be(3);
            sourceVsNull.OnlyThis.Should().Equal(1, 2, 3);
            sourceVsNull.OnlyOther.Should().BeEmpty();

            difference.DifferenceCount.Should().Be(2);
            difference.OnlyThis.Should().Equal(1);
            difference.OnlyOther.Should().Equal(4);

            bothNullAction
                .Should()
                .Throw<ArgumentException>()
                .WithMessage("*both IEnumerable*parameters were null*");
        }

        [TestMethod]
        public void CompareTo_WhenSequencesMatch_ReturnsNoDifferences()
        {
            // Arrange
            var left = new[] { 1, 2, 3 };
            var right = new[] { 1, 2, 3 };

            // Act
            var result = left.CompareTo(right);

            // Assert
            result.DifferenceCount.Should().Be(0);
            result.OnlyThis.Should().BeEmpty();
            result.OnlyOther.Should().BeEmpty();
        }

        [TestMethod]
        public void IsSubsetOf_ReturnsFalseForNullAndTrueForContainedValues()
        {
            // Act / Assert
            ((IEnumerable<int>)null)
                .IsSubsetOf(new[] { 1, 2 })
                .Should()
                .BeFalse();
            new[] { 1, 2 }.IsSubsetOf(null).Should().BeFalse();
            new[] { 1, 2 }.IsSubsetOf(new[] { 1, 2, 3 }).Should().BeTrue();
            new[] { 1, 4 }.IsSubsetOf(new[] { 1, 2, 3 }).Should().BeFalse();
        }

        [TestMethod]
        public void SelectGroupAndStringJoin_ReturnExpectedMatchesAndFormatting()
        {
            // Arrange
            var groups = new[]
            {
                new { Key = "odd", Value = 1 },
                new { Key = "even", Value = 2 },
                new { Key = "odd", Value = 3 },
            }.GroupBy(x => x.Key, x => x.Value);

            // Act
            var selected = groups.SelectGroup("odd");
            var stringJoin = new[] { "alpha", "beta", "gamma" }.StringJoin("|");
            var charJoin = new[] { 'α', 'β', 'γ' }.StringJoin("-");

            // Assert
            selected.Should().Equal(1, 3);
            stringJoin.Should().Be("alpha|beta|gamma");
            charJoin.Should().Be("α-β-γ");
        }

        [TestMethod]
        public void ToStack_PreservesLifoBehaviorForSingleAndMultipleItems()
        {
            // Arrange
            var single = new[] { 42 };
            var many = new[] { 1, 2, 3 };

            // Act
            var singleStack = single.ToStack();
            var manyStack = many.ToStack();

            // Assert
            singleStack.Pop().Should().Be(42);
            manyStack.Pop().Should().Be(3);
            manyStack.Pop().Should().Be(2);
            manyStack.Pop().Should().Be(1);
        }

        [TestMethod]
        public void GetProgressMessage_WhenNoItemsAreComplete_UsesZeroRate()
        {
            // Arrange
            var method = typeof(IEnumerableExtensions).GetMethod(
                "GetProgressMessage",
                BindingFlags.NonPublic | BindingFlags.Static
            );
            method.Should().NotBeNull();
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            // Act
            var result = (string)method.Invoke(null, new object[] { 0, 5, stopwatch });

            // Assert
            result.Should().Contain("Completed 0 of 5");
            result.Should().Contain("0.00 spm");
        }

        [TestMethod]
        public void GetProgressMessage_WhenItemsAreComplete_UsesMeasuredRate()
        {
            // Arrange
            var method = typeof(IEnumerableExtensions).GetMethod(
                "GetProgressMessage",
                BindingFlags.NonPublic | BindingFlags.Static
            );
            method.Should().NotBeNull();
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            System.Threading.Thread.Sleep(25);

            // Act
            var result = (string)method.Invoke(null, new object[] { 2, 5, stopwatch });

            // Assert
            result.Should().Contain("Completed 2 of 5");
            result.Should().Contain("spm");
            result.Should().Contain("remaining");
        }

        [TestMethod]
        public void ToList_InternalHelper_ConsumesEnumerableAndReportsProgress()
        {
            // Arrange
            var tracker = new CapturingProgressTracker();
            IEnumerable<int> source = Enumerable
                .Range(1, 3)
                .Select(value =>
                {
                    // Sleep for 700 ms (> the 500 ms timer period) so that at least one
                    // timer tick fires while completed > 0, satisfying the Value > 0 assertion
                    // even when Thread.Sleep runs slightly long under test-suite load.
                    System.Threading.Thread.Sleep(700);
                    return value;
                });
            var method = typeof(IEnumerableExtensions).GetMethod(
                "ToList",
                BindingFlags.NonPublic | BindingFlags.Static
            );
            method.Should().NotBeNull();

            // Act
            var result =
                (List<int>)
                    method
                        .MakeGenericMethod(typeof(int))
                        .Invoke(null, new object[] { source, 3, tracker });

            // Assert
            result.Should().Equal(1, 2, 3);
            tracker.Reports.Should().Contain(report => report.Value == 0);
            tracker.Reports.Should().Contain(report => report.Value > 0);
            tracker.Reports.Should().Contain(report => report.JobName.Contains("of 3"));
        }

        [TestMethod]
        public void WithProgressReporting_ReportsProgressOnlyWhenEnumerated()
        {
            // Arrange
            var progressUpdates = new List<int>();
            var source = new[] { "a", "b", "c" };

            // Act
            var deferred = source.WithProgressReporting(3L, value => progressUpdates.Add(value));
            progressUpdates.Should().BeEmpty();
            var actual = deferred.ToArray();

            // Assert
            actual.Should().Equal("a", "b", "c");
            progressUpdates.Should().Equal(33, 66, 100);
        }

        [TestMethod]
        public void WithProgressReporting_WithCompletedAndTotalCallback_ReturnsLargeSequenceAndReportsCounts()
        {
            // Arrange
            var updates = new List<(long Completed, long Total)>();
            var source = Enumerable.Range(1, 105);

            // Act
            var actual = source
                .WithProgressReporting(105L, (completed, total) => updates.Add((completed, total)))
                .ToArray();

            // Assert
            actual.Should().HaveCount(105);
            actual.First().Should().Be(1);
            actual.Last().Should().Be(105);
            updates.Should().HaveCount(105);
            updates.First().Should().Be((1L, 105L));
            updates.Last().Should().Be((105L, 105L));
        }

        [TestMethod]
        public void WithProgressReporting_WhenEnumerableIsNull_ThrowsArgumentNullException()
        {
            // Arrange
            IEnumerable<int> source = null;

            // Act
            Action intProgressAction = () => source.WithProgressReporting(1L, _ => { }).ToArray();
            Action longProgressAction = () =>
                source.WithProgressReporting(1L, (_, _) => { }).ToArray();

            // Assert
            intProgressAction
                .Should()
                .Throw<ArgumentNullException>()
                .Which.ParamName.Should()
                .Be("enumerable");
            longProgressAction
                .Should()
                .Throw<ArgumentNullException>()
                .Which.ParamName.Should()
                .Be("enumerable");
        }

        [TestMethod]
        public void WithAction_IsDeferredAndInvokesActionPerItem()
        {
            // Arrange
            var invocations = 0;
            var source = new[] { 1, 2, 3 };

            // Act
            var deferred = source.WithAction(() => invocations++);
            var beforeEnumeration = invocations;
            var actual = deferred.ToArray();

            // Assert
            beforeEnumeration.Should().Be(0);
            actual.Should().Equal(1, 2, 3);
            invocations.Should().Be(3);
        }

        [TestMethod]
        public void ToDataTable_CreatesColumnsAndRowsFromObjectProperties()
        {
            // Arrange
            var source = new[]
            {
                new SampleRow { Id = 1, Name = "alpha" },
                new SampleRow { Id = 2, Name = "beta" },
            };

            // Act
            DataTable actual = IEnumerableExtensions.ToDataTable(source);

            // Assert
            actual
                .Columns.Cast<DataColumn>()
                .Select(column => column.ColumnName)
                .Should()
                .Equal("Id", "Name");
            actual.Rows.Count.Should().Be(2);
            actual.Rows[0]["Id"].Should().Be(1);
            actual.Rows[1]["Name"].Should().Be("beta");
        }

        [TestMethod]
        public void Unzip_SeparatesPairsAndTriplesIntoIndependentSequences()
        {
            // Arrange
            var pairs = new[] { (1, "one"), (2, "two") };
            var triples = new[] { (1, "one", true), (2, "two", false) };

            // Act
            var pairResult = pairs.Unzip();
            var tripleResult = triples.Unzip();

            // Assert
            pairResult.Item1.Should().Equal(1, 2);
            pairResult.Item2.Should().Equal("one", "two");
            tripleResult.Item1.Should().Equal(1, 2);
            tripleResult.Item2.Should().Equal("one", "two");
            tripleResult.Item3.Should().Equal(true, false);
        }

        [TestMethod]
        public void Transpose_ConvertsRectangularSequencesIntoColumns()
        {
            // Arrange
            IEnumerable<IEnumerable<int>> rectangular = [new[] { 1, 2, 3 }, new[] { 4, 5, 6 }];

            // Act
            var actual = rectangular.Transpose().Select(row => row.ToArray()).ToArray();

            // Assert
            actual.Should().HaveCount(3);
            actual[0].Should().Equal(1, 4);
            actual[1].Should().Equal(2, 5);
            actual[2].Should().Equal(3, 6);
        }

        [TestMethod]
        public void Chunk_ValidatesArgumentsAndSplitsEmptySingleAndLargeSequences()
        {
            // Arrange
            IEnumerable<int> nullSource = null;
            var single = new[] { 7 };
            var large = Enumerable.Range(1, 105);

            // Act
            Action nullAction = () => nullSource.Chunk(2).ToArray();
            Action invalidSizeAction = () => single.Chunk(0).ToArray();
            var empty = Array.Empty<int>().Chunk(3).ToArray();
            var singleResult = single.Chunk(4).ToArray();
            var largeResult = large.Chunk(10).ToArray();

            // Assert
            nullAction
                .Should()
                .Throw<ArgumentNullException>()
                .Which.ParamName.Should()
                .Be("source");
            invalidSizeAction
                .Should()
                .Throw<ArgumentOutOfRangeException>()
                .Which.ParamName.Should()
                .Be("size");
            empty.Should().BeEmpty();
            singleResult.Should().HaveCount(1);
            singleResult[0].Should().Equal(7);
            largeResult.Should().HaveCount(11);
            largeResult[0].Should().Equal(1, 2, 3, 4, 5, 6, 7, 8, 9, 10);
            largeResult[^1].Should().Equal(101, 102, 103, 104, 105);
        }

        [TestMethod]
        public void SplitTestTrain_ValidatesInputAndReturnsAStablePartitionOfTheSource()
        {
            // Arrange
            IEnumerable<int> nullSource = null;
            var source = new[] { 1, 2, 3, 4, 5 };
            var single = new[] { 42 };

            // Act
            Action nullAction = () => nullSource.SplitTestTrain(0.5);
            Action emptyAction = () => Array.Empty<int>().SplitTestTrain(0.5);
            Action invalidLowAction = () => source.SplitTestTrain(-0.1);
            Action invalidHighAction = () => source.SplitTestTrain(1.1);
            var (train, test) = source.SplitTestTrain(0.5);
            var singleSplit = single.SplitTestTrain(1.0);

            // Assert
            nullAction.Should().Throw<ArgumentNullException>();
            emptyAction.Should().Throw<ArgumentNullException>();
            invalidLowAction
                .Should()
                .Throw<ArgumentOutOfRangeException>()
                .Which.ParamName.Should()
                .Be("trainPercent");
            invalidHighAction
                .Should()
                .Throw<ArgumentOutOfRangeException>()
                .Which.ParamName.Should()
                .Be("trainPercent");

            train.Concat(test).Should().BeEquivalentTo(source);
            train.Intersect(test).Should().BeEmpty();
            (train.Length + test.Length).Should().Be(source.Length);

            singleSplit.Train.Should().Equal(42);
            singleSplit.Test.Should().BeEmpty();
        }

        [TestMethod]
        public void WithAction_WhenEnumerableIsNull_ThrowsArgumentNullException()
        {
            // Arrange
            IEnumerable<int> source = null;

            // Act
            Action act = () => source.WithAction(() => { }).ToArray();

            // Assert
            act.Should().Throw<ArgumentNullException>().Which.ParamName.Should().Be("enumerable");
        }

        [TestMethod]
        public void StringJoin_WithDefaultSeparator_JoinsWithComma()
        {
            // Arrange
            var strings = new[] { "a", "b", "c" };

            // Act
            var result = strings.StringJoin();

            // Assert
            result.Should().Be("a,b,c");
        }

        [TestMethod]
        public void StringJoin_Chars_WithDefaultSeparator_JoinsWithEmpty()
        {
            // Arrange
            var chars = new[] { 'x', 'y', 'z' };

            // Act
            var result = chars.StringJoin();

            // Assert
            result.Should().Be("xyz");
        }

        private sealed class SampleRow
        {
            public int Id { get; set; }

            public string Name { get; set; }
        }

        private sealed class CapturingProgressTracker : ProgressTracker
        {
            public CapturingProgressTracker()
                : base(new System.Threading.CancellationTokenSource()) { }

            public ConcurrentQueue<(int Value, string JobName)> Reports { get; } = new();

            public override void Report((int Value, string JobName) report)
            {
                Reports.Enqueue(report);
            }

            public override void Report(double value, string jobName)
            {
                Reports.Enqueue(((int)value, jobName ?? string.Empty));
            }

            public override void Report(double value)
            {
                Reports.Enqueue(((int)value, string.Empty));
            }
        }
    }
}
