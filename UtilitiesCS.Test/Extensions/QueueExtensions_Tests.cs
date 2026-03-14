using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using UtilitiesCS;

namespace UtilitiesCS.Test.Extensions
{
    [TestClass]
    public class QueueExtensions_Tests
    {
        [TestMethod]
        public void DequeueChunk_WhenQueueIsNull_ThrowsNullReferenceExceptionOnEnumeration()
        {
            // Arrange
            Queue<int> queue = null;

            // Act
            Action action = () => queue.DequeueChunk(1).ToArray();

            // Assert
            action.Should().Throw<NullReferenceException>();
        }

        [TestMethod]
        public void DequeueChunk_ReturnsEmptySequenceForEmptyQueueAndZeroChunkSize()
        {
            // Arrange
            var emptyQueue = new Queue<int>();
            var populatedQueue = new Queue<int>(new[] { 1, 2, 3 });

            // Act
            var emptyResult = emptyQueue.DequeueChunk(3).ToArray();
            var zeroChunkResult = populatedQueue.DequeueChunk(0).ToArray();

            // Assert
            emptyResult.Should().BeEmpty();
            zeroChunkResult.Should().BeEmpty();
            populatedQueue.Should().Equal(1, 2, 3);
        }

        [TestMethod]
        public void DequeueChunk_DequeuesSingleAndMultipleItemsUpToRequestedBoundary()
        {
            // Arrange
            var singleItemQueue = new Queue<string>(new[] { "only" });
            var multipleItemQueue = new Queue<int>(new[] { 1, 2, 3, 4 });

            // Act
            var singleResult = singleItemQueue.DequeueChunk(5).ToArray();
            var boundaryResult = multipleItemQueue.DequeueChunk(3).ToArray();

            // Assert
            singleResult.Should().Equal("only");
            singleItemQueue.Should().BeEmpty();

            boundaryResult.Should().Equal(1, 2, 3);
            multipleItemQueue.Should().Equal(4);
        }
    }
}
