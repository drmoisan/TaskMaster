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

        // The two collaborator seams use nullable-reference annotations (default-null). This file's
        // project has no project-wide #nullable context, so a narrow annotations-only context is opened
        // to keep the '?' annotation warning-clean under the analyzer build (no CS8632) while remaining
        // correct under the nullable/type-check build.
#nullable enable annotations
        /// <summary>
        /// Optional injectable seam for the inbox <c>ItemAdd</c> processing call. When null (default,
        /// production), <see cref="HandleInboxItemAddAsync"/> awaits <see cref="AppEvents.ProcessMailItemAsync"/>.
        /// A deterministic unit test can assign a throwing delegate to exercise the fault-containment path
        /// without a live Outlook process (issue #270).
        /// </summary>
        internal System.Func<
            object,
            System.Threading.Tasks.Task
        >? InboxItemAddCollaborator { get; set; }

        /// <summary>
        /// Optional injectable seam for the to-do <c>ItemChange</c> processing call. When null (default,
        /// production), <see cref="HandleToDoItemChangeAsync"/> awaits <c>ToDoEvents.OlToDoItems_ItemChange</c>.
        /// A deterministic unit test can assign a throwing delegate to exercise the fault-containment path
        /// without a live Outlook process (issue #270).
        /// </summary>
        internal System.Func<
            object,
            System.Threading.Tasks.Task
        >? ToDoItemChangeCollaborator { get; set; }

#nullable restore annotations

        /// <summary>
        /// Core, host-neutral inbox <c>ItemAdd</c> handler holding the try/catch. Faults are contained here
        /// so the <see cref="OlInboxItems_ItemAdd"/> <c>async void</c> wrapper never reschedules an exception
        /// onto the ThreadPool (which would terminate <c>outlook.exe</c>).
        /// </summary>
        internal async Task HandleInboxItemAddAsync(object item)
        {
            try
            {
                await (InboxItemAddCollaborator ?? (i => ProcessMailItemAsync(i)))(item);
            }
            catch (System.Exception ex)
            {
                logger.Error(
                    "OlInboxItems_ItemAdd handler faulted; contained to prevent process termination.",
                    ex
                );
            }
        }

        /// <summary>
        /// Core, host-neutral to-do <c>ItemChange</c> handler holding the try/catch. Faults are contained
        /// here so the <see cref="OlToDoItems_ItemChange"/> <c>async void</c> wrapper never reschedules an
        /// exception onto the ThreadPool (which would terminate <c>outlook.exe</c>).
        /// </summary>
        internal async Task HandleToDoItemChangeAsync(object item)
        {
            try
            {
                await (
                    ToDoItemChangeCollaborator
                    ?? (i => ToDoEvents.OlToDoItems_ItemChange(i, OlToDoItems, Globals))
                )(item);
            }
            catch (System.Exception ex)
            {
                logger.Error(
                    "OlToDoItems_ItemChange handler faulted; contained to prevent process termination.",
                    ex
                );
            }
        }

        private async void OlToDoItems_ItemChange(object item) =>
            await HandleToDoItemChangeAsync(item);

        internal async void OlInboxItems_ItemAdd(object item) =>
            await HandleInboxItemAddAsync(item);
    }
}
