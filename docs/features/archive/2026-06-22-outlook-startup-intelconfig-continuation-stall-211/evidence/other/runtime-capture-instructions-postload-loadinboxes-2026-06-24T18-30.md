# Runtime Capture Instructions — PostLoad / LoadInboxes attribution (Issue #211)

Timestamp: 2026-06-24T18-30

Purpose: pinpoint the COM call responsible for the ~121 s PostLoad STA freeze (full-lifetime
heartbeat `gapMs=120774` after "Finished loading globals") using the two diagnosis-only probes
added by this plan. This capture is runtime evidence and is NOT CI-automatable — it requires a
non-debugger cold start against a live Outlook profile. The instrumentation is behavior-preserving;
it only emits additional `OutputDebugString`/log4net lines.

## Build / run steps (non-debugger cold start)

1. Build Debug:
   `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU"`
2. Ensure the add-in's log4net `DebugAppender` (OutputDebugString) is active, OR attach DebugView.
   - Run Sysinternals DebugView (`Dbgview.exe`) elevated, with "Capture Global Win32" enabled, BEFORE
     launching Outlook. Do NOT attach a debugger to Outlook (a debugger perturbs STA timing).
3. Fully close Outlook. Clear any warm state that would mask a cold start (the freeze reproduces on a
   cold start where stores are not yet ready).
4. Launch Outlook normally (double-click / Start menu), not from a debugger.
5. Let startup run to completion (or until the multi-minute freeze is observed). Stop the DebugView
   capture and save the log to a text file.
6. Filter the saved log for the two probe prefixes and paste the relevant window into the
   `PENDING MAINTAINER CAPTURE` section below.

## Expected line patterns to collect

Readiness-hookup per-step markers (emitted by `AppEvents.PerformReadinessHookup`):

```
[readiness-hookup] step=ToDoFolder.Items start
[readiness-hookup] step=ToDoFolder.Items end elapsedMs=<F2>
[readiness-hookup] step=OlReminders start
[readiness-hookup] step=OlReminders end elapsedMs=<F2>
[readiness-hookup] step=Inboxes start
[readiness-hookup] step=Inboxes end elapsedMs=<F2>
```

Per-store attribution (emitted by `AppOlObjects.LoadInboxes`, one line per enumerated store):

```
[loadinboxes] store=<DisplayName> shouldIncludeMs=<F2> included=<true|false> getDefaultFolderMs=<F2 or n/a>
```

## Interpretation rule

1. Readiness step: the LAST `[readiness-hookup] ... start` line that has NO matching
   `[readiness-hookup] ... end` line before the freeze names the blocking operation among the three
   (`ToDoFolder.Items`, `OlReminders`, `Inboxes`). If the unmatched start is `step=Inboxes`, the freeze
   is inside the inbox enumeration/subscription, which drives `Globals.Ol.Inboxes` -> `LoadInboxes`.
2. Store and call: among the `[loadinboxes]` lines, the line whose `shouldIncludeMs` OR
   `getDefaultFolderMs` is multi-second (thousands of ms) names the blocking store AND which COM call
   blocked:
   - large `shouldIncludeMs` => the block is inside `StoresWrapper.ShouldIncludeStore` (the FilePath
     read on the store);
   - large `getDefaultFolderMs` => the block is inside `store.GetDefaultFolder(olFolderInbox)`.
   - If `[loadinboxes]` lines stop after a `start` with no subsequent line for a store, the block is in
     the guarded `DisplayName` read or `ShouldIncludeStore` for the NEXT store (the per-store line is
     emitted only after `ShouldIncludeStore` returns; a missing line for a store whose predecessor
     completed indicates the block occurred before that store's line could be emitted).

This evidence is diagnosis-only. It identifies the blocking operation; it does not itself fix the
freeze. Any fix is out of scope for this plan and must be planned separately.

## PENDING MAINTAINER CAPTURE

> This section is a placeholder. No runtime lines have been captured yet. The maintainer must perform
> the cold-start capture above and paste the filtered `[readiness-hookup]` and `[loadinboxes]` lines
> here, then record the interpretation (blocking step / blocking store / blocking COM call) per the
> rule above. Until this section is filled, NO captured-evidence claim is asserted.

```
<paste filtered [readiness-hookup] and [loadinboxes] lines here>
```

Interpretation (to be completed by maintainer):
- Blocking readiness step: <pending>
- Blocking store DisplayName: <pending>
- Blocking COM call (ShouldIncludeStore FilePath vs GetDefaultFolder): <pending>
