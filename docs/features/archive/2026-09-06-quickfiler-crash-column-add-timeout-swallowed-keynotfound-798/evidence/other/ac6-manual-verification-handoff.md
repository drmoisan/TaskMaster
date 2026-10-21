# AC6 — manual verification handoff

Timestamp: 2026-09-07T05-53
Task: [P9-T6]
Issue: #798

## Status

**PENDING MANUAL.** AC6 is not checked off by this plan and its checkbox in spec.md remains `- [ ]`.

AC6 reads: "Launching QuickFiler on the "T&E" folder either succeeds or shows the AC1/AC3 error
message (manual verification)." It requires a live Outlook session against a specific mailbox folder,
which no automated test in this repository can supply: the repository's own unit-test policy
prohibits tests that depend on external processes, and the column-add path this change modifies is
reached only through `Microsoft.Office.Interop.Outlook` against a real `MAPIFolder`. The criterion is
therefore verifiable only by a person driving the add-in.

## Why no note is written beside the AC6 checkbox

spec.md's Authority blockquote states that AC1 through AC6 are reproduced verbatim from the criteria
settled with the maintainer on 2026-09-06 and "must not be renumbered, reworded, merged, split, or
weakened". The acceptance-criteria-tracking skill independently permits exactly one edit to a
criterion line: changing `- [ ]` to `- [x]`. Annotating the AC6 line would violate both.

The pending status is therefore recorded here and in the P9-T15 acceptance-criteria status summary
instead of beside the criterion.

Verified: the AC6 line in spec.md is byte-identical to its authored text. `git diff` of spec.md
against commit 028e09d5, which introduced the file, shows the AC6 line as an unchanged context line;
the only modifications to the file's acceptance-criteria section are `- [ ]` to `- [x]` transitions
on AC1 through AC5 and AC7.

## Manual steps for the verifier

Perform these in order. Each has an explicit pass condition.

### Preparation

1. Build the solution in **Debug** configuration from this branch,
   `bug/quickfiler-crash-column-add-timeout-swallowed-keynotfound-798`, and register the VSTO add-in
   so Outlook loads this build rather than a previously installed one.
2. Confirm log4net debug-level output is being captured to the add-in's configured log destination
   before launching anything, since steps 3 and 4 both assert on log content. The two `TaskMaster`
   and `QuickFiler` assemblies carry the log4net configurator attributes, so the repository is
   configured in the running host even though it is not in the unit-test process.

### Step 1 — reproduction folder

3. Launch QuickFiler on the **"T&E"** folder, the folder named in the original report.

   **Pass condition — either outcome is acceptable, and exactly one must occur:**
   - QuickFiler launches successfully and lists items; or
   - an error dialog appears whose message names the folder and the failing step. Two message shapes
     satisfy this, corresponding to the two defects this change fixes:
     - the AC1 timeout-exhausted message, which names the folder and the step token `column add`; or
     - the AC3 required-column message, which names the missing column key or keys and the folder.

   **Fail condition:** Outlook surfaces an unhandled exception, or the add-in disappears, hangs, or
   returns to an idle state with no dialog and no listed items. The pre-change behaviour was the
   third of these — a silent return followed by a `KeyNotFoundException` further downstream — so a
   silent no-op is the specific regression this step is checking for.

### Step 2 — regression check on a healthy folder

4. Launch QuickFiler on **Inbox**.

   **Pass condition:** QuickFiler launches successfully, exactly as it did before this change. No
   error dialog appears. This step exists because AC1 converts a previously silent degradation into a
   hard failure, and the total timeout budget was deliberately left at 9000 ms — three deadlines of
   3000 ms — so that no folder which succeeds today begins to fail on timing alone. A dialog here
   would indicate the budget change guard did not hold.

### Step 3 — timing instrumentation, required in both cases

5. After each of the two launches above, inspect the debug log and confirm it contains the per-step
   column-add timing lines that AC2 adds.

   **Pass condition:** the log contains `[Df timing]`-prefixed lines covering the user-defined
   property enumeration and each of the six individual column operations — three `Columns.Add` and
   three `Columns.Remove`. Seven instrumented call sites exist in
   `UtilitiesCS/Extensions/DfDeedle.QfcColumns.cs`, verified by counted search in P3-T6.

   This step is required in **both** the success case and the error-dialog case. The timing lines are
   what make an AC1 failure diagnosable: without them a timeout names the folder but not which column
   operation consumed the budget.

## Recording the outcome

Record the result of each of the three steps, with the observed dialog text where one appeared and
the relevant `[Df timing]` log excerpt. If all pass, AC6 may be checked off in spec.md by changing
`- [ ]` to `- [x]` on that line and nothing else. If any step fails, do not check it off; the failure
should be reported against issue #798.

Output Summary: AC6 is recorded as PENDING MANUAL and its spec.md checkbox is left unchecked and
byte-identical to its authored text. Three manual steps are specified with explicit pass and fail
conditions: launch on the "T&E" reproduction folder, launch on Inbox as a regression check, and
confirm the per-step `[Df timing]` column-add lines in the debug log in both cases.
