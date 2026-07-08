using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using FluentAssertions;
using log4net;
using log4net.Appender;
using log4net.Repository.Hierarchy;
using Microsoft.Office.Interop.Outlook;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using TaskMaster.Properties;
using UtilitiesCS;
using UtilitiesCS.ReusableTypeClasses;

namespace TaskMaster.Test.AppGlobals
{
    [TestClass]
    public partial class AppEventsTests
    {
        private bool _originalEventsHooked;

        [TestInitialize]
        public void TestInitialize()
        {
            _originalEventsHooked = Settings.Default.EventsHooked;
            Settings.Default.EventsHooked = false;
        }

        [TestCleanup]
        public void TestCleanup()
        {
            Settings.Default.EventsHooked = _originalEventsHooked;
        }

        [TestMethod]
        public async Task LoadAsync_EmitsStartupInboxTimingEnvelopeBeforeDeferredProcessingWindow()
        {
            var globals = CreateGlobalsWithNoEngines();
            var sut = new AppEvents(globals.Object);
            var appender = AttachMemoryAppender(typeof(AppEvents));

            try
            {
                await sut.LoadAsync();

                var messages = appender
                    .GetEvents()
                    .Select(loggingEvent => loggingEvent.RenderedMessage)
                    .ToArray();

                FindMessageIndex(messages, "LoadAsync start | startup-active status")
                    .Should()
                    .BeLessThan(
                        FindMessageIndex(
                            messages,
                            "LoadAsync entering deferred processing window before await ProcessNewInboxItemsAsync()"
                        )
                    );
                FindMessageIndex(
                        messages,
                        "LoadAsync entering deferred processing window before await ProcessNewInboxItemsAsync()"
                    )
                    .Should()
                    .BeLessThan(
                        FindMessageIndex(
                            messages,
                            "ProcessNewInboxItemsAsync start | startup-active status"
                        )
                    );
                FindMessageIndex(
                        messages,
                        "ProcessNewInboxItemsAsync start | startup-active status"
                    )
                    .Should()
                    .BeLessThan(
                        FindMessageIndex(messages, "LoadAsync complete | startup-active status")
                    );
            }
            finally
            {
                DetachMemoryAppender(typeof(AppEvents), appender);
            }
        }

        [TestMethod]
        public async Task ProcessNewInboxItemsAsync_BatchesMailboxProcessingAfterInteractiveCheckpoint()
        {
            var globals = CreateGlobalsWithNoEngines();
            var sut = new AppEvents(globals.Object)
            {
                OlInboxes = BuildInboxSubscriptions(BuildUnprocessedInboxItems(2)),
            };
            var appender = AttachMemoryAppender(typeof(AppEvents));

            try
            {
                await sut.ProcessNewInboxItemsAsync();

                var messages = appender
                    .GetEvents()
                    .Select(loggingEvent => loggingEvent.RenderedMessage)
                    .ToArray();

                FindMessageIndex(messages, "interactive checkpoint")
                    .Should()
                    .BeLessThan(
                        FindMessageIndex(messages, "startup inbox batch start | batch processing")
                    );
                FindMessageIndex(messages, "startup inbox batch start | batch processing")
                    .Should()
                    .BeLessThan(
                        FindMessageIndex(
                            messages,
                            "startup inbox batch complete | batch processing"
                        )
                    );
                messages.Should().Contain(message => message.Contains("batch=1"));
            }
            finally
            {
                DetachMemoryAppender(typeof(AppEvents), appender);
            }
        }

        [TestMethod]
        public async Task ProcessNewInboxItemsAsync_WhenEngineProcessesStartupBatch_LogsSuccessAndDeferredContinuation()
        {
            var globals = CreateGlobalsWithApplicableEngine();
            var processableMail = CreateProcessableMailItem();
            var sut = new AppEvents(globals.Object)
            {
                OlInboxes = BuildInboxSubscriptions(
                    BuildUnprocessedInboxItems(processableMail, 11)
                ),
            };
            var appender = AttachMemoryAppender(typeof(AppEvents));

            try
            {
                await sut.ProcessNewInboxItemsAsync();

                var messages = appender
                    .GetEvents()
                    .Select(loggingEvent => loggingEvent.RenderedMessage)
                    .ToArray();

                messages
                    .Should()
                    .Contain(message => message.Contains("Successfully processed item"));
                messages
                    .Should()
                    .Contain(message =>
                        message.Contains("deferred continuation checkpoint | batch processing")
                    );
                messages.Should().Contain(message => message.Contains("batch=1"));
                messages.Should().Contain(message => message.Contains("batch=2"));
            }
            finally
            {
                DetachMemoryAppender(typeof(AppEvents), appender);
            }
        }

        [TestMethod]
        public async Task LoadAsync_WhenEventsHooked_EmitsStartupHookLifecycleLogs()
        {
            Settings.Default.EventsHooked = true;
            var globals = CreateGlobalsWithHookableOutlookObjects();
            var sut = new AppEvents(globals.Object);
            var appender = AttachMemoryAppender(typeof(AppEvents));

            try
            {
                await sut.LoadAsync();

                var messages = appender
                    .GetEvents()
                    .Select(loggingEvent => loggingEvent.RenderedMessage)
                    .ToArray();

                FindMessageIndex(messages, "LoadAsync startup hook dispatch | startup hook")
                    .Should()
                    .BeLessThan(FindMessageIndex(messages, "Hook start | startup hook"));
                FindMessageIndex(messages, "Hook start | startup hook")
                    .Should()
                    .BeLessThan(
                        FindMessageIndex(messages, "LoadAsync startup hook complete | startup hook")
                    );

                messages
                    .Should()
                    .NotContain(
                        message => message.Contains("Hook complete | startup hook"),
                        "completion is deferred to the coordinator's DispatcherTimer poll, which "
                            + "does not fire on the pump-less MSTest host (covered by "
                            + "HookReadinessCoordinatorTests)"
                    );
                messages
                    .Should()
                    .NotContain(
                        message => message.Contains("ProcessNewInboxItemsAsync start"),
                        "issue #243 requires startup inbox processing to wait until the "
                            + "readiness-hookup path has populated OlInboxes"
                    );

                var hookup = typeof(AppEvents).GetMethod(
                    "PerformReadinessHookup",
                    BindingFlags.NonPublic | BindingFlags.Instance
                );
                Assert.IsNotNull(hookup);
                hookup.Invoke(sut, null);
                messages = appender.GetEvents().Select(e => e.RenderedMessage).ToArray();

                FindMessageIndex(messages, "Hook complete | startup hook")
                    .Should()
                    .BeLessThan(
                        FindMessageIndex(
                            messages,
                            "ProcessNewInboxItemsAsync start | startup-active status"
                        )
                    );
            }
            finally
            {
                DetachMemoryAppender(typeof(AppEvents), appender);
            }
        }

        [TestMethod]
        public void LogStartupTiming_PrefixesPhaseAndIncludesStartupState()
        {
            var method = typeof(AppEvents).GetMethod(
                "LogStartupTiming",
                BindingFlags.NonPublic | BindingFlags.Static
            );
            var appender = AttachMemoryAppender(typeof(AppEvents));
            Assert.IsNotNull(method);

            try
            {
                method.Invoke(
                    null,
                    new object[] { "Hook start | startup hook", true, "detail-segment" }
                );

                appender
                    .GetEvents()
                    .Select(loggingEvent => loggingEvent.RenderedMessage)
                    .Should()
                    .ContainSingle(message =>
                        message.Contains("[Startup timing] Hook start | startup hook")
                        && message.Contains("startup-active=True")
                        && message.Contains("detail-segment")
                    );
            }
            finally
            {
                DetachMemoryAppender(typeof(AppEvents), appender);
            }
        }

        [TestMethod]
        public async Task HandleInboxItemAddAsync_WhenCollaboratorThrows_LogsExceptionAndDoesNotRethrow()
        {
            // Arrange: inject a collaborator that throws the framework fault that terminated Outlook
            // (issue #270). The core handler must contain and log the fault, never rethrow it.
            var globals = CreateGlobalsWithNoEngines();
            var sut = new AppEvents(globals.Object);
            var injected = new System.ArgumentException(
                "The parameter 'sectionGroupName' is invalid."
            );
            sut.InboxItemAddCollaborator = _ => throw injected;
            var appender = AttachMemoryAppender(typeof(AppEvents));

            try
            {
                // Act
                System.Func<Task> act = () => sut.HandleInboxItemAddAsync(new object());

                // Assert: the fault is contained (no rethrow) and logged with the original exception.
                await act.Should().NotThrowAsync();
                appender
                    .GetEvents()
                    .Should()
                    .ContainSingle(loggingEvent =>
                        ReferenceEquals(loggingEvent.ExceptionObject, injected)
                    );
            }
            finally
            {
                DetachMemoryAppender(typeof(AppEvents), appender);
            }
        }

        [TestMethod]
        public async Task HandleToDoItemChangeAsync_WhenCollaboratorThrows_LogsExceptionAndDoesNotRethrow()
        {
            // Arrange: inject a collaborator that throws the framework fault that terminated Outlook
            // (issue #270). The core handler must contain and log the fault, never rethrow it.
            var globals = CreateGlobalsWithNoEngines();
            var sut = new AppEvents(globals.Object);
            var injected = new System.ArgumentException(
                "The parameter 'sectionGroupName' is invalid."
            );
            sut.ToDoItemChangeCollaborator = _ => throw injected;
            var appender = AttachMemoryAppender(typeof(AppEvents));

            try
            {
                // Act
                System.Func<Task> act = () => sut.HandleToDoItemChangeAsync(new object());

                // Assert: the fault is contained (no rethrow) and logged with the original exception.
                await act.Should().NotThrowAsync();
                appender
                    .GetEvents()
                    .Should()
                    .ContainSingle(loggingEvent =>
                        ReferenceEquals(loggingEvent.ExceptionObject, injected)
                    );
            }
            finally
            {
                DetachMemoryAppender(typeof(AppEvents), appender);
            }
        }
    }
}
