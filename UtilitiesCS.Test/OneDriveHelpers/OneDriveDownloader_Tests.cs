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
        public void SetClientGetAsync(Func<string, CancellationToken, Task<HttpResponseMessage>> func)
        {
            ClientGetAsync = func;
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

            var result = await downloader.TryGetUrlStreamAsync("http://example.com/test", 5000, default);
            result.Should().NotBeNull();
        }

        [TestMethod]
        public async Task TryGetUrlStreamAsync_FailedResponse_ReturnsNull()
        {
            var downloader = new TestableOneDriveDownloader();
            var response = new HttpResponseMessage(HttpStatusCode.NotFound);

            downloader.SetClientGetAsync((url, token) => Task.FromResult(response));

            var result = await downloader.TryGetUrlStreamAsync("http://example.com/test", 5000, default);
            result.Should().BeNull();
        }

        #endregion
    }
}
