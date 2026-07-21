# Policy Audit — tesseract-engine-initialization-failure (Issue #209)

- Feature folder: `docs/features/active/2026-06-19-tesseract-engine-initialization-failure-209`
- Branch: `bug/tesseract-engine-initialization-failure-209`
- Resolved base: `main` @ `a4977216467c6a275648e6ce134adf847693fc6a` (merge-base recomputed independently via `git merge-base HEAD origin/main`; confirmed identical to the caller-supplied SHA — zero drift)
- Head: `376f9b0d799ef33790f9315f7eaae82858525a05`
- Work Mode: `minor-audit` (per `issue.md` marker) — AC source is `issue.md` `## Acceptance Criteria` (AC1–AC5) only; no `spec.md`/`user-story.md` present (confirmed absent, consistent with minor-audit).
- Timestamp: 2026-07-18T17-42

## Executive Summary

This is a narrowly-scoped `minor-audit` bugfix. It introduces an `IOcrTextExtractor` seam (`UtilitiesCS/EmailIntelligence/EmailParsingSorting/TesseractOcrTextExtractor.cs`), rewires `ImageStripper` to depend on the seam instead of constructing `Tesseract.TesseractEngine` directly, and updates two unit tests to inject a Moq fake instead of exercising a live Tesseract engine. All five acceptance criteria (AC1–AC5) are satisfied by direct code inspection and by the executor's own baseline/final evidence (full 8-assembly, 5701-test MSTest run; CSharpier; analyzer build; nullable build). Toolchain order (format → lint → type-check → test) was followed and evidence exists for every step.

One policy gap was found and is Blocking: the new production file `TesseractOcrTextExtractor.cs` has **0% line coverage** in the final Cobertura evidence, which fails both this repo's uniform 85%/75% new-code floor (`.claude/rules/general-unit-test.md`, `.claude/rules/quality-tiers.md`) and CLAUDE.md's 90% new-module floor. This class does not fall within any of the enumerated COM/VSTO/WinForms/Interop coverage exemptions in CLAUDE.md UT2, so no exemption applies as written. See `## Rejected Scope Narrowing` (none found in the delegating prompt) and the Coverage section below for detail, and `remediation-inputs.2026-07-18T17-42.md` for the remediation trigger.

## Rejected Scope Narrowing

No narrowing of the audit scope was attempted by the delegating prompt for this review. The prompt explicitly states "Scope determination is your responsibility; do not narrow it," and the resolved merge-base (`a4977216467c6a275648e6ce134adf847693fc6a`) was independently re-verified via `git merge-base HEAD origin/main` and found identical to the supplied value (zero drift). No caller text matching a narrowing pattern (plan/task/phase-scoped narrowing, "out of scope," "informational only," skip-a-toolchain instructions, etc.) was present. This section is included for completeness; nothing was rejected.

## PR Context Artifact Refresh

The pre-existing `artifacts/pr_context.summary.txt` and `artifacts/pr_context.appendix.txt` (timestamped 2026-07-18T11:49) described an unrelated branch (`bug/stale-app-config-binding-redirects-354`, merge-base `7b8a2144d...`) — stale relative to this review's branch. Both artifacts were regenerated in this cycle from `git diff a4977216467c6a275648e6ce134adf847693fc6a HEAD --numstat` (bullet format `- <path> (+N/-N)`, matching the format consumed by `Get-ChangedLanguageSet` in `.claude/hooks/validate-feature-review-coverage.ps1`) and a full unified diff (excluding the two large Cobertura evidence XML files, which are cited by path instead of inlined). Refreshed files: `artifacts/pr_context.summary.txt`, `artifacts/pr_context.appendix.txt`.

## Changed-Language Inventory (full branch diff, base..HEAD)

Only **C#** (`.cs`) files are changed on this branch:
- `UtilitiesCS/EmailIntelligence/EmailParsingSorting/ImageStripper.cs` (modified, +10/-34)
- `UtilitiesCS/EmailIntelligence/EmailParsingSorting/TesseractOcrTextExtractor.cs` (new, +51/-0)
- `UtilitiesCS.Test/EmailIntelligence/ImageStripper_Tests.cs` (modified, +6/-2)
- `UtilitiesCS/UtilitiesCS.csproj` (modified, +1/-0; one `<Compile Include>` wiring line, non-executable)

No TypeScript, Python, or PowerShell files are present in the branch diff. A coverage verdict below is therefore required for the C# language only. TypeScript, Python, and PowerShell each have zero changed files on this branch, confirmed via `git diff --numstat`; those languages carry no coverage obligation here because the branch touches none of their files, not because of any scope decision by this review.

All other changed paths (`.claude/agent-memory/**`, `docs/features/active/.../evidence/**`, `docs/features/active/.../issue.md`, `docs/features/active/.../plan.*.md`) are documentation, plan, evidence, or agent-memory files — not source, test, or build-configuration files subject to the code-change/unit-test policies.

## 1. General Code Change Policy Compliance

| Check | Verdict | Evidence |
|---|---|---|
| Simplicity / minimal diff | PASS | Change is a targeted seam extraction: one new 51-line file, a 10/-34 net-negative diff on `ImageStripper.cs` (constructor delegation removed duplicated logic), a 2-line test-arrange change x2. No unrelated refactors. |
| Separation of concerns (I/O isolated) | PASS | `TesseractOcrTextExtractor` isolates the native-engine call behind `IOcrTextExtractor`; `ImageStripper.extract_text` is now pure delegation. |
| Public API compatibility | PASS | Existing `ImageStripper()` and `ImageStripper(string)` constructors are preserved (now chaining) with unchanged signatures and unchanged default behavior (`_ocrTextExtractor ?? new TesseractOcrTextExtractor()`), confirmed by direct code read. Two new constructor overloads are additive only. |
| File size limit (500 lines) | PASS | `ImageStripper.cs` 359 lines, `TesseractOcrTextExtractor.cs` 51 lines, `ImageStripper_Tests.cs` 439 lines (baseline was 435 — test file grew by 4 lines net, well under 500). `UtilitiesCS.csproj` is a legacy MSBuild project file (data, not source/test code) and is exempt from the 500-line production/test limit. |
| Error handling / fail-fast | PASS | No new broad catch blocks introduced; `ExtractText` lets `TesseractException` propagate unchanged (same behavior as the pre-change inline code), consistent with "fail fast and explicitly." |
| Naming | PASS | `IOcrTextExtractor`, `TesseractOcrTextExtractor`, `ExtractText` follow PascalCase-for-types/members convention; `_ocrTextExtractor` follows camelCase-private-field convention. |
| Dependencies | PASS | No new third-party dependency added; `Tesseract` namespace usage is moved, not added. |
| I/O boundary isolation | PASS | The only I/O (native Tesseract engine construction reading `tessdata` from `%LOCALAPPDATA%`) is now confined to `TesseractOcrTextExtractor`, consistent with "Core domain logic must be testable without touching the network or filesystem." |

## 2. General Unit Test Policy Compliance

| Check | Verdict | Evidence |
|---|---|---|
| Independence / isolation / determinism | PASS | Both updated tests now use `Mock<IOcrTextExtractor>` returning a fixed `string.Empty`; no live engine, no filesystem/tessdata dependency, deterministic outcome. |
| No external dependencies in tests | PASS (fixes a prior violation) | Previously `Analyze_WithTesseractAndValidImageAttachment_ReturnsNoTextFoundToken` and `ExtractOcrInfo_WithBitmap_WhenNoTextIsDetected_ReturnsNoTextToken` implicitly depended on a live `TesseractEngine` + on-disk `tessdata` (an external-dependency policy violation per `.claude/rules/general-unit-test.md` UT4/`.claude/rules/csharp.md` Deterministic Test Rules); both are now fully isolated via the injected mock. Confirmed via `grep -n "TesseractEngine" UtilitiesCS.Test/EmailIntelligence/ImageStripper_Tests.cs` → 0 matches. |
| Arrange-Act-Assert | PASS | Both modified tests preserve existing AAA structure; only the Arrange block changed (mock construction replaces live-engine construction). |
| Temporary files in tests | PASS | No temp-file usage introduced or present. |
| Test file location | PASS | `UtilitiesCS.Test/EmailIntelligence/ImageStripper_Tests.cs` mirrors `UtilitiesCS/EmailIntelligence/EmailParsingSorting/ImageStripper.cs` per the existing repo test-tree convention (pre-existing file, not newly located). |
| Coverage — new code | **FAIL (Blocking)** | See `## 3. Coverage Verification` below. `TesseractOcrTextExtractor.cs` (new file) is at 0% line coverage; fails the 85%/90% new-code floor. |
| Coverage — modified files, no regression on changed lines | PASS | See `## 3. Coverage Verification`; the specific changed/added lines in `ImageStripper.cs` (new constructor chain, `extract_text` delegation) are 100%-covered in the final Cobertura evidence. |
| Repo-wide coverage floor | **FAIL** against the stricter, repo-canonical 85% line floor (below by 1.2 points); **PASS** against the CLAUDE.md 80% floor. See conflict note below. |

### Known, pre-existing threshold conflict (not introduced by this change)

`CLAUDE.md`'s C# Unit Test Policy states repo-wide line coverage must remain `>= 80%` and new modules must reach `>= 90%`. `.claude/rules/general-unit-test.md` and `.claude/rules/quality-tiers.md` (also auto-loaded) state a uniform `>= 85%` line / `>= 75%` branch floor across all tiers with "tier-specific lower coverage thresholds are not used." Per `policy-compliance-order`'s hard constraint to halt and notify on conflicting instructions rather than silently picking one, this conflict is flagged explicitly rather than resolved unilaterally. This audit applies the stricter combined bar (85% line / 75% branch uniformly, plus 90% for genuinely new modules) as the safer default, consistent with prior in-repo guidance recorded in `.claude/agent-memory/atomic-planner/project_coverage_threshold_conflict_claude_md_vs_general_unit_test.md` (added on this same branch). Under that stricter bar, repo-wide C# line coverage (83.7806%) is a FAIL; under the CLAUDE.md 80% floor alone it would PASS. Both readings agree the new file's 0% is a FAIL.

## 3. Coverage Verification (C# — the only changed language)

**Canonical-artifact note:** The generic per-language artifact table names `artifacts/csharp/coverage.xml` as the canonical C# coverage artifact. That file exists but is dated 2026-06-02 (predates this branch's `a4977216` merge-base by weeks), is Cobertura-format (not JaCoCo, confirmed by direct inspection — root element is `<coverage line-rate="..." ...>` with zero `<counter type="LINE">` elements), and reflects an unrelated prior feature's diff. It was **not** regenerated for this branch. This repo's Evidence Location Invariant additionally directs coverage artifacts to `<FEATURE>/evidence/<kind>/`, not `artifacts/`. The authoritative, freshly-generated coverage evidence for this branch is therefore the feature-local Cobertura pair produced by the plan's own Phase 0/Phase 2 tasks:
- Baseline: `docs/features/active/2026-06-19-tesseract-engine-initialization-failure-209/evidence/baseline/coverage-baseline.cobertura.xml` (captured 2026-07-18T17-10, pre-fix)
- Final: `docs/features/active/2026-06-19-tesseract-engine-initialization-failure-209/evidence/qa-gates/coverage-final.cobertura.xml` (captured 2026-07-18T17-24, post-fix)

Both were independently re-parsed by this review (not re-generated) by reading the root `<coverage>` element and the per-class `<class>` blocks for the three changed C# source files.

### 3.1 Repo-wide (all 8 first-party `*.Test.dll` assemblies, `/InIsolation`)

| Metric | Baseline: | Post-change: | Change: | Disposition: |
|---|---|---|---|---|
| Line coverage | 83.7981% | 83.7806% | -0.0175 pp | FAIL vs. the 85% uniform floor (pre-existing gap, not a regression introduced by this change — baseline was already below 85%); PASS vs. the CLAUDE.md 80% floor |
| Branch coverage | 76.3370% | 76.3524% | +0.0154 pp | PASS vs. both the 75% uniform floor and no-regression requirement |

Evidence: `docs/features/active/2026-06-19-tesseract-engine-initialization-failure-209/evidence/baseline/coverage-baseline.cobertura.xml` root `<coverage line-rate="0.837981" branch-rate="0.76337">`; `.../evidence/qa-gates/coverage-final.cobertura.xml` root `<coverage line-rate="0.837806" branch-rate="0.763524">`. New/changed-code coverage: 0% (see 3.2 below; drives the tiny net line-coverage decrease).

### 3.2 New code — `TesseractOcrTextExtractor.cs`

| Metric | Baseline: | Post-change: | Change: | Disposition: |
|---|---|---|---|---|
| Line coverage (new file) | N/A (file did not exist) | 0% (0/13 executable lines hit) | n/a — wholly new, wholly uncovered | **FAIL** — below both the 85% general-unit-test.md floor and the 90% CLAUDE.md new-module floor |
| Branch coverage (new file) | N/A | 100% (branch-rate="1", trivially — no branching logic in the method) | n/a | PASS in isolation, but the line-coverage FAIL is controlling |

Evidence: `coverage-final.cobertura.xml`, `<class line-rate="0" branch-rate="1" ... name="UtilitiesCS.EmailIntelligence.TesseractOcrTextExtractor" filename="UtilitiesCS\EmailIntelligence\EmailParsingSorting\TesseractOcrTextExtractor.cs">`, method `ExtractText` — all 13 executable lines (32–49, excluding braces) recorded `hits="0"`.

This is a genuine, unremediated gap, not a documentation oversight: the class is not on CLAUDE.md UT2's enumerated exemption list (VSTO add-in lifecycle classes; WinForms Designer code; Outlook-Interop event handlers in `TaskVisualization`/`QuickFiler`/`TaskMaster`/`ToDoModel`/`Tags` with no injectable seam). `TesseractOcrTextExtractor` depends on the third-party native `Tesseract.TesseractEngine`, not on Outlook COM/Interop types, so it falls outside the letter of that exemption. The general-unit-test.md Coverage Exclusion Policy is directly on point: "The correct response to a file that contains untestable lines is to refactor it — extract all logic into host-neutral, testable modules and leave only the thinnest possible wiring in the host-bound entry point." The class currently keeps the (testable) `tessdataPath` string-formatting logic inline inside the same method as the (genuinely untestable) native engine call, which is avoidable — the issue's own "Proposed Fix / Validation Ideas" section explicitly named "tessdata path resolution as a pure helper" as a unit-coverage area, and that extraction was not done. See `remediation-inputs.2026-07-18T17-42.md`.

### 3.3 Modified file — `ImageStripper.cs`

| Metric | Baseline: | Post-change: | Change: | Disposition: |
|---|---|---|---|---|
| Class-level line coverage | not separately isolated at baseline (class existed; changed lines did not) | 84.6154% (class-wide) | n/a (whole-class figure includes large pre-existing legacy methods unrelated to this diff, e.g. `PIL_decode_parts`) | Slightly below the 85% floor at the whole-class granularity, but this reflects pre-existing untouched code, not new debt |
| Class-level branch coverage | n/a | 79.3651% | n/a | PASS vs. 75% floor |
| Changed/added lines specifically (4 constructors + `extract_text` delegation) | n/a (lines did not exist pre-change in this form) | 100% (`.ctor(string, IOcrTextExtractor)`: lines 29–33 all `hits="1"`; `extract_text`: lines 355–357 all `hits="1"`) | n/a | **PASS** — no regression on changed lines; the specific lines this PR touched/added are fully exercised by the existing test suite plus the two updated tests |

Evidence: `coverage-final.cobertura.xml`, `<class name="UtilitiesCS.EmailIntelligence.ImageStripper" ...>` method blocks for `.ctor(string, IOcrTextExtractor)` and `extract_text(System.Drawing.Bitmap)`.

### Coverage Evidence Checklist

- TypeScript baseline coverage artifact: N/A — zero `.ts`/`.tsx` files changed on this branch (not a narrowing device; confirmed via `git diff --numstat`).
- TypeScript post-change coverage artifact: N/A — same reason.
- PowerShell baseline coverage artifact: N/A — zero `.ps1`/`.psm1` files changed on this branch.
- PowerShell post-change coverage artifact: N/A — same reason.
- Python baseline/post-change coverage artifact: N/A — zero `.py` files changed on this branch.
- **C# baseline coverage artifact:** `docs/features/active/2026-06-19-tesseract-engine-initialization-failure-209/evidence/baseline/coverage-baseline.cobertura.xml` — PRESENT.
- **C# post-change coverage artifact:** `docs/features/active/2026-06-19-tesseract-engine-initialization-failure-209/evidence/qa-gates/coverage-final.cobertura.xml` — PRESENT.
- **C# coverage verdict: FAIL** (new-code floor violated on `TesseractOcrTextExtractor.cs`; repo-wide line coverage also below the stricter 85% uniform floor, though not a regression from baseline). This FAIL is Blocking pending remediation per `remediation-inputs.2026-07-18T17-42.md`.

## 4. Language-Specific (C#) Code Change Policy Compliance (`.claude/rules/csharp.md`)

| Check | Verdict | Evidence |
|---|---|---|
| CSharpier formatting | PASS | Executor evidence: `final-csharpier.2026-07-18T17-16.md`, EXIT_CODE 0, 0 files reformatted beyond intended edits. Independently re-verified by this review: `dotnet tool run csharpier check` on all four changed C# files → `Checked 3 files in 765ms.` with 0 diffs (the `.csproj` is non-`.cs`, correctly skipped by CSharpier). |
| .NET analyzer build | PASS | `final-analyzer-build.2026-07-18T17-17.md`: EXIT_CODE 0, 0 Error(s), 75 Warning(s) — identical warning count to baseline; no new warnings attributable to the touched/new files. |
| Nullable / type-check build | PASS | `final-nullable-build.2026-07-18T17-23.md`: EXIT_CODE 0, 0 Error(s), 0 Warning(s). Evidence artifact additionally documents a supplementary forced-recompile investigation confirming the touched/new files introduce zero nullable diagnostics even under a genuine (non-cached) recompile. |
| Analyzer/BannedSymbols wiring for new file | PASS | `UtilitiesCS.csproj` already wires the five-analyzer stack (Meziantou, SonarAnalyzer.CSharp, Roslynator, AsyncFixer, BannedApiAnalyzers) and `BannedSymbols.txt` at the project level (pre-existing `<Analyzer Include>`/`<AdditionalFiles>` entries); the new file is compiled into the same project (confirmed via the added `<Compile Include>` line) and is therefore automatically covered — no new wiring needed. Direct inspection of `TesseractOcrTextExtractor.cs` shows no banned-symbol usage (`DateTime.Now`, `DateTime.UtcNow`, `Thread.Sleep`, `Task.Delay`, `Random.Shared` — 0 matches). |
| DI seam selection | PASS | Interface seam (`IOcrTextExtractor`) is the correct, preferred choice per `.claude/rules/csharp.md` "DI Seams" ordering (interface seam preferred over delegate/adapter) for this boundary. |
| Architecture boundaries (No-COM rules) | PASS | Neither `TesseractOcrTextExtractor.cs` nor the changed portions of `ImageStripper.cs` reference `Microsoft.Office.Tools.*`, introduce `[ComVisible(true)]`, or add new Outlook Interop dependencies. (The pre-existing `Microsoft.Office.Interop.Outlook` `using` in `ImageStripper.cs` is untouched, unrelated legacy code, not part of this diff's architecture surface.) |

## 5. Language-Specific (C#) Unit Test Policy Compliance (CUT1–CUT3)

| Check | Verdict | Evidence |
|---|---|---|
| MSTest framework | PASS | `[TestClass]`/`[TestMethod]` used throughout `ImageStripper_Tests.cs`, unchanged by this diff. |
| Moq for mocking | PASS | Both modified tests use `Mock<IOcrTextExtractor>` — this is precisely the pattern CUT2 requires and AC2 demands. |
| FluentAssertions | PASS | `using FluentAssertions;` present; both modified tests retain pre-existing `.Should().NotBeNull()` / `.Should().Contain(...)` assertions unchanged. |
| Full-suite vstest + coverage | PASS | `scripts/vscode/Invoke-MSTestWithCoverage.ps1` (repo-canonical wrapper around `dotnet-coverage collect` + `vstest.console.exe /InIsolation`) run across all 8 first-party `*.Test.dll` assemblies at both baseline and final; satisfies CUT3 item 4. |

## 6. Test Execution Metrics

| Metric | Baseline | Final | Disposition |
|---|---|---|---|
| Total tests | 5701 | 5701 | No change |
| Passed | 5701 | 5701 | No change |
| Failed | 0 | 0 | No change |
| Skipped | 0 | 0 | No change |
| `Failed loading language 'eng'` occurrences | 2 | 0 | AC3-relevant: eliminated |
| `Error opening data file ... tessdata` occurrences | 2 | 0 | AC3-relevant: eliminated |

Evidence: `evidence/baseline/baseline-mstest.2026-07-18T17-10.md`, `evidence/qa-gates/final-mstest.2026-07-18T17-24.md`. Both named OCR tests (`Analyze_WithTesseractAndValidImageAttachment_ReturnsNoTextFoundToken`, `ExtractOcrInfo_WithBitmap_WhenNoTextIsDetected_ReturnsNoTextToken`) passed at both baseline (via the live-engine-failure fallback path) and final (via the deterministic mock) — an explained mechanism change with no outcome change, consistent with AC4's "no regression, no incidental masking" requirement.

## 7. Code Quality Checks

| Check | Verdict | Evidence |
|---|---|---|
| Toolchain order followed (format -> lint -> type-check -> test) | PASS | Plan Phase 2 tasks P2-T1 through P2-T14 executed in the required order; each step's evidence artifact records EXIT_CODE and is timestamped in ascending order (17:16, 17:17, 17:23, 17:24, 17:27). |
| Restart-on-failure discipline | PASS | Evidence explicitly records "no restart of the loop (P2-T2) was required" (CSharpier) and that no step failed or changed files requiring a restart. |
| No temp-file usage | PASS | No temporary files created or referenced by the new/modified test or production code. |
| No workflow files touched | PASS (no `modified-workflow-needs-green-run` obligation) | `git diff --name-only` shows zero `.github/workflows/**` paths changed. |
| No benchmark baselines touched | PASS (`.claude/rules/benchmark-baselines.md` not applicable) | Zero baseline/benchmark files changed. |
| Evidence Location Invariant | PASS | All evidence for this feature is under `docs/features/active/2026-06-19-tesseract-engine-initialization-failure-209/evidence/{baseline,qa-gates}/`; `git diff --name-only <merge-base>..HEAD \| grep -E "^artifacts/(baselines\|qa\|evidence\|coverage)/"` returns zero matches. No `validate_evidence_locations.py` exists in this repo (confirmed via `find`); the manual grep substitute is the working check per prior-session memory. |

## Acceptance Criteria Cross-Reference (detail in `feature-audit`)

All five AC items in `issue.md` are pre-checked `[x]`. This policy audit independently confirmed AC1–AC5 by direct code/evidence inspection (see `## 4`/`## 5`/`## 6` above); see `feature-audit.2026-07-18T17-42.md` for the formal per-criterion evaluation table.

## Overall Disposition

**PARTIAL** (not a clean PASS). All acceptance criteria are satisfied and the toolchain gates (format/lint/nullable/test) all pass cleanly with no regressions. The one Blocking gap is the 0% line coverage on the new `TesseractOcrTextExtractor.cs` file, which violates the repo's new-code coverage floor (85%/90% depending on which policy document is applied — both agree it fails at 0%). This is a policy-compliance gap independent of AC satisfaction; see `remediation-inputs.2026-07-18T17-42.md` for the recommended remediation path.
