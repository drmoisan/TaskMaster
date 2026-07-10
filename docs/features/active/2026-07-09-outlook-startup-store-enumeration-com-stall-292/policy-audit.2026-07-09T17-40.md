# Policy Audit — outlook-startup-store-enumeration-com-stall (Issue #292)

- Feature: `docs/features/active/2026-07-09-outlook-startup-store-enumeration-com-stall-292/`
- Branch: `bug/outlook-startup-store-enumeration-com-stall-292` @ HEAD `8f391d8f59afc74fdc9aacd3e53c59174e414884`
- Resolved base branch: `main` @ `c9ddbf289c06f5fbf61673549911dac80917ce24` (merge-base, verified via `git merge-base HEAD origin/main`)
- Diff range: `c9ddbf289c06f5fbf61673549911dac80917ce24..8f391d8f59afc74fdc9aacd3e53c59174e414884`
- Work Mode: `full-bug` (AC source: `spec.md`, mirrored in `issue.md`)
- Reviewer: feature-review agent
- Timestamp: 2026-07-09T17-40
- Review context: re-audit after remediation cycle 1 (the cycle-1 fix — `[DoNotParallelize]` on 8 `UtilitiesCS.Test` classes — is now part of the branch diff)

## Executive Summary

Issue #292 delivers a causation-scoped, two-part C# change plus regression tests to the #260 startup
store-lockup resilience system: one phase-identity constant on `CurrentStoreContext`, both
`Namespace.Stores` materialization sites in `StoresWrapper` wrapped in an ambient enumeration-phase scope
via one extracted private helper (`MaterializeFilteredStores`), and a terminal phase-identity branch in
`StoreLockupResponder` that emits an attributed WARN with `autoDisabled: false` and returns before any
`IStoreDisableService` call. The change is additive and observational; it does not alter the included-store
set or enumeration order and adds no public API surface. Remediation cycle 1 added `[DoNotParallelize]` to
8 existing `UtilitiesCS.Test` classes to remove a shared-static (`CurrentStoreContext._current`)
test-isolation race under class-level parallel execution; that change is test-attribute-only (no production
code).

The recorded C# toolchain passes in order: CSharpier check (EXIT 0, 1318 files, no reformatting), .NET
analyzers (EXIT 0, 0 errors, 0 warnings), nullable/`TreatWarningsAsErrors` (EXIT 0, 0 errors, 0 warnings),
and the CI-equivalent `vstest ... /EnableCodeCoverage /InIsolation` full-suite run (EXIT 0, 5141/5141 pass,
0 fail, confirmed deterministic across 4+ passes). New/changed executable-code coverage is 14/14 = 100%
(independently re-anchored from `artifacts/csharp/coverage.xml`: `MaterializeFilteredStores()`
`line_coverage="100.00"`). No changed file exceeds the 500-line limit. C# repo-wide line coverage on the
testable denominator is 81.80% (>= 80% CLAUDE.md floor); the touched assembly `UtilitiesCS` is 88.33%.

One non-blocking Major finding is recorded: the new `TaskMaster.Test` regression class
`StoresWrapperEnumerationScopeTests` is an unmarked `CurrentStoreContext` null-baseline reader/writer of the
same defect class fixed in cycle 1 for the sibling assembly. It is race-free under the actual CI invocation
(`TaskMaster.Test` has no `[assembly: Parallelize]`, so it runs sequentially) and flakes only under the
non-gate VS Code coverage runsettings that force class-level parallelization. It does not fail the required
CI check and is not a merge blocker; a follow-up is recommended (see section 7). Overall verdict: PASS. Zero
Blocking findings.

## Scope Confirmation (Legitimate Base)

The audit scope is the full branch diff against the resolved base `main` @ `c9ddbf28`. The supplied
merge-base SHA matches the recomputed `git merge-base HEAD origin/main` output exactly, so the base is
current (not behind). The C# coverage gate applies because the branch has changed `.cs` files. Scope was
determined from `git diff --numstat`/`--name-status`, not from the automated summary overview.

### PR-context summary correction (recurring C#-as-docs misclassification)

The automated `artifacts/pr_context.summary.txt` "Changed files overview" again reported
`Core logic changes: 0 files` and classified all changes as `Docs/templates/agents/tooling: 44 files`. This
is the recurring misclassification in which C# production changes are labeled as documentation. The branch
in fact changes three C# production files (`StoresWrapper.cs`, `CurrentStoreContext.cs`,
`StoreLockupResponder.cs`) plus a new C# regression test file, 8 modified `UtilitiesCS.Test` `.cs` files
(the cycle-1 `[DoNotParallelize]` additions), one modified `UtilitiesCS.Test` `.cs` file with a new test
(`StoreLockupResponderTests.cs`), and one modified `.csproj`. The summary overview was corrected in place on
2026-07-09T17-40 so the C# coverage gate is applied. The corrected inventory is authoritative for this
audit.

### Rejected Scope Narrowing

None. No caller instruction attempted to narrow the audit to a plan/task/phase subset, to a subset of
changed files, or to mark any language with changed files as out of plan scope, informational only, or not
applicable. The caller explicitly stated "Run the complete review contract with no scope narrowing" and
framed the C# coverage description as "factual context ... this is not a scope instruction." The full
feature-vs-base audit was performed over all changed files.

## Changed-File Inventory by Language

| Language | Changed files (branch diff) | Coverage-gate applies |
|---|---|---|
| C# (`.cs`, `.csproj`) | 3 modified production `.cs` (`StoresWrapper.cs`, `CurrentStoreContext.cs`, `StoreLockupResponder.cs`); 1 new test `.cs` (`StoresWrapperEnumerationScopeTests.cs`); 9 modified test `.cs` (8 `[DoNotParallelize]` additions + `StoreLockupResponderTests.cs`); 1 modified test `.csproj` | Yes |
| Markdown (`.md`) | spec, issue, plan, research, evidence artifacts, agent-memory notes | Exempt (docs) |
| TypeScript / Python / PowerShell | 0 changed files | Coverage gate not triggered — zero changed files on branch |

No `.ps1`/`.psm1`, `.ts`/`.tsx`, or `.py` files are changed on the branch, so PowerShell, TypeScript, and
Python coverage gates are not triggered. C# is the only language with changed files.

## 1. Coverage Compliance

Applied thresholds are the CLAUDE.md-authoritative C# thresholds for this legacy VSTO/WinForms area:
repository line coverage >= 80% on the testable denominator (with the documented COM/VSTO/WinForms/Outlook-
interop exemptions), >= 90% for new code, and no regression on changed lines. The change edits code inside
`UtilitiesCS`, which is host-neutral and fully reachable through the existing `ReflectionRealProxy` / Moq
seams. Production code is byte-identical to remediation cycle 1; cycle 2 added only `[DoNotParallelize]`
test attributes, which add no production lines and cannot change the coverage denominator.

### 1.1 C# coverage verdict (verified from `artifacts/csharp/coverage.xml` and evidence artifacts)

| Row | Baseline | Post-change | Change | Disposition | Evidence | New/changed-code coverage | Verdict |
|---|---|---|---|---|---|---|---|
| C# new/changed executable-code line coverage (the binding numeric gate) | new lines | 14/14 = 100% | +14 covered / 0 uncovered | Above the 90% new-code target and the 80% line floor; every added executable line is exercised (hits=1) | coverage-delta.2026-07-09T16-05.md; final-test-coverage.2026-07-09T16-05.md; re-anchored below | 100% | PASS |
| C# changed call-site line coverage (`StoresWrapper.cs` L44/L89 refactored to `MaterializeFilteredStores()`) | covered | covered (hits=1) | no regression | No previously-covered line became uncovered | coverage-delta.2026-07-09T16-05.md | changed lines covered | PASS |
| C# repository-wide line coverage on the testable denominator | 81.80% (121602/148653) | 81.80% (121602/148653) | 0.00 pp | At or above the 80% CLAUDE.md floor; unchanged (cycle-2 attributes add no production lines) | final-test-coverage.2026-07-09T16-05.md; coverage-delta.2026-07-09T16-05.md | testable-denominator repo aggregate | PASS |
| C# touched-assembly (`UtilitiesCS`) line coverage | 88.36% | 88.33% | -0.03 pp (measurement noise) | At or above the 80% floor; the -0.03 pp is within known `dotnet-coverage` denominator run-to-run variance and adds no production lines | coverage-delta.2026-07-09T16-05.md | assembly aggregate | PASS |

Baseline: 81.80% repository-wide testable-denominator line coverage. Post-change: 81.80% repository-wide
testable-denominator line coverage. Disposition: no regression; the binding new/changed-code figure is 100%
and the repository floor holds.

Independent re-anchoring (this reviewer, from the committed `artifacts/csharp/coverage.xml`, the Visual
Studio merged `.coverage` XML):

- `StoresWrapper.MaterializeFilteredStores()` — `line_coverage="100.00"` (verified via direct grep of the
  committed artifact). The new helper is fully covered.
- `StoreLockupResponder.OnLockupDetected(...)` — the new phase-identity branch lines are covered by T3; the
  method's 2 uncovered lines belong to the pre-existing disable/notify path, not the added phase branch.
- `CurrentStoreContext.StoresEnumerationPhaseIdentity` is a compile-time `const` with no executable IL; it
  carries no coverable sequence point and is legitimately outside the executable denominator per the
  type-only/no-executable-behavior clarification in `general-unit-test.md`. Its value is exercised by T1/T2/T3
  and by both production call sites.

### 1.2 Testable-denominator vs raw all-module note (C# coverage PASS)

The committed raw all-DLL Cobertura merge (`artifacts/csharp/coverage.postchange.cobertura.xml`) reads a
root line-rate of 51.49% over a 215016-line denominator. That figure is not the repository testable
denominator: its denominator includes vendored/third-party assemblies and the COM/VSTO/WinForms and
Outlook-interop assemblies that CLAUDE.md formally exempts from the 80% floor (a vendor-inflated
denominator). The testable-denominator figure — produced with `coverage.config` first-party exclusions — is
81.80% (121602/148653), which clears the 80% floor. This change adds only host-neutral, fully-covered lines
and cannot reduce the testable-denominator rate; the binding numeric gate for this change is the 100%
new/changed-code line coverage above. C# coverage verdict: PASS.

The SubagentStop coverage hook (`validate-feature-review-coverage.ps1`) parses `artifacts/csharp/coverage.xml`
with a JaCoCo `//counter[@type="LINE"]` selector. That artifact is Visual Studio merged `.coverage` XML
(Cobertura companions have no JaCoCo `counter` nodes), so the hook's repo-wide and branch parse both yield
`$null`, its 85%-floor and 75%-branch-floor branches do not fire, and it then requires only an explicit C#
coverage PASS/FAIL row with no scope-narrowing phrase — which sections 1.1/1.2 supply (verdict: PASS).

Branch coverage: CLAUDE.md's authoritative C# policy gates line coverage (>= 80% testable denominator, >= 90%
new code) and does not separately gate branch coverage; the raw all-module Cobertura branch-rate (59.66%) is
dominated by the same exempted vendored/COM assemblies and is directional only. The new branch introduced by
this change (the phase-identity guard) is exercised by T3 (`MockBehavior.Strict`, zero disable calls). No
new-code branch is left uncovered.

## 2. Toolchain Compliance (C# loop, run in order; CI-equivalent full-suite test)

| Stage | Command | Result | Evidence | Verdict |
|---|---|---|---|---|
| 1. Format | `dotnet tool run csharpier check .` | EXIT 0; 1318 files checked; CLEAN, no reformatting | final-format.2026-07-09T16-05.md | PASS |
| 2. Analyzers | `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` | EXIT 0; Build succeeded; 0 Error(s), 0 Warning(s) | final-analyzer.2026-07-09T16-05.md | PASS |
| 3. Nullable / type-check | `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true` | EXIT 0; Build succeeded; 0 Error(s), 0 Warning(s) | final-nullable.2026-07-09T16-05.md | PASS |
| 4. Test + coverage (CI-equivalent) | `vstest.console.exe <all 7 *.Test.dll> /EnableCodeCoverage /InIsolation /Logger:trx /TestCaseFilter:"TestCategory!=LiveOutlook"` | EXIT 0; 5141/5141 pass, 0 fail; deterministic across 4 definitive + 7 confirmation passes | final-test-coverage.2026-07-09T16-05.md; green-after-fix.2026-07-09T16-05.md | PASS |

The recorded commands match the CLAUDE.md C# toolchain (CUT3). The remediation-cycle-1 fix is validated by a
4-pass deterministic green proof (plus 7 confirmation passes) and a git-stash causation-separation diagnostic
that reproduced the pre-fix race (10 failures in 1 of 4 pristine-baseline passes) and confirmed its removal.

## 3. Design & Structure Policy (General + C#)

| Check | Finding | Verdict |
|---|---|---|
| File size <= 500 lines (all new/modified) | `StoresWrapperEnumerationScopeTests.cs` 320 (new); `StoreLockupResponderTests.cs` 242; `StoresWrapper.cs` 469; `StoreLockupResponder.cs` 158; `CurrentStoreContext.cs` 98; the 8 `[DoNotParallelize]`-edited files each +1 line (largest, `StoresWrapperTests.cs`, 467) | PASS |
| Test-file split to respect 500-line cap | New sibling file `StoresWrapperEnumerationScopeTests.cs` created rather than extending `StoresWrapperTests.cs` (466 -> 467) | PASS |
| Every new `.cs` wired into its `.csproj` | `StoresWrapperEnumerationScopeTests.cs` explicitly added to `TaskMaster.Test.csproj` `<Compile>` group (verified in diff) | PASS |
| Separation of concerns (observational logic vs I/O) | The enumeration-phase scope is observational only; the responder phase branch performs no disable-service write | PASS |
| No new public API surface / no breaking change | `StoresWrapper.MaterializeFilteredStores` is `private`; `CurrentStoreContext` adds one `const`; `StoreLockupResponder` adds one terminal branch | PASS |
| Error handling: `using` scope restore-on-failure | `MaterializeFilteredStores` uses `using (CurrentStoreContext.Begin(...))`, guaranteeing ambient-value restore on normal completion and on a thrown exception (verified by T5) | PASS |
| Guard-ordering invariant | Responder guard order is blank -> unresolved -> phase-identity -> already-disabled -> disable/notify; the phase-identity guard precedes every `IStoreDisableService` call (verified in source and by T3) | PASS |
| Logging pattern | Phase branch emits via the injected `logSink` using the existing `StoreLockupAttribution.FormatLine(autoDisabled:false)` overload | PASS |
| XML docs on non-obvious members | `MaterializeFilteredStores`, the `StoresEnumerationPhaseIdentity` const, and the responder phase branch carry explanatory XML docs / comments citing #292 rationale | PASS |
| Naming conventions | `MaterializeFilteredStores`, `StoresEnumerationPhaseIdentity` follow C# conventions | PASS |

## 4. Test Policy (General + C# Unit Test)

| Check | Finding | Verdict |
|---|---|---|
| MSTest + Moq + FluentAssertions | All three used; `[TestClass]`/`[TestMethod]`; `Mock<IStoreDisableService>(MockBehavior.Strict)`; FluentAssertions `.Should()` | PASS |
| No live Outlook, no temp files | Existing `ReflectionRealProxy` / `Mock<Stores>().As<IEnumerable>()` seams and `StubApplicationGlobals`; no filesystem use | PASS |
| Determinism (no sleeps/real timers/wall-clock) | T1/T2/T4/T5 use no clock; T3 passes an explicit `TimeSpan.FromSeconds(6)`; no banned timing APIs | PASS |
| Test-isolation under parallelization (cycle-1 remediation) | 8 `UtilitiesCS.Test` scope-opening classes now carry `[DoNotParallelize]`; the process-global-static race is removed by structural mutual exclusion, proven deterministic across 4+ passes; completeness gate confirms zero unmarked scope-opening classes in `UtilitiesCS.Test` | PASS |
| RED-before-GREEN discipline | T1/T2/T3 RED on HEAD (EXIT 1) then GREEN after fix; T4/T5 GREEN before and after | PASS |
| Scenario completeness | Attribution parity at both sites (T1/T2), responder phase branch with zero disable calls (T3), behavior preservation (T4), scope-restore-on-failure (T5) | PASS |
| Strict-mock verification of the crash-safe contract | T3 uses `MockBehavior.Strict` and `VerifyNoOtherCalls()` to assert zero `IStoreDisableService` interactions | PASS |
| Test file location mirrors source | `TaskMaster.Test/OutlookObjects/Store/...` and `UtilitiesCS.Test/Threading/...` mirror production paths | PASS |
| `TaskMaster.Test` isolation parity for the new reader class | The new `StoresWrapperEnumerationScopeTests` is a `CurrentStoreContext` null-baseline reader/writer but is NOT marked `[DoNotParallelize]` — see the non-blocking Major finding in section 7 | PARTIAL (non-blocking) |

## 5. Workflow / CI-Gate Policy

`modified-workflow-needs-green-run` does not apply: the branch diff contains no paths matching
`.github/workflows/**`, `scripts/benchmarks/**`, or `.github/actions/**` (verified via `git diff --numstat`).
No benchmark baseline files are added, so `benchmark-baselines.md` does not apply. No Blocking finding from
this section.

## 6. Evidence Location Compliance

All feature evidence is written under the canonical
`docs/features/active/2026-07-09-outlook-startup-store-enumeration-com-stall-292/evidence/{baseline,remediation-baseline,qa-gates,regression-testing,issue-updates,other}/`
tree per `evidence-and-timestamp-conventions`. The branch diff contains no files under `artifacts/baselines/`,
`artifacts/qa/`, `artifacts/evidence/`, or `artifacts/coverage/`. No evidence-location violations. The
`artifacts/csharp/coverage.xml` and `coverage.*.cobertura.xml` collection outputs sit at the canonical
coverage path and are gitignored build outputs, not committed branch-diff files. No
`EVIDENCE_LOCATION_OVERRIDE_REJECTED` events occurred.

## 7. Non-Blocking Findings and Observations

1. Major (non-blocking) — `TaskMaster.Test` test-determinism robustness gap. The new regression class
   `TaskMaster.Test/OutlookObjects/Store/StoresWrapperEnumerationScopeTests.cs` both opens a
   `CurrentStoreContext` scope (via `StoresWrapper.Init()`/`RewireAfterDeserializeAsync()` ->
   `MaterializeFilteredStores` -> `Begin`) and asserts `CurrentStoreContext.Current` is null (T4 line 115,
   T5 line 136), yet is not marked `[DoNotParallelize]`. This is a second instance of the same
   process-global-static defect class that cycle 1 fixed in `UtilitiesCS.Test`. Assessed against the
   repository test-determinism policy (`general-unit-test.md`: "given the same inputs and environment, tests
   must produce the same results; avoid flakiness"): under the actual required CI invocation
   (`vstest ... /EnableCodeCoverage /InIsolation`, no `/Settings:`), `TaskMaster.Test` has no
   `[assembly: Parallelize]` and runs its classes sequentially, so the class is race-free — confirmed by 5
   green post-fix full-suite passes and by CI run 29046195330 showing zero `TaskMaster.Test` failures. The
   two intermittent failures appear only under the developer VS Code coverage runsettings
   (`scripts/vscode/TaskMaster.cli.runsettings`), which force `Workers=0/ClassLevel` parallelization on every
   assembly. Severity is Major, not Blocking: it does not fail the required CI check and manifests only under
   a non-gate coverage-measurement path. It is a latent robustness gap because the sequential-run safety is an
   incidental configuration fact (absence of `[assembly: Parallelize]`), not a designed isolation guarantee,
   and the team already marks the sibling reader `AppOlObjectsAttributionContextTests` `[DoNotParallelize]`.
   Recommended follow-up (non-gating): add `[DoNotParallelize]` to `StoresWrapperEnumerationScopeTests` and
   audit sibling `TaskMaster.Test` writers (`StoresWrapperTests`, `AppOlObjectsTests`,
   `AppOlObjectsCoverageTests`), extending the cycle-1 approach (A) to this assembly. Executor evidence:
   `evidence/other/out-of-scope-finding-taskmaster-test-race.2026-07-09T16-05.md`.
2. Observation — `OnLockupDetected` shows 2 uncovered lines in the merged `.coverage` XML; these belong to
   the pre-existing disable/notify path, not the added phase branch, and are outside this change's scope.
3. Observation — the committed raw all-DLL Cobertura merge (51.49% root) uses a vendor-inflated denominator
   and is not the testable-denominator figure; the deterministic binding gates (all tests pass; 100%
   new-executable-line coverage; no changed-line regression; 81.80% testable-denominator repo-wide) are the
   authoritative evidence and are met.

## Appendix A — Coverage Artifact Provenance

- C# coverage artifact consumed: `artifacts/csharp/coverage.xml` (Visual Studio merged `.coverage` XML,
  ~40 MB), produced by the `/EnableCodeCoverage /InIsolation` run; per-line Cobertura companions
  `coverage.baseline.cobertura.xml` and `coverage.postchange.cobertura.xml`.
- New-method line coverage independently re-anchored from `artifacts/csharp/coverage.xml`:
  `StoresWrapper.MaterializeFilteredStores()` = `line_coverage="100.00"` (direct grep of the committed
  artifact by this reviewer).
- Repository-wide testable-denominator line coverage 81.80% (121602/148653) from
  `final-test-coverage.2026-07-09T16-05.md` (`dotnet-coverage collect --settings coverage.config` first-party
  filter). Raw all-DLL Cobertura root 51.49% (110722/215016) is vendor-inflated and non-authoritative.
- The SubagentStop hook's `artifacts/csharp/coverage.xml` JaCoCo `LINE`/`BRANCH` parse returns `$null` on
  this Visual Studio `.coverage` XML; repo-wide floor enforcement for the testable denominator is deferred to
  the PR CI run per CLAUDE.md, and the binding per-change gate (100% new-code coverage) is met locally.

## Appendix B — Command Reference

- Base/head/merge-base: `git merge-base HEAD origin/main` -> `c9ddbf28...`; `git rev-parse HEAD` -> `8f391d8f...`
- Scope: `git diff --numstat c9ddbf28..8f391d8f`; `git diff --name-status c9ddbf28..8f391d8f -- '*.cs' '*.csproj'`
- Format: `dotnet tool run csharpier check .`
- Analyzers: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
- Nullable: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`
- Test + coverage (CI-equivalent): `vstest.console.exe <all 7 *.Test.dll> /EnableCodeCoverage /InIsolation /Logger:trx /TestCaseFilter:"TestCategory!=LiveOutlook"`
- Coverage re-anchoring: grep of `MaterializeFilteredStores` `line_coverage` in `artifacts/csharp/coverage.xml`; root line-rate in `artifacts/csharp/coverage.postchange.cobertura.xml`

## Verdict

PASS. Zero Blocking findings. One Major non-blocking finding (TaskMaster.Test determinism robustness gap)
and two observations recorded. C# coverage verdict: PASS.
