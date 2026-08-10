# Manual Verification Checklist — AC19, AC20, AC21 (Issue #503)

Timestamp: 2026-08-08T15-00

Status: PENDING MAINTAINER EXECUTION

## Why this checklist exists

AC19, AC20, and AC21 are **MANUAL-ONLY**. They require a running Outlook process, a live mail profile, and physical clicks on the Office ribbon. There is no Outlook UI-automation harness in this repository, and the general unit-test policy prohibits tests that depend on external processes. These three criteria must **never** be checked off in `spec.md` on the strength of unit tests, source inspection, or any automated artifact. They remain `- [ ]` until a maintainer executes the steps below and records the outcome here.

## Scope: the eight engine-backed commands

| # | Ribbon control id | Label | Backing engine |
|---|---|---|---|
| 1 | `TrainSpam` | Train Spam | `Spam` |
| 2 | `TrainHam` | Train Ham | `Spam` |
| 3 | `TestSpam` | Test Spam | `Spam` |
| 4 | `TriageSetA` | Set A | `Triage` |
| 5 | `TriageSetB` | Set B | `Triage` |
| 6 | `TriageSetC` | Set C | `Triage` |
| 7 | `ClearTriage` | Clear Triage Field | `Triage` |
| 8 | `FilterTriageGroup` | Filter | `Triage` |

`TrainSpam` and `TrainHam` are on the `SpamBayesGroup` group. `TestSpam` is inside the `OtherSpamActions` menu. `TriageSetA`/`B`/`C` are on the `TriageGroup` group. `FilterTriageGroup` and `ClearTriage` are inside the `OtherTriageActions` menu. All are on the "Taskmaster" tab of the Outlook Explorer ribbon.

---

## Step 1 — Clicks during initialization produce no exception (satisfies **AC19**)

Reload the TaskMaster add-in (or restart Outlook) so `AppItemEngines.InitAsync()` begins. **Before initialization completes**, click each of the eight engine-backed commands listed above.

For each, confirm:

- No `NullReferenceException` appears in the log.
- No `KeyNotFoundException` appears in the log (this is specifically the `TestSpam` path, which uses a dictionary indexer).
- A "still loading" indication appears, naming the command and its engine.

Log location: the log4net log configured by `TaskMaster\log4net.config`.

| # | Control | No `NullReferenceException`? | No `KeyNotFoundException`? | "Still loading" indication shown? |
|---|---|---|---|---|
| 1 | `TrainSpam` | | | |
| 2 | `TrainHam` | | | |
| 3 | `TestSpam` | | | |
| 4 | `TriageSetA` | | | |
| 5 | `TriageSetB` | | | |
| 6 | `TriageSetC` | | | |
| 7 | `ClearTriage` | | | |
| 8 | `FilterTriageGroup` | | | |

Overall Step 1 outcome (PASS / FAIL / NOT RUN): ______________
Executed by: ______________    Date: ______________
Notes: ______________

---

## Step 2 — Office visually greys the eight buttons during initialization (satisfies **AC21**, first half)

During the same initialization window, before clicking anything, observe the ribbon.

Confirm each of the eight controls renders **disabled** (greyed). Also confirm that controls this fix does not own remain **enabled** — in particular the save-location, folder-settings, and enable-toggle commands inside the `OtherSpamActions` and `OtherTriageActions` menus, and the menus themselves.

| # | Control | Rendered disabled during init? |
|---|---|---|
| 1 | `TrainSpam` | |
| 2 | `TrainHam` | |
| 3 | `TestSpam` | |
| 4 | `TriageSetA` | |
| 5 | `TriageSetB` | |
| 6 | `TriageSetC` | |
| 7 | `ClearTriage` | |
| 8 | `FilterTriageGroup` | |

Non-engine controls in the same menus remained enabled (Yes / No): ______________

Overall Step 2 outcome (PASS / FAIL / NOT RUN): ______________
Executed by: ______________    Date: ______________
Notes: ______________

---

## Step 3 — The eight buttons become enabled after initialization, without an add-in restart (satisfies **AC21**, second half)

Wait for initialization to complete (the log records "Finished loading globals"). Do **not** restart Outlook or reload the add-in.

Confirm each of the eight controls becomes **enabled**.

This step is what proves two things that cannot be observed locally: that the post-initialization `IRibbonUI.InvalidateControl` refresh actually fired, and that the `getEnabled` callback is genuinely bound. VSTO silently ignores a callback signature mismatch — the code compiles and nothing happens — so only a live load can demonstrate binding.

| # | Control | Became enabled without restart? |
|---|---|---|
| 1 | `TrainSpam` | |
| 2 | `TrainHam` | |
| 3 | `TestSpam` | |
| 4 | `TriageSetA` | |
| 5 | `TriageSetB` | |
| 6 | `TriageSetC` | |
| 7 | `ClearTriage` | |
| 8 | `FilterTriageGroup` | |

Overall Step 3 outcome (PASS / FAIL / NOT RUN): ______________
Executed by: ______________    Date: ______________
Notes: ______________

---

## Step 4 — Each command behaves exactly as before once enabled (satisfies **AC20**)

With initialization complete and the buttons enabled, exercise each of the eight commands against a suitable mail selection and confirm the behaviour is identical to the pre-change behaviour.

| # | Control | Expected behaviour | Behaves as before? |
|---|---|---|---|
| 1 | `TrainSpam` | Trains the selection as spam (`SB.TrainAsync(OlSelection, true)`) | |
| 2 | `TrainHam` | Trains the selection as ham (`SB.TrainAsync(OlSelection, false)`) | |
| 3 | `TestSpam` | Runs the spam test over the selection (`SpamBayes.TestAsync(OlSelection)`) | |
| 4 | `TriageSetA` | Trains the selection into Triage set "A" | |
| 5 | `TriageSetB` | Trains the selection into Triage set "B" | |
| 6 | `TriageSetC` | Trains the selection into Triage set "C" | |
| 7 | `ClearTriage` | Untrains the selection (`UnTrainSelectionAsync()`) | |
| 8 | `FilterTriageGroup` | Applies the Triage filter view (`FilterViewAsync()`) | |

Overall Step 4 outcome (PASS / FAIL / NOT RUN): ______________
Executed by: ______________    Date: ______________
Notes: ______________

---

## Acceptance-criteria mapping

| Step | Acceptance criterion |
|---|---|
| Step 1 | **AC19** |
| Step 2 | **AC21** (visual disable during initialization) |
| Step 3 | **AC21** (re-enable after invalidation, without restart) |
| Step 4 | **AC20** |

## Check-off instruction for the maintainer

After executing the steps and recording the outcomes above, check off AC19, AC20, and AC21 in
`docs\features\active\2026-08-08-ribbon-engine-readiness-guard-503\spec.md` **only** for the criteria whose steps recorded PASS. Leave any criterion whose step recorded FAIL or NOT RUN as `- [ ]`, and record the gap.
