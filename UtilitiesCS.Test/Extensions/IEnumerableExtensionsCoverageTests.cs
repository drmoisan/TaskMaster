using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS;

namespace UtilitiesCS.Test.Extensions
{
    [TestClass]
    public class IEnumerableExtensionsCoverageTests
    {
        [TestMethod]
        public void CastNullSafe_WithNonGenericSource_ConvertsValuesAndNulls()
        {
            // Arrange
            IEnumerable source = new object[] { 1, null, 3 };

            // Act
            int[] actual = source.CastNullSafe<int>().ToArray();

            // Assert
            actual.Should().Equal(1, 0, 3);
        }

        [TestMethod]
        public void CastNullSafe_WithTypedSource_ReturnsExistingEnumerable()
        {
            // Arrange
            IEnumerable<string> source = new[] { "alpha", "beta" };

            // Act
            IEnumerable<string> actual = source.CastNullSafe<string>();

            // Assert
            actual.Should().BeSameAs(source);
        }

        [TestMethod]
        public void CompareTo_WithEmptyAndNullInputs_ReturnsExpectedDifferences()
        {
            // Arrange
            IEnumerable<int> nullSource = null;
            var empty = Array.Empty<int>();
            var values = new[] { 1, 2 };

            // Act
            var emptyVersusValues = empty.CompareTo(values);
            var nullVersusValues = nullSource.CompareTo(values);
            var valuesVersusNull = values.CompareTo(nullSource);
            Action bothNull = () => nullSource.CompareTo(nullSource);

            // Assert
            emptyVersusValues.DifferenceCount.Should().Be(2);
            emptyVersusValues.OnlyThis.Should().BeEmpty();
            emptyVersusValues.OnlyOther.Should().Equal(1, 2);

            nullVersusValues.DifferenceCount.Should().Be(2);
            nullVersusValues.OnlyThis.Should().BeEmpty();
            nullVersusValues.OnlyOther.Should().Equal(1, 2);

            valuesVersusNull.DifferenceCount.Should().Be(2);
            valuesVersusNull.OnlyThis.Should().Equal(1, 2);
            valuesVersusNull.OnlyOther.Should().BeEmpty();

            bothNull.Should().Throw<ArgumentException>();
        }

        [TestMethod]
        public void WithProgressReporting_IsDeferredUntilEnumeration()
        {
            // Arrange
            var source = new TrackingEnumerable<int>(new[] { 1, 2, 3 });
            var percentages = new List<int>();

            // Act
            IEnumerable<int> deferred = source.WithProgressReporting(
                3L,
                percent => percentages.Add(percent)
            );

            // Assert
            source.MoveNextCount.Should().Be(0);
            percentages.Should().BeEmpty();

            deferred.ToArray().Should().Equal(1, 2, 3);
            source.MoveNextCount.Should().Be(4);
            percentages.Should().Equal(33, 66, 100);
        }

        [TestMethod]
        public void WithProgressReporting_WithCountCallback_HandlesEmptySource()
        {
            // Arrange
            var updates = new List<(long Completed, long Total)>();

            // Act
            int[] actual = Array
                .Empty<int>()
                .WithProgressReporting(0L, (completed, total) => updates.Add((completed, total)))
                .ToArray();

            // Assert
            actual.Should().BeEmpty();
            updates.Should().BeEmpty();
        }

        [TestMethod]
        public void WithProgressReporting_WithNullSource_ThrowsArgumentNullException()
        {
            // Arrange
            IEnumerable<int> source = null;

            // Act
            Action percentAction = () => source.WithProgressReporting(1L, _ => { }).ToArray();
            Action countAction = () => source.WithProgressReporting(1L, (_, _) => { }).ToArray();

            // Assert
            percentAction
                .Should()
                .Throw<ArgumentNullException>()
                .Which.ParamName.Should()
                .Be("enumerable");
            countAction
                .Should()
                .Throw<ArgumentNullException>()
                .Which.ParamName.Should()
                .Be("enumerable");
        }

        [TestMethod]
        public void WithAction_IsDeferredAndRunsOncePerItem()
        {
            // Arrange
            var source = new TrackingEnumerable<string>(new[] { "a", "b" });
            var invocations = 0;

            // Act
            IEnumerable<string> deferred = source.WithAction(() => invocations++);

            // Assert
            source.MoveNextCount.Should().Be(0);
            invocations.Should().Be(0);

            deferred.ToArray().Should().Equal("a", "b");
            source.MoveNextCount.Should().Be(3);
            invocations.Should().Be(2);
        }

        [TestMethod]
        public void WithAction_WithNullSource_ThrowsArgumentNullException()
        {
            // Arrange
            IEnumerable<int> source = null;

            // Act
            Action action = () => source.WithAction(() => { }).ToArray();

            // Assert
            action
                .Should()
                .Throw<ArgumentNullException>()
                .Which.ParamName.Should()
                .Be("enumerable");
        }

        [TestMethod]
        public void Chunk_ValidatesNullEmptyAndBoundaryInputs()
        {
            // Arrange
            IEnumerable<int> nullSource = null;
            var values = new[] { 1, 2, 3, 4, 5 };

            // Act
            Action nullAction = () => nullSource.Chunk(2).ToArray();
            Action invalidSize = () => values.Chunk(0).ToArray();
            int[][] empty = Array.Empty<int>().Chunk(3).ToArray();
            int[][] oversized = values.Chunk(10).ToArray();
            int[][] exact = values.Chunk(5).ToArray();

            // Assert
            nullAction
                .Should()
                .Throw<ArgumentNullException>()
                .Which.ParamName.Should()
                .Be("source");
            invalidSize
                .Should()
                .Throw<ArgumentOutOfRangeException>()
                .Which.ParamName.Should()
                .Be("size");
            empty.Should().BeEmpty();
            oversized.Should().ContainSingle().Which.Should().Equal(1, 2, 3, 4, 5);
            exact.Should().ContainSingle().Which.Should().Equal(1, 2, 3, 4, 5);
        }

        [TestMethod]
        public void Chunk_DoesNotEnumerateSourceBeforeResultIsEnumerated()
        {
            // Arrange
            var source = new TrackingEnumerable<int>(new[] { 1, 2, 3, 4 });

            // Act
            IEnumerable<int[]> deferred = source.Chunk(2);

            // Assert
            source.MoveNextCount.Should().Be(0);
            deferred
                .Select(chunk => chunk.ToArray())
                .Should()
                .BeEquivalentTo(new[] { new[] { 1, 2 }, new[] { 3, 4 } });
            source.MoveNextCount.Should().Be(5);
        }

        [TestMethod]
        public void SplitTestTrain_UsesDeterministicBoundaryPercentages()
        {
            // Arrange
            var values = new[] { 1, 2, 3, 4 };

            // Act
            var noneTraining = values.SplitTestTrain(0);
            var allTraining = values.SplitTestTrain(1);
            var halfTraining = values.SplitTestTrain(0.5);

            // Assert
            noneTraining.Train.Should().BeEmpty();
            noneTraining.Test.Should().Equal(1, 2, 3, 4);
            allTraining.Train.Should().Equal(1, 2, 3, 4);
            allTraining.Test.Should().BeEmpty();
            halfTraining.Train.Should().Equal(1, 2);
            halfTraining.Test.Should().Equal(3, 4);
        }

        [TestMethod]
        public void SplitTestTrain_ValidatesNullEmptyAndInvalidPercentages()
        {
            // Arrange
            IEnumerable<int> nullSource = null;
            var values = new[] { 1, 2 };

            // Act
            Action nullAction = () => nullSource.SplitTestTrain(0.5);
            Action emptyAction = () => Array.Empty<int>().SplitTestTrain(0.5);
            Action negativePercent = () => values.SplitTestTrain(-0.01);
            Action highPercent = () => values.SplitTestTrain(1.01);

            // Assert
            nullAction.Should().Throw<ArgumentNullException>();
            emptyAction.Should().Throw<ArgumentNullException>();
            negativePercent
                .Should()
                .Throw<ArgumentOutOfRangeException>()
                .Which.ParamName.Should()
                .Be("trainPercent");
            highPercent
                .Should()
                .Throw<ArgumentOutOfRangeException>()
                .Which.ParamName.Should()
                .Be("trainPercent");
        }

        private sealed class TrackingEnumerable<T> : IEnumerable<T>
        {
            private readonly IReadOnlyList<T> values;

            public TrackingEnumerable(IReadOnlyList<T> values)
            {
                this.values = values;
            }

            public int MoveNextCount { get; private set; }

            public IEnumerator<T> GetEnumerator()
            {
                foreach (T value in values)
                {
                    MoveNextCount++;
                    yield return value;
                }

                MoveNextCount++;
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }
    }
}
