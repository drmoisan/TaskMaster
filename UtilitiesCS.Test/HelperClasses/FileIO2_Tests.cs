using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UtilitiesCS.Test.HelperClasses
{
    [TestClass]
    public class FileIO2_Tests
    {
        [TestMethod]
        public void DeleteTextFile_WhenTargetIsMissing_ShouldNotThrow()
        {
            Action act = () => FileIO2.DELETE_TextFile("missing.csv", GetMissingFolder());

            act.Should().NotThrow();
        }

        [TestMethod]
        public void WriteTextFile_WhenDevicePathIsUsed_ShouldThrowNotSupportedException()
        {
            Action act = () => FileIO2.WriteTextFile("NUL", new[] { "alpha", "beta" }, "");

            act.Should().Throw<NotSupportedException>();
        }

        /// <summary>
        /// A failure raised after the writer opened is terminal: the file is opened in append mode,
        /// so retrying after a partial flush would duplicate lines. The observable proof that no
        /// retry occurred is a delay-delegate invocation count of zero.
        /// </summary>
        [TestMethod]
        public async Task WriteTextFileAsync_WhenWriteFailsAfterOpen_ShouldReturnFalseWithoutRetrying()
        {
            // Arrange
            int midWriteFactoryCalls = 0;
            int midWriteDelayCalls = 0;
            using var cts = new CancellationTokenSource();

            // Act
            bool midWriteResult = await FileIO2.WriteTextFileAsync(
                "irrelevant.csv",
                new[] { "alpha" },
                "irrelevant-folder",
                cts.Token,
                writerFactory: _ =>
                {
                    midWriteFactoryCalls++;
                    return new ThrowingOnWriteTextWriter();
                },
                delay: (ms, t) =>
                {
                    midWriteDelayCalls++;
                    return Task.CompletedTask;
                }
            );

            // Assert
            midWriteFactoryCalls.Should().Be(1);
            midWriteDelayCalls.Should().Be(0);
            midWriteResult.Should().BeFalse();
        }

        /// <summary>
        /// Retry exhaustion: every open attempt fails, so the loop consumes its whole 100-attempt
        /// budget and awaits 99 delays between them. No filesystem access and no wall-clock wait.
        /// </summary>
        [TestMethod]
        public async Task WriteTextFileAsync_WhenEveryOpenAttemptFails_ShouldReturnFalseAfterBudget()
        {
            // Arrange
            int exhaustionFactoryCalls = 0;
            int exhaustionDelayCalls = 0;
            using var cts = new CancellationTokenSource();

            // Act
            bool exhaustionResult = await FileIO2.WriteTextFileAsync(
                "irrelevant.csv",
                new[] { "alpha" },
                "irrelevant-folder",
                cts.Token,
                writerFactory: _ =>
                {
                    exhaustionFactoryCalls++;
                    throw new IOException("Simulated open failure.");
                },
                delay: (ms, t) =>
                {
                    exhaustionDelayCalls++;
                    return Task.CompletedTask;
                }
            );

            // Assert
            exhaustionResult.Should().BeFalse();
            exhaustionFactoryCalls.Should().Be(100);
            exhaustionDelayCalls.Should().Be(99);
        }

        /// <summary>
        /// The success path: a transient inability to open resolves within the retry budget, so the
        /// method reports success and the writer receives every supplied line.
        /// </summary>
        [TestMethod]
        public async Task WriteTextFileAsync_WhenTransientOpenFailureThenSucceeds_ShouldReturnTrueAndWriteAllLines()
        {
            // Arrange
            int transientOpenAttempts = 0;
            int transientDelayCalls = 0;
            var sink = new StringWriter();
            var lines = new[] { "alpha", "beta" };
            string expectedContent = "alpha" + Environment.NewLine + "beta" + Environment.NewLine;
            using var cts = new CancellationTokenSource();

            // Act
            bool transientResult = await FileIO2.WriteTextFileAsync(
                "irrelevant.csv",
                lines,
                "irrelevant-folder",
                cts.Token,
                writerFactory: _ =>
                {
                    transientOpenAttempts++;
                    if (transientOpenAttempts <= 3)
                    {
                        throw new IOException("Simulated transient open failure.");
                    }
                    return sink;
                },
                delay: (ms, t) =>
                {
                    transientDelayCalls++;
                    return Task.CompletedTask;
                }
            );
            string transientContent = sink.ToString();

            // Assert
            transientResult.Should().BeTrue();
            transientDelayCalls.Should().Be(3);
            transientContent.Should().Be(expectedContent);
        }

        /// <summary>
        /// A token that is already cancelled must be observed before the writer is ever obtained,
        /// so no file handle is opened on a doomed call.
        /// </summary>
        [TestMethod]
        public async Task WriteTextFileAsync_WhenTokenAlreadyCancelled_ShouldThrowBeforeOpening()
        {
            // Arrange
            int cancelledFactoryCalls = 0;
            using var cts = new CancellationTokenSource();
            cts.Cancel();

            Func<Task> act = () =>
                FileIO2.WriteTextFileAsync(
                    "irrelevant.csv",
                    new[] { "alpha" },
                    "irrelevant-folder",
                    cts.Token,
                    writerFactory: _ =>
                    {
                        cancelledFactoryCalls++;
                        return new StringWriter();
                    },
                    delay: (ms, t) => Task.CompletedTask
                );

            // Act & Assert
            await act.Should().ThrowAsync<OperationCanceledException>();
            cancelledFactoryCalls.Should().Be(0);
        }

        /// <summary>
        /// Cancellation signalled from inside the retry window is observed by the next iteration's
        /// cancellation check, so the call abandons promptly instead of consuming the whole budget.
        /// The delay seam does the cancelling, so no wall-clock wait is involved.
        /// </summary>
        [TestMethod]
        public async Task WriteTextFileAsync_WhenCancelledDuringRetryWindow_ShouldThrowPromptly()
        {
            // Arrange
            int retryCancelFactoryCalls = 0;
            using var cts = new CancellationTokenSource();

            Func<Task> act = () =>
                FileIO2.WriteTextFileAsync(
                    "irrelevant.csv",
                    new[] { "alpha" },
                    "irrelevant-folder",
                    cts.Token,
                    writerFactory: _ =>
                    {
                        retryCancelFactoryCalls++;
                        throw new IOException("Simulated open failure.");
                    },
                    delay: (ms, t) =>
                    {
                        cts.Cancel();
                        return Task.CompletedTask;
                    }
                );

            // Act & Assert
            await act.Should().ThrowAsync<OperationCanceledException>();
            retryCancelFactoryCalls.Should().Be(1);
        }

        /// <summary>
        /// The caller's token must reach the retry delay. Without this the delay is uncancellable
        /// and a caller supplying a real token is still stalled for the whole retry window.
        /// </summary>
        [TestMethod]
        public async Task WriteTextFileAsync_WhenRetrying_ShouldPassCallerTokenToDelay()
        {
            // Arrange
            int tokenOpenAttempts = 0;
            var capturedTokens = new List<CancellationToken>();
            using var cts = new CancellationTokenSource();
            CancellationToken token = cts.Token;

            // Act
            await FileIO2.WriteTextFileAsync(
                "irrelevant.csv",
                new[] { "alpha" },
                "irrelevant-folder",
                token,
                writerFactory: _ =>
                {
                    tokenOpenAttempts++;
                    if (tokenOpenAttempts <= 2)
                    {
                        throw new IOException("Simulated transient open failure.");
                    }
                    return new StringWriter();
                },
                delay: (ms, t) =>
                {
                    capturedTokens.Add(t);
                    return Task.CompletedTask;
                }
            );

            // Assert
            capturedTokens.Should().HaveCount(2);
            capturedTokens.Should().OnlyContain(t => t.Equals(token));
        }

        /// <summary>
        /// A <see cref="TextWriter"/> that opens successfully and then fails on the first write.
        /// This is the only way to observe a mid-write failure without external interference,
        /// because a real StreamWriter cannot be made to fail after opening from inside a test.
        /// </summary>
        private sealed class ThrowingOnWriteTextWriter : TextWriter
        {
            public override System.Text.Encoding Encoding => System.Text.Encoding.UTF8;

            public override Task WriteLineAsync(string value)
            {
                throw new IOException("Simulated mid-write failure.");
            }
        }

        [TestMethod]
        public void CsvReaders_WithFixtureAndMissingFiles_ShouldRespectHeaderOptions()
        {
            var (fileName, folderPath) = GetFixtureLocation();

            FileIO2.CSV_ReadTxtF(fileName, folderPath).Should().Equal("Alpha,1", "Beta,2");
            FileIO2
                .CSV_ReadTxtF(fileName, folderPath, skipHeaders: false)
                .Should()
                .Equal("Name,Value", "Alpha,1", "Beta,2");
            FileIO2.CSV_ReadTxtF("missing.csv", folderPath).Should().BeNull();
            FileIO2
                .CsvRead(fileName, folderPath, skipHeaders: true)
                .Should()
                .Equal("Alpha,1", "Beta,2");
            FileIO2.CsvRead(fileName, folderPath).Should().Equal("Name,Value", "Alpha,1", "Beta,2");
            FileIO2.CsvRead("missing.csv", folderPath).Should().BeNull();
        }

        [TestMethod]
        public void SplitArrayTo2D_ShouldSupportZeroAndOneBasedLayouts()
        {
            var source = new[] { "A,B", "C,D,E" };

            var oneBased = FileIO2.SplitArrayTo2D(source);
            var zeroBased = FileIO2.SplitArrayTo2D(source, zerobased: true);

            oneBased[1, 1].Should().Be("A");
            oneBased[2, 3].Should().Be("E");
            zeroBased[0, 0].Should().Be("A");
            zeroBased[1, 2].Should().Be("E");
        }

        [TestMethod]
        public void CsvReadTo2D_AndCsvReadToJagged_ShouldProjectFixtureRows()
        {
            var (fileName, folderPath) = GetFixtureLocation();

            var matrix = FileIO2.CsvReadTo2D(fileName, folderPath, skipHeaders: true);
            var jagged = FileIO2.CsvReadToJagged(fileName, folderPath, skipHeaders: true);

            matrix[1, 1].Should().Be("Alpha");
            matrix[2, 2].Should().Be("2");
            jagged[0].Should().Equal("Alpha", "1");
            jagged[1].Should().Equal("Beta", "2");
        }

        private static string GetMissingFolder()
        {
            return Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "missing-fileio2-folder-for-tests"
            );
        }

        private static (string FileName, string FolderPath) GetFixtureLocation()
        {
            var fullPath = Path.GetFullPath(
                Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    @"..\..\TestData\FileIO2\sample.csv"
                )
            );

            return (Path.GetFileName(fullPath), Path.GetDirectoryName(fullPath));
        }
    }
}
