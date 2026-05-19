# 2026-03-27-quickfiler-navigation-key-collision-111 (Plan)

- **Issue:** #111
- **Owner:** drmoisan
- **Last Updated:** 2026-03-27
- **Status:** Phase 2 QA complete; awaiting refreshed audit
- **Work Mode:** minor-audit
- **Directive:** MINIMAL-AUDIT PLAN REQUIRED
- **Requirements Source:** `docs/features/active/2026-03-27-quickfiler-navigation-key-collision-111/issue.md` is the sole requirements source for this plan.
- **Plan Path Continuity:** Update only `docs/features/active/2026-03-27-quickfiler-navigation-key-collision-111/plan.2026-03-27T12-45.md`; do not create sibling plan files.
- **Scope Guardrails:** Limit production edits to `QuickFiler/Controllers/KbdActions.cs` and, only if required for navigation-registration compatibility, `QuickFiler/Controllers/QfcCollectionController.cs`. Limit test edits to `QuickFiler.Test/Controllers/KbdActionsTests.cs`.
- **Bug Context Captured for Execution:** The reported failure is `System.ArgumentException: Cannot add key because it already exists. Key 1 SourceId Collection` from `QuickFiler.Controllers.KbdActions.Add` during `QfcCollectionController.RegisterNavigationAsyncAction`, `RegisterNavigation`, or `RemovedItemMonitor`. Investigation has already confirmed that `KaStringAsync.KeyEquals` uses substring matching via `Key.Contains(other)`, so storage operations in `KbdActions` must preserve keyboard-input matching behavior while no longer treating distinct stored keys such as `"1"`, `"01"`, and `"10"` as identical.
- **Change-Plan Note:** This in-place plan update is the required documented change plan before any code edits begin.

### Phase 0 — Baseline Capture

- [x] [P0-T1] Verify the minor-audit requirements boundary before execution begins.
  - Acceptance: The feature folder contains `issue.md` and `plan.2026-03-27T12-45.md`; `spec.md`, `user-story.md`, and `research.md` are absent; this plan header states `Work Mode: minor-audit` and identifies `issue.md` as the sole requirements source.

- [x] [P0-T2] Record policy-read evidence in `docs/features/active/2026-03-27-quickfiler-navigation-key-collision-111/evidence/baseline/phase0-instructions-read.{yyyy-MM-ddTHH-mm}.md`.
  - Acceptance: The artifact exists and contains `Timestamp:`, `Policy Order:`, and an explicit read list that includes `.github/copilot-instructions.md`, `.github/instructions/general-code-change.instructions.md`, `.github/instructions/general-unit-test.instructions.md`, `.github/instructions/csharp-code-change.instructions.md`, and `.github/instructions/csharp-unit-test.instructions.md`.

- [x] [P0-T3] Capture the C# formatting baseline with the exact plan command `dotnet tool run csharpier .` and save the result in `docs/features/active/2026-03-27-quickfiler-navigation-key-collision-111/evidence/baseline/p0-t3-format.{yyyy-MM-ddTHH-mm}.md`.
  - Acceptance: The artifact exists and contains `Timestamp:`, `Command: dotnet tool run csharpier .`, `EXIT_CODE:`, and `Output Summary:` recording the environment-specific command behavior.

- [x] [P0-T4] Capture the analyzer-build baseline with `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform 'Any CPU' -EnableNETAnalyzers -EnforceCodeStyleInBuild` and save the result in `docs/features/active/2026-03-27-quickfiler-navigation-key-collision-111/evidence/baseline/p0-t4-analyzers.{yyyy-MM-ddTHH-mm}.md`.
  - Acceptance: The artifact exists and contains `Timestamp:`, the exact `Command:`, `EXIT_CODE:`, and `Output Summary:`.

- [x] [P0-T5] Capture the nullable/type-check baseline with `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform 'Any CPU' -EnableNullable -TreatWarningsAsErrors` and save the result in `docs/features/active/2026-03-27-quickfiler-navigation-key-collision-111/evidence/baseline/p0-t5-nullable.{yyyy-MM-ddTHH-mm}.md`.
  - Acceptance: The artifact exists and contains `Timestamp:`, the exact `Command:`, `EXIT_CODE:`, and `Output Summary:`.

- [x] [P0-T6] Capture the coverage-enabled test baseline with `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug` and save the result in `docs/features/active/2026-03-27-quickfiler-navigation-key-collision-111/evidence/baseline/p0-t6-tests-with-coverage.{yyyy-MM-ddTHH-mm}.md`.
  - Acceptance: The artifact exists and contains `Timestamp:`, the exact `Command:`, `EXIT_CODE:`, and `Output Summary:` with numeric coverage headline values.

### Phase 1 — Constrained Small-Path Implementation Placeholder

Phase 1 execution note: Regression and verification tests must target `KbdActions` storage identity compatibility with existing keyboard-input matching behavior. Storage must distinguish registered keys such as `"1"`, `"01"`, and `"10"` by exact stored identity, while runtime keyboard matching may continue to flow through `KaStringAsync.KeyEquals`.

- [x] [P1-T1] Use this updated plan as the approved pre-edit change plan and lock the small-path scope before touching code.
  - Acceptance: This file contains the scope guardrails above, explicitly names `QuickFiler/Controllers/KbdActions.cs`, conditionally allows `QuickFiler/Controllers/QfcCollectionController.cs`, and names `QuickFiler.Test/Controllers/KbdActionsTests.cs` as the only planned test file.

- [x] [P1-T2] [expect-fail] Add an MSTest regression in `QuickFiler.Test/Controllers/KbdActionsTests.cs` for `KbdActions<string, KaStringAsync, Func<string, Task>>.Add` proving that `SourceId = "Collection"` can register `"1"` and `"10"` as distinct stored keys.
  - Acceptance: The new test uses MSTest with FluentAssertions; running `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-MSTest.ps1 -SearchRoot QuickFiler.Test -Configuration Debug` produces a non-zero exit code for the pre-fix state; and `docs/features/active/2026-03-27-quickfiler-navigation-key-collision-111/evidence/regression-testing/p1-t2-kbdactions-distinct-keys.{yyyy-MM-ddTHH-mm}.md` exists with `Timestamp:`, the exact `Command:`, `EXIT_CODE:`, and `Output Summary:` that names the intended failing test, including the direct test-platform fallback used when the repository script aborts before dispatch.

- [x] [P1-T3] Add an MSTest scenario in `QuickFiler.Test/Controllers/KbdActionsTests.cs` proving that `KbdActions<string, KaStringAsync, Func<string, Task>>.Add` still throws `ArgumentException` for an exact duplicate `SourceId = "Collection"` and key `"1"`.
  - Acceptance: The test uses MSTest with FluentAssertions and passes after the implementation in [P1-T4].

- [x] [P1-T4] Implement the minimal targeted storage-identity fix in `QuickFiler/Controllers/KbdActions.cs`, touching `QuickFiler/Controllers/QfcCollectionController.cs` only if a navigation-registration compatibility adjustment is required for `RegisterNavigationAsyncAction`, `RegisterNavigation`, or `RemovedItemMonitor`.
  - Acceptance: Duplicate detection in `KbdActions.Add` no longer relies on `KeyEquals`; [P1-T2] and [P1-T3] pass after the fix; no production file outside the scope guardrails is edited.

- [x] [P1-T5] Add an MSTest compatibility scenario in `QuickFiler.Test/Controllers/KbdActionsTests.cs` confirming that stored keys `"1"` and `"10"` can coexist while keyboard-input matching still routes through `KaStringAsync.KeyEquals` for filtering or lookup behavior.
  - Acceptance: The test uses MSTest with FluentAssertions and passes after [P1-T4], demonstrating storage identity compatibility without changing the expected keyboard-input matching contract.

### Phase 2 — Final QC Loop

Phase 2 execution note: Run the commands below in order and restart from [P2-T1] whenever any step changes files or fails. Keep the final clean-pass artifacts under `evidence/qa-gates/`.

- [x] [P2-T1] Run the final formatting pass with the environment-equivalent safe invocation `dotnet tool run csharpier format .` and save the clean-pass result in `docs/features/active/2026-03-27-quickfiler-navigation-key-collision-111/evidence/qa-gates/p2-t1-format.{yyyy-MM-ddTHH-mm}.md`.
  - Acceptance: The artifact exists and contains `Timestamp:`, `Command: dotnet tool run csharpier format .`, `EXIT_CODE: 0`, and `Output Summary:` from the clean pass.

- [x] [P2-T2] Run the final analyzer build with `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform 'Any CPU' -EnableNETAnalyzers -EnforceCodeStyleInBuild` and save the clean-pass result in `docs/features/active/2026-03-27-quickfiler-navigation-key-collision-111/evidence/qa-gates/p2-t2-analyzers.{yyyy-MM-ddTHH-mm}.md`.
  - Acceptance: The artifact exists and contains `Timestamp:`, the exact `Command:`, `EXIT_CODE: 0`, and `Output Summary:` from the clean pass.

- [x] [P2-T3] Run the final nullable/type-check build with `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform 'Any CPU' -EnableNullable -TreatWarningsAsErrors` and save the clean-pass result in `docs/features/active/2026-03-27-quickfiler-navigation-key-collision-111/evidence/qa-gates/p2-t3-nullable.{yyyy-MM-ddTHH-mm}.md`.
  - Acceptance: The artifact exists and contains `Timestamp:`, the exact `Command:`, `EXIT_CODE: 0`, and `Output Summary:` from the clean pass.

- [x] [P2-T4] Run the final coverage-enabled MSTest pass with `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug` and save the clean-pass result in `docs/features/active/2026-03-27-quickfiler-navigation-key-collision-111/evidence/qa-gates/p2-t4-tests-with-coverage.{yyyy-MM-ddTHH-mm}.md`.
  - Acceptance: The artifact exists and contains `Timestamp:`, the exact `Command:`, `EXIT_CODE: 0`, and `Output Summary:` with numeric coverage headline values from the clean pass.
