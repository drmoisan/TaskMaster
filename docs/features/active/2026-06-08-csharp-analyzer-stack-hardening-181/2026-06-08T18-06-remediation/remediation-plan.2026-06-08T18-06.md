# Remediation Plan: csharp-analyzer-stack-hardening (Issue #181) — Cycle 2

- Cycle entry timestamp: 2026-06-08T18-06
- Feature folder: `docs/features/active/2026-06-08-csharp-analyzer-stack-hardening-181`
- Work mode: `full-feature` (resolved from `issue.md` metadata `- Work Mode: full-feature`)
- Base branch: `main` @ `2a522ed831865c2918ab02df153ef2929b0617dc`
- Head branch: `feature/csharp-analyzer-stack-181` @ `71e0777ada475c408d85d3b6c68e6192b4bc070b`
- PR: https://github.com/drmoisan/TaskMaster/pull/182
- Authoritative inputs: `remediation-inputs.2026-06-08T18-06.md`
- Evidence root (canonical, non-overridable): `docs/features/active/2026-06-08-csharp-analyzer-stack-hardening-181/evidence/<kind>/`

## Scope (single, narrow)

Apply CSharpier 1.2.6 formatter output to exactly ONE production file,
`UtilitiesCS/Extensions/IEnumerableExtensions.cs`, so the whole-repo CI step
`dotnet csharpier check .` ("Verify formatting", the first step of the required
"Format, build, analyze, and test" check) exits 0. This file is byte-identical to
`main` and is a pre-existing `main` regression inherited by the branch; CI cannot
reach AC6 GREEN while the blocking formatting gate fails. This is a pure formatting
change: no logic, no behavior, no public-API change. No test changes are required.

## Scope Guard (do not do — from remediation-inputs.2026-06-08T18-06.md)

- Do NOT modify the logic of `IEnumerableExtensions.cs`; apply only CSharpier formatter output.
- Do NOT touch any other `.cs` source file; only this one file is unformatted per the CI log.
- Do NOT alter the analyzer-stack build-config delivered earlier in this feature.
- Do NOT introduce any CS8032 suppression or re-add SecurityCodeScan.
- Do NOT touch the two vendored projects (SVGControl, UtilitiesSwordfish.NET.General).
- Do NOT promote RS0030 or any analyzer rule from suggestion to warning/error.
- Do NOT modify `.claude/rules/` policy documents beyond the already-delivered `csharp.md`.
- Do NOT weaken or skip any CI gate to force green.

---

### Phase 0 — Policy Reads and Baseline Capture

- [x] [P0-T1] Read the repository policy documents in the mandatory order defined by `policy-compliance-order`: `CLAUDE.md`, `.claude/rules/general-code-change.md`, `.claude/rules/general-unit-test.md`, `.claude/rules/csharp.md`, and `.claude/rules/ci-workflows.md`. Record an evidence artifact at `docs/features/active/2026-06-08-csharp-analyzer-stack-hardening-181/evidence/baseline/phase0-instructions-read.2026-06-08T18-06.md` containing `Timestamp:`, `Policy Order:` (the ordered list), and an explicit list of files read. Acceptance: artifact exists and lists all five files in the policy order.
- [x] [P0-T2] Capture the current branch HEAD SHA and clean working-tree state. Run `git rev-parse HEAD` and `git status --porcelain`. Write evidence to `docs/features/active/2026-06-08-csharp-analyzer-stack-hardening-181/evidence/baseline/baseline-git-state.2026-06-08T18-06.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:` recording the HEAD SHA and whether the tree is clean. Acceptance: artifact records HEAD SHA equal to `71e0777ada475c408d85d3b6c68e6192b4bc070b` (or the current head if advanced) and a clean tree prior to the fix.
- [x] [P0-T3] Capture the fail-before formatting baseline. Run `dotnet tool restore` then `dotnet tool run csharpier check .`. Write evidence to `docs/features/active/2026-06-08-csharp-analyzer-stack-hardening-181/evidence/baseline/baseline-format.2026-06-08T18-06.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:`. Acceptance: artifact records a non-zero `EXIT_CODE` and the summary names `UtilitiesCS/Extensions/IEnumerableExtensions.cs` as "Was not formatted" (CSharpier 1.2.6). This is the pre-fix failing-gate proof for the formatting AC.
- [x] [P0-T4] Capture the pre-fix `*.cs` diff scope against base. Run `git diff --name-only main..HEAD -- "*.cs"`. Write evidence to `docs/features/active/2026-06-08-csharp-analyzer-stack-hardening-181/evidence/baseline/baseline-diff-scope.2026-06-08T18-06.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:` listing the changed `.cs` files (which must NOT include `UtilitiesCS/Extensions/IEnumerableExtensions.cs` before the fix, confirming the file is byte-identical to `main`). Acceptance: artifact records the pre-fix changed-`.cs` set as the diff-scope baseline.

---

### Phase 1 — Apply CSharpier Formatting to the Single File

- [x] [P1-T1] Apply CSharpier 1.2.6 formatter output to `UtilitiesCS/Extensions/IEnumerableExtensions.cs` only, by running `dotnet tool run csharpier UtilitiesCS/Extensions/IEnumerableExtensions.cs`. Do not hand-edit; the formatter output is authoritative. Acceptance: the command exits 0 and `UtilitiesCS/Extensions/IEnumerableExtensions.cs` is the only file modified (no logic, behavior, or public-API token changes — whitespace and line-wrapping only).
- [x] [P1-T2] Verify the formatting gate now passes repo-wide. Run `dotnet tool run csharpier check .`. Write evidence to `docs/features/active/2026-06-08-csharp-analyzer-stack-hardening-181/evidence/regression-testing/format-check-after-fix.2026-06-08T18-06.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:`. Acceptance: `EXIT_CODE: 0` and the summary reports no unformatted files (pass-after proof; pairs with the P0-T3 fail-before baseline).
- [x] [P1-T3] Verify diff scope is exactly one production file and is formatting-only. Run `git diff --name-only main..HEAD -- "*.cs"` and `git diff main..HEAD -- "UtilitiesCS/Extensions/IEnumerableExtensions.cs"`. Write evidence to `docs/features/active/2026-06-08-csharp-analyzer-stack-hardening-181/evidence/regression-testing/diff-scope-after-fix.2026-06-08T18-06.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:`. Acceptance: the `*.cs` name-only list contains ONLY `UtilitiesCS/Extensions/IEnumerableExtensions.cs`, and the unified diff shows only whitespace/line-wrapping changes with no token, identifier, operator, or statement changes.

---

### Phase 2 — Final QA Loop (Full Toolchain) and AC Reconciliation

- [x] [P2-T1] Final-QC step 1 (format). Run `dotnet tool run csharpier check .`. Write evidence to `docs/features/active/2026-06-08-csharp-analyzer-stack-hardening-181/evidence/qa-gates/final-format.2026-06-08T18-06.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:`. Acceptance: `EXIT_CODE: 0`. If this step changes any file, restart the loop from P2-T1.
- [x] [P2-T2] Final-QC restore. Run `nuget restore TaskMaster.sln`. Write evidence to `docs/features/active/2026-06-08-csharp-analyzer-stack-hardening-181/evidence/qa-gates/final-restore.2026-06-08T18-06.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:`. Acceptance: `EXIT_CODE: 0` and packages restore without error.
- [x] [P2-T3] Final-QC step 2 (analyzer / code-style build). Run `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`. Write evidence to `docs/features/active/2026-06-08-csharp-analyzer-stack-hardening-181/evidence/qa-gates/final-analyzer-build.2026-06-08T18-06.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:`. Acceptance: `EXIT_CODE: 0` (build succeeds; analyzer diagnostics remain at suggestion severity per the delivered config; no new first-party diagnostics introduced by the formatting change).
- [x] [P2-T4] Final-QC step 3 (nullable type-check, warnings-as-errors). Run `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`. Write evidence to `docs/features/active/2026-06-08-csharp-analyzer-stack-hardening-181/evidence/qa-gates/final-nullable-build.2026-06-08T18-06.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:`. Acceptance: the result holds at the pre-existing vendored-only 84-error baseline — zero first-party errors, zero CS8032 — with no new errors attributable to the formatting change (AC5: nullable gate does NOT regress).
- [x] [P2-T5] Final-QC step 4 (MSTest suite with coverage). Discover first-party `*.Test.dll` assemblies under `bin\Debug` (excluding `obj`/`ref`, and excluding the vendored SVGControl.Test and UtilitiesSwordfish.Test) and run `vstest.console.exe <first-party test dlls> /EnableCodeCoverage /InIsolation /Logger:trx`. Write evidence to `docs/features/active/2026-06-08-csharp-analyzer-stack-hardening-181/evidence/qa-gates/final-test-coverage.2026-06-08T18-06.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:` including the numeric repository-wide line-coverage headline and pass/fail counts. Acceptance: no test regression beyond the documented flaky wall-clock-timer tests; repository-wide line coverage remains `>= 80%`; the formatting-only change does not reduce coverage on the touched file's lines.
- [x] [P2-T6] Coverage delta reconciliation. Compare the P2-T5 numeric coverage against the cycle-1 baseline (`evidence/baseline/baseline-test-coverage.2026-06-08T12-12.md`) and final (`evidence/qa-gates/final-test-coverage.2026-06-08T12-12.md`). Write evidence to `docs/features/active/2026-06-08-csharp-analyzer-stack-hardening-181/evidence/qa-gates/coverage-delta.2026-06-08T18-06.md` with `Timestamp:`, baseline coverage, post-change coverage, and changed-line coverage for `UtilitiesCS/Extensions/IEnumerableExtensions.cs`. Acceptance: no coverage regression on changed lines; repository-wide line coverage `>= 80%` (the change is whitespace-only, so changed-line coverage is expected to be unchanged from baseline).
- [x] [P2-T7] AC reconciliation and exit-state note. Write evidence to `docs/features/active/2026-06-08-csharp-analyzer-stack-hardening-181/evidence/qa-gates/acceptance-summary.2026-06-08T18-06.md` with `Timestamp:` recording: AC5 (nullable gate does NOT regress) corroborated by P2-T4; the formatting gate now passes locally (P1-T2, P2-T1); diff scope is exactly one production file, formatting-only (P1-T3). Note that AC6 (PR #182 CI GREEN, all steps) is confirmed by the orchestrator re-pushing the branch and watching the required "Format, build, analyze, and test" check — out of plan-execution scope but referenced here for the cycle exit gate (`blocking_count == 0`). Acceptance: artifact records the local-gate results and the explicit AC6 deferral to orchestrator CI observation.

---

## Toolchain Loop Restart Rule

Treat Phase 2 steps P2-T1 through P2-T5 as one toolchain pass in CLAUDE.md order
(format -> restore -> analyzer build -> nullable build -> test). If any step fails
or auto-fixes/changes any file, restart the pass from P2-T1. Do not stop the loop
while any step is failing or changing files.

## Out-of-Scope (handled by orchestrator at cycle exit)

- Re-push of the head branch and observation of PR #182 required CI ("Format, build,
  analyze, and test") to GREEN, which is the authoritative confirmation of AC6.
- `code-review.<exit-ts>.md`, `feature-audit.<exit-ts>.md`, `policy-audit.<exit-ts>.md`
  authored by `feature-review` at cycle exit after CI is green.
- Exit gate: `blocking_count == 0` (PR #182 CI GREEN; AC6 PASS, AC5 corroborated).
