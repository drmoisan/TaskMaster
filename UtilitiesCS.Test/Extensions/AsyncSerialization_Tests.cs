using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UtilitiesCS.Test.Extensions
{
    [TestClass]
    public class AsyncSerialization_Tests
    {
        [TestMethod]
        public async Task CopyToAsync_ShouldCopyStreamAndReportProgress()
        {
            // Arrange
            using var source = new MemoryStream(new byte[] { 1, 2, 3, 4, 5 });
            using var destination = new MemoryStream();
            var reports = new List<KeyValuePair<long, long>>();
            var progress = new Progress<KeyValuePair<long, long>>(reports.Add);

            // Act
            await source.CopyToAsync(
                sourceLength: source.Length,
                destination,
                bufferSize: 5,
                progress,
                CancellationToken.None
            );

            // Assert
            destination.ToArray().Should().Equal(new byte[] { 1, 2, 3, 4, 5 });
            reports.Should().NotBeEmpty();
            reports[^1].Key.Should().Be(5);
            reports[^1].Value.Should().Be(5);
        }

        [TestMethod]
        public async Task CopyToAsync_ShouldThrowWhenCancellationIsRequestedBeforeCopy()
        {
            // Arrange
            using var source = new MemoryStream(new byte[] { 1, 2, 3 });
            using var destination = new MemoryStream();
            using var cancellationSource = new CancellationTokenSource();
            cancellationSource.Cancel();

            // Act
            Func<Task> act = async () =>
                await source.CopyToAsync(
                    sourceLength: source.Length,
                    destination,
                    bufferSize: 2,
                    progress: null,
                    cancellationSource.Token
                );

            // Assert
            await act.Should().ThrowAsync<OperationCanceledException>();
        }

        [TestMethod]
        public async Task CopyToAsync_ShouldSupportEmptyStreams()
        {
            // Arrange
            using var source = new MemoryStream(Array.Empty<byte>());
            using var destination = new MemoryStream();
            var reports = new List<KeyValuePair<long, long>>();
            var progress = new Progress<KeyValuePair<long, long>>(reports.Add);

            // Act
            await source.CopyToAsync(
                sourceLength: 0,
                destination,
                bufferSize: 0,
                progress,
                CancellationToken.None
            );

            // Assert
            destination.Length.Should().Be(0);
            reports.Should().ContainSingle();
            reports[0].Key.Should().Be(0);
            reports[0].Value.Should().Be(0);
        }
    }
}
