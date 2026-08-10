# Human-Exception Runbook — Live-Outlook Verification of AC19, AC20, and AC21 (Issue #503)

- **Issue:** #503
- **Feature folder:** `docs/features/active/2026-08-08-ribbon-engine-readiness-guard-503/`
- **Branch:** `bug/ribbon-engine-readiness-guard-503`
- **Authored:** 2026-08-08
- **Applies to acceptance criteria:** AC19, AC20, AC21 (all designated **MANUAL-ONLY** in `spec.md`)
- **Companion record:** `evidence/manual-verification/ac19-ac21-checklist.2026-08-08T15-00.md`

This runbook is the procedure. The companion checklist is the record. Follow this runbook, then write the
outcomes into that checklist. This runbook does not replace the checklist and does not change what it asks
for; it supplies the technique the checklist assumes and the decision rules for interpreting what is
observed.

---

## Cue

Act on this runbook when the orchestrator has recorded an `exception` response for the unautomatable
requirement "live-Outlook verification of the engine-readiness ribbon guard" on issue #503.

The exception exists because AC19, AC20, and AC21 require a running Outlook desktop process with a real
mail profile. `.claude/rules/general-unit-test.md` and CLAUDE.md § UT4 prohibit automated tests from
depending on external processes, and `spec.md` § Scope & Non-Goals explicitly excludes "no live Outlook
process, no live mail profile" from every automated test in this change. No Outlook UI-automation harness
exists in this repository. The three criteria are therefore verified by a human against a live profile, or
not at all.

**Binding constraint.** AC19, AC20, and AC21 must **not** be checked off in
`docs/features/active/2026-08-08-ribbon-engine-readiness-guard-503/spec.md` on the strength of unit tests,
source inspection, coverage figures, or any other automated artifact. `spec.md` states this at the head of
its Acceptance Criteria section and again in § Delivery Notes and Deviations → Outstanding; the
`feature-audit.2026-08-08T15-40.md` reviewer recorded the same position and declined to check them. The 27
automated criteria that already pass provide no evidence for these three. The reflection test behind AC8
pins the `EngineCommand_GetEnabled` signature but cannot prove that Office actually bound the callback,
because VSTO compiles a mismatched callback and then does nothing at runtime. Only a live load
demonstrates binding.

Act on this runbook before the pull request for #503 merges. The feature audit's GO recommendation is
conditional on it.

---

## Prerequisites

**Environment**

- A Windows workstation with **Outlook desktop** (the VSTO add-in host) and a **live mail profile** that
  the TaskMaster add-in already loads against. `spec.md` § Context records the target environment as
  Windows 11, .NET Framework 4.8.1.
- A build of the `bug/ribbon-engine-readiness-guard-503` branch **installed and loading in that Outlook
  profile**. Verifying a stale build produces a false result: a build from before this change has no
  `getEnabled` attribute in `RibbonExplorer.xml` and will fail AC21 for reasons unrelated to the current
  code.
- A mail folder containing at least a few messages that can be safely used as a selection for training and
  triage commands (AC20 exercises real engine behavior against whatever is selected).
- Permission to write to the feature folder to record the outcome.

**Knowledge and access**

- The location of the log4net output. `TaskMaster\log4net.config` configures a rolling file appender with
  `<file value="logs\\" />` and `<datePattern value="'debug_'yyyy-MM-dd'.log'" />`, so the file is
  `logs\debug_<yyyy-MM-dd>.log`. `ThisAddIn.EnsureLogDirectoryBeforeConfiguration` (`TaskMaster\ThisAddIn.cs:137-143`)
  resolves `logs` against `Environment.CurrentDirectory` of the **Outlook process**, which is not
  necessarily the add-in's install directory. If the location is unknown, find it once by sorting candidate
  roots for the most recently written `debug_<today>.log` before starting the timed run; do not spend the
  initialization window searching for it.
- A shell able to tail a file. PowerShell `Get-Content -Path <log> -Wait -Tail 40` is sufficient.
- A screen-recording facility. The Windows Game Bar recorder (`Win`+`Alt`+`R`) records the Outlook window
  and produces an artifact that can be reviewed frame by frame. This is the recommended way to capture
  AC21, because the visual state changes without warning and cannot be re-observed after the fact.

**State that must be true before starting**

- The add-in is **not** in Outlook's Disabled Application Add-ins or Inactive Application Add-ins list. If
  it is, re-enable it first (see Step 1) — a soft- or hard-disabled add-in never runs
  `Application_Startup`, so the entire window under test never occurs.
- Outlook is **closed** at the start of Step 3. The recorded run must begin from a full application start.

**Reference facts the verifier needs**

The eight engine-backed controls, all on the "Taskmaster" tab of the Outlook **Explorer** ribbon:

| # | Control id | Label | Engine key | Container |
|---|---|---|---|---|
| 1 | `TrainSpam` | Train Spam | `Spam` | group `SpamBayesGroup` |
| 2 | `TrainHam` | Train Ham | `Spam` | group `SpamBayesGroup` |
| 3 | `TestSpam` | Test Spam | `Spam` | menu `OtherSpamActions` |
| 4 | `TriageSetA` | Set A | `Triage` | group `TriageGroup` |
| 5 | `TriageSetB` | Set B | `Triage` | group `TriageGroup` |
| 6 | `TriageSetC` | Set C | `Triage` | group `TriageGroup` |
| 7 | `ClearTriage` | Clear Triage Field | `Triage` | menu `OtherTriageActions` |
| 8 | `FilterTriageGroup` | Filter | `Triage` | menu `OtherTriageActions` |

Two of the eight (`TestSpam`, and the pair `ClearTriage`/`FilterTriageGroup`) are inside dropdown menus.
The menus themselves are deliberately **not** gated (AC6), so they open normally during initialization and
the gated items appear greyed inside them. Opening a menu is required to observe those items; they are not
visible on the collapsed ribbon.

---

## Step-by-step Instructions

### Step 1 — Confirm the add-in is enabled

1. Start Outlook.
2. Select the **File** tab.
3. Select **Options**.
4. In the categories pane, select **Add-ins**.
5. Confirm the TaskMaster add-in appears under **Active Application Add-ins**. If it appears under
   **Inactive Application Add-ins**, set the **Manage** box to **COM Add-ins**, select **Go**, select the
   check box next to the add-in, and select **OK**. If it appears under **Disabled Application Add-ins**,
   set the **Manage** box to **Disabled Items**, select **Go**, select the add-in, select **Enable**, then
   **Close**.
6. Close Outlook.

### Step 2 — Enable the Office developer option that surfaces add-in UI errors

By default Outlook displays no message when an add-in fails while manipulating the Office UI. Turn the
diagnostic on before the timed run.

1. Start Outlook.
2. Select the **File** tab.
3. Select **Options**.
4. In the categories pane, select **Advanced**.
5. In the details pane, select **Show VSTO Add-in user interface errors**, then select **OK**. In Outlook
   this check box is in the **Developer** section of the details pane, not the **General** section where
   other Office applications place it.
6. Close Outlook.

**What this option does and does not do.** It surfaces errors raised while the add-in manipulates the
ribbon — for example an exception thrown out of `EngineCommand_GetEnabled`. It does **not** report a
callback signature mismatch: a mismatched callback compiles and simply never runs, which produces no error
to display. Absence of an error dialog is therefore not evidence that the callback is bound. Step 5 is the
only check that establishes binding.

### Step 3 — Start the log tail and the screen recording, then start Outlook

Do these in order. The tail and the recording must be running before Outlook starts, because the window
under test opens within the first seconds of `Application_Startup`.

1. In a shell, tail today's log:

   ```powershell
   Get-Content -Path '<resolved-path>\logs\debug_2026-08-08.log' -Wait -Tail 40
   ```

   If the file for today does not exist yet, start the tail immediately after Outlook launches and accept
   the first few lines being missed; the markers that matter arrive later.
2. Start the Game Bar recorder (`Win`+`Alt`+`R`) once the Outlook window appears, or start a full-desktop
   recording before launching Outlook.
3. Start Outlook and select the **Taskmaster** tab on the Explorer ribbon as soon as it renders.

### Step 4 — Locate the initialization window using the log markers

The window is not as short as it appears, and it does not end where a first-time verifier expects. Use
these log lines as its exact boundaries. All are emitted through the same log4net logger and appear in
`debug_<date>.log`.

| Marker (substring to watch for) | Emitted at | Meaning for this verification |
|---|---|---|
| `Application_Startup() fired` | `ThisAddIn.cs:58` | Add-in startup has begun. The ribbon may not have rendered yet. |
| `[engine-init-config] configMs=` | `EngineInitTimingProbe.EmitConfigTiming`, called from `AppItemEngines.cs:52` | The `await Globals.AF.Manager.Configuration` inside `InitAsync()` has just returned. Everything before this line is the longest single sub-window and the `configMs=` value tells you how long it lasted. |
| `[engine-init] engineName=Spam engineMs=` | `EngineInitTimingProbe.TimeEngineAsync` | The `Spam` engine finished constructing. `engineNull=True` on this line means the engine was filtered out or its factory returned null — see the note below. |
| `[engine-init] engineName=Triage engineMs=` | same | The `Triage` engine finished constructing. |
| `[phase-net] phase=Engines` | `StartupDiagnosticsProbe`, from `ApplicationGlobals.cs:219` | `InitAsync()` has completed and `InboxEngines` is populated. Engine readiness has flipped from S0/S1 to S2. **This is the boundary AC19 refers to:** clicks recorded for AC19 must land before this line. |
| `[phase-net] phase=Events` | `ApplicationGlobals.cs:226` | The last startup phase completed; `LoadAsync` is about to return. |
| `Finished loading globals` | `ThisAddIn.cs:84` | Emitted **immediately after** `_ribbonController.RefreshEngineCommands()` (`ThisAddIn.cs:82`). This is the definitive marker that the `InvalidateControl` refresh has been issued. |

**The single most useful fact for planning the run.** The buttons do not re-enable when `InitAsync()`
finishes. They re-enable when the refresh fires, which is after the whole `LoadSequentialAsync` sequence —
`OlObjects`, `ToDo`, `AutoFile`, `Engines`, `Events` — has completed. The `Engines` phase is fourth of
five. The visually-disabled window therefore spans the entire startup load, not just engine construction,
and is materially longer than the `[engine-init]` timings alone suggest. A verifier who assumes the window
ends at `[phase-net] phase=Engines` will conclude the buttons are stuck; a verifier who assumes the window
is only a few hundred milliseconds will report that the buttons are never disabled. Both conclusions are
wrong. Use `Finished loading globals` as the end boundary for the visual state and
`[phase-net] phase=Engines` as the end boundary for engine readiness.

**Lengthening the window if it is still too short to observe.** Use these before resorting to anything
invasive:

- Run the verification on a **cold profile** — the first Outlook launch after a machine restart. The
  configuration deserialize and the SpamBayes deserialize both read from disk without OS file-cache
  assistance. Compare `configMs=` and `engineMs=` against a warm run to confirm the effect.
- Point the engine save location at the **network path** using the ribbon's save-location commands in the
  `OtherSpamActions` / `OtherTriageActions` menus. Those commands are among the callbacks verified
  race-safe and are deliberately left enabled during initialization (AC6), so they remain usable. Loading
  engine data over the network lengthens the `Engines` phase measurably.
- Review the **screen recording** frame by frame rather than trying to observe in real time. The recording
  is the primary instrument for AC21; live observation is a convenience.

Do not pause the process in a debugger to hold the window open for the visual check. A paused process does
not pump messages, so the ribbon does not repaint and the greyed/enabled state cannot be observed
faithfully.

**If a `[engine-init]` line reports `engineNull=True`.** That engine was configured off or its factory
returned null, so its key never enters `InboxEngines`. Its controls will be **permanently** disabled for
the session, not merely disabled during initialization. This is state S3 in `spec.md`, and it is correct
behavior, not a defect — but it means AC20 cannot be exercised for that engine's controls in this session.
Re-enable the engine in configuration and repeat the run rather than recording a failure.

### Step 5 — Observe the ribbon during initialization (AC21, first half)

Between `Application_Startup() fired` and `Finished loading globals`, with the **Taskmaster** tab
selected:

1. Observe `TrainSpam`, `TrainHam`, `TriageSetA`, `TriageSetB`, and `TriageSetC` on the ribbon surface.
2. Open the **Other Spam Actions** menu and observe `TestSpam`.
3. Open the **Other Triage Actions** menu and observe `FilterTriageGroup` and `ClearTriage`.
4. Confirm that the non-engine commands in those same menus — save-location, folder-settings, and
   enable-toggle commands — and the menus themselves remain **enabled**. Over-disabling is a failure as
   surely as under-disabling.

Record which controls rendered greyed. The screen recording is the evidence; the checklist Step 2 table is
the record.

### Step 6 — Click each of the eight commands during initialization (AC19)

Still before `[phase-net] phase=Engines` appears in the log, attempt to click each of the eight controls.
Then classify what happened using the decision rules in the Verification section below. Three outcomes are
possible and they look similar to a user; distinguishing them is the substance of this step.

For each control, note:

- whether the control accepted the click at all,
- whether a message box appeared and what it said,
- what, if anything, was written to the log at that moment.

Attempt all eight even if the first is greyed. The eight are not interchangeable: `TestSpam` uses a
dictionary indexer rather than `TryGetValue`, so its pre-fix failure mode is `KeyNotFoundException` rather
than `NullReferenceException`, and it is the one control whose regression would show a different exception
type.

### Step 7 — Confirm the buttons re-enable without an add-in restart (AC21, second half)

1. Wait for `Finished loading globals` in the log tail. Do **not** restart Outlook, do not reload the
   add-in, and do not switch profiles.
2. Re-observe all eight controls, opening the two menus again.
3. Confirm each of the eight is now **enabled**.

This is the step that proves two things nothing in the repository can prove locally: that the
post-initialization `IRibbonUI.InvalidateControl` refresh actually fired, and that
`EngineCommand_GetEnabled` is genuinely bound to the eight buttons. Office caches each callback's response
per control until the add-in invalidates it, so a button that becomes enabled here can only have done so
because the callback was queried again and returned `true`.

If the buttons were never greyed in Step 5 **and** are enabled here, that is not a pass — it is consistent
with the callback never having been bound at all. Read the Verification section before recording an
outcome.

### Step 8 — Exercise each command after initialization (AC20)

With initialization complete and the buttons enabled, select a suitable mail item or items and run each of
the eight commands. Confirm each behaves exactly as it did before this change:

| # | Control | Expected behavior |
|---|---|---|
| 1 | `TrainSpam` | Trains the selection as spam (`SB.TrainAsync(OlSelection, true)`) |
| 2 | `TrainHam` | Trains the selection as ham (`SB.TrainAsync(OlSelection, false)`) |
| 3 | `TestSpam` | Runs the spam test over the selection |
| 4 | `TriageSetA` | Trains the selection into Triage set "A" |
| 5 | `TriageSetB` | Trains the selection into Triage set "B" |
| 6 | `TriageSetC` | Trains the selection into Triage set "C" |
| 7 | `ClearTriage` | Untrains the selection |
| 8 | `FilterTriageGroup` | Applies the Triage filter view |

An exception thrown by a command on this path is **not** suppressed by the guard and is not expected to
be: `EngineGatedCommandRunner.RunAsync` suppresses invocation when the engine is absent, never errors when
the engine is present (AC14). An exception here is a genuine regression in the command, not a guard
defect, and must be recorded as an AC20 failure.

### Step 9 — Restore the environment and record the outcome

1. Turn off **Show VSTO Add-in user interface errors** (Step 2 in reverse) if it was off before this run.
2. Restore the engine save location if Step 4 changed it.
3. Save the screen recording and a copy of the relevant `debug_<date>.log` excerpt — from
   `Application_Startup() fired` through `Finished loading globals` — into
   `docs/features/active/2026-08-08-ribbon-engine-readiness-guard-503/evidence/manual-verification/`.
4. Fill in the record as described in the Verification section.

---

## Verification

### Decision rule — the three outcomes of a click during initialization

A click on an engine-backed command during initialization can end in exactly three ways. They are easy to
confuse and they mean different things.

| Outcome | What the verifier sees | What the log shows | What it means |
|---|---|---|---|
| **A — disabled by `getEnabled`** | The control is greyed. The click does nothing; the ribbon does not respond and no dialog appears. | Nothing new is written at the moment of the click. | **Correct, and the strongest result.** `EngineCommand_GetEnabled` returned `false` and Office suppressed the click before it reached the add-in. |
| **B — enabled, but suppressed by the click guard** | The control looks normal and accepts the click. A modal message box appears reading `The command '<controlId>' is still loading because its engine '<engineKey>' is not available yet. Please try again once initialization completes.` | One `WARN`-level line containing the same text (`RibbonController.NotifyEngineCommandNotReady`, `RibbonController.EngineCommands.cs:94-98`). | **Correct fallback, but a signal.** The defense-in-depth guard worked and no exception occurred, so AC19 is satisfied for that control. However, the control should not have been clickable. This is what will be observed if `getEnabled` never bound, or if the cached response was stale. Record it, and treat AC21's first half as failing for that control. |
| **C — the original defect** | The control accepts the click and either nothing visible happens or Outlook becomes unstable. | A `NullReferenceException` stack (or, for `TestSpam`, a `KeyNotFoundException`) naming the corresponding `*_Click` handler. | **Failure.** This is the #503 defect. The fix did not take effect in the build under test. Verify you are running a branch build before concluding the fix is wrong. |

Outcome A is the expected result for all eight controls when the fix is working as designed. A consequence
worth stating plainly, because it will otherwise look like a discrepancy against the companion checklist:
**when outcome A holds, the "still loading" message box does not appear, and cannot appear.** Office does
not dispatch the action for a disabled control, so the click never reaches
`EngineGatedCommandRunner.RunAsync` and the notification is never emitted. That is the correct and intended
behavior, not a missing indication.

The checklist's Step 1 column "'Still loading' indication shown?" is therefore satisfied by either of two
recordings:

- `N/A — outcome A (control disabled, click not dispatched)`, or
- `Yes — outcome B (message box observed, text recorded)`.

Record whichever was actually observed, per control, and put the classification in the checklist's Notes
field. Do not force a "Yes" for a control that was greyed, and do not record a failure for a control whose
click produced no indication because it was correctly disabled.

### Pass conditions per criterion

**AC19 passes** when, for all eight controls, the click during initialization produced outcome A or
outcome B, and no `NullReferenceException` and no `KeyNotFoundException` appears in the log for any of the
eight `*_Click` handlers during the window. Outcome C for any control fails AC19.

**AC20 passes** when, after `Finished loading globals`, all eight controls are enabled and each performs
its documented behavior with no exception and no change from the pre-change behavior. A control that
remains disabled after the refresh cannot be exercised and fails AC20 as well as AC21.

**AC21 passes** when both halves hold: all eight controls rendered greyed during the window (Step 5,
outcome A), **and** all eight became enabled after `Finished loading globals` without an add-in restart
(Step 7). Either half failing fails AC21. Widespread outcome B in Step 6 is direct evidence that the first
half failed.

Additionally, for AC21 to be meaningful, the non-engine commands in the `OtherSpamActions` and
`OtherTriageActions` menus must have remained enabled throughout. If those were also greyed, the wiring
over-disabled the UI and AC21 fails even though the eight behaved correctly.

### Where the outcome is recorded

1. **Primary record —** `docs/features/active/2026-08-08-ribbon-engine-readiness-guard-503/evidence/manual-verification/ac19-ac21-checklist.2026-08-08T15-00.md`.
   Fill in every cell of the four tables, the per-step PASS / FAIL / NOT RUN line, the "Executed by" and
   "Date" fields, and the Notes fields. Change the header `Status: PENDING MAINTAINER EXECUTION` to the
   executed status. That file's step-to-criterion mapping is authoritative: Step 1 → AC19, Step 2 → AC21
   (disable), Step 3 → AC21 (re-enable), Step 4 → AC20.
2. **Supporting evidence —** the screen recording and the log excerpt, saved alongside the checklist under
   `evidence/manual-verification/`.
3. **Acceptance criteria —** in
   `docs/features/active/2026-08-08-ribbon-engine-readiness-guard-503/spec.md`, change `- [ ]` to `- [x]`
   **only** for a criterion whose corresponding checklist steps all recorded PASS. Leave any criterion with
   a FAIL or NOT RUN step as `- [ ]`. Checking a criterion without a recorded live-Outlook PASS is a false
   attestation and is prohibited by `spec.md` and by the feature audit.
4. **Runbook reference —** the orchestrator records this runbook's path in
   `artifacts/orchestration/orchestrator-state.json` under
   `human_interaction.requirements[].runbook_path`. This runbook does not modify the checkpoint; report
   completion to the orchestrator rather than editing the checkpoint by hand.

### What a recorded failure triggers

A FAIL is a defect in delivered work, not a documentation gap. Do not adjust the criterion text, the
checklist, or this runbook to accommodate it.

1. Record the failure in the checklist with the observed outcome class (A / B / C), the control ids
   affected, and the verbatim log lines.
2. Leave the affected criterion unchecked in `spec.md`.
3. **Do not merge the pull request for #503.** The feature audit's GO recommendation
   (`feature-audit.2026-08-08T15-40.md`) is explicitly conditional on this verification passing against a
   live profile before merge.
4. Route the failure back into the delivery workflow rather than fixing it ad hoc: report the outcome to
   the orchestrator, which opens a remediation cycle and hands off to the atomic planner per
   `.claude/skills/remediation-handoff-atomic-planner/SKILL.md`. Per the repository's bugfix workflow, the
   remediation begins with the smallest deterministic regression test that can be written without a live
   Outlook process, and only then the targeted fix.
5. Failure-mode triage, to speed the handoff:
   - **Outcome B on all eight, buttons never greyed** — `getEnabled` is not bound. Check that the build
     under test is from the branch, that `RibbonExplorer.xml` in the loaded assembly carries
     `getEnabled="EngineCommand_GetEnabled"` on the eight buttons, and that `EngineCommand_GetEnabled` is a
     `public` instance method returning `bool` with one `Office.IRibbonControl` parameter. A mismatch
     compiles silently.
   - **Buttons greyed and still greyed after `Finished loading globals`** — the invalidation did not take
     effect. Check `RefreshEngineCommands` at `ThisAddIn.cs:82` and the STA marshalling in
     `RibbonViewer.InvalidateEngineCommands`. `IRibbonUI` must be called back on the STA it was handed to.
   - **Outcome C on any control** — the guard did not take effect for that handler. Check that the
     handler routes through `Controller.RunEngineCommandAsync` with the engine dereference inside the
     lambda.
   - **Non-engine menu commands also greyed** — over-disabling. Check that no `menu`, `group`, or `tab`
     element acquired the `getEnabled` attribute.

---

## Source and Citation

**Sourcing-rule note (MCP-first / web-second).** `.claude/skills/human-exception-runbook/SKILL.md` requires
third-party UI steps to be sourced MCP-first, web-second. No MCP documentation-retrieval tool is callable
in this repository; a repository-wide check found no `mcp__*` documentation tool wired as a dependency,
re-verified 2026-08-08. This limitation is recorded as out of scope in the two-axis-model-selection spec
and is not resolved here. Every third-party UI step below is therefore sourced **web-second** from the
vendor's current published documentation, retrieved 2026-08-08, with the page's own `updated_at` recorded.
Training data was not used as a sole source for any step.

**Third-party UI navigation**

- Step 1 (re-enabling a disabled add-in; **File** > **Options** > **Add-ins** > **Manage** box >
  **COM Add-ins** / **Disabled Items** > **Go**): Microsoft Learn — "Re-enable a VSTO Add-in that has been
  disabled." <https://learn.microsoft.com/en-us/visualstudio/vsto/how-to-re-enable-a-vsto-add-in-that-has-been-disabled>
  — updated_at: 2026-04-24. Retrieved 2026-08-08.
- Step 2 (**File** > **Options** > **Advanced** > **Show VSTO Add-in user interface errors**, located in
  the **Developer** section for Outlook): Microsoft Learn — "Show Add-in user interface errors."
  <https://learn.microsoft.com/en-us/visualstudio/vsto/how-to-show-add-in-user-interface-errors> —
  updated_at: 2026-04-24. Retrieved 2026-08-08. The Outlook-specific placement of the check box is stated
  verbatim in that page's note.

**Office ribbon callback and invalidation mechanics**

- Callback caching and `InvalidateControl` semantics (the basis for Step 4's "the buttons re-enable only
  after the refresh fires" and for Step 7 being the proof of binding): Microsoft Learn — "IRibbonUI.InvalidateControl
  method (Office)." <https://learn.microsoft.com/en-us/office/vba/api/office.iribbonui.invalidatecontrol>
  — updated_at: 2024-02-01. Retrieved 2026-08-08. Quoted: "For each of the callbacks that the add-in
  implements, the responses are cached … This process remains in place for the control until the add-in
  signals that the cached values are invalid by using the **InvalidateControl** method, at which time, the
  callback procedure is again called and the return response is cached."
- Silent signature-mismatch behavior (the basis for Step 2's caveat and for Step 7 being the only proof of
  binding): Microsoft Learn — "Customize a project ribbon with Ribbon (XML) item."
  <https://learn.microsoft.com/en-us/visualstudio/vsto/ribbon-xml> — updated_at: 2026-04-24. Retrieved
  2026-08-08. Quoted: "If you create a callback method that does not match a valid signature, the code will
  compile, but nothing will occur when the user clicks the control." The same page states the callback must
  be declared `public` and its name must match the XML attribute value.
- `getEnabled` C# callback signature `bool GetEnabled(IRibbonControl control)`, and the statement that
  callback ordering cannot be predicted or controlled: Microsoft Learn (archived) — "Customizing the 2007
  Office Fluent Ribbon for Developers (Part 3 of 3)."
  <https://learn.microsoft.com/en-us/previous-versions/office/developer/office-2007/aa722523(v=office.12)>
  — updated_at: 2018-04-13. Retrieved 2026-08-08.

**Repository sources (in-repo, no URL)**

- Acceptance criteria AC19, AC20, AC21 and the MANUAL-ONLY designation:
  `docs/features/active/2026-08-08-ribbon-engine-readiness-guard-503/spec.md` § Acceptance Criteria,
  § Test Strategy → Manual validation, and § Delivery Notes and Deviations → Outstanding. Dated 2026-08-08.
- Companion record and step-to-criterion mapping:
  `.../evidence/manual-verification/ac19-ac21-checklist.2026-08-08T15-00.md`. Dated 2026-08-08T15-00.
- Residual-risk assessment and the conditional GO: `.../feature-audit.2026-08-08T15-40.md`. Dated
  2026-08-08T15-40.
- Office ribbon `getEnabled` / invalidation analysis as applied to this codebase:
  `.../research/2026-08-08T12-45-ribbon-engine-readiness-guard-research.md` § 1.8 and § 2. Dated
  2026-08-08T12-45.
- Log markers and their emission sites, read from the branch working tree on 2026-08-08:
  `TaskMaster\ThisAddIn.cs:58,76-88,137-143`; `TaskMaster\AppGlobals\AppItemEngines.cs:40-86`;
  `TaskMaster\AppGlobals\EngineInitTimingProbe.cs:86-108`;
  `TaskMaster\AppGlobals\StartupDiagnosticsProbe.cs:244-258`;
  `TaskMaster\AppGlobals\ApplicationGlobals.cs:190-232`.
- "Still loading" message text and its presentation:
  `TaskMaster\Ribbon\EngineGatedCommandRunner.cs:104-136` (message construction) and
  `TaskMaster\Ribbon\RibbonController.EngineCommands.cs:94-98` (`logger.Warn` plus `MessageBox.Show`).
  Read 2026-08-08.
- Log file naming and appender configuration: `TaskMaster\log4net.config`. Read 2026-08-08.
- Prohibition on automated tests depending on external processes: `.claude/rules/general-unit-test.md`
  § External Dependencies; `CLAUDE.md` § UT4.
- Remediation handoff path on failure: `.claude/skills/remediation-handoff-atomic-planner/SKILL.md`;
  `CLAUDE.md` § Bugfix Workflow.
