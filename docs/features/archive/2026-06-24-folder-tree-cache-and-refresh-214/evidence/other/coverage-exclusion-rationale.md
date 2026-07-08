Timestamp: 2026-06-24T18:47-04:00
Issue: #214

# Coverage Exclusion Rationale

The following exclusions are limited to members classified `COM_OR_WPF_BOUND` in `docs/features/active/2026-06-24-folder-tree-cache-and-refresh-214/evidence/qa-gates/issue-214-coverage-gap-map.md` and adapter seams that require live Outlook COM objects or a WPF dispatcher. The fakeable domain logic around snapshot construction, request filtering, subtree projection, stale snapshot behavior, and caller migration remains covered by unit tests.

## UtilitiesCS/EmailIntelligence/EmailParsingSorting/EmailDataMiner.FolderExtraction.cs

- `CreateFolderHandleResolver()`: excluded because the method constructs `OutlookFolderHandleResolver` from `_globals.Ol.NamespaceMAPI`, which requires a live Outlook COM namespace.

## UtilitiesCS/EmailIntelligence/EmailParsingSorting/EmailDataMiner.cs

- `DeleteStagingFilesAsync()`: excluded because this public wrapper schedules filesystem cleanup through the AppData production boundary; the injectable `DeleteStagingFiles(string, Func<string, bool>, Func<string, string[]>, Action<string>)` method contains the deletion logic and is covered by unit tests.
- `DeleteStagingFilesFromAppData()`: excluded because it binds the tested deletion helper to production `Directory` and `File` APIs using `_globals.FS.SpecialFolders`; tests cover the same control flow through the injected delegates on `DeleteStagingFiles`.

## UtilitiesCS/EmailIntelligence/SubjectMap/SubjectMapSco.Orchestration.cs

- `CreateFolderHandleResolver(IApplicationGlobals appGlobals)`: excluded because the method constructs `OutlookFolderHandleResolver` from `appGlobals.Ol.NamespaceMAPI`, which requires a live Outlook COM namespace.
- `ShowSummaryMetrics()`: excluded because this public wrapper constructs and shows the WinForms `SubjectMapMetrics` viewer; the injectable `ShowSummaryMetrics(Action<IEnumerable<SummaryMetric>> showViewer)` overload contains the pure summary aggregation and is covered by unit tests.
- `RebuildCore(object state)`: excluded because it is the worker body for the already excluded `RebuildAsync(IApplicationGlobals appGlobals)` Outlook rebuild workflow; pure folder querying, mail tuple projection, entry rebuild, and encoder orchestration are covered through injectable methods.

## UtilitiesCS/OutlookObjects/Folder/OutlookFolderHierarchyReader.cs

- `OutlookFolderHierarchyReader(Outlook.NameSpace namespaceMapi, StoresWrapper storesWrapper)`: excluded because it enumerates `namespaceMapi.Stores`, which requires a live Outlook COM namespace.
- `ReadFolders(FolderTreeRequest request, CancellationToken cancellationToken)`: excluded because its production path is the live Outlook store hierarchy read and cannot execute without Outlook-backed store adapters.
- `ReadRecords(FolderTreeRequest request, CancellationToken cancellationToken)`: excluded because it enumerates included Outlook stores and calls `GetRootFolder()` on store adapters backed by COM objects in production.
- `ReadStore(IOutlookFolderAdapter root, string storeId, ICollection<OutlookFolderHierarchyRecord> records, CancellationToken cancellationToken)`: excluded because production execution walks live Outlook folder children exposed by COM-backed adapters.
- `ToNode(OutlookFolderHierarchyRecord record, IEnumerable<OutlookFolderHierarchyRecord> records)`: excluded as part of the live hierarchy reader pipeline scoped by the `COM_OR_WPF_BOUND` gap-map classification for P9-T27.
- `OutlookStoreAdapter`: excluded because it wraps `Outlook.Store`, calls `GetRootFolder()`, and passes live COM store objects into `StoresWrapper.ShouldIncludeStore`.
- `OutlookFolderAdapter`: excluded because it wraps `Outlook.MAPIFolder` and enumerates live COM child folders through `MAPIFolder.Folders`.

## UtilitiesCS/OutlookObjects/Folder/OutlookFolderHandleResolver.cs

- `OutlookFolderHandleResolver(Outlook.NameSpace namespaceMapi)`: excluded because it constructs the live Outlook lookup seam from a COM namespace.
- `OutlookFolderHandleResolver(IFolderLookup folderLookup)`: excluded under the P9-T27 gap-map classification for the resolver construction seam; production callers bind it to a live Outlook namespace lookup.
- `Resolve(FolderTreeSnapshotNode node)`: excluded because production resolution delegates to Outlook folder lookup state through `IFolderLookup`.
- `TryResolve(FolderTreeSnapshotNode node, out object folder)`: excluded because production resolution delegates to Outlook folder lookup state through `IFolderLookup`.
- `OutlookFolderLookup`: excluded because `GetFolderFromId` delegates to `Outlook.NameSpace.GetFolderFromID`, which requires live Outlook COM state.

## UtilitiesCS/OutlookObjects/Folder/OutlookFolderNotificationSink.cs

- `OutlookFolderNotificationSink(Outlook.NameSpace namespaceMapi)`: excluded because production subscription discovery is bound to a live Outlook namespace.
- `OutlookFolderNotificationSink(IEnumerable<IOutlookFolderNotificationSubscription> subscriptions)`: excluded under the P9-T27 gap-map classification for the notification subscription seam.
- `Start()`: excluded because production execution subscribes to Outlook folder and store notification sources.
- `Dispose()`: excluded because production execution unsubscribes from Outlook folder and store notification sources.
- `HandleNotification(object sender, FolderTreeNotification notification)`: excluded because production execution handles live Outlook notification callbacks.

## UtilitiesCS/OutlookObjects/Folder/WpfDispatcherYield.cs

- `WpfDispatcherYield`: excluded because `YieldAsync` depends on `Dispatcher.Yield(DispatcherPriority.Background)`, which requires a WPF dispatcher context for deterministic execution.
