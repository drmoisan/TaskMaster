# Policy Audit — tesseract-engine-initialization-failure (Issue #209) — R4 Re-Audit (remediation_pass 2)

- Feature folder: `docs/features/active/2026-06-19-tesseract-engine-initialization-failure-209`
- Branch: `bug/tesseract-engine-initialization-failure-209`
- Resolved base: `main` @ `a4977216467c6a275648e6ce134adf847693fc6a` (recomputed independently via `git merge-base HEAD origin/main`; identical to both prior cycles' resolved base — zero drift, branch not behind main)
- Head: `9ef69247deba0f93d11d801c6a6e9d26da49bd9e` (one docs-only commit ahead of the pass-1 re-audit's head `1c8daf4f`)
- Work Mode: `minor-audit` (per `issue.md` marker) — AC source is `issue.md` `## Acceptance Criteria` (AC1–AC5) only; no `spec.md`/`user-story.md` present.
- Timestamp: 2026-07-18T22-10
- Prior cycles:
  - `policy-audit.2026-07-18T17-42.md` (pass 1 of the original review): one Blocking finding (0% coverage, `TesseractOcrTextExtractor.cs`); remediated by commits `727ec8f5`, `1c8daf4f`.
  - `policy-audit.2026-07-18T21-15.md` (R4 re-audit, remediation_pass 1): remediation raised coverage to 7.6923%, still below the 85%/90% new-code floor with no ratified exemption; recorded as **Blocking**, requiring a maintainer disposition (Option B ratified exemption, or Option C accepted documented residual) per `remediation-inputs.2026-07-18T21-15.md`. No other Blocking or High-severity findings were identified in that cycle.

## Executive Summary

This is remediation_pass 2 of the R4 re-audit. The only change since the previous cycle is commit `9ef69247` ("docs(209): record R4 re-audit findings and maintainer coverage-residual decision"), which is **documentation-only**: it adds a `## Maintainer Decision — Coverage Residual on TesseractOcrTextExtractor.cs (2026-07-18)` section to `issue.md` and a corresponding entry in `.claude/agent-memory/feature-review/`. Confirmed via `git diff --numstat 1c8daf4f..9ef69247`: zero `.cs`/`.csproj` files touched; the entire commit is two documentation/memory files plus the pass-1 re-audit's own already-reviewed artifacts.

The recorded maintainer decision is **Option C: accept the 7.6923% line-coverage figure on `TesseractOcrTextExtractor.cs` as a documented, permanent residual.** No `[ExcludeFromCodeCoverage]` attribute or `coverage.config` exclude was added; the class remains fully in the coverage denominator, and its low percentage continues to be visibly carried in the repo-wide C# figure.

**This audit determines that Option C is a valid, policy-compliant closure of the pass-1 Blocking finding**, for the following reasons:

1. `.claude/rules/general-unit-test.md`'s Coverage Exclusion Policy states unconditionally that "No production file may be excluded from coverage measurement" and separately describes, as its stated design intent, a refactor pattern that "leave[s] only the thinnest possible wiring in the host-bound entry point... a real and visible cost in the coverage metric." Option C satisfies both clauses simultaneously: the file is not excluded from measurement (no attribute, no config exclude — repo-wide coverage still carries its uncovered lines), and the residual is explicitly documented rather than silently tolerated.
2. Option C does not invoke or rely on CLAUDE.md UT2's maintainer-ratified `[ExcludeFromCodeCoverage]` exemption mechanism, so it does not require resolving the previously-flagged CLAUDE.md-vs-general-unit-test.md numeric-floor conflict (`.claude/agent-memory/atomic-planner/project_coverage_threshold_conflict_claude_md_vs_general_unit_test.md`) in either document's favor. That conflict remains open but is no longer load-bearing for this specific finding's disposition.
3. The decision is recorded in a durable, in-repo location (`issue.md`, committed at `9ef69247`) with an explicit forward-looking instruction that a future review must not reopen this class's coverage percentage as a fresh Blocking finding absent an implementation change to `ExtractText` — satisfying the pass-1 remediation-inputs' own closure condition ("recording that decision ... so a future review cycle does not re-open this as a fresh Blocking finding").
4. This review independently re-confirms (see `## 3`) that the underlying architectural analysis from pass 1 still holds: the 12 uncovered lines are the literal native `Tesseract.TesseractEngine` construction/`Process`/`GetText` calls in the sole concrete implementation of an already-elsewhere-mocked `IOcrTextExtractor` seam, and no source change has occurred since that analysis was made.

Consistent with the `coverage-hook-forces-fail-below-floor-despite-exemption` disposition pattern used elsewhere in this repository's review history, the raw numeric coverage verdict for this file is still recorded as **FAIL against the literal 85%/90% floor** — the number itself has not changed and is not being reported as if it had. What has changed is the **disposition**: this FAIL is now **non-blocking**, closed by an explicit, documented, dated maintainer decision, rather than an open item awaiting one.

**All five acceptance criteria (AC1–AC5) remain satisfied**, re-confirmed independently at the new HEAD (no source changed since the last AC re-verification). **No other Blocking or High-severity findings were identified in this pass** — the full branch diff against the resolved base was re-inspected and no new source, test, build-configuration, or workflow file has changed beyond the two documentation/memory files in commit `9ef69247`.

## Rejected Scope Narrowing

No narrowing of the audit scope was attempted by the delegating prompt for this review. The prompt explicitly directs execution of "the full `feature-review-workflow` SKILL contract end-to-end against the current branch head (no scope narrowing)." No caller text matching a narrowing pattern (plan/task/phase-scoped narrowing, "out of scope," "informational only," skip-a-toolchain instructions, language-not-applicable claims, etc.) was present in this cycle's delegation input. This section is included for completeness; nothing was rejected.

## PR Context Artifact Refresh

`artifacts/pr_context.summary.txt` and `artifacts/pr_context.appendix.txt` were stale (recorded Head `1c8daf4f`, the pass-1 re-audit's head) relative to this cycle's actual HEAD (`9ef69247`, one docs-only commit ahead). Both artifacts were refreshed in place: the summary's Base/Head, Remediation Cycle Context, Changed Files, and staleness-note sections were updated in place, and the appendix's header was updated with a pass-2 delta diff (`git diff 1c8daf4f..9ef69247`) appended at the end, confirming the delta touches only documentation/memory files.

## Changed-Language Inventory (full branch diff, base..HEAD, this pass-2 cycle)

Only **C#** (`.cs`/`.csproj`) files are changed on this branch, across all three cycles (original + remediation + this docs-only pass):
- `UtilitiesCS/EmailIntelligence/EmailParsingSorting/ImageStripper.cs` (modified, +10/-34)
- `UtilitiesCS/EmailIntelligence/EmailParsingSorting/TesseractOcrTextExtractor.cs` (new in R1, modified again in the remediation cycle to extract `ResolveTessdataPath()`; unchanged since)
- `UtilitiesCS.Test/EmailIntelligence/ImageStripper_Tests.cs` (modified, +6/-2)
- `UtilitiesCS.Test/EmailIntelligence/EmailParsingSorting/TesseractOcrTextExtractor_Tests.cs` (new, added in the remediation cycle, +30/-0)
- `UtilitiesCS/UtilitiesCS.csproj` (modified, +1/-0)
- `UtilitiesCS.Test/UtilitiesCS.Test.csproj` (modified, +1/-0)

Verified via `git diff --numstat a4977216467c6a275648e6ce134adf847693fc6a..HEAD -- '*.cs' '*.csproj' '*.ps1' '*.py' '*.ts' '*.tsx'`: the six files above are the complete set; identical to the pass-1 re-audit's inventory. No TypeScript, Python, or PowerShell files are present in the branch diff. A coverage verdict below is therefore required for the C# language only.

All other changed paths (`.claude/agent-memory/**`, `docs/features/active/.../evidence/**`, `docs/features/active/.../{issue,plan,policy-audit,code-review,feature-audit,remediation-inputs,remediation-plan}.*.md`) are documentation, plan, evidence, or agent-memory files — not source, test, or build-configuration files subject to the code-change/unit-test policies.

## 1. General Code Change Policy Compliance

No source file changed since the pass-1 re-audit; all findings below are re-confirmations, not new evaluations.

| Check | Verdict | Evidence |
|---|---|---|
| Simplicity / minimal diff | PASS | Unchanged from pass 1; this pass adds no source changes at all. |
| Separation of concerns (I/O isolated) | PASS | `ResolveTessdataPath()` remains a pure, side-effect-free string-formatting function, separated from the native-engine I/O in `ExtractText`. |
| Public API compatibility | PASS | No public API changed since pass 1. |
| File size limit (500 lines) | PASS | Re-verified via `awk 'END{print NR}'`: `TesseractOcrTextExtractor.cs` 53 lines; `TesseractOcrTextExtractor_Tests.cs` 30 lines; `ImageStripper.cs` 359 lines; `ImageStripper_Tests.cs` 439 lines. All well under 500. |
| Error handling / fail-fast | PASS | No new catch blocks; unchanged since pass 1. |
| Naming | PASS | Unchanged since pass 1. |
| Dependencies | PASS | No new dependency introduced. |
| I/O boundary isolation | PASS | Unchanged since pass 1. |

## 2. General Unit Test Policy Compliance

| Check | Verdict | Evidence |
|---|---|---|
| Independence / isolation / determinism | PASS | Unchanged since pass 1. |
| No external dependencies in tests | PASS | Unchanged since pass 1. |
| Arrange-Act-Assert | PASS | Unchanged since pass 1. |
| Temporary files in tests | PASS | None used. |
| Test file location | PASS | Unchanged since pass 1. |
| Coverage — new code (`TesseractOcrTextExtractor.cs`) | **FAIL against the raw 85%/90% floor, dispositioned non-blocking via maintainer-accepted residual (Option C)** | See `## 3.2`. Raw figure unchanged at 7.6923% (1/13 lines); the maintainer's 2026-07-18 Option C decision, recorded in `issue.md`, closes this as a documented, permanent residual rather than an open remediation item. |
| Coverage — modified files, no regression on changed lines | PASS | Unchanged since pass 1; `ImageStripper.cs` changed/added lines remain 100%-covered. |
| Repo-wide coverage floor | **FAIL** against the stricter repo-canonical 85% line floor (pre-existing, unchanged, not a regression); **PASS** against the CLAUDE.md 80% floor and against the 75% branch floor under both readings. |

### Known, pre-existing threshold conflict (unchanged, carried forward)

`CLAUDE.md`'s C# Unit Test Policy (80% repo-wide / 90% new-code) and `.claude/rules/general-unit-test.md`/`quality-tiers.md` (uniform 85% line / 75% branch, no tier floors) remain in unresolved tension, per `.claude/agent-memory/atomic-planner/project_coverage_threshold_conflict_claude_md_vs_general_unit_test.md`. As determined in the Executive Summary, the maintainer's Option C decision for this specific class does not require resolving that conflict, because Option C does not invoke either document's exemption mechanism — it accepts the visible metric cost, which both documents' philosophies permit.

## 3. Coverage Verification (C# — the only changed language)

No coverage evidence was regenerated this pass (no source changed). The authoritative evidence remains the same feature-local Cobertura files independently re-parsed at pass 1:
- Remediation baseline: `docs/features/active/2026-06-19-tesseract-engine-initialization-failure-209/evidence/remediation-baseline/remediation1-coverage-baseline.cobertura.xml`
- Remediation final (authoritative): `docs/features/active/2026-06-19-tesseract-engine-initialization-failure-209/evidence/qa-gates/remediation1-coverage-final.cobertura.xml`

**Canonical-artifact note (unchanged from prior cycles):** `artifacts/csharp/coverage.xml` predates this branch's merge-base, is Cobertura-format with zero `<counter type="LINE">` elements, and reflects an unrelated prior feature's diff. Per the Evidence Location Invariant, the feature-local evidence above is authoritative for this branch.

### 3.1 Repo-wide (all 8 first-party `*.Test.dll` assemblies, `/InIsolation`)

| Metric | Baseline: | Post-change: | Change: | Disposition: |
|---|---|---|---|---|
| Line coverage | 83.7729% | 83.7826% | +0.0097 pp | FAIL vs. the 85% uniform floor (pre-existing gap, unchanged this pass, not a regression); PASS vs. the CLAUDE.md 80% floor |
| Branch coverage | 76.3407% | 76.3446% | +0.0039 pp | PASS vs. both the 75% uniform floor and no-regression requirement |

Evidence: `remediation1-coverage-baseline.cobertura.xml` root `<coverage line-rate="0.837729" branch-rate="0.763407">`; `remediation1-coverage-final.cobertura.xml` root `<coverage line-rate="0.837826" branch-rate="0.763446">`. New/changed-code coverage: 7.6923% (see 3.2). Unchanged from pass 1 — no new source commits since.

### 3.2 New code — `TesseractOcrTextExtractor.cs` (the finding closed this pass)

| Metric | Baseline (pre-remediation): | Post-remediation (current, unchanged since pass 1): | Change: | Disposition: |
|---|---|---|---|---|
| Line coverage (new file) | 0% (0/13 executable lines hit) | 7.6923% (1/13 lines hit) | +7.6923 pp (+1 line) | **FAIL against the raw 85%/90% floor; dispositioned non-blocking via the 2026-07-18 maintainer Option C decision recorded in `issue.md`** |
| Branch coverage (new file) | 100% | 100% (unchanged) | 0 | PASS in isolation |

Evidence: `remediation1-coverage-final.cobertura.xml`, `<class line-rate="0.07692307692307693" branch-rate="1" name="UtilitiesCS.EmailIntelligence.TesseractOcrTextExtractor">`: method `ResolveTessdataPath` line 31 `hits="1"`; method `ExtractText(Bitmap)` remaining 12 lines (35, 36, 39–43, 45, 46, 48, 49, 51) all `hits="0"` — the native `TesseractEngine` construction, `Process`, and `GetText` calls. This review re-confirmed by direct inspection that these lines are unchanged from pass 1 (no source diff exists to re-parse against).

**Closure rationale (why this is no longer Blocking):**

The pass-1 re-audit correctly identified this residual as architecturally close to irreducible — this class is the sole concrete implementation of `IOcrTextExtractor`, and the rest of the codebase already mocks the interface at the correct boundary (`ImageStripper_Tests.cs` uses `Mock<IOcrTextExtractor>`). It recommended a maintainer decision between Option B (ratified `[ExcludeFromCodeCoverage]` exemption) and Option C (documented accepted residual, no attribute). The maintainer has now made that decision — Option C — and recorded it durably in `issue.md` at commit `9ef69247`, with an explicit forward-looking statement that a future review must not reopen this class's coverage percentage absent an `ExtractText` implementation change.

This audit accepts Option C as a valid closure because:
- It does not conflict with general-unit-test.md's "no production file may be excluded from coverage measurement" prohibition — the file remains fully measured; only the *disposition* of its known-low figure has changed from "open, pending decision" to "closed, accepted."
- It matches the Coverage Exclusion Policy's own stated design intent for host/native-bound entry points: a real, ongoing, visible cost in the metric rather than a hidden exclusion.
- It is dated, attributable, and durable (checked into the feature's `issue.md`, not merely asserted in a review comment), satisfying the same evidentiary bar this audit would require of any other policy exception.
- The `policy-compliance-order` hard constraint to halt and notify on unresolved conflicting instructions was already satisfied at pass 1 (the conflict was surfaced explicitly rather than silently resolved); the maintainer's subsequent decision is the appropriate resolution mechanism, not a bypass of it.

Consistent with `.claude/agent-memory/feature-review/coverage-hook-forces-fail-below-floor-despite-exemption.md`'s established disposition pattern, this row is recorded as a raw **FAIL** against the literal percentage, with an explicit **non-blocking** disposition — not as a bare PASS, since the number itself remains below floor and this audit does not want a future reviewer to mistake this for a genuine coverage improvement.

### 3.3 Modified file — `ImageStripper.cs`

| Metric | Baseline: | Post-change: | Change: | Disposition: |
|---|---|---|---|---|
| Class-level line coverage | 84.4156% | 84.6154% | +0.1998 pp | Slightly below 85% floor at whole-class granularity; unchanged this pass; reflects pre-existing untouched legacy methods |
| Class-level branch coverage | 78.6885% | 79.3651% | +0.6766 pp | PASS vs. 75% floor |
| Changed/added lines specifically | n/a | 100% | n/a | **PASS** — no regression on changed lines |

Evidence: `remediation1-coverage-final.cobertura.xml`, `<class line-rate="0.846154" branch-rate="0.793651" name="UtilitiesCS.EmailIntelligence.ImageStripper">`. Unchanged from pass 1.

### Coverage Evidence Checklist

- TypeScript baseline/post-change coverage artifact: N/A — zero `.ts`/`.tsx` files changed on this branch.
- PowerShell baseline/post-change coverage artifact: N/A — zero `.ps1`/`.psm1` files changed on this branch.
- Python baseline/post-change coverage artifact: N/A — zero `.py` files changed on this branch.
- **C# baseline coverage artifact:** `docs/features/active/2026-06-19-tesseract-engine-initialization-failure-209/evidence/remediation-baseline/remediation1-coverage-baseline.cobertura.xml` — PRESENT.
- **C# post-change coverage artifact:** `docs/features/active/2026-06-19-tesseract-engine-initialization-failure-209/evidence/qa-gates/remediation1-coverage-final.cobertura.xml` — PRESENT.
- **C# coverage verdict: PASS (non-blocking disposition).** The single new-code coverage row that failed the raw 85%/90% floor (`TesseractOcrTextExtractor.cs`, 7.6923%) is now closed via a dated, durable maintainer decision (Option C) recorded in `issue.md` at commit `9ef69247`. No other C# coverage row fails in a way requiring remediation: repo-wide line coverage's marginal below-85%-floor gap is pre-existing, unregressed debt (also PASS under the CLAUDE.md 80% floor), and `ImageStripper.cs`'s changed lines are 100% covered with no regression.

## 4. Language-Specific (C#) Code Change Policy Compliance (`.claude/rules/csharp.md`)

No source file changed since pass 1; all gates below are re-confirmations against unchanged evidence.

| Check | Verdict | Evidence |
|---|---|---|
| CSharpier formatting | PASS | `remediation1-final-csharpier.2026-07-18T19-06.md`: EXIT_CODE 0. |
| .NET analyzer build | PASS | `remediation1-final-analyzer-build.2026-07-18T19-07.md`: EXIT_CODE 0, 0 Error(s), 75 Warning(s) — unchanged pre-existing count. |
| Nullable / type-check build | PASS | `remediation1-final-nullable-build.2026-07-18T19-41.md`: EXIT_CODE 0, 0 Error(s), 0 Warning(s). |
| Analyzer/BannedSymbols wiring for new files | PASS | Unchanged since pass 1; no banned-symbol usage. |
| `InternalsVisibleTo` accessibility | PASS | Unchanged; `UtilitiesCS/Properties/AssemblyInfo.cs` line 17. |
| Architecture boundaries (No-COM rules) | PASS | Unchanged; no `Microsoft.Office.Tools.*`/`[ComVisible(true)]`/new Outlook Interop dependencies. |

## 5. Language-Specific (C#) Unit Test Policy Compliance (CUT1–CUT3)

| Check | Verdict | Evidence |
|---|---|---|
| MSTest framework | PASS | Unchanged since pass 1. |
| Moq for mocking | PASS (N/A for the new pure-static-method test) | Unchanged since pass 1. |
| FluentAssertions | PASS | Unchanged since pass 1. |
| Full-suite vstest + coverage | PASS | Unchanged since pass 1; no re-execution needed this pass since no source changed. |

## 6. Test Execution Metrics

No test execution occurred this pass (no source changed); the last verified full-suite run remains authoritative.

| Metric | Baseline (remediation) | Final (remediation) | Disposition |
|---|---|---|---|
| Total tests | 5701 | 5702 | +1, matches the single new test added at the remediation cycle |
| Passed | 5701 | 5702 | +1 |
| Failed | 0 | 0 | No change |
| Skipped | 0 | 0 | No change |

Evidence: `evidence/qa-gates/remediation1-final-mstest.2026-07-18T20-29.md`, `evidence/qa-gates/remediation1-final-regression-verification.2026-07-18T20-29.md`. Unchanged from pass 1.

## 7. Code Quality Checks

| Check | Verdict | Evidence |
|---|---|---|
| Toolchain order followed (format -> lint -> type-check -> test) | PASS | Unchanged since pass 1; no new toolchain run required (no source change). |
| Restart-on-failure discipline | PASS | N/A this pass — no toolchain run triggered. |
| No temp-file usage | PASS | No temporary files created or referenced. |
| No workflow files touched | PASS | `git diff --name-only a4977216467c6a275648e6ce134adf847693fc6a..HEAD \| grep -E "^\.github/workflows/"` re-run this pass — zero matches. |
| No benchmark baselines touched | PASS | Zero baseline/benchmark files changed. |
| Evidence Location Invariant | PASS | See `## Evidence Location Compliance` below. |

## Evidence Location Compliance

`git diff --name-only a4977216467c6a275648e6ce134adf847693fc6a..HEAD | grep -E "^artifacts/(baselines|qa|evidence|coverage)/"` re-run this pass against the full branch diff (base..`9ef69247`) — zero matches. All evidence for this feature (across all three cycles) remains under `docs/features/active/2026-06-19-tesseract-engine-initialization-failure-209/evidence/{baseline,qa-gates,remediation-baseline}/`, the canonical location. No `validate_evidence_locations.py` script exists in this repository (confirmed via `find . -iname "validate_evidence_locations.py"` — zero results, re-run this pass); the manual `git diff --name-only` grep above is the working substitute in TaskMaster. No violations found.

## Acceptance Criteria Cross-Reference (detail in `feature-audit`)

All five AC items in `issue.md` remain checked `[x]`; the docs-only commit `9ef69247` did not modify the AC checkbox lines (confirmed via `git diff 1c8daf4f..9ef69247 -- docs/features/active/2026-06-19-tesseract-engine-initialization-failure-209/issue.md`, which shows only an appended new section after the existing AC list, with lines 78-84 unchanged). See `feature-audit.2026-07-18T22-10.md` for the formal per-criterion re-evaluation table.

## Overall Disposition

**PASS.** This is an upgrade from the pass-1 re-audit's PARTIAL disposition. All acceptance criteria remain satisfied and all toolchain gates (format/lint/nullable/test) pass cleanly with zero regressions, unchanged since pass 1 (no source has changed). The sole outstanding Blocking finding from pass 1 — `TesseractOcrTextExtractor.cs` new-code coverage below the 85%/90% floor with no disposition on record — is now resolved: the maintainer has made and durably recorded the Option C decision (accept as documented residual) in `issue.md` at commit `9ef69247`. No other Blocking or High-severity findings exist in this pass.

**Blocking finding count for this pass: 0.**
