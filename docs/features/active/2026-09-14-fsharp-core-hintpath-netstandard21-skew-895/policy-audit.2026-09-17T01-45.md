# Policy Audit — Issue #895 (FSharp.Core HintPath netstandard2.1 skew)

- **Feature:** `docs/features/active/2026-09-14-fsharp-core-hintpath-netstandard21-skew-895`
- **Issue:** #895
- **Work Mode:** `full-bug` (marker read at `issue.md:13`) — `spec.md` is the sole AC source; `user-story.md` is correctly absent
- **Reviewer:** feature-review agent
- **Timestamp:** 2026-09-17T01-45
- **Branch:** `bug/fsharp-core-hintpath-netstandard21-skew-895` at `7b865c4daaf40f396221ed10dc2a3287a8cd42d5` (7 commits ahead of base)
- **Base branch:** `origin/main` at `91746d2e4776a59ee1db1856c5c490a009c4958b` after an explicit `git fetch origin main`; `git merge-base HEAD origin/main` resolves to the same SHA, so the two-dot and three-dot diff forms coincide
- **Execution worktree leaf:** `agent-a8bc4dc5978785885` (git status clean at review start)
- **Overall verdict:** **PASS** — 0 blocking findings, 0 PARTIAL acceptance criteria, 3 warnings (all non-blocking), 9 informational notes

---

## Template Provenance

The `policy-audit-template-usage` skill requires the template to be resolved through
`mcp__drm-copilot__resolve_policy_audit_template_asset`. No `mcp__drm-copilot__*` tool is exposed in
this session, so the asset could not be resolved and `mcp__drm-copilot__validate_orchestration_artifacts`
could not be run by this reviewer. Per the recorded precedent for this condition, this artifact is
hand-authored preserving all twelve canonical major headings rather than being marked BLOCKED. The
orchestrator runs the validator; this deviation is recorded rather than left implicit.

## Scope

- **Diff base and method.** Every diff in this audit is anchored on `origin/main` after an explicit
  fetch (`git -C <repo-root> fetch origin main`), never on local `main`. The full branch footprint is
  `git diff --numstat origin/main...HEAD`: 46 files, 4575 insertions, 143 deletions. The two-dot form
  reports the identical set, as expected when the merge base equals `origin/main`.
- **PR context artifacts.** No `artifacts/pr_context.summary.txt` or `.appendix.txt` existed in the
  execution worktree, and no PR-context MCP tool is on this agent's tool surface. The pair in the
  session-root checkout is a stale copy for another cohort item
  (`bug/quickfiler-itemviewer-ui-marshalling-seam-743`, head `841fba74`) and was not used. Per the
  recorded fallback, both files were hand-authored from `git diff --numstat origin/main...HEAD` into the
  execution worktree's git-ignored `artifacts/` directory (`.gitignore:57`), in the `- <path> (+N/-N)`
  bullet form. They are a self-generated substitute, not collector output.
- **Change footprint by category (re-derived from git):**
  - 3 project files, one HintPath value each: `QuickFiler/QuickFiler.csproj` (line 52),
    `QuickFiler.Test/QuickFiler.Test.csproj` (line 259), `ToDoModel/ToDoModel.csproj` (line 42); each `1 1`.
  - 1 project file, two `Compile Include` insertions: `TaskMaster.Test/TaskMaster.Test.csproj` (`2 0`).
  - 2 new test files: `TaskMaster.Test/Bootstrap/FSharpCoreHintPathAlignmentTests.cs` (211 lines),
    `TaskMaster.Test/Bootstrap/FSharpCoreDeployedIdentityTests.cs` (244 lines).
  - 1 test file, XML-doc comment only: `TaskMaster.Test/Bootstrap/NetstandardBindChildDomainTests.cs`
    (`11 7`, hunk `@@ -364,7 +364,11 @@`, zero non-`///` changed lines).
  - 7 files under `.claude/agent-memory/` on commit `f1c53f9b8` (preparation run, before execution).
  - 32 markdown documents under the feature folder (issue, spec, plan, research, 28 evidence files).
- **Zero production `.cs` files changed.** Re-derived: `git diff --name-only origin/main...HEAD -- "*.cs"`
  lists only the three `TaskMaster.Test/Bootstrap/` files.
- **Languages with changed files:** C# only (`.cs` and `.csproj`). Zero TypeScript, Python or PowerShell files.
- **Verification basis labels used below:** **re-derived** (this reviewer measured it directly from the
  working tree, a git command, a Cobertura document, a TRX, or a build log) or **evidence-attested**
  (read from a committed evidence artifact and not independently re-derivable).

## Rejected Scope Narrowing

**None detected.** The caller supplied the full branch, the correct base (`origin/main` after fetch) and
the correct head SHA, and directed a full feature review. Three caller statements were examined and
found to be rulings requested or facts stated, not narrowing:

1. "Inherited-path rule: any path already changed relative to origin/main before execution began, and any
   path under `.claude/agent-memory/`, is outside every scope assertion." This governs the *plan's* write-set
   containment gate, not this audit's scope. The seven agent-memory files remained inside this audit's
   diff scope: each was confirmed to sit on the preparation commit `f1c53f9b8` (not on an execution
   commit), and a host-path scan over that commit returned zero matches. See I-6.
2. "`user-story.md` is correctly absent." Verified: `issue.md:13` records `- Work Mode: full-bug`, which the
   `acceptance-criteria-tracking` skill maps to `spec.md` only. Not a narrowing.
3. Items (a), (b), (c) and the loop-closure note are requests for explicit rulings; they widen rather than
   narrow the audit and are ruled on in section 8.

The caller did not mark any language as informational-only or waive any toolchain or coverage check.

## Evidence Location Compliance

- `validate_evidence_locations.py` does not exist in this repository (`find` over `scripts/` and
  `.claude/` returned nothing); the working substitute is the anchored name-only diff.
- Re-derived: `git diff --name-only origin/main...HEAD` contains **zero** paths under `artifacts/`
  of any kind. No file was written under `artifacts/baselines/`, `artifacts/qa/`, `artifacts/evidence/` or
  `artifacts/coverage/`.
- All 28 evidence artifacts this feature produced are under
  `<FEATURE>/evidence/{baseline,regression-testing,qa-gates,issue-updates,other}/`, which are canonical
  kinds per `evidence-and-timestamp-conventions`. Every artifact carries the plan's fixed token
  `2026-09-16T23-27` in its filename and a `Timestamp:` field with the write time, as the plan's binding
  rule requires.
- The two hand-authored PR-context files written by this review sit at `artifacts/pr_context.*.txt`,
  which is git-ignored and is not an evidence kind. No `EVIDENCE_LOCATION_OVERRIDE_REJECTED` event
  occurred: the caller supplied no non-canonical evidence path.

**PASS.**

---

## Executive Summary

Issue #895 is a build-order nondeterminism defect: the six `FSharp.Core` HintPath entries in the
solution were split three/three between the `lib\netstandard2.0` and `lib\netstandard2.1` flavours of
`packages\FSharp.Core.11.0.100`. Both flavours carry the identical assembly identity, so
ResolveAssemblyReferences saw no conflict, and the netstandard2.1 flavour's own reference to
`netstandard, Version=2.1.0.0` is unsatisfiable on .NET Framework. The delivered fix changes exactly one
HintPath value in each of three project files, adds two regression test classes (a static source-shape
assertion and a deployed-binary assertion read through `System.Reflection.Metadata`), registers them with
two `Compile Include` items, and corrects one XML-doc `<remarks>` block that the fix would otherwise have
made false.

Independently re-derived by this reviewer on the execution worktree: all six HintPaths now end in
`lib\netstandard2.0\FSharp.Core.dll` (grep census, 6/6); every one of the fifteen deployed
`FSharp.Core.dll` copies is SHA-256-identical to the package's netstandard2.0 binary and none matches the
netstandard2.1 binary; the netstandard2.0 package binary references `netstandard=2.0.0.0` and the
netstandard2.1 binary references `netstandard=2.1.0.0` (PEReader, read directly); the P1-T6 TRX on disk
carries the Shape A failure message naming exactly the three misaligned project files; both build logs show
`Skipping target "CoreCompile"` = 0 and exactly one `csc.exe` invocation per project; and the post-change
Cobertura root element reads line-rate 0.858678 / branch-rate 0.800317 over 65616 valid lines.

C# coverage verdict: PASS (repo-wide 85.87% lines and 80.03% branches on the post-change Cobertura; zero changed production lines; both new files are test code outside the instrumented denominator).

All five acceptance criteria are verified PASS. The full C# toolchain passed in a single clean pass with
no restart. Blocking findings: **0**. Three non-blocking warnings (two plan-literal deviations ratified by
independent re-measurement, one owed follow-up filing) and nine informational notes are recorded in
section 8. No `remediation-inputs` artifact is produced.

---

## 1. General Unit Test Policy Compliance

Source: `.claude/rules/general-unit-test.md` and CLAUDE.md UT1–UT5.

| Requirement | Verdict | Evidence and verification basis |
|---|---|---|
| Independence — any order | **PASS** | Re-derived. Both new classes are read-only over files; neither writes a static, a field, a file or an environment value. `FSharpCoreDeployedIdentityTests` calls `FSharpCoreHintPathAlignmentTests.FindRepositoryRoot()` and `DiscoverFSharpCoreHintPaths()`, both pure functions of the tree. |
| Isolation — one unit per test | **PASS** | Re-derived. Shape A: count (`:154-172`) and flavour (`:178-210`) are separate tests. Shape B: one `[DataRow]` per directory (15) plus one positive control. |
| Fast execution | **PASS** | Evidence-attested and corroborated. Scoped runs: Shape A 2 tests in 1.35 s, Shape B 16 results; full suite 7311 in a single run with no stall recorded. |
| Determinism — no flakiness | **PASS** | Re-derived. Grep over both new files for `Thread.Sleep`, `Task.Delay`, `DateTime.Now`, `DateTime.UtcNow`, `Random(`, `GetTempPath`, `GetTempFileName`, `Environment.CurrentDirectory`: **zero** matches. No retry, no timing tolerance, no AppDomain, no process. Shape B is deterministic given a completed whole-solution rebuild and throws `InvalidOperationException` naming the path otherwise (`:49-56`), never skips. |
| Readability and maintainability | **PASS** | Re-derived. Every test and helper carries an XML `<summary>`; every assertion carries a `because` string; both files use explicit `// Arrange` / `// Act` / `// Assert` markers. |
| Line coverage >= 85% (rules) / >= 80% (CLAUDE.md) | **PASS** | Re-derived. `/coverage/@line-rate` = `0.858678` read directly from the root element of `coverage/coverage.cobertura.xml` in the execution worktree (85.87%). Clears both floors; the unreconciled 80-vs-85 conflict between CLAUDE.md and the rules files does not change the verdict. |
| Branch coverage >= 75% | **PASS** | Re-derived. `/coverage/@branch-rate` = `0.800317` from the same element (80.03%). |
| No regression on changed lines | **PASS** | Re-derived. The changed-production-line set is empty (`git diff --name-only origin/main...HEAD -- "*.cs"` lists only test files). Repo-wide movement is −2 covered lines on an identical 65616-line denominator, which is the known run-to-run band and is not attributable to a change that alters no instrumented line. |
| Coverage exclusion policy — no production file excluded | **PASS** | Re-derived. No `coverage.config`, `.runsettings`, `.editorconfig` or `ExcludeFromCodeCoverage` change in the footprint. The runner's `.*\.Test\.dll$` module exclusion is a pre-existing, permitted test-file exclusion. |
| Scenario completeness — positive flows | **PASS** | Re-derived. Count of six, flavour of each, fifteen per-directory netstandard versions, positive control on the 2.1 binary. |
| Scenario completeness — negative / boundary | **PASS** | Re-derived. Missing directory or file throws (`:49-56`); unrestored `packages/` makes the control throw; a seventh HintPath or a deleted one fails the count test; a tree with all six on netstandard2.1 fails on flavour and on every Shape B row. Each was observed failing on the unfixed tree where the state existed (three rows, three offenders). |
| Scenario completeness — error handling | **PASS** | Re-derived. Precondition failures are thrown with the full path (fail loud), assertion failures name the directory or project file and the observed version. |
| Scenario completeness — concurrency | **PASS (called-out exception)** | Not applicable to two read-only file-inspection classes; both run under the standard `Workers=0` / `ClassLevel` runsettings alongside the `[DoNotParallelize]`-pinned #879 class without shared state. Recorded, not a gap. |
| Arrange–Act–Assert | **PASS** | Re-derived. Present in all four test methods. |
| Clear failure messages | **PASS** | Re-derived. Shape A projects every offender into the reason text (see section 8, item (c) ruling); Shape B names `projectDirectoryName` in each `because`. The P1-T6 TRX message on disk was read directly and carries all three offending project names. |
| No external dependencies | **PASS** | Re-derived. No network, database, process or remote API. Both classes read files under the repository root, following the existing `RibbonControllerTests` / `AddInEagerInstallShapeTests` precedent; Shape B additionally depends on a completed solution build, which CI performs before tests (`.github/workflows/_mstest-coverage.yml:82` builds the solution, `:94` runs the coverage runner). |
| No temporary files | **PASS** | Re-derived. Zero `GetTempPath` / `GetTempFileName` / file writes in either new file. |
| Test file location mirrors source | **PASS (repo convention)** | Re-derived. `TaskMaster.Test/Bootstrap/` is the pre-existing home of the #879 bootstrap tests these two classes extend. The rule's literal `tests/` tree is not how any C# test in this repository is laid out. |
| Banned APIs in test code | **PASS** | Re-derived. None present (grep above). |

### 1.1 Determinism Infrastructure

No clock, RNG, timer or async path exists in either new class, so no `TimeProvider`, seeded RNG or fake
timer is required. The only I/O is read-only `XDocument.Load` over project files and a read-only
`FileStream` wrapped in `PEReader`, both disposed via `using`.

### 1.2 Coverage Metrics

Coverage artifact used: the post-change Cobertura document written by the Coverage Command Of Record
(`scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug`) at
`coverage/coverage.cobertura.xml` in the execution worktree (git-ignored; mtime 2026-09-17 01:27 local;
root `timestamp="1789622819"`, which matches the P4-T9 run time). Its root element was read directly and
matches the committed extract `evidence/qa-gates/test-final.2026-09-16T23-27.md` exactly. The baseline
figures come from the committed extract `evidence/baseline/test-coverage-baseline.2026-09-16T23-27.md`
(the raw baseline document was overwritten in place by the post-change run).

**Coverage Metrics by Language:**

| Language | Files Changed | Tests | Test Result | Baseline Coverage | Post-Change Coverage | New Code Coverage |
|---|---|---|---|---|---|---|
| C# | 7 | 7311 | 7311 passed, 0 failed, 0 skipped | 85.87% lines / 80.05% branch | 85.87% lines / 80.03% branch | N/A |
| TypeScript | 0 | 0 | N/A | N/A | N/A | N/A |
| Python | 0 | 0 | N/A | N/A | N/A | N/A |
| PowerShell | 0 | 0 | N/A | N/A | N/A | N/A |

The C# `New Code Coverage` cell has no numeric value because the changed-production-line set is empty: the
three edited `.csproj` files contain no executable lines, the two added `.cs` files are test code that the
runner excludes from instrumentation (re-derived: zero occurrences of either class name in the Cobertura
document), and the third `.cs` file changed only inside `///` comment lines.

### Coverage Evidence Checklist

- C# baseline coverage artifact: `docs/features/active/2026-09-14-fsharp-core-hintpath-netstandard21-skew-895/evidence/baseline/test-coverage-baseline.2026-09-16T23-27.md` (committed extract of the merge-base run; the raw baseline Cobertura was overwritten in place by the post-change run)
- C# post-change coverage artifact: `coverage/coverage.cobertura.xml` in the execution worktree (git-ignored, read directly by this review; root line-rate 0.858678, branch-rate 0.800317) with the committed extract `docs/features/active/2026-09-14-fsharp-core-hintpath-netstandard21-skew-895/evidence/qa-gates/test-final.2026-09-16T23-27.md`
- TypeScript baseline coverage artifact: zero TypeScript files changed on this branch; no artifact required
- TypeScript post-change coverage artifact: zero TypeScript files changed on this branch; no artifact required
- PowerShell baseline coverage artifact: zero PowerShell files changed on this branch; no artifact required
- PowerShell post-change coverage artifact: zero PowerShell files changed on this branch; no artifact required
- Python baseline coverage artifact: zero Python files changed on this branch; no artifact required
- Python post-change coverage artifact: zero Python files changed on this branch; no artifact required
- Per-language comparison summary: the per-language coverage comparison block below

### 1.2.1 Per-Language Coverage Comparison

- C#: Baseline: 85.87% lines (56345/65616) -> Post-change: 85.87% lines (56343/65616). Change: -0.003% lines (2 fewer covered lines on an identical 65616-line denominator; branch 80.05% -> 80.03%; zero changed production lines, so no per-line comparison exists). Disposition: PASS. Evidence: the two committed extracts named in the checklist above and the git-ignored `coverage/coverage.cobertura.xml` root element read directly by this review.
- TypeScript: Baseline: N/A. Post-change: N/A. Change: N/A. Disposition: N/A. Evidence: N/A - zero TypeScript files changed on this branch.
- Python: Baseline: N/A. Post-change: N/A. Change: N/A. Disposition: N/A. Evidence: N/A - zero Python files changed on this branch.
- PowerShell: Baseline: N/A. Post-change: N/A. Change: N/A. Disposition: N/A. Evidence: N/A - zero PowerShell files changed on this branch.

### 1.2.2 Coverage Artifact State

- The canonical path `artifacts/csharp/coverage.xml` is absent in both the execution worktree and the
  session-root checkout. It was not created by this review: the reviewer does not rerun or relocate
  coverage generation, and the repository's evidence-location rule treats the feature folder, not
  `artifacts/`, as the durable location. The Cobertura document at `coverage/coverage.cobertura.xml`
  is the runner's own output location and was used as the present post-change artifact.
- `coverage/coverage.cobertura.jacoco.xml` (the runner's JaCoCo summary) was also read: package
  counters sum to the same figures. Package rates from the Cobertura document, for reference:
  QuickFiler 81.72%, UtilitiesCS 90.30%, TaskVisualization 90.89%, SVGControl 47.30%, ToDoModel 58.20%,
  Tags 92.61%, TaskMaster 75.29%, TaskTree 96.41%, VBFunctions 100%. None of these packages contains a
  line this change touched.
- Durability caveat (I-7): the raw Cobertura, TRX and build logs are git-ignored, so the independent
  re-derivation performed here is repeatable only while the execution worktree exists; the committed
  markdown extracts are the durable record.

### 1.3 Coverage Exclusion Policy

No `exclude` entry, `[ExcludeFromCodeCoverage]` attribute, `coverage.config` or runsettings change appears
in the footprint (re-derived from the name-only diff). **PASS.**

## 2. General Code Change Policy Compliance

Source: `.claude/rules/general-code-change.md` and CLAUDE.md.

| Requirement | Verdict | Evidence and verification basis |
|---|---|---|
| Bugfix Workflow — failing regression test first | **PASS** | Evidence-attested, corroborated on disk. Commit `8454cf902` (tests, observed failing) precedes `59144b4ab` (fix). `expect-fail-shape-a`: `TOTAL=2 PASSED=1 FAILED=1` with the flavour test failing; `expect-fail-shape-b`: `TOTAL=16 FAILED=3`, the `[QuickFiler]`, `[QuickFiler.Test]`, `[ToDoModel]` rows, control passed. The P1-T6 TRX at `TestResults/p1-t6/p1-t6.trx` was read directly and its `<Message>` names all three offending project files. |
| Bugfix Workflow — minimal, targeted fix | **PASS** | Re-derived. Three one-line HintPath edits; `-U0` hunks `@@ -52 +52 @@`, `@@ -259 +259 @@`, `@@ -42 +42 @@`. No reordering or reformatting in any project file. |
| Bugfix Workflow — open a new issue rather than widen scope | **PARTIAL, non-blocking** | Re-derived. Two latent defects are recorded rather than fixed (`ToDoModel.Test/packages.config` omission; `Sync-PackageReferences.ps1:13-19` TFM order). Both were verified on disk by this reviewer. Neither is yet filed as a GitHub issue. See W-3. |
| Simplicity first | **PASS** | Re-derived. The fix is the smallest possible change (one path segment per file). The tests use direct file reads and metadata inspection with no abstraction layer. |
| Reusability | **PASS** | Re-derived. `FindRepositoryRoot` and `DiscoverFSharpCoreHintPaths` are `internal static` and reused by Shape B rather than duplicated. |
| Separation of concerns | **PASS** | Re-derived. No production code changed. |
| Fail fast and explicitly | **PASS** | Re-derived. Missing precondition throws `InvalidOperationException` with the full path; no catch clause exists in either new file. |
| No broad catch-all added | **PASS** | Re-derived. Zero `catch` in the footprint. |
| Invariants enforced at initialization | **PASS** | Not applicable to test-only code; no constructor state. |
| Naming conventions | **PASS** | Re-derived. PascalCase types and methods, camelCase locals, descriptive test names. |
| No public API break | **PASS** | Re-derived. No production surface changed; the two helpers are `internal`. |
| No new dependencies | **PASS** | Re-derived. `System.Reflection.Metadata 10.0.0.12` was already referenced by `TaskMaster.Test.csproj` and pinned in its `packages.config`; no `packages.config` changed. |
| **File size limit — 500 lines** | **PASS** | Re-derived by `wc -l`: `FSharpCoreHintPathAlignmentTests.cs` = **211**; `FSharpCoreDeployedIdentityTests.cs` = **244**; `NetstandardBindChildDomainTests.cs` = **470** (466 at base; 30 headroom, see I-5). |
| Toolchain loop run in order, restarting on any change | **PASS** | Evidence-attested and corroborated. One iteration, `REWRITTEN-COUNT: 0`, `FORMAT_CHANGED_TREE=False`; content hashes of the three `.cs` files identical before and after the format step. Build-log counts re-derived in section 7. |
| Absolute host paths in committed artifacts | **PASS** | Re-derived. Grep over the full branch diff for the user-profile drive prefix, the account name and the machine name returns one match, which is a removed line in an agent-memory index quoting the generic `<user-profile>\<account>` placeholder form; no real path. Evidence records the worktree leaf name only. |

## 3. Language-Specific Code Change Policy Compliance

Source: CLAUDE.md §C#1–C#7 and `.claude/rules/csharp.md`.

| Requirement | Verdict | Evidence and verification basis |
|---|---|---|
| CSharpier formatting via `dotnet tool run` | **PASS** | Evidence-attested. `format-final`: `dotnet tool run csharpier format .` exit 0, `Formatted 1641 files`, SHA-256 of all three `.cs` files unchanged; `dotnet tool run csharpier check .` exit 0, `Checked 1641 files in 5450ms.` = baseline 1639 + 2 new files, `CHECKED-DELTA-RESIDUAL: 0`. Project files are excluded by `.csharpierignore` (`*.csproj`), re-derived by reading that file. |
| `dotnet format` not used | **PASS** | Re-derived. No `dotnet format` string in any evidence artifact. |
| Analyzer gate `/t:Rebuild`, not `/t:Build` | **PASS** | Evidence-attested. Command recorded verbatim in `analyzer-final` with `/t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`. |
| Analyzer gate non-vacuous | **PASS** | **Re-derived** from `coverage/logs/p4-t7-analyzer.txt`: `Skipping target "CoreCompile"` = 0; `^\s+0 Error(s)$` = 1; 36 `out:obj` lines (2 per project across 18 projects). 0 warnings, 0 errors. |
| Nullable gate command form, no `/p:Nullable=enable` | **PASS** | Evidence-attested. `nullable-final` records the CLAUDE.md form character-for-character and states `/p:Nullable=enable` was not added. |
| Nullable gate non-vacuous, zero CS86xx | **PASS** | **Re-derived** from `coverage/logs/p4-t8-nullable.txt`: `Skipping target "CoreCompile"` = 0; `^\s+0 Error(s)$` = 1. Neither new file carries `#nullable`, matching the two sibling files in `TaskMaster.Test/Bootstrap/`; this is a recorded convention choice, not a suppression. |
| Strong contracts, explicit types at boundaries | **PASS** | Re-derived. Helper return types are explicit (`IReadOnlyList<(string, string)>`, `IReadOnlyList<(string Name, Version Version)>`); `var` appears once on `using var reader = new PEReader(stream)` where the type is on the right-hand side. |
| XML documentation on non-obvious API | **PASS** | Re-derived. Both classes, all four helpers and all four test methods carry XML docs; the `<remarks>` on `DiscoverFSharpCoreHintPaths` explains why dot-prefixed directories are skipped. |
| Comment *why*, comments synchronized with behavior | **PASS** | Re-derived. The corrected `<remarks>` on `ProbeApplicationBase` now describes the post-fix state; the surviving `unsatisfiable` occurrence (line 281) remains true because that test binds the display name directly. |
| No suppression added | **PASS** | Re-derived. No `#pragma warning disable`, `SuppressMessage` or `null!` in either new file. |
| No analyzer severity, `.editorconfig`, or `.globalconfig` change | **PASS** | Re-derived from the name-only diff. |
| Project-file discipline (`*.csproj` in scope of this rule) | **PASS** | Re-derived. Four project files changed; three by exactly one HintPath value, one by two `Compile Include` insertions placed beside the existing three Bootstrap entries. No `<Analyzer Include>` line touched (issue #898 remains separate). |

## 4. Language-Specific Unit Test Policy Compliance

Source: CLAUDE.md §CUT1–CUT3.

| Requirement | Verdict | Evidence and verification basis |
|---|---|---|
| MSTest framework | **PASS** | Re-derived. `using Microsoft.VisualStudio.TestTools.UnitTesting;` in both files; `[TestClass]`, `[TestMethod]`, `[DataTestMethod]` + 15 `[DataRow]`. No xUnit or NUnit. |
| Moq for mocking | **PASS (not required)** | Re-derived. No seam exists to mock: the units under test are the tree and the deployed binaries. |
| FluentAssertions for assertions | **PASS** | Re-derived. Every assertion uses `.Should()`: `HaveCount`, `BeEmpty`, `ContainSingle().Which.Should().Be`, `NotContain`, `NotBeEmpty`. No MSTest `Assert.*`. |
| MSTest attribute style | **PASS** | Re-derived. See I-9 for the `[DataTestMethod]` spelling note. |
| Toolchain command selection | **PASS** | See sections 3 and 7. |

## 5. Test Coverage Detail

### Repo-wide, per language

- **C#** — repo-wide line coverage **85.87%** (56343/65616), branch coverage **80.03%** (13623/17022),
  read directly from the root element of `coverage/coverage.cobertura.xml`. Line >= 85% and branch >= 75%
  (rules files) and line >= 80% (CLAUDE.md) are all met. Baseline: 85.87% (56345/65616) / 80.05%.
  Verdict: **PASS**.
- **PowerShell** — zero changed `.ps1`/`.psm1` files on the branch.
- **Python** — zero changed `.py` files on the branch.
- **TypeScript** — zero changed `.ts`/`.tsx` files on the branch.

### Changed files (7 source paths)

| File | Status | Instrumented? | Verdict |
|---|---|---|---|
| `QuickFiler/QuickFiler.csproj` | Modified (1/1) | No executable lines | PASS |
| `QuickFiler.Test/QuickFiler.Test.csproj` | Modified (1/1) | No executable lines | PASS |
| `ToDoModel/ToDoModel.csproj` | Modified (1/1) | No executable lines | PASS |
| `TaskMaster.Test/TaskMaster.Test.csproj` | Modified (2/0) | No executable lines | PASS |
| `TaskMaster.Test/Bootstrap/FSharpCoreHintPathAlignmentTests.cs` | Added (211) | Test assembly, excluded by the runner's `.*\.Test\.dll$` rule; 0 occurrences in the Cobertura document | PASS |
| `TaskMaster.Test/Bootstrap/FSharpCoreDeployedIdentityTests.cs` | Added (244) | Same | PASS |
| `TaskMaster.Test/Bootstrap/NetstandardBindChildDomainTests.cs` | Modified (11/7, comment-only) | Same | PASS |

### Changed-line coverage — re-derived

The changed-production-line set is empty. Two complementary spans were re-run by this reviewer:
`git diff --name-only origin/main...HEAD -- "*.cs"` lists only the three `TaskMaster.Test/Bootstrap/`
paths, and `git status --porcelain --untracked-files=all` on the execution worktree is empty. There is
therefore no per-line figure to compute and no regression possible on changed lines.

The two new tests are nonetheless exercised: the P4-T9 full-suite TRX records `NEW_TEST_RESULT_COUNT=18`
/ `NEW_TEST_PASSED_COUNT=18` (evidence-attested), and the total grew from 7293 to 7311, which is exactly
the 18 results the two classes contribute (2 + 15 + 1).

**Coverage verdict: PASS. No coverage remediation trigger.**

## 6. Test Execution Metrics

Re-derived where a document exists on disk; evidence-attested otherwise.

- Baseline full suite (P0-T7, merge-base tree, before any test file existed): 7293 total / 7293 passed /
  0 failed; Cobertura POSTPROCESSED, line-rate 0.858708, branch-rate 0.800493, 65616 lines valid.
  Baseline failing set: NONE (neither the #780 intermittent nor the breadcrumb thread-affinity tests failed).
- Expect-fail Shape A (P1-T6, unfixed tree): 2 total / 1 passed / 1 failed; `EveryFSharpCoreHintPath_SelectsNetstandard20`
  failed, `SolutionHasExactlySixFSharpCoreHintPaths` passed (count 6 proves the enumeration live). TRX read
  directly by this reviewer at `TestResults/p1-t6/p1-t6.trx`.
- Expect-fail Shape B (P1-T7, unfixed freshly rebuilt tree): 16 total / 13 passed / 3 failed; failed rows
  `[QuickFiler]`, `[QuickFiler.Test]`, `[ToDoModel]`, each message containing `2.1.0.0`; control passed;
  no additional failing rows (the six flip-capable directories received netstandard2.0 on that build,
  recorded as an observed build-order outcome).
- Pass-after Shape A (P4-T2): 2 / 2 / 0. Pass-after Shape B (P4-T3): 16 / 16 / 0.
- Whole `TaskMaster.Test.Bootstrap` namespace (P4-T4): 29 / 29 / 0, including
  `NegativeControl_WithoutInstall_Netstandard21Throws` passed (the #879 isolation invariant holds).
- Final full suite with coverage (P4-T9): 7311 total / 7311 passed / 0 failed / 0 skipped;
  `RUNSETTINGS-UNCHANGED: NONE`; `NEWLY-FAILING: NONE`; single run, no re-run, nothing serialised.
- Independent post-state check by this reviewer (no test run; check-only): SHA-256 of every deployed
  `FSharp.Core.dll` in all fifteen enumerated `bin/Debug` directories equals the package's
  `lib/netstandard2.0` binary (`9C29FE3DB01726CF…`) and none equals the `lib/netstandard2.1` binary
  (`3882D154D637EFBA…`); `SVGControl`, `SVGControl.Test` and `VBFunctions` receive no copy, as the
  research predicted. `TaskMaster.Test.dll` mtime 05:26:03Z postdates the P4-T8 nullable rebuild start.

## 7. Code Quality Checks

| Gate | Command form | Result | Verification basis |
|---|---|---|---|
| 1. Format | `dotnet tool run csharpier format .` | exit 0; `Formatted 1641 files`; `REWRITTEN-COUNT: 0`; `FORMAT_CHANGED_TREE=False` | Evidence-attested (hash pairs recorded) |
| 2. Format verify | `dotnet tool run csharpier check .` | exit 0; `Checked 1641 files in 5450ms.` | Evidence-attested; lower bound 1639+2 holds with equality |
| 3. Analyzers | `msbuild TaskMaster.sln /t:Rebuild /m ... /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` | exit 0; 0 Warning(s); 0 Error(s); skipped CoreCompile 0 | **Re-derived** from `coverage/logs/p4-t7-analyzer.txt` |
| 4. Nullable | `msbuild TaskMaster.sln /t:Rebuild /m ... /p:TreatWarningsAsErrors=true` | exit 0; 0 Warning(s); 0 Error(s); skipped CoreCompile 0 | **Re-derived** from `coverage/logs/p4-t8-nullable.txt` |
| 5. Coverage-enabled tests | Coverage Command Of Record | exit 0; 7311 / 7311 / 0; POSTPROCESSED | **Re-derived** root element of `coverage/coverage.cobertura.xml` |

Loop discipline: one iteration, no restart, no file changed by the formatter. `loop-closure` records five
`EXPECTATION-MET: YES` lines and `LOOP: CLEAN PASS` (anchored counts re-derived: 5 and 1; see I-1 for the
naive-count note).

Positive-control re-derivation for the two build gates and the two whole-solution rebuilds (P1-T5,
P4-T1): each of the fifteen enumerated projects has exactly two log lines carrying
`/out:obj\Debug\<name>.dll`, one beginning with the `csc.exe` path and one beginning `BuildResponseFile =`.
Exactly one compiler invocation per project in every log. See W-2.

Confidentiality masking scan: clean (section 2, last row). Suppression scan (added lines): zero
suppressions. Workflow change scan: `git diff --name-only origin/main...HEAD -- .github` is empty; no
workflow was modified, so no green-run gate applies.

**No policy document, coverage threshold, analyzer severity, runsettings or exclusion list was lowered,
weakened, or deleted.** Re-derived: none of `.claude/rules/**`, `CLAUDE.md`, `.editorconfig`,
`coverage.config`, `scripts/vscode/TaskMaster.cli.runsettings` or any `app.config`/`packages.config`
appears in the footprint.

## 8. Gaps and Exceptions

Blocking: **0**. PARTIAL acceptance criteria: **0**. Warnings: **3** (W-1, W-2, W-3). Informational: **9**.

### Rulings requested by the caller

#### Item (a) — P3-T2 and P4-T15 checked off although the acceptance literals were not observed — **W-1, Warning, ratified deviation, non-blocking**

- **Facts re-derived:** `git diff --numstat origin/main...HEAD -- TaskMaster.Test/Bootstrap/NetstandardBindChildDomainTests.cs`
  = `11 7`; `git diff -U0` hunk header `@@ -364,7 +364,11 @@`; non-`///` changed lines = **0**;
  `unsatisfiable` occurrences now 1 (was 2); the `because` string at line 206 is untouched. The plan's
  literals `CHANGED_LINES=22`, numstat `13 9`, deletions 9 were derived at preflight revision R1 item (4)
  from the size of the replaced span (13 new lines for 9 old) rather than from the diff git renders. The
  first and last lines of the replacement (`/// <remarks>` and `/// </remarks>`) are identical to the
  originals, so git emits them as context; 13−2 = 11 and 9−2 = 7. The executor's explanation is correct
  and the orchestrator's independent re-measurement agrees.
- **Ruling on the fail-closed rule:** the rule as written ("a named artifact is absent, or is present but
  missing a required field, → BLOCKED or INCOMPLETE, never PASS") is not triggered: both artifacts exist
  and every required field is present with a measured value. What is unmet is a stated acceptance
  literal, which is a different condition. An executor may not silently pass a task whose literal is
  unmet; this executor did not do so silently — both artifacts carry a `NOT AS WRITTEN` clause with the
  root cause and the deviation is escalated in the completion report. The `[x]` marks are therefore a
  recorded deviation awaiting ratification rather than a concealed failure. The orchestrator's
  re-measurement and this review's re-derivation ratify it: the literal was arithmetically wrong, and the
  substantive AC5 claim (comment-only) is measured directly by `NON_COMMENT_CHANGED_LINES=0`.
- **Disposition:** acceptable, non-blocking. The stricter path (leave both tasks unchecked and halt for a
  plan amendment) was available and would have been equally correct; the chosen path is acceptable only
  because the deviation is recorded in the artifacts and now ratified outside them. Owed record: the PR
  body or merge record should state that P3-T2 and P4-T15 were closed on the corrected literals
  (18 / `11 7` / 7). Not a code change; not a remediation trigger.

#### Item (b) — P1-T5 and P4-T1 gate `CSC_OUT=1`; measured 2 — **W-2, Warning, independently verified, non-blocking**

- **Independent verification (re-derived, not previously done by the orchestrator):** over
  `coverage/logs/p1-t5-build.txt` and `coverage/logs/p4-t1-build.txt`, for each of the fifteen enumerated
  projects the token `/out:obj\Debug\<name>.dll` appears on exactly **2** lines; of those, exactly **1**
  contains `csc.exe` and exactly **1** begins `BuildResponseFile =`. Total `out:obj` lines per log = 36
  = 2 × 18 projects, which is also the `CSC_OUT_LINES=36` the Phase 0 baselines recorded under an
  "at least 15" lower bound. `Skipping target "CoreCompile"` = 0 in both logs. The executor's
  `CSC_INVOCATIONS=1` disambiguation is therefore confirmed for all fifteen projects in both logs.
- **Ruling:** the literal `CSC_OUT=1` is unsatisfiable by any correct build at this MSBuild version's
  normal verbosity; the clause's intent (one real compilation per project) is met and evidenced by the
  csc.exe-line count together with `SKIPPED_CORECOMPILE=0` and `OWN_DLL_FRESH=True`. Same class and same
  disposition as W-1. Note that P5-T2's check-off condition also cites "fifteen `CSC_OUT=1` lines"; AC2's
  own text in `spec.md` does not, so the acceptance criterion is unaffected.

#### Item (c) — `BeEmpty(because)` with offenders projected into the reason — **sound; assertion not weakened**

- `FSharpCoreHintPathAlignmentTests.cs:196-209`. The assertion is `offenders.Should().BeEmpty(reason)`.
  Pass/fail semantics depend only on whether `offenders` is empty; the `because` argument is diagnostic
  text and cannot make a non-empty collection pass or an empty one fail. The projection is evaluated
  eagerly on every run (`string.Join` over a list that is empty on the passing path), which is cheap and
  side-effect free. The P1-T6 TRX message on disk confirms the design does what it claims: the reason
  segment lists all three offenders and the renderer's `{...}` segment names one representative.
- The alternative `BeEquivalentTo(Array.Empty<string>())` would render the full collection but reads
  less clearly; the chosen form is the better test design. No finding.

#### Loop-closure legibility — **I-1, informational**

- `evidence/qa-gates/loop-closure.2026-09-16T23-27.md`: a naive substring count reads 6 `EXPECTATION-MET:`
  and 3 `LOOP:` because lines 37–38 of the Acceptance section restate the tokens in backticks. Anchored
  counts (`^EXPECTATION-MET:` and `^LOOP:`) read 5 and 1, which is what P4-T10 requires. The single
  `LOOP:` declaration is `LOOP: CLEAN PASS` at line 33; the word `BLOCKED` at line 38 is prose
  ("rather than `BLOCKED`") and does not form a `LOOP: BLOCKED` declaration.
- **Ruling:** no defect in the declared values; a line-anchored reader gets the required counts. Leaving
  the file as committed was the right call given the terminal clean-tree gate. Recommendation for future
  artifacts: do not restate declaration tokens in prose, or use the plain word without the colon.

### Warnings

- **W-1** — see item (a). Owed: ratification note in the PR body / merge record.
- **W-2** — see item (b). Owed: same note. Upstream: the planner's per-project `CSC_OUT` literal should
  count `csc.exe`-prefixed lines only.
- **W-3 — Two latent defects recorded but not yet filed as issues (owed, non-blocking).** Both verified on
  disk by this reviewer: `ToDoModel.Test/packages.config` has no `FSharp.Core` and no `Deedle` entry while
  `ToDoModel.Test/ToDoModel.Test.csproj` carries HintPaths for both; `scripts/vscode/Sync-PackageReferences.ps1:13-19`
  lists `netstandard2.1` before `netstandard2.0` in `$tfmPreference`. The spec, plan, `ac-status` and the
  issue mirror all record them as follow-ups "not opened by this plan". Evidence prose inside a feature
  folder does not survive archival, so both must be promoted into real GitHub issues before #895 is
  closed. Recommended owner: the orchestrator, via the promotion lifecycle.

### Informational

- **I-1** — loop-closure naive count (ruled above).
- **I-2** — Shape A skips every directory whose name begins with `.` (`:101`), a superset of AC1's literal
  list (`.git`, `.claude`) that also excludes `.dotnet-sdk`, `.vs` and `.github`. None of those holds a
  project file; the count test (6) would expose any over-exclusion. Acceptable and documented in the
  method's `<remarks>`.
- **I-3** — Shape B's second assertion (`:180-192`) checks `NotContain(2.1.0.0)` over *all* referenced
  assembly versions, not only the `netstandard` reference. This mirrors the spec text ("contains no
  reference at version 2.1.0.0") and is over-broad only in theory: the netstandard2.0 flavour references
  `netstandard 2.0.0.0` and `System.Runtime.Numerics 4.0.1.0` (read directly). No action.
- **I-4** — `NetstandardBindChildDomainTests.cs` carries `[DoNotParallelize]` (line 30) and a comment
  naming it (line 22): 2 occurrences, identical at base, placed by #879. Nothing was added by this change
  (new files: 0 and 0). Constraint block 1 is satisfied.
- **I-5** — `NetstandardBindChildDomainTests.cs` is at 470 of 500 lines. The next non-trivial edit to
  that file will need to split it.
- **I-6** — Seven `.claude/agent-memory/` files are in the branch diff on commit `f1c53f9b8` (preparation
  run). They are outside the plan's Write Set by the inherited-path rule and are tracked files that will
  merge with the branch. Host-path scan over that commit: zero matches. Content is agent memory notes; not
  further audited.
- **I-7** — The canonical `artifacts/csharp/coverage.xml` path is not populated and the raw Cobertura,
  TRX and build logs are git-ignored. The committed markdown extracts are the only durable record; this
  review's direct re-derivation is repeatable only while the execution worktree exists.
- **I-8** — Environment observations outside this change's scope: `CLAUDE.md` states there is no
  `Directory.Build.props`, while the research artifact records one (18 lines); and issue #898 (Meziantou
  analyzer HintPath skew) was worked around at P0-T3 by materialising `3.0.203` into the git-ignored
  `packages/` directory with no project file touched (re-derived: no `<Analyzer Include>` line in the
  diff). CI build health with respect to #898 is that issue's concern.
- **I-9** — `[DataTestMethod]` (`FSharpCoreDeployedIdentityTests.cs:94`) is the legacy spelling; MSTest 3+
  accepts `[TestMethod]` with `[DataRow]`. The analyzer gate produced no diagnostic. Optional tidy-up.

## 9. Summary of Changes

| Path | Status | Lines (base → head) | Notes |
|---|---|---|---|
| `QuickFiler/QuickFiler.csproj` | Modified | 1/1 | HintPath line 52: `lib\netstandard2.1` → `lib\netstandard2.0`. |
| `QuickFiler.Test/QuickFiler.Test.csproj` | Modified | 1/1 | HintPath line 259, same edit. |
| `ToDoModel/ToDoModel.csproj` | Modified | 1/1 | HintPath line 42, same edit. |
| `TaskMaster.Test/TaskMaster.Test.csproj` | Modified | 2/0 | Two `Compile Include` items after `AddInEagerInstallShapeTests.cs`. |
| `TaskMaster.Test/Bootstrap/FSharpCoreHintPathAlignmentTests.cs` | Added | 211 | Shape A: count-of-six and flavour tests; `FindRepositoryRoot`, `DiscoverFSharpCoreHintPaths` helpers. |
| `TaskMaster.Test/Bootstrap/FSharpCoreDeployedIdentityTests.cs` | Added | 244 | Shape B: 15 `[DataRow]` deployed-binary rows plus positive control; `ReadAssemblyReferences` via `PEReader`. |
| `TaskMaster.Test/Bootstrap/NetstandardBindChildDomainTests.cs` | Modified | 466 → 470 (11/7) | `<remarks>` on `ProbeApplicationBase` rewritten; comment-only. |
| `docs/features/.../spec.md` | Modified | 5 check-offs | `- [ ]` → `- [x]` on AC1–AC5 only; no criterion text changed (re-derived). |
| `docs/features/.../plan.2026-09-16T23-27.md` | Modified | 48 check-offs | Task checkboxes only (re-derived: no non-checkbox line changed after `91e2e951a`). |
| `docs/features/.../{issue.md, research/…}` | Added | — | Feature documents. |
| `docs/features/.../evidence/**` | Added | 28 files | All under canonical evidence kinds. |
| `.claude/agent-memory/**` | Modified/Added | 7 files | Preparation-run memory notes, commit `f1c53f9b8`. |

Not modified, and verified not modified: `UtilitiesCS/UtilitiesCS.csproj`, `UtilitiesCS.Test/UtilitiesCS.Test.csproj`,
`ToDoModel.Test/ToDoModel.Test.csproj`, every `app.config` and `packages.config`, `ChildDomainBindProbe.cs`,
`AddInEagerInstallShapeTests.cs`, `UtilitiesCS/Bootstrap/AssemblyBindingFallback.cs`,
`scripts/vscode/TaskMaster.cli.runsettings`, `coverage.config`, `.editorconfig`, `CLAUDE.md`, every file
under `.claude/rules/` and every path under `.github/workflows/`.

## 10. Compliance Verdict

**PASS. Blocking findings: 0.**

The fix is the minimal correct change: three one-segment HintPath edits that remove the only unloadable
flavour from every path the repository controls. The regression tests detect the defect at two layers
(source and deployed binary), were observed failing on the unfixed tree in exactly the predicted rows and
passing after the fix, carry a positive control that proves the detector can see a 2.1.0.0 reference, and
are read-only, deterministic and parallel-safe. The comment-only correction keeps a doc comment truthful
after the fix. The full C# toolchain passed in one clean iteration with every gate figure independently
re-derived from the on-disk logs and Cobertura document, and the deployed-binary state was independently
confirmed by hashing all fifteen copies against the two package flavours.

The three warnings are process items: two plan-literal prediction errors that the executor recorded and
that independent re-measurement has now ratified (W-1, W-2), and two latent defects that are correctly
scoped out but still owed as GitHub issues (W-3). None requires a code change on this branch.

No `remediation-inputs` artifact is produced. Ready to merge.

---

## Appendix A: Test Inventory

`TaskMaster.Test/Bootstrap/FSharpCoreHintPathAlignmentTests.cs` — 2 `[TestMethod]`, both passing.

| # | Test | Line | Role | Seam |
|---|---|---|---|---|
| 1 | `SolutionHasExactlySixFSharpCoreHintPaths` | 154 | AC1 count gate (non-vacuity control for the flavour test) | `XDocument` over `*.csproj` under the repository root |
| 2 | `EveryFSharpCoreHintPath_SelectsNetstandard20` | 178 | **AC1 primary gate**, observed failing pre-fix | same; offenders projected into the reason |

`TaskMaster.Test/Bootstrap/FSharpCoreDeployedIdentityTests.cs` — 1 `[DataTestMethod]` × 15 `[DataRow]` + 1 `[TestMethod]`, all 16 passing.

| # | Test | Line | Role | Seam |
|---|---|---|---|---|
| 3–17 | `DeployedFSharpCore_ReferencesNetstandard20 [<dir>]` for QuickFiler, QuickFiler.Test, ToDoModel, UtilitiesCS, UtilitiesCS.Test, ToDoModel.Test, Tags, Tags.Test, VBFunctions.Test, TaskTree, TaskTree.Test, TaskVisualization, TaskVisualization.Test, TaskMaster, TaskMaster.Test | 94–156 | **AC2 primary gate**, three rows observed failing pre-fix | `PEReader` / `MetadataReader.AssemblyReferences` over `<dir>/bin/Debug/FSharp.Core.dll` |
| 18 | `Detector_OnPackageNetstandard21Binary_Reports21` | 209 | AC2 positive control | same, over the package's `lib/netstandard2.1` binary resolved from a discovered HintPath |

`TaskMaster.Test/Bootstrap/NetstandardBindChildDomainTests.cs` — 9 `[TestMethod]`, unchanged (comment-only
edit); all 9 passing in the P4-T4 namespace run, including `NegativeControl_WithoutInstall_Netstandard21Throws`.
`AfterInstall_DeedleTypeInitializerSucceeds` now passes with or without the #879 installer (recorded, not fixed).

## Appendix B: Toolchain Commands Reference

Run in this exact order; any failure or formatter auto-fix restarts at step 1.

1. `dotnet tool run csharpier format .`
2. `dotnet tool run csharpier check .`
3. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
4. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`
5. `vstest.console.exe <test-assembly-paths> /EnableCodeCoverage` — realised in this run as the Coverage
   Command Of Record, `scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug`,
   with scoped `vstest.console.exe … /Settings:scripts/vscode/TaskMaster.cli.runsettings /InIsolation`
   runs for the expect-fail / pass-after pairs.

Non-vacuity assertions applied to steps 3 and 4 and to the two whole-solution rebuilds: zero occurrences
of `Skipping target "CoreCompile"` paired with a positive count of compiler invocations (one `csc.exe`
line per project). An exit code alone cannot distinguish a clean compile from a skipped one.
