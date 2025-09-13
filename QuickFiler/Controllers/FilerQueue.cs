using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilitiesCS;
using UtilitiesCS.EmailIntelligence.EmailParsingSorting;
using UtilitiesCS.Extensions;
using UtilitiesCS.Threading;

namespace QuickFiler.Controllers
{
    public class FilerQueue
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        internal BlockingCollection<FilerQueueItem> Queue { get; private set; } = [];

        public void Enqueue(FilerQueueItem item)
        {
            Queue.Add(item);
            if (guard.CheckAndSetFirstCall)
            {
                Consumer = ConsumeAsync();
            }
        }

        public void Enqueue(EmailFiler filer, IList<MailItemHelper> helpers)
        {
            Queue.Add(new FilerQueueItem(filer, helpers));
            if (guard.CheckAndSetFirstCall)
            {
                Consumer = ConsumeAsync();
            }
        }

        ThreadSafeSingleShotGuard guard = new ThreadSafeSingleShotGuard();

        public Task Consumer { get; private set; } = Task.CompletedTask;

        public async Task ConsumeAsync() 
        {
            await Task.Run(async () => 
            {
                while (Queue.TryTake(out var item))
                {
                    try
                    {
                        await item.Filer.SortAsync(item.Helpers);
                    }
                    catch (Exception e)
                    {
                        var first = item.Helpers.First();
                        logger.Error($"Error sorting mail items Subject: {first.Subject} Sent On: {first.SentOn} from {first.SenderName} {e.Message}",e);
                    }
                    
                }
                guard = new ThreadSafeSingleShotGuard();
            });
        }

    }

    public class FilerQueueItem
    {
        public FilerQueueItem(EmailFiler filer, IList<MailItemHelper> helpers)
        {
            Filer = filer.ThrowIfNull();
            Helpers = helpers.ThrowIfNull();
            if (helpers.Any(h => h is null))
            {
                throw new ArgumentNullException("Helpers cannot contain null values");
            }
        }
        public EmailFiler Filer { get; private set; }
        public IList<MailItemHelper> Helpers { get; private set; }
    }
}
