using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UtilitiesCS.HelperClasses;
using UtilitiesCS.Interfaces;
using UtilitiesCS.Threading;

namespace TaskVisualization
{
    public class FlagChangeTrainingQueue : IFlagChangeTrainingQueue
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType
        );

        public FlagChangeTrainingQueue() { }

        public IFlagChangeTrainingQueue Init()
        {
            ConsumerTimer = new TimedAsyncTask(new TimeSpan(0, 0, 0, 0, 500), ConsumeAsync);
            return this;
        }

        public IFlagChangeTrainingQueue.QueueOptions Options { get; set; } =
            IFlagChangeTrainingQueue.QueueOptions.Timed;
        internal CancellationToken Cancel { get; private set; } = default;
        internal BlockingCollection<IFlagChangeGroup> Queue { get; private set; } = [];
        private ThreadSafeSingleShotGuard _guard = new();

        internal Task Consumer { get; private set; } = Task.CompletedTask;
        internal TimedAsyncTask ConsumerTimer { get; private set; }

        internal async Task ConsumeAsync()
        {
            await Task.Run(
                async () =>
                {
                    // Issue #726 finding 1: the identical handshake window that motivated the
                    // FilerQueue fix exists here too. The guard reset was the loop's last statement,
                    // unprotected by try/finally, so any exception escaping the loop -- including one
                    // from the catch handler's own diagnostic expression -- would leave _guard
                    // permanently in its already-fired state and stop Immediate-mode consumption from
                    // ever restarting.
                    try
                    {
                        while (Queue.TryTake(out var item))
                        {
                            try
                            {
                                await item.ProcessGroupAsync();
                            }
                            catch (Exception e)
                            {
                                logger.Error(
                                    $"Error training flags for email with subject: {(item as FlagChangeGroup)?.Subject}. {e.Message}",
                                    e
                                );
                            }
                        }
                    }
                    finally
                    {
                        _guard = new ThreadSafeSingleShotGuard();
                    }
                },
                Cancel
            );
        }

        public void Enqueue(IFlagChangeGroup item)
        {
            Queue.Add(item);
            if (Options == IFlagChangeTrainingQueue.QueueOptions.Immediate)
            {
                if (_guard.CheckAndSetFirstCall)
                {
                    Consumer = ConsumeAsync();
                }
            }
            else
            {
                ConsumerTimer?.RequestOrResetTask();
            }
        }
    }
}
