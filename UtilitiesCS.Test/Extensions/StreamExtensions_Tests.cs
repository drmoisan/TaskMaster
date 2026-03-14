using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UtilitiesCS.Extensions;

namespace UtilitiesCS.Test.Extensions
{
    [TestClass]
    public class StreamExtensions_Tests
    {
        [TestMethod]
        public async Task TryCopyToAsyncWithTimeout_ShouldCopyMemoryStreamContents_WhenCopyCompletesInTime()
        {
            // Arrange
            using var source = new MemoryStream(Encoding.UTF8.GetBytes("hello world"));
            using var destination = new MemoryStream();

            // Act
            bool copied = await source.TryCopyToAsyncWithTimeout(destination, CancellationToken.None, timeoutMs: 100, maxRetries: 0, throwOnFail: true);

            // Assert
            copied.Should().BeTrue();
            Encoding.UTF8.GetString(destination.ToArray()).Should().Be("hello world");
        }

        [TestMethod]
        public async Task TryCopyToAsyncWithTimeout_ShouldReturnFalse_WhenCancellationIsAlreadyRequested()
        {
            // Arrange
            using var source = new MemoryStream(Encoding.UTF8.GetBytes("hello world"));
            using var destination = new MemoryStream();
            using var cancellationSource = new CancellationTokenSource();
            cancellationSource.Cancel();

            // Act
            bool copied = await source.TryCopyToAsyncWithTimeout(destination, cancellationSource.Token, timeoutMs: 100, maxRetries: 0, throwOnFail: true);

            // Assert
            copied.Should().BeFalse();
            destination.Length.Should().Be(0);
        }

        [TestMethod]
        public async Task TryCopyToAsyncWithTimeout_ShouldStillReturnTrue_WhenCopyIsSlowButCancellationDoesNotOccur()
        {
            // Arrange
            using var source = new SlowMemoryStream(Encoding.UTF8.GetBytes("hello world"), delayMilliseconds: 100);
            using var destination = new MemoryStream();

            // Act
            bool copied = await source.TryCopyToAsyncWithTimeout(destination, CancellationToken.None, timeoutMs: 10, maxRetries: 0, throwOnFail: true);

            // Assert
            copied.Should().BeTrue();
            Encoding.UTF8.GetString(destination.ToArray()).Should().Be("hello world");
        }

        private sealed class SlowMemoryStream : MemoryStream
        {
            private readonly int _delayMilliseconds;

            public SlowMemoryStream(byte[] buffer, int delayMilliseconds) : base(buffer)
            {
                _delayMilliseconds = delayMilliseconds;
            }

            public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
            {
                await Task.Delay(_delayMilliseconds, cancellationToken);
                return await base.ReadAsync(buffer, offset, count, cancellationToken);
            }
        }
    }
}
