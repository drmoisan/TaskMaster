using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using UtilitiesCS.OneDriveHelpers;
using System.Net.Http;
using System.Net;
using System.IO;
using System.Text;
using System.Linq;
using FluentAssertions;

namespace UtilitiesCS.Test.OneDriveHelpers
{
    [TestClass]
    public class OneDriveDownloaderTests
    {
        private MockRepository mockRepository;
        //private Mock<OneDriveDownloader> downloaderMock;
        //private Mock<HttpClient> clientMock;
        //private HttpResponseMessage httpResponseMessage;
        



        [TestInitialize]
        public void TestInitialize()
        {
            this.mockRepository = new MockRepository(MockBehavior.Loose);
            
            
        }

        public class Downloader : OneDriveDownloader
        {
            public Downloader() : base() { }
            public void SetClientGetter(Func<string, CancellationToken, Task<HttpResponseMessage>> clientGetter) => ClientGetAsync = clientGetter;
            public void SetFilestreamWriterGetter(Func<string, Stream> filestreamWriterGetter) => GetFileStreamWriter = filestreamWriterGetter;
        }

        private OneDriveDownloader CreateOneDriveDownloader()
        {
            return new OneDriveDownloader();
        }

        public byte[] StringToByteArray(string input)
        {
            return Encoding.UTF8.GetBytes(input);
        }

        public byte[] StreamToByteArray(Stream input) 
        {
            using (var memoryStream = new MemoryStream())
            {
                input.CopyTo(memoryStream);
                return memoryStream.ToArray();
            };
        }

        //[TestMethod]
        //public async Task TryGetUrlStreamAsync_StateUnderTest_ExpectedBehavior()
        //{
        //    // Arrange
        //    var downloader = new Downloader();
        //    httpResponseMessage = new HttpResponseMessage(HttpStatusCode.Created);
        //    var expected = StringToByteArray("test");
        //    httpResponseMessage.Content = new ByteArrayContent(expected);
        //    downloader.SetClientGetter((string url, CancellationToken cancel) => Task.FromResult(httpResponseMessage));
        //    string url = "https://test.com";
        //    int timeoutMs = 1000;
        //    CancellationTokenSource tokenSource = new();
        //    CancellationToken cancel = tokenSource.Token;
        //    byte[] actual = null;

        //    // Act
        //    var result = await downloader.TryGetUrlStreamAsync(
        //        url,
        //        timeoutMs,
        //        cancel);
            
        //    actual = StreamToByteArray(result);
            
        //    // Assert
        //    expected.Should().BeEquivalentTo(actual);
        //}

        //[TestMethod]
        //public async Task DownloadFileAsync_StateUnderTest_ExpectedBehavior()
        //{
        //    // Arrange
        //    var oneDriveDownloader = this.CreateOneDriveDownloader();
        //    string url = "https://sabradipping.sharepoint.com/:x:/s/Sales-LargeFormatTeam2/EVpXvJ8dfZ9DmaoSjGJ16uIBuc7kN7LG9esDj9pciuoriA";
        //    string destinationPath = "C:\\Temp\\test.xlsx";
        //    int timeoutMs = 10000;
        //    CancellationTokenSource tokenSource = new();
        //    CancellationToken cancel = tokenSource.Token;

        //    // Act
        //    await oneDriveDownloader.DownloadFileAsync(
        //        url,
        //        destinationPath,
        //        timeoutMs,
        //        cancel);

        //    // Assert
            
        //}

        //[TestMethod]
        //public async Task TryGetFileStreamWriter_StateUnderTest_ExpectedBehavior()
        //{
        //    // Arrange
        //    var downloader = new Downloader();
        //    var expected = StringToByteArray("Test");
        //    downloader.SetFilestreamWriterGetter((string destinationPath) => new MemoryStream(expected));
        //    string destinationPath = null;
        //    int timeoutMs = 100000;
        //    CancellationTokenSource tokenSource = new();
        //    CancellationToken cancel = tokenSource.Token;

        //    // Act
        //    var result = await downloader.TryGetFileStreamWriter(
        //        destinationPath,
        //        timeoutMs,
        //        cancel);
        //    var actual = StreamToByteArray(result);
            

        //    // Assert
        //    actual.Should().BeEquivalentTo(expected);
        //}
    }
}
