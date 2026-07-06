using System.Threading;
using System.Threading.Tasks;
using ToDoModel;

namespace TaskMaster
{
    /// <summary>
    /// Partial of <see cref="AppEvents"/> holding the cohesive, self-contained event-subscription
    /// teardown and item-event handler helpers (<c>Unhook</c>, <c>LogAsync</c>, and the
    /// <c>ItemAdd</c>/<c>ItemChange</c> handlers). These were relocated out of <c>AppEvents.cs</c>
    /// to create file-size headroom for the issue #211 <c>[readiness-hookup]</c> per-step markers
    /// added to <c>PerformReadinessHookup</c>; the relocation is a pure byte-equivalent move with no
    /// logic change. This file intentionally contains no readiness/Hook logic and does not touch
    /// <c>PerformReadinessHookup</c> or the existing <c>[Startup timing]</c> instrumentation.
    /// </summary>
    public partial class AppEvents
    {
        public void Unhook()
        {
            OlToDoItems = null;
            OlReminders = null;
            OlInboxes.Clear(items => items.ItemAdd -= OlInboxItems_ItemAdd);
        }

        internal async Task LogAsync(string message)
        {
            await Task.Run(() => logger.Debug(message));
        }

        private void ProcessStartupInboxItemsAfterReadinessHookup()
        {
            var processingTask = ProcessNewInboxItemsAsync();
            if (processingTask.IsCompleted)
            {
                if (processingTask.IsFaulted)
                {
                    logger.Error(
                        "Startup inbox processing failed after readiness hookup.",
                        processingTask.Exception
                    );
                }

                return;
            }

            _ = processingTask.ContinueWith(
                task =>
                    logger.Error(
                        "Startup inbox processing failed after readiness hookup.",
                        task.Exception
                    ),
                CancellationToken.None,
                TaskContinuationOptions.OnlyOnFaulted,
                TaskScheduler.Default
            );
        }

        private void OlToDoItems_ItemAdd(object item)
        {
            ToDoEvents.OlToDoItems_ItemAdd(item, Globals);
        }

        private async void OlToDoItems_ItemChange(object item)
        {
            try
            {
                await ToDoEvents.OlToDoItems_ItemChange(item, OlToDoItems, Globals);
            }
            catch (System.Exception)
            {
                throw;
            }
        }

        internal async void OlInboxItems_ItemAdd(object item)
        {
            try
            {
                await ProcessMailItemAsync(item);
            }
            catch (System.Exception)
            {
                throw;
            }
        }
    }
}
