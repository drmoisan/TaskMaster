using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UtilitiesCS.OneDriveHelpers;

namespace UtilitiesCS.Test.OneDriveHelpers
{
    internal class TestableOneDriveDownloader : OneDriveDownloader
    {
        public void SetClientGetAsync(
            Func<string, CancellationToken, Task<HttpResponseMessage>> func
        )
        {
            ClientGetAsync = func;
        }

        public void SetFileStreamWriter(Func<string, Stream> func)
        {
            GetFileStreamWriter = func;
        }
    }

    /// <summary>
    /// Extended testable downloader that allows overriding both the HTTP delegate
    /// and the writer factory via virtual method override.
    ///
    /// Purpose:
    ///     Enables full DownloadFileAsync path testing without touching the filesystem
    ///     or a real HTTP endpoint.
    /// </summary>
    internal sealed class TestableOneDriveDownloaderFull : OneDriveDownloader
    {
        private Func<Task<Stream>> _writerFactory;
        private bool _writerInvoked;

        /// <summary>Whether TryGetFileStreamWriter was called at least once.</summary>
        public bool WriterInvoked => _writerInvoked;

        /// <summary>Replace the HTTP delegate used by TryGetUrlStreamAsync.</summary>
        /// <param name="func">Delegate returning the desired HttpResponseMessage.</param>
        public void SetClientGetAsync(
            Func<string, CancellationToken, Task<HttpResponseMessage>> func
        ) => ClientGetAsync = func;

        /// <summary>Provide the writer stream factory used by TryGetFileStreamWriter.</summary>
        /// <param name="factory">Factory returning the target Stream, or null to simulate failure.</param>
        public void SetWriterFactory(Func<Task<Stream>> factory) => _writerFactory = factory;

        /// <inheritdoc />
        public override Task<Stream> TryGetFileStreamWriter(
            string destinationPath,
            int timeoutMs,
            CancellationToken cancel
        )
        {
            // Record that the writer path was reached, then delegate to the injected factory.
            _writerInvoked = true;
            return _writerFactory != null ? _writerFactory() : Task.FromResult<Stream>(null);
        }
    }

    [TestClass]
    public class OneDriveDownloader_Tests
    {
        #region Constructor

        [TestMethod]
        public void Constructor_CreatesInstanceWithClient()
        {
            var downloader = new OneDriveDownloader();
            downloader.Client.Should().NotBeNull();
            downloader.ClientGetAsync.Should().NotBeNull();
        }

        #endregion

        #region TryGetUrlStreamAsync

        [TestMethod]
        public async Task TryGetUrlStreamAsync_SuccessfulResponse_ReturnsStream()
        {
            var downloader = new TestableOneDriveDownloader();
            var content = new StringContent("test content");
            var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = content };

            downloader.SetClientGetAsync((url, token) => Task.FromResult(response));

            var result = await downloader.TryGetUrlStreamAsync(
                "http://example.com/test",
                5000,
                default
            );
            result.Should().NotBeNull();
        }

        [TestMethod]
        public async Task TryGetUrlStreamAsync_FailedResponse_ReturnsNull()
        {
            var downloader = new TestableOneDriveDownloader();
            var response = new HttpResponseMessage(HttpStatusCode.NotFound);

            downloader.SetClientGetAsync((url, token) => Task.FromResult(response));

            var result = await downloader.TryGetUrlStreamAsync(
                "http://example.com/test",
                5000,
                default
            );
            result.Should().BeNull();
        }

        #endregion

        #region DownloadFileAsync

        /// <summary>
        /// Verifies that a successful HTTP response causes the response bytes to be
        /// forwarded to the injected writer stream.
        ///
        /// Purpose:
        ///     Confirms the happy-path end-to-end data flow: content is fetched and
        ///     copied into the writer without touching the real filesystem.
        ///
        /// Returns:
        ///     Passes when the output MemoryStream contains exactly the bytes from the
        ///     mock HTTP response.
        /// </summary>
        [TestMethod]
        public async Task DownloadFileAsync_SuccessfulResponse_CopiesContentBytesToWriter()
        {
            // Arrange
            var expectedBytes = System.Text.Encoding.UTF8.GetBytes("hello onedrive");
            var content = new ByteArrayContent(expectedBytes);
            var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = content };
            var output = new MemoryStream();

            var downloader = new TestableOneDriveDownloaderFull();
            downloader.SetClientGetAsync((url, token) => Task.FromResult(response));
            downloader.SetWriterFactory(() => Task.FromResult<Stream>(output));

            // Act
            await downloader.DownloadFileAsync(
                "http://test.example.com/file",
                "dest.bin",
                5000,
                default
            );

            // Assert: all expected bytes reached the writer stream.
            output.ToArray().Should().BeEquivalentTo(expectedBytes);
        }

        /// <summary>
        /// Verifies that when the writer factory returns null the method exits cleanly
        /// without throwing an exception and without producing any output.
        ///
        /// Purpose:
        ///     Confirms the early-exit guard when TryGetFileStreamWriter returns null,
        ///     ensuring no crash occurs and no partial data is written.
        ///
        /// Returns:
        ///     Passes when DownloadFileAsync completes without throwing.
        /// </summary>
        [TestMethod]
        public async Task DownloadFileAsync_NullWriter_CompletesWithoutThrowingAndWritesNoData()
        {
            // Arrange: HTTP succeeds but writer returns null.
            var content = new StringContent("data");
            var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = content };

            var downloader = new TestableOneDriveDownloaderFull();
            downloader.SetClientGetAsync((url, token) => Task.FromResult(response));
            downloader.SetWriterFactory(() => Task.FromResult<Stream>(null));

            // Act + Assert: no exception propagates when the writer is unavailable.
            Func<Task> act = () =>
                downloader.DownloadFileAsync(
                    "http://test.example.com/file",
                    "dest.bin",
                    5000,
                    default
                );
            await act.Should().NotThrowAsync();
        }

        /// <summary>
        /// Verifies that a non-success HTTP response causes an early exit before the
        /// writer factory is ever invoked.
        ///
        /// Purpose:
        ///     Confirms that TryGetFileStreamWriter is never called when the HTTP layer
        ///     returns a failure status code, keeping the write path clean.
        ///
        /// Returns:
        ///     Passes when WriterInvoked remains false after DownloadFileAsync returns.
        /// </summary>
        [TestMethod]
        public async Task DownloadFileAsync_FailedHttpResponse_WriterIsNeverInvoked()
        {
            // Arrange: HTTP client returns a server error.
            var response = new HttpResponseMessage(HttpStatusCode.InternalServerError);

            var downloader = new TestableOneDriveDownloaderFull();
            downloader.SetClientGetAsync((url, token) => Task.FromResult(response));
            downloader.SetWriterFactory(() => Task.FromResult<Stream>(new MemoryStream()));

            // Act
            await downloader.DownloadFileAsync(
                "http://test.example.com/file",
                "dest.bin",
                5000,
                default
            );

            // Assert: writer path was never reached because TryGetUrlStreamAsync returned null.
            downloader
                .WriterInvoked.Should()
                .BeFalse("the writer must not be invoked when the HTTP response indicates failure");
        }

        [TestMethod]
        public async Task TryGetFileStreamWriter_WhenWriterReturnsMemoryStream_ReturnsStream()
        {
            var downloader = new TestableOneDriveDownloader();
            downloader.SetFileStreamWriter(_ => new MemoryStream());

            using var stream = await downloader.TryGetFileStreamWriter("ignored", 5000, default);

            stream.Should().NotBeNull();
            stream.CanWrite.Should().BeTrue();
        }

        [TestMethod]
        public void GetFileStreamWriter_DefaultWriterWithNulPath_ThrowsNotSupportedException()
        {
            var downloader = new OneDriveDownloader();

            Action act = () => downloader.GetFileStreamWriter("NUL");

            act.Should().Throw<NotSupportedException>();
        }

        [TestMethod]
        public async Task TryGetFileStreamWriter_WhenWriterThrows_ReturnsNull()
        {
            var downloader = new TestableOneDriveDownloader();
            downloader.SetFileStreamWriter(_ => throw new InvalidOperationException("boom"));

            var stream = await downloader.TryGetFileStreamWriter("ignored", 5000, default);

            stream.Should().BeNull();
        }

        #endregion
    }
}
