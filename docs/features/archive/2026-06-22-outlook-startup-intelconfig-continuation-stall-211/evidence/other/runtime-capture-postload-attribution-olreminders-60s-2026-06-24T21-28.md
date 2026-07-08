Timestamp: 2026-06-24T21-28
Command: Maintainer non-debugger cold-start DebugView capture on the PostLoad-probe build (HEAD e298d8d6).
EXIT_CODE: 0

# Runtime Capture: PostLoad freeze attributed — OlReminders (App.Reminders) = 60s

## Probe result (definitive)

```
[readiness-hookup] step=ToDoFolder.Items end elapsedMs=2.90        <- fast
[readiness-hookup] step=OlReminders start
   ... 60s WrappedMSProvider::Logon / Gmail / address-book churn ...
[readiness-hookup] step=OlReminders end elapsedMs=60037.31         <- 60s BLOCK
[readiness-hookup] step=Inboxes start
[loadinboxes] store=dmoisan@realgoodfoods.com shouldIncludeMs=0.65 included=true getDefaultFolderMs=0.10   <- Exchange store, fast
   ... ~62s more freeze (second store inbox or remainder of Inboxes step; just past captured log) ...
[startup-lifetime-heartbeat] PostLoad gapMs=122432.9
```

Total PostLoad STA freeze ~123s = OlReminders (60s) + ~62s after Inboxes start.

## Attribution

- `AppEvents.PerformReadinessHookup` line 224: `OlReminders = Globals.Ol.OlReminders`
  -> `AppOlObjects.OlReminders` -> `App.Reminders` (the GLOBAL Outlook Reminders
  collection across ALL stores). Accessing it forces Outlook to load reminders from
  every store including the failing Gmail/GWSO store -> 60s block on
  `WrappedMSProvider::Logon`.
- `LoadInboxes` enumerated one store (the Exchange default) FAST (0.10ms). The
  additional ~62s after `Inboxes start` is most likely a SECOND store's inbox
  (`store.GetDefaultFolder(Inbox)` on the Gmail store) whose `[loadinboxes]` line is
  just beyond the captured log, or the remainder of the Inboxes step. Not fully
  confirmed in this capture.

## Important distinction (TaskMaster-fixable vs environmental)

- JunkCertain (AC10, FIXED): TaskMaster built an unnecessary full FolderTree of the
  default store. Clear add-in inefficiency; the direct-nav fix eliminated it.
- OlReminders (60s): `App.Reminders` is a single GLOBAL Outlook OOM call. It is slow
  because the GWSO/Gmail store's provider is failing to log on. It is NOT a
  TaskMaster over-enumeration and CANNOT be store-scoped (there is no per-store
  reminders collection) nor made non-blocking (synchronous STA COM cannot be
  cancelled/timed-out in flight). The add-in's only levers are: defer it off the
  critical startup path (shifts WHEN the freeze occurs; does not eliminate it), make
  the reminder hookup optional/skippable, or gate it behind an all-stores readiness
  check with a fallback.
- Inbox second-store block (if confirmed): this one IS in LoadInboxes (per-store
  GetDefaultFolder) and is amenable to the "skip not-ready store" approach.

## Root cause (escalating clarity)

The dominant remaining startup cost is Outlook's own GWSO/Gmail MAPI provider failing
to log on (repeating 0x80040401 across EmailAliases / GmailSyncImpl / GLookSyncer /
WrappedMSProvider::Logon), and TaskMaster touching global/multi-store Outlook
collections (App.Reminders, multi-store inboxes, address book) that inherently
include that broken store. Each synchronous touch blocks the STA ~60s.

## Options (need maintainer direction)

1. Fix the LoadInboxes second-store block via skip-not-ready-store (in-scope, real).
2. OlReminders: defer the reminder hookup off the critical startup path and/or gate
   it behind readiness with a timeout fallback. Honest caveat: a synchronous
   `App.Reminders` access still blocks the STA ~60s whenever it runs; deferral moves
   the freeze out of the initial startup window but does not remove it.
3. Environmental root resolution: repair/re-authenticate the GWSO (Google Workspace
   Sync) account so its store stops failing logon. This is outside the add-in but is
   the only path that eliminates the ~60s-per-touch cost at the source.
