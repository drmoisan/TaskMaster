#nullable enable
using System;

namespace UtilitiesCS
{
    public static partial class OlTableExtensions
    {
        /// <summary>
        /// Builds the exception that reports an exhausted table-acquisition budget. It returns the
        /// exception rather than throwing it so that definite-assignment and nullable flow analysis
        /// stay correct at every call site without a does-not-return attribute, which the net48 base
        /// class library does not provide.
        /// </summary>
        /// <param name="counter">The retry number the exhausted attempt was made on.</param>
        /// <param name="timeoutMs">The millisecond budget each attempt was given.</param>
        /// <param name="inner">
        /// The originating exception, carried as the inner exception so the cause survives. A null
        /// value is legal and produces an exception with no inner exception.
        /// </param>
        /// <returns>A <see cref="TimeoutException"/> the caller is expected to throw.</returns>
        private static TimeoutException AcquisitionTimeout(
            int counter,
            int timeoutMs,
            System.Exception? inner = null
        )
        {
            return new TimeoutException(
                $"The table acquisition timed out on retry {counter} after {timeoutMs} ms.",
                inner
            );
        }
    }
}
