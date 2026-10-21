# 2026-08-26-qfc-remove-stackmoveditems-parameter — Plan

- **Issue:** #629
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-09-02T11-00
- **Status:** Draft
- **Version:** 0.1

## Required References

- General Coding Standards: `.claude/rules/general-code-change.md`
- General Unit Test Policy: `.claude/rules/general-unit-test.md`
- C# Code Change Policy and C# Unit Test Policy: `CLAUDE.md`

**All work must comply with these policies; do not duplicate their content here.**

## Implementation Plan (Atomic Tasks)

### Phase 0 — Baseline Capture

- [x] [P0-T1] Read `CLAUDE.md`, `.claude/rules/general-code-change.md`, `.claude/rules/general-unit-test.md`, `.claude/rules/tonality.md`. Record confirmation in `evidence/baseline/phase0-instructions-read.md`.
- [x] [P0-T2] Confirm `git status --porcelain` is clean and record `git rev-parse HEAD` in `evidence/baseline/base-ref.md`.
- [x] [P0-T3] Confirm `.dotnet-sdk` and `packages/` exist in this worktree; bootstrap via `scripts/vscode/Install-RepoDotNetSdk.ps1` and `pwsh -NoProfile -File scripts/vscode/Invoke-Restore.ps1` if absent. Record output in `evidence/baseline/p0-t3-sdk-bootstrap.md`.
- [x] [P0-T4] Run `dotnet tool run csharpier check .`; record EXIT_CODE and any diagnostics in `evidence/baseline/p0-t4-csharpier-check.md`.
- [x] [P0-T5] Run `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`; record EXIT_CODE and warning count in `evidence/baseline/p0-t5-analyzer-build.md`.
- [x] [P0-T6] Run `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`; record EXIT_CODE in `evidence/baseline/p0-t6-nullable-build.md`.
- [x] [P0-T7] Run the full `QuickFiler.Test` suite via `pwsh -NoProfile -File scripts/vscode/Invoke-MSTestWithCoverage.ps1` scoped to `QuickFiler.Test`; record pass/fail counts and the Cobertura headline coverage figure in `evidence/baseline/p0-t7-baseline-coverage.md`.
- [x] [P0-T8] Grep `QuickFiler.Test` for every reference to `MoveEmailsAsync` and `Mock<IQfcCollectionController>`; record the full file:line list in `evidence/baseline/p0-t8-mock-sweep.md`. This is the authoritative set of test call sites this plan must update.

### Phase 1 — Constrained Implementation

- [x] [P1-T1] In `QuickFiler/Interfaces/IQfcCollectionController.cs:63`, remove the `SloStack<IMovedMailInfo> stackMovedItems` (or `StackMovedItems`) parameter from the `MoveEmailsAsync` declaration, leaving `Task MoveEmailsAsync();`. Preserve the existing XML doc comment, updating only the `<param>` tag removal.
- [x] [P1-T2] In `QuickFiler/Controllers/QfcCollectionController.cs:2253-2260`, remove the parameter from the `MoveEmailsAsync` implementation signature and delete the `_ = stackMovedItems;` discard statement. Preserve the method's XML doc comment's statement of how the undo stack is actually populated (via `EmailFiler.PushToUndoStack`); if that statement is not already present, add one sentence recording it.
- [x] [P1-T3] In `QuickFiler/Controllers/QfcFormController.EventHandlers.cs:225`, change `await _groups.MoveEmailsAsync(_movedItems);` to `await _groups.MoveEmailsAsync();`. Do not touch any other line in this file.
- [x] [P1-T4] For each file:line recorded in `evidence/baseline/p0-t8-mock-sweep.md` (P0-T8): update any `Mock<IQfcCollectionController>` `Setup`/`Verify` call that names the old single-parameter overload to the zero-parameter overload.
- [x] [P1-T5] Locate `MoveEmailsAsync_WithNullStack_BehavesIdenticallyToAnEmptyStack` in `QuickFiler.Test/Controllers/QfcCollectionControllerDefects468MoveTests.cs`. Determine whether the behavior it pins (safe operation regardless of the caller's undo-stack argument) is still meaningfully testable post-removal. If not, retire the test with a one-line comment recording why; if the underlying safe-operation behavior can be re-expressed without the removed parameter, rewrite it instead. Record the disposition (retire vs. rewrite) and its one-sentence justification in `evidence/other/p1-t5-test-disposition.md`.
- [x] [P1-T6] Confirm the diff so far touches only the four files named in P1-T1 through P1-T5 (plus this feature folder). Record `git diff --name-only` output in `evidence/other/p1-t6-footprint-check.md`.

### Phase 2 — Final QC Loop

- [x] [P2-T1] Run `dotnet tool run csharpier format .`; record output in `evidence/qa-gates/p2-t1-csharpier-format.md`.
- [x] [P2-T2] Run `dotnet tool run csharpier check .`; record EXIT_CODE 0 in `evidence/qa-gates/p2-t2-csharpier-check.md`. If nonzero, restart from P2-T1.
- [x] [P2-T3] Run the analyzer build (same command as P0-T5); record EXIT_CODE 0 and confirm no new diagnostics relative to the P0-T5 baseline in `evidence/qa-gates/p2-t3-analyzer-build.md`. If nonzero or new diagnostics appear, fix and restart from P2-T1.
- [x] [P2-T4] Run the nullable build (same command as P0-T6); record EXIT_CODE 0 in `evidence/qa-gates/p2-t4-nullable-build.md`. If nonzero, fix and restart from P2-T1.
- [x] [P2-T5] Run the full `QuickFiler.Test` suite with coverage (same command as P0-T7); record pass/fail counts and the Cobertura headline coverage figure in `evidence/qa-gates/p2-t5-final-coverage.md`. Every test must pass; zero regressions permitted. If any fails, fix and restart from P2-T1.
- [x] [P2-T6] Compare the P2-T5 coverage figure against the P0-T7 baseline; record the delta in `evidence/qa-gates/p2-t6-coverage-delta.md`. A same-or-improved line coverage is expected, since dead code (the discard statement) was removed without removing any tested behavior.
- [ ] [P2-T7] Confirm AC1 through AC8 in `spec.md` against the final diff and test run; check off each in `spec.md` with a one-line evidence citation. Record the full acceptance-criteria walkthrough in `evidence/qa-gates/p2-t7-acceptance-summary.md`.
- [ ] [P2-T8] Confirm the final `git diff --name-only origin/main...HEAD` footprint matches exactly: `QuickFiler/Interfaces/IQfcCollectionController.cs`, `QuickFiler/Controllers/QfcCollectionController.cs`, `QuickFiler/Controllers/QfcFormController.EventHandlers.cs`, the test file(s) touched by P1-T4/P1-T5, and this feature folder's own paths. Record in `evidence/qa-gates/p2-t8-final-footprint.md`.
- [ ] [P2-T9] Commit all production, test, and evidence changes with a descriptive message. Record the commit SHA in `evidence/qa-gates/p2-t9-commit.md`.
- [ ] [P2-T10] Confirm `git status --porcelain` is empty after the commit. Record confirmation in `evidence/qa-gates/p2-t10-clean-tree.md`.
