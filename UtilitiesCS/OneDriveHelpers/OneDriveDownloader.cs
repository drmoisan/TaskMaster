using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UtilitiesCS;
using UtilitiesCS.Extensions;

namespace UtilitiesCS.OneDriveHelpers
{
    public class OneDriveDownloader
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public OneDriveDownloader() 
        {
            Client = new();
            ClientGetAsync = Client.GetAsync;
        }

        protected HttpClient _client;
        public virtual HttpClient Client { get => _client; protected set => _client = value; }

        protected Func<string, CancellationToken, Task<HttpResponseMessage>> _clientGetAsync;
        public virtual Func<string, CancellationToken, Task<HttpResponseMessage>> ClientGetAsync { get => _clientGetAsync; protected set => _clientGetAsync = value; }

        public async Task<Stream> TryGetUrlStreamAsync(string url, int timeoutMs, CancellationToken cancel)
        {
            var response = await ClientGetAsync.RunWithTimeout(url, cancel, timeoutMs, 3, false);
            
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsStreamAsync();
            }
            else 
            { 
                logger.Debug($"Failed to get stream from {url}");
                return null;  
            }
        }

        public async Task DownloadFileAsync(string url, string destinationPath, int timeoutMs, CancellationToken cancel) 
        { 
            var contentStream = await TryGetUrlStreamAsync(url, timeoutMs, cancel);
            if (contentStream is null) { return; }
            var fileStream = await TryGetFileStreamWriter(destinationPath, timeoutMs, cancel);
            if (fileStream is null) { return; }
            
            await contentStream.TryCopyToAsyncWithTimeout(fileStream, cancel, timeoutMs, 3, false);
            fileStream?.Dispose();
            contentStream?.Dispose();
        }
                
        
        
        public virtual async Task<Stream> TryGetFileStreamWriter(string destinationPath, int timeoutMs, CancellationToken cancel)
        {
            try
            {
                var stream = await GetFileStreamWriter.RunWithTimeout(destinationPath, cancel, timeoutMs, 3, false);
                return stream;
            }
            catch (Exception)
            {
                return null;
            }
        }
        public virtual Func<string, Stream> GetFileStreamWriter { get => _getFileStreamWriter; protected set => _getFileStreamWriter = value; }
        protected Func<string, Stream> _getFileStreamWriter = (string destinationPath) => new FileStream(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true);

    }
}
