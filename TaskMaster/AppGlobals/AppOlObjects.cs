using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Office.Interop.Outlook;
using UtilitiesCS;
using UtilitiesCS.OutlookObjects.Folder;
using UtilitiesCS.OutlookObjects.Store;
using UtilitiesCS.ReusableTypeClasses;
using UtilitiesCS.Threading;
using UtilitiesCS.Windows_Forms;
// Microsoft.Office.Interop.Outlook also declares a type named Exception, so an unqualified
// Exception in this file is CS0104-ambiguous. The alias resolves it to the BCL type, matching the
// precedent at UtilitiesCS.Test/OutlookObjects/Table/OlToDoTable_Tests.cs.
using Exception = System.Exception;

namespace TaskMaster
{
    public partial class AppOlObjects : IOlObjects, IDisposable
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType
        );

        public AppOlObjects(Application olApplication, IApplicationGlobals appGlobals)
        {
            _globals = appGlobals;
            _olApplication = olApplication;
            ResetLazyInboxes();
        }

        private IApplicationGlobals _globals;
        internal ISmartSerializableNonTyped SmartSerializable { get; set; } =
            new SmartSerializableNonTyped();

        private Application _olApplication;
        public Application App
        {
            get => _olApplication;
        }

        private string _viewWide;
        public string ViewWide
        {
            get
            {
                if (_viewWide is null)
                    _viewWide = Properties.Settings.Default.View_Wide;
                return _viewWide;
            }
        }

        private string _viewCompact;
        public string ViewCompact
        {
            get
            {
                if (_viewCompact is null)
                    _viewCompact = Properties.Settings.Default.View_Wide;
                return _viewCompact;
            }
        }

        private NameSpace _namespaceMAPI;
        public NameSpace NamespaceMAPI
        {
            get
            {
                if (_namespaceMAPI is null)
                {
                    _namespaceMAPI = App.GetNamespace("MAPI");
                }
                return App.Application.GetNamespace("MAPI");
            }
        }

        private Folder _toDoFolder;
        public Folder ToDoFolder
        {
            get
            {
                if (_toDoFolder is null)
                    _toDoFolder = (Folder)
                        NamespaceMAPI.GetDefaultFolder(OlDefaultFolders.olFolderToDo);
                return _toDoFolder;
            }
        }

        private Lazy<IEnumerable<Folder>> _inboxes;
        public IEnumerable<Folder> Inboxes => _inboxes.Value;

        internal IEnumerable<Folder> LoadInboxes()
        {
            var storesWrapper = StoresWrapper ?? new StoresWrapper() { };
            var stores = NamespaceMAPI.Stores.Cast<Store>();

            // Issue #211 diagnosis-only per-store attribution probe. The included-store set, the
            // inbox-list result, the (Folder)inbox cast, and the COMException rethrow below are all
            // unchanged; the extracted method only adds Stopwatch timing + one emitted line per store.
            var attributionProbe = new StartupInboxAttributionProbe(s => logger.Debug(s));

            var inboxes = new List<Folder>();
            foreach (var store in stores)
            {
                // Per-store body extracted to ResolveInboxForStore (AppOlObjects.StoreRehook.cs) so
                // the bulk startup load and the runtime rehook path share one implementation. The
                // transient-HRESULT rethrow / log-and-skip policy is unchanged and lives inside the
                // primitive, so a transient COMException still propagates out of LoadInboxes.
                var inbox = ResolveInboxForStore(store, storesWrapper, attributionProbe);
                if (inbox is not null)
                {
                    inboxes.Add(inbox);
                }
            }
            return inboxes;
        }

        /// <summary>
        /// Issue #211 diagnosis-only per-store attribution for <see cref="LoadInboxes"/>. Computes the
        /// <c>ShouldIncludeStore</c> timing, the include/exclude result, and (only when included) the
        /// <c>GetDefaultFolder(olFolderInbox)</c> timing, emits one <c>[loadinboxes]</c> line via the
        /// supplied <paramref name="probe"/>, and returns the default-inbox folder to add (or
        /// <see langword="null"/> when the store is excluded or has no inbox folder). The COM and store
        /// boundary is fully expressed through injectable delegates so a fake store can drive this
        /// method without live COM; <see cref="LoadInboxes"/> supplies the real delegates. Behavior is
        /// preserved: an excluded store returns <see langword="null"/> (the caller skips the add, the
        /// byte-equivalent of the original <c>continue</c>), and any exception thrown by a delegate
        /// (for example a transient COMException from <paramref name="getDefaultFolder"/>) propagates
        /// unchanged so the caller's existing <c>catch (COMException)</c> rethrow logic still applies.
        /// </summary>
        /// <param name="shouldInclude">Evaluates <c>StoresWrapper.ShouldIncludeStore(store)</c> for this store.</param>
        /// <param name="getDefaultFolder">
        /// Returns <c>store.GetDefaultFolder(olFolderInbox)</c> (a <see cref="MAPIFolder"/>). Invoked only
        /// when <paramref name="shouldInclude"/> returns <see langword="true"/>.
        /// </param>
        /// <param name="readDisplayName">Guarded read of the store's <c>DisplayName</c> (returns a sentinel when the read throws).</param>
        /// <param name="probe">The coverable attribution formatter/sink.</param>
        /// <returns>The default-inbox <see cref="MAPIFolder"/> to add, or <see langword="null"/> when excluded or absent.</returns>
        internal static MAPIFolder EmitPerStoreInboxAttribution(
            Func<bool> shouldInclude,
            Func<MAPIFolder> getDefaultFolder,
            Func<string> readDisplayName,
            StartupInboxAttributionProbe probe
        )
        {
            var displayName = readDisplayName();

            var shouldIncludeStopwatch = Stopwatch.StartNew();
            var included = shouldInclude();
            shouldIncludeStopwatch.Stop();

            if (!included)
            {
                probe.EmitLoadInboxesStore(
                    displayName,
                    shouldIncludeStopwatch.Elapsed.TotalMilliseconds,
                    included: false,
                    getDefaultFolderMs: null
                );
                return null;
            }

            // why: issue #264. Attribute any UI-thread lockup inside the blocking
            // GetDefaultFolder(Inbox) COM call to this store, using the already-computed displayName
            // (no new COM read). Additive: the [loadinboxes] attribution line and the exclusion path
            // below are unchanged.
            var getDefaultFolderStopwatch = Stopwatch.StartNew();
            MAPIFolder inbox;
            using (CurrentStoreContext.Begin(displayName))
            {
                inbox = getDefaultFolder();
            }
            getDefaultFolderStopwatch.Stop();

            probe.EmitLoadInboxesStore(
                displayName,
                shouldIncludeStopwatch.Elapsed.TotalMilliseconds,
                included: true,
                getDefaultFolderMs: getDefaultFolderStopwatch.Elapsed.TotalMilliseconds
            );

            return inbox;
        }

        internal void ResetLazyInboxes() => _inboxes = new Lazy<IEnumerable<Folder>>(LoadInboxes);

        private Reminders _olReminders;
        public Reminders OlReminders
        {
            get
            {
                if (_olReminders is null)
                    _olReminders = App.Reminders;
                return _olReminders;
            }
        }

        private Folder _root;
        public Folder Root
        {
            get
            {
                if (_root is null)
                    _root = (Folder)App.Session.DefaultStore.GetRootFolder();
                return _root;
            }
        }

        private Folder _inbox;
        public Folder Inbox
        {
            get
            {
                if (_inbox is null)
                    _inbox = (Folder)
                        App.Session.DefaultStore.GetDefaultFolder(OlDefaultFolders.olFolderInbox);
                return _inbox;
            }
        }

        private string _inboxRootPath;
        public string InboxPath
        {
            get
            {
                if (_inboxRootPath is null)
                {
                    _inboxRootPath = Inbox.FolderPath;
                }
                return _inboxRootPath;
            }
        }

        private string _archiveRootPath;

        /// <summary>
        /// Full Outlook path of the archive root.
        /// <para>
        /// #614 D6: this previously returned an UNVERIFIED default-store-scoped string combine,
        /// so a profile whose archive lives in another store, or which has no Archive folder at
        /// all, silently produced a path no folder answers to, which every downstream consumer
        /// then treated as authoritative. The value is now cross-checked ONCE, at resolution
        /// time, against the folder that actually resolves for it, and the validated result is
        /// cached; the check therefore adds no per-filed-item COM round-trip.
        /// </para>
        /// </summary>
        /// <exception cref="InvalidOperationException">The archive root is unresolvable or lies
        /// outside the composed path. The diagnostic names the rule only; the path is withheld
        /// because it carries a mailbox address (#602).</exception>
        public string ArchiveRootPath
        {
            get
            {
                if (_archiveRootPath is null)
                {
                    _archiveRootPath = ArchiveRootPathGuard.RequireResolvedArchiveRoot(
                        Path.Combine(Root.FolderPath, "Archive"),
                        ArchiveRoot?.FolderPath,
                        message => logger.Error(message)
                    );
                }
                return _archiveRootPath;
            }
        }

        private Folder _archiveRoot;
        public Folder ArchiveRoot => Initializer.GetOrLoad(ref _archiveRoot, LoadArchiveRoot);

        internal Folder LoadArchiveRoot()
        {
            var folderHandler = new FolderPredictor(_globals);
            return folderHandler.GetFolder(Root.Folders, "Archive");
        }

        public string EmailPrefixToStrip => Properties.Resources.Email_Prefix_To_Strip;

        private StackObjectCS<object> _movedMailsStack;
        public StackObjectCS<object> MovedMailsStack
        {
            get { return _movedMailsStack; }
            set { _movedMailsStack = value; }
        }

        private TimedDiskWriter<string> _emailMoveWriter;
        public TimedDiskWriter<string> EmailMoveWriter =>
            Initializer.GetOrLoad(ref _emailMoveWriter, LoadEmailMoveWriter);

        public TimedDiskWriter<string> LoadEmailMoveWriter()
        {
            var writer = new TimedDiskWriter<string>();
            writer.Config.WriteInterval = TimeSpan.FromSeconds(5);
            writer.Config.TryAddTimeout = 20;
            if (_globals.FS.SpecialFolders.TryGetValue("MyDocuments", out var myDocuments))
            {
                SortEmail.WriteCSV_StartNewFileIfDoesNotExist(
                    _globals.FS.Filenames.MovedMails,
                    myDocuments
                );
                writer.DiskWriter = async (items) =>
                {
                    // This lambda is assigned to Action<IEnumerable<T>>, so it is async void and is
                    // invoked from a System.Timers.Timer elapsed callback with no
                    // SynchronizationContext. An exception escaping here is re-raised on the thread
                    // pool and terminates the Outlook host process, so the broad catch is the
                    // deliberate boundary treatment rather than a swallowed error.
                    try
                    {
                        bool movedMailsWritten = await FileIO2.WriteTextFileAsync(
                            _globals.FS.Filenames.MovedMails,
                            items.ToArray(),
                            myDocuments,
                            default
                        );
                        if (!movedMailsWritten)
                        {
                            logger.Error(
                                $"Timed disk write of {_globals.FS.Filenames.MovedMails} did not complete."
                            );
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Error(
                            $"Timed disk write of {_globals.FS.Filenames.MovedMails} threw.",
                            ex
                        );
                    }
                };
                return writer;
            }
            else
            {
                return null;
            }
        }

        private string _userEmailAddress;

        public event PropertyChangedEventHandler PropertyChanged;

        public string UserEmailAddress
        {
            get
            {
                if (_userEmailAddress is null)
                {
                    _userEmailAddress = ResolveCurrentUserEmailAddress();
                }
                return _userEmailAddress;
            }
        }

        internal string ResolveCurrentUserEmailAddress()
        {
            // Outlook COM objects must be accessed from the STA thread on which they were
            // created. If this method is called from a background (MTA/ThreadPool) thread,
            // marshal synchronously to the UI thread to avoid COMException 0xEF640201.
            if (Thread.CurrentThread.ManagedThreadId != UiThread.UiThreadId)
            {
                string result = string.Empty;
                UiThread.UiSyncContext.Send(_ => result = ResolveCurrentUserEmailAddress(), null);
                return result;
            }

            try
            {
                var session = App?.Session ?? NamespaceMAPI;
                var addressEntry = session?.CurrentUser?.AddressEntry;
                return TryGetSmtpAddress(addressEntry) ?? string.Empty;
            }
            catch (COMException e)
            {
                logger.Warn($"Error retrieving current user SMTP address. {e.Message}", e);
                return string.Empty;
            }
        }

        internal static string TryGetSmtpAddress(AddressEntry addressEntry)
        {
            if (addressEntry is null)
            {
                return null;
            }

            try
            {
                var primarySmtpAddress = addressEntry.GetExchangeUser()?.PrimarySmtpAddress;
                if (!string.IsNullOrWhiteSpace(primarySmtpAddress))
                {
                    return primarySmtpAddress;
                }
            }
            catch (COMException) { }

            try
            {
                var address = addressEntry.Address;
                if (!string.IsNullOrWhiteSpace(address) && address.Contains("@"))
                {
                    return address;
                }
            }
            catch (COMException) { }

            return null;
        }

        private bool _darkMode = Properties.Settings.Default.DarkMode;

        [NotifyParentProperty(true)]
        public bool DarkMode
        {
            get => _darkMode;
            set
            {
                _darkMode = value;
                Properties.Settings.Default.DarkMode = value;
                Properties.Settings.Default.Save();
                NotifyPropertyChanged();
            }
        }

        public int GetExplorerScreenNumber()
        {
            System.Windows.Forms.Screen screen = GetExplorerScreen();
            return System.Windows.Forms.Screen.AllScreens.ToList().IndexOf(screen);
        }

        public Size GetExplorerScreenSize()
        {
            var explorer = App.ActiveExplorer();
            Rectangle bounds = new(explorer.Left, explorer.Top, explorer.Width, explorer.Height);
            return bounds.Size;
        }

        public System.Windows.Forms.Screen GetExplorerScreen()
        {
            var explorer = App.ActiveExplorer();
            Rectangle bounds = new(explorer.Left, explorer.Top, explorer.Width, explorer.Height);
            return System.Windows.Forms.Screen.AllScreens.FindMax(
                (s1, s2) =>
                {
                    var a1 = Rectangle.Intersect(s1.Bounds, bounds).Area();
                    var a2 = Rectangle.Intersect(s2.Bounds, bounds).Area();
                    return a2 > a1 ? s2 : s1;
                }
            );
        }

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        //public bool ShowInConversations
        //{
        //    get
        //    {
        //        if (App.ActiveExplorer().CommandBars.GetPressedMso("ShowInConversations"))
        //        {
        //            return true;
        //        }
        //        else
        //        {
        //            return false;
        //        }
        //    }
        //    set
        //    {
        //        View objView = (View)App.ActiveExplorer().CurrentView;
        //        if (value == false & App.ActiveExplorer().CommandBars.GetPressedMso("ShowInConversations") == true)
        //        {
        //            // Turn Off Conversation View
        //            objView.XML = Strings.Replace(objView.XML, "<upgradetoconv>1</upgradetoconv>", "", 1, Compare: Constants.vbTextCompare);
        //            objView.Save();
        //        }
        //        else if (value == true & App.ActiveExplorer().CommandBars.GetPressedMso("ShowInConversations") == false)
        //        {
        //            // Turn On Conversation View
        //            string strReplace = "<arrangement>" + Constants.vbCrLf + "        <upgradetoconv>1</upgradetoconv>";
        //            objView.XML = Strings.Replace(objView.XML, "<arrangement>", strReplace, 1, Compare: Constants.vbTextCompare);
        //            objView.Save();
        //        }
        //    }
        //}
    }
}
