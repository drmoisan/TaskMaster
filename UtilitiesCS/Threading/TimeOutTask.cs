using Microsoft.Office.Core;
using Microsoft.Office.Interop.Outlook;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace UtilitiesCS
{
    public static class TimeOutTask
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #region RunWithTimeout<TResult>

        public static async Task<TResult> RunWithTimeout<TResult>(this Func<TResult> function, CancellationToken token, int milliseconds, int maxAttempts, bool strict)
        {
            return await function.RunWithTimeout(token, milliseconds, maxAttempts, strict, 0);
        }

        private static async Task<TResult> RunWithTimeout<TResult>(this Func<TResult> function, CancellationToken token, int milliseconds, int maxAttempts, bool strict, int attempt)
        {
            token.ThrowIfCancellationRequested();

            var timeoutSource = new CancellationTokenSource(milliseconds);
            var combinedToken = CancellationTokenSource.CreateLinkedTokenSource(token, timeoutSource.Token);

            TResult result = default(TResult);
            try
            {
                result = await Task.Run(() => function(), combinedToken.Token);
            }
            catch (TaskCanceledException)
            {
                token.ThrowIfCancellationRequested();

                if (attempt < maxAttempts)
                {
                    result = await function.RunWithTimeout(token, milliseconds, maxAttempts, strict, attempt + 1);
                }
                else
                {
                    logger.Warn($"Task timed out after {attempt} attempts.");
                }
            }
            catch (System.Exception e)
            {
                logger.Error(e);
                if (strict) { throw e; }
            }

            return result;
        }

        public static async Task<TResult> RunWithTimeout<TResult>(this Func<CancellationToken, Task<TResult>> function, CancellationToken token, int milliseconds, int maxAttempts, bool strict)
        {
            return await function.RunWithTimeout(token, milliseconds, maxAttempts, strict, 0);
        }

        private static async Task<TResult> RunWithTimeout<TResult>(this Func<CancellationToken, Task<TResult>> task, CancellationToken token, int milliseconds, int maxAttempts, bool strict, int attempt)
        {
            token.ThrowIfCancellationRequested();

            var timeoutSource = new CancellationTokenSource(milliseconds);
            var combinedToken = CancellationTokenSource.CreateLinkedTokenSource(token, timeoutSource.Token);

            TResult result = default;
            try
            {
                result = await task(combinedToken.Token);
            }
            catch (TaskCanceledException)
            {
                token.ThrowIfCancellationRequested();

                if (attempt < maxAttempts)
                {
                    result = await task.RunWithTimeout(token, milliseconds, maxAttempts, strict, attempt + 1);
                }
                else
                {
                    logger.Warn($"Task timed out after {attempt} attempts.");
                }
            }
            catch (System.Exception e)
            {
                logger.Error(e);
                if (strict) { throw e; }
            }

            return result;
        }


        #endregion RunWithTimeout<TResult>

        #region RunWithTimeout<T1, TResult>

        public static async Task<TResult> RunWithTimeout<T1, TResult>(this Func<T1, TResult> function, T1 arg1, CancellationToken token, int milliseconds, int maxAttempts, bool strict)
        {
            return await function.RunWithTimeout(arg1, token, milliseconds, maxAttempts, strict, 0);
        }

        private static async Task<TResult> RunWithTimeout<T1, TResult>(this Func<T1, TResult> function, T1 arg1, CancellationToken token, int milliseconds, int maxAttempts, bool strict, int attempt)
        {
            token.ThrowIfCancellationRequested();

            var timeoutSource = new CancellationTokenSource(milliseconds);
            var combinedToken = CancellationTokenSource.CreateLinkedTokenSource(token, timeoutSource.Token);

            TResult result = default;
            try
            {
                result = await Task.Run(() => function(arg1), combinedToken.Token);
            }
            catch (TaskCanceledException)
            {
                token.ThrowIfCancellationRequested();

                if (attempt < maxAttempts)
                {
                    result = await function.RunWithTimeout(arg1, token, milliseconds, maxAttempts, strict, attempt + 1);
                }
                else
                {
                    logger.Warn($"Task timed out after {attempt} attempts.");
                }
            }
            catch (System.Exception e)
            {
                logger.Error(e);
                if (strict) { throw e; }
            }

            return result;
        }

        public static async Task<TResult> RunWithTimeout<T1, TResult>(this Func<T1, CancellationToken, Task<TResult>> task, T1 arg1, CancellationToken token, int milliseconds, int maxAttempts, bool strict)
        {
            return await task.RunWithTimeout(arg1, token, milliseconds, maxAttempts, strict, 0);
        }

        private static async Task<TResult> RunWithTimeout<T1, TResult>(this Func<T1, CancellationToken, Task<TResult>> task, T1 arg1, CancellationToken cancel, int milliseconds, int maxAttempts, bool strict, int attempt)
        {
            cancel.ThrowIfCancellationRequested();

            var timeoutSource = new CancellationTokenSource(milliseconds);
            var combinedToken = CancellationTokenSource.CreateLinkedTokenSource(cancel, timeoutSource.Token);

            TResult result = default;

            try
            {
                result = await task(arg1, combinedToken.Token);
            }

            catch (TaskCanceledException)
            {
                cancel.ThrowIfCancellationRequested();
            }

            catch (TimeoutException)
            {
                if (attempt < maxAttempts)
                {
                    result = await task.RunWithTimeout(arg1, cancel, milliseconds, maxAttempts, strict, attempt + 1);
                }
                else
                {
                    logger.Warn($"Task timed out after {attempt} attempts.");
                }
            }

            catch (System.Exception e)
            {
                logger.Error(e);
                if (strict) { throw; }
            }

            return result;
        }


        #endregion RunWithTimeout<T1, TResult>

        #region RunWithTimeout<T1, T2, TResult>

        public static async Task<TResult> RunWithTimeout<T1, T2, TResult>(this Func<T1, T2, TResult> function, T1 arg1, T2 arg2, CancellationToken token, int milliseconds, int maxAttempts, bool strict)
        {
            return await function.RunWithTimeout(arg1, arg2, token, milliseconds, maxAttempts, strict, 0);
        }

        private static async Task<TResult> RunWithTimeout<T1, T2, TResult>(this Func<T1, T2, TResult> function, T1 arg1, T2 arg2, CancellationToken token, int milliseconds, int maxAttempts, bool strict, int attempt)
        {
            token.ThrowIfCancellationRequested();

            var timeoutSource = new CancellationTokenSource(milliseconds);
            var combinedToken = CancellationTokenSource.CreateLinkedTokenSource(token, timeoutSource.Token);

            TResult result = default(TResult);
            try
            {
                result = await Task.Run(() => function(arg1, arg2), combinedToken.Token);
            }
            catch (TaskCanceledException)
            {
                token.ThrowIfCancellationRequested();

                if (attempt < maxAttempts)
                {
                    result = await function.RunWithTimeout(arg1, arg2, token, milliseconds, maxAttempts, strict, attempt + 1);
                }
                else
                {
                    logger.Warn($"Task timed out after {attempt} attempts.");
                }
            }
            catch (System.Exception e)
            {
                logger.Error(e);
                if (strict) { throw e; }
            }

            return result;
        }

        public static async Task<TResult> RunWithTimeout<T1, T2, TResult>(this Func<T1, T2, CancellationToken, Task<TResult>> task, T1 arg1, T2 arg2, CancellationToken cancel, int milliseconds, int maxAttempts, bool strict)
        {
            return await task.RunWithTimeout(arg1, arg2, cancel, milliseconds, maxAttempts, strict, 0);
        }

        private static async Task<TResult> RunWithTimeout<T1, T2, TResult>(this Func<T1, T2, CancellationToken, Task<TResult>> task, T1 arg1, T2 arg2, CancellationToken cancel, int milliseconds, int maxAttempts, bool strict, int attempt)
        {
            cancel.ThrowIfCancellationRequested();

            var timeoutSource = new CancellationTokenSource(milliseconds);
            var combinedToken = CancellationTokenSource.CreateLinkedTokenSource(cancel, timeoutSource.Token);

            TResult result = default(TResult);
            try
            {
                result = await task(arg1, arg2, cancel);
            }
            catch (TaskCanceledException)
            {
                cancel.ThrowIfCancellationRequested();

                if (attempt < maxAttempts)
                {
                    result = await task.RunWithTimeout(arg1, arg2, cancel, milliseconds, maxAttempts, strict, attempt + 1);
                }
                else
                {
                    logger.Warn($"Task timed out after {attempt} attempts.");
                }
            }
            catch (System.Exception e)
            {
                logger.Error(e);
                if (strict) { throw e; }
            }

            return result;
        }

        public static async Task RunWithTimeout<T1, T2>(this Func<T1, T2, CancellationToken, Task> task, T1 arg1, T2 arg2, CancellationToken cancel, int milliseconds, int maxAttempts, bool strict)
        {
            await task.RunWithTimeout(arg1, arg2, cancel, milliseconds, maxAttempts, strict, 0);
        }

        private static async Task RunWithTimeout<T1, T2>(this Func<T1, T2, CancellationToken, Task> task, T1 arg1, T2 arg2, CancellationToken cancel, int milliseconds, int maxAttempts, bool strict, int attempt)
        {
            cancel.ThrowIfCancellationRequested();

            var timeoutSource = new CancellationTokenSource(milliseconds);
            var combinedToken = CancellationTokenSource.CreateLinkedTokenSource(cancel, timeoutSource.Token);

            try
            {
                await task(arg1, arg2, cancel);
            }
            catch (TaskCanceledException)
            {
                cancel.ThrowIfCancellationRequested();

                if (attempt < maxAttempts)
                {
                    await task.RunWithTimeout(arg1, arg2, cancel, milliseconds, maxAttempts, strict, attempt + 1);
                }
                else
                {
                    logger.Warn($"Task timed out after {attempt} attempts.");
                }
            }
            catch (System.Exception e)
            {
                logger.Error(e);
                if (strict) { throw e; }
            }

        }

        #endregion RunWithTimeout<T1, T2, TResult>

        #region RunWithTimeout<T1, T2, T3, TResult>

        public static async Task<TResult> RunWithTimeout<T1, T2, T3, TResult>(this Func<T1, T2, T3, TResult> function, T1 arg1, T2 arg2, T3 arg3, CancellationToken token, int milliseconds, int maxAttempts, bool strict)
        {
            return await function.RunWithTimeout(arg1, arg2, arg3, token, milliseconds, maxAttempts, strict, 0);
        }

        private static async Task<TResult> RunWithTimeout<T1, T2, T3, TResult>(this Func<T1, T2, T3, TResult> function, T1 arg1, T2 arg2, T3 arg3, CancellationToken token, int milliseconds, int maxAttempts, bool strict, int attempt)
        {
            token.ThrowIfCancellationRequested();

            var timeoutSource = new CancellationTokenSource(milliseconds);
            var combinedToken = CancellationTokenSource.CreateLinkedTokenSource(token, timeoutSource.Token);

            TResult result = default;
            try
            {
                result = await Task.Run(() => function(arg1, arg2, arg3), combinedToken.Token);
            }
            catch (TaskCanceledException)
            {
                token.ThrowIfCancellationRequested();

                if (attempt < maxAttempts)
                {
                    result = await function.RunWithTimeout(arg1, arg2, arg3, token, milliseconds, maxAttempts, strict, attempt + 1);
                }
                else
                {
                    logger.Warn($"Task timed out after {attempt} attempts.");
                }
            }
            catch (System.Exception e)
            {
                logger.Error(e);
                if (strict) { throw e; }
            }

            return result;
        }

        public static async Task<TResult> RunWithTimeout<T1, T2, T3, TResult>(this Func<T1, T2, T3, CancellationToken, Task<TResult>> task, T1 arg1, T2 arg2, T3 arg3, CancellationToken cancel, int milliseconds, int maxAttempts, bool strict)
        {
            return await task.RunWithTimeout(arg1, arg2, arg3, cancel, milliseconds, maxAttempts, strict, 0);
        }

        private static async Task<TResult> RunWithTimeout<T1, T2, T3, TResult>(this Func<T1, T2, T3, CancellationToken, Task<TResult>> task, T1 arg1, T2 arg2, T3 arg3, CancellationToken cancel, int milliseconds, int maxAttempts, bool strict, int attempt)
        {
            cancel.ThrowIfCancellationRequested();

            var timeoutSource = new CancellationTokenSource(milliseconds);
            var combinedToken = CancellationTokenSource.CreateLinkedTokenSource(cancel, timeoutSource.Token);

            TResult result = default;
            try
            {
                result = await task(arg1, arg2, arg3, cancel);
            }
            catch (TaskCanceledException)
            {
                cancel.ThrowIfCancellationRequested();

                if (attempt < maxAttempts)
                {
                    result = await task.RunWithTimeout(arg1, arg2, arg3, cancel, milliseconds, maxAttempts, strict, attempt + 1);
                }
                else
                {
                    logger.Warn($"Task timed out after {attempt} attempts.");
                }
            }
            catch (System.Exception e)
            {
                logger.Error(e);
                if (strict) { throw e; }
            }

            return result;
        }

        public static async Task RunWithTimeout<T1, T2, T3>(this Func<T1, T2, T3, CancellationToken, Task> task, T1 arg1, T2 arg2, T3 arg3, CancellationToken cancel, int milliseconds, int maxAttempts, bool strict)
        {
            await task.RunWithTimeout(arg1, arg2, arg3, cancel, milliseconds, maxAttempts, strict, 0);
        }

        private static async Task RunWithTimeout<T1, T2, T3>(this Func<T1, T2, T3, CancellationToken, Task> task, T1 arg1, T2 arg2, T3 arg3, CancellationToken cancel, int milliseconds, int maxAttempts, bool strict, int attempt)
        {
            cancel.ThrowIfCancellationRequested();

            var timeoutSource = new CancellationTokenSource(milliseconds);
            var combinedToken = CancellationTokenSource.CreateLinkedTokenSource(cancel, timeoutSource.Token);

            try
            {
                await task(arg1, arg2, arg3, cancel);
            }
            catch (TaskCanceledException)
            {
                cancel.ThrowIfCancellationRequested();

                if (attempt < maxAttempts)
                {
                    await task.RunWithTimeout(arg1, arg2, arg3, cancel, milliseconds, maxAttempts, strict, attempt + 1);
                }
                else
                {
                    logger.Warn($"Task timed out after {attempt} attempts.");
                }
            }
            catch (System.Exception e)
            {
                logger.Error(e);
                if (strict) { throw e; }
            }
        }

        #endregion RunWithTimeout<T1, T2, T3, TResult>


        internal static void MarshalTaskResults<TResult>(
            Task source, TaskCompletionSource<TResult> proxy)
        {
            switch (source.Status)
            {
                case TaskStatus.Faulted:
                    proxy.TrySetException(source.Exception);
                    break;
                case TaskStatus.Canceled:
                    proxy.TrySetCanceled();
                    break;
                case TaskStatus.RanToCompletion:
                    Task<TResult> castedSource = source as Task<TResult>;
                    proxy.TrySetResult(
                        castedSource == null ? default(TResult) : // source is a Task
                            castedSource.Result); // source is a Task<TResult>
                    break;
            }
        }

        internal struct VoidTypeStruct { }  // See Footnote #1

        public static Task<TResult> TimeoutAfter<TResult>(this Task<TResult> task, int millisecondsTimeout, int repeatAttempts)
        {
            Task<TResult> result = default;

            try
            {
                result = task.TimeoutAfter(millisecondsTimeout);
            }
            catch (TimeoutException)
            {
                logger.Warn($"Task timed out. {repeatAttempts} attempts remaining.");
                if (repeatAttempts > 0)
                {
                    result = task.TimeoutAfter(millisecondsTimeout, repeatAttempts - 1);
                }
                else
                {
                    logger.Warn($"Task timed out after {repeatAttempts} attempts.");
                }
            }
            return result;
        }

        /// <summary>
        /// https://devblogs.microsoft.com/pfxteam/crafting-a-task-timeoutafter-method/
        /// </summary>
        /// <param name="task"></param>
        /// <param name="millisecondsTimeout"></param>
        /// <returns></returns>
        public static Task<TResult> TimeoutAfter<TResult>(this Task<TResult> task, int millisecondsTimeout)
        {
            // Short-circuit #1: infinite timeout or task already completed
            if (task.IsCompleted || (millisecondsTimeout == Timeout.Infinite))
            {
                // Either the task has already completed or timeout will never occur.
                // No proxy necessary.
                return task;
            }

            // tcs.Task will be returned as a proxy to the caller
            TaskCompletionSource<TResult> tcs =
                new TaskCompletionSource<TResult>();

            // Short-circuit #2: zero timeout
            if (millisecondsTimeout == 0)
            {
                // We've already timed out.
                tcs.SetException(new TimeoutException());
                return tcs.Task;
            }

            // Set up a timer to complete after the specified timeout period
            Timer timer = new Timer(state =>
            {
                // Recover your state information
                var myTcs = (TaskCompletionSource<TResult>)state;

                // Fault our proxy with a TimeoutException
                myTcs.TrySetException(new TimeoutException());
            }, tcs, millisecondsTimeout, Timeout.Infinite);

            // Wire up the logic for what happens when source task completes
            task.ContinueWith((antecedent, state) =>
            {
                // Recover our state data
                var tuple =
                    (Tuple<Timer, TaskCompletionSource<TResult>>)state;

                // Cancel the Timer
                tuple.Item1.Dispose();

                // Marshal results to proxy
                MarshalTaskResults(antecedent, tuple.Item2);
            },
            Tuple.Create(timer, tcs),
            CancellationToken.None,
            TaskContinuationOptions.ExecuteSynchronously,
            TaskScheduler.Default);

            return tcs.Task;
        }

        public static Task TimeoutAfter(this Task task, int millisecondsTimeout, int repeatAttempts)
        {
            Task result = default;

            try
            {
                result = task.TimeoutAfter(millisecondsTimeout);
            }
            catch (TimeoutException)
            {
                if (repeatAttempts > 0)
                {
                    result = task.TimeoutAfter(millisecondsTimeout, repeatAttempts - 1);
                }
            }
            return result;
        }

        public static Task TimeoutAfter(this Task task, int millisecondsTimeout)
        {
            // Short-circuit #1: infinite timeout or task already completed
            if (task.IsCompleted || (millisecondsTimeout == Timeout.Infinite))
            {
                // Either the task has already completed or timeout will never occur.
                // No proxy necessary.
                return task;
            }

            // tcs.Task will be returned as a proxy to the caller
            TaskCompletionSource<VoidTypeStruct> tcs =
                new TaskCompletionSource<VoidTypeStruct>();

            // Short-circuit #2: zero timeout
            if (millisecondsTimeout == 0)
            {
                // We've already timed out.
                tcs.SetException(new TimeoutException());
                return tcs.Task;
            }

            // Set up a timer to complete after the specified timeout period
            Timer timer = new Timer(state =>
            {
                // Recover your state information
                var myTcs = (TaskCompletionSource<VoidTypeStruct>)state;

                // Fault our proxy with a TimeoutException
                myTcs.TrySetException(new TimeoutException());
            }, tcs, millisecondsTimeout, Timeout.Infinite);

            // Wire up the logic for what happens when source task completes
            task.ContinueWith((antecedent, state) =>
            {
                // Recover our state data
                var tuple =
                    (Tuple<Timer, TaskCompletionSource<VoidTypeStruct>>)state;

                // Cancel the Timer
                tuple.Item1.Dispose();

                // Marshal results to proxy
                MarshalTaskResults(antecedent, tuple.Item2);
            },
            Tuple.Create(timer, tcs),
            CancellationToken.None,
            TaskContinuationOptions.ExecuteSynchronously,
            TaskScheduler.Default);

            return tcs.Task;
        }

        

    }

}    
    

