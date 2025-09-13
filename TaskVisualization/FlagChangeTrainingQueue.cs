using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UtilitiesCS.HelperClasses;
using UtilitiesCS.Threading;
using UtilitiesCS.Interfaces;

namespace TaskVisualization
{
    public class FlagChangeTrainingQueue : IFlagChangeTrainingQueue
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public FlagChangeTrainingQueue() { }

        public IFlagChangeTrainingQueue Init()
        {
            ConsumerTimer = new TimedAsyncTask(new TimeSpan(0, 0, 0, 0, 500), ConsumeAsync);
            return this;
        }

        
        public IFlagChangeTrainingQueue.QueueOptions Options { get; set; } = IFlagChangeTrainingQueue.QueueOptions.Timed;
        internal CancellationToken Cancel { get; private set; } = default;
        internal BlockingCollection<IFlagChangeGroup> Queue { get; private set; } = [];
        private ThreadSafeSingleShotGuard _guard = new();

        internal Task Consumer { get; private set; } = Task.CompletedTask;
        internal TimedAsyncTask ConsumerTimer { get; private set; }

        internal async Task ConsumeAsync()
        {
            await Task.Run(async () =>
            {
                while (Queue.TryTake(out var item))
                {
                    try
                    {
                        await item.ProcessGroupAsync();
                    }
                    catch (Exception e)
                    {
                        logger.Error($"Error training flags for email with subject: {(item as FlagChangeGroup)?.Subject}. {e.Message}", e);
                    }

                }
                _guard = new ThreadSafeSingleShotGuard();
            }, Cancel);
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
