# [P8-T2] AC-U5 manual verification (runbook, user-story.md lines 52-86)

- Issue: #792
- Timestamp: 2026-09-18T06-30
- Command: human execution of the AC-U5 runbook (steps 1-9, both entry points, QuickFiler opened first so an item-body WebView was already running) against the Outlook session that loaded the add-in rebuilt in [P8-T1]; step 9 instrumented by the executor with `Get-Content -LiteralPath TaskMaster/bin/Debug/logs/debug_2026-09-17.log` piped to `Select-String -SimpleMatch -CaseSensitive` for each literal below, plus a header-regex parse of every line into `(level, logger)` (run under `pwsh -NoProfile -WorkingDirectory <item worktree>`; the log path is relative to the item worktree and the log file is not committed)
- EXIT_CODE: 0
- Output Summary: Outlook session start 2026-09-17 21:43:22 (add-in `ThisAddIn_Startup() fired`; process start 21:43:18). Observations 1-3 maintainer-confirmed PASS; observation 4 instrument-verified PASS: 0 occurrences of `Breadcrumb CoreWebView2 initialization failed`, 0 of `0x8007139F`, 0 of `resource not in correct state` in a 284,409-byte log spanning 21:43:22 to 23:48:15, with 103 lines written by `QuickFiler.*` loggers and 40 `ERROR`-level lines (all unrelated) as positive controls. `AC-U5: PASS`.

## Session

- Outlook process start: 2026-09-17 21:43:18 (`Get-Process -Name OUTLOOK`, `StartTime`; pid observed once, still running on 2026-09-18 and not closed or killed by the executor).
- Add-in session start: 2026-09-17 21:43:22,837, the first line of the log: `ThisAddIn_Startup() fired` (transcribed under "Log lines inspected").
- Add-in loaded from this worktree's `TaskMaster/bin/Debug` (registry `Manifest` resolves to that directory's `TaskMaster.vsto|vstolocal`, `LoadBehavior` 3; see [P8-T1]).
- Ordering precondition of the runbook honoured: QuickFiler was opened first (step 4), so an item-body WebView was running before the first Efc pop-out (step 5). The log shows `QuickFiler.Controllers.QfcFormController` and `QuickFiler.Controllers.QfcItemController` lines before the first `QuickFiler.Controllers.EfcDataModel` constructor line at 21:45:13.

## The four observations

The runbook's pass/fail section (user-story.md lines 75-84) lists four observations. They fall into two evidentiary classes, kept separate here.

### Maintainer-confirmed (human observation, reported by the person who ran the session)

1. Step 5, pop-out from QuickFiler into the Efc view: the folder area showed one or more suggestion rows, not a blank list. **PASS**.
2. Step 6, typed search in the popped-out Efc view: the visible rows changed in response to the typed text. **PASS**.
3. Step 7, ribbon Sort Email: the area under the "Matched Folders:" label showed one or more suggestion rows, not a blank list. **PASS**.

These three are the maintainer's direct observations of the running UI. The executor did not and cannot observe them; they are recorded as reported.

### Instrument-verified (measured by the executor against the session log)

4. Step 9, session log clean. **PASS**.
   - Log file: the item worktree's `TaskMaster/bin/Debug/logs/debug_2026-09-17.log` (relative path; the file is not committed).
   - Size 284,409 bytes; 1,361 lines; last written 2026-09-17 23:48:15; first timestamp 21:43:22,837, last timestamp 23:48:15,842. The file has been growing during the session (an earlier figure of 279,805 bytes at 21:47 was supplied to the executor and is superseded by this later measurement).
   - `Breadcrumb CoreWebView2 initialization failed`: 0 occurrences (case-sensitive literal).
   - `0x8007139F`: 0 occurrences (case-sensitive literal); `8007139F` case-insensitive: 0.
   - `resource not in correct state`: 0 occurrences (case-sensitive literal).
   - `initialization failed` case-insensitive: 0. `Breadcrumb` (any case-sensitive occurrence): 0.
   - `WebView2` (case-insensitive): 3 lines, all `QfcFormController.ParkFocusAndCancelSelectors entered. WebView2Focused=False ...` DEBUG lines tagged Issue #796 (transcribed below); none is an initialization event or an error.

The verification passes only if all four hold. All four hold.

AC-U5: PASS

## Positive controls that make observation 4 meaningful

A zero-match is uninformative on its own: it is equally consistent with the breadcrumb host's logger never reaching this appender, and with a mistyped pattern. The following establish the instrument.

- **Same pattern form finds known-present text.** In the same invocation, `Select-String -SimpleMatch -CaseSensitive 'tesseract'` returned 38 lines. The zero counts above come from the same cmdlet and switches, so they are true zeros for this file.
- **The QuickFiler namespace reaches this appender.** A header-regex parse of the file (1,120 of 1,361 lines parse as log headers; the remaining 241 are stack-trace continuation lines) attributes 103 lines to loggers in the `QuickFiler.*` namespace: `QuickFiler.Helper_Classes.ConversationResolver` 38, `QuickFiler.Controllers.QfcFormController` 36, `QuickFiler.Controllers.QfcItemController` 18, `QuickFiler.Controllers.EfcDataModel` 7, `QuickFiler.Controllers.QfcHomeController` 2, `QuickFiler.EfcHomeController` 2. (A looser count of lines containing the token `QuickFiler.` anywhere, including inside messages and stack frames, is 126; the 103 figure is the logger-field count and is the one relied on.)
- **Inference from log4net hierarchical routing (not a direct observation).** `WebView2BreadcrumbHost` is `QuickFiler.Viewers.WebView2BreadcrumbHost` (re-derived at execution time: `QuickFiler/Viewers/WebView2BreadcrumbHost.cs` line 11 `namespace QuickFiler.Viewers`, line 34 `public sealed class WebView2BreadcrumbHost`; the plan's self-review cites line 12 for the namespace, which is off by one against the current tree). Under log4net's hierarchical logger configuration, a `QuickFiler.Viewers.*` logger inherits the appenders of its `QuickFiler` ancestors, so an `ERROR` from the breadcrumb host would have landed in this file. `QuickFiler.Viewers.*` itself has zero lines, which is the expected shape: that class logs only on failure. This step is an inference from configuration, not something the session log shows directly.
- **The error path to this file is live.** 40 `ERROR`-level lines exist (level token parsed from the header): 38 from `UtilitiesCS.EmailIntelligence.ImageStripper` (tesseract engine initialisation) and 2 from `UtilitiesCS.OutlookObjects.Store.StoreWrapper` (PrimarySmtpAddress on a secondary inbox). All are unrelated to the breadcrumb host; their presence proves that `ERROR` writes from the add-in reach this file during this session. (A case-insensitive count of the token `error` anywhere in a line is 48; the 40 figure is the level-field count. A figure of 48 `ERROR` entries was supplied to the executor and is superseded by the level-parsed 40.)

## Why observation 4 is the discriminating observation

AC-U1 adds a retry to `CoreWebView2` initialization. A session can therefore look entirely correct to a person (observations 1-3 all PASS) while the log still records the initialization failing on a first attempt and recovering on a retry. Observations 1-3 cannot separate "the three environment-creation sites genuinely converged on one owner and the user-data-folder conflict is gone" from "the retry is masking a conflict that still occurs on every pop-out". A clean log, with the error path proven live, separates them: zero initialization failures across both entry points and a two-hour session means no attempt failed, so nothing was retried and nothing was masked.

## Log lines inspected (transcribed as text; no log file committed)

Lines are transcribed verbatim except that no line containing an absolute host path was selected for transcription (22 lines of the file contain one inside stack frames or file paths; none of them is relevant to observation 4). The line numbers are those of the file at the 23:48:15 measurement.

Session start (line 1):

```
2026-09-17 21:43:22,837 [VSTA_Main] DEBUG TaskMaster.ThisAddIn [(null)] - ThisAddIn_Startup() fired
```

The three `WebView2`-mentioning lines (the only such lines; DEBUG, unrelated to initialization):

```
L1236: 2026-09-17 21:45:15,502 [VSTA_Main] DEBUG QuickFiler.Controllers.QfcFormController [(null)] - Issue #796: QfcFormController.ParkFocusAndCancelSelectors entered. WebView2Focused=False ActiveFormNull=False Groups=8
L1285: 2026-09-17 21:46:24,814 [VSTA_Main] DEBUG QuickFiler.Controllers.QfcFormController [(null)] - Issue #796: QfcFormController.ParkFocusAndCancelSelectors entered. WebView2Focused=False ActiveFormNull=False Groups=8
L1322: 2026-09-17 21:47:53,493 [VSTA_Main] DEBUG QuickFiler.Controllers.QfcFormController [(null)] - Issue #796: QfcFormController.ParkFocusAndCancelSelectors entered. WebView2Focused=False ActiveFormNull=False Groups=8
```

First Efc data-model line (evidence that the Efc view was constructed after QuickFiler was already open):

```
L1228: 2026-09-17 21:45:13,654 [VSTA_Main] DEBUG QuickFiler.Controllers.EfcDataModel [(null)] - [Data model timing] EfcDataModel constructor load start | constructor load | threadId=1; syncContext=System.Windows.Forms.WindowsFormsSynchronizationContext
```

Representative `ERROR` lines (the two StoreWrapper lines and the first of the 38 ImageStripper lines; the error path is live and every error is unrelated):

```
L157: 2026-09-17 21:43:56,433 [VSTA_Main] ERROR UtilitiesCS.OutlookObjects.Store.StoreWrapper [(null)] - Error retrieving PrimarySmtpAddress from secondary inbox. The operation failed.
L180: 2026-09-17 21:43:56,550 [VSTA_Main] ERROR UtilitiesCS.OutlookObjects.Store.StoreWrapper [(null)] - Error retrieving PrimarySmtpAddress from secondary inbox. The operation failed.
2026-09-17 21:43:59,742 [5] ERROR UtilitiesCS.EmailIntelligence.ImageStripper [(null)] - Failed to initialise tesseract engine.. See https://github.com/charlesw/tesseract/wiki/Error-1 for details.
```

Search results for the failure literals (each `Select-String -SimpleMatch -CaseSensitive` over the whole file):

```
COUNT [Breadcrumb CoreWebView2 initialization failed]: 0
COUNT [0x8007139F]: 0
COUNT [resource not in correct state]: 0
COUNT [Breadcrumb]: 0
COUNT [QuickFiler.Viewers]: 0
COUNT [tesseract]: 38   (positive control, same cmdlet and switches)
```

## Scope notes

- This task is human-executed for observations 1-3 and is not automated; no `[TestMethod]` exists for AC-U5 and none counts toward any figure (plan decision D11).
- The log file stays in the gitignored `TaskMaster/bin/Debug/logs/` directory; only the transcriptions above are committed.
- Outlook was running throughout and was not closed or ended by the executor.
