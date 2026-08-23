# Policy Audit — ribbon-controller-engines-null-unsafe (#507)

Timestamp: 2026-08-08T17-45
Work Mode: `minor-audit`
Scope: full branch diff, `git diff 003c5715055d7d1933db68a742531332756e30b2...HEAD` (branch
`bug/ribbon-controller-engines-null-unsafe-507` vs merge base `003c5715055d7d1933db68a742531332756e30b2`,
head `e589fad7`).

## Executive Summary

The change is a single-line null-conditional guard (`Globals.Engines` -> `Globals?.Engines`) in
`TaskMaster/Ribbon/RibbonController.Intelligence.cs`, plus two new MSTest regression tests in
`TaskMaster.Test/Ribbon/RibbonControllerTests.cs`. The production change is minimal, matches the
sibling `SB` precedent, and is verified by evidence. Two Blocking findings were identified: (1) the
modified test file now exceeds the repository's 500-line file-size limit, and (2) the fix relocates
rather than eliminates the reachable `NullReferenceException` for every one of the 11 real
production call sites of `Engines` (all live in `RibbonViewer.cs`, none null-guarded). Total
Blocking count: **2**. Full detail in `code-review.2026-08-08T17-45.md` and
`feature-audit.2026-08-08T17-45.md`.

## Rejected Scope Narrowing

None detected. The task prompt's context items (coverage exemption rationale, nullable-gate
divergence rationale, out-of-scope `RibbonViewer.cs` confirmation, "you do not need to re-run the
toolchain") point to pre-existing, fully evidenced verification artifacts rather than instruct
skipping any check; none of them ask this audit to omit a toolchain stage or a coverage row for a
language with changed files. No caller text is recorded here because none met the narrowing
criteria in the Scope Invariant.

One instruction required active handling rather than rejection: "Do NOT create or write
`artifacts/csharp/coverage.xml`." This is not scope narrowing — it does not ask coverage
verification to be skipped. It points at feature-evidence Cobertura files instead of the canonical
hook path so that an incomplete write does not trip a hard-coded 85% floor check against a partial
artifact. This audit still produces an explicit C# coverage verdict below, sourced from the
feature-evidence Cobertura files, consistent with the canonical evidence-location convention (see
`## Evidence Location Compliance`).

## Evidence Location Compliance

`validate_evidence_locations.py` is not present in this repository's `scripts/` tree (checked via
`git diff --name-only` and a repo-wide search; no such script exists), so it could not be invoked.
Manual scan of the branch diff for `artifacts/baselines/`, `artifacts/qa/`, `artifacts/evidence/`,
or `artifacts/coverage/` paths found **zero** matches
(`git diff 003c5715055d7d1933db68a742531332756e30b2...HEAD --name-only | grep -i "artifacts/"`
returned no output). All evidence in this change is written under the canonical
`docs/features/active/2026-08-08-ribbon-controller-engines-null-unsafe-507/evidence/{baseline,
regression-testing,qa-gates,other}/` tree. No violation.

This review's own artifacts (`artifacts/pr_context.summary.txt`, `artifacts/pr_context.appendix.txt`)
were hand-authored in this session because the `collect_pr_context` MCP tool was unavailable; they
sit at the canonical PR-context locations defined in `pr-context-artifacts` (not evidence
artifacts, so the evidence-location rule does not apply to them). No `artifacts/csharp/coverage.xml`
was created, per the reviewed feature's explicit instruction and to avoid a false floor-check
against a partial/absent artifact.

## 1. Coverage Verification

### 1.1 Changed languages

`git diff --numstat` shows exactly two `.cs` files touched (one production, one test); no
`.ts`/`.tsx`/`.py`/`.ps1`/`.psm1` files are in the diff. **CSharp** is the only language requiring a
coverage verdict.

### 1.2 CSharp coverage row

- **Artifact used**: `artifacts/csharp/coverage.xml` (canonical hook path) is intentionally absent
  in this session (see `## Rejected Scope Narrowing`). Verification instead uses the feature's own
  committed Cobertura evidence:
  `docs/features/active/2026-08-08-ribbon-controller-engines-null-unsafe-507/evidence/baseline/phase0-baseline-coverage.cobertura.xml`
  (baseline, pre-fix) and
  `docs/features/active/2026-08-08-ribbon-controller-engines-null-unsafe-507/evidence/qa-gates/phase2-final-coverage.cobertura.xml`
  (post-fix), both produced by `dotnet-coverage merge -f cobertura` against the same 9-assembly
  vstest run used for AC5/AC6 verification.
- **Baseline**: repo-wide raw `line-rate` = **74.43%** (`lines-covered` 158,543 / `lines-valid`
  213,002; per `evidence/baseline/phase0-baseline-vstest-coverage.md`).
- **Post-change**: repo-wide raw `line-rate` = **61.66%** (`lines-covered` 160,251 / `lines-valid`
  259,906; per `evidence/qa-gates/phase2-final-vstest-coverage.md`).
- **Change**: -12.77 percentage points on the raw repo-wide denominator, but `lines-covered`
  *increased* by 1,708. The evidence author traced the swing to `dotnet-coverage`'s known run-to-run
  denominator nondeterminism (enumerated `<class>` count grew 1,924 -> 2,336 with no assembly-set
  change), then ran a per-file `line-rate` comparison across all 1,924 baseline files: 0 files
  missing from the final run, exactly 1 file (`SubjectMapSco.Orchestration.cs`, untouched by this
  feature) regressed by more than 1 point, attributable to ordinary async/test-order variance.
  `RibbonController.Intelligence.cs` does not appear as an instrumented class in either file
  (consistent with the `[ExcludeFromCodeCoverage]` exemption ratified on `RibbonController`), so the
  changed production line adds no coverage surface in either direction.
- **New/changed-code coverage**: N/A — no new files were added; the sole modified production line
  sits inside an exempt, non-instrumented class per the ratified VSTO/COM ribbon-handler exemption
  (`CLAUDE.md` § UT2; `TaskMaster/Ribbon/RibbonController.cs:36`). The modified test file is
  correctly excluded from the coverage denominator per policy (coverage tooling excludes test
  files).
- **Disposition**: **FAIL** against both the uniform 85%/75% floor (`.claude/rules/general-unit-test.md`)
  and the CLAUDE.md § UT2 80% floor, on the raw (unfiltered, vendor-inclusive) repo-wide figure —
  both 74.43% and 61.66% are below floor. This condition is **pre-existing and not caused by this
  change**: it was already present at baseline before the fix was applied, the per-file comparison
  shows zero attributable regression, and the changed line itself carries no coverage surface. Per
  this repository's established disposition pattern for pre-existing sub-floor repo-wide C# coverage
  (raw `dotnet-coverage` merges undercount due to vendor/third-party assembly inclusion; a
  first-party-only figure has previously been shown to clear 80%/85% — no such filtered figure was
  computed by the executor for this feature), this FAIL is recorded as **non-blocking for this PR**
  and is not counted toward this review's Blocking total. It is not a new remediation trigger for
  this minor-audit bugfix; it is a standing repository condition that should be tracked separately.
- **Verdict**: **FAIL** (repo-wide raw line coverage below floor) — **non-blocking disposition**,
  pre-existing, evidenced no regression from this change.

### 1.3 Other languages

TypeScript, Python, PowerShell: zero changed files in the branch diff. No coverage row required or
produced for these languages (not narrowed — genuinely zero changed files, confirmed via
`git diff --numstat`).

## 2. Toolchain Verification (C#)

All four stages were run by the orchestrator in a single clean pass and are backed by
timestamped, command+exit-code+output evidence:

| Stage | Command | EXIT_CODE | Evidence |
|---|---|---|---|
| Format | `csharpier check .` | 0 | `evidence/qa-gates/phase2-final-csharpier.md` (1488 files, 0 reformatted) |
| Analyzers | `msbuild ... /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` | 0 | `evidence/qa-gates/phase2-final-msbuild-analyzers.md` (0 errors) |
| Nullable (CI-enforced form) | `msbuild ... /t:Rebuild /p:TreatWarningsAsErrors=true` (no `/p:Nullable=enable`, matching `.github/workflows/ci.yml`) | 0 | `evidence/qa-gates/phase2-orchestrator-ci-gate-reconciliation.md` (0 errors, 0 CS8603) |
| Test | `vstest.console.exe <9 assemblies> /EnableCodeCoverage` | 0 | `evidence/qa-gates/phase2-final-vstest-coverage.md` (6296/6296 passed in the executor's own run; the orchestrator's independent re-run in the reconciliation artifact records 6295/6295 — see § 5 below for the discrepancy note) |

**CLAUDE.md-vs-ci.yml nullable command divergence**: `CLAUDE.md` § C#1.3 and § CUT3 document
`/p:Nullable=enable` as the nullable toolchain command; `.github/workflows/ci.yml`'s enforced gate
omits that flag and relies on each file's own `#nullable enable` pragma. Under the literal
`CLAUDE.md` command, the changed line does emit `CS8603` (verified by the executor, reported in
`evidence/qa-gates/phase2-final-msbuild-nullable.md`), because `RibbonController.Intelligence.cs`
carries no `#nullable` pragma and `/p:Nullable=enable` forces analysis on it anyway, surfacing 195 +
219 pre-existing errors elsewhere in the solution unrelated to this change. `.github/workflows/ci.yml`
is the gate that actually governs merge (per the Policy Compliance Order, CLAUDE.md is read first,
but the enforced CI gate is the operative merge check; the two diverging is itself the defect).
Under the CI-enforced command, the change is clean (0 errors, 0 CS8603). This CLAUDE.md/ci.yml
divergence is a genuine, pre-existing repository documentation defect, independent of this change,
and per the reviewed feature's own instruction it is reported here for separate triage rather than
treated as a defect in this PR: **Informational, not blocking.** Recommend a documentation-fix issue
be opened against `CLAUDE.md` §§ C#1.3/CUT3 to either match `ci.yml`'s command or add `/p:Nullable=enable`
to the CI gate (a repository-wide decision, out of scope for a minor-audit single-line bugfix).

## 3. General Code Change Policy

- **Simplicity/minimal diff**: PASS. One production line changed; matches the existing `Globals?.`
  pattern already used by the sibling `SB` property and by the two other `Globals?.Engines?...`
  chains already present in the same file (lines 198, 288).
- **Scope boundary**: PASS. `git diff --name-only` confirms only
  `TaskMaster/Ribbon/RibbonController.Intelligence.cs` (production) and
  `TaskMaster.Test/Ribbon/RibbonControllerTests.cs` (test) were touched among source files;
  `TaskMaster/Ribbon/RibbonViewer.cs` is absent from the diff, independently confirmed by this
  review's own `git diff --numstat` (see § Evidence Location Compliance) and by
  `evidence/qa-gates/phase2-ribbonviewer-guard.md`.
- **File size limit (CLAUDE.md § 4.1 / `.claude/rules/general-code-change.md` § File Size Limit)**:
  **FAIL — Blocking.** `TaskMaster.Test/Ribbon/RibbonControllerTests.cs` was 452 lines at the merge
  base (`git show 003c5715055d7d1933db68a742531332756e30b2:TaskMaster.Test/Ribbon/RibbonControllerTests.cs | wc -l`)
  and is 513 lines at HEAD (`wc -l TaskMaster.Test/Ribbon/RibbonControllerTests.cs`) — the two new
  test methods (61 added lines) push it 13 lines past the repository's 500-line hard cap. No
  exception in the policy applies: this is not a throwaway script, a raw text fixture, or Markdown.
  See `code-review.2026-08-08T17-45.md` for full detail and remediation.
- **Error handling / logging / contracts**: N/A for this diff — no new error-handling or logging
  code was introduced; the fix is a return-expression change only.
- **Naming**: PASS. Test method names (`Engines_WhenGlobalsNotAssigned_ReturnsNullInsteadOfThrowing`,
  `Engines_WhenGlobalsAssigned_ReturnsGlobalsEngines`) are descriptive and follow existing
  conventions in the file.

## 4. General + C# Unit Test Policy

- **Framework/mocking/assertions**: PASS. Both new tests use `[TestMethod]` (MSTest), `Moq`
  (`new Mock<IAppItemEngines>().Object` in the second test), and FluentAssertions
  (`.Should().NotThrow()`, `.Should().BeNull()`, `.Should().BeSameAs()`).
- **Arrange-Act-Assert**: PASS. Both tests carry explicit `// Arrange`, `// Act`, `// Assert`
  comments (first test's assert is embedded inside the `Action` under test, with `NotThrow()` as
  the outer assertion — a standard FluentAssertions pattern for asserting no-throw plus a value
  simultaneously).
- **Independence/isolation**: PASS. Each test builds its own `RibbonController`/`ApplicationGlobals`
  instance (`new RibbonController()` or `CreateController()`); neither touches `Settings.Default`
  (unlike other tests in the file, which is why those use `[TestInitialize]`/`[TestCleanup]`
  snapshot/restore — the new tests correctly do not need that machinery). The class carries
  `[DoNotParallelize]`, consistent with the file's existing tests.
- **Determinism / no temp files / no external dependencies**: PASS. No filesystem, network, or
  environment dependency in either test; reflection is used only against in-process objects.
- **Coverage exemption compliance**: PASS. Neither test attempts to remove or widen
  `[ExcludeFromCodeCoverage]` on `RibbonController`, matching the explicit constraint in `issue.md`.
- **Test file size**: see § 3 above (Blocking finding, file-size limit).

## 5. Evidence Consistency Note (Informational)

Two independent post-fix vstest runs disagree on total test count: the executor's Phase 2 run
(`evidence/qa-gates/phase2-final-vstest-coverage.md`) reports 6296/6296 passed (baseline 6294 + 2
new tests, arithmetically consistent); the orchestrator's independent reconciliation run
(`evidence/qa-gates/phase2-orchestrator-ci-gate-reconciliation.md`) reports 6295/6295 passed. Both
runs report zero failures and `total == passed`, so AC6 ("no pre-existing test regresses... no
worse than the recorded Phase 0 baseline") is satisfied either way. The one-test discrepancy between
6296 and 6295 is not explained in either artifact and is most likely attributable to test-discovery
or `TestCategory` filter variance between the two separate invocations (the reconciliation run added
a `/TestCaseFilter:"TestCategory!=LiveOutlook"` clause not present in the Phase 2 executor run).
Recorded here as an evidence-hygiene note; does not change any AC verdict. **Informational, not
blocking.**

## 6. Summary of Findings by Severity

| Severity | Count | Findings |
|---|---|---|
| Blocking | 2 | File-size limit exceeded (test file, § 3); `Engines` null-return relocates rather than eliminates NRE at 11 unguarded `RibbonViewer.cs` call sites (see `feature-audit.2026-08-08T17-45.md` and `code-review.2026-08-08T17-45.md`) |
| Non-blocking | 0 | — |
| Informational | 3 | CLAUDE.md/ci.yml nullable command divergence (pre-existing, reported separately); evidence test-count discrepancy (6296 vs 6295); pre-existing sub-floor repo-wide C# coverage (dispositioned non-blocking) |

**Total Blocking count: 2.**
