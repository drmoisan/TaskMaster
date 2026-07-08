# Remediation / Scope-Change Inputs — Issue #199 (2026-06-14T17-00)

- Canonical issue number: 199
- Trigger: Maintainer-directed scope change (chose option B). The prior feature-review
  (2026-06-14T16-05) returned GO with 0 blocking findings. AC1 remained PARTIAL (one
  sub-branch flagged-and-stopped). The maintainer has now authorized a third production seam
  to close that remaining AC1 sub-branch.
- Active folder: `docs/features/active/2026-06-14-coverage-increments-1-3-testable-seams-199/`
- Branch: `refactor/coverage-increments-1-3-199`

## Background

Phase 5 introduced two authorized seams and covered the malformed-ID validation branch and
the `CompareTo` length tie-break on `ProjectEntry` (AC1 PARTIAL). One AC1 sub-branch
remains uncovered:

- `ProjectEntry.ProjectID` property setter (lines ~36–77, `ToDoModel/Data Model/Project/ProjectEntry.cs`)
  contains raw `System.Windows.Forms.MessageBox.Show(...)` calls in its `else if (_projectID != value)`
  arm. When a test attempts to commit a changed project ID, the setter fires this raw modal dialog
  on the STA test thread with no message pump; the test host deadlocks (verified: EXIT 124 timeout).
  See evidence: `evidence/other/p5-projectentry-changeconfirm-gap.2026-06-14T15-10.md`.

The `MyBox.DialogInvoker` seam is already injectable from `ToDoModel.Test` (Phase 5 `InternalsVisibleTo`
attribute). The only blocker is the raw `MessageBox.Show` in the setter bypassing that seam.

## Directive

The maintainer has AUTHORIZED a **single, targeted production change** to close the remaining
AC1 change-confirmation sub-branch:

Route all `System.Windows.Forms.MessageBox.Show(...)` calls in the `ProjectID` property setter
through the existing `MyBox.ShowDialog(...)` / `MyBox` seam, matching the pattern already used
in `SetProjectId`/`ChangeId`. This is the third and final production seam for #199.

## Authorized production change

**Production file:** `ToDoModel/Data Model/Project/ProjectEntry.cs`
- In the `ProjectID` property setter's `else if (_projectID != value)` arm, replace every
  `MessageBox.Show(...)` call with the equivalent `MyBox.ShowDialog(...)` call.
- Preserve exact dialog text, button style, and icon. Map the return-value comparison
  (`== DialogResult.Yes`) identically.
- The seam must default to the real dialog in production (unchanged from current behavior).
- No other logic changes. Setter logic, guard conditions, side effects, and assignments
  (`_projectID = value`, `_idUpdate?.Invoke(value)`, etc.) remain identical.

## Required test additions

**Test file:** `ToDoModel.Test/Data Model/Project/ProjectEntryDialogBranchesTests.cs`
(already exists; add new test methods to it)

Add test methods covering the following scenarios (each must be deterministic, no WinForms
message loop):

1. **SetProjectId_ChangeConfirmedYes_UpdatesProjectId** — set a valid ID over an existing
   valid ID; inject `DialogInvoker` returning `DialogResult.Yes`; assert `ProjectID` changes
   to the new value and no exception is thrown.
2. **SetProjectId_ChangeConfirmedNo_LeavesProjectIdUnchanged** — same setup; inject returning
   `DialogResult.No`; assert `ProjectID` is unchanged.
3. **SetProjectId_ChangeConfirmedYes_WithUpdateAction_InvokesAction** — inject `DialogResult.Yes`,
   supply a non-null `_idUpdate` action via `SetProjectId`; assert both the ID changes and
   the action is invoked with the new ID.
4. (Optional but preferred) **SetProjectId_ChangeConfirmedNo_WithUpdateAction_DoesNotInvokeAction** —
   inject `DialogResult.No`; assert the update action is NOT invoked.

Setup pattern (identical to Phase 5's `ProjectEntryDialogBranchesTests.cs` tests):
- `MyBox.DialogInvoker = () => DialogResult.Yes;` / `DialogResult.No;`
- Restore `MyBox.DialogInvoker = null;` in `[TestCleanup]`.

## Constraints

- Production change: EXACTLY the `ProjectID` setter's `MessageBox.Show` → `MyBox.ShowDialog`
  replacement. No logic, API, or behavior change beyond routing the same calls through the seam.
- If the setter references `System.Windows.Forms` anywhere else beyond what is already present,
  flag-and-stop before touching it (scope-change rule).
- Full C# toolchain green: csharpier → analyzers → nullable → MSTest.
- Test framework: MSTest + Moq + FluentAssertions, AAA pattern, no temp files, no WinForms
  message loop, deterministic.
- Update `spec.md` Invariants section to note this third seam is also maintainer-authorized.
- Update `spec.md` AC1 to reflect the change-confirmation sub-branch is now FULLY covered.
- Do NOT touch Phase 5 artifacts (already committed and reviewed).
- Do NOT write full Cobertura coverage XML into the feature evidence folder; write it to
  `artifacts/csharp/` only.

## Acceptance for this cycle

- `ProjectEntry.ProjectID` setter routes all confirmation dialogs through `MyBox.ShowDialog`.
- `ProjectEntryDialogBranchesTests.cs` contains tests covering the change-confirmation Yes and
  No paths (and optionally the `_idUpdate` action invocation).
- All tests pass; the change-confirmation branch is no longer a flag-and-stop residual.
- Toolchain green; AC1 fully PASS.
