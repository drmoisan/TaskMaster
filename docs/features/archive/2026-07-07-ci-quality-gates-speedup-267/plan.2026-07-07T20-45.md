# ci-quality-gates-speedup (Plan)

- **Issue:** #267
- **Issue URL:** https://github.com/drmoisan/TaskMaster/issues/267
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-07-07T22-00
- **Status:** Draft
- **Version:** 1.1
- **Work Mode:** minor-audit
- **Requirements Source:** `docs/features/active/2026-07-07-ci-quality-gates-speedup-267/issue.md` (`## Acceptance Criteria` AC1-AC6; `## Scope Decision (2026-07-07)` note)

**Revision note (Version 1.1):** Revised per the approved scope change recorded in `issue.md`'s `## Scope Decision (2026-07-07)` note (Option A). The two `msbuild /t:Build` passes are RETAINED as two separate steps, each gaining `/m`; the prior plan's build-consolidation branch (Version 1.0, P1-T3) is dropped because local verification proved consolidation is not behavior-neutral — it surfaces 84 pre-existing nullable defects in the vendored projects `SVGControl` and `UtilitiesSwordfish.NET.General` that the current two-pass sequence silently skips via MSBuild's incremental up-to-date short-circuit in the second pass. The discovered gap is tracked separately at `docs/features/potential/2026-07-07-ci-nullable-check-skipped-vendored-projects.md` and is out of scope for this plan. P1-T3 and P1-T4 are reset to unchecked because the current `.github/workflows/ci.yml` on disk reflects the now-dropped consolidated single-pass step and must be reverted to the two-pass form before those tasks can be marked complete. Phase 0 (P0-T1 through P0-T6) is unaffected and remains checked; those baseline captures are reused unchanged by the retained-two-pass verification in the revised Phase 2.

**Fail-closed evidence rule:** This plan includes explicit baseline artifact tasks and final-QC artifact tasks for the CI workflow change. If any required baseline or final-QC artifact is missing, or its required fields (`Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`) are incomplete, the audit verdict must be BLOCKED or INCOMPLETE, never PASS.

**Evidence accounting rule:** Each evidence-producing task names its exact artifact path under `docs/features/active/2026-07-07-ci-quality-gates-speedup-267/evidence/<kind>/`. Do not mark an evidence-backed task complete without the artifact on disk.

## Requirements Boundary

This minor-audit plan uses only `docs/features/active/2026-07-07-ci-quality-gates-speedup-267/issue.md` as the requirements source. Acceptance criteria are limited to the checkbox items (AC1-AC6) under that file's explicit `## Acceptance Criteria` section (confirmed present). `spec.md` and `user-story.md` are not required for minor-audit mode; if either is unexpectedly present in the feature folder, that is a fail-closed condition and must be reported, not silently ignored.

**Sole production file in scope:** `.github/workflows/ci.yml`. No `.cs`, `.csproj`, `packages.config`, `dotnet-tools.json`, or `global.json` file is touched by this plan. Because no C# source or test file changes, C# coverage-comparison tasks (required by `atomic-plan-contract`'s Coverage Evidence Contract "when repository policy requires coverage validation" for an in-scope *language*) do not apply here — the in-scope artifact is CI/YAML configuration. The local `msbuild` invocations — two in Phase 0 (P0-T5, P0-T6) and two more within Phase 2 (P2-T2) — exist solely to empirically verify the diagnostic behavior of the workflow's own two retained build passes (per the explicit verification constraint below), not as a code-change coverage gate.

**Bugfix-workflow applicability:** Issue #267 is a CI performance/efficiency change (removing redundant restores and adding cache steps and `/m`), not a functional defect. The Bugfix Workflow's fail-first regression test requirement (`CLAUDE.md` § Bugfix Workflow) applies only to defects; it is not invoked here. Verification instead takes the form of local reproduction of the modified workflow's own commands (actionlint, the two retained msbuild passes) before the change is trusted.

**Verification constraint (retained-two-pass approach):** Per the Scope Decision (2026-07-07) recorded in `issue.md`, consolidating the two build passes into one is **not behavior-neutral**: local verification during the prior execution attempt proved that the merged single pass surfaces 84 pre-existing nullable defects in the vendored projects `SVGControl` and `UtilitiesSwordfish.NET.General` that the current two-pass sequence silently skips via MSBuild's incremental up-to-date short-circuit on the second pass (the short-circuit behavior is already visible in the baseline capture `csharp-nullable-baseline.2026-07-07T20-45.md`, which recorded "68 Skipping target" lines and a 1.28s pass). Per user decision (Option A), consolidation is dropped; the two original steps are retained verbatim except for the addition of `/m` to each. Phase 0 captures each original pass's diagnostic baseline (P0-T5, P0-T6); Phase 2 re-runs both retained passes in sequence and confirms diagnostic parity against each corresponding baseline, with the only permitted textual difference being the added `/m` flag. The discovered CI nullable-check gap is tracked as a separate follow-up at `docs/features/potential/2026-07-07-ci-nullable-check-skipped-vendored-projects.md`, and is not fixed by this plan.

**AC6 is out-of-band:** AC6 (a green CI run against the branch head, gated by `modified-workflow-needs-green-run`) is satisfied by the orchestrator's post-PR CI-green gate. It is not an executable task in this local plan and is recorded, not checked, in Phase 2.

**Environment note (MSBuild PATH):** `msbuild` is not on the executor's `PATH`. The MSBuild executable is at `C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe`. Every local `msbuild` command task in this plan (P0-T5, P0-T6, and both commands in P2-T2) requires that directory to be added to `PATH` (or the command invoked via its full path) before execution; this is an execution-environment prerequisite, not a change to the commands' properties or flags.

All evidence must be written under `docs/features/active/2026-07-07-ci-quality-gates-speedup-267/evidence/<kind>/`.

## Confirmed Facts (from source inspection, recorded for the Phase 0 investigation task)

- `.github/workflows/ci.yml` `quality-gates` job (lines 38-137) currently runs, in order: Checkout (49-52), Setup .NET SDK (54-57), Setup MSBuild (59-60), Setup NuGet (62-65), Restore solution / `nuget restore` (67-69), Setup CSharpier / `dotnet tool restore` (71-73), Verify formatting / `dotnet csharpier check .` (75-77), Build with analyzers and code style enforcement (79-85), Build with nullable warnings treated as errors (87-93), Run MSTest suite with coverage (95-127), Upload test results (129-137). This is the pre-edit (target-state, post-revert) shape that P1-T3 restores.
- No `actions/cache@v4` step exists in the original pre-edit file; every run performs a full `nuget restore` (17 `packages.config` projects) and a full `dotnet tool restore` (CSharpier from `dotnet-tools.json`). AC1/AC2 cache steps (P1-T1, P1-T2) are unaffected by the Scope Decision and remain implemented.
- Both original `msbuild ... /t:Build` invocations (lines 82-84, 90-92) omit `/m` and run as two separate single-process full-solution passes; the second invocation's changed properties (`Nullable=enable`, `TreatWarningsAsErrors=true`) invalidate incremental state from the first, forcing a full second recompile in a from-scratch CI checkout.
- **Scope Decision discovery:** a prior execution attempt implemented AC4 by merging the two passes into one invocation carrying all four properties. Local verification of that merged pass surfaced 84 pre-existing nullable defects in the vendored projects `SVGControl` and `UtilitiesSwordfish.NET.General`. Those defects are invisible in the current two-pass CI sequence because the second pass's local/CI incremental state causes MSBuild to skip re-analyzing already-built vendored projects ("Skipping target" short-circuit). Per Option A, the merge is reverted; the two passes are retained verbatim with `/m` added to each, and the discovered gap is tracked separately at `docs/features/potential/2026-07-07-ci-nullable-check-skipped-vendored-projects.md`.
- Repo root `dotnet-tools.json` (not under `.config/`) pins `csharpier` version `1.2.6`; `hashFiles('dotnet-tools.json')` is the correct cache-key input.
- 17 `packages.config` files exist under first-party and test project folders (`SVGControl`, `SVGControl.Test`, `Tags`, `Tags.Test`, `TaskMaster`, `TaskMaster.Test`, `TaskTree`, `TaskVisualization`, `TaskVisualization.Test`, `ToDoModel`, `ToDoModel.Test`, `UtilitiesCS`, `UtilitiesCS.Test`, `VBFunctions`, `VBFunctions.Test`, `QuickFiler`, `QuickFiler.Test`); `hashFiles('**/packages.config')` is the correct cache-key input for AC1.
- A local `actionlint.exe` is vendored at `actionlint-bin/actionlint.exe` and wrapped by `scripts/dev-tools/run-actionlint.ps1`, which throws if the executable is missing and propagates `$LASTEXITCODE`. This is the executor's local equivalent of the `actionlint` job's `./actionlint` step (lines 29-36).

---

### Phase 0 — Policy and Baseline Evidence

- [x] [P0-T1] Record policy-read evidence for issue #267 before implementation begins.
  - Files read (in order): `CLAUDE.md`, `.claude/rules/general-code-change.md`, `.claude/rules/general-unit-test.md`, `.claude/rules/csharp.md`, `.claude/rules/ci-workflows.md`, `.claude/skills/atomic-plan-contract/SKILL.md`, `.claude/skills/evidence-and-timestamp-conventions/SKILL.md`, `.claude/skills/acceptance-criteria-tracking/SKILL.md`, `docs/features/active/2026-07-07-ci-quality-gates-speedup-267/issue.md`
  - Evidence: `docs/features/active/2026-07-07-ci-quality-gates-speedup-267/evidence/baseline/phase0-instructions-read.md`
  - Acceptance: Evidence file exists and contains `Timestamp:`, `Policy Order:`, and the explicit list of files read above, in order.

- [x] [P0-T2] Verify the minor-audit requirements boundary for issue #267.
  - Files: `docs/features/active/2026-07-07-ci-quality-gates-speedup-267/issue.md` (and confirm the presence/absence of `spec.md`, `user-story.md` in the same folder)
  - Evidence: `docs/features/active/2026-07-07-ci-quality-gates-speedup-267/evidence/baseline/minor-audit-scope.2026-07-07T20-45.md`
  - Acceptance: Evidence confirms `issue.md` contains `- Work Mode: minor-audit`, contains an explicit `## Acceptance Criteria` section listing AC1-AC6, treats only that section as the AC source, and records whether `spec.md`/`user-story.md` are present or absent in the feature folder (fail-closed if unexpectedly present).

- [x] [P0-T3] Record baseline inventory of the current (pre-edit) `.github/workflows/ci.yml` steps targeted by AC1-AC4, citing exact line numbers.
  - Files: `.github/workflows/ci.yml`
  - Evidence: `docs/features/active/2026-07-07-ci-quality-gates-speedup-267/evidence/baseline/investigation-notes.2026-07-07T20-45.md`
  - Acceptance: Evidence cites, with line numbers from the pre-edit file: (a) the "Setup NuGet" step and "Restore solution" step with no cache step between them; (b) the "Setup CSharpier" step with no cache step before it; (c) both `msbuild /t:Build` invocations, confirming neither carries `/m` and that they run as two separate full-solution passes carrying two of the four target properties each. This artifact is reused unchanged as the byte-for-byte reference text for the retained-two-pass diagnostic-parity check in Phase 2 (P2-T3).

- [x] [P0-T4] Run the baseline actionlint check against the current (unmodified) `.github/workflows/ci.yml`.
  - Files: `.github/workflows/ci.yml`, `scripts/dev-tools/run-actionlint.ps1`
  - Command: `pwsh -File scripts\dev-tools\run-actionlint.ps1`
  - Evidence: `docs/features/active/2026-07-07-ci-quality-gates-speedup-267/evidence/baseline/actionlint-baseline.2026-07-07T20-45.md`
  - Acceptance: Evidence contains `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:` stating pass/fail and any diagnostic count for the unmodified workflow, establishing the pre-edit lint state.

- [x] [P0-T5] Run the baseline analyzer/code-style msbuild pass (pass 1 of the two retained invocations) against `TaskMaster.sln` to record its diagnostic output for later diagnostic-parity comparison.
  - Files: `TaskMaster.sln`
  - Command: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
  - Evidence: `docs/features/active/2026-07-07-ci-quality-gates-speedup-267/evidence/baseline/csharp-analyzers-baseline.2026-07-07T20-45.md`
  - Acceptance: Evidence contains `Timestamp:`, the exact `Command:`, `EXIT_CODE:`, and `Output Summary:` with the warning/error count. Reused as the pass-1 baseline for the Phase 2 retained-two-pass diagnostic-parity check (P2-T3); no re-capture required.

- [x] [P0-T6] Run the baseline nullable/warnings-as-errors msbuild pass (pass 2 of the two retained invocations) against `TaskMaster.sln` to record its diagnostic output for later diagnostic-parity comparison.
  - Files: `TaskMaster.sln`
  - Command: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`
  - Evidence: `docs/features/active/2026-07-07-ci-quality-gates-speedup-267/evidence/baseline/csharp-nullable-baseline.2026-07-07T20-45.md`
  - Acceptance: Evidence contains `Timestamp:`, the exact `Command:`, `EXIT_CODE:`, and `Output Summary:` with the warning/error count. Reused as the pass-2 baseline for the Phase 2 retained-two-pass diagnostic-parity check (P2-T3); no re-capture required.

---

### Phase 1 — Constrained Implementation (Cache Steps, /m, and Retained Two-Pass Build)

- [x] [P1-T1] Add the NuGet package cache step (AC1) before the "Restore solution" step.
  - Files: `.github/workflows/ci.yml`
  - Precondition: Phase 0 complete.
  - Change: Insert a new step immediately after "Setup NuGet" and strictly before "Restore solution":
    ```yaml
          - name: Cache NuGet packages
            uses: actions/cache@v4
            with:
              path: packages
              key: nuget-${{ runner.os }}-${{ hashFiles('**/packages.config') }}
              restore-keys: |
                nuget-${{ runner.os }}-
    ```
  - Acceptance: The new step appears after "Setup NuGet" and strictly before "Restore solution"; the "Restore solution" step (`nuget restore $env:SOLUTION_PATH`) is otherwise unmoved and unconditional (no `if:` guard added), so it still executes on both cache hit and cache miss; `uses: actions/cache@v4`; `path: packages`; `key: nuget-${{ runner.os }}-${{ hashFiles('**/packages.config') }}`; a `restore-keys:` fallback is present. No other step in the file changes. Satisfies AC1.

- [x] [P1-T2] Add the CSharpier tool-restore cache step (AC2) before the "Setup CSharpier" step.
  - Files: `.github/workflows/ci.yml`
  - Precondition: P1-T1 complete.
  - Change: Insert a new step immediately after "Restore solution" and strictly before "Setup CSharpier":
    ```yaml
          - name: Cache dotnet tools
            uses: actions/cache@v4
            with:
              path: ~/.nuget/packages
              key: dotnet-tools-${{ runner.os }}-${{ hashFiles('dotnet-tools.json') }}
              restore-keys: |
                dotnet-tools-${{ runner.os }}-
    ```
  - Acceptance: The new step appears strictly before "Setup CSharpier"; the "Setup CSharpier" step (`dotnet tool restore`) is otherwise unmoved and unconditional, so it still executes on both cache hit and cache miss; `uses: actions/cache@v4`; `path: ~/.nuget/packages`; `key: dotnet-tools-${{ runner.os }}-${{ hashFiles('dotnet-tools.json') }}`; a `restore-keys:` fallback is present. Satisfies AC2.

- [x] [P1-T3] Restore the two original `msbuild /t:Build` steps (reverting the prior consolidation) and add `/m` to each (AC3 and AC4, retained-two-pass branch).
  - Files: `.github/workflows/ci.yml`
  - Precondition: P1-T2 complete. Note: the file currently contains a single merged step ("Build with analyzers, code style, and nullable warnings-as-errors enforcement") from a prior, now-dropped, consolidation attempt; this task reverts that merge.
  - Change: Replace the single merged step with the two original steps below, each carrying only its original properties plus `/m` (properties and step names verbatim from `investigation-notes.2026-07-07T20-45.md`; no property added beyond `/m`, none dropped):
    ```yaml
          - name: Build with analyzers and code style enforcement
            shell: pwsh
            run: |
              & msbuild $env:SOLUTION_PATH /t:Build /m /p:Configuration=Debug `
                  "/p:Platform=Any CPU" `
                  /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true
              if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

          - name: Build with nullable warnings treated as errors
            shell: pwsh
            run: |
              & msbuild $env:SOLUTION_PATH /t:Build /m /p:Configuration=Debug `
                  "/p:Platform=Any CPU" `
                  /p:Nullable=enable /p:TreatWarningsAsErrors=true
              if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
    ```
  - Acceptance: Exactly TWO `msbuild ... /t:Build` invocations exist in the modified file (`grep -c "/t:Build" .github/workflows/ci.yml` equals 2); pass 1 ("Build with analyzers and code style enforcement") carries `/p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` and `/m`, and no other build property; pass 2 ("Build with nullable warnings treated as errors") carries `/p:Nullable=enable /p:TreatWarningsAsErrors=true` and `/m`, and no other build property; each step's `if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }` guard is preserved; no fifth property is merged into either step and no property present in the original pre-edit steps is dropped. Satisfies AC3 and AC4 via the Scope Decision (2026-07-07) "retained as two, with no reduction in enforced diagnostics" branch.

- [x] [P1-T4] Record implementation-scope evidence confirming `.github/workflows/ci.yml` is the sole file changed, reflecting the retained-two-pass state after P1-T3.
  - Files: `.github/workflows/ci.yml`
  - Precondition: P1-T3 complete.
  - Evidence: `docs/features/active/2026-07-07-ci-quality-gates-speedup-267/evidence/regression-testing/implementation-scope.2026-07-07T22-00.md`
  - Acceptance: Evidence records `git diff --stat` output and confirms the only changed file is `.github/workflows/ci.yml`; explicitly confirms no `.cs`, `.csproj`, `packages.config`, `dotnet-tools.json`, or `global.json` file was modified; this artifact supersedes the earlier `implementation-scope.2026-07-07T20-45.md` capture, which recorded the now-reverted consolidated-build diff and no longer reflects the current tree.

---

### Phase 2 — Final QC Loop (Workflow Lint and Local Build Verification, Retained Two-Pass)

- [x] [P2-T1] Run actionlint against the modified `.github/workflows/ci.yml` (AC5).
  - Files: `.github/workflows/ci.yml`, `scripts/dev-tools/run-actionlint.ps1`
  - Command: `pwsh -File scripts\dev-tools\run-actionlint.ps1`
  - Evidence: `docs/features/active/2026-07-07-ci-quality-gates-speedup-267/evidence/qa-gates/actionlint-final.2026-07-07T22-00.md`
  - Acceptance: Evidence contains `Timestamp:`, `Command:`, `EXIT_CODE: 0`, and `Output Summary:` confirming zero actionlint findings on the modified workflow; if this command fails, fix the workflow and restart Phase 2 from P2-T1. Satisfies AC5.

- [x] [P2-T2] Run both retained `msbuild /t:Build` passes locally, in sequence (pass 1 then pass 2), exactly as the modified workflow will run them, from the current working tree, to verify both exit 0. Do NOT run a single consolidated pass and do NOT perform a from-scratch/clean rebuild.
  - Files: `TaskMaster.sln`, `.github/workflows/ci.yml`
  - Command (pass 1): `msbuild TaskMaster.sln /t:Build /m /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
  - Command (pass 2, run after pass 1 completes): `msbuild TaskMaster.sln /t:Build /m /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`
  - Evidence: `docs/features/active/2026-07-07-ci-quality-gates-speedup-267/evidence/qa-gates/csharp-two-pass-build-final.2026-07-07T22-00.md`
  - Acceptance: Evidence contains two distinct entries, one per pass, each with `Timestamp:`, the exact `Command:`, `EXIT_CODE: 0`, and `Output Summary:` with the warning/error count; confirms pass 1 completed before pass 2 started (sequential execution, reproducing current/target CI order); confirms no single invocation carrying all four properties was executed; if either command fails, halt and report — do not weaken any of the four enforced properties across the two passes to force a pass. Satisfies the local-verification half of AC4.

- [x] [P2-T3] Compare each retained pass's final diagnostic output (P2-T2) against its corresponding baseline capture (pass 1 vs. P0-T5, pass 2 vs. P0-T6) to confirm the two retained passes are byte-identical to the original two steps except for the added `/m`, i.e. no enforced diagnostic is dropped and no new enforcement is introduced.
  - Files: `docs/features/active/2026-07-07-ci-quality-gates-speedup-267/evidence/baseline/csharp-analyzers-baseline.2026-07-07T20-45.md`, `docs/features/active/2026-07-07-ci-quality-gates-speedup-267/evidence/baseline/csharp-nullable-baseline.2026-07-07T20-45.md`, `docs/features/active/2026-07-07-ci-quality-gates-speedup-267/evidence/baseline/investigation-notes.2026-07-07T20-45.md`, `docs/features/active/2026-07-07-ci-quality-gates-speedup-267/evidence/qa-gates/csharp-two-pass-build-final.2026-07-07T22-00.md`
  - Evidence: `docs/features/active/2026-07-07-ci-quality-gates-speedup-267/evidence/qa-gates/build-diagnostic-parity.2026-07-07T22-00.md`
  - Acceptance: Evidence states the warning/error counts from all four runs (P0-T5, P0-T6, P2-T2 pass 1, P2-T2 pass 2); confirms pass-1-final matches pass-1-baseline (P0-T5) diagnostic-for-diagnostic and pass-2-final matches pass-2-baseline (P0-T6) diagnostic-for-diagnostic (accounting for the incremental-skip caveat already documented in `csharp-nullable-baseline.2026-07-07T20-45.md`); confirms, by comparison against `investigation-notes.2026-07-07T20-45.md`'s pre-edit step quotations, that the only textual difference between each retained step and its original pre-edit counterpart is the added `/m` flag; confirms no enforced diagnostic is dropped and no new enforcement is introduced; cites the Scope Decision (2026-07-07) in `issue.md` as the basis for retaining two passes instead of consolidating. Satisfies the no-reduction/diagnostic-parity portion of AC4.

- [x] [P2-T4] Confirm both retained msbuild steps in the modified workflow include `/m`.
  - Files: `.github/workflows/ci.yml`
  - Evidence: `docs/features/active/2026-07-07-ci-quality-gates-speedup-267/evidence/qa-gates/parallel-build-flag-check.2026-07-07T22-00.md`
  - Acceptance: Evidence quotes the exact lines containing `/t:Build /m` from both retained steps ("Build with analyzers and code style enforcement" and "Build with nullable warnings treated as errors") in the modified workflow; confirms there are exactly TWO `msbuild ... /t:Build` invocations in the file (not one, not three); confirms both carry `/m`. Satisfies AC3.

- [x] [P2-T5] Record cache-step placement evidence confirming the AC1 and AC2 cache steps precede their respective restore steps and that those restore steps remain unconditional (cache-miss-safe).
  - Files: `.github/workflows/ci.yml`
  - Evidence: `docs/features/active/2026-07-07-ci-quality-gates-speedup-267/evidence/qa-gates/cache-placement-check.2026-07-07T22-00.md`
  - Acceptance: Evidence quotes the final step ordering (step names, in file order) from "Setup NuGet" through "Verify formatting", confirming: "Cache NuGet packages" precedes "Restore solution"; "Cache dotnet tools" precedes "Setup CSharpier"; "Restore solution" and "Setup CSharpier" carry no `if:` guard, so a cache miss still executes a full restore. Satisfies AC1 and AC2.

- [x] [P2-T6] Update issue #267 acceptance-criteria status for AC1-AC5 after verified completion; record AC6 as an out-of-band gate.
  - Files: `docs/features/active/2026-07-07-ci-quality-gates-speedup-267/issue.md`
  - Evidence: `docs/features/active/2026-07-07-ci-quality-gates-speedup-267/evidence/issue-updates/ac-status.2026-07-07T22-00.md`
  - Acceptance: Only AC1-AC5 under `## Acceptance Criteria` in `issue.md` are changed from `[ ]` to `[x]`, each backed by the corresponding Phase 1/Phase 2 evidence named above, with AC4 explicitly recorded as satisfied via the "retained as two, with no reduction in enforced diagnostics" branch of the Scope Decision (2026-07-07); AC6 remains unchecked in `issue.md`, and the evidence explicitly records that AC6 (a green CI run against the branch head) is satisfied by the orchestrator's post-PR `modified-workflow-needs-green-run` gate, not by a local executor task, per `.claude/rules/ci-workflows.md` and this plan's Requirements Boundary. Unchanged text in `issue.md` is preserved.

- [x] [P2-T7] Record final minor-audit readiness evidence for issue #267.
  - Files: `docs/features/active/2026-07-07-ci-quality-gates-speedup-267/plan.2026-07-07T20-45.md`, `docs/features/active/2026-07-07-ci-quality-gates-speedup-267/issue.md`, `docs/features/active/2026-07-07-ci-quality-gates-speedup-267/evidence/baseline/phase0-instructions-read.md`, `docs/features/active/2026-07-07-ci-quality-gates-speedup-267/evidence/regression-testing/implementation-scope.2026-07-07T22-00.md`, `docs/features/active/2026-07-07-ci-quality-gates-speedup-267/evidence/qa-gates/build-diagnostic-parity.2026-07-07T22-00.md`
  - Evidence: `docs/features/active/2026-07-07-ci-quality-gates-speedup-267/evidence/qa-gates/minor-audit-readiness.2026-07-07T22-00.md`
  - Acceptance: Evidence confirms Phase 0 baseline artifacts exist, Phase 1 implementation-scope evidence exists (reflecting the retained-two-pass state), Phase 2 QC artifacts exist, every command-bearing task recorded an executed numeric `EXIT_CODE` (no `SKIPPED`), the modified workflow retains exactly two `msbuild ... /t:Build` invocations, AC1-AC5 are checked off in `issue.md`, AC6 is correctly recorded as out-of-band, and the only production file changed across the whole plan is `.github/workflows/ci.yml`.
