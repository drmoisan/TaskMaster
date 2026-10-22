# Human-Exception Runbook — Confirm QuickFiler Folder Drop-Down Close Ordering (Issue #796, AC6)

This runbook is a contract-conformant human-exception runbook per
`.claude/skills/human-exception-runbook/SKILL.md`. It discharges the single manual observation
recorded as a permitted `exception` for issue #796 acceptance criterion AC6.

## Cue

Act on this runbook when the orchestrator has recorded an `exception` response for the following
requirement:

> AC6 requires the runtime ordering between `QfcFormController.ParkFocusAndCancelSelectors` and
> `BreadcrumbDropDownHost.OnDropDownClosed` to be confirmed before the fix is chosen. The
> instrumentation itself is fully automatable, but reading the resulting ordering requires a live
> Outlook process, a real WebView2 surface, a real `ToolStripDropDown`, and human mouse and keyboard
> gestures. The repository unit-test policy forbids external processes and shown windows, so no
> automated test can produce the observation.

Execute the runbook after an engineer has landed the AC6 instrumentation at the two named sites and
produced a Debug build from it, and before the fix shape is chosen. The observation determines which
of three candidate close paths fires first, which in turn determines whether the AC2 self-inflicted
deactivation latch or the AC3 commit-before-cancel ordering carries the primary fix and the
fail-before regression test.

This runbook is not a merge gate. It does not replace, relax, or substitute for AC6: AC6 requires the
instrumentation to exist and the ordering to be confirmed, and this runbook is the procedure by which
the confirmation is produced reliably.

The three candidate close paths, as recorded in `issue.md` "Suspected Cause / Notes", are:

1. `QfcFormController.ParkFocusAndCancelSelectors` cancelling every item's selector when the
   QuickFiler form loses activation.
2. Native `ToolStripDropDown` auto-close reaching `BreadcrumbDropDownHost.OnDropDownClosed` and then
   `FinishClose` with an `Uncommitted` reason.
3. `QfcItemController.TextBoxSearch_Leave` closing the drop-down when the search box loses focus.

## Prerequisites

- **An instrumented Debug build.** A build produced from branch
  `bug/quickfiler-folder-dropdown-closes-on-open-796` after the AC6 instrumentation landed, with
  output in the repository's `TaskMaster/bin/Debug` directory, registered as a VSTO add-in and loaded
  by the Outlook profile you will use. The instrumentation must emit, at minimum:
  - an entry line from `QfcFormController.ParkFocusAndCancelSelectors`
    (`QuickFiler/Controllers/QfcFormController.Deactivate.cs:39-71`) recording the method name,
    `WebView2Focused=`, an active-form indicator, `Groups=<count>`, and per item `ItemNumber=<n>` and
    `SelectorWasOpen=<bool>`;
  - an entry line from `BreadcrumbDropDownHost.OnDropDownClosed`
    (`QuickFiler/Viewers/BreadcrumbDropDownHost.cs:426-437`) recording the method name,
    `CloseReason=<e.CloseReason>`, `ProgrammaticClose=`, `OpenState=`, `AutoClose=`, `Disposed=`, and
    `PendingClose=`, emitted at method entry **before** the guard returns, so a suppressed close is
    still visible.
- **Optional third instrumentation site.** A log line at `QfcItemController.TextBoxSearch_Leave`
  (`QuickFiler/Controllers/QfcItemController.EventHandlers.cs:217-228`) recording
  `HandoffPending=` and `DropDownOpen=`. AC6 does not require it. If it is absent, candidate 3 can
  only be assessed indirectly, by whether the observed ordering leaves room for a third close.
- **Classic Outlook for Windows**, with the QuickFiler add-in loaded. A VSTO add-in surfaces its
  commands through a custom Ribbon tab or group; confirming that tab is present is the check that the
  add-in loaded.
- **A mailbox with enough folders** that typing two or three letters into the QuickFiler folder
  search box returns at least two rows in the expanded list. Identify the letter fragment before
  starting.
- **Log file location and format.** The runtime log for a Debug build is written to the repository's
  `TaskMaster/bin/Debug/logs/` directory, one file per day named `debug_<yyyy-MM-dd>.log`
  (`TaskMaster/log4net.config:22-23`). Each line follows the conversion pattern
  `%date [%thread] %-5level %logger [%property{NDC}] - %message%newline`
  (`TaskMaster/log4net.config:30`), so every line carries a millisecond timestamp, the thread name,
  the level, and the logger name. On the log observed on 2026-09-06 the Outlook UI thread appears as
  `VSTA_Main`.
- **Logger names to filter on.** `QuickFiler.Controllers.QfcFormController` and
  `QuickFiler.Viewers.BreadcrumbDropDownHost`, plus `QuickFiler.Controllers.QfcItemController` if the
  optional third site was instrumented.
- **Expected absence before instrumentation lands.** `QuickFiler.Viewers.BreadcrumbDropDownHost`
  emits nothing today, because that type declares no logger at all until the AC6 instrumentation adds
  one. Seeing no lines from that logger in a pre-instrumentation log is expected and is not a failed
  prerequisite. If lines from that logger are still absent after the instrumentation is supposed to
  have landed, the running build is not the instrumented build; see Verification.
- **Read-only handling of logs.** Do not modify, move, truncate, or delete any existing log file. The
  log is read, not edited. If a clean segment is wanted, close Outlook, record the current end of the
  file, and reopen Outlook, so the new session's lines follow the recorded end point.
- **A text viewer able to open the file without locking it.** The appender uses
  `FileAppender+MinimalLock` (`TaskMaster/log4net.config:21`), which does not hold the file open
  between writes, so the file can be read while Outlook is running. Use a viewer that opens the file
  read-only.

## Step-by-step Instructions

1. Confirm the build under test. Verify that the assemblies in the repository's
   `TaskMaster/bin/Debug` directory were produced after the AC6 instrumentation was committed, and
   that both instrumentation call sites named in Prerequisites are present in the source that
   produced them. Record the commit SHA of the build.
2. Close any running Outlook process, so the session boundary in the log is unambiguous.
3. Record the baseline end of today's log. Open
   `TaskMaster/bin/Debug/logs/debug_<yyyy-MM-dd>.log` read-only, and write down the timestamp and
   text of the last line currently in the file (or the current line count). Do not edit, truncate,
   move, or delete the file. If today's file does not exist yet, record that fact instead.
4. Start classic Outlook for Windows. Confirm the QuickFiler add-in loaded by locating its Ribbon tab
   or group; a VSTO add-in exposes its commands through custom Ribbon tabs and groups. If the tab is
   absent, the add-in did not load and the remaining steps will produce no evidence.
5. Select a mail item in the Inbox view and start QuickFiler from the Ribbon.
6. Wait until item loading has finished. Per-item load telemetry from
   `QuickFiler.Controllers.QfcItemController` stops appearing in the log when loading completes;
   waiting keeps those lines out of the gesture segment you will read.
7. Write down the current wall-clock time and the label "Gesture A" before performing the next step.
   Repeat this labelling before each gesture, so the log segment for each gesture can be identified
   later.
8. Gesture A — arrow click. On any item, single-click the drop-down arrow in the folder field.
   Record what you observe: whether the list opens, whether it closes on its own, and approximately
   how long it remained open. Perform no further gesture for at least three seconds.
9. Write down the current time and the label "Gesture B".
10. Gesture B — keyboard open. Click into the folder search box to place the caret, then press the
    Down key once. Record what you observe, using the same three observations as step 8. Perform no
    further gesture for at least three seconds.
11. Write down the current time and the label "Gesture C".
12. Gesture C — type, then click a row. Type the two-or-three-letter fragment identified in
    Prerequisites into the search box. Confirm the list expands and stays open. Then click one row in
    the expanded list with the mouse. Record whether the list closed and whether the folder field
    changed to the clicked row's folder or still shows the previous selection.
13. Perform no unrelated gestures between steps 8 and 12. If any additional click, key press, or
    window switch occurs, write it down with its time; it will appear in the log segment and must not
    be mistaken for one of the three gestures.
14. Close QuickFiler and exit Outlook.
15. Open `TaskMaster/bin/Debug/logs/debug_<yyyy-MM-dd>.log` read-only and locate the first line that
    follows the baseline recorded in step 3. Everything from that point forward is this session's
    segment.
16. Within that segment, locate the lines whose logger name is
    `QuickFiler.Controllers.QfcFormController` or `QuickFiler.Viewers.BreadcrumbDropDownHost`, plus
    `QuickFiler.Controllers.QfcItemController` if the optional third site was instrumented. Use the
    gesture times recorded in steps 7, 9 and 11 to split the segment into the three gestures.
17. Transcribe, separately for Gesture A, Gesture B and Gesture C, the matching lines **in the order
    they appear in the file**. Do not reorder them, and do not sort or group them by timestamp.
18. Apply the decision rules in the Verification section to each gesture's transcript, and record for
    each gesture which candidate was confirmed and which were refuted.
19. Write the evidence artifact. Create a file under
    `docs/features/active/2026-09-06-quickfiler-folder-dropdown-closes-on-open-and-click-does-not-select-796/evidence/other/`
    whose filename begins with an ISO-8601 `yyyy-MM-ddTHH-mm` timestamp, for example
    `2026-09-07T09-15-dropdown-close-ordering-observation.md`. The artifact must contain:
    - a `Timestamp:` field carrying the same ISO-8601 `yyyy-MM-ddTHH-mm` value;
    - the commit SHA of the build recorded in step 1;
    - the excerpted log lines for each of the three gestures, in file order;
    - an explicit statement, per gesture, of which candidate was confirmed and which were refuted,
      using the rules in Verification.
20. Redact before saving. In the excerpt, replace absolute host paths, user names, mailbox addresses,
    and any folder name that identifies a person with a neutral placeholder (for example
    `<repo-root>`, `<user>`, `<mailbox>`, `<folder-1>`). Keep the field names, the field values that
    carry the decision (close reason, boolean flags, counts), and the line order intact.

## Verification

**Ordering is read from file order, not from timestamps.** Both instrumentation sites run on the
single Outlook UI thread, log4net appends events to the file in call order, and the entire flash
occurs inside a fraction of a second, so several of these lines can share a millisecond timestamp.
Comparing millisecond timestamps is therefore not a sound way to order them. The order of the lines
in the file is the ordering. Confirm before applying the rules below that all lines under
consideration carry the same thread name (`VSTA_Main` on the log observed on 2026-09-06); if they do
not, the same-thread premise does not hold for those lines and the ordering claim must be reported as
inconclusive rather than resolved.

Apply the following rules separately to each gesture's transcript.

**Candidate 1 — `QfcFormController.ParkFocusAndCancelSelectors` (form deactivation).**

- Confirmed when a `ParkFocusAndCancelSelectors` entry line appears **before** any
  `OnDropDownClosed` line, and that `OnDropDownClosed` line reports a close reason of `CloseCalled`
  with the programmatic-close flag `True`.
- Refuted when `ParkFocusAndCancelSelectors` is never entered during the flash, or is entered but
  reports `Groups=0` or zero cancels, or is entered strictly **after** `OnDropDownClosed`.

**Candidate 2 — native `ToolStripDropDown` auto-close.**

- Confirmed when an `OnDropDownClosed` line appears **first**, reporting a close reason of
  `AppFocusChange` or `AppClicked`, with the programmatic-close flag `False`, the open-state flag
  `True`, and the auto-close flag `True`.
- Refuted by a close reason of `CloseCalled`, or by an auto-close flag of `False` at the moment it
  fires.

**Candidate 3 — `QfcItemController.TextBoxSearch_Leave`.**

- Confirmed when a `TextBoxSearch_Leave` line reporting a `False` handoff-pending flag and a `True`
  drop-down-open flag immediately precedes the close.
- If the optional third instrumentation site was not added, record candidate 3 as not directly
  observable in this run, and state whether the observed ordering leaves room for a third close.

**Outcome to record.** For each gesture, state which single candidate fired first, and state the
status of the other two as confirmed, refuted, or not directly observable. The three gestures may
yield different answers; record each separately rather than generalising from one.

**Inconclusive results and what they indicate.**

- No lines from `QuickFiler.Viewers.BreadcrumbDropDownHost` anywhere in the segment: the running
  build is not the instrumented build, or the instrumentation is not reached. Re-check step 1 and
  repeat. Do not report an ordering.
- Lines from only one of the two required sites: the ordering cannot be established. Report which
  site produced lines and which did not, and return the observation to the engineer.
- Lines present but on differing thread names: report as inconclusive, per the same-thread premise
  above.
- Do not resolve an inconclusive result by inference. The purpose of this observation is to replace
  an inference with a measurement.

**Completion.** The runbook is complete when the evidence artifact described in step 19 exists under
the feature's `evidence/other/` directory, carries the `Timestamp:` field, contains the redacted log
excerpts in file order, and states which candidate was confirmed and which were refuted for each
gesture.

## Source and Citation

- Requirement origin and the three candidate close paths, the reproduction gestures, and AC1-AC6:
  `docs/features/active/2026-09-06-quickfiler-folder-dropdown-closes-on-open-and-click-does-not-select-796/issue.md`,
  sections "Steps to Reproduce" and "Suspected Cause / Notes" (repository file) — updated_at:
  2026-09-06.
- Decision rules for each candidate (confirm and refute conditions), the minimum log-line content,
  and the file-order-not-timestamps ordering rule:
  `docs/features/active/2026-09-06-quickfiler-folder-dropdown-closes-on-open-and-click-does-not-select-796/research/2026-09-06T21-30-quickfiler-folder-dropdown-close-ordering-research.md`
  sections 4.1, 4.2 and 10 (repository file) — updated_at: 2026-09-06.
- Justification that the drop-down pipeline currently emits no log lines, and that
  `QuickFiler.Viewers.BreadcrumbDropDownHost` produces zero lines because the type declares no
  logger: same research artifact, section 8.3 — updated_at: 2026-09-06.
- Log file directory, per-day file name pattern, conversion pattern, and the `MinimalLock` locking
  model: `TaskMaster/log4net.config` lines 21, 22-23 and 30 (repository file) — updated_at:
  2026-09-06.
- Instrumentation site 1 and the existing log4net field it uses:
  `QuickFiler/Controllers/QfcFormController.Deactivate.cs:39-71` and
  `QuickFiler/Controllers/QfcFormController.cs:21-23` (repository files) — updated_at: 2026-09-06.
- Instrumentation site 2 and the close completion point whose reason the rules read:
  `QuickFiler/Viewers/BreadcrumbDropDownHost.cs:426-437` (`OnDropDownClosed`) and `:439-455`
  (`FinishClose`) (repository files) — updated_at: 2026-09-06.
- Optional third instrumentation site and its handoff latch:
  `QuickFiler/Controllers/QfcItemController.EventHandlers.cs:217-228` and `:195` (repository files) —
  updated_at: 2026-09-06.
- Third-party UI step (steps 4 and 5, locating the add-in's Ribbon tab or group in Outlook), sourced
  web-second: Microsoft Learn, "Customize the UI for Office applications", section "Custom Ribbon
  UI", which states that a VSTO Add-in can create its own Ribbon tabs and groups to give users access
  to the solution's functionality, and lists Outlook among the supported applications. Source URL:
  https://learn.microsoft.com/en-us/visualstudio/vsto/office-ui-customization — ms.date: 2017-02-02;
  updated_at: 2026-04-24; retrieved 2026-09-06.
- Sourcing-rule note (MCP-first / web-second): no callable `mcp__*` documentation-retrieval tool is
  wired in this repository, re-verified 2026-09-06, so the MCP-first clause could not be satisfied
  for the third-party UI step above. `WebFetch` was used as the web-second mechanism. This limitation
  is recorded in the two-axis-model-selection spec's Out of Scope section and is not resolved here.
- **Unsourced-step disclosure.** The Prerequisites mention verifying add-in load state through the
  Outlook COM Add-ins dialog (File > Options > Add-ins) as an alternative to locating the Ribbon tab.
  No external source was obtained for that navigation path: on 2026-09-06 the candidate Microsoft
  Learn URLs `https://learn.microsoft.com/en-us/microsoft-365-apps/outlook/manage/view-manage-install-add-ins`,
  `https://learn.microsoft.com/en-us/office/troubleshoot/outlook/determine-if-add-in-causing-problem`
  and `https://learn.microsoft.com/en-us/visualstudio/vsto/how-to-install-and-uninstall-vsto-add-ins`
  each returned HTTP 404, and the support.microsoft.com add-ins article retrieved on the same date
  does not give the Outlook COM Add-ins navigation. That alternative is therefore presented as
  unsourced; the sourced Ribbon-tab check in step 4 is the procedure's actual load check.
- Repository steps that are grounded in this repository's own code and configuration rather than a
  vendor UI (steps 1-3 and 6-20) are sourced by the repository citations above and require no
  external URL.
- Precedent for a maintainer-sanctioned manual-verification exception with a runbook and an evidence
  artifact:
  `docs/features/archive/2026-08-07-quickfiler-search-keystroke-focus-steal-438/runbooks/verify-search-focus-retention.runbook.md`
  (repository file) — updated_at: 2026-08-08.
