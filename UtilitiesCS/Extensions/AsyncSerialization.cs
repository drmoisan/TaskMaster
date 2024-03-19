using ExCSS;
using Microsoft.IO;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace UtilitiesCS
{
    public static class AsyncSerialization
    {
        internal const double MB = 1000000;

        internal static string ToMbString(this long bytes)
        {
            var mb = bytes / MB;
            return $"{mb:N1} MB";
        }

        
        public static async Task<string> ReadTextAsync(string filePath, IProgress<(double current, double total)> progress)
        {
            using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            using var reader = new StreamReader(stream, Encoding.UTF8);

            var readTask = reader.ReadToEndAsync();

            var progressTask = Task.Run(async () =>
            {
                while (stream.Position < stream.Length)
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(100));
                    progress.Report((stream.Position, stream.Length));
                }
            });

            await Task.WhenAll(readTask, progressTask);

            return readTask.Result;
        }

        public static async Task<string> ReadTextWithProgressAsync(this FilePathHelper disk, ProgressTrackerPane progress, string messagePrefix = "")
        {
            var sw = await Task.Run(Stopwatch.StartNew).ConfigureAwait(false);
            
            using var stream = new FileStream(disk.FilePath, FileMode.Open, FileAccess.Read);
            using var reader = new StreamReader(stream, Encoding.UTF8);

            var readTask = reader.ReadToEndAsync();

            var progressTask = Task.Run(async () =>
            {
                while (stream.Position < stream.Length)
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(1000));
                    var percent = ((double)stream.Position / stream.Length) * 100;
                    var msg = GetProgressMessage(stream.Position, stream.Length, sw, messagePrefix);

                    progress.Report(percent, msg);
                }
            });

            await Task.WhenAll(readTask, progressTask);

            return readTask.Result;
        }

        public static async Task<string> ReadTextWithProgressAsync(this FilePathHelper disk, ProgressTracker progress, string messagePrefix = "")
        {
            var sw = await Task.Run(Stopwatch.StartNew).ConfigureAwait(false);

            using var stream = new FileStream(disk.FilePath, FileMode.Open, FileAccess.Read);
            using var reader = new StreamReader(stream, Encoding.UTF8);

            var readTask = reader.ReadToEndAsync();

            var progressTask = Task.Run(async () =>
            {
                while (stream.Position < stream.Length)
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(200));
                    var percent = ((double)stream.Position / stream.Length) * 100;
                    var msg = GetProgressMessage(stream.Position, stream.Length, sw, messagePrefix);

                    progress.Report(percent, msg);
                }
            });

            await Task.WhenAll(readTask, progressTask);

            return readTask.Result;
        }

        public static async Task WriteTextWithProgressAsync(this FilePathHelper disk, string texts, ProgressTrackerPane progress, string messagePrefix = "")
        {
            var length = Encoding.UTF8.GetByteCount(texts);
            
            using var stream = new FileStream(disk.FilePath, FileMode.Open, FileAccess.Write);
            using var writer = new StreamWriter(stream, Encoding.UTF8);

            var writeTask = writer.WriteAsync(texts);

            var sw = await Task.Run(Stopwatch.StartNew).ConfigureAwait(false);
            var progressTask = Task.Run(async () =>
            {
                while (stream.Position < length)
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(200));
                    var percent = ((double)stream.Position / length) * 100;
                    var msg = GetProgressMessage(stream.Position, length, sw, messagePrefix);

                    progress.Report(percent, msg);
                }
            });

            await Task.WhenAll(writeTask, progressTask);

        }

        public static async Task SerializeWithProgressAsync<T>(this JsonSerializer serializer, T obj, FilePathHelper disk, ProgressTrackerPane progress, CancellationToken cancel, string messagePrefix = "")
        {
            // create this in the constructor, stream manages can be reused
            // see details in this answer https://stackoverflow.com/a/42599288/185498
            var streamManager = new RecyclableMemoryStreamManager();

            using var file = File.Open(disk.FilePath, FileMode.Create);

            // RecyclableMemoryStream will be returned, it inherits MemoryStream, however prevents data allocation into the LOH
            using var memoryStream = streamManager.GetStream(); 
            
            using var writer = new StreamWriter(memoryStream);
            serializer.Serialize(writer, obj);

            await writer.FlushAsync().ConfigureAwait(false);

            memoryStream.Seek(0, SeekOrigin.Begin);
            var length = memoryStream.Length;

            //await memoryStream.CopyToAsync(file).ConfigureAwait(false);
            //await file.FlushAsync().ConfigureAwait(false);
            
            var copyTask = memoryStream.CopyToAsync(file, 81920, cancel);
            var writeTask = copyTask.ContinueWith(t => file.FlushAsync(), cancel);

            var sw = await Task.Run(Stopwatch.StartNew).ConfigureAwait(false);
            var progressTask = Task.Run(async () =>
            {
                while (file.Position < length)
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(200));
                    var percent = ((double)file.Position / length) * 100;
                    var msg = GetProgressMessage(file.Position, length, sw, messagePrefix);

                    progress.Report(percent, msg);
                }
            }, cancel);

            await Task.WhenAll(writeTask, progressTask);

        }

        public static async Task CopyToAsync(
            this Stream source,
            long sourceLength,
            Stream destination,
            int bufferSize,
            ProgressTrackerPane progress, 
            string messagePrefix,
            CancellationToken cancellationToken)
        {
            const int _DefaultBufferSize = 81920;
            var sw = await Task.Run(Stopwatch.StartNew).ConfigureAwait(false);

            if (0 == bufferSize)
                bufferSize = _DefaultBufferSize;

            var buffer = new byte[bufferSize];
            if (0 > sourceLength && source.CanSeek)
                sourceLength = source.Length - source.Position;
            var totalBytesCopied = 0L;
            
            var (percent, message) = GetProgressParams(totalBytesCopied, sourceLength, sw, messagePrefix);
            progress?.Report(percent, message);
            
            var bytesRead = -1;

            while (0 != bytesRead && !cancellationToken.IsCancellationRequested)
            {
                bytesRead = await source.ReadAsync(buffer, 0, buffer.Length);
                if (0 == bytesRead || cancellationToken.IsCancellationRequested)
                    break;
                await destination.WriteAsync(buffer, 0, buffer.Length);
                totalBytesCopied += bytesRead;
                (percent, message) = GetProgressParams(totalBytesCopied, sourceLength, sw, messagePrefix);
                progress?.Report(percent, message);
            }

            if (0 < totalBytesCopied)
            {
                progress.Report(100);
            }
                
            cancellationToken.ThrowIfCancellationRequested();
        }


        /// <summary>
        /// Copys a stream to another stream
        /// </summary>
        /// <param name="source">The source <see cref="Stream"/> to copy from</param>
        /// <param name="sourceLength">The length of the source stream, 
        /// if known - used for progress reporting</param>
        /// <param name="destination">The destination <see cref="Stream"/> to copy to</param>
        /// <param name="bufferSize">The size of the copy block buffer</param>
        /// <param name="progress">An <see cref="IProgress{T}"/> implementation 
        /// for reporting progress</param>
        /// <param name="cancellationToken">A cancellation token</param>
        /// <returns>A task representing the operation</returns>
        public static async Task CopyToAsync(
            this Stream source,
            long sourceLength,
            Stream destination,
            int bufferSize,
            IProgress<KeyValuePair<long, long>> progress,
            CancellationToken cancellationToken)
        {
            const int _DefaultBufferSize = 81920;

            if (0 == bufferSize)
                bufferSize = _DefaultBufferSize;

            var buffer = new byte[bufferSize];
            if (0 > sourceLength && source.CanSeek)
                sourceLength = source.Length - source.Position;
            var totalBytesCopied = 0L;
            if (null != progress)
                progress.Report(new KeyValuePair<long, long>(totalBytesCopied, sourceLength));
            var bytesRead = -1;

            while (0 != bytesRead && !cancellationToken.IsCancellationRequested)
            {
                bytesRead = await source.ReadAsync(buffer, 0, buffer.Length);
                if (0 == bytesRead || cancellationToken.IsCancellationRequested)
                    break;
                await destination.WriteAsync(buffer, 0, buffer.Length);
                totalBytesCopied += bytesRead;
                if (null != progress)
                    progress.Report(new KeyValuePair<long, long>(totalBytesCopied, sourceLength));
            }

            if (0 < totalBytesCopied)
                progress.Report(new KeyValuePair<long, long>(totalBytesCopied, sourceLength));
            cancellationToken.ThrowIfCancellationRequested();
        }

        private static (double Percent, string Message) GetProgressParams(long complete, long count, Stopwatch sw, string messagePrefix)
        {
            var percent = ((double)complete / count) * 100;
            var msg = GetProgressMessage(complete, count, sw, messagePrefix);
            return (percent, msg);
        }

        private static string GetProgressMessage(long complete, long count, Stopwatch sw, string prefix)
        {
            double seconds = complete > 0 ? sw.Elapsed.TotalSeconds / complete : 0;
            var remaining = count - complete;
            var remainingSeconds = remaining * seconds;
            var ts = TimeSpan.FromSeconds(remainingSeconds);
            string msg = $"{prefix}Completed {complete.ToMbString()} of {count.ToMbString()} ({MB * seconds:N2} mps) " +
                $"({sw.Elapsed:%m\\:ss} elapsed {ts:%m\\:ss} remaining)";
            return msg;
        }
    }
}
