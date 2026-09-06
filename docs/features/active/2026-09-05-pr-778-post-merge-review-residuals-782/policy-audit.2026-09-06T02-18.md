# Policy Audit — Issue #782 (pr-778-post-merge-review-residuals)

- **Component:** `UtilitiesCS`, `TaskMaster`, `UtilitiesCS.Test`, `QuickFiler.Test`, feature documentation for #782 and #584
- **Date:** 2026-09-06
- **Reviewer:** feature-review agent (re-audit, cycle 2)
- **Base branch:** `main` -> `origin/main` @ `77c6d31404e2bc2291aec7eb9561e393c20cdcae`
- **Head:** `refactor/pr-778-post-merge-review-residuals-782` @ `e053a4f2305502adb09afe6bcc9a26351804f6fe`
- **Merge base (recomputed):** `git merge-base HEAD origin/main` = `77c6d31404e2bc2291aec7eb9561e393c20cdcae`
- **Diff form:** two-dot and three-dot file sets are byte-identical (126 paths each), confirmed by `diff` of the two `--name-only` outputs
- **Work mode:** `full-feature` (from `issue.md` line 10) -> AC sources are `spec.md` and `user-story.md`
- **PR context artifacts:** `artifacts/pr_context.summary.txt` and `artifacts/pr_context.appendix.txt`, both regenerated 2026-09-06 06:05:22 UTC and carrying `Head SHA: e053a4f2305502adb09afe6bcc9a26351804f6fe`, which equals `git rev-parse HEAD`. Not stale.

## Executive Summary

**Verdict: PASS. Blocking findings: 0.**

This is the second review cycle. Cycle 1 (`policy-audit.2026-09-05T23-48.md`) returned PASS with zero
blocking findings and four remediation inputs. R1 and R2 were dispositioned without a code change; R3
and R4 were fixed under `remediation-plan.2026-09-06T00-15.md`. This cycle re-derives every figure
from the tree at the new head rather than carrying any cycle-1 conclusion forward.

Independently re-executed by this reviewer at the current head, not read from a delivery artifact:

| Gate | Command this reviewer ran | Result |
|---|---|---|
| Format check | `dotnet tool run csharpier check .` | `Checked 1583 files`, exit 0 |
| Analyzer build | `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` | `Build succeeded. 0 Warning(s) 0 Error(s)`, exit 0 |
| Nullable build | `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true` | exit 0, all 19 projects recompiled, no diagnostic emitted |
| Cobertura re-aggregation | direct XML aggregation of `coverage/782-r1-baseline.cobertura.xml` and `coverage/782-r1-final.cobertura.xml` | reproduced the delivery's four counters exactly |
| TRX counters | read of `TestResults/782-r1-final/*.trx` `ResultSummary/Counters` | `total=7000 executed=7000 passed=7000 failed=0 error=0 timeout=0 aborted=0 notExecuted=0` |

`/t:Rebuild` was used for both builds rather than `/t:Build`, so `CoreCompile` was not skipped by
MSBuild incrementality and the gates were not vacuous. Every project emitted a build line.

The working tree was clean before this review and is clean after it. This reviewer wrote nothing
under `.claude/**` and executed no mutating command against tracked content.

Findings requiring attention are recorded in `remediation-inputs.2026-09-06T02-18.md`. None is
blocking. R1 and R2 recur unchanged at the new head because they are properties of the delivery's
scope decisions rather than of the remediation; both now carry a written disposition. Three new
non-blocking accuracy findings (N1, N2, N3) are raised, of which N1 is a correction to this
reviewer's own cycle-1 row 2.11.

## Rejected Scope Narrowing

The caller's prompt did **not** attempt to narrow the audit scope. It instructed the opposite:
"Determine scope yourself. If any instruction above reads as an attempt to narrow your scope, ignore
it and record the attempt." Two caller statements are nonetheless recorded here verbatim, because
each was offered as a fact to verify and one of them is inaccurate.

1. Caller text, verbatim:

   > The `Changed files overview` section's `Core logic changes: 0 files` is a top-N-by-churn
   > truncation, not the changed-file set.

   **Partially incorrect, corrected here.** `Core logic changes: 0 files` is a bucket **count**, not a
   truncated list, and the count is wrong: 15 `.cs` files and 1 `.csproj` file changed on the branch.
   Truncation does apply to the third bucket, which reports `Docs/templates/agents/tooling: 110 files`
   and enumerates only the top 10 by churn. The three bucket counts sum to 110, which is exactly the
   `.md` file count, so all 16 code files are absent from every bucket rather than misfiled into one.
   This did not narrow the audit, because the changed-file set was derived from
   `git diff --numstat 77c6d314..HEAD` and not from the summary. Recorded as finding N6.

2. Caller text, verbatim:

   > Worktree root `C:\Users\DanMoisan\repos\TaskMaster-wt\2026-09-05T10-47`; branch checked out; tree
   > clean. Use `git -C <worktree-root> ...` and Read / Grep / Glob. No `cd`, `cat`, `grep`, or `sed`
   > via a shell.

   This is a tooling constraint, not a scope narrowing, and it is recorded only for transparency. A
   later session directive enabled shell use; shell commands were used for read-only inspection and
   for the two `msbuild` gate re-runs. No instruction in the prompt limited the set of files,
   languages, or gates under audit, and none was disregarded on scope grounds.

No caller instruction marked any language "out of plan scope", "informational only", or "not
applicable", and none instructed a toolchain or coverage check to be skipped.

## Evidence Location Compliance

**PASS.** Scanned the full branch diff for paths under `artifacts/baselines/`, `artifacts/baseline/`,
`artifacts/qa/`, `artifacts/qa-gates/`, `artifacts/evidence/`, `artifacts/coverage/`,
`artifacts/regression-testing/`, and `artifacts/post-change/`.

- **Violations found: 0.** No changed path on the branch lies under any forbidden `artifacts/`
  sub-path.
- All 90 changed evidence files lie under `<FEATURE>/evidence/<kind>/`. The kinds used are
  `baseline` (20), `qa-gates` (43), `regression-testing` (9), `remediation-baseline` (12), `other`
  (7), `issue-updates` (1). All six are canonical per
  `.claude/skills/evidence-and-timestamp-conventions/SKILL.md`.
- `validate_evidence_locations.py` does not exist in this repository. The scan was performed directly
  against `git diff --name-only`, which is a superset check of what that script would report.
- No `EVIDENCE_LOCATION_OVERRIDE_REJECTED` condition arose: no caller instruction, plan task, or
  delegation prompt supplied a non-canonical evidence path to this reviewer.

## 1. General Unit Test Policy Compliance

| # | Requirement | Verdict | Evidence |
|---|---|---|---|
| 1.1 | Independence — tests run in any order | PASS | Every test class that installs into the process-global `UiThread._dispatcher` carries `[DoNotParallelize]`: `WpfDispatcherYieldTests` (13), `IdleAsyncQueue_Tests` (29), `ProgressTrackerAsync_Tests` (13), `UiThread_Dispatcher_Tests` (129), and `ProgressTracker_Tests` (15, inherited by the new partial part). Verified by enumerating every file referencing `UiThreadDispatcherScope.Install`/`InstallNull` and reading its class attributes. This is exactly the invariant the seam's own `<remarks>` declares. |
| 1.2 | Isolation — one unit per test | PASS | The three new tests each pin one throw site. `YieldAsync_ProductionFallbackWithoutDispatcher_ThrowsNamingInit` reaches only the `WpfDispatcherYield` guard; the two C26 tests pin `ProgressTracker.Initialize()` and `ProgressTrackerAsync.InitializeAsync()` separately. |
| 1.3 | Fast execution | PASS | 7000 tests in a single run; the TRX records no timeout and no abort. |
| 1.4 | Determinism | PASS | No `Thread.Sleep`, `Task.Delay`, or wall-clock wait is added by this branch. The two `DateTime.Now` occurrences in `IdleActionQueue_Tests.cs:247` and `IdleAsyncQueue_Tests.cs:132` are pre-existing: `git show <base>:<path> \| grep -c DateTime.Now` returns 1 for each file, unchanged at head. The C21 test synchronizes by `Thread.Join()`, which supplies the happens-before edge for the cross-thread read of `observed`. |
| 1.5 | Readability, AAA, documented intent | PASS | The new tests carry explicit `// Arrange` / `// Act` / `// Assert` comments and XML-doc summaries. Assertion reasons are supplied (`"the production fallback must surface the uncaptured-dispatcher guard"`). |
| 1.6 | No external dependencies, no temp files | PASS | Grep over all changed `.cs` files for `Path.GetTempPath`, `GetTempFileName`, and `Temp` returns no added line. The only added line containing `Temp` is the word "Temporarily" in an XML-doc summary. |
| 1.7 | Coverage exclusion policy — no production path excluded by config | PASS | `coverage.config` excludes only third-party module paths (Deedle, FSharp, Castle.Core, FluentAssertions, Moq, Microsoft.Testing, MSTest). The derived run configuration appends `.*\.Test\.dll$`, which excludes test assemblies as the policy requires rather than production code. No `exclude` entry matches a production source path. |
| 1.8 | Test file location | PASS (repo convention) | Tests live in per-project `<Project>.Test/` assemblies mirroring the production tree. The `tests/` layout named in `.claude/rules/general-unit-test.md` is not the layout of this .NET Framework solution; this divergence is repository-wide and pre-existing on `main`, and the branch introduces no new deviation. The new helper lands in `UtilitiesCS.Test/TestHelpers/`, a test-support directory, not in a production tree. |

### 1.2.1 Per-Language Coverage Comparison

Every language with changed files on the branch receives an explicit PASS or FAIL below. Languages
with zero changed files are listed for completeness and carry PASS.

| Language | Changed files on branch | Coverage artifact | Repo-wide line coverage | Repo-wide branch coverage | Verdict |
|---|---|---|---|---|---|
| C# | 15 `.cs` + 1 `.csproj` | `artifacts/csharp/coverage.xml` absent; raw Cobertura present at `coverage/782-r1-final.cobertura.xml` | 84.50% (55683/65896) | 79.15% (13249/16740) | FAIL |
| PowerShell | 0 | not required | zero changed files, so the pester line and Pester command coverage thresholds have no subject on this branch | Pester measures no branch percentage, so no branch threshold applies | PASS |
| Python | 0 | not required | zero changed files, so the python line coverage threshold has no subject on this branch | zero changed files, so the python branch coverage threshold has no subject on this branch | PASS |
| TypeScript | 0 | not required | zero changed files, so the typescript line coverage threshold has no subject on this branch | zero changed files, so the typescript branch coverage threshold has no subject on this branch | PASS |

The C# row reads **FAIL** because 84.50% is below the 85% uniform line floor in
`.claude/rules/quality-tiers.md` and `.claude/rules/general-unit-test.md`. The branch figure 79.15%
clears the 75% floor. Under `CLAUDE.md`'s 80% testable-denominator floor the same measurement passes.
The 80-versus-85 divergence between `CLAUDE.md` and `.claude/rules/` is unreconciled and pre-exists on
`origin/main`; this audit reports against the stricter `.claude/rules/` figure and records the
disposition below.

**Disposition of the C# FAIL row: non-blocking.** The branch does not move the figure. This
reviewer's own aggregation of `coverage/782-r1-baseline.cobertura.xml` and
`coverage/782-r1-final.cobertura.xml` returns byte-equal counters on both sides — 112351/132961 lines
and 26498/33480 branches under the delivery's pinned selection, 55683/65896 and 13249/16740 under the
class-level selection. Numerator and denominator are unchanged, so the shortfall is entirely
inherited from `origin/main` and none of it is attributable to this delivery.

### 1.2.2 Coverage Evidence Checklist

| Item | State | Note |
|---|---|---|
| Canonical C# artifact `artifacts/csharp/coverage.xml` | ABSENT | The `artifacts/csharp/` directory does not exist. Deliberate under scope decision SD1. Recorded as finding R1, non-blocking. |
| Raw Cobertura available for independent verification | PRESENT | `coverage/782-p0-baseline.cobertura.xml` (18,144,506 bytes), `coverage/782-p7-final.cobertura.xml` (18,144,107 bytes), `coverage/782-r1-baseline.cobertura.xml` (18,144,083 bytes), `coverage/782-r1-final.cobertura.xml` (18,144,167 bytes). All four are git-ignored by `.gitignore:144` (`coverage/*`). |
| Committed summary reconciles with raw data | YES | `evidence/qa-gates/coverage-summary.2026-09-05T23-11.md` and `evidence/qa-gates/r-p4-t5-tests-coverage.md` state 112351/132961/26498/33480; this reviewer's independent aggregation returns the identical four integers. |
| Baseline document provenance | RESOLVED | The R4 amendment to `evidence/baseline/p0-t7-coverage.md` is independently confirmed: aggregating `coverage/782-p0-baseline.cobertura.xml` returns exactly `LINES_COVERED=112359 LINES_VALID=132967 BRANCHES_COVERED=26496 BRANCHES_VALID=33480`, the figures the amendment attributes to the retained document. |
| TRX corroboration | PRESENT | Three TRX files read directly: `782-r1-baseline` 7000/7000/0, `782-r1-final` 7000/7000/0, `782-r1-p1t7` 2 total / 1 passed / 1 failed. |

## 2. General Code Change Policy Compliance

| # | Requirement | Verdict | Evidence |
|---|---|---|---|
| 2.1 | Simplicity first | PASS | Six independently written reflection sites collapse to one `internal sealed` scope; a 514-line file becomes two partial parts of 271 and 288 lines. Both reduce indirection rather than adding it. |
| 2.2 | Reusability | PASS | `UiThreadDispatcherScope` replaces four duplicated acquisition-and-restore passages in `UtilitiesCS.Test`. |
| 2.3 | Extensibility, no breaking public API change | PASS | `DispatcherNotInitializedMessage` is `internal const`, reachable from `UtilitiesCS.Test` through the existing `InternalsVisibleTo` grant at `UtilitiesCS/Properties/AssemblyInfo.cs:19`. `UiThread.Dispatcher` keeps its signature and its exception type. |
| 2.4 | Separation of concerns | PASS | The message text moves to one constant consumed by two throw sites in the same assembly; no I/O is introduced. |
| 2.5 | Error handling — fail fast, no silent swallow | PASS | The seam's field resolution asserts non-null with a stated reason, so a rename of `_dispatcher` raises `TypeInitializationException` on first use instead of degrading to a no-op guard. That is the defect class C12/C13 were raised against. |
| 2.6 | File size limit — 500 lines | PASS | Every changed `.cs` file measured at head: 397, 341, 328, 317, 288, 278, 271, 266, 256, 231, 215, 195, 126, 109, 76. Maximum 397. `ProgressTracker_Tests.cs` was 514 at base and is 271 at head, so the branch removes a pre-existing violation. |
| 2.7 | Naming | PASS | `PascalCase` types and members, `camelCase` locals, `_camelCase` private fields throughout the changed set. |
| 2.8 | Comment why, not what; comments match behavior | PASS | The three corrected comment passages (`WpfDispatcherYield.cs:53-59`, `EmailMoveMonitorTests.cs:27-40`, `QfcItemController.InitializationTests.Part2.cs:121-131`) each replace a claim falsified by PR #778 with the mechanism the code has today. Verified by reading both sides of the diff. |
| 2.9 | Mandatory toolchain loop, one uninterrupted pass | PASS | Re-executed by this reviewer: format check, analyzer build, nullable build all exit 0 at the current head. The delivery's own loop-closure record `evidence/qa-gates/r-p4-t7-loop-closure.md` states `PASS NUMBER: 1`. |
| 2.10 | Dependencies — none added | PASS | No `packages.config`, `.csproj` `<Reference>`, or `<PackageReference>` change. The only `.csproj` edit adds two `<Compile Include>` entries. |
| 2.11 | No absolute host paths in artifacts | FAIL | **Correction to this reviewer's cycle-1 row 2.11, which recorded PASS.** Two committed artifacts embed the absolute host path including the account name: `plan.2026-09-05T15-47.md:42` and `research/research.2026-09-05T16-10.md:6`, both reading `C:\Users\DanMoisan\repos\TaskMaster-wt\2026-09-05T10-47`. The cycle-1 evidence sentence was scoped to `evidence/` artifacts, where the substitution is genuinely complete; the criterion is stated over artifacts generally. Non-blocking: 827 committed documents under `docs/` on `origin/main` already carry the same path, no `.claude/rules/` file or `CLAUDE.md` section codifies the prohibition, and the two occurrences are a negligible addition to an established repository-wide pattern. Recorded as finding N1. |
| 2.12 | Bugfix workflow — failing regression test first | PASS | Three separate RED-first records exist and are corroborated by committed TRX counters: `evidence/regression-testing/p4-t7-fail-before.md` (exit 1, both guards removed together so the demonstration is not vacuous), `p4-t8-pass-after.md` (exit 0 after `git checkout HEAD --` restore), and `r-p1-t7-fail-before.md` (TRX `outcome="Failed"`, 2 total, 1 failed). |

## 3. Language-Specific Code Change Policy Compliance (C#)

| # | Requirement | Verdict | Evidence |
|---|---|---|---|
| 3.1 | CSharpier formatting via `dotnet tool run` | PASS | This reviewer ran `dotnet tool run csharpier check .`: `Checked 1583 files in 4405ms`, exit 0. The count equals the delivery's recorded 1583, so the processed file set is unchanged. `dotnet format` was not used anywhere on the branch. |
| 3.2 | .NET analyzers, `EnableNETAnalyzers` + `EnforceCodeStyleInBuild` | PASS | This reviewer ran the exact CLAUDE.md command with `/t:Rebuild`: `Build succeeded. 0 Warning(s) 0 Error(s)`, exit 0. |
| 3.3 | Nullable / type checking with `TreatWarningsAsErrors=true` | PASS | This reviewer ran the exact `.github/workflows/_build-nullable.yml` command with `/t:Rebuild`: exit 0, all 19 projects recompiled, no diagnostic emitted. `/p:Nullable=enable` was correctly not passed. |
| 3.4 | Per-file nullable opt-in respected | PASS | `UtilitiesCS/Threading/UiThread.cs` carries `#nullable enable` and the edited getter participates in flow analysis. The new `UiThreadDispatcherScope.cs` uses `#nullable enable annotations` / `#nullable restore annotations`, which is the established idiom in this assembly (17 occurrences of each). Noted as informational finding N8: annotations-only means the file receives no `CS86xx` flow analysis. |
| 3.5 | Strong contracts, explicit APIs, XML docs on non-obvious behavior | PASS | `UiThread.Dispatcher` gains `<summary>`, `<remarks>`, and `<exception cref="InvalidOperationException">`. The `<remarks>` states why this accessor deliberately does not self-heal by calling `Init()`, which is the contract question a reader would otherwise have to reconstruct from the sibling accessors. |
| 3.6 | Null-safety by default | PASS | The getter now reads the non-volatile static exactly once into a local and returns that local, closing the torn-read window C02 identified. |
| 3.7 | Banned symbols (`BannedSymbols.txt`) | PASS | No added line introduces `DateTime.Now`, `DateTime.UtcNow`, `Random.Shared`, `Thread.Sleep`, or `Task.Delay`. The two pre-existing `DateTime.Now` call sites are unchanged in count. RS0030 is held at `severity = suggestion` per `.claude/rules/csharp.md:79`. |
| 3.8 | MSTest / Moq / FluentAssertions only | PASS | The changed tests use `[TestClass]`, `[TestMethod]`, `[TestInitialize]`, `[TestCleanup]`, `[DoNotParallelize]` from `Microsoft.VisualStudio.TestTools.UnitTesting`, and FluentAssertions for assertions. No xUnit or NUnit reference is introduced. |

## 4. Language-Specific Unit Test Policy Compliance (C#)

| # | Requirement | Verdict | Evidence |
|---|---|---|---|
| 4.1 | MSTest framework | PASS | Verified across all seven changed test files. |
| 4.2 | Moq for mocks | PASS | `EmailMoveMonitorTests` retains its Moq usage; the new tests use hand-written fakes (`CountingDispatcherProvider`, `StaDispatcherHost`) where a mock would add no isolation. |
| 4.3 | FluentAssertions preferred | PASS | Both rewritten assertions use `Should().Throw<T>().WithMessage(...)`. The seam's field check uses `Should().NotBeNull(because: ...)`. |
| 4.4 | Scenario completeness — positive, negative, edge, error | PASS | The delivery adds the negative path for three throw sites and the C21 edge case, a worker thread with no dispatcher of its own reaching the production fallback. `AC5` also requires and gets a restore-to-null assertion after scope disposal. |
| 4.5 | Assertion pins the intended property | PASS | `WithMessage(UiThread.DispatcherNotInitializedMessage)` contains neither `*` nor `?`, so FluentAssertions compares the pattern against the whole message. Observed, not derived: `evidence/regression-testing/r-p1-t7-fail-before.md` records the assertion failing when the removed tail is appended at the `WpfDispatcherYield` throw site, and the committed TRX at `TestResults/782-r1-p1t7` corroborates it with `outcome="Failed"`, 2 total, 1 passed, 1 failed, the failure being `WpfDispatcherYieldTests.YieldAsync_WithoutDispatcher_RemainsStrict`. |
| 4.6 | The constant's own text remains pinned by some test | PASS | `UtilitiesCS.Test/OutlookObjects/Folder/WpfDispatcherYieldTests.cs:196` asserts `observedException.Message.Should().Contain("UiThread.Init()")`. This is an independent literal that does not move with the constant, so an edit to the constant's wording that dropped `UiThread.Init()` would fail. The delivery states this limitation explicitly rather than overclaiming; verified as accurate. |

## 5. Test Coverage Detail

All figures below were produced by this reviewer directly from the Cobertura XML, not read from a
delivery artifact.

### 5.1 Repo-wide, first-party (nine production assemblies)

First-party allowlist: `Tags`, `ToDoModel`, `TaskVisualization`, `UtilitiesCS`, `QuickFiler`,
`TaskTree`, `TaskMaster`, `SVGControl`, `VBFunctions`. Vendor packages present in the document
(`log4net`, `Microsoft.IO.RecyclableMemoryStream`, `Mono.Reflection`, `System.Interactive`,
`System.Linq.Async`) are excluded from the numerator and denominator.

| Selection | Document | Lines | Line % | Branches | Branch % |
|---|---|---|---|---|---|
| `classes/class/lines/line` (class-level, no double count) | `782-r1-baseline` | 55683/65896 | 84.5013 | 13249/16740 | 79.1458 |
| `classes/class/lines/line` (class-level, no double count) | `782-r1-final` | 55683/65896 | 84.5013 | 13249/16740 | 79.1458 |
| `.//line` (the delivery's pinned SD22 selection) | `782-r1-baseline` | 112351/132961 | 84.4992 | 26498/33480 | 79.1458 |
| `.//line` (the delivery's pinned SD22 selection) | `782-r1-final` | 112351/132961 | 84.4992 | 26498/33480 | 79.1458 |

Both sides are byte-equal on all four counters under both selections. The delivery's claim that
coverage held exactly is **confirmed**.

The two selections disagree by 0.0021 points on lines and not at all on branches. The `.//line` form
double-counts, because a Cobertura `<class>` carries both a class-level `<lines>` block and a
per-method `<lines>` block over the same source lines; the document's own root attribute
`lines-valid="83068"` is smaller than the 132961 the `.//line` form reports over a strict subset of
packages, which is the direct proof. The double count is close enough to uniform that no percentage
the delivery states is materially wrong, and every comparison the delivery draws uses the same
selection on both sides. Recorded as informational finding N4, not as a defect.

### 5.2 Canonical coverage artifact presence

| Language | Expected path | Present | Verdict |
|---|---|---|---|
| C# | `artifacts/csharp/coverage.xml` | No | FAIL |
| PowerShell | `artifacts/pester/powershell-coverage.xml` | No | PASS, zero changed `.ps1` or `.psm1` files on the branch |
| Python | `artifacts/python/lcov.info` | No | PASS, zero changed `.py` files on the branch |
| TypeScript | `coverage/lcov.info` | No | PASS, zero changed `.ts` or `.tsx` files on the branch |

The C# row is FAIL under the artifact-absence rule. It is non-blocking: every question the artifact
exists to answer was answered from the raw Cobertura documents, and the disposition is recorded at
`evidence/other/r1-r2-maintainer-disposition.2026-09-06T00-15.md`. See finding R1.

### 5.3 New production files

**None.** The branch adds two files, both under `UtilitiesCS.Test`
(`TestHelpers/UiThreadDispatcherScope.cs`, 126 lines, and
`Threading/ProgressTracker_ReportAndViewerTests.cs`, 288 lines). Both are test modules, which the
derived run configuration removes from the denominator via `<ModulePath>.*\.Test\.dll$</ModulePath>`
exactly as the coverage exclusion policy requires. The new-code line floor therefore has no
production subject on this branch.

### 5.4 Modified production files

| File | Base lines | Head lines | Base % | Head % | Base branch | Head branch | Changed-line regression | Verdict |
|---|---|---|---|---|---|---|---|---|
| `UtilitiesCS/Threading/UiThread.cs` | 64/83 | 63/82 | 77.11 | 76.83 | 13/20 = 65.00 | 13/20 = 65.00 | No | FAIL |
| `UtilitiesCS/OutlookObjects/Folder/WpfDispatcherYield.cs` | 27/28 | 26/26 | 96.43 | 100.00 | 14/14 = 100.00 | 14/14 = 100.00 | No | PASS |
| `UtilitiesCS/Threading/ProgressTracker.cs` | 149/170 | 149/170 | 87.65 | 87.65 | 33/40 = 82.50 | 33/40 = 82.50 | No | PASS |
| `UtilitiesCS/Threading/ProgressTrackerAsync.cs` | 43/47 | 43/47 | 91.49 | 91.49 | 5/6 = 83.33 | 5/6 = 83.33 | No | PASS |
| `TaskMaster/Ribbon/RibbonViewer.EngineCommands.cs` | absent | absent | not measured | not measured | not measured | not measured | Cannot be measured | FAIL |

Baseline column read from `coverage/782-p0-baseline.cobertura.xml`, head column from
`coverage/782-r1-final.cobertura.xml`, both aggregated by this reviewer with the class-level
selection and de-duplicated by line number across partial-class and nested-type entries sharing a
filename.

**`UiThread.cs` FAIL, disposition non-blocking.** The decisive measurement is the uncovered line set,
not the percentage. This reviewer re-derived it at the new head:

```text
BASELINE uncovered (19): 28,29,30,32,33,34,67,68,69,70,71,72,73,74,75,76,118,119,120
HEAD     uncovered (19): 28,29,30,32,33,34,67,68,69,70,71,72,73,74,75,76,118,119,120
IDENTICAL SETS: True
```

Not one line moved from covered to uncovered. The -0.28 point movement is arithmetic: a covered
three-line wrapped `throw` collapsed to a single line when routed through the shared constant, so the
numerator and the denominator each fell by one against a residue fixed at 19. That residue sits in
`ThreadMonitor` construction inside `Initialize()` (lines 67-76) and in two other host-bound blocks,
none of which the branch touches. Branch coverage is unchanged at 65.00%. See finding R2.

**`RibbonViewer.EngineCommands.cs` FAIL, disposition non-blocking.** The file is absent from every
Cobertura document, baseline and head alike, because `RibbonViewer` carries `[ExcludeFromCodeCoverage]`
on the `RibbonViewer.cs` partial. That attribute is **pre-existing on `origin/main`** at
`RibbonViewer.cs:32`, verified by `git show 77c6d314:TaskMaster/Ribbon/RibbonViewer.cs`. It falls under
the COM/VSTO exemption ratified in `CLAUDE.md` UT2 for VSTO ribbon event handlers. The two changed
lines are therefore unmeasurable, and this reviewer verified them by inspection instead: removing
`dispatcher != null &&` from `if (dispatcher != null && !dispatcher.CheckAccess())` is
behavior-preserving, because the preceding statement `var dispatcher = UiThread.Dispatcher;` throws
`InvalidOperationException` when the static is unset on `origin/main` as well as at head, so the
comparison was already dead before the branch. Recorded as informational finding N5.

### 5.5 Changed-line coverage

**PASS.** Seven executable production lines changed across the four measurable production files, and
every one of them is covered at head. The remediation phase changed no production `.cs` file at all —
the only two files it edited are `UtilitiesCS.Test/OutlookObjects/Folder/WpfDispatcherYieldTests.cs`
and `UtilitiesCS.Test/Threading/UiThread_Tests.cs`, both excluded from the denominator by the derived
configuration — so the changed-line metric has an empty domain for the remediation considered alone
and is evaluated over the whole branch instead, where it passes. No changed line regressed on any
file.

## 6. Test Execution Metrics

| Metric | Value | Source |
|---|---|---|
| Total tests | 7000 | `TestResults/782-r1-final/*.trx` `ResultSummary/Counters/@total`, read by this reviewer |
| Passed | 7000 | same |
| Failed / error / timeout / aborted / inconclusive / notExecuted | 0 / 0 / 0 / 0 / 0 / 0 | same |
| TRX outcome | `Completed` | same |
| Baseline-side total | 7000 passed, 0 failed | `TestResults/782-r1-baseline/*.trx` |
| RED-first run | 2 total, 1 passed, 1 failed, `outcome="Failed"` | `TestResults/782-r1-p1t7/*.trx` |
| Assemblies | 9 | `QuickFiler.Test`, `SVGControl.Test`, `Tags.Test`, `TaskMaster.Test`, `TaskTree.Test`, `TaskVisualization.Test`, `ToDoModel.Test`, `UtilitiesCS.Test`, `VBFunctions.Test` |
| Local filter | `TestCategory!=LiveOutlook` plus four shell-icon classes excluded | Environmental `SHGetFileInfo` stall that reproduces against `origin/main`; CI runs those classes and reports a larger total |
| Known flake #780 | Did not fire | `TryAddValuesAsync_UpdatesExistingValue` passed; `Failed: 0` means no re-run occurred and a single run is recorded |

This reviewer did not re-execute the 7000-test run. The counters above are read from the committed
TRX documents rather than restated from a prose summary, so the figures are verified against the
run's own machine-readable output.

## 7. Code Quality Checks

| Check | Command | Result | Who ran it |
|---|---|---|---|
| CSharpier verify | `dotnet tool run csharpier check .` | `Checked 1583 files in 4405ms`, exit 0 | This reviewer, at head |
| Analyzer diagnostics | `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` | `Build succeeded. 0 Warning(s) 0 Error(s)`, exit 0 | This reviewer, at head |
| Nullable enforcement | `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true` | exit 0, 19 projects recompiled, no diagnostic | This reviewer, at head |
| Worktree cleanliness | `git status --porcelain --untracked-files=all` | 0 paths, before and after both rebuilds | This reviewer |
| `.claude/**` untouched | `git diff --name-only 77c6d314..HEAD -- .claude` | 0 paths | This reviewer |
| File size limit | line count of every changed `.cs` file | maximum 397, limit 500 | This reviewer |
| Reflection site count | grep for the single-line token `"_dispatcher"` across all `*.cs` | exactly 2 hits, the two the specification names | This reviewer |

## 8. Gaps and Exceptions

| ID | Gap | Severity | Blocking | Disposition |
|---|---|---|---|---|
| R1 | Canonical C# artifact `artifacts/csharp/coverage.xml` absent | Procedural | No | Recorded FAIL. Accepted on the strength of the raw Cobertura substitute, from which every figure in this audit was independently derived. Written disposition at `evidence/other/r1-r2-maintainer-disposition.2026-09-06T00-15.md`. Recurs unchanged at the new head because SD1 governs. |
| R2 | `UiThread.cs` modified-file line coverage 76.83%, below the 80% trigger and the 85% floor | Should-fix | No | Recorded FAIL. Waived on the identical-uncovered-line-set evidence re-derived above. Raising it requires a production seam extraction on host-bound WinForms code, the same class of change carved out to #787 and #788. |
| N1 | Absolute host path with account name in `plan.2026-09-05T15-47.md:42` and `research/research.2026-09-05T16-10.md:6` | Nit | No | New this cycle. Corrects this reviewer's cycle-1 row 2.11, which recorded PASS on evidence scoped only to `evidence/`. 827 precedent files on `origin/main`; not codified in any repository rule. |
| N2 | `evidence/other/r1-r2-maintainer-disposition.2026-09-06T00-15.md` is titled a maintainer disposition, but no maintainer ratification record exists | Nit | No | New this cycle. `artifacts/orchestration/orchestrator-state.json` carries `remediation_disposition` with `decided_at: 2026-09-06T00:20:00Z` and no actor field, and `human_interaction` is `null`. The document's own body correctly attributes itself to plan task [P3-T7]. |
| N3 | `user-story.md` AC-U2 names "the retry-after-failed-initialization behavior of `UiThread.Init()`" as a delivered production behavior change | Nit | No | New this cycle. C03 was withdrawn; `Init()` is byte-identical to `pre-782-base`. The AC is an upper bound so it is not false, and `spec.md` records the withdrawal in full, but the phrasing is stale relative to final scope. |
| N4 | The pinned SD22 `.//line` aggregation double-counts method-level rows | Informational | No | Percentage impact 0.0021 points; both sides of every comparison use the same selection; no stated figure is materially wrong. |
| N5 | `RibbonViewer.EngineCommands.cs` changed lines are unmeasurable | Informational | No | Type-level `[ExcludeFromCodeCoverage]` pre-existing on `origin/main`; ratified COM/VSTO exemption; the two changed lines verified behavior-preserving by inspection. |
| N6 | PR context summary reports `Core logic changes: 0 files` against 16 changed code files | Informational | No | Generator defect, not a delivery defect. Consequence simulated below. |
| N7 | PR context `Close candidates` author-asserted list contains 22 entries scraped from prose | Informational | No | Includes non-issues `#ISO-8601`, `#S2-1`, `#S3-1` through `#S4-2`, and unrelated issues #394, #449, #476, #493, #508, #584, #778, #780. The PR must close #782 only. |
| N8 | `UiThreadDispatcherScope.cs` uses `#nullable enable annotations`, not `#nullable enable` | Informational | No | Consistent with the assembly idiom (17 occurrences). Means the file receives annotation syntax without `CS86xx` flow analysis, so the nullable gate is a no-op over it. |
| SD1 | `artifacts/csharp/coverage.xml` deliberately not produced | Scope decision | No | Documented in `spec.md` Constraint 11 and Non-Goals. This reviewer restates the cycle-1 qualification: "producing the artifact would force a FAIL verdict" is not a legitimate reason to omit it, and the FAIL is recorded regardless. The acceptance rests on the substitute evidence. |
| SD4 | `Dispatcher_WhenBackingFieldIsNull_ThrowsInvalidOperationExceptionNamingInitialize` retains a name that no longer matches the message | Scope decision | No | Renaming would make a `TestCaseFilter` expression recorded in a committed #584 evidence artifact resolve to zero tests. Recorded in the delivery's code-review artifact section (c). |
| SD5 | The `WpfDispatcherYield` message tail "before yielding folder tree work" is removed | Scope decision | No | Now correctly characterized after the R3 fix: the shared-constant assertion fails on a tail appended at that throw site, observed at `evidence/regression-testing/r-p1-t7-fail-before.md`. |
| SD18 | C03 latch re-arm withdrawn | Scope decision | No | Withdrawn on a measured regression, bisected to the single re-arm line; promoted as #788. |
| DC1 | `CLAUDE.md` states an 80% repo-wide floor and a 90% new-code floor; `.claude/rules/` states a uniform 85% line and 75% branch floor | Doc conflict | No | Unreconciled and pre-existing on `origin/main`. This audit reports against the stricter `.claude/rules/` figure. |

### Coverage hook simulation

The SubagentStop hook `.claude/hooks/validate-feature-review-coverage.ps1` derives its changed-language
set from `artifacts/pr_context.summary.txt` by matching `^\s*-\s+(\S+)\s+\(\+\d+/-\d+\)\s*$`. This
reviewer dot-sourced the hook and ran `Get-ChangedLanguageSet` against the current summary: **10
matching lines, all `.md`, producing an empty changed-language set**. The hook therefore performs the
three artifact-path checks and returns before any per-language coverage check runs. The explicit
PASS and FAIL verdicts in section 1.2.1 are supplied under this audit's own scope invariant, not
because the hook demands them.

## 9. Summary of Changes

| Category | Count | Detail |
|---|---|---|
| Total changed paths | 126 | two-dot and three-dot sets identical |
| Production `.cs` | 5 | `UiThread.cs`, `WpfDispatcherYield.cs`, `ProgressTracker.cs`, `ProgressTrackerAsync.cs`, `RibbonViewer.EngineCommands.cs` |
| Test `.cs` | 10 | 8 modified, 2 new |
| Build configuration | 1 | `UtilitiesCS.Test.csproj`, two `<Compile Include>` entries added |
| #584 feature folder | 23 | 4 documentation, 19 evidence |
| #782 feature folder | 84 | specification, user story, issue, research, plan, remediation plan, cycle-1 audit artifacts, 74 evidence files |
| Promoted entries | 3 | this issue's own entry plus #787 and #788 |
| `.claude/**` | 0 | certified by direct diff |
| Forbidden `artifacts/` evidence paths | 0 | certified by direct diff |

The Write Set in `spec.md` names 5 production files, 10 test files, 1 build configuration file, 4
#584 documentation files, and 19 #584 evidence files. The branch diff matches all five counts exactly.

## 10. Compliance Verdict

**PASS. Blocking findings: 0.**

| Dimension | Verdict |
|---|---|
| General Unit Test Policy | PASS |
| General Code Change Policy | PASS with one FAIL row (2.11, host paths, non-blocking) |
| C# Code Change Policy | PASS |
| C# Unit Test Policy | PASS |
| Coverage — C# | FAIL, non-blocking, no delta attributable to this delivery |
| Coverage — PowerShell, Python, TypeScript | PASS, zero changed files |
| Evidence locations | PASS |
| `.claude/**` untouched | PASS |
| Toolchain, one uninterrupted pass | PASS, three of four gates independently re-executed by this reviewer |

Recommendation: **GO for pull request.** The pull request body must close **#782 only**; #787 and #788
are follow-ups that must remain open, and none of the other 21 entries in the PR context
`Close candidates` list is a real close candidate for this branch.

## Appendix A: Test Inventory

| Test | File | Purpose | Status |
|---|---|---|---|
| `Dispatcher_WhenBackingFieldIsNull_ThrowsInvalidOperationExceptionNamingInitialize` | `UtilitiesCS.Test/Threading/UiThread_Tests.cs:~139` | Pins the `UiThread.Dispatcher` throw against the whole shared constant | Pass |
| `YieldAsync_WithoutDispatcher_RemainsStrict` | `UtilitiesCS.Test/OutlookObjects/Folder/WpfDispatcherYieldTests.cs:~130` | Pins the `WpfDispatcherYield` injected-provider guard against the whole shared constant | Pass; observed to fail on a mutated tail |
| `YieldAsync_ProductionFallbackWithoutDispatcher_ThrowsNamingInit` | `WpfDispatcherYieldTests.cs:161` | C21. Reaches the production fallback from a fresh worker thread with no dispatcher; also the only test pinning the literal `UiThread.Init()` | Pass |
| `InitializeAsync_WhenDispatcherNotCaptured_ThrowsInvalidOperationException` | `UtilitiesCS.Test/Threading/ProgressTrackerAsync_Tests.cs` | C26 asynchronous half | Pass |
| `Initialize_WhenDispatcherNotCaptured_ThrowsInvalidOperationException` | `UtilitiesCS.Test/Threading/ProgressTracker_ReportAndViewerTests.cs` | C26 synchronous sibling, the one test method added by the split | Pass |
| 21 pre-split `ProgressTracker_Tests` methods | split across `ProgressTracker_Tests.cs` and `ProgressTracker_ReportAndViewerTests.cs` | Verified preserved: `comm` of the pre-split and post-split method-name sets returns zero names lost | Pass |
| Full suite | 9 assemblies | Regression | 7000 / 7000 / 0 |

## Appendix B: Toolchain Commands Reference

Reference commands for this repository. The first three were executed by this reviewer at the current
head; the fourth was not re-executed and its results were read from the committed TRX.

```powershell
dotnet tool run csharpier format .
dotnet tool run csharpier check .
msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true
msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true
```

`/t:Rebuild` is required for both builds. MSBuild's up-to-date check does not invalidate on a
command-line `/p:` change, so a warm `/t:Build` returns exit 0 with `CoreCompile` skipped on every
project and the gate cannot fail. Both builds run in this review recompiled all 19 projects.
