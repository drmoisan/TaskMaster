using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using FluentAssertions;
using Microsoft.Office.Interop.Outlook;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using Moq;
using QuickFiler.Controllers;
using QuickFiler.Helper_Classes;
using QuickFiler.Interfaces;
using QuickFiler.Test.TestSupport;
using QuickFiler.Viewers;
using UtilitiesCS;
using UtilitiesCS.EmailIntelligence.Bayesian;
using UtilitiesCS.EmailIntelligence.EmailParsingSorting;
using UtilitiesCS.ReusableTypeClasses.SerializableNew.Concurrent.Observable;
using UtilitiesCS.Threading;

namespace QuickFiler.Controllers.Tests
{
    /// <summary>
    /// #230 shared fixture for the pump-hosted initialization tests. Partial continuation of
    /// <see cref="QfcItemController_InitializationTests"/> (no second [TestClass] attribute); split
    /// out because the combined file exceeds the 500-line repository limit.
    /// </summary>
    public partial class QfcItemController_InitializationTests
    {
        // ------------------------- #230 pump-hosted initialization tests -------------------------

        /// <summary>
        /// Serializes the <c>UiThread.Dispatcher</c> swap across every pump-hosted test in this
        /// assembly. Two separate test classes consume <see cref="BuildPumpHarnessAsync"/> — this
        /// one and <c>QfcItemController_SeamFactoryTests</c> — and MSTest class-level
        /// parallelization runs them concurrently. Without this gate one class's
        /// <c>PumpHarness.Restore</c> can revert the process-wide static to the parked dispatcher
        /// seeded by <c>QfcItemControllerTestSupport.EnsureUiThreadDispatcher</c> while the other
        /// class's member under test is still awaiting a dispatcher operation; the parked
        /// dispatcher never runs a frame, so that await never completes and the test fails on its
        /// <c>[Timeout]</c> instead of on an assertion.
        /// </summary>
        /// <remarks>
        /// The gate is a deterministic completion signal, not a wall-clock wait: <c>WaitAsync</c>
        /// is released by the preceding test's <c>Restore</c>, never by elapsed time.
        /// </remarks>
        private static readonly SemaphoreSlim UiThreadDispatcherGate = new SemaphoreSlim(1, 1);

        /// <summary>
        /// #230: builds a controller wired for a full initialization run against a real
        /// <see cref="QuickFiler.ItemViewer"/> constructed on <paramref name="host"/>'s pump thread.
        /// Every external boundary is a seam: the web-view core initializer is mocked (so no real
        /// WebView2 runtime is ever reached), the UI dispatcher executes inline, the mail item is a
        /// Moq'd COM interface, and no live Outlook object is touched.
        /// </summary>
        internal static async Task<PumpHarness> BuildPumpHarnessAsync(
            WinFormsPumpHost host,
            bool darkMode
        )
        {
            // Held until PumpHarness.Restore, so only one pump fixture owns the static
            // UiThread.Dispatcher at a time across all test classes in this assembly.
            await UiThreadDispatcherGate.WaitAsync().ConfigureAwait(false);
            try
            {
                return await BuildPumpHarnessCoreAsync(host, darkMode).ConfigureAwait(false);
            }
            catch
            {
                UiThreadDispatcherGate.Release();
                throw;
            }
        }

        private static async Task<PumpHarness> BuildPumpHarnessCoreAsync(
            WinFormsPumpHost host,
            bool darkMode
        )
        {
            QuickFiler.ItemViewer viewer = await host.InvokeAsync(() => new QuickFiler.ItemViewer())
                .ConfigureAwait(false);

            // #571: Control.Invoke throws on a handle-less control, and
            // Application.Run(new ApplicationContext()) never creates one, so the viewer can
            // reach the act with no window handle. Reading .Handle is non-recursive, so the two
            // WebView2 children are not dragged into handle creation; CreateControl() would
            // recurse into them because it is Visible-gated over visible children.
            _ = await host.InvokeAsync(() => viewer.Handle).ConfigureAwait(false);

            Mock<IQfcKeyboardHandler> kbd;
            Mock<IQfcExplorerController> explorer;
            CancellationTokenSource cts;
            Mock<IFilerHomeController> home = BuildHomeController(out kbd, out explorer, out cts);
            Mock<IApplicationGlobals> globals = BuildInitGlobals(darkMode);
            Mock<MailItem> mail = BuildInitMailItemMock();
            Mock<IWebViewCoreInitializer> webView = BuildWebViewInitializerMock();

            // Inject the two behavioral seams first, then run the production SaveParameters path so
            // its ??= defaults supply every remaining collaborator (folder-predictor and
            // conversation-resolver factories, mail actions) exactly as production does. Injecting
            // fields one by one instead would leave those factories null and fail inside
            // LoadFolderHandlerAsync rather than at the seam under test.
            HarnessController controller = new HarnessController();
            QfcItemControllerTestSupport.SetField(
                controller,
                "_uiDispatcher",
                QfcItemControllerTestSupport.BuildSyncDispatcher().Object
            );
            QfcItemControllerTestSupport.SetField(
                controller,
                "_webViewInitializer",
                webView.Object
            );
            controller.SaveParameters(
                globals.Object,
                home.Object,
                new Mock<IQfcCollectionController>().Object,
                viewer,
                viewerPosition: 1,
                itemNumberDigits: 2,
                mailItem: mail.Object,
                tlpStates: null
            );

            // QfcTipsDetails.ToggleAsync marshals through the process-wide static
            // UtilitiesCS.UiThread.Dispatcher. In production that is the live UI thread's
            // dispatcher; in this assembly it is either unset or the deliberately parked instance
            // from QfcItemControllerTestSupport.EnsureUiThreadDispatcher, neither of which can
            // complete an InvokeAsync. Point it at the pump thread's dispatcher (serviced by the
            // WinForms loop, proven by WinFormsPumpHostTests.BothMarshalRoutes_*) for the duration
            // of the test, and restore the previous value in PumpHarness.Restore so no state leaks.
            Dispatcher previousUiThreadDispatcher = SwapUiThreadDispatcher(viewer.UiDispatcher);

            return new PumpHarness(controller, viewer, cts, webView, previousUiThreadDispatcher);
        }

        /// <summary>
        /// Replaces the static <c>UiThread._dispatcher</c> backing field and returns the previous
        /// value, mirroring the reflection pattern in
        /// <c>QfcItemControllerTestSupport.EnsureUiThreadDispatcher</c>.
        /// </summary>
        private static Dispatcher SwapUiThreadDispatcher(Dispatcher replacement)
        {
            FieldInfo field = typeof(UiThread).GetField(
                "_dispatcher",
                BindingFlags.NonPublic | BindingFlags.Static
            );
            field.Should().NotBeNull(because: "UiThread._dispatcher backing field must exist");
            Dispatcher previous = (Dispatcher)field.GetValue(null);
            field.SetValue(null, replacement);
            return previous;
        }

        /// <summary>
        /// Globals mock covering every read the initialization members perform:
        /// <c>Ol.DarkMode</c> (theme branch), <c>Ol.EmailPrefixToStrip</c> (MailItemHelper), and
        /// <c>QfSettings</c> (AssignControls).
        /// </summary>
        private static Mock<IApplicationGlobals> BuildInitGlobals(bool darkMode)
        {
            Mock<IAppQuickFilerSettings> settings = new Mock<IAppQuickFilerSettings>();
            settings.SetupGet(s => s.MoveEntireConversation).Returns(false);
            settings.SetupGet(s => s.SaveEmailCopy).Returns(false);
            settings.SetupGet(s => s.SaveAttachments).Returns(false);
            settings.SetupGet(s => s.SavePictures).Returns(false);

            Mock<IOlObjects> ol = new Mock<IOlObjects>();
            ol.SetupGet(o => o.DarkMode).Returns(darkMode);
            ol.SetupGet(o => o.EmailPrefixToStrip).Returns(string.Empty);

            // InitializeAsync additionally drives PopulateFolderComboBoxAsync -> FolderPredictor ->
            // FolderScorer, which reads _globals.AF.CtfMap. An empty CtfMap makes ContainsId false,
            // so the conversation-based suggestion path is exercised and returns no matches without
            // any live Outlook query.
            Mock<IAppAutoFileObjects> autoFile = new Mock<IAppAutoFileObjects>();
            autoFile.SetupGet(a => a.CtfMap).Returns(new CtfMap());
            autoFile.SetupGet(a => a.LngConvCtPwr).Returns(1);

            // The Bayesian suggestion path resolves its predictor through
            // OlFolderClassifierGroup.GetFolderPredictorAsync. Selecting the LCPPN seam
            // (UseLcppnPredictor = true) with an injected IFolderPredictor keeps the whole flat
            // Manager["Folder"] classifier stack out of the test; the predictor returns no
            // predictions so the folder combo box is populated with an empty suggestion set.
            Mock<IFolderPredictor> folderPredictor = new Mock<IFolderPredictor>();
            folderPredictor
                .Setup(p => p.Classify(It.IsAny<string[]>()))
                .Returns(
                    Array
                        .Empty<Prediction<string>>()
                        .AsParallel()
                        .OrderByDescending(prediction => prediction.Probability)
                );
            autoFile.SetupGet(a => a.UseLcppnPredictor).Returns(true);
            autoFile.SetupGet(a => a.FolderPredictor).Returns(folderPredictor.Object);

            // FolderPredictor.FolderArray appends the recents list after the suggestions; an empty
            // list keeps that branch deterministic without any persisted state.
            autoFile.SetupGet(a => a.RecentsList).Returns(new SloLinkedList<string>());

            Mock<IApplicationGlobals> globals = new Mock<IApplicationGlobals>();
            globals.SetupGet(g => g.QfSettings).Returns(settings.Object);
            globals.SetupGet(g => g.Ol).Returns(ol.Object);
            globals.SetupGet(g => g.AF).Returns(autoFile.Object);
            return globals;
        }

        /// <summary>
        /// A <see cref="Mock{MailItem}"/> exposing the members <c>MailItemHelper</c> materializes
        /// during <c>PopulateControlsAsync</c>. <c>MailItem</c> is a mockable COM interface, so no
        /// live Outlook process is required.
        /// </summary>
        private static Mock<MailItem> BuildInitMailItemMock()
        {
            Mock<PropertyAccessor> propertyAccessor = new Mock<PropertyAccessor>();
            Mock<AddressEntry> sender = new Mock<AddressEntry>();
            sender
                .SetupGet(x => x.AddressEntryUserType)
                .Returns(OlAddressEntryUserType.olSmtpAddressEntry);
            sender.SetupGet(x => x.Name).Returns("Ada Sender");
            sender.SetupGet(x => x.Address).Returns("ada@example.com");
            sender.SetupGet(x => x.PropertyAccessor).Returns(propertyAccessor.Object);

            Mock<Recipients> recipients = new Mock<Recipients>();
            recipients.SetupGet(x => x.Count).Returns(0);
            recipients
                .Setup(x => x.GetEnumerator())
                .Returns(() => ((IEnumerable)Array.Empty<Recipient>()).GetEnumerator());

            Mock<Attachments> attachments = new Mock<Attachments>();
            attachments.SetupGet(x => x.Count).Returns(0);
            attachments
                .Setup(x => x.GetEnumerator())
                .Returns(() => ((IEnumerable)Array.Empty<Attachment>()).GetEnumerator());

            Mock<UserProperties> userProperties = new Mock<UserProperties>();
            userProperties
                .Setup(x => x.Find(It.IsAny<string>(), It.IsAny<object>()))
                .Returns((UserProperty)null);

            Mock<MailItem> mailItem = new Mock<MailItem>();
            mailItem.SetupGet(x => x.Subject).Returns("Subject");
            mailItem.SetupGet(x => x.Body).Returns("Body");
            mailItem.SetupGet(x => x.HTMLBody).Returns("<html><body>Body</body></html>");
            mailItem.SetupGet(x => x.SenderName).Returns("Ada Sender");
            mailItem.SetupGet(x => x.SenderEmailAddress).Returns("ada@example.com");
            mailItem.SetupGet(x => x.EntryID).Returns("entry-1");
            mailItem.SetupGet(x => x.Sender).Returns(sender.Object);
            mailItem.SetupGet(x => x.Recipients).Returns(recipients.Object);
            mailItem.SetupGet(x => x.Attachments).Returns(attachments.Object);
            mailItem.SetupGet(x => x.FlagStatus).Returns(OlFlagStatus.olNoFlag);
            mailItem.SetupGet(x => x.SentOn).Returns(new DateTime(2026, 1, 1));
            mailItem.SetupGet(x => x.Categories).Returns(string.Empty);
            mailItem.SetupGet(x => x.UserProperties).Returns(userProperties.Object);
            return mailItem;
        }

        /// <summary>
        /// The web-view seam is always mocked and faults fast at its first call
        /// (<c>CreateEnvironmentAsync</c>) with <see cref="WebViewSentinelException"/>, so
        /// <c>InitializeWebViewAsync</c> can never reach the real CoreWebView2 runtime (an external
        /// process barred by the unit-test policy). The second seam member is stubbed the same way
        /// as a defence in depth.
        /// </summary>
        private static Mock<IWebViewCoreInitializer> BuildWebViewInitializerMock()
        {
            Mock<IWebViewCoreInitializer> webView = new Mock<IWebViewCoreInitializer>();
            webView
                .Setup(w =>
                    w.CreateEnvironmentAsync(
                        It.IsAny<string>(),
                        It.IsAny<CoreWebView2EnvironmentOptions>()
                    )
                )
                .ThrowsAsync(new WebViewSentinelException());
            webView
                .Setup(w =>
                    w.EnsureCoreWebView2Async(
                        It.IsAny<WebView2>(),
                        It.IsAny<CoreWebView2Environment>()
                    )
                )
                .ThrowsAsync(new WebViewSentinelException());
            return webView;
        }

        /// <summary>
        /// Distinguishable marker thrown by the mocked web-view seam so a test can assert that
        /// execution stopped exactly there rather than anywhere else.
        /// </summary>
        internal sealed class WebViewSentinelException : System.Exception
        {
            internal WebViewSentinelException()
                : base("mocked-webview-seam") { }
        }

        /// <summary>
        /// The objects a pump-hosted initialization test needs to arrange, act, and assert.
        /// A plain class (net481 has no <c>IsExternalInit</c>, so no <c>record</c>/<c>init</c>).
        /// </summary>
        internal sealed class PumpHarness
        {
            private readonly Dispatcher _previousUiThreadDispatcher;
            private bool _restored;

            internal PumpHarness(
                HarnessController controller,
                QuickFiler.ItemViewer viewer,
                CancellationTokenSource tokenSource,
                Mock<IWebViewCoreInitializer> webViewInitializer,
                Dispatcher previousUiThreadDispatcher
            )
            {
                Controller = controller;
                Viewer = viewer;
                TokenSource = tokenSource;
                WebViewInitializer = webViewInitializer;
                _previousUiThreadDispatcher = previousUiThreadDispatcher;
            }

            internal HarnessController Controller { get; }

            internal QuickFiler.ItemViewer Viewer { get; }

            internal CancellationTokenSource TokenSource { get; }

            internal Mock<IWebViewCoreInitializer> WebViewInitializer { get; }

            /// <summary>
            /// Restores the static <c>UiThread.Dispatcher</c> captured at arrange time, disposes the
            /// token source, and releases the fixture gate, so the test leaves no process-wide state
            /// behind. Always called from the test's <c>finally</c> block; idempotent, so a second
            /// call cannot over-release the gate.
            /// </summary>
            internal void Restore()
            {
                if (_restored)
                {
                    return;
                }

                _restored = true;
                SwapUiThreadDispatcher(_previousUiThreadDispatcher);
                TokenSource.Dispose();
                UiThreadDispatcherGate.Release();
            }
        }

        /// <summary>
        /// #230: the argument set the static factories need, reusing the collaborators an existing
        /// <see cref="PumpHarness"/> already built. The harness controller itself is not passed to
        /// the factory (the factory constructs its own controller); only its collaborators are.
        /// </summary>
        internal static FactoryArguments BuildFactoryArguments(PumpHarness harness)
        {
            IApplicationGlobals globals = (IApplicationGlobals)
                QfcItemControllerTestSupport.GetField(harness.Controller, "_globals");
            IFilerHomeController home = (IFilerHomeController)
                QfcItemControllerTestSupport.GetField(harness.Controller, "_homeController");
            IUiDispatcher dispatcher = (IUiDispatcher)
                QfcItemControllerTestSupport.GetField(harness.Controller, "_uiDispatcher");
            return new FactoryArguments(
                globals,
                home,
                new Mock<IQfcCollectionController>().Object,
                harness.Controller.Mail,
                harness.TokenSource.Token,
                dispatcher,
                mail => new ConversationResolver(globals, mail)
            );
        }

        /// <summary>
        /// Plain carrier for the static-factory argument set (net481 has no <c>IsExternalInit</c>,
        /// so no <c>record</c>/<c>init</c>).
        /// </summary>
        internal sealed class FactoryArguments
        {
            internal FactoryArguments(
                IApplicationGlobals globals,
                IFilerHomeController homeController,
                IQfcCollectionController parent,
                MailItem mailItem,
                CancellationToken token,
                IUiDispatcher uiDispatcher,
                Func<MailItem, ConversationResolver> conversationResolverFactory
            )
            {
                Globals = globals;
                HomeController = homeController;
                Parent = parent;
                MailItem = mailItem;
                Token = token;
                UiDispatcher = uiDispatcher;
                ConversationResolverFactory = conversationResolverFactory;
            }

            internal IApplicationGlobals Globals { get; }

            internal IFilerHomeController HomeController { get; }

            internal IQfcCollectionController Parent { get; }

            internal MailItem MailItem { get; }

            internal CancellationToken Token { get; }

            internal IUiDispatcher UiDispatcher { get; }

            internal Func<MailItem, ConversationResolver> ConversationResolverFactory { get; }
        }
    }
}
