# Atomic Plan — Non-Deterministic CreateAsync Task.Wait Tests (Issue #219)

- Issue: #219
- Work Mode: minor-audit
- AC Source: `docs/features/active/2026-06-28-non-deterministic-createasync-task-wait-tests-219/issue.md` (`## Acceptance Criteria`, AC1–AC4)
- Target file (only file changed): `UtilitiesCS.Test/HelperClasses/QfcTipsDetails_Tests.cs`
- Plan path: `docs/features/active/2026-06-28-non-deterministic-createasync-task-wait-tests-219/plan.2026-06-28T19-42.md`
- Evidence root (canonical, non-overridable): `docs/features/active/2026-06-28-non-deterministic-createasync-task-wait-tests-219/evidence/`

## Scope Lock

- Test-only change. No production code is touched. No new regression test is added.
- Exactly one method changes: `CreateAsync_VisibleLabel_WithMatchingSyncContext_ReturnsOnState()` (currently approx. lines 696–726) in `UtilitiesCS.Test/HelperClasses/QfcTipsDetails_Tests.cs`.
- The sibling `CreateAsync_HiddenLabel_WithMatchingSyncContext_ReturnsInitializedDetails()` is already fixed in the working tree; it is verify-only and MUST NOT be re-edited.
- Test assembly under test: `UtilitiesCS.Test/bin/Debug/UtilitiesCS.Test.dll`.

### Phase 0 — Baseline Capture

- [x] [P0-T1] Read policy files in mandatory order (`CLAUDE.md`, `.claude/rules/general-code-change.md`, `.claude/rules/general-unit-test.md`, `.claude/rules/csharp.md`, `.claude/skills/atomic-plan-contract/SKILL.md`, `.claude/skills/evidence-and-timestamp-conventions/SKILL.md`) and `docs/features/active/2026-06-28-non-deterministic-createasync-task-wait-tests-219/issue.md`. Write `docs/features/active/2026-06-28-non-deterministic-createasync-task-wait-tests-219/evidence/baseline/phase0-instructions-read.md` containing `Timestamp:`, `Policy Order:`, and the explicit list of files read. Acceptance: artifact exists with all three fields populated.
- [x] [P0-T2] Confirm baseline state of `UtilitiesCS.Test/HelperClasses/QfcTipsDetails_Tests.cs`: the VisibleLabel method still uses `task.Wait(TimeSpan.FromSeconds(10))` and the timeout-based `completed` assertion, while the HiddenLabel sibling is already `async Task` with no `Task.Wait`. Write `docs/features/active/2026-06-28-non-deterministic-createasync-task-wait-tests-219/evidence/baseline/target-state.md` with `Timestamp:`, `Command:` (the grep/read used), `EXIT_CODE:`, and `Output Summary:` recording the exact lines that match the forbidden pattern. Acceptance: artifact records the `Task.Wait(TimeSpan)` occurrence on the VisibleLabel method and its absence on the HiddenLabel method.
- [x] [P0-T3] Capture baseline test result for the two target methods by running `vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /Tests:CreateAsync_VisibleLabel_WithMatchingSyncContext_ReturnsOnState,CreateAsync_HiddenLabel_WithMatchingSyncContext_ReturnsInitializedDetails /EnableCodeCoverage`. Write `docs/features/active/2026-06-28-non-deterministic-createasync-task-wait-tests-219/evidence/baseline/baseline-tests.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:` (pass/fail counts and coverage headline for `QfcTipsDetails`). Acceptance: artifact records pre-change pass/fail status and a numeric coverage headline.
- [x] [P0-T4] In docs/features/active/2026-06-28-non-deterministic-createasync-task-wait-tests-219/issue.md, normalize the AC section heading from "## Acceptance Criteria (early draft)" to exactly "## Acceptance Criteria", preserving AC1–AC4 text verbatim. Write docs/features/active/2026-06-28-non-deterministic-createasync-task-wait-tests-219/evidence/baseline/ac-heading-normalized.md with Timestamp:, Command:, EXIT_CODE:, and Output Summary: confirming the exact heading. Acceptance: issue.md contains the exact heading "## Acceptance Criteria" and the four AC items are unchanged.

### Phase 1 — Convert VisibleLabel Test to Awaited async Task

- [x] [P1-T1] In `UtilitiesCS.Test/HelperClasses/QfcTipsDetails_Tests.cs`, change the `CreateAsync_VisibleLabel_WithMatchingSyncContext_ReturnsOnState` signature from `public void` to `public async Task`, preserving the `[TestMethod]` attribute, the XML doc `<summary>`, `Purpose`, and `Side Effects` notes immediately above it. Acceptance: the method declaration reads `public async Task CreateAsync_VisibleLabel_WithMatchingSyncContext_ReturnsOnState()` and the XML doc block above it is unchanged.
- [x] [P1-T2] In the same method, replace `var task = Task.Run(async () => { ... });` with `var details = await Task.Run(async () => { ... });`, preserving verbatim the lambda body: the `Panel`, the `Label { Visible = true }` with its On-branch comment, `panel.Controls.Add(label)`, the `SynchronizationContext` setup/`try`/`finally` reset, and the `return await QfcTipsDetails.CreateAsync(label, ctx, CancellationToken.None);` call inside `Task.Run`. Acceptance: the `Task.Run(async () => ...)` wrapper, the `Visible = true` On-branch intent, and the `SynchronizationContext` setup/reset are all retained, and the result is bound to an awaited `details` local.
- [x] [P1-T3] Remove the `bool completed = task.Wait(TimeSpan.FromSeconds(10));` line and the `completed.Should().BeTrue(...)` timeout assertion, and remove the `task.Exception.Should().BeNull(...)` assertion (exception propagation is now handled by `await`). Replace the `task.Result.Should().NotBeNull(...)` assertion with `details.Should().NotBeNull("CreateAsync must return a details object for a visible label");`. Acceptance: the method contains no `Task.Wait`, no `completed` local, no `task.Exception`, and no `task.Result`; the only end-state assertion is `details.Should().NotBeNull(...)`.
- [x] [P1-T4] Verify by inspection that no other method, no other file, and no production code was modified, and that `CreateAsync_HiddenLabel_WithMatchingSyncContext_ReturnsInitializedDetails` is byte-identical to its pre-change state. Acceptance: `git diff --stat` shows exactly one changed file (`UtilitiesCS.Test/HelperClasses/QfcTipsDetails_Tests.cs`) and the diff hunks are confined to the VisibleLabel method.

### Phase 2 — Final QA Loop

Run the full C# toolchain in order. If any step changes files or fails, fix and restart from P2-T1.

- [x] [P2-T1] Run `dotnet tool run csharpier .` (or `csharpier .`). Write `docs/features/active/2026-06-28-non-deterministic-createasync-task-wait-tests-219/evidence/qa-gates/format.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:`. Acceptance: formatter exits 0 and the artifact records whether any file was reformatted (if reformatted, restart from P2-T1).
- [x] [P2-T2] Run `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`. Write `docs/features/active/2026-06-28-non-deterministic-createasync-task-wait-tests-219/evidence/qa-gates/analyzers.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:`. Acceptance: build succeeds with no analyzer errors (EXIT_CODE 0).
- [x] [P2-T3] Run `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`. Write `docs/features/active/2026-06-28-non-deterministic-createasync-task-wait-tests-219/evidence/qa-gates/nullable.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:`. Acceptance: build succeeds with no nullable/type warnings treated as errors (EXIT_CODE 0).
- [x] [P2-T4] Run `vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /EnableCodeCoverage`. Write `docs/features/active/2026-06-28-non-deterministic-createasync-task-wait-tests-219/evidence/qa-gates/tests.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:` including numeric post-change coverage for the changed lines/`QfcTipsDetails` and explicit pass status for both `CreateAsync_VisibleLabel_WithMatchingSyncContext_ReturnsOnState` and `CreateAsync_HiddenLabel_WithMatchingSyncContext_ReturnsInitializedDetails`. Acceptance: EXIT_CODE 0, both named tests pass, and changed-line coverage does not regress versus the P0-T3 baseline.
- [x] [P2-T5] Check off AC1–AC4 in `docs/features/active/2026-06-28-non-deterministic-createasync-task-wait-tests-219/issue.md` only after their corresponding evidence is recorded (AC1 satisfied by the already-fixed HiddenLabel sibling verified in P2-T4; AC2 by P1-T1–P1-T3; AC3 by P1-T4; AC4 by P2-T1–P2-T4). Acceptance: each checked AC has a corresponding passing evidence artifact, and unmet items remain unchecked.

## Coverage Evidence Contract

- Baseline coverage: captured in `evidence/baseline/baseline-tests.md` (P0-T3).
- Post-change coverage: captured in `evidence/qa-gates/tests.md` (P2-T4).
- No-regression check: P2-T4 acceptance requires changed-line coverage to be no lower than the P0-T3 baseline. If coverage values are unavailable, outcome is remediation-required, not PASS.

## Evidence Location Invariant

All evidence artifacts resolve under `docs/features/active/2026-06-28-non-deterministic-createasync-task-wait-tests-219/evidence/<kind>/`. Any caller-supplied non-canonical path (e.g., `artifacts/baselines/`, `artifacts/qa/`, `artifacts/coverage/`) is rejected and replaced with the canonical path. No non-canonical evidence path was supplied for this plan.
