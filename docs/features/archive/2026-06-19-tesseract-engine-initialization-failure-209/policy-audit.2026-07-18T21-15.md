# Policy Audit — tesseract-engine-initialization-failure (Issue #209) — R4 Re-Audit (remediation_pass 1)

- Feature folder: `docs/features/active/2026-06-19-tesseract-engine-initialization-failure-209`
- Branch: `bug/tesseract-engine-initialization-failure-209`
- Resolved base: `main` @ `a4977216467c6a275648e6ce134adf847693fc6a` (merge-base recomputed independently via `git merge-base HEAD origin/main`; identical to the R1 review's resolved base — zero drift, branch not behind main)
- Head: `1c8daf4f4140917ee47047f07f96a116880089ed` (two remediation commits ahead of the R1 review's head `376f9b0d`: `727ec8f5`, `1c8daf4f`)
- Work Mode: `minor-audit` (per `issue.md` marker) — AC source is `issue.md` `## Acceptance Criteria` (AC1–AC5) only; no `spec.md`/`user-story.md` present.
- Timestamp: 2026-07-18T21-15
- Prior cycle: `policy-audit.2026-07-18T17-42.md` found exactly one Blocking finding (0% line coverage, `TesseractOcrTextExtractor.cs`), recorded in `remediation-inputs.2026-07-18T17-42.md`. Remediation plan `remediation-plan.2026-07-18T17-42.md` (all P0–P2 tasks checked) directed Option A (extract `ResolveTessdataPath()` as a directly-testable static helper).

## Executive Summary

This R4 re-audit reviews the **full branch diff against the resolved base**, not only the remediation commits, per the Scope Invariant. The remediation cycle (commits `727ec8f5`, `1c8daf4f`) extracted `internal static string ResolveTessdataPath()` out of `TesseractOcrTextExtractor.ExtractText`, added `UtilitiesCS.Test/EmailIntelligence/EmailParsingSorting/TesseractOcrTextExtractor_Tests.cs` covering it, wired the new test file into `UtilitiesCS.Test.csproj`, and re-ran the full toolchain (CSharpier, analyzer build, nullable build, full 8-assembly MSTest+coverage). All four gates pass with `EXIT_CODE 0` and zero regressions (5701 -> 5702 tests, +1 exactly matching the new test, 0 failures both before and after).

**The remediation raised `TesseractOcrTextExtractor.cs` line coverage from 0% to 7.6923% (1/13 lines) — an improvement, but the file remains far below both applicable new-code coverage floors (85% general-unit-test.md / quality-tiers.md uniform floor; 90% CLAUDE.md new-module floor).** No formal, maintainer-ratified coverage exemption (`[ExcludeFromCodeCoverage]` attribute or `coverage.config` exclude) was added for the residual, native-engine-bound lines. Per the mandatory coverage-verification procedure (new file below 90% -> FAIL, regardless of exemption category applicability), this remains a **Blocking** finding requiring an explicit disposition — either a maintainer-ratified exemption or an accepted-residual decision — before this can be closed as fully policy-compliant. This is a different, narrower finding than R1's: R1 found 0% coverage with no attempt to extract testable logic; R4 finds a partial, good-faith remediation whose ceiling is architecturally constrained by the class's own purpose (the innermost adapter wrapping a native, unmockable Tesseract engine call). See `## 3.2` and `remediation-inputs.2026-07-18T21-15.md`.

All five acceptance criteria (AC1–AC5) remain satisfied; the remediation cycle did not re-litigate them and this audit independently re-confirms all five are still true at the new HEAD.

## Rejected Scope Narrowing

No narrowing of the audit scope was attempted by the delegating prompt for this review. The prompt explicitly instructs: "Do not narrow scope to only the remediated file — review the full branch diff against the resolved base/merge-base as your standard procedure requires." No caller text matching a narrowing pattern (plan/task/phase-scoped narrowing, "out of scope," "informational only," skip-a-toolchain instructions, etc.) was present in this cycle's delegation input. This section is included for completeness; nothing was rejected.

## PR Context Artifact Refresh

`artifacts/pr_context.summary.txt` and `artifacts/pr_context.appendix.txt` were stale (recorded Head `376f9b0d`, the R1 review's pre-remediation commit) relative to this cycle's actual HEAD (`1c8daf4f`, two commits ahead). Both artifacts were regenerated in this cycle from `git diff a4977216467c6a275648e6ce134adf847693fc6a..HEAD --numstat` (bullet format `- <path> (+N/-N)`, matching the format consumed by `Get-ChangedLanguageSet` in `.claude/hooks/validate-feature-review-coverage.ps1`) and a full unified diff (excluding the four large Cobertura evidence XML files, cited by path instead of inlined). Refreshed files: `artifacts/pr_context.summary.txt`, `artifacts/pr_context.appendix.txt`.

## Changed-Language Inventory (full branch diff, base..HEAD, this R4 cycle)

Only **C#** (`.cs`/`.csproj`) files are changed on this branch, across both the original cycle and the remediation cycle:
- `UtilitiesCS/EmailIntelligence/EmailParsingSorting/ImageStripper.cs` (modified, +10/-34)
- `UtilitiesCS/EmailIntelligence/EmailParsingSorting/TesseractOcrTextExtractor.cs` (new in R1, +53/-0 net vs. base; modified again in the remediation cycle to extract `ResolveTessdataPath()`)
- `UtilitiesCS.Test/EmailIntelligence/ImageStripper_Tests.cs` (modified, +6/-2)
- `UtilitiesCS.Test/EmailIntelligence/EmailParsingSorting/TesseractOcrTextExtractor_Tests.cs` (new, added in the remediation cycle, +30/-0)
- `UtilitiesCS/UtilitiesCS.csproj` (modified, +1/-0; one `<Compile Include>` wiring line)
- `UtilitiesCS.Test/UtilitiesCS.Test.csproj` (modified, +1/-0; one `<Compile Include>` wiring line, added in the remediation cycle)

No TypeScript, Python, or PowerShell files are present in the branch diff (confirmed via `git diff --numstat` against the full branch, not just the remediation commits). A coverage verdict below is therefore required for the C# language only; the other three languages carry no obligation because the branch touches none of their files.

All other changed paths (`.claude/agent-memory/**`, `docs/features/active/.../evidence/**`, `docs/features/active/.../{issue,plan,policy-audit,code-review,feature-audit,remediation-inputs,remediation-plan}.*.md`) are documentation, plan, evidence, or agent-memory files — not source, test, or build-configuration files subject to the code-change/unit-test policies.

## 1. General Code Change Policy Compliance

| Check | Verdict | Evidence |
|---|---|---|
| Simplicity / minimal diff | PASS | Remediation cycle is a small, targeted addition: one new `internal static` method (2 lines) inside the existing file, one new 30-line test file, two `<Compile Include>` wiring lines. No unrelated refactors. |
| Separation of concerns (I/O isolated) | PASS | `ResolveTessdataPath()` is now a pure, side-effect-free string-formatting function, separated from the native-engine I/O in `ExtractText`. |
| Public API compatibility | PASS | `ResolveTessdataPath()` is `internal`, not part of any public surface; no public API changed since R1. |
| File size limit (500 lines) | PASS | `TesseractOcrTextExtractor.cs` 53 lines; `TesseractOcrTextExtractor_Tests.cs` 30 lines; `ImageStripper.cs` 359 lines (unchanged from R1); `ImageStripper_Tests.cs` 439 lines (baseline 435). All well under 500. |
| Error handling / fail-fast | PASS | No new catch blocks; `ResolveTessdataPath()` cannot throw under normal conditions (string interpolation only). |
| Naming | PASS | `ResolveTessdataPath` (PascalCase method name) and `ResolveTessdataPath_ReturnsLocalAppDataTaskMasterTessdataPath` (descriptive test name) follow repo conventions. |
| Dependencies | PASS | No new dependency introduced. |
| I/O boundary isolation | PASS | The remaining I/O (native `TesseractEngine` construction) is unchanged and still confined to `ExtractText`. |

## 2. General Unit Test Policy Compliance

| Check | Verdict | Evidence |
|---|---|---|
| Independence / isolation / determinism | PASS | `ResolveTessdataPath_ReturnsLocalAppDataTaskMasterTessdataPath` depends only on `Environment.GetFolderPath`/`Path.DirectorySeparatorChar`, both deterministic on a given machine and identical between Arrange and Act. |
| No external dependencies in tests | PASS | No live engine, no filesystem I/O, no temp files. |
| Arrange-Act-Assert | PASS | Test follows explicit `// Arrange` / `// Act` / `// Assert` comments. |
| Temporary files in tests | PASS | None used. |
| Test file location | PASS | `UtilitiesCS.Test/EmailIntelligence/EmailParsingSorting/TesseractOcrTextExtractor_Tests.cs` mirrors `UtilitiesCS/EmailIntelligence/EmailParsingSorting/TesseractOcrTextExtractor.cs`. |
| Coverage — new code (`TesseractOcrTextExtractor.cs`) | **FAIL (Blocking, residual)** | See `## 3.2`. Improved 0% -> 7.6923% but remains below both the 85%/90% new-code floors; no ratified exemption present. |
| Coverage — modified files, no regression on changed lines | PASS | See `## 3.3`; `ImageStripper.cs` changed/added lines remain 100%-covered; class-level line/branch coverage both improved slightly (84.4156% -> 84.6154% line; 78.6885% -> 79.3651% branch). |
| Repo-wide coverage floor | **FAIL** against the stricter repo-canonical 85% line floor (pre-existing gap, marginally narrower than at R1); **PASS** against the CLAUDE.md 80% floor and against the 75% branch floor under both readings. |

### Known, pre-existing threshold conflict (not introduced by this change, carried forward from R1)

`CLAUDE.md`'s C# Unit Test Policy states repo-wide line coverage must remain `>= 80%` and new modules must reach `>= 90%`. `.claude/rules/general-unit-test.md` and `.claude/rules/quality-tiers.md` state a uniform `>= 85%` line / `>= 75%` branch floor with "tier-specific lower coverage thresholds are not used." This audit again applies the stricter combined bar (85% line / 75% branch, plus 90% for genuinely new modules) as the safer default, consistent with `.claude/agent-memory/atomic-planner/project_coverage_threshold_conflict_claude_md_vs_general_unit_test.md`. Under both readings, the new file's 7.6923% is a FAIL (7.69% is below 80% too, so this is not a marginal/threshold-dependent case).

## 3. Coverage Verification (C# — the only changed language)

**Canonical-artifact note (unchanged from R1):** `artifacts/csharp/coverage.xml` exists but is dated 2026-06-02 (predates this branch's merge-base by weeks), is Cobertura-format (root element `<coverage line-rate="0.5798...">`, zero `<counter type="LINE">` elements — confirmed by direct inspection), and reflects an unrelated prior feature's diff; it was not regenerated for this branch. Per the Evidence Location Invariant, the authoritative coverage evidence for this branch is the feature-local Cobertura evidence produced by this feature's own plan/remediation-plan tasks:
- R1 baseline: `docs/features/active/2026-06-19-tesseract-engine-initialization-failure-209/evidence/baseline/coverage-baseline.cobertura.xml` (2026-07-18T17-10, pre-fix)
- R1 final: `docs/features/active/2026-06-19-tesseract-engine-initialization-failure-209/evidence/qa-gates/coverage-final.cobertura.xml` (2026-07-18T17-24, post-R1-fix, pre-remediation)
- Remediation baseline: `docs/features/active/2026-06-19-tesseract-engine-initialization-failure-209/evidence/remediation-baseline/remediation1-coverage-baseline.cobertura.xml` (2026-07-18T18-11)
- Remediation final (authoritative for this R4 audit): `docs/features/active/2026-06-19-tesseract-engine-initialization-failure-209/evidence/qa-gates/remediation1-coverage-final.cobertura.xml` (2026-07-18T20-29, post-remediation)

All were independently re-parsed by this review (not re-generated) by reading the root `<coverage>` element and the per-class `<class>` blocks for the changed C# source files.

### 3.1 Repo-wide (all 8 first-party `*.Test.dll` assemblies, `/InIsolation`)

| Metric | Baseline: | Post-change: | Change: | Disposition: |
|---|---|---|---|---|
| Line coverage | 83.7729% (remediation baseline) | 83.7826% (remediation final) | +0.0097 pp | FAIL vs. the 85% uniform floor (pre-existing gap, not a regression — improved slightly, not worsened); PASS vs. the CLAUDE.md 80% floor |
| Branch coverage | 76.3407% | 76.3446% | +0.0039 pp | PASS vs. both the 75% uniform floor and no-regression requirement |

Evidence: `remediation1-coverage-baseline.cobertura.xml` root `<coverage line-rate="0.837729" branch-rate="0.763407">`; `remediation1-coverage-final.cobertura.xml` root `<coverage line-rate="0.837826" branch-rate="0.763446">`. New/changed-code coverage: 7.6923% (see 3.2 below; the residual uncovered native-engine lines are the controlling factor keeping repo-wide line coverage below the stricter 85% floor).

### 3.2 New code — `TesseractOcrTextExtractor.cs` (the residual Blocking item)

| Metric | Baseline (R1 final, pre-remediation): | Post-remediation: | Change: | Disposition: |
|---|---|---|---|---|
| Line coverage (new file) | 0% (0/13 executable lines hit) | 7.6923% (1/13 lines hit) | +7.6923 pp (+1 line) | **FAIL** — still below both the 85% general-unit-test.md floor and the 90% CLAUDE.md new-module floor; no ratified exemption present |
| Branch coverage (new file) | 100% (branch-rate="1", trivially — no branching logic) | 100% (unchanged) | 0 | PASS in isolation, but the line-coverage FAIL is controlling |

Evidence: `remediation1-coverage-final.cobertura.xml`, `<class line-rate="0.07692307692307693" branch-rate="1" name="UtilitiesCS.EmailIntelligence.TesseractOcrTextExtractor" filename="UtilitiesCS\EmailIntelligence\EmailParsingSorting\TesseractOcrTextExtractor.cs">`: method `ResolveTessdataPath` — line 31, `hits="1"` (now covered). Method `ExtractText(Bitmap)` — 12 remaining lines (35, 36, 39–43, 45, 46, 48, 49, 51), all `hits="0"` (native `TesseractEngine` construction, `Process`, `GetText` calls).

**Analysis of the residual (why this is judged Blocking, not accepted-as-is):** The remediation extracted exactly the testable portion the R1 remediation-inputs identified (`ResolveTessdataPath()`), and this review independently confirms no further seam decomposition would materially move the needle: the remaining 12 lines are the literal construction of a third-party native `Tesseract.TesseractEngine`, the call to `engine.Process(bitmap)`, and `page.GetText()` — this class is, by design, the single concrete implementation of `IOcrTextExtractor` that the rest of the codebase already mocks out at the interface boundary (confirmed: `ImageStripper_Tests.cs` uses `Mock<IOcrTextExtractor>`, not this concrete class). Testing these 12 lines directly would require either a live, provisioned `tessdata` directory (the exact external dependency this refactor was designed to eliminate from tests) or a second layer of indirection whose own default implementation would still be equally untestable — moving the problem, not solving it. `.claude/rules/general-unit-test.md`'s Coverage Exclusion Policy explicitly anticipates this pattern ("The correct response to a file that contains untestable lines is to refactor it — extract all logic into host-neutral, testable modules and leave only the thinnest possible wiring in the host-bound entry point. The entry point's uncovered lines then represent a real and visible cost in the coverage metric") — which is a philosophy of **accepting the residual as a visible, ongoing cost**, not one of granting a formal exclusion. That same policy document, however, states unconditionally that "No production file may be excluded from coverage measurement," which conflicts with CLAUDE.md UT2's separate, maintainer-ratifiable exemption mechanism (a mechanism this class does not currently qualify for under UT2's literal enumerated categories — VSTO lifecycle, WinForms Designer, Outlook-Interop event handlers — since this class depends on Tesseract, not Outlook COM/Interop or VSTO).

This audit does not treat the residual as silently acceptable, because doing so would require choosing one side of an unresolved, previously-flagged policy conflict without user/maintainer input (`policy-compliance-order`'s hard constraint to halt and notify rather than silently pick). It also does not treat further code-level remediation as a productive next step, because the residual is architecturally close to irreducible. The disposition this audit recommends is therefore a **maintainer decision** (see `remediation-inputs.2026-07-18T21-15.md`): either ratify a narrow, documented `[ExcludeFromCodeCoverage]` exemption for `TesseractOcrTextExtractor.ExtractText`'s native-engine body per CLAUDE.md UT2's exemption mechanism, or explicitly accept the residual as a documented, permanent cost under general-unit-test.md's Coverage Exclusion Policy philosophy and record that decision so it is not re-flagged as a fresh Blocking finding in a future review cycle.

### 3.3 Modified file — `ImageStripper.cs`

| Metric | Baseline: | Post-change: | Change: | Disposition: |
|---|---|---|---|---|
| Class-level line coverage | 84.4156% (R1 baseline) | 84.6154% (remediation final) | +0.1998 pp | Slightly below the 85% floor at whole-class granularity, but improved (not regressed); reflects pre-existing untouched legacy methods, not new debt |
| Class-level branch coverage | 78.6885% | 79.3651% | +0.6766 pp | PASS vs. 75% floor |
| Changed/added lines specifically (4 constructors + `extract_text` delegation) | n/a (added in R1) | 100% (unchanged from R1; confirmed still `hits="1"` in remediation-final evidence) | n/a | **PASS** — no regression on changed lines |

Evidence: `remediation1-coverage-final.cobertura.xml`, `<class line-rate="0.846154" branch-rate="0.793651" name="UtilitiesCS.EmailIntelligence.ImageStripper">`.

### Coverage Evidence Checklist

- TypeScript baseline coverage artifact: N/A — zero `.ts`/`.tsx` files changed on this branch (confirmed via `git diff --numstat`, full branch, not narrowed to remediation commits).
- TypeScript post-change coverage artifact: N/A — same reason.
- PowerShell baseline coverage artifact: N/A — zero `.ps1`/`.psm1` files changed on this branch.
- PowerShell post-change coverage artifact: N/A — same reason.
- Python baseline/post-change coverage artifact: N/A — zero `.py` files changed on this branch.
- **C# baseline coverage artifact:** `docs/features/active/2026-06-19-tesseract-engine-initialization-failure-209/evidence/remediation-baseline/remediation1-coverage-baseline.cobertura.xml` — PRESENT.
- **C# post-change coverage artifact:** `docs/features/active/2026-06-19-tesseract-engine-initialization-failure-209/evidence/qa-gates/remediation1-coverage-final.cobertura.xml` — PRESENT.
- **C# coverage verdict: FAIL** (new-code floor still violated on `TesseractOcrTextExtractor.cs`, improved from 0% to 7.6923% but not resolved; repo-wide line coverage also below the stricter 85% uniform floor, pre-existing, not a regression, slightly improved). This FAIL is Blocking pending a maintainer disposition per `remediation-inputs.2026-07-18T21-15.md`.

## 4. Language-Specific (C#) Code Change Policy Compliance (`.claude/rules/csharp.md`)

| Check | Verdict | Evidence |
|---|---|---|
| CSharpier formatting | PASS | `remediation1-final-csharpier.2026-07-18T19-06.md`: EXIT_CODE 0, "Checked 2 files" with zero reformats. |
| .NET analyzer build | PASS | `remediation1-final-analyzer-build.2026-07-18T19-07.md`: EXIT_CODE 0, 0 Error(s), 75 Warning(s) — identical pre-existing warning count; no new warning attributable to the touched files. |
| Nullable / type-check build | PASS | `remediation1-final-nullable-build.2026-07-18T19-41.md`: EXIT_CODE 0, 0 Error(s), 0 Warning(s). |
| Analyzer/BannedSymbols wiring for new files | PASS | Both new files (`TesseractOcrTextExtractor.cs`'s new method, `TesseractOcrTextExtractor_Tests.cs`) compile into their existing, already-wired projects; direct inspection shows no banned-symbol usage (`DateTime.Now`, `DateTime.UtcNow`, `Thread.Sleep`, `Task.Delay`, `Random.Shared` — 0 matches). |
| `InternalsVisibleTo` accessibility | PASS | `UtilitiesCS/Properties/AssemblyInfo.cs` line 17: `[assembly: InternalsVisibleTo("UtilitiesCS.Test")]` — confirmed present, permitting `internal static` `ResolveTessdataPath()` to be called directly from the test project with no reflection or visibility workaround. |
| Architecture boundaries (No-COM rules) | PASS | Neither changed file references `Microsoft.Office.Tools.*`, introduces `[ComVisible(true)]`, or adds new Outlook Interop dependencies. |

## 5. Language-Specific (C#) Unit Test Policy Compliance (CUT1–CUT3)

| Check | Verdict | Evidence |
|---|---|---|
| MSTest framework | PASS | `[TestClass]`/`[TestMethod]` used in the new test file. |
| Moq for mocking | PASS (N/A for this specific test) | The new test targets a pure static method with no mockable dependency; Moq usage is unchanged/correct elsewhere in the same feature (`ImageStripper_Tests.cs`). |
| FluentAssertions | PASS | `using FluentAssertions;` present; `actual.Should().Be(expected);` used. |
| Full-suite vstest + coverage | PASS | `scripts/vscode/Invoke-MSTestWithCoverage.ps1` run across all 8 first-party `*.Test.dll` assemblies at both remediation-baseline and remediation-final. |

## 6. Test Execution Metrics

| Metric | Baseline (remediation) | Final (remediation) | Disposition |
|---|---|---|---|
| Total tests | 5701 | 5702 | +1, matches the single new test added |
| Passed | 5701 | 5702 | +1 |
| Failed | 0 | 0 | No change |
| Skipped | 0 | 0 | No change |

Evidence: `evidence/remediation-baseline/remediation1-baseline-coverage-target.2026-07-18T18-11.md`, `evidence/qa-gates/remediation1-final-mstest.2026-07-18T20-29.md`, `evidence/qa-gates/remediation1-final-regression-verification.2026-07-18T20-29.md`. No unexplained test-status change; the +1 delta is fully explained by `ResolveTessdataPath_ReturnsLocalAppDataTaskMasterTessdataPath`.

## 7. Code Quality Checks

| Check | Verdict | Evidence |
|---|---|---|
| Toolchain order followed (format -> lint -> type-check -> test) | PASS | Remediation Phase 2 tasks executed in order; artifacts timestamped ascending (19:06, 19:07, 19:41, 20:29). |
| Restart-on-failure discipline | PASS | `remediation1-final-csharpier.2026-07-18T19-06.md` records zero files reformatted beyond intended edits; no restart required. |
| No temp-file usage | PASS | No temporary files created or referenced. |
| No workflow files touched | PASS | `git diff --name-only` (full branch) shows zero `.github/workflows/**` paths changed. |
| No benchmark baselines touched | PASS | Zero baseline/benchmark files changed. |
| Evidence Location Invariant | PASS | See `## Evidence Location Compliance` below. |

## Evidence Location Compliance

`git diff --name-only a4977216467c6a275648e6ce134adf847693fc6a..HEAD | grep -E "^artifacts/(baselines|qa|evidence|coverage)/"` returns zero matches. All evidence for this feature (both R1 and this remediation cycle) is under `docs/features/active/2026-06-19-tesseract-engine-initialization-failure-209/evidence/{baseline,qa-gates,remediation-baseline}/`, the canonical location. No `validate_evidence_locations.py` script exists in this repository (confirmed via `find . -iname "validate_evidence_locations.py"` — zero results); per prior-session memory this is a cross-repo artifact reference and the manual `git diff --name-only` grep above is the working substitute in TaskMaster. No violations found; no non-canonical path was written to by this cycle's remediation work.

## Acceptance Criteria Cross-Reference (detail in `feature-audit`)

All five AC items in `issue.md` remain pre-checked `[x]`; the remediation cycle did not re-litigate them (per its own plan's scope statement) and this audit independently re-confirms all five are still true at HEAD (`1c8daf4f`) — see `feature-audit.2026-07-18T21-15.md` for the formal per-criterion re-evaluation table.

## Overall Disposition

**PARTIAL** (not a clean PASS). All acceptance criteria remain satisfied and the toolchain gates (format/lint/nullable/test) all pass cleanly with zero regressions across both cycles. The Blocking gap carried into this R4 cycle is narrower than R1's but not fully closed: `TesseractOcrTextExtractor.cs` improved from 0% to 7.6923% line coverage, still below the 85%/90% new-code floor, with the residual now judged architecturally close to irreducible and requiring an explicit maintainer disposition (ratified exemption or documented accepted-residual decision) rather than further code changes. See `remediation-inputs.2026-07-18T21-15.md`.

**Blocking finding count for this pass: 1.**
