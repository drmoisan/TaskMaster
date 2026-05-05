# Thread-Affinity Inspection Evidence

Timestamp: 2026-05-05T09:30:00-04:00
Inspected Methods:
- ThisAddIn.Application_Startup
- ApplicationGlobals.LoadAsync(bool)
- ApplicationGlobals.LoadSequentialAsync()
- AppOlObjects.LoadAsync()
- AppOlObjects.LoadStoresAsync()
- StoresWrapper.RewireOlObjectsAsync()
- AppToDoObjects.LoadIdListAsync()
- AppToDoObjects.LoadProjInfoAsync()
- AppEvents.LoadAsync()
- AppEvents.Hook()
- AppEvents.ProcessNewInboxItemsAsync()
- AppAutoFileObjects.LoadAsync(bool)
- Manager.InitAsync()
- AppItemEngines.InitAsync()
Decision:
- ThisAddIn.Application_Startup: remain UI-thread-only as the Outlook COM startup entry point; no direct code change is required because coordinator phasing can be handled inside `ApplicationGlobals.LoadSequentialAsync()`.
- ApplicationGlobals.LoadAsync(bool): caller-owned coordinator that may offload only background-safe phases while preserving UI-thread ownership for COM phases.
- ApplicationGlobals.LoadSequentialAsync(): split-phase coordinator; `_olObjects.LoadAsync()` and `_events.LoadAsync()` must remain on the caller STA/UI thread, while background-safe phases may be explicitly offloaded with cooperative yields between heavy phases.
- AppOlObjects.LoadAsync(): remain UI-thread-only because it immediately enters store loading and Outlook COM-backed materialization.
- AppOlObjects.LoadStoresAsync(): remain UI-thread-only because deserialization currently triggers store rewire completion that depends on Outlook COM-backed store objects.
- StoresWrapper.RewireOlObjectsAsync(): remain UI-thread-only; store enumeration, `StoreWrapper.Init()`, and restore paths dereference Outlook COM and may only gain cooperative yield boundaries on the caller STA/UI thread.
- AppToDoObjects.LoadIdListAsync(): requires split-phase handling; pure file/data loading may stay background-safe, but any `Parent.Ol.App` access and `IDList.RefreshIDList()` work must return to the caller STA/UI thread.
- AppToDoObjects.LoadProjInfoAsync(): requires split-phase handling; `ProjectData` file loading is background-safe, but `ProjectData.Rebuild(Parent.Ol.App)` must remain on the caller STA/UI thread.
- AppEvents.LoadAsync(): remain UI-thread-only because it invokes `Hook()` and startup inbox processing over Outlook COM collections.
- AppEvents.Hook(): remain UI-thread-only because it binds Outlook `Items`, reminders, and inbox event handlers from COM-backed folders.
- AppEvents.ProcessNewInboxItemsAsync(): remain UI-thread-only for the inbox `Restrict`, `Cast`, and `MailItem` materialization path; no contingent-file promotion is required for this bug fix.
- AppAutoFileObjects.LoadAsync(bool): requires split-phase implementation if moved; most subordinate loads are already background-safe, but `LoadProgressPane()` is UI-only, so this contingent file stays out of scope for the current minimal fix.
- Manager.InitAsync(): proven background-safe; it resets async classifier loaders from embedded/resource-backed configuration and does not dereference Outlook COM.
- AppItemEngines.InitAsync(): proven background-safe for initialization; it reads manager configuration and creates engine wrappers without performing Outlook COM access during the init path.
- TaskMaster/ThisAddIn.cs required: no.
- TaskMaster/AppGlobals/AppEvents.cs must move in-scope: no.
- TaskMaster/AppGlobals/AppAutoFileObjects.cs must move in-scope: no.
- TaskMaster/AppGlobals/AppItemEngines.cs must move in-scope: no.
- _autoFileObjects.LoadAsync(false): requires split-phase implementation; for this bug fix it will remain on the caller STA/UI thread unless later evidence requires promotion.
- Engines.InitAsync(): proven background-safe.
- Any contingent production file from [P1-T1] must move in-scope: no.
Promoted Contingent File Count: 0
Stop Rule: Execution must stop for plan revision if Promoted Contingent File Count: is greater than 1.
