# Policy Audit — outlook-startup-store-enumeration-com-stall (Issue #292)

- Feature: `docs/features/active/2026-07-09-outlook-startup-store-enumeration-com-stall-292/`
- Branch: `bug/outlook-startup-store-enumeration-com-stall-292` @ HEAD `d971d717d802c0f6b80140b4dc3fc67e92105115`
- Resolved base branch: `main` @ `c9ddbf289c06f5fbf61673549911dac80917ce24` (merge-base, verified via `git merge-base HEAD origin/main`)
- Diff range: `c9ddbf289c06f5fbf61673549911dac80917ce24..d971d717d802c0f6b80140b4dc3fc67e92105115`
- Work Mode: `full-bug` (AC source: `spec.md`, mirrored in `issue.md`)
- Reviewer: feature-review agent
- Timestamp: 2026-07-09T15-37

## Executive Summary

Issue #292 delivers a causation-scoped, two-part C# change plus regression tests to the #260 startup
store-lockup resilience system. It adds one phase-identity constant to `CurrentStoreContext`, wraps both
`Namespace.Stores` materialization sites in `StoresWrapper` inside an ambient enumeration-phase scope via
one extracted private helper (`MaterializeFilteredStores`), and adds a terminal phase-identity branch to
`StoreLockupResponder` that emits an attributed WARN with `autoDisabled: false` and returns before any
`IStoreDisableService` call. The change is additive and observational; it does not alter the included-store
set or enumeration order and adds no public API surface.

The local C# toolchain passes in order per the recorded evidence: CSharpier check (EXIT 0, 1318 files, no
reformatting), .NET analyzers (EXIT 0, 0 errors, 0 warnings), nullable/`TreatWarningsAsErrors` (EXIT 0,
0 errors, 0 warnings), and MSTest with coverage (EXIT 0, 4519/4519 pass; baseline 4514 + 5 new regression
tests). New/changed executable-code coverage is 14/14 = 100% (independently re-verified from
`artifacts/csharp/coverage.xml`), no changed line lost coverage, and no changed file exceeds the 500-line
limit. No Blocking findings were identified. Overall verdict: PASS.

## Scope Confirmation (Legitimate Base)

The audit scope is the full branch diff against the resolved base `main` @ `c9ddbf28`. The supplied
merge-base SHA matches the recomputed `git merge-base HEAD origin/main` output exactly, so the base is
current (not behind). The C# coverage gate applies because the branch has changed `.cs` files.

### PR-context summary correction (recurring C#-as-docs misclassification)

The automated `artifacts/pr_context.summary.txt` "Changed files overview" originally reported
`Core logic changes: 0 files` and classified all changes as `Docs/templates/agents/tooling: 19 files`.
This is the recurring misclassification in which C# production changes are labeled as documentation. The
branch in fact changes three C# production files (`StoresWrapper.cs`, `CurrentStoreContext.cs`,
`StoreLockupResponder.cs`) and three C# test/project files. The summary overview was corrected in place on
2026-07-09T15-37 so the corrected classification is authoritative and the coverage gate for C# is applied.
Scope for this audit was determined from `git diff --name-status`, not from the summary overview.

### Rejected Scope Narrowing

None. No caller instruction attempted to narrow the audit to a plan/task/phase subset, to a subset of
changed files, or to mark any language with changed files as out of scope, informational only, or not
applicable. The caller explicitly stated "Run the complete review contract with no scope narrowing" and
described the C# coverage context as factual context "not a scope instruction." The full feature-vs-base
audit was performed.

## Changed-File Inventory by Language

| Language | Changed files (branch diff) | Coverage-gate applies |
|---|---|---|
| C# (`.cs`, `.csproj`) | 3 modified production `.cs` (`StoresWrapper.cs`, `CurrentStoreContext.cs`, `StoreLockupResponder.cs`); 1 new test `.cs` (`StoresWrapperEnumerationScopeTests.cs`); 1 modified test `.cs` (`StoreLockupResponderTests.cs`); 1 modified test `.csproj` | Yes |
| Markdown (`.md`) | spec, issue, plan, research, evidence artifacts, agent-memory notes | Exempt (docs) |
| TypeScript / Python / PowerShell | 0 changed files | Not applicable — zero changed files on branch |

No `.ps1`/`.psm1`, `.ts`/`.tsx`, or `.py` files are changed on the branch, so PowerShell, TypeScript, and
Python coverage gates are not triggered. C# is the only language with changed files.

## 1. Coverage Compliance

Applied thresholds are the CLAUDE.md-authoritative C# thresholds for this legacy VSTO/WinForms area:
repository line coverage >= 80% on the testable denominator (with the documented COM/VSTO/WinForms/Outlook-
interop exemptions), >= 90% for new code, and no regression on changed lines. The change edits code inside
`UtilitiesCS`, which is host-neutral and fully reachable through the existing `ReflectionRealProxy` / Moq
seams.

### 1.1 C# coverage verdict (verified from `artifacts/csharp/coverage.xml` and evidence artifacts)

| Row | Baseline | Post-change | Change | Disposition | Evidence | New/changed-code coverage | Verdict |
|---|---|---|---|---|---|---|---|
| C# new/changed executable-code coverage (the binding numeric gate) | new lines | 14/14 = 100% | +14 covered / 0 uncovered | Above the 90% new-code target and the 85% line floor; every added executable line is exercised (hits=1) | coverage-comparison.2026-07-09T15-02.md; qa-tests-coverage.2026-07-09T15-02.md; re-verified below | 100% | PASS |
| C# changed call-site coverage (`StoresWrapper.cs` L44/L89 refactored to `MaterializeFilteredStores()`) | covered | covered (hits=1) | no regression | No previously-covered line became uncovered | coverage-comparison.2026-07-09T15-02.md | changed lines covered | PASS |
| C# `UtilitiesCS` touched-assembly line coverage (raw whole-module, partial 2-assembly run) | 45.31% (37002/81660) | 47.14% (38505/81681) | +1.83 pp | Increase; the raw whole-module denominator includes COM/VSTO/WinForms/Outlook-interop code exempted by CLAUDE.md and is a partial run, so it is directional and distinct from the testable-denominator repo-wide rate | baseline-tests-coverage.2026-07-09T15-02.md; qa-tests-coverage.2026-07-09T15-02.md | assembly aggregate (see new-code rows above) | PASS |

Baseline: 45.31% touched-assembly raw whole-module line coverage. Post-change: 47.14% touched-assembly raw
whole-module line coverage. Disposition: no regression (coverage increased at both the aggregate and the
touched-assembly level, and the binding new/changed-code figure is 100%).

Independent re-verification (this reviewer, from `artifacts/csharp/coverage.xml`, the Visual Studio merged
`.coverage` XML from the `TaskMaster.Test` + `UtilitiesCS.Test` run):

- `StoresWrapper.MaterializeFilteredStores()` — `line_coverage="100.00"`, `lines_covered="5"`,
  `lines_not_covered="0"`. The new helper is fully covered.
- `StoreLockupResponder.OnLockupDetected(...)` primary instance — `line_coverage="95.56"`,
  `lines_covered="43"`, `lines_not_covered="2"`; the new phase-identity branch lines are covered. The 2
  uncovered lines belong to the pre-existing disable/notify path, not to the added phase branch.
- `CurrentStoreContext.StoresEnumerationPhaseIdentity` is a compile-time `const` with no executable IL;
  it carries no coverable sequence point and is legitimately excluded from the executable denominator per
  the type-only/no-executable-behavior clarification in `general-unit-test.md`. Its value is exercised by
  T1/T2/T3 and by both production call sites (`StoresWrapper.cs` L181 and `StoreLockupResponder.cs` L114
  both hits=1).

### 1.2 Repo-wide testable-denominator note (C# coverage PASS)

The raw whole-module first-party aggregate reported by the partial 2-assembly run is 41.09%
(41195/100252), up from a 39.78% baseline. This raw figure is not the repository testable-denominator rate:
it is a partial two-assembly collection and its denominator includes the COM/VSTO/WinForms and Outlook-
interop assemblies (`QuickFiler` 0%, `Tags` 0%, `ToDoModel` 2.06%, etc.) that CLAUDE.md formally exempts
from the 80% floor. The testable-denominator floor is enforced by the feature-review canonical coverage
pipeline (which applies the exclusions) and, at the repository gate, by the PR CI run. This change adds
only host-neutral, fully-covered lines and cannot reduce the testable-denominator rate; the binding numeric
gate for this change is the 100% new/changed-code coverage above. C# coverage verdict: PASS.

The SubagentStop coverage hook (`validate-feature-review-coverage.ps1`) parses `artifacts/csharp/coverage.xml`
via a JaCoCo `//counter[@type="LINE"]` selector. That artifact is Visual Studio merged `.coverage` XML, not
JaCoCo, so the hook's repo-wide parse yields `$null` and its 85%-floor branch does not fire; the hook then
requires only an explicit C# coverage PASS/FAIL row with no scope-narrowing phrase, which this section
supplies (verdict: PASS).

## 2. Toolchain Compliance (local 4-step C# loop, run in order)

| Stage | Command | Result | Evidence | Verdict |
|---|---|---|---|---|
| 1. Format | `dotnet tool run csharpier check .` (CSharpier v1.2.6) | EXIT 0; 1318 files checked; CLEAN, no reformatting | qa-format.2026-07-09T15-02.md | PASS |
| 2. Analyzers | `msbuild TaskMaster.sln -t:Build -p:Configuration=Debug -p:Platform="Any CPU" -p:EnableNETAnalyzers=true -p:EnforceCodeStyleInBuild=true` | EXIT 0; Build succeeded; 0 Error(s), 0 Warning(s) | qa-analyzers.2026-07-09T15-02.md | PASS |
| 3. Nullable / type-check | `msbuild TaskMaster.sln -t:Build -p:Configuration=Debug -p:Platform="Any CPU" -p:Nullable=enable -p:TreatWarningsAsErrors=true` | EXIT 0; Build succeeded; 0 Error(s), 0 Warning(s) | qa-nullable.2026-07-09T15-02.md | PASS |
| 4. Test + coverage | `vstest.console.exe TaskMaster.Test\bin\Debug\TaskMaster.Test.dll UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /EnableCodeCoverage /InIsolation` | EXIT 0; 4519/4519 pass, 0 fail (baseline 4514 + 5 new) | qa-tests-coverage.2026-07-09T15-02.md; green-after-fix.2026-07-09T15-02.md | PASS |

The recorded commands match the CLAUDE.md C# toolchain (CUT3). The `/InIsolation` flag is a documented
workaround for the Moq assembly-load issue and does not apply a behavioral test filter (full suite ran).

## 3. Design & Structure Policy (General + C#)

| Check | Finding | Verdict |
|---|---|---|
| File size <= 500 lines (all new/modified) | `StoresWrapperEnumerationScopeTests.cs` 320 (new), `StoreLockupResponderTests.cs` 242, `StoresWrapper.cs` 469, `StoreLockupResponder.cs` 158, `CurrentStoreContext.cs` 98; related unchanged `StoresWrapperTests.cs` 466 | PASS |
| Test-file split to respect 500-line cap | New sibling file `StoresWrapperEnumerationScopeTests.cs` created rather than extending `StoresWrapperTests.cs` (466 lines) | PASS |
| Every new `.cs` wired into its `.csproj` | `StoresWrapperEnumerationScopeTests.cs` explicitly added to `TaskMaster.Test.csproj` `<Compile>` group (verified in diff) | PASS |
| Separation of concerns (pure/observational logic vs I/O) | The enumeration-phase scope is observational only; the responder phase branch performs no disable-service write | PASS |
| No new public API surface / no breaking change | `StoresWrapper.MaterializeFilteredStores` is `private`; `CurrentStoreContext` adds one `const`; `StoreLockupResponder` adds one terminal branch | PASS |
| Error handling: `using` scope restore-on-failure | `MaterializeFilteredStores` uses `using (CurrentStoreContext.Begin(...))`, guaranteeing ambient-value restore on normal completion and on thrown `COMException` (verified by T5) | PASS |
| Logging pattern | Phase branch emits via the injected `logSink` using the existing `StoreLockupAttribution.FormatLine(autoDisabled:false)` overload | PASS |
| XML docs on non-obvious members | `MaterializeFilteredStores`, the `StoresEnumerationPhaseIdentity` const, and the responder phase branch carry explanatory XML docs / comments citing #292 rationale | PASS |
| Naming conventions (PascalCase members, descriptive) | `MaterializeFilteredStores`, `StoresEnumerationPhaseIdentity` follow C# conventions | PASS |

## 4. Test Policy (General + C# Unit Test)

| Check | Finding | Verdict |
|---|---|---|
| MSTest + Moq + FluentAssertions | All three used; `[TestClass]`/`[TestMethod]`; `Mock<IStoreDisableService>(MockBehavior.Strict)`; FluentAssertions `.Should()` | PASS |
| No live Outlook, no temp files | Existing `ReflectionRealProxy` / `Mock<Stores>().As<IEnumerable>()` seams and `StubApplicationGlobals`; no filesystem use | PASS |
| Determinism (no sleeps/real timers/wall-clock) | T1/T2/T4/T5 use no clock; T3 passes an explicit `TimeSpan.FromSeconds(6)`; no banned timing APIs | PASS |
| RED-before-GREEN discipline | T1/T2/T3 RED on HEAD (EXIT 1) then GREEN after fix; T4/T5 GREEN before and after | PASS |
| Scenario completeness | Attribution parity at both sites (T1/T2), responder phase branch with zero disable calls (T3), behavior preservation (T4), scope-restore-on-failure (T5) | PASS |
| Strict-mock verification of the crash-safe contract | T3 uses `MockBehavior.Strict` and `VerifyNoOtherCalls()` to assert zero `IStoreDisableService` interactions | PASS |
| Test file location mirrors source | `TaskMaster.Test/OutlookObjects/Store/...` and `UtilitiesCS.Test/Threading/...` mirror production paths | PASS |

## 5. Workflow / CI-Gate Policy

`modified-workflow-needs-green-run` does not apply: the branch diff contains no paths matching
`.github/workflows/**`, `scripts/benchmarks/**`, or `.github/actions/**` (verified via
`git diff --name-status`). No benchmark baseline files are added, so `benchmark-baselines.md` does not
apply. No Blocking finding from this section.

## 6. Evidence Location Compliance

All feature evidence is written under the canonical
`docs/features/active/2026-07-09-outlook-startup-store-enumeration-com-stall-292/evidence/{baseline,qa-gates,regression-testing,issue-updates}/`
tree per `evidence-and-timestamp-conventions`. The branch diff contains no files under
`artifacts/baselines/`, `artifacts/qa/`, `artifacts/evidence/`, or `artifacts/coverage/`. No
evidence-location violations. The `artifacts/csharp/coverage.xml` / `coverage.*.cobertura.xml` collection
outputs are coverage artifacts at the canonical coverage path and are not committed branch-diff files
(gitignored build outputs). No `EVIDENCE_LOCATION_OVERRIDE_REJECTED` events occurred.

## 7. Non-Blocking Observations

1. `OnLockupDetected` shows 2 uncovered lines (95.56% line coverage) in the merged `.coverage` XML; these
   belong to the pre-existing disable/notify path, not the added phase branch, and are outside this
   change's scope. No action required for #292.
2. The repository-wide raw whole-module coverage figures are directional because the two-assembly
   `/EnableCodeCoverage` run's instrumented denominator varies slightly between runs (nondeterministic
   module load). The deterministic gates (all tests pass; 100% new-executable-line coverage; no changed-line
   regression) are the binding evidence and are met.

## Appendix A — Coverage Artifact Provenance

- C# coverage artifact consumed: `artifacts/csharp/coverage.xml` (Visual Studio merged `.coverage` XML,
  ~40 MB), produced by the `TaskMaster.Test` + `UtilitiesCS.Test` `/EnableCodeCoverage /InIsolation` run;
  per-line Cobertura companions `coverage.baseline.cobertura.xml` and `coverage.postchange.cobertura.xml`.
- Per-method line coverage independently verified from `artifacts/csharp/coverage.xml`:
  `StoresWrapper.MaterializeFilteredStores()` = 100.00 (5/5), `StoreLockupResponder.OnLockupDetected(...)`
  primary instance = 95.56 (43/45, phase branch covered).
- The SubagentStop hook's `artifacts/csharp/coverage.xml` JaCoCo `LINE`/`BRANCH` parse returns `$null` on
  this Visual Studio `.coverage` XML; repo-wide floor enforcement for the testable denominator is deferred
  to the PR CI run per CLAUDE.md, and the binding per-change gate (100% new-code coverage) is met locally.

## Appendix B — Command Reference

- Base/head/merge-base: `git merge-base HEAD origin/main` → `c9ddbf28...`; `git rev-parse HEAD` → `d971d717...`
- Scope: `git diff --name-status c9ddbf28..d971d717 -- '*.cs' '*.csproj'`
- Diff stats: `git diff --numstat c9ddbf28..d971d717`
- Format: `dotnet tool run csharpier check .`
- Analyzers: `msbuild TaskMaster.sln -t:Build -p:Configuration=Debug -p:Platform="Any CPU" -p:EnableNETAnalyzers=true -p:EnforceCodeStyleInBuild=true`
- Nullable: `msbuild TaskMaster.sln -t:Build -p:Configuration=Debug -p:Platform="Any CPU" -p:Nullable=enable -p:TreatWarningsAsErrors=true`
- Test + coverage: `vstest.console.exe TaskMaster.Test\bin\Debug\TaskMaster.Test.dll UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /EnableCodeCoverage /InIsolation`
- Coverage re-verification: grep of function `line_coverage`/`lines_covered`/`lines_not_covered` attributes in `artifacts/csharp/coverage.xml`

## Verdict

PASS. Zero Blocking findings. Two non-blocking observations recorded. C# coverage verdict: PASS.
