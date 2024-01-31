using Microsoft.Office.Interop.Outlook;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UtilitiesCS.HelperClasses;

namespace UtilitiesCS.Threading
{
    public static class AsyncMultiTasker
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static async Task<ConcurrentBag<TOut>> AsyncMultiTaskChunker<T, TOut>(
            IEnumerable<T> obj, 
            Func<T, Task<TOut>> func, 
            IProgress<(int Value, string JobName)> progress,
            string messagePrefix,
            CancellationToken cancel) 
        {
            int count = obj.Count();
            int complete = 0;

            int chunkNum = Environment.ProcessorCount - 1;
            int chunkSize = count / chunkNum;
            
            var chunks = obj.Chunk(chunkSize);
            var result = new ConcurrentBag<TOut>();
            List<Task> tasks = [];
            var sw = Stopwatch.StartNew();

            foreach (var chunk in chunks)
            {
                tasks.Add(Task.Run(async () => 
                {
                    foreach (var item in chunk)
                    {
                        try
                        {
                            result.Add(await func(item));
                            Interlocked.Increment(ref complete);
                        }
                        catch (OperationCanceledException)
                        {
                            logger.Debug("Request to cancel task was received");
                            break;
                        }
                        catch (System.Exception e)
                        {
                            logger.Debug($"Skipping {typeof(T)} {item} due to exception: {e.Message}");
                        }
                    }
                },
                cancel));
            }

            var timer = new TimerWrapper(TimeSpan.FromSeconds(1));
            timer.Elapsed += (sender, e) =>
            {
                if (count > 0)
                {
                    progress.Report(
                        ((int)(((double)complete / count) * 100),
                        GetReportMessage(messagePrefix, complete, count, sw)));
                }
            };
            timer.AutoReset = true;

            try
            {
                timer.StartTimer();
                await Task.WhenAll(tasks);
                return result;
            }
            catch (TaskCanceledException)
            {
                logger.Debug("Request to cancel task was received");
                timer.StopTimer();
                timer.Dispose();
                throw;
            }
            catch (System.Exception e)
            {
                logger.Error($"{e.Message}", e);
                timer.StopTimer();
                timer.Dispose();
                throw;
            }
            finally
            {
                timer.StopTimer();
                timer.Dispose();
            }            
        }

        public static async Task AsyncMultiTaskChunker<T>(
            IEnumerable<T> obj,
            Func<T, Task> func,
            IProgress<(int Value, string JobName)> progress,
            string messagePrefix,
            CancellationToken cancel)
        {
            int count = obj.Count();
            int complete = 0;

            int chunkNum = Environment.ProcessorCount - 2;
            int chunkSize = count / chunkNum;

            var chunks = obj.Chunk(chunkSize).ToArray();

            List<Task> tasks = [];
            var sw = Stopwatch.StartNew();

            foreach (var chunk in chunks)
            {
                tasks.Add(Task.Run(async () =>
                {
                    foreach (var item in chunk)
                    {
                        try
                        {
                            await func(item);
                            Interlocked.Increment(ref complete);
                        }
                        catch (OperationCanceledException)
                        {
                            logger.Debug("Request to cancel task was received");
                            break;
                        }
                        catch (System.Exception e)
                        {
                            logger.Debug($"Skipping {typeof(T)} {item} due to exception: {e.Message}");
                        }
                    }
                },
                cancel));
            }

            var timer = new TimerWrapper(TimeSpan.FromSeconds(1));
            timer.Elapsed += (sender, e) =>
            {
                if (count > 0)
                {
                    progress.Report(
                        ((int)(((double)complete / count) * 100),
                        GetReportMessage(messagePrefix, complete, count, sw)));
                }
            };
            timer.AutoReset = true;

            var tasksComplete = Task.WhenAll(tasks);

            try
            {
                timer.StartTimer();
                await Task.WhenAll(tasks);
            }
            catch (TaskCanceledException)
            {
                logger.Debug("Request to cancel task was received");
            }
            catch (System.Exception e)
            {                
                logger.Error($"{e.Message}", e);
                timer.StopTimer();
                timer.Dispose();
                throw;
            }
            finally
            {
                timer.StopTimer();
                timer.Dispose();
            }
        }


        public static async Task<ConcurrentBag<TOut>> AsyncMultiTaskChunker<T, TOut>(
            IEnumerable<T> obj,
            Func<T, TOut> func,
            IProgress<(int Value, string JobName)> progress,
            string messagePrefix,
            CancellationToken cancel)
        {
            int count = obj.Count();
            int complete = 0;

            int chunkNum = Environment.ProcessorCount - 1;
            int chunkSize = count / chunkNum;

            var chunks = obj.Chunk(chunkSize);
            var result = new ConcurrentBag<TOut>();
            List<Task> tasks = [];
            var sw = Stopwatch.StartNew();

            foreach (var chunk in chunks)
            {
                tasks.Add(Task.Run(() =>
                {
                    foreach (var item in chunk)
                    {
                        try
                        {
                            result.Add(func(item));
                            Interlocked.Increment(ref complete);
                        }
                        catch (OperationCanceledException)
                        {
                            logger.Debug("Request to cancel task was received");
                            break;
                        }
                        catch (System.Exception e)
                        {
                            logger.Debug($"Skipping {typeof(T)} {item} due to exception: {e.Message}");
                        }
                    }
                },
                cancel));
            }

            var timer = new TimerWrapper(TimeSpan.FromSeconds(1));
            timer.Elapsed += (sender, e) =>
            {
                if (count > 0)
                {
                    progress.Report(
                        ((int)(((double)complete / count) * 100),
                        GetReportMessage(messagePrefix, complete, count, sw)));
                }
            };
            timer.AutoReset = true;

            try
            {
                timer.StartTimer();
                await Task.WhenAll(tasks);
                return result;
            }
            catch (TaskCanceledException)
            {
                logger.Debug("Request to cancel task was received");
                timer.StopTimer();
                timer.Dispose();
                throw;
            }
            catch (System.Exception e)
            {
                logger.Error($"{e.Message}", e);
                timer.StopTimer();
                timer.Dispose();
                throw;
            }
            finally
            {
                timer.StopTimer();
                timer.Dispose();
            }
        }

        public static async Task AsyncMultiTaskChunker<T>(
            IEnumerable<T> obj,
            Action<T> action,
            IProgress<(int Value, string JobName)> progress,
            string messagePrefix,
            CancellationToken cancel)
        {
            int count = obj.Count();
            int complete = 0;

            int chunkNum = Environment.ProcessorCount - 1;
            int chunkSize = count / chunkNum;

            var chunks = obj.Chunk(chunkSize);

            List<Task> tasks = [];
            var sw = Stopwatch.StartNew();

            foreach (var chunk in chunks)
            {
                tasks.Add(Task.Run(() =>
                {
                    foreach (var item in chunk)
                    {
                        try
                        {
                            action(item);
                            Interlocked.Increment(ref complete);
                        }
                        catch (OperationCanceledException)
                        {
                            logger.Debug("Request to cancel task was received");
                            break;
                        }
                        catch (System.Exception e)
                        {
                            logger.Debug($"Skipping {typeof(T)} {item} due to exception: {e.Message}");
                        }
                    }
                },
                cancel));
            }

            var timer = new TimerWrapper(TimeSpan.FromSeconds(1));
            timer.Elapsed += (sender, e) =>
            {
                if (count > 0)
                {
                    progress.Report(
                        ((int)(((double)complete / count) * 100),
                        GetReportMessage(messagePrefix, complete, count, sw)));
                }
            };
            timer.AutoReset = true;

            try
            {
                timer.StartTimer();
                await Task.WhenAll(tasks);
            }
            catch (TaskCanceledException)
            {
                logger.Debug("Request to cancel task was received");
            }
            catch (System.Exception e)
            {
                logger.Error($"{e.Message}", e);
                timer.StopTimer();
                timer.Dispose();
                throw;
            }
            finally
            {
                timer.StopTimer();
                timer.Dispose();
            }
        }

        private static string GetReportMessage(string messagePrefix, int complete, int count, Stopwatch sw)
        {
            double seconds = complete > 0 ? sw.Elapsed.TotalSeconds / complete : 0;
            var remaining = count - complete;
            var remainingSeconds = remaining * seconds;
            var ts = TimeSpan.FromSeconds(remainingSeconds);
            string msg = $"{messagePrefix} Completed {complete} of {count} ({seconds:N2} spm) " +
                $"({sw.Elapsed:%m\\:ss} elapsed {ts:%m\\:ss} remaining)";
            return msg;
        }

        #region Alt Timer Code

        //var reportTask = Task.Run(() =>
        //{
        //    new Thread(() => 
        //    {
        //        Thread.CurrentThread.IsBackground = true;
        //        while (!tasksComplete.IsCompleted)
        //        {
        //            if (count > 0)
        //            {
        //                progress.Report(
        //                    ((int)(((double)complete / count) * 100),
        //                    GetReportMessage(messagePrefix, complete, count, sw)));
        //            }
        //            Thread.Sleep(1000);
        //        }
        //    }).Start();
        //}, cancel);

        //var reportTask = Task.Run(async () =>
        //{ 
        //    while (!tasksComplete.IsCompleted)
        //    {
        //        if (count > 0)
        //        {
        //            progress.Report(
        //                ((int)(((double)complete / count) * 100),
        //                GetReportMessage(messagePrefix, complete, count, sw)));
        //        }
        //        await Task.Delay(1000);
        //    }
        //}, cancel);
        //var tasks2 = new List<Task> { tasksComplete, reportTask };

        #endregion Alt Timer Code
    }
}
