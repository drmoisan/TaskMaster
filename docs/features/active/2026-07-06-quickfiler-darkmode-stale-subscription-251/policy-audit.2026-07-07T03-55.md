# Policy Audit — Issue #251 (quickfiler-darkmode-stale-subscription)

- Timestamp: 2026-07-07T03-55
- Work Mode: minor-audit
- AC Source: `docs/features/active/2026-07-06-quickfiler-darkmode-stale-subscription-251/issue.md` (`## Acceptance Criteria`, AC1-AC8)
- Base branch (resolved): `main` @ `c7c8e7e7ea8ce53d552745e9a15ef02cbce599e0` (independently recomputed via `git merge-base HEAD origin/main`; matches the caller-supplied merge-base)
- Head SHA: `6304fa13af4c8dfa9e9c5273f40dac04579e9178`
- Range: `c7c8e7e7ea8ce53d552745e9a15ef02cbce599e0..6304fa13af4c8dfa9e9c5273f40dac04579e9178`

## Executive Summary

The change is a minimal, targeted defect fix in `QfcCollectionController` (unsubscribe the dark-mode `PropertyChanged` handler in `Cleanup()`/`CleanupAsync()` before nulling `_globals`, plus a defensive guard in the handler itself). Two new MSTest/Moq/FluentAssertions regression tests reproduce the pre-fix `NullReferenceException` and confirm the post-fix no-throw/no-side-effect behavior. All four toolchain stages (CSharpier, analyzer build, nullable build, MSTest) were independently re-verified by this review and pass with no regressions. Repo-wide C# coverage was freshly measured with the full multi-assembly suite (all 8 built `*.Test.dll` projects, 4991 tests, matching the shape of `.github/workflows/ci.yml`'s test step) rather than trusting a single-assembly figure. No blocking findings were identified. Several pre-existing, non-blocking conditions (unrelated to this diff) are documented below for transparency.

**Verdict: PASS** (no blocking findings introduced by this branch).

## Scope Determination (independent of caller framing)

Full `git diff --name-status` between merge-base and head (27 files):

- Production C#: `QuickFiler/Controllers/QfcCollectionController.cs` (M, +35/-3)
- Test C#: `QuickFiler.Test/Controllers/QfcCollectionControllerDarkModeTests.cs` (A, +155/-0)
- Test project config: `QuickFiler.Test/QuickFiler.Test.csproj` (M, +1/-0, adds one `<Compile Include>` item)
- Memory doc: `.claude/agent-memory/orchestrator/pr-author-hook-blocks-gh-in-this-repo.md` (M, docs-only)
- 22 feature-folder docs/evidence files under `docs/features/active/2026-07-06-quickfiler-darkmode-stale-subscription-251/` (issue.md, plan, evidence baselines/qa-gates/regression-testing, including two large Cobertura XML evidence files)

No TypeScript, Python, or PowerShell files are present in the diff. Only C# is in scope for language-specific policy/coverage checks.

## Rejected Scope Narrowing

- **Stale/incorrect automated classification (recurring defect, not a caller-supplied narrowing attempt):** `artifacts/pr_context.summary.txt`'s "Changed files overview" originally reported `Core logic changes: 0 files` and omitted the two changed `.cs` files and the `.csproj` change entirely from its per-file `(+N/-M)` enumeration, listing only doc/evidence files under "Docs/templates/agents/tooling: 23 files". This is the same recurring PR-context-summary misclassification observed on issues #171, #181, and #244 (see `.claude/agent-memory/feature-review/project_pr-context-summary-misclassifies-cs.md`). It was corrected in place in `artifacts/pr_context.summary.txt` (a `[STALE-EVIDENCE CORRECTION]` block with the three omitted `(+N/-M)` lines, sourced from `git diff --numstat`) so that both this audit and the `validate-feature-review-coverage.ps1` SubagentStop hook's `Get-ChangedLanguageSet` parser operate on truthful data. This audit's scope determination above was made directly from `git diff --name-status`/`--numstat`, not from the (subsequently corrected) summary artifact.
- No caller prompt, plan, or delegation instruction in this session attempted to narrow scope, mark any language out of scope, or instruct skipping a toolchain/coverage check. No other narrowing instances were found.

## 1. Toolchain Verification (C#)

All four stages independently re-run by this review (not merely read from executor evidence):

| Stage | Command | Result (this review) | Executor evidence (corroborating) |
|---|---|---|---|
| Format | `csharpier check QuickFiler/Controllers/QfcCollectionController.cs QuickFiler.Test/Controllers/QfcCollectionControllerDarkModeTests.cs` | `Checked 2 files in 632ms.` — 0 files need reformatting | `evidence/qa-gates/csharpier-final.2026-07-06T23-08.md`, `csharpier-final-iteration2...md` — both EXIT_CODE 0 |
| Analyzer build | `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` | EXIT_CODE 0, 0 errors, 0 warnings (full solution) | `evidence/qa-gates/csharp-analyzers-final.2026-07-06T23-08.md` — EXIT_CODE 0, 1 pre-existing unrelated `MSTEST0032` warning |
| Nullable build | `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true` | EXIT_CODE 0, 0 errors, 0 warnings (full solution) | `evidence/qa-gates/csharp-nullable-final.2026-07-06T23-08.md` — EXIT_CODE 0, 0/0 |
| Test | `vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /InIsolation /TestCaseFilter:"FullyQualifiedName~QfcCollectionControllerDarkModeTests"` | 2/2 passed (`Cleanup_ThenDarkModePropertyChanged_DoesNotThrow`, `CleanupAsync_ThenDarkModePropertyChanged_DoesNotThrow`) | `evidence/regression-testing/targeted-vstest-coverage.2026-07-06T23-08.md`, `evidence/qa-gates/csharp-vstest-coverage-final...md` — 488/488 full-suite pass |

Toolchain order (format → analyzer → nullable → test) was followed by the executor per the evidence trail (Phase 0 baseline, Phase 1 red→green, Phase 2 final loop); no step required a restart. **PASS.**

## 2. Coverage Verification

### 2.1 C# (CSharp) — changed files present on this branch

**Methodology note (per this review's explicit instruction to avoid understating repo-wide coverage):** the feature's own evidence (`evidence/qa-gates/csharp-vstest-coverage-final.2026-07-06T23-08.md`) measured coverage with a single test assembly (`QuickFiler.Test.dll` only) and explicitly labels its repo-wide figure as "includes vendored/third-party" (20.23%) — this single-assembly, non-canonical methodology understates true first-party repo-wide coverage and is not used as this audit's coverage-gate figure. This review instead built the full solution and ran `scripts/vscode/Invoke-MSTestWithCoverage.ps1` (the canonical Koverage script), which discovers all built `*.Test.dll` under the repo (7 assemblies produced a coverage-eligible DLL: `QuickFiler.Test`, `TaskVisualization.Test`, `Tags.Test`, `ToDoModel.Test`, `TaskMaster.Test`, `UtilitiesCS.Test`, `VBFunctions.Test`; `SVGControl.Test`/`UtilitiesSwordfish.Test` are not present in `TaskMaster.sln` and are out of the solution's own test scope), runs them together under `dotnet-coverage collect` with `coverage.config` third-party exclusions, and writes Cobertura XML to the canonical path `artifacts/csharp/coverage.xml`. Result: **4991/4991 tests passed**, 0 failures.

- **Repo-wide C# line coverage: 81.27%** (`lines-covered=79556 / lines-valid=97896`, source `artifacts/csharp/coverage.xml`, freshly generated, not committed — gitignored per `.gitignore` `artifacts/`).
- **Repo-wide C# branch coverage: 73.68%** (`branches-covered=17201 / branches-valid=23346`, same source).
- **Coverage-gate verdict: PASS** against this repo's own inline C# gate in `CLAUDE.md` (`>= 80%` line, no branch gate; per the `#178` governance-sync decision this 80%/90% line-only gate is this repo's adopted C# policy) and against this review's procedural "Verification Procedure" numeric gate (repo-wide FAIL only below 80%; 81.27% >= 80%). **No remediation trigger.**
- **Policy-conflict note (pre-existing, not caused by this branch):** `.claude/rules/quality-tiers.md` and `.claude/rules/general-unit-test.md` currently state a uniform `>= 85%` line / `>= 75%` branch floor for all tiers ("tier-specific lower thresholds are not used in this repository"), which conflicts with `CLAUDE.md`'s own inline `>= 80%` line-only C# gate. Under the stricter 85%/75% reading, repo-wide C# **does not meet** the line floor (81.27% < 85%, a 3.73-point gap) or the branch floor (73.68% < 75%, a 1.32-point gap). This gap is a repo-wide condition that predates this branch and is not attributable to this diff (see the `QuickFiler` package comparison below, which shows no regression from this PR's own touched code). This audit records the operative PASS per the procedural 80% gate and CLAUDE.md, and documents the 85%/75% shortfall transparently rather than silently resolving the two conflicting policy documents.
- **`QuickFiler` package (the package containing the touched file):** line-rate 72.48% (0.724793388429752), branch-rate 62.95%, complexity 1525 (fresh full-suite measurement) — consistent with, and marginally higher than, the executor's single-assembly baseline/final figures (72.42424242424242%, bit-for-bit identical between baseline and final in the executor's own evidence). No regression.
- **New/changed-code coverage for the sole production file, `QuickFiler/Controllers/QfcCollectionController.cs`:** the class `QfcCollectionController` carries a class-level `[ExcludeFromCodeCoverage]` attribute. This attribute is confirmed present, unchanged, at the merge-base revision (`git show c7c8e7e7...:QuickFiler/Controllers/QfcCollectionController.cs` shows the identical attribute at the identical line) — it is a pre-existing condition, not introduced by this diff. Grepping the fresh `artifacts/csharp/coverage.xml` for `QfcCollectionController` returns zero `<class>` entries: the whole class, including the three changed regions (`Cleanup()`, `CleanupAsync()`, `DarkMode_CheckedChanged`), is excluded from the coverage tool's measured denominator in both the baseline and post-change state — there is no numeric line/branch coverage figure to report for the changed lines, and by the same token no possible regression, since neither state measured anything for this class. The new test file's own class (`QuickFiler.Controllers.Tests.QfcCollectionControllerDarkModeTests`) reports `line-rate="1"` (100%) in the fresh Cobertura output, matching the executor's targeted-run evidence.
- **Exemption-scope observation (pre-existing, non-blocking for this PR; see code-review for detail):** `CLAUDE.md`'s COM/VSTO/WinForms coverage exemption text explicitly carves back "testable seams... are explicitly NOT exempt." `QfcCollectionController`'s constructor takes only interface-typed collaborators (`IApplicationGlobals`, `IQfcFormViewer`, `IFilerHomeController`, `IFilerFormController`) and no raw `Microsoft.Office.Interop.Outlook` types — an injectable seam that this PR's own regression tests demonstrate is sufficient to fully unit-test the class with Moq, with no Outlook/COM dependency. Whether the pre-existing class-wide `[ExcludeFromCodeCoverage]` attribute is still correctly scoped given this demonstrated testability is a legitimate open question, but the attribute predates this branch and this PR does not add or remove it, so it is not treated as a blocking finding on this PR.

### 2.2 TypeScript / Python / PowerShell — no changed files on this branch

`git diff --name-only` for the full range contains zero `.ts`/`.tsx`/`.py`/`.ps1`/`.psm1` files. These languages are correctly excluded from the coverage-verification requirement because they have no changed files in the branch diff (not because of any scope narrowing).

## 3. AC6 — Minimal, Targeted Change Verification

`git diff --name-status` confirms the only **production** file changed is `QuickFiler/Controllers/QfcCollectionController.cs`. All other changed files are: one new test file, one test-project config change (`<Compile Include>` wiring), one unrelated memory doc, and feature-folder docs/evidence. **PASS.**

## 4. File Size Limit (`general-code-change.md`, `.claude/rules/csharp.md`)

`QuickFiler/Controllers/QfcCollectionController.cs` is 2340 lines after this change (2308 at merge-base, +32 net) — both far exceed the repo's 500-line file-size limit. **This is a pre-existing violation, not introduced by this branch** (the file was already 2308 lines before this fix). Per the Bugfix Workflow guidance ("change only what is needed... if you uncover deeper design problems, open a new issue instead of widening scope"), refactoring this legacy class into multiple <=500-line files is out of scope for a minimal, targeted defect fix and is not required by this PR. Documented as a pre-existing condition; **non-blocking for this PR.**

`QuickFiler.Test/Controllers/QfcCollectionControllerDarkModeTests.cs` is 155 lines — well within limit.

## 5. Evidence Location Compliance

All 22 evidence/doc files added by this branch reside under the canonical `docs/features/active/2026-07-06-quickfiler-darkmode-stale-subscription-251/evidence/<kind>/` tree (baseline/, qa-gates/, regression-testing/, issue-updates/), consistent with `evidence-and-timestamp-conventions`. No files were found under `artifacts/baselines/`, `artifacts/qa/`, `artifacts/evidence/`, or `artifacts/coverage/` in the branch diff.

`validate_evidence_locations.py` does not exist in this repository (searched full tree; not present) — this specific automated check is **UNVERIFIED (script absent)**; the manual `git diff` scan above found zero violations and is the basis for the "no violations found" conclusion.

This review's own freshly-generated `artifacts/csharp/coverage.xml` was written to the canonical, language-specific path named in this task's "Coverage Artifact Paths by Language" table (a fixed, non-feature-specific location distinct from the per-feature `<FEATURE>/evidence/<kind>/` tree used for audit-trail evidence produced by the executor). It is gitignored and not committed to the branch.

## 6. Architecture Boundaries / CI Workflows / Benchmark Baselines

No files under `.github/workflows/**` or `scripts/benchmarks/**` are present in the diff; `ci-workflows.md` and `benchmark-baselines.md` are not triggered. No TypeScript/`.NET` layer-boundary changes are present; `architecture-boundaries.md` is not triggered (this is legacy VSTO/COM code, already exempted from the No-COM rules which apply to *new* runtime code).

## 7. Summary Table

| Check | Verdict | Note |
|---|---|---|
| Toolchain (format/analyzer/nullable/test) | PASS | independently re-verified |
| C# repo-wide coverage (CLAUDE.md 80% gate / procedural 80% gate) | PASS | 81.27% line, fresh full-suite measurement |
| C# repo-wide coverage (quality-tiers.md 85%/75% uniform gate) | FAIL | 81.27% line / 73.68% branch; pre-existing, unrelated to this PR |
| New/changed production-code coverage | Not numerically measurable | pre-existing `[ExcludeFromCodeCoverage]` on the touched class; no regression possible (excluded in both states) |
| AC6 (minimal/targeted change) | PASS | only one production file changed |
| File size limit | Pre-existing violation (non-blocking) | 2340-line file predates this diff |
| Evidence location compliance | PASS (manual scan); UNVERIFIED (script absent) | no violations found |
| Architecture boundaries / CI workflows / benchmarks | Not triggered | no matching files changed |

## Overall Disposition

**PASS.** No blocking findings were introduced by this branch.

The C# repo-wide coverage figure is documented under both conflicting repo policy statements (CLAUDE.md 80%/quality-tiers.md 85%) for transparency; the operative, procedurally-directed verdict is PASS, and the gap under the stricter reading is a pre-existing, non-regressed, repo-wide condition unrelated to this diff.

AC8 (CI checks green on the PR head SHA) remains open pending PR creation; it is deferred by explicit plan authorization, not a coverage finding — see feature-audit.
