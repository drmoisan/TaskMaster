using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace UtilitiesCS.Extensions
{
    public static class StreamExtensions
    {
        public static async Task<bool> TryCopyToAsyncWithTimeout(
            this Stream source,
            Stream destination,
            CancellationToken cancel,
            int timeoutMs,
            int maxRetries,
            bool throwOnFail
        )
        {
            Func<Stream, int, CancellationToken, Task> copy = source.CopyToAsync;
            try
            {
                await copy.RunWithTimeout(
                    destination,
                    81920,
                    cancel,
                    timeoutMs,
                    maxRetries,
                    throwOnFail
                );
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
