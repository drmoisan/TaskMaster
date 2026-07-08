# Policy Audit — store-wrapper-launch-npe (Issue #240)

- Timestamp: 2026-07-06T12-15
- Feature folder: `docs/features/active/2026-07-06-store-wrapper-launch-npe-240/`
- Work mode: `minor-audit`
- Base branch (resolved): `main` @ `4022fe7c9b07119224ca5aaa880b0a4003ef08db`
- Head: `TaskMaster-wt-2026-07-06-06-35` @ `dfbebb13fdc9ce2e9240376be2214dddf56ee5d0`
- Range audited: `4022fe7c9b07119224ca5aaa880b0a4003ef08db..dfbebb13fdc9ce2e9240376be2214dddf56ee5d0` (full branch diff, per Scope Invariant)

## Rejected Scope Narrowing

No caller instruction attempted to narrow this audit to a plan/task/phase subset, mark a language as out of scope, or skip a toolchain/coverage check. The orchestrator-supplied inputs (resolved base branch, merge-base SHA, active feature folder, AC source per work mode) are legitimate scope sources and were used as provided. No entries to record.

## Evidence Location Compliance

`git diff --name-only` over the audited range was scanned for paths under `artifacts/baselines/`, `artifacts/qa/`, `artifacts/evidence/`, and `artifacts/coverage/`. No matches were found. All 21 evidence/documentation files added by this branch live under the canonical `docs/features/active/2026-07-06-store-wrapper-launch-npe-240/evidence/<kind>/` tree (`baseline/`, `regression-testing/`, `qa-gates/`, `issue-updates/`, `other/`). `scripts/dev_tools/validate_evidence_locations.py` referenced by the review contract does not exist in this repository; the scan above was performed manually via `git diff --name-only` and directory inspection instead. No `EVIDENCE_LOCATION_OVERRIDE_REJECTED` entries are required — no non-canonical path was supplied or used.

## Executive Summary

Issue #240 fixes an unhandled `NullReferenceException` in `StoreWrapperController.Launch()` by extracting a testable `EvaluateLaunchReadiness()` decision method and gating the pre-existing `[ExcludeFromCodeCoverage]` WinForms shell on its result. The change is scoped exactly as planned (one production file, one test file), is backed by a fail-before/pass-after MSTest regression pair using Moq and FluentAssertions, and the new decision logic is independently verified at 100% line/block coverage. Toolchain execution is documented transparently, including two disclosed deviations: (1) the solution-wide nullable gate's `EXIT_CODE 1` is a pre-existing, unrelated vendored-project condition, and (2) the test file `StoreWrapperController_Tests.cs` was already over the repository's 500-line limit before this change and grew further (582 -> 781 lines). Independent inspection of the coverage run's underlying data also shows that the "repository line coverage" figure cited in the feature's own evidence (85.88%) is scoped to a single assembly (`UtilitiesCS.dll`), not the full C# solution; no canonical repo-wide coverage artifact exists for this review session. Both the file-size overage and the repo-wide coverage scope gap are recorded below and carried into remediation inputs.

## PR-Context Artifact Reliability Note

`artifacts/pr_context.summary.txt` reports "Core logic changes: 0 files" and files both changed `.cs` files into "Docs/templates/agents/tooling: 21 files" (actual: 21 docs + 2 `.cs` = 23). This audit did not rely on that classification; scope and file lists below were independently derived from `git diff --name-status 4022fe7c9b07119224ca5aaa880b0a4003ef08db..dfbebb13fdc9ce2e9240376be2214dddf56ee5d0` and `git diff --stat`.

## 1. General Unit Test Policy Compliance

### 1.1 Core Principles (Independence, Isolation, Fast Execution, Determinism, Readability)

- Independence/Isolation: each new test constructs its own `Mock<IApplicationGlobals>`/`Mock<IOlObjects>` and `StoreWrapperController`; no shared mutable state between tests. **PASS**.
- The two `Launch()` tests mutate the static `MyBox.DialogInvoker` seam but save/restore it in `try`/`finally`, preventing cross-test leakage. **PASS**.
- Fast/Deterministic: no `Thread.Sleep`, `Task.Delay`, wall-clock reads, or unseeded randomness in the new tests. **PASS**.
- Readability: descriptive test names (`Launch_WhenStoresWrapperIsNull_ShowsUserMessageAndDoesNotThrowOrOpenViewer`, `EvaluateLaunchReadiness_WhenStoresListIsNull_ReturnsStoresUnavailable`) and XML-doc comments stating scenario/expected outcome on every new test. **PASS**.
- Arrange-Act-Assert structure observed in all 7 new tests (verified by direct reading of `UtilitiesCS.Test/OutlookObjects/Store/StoreWrapperController_Tests.cs`). **PASS**.

### 1.2 Coverage and Scenarios

#### 1.2.1 Per-Language Coverage Comparison

- C#: Baseline: 85.87% line / 86.68% block (`UtilitiesCS.dll`, `evidence/baseline/test-coverage-baseline.md`). Post-change: 85.88% line / 86.69% block (`UtilitiesCS.dll`, `evidence/qa-gates/qa-04-test-coverage.md`, cross-verified against `TestResults/final-coverage.xml` module `UtilitiesCS.dll`: lines_covered=36897/42964). Change: +0.01% line, no regression on previously-covered lines (4163/4163 baseline-passing tests still pass; 4170/4170 post-change). New/changed-code coverage: 100.00% line / 100.00% block for `EvaluateLaunchReadiness()` and the two `StoreLaunchReadiness` factory methods (verified via `TestResults/final-coverage.xml` function id 295736 and related entries; `Launch()` itself is reported `skipped_function reason="attribute_excluded"`, consistent with its pre-existing `[ExcludeFromCodeCoverage]` attribute, unchanged by this PR). Disposition: PASS for new/changed-code and no-regression checks; **FAIL for the repo-wide row** — see below. Evidence: `evidence/baseline/test-coverage-baseline.md`, `evidence/qa-gates/qa-04-test-coverage.md`, `evidence/qa-gates/qa-05-coverage-delta.md`, `TestResults/final-coverage.xml` (uncommitted, generated this session, not at the canonical `artifacts/csharp/coverage.xml` path).
- TypeScript: Baseline: N/A. Post-change: N/A. Change: N/A. Disposition: N/A. Evidence: N/A — no TypeScript files changed on this branch.
- PowerShell: Baseline: N/A. Post-change: N/A. Change: N/A. Disposition: N/A. Evidence: N/A — no PowerShell files changed on this branch.
- Python: Baseline: N/A. Post-change: N/A. Change: N/A. Disposition: N/A. Evidence: N/A — no Python files changed on this branch.

##### Coverage Evidence Checklist

- TypeScript baseline coverage artifact: N/A - out of scope (no `.ts`/`.tsx` files changed).
- TypeScript post-change coverage artifact: N/A - out of scope.
- PowerShell baseline coverage artifact: N/A - out of scope (no `.ps1`/`.psm1` files changed).
- PowerShell post-change coverage artifact: N/A - out of scope.

#### 1.2.2 Repo-Wide Coverage Verdict (Mandatory Coverage Verification)

Per the review contract's Coverage Verification procedure, a canonical repo-wide coverage artifact (`artifacts/csharp/coverage.xml`) is required whenever C# files changed in the branch diff. This branch changes two `.cs` files, so the check is mandatory.

- **No `artifacts/csharp/coverage.xml` exists in this repository** (the `artifacts/` directory is entirely git-ignored and was not populated in this session). Per the mandatory procedure: **FAIL** — "coverage artifact absent for C#; coverage verification is mandatory for all languages with changed files."
- The feature's own evidence (`evidence/qa-gates/qa-04-test-coverage.md`, `qa-05-coverage-delta.md`) labels 85.88% as "repository (testable-denominator) line coverage," but this figure is scoped to the single `UtilitiesCS.dll` module, not the full C# solution.
- This review independently inspected the underlying coverage data generated during the feature's own P3-T4 run (`TestResults/final-coverage.xml`, uncommitted). That run also instrumented other first-party/vendored modules loaded transitively by `UtilitiesCS.Test`: `TaskMaster.dll` (8.58% line), `SVGControl.dll` (15.15% line), `Swordfish.NET.General.dll` (45.86% line), `Tags.dll` (0.00% line), `ToDoModel.dll` (0.00% line), `QuickFiler.dll` (0.00% line). The near-zero readings for `Tags.dll`/`ToDoModel.dll`/`QuickFiler.dll` most likely reflect that their own dedicated test projects were not executed in this run (only `UtilitiesCS.Test.dll` was run), not that those modules are genuinely untested — so this data point cannot be treated as a defensible "true repo-wide" percentage either; it merely confirms that no single-project test run produces a valid repo-wide figure.
- A line-weighted aggregate across the modules above (from the same uncommitted artifact) is approximately 64% line coverage — below both the CLAUDE.md 80% floor and the repo-rule 85%/75% floor — but is presented here only as corroborating evidence of risk, not as a certified repo-wide measurement, for the reason above.
- **Disposition: FAIL** (artifact absent / no valid canonical repo-wide measurement exists). This condition pre-dates issue #240 (it is not attributable to the two files this branch touches) and is partially addressed by the project's own ratified COM/VSTO/WinForms coverage-exemption initiative referenced in `CLAUDE.md` UT2 (tracked in `feature/csharp-coverage-uplift`). It is carried into remediation inputs as a systemic, non-blocking-for-this-bugfix tracking item, not as a defect introduced by this PR.

### 1.3 Scenario Completeness

For `EvaluateLaunchReadiness()`: null `Globals` (edge), null `Ol` (edge), null `StoresWrapper` (negative/root-cause primary), null `Stores` list (negative/root-cause secondary), and the happy path with populated stores (positive) are all covered — 5 of 5 branches of the readiness state machine. For `Launch()`: both not-ready paths (null model, null stores list) are covered with an assertion that no exception is thrown, the dialog seam fires exactly once, and `Viewer` remains null. **PASS**.

### 1.4 External Dependencies and Environment

No live Outlook process, no real files, no temporary files. `IApplicationGlobals`/`IOlObjects` are mocked via Moq; the WinForms dialog is intercepted via the existing `MyBox.DialogInvoker` injectable seam (not a raw `MessageBox.Show`/`Form.ShowDialog` call), confirmed by direct inspection of `UtilitiesCS/Dialogs/MyBox.cs`. **PASS**.

### 1.5 Test File Location

Tests are added to the existing `UtilitiesCS.Test/OutlookObjects/Store/StoreWrapperController_Tests.cs`, mirroring `UtilitiesCS/OutlookObjects/Store/StoreWrapperController.cs` under the repo's established `<Project>.Test` mirrored-structure convention (this repo does not use a literal `tests/` root; the `<Project>.Test` convention is applied consistently across the whole C# codebase). Consistent with CLAUDE.md §7 ("match existing style"). **PASS** by established convention.

### 1.6 File-Size Limit (Test Code)

`UtilitiesCS.Test/OutlookObjects/Store/StoreWrapperController_Tests.cs` is **781 lines** after this change (`wc -l`/`awk` verified), up from a pre-existing **582-line** baseline at the merge-base commit (already over the repo's 500-line limit before this issue). This PR's diff adds 199 lines to the file (`git diff --stat`: `StoreWrapperController_Tests.cs | 199 +++++++++++++++++++++`). The general-code-change policy's 500-line file limit applies to test code with no exemption for this case. **FAIL** — this is a genuine, worsened policy violation, self-disclosed by the executor in `evidence/other/scope-budget-confirmation.md` and `evidence/other/plan-status-summary.md` as an unresolved deviation flagged for remediation rather than fixed unilaterally.

## 2. General Code Change Policy Compliance

- Design principles: the fix follows simplicity-first and separation-of-concerns — pure decision logic (`EvaluateLaunchReadiness`) is extracted from the I/O/UI shell (`Launch()`), which keeps its pre-existing `[ExcludeFromCodeCoverage]` attribute. **PASS**.
- Error handling: no exception is silently swallowed; the not-ready paths return a typed sentinel and the caller explicitly branches on it rather than catching an exception. **PASS**.
- Naming: `StoreLaunchReadinessState`, `StoreLaunchReadiness`, `EvaluateLaunchReadiness` are descriptive and unambiguous. **PASS**.
- Public API impact: `EvaluateLaunchReadiness()` and the new types are `internal`, and `Launch()`'s public signature is unchanged. No breaking change. **PASS**.
- File-size limit: production file `StoreWrapperController.cs` is 396 lines (`<= 500`), verified directly. **PASS**. Test file: see §1.6 above — **FAIL**.
- Comment quality: the new XML doc comments and the `#pragma` "why:" comment explain rationale, not restatement of code. **PASS**.
- I/O boundary isolation / no temp files: confirmed, see §1.4. **PASS**.

## 3. Language-Specific Code Change Policy Compliance (C#)

- Formatting (CSharpier): `evidence/qa-gates/qa-01-format.md` — mutation pass reformatted the two touched files, loop was restarted per the plan's loop rule, and the subsequent verification pass (`csharpier check .`) reports 0 files requiring reformatting, `EXIT_CODE 0`. **PASS**.
- .NET Analyzers: `evidence/qa-gates/qa-02-analyzers.md` — `EXIT_CODE 0`, 70 warnings (baseline 72, no increase), zero diagnostics attributable to either touched file. **PASS**.
- Nullable / TreatWarningsAsErrors: `evidence/qa-gates/qa-03-nullable.md` — solution-wide `EXIT_CODE 1` reproduces the exact pre-existing baseline condition (`evidence/baseline/nullable-baseline.md`, itself `EXIT_CODE 1` for the same vendored `SVGControl.csproj`/`Swordfish.NET.General.csproj` reasons, unrelated to this issue). A scoped rebuild of the touched projects (`-p:BuildProjectReferences=false`) confirms `StoreWrapperController_Tests.cs` contributes zero diagnostics and `StoreWrapperController.cs` contributes diagnostics only on pre-existing, unmodified lines (constructor `CS8618` x8, `SelectFolder`/`SelectFsFolder` `CS8603` x3). One genuine 2-diagnostic regression (`CS8625` on the new `StoreLaunchReadiness.NotReady` constructor call) was found and fixed with a narrowly-scoped, documented `#pragma warning disable/restore CS8625`, per C#7's suppression-narrowness requirement. **PASS with documented pre-existing exception** (consistent with AC5's own caveat wording).
- Suppression practice: the `#pragma` block is minimal (wraps exactly the `new(state, null, null)` call), carries a `why:` comment explaining the rejected alternative (`?`-nullable annotation, which would introduce new `CS8632` warnings on a project without a `<Nullable>` setting), and is restored immediately after. **PASS**.

## 4. Language-Specific Unit Test Policy Compliance (C#)

- Framework: MSTest (`[TestClass]`/`[TestMethod]`), confirmed by direct inspection of the diff. **PASS**.
- Mocking: Moq (`Mock<IApplicationGlobals>`, `Mock<IOlObjects>`), used for all 7 new tests. **PASS**.
- Assertions: FluentAssertions (`Should().NotThrow()`, `Should().Be(...)`, `Should().BeNull()`, `Should().Equal(...)`) used throughout; no bare MSTest `Assert` calls in the new tests. **PASS**.
- Toolchain command selection matches CUT3 (`csharpier`, `msbuild ... EnableNETAnalyzers`, `msbuild ... Nullable=enable`, `vstest.console.exe ... /EnableCodeCoverage`). **PASS**.

## 5. Test Coverage Detail

| Scope | Metric | Value | Threshold | Verdict |
|---|---|---|---|---|
| New code (`EvaluateLaunchReadiness`, `StoreLaunchReadiness.NotReady/Ready`) | Line | 100.00% | >= 90% (CLAUDE.md) / >= 85% (repo rule) | PASS |
| New code (same scope) | Block/branch | 100.00% | >= 75% | PASS |
| Modified file changed lines (`Launch()` guard branch) | N/A — excluded via pre-existing `[ExcludeFromCodeCoverage]`, unchanged by this PR | N/A | No regression | PASS |
| Single assembly `UtilitiesCS.dll` (mislabeled "repository" in feature evidence) | Line | 85.88% (baseline 85.87%) | Informational only — not the canonical repo-wide gate | Context only |
| Repo-wide, all C# assemblies (canonical `artifacts/csharp/coverage.xml`) | Line | Not measurable this session — no canonical artifact; partial same-session data suggests ~64% line coverage across loaded modules, with the caveat in §1.2.2 | >= 80% (CLAUDE.md) / >= 85% (repo rule) | **FAIL** (artifact absent) |

## 6. Test Execution Metrics

- Baseline (`evidence/baseline/test-coverage-baseline.md`): 4163 total, 4163 passed, 0 failed.
- Fail-before (`evidence/regression-testing/fail-before-240.md`): 2 total (filtered), 0 passed, 2 failed — reproduces the issue #240 crash (one `NullReferenceException`, one `ArgumentNullException`, both unhandled exceptions from the same unguarded code path).
- Pass-after (`evidence/regression-testing/pass-after-240.md` / `evidence/qa-gates/qa-04-test-coverage.md`): 4170 total, 4170 passed, 0 failed. Delta: +7 tests (2 regression + 5 unit), 0 regressions.
- Cross-verification: `TestResults/final-coverage.xml` (uncommitted) lists per-function coverage for all 7 new test methods with non-zero `blocks_covered`/`lines_covered`, corroborating that they executed (consistent with, though not a substitute for, the reported "Passed" counts).

## 7. Code Quality Checks

| Check | Command / Method | Result |
|---|---|---|
| Confidentiality masking scan | Manual diff review for secrets/credentials in new `.cs`/`.md` files | No secrets or credentials found |
| Suppression scan (added lines) | Manual review of new `#pragma` usage | 1 narrowly-scoped `#pragma warning disable/restore CS8625` pair, documented with a `why:` comment; compliant with C#7 |
| Workflow change scan | `git diff --name-only` filtered for `.github/workflows/**` | No workflow files changed; `.claude/rules/ci-workflows.md` not applicable |
| Benchmark baseline scan | `git diff --name-only` filtered for `scripts/benchmarks/**` | No benchmark files changed; `.claude/rules/benchmark-baselines.md` not applicable |
| Orchestrator-state scan | `git diff --name-only` filtered for `orchestrator-state.json` | No orchestrator-state checkpoint changed; `.claude/rules/orchestrator-state.md` not applicable |

## 8. Gaps and Exceptions

1. **Test file 500-line limit** (`UtilitiesCS.Test/OutlookObjects/Store/StoreWrapperController_Tests.cs`, 781 lines) — pre-existing violation (582 lines at baseline), worsened by +199 lines from this PR. Self-disclosed by the executor; not resolved. Carried to remediation inputs as **Blocking**.
2. **Repo-wide C# coverage artifact absent** — no `artifacts/csharp/coverage.xml`; the feature's "repository line coverage" claim (85.88%) is single-assembly-scoped. Pre-existing, systemic, not attributable to this PR's two changed files; the project's own new/changed-code coverage (100%) is not in question. Carried to remediation inputs as a **tracked, non-blocking-for-#240** systemic item.
3. **PR-context summary misclassification** — "Core logic changes: 0 files" omits both changed `.cs` files. Process/tooling gap, not a code defect. Carried to remediation inputs as **informational**.
4. Nullable gate's solution-wide `EXIT_CODE 1` is a pre-existing, documented, unrelated vendored-project condition — accepted, not a gap requiring remediation for this PR.

## 9. Summary of Changes

- `UtilitiesCS/OutlookObjects/Store/StoreWrapperController.cs`: +101/-4 lines. Adds `StoreLaunchReadinessState`, `StoreLaunchReadiness`, `EvaluateLaunchReadiness()`; modifies `Launch()` to branch on readiness before touching `Model`.
- `UtilitiesCS.Test/OutlookObjects/Store/StoreWrapperController_Tests.cs`: +199 lines. Adds 2 `Launch()` regression tests and 5 `EvaluateLaunchReadiness()` unit tests.
- 21 documentation/evidence files added under `docs/features/active/2026-07-06-store-wrapper-launch-npe-240/` (issue, plan, research, evidence artifacts).
- No changes to `TaskMaster/Ribbon/RibbonController.cs` or `TaskMaster/AppGlobals/AppOlObjects.cs` (confirmed excluded from scope by the plan and by `git diff --name-only`).

## 10. Compliance Verdict

**PARTIAL.** The core fix, its regression tests, and its new-code coverage fully satisfy the General/C#-specific Code Change and Unit Test policies. Two findings prevent an unqualified PASS: (a) the test file's 500-line limit violation, worsened by this PR (Blocking), and (b) the absence of a canonical repo-wide C# coverage artifact, which is a pre-existing, systemic condition not caused by this PR but which the coverage-verification procedure requires to be reported as FAIL for any language with changed files. See `remediation-inputs.2026-07-06T12-15.md`.

## Appendix A: Test Inventory

| Test | Type | Target | Result |
|---|---|---|---|
| `Launch_WhenStoresWrapperIsNull_ShowsUserMessageAndDoesNotThrowOrOpenViewer` | Regression (fail-before/pass-after) | `Launch()` (AC1) | Fail-before: FAIL (`NullReferenceException`). Pass-after: PASS |
| `Launch_WhenStoresListIsNull_ShowsUserMessageAndDoesNotThrowOrOpenViewer` | Regression (fail-before/pass-after) | `Launch()` (AC2) | Fail-before: FAIL (`ArgumentNullException`). Pass-after: PASS |
| `EvaluateLaunchReadiness_WhenGlobalsIsNull_ReturnsModelUnavailable` | Unit (edge) | `EvaluateLaunchReadiness()` | PASS |
| `EvaluateLaunchReadiness_WhenOlIsNull_ReturnsModelUnavailable` | Unit (edge) | `EvaluateLaunchReadiness()` | PASS |
| `EvaluateLaunchReadiness_WhenStoresWrapperIsNull_ReturnsModelUnavailable` | Unit (negative) | `EvaluateLaunchReadiness()` | PASS |
| `EvaluateLaunchReadiness_WhenStoresListIsNull_ReturnsStoresUnavailable` | Unit (negative) | `EvaluateLaunchReadiness()` | PASS |
| `EvaluateLaunchReadiness_WhenModelAndStoresPopulated_ReturnsReadyWithDisplayNames` | Unit (positive) | `EvaluateLaunchReadiness()` | PASS |

## Appendix B: Toolchain Commands Reference

| Stage | Command | Result |
|---|---|---|
| Format | `dotnet tool run csharpier check .` (then `format .`, then re-`check .`) | 0 files require reformatting after mutation pass; `EXIT_CODE 0` |
| Analyzers | `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` | `EXIT_CODE 0`, 70 warnings (baseline 72), 0 errors |
| Nullable | `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true` (invoked as `-t:Rebuild`) | `EXIT_CODE 1` — pre-existing, unrelated vendored-project condition; scoped rebuild confirms 0 new diagnostics on touched files |
| Test + Coverage | `vstest.console.exe UtilitiesCS.Test/bin/Debug/UtilitiesCS.Test.dll /EnableCodeCoverage /InIsolation` | `EXIT_CODE 0`, 4170/4170 passed |
