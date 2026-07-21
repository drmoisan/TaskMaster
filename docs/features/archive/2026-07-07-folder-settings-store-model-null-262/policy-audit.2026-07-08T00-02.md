# Policy Audit — Issue #262 (folder-settings-store-model-null), Epic #260 F2

- Feature folder: `docs/features/active/2026-07-07-folder-settings-store-model-null-262/`
- Work mode: `full-bug` (AC source = `spec.md`, AC1–AC7)
- Branch: `bug/folder-settings-store-model-null-262`
- Base branch (epic child): `origin/epic/store-lockup-resilience-integration`
- Merge-base: `8bd91d1d5db08400a47e04b141bf4a2c4c4a9a82`
- Diff command: `git diff origin/epic/store-lockup-resilience-integration...HEAD`
- Timestamp: 2026-07-08T00-02
- Overall verdict: PASS (blocking findings: 0)

## Executive Summary

The change is a C# bug fix confined to the four permitted files. It restructures
`AppOlObjects.LoadStoresAsync` so both recoverable null paths fall back to a fresh live-store build,
wraps the method in a bounded try/catch, and extracts the store-loading concern into a new partial
for the 500-line file-size cap. The bugfix workflow (RED-before, GREEN-after) is evidenced, the full
C# toolchain passes in order, new/changed-code coverage is 100%, and there is no coverage regression.
No policy violation rises to a blocking level. Two non-blocking observations are recorded (canonical
C# coverage-artifact deposit path not populated; and a pre-existing CI test-filter observation on an
out-of-scope file).

## Scope Confirmation

Diff (source) confined to exactly the four permitted files:

| File | Status | +/- |
|---|---|---|
| `TaskMaster/AppGlobals/AppOlObjects.cs` | Modified | +0/-30 |
| `TaskMaster/AppGlobals/AppOlObjects.StoreLoading.cs` | Added | +75/-0 |
| `TaskMaster/TaskMaster.csproj` | Modified | +1/-0 |
| `TaskMaster.Test/AppGlobals/AppOlObjectsCoverageTests.cs` | Modified | +203/-3 |

Prohibited files verified UNCHANGED (absent from `git diff` and `git status`), corroborated by
`evidence/other/scope-lock-confirmation.md`:
`UtilitiesCS/OutlookObjects/Store/StoresWrapper.cs` (F1 #261),
`UtilitiesCS/EmailIntelligence/IntelligenceConfig.cs`,
`UtilitiesCS/OutlookObjects/Store/StoreWrapperController.cs`,
`TaskMaster.Test/AppGlobals/AppOlObjectsTests.cs`.

All other working-tree changes are inside the feature folder (docs, plan, spec, evidence). Scope lock
holds.

## Rejected Scope Narrowing

None. The caller (orchestrator) prompt framed the full-branch audit against the epic integration base
and did not attempt to narrow coverage or skip any language with changed files. The two adjudication
items the caller raised (pre-existing LiveHookup failure; repo-wide fresh-recompute limitation) are
change-independent conditions, adjudicated explicitly below, not scope-narrowing directives.

## Evidence Location Compliance

Scan of the branch diff for files written under `artifacts/baselines/`, `artifacts/qa/`,
`artifacts/evidence/`, or `artifacts/coverage/`: none found. All evidence artifacts are under the
canonical `docs/features/active/<feature>/evidence/<kind>/` tree (baseline, qa-gates,
regression-testing, issue-updates, other). No `EVIDENCE_LOCATION_OVERRIDE_REJECTED` events. PASS.

## Toolchain Compliance (CLAUDE.md C# order: format -> analyzers -> nullable/type -> test)

| Stage | Command | Result | Evidence | Verdict |
|---|---|---|---|---|
| Format | `csharpier check .` | 0 files need reformatting (1278 checked) | qa-01-format.md | PASS |
| Analyzers | `msbuild /t:Rebuild ... /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` | 0 errors, 72 warnings = baseline (no new diagnostics on touched files) | qa-02-analyzers.md | PASS |
| Nullable/Type | `msbuild /t:Build ... /p:Nullable=enable /p:TreatWarningsAsErrors=true` | 0 warnings, 0 errors (matches baseline 0/0) | qa-03-nullable.md | PASS |
| Test + coverage | `vstest.console.exe TaskMaster.Test.dll` (Cobertura runsettings, `/InIsolation`) | 202/203 pass; sole failure is env-dependent live-COM test | qa-04-test-coverage.md, full-suite-after-262.md | PASS |

## Bugfix Workflow Compliance

| Requirement | Evidence | Verdict |
|---|---|---|
| Failing regression tests precede the fix (RED) | fail-before-262.md: 3 tests, 0 passed / 3 failed, EXIT 1; Phase-1 production code byte-for-byte original | PASS |
| Tests pass after the fix (GREEN) | pass-after-262.md: 4 tests, 4 passed / 0 failed, EXIT 0 | PASS |
| Minimal, targeted fix; boundaries intact | Source diff restructures only `LoadStoresAsync`, adds `BuildFreshStoresWrapper` seam; controller and `StoresWrapper.cs` untouched | PASS |
| MSTest + Moq + FluentAssertions | Touched test uses `[TestMethod]`, `Mock<...>`, `.Should()` throughout | PASS |
| No temp files / no live Outlook in unit tests | COM chain fully mocked (`Mock<Stores>`/`Mock<NameSpace>`/`Mock<Store>`); no `Path.GetTemp`/`File.*`/`Directory.Create`; grep clean | PASS |

## Unit-Test Policy Compliance (General + C#)

| Rule | Observation | Verdict |
|---|---|---|
| Determinism / banned APIs in touched test | No `Thread.Sleep`, `Task.Delay`, `DateTime.Now/UtcNow`, `Stopwatch`, `new Random()` in `AppOlObjectsCoverageTests.cs` | PASS |
| Independence / isolation / AAA | Each test arranges its own stubs; explicit Arrange/Act/Assert comment blocks; single behavior per test | PASS |
| Framework selection (MSTest, no xUnit/NUnit) | MSTest attributes only | PASS |
| Clear intent / failure messages | Descriptive names + reason strings on `.Should().Be(0, "...")` assertions | PASS |
| Test-file location mirrors source | `TaskMaster.Test/AppGlobals/...` mirrors `TaskMaster/AppGlobals/...` | PASS |

## File-Size Cap (General Code Change Policy: <= 500 lines)

| File | Head line count | Verdict |
|---|---|---|
| `AppOlObjects.cs` | 495 | PASS |
| `AppOlObjects.StoreLoading.cs` (new) | 75 | PASS |
| `AppOlObjectsCoverageTests.cs` | 344 (baseline 144) | PASS (under 500) |

The changed test file grew from 144 to 344 lines; it remains under the 500-line cap, so no file-size
finding. AC6 (both production files <= 500) is satisfied.

## Coverage Verification

Repo C# coverage policy (CLAUDE.md governs for C#): repository-wide `>= 80%` on the testable
denominator (COM/VSTO/WinForms/Interop members exempted), new/changed code `>= 90%`, and no regression
on changed lines. This supersedes the 85/75 general-tier numbers for C#.

Coverage-artifact note: the canonical automated-gate path `artifacts/csharp/coverage.xml` (JaCoCo
schema expected by the review hook) was not deposited this cycle. C# coverage was instead measured
through the repo-standard `vstest.console.exe` Cobertura-runsettings path and recorded with concrete
figures in the evidence tree (qa-04, qa-05, full-suite-after-262). The figures below are read from
that evidence.

### C# (.NET / CSharp) coverage — verdicts

C# (dotnet) coverage verdict: new/changed-code line and branch coverage 100% (>= 90% target) PASS; no-regression on changed lines PASS; repo-wide testable-denominator no-regression PASS.

- C# new/changed-code coverage: line 100%, branch 100% (AppOlObjects.StoreLoading.cs: restructured
  `LoadStoresAsync` branches, `BuildFreshStoresWrapper` seam, `LoadAsync`, `StoresWrapper`,
  `AwaitStoreRewireAsync`). Target `>= 90%`. Verdict: PASS. Evidence: qa-04-test-coverage.md,
  qa-05-coverage-delta.md, full-suite-after-262.md.
- C# no-regression on changed lines (dotnet): TaskMaster production package line-rate rose from
  63.64% baseline to 63.92% post-change; the `LoadStoresAsync` state machine held 100% line/branch
  across the restructure; moved members remained fully covered. Verdict: PASS. Evidence:
  qa-05-coverage-delta.md check (a).
- C# repo-wide testable-denominator coverage (dotnet): the raw TaskMaster project line-rate of 63.92%
  sits below the 80% floor only because it includes COM/VSTO/WinForms-bound `AppOlObjects` members
  that are policy-exempt from the testable denominator per CLAUDE.md. The floor is a no-regression
  ("must remain") gate; this change touches only the TaskMaster project and strictly increases its
  coverage, so repo-wide testable-denominator coverage cannot decrease as a result of this change.
  Verdict: PASS (no-regression basis). Evidence: qa-05-coverage-delta.md check (c),
  test-coverage-baseline.md.

Repo-wide fresh-recompute limitation (adjudicated in Item 2 below): a fresh absolute repository-wide
number could not be regenerated this cycle because UtilitiesCS.Test deadlocks the pump-less CLI test
host under coverage collection. This is a pre-existing, change-independent measurement constraint; it
does not lower any change-scope coverage verdict above.

### Other languages

TypeScript, Python, PowerShell: zero changed files on the branch. No coverage obligation.

### Coverage checklist

- [x] TypeScript: no changed files on branch — coverage obligation not applicable to this language.
- [x] Python: no changed files on branch — coverage obligation not applicable to this language.
- [x] PowerShell: no changed files on branch — coverage obligation not applicable to this language.
- [x] C# (.NET): changed files present; new-code 100% (PASS), no-regression (PASS), repo-wide
  testable-denominator no-regression (PASS).

Baseline vs post-change comparison line: Baseline 63.64% -> Post-change 63.92% -> Disposition: PASS
(no regression; +0.28 points; new/changed-code line coverage 100%).

## Findings

| Severity | Finding | Disposition |
|---|---|---|
| LOW (non-blocking) | Canonical `artifacts/csharp/coverage.xml` not deposited; C# coverage evidenced via Cobertura runsettings in the evidence tree instead. | Recommend depositing the canonical artifact in future cycles to enable automated hook-side gating. Does not affect this change's verified coverage obligations. |
| INFORMATIONAL (non-blocking, out of scope) | `.github/workflows/ci.yml:140` invokes `vstest` without `/TestCaseFilter:"TestCategory!=LiveOutlook"`, contradicting the `LiveOutlookHookupIntegrationTests` doc comment that CI excludes that category. | ci.yml and the LiveOutlook test are outside F2's four-file scope and unchanged by this branch. Recommend the epic owner confirm the integration-branch CI is green and/or reconcile the filter. |

## Adjudicated Items

### Item 1 — Pre-existing LiveHookup test failure

`LiveHookup_OnSta_CompletesAndDoesNotBlockStaBeyondThreshold`
(`TaskMaster.Test/AppGlobals/LiveOutlookHookupIntegrationTests.cs`) fails with COMException 0x80010100
RPC_E_SYS_CALL_FAILED because it requires a live Outlook COM class factory absent in the headless
environment. Verdict: NOT a blocking finding for F2. Basis: (a) the test file is outside F2's four
permitted files and is unchanged; (b) it fails identically pre-change (baseline P0-T12 direct-Cobertura
run, 199/200) and post-change (full-suite-after-262, 202/203), so it is change-independent; (c) it
carries `[TestCategory("LiveOutlook")]` and is documented as excluded from the standard QC run via
`/TestCaseFilter:"TestCategory!=LiveOutlook"`. The observation that ci.yml does not currently apply
that filter is recorded as an INFORMATIONAL finding above and is an integration-branch/CI-config matter
that F2 cannot address within scope.

### Item 2 — Repo-wide testable-denominator fresh recomputation limitation

A fresh absolute repository-wide testable-denominator percentage could not be regenerated this cycle
because UtilitiesCS.Test hard-deadlocks the pump-less CLI test host under coverage collection (stalls
~3883/3907; MSTest TestTimeout cannot abort a synchronous STA-pump deadlock) and the offline
`.coverage`->Cobertura merge yields an empty report. Verdict: NOT a change-introduced blocker. Basis:
the limitation is pre-existing and change-independent (documented at the P0-T12 baseline; CI computes
no percentage gate); the change is confined to the TaskMaster project and strictly increases its
coverage, so repo-wide testable-denominator coverage cannot regress; and the two obligations that
actually gate this change — new/changed-code coverage (100% >= 90%) and no-regression on changed lines
— are measured precisely from TaskMaster.Test. Classification: pre-existing measurement limitation with
all change-scope coverage gates passing.

## Verdict

PASS. Zero blocking findings. Two non-blocking observations recorded for the epic owner.
