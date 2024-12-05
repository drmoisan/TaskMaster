using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Threading;

namespace UtilitiesCS.Extensions
{
    public static class StreamExtensions
    {
        public static async Task<bool> TryCopyToAsyncWithTimeout(this Stream source, Stream destination, CancellationToken cancel, int timeoutMs, int maxRetries, bool throwOnFail)
        {
            Func<Stream, int, CancellationToken, Task> copy = source.CopyToAsync;
            try
            {
                await copy.RunWithTimeout(destination, 81920, cancel, timeoutMs, maxRetries, throwOnFail);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
            
    }
}
