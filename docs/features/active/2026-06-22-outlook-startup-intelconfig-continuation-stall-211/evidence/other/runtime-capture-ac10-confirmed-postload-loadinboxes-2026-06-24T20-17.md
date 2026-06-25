Timestamp: 2026-06-24T20-17
Command: Maintainer non-debugger cold-start DebugView capture on the AC10-fix build (HEAD c1cdf689).
EXIT_CODE: 0

# Runtime Capture: AC10 fix CONFIRMED; separate PostLoad ~121s freeze in LoadInboxes

## Result 1 — AC10 fix works (JunkCertain block eliminated)

| Metric | Before (T13-11) | After (this, c1cdf689) |
| --- | ---: | ---: |
| [spam-init] ValidatePathsSet.JunkCertain | 50,172 ms | 44.1 ms |
| [spam-init] ValidatePathsSet (total) | 51,195 ms | 777.2 ms |
| Engines phase | 0:51.29 | 0:00.90 |
| [Startup timing] TOTAL (LoadSequentialAsync) | ~1:56 | 0:02.07 |

`InitAsync(modelLoad)` 0.9 ms (deserialize never the cause). The direct-navigation
fix removed the full default-store FolderTree enumeration. All `[phase-net]` /
`[ui-heartbeat]` during LoadSequentialAsync are responsive.

## Result 2 — Separate ~121s STA freeze AFTER startup (PostLoad)

The Phase 3.3 full-lifetime heartbeat caught it:
```
20:18:05,833 [startup-lifetime-heartbeat] stageLabel=PostLoad gapMs=32.3
20:20:06,857 [startup-lifetime-heartbeat] stageLabel=PostLoad gapMs=120774.6   <- ~121s STA freeze
```
Freeze window ~20:18:05.8 -> 20:20:06.8, after `Finished loading globals`
(20:18:05.014), during `WrappedMSProvider::Logon` / `GLookSyncer` /
`GmailSyncImpl::Init` / `EmailAliases` and address-book `ABContainer::OpenEntry` /
`PrepareRecipient` / `ABLogon::PrepareRecips` churn. TaskMaster logs nothing during
the freeze (a blocked synchronous COM call does not return to log).

## Attribution of the PostLoad freeze (code-grounded)

`AppEvents.Hook()` defers the real event-hookup to `PerformReadinessHookup()`
(`AppEvents.cs:215`) via a readiness-gated DispatcherTimer, so it runs in PostLoad.
`PerformReadinessHookup` does three COM operations (lines 220/224/228):
1. `Globals.Ol.ToDoFolder.Items` — `NamespaceMAPI.GetDefaultFolder(ToDo)` (default store; expected fast).
2. `Globals.Ol.OlReminders` (expected fast).
3. **`Globals.Ol.Inboxes`** -> `LoadInboxes` (`AppOlObjects.cs:98-139`): iterates ALL
   stores (`NamespaceMAPI.Stores`) and for each that passes `ShouldIncludeStore`
   (line 108) calls **`store.GetDefaultFolder(olFolderInbox)` (line 113)** — which
   requires the store to log on. For the failing Gmail/GWSO store this blocks ~121s
   on `WrappedMSProvider::Logon`.

"Hook complete" (which logs toDoItemsMs/remindersMs/inboxSubscribeMs) never appears
in this capture because `PerformReadinessHookup` is blocked mid-method. Prime
suspect: line 228 -> LoadInboxes -> line 113 `GetDefaultFolder(Inbox)` on the
failing store. The GWSO exclusion (`ShouldIncludeStore`) should skip it but does
not — consistent with the research's predicted bug: when `store.FilePath` throws,
`filePath` stays null and the GWSO guard `!IsNullOrWhiteSpace(filePath)` never
fires, so the store is included and `GetDefaultFolder` runs on it.

The per-store COMException handler (`AppOlObjects.cs:119-136`) only catches fast
throws; it does not prevent the synchronous logon BLOCK.

## Why this differs from the JunkCertain (AC10) block

JunkCertain enumerated the DEFAULT store (cannot be skipped); the fix was to stop
the full FolderTree enumeration (direct navigation). LoadInboxes iterates MULTIPLE
stores and is DESIGNED to skip stores (`ShouldIncludeStore`), so the maintainer's
"skip not-logged-on store" (Option 2) genuinely applies here.

## Next fix increment (proposed)

1. Make `LoadInboxes` not block on a failing / not-ready store: exclude the
   GWSO/Google store robustly BEFORE `GetDefaultFolder` even when `store.FilePath`
   throws/null (research Part A: harden `ShouldIncludeStore`/`StoreFilterAttribution`
   for the FilePath-null case using a non-blocking signal such as DisplayName /
   ExchangeStoreType), and/or skip stores that are not logged on. Non-hardcoded
   (pattern/readiness-based; no user-specific store identity).
2. Add per-step START markers to `PerformReadinessHookup` (before ToDoFolder.Items,
   OlReminders, Inboxes) so the next capture CONFIRMS line 228 is the culprit and
   verifies the fix.
3. Red-before-green: a fake store whose FilePath throws / that is not logged on must
   be skipped WITHOUT `GetDefaultFolder` being called.
