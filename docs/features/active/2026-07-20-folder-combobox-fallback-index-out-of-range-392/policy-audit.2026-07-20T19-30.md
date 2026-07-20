# Policy Compliance Audit — folder-combobox-fallback-index-out-of-range (Issue #392)

- Timestamp: 2026-07-20T19-30
- Reviewer: feature-review (remediation re-audit, cycle 1, R4; structurally repaired in place per
  MCP validator feedback — content and PASS verdict unchanged from the first write of this file)
- Base branch (resolved): `main`
- Merge-base SHA: `bd43572498474be89d80e1f9620dffb132ade377`
- Head SHA: `8a1b7b98b7d12dac69fd1bee5d5f109d4095c3c6` (two commits ahead of merge-base:
  `8f34f8ef` fix, `8a1b7b98` remediation)
- Work Mode: `minor-audit` (AC source: explicit `## Acceptance Criteria` section of `issue.md`)
- Scope: full branch diff vs merge-base (feature-vs-base). Not narrowed to any plan, task, or phase.

## Executive Summary

This is a re-audit after remediation cycle 1, which addressed both open items from
`policy-audit.2026-07-20T18-00.md` Section 5:

- **R1** — the marginal class-level branch-coverage gap in
  `QfcItemController.FolderHandling.cs` (73.81%, floor 75%) is closed: one new MSTest test
  (`PopulateFolderComboBox_WhenInvokeRequired_MarshalsAssignFolderComboBoxViaInvoke`) exercises a
  previously-uncovered, pre-existing branch with zero production-code change, raising class-level
  coverage to **95.95% line / 76.19% branch**, independently re-verified against the regenerated
  canonical `artifacts/csharp/coverage.xml`.
- **R2** — the `QuickFiler` package-wide (73.72%/64.69%) and canonical repo-wide artifact
  (16.26%/13.61%) coverage gaps remain below the 85%/75% floor, unchanged in substance from cycle 1,
  but are now properly **ratified** via a `human_interaction` `scope_change` entry in
  `artifacts/orchestration/orchestrator-state.json`, citing open GitHub issue #136
  (*quickfiler-80-per-file-coverage*), the `#328` `StoreWrapper` precedent, and CLAUDE.md's
  COM/VSTO/WinForms testable-denominator exemption — the same ratification pattern used for the
  `StoreWrapper` exception in issue #328.

The production and test diff for the two Scope-Lock files is unchanged from cycle 1 except for one
added test method (verified: `git diff bd435724..8a1b7b98 -- QuickFiler/Controllers/QfcItemController.FolderHandling.cs`
is byte-identical to cycle 1's diff; the test file's only substantive change beyond comment removal
is the one new test). All five acceptance criteria remain PASS, unaffected by this coverage-only
cycle. The toolchain passes (format, analyzers, tests; nullable reproduces the same dispositioned
pre-existing vendored condition). 542/542 tests pass, up from 541, with zero regressions.

**Overall verdict: PASS (go). Blocking findings: 0.** One item — the `QuickFiler` package-wide and
canonical repo-wide coverage gap — remains below the 85%/75% floor and is recorded as FAIL on its raw
numeric verdict, but is a ratified, documented pre-existing exception (not introduced or worsened by
this branch; baseline was 73.68%/64.62%, virtually unchanged at 73.72%/64.69%), consistent with the
`#328` `StoreWrapper` precedent, and does not block this cycle's exit gate.

**C# coverage verdict:** touched-class coverage is PASS (95.95% line / 76.19% branch); package-wide and repo-wide coverage is FAIL on the raw numeric floor check, dispositioned as a ratified, non-blocking, pre-existing exception per Section 8.

- Changed languages with files in the branch diff: **C# only** (2 `.cs` files: 1 production, 1 test).
  TypeScript, Python, and PowerShell have zero changed files on this branch; their coverage rows
  below are recorded `N/A - out of scope` per policy, not omitted.

## 1. General Unit Test Policy Compliance

| Requirement | Verdict | Evidence |
|---|---|---|
| Independence | PASS | The new test constructs its own `Mock<IItemViewer>`, `Mock<IApplicationGlobals>`, and `FolderController`; no shared mutable state with any other test in the file. |
| Isolation | PASS | The new test targets a single unit of behavior (`PopulateFolderComboBox`'s `InvokeRequired == true` marshaling branch). |
| Fast execution | PASS | 542/542 tests complete in 7.7651 seconds (`evidence/qa-gates/remediation-vstest-coverage-final.2026-07-20T18-40.md`). |
| Determinism | PASS | No `Thread.Sleep`, `Task.Delay`, `DateTime.Now/UtcNow`, temp files, or `new Random()` in the new or modified test code (grep clean on the changed test file). |
| Readability / maintainability | PASS | Descriptive test name (`PopulateFolderComboBox_WhenInvokeRequired_MarshalsAssignFolderComboBoxViaInvoke`) and an inline comment stating the scenario and expected outcome. |
| Coverage — repo-wide >= 85% line / 75% branch | FAIL at package/repo-wide scope, PASS at touched-class scope | See Section 5 (Test Coverage Detail) for the full breakdown and disposition. |
| Scenario completeness | PASS | The new test closes a previously-untested `InvokeRequired == true` marshaling branch, an established idiom already used identically in five sibling test files in this project (per `evidence/other/branch-gap-analysis.2026-07-20T18-20.md`). |
| Arrange-Act-Assert structure | PASS | The new test follows Arrange (mock/controller setup) - Act (`controller.PopulateFolderComboBox()`) - Assert (`viewer.Verify(...)`) ordering, delineated by blank lines. |
| External dependencies / mocks | PASS | Moq (`Mock<IItemViewer>`, `Mock<IApplicationGlobals>`) used to isolate the unit under test; no network, database, or filesystem access. |
| No temporary files | PASS | Grep of the changed test file confirms no `Path.GetTemp*` or filesystem writes. |
| Test file location | PASS | `QuickFiler.Test/Controllers/QfcItemController.FolderHandlingTests.cs` mirrors the production path `QuickFiler/Controllers/QfcItemController.FolderHandling.cs` under the existing `QuickFiler.Test` tree; no colocation in `src/`-equivalent production trees. |

## 2. General Code Change Policy Compliance

| Requirement | Verdict | Evidence |
|---|---|---|
| Simplicity first | PASS | No production code changed this cycle; the original fix remains a single ternary clamp at each of the two pre-existing fallback sites. |
| Reusability | PASS with carried-forward note | The `<count> == 1 ? 0 : 1` clamp remains duplicated across `AssignFolderComboBox` and `PopulateAndSelectFolder` (unchanged from cycle 1's code-review Finding CR-1); non-blocking, tracked for future cleanup. |
| Extensibility / public API compatibility | PASS | No signature changes to `AssignFolderComboBox()` or `PopulateAndSelectFolder(...)`. |
| Separation of concerns | PASS | No I/O, COM, or UI framework logic was added or changed this cycle. |
| Error handling / fail-fast | PASS | No new try/catch or swallowed exceptions. |
| Backward compatibility | PASS | Existing multi-suggestion and predetermined-folder behavior is unchanged and re-verified (542/542 tests pass). |
| File size <= 500 lines | PASS | `QuickFiler/Controllers/QfcItemController.FolderHandling.cs`: 235 lines (unchanged). `QuickFiler.Test/Controllers/QfcItemController.FolderHandlingTests.cs`: 498 lines (was exactly 500 at cycle-1 entry with zero headroom; 26 purely-structural `// Act`/`// Assert` comment-header lines were removed from 12 pre-existing tests — verified via diff to be comment-only deletions, no assertion/name/behavior change — then the one new test was added, netting 498 lines, now with 2 lines of headroom). |
| I/O boundary isolation | PASS | No disk/network/API code added; `PopulateAndSelectFolder` remains pure WinForms logic with no `InvokeRequired` marshaling, as before. |
| Dependency approval | PASS | No new dependency added; Moq/MSTest/FluentAssertions already approved for this repo. |

## 3. Language-Specific Code Change Policy Compliance

Language in scope: C# only. Evidence source: executor QA-gate artifacts under `evidence/qa-gates/remediation-*` (this cycle).
Independently re-verified where practical on this review host.

| Stage | Command | Evidence | Exit | Verdict |
|---|---|---|---|---|
| Formatting (CSharpier) | `csharpier format .` then `csharpier check .` | `evidence/qa-gates/remediation-csharpier-final.2026-07-20T18-30.md` | 0 (both runs) | PASS — independently reproduced: `csharpier check .` run directly on this review host (csharpier 1.3.0) returns "Checked 1406 files in 2616ms" with **0 errors**, confirming the evidence's claim. |
| Linting / .NET analyzers | `MSBuild.exe TaskMaster.sln /t:Build ... /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` | `evidence/qa-gates/remediation-analyzer-final.2026-07-20T18-32.md` | 0 | PASS |
| Type checking / nullable | `MSBuild.exe TaskMaster.sln /t:Rebuild ... /p:Nullable=enable /p:TreatWarningsAsErrors=true` | `evidence/qa-gates/remediation-nullable-final.2026-07-20T18-35.md` | 1 (34 errors, byte-identical error-set to the original P0-T11 baseline, all attributed exclusively to vendored `SVGControl.csproj`; 0 new, 0 first-party) | PASS (dispositioned; unchanged from cycle 1's AC-5 scope note — nullable enforcement scoped to first-party projects per `.claude/rules/csharp.md`) |

**Evidence-quality note (non-blocking):** cycle 1's baseline (`evidence/baseline/csharpier-baseline.2026-07-20T13-15.md`,
captured 2026-07-20T13-15) recorded 32 pre-existing `app.config`/`packages.config` formatting errors;
this cycle's evidence attributes their disappearance to upstream commit `78e847ec` ("style: apply
csharpier formatting to dependabot config changes"). That commit is confirmed to already be an
ancestor of the merge-base (`git merge-base --is-ancestor 78e847ec bd435724` succeeds), meaning it
predates both the merge-base and the original 13:15 baseline capture — so it cannot be the direct
cause of a change observed only between 13:15 and 18:30 on the same checkout. The practical fact
(0 errors, independently reproduced on this review host) is not in question; the causal attribution
in the evidence narrative is imprecise, most likely reflecting a working-tree/tool-state difference
between the two capture times rather than a defect in this branch. This does not change the PASS
verdict.

No production `.cs`, `.csproj`, `.props`, or `.targets` file was changed in this remediation cycle.

## 4. Language-Specific Unit Test Policy Compliance

Language in scope: C# only.

| Requirement | Verdict | Evidence |
|---|---|---|
| Framework: MSTest | PASS | The new test uses `[TestMethod]` inside the existing `[TestClass] QfcItemController_FolderHandlingTests`. |
| Mocking: Moq | PASS | `Mock<IItemViewer>`, `Mock<IApplicationGlobals>` used in the new test. |
| Assertions: FluentAssertions preferred, MSTest-style acceptable when FluentAssertions is not practical | PASS | The new test uses `viewer.Verify(v => v.Invoke(It.IsAny<Delegate>()), Times.Once())` (Moq-idiomatic verification), consistent with sibling tests in the same file and project that verify mock interactions the same way. |
| MSTest style (`[TestClass]`/`[TestMethod]`) | PASS | Confirmed. |
| Test-file line-count regression | PASS | Net +18 lines cumulative from merge-base (61 added / 43 removed); the removed lines are comment-only (verified via diff). |
| No test weakened or deleted | PASS | `git diff 8f34f8ef..8a1b7b98` shows the only non-comment change is the addition of one new test method; all pre-existing assertions, names, and mock setups are byte-identical. |

## 5. Test Coverage Detail

Mandatory for C# (changed files present). Canonical artifact: `artifacts/csharp/coverage.xml`
(JaCoCo, regenerated this cycle at `evidence/qa-gates/remediation-coverage-conversion.2026-07-20T18-42.md`),
independently re-parsed on this review host (Python `xml.etree.ElementTree`).

**Coverage Metrics by Language:**

| Language | Files Changed | Tests | Test Result | Baseline Coverage | Post-Change Coverage | New Code Coverage |
|----------|--------------|-------|-------------|-------------------|---------------------|-------------------|
| C# | 2 files | 542 tests | PASS 542 pass, 0 fail | 91.89% lines, 73.81% branch (touched class) | 95.95% lines, 76.19% branch (touched class) | 100% |

(TypeScript, Python, and PowerShell rows are omitted per the template note — zero changed files for
those languages on this branch; see the `### Coverage Evidence Checklist` below for the explicit
`N/A - out of scope` record.)

### Coverage Metrics — Multi-Scope Breakdown (supplementary detail)

The single required table row above summarizes the touched class only. This repo's coverage policy
(`.claude/rules/quality-tiers.md`) also requires evaluating package-wide and repo-wide C# coverage;
the richer breakdown below is supplementary detail supporting the Section 5 findings and is not a
substitute for the required table above.

| Language | Scope | Baseline Coverage | Post-Change Coverage | New Code Coverage | Disposition |
|---|---|---|---|---|---|
| C# | `QfcItemController.FolderHandling.cs` (touched class) | 91.89% line / 73.81% branch | 95.95% line / 76.19% branch | 100% line (original fix's 5 sequence points) | PASS |
| C# | `QuickFiler` package (whole assembly) | 73.67% line / 64.53% branch | 73.72% line / 64.69% branch | N/A - no new file added | FAIL (floor), dispositioned ratified non-blocking exception |
| C# | Canonical artifact, raw six-package aggregate | 16.25% line / 13.60% branch | 16.26% line / 13.61% branch | N/A - no new file added | FAIL (floor), dispositioned ratified non-blocking exception (single-suite local-collection artifact; see Section 8) |
| TypeScript | N/A - out of scope | N/A - out of scope | N/A - out of scope | N/A - out of scope | N/A - out of scope (0 changed files on this branch) |
| Python | N/A - out of scope | N/A - out of scope | N/A - out of scope | N/A - out of scope | N/A - out of scope (0 changed files on this branch) |
| PowerShell | N/A - out of scope | N/A - out of scope | N/A - out of scope | N/A - out of scope | N/A - out of scope (0 changed files on this branch) |

Independent re-derivation (Python `xml.etree.ElementTree` against `artifacts/csharp/coverage.xml`):
report-level raw six-package aggregate LINE 9024/55511 = 16.26%, BRANCH 1869/13736 = 13.61% (matches
the executor's evidence exactly); `QuickFiler` package LINE 5696/7727 = 73.72%, BRANCH 1013/1566 =
64.69% (matches exactly); `QfcItemController.FolderHandling.cs` class-level entry (matched by
`sourcefilename`) `<counter type="LINE" missed="3" covered="71" />` = 95.95%, `<counter
type="BRANCH" missed="10" covered="32" />` = 76.19% (matches exactly).

### 1.2.1 Per-Language Coverage Comparison

- C#: Baseline: 91.89% lines -> Post-change: 95.95% lines. Change: +4.06% lines. New/changed-code coverage: 100%. Disposition: PASS. Evidence: `evidence/qa-gates/remediation-coverage-delta.2026-07-20T18-44.md`, `evidence/qa-gates/remediation-vstest-coverage-final.2026-07-20T18-40.md` (touched class `QfcItemController.FolderHandling.cs`; branch coverage 73.81% -> 76.19%, +2.38%, also clearing the 75% floor).
- C# package-wide (`QuickFiler` package, whole assembly, supplementary detail): Baseline: 73.67% line
  / 64.53% branch (pre-fix). Post-change: 73.72% line / 64.69% branch. Change: +0.05% line, +0.16%
  branch (no regression; small net improvement from the one new test). New/changed-code coverage:
  N/A - no new file added to this package. Disposition: FAIL floor, dispositioned as ratified
  non-blocking pre-existing exception (Section 8). Evidence:
  `evidence/qa-gates/remediation-vstest-coverage-final.2026-07-20T18-40.md`,
  `evidence/qa-gates/coverage-disposition-decision.2026-07-20T18-17.md`,
  `artifacts/orchestration/orchestrator-state.json` `human_interaction.requirements[0]`.
- C# repo-wide (canonical artifact, raw six-package aggregate, supplementary detail): Baseline:
  16.25% line / 13.60% branch. Post-change: 16.26% line / 13.61% branch. Change: +0.01% line, +0.01%
  branch (no regression). New/changed-code coverage: N/A - no new file added. Disposition: FAIL
  floor, dispositioned as ratified non-blocking pre-existing exception; the raw aggregate is a
  single-suite (`QuickFiler.Test` only) local-collection artifact, not the true PR-CI repo-wide
  figure (Section 8). Evidence:
  `evidence/qa-gates/remediation-coverage-conversion.2026-07-20T18-42.md`.
- TypeScript: Baseline: N/A. Post-change: N/A. Change: N/A. New/changed-code coverage: N/A.
  Disposition: N/A. Evidence: N/A — no TypeScript files changed on this branch
  (`git diff --numstat bd435724..8a1b7b98` shows 0 `.ts`/`.tsx` files).
- Python: Baseline: N/A. Post-change: N/A. Change: N/A. New/changed-code coverage: N/A. Disposition:
  N/A. Evidence: N/A — no Python files changed on this branch.
- PowerShell: Baseline: N/A. Post-change: N/A. Change: N/A. New/changed-code coverage: N/A.
  Disposition: N/A. Evidence: N/A — no PowerShell files changed on this branch.

### 1.2.2 (comparison-bullet scan terminator)

(Section boundary marker; no further per-language comparison bullets follow this heading.)

### Coverage Evidence Checklist

- C# baseline coverage artifact: `evidence/remediation-baseline/coverage-baseline.2026-07-20T18-15.md`
  (this cycle's entry baseline) and cycle 1's `evidence/qa-gates/coverage-conversion-392.2026-07-20T14-50.md`
  (original fix baseline).
- C# post-change coverage artifact: `artifacts/csharp/coverage.xml` (regenerated this cycle) and
  `evidence/qa-gates/remediation-coverage-conversion.2026-07-20T18-42.md`.
- TypeScript baseline coverage artifact: N/A - out of scope (0 `.ts`/`.tsx` files changed on this branch).
- TypeScript post-change coverage artifact: N/A - out of scope (0 `.ts`/`.tsx` files changed on this branch).
- PowerShell baseline coverage artifact: N/A - out of scope (0 `.ps1`/`.psm1` files changed on this branch).
- PowerShell post-change coverage artifact: N/A - out of scope (0 `.ps1`/`.psm1` files changed on this branch).
- Per-language comparison summary: C# is the only changed language on this branch; its coverage
  comparison is recorded in full above (Section 5, `### 1.2.1`). TypeScript, Python, and PowerShell
  have zero changed files on this branch (confirmed via `git diff --numstat bd435724..8a1b7b98`,
  which enumerates exactly 2 `.cs` files and 53 `.md` files); their rows are recorded `N/A - out of
  scope` rather than omitted.

## 6. Test Execution Metrics

| Metric | Value | Evidence |
|---|---|---|
| Total tests executed | 542 | `evidence/qa-gates/remediation-vstest-coverage-final.2026-07-20T18-40.md` |
| Passed | 542 | Same |
| Failed | 0 | Same |
| Skipped | 0 | Same |
| Test-count delta vs. prior cycle | +1 (541 -> 542; the one new regression test from R1) | Same |
| Total execution time | 7.7651 seconds | Same |
| No-regression check | PASS — every one of the 541 original-cycle test names is present in this cycle's 542-name passed set (`comm -23` set-difference produced zero output) | `evidence/qa-gates/remediation-regression-check.2026-07-20T18-46.md` |
| Fail-before / pass-after (original fix, unaffected this cycle) | PASS | `evidence/regression-testing/fail-before-392.2026-07-20T14-05.md` (EXIT_CODE 1), `evidence/regression-testing/pass-after-392.2026-07-20T14-10.md` (EXIT_CODE 0) |
| New-test targeted run (R1) | PASS (EXIT_CODE 0) | `evidence/regression-testing/new-branch-test-pass.2026-07-20T18-25.md` |

## 7. Code Quality Checks

| Check | Result | Evidence |
|---|---|---|
| Formatting scan | 0 errors (2 Scope-Lock files clean) | `evidence/qa-gates/remediation-csharpier-final.2026-07-20T18-30.md`; independently reproduced on this review host |
| Analyzer scan | 0 errors, 0 new warnings attributable to Scope-Lock files | `evidence/qa-gates/remediation-analyzer-final.2026-07-20T18-32.md` |
| Nullable scan | 0 new, 0 first-party errors (34 pre-existing vendored errors, dispositioned) | `evidence/qa-gates/remediation-nullable-final.2026-07-20T18-35.md` |
| File-size scan | 0 files over the 500-line limit | Section 2 |
| Duplication scan | 1 low-severity, non-blocking finding carried forward (CR-1, fallback-clamp duplication) | `code-review.2026-07-20T19-30.md` Findings Table |
| Workflow change scan | 0 files matching `.github/workflows/**`, `scripts/benchmarks/**`, `.github/actions/**` | `git diff --name-only bd435724..8a1b7b98` |
| Evidence location scan | 0 files under `artifacts/baselines/`, `artifacts/qa/`, `artifacts/evidence/`, `artifacts/coverage/` | `git diff --name-only bd435724..8a1b7b98` |

## 8. Gaps and Exceptions

**Ratified exception — `QuickFiler` package-wide and canonical repo-wide C# coverage.** Both remain
below the 85%/75% floor (Section 5). This is dispositioned as a ratified, documented pre-existing
exception, independently confirmed via:

- `artifacts/orchestration/orchestrator-state.json` `human_interaction.requirements[0]`: `id:
  quickfiler-package-coverage-disposition`, `response: scope_change`, `resolved_at:
  2026-07-20T19:20:00Z`, citing GitHub issue #136 (*quickfiler-80-per-file-coverage*), the `#328`
  `StoreWrapper` precedent, and CLAUDE.md's COM/VSTO/WinForms testable-denominator exemption. The
  block satisfies the shape required by `.claude/rules/orchestrator-state.md` (non-empty
  `requirements` list; `response` in the valid enum `{scope_change, exception, halt}`).
- `evidence/qa-gates/coverage-disposition-decision.2026-07-20T18-17.md` — the feature-folder evidence
  mirror of the same disposition, with the same citations.
- No regression: baseline `QuickFiler` package figure (73.67%/64.53%, pre-fix) is virtually identical
  to this cycle's figure (73.72%/64.69%) — a small net improvement, not a worsening.
- No production source file is excluded from coverage measurement to produce this figure; no
  `coverage.config` or `.csproj` coverage-exclude was added or modified.

This mirrors the `#328` `StoreWrapper` branch-floor exception precedent exactly: a pre-existing,
broad, unrelated-to-the-immediate-fix coverage shortfall is ratified rather than requiring the
minor-audit fix's Scope-Lock to expand into unrelated WinForms/UI code. The true all-first-party
repo-wide figure is measured by the PR CI full-suite run, per the disposition's citation.

**Carried-forward, non-blocking finding — fallback-clamp duplication (CR-1).** Unchanged from cycle
1; see `code-review.2026-07-20T19-30.md`.

**Carried-forward, informational note — `PopulateAndSelectFolder` empty-array gap (CR-3).**
Unchanged from cycle 1, unaffected by this cycle (no production code modified); see
`code-review.2026-07-20T19-30.md`.

**Evidence-quality note — csharpier baseline-resolution attribution.** See Section 3. Non-blocking.

**Incidental scope note — agent-memory files in the diff.** `git diff --name-status` shows
`.claude/agent-memory/feature-review/MEMORY.md` (modified) and
`.claude/agent-memory/feature-review/project_rescoping-to-instrumented-package-does-not-always-clear-floor.md`
(added) inside this branch's diff — version-controlled reviewer-memory artifacts written by the
cycle-1 `feature-review` pass and swept into the `8a1b7b98` remediation commit alongside the intended
remediation changes. Markdown, non-production, exempt from the 500-line and coverage rules; no policy
or source file was affected. Not a finding.

## 9. Summary of Changes

- Range audited: `bd43572498474be89d80e1f9620dffb132ade377..8a1b7b98b7d12dac69fd1bee5d5f109d4095c3c6`
  (two commits: `8f34f8ef` fix, `8a1b7b98` remediation).
- Merge-base recomputed independently: `git merge-base HEAD origin/main` =
  `bd43572498474be89d80e1f9620dffb132ade377` (matches the supplied base; unchanged from cycle 1).
- Changed languages with files in the branch diff: C# only (2 `.cs` files: 1 production, 1 test;
  unchanged file count from cycle 1). 53 Markdown files (plan, issue, evidence, and two committed
  agent-memory files) were also added/modified.
- `git status` shows a clean working tree at HEAD; the code under test is unchanged since the
  remediation evidence was produced.
- **PR-context summary correction (recurring C#-as-docs misclassification, again):** the refreshed
  `artifacts/pr_context.summary.txt` again reported `Core logic changes: 0 files`, repeating the same
  generator misclassification found and corrected in cycle 1 (`pr_context` artifacts do not
  auto-update after a remediation commit and were freshly regenerated for this cycle, reproducing the
  same defect rather than inheriting the prior correction). `git diff --numstat` confirms 2 changed
  `.cs` files, unchanged in count from cycle 1. The summary was corrected in place (annotated
  `CORRECTED BY feature-review 2026-07-20 (R4 re-audit)`) with space-free `.cs` paths in the
  generator's `(+N/-N)` format so downstream language-detection recognizes C# as a changed language.
- Production code: byte-identical to cycle 1 (verified via `git diff bd435724..8a1b7b98 --
  QuickFiler/Controllers/QfcItemController.FolderHandling.cs`). Test code: one new test method added
  (`PopulateFolderComboBox_WhenInvokeRequired_MarshalsAssignFolderComboBoxViaInvoke`), plus 26
  comment-only line removals from 12 pre-existing tests (disclosed, diff-verified).

## 10. Compliance Verdict

**Overall verdict: PASS (go). Blocking findings: 0.**

- General Unit Test Policy Compliance: PASS.
- General Code Change Policy Compliance: PASS.
- Language-Specific (C#) Code Change Policy Compliance: PASS (nullable dispositioned per amended
  AC-5 scope note).
- Language-Specific (C#) Unit Test Policy Compliance: PASS.
- Test Coverage Detail: PASS at the touched-class scope (95.95% line / 76.19% branch, both clear
  floor); FAIL at the `QuickFiler` package-wide and canonical repo-wide scopes, dispositioned as a
  ratified, non-blocking, pre-existing exception (Section 8) consistent with the `#328` precedent.
- Test Execution Metrics: PASS (542/542, zero regressions).
- Code Quality Checks: PASS (one carried-forward, non-blocking low-severity finding).
- Gaps and Exceptions: one ratified exception (ledgered), two carried-forward non-blocking findings,
  one evidence-quality note, one incidental scope note — none blocking.

**Remediation trigger: not triggered.** No new `remediation-inputs.<timestamp>.md` is produced for
this cycle; zero blocking findings remain.

## Rejected Scope Narrowing

The coordinator's re-audit delegation explicitly stated "execute the full `feature-review-workflow`
SKILL contract end-to-end again, same inputs as your original review, no scope narrowing" and did not
attempt to narrow scope to any plan, task, phase, or file subset. No caller instruction marked any
language's coverage as "plan scope only," "out of scope" (as a narrowing instruction — the `N/A - out
of scope` rows above reflect a factual absence of changed files for those languages, not an
instructed narrowing), "informational only," or equivalent. No scope-narrowing instruction was
detected or rejected in this review cycle.

## Appendix A: Test Inventory

Tests directly relevant to this fix and this remediation cycle, in
`QuickFiler.Test/Controllers/QfcItemController.FolderHandlingTests.cs`:

| Test | Status this cycle | Purpose |
|---|---|---|
| `PopulateAndSelectFolder_SingleItemNoPredeterminedMatch_SelectsIndexZeroWithoutThrowing` | Pre-existing (cycle 1), re-verified | AC-1/AC-4: single-suggestion fallback selects index 0 without throwing. |
| `AssignFolderComboBox_WhenSingleSuggestionNoPredeterminedMatch_SelectsIndexZero` | Pre-existing (cycle 1), re-verified | AC-1/AC-2: single-suggestion fallback selects index 0 via the viewer seam. |
| `PopulateAndSelectFolder_ExactMatchAtIndexZero` | Pre-existing, re-verified | AC-3: predetermined-folder match is preselected. |
| `PopulateAndSelectFolder_AllMissingPredetermined_SelectsIndexOne` | Pre-existing, re-verified | AC-3: multi-suggestion, no predetermined match, index 1 fallback preserved. |
| `PopulateAndSelectFolder_EmptyArray_ThrowsOnIndexOneSelection` | Pre-existing, re-verified | Documents the pre-existing, unrelated empty-array throw in the unused static helper (CR-3). |
| `AssignFolderComboBox_WhenNoPredeterminedFolder_SelectsTopSuggestionViaViewer` | Pre-existing, re-verified | AC-3: multi-suggestion fallback via the viewer seam. |
| `AssignFolderComboBox_WhenPredeterminedFolderPresent_SelectsPreDeterminedFolder` | Pre-existing, re-verified | AC-3: predetermined-folder match via the viewer seam. |
| `AssignFolderComboBox_WhenFolderHandlerNull_NoOps` | Pre-existing, re-verified | Guard-clause behavior when `_folderHandler` is unset. |
| `PopulateFolderComboBox_WhenFactorySucceeds_LoadsHandlerAndAssignsComboFromViewer` | Pre-existing, re-verified | Covers the `InvokeRequired == false` / `else` branch of `PopulateFolderComboBox`. |
| `PopulateFolderComboBox_WhenInvokeRequired_MarshalsAssignFolderComboBoxViaInvoke` | **New this cycle (R1)** | Closes the `InvokeRequired == true` branch-coverage gap identified in cycle 1's audit; raises class-level branch coverage 73.81% -> 76.19%. |

Full suite: 542 tests total (541 pre-existing across the `QuickFiler.Test` project + 1 new this
cycle), 542 passed, 0 failed (`evidence/qa-gates/remediation-vstest-coverage-final.2026-07-20T18-40.md`).

## Appendix B: Toolchain Commands Reference

- `git merge-base HEAD origin/main` — recomputed base SHA `bd43572498474be89d80e1f9620dffb132ade377`.
- `git diff --numstat bd435724..8a1b7b98` — enumerated the full branch diff (2 `.cs`, 53 `.md`).
- `git diff bd435724..8a1b7b98 -- <path>` / `git diff 8f34f8ef..8a1b7b98 -- <path>` — read the
  cumulative and remediation-only diffs.
- `wc -l <file>` — file-size checks at HEAD.
- `csharpier check .` (v1.3.0) — independently reproduced 0 errors on this review host.
- `git merge-base --is-ancestor 78e847ec bd435724` — confirmed the cited commit predates the
  merge-base (Section 3 evidence-quality note).
- Python `xml.etree.ElementTree` parse of `artifacts/csharp/coverage.xml` — independent re-derivation
  of report-level, `QuickFiler`-package-level, and `QfcItemController.FolderHandling.cs`
  class-level LINE/BRANCH counters (Section 5).
- `python3 -c "import json; ..."` against `artifacts/orchestration/orchestrator-state.json` —
  confirmed the `human_interaction` `scope_change` ratification record (Section 8).
- `MSBuild.exe TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU"
  /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true /m` — analyzer gate (executor evidence).
- `MSBuild.exe TaskMaster.sln /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU"
  /p:Nullable=enable /p:TreatWarningsAsErrors=true` — nullable gate (executor evidence).
- `dotnet-coverage collect -f cobertura -s coverage-exclude-deedle.xml -o
  remediation-final-coverage.cobertura.xml -- vstest.console.exe
  QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /InIsolation` — test + coverage collection (executor
  evidence).
- Executor toolchain evidence (not re-run except csharpier, above): `remediation-csharpier-final`,
  `remediation-analyzer-final`, `remediation-nullable-final`, `remediation-vstest-coverage-final`,
  `remediation-coverage-conversion`, `remediation-coverage-delta`, `remediation-regression-check`
  under `evidence/qa-gates/`.
