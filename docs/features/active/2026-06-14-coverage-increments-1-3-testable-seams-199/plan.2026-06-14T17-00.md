# coverage-increments-1-3-testable-seams — Phase 6 Atomic Implementation Plan

- **Issue:** #199
- **Parent plan:** `plan.2026-06-14T08-22.md` (Phases 1–5, committed at `deeda7d0`)
- **Owner:** drmoisan
- **Last Updated:** 2026-06-14T17-00
- **Status:** Draft
- **Version:** 1.0
- **Work Mode:** full-feature

## Scope

Phase 6 closes the single remaining AC1 sub-branch: the `ProjectEntry.ProjectID` property
setter's change-confirmation dialog. The setter currently calls raw
`System.Windows.Forms.MessageBox.Show(...)` rather than `MyBox.ShowDialog(...)`, preventing
the `MyBox.DialogInvoker` seam from suppressing the dialog in tests (verified: EXIT 124 hang).

The maintainer has authorized a third production seam: replace every `MessageBox.Show` call in
the `ProjectID` setter with the equivalent `MyBox.ShowDialog` call, then add the Yes/No
change-confirmation tests to `ProjectEntryDialogBranchesTests.cs`.

## Authoritative Requirements

- `docs/features/active/2026-06-14-coverage-increments-1-3-testable-seams-199/remediation-inputs.2026-06-14T17-00.md`
- `docs/features/active/2026-06-14-coverage-increments-1-3-testable-seams-199/spec.md`
- `docs/features/active/2026-06-14-coverage-increments-1-3-testable-seams-199/evidence/other/p5-projectentry-changeconfirm-gap.2026-06-14T15-10.md`

Canonical issue number: **199**. All artifact file names and cross-references must use this number.

## Hard Constraints (inherited from plan.2026-06-14T08-22.md + this directive)

- Production change is EXACTLY the `ProjectID` setter `MessageBox.Show` → `MyBox.ShowDialog`
  replacement. Preserve exact dialog text, button styles, icons, and `DialogResult.Yes`
  return-value comparisons. No other logic, API, or behavior change.
- If the setter references `System.Windows.Forms` anywhere beyond what the gap-evidence
  documents (lines 40–43, 51–57, 62–68), halt and Flag-and-Stop before touching anything.
- MSTest + Moq + FluentAssertions, AAA, no temp files, no WinForms message loop, no live
  Outlook, deterministic.
- Full C# toolchain green: csharpier → analyzers → nullable → MSTest.
- Do NOT write Cobertura XML into the feature evidence folder; write it to `artifacts/csharp/` only.
- Do NOT touch the existing `plan.2026-06-14T08-22.md` or Phase 1–5 evidence artifacts.

## Flag-and-Stop Rule

If any task reveals a scope boundary violation (unexpected `System.Windows.Forms` usage,
API-contract mismatch, nullable warning in unrelated code, or other out-of-scope
production change required), **halt immediately**, record a flag artifact in
`evidence/other/`, and do not proceed until the maintainer provides direction.

---

### Phase 0 — Pre-flight Baseline Capture

- [x] [P0-T1] Read policy files in required order and write a phase-0 policy-read evidence
  artifact to
  `docs/features/active/2026-06-14-coverage-increments-1-3-testable-seams-199/evidence/baseline/phase0-instructions-read.2026-06-14T17-00.md`.
  Required fields: `Timestamp:`, `Policy Order:`, explicit list of files read (CLAUDE.md,
  `.claude/rules/general-code-change.md`, `.claude/rules/general-unit-test.md`,
  `.claude/rules/csharp.md`, `.claude/skills/atomic-plan-contract/SKILL.md`,
  `.claude/skills/evidence-and-timestamp-conventions/SKILL.md`).
  Acceptance: artifact exists with all required fields.

- [x] [P0-T2] Run csharpier baseline check on branch head `deeda7d0`.
  Command: `dotnet tool run csharpier . --check`
  Write result to
  `docs/features/active/2026-06-14-coverage-increments-1-3-testable-seams-199/evidence/baseline/p6-csharpier-baseline.2026-06-14T17-00.md`.
  Required fields: `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`.
  Acceptance: artifact exists; `EXIT_CODE: 0` (no formatting changes outstanding).

- [x] [P0-T3] Run analyzer/build baseline check on branch head.
  Command: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
  Write result to
  `docs/features/active/2026-06-14-coverage-increments-1-3-testable-seams-199/evidence/baseline/p6-msbuild-analyzers-baseline.2026-06-14T17-00.md`.
  Required fields: `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`.
  Acceptance: artifact exists; `EXIT_CODE: 0` (build succeeds, no analyzer errors).

- [x] [P0-T4] Run nullable/type-check baseline check on branch head.
  Command: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`
  Write result to
  `docs/features/active/2026-06-14-coverage-increments-1-3-testable-seams-199/evidence/baseline/p6-msbuild-nullable-baseline.2026-06-14T17-00.md`.
  Required fields: `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`.
  Acceptance: artifact exists; `EXIT_CODE: 0` (no nullable violations treated as errors).

- [x] [P0-T5] Run MSTest baseline on `ToDoModel.Test` assembly (the assembly this phase modifies)
  with coverage enabled.
  Command: `vstest.console.exe <path-to-ToDoModel.Test.dll> /EnableCodeCoverage`
  Resolve the assembly path from the `Debug` build output (typically
  `ToDoModel.Test\bin\Debug\ToDoModel.Test.dll`).
  Write result to
  `docs/features/active/2026-06-14-coverage-increments-1-3-testable-seams-199/evidence/baseline/p6-mstest-todomodel-baseline.2026-06-14T17-00.md`.
  Required fields: `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` (include pass
  count, fail count, and numeric coverage headline for `ToDoModel` production-only lines).
  Acceptance: artifact exists; `EXIT_CODE: 0`; all existing tests pass; baseline coverage
  value recorded.

---

### Phase 1 — Production Seam: Replace MessageBox.Show in ProjectID Setter

- [x] [P1-T1] Read `ToDoModel/Data Model/Project/ProjectEntry.cs` in full and confirm the
  exact lines containing `MessageBox.Show(...)` in the `ProjectID` setter's
  `else if (_projectID != value)` arm (expected: three call sites at approximately lines 40–43,
  51–57, and 62–68 per the gap-evidence document). Verify no other `System.Windows.Forms`
  references exist in the setter beyond those three calls. If additional unexpected references
  are found, Flag-and-Stop and write an artifact to
  `docs/features/active/2026-06-14-coverage-increments-1-3-testable-seams-199/evidence/other/p6-unexpected-winforms-flag.2026-06-14T17-00.md`.
  Acceptance: all three (and only three) `MessageBox.Show` sites identified; no flag raised.

- [x] [P1-T2] In `ToDoModel/Data Model/Project/ProjectEntry.cs`, replace the first
  `MessageBox.Show(...)` call in the `ProjectID` setter — the malformed-id warning at lines
  ~40–43 — with the equivalent `MyBox.ShowDialog(...)` call.
  Replacement form:
  ```csharp
  MyBox.ShowDialog(
      $"{nameof(ProjectID)} cannot be set with malformed value {value}."
          + "Value should be 4 digits or characters",
      "Dialog",
      MessageBoxButtons.OK,
      MessageBoxIcon.Warning
  );
  ```
  Preserve original dialog text exactly. Preserve the surrounding `if` guard and the absence
  of an assignment (error path does nothing further after the dialog). Do not change any other
  lines in the method.
  Acceptance: file modified; the first `MessageBox.Show` call is replaced with
  `MyBox.ShowDialog`; all other lines are byte-for-byte unchanged.

  > Note: The original call at lines 40–43 passes only `message` (no title, buttons, or icon),
  > which maps to `MessageBox.Show(string)`. The replacement must use the
  > `MyBox.ShowDialog(string message, string title, MessageBoxButtons buttons, MessageBoxIcon icon)`
  > overload that accepts standard `MessageBoxButtons`/`MessageBoxIcon` values. Use
  > `MessageBoxButtons.OK` and `MessageBoxIcon.Warning` (information/warning for a validation
  > message). If the exact icon choice is ambiguous, use `MessageBoxIcon.Warning` and note the
  > decision in the task evidence. Do NOT silently use `MessageBoxIcon.None`.

- [x] [P1-T3] In `ToDoModel/Data Model/Project/ProjectEntry.cs`, replace the second
  `MessageBox.Show(...)` call in the `ProjectID` setter — the primary change-confirmation
  prompt at lines ~51–57 — with the equivalent `MyBox.ShowDialog(...)` call.
  Replacement form (preserve text, buttons, icon, and `== DialogResult.Yes` check):
  ```csharp
  var response = MyBox.ShowDialog(
      $"Are you sure you want to change {nameof(ProjectID)} from"
          + $"{_projectID} to {value}",
      "Dialog",
      MessageBoxButtons.YesNo,
      MessageBoxIcon.Question
  );
  ```
  Do not change the `if (response == DialogResult.Yes)` guard or any other surrounding logic.
  Acceptance: file modified; the second `MessageBox.Show` call is replaced with
  `MyBox.ShowDialog`; the `DialogResult.Yes` comparison is unchanged; all other lines are
  byte-for-byte unchanged.

- [x] [P1-T4] In `ToDoModel/Data Model/Project/ProjectEntry.cs`, replace the third
  `MessageBox.Show(...)` call in the `ProjectID` setter — the `_idUpdate` secondary
  confirmation prompt at lines ~62–68 — with the equivalent `MyBox.ShowDialog(...)` call.
  Replacement form (preserve text, buttons, icon, and `== DialogResult.Yes` check):
  ```csharp
  var response2 = MyBox.ShowDialog(
      "Would you like to change underlying outlook objects, "
          + "child objects, and update ID List?",
      "Dialog",
      MessageBoxButtons.YesNo,
      MessageBoxIcon.Question
  );
  ```
  Do not change the `if (response2 == DialogResult.Yes)` guard or the `_idUpdate.Invoke`
  call.
  Acceptance: file modified; the third `MessageBox.Show` call is replaced with
  `MyBox.ShowDialog`; the `DialogResult.Yes` comparison and `_idUpdate.Invoke` call are
  unchanged; all other lines are byte-for-byte unchanged.

- [x] [P1-T5] Verify that `ToDoModel/Data Model/Project/ProjectEntry.cs` contains zero
  remaining bare `MessageBox.Show(` calls (i.e., grep for `MessageBox.Show` in the file
  returns no matches). Verify that the `using System.Windows.Forms;` directive is still
  present (it is still needed for `DialogResult`, `MessageBoxButtons`, `MessageBoxIcon`).
  Write a brief confirmation note to
  `docs/features/active/2026-06-14-coverage-increments-1-3-testable-seams-199/evidence/other/p6-production-seam-verified.2026-06-14T17-00.md`.
  Acceptance: zero `MessageBox.Show(` occurrences found; `using System.Windows.Forms;` still
  present; confirmation artifact written.

---

### Phase 2 — Test Additions: Change-Confirmation Branches

- [x] [P2-T1] Open `ToDoModel.Test/Data Model/Project/ProjectEntryDialogBranchesTests.cs`.
  Remove the FLAG-AND-STOP comment block (lines ~81–99) that documents the change-confirmation
  branch as blocked. The comment begins with
  `// ---- SetProjectId / ChangeId: change-confirmation branch (FLAG-AND-STOP, not covered) ----`
  and ends with the final line of that block. Replace it with a section heading comment:
  `// ---- ProjectID setter: change-confirmation branch ----`
  Do not alter any other line in the file.
  Acceptance: the FLAG-AND-STOP comment block is removed; the replacement section heading
  comment is in its place; no other lines changed.

- [x] [P2-T2] Add test method `SetProjectId_ChangeConfirmedYes_UpdatesProjectId` to
  `ToDoModel.Test/Data Model/Project/ProjectEntryDialogBranchesTests.cs`.
  Scenario: assign a valid 4-char ID to an entry that already has a valid 4-char ID; inject
  `MyBox.DialogInvoker` returning `DialogResult.Yes` for every invocation; assert `ProjectID`
  equals the new value and no exception is thrown.
  Structure: `[TestMethod]`, AAA, FluentAssertions assertions, descriptive XML doc comment.
  The test must exercise the `ProjectID` setter's `else if (_projectID != value)` arm with
  the seam returning Yes, confirming `_projectID = value` is reached.
  Acceptance: method exists; it is a `[TestMethod]` on a `[STATestClass]` class; AAA
  structure; `ProjectID` is asserted to equal the new value after the call.

- [x] [P2-T3] Add test method `SetProjectId_ChangeConfirmedNo_LeavesProjectIdUnchanged` to
  `ToDoModel.Test/Data Model/Project/ProjectEntryDialogBranchesTests.cs`.
  Scenario: same setup as P2-T2 but inject `MyBox.DialogInvoker` returning `DialogResult.No`;
  assert `ProjectID` is unchanged (equals the original value) and no exception is thrown.
  The test must exercise the `ProjectID` setter's `else if` arm with the seam returning No,
  confirming the assignment is skipped.
  Acceptance: method exists; AAA structure; `ProjectID` is asserted to equal the original
  value (not the new value) after the call.

- [x] [P2-T4] Add test method
  `SetProjectId_ChangeConfirmedYes_WithUpdateAction_InvokesAction` to
  `ToDoModel.Test/Data Model/Project/ProjectEntryDialogBranchesTests.cs`.
  Scenario: create an entry with a valid ID; call `SetIdUpdateAction` with a tracking
  `Action<string, string>` delegate; inject `MyBox.DialogInvoker` returning `DialogResult.Yes`
  for every invocation; assign a different valid 4-char ID via the `ProjectID` setter (directly,
  not via `SetProjectId`); assert `ProjectID` equals the new value AND the action was invoked
  exactly once with the old ID and the new ID as arguments.
  The test exercises the `_idUpdate is not null` arm inside the setter, with both the primary
  and secondary dialog seam returning Yes.
  Acceptance: method exists; AAA structure; `ProjectID` change asserted; action invocation
  count and arguments asserted (using a captured flag/counter or a Moq mock of the action).

- [x] [P2-T5] Add test method
  `SetProjectId_ChangeConfirmedNo_WithUpdateAction_DoesNotInvokeAction` to
  `ToDoModel.Test/Data Model/Project/ProjectEntryDialogBranchesTests.cs`.
  Scenario: same setup as P2-T4 but inject `MyBox.DialogInvoker` returning `DialogResult.No`;
  assert `ProjectID` is unchanged AND the action was NOT invoked.
  Acceptance: method exists; AAA structure; `ProjectID` unchanged asserted; action invocation
  count asserted to be zero.

---

### Phase 3 — Toolchain Loop

Run the full toolchain in the required order. Restart from step 1 if any step fails or
auto-modifies files.

- [x] [P3-T1] Run csharpier formatting.
  Command: `dotnet tool run csharpier .`
  Write result to
  `docs/features/active/2026-06-14-coverage-increments-1-3-testable-seams-199/evidence/qa-gates/p6-csharpier-format.2026-06-14T17-00.md`.
  Required fields: `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`.
  Acceptance: `EXIT_CODE: 0`; no files modified; if files were modified, restart from P3-T1.

- [x] [P3-T2] Run analyzer/build check.
  Command: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
  Write result to
  `docs/features/active/2026-06-14-coverage-increments-1-3-testable-seams-199/evidence/qa-gates/p6-msbuild-analyzers.2026-06-14T17-00.md`.
  Required fields: `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`.
  Acceptance: `EXIT_CODE: 0`; build succeeds with zero errors; no new analyzer warnings
  promoted to errors.

- [x] [P3-T3] Run nullable/type-check build.
  Command: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`
  Write result to
  `docs/features/active/2026-06-14-coverage-increments-1-3-testable-seams-199/evidence/qa-gates/p6-msbuild-nullable.2026-06-14T17-00.md`.
  Required fields: `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`.
  Acceptance: `EXIT_CODE: 0`; no nullable violations treated as errors.

- [x] [P3-T4] Run MSTest with coverage on `ToDoModel.Test` assembly.
  Command: `vstest.console.exe <path-to-ToDoModel.Test.dll> /EnableCodeCoverage`
  Write result to
  `docs/features/active/2026-06-14-coverage-increments-1-3-testable-seams-199/evidence/qa-gates/p6-mstest-todomodel.2026-06-14T17-00.md`.
  Required fields: `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` (include pass
  count, fail count, numeric coverage headline for `ToDoModel` production-only lines, and
  explicit delta versus Phase 0 baseline from P0-T5).
  Acceptance: `EXIT_CODE: 0`; all tests pass (including the four new change-confirmation
  tests from Phase 2); coverage headline is >= Phase 0 baseline value (no regression);
  new/changed lines target >= 90% coverage.

  If any test fails or any earlier step changed files, restart from P3-T1.

---

### Phase 4 — Documentation Updates

- [x] [P4-T1] Update `docs/features/active/2026-06-14-coverage-increments-1-3-testable-seams-199/spec.md`
  **Invariants section**: append a sentence noting the third authorized production seam — the
  `ProjectEntry.ProjectID` setter `MessageBox.Show` → `MyBox.ShowDialog` replacement —
  authorized by the maintainer in the Phase 6 remediation directive
  (`remediation-inputs.2026-06-14T17-00.md`). The sentence must note that the seam preserves
  exact runtime behavior (identical dialog text, buttons, icons, and return-value semantics;
  only the call routing changes).
  Acceptance: `spec.md` Invariants section contains the third seam description; no other
  sections altered.

- [x] [P4-T2] Update `docs/features/active/2026-06-14-coverage-increments-1-3-testable-seams-199/spec.md`
  **Acceptance Criteria — AC1 (Increment 1)**: update the AC1 checkbox entry to reflect that
  the change-confirmation Yes/No sub-branch is now fully covered (no longer a Flag-and-Stop
  residual). Remove or replace the sentence that currently reads "The change-confirmation
  Yes/No sub-branch remains uncovered" with a statement that it is now covered by Phase 6
  (the third authorized production seam + P6 tests). Mark AC1 status as FULLY PASS.
  Acceptance: AC1 entry updated; the old Flag-and-Stop residual sentence is removed or
  superseded; no other AC entries altered.

- [x] [P4-T3] Update `docs/features/active/2026-06-14-coverage-increments-1-3-testable-seams-199/spec.md`
  **Definition of Done**: update the unchecked DoD item "All target seams enumerated in Scope
  are covered with positive/negative/edge/error scenarios" to reflect that the Phase 6
  change-confirmation seam is now covered and check the item off (`[x]`).
  Acceptance: the DoD item is checked; the note about the residual gap is updated to reflect
  Phase 6 closure; no other DoD items altered.

---

### Phase 5 — Final QA Loop

Re-run the full toolchain after all Phase 1–4 changes to confirm a clean final pass.

- [x] [P5-T1] Run csharpier final check.
  Command: `dotnet tool run csharpier . --check`
  Write result to
  `docs/features/active/2026-06-14-coverage-increments-1-3-testable-seams-199/evidence/qa-gates/p6-final-csharpier.2026-06-14T17-00.md`.
  Required fields: `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`.
  Acceptance: `EXIT_CODE: 0`; no formatting differences detected.

- [x] [P5-T2] Run analyzer/build final check.
  Command: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
  Write result to
  `docs/features/active/2026-06-14-coverage-increments-1-3-testable-seams-199/evidence/qa-gates/p6-final-msbuild-analyzers.2026-06-14T17-00.md`.
  Required fields: `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`.
  Acceptance: `EXIT_CODE: 0`; zero errors; zero new analyzer warnings.

- [x] [P5-T3] Run nullable/type-check final check.
  Command: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`
  Write result to
  `docs/features/active/2026-06-14-coverage-increments-1-3-testable-seams-199/evidence/qa-gates/p6-final-msbuild-nullable.2026-06-14T17-00.md`.
  Required fields: `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`.
  Acceptance: `EXIT_CODE: 0`; no nullable violations.

- [x] [P5-T4] Run final MSTest with coverage on `ToDoModel.Test` assembly.
  Command: `vstest.console.exe <path-to-ToDoModel.Test.dll> /EnableCodeCoverage`
  Write result to
  `docs/features/active/2026-06-14-coverage-increments-1-3-testable-seams-199/evidence/qa-gates/p6-final-mstest-todomodel.2026-06-14T17-00.md`.
  Required fields: `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` (pass count, fail
  count, numeric coverage headline, delta versus Phase 0 baseline).
  Acceptance: `EXIT_CODE: 0`; all tests pass; coverage >= Phase 0 baseline; new/changed lines
  >= 90%; artifact written.

  If any step above changed files or reported an error, restart the loop from P5-T1.

---

## Preflight Signal

`DIRECTIVE: PREFLIGHT VALIDATION ONLY`

This plan must pass `mcp__drm-copilot__validate_orchestration_artifacts` with
`artifact_type: "plan"` and `artifact_path: docs/features/active/2026-06-14-coverage-increments-1-3-testable-seams-199/plan.2026-06-14T17-00.md`
before execution begins.

Expected signal: `PREFLIGHT: ALL CLEAR`
