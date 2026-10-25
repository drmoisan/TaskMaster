using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Time.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace UtilitiesCS.Test.Extensions
{
    /// <summary>
    /// <see cref="DfDeedle"/> AddQfcColumnsAsync tests. Each call passes a
    /// <see cref="FakeTimeProvider"/> that is never advanced through the existing
    /// <c>timeProvider</c> parameter, so no column-add deadline can fire and the outcome depends
    /// only on the column-add work, never on wall-clock time or scheduler load. The
    /// <c>columnAdder</c> parameter is left at its production default so the real
    /// <c>AddQfcColumns</c> runs against the mocked COM table.
    /// </summary>
    public partial class DfDeedle_COM_Tests
    {
        [TestMethod]
        public async Task AddQfcColumnsAsync_HappyPath_CompletesWithoutThrowing()
        {
            // Arrange: table + folder arranged so AddQfcColumns succeeds; the fake clock never
            // advances, so none of the three column-add deadlines can expire.
            var (mockTable, mockColumns) = BuildTableMock();
            var folder = BuildFolderWithUdp("Triage");
            using var cts = new CancellationTokenSource();

            // Act: call the internal method directly and await it. Reflection is not usable here:
            // it does not apply C# default parameter values without Type.Missing plus
            // BindingFlags.OptionalParamBinding.
            var act = async () =>
                await DfDeedle.AddQfcColumnsAsync(
                    mockTable.Object,
                    folder.Object,
                    cts.Token,
                    0,
                    timeProvider: new FakeTimeProvider()
                );

            // Assert: completes without exception and the production adder added the column.
            await act.Should().NotThrowAsync();
            mockColumns.Verify(c => c.Add("SentOn"), Times.Once);
        }

        [TestMethod]
        public async Task AddQfcColumnsAsync_PreCancelledToken_CompletesGracefully()
        {
            // Arrange: a pre-cancelled token causes the inner Task.Run to be cancelled
            // immediately; the method's catch block must handle it without re-throwing.
            var (mockTable, _) = BuildTableMock();
            var folder = BuildFolderWithUdp("Triage");
            using var cts = new CancellationTokenSource();
            cts.Cancel();

            // Act
            var act = async () =>
                await DfDeedle.AddQfcColumnsAsync(
                    mockTable.Object,
                    folder.Object,
                    cts.Token,
                    0,
                    timeProvider: new FakeTimeProvider()
                );

            // Assert: must not propagate the cancellation as an unhandled exception.
            await act.Should().NotThrowAsync();
        }
    }
}
