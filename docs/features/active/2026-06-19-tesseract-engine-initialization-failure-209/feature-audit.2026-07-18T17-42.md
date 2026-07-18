# Feature Audit — tesseract-engine-initialization-failure (Issue #209)

- Branch: `bug/tesseract-engine-initialization-failure-209`
- Base: `main` @ `a4977216467c6a275648e6ce134adf847693fc6a`
- Work Mode: `minor-audit`
- Timestamp: 2026-07-18T17-42

## Summary

This feature-audit verifies AC1–AC5 from `docs/features/active/2026-06-19-tesseract-engine-initialization-failure-209/issue.md`'s `## Acceptance Criteria` section against the actual branch diff and the executor's baseline/final evidence artifacts. All five criteria are verified PASS by this review through direct code inspection, grep-based re-verification, and independent Cobertura-XML parsing (not solely by trusting the pre-existing `[x]` marks in `issue.md`).

## Scope and Baseline

- Resolved base branch: `main`
- Merge-base: `a4977216467c6a275648e6ce134adf847693fc6a` (re-verified via `git merge-base HEAD origin/main`; zero drift from the caller-supplied value)
- Head: `376f9b0d799ef33790f9315f7eaae82858525a05`
- Work-mode routing: `minor-audit` per the `- Work Mode: minor-audit` marker in `issue.md`; AC source is exclusively the `## Acceptance Criteria` section of `issue.md` (lines 78-84). No `spec.md` or `user-story.md` exists in the feature folder (confirmed via directory listing), consistent with `minor-audit` mode and with the plan's own P0-T6 confirmation.
- This cycle's scope, per the issue's "Scope Note (2026-07-18)" addendum, is explicitly limited to the test-isolation/seam-extraction fix, not the live-Outlook OCR runtime tessdata-deployment problem described in the issue's original `Summary`/`Steps to Reproduce` sections — that remains open as a separate follow-up per the issue text itself. This audit evaluates only the five checkbox AC items, which match that narrowed scope; this is the issue author's own scope statement, not a narrowing imposed by this review.

## Acceptance Criteria Inventory

| ID | Criterion (verbatim from `issue.md`) | Source checkbox state |
|---|---|---|
| AC1 | `ImageStripper` obtains OCR text extraction through an injectable seam (interface), not a directly-constructed `Tesseract.TesseractEngine`. Default/production behavior is unchanged (still uses the real Tesseract engine when no fake is injected). | `[x]` |
| AC2 | `Analyze_WithTesseractAndValidImageAttachment_ReturnsNoTextFoundToken` and `ExtractOcrInfo_WithBitmap_WhenNoTextIsDetected_ReturnsNoTextToken` in `ImageStripper_Tests.cs` no longer construct or depend on a live `Tesseract.TesseractEngine`; they inject a Moq-based fake through the new seam instead. | `[x]` |
| AC3 | Full MSTest suite run (`vstest.console.exe` across all first-party `*.Test.dll` assemblies) shows zero occurrences of `Failed loading language 'eng'` / `Error opening data file ... tessdata` in the output. | `[x]` |
| AC4 | No other test in the suite newly fails or newly passes as a result of this change (no regression, no incidental masking). | `[x]` |
| AC5 | CSharpier format, .NET analyzer build, and nullable build all pass with zero errors. | `[x]` |

## Acceptance Criteria Evaluation

| ID | Verdict | Evidence |
|---|---|---|
| AC1 | **PASS** | Direct code read of `UtilitiesCS/EmailIntelligence/EmailParsingSorting/ImageStripper.cs` (post-change): `extract_text(Bitmap bitmap)` now reads `return _ocrTextExtractor.ExtractText(bitmap);` with zero references to `Tesseract.TesseractEngine` in that method or anywhere else in the file (confirmed: the file's only remaining `Tesseract`-namespace reference was removed — `using Tesseract;` is deleted from `ImageStripper.cs`'s using block). The parameterless `ImageStripper()` constructor chains `: this(cachefile: null, ocrTextExtractor: null)`, and the two-parameter constructor sets `_ocrTextExtractor = ocrTextExtractor ?? new TesseractOcrTextExtractor();` — confirming default/production behavior (real Tesseract engine) is preserved when no fake is supplied. `TesseractOcrTextExtractor` is a genuine interface-backed seam (`IOcrTextExtractor`), not a partial/leaky abstraction. |
| AC2 | **PASS** | `grep -n "TesseractEngine" UtilitiesCS.Test/EmailIntelligence/ImageStripper_Tests.cs` → 0 matches (re-run independently by this review, not only cited from the plan's P1-T10 task). Direct read of both named test methods (lines 254-271, 273-288) confirms each constructs `var mockExtractor = new Mock<IOcrTextExtractor>();`, sets up `ExtractText(It.IsAny<Bitmap>())` to return `string.Empty`, and constructs `new ImageStripper(mockExtractor.Object)`. Both pre-existing Assert lines are unchanged. |
| AC3 | **PASS** | `docs/features/active/2026-06-19-tesseract-engine-initialization-failure-209/evidence/qa-gates/final-mstest.2026-07-18T17-24.md` records: baseline had 2 occurrences of `Failed loading language 'eng'` and 2 of `Error opening data file` (both against the missing `%LOCALAPPDATA%\TaskMaster\tessdata\eng.traineddata`); the final run records 0 occurrences of either string across the same 8-assembly, full-suite run. This is a direct, itemized before/after comparison, not an assumption. |
| AC4 | **PASS** | Baseline: 5701 total / 5701 passed / 0 failed / 0 skipped. Final: 5701 total / 5701 passed / 0 failed / 0 skipped. Identical aggregate counts. The two directly-affected tests are itemized in `final-mstest.2026-07-18T17-24.md`: both PASSED at baseline (via the live-engine-failure "no text found" fallback path) and PASS at final (via the injected mock's deterministic "no text found" return) — an explained, expected mechanism change with no outcome change. No other test in the suite changed status. |
| AC5 | **PASS** | `evidence/qa-gates/final-csharpier.2026-07-18T17-16.md` (EXIT_CODE 0), `final-analyzer-build.2026-07-18T17-17.md` (EXIT_CODE 0, 0 errors, 75 warnings — unchanged from baseline), `final-nullable-build.2026-07-18T17-23.md` (EXIT_CODE 0, 0 errors, 0 warnings). Independently re-verified by this review: `dotnet tool run csharpier check` against all three changed `.cs` files returned zero formatting diffs. |

## Acceptance Criteria Check-off

All five items were already checked `[x]` in `issue.md` at the start of this review cycle (checked off by the plan's own P2-T verification tasks during execution, per `acceptance-criteria-tracking`'s "When Executors Check Off AC" protocol). This feature-audit's independent re-verification above confirms all five check-offs are correctly supported by evidence; no un-check or correction is required.

### Acceptance Criteria Status
- Source: `docs/features/active/2026-06-19-tesseract-engine-initialization-failure-209/issue.md` (`## Acceptance Criteria` section)
- Total AC items: 5
- Checked off (delivered): 5
- Remaining (unchecked): 0
- Items remaining: none

## Relationship to the Open Follow-Up

The issue's original `Summary` and `Steps to Reproduce` describe a live-Outlook OCR runtime failure (tessdata not provisioned in the deployed working directory). The issue's own "Scope Note (2026-07-18)" explicitly defers that live-environment problem to a separate follow-up, scoping this cycle to the test-isolation fix only. This feature-audit does not evaluate the live-Outlook runtime scenario (it requires a live Outlook/VSTO process and provisioned tessdata files, which are unavailable in this review environment) because it is out of scope for the AC set actually authored for this cycle, not because this review declined to look — the issue's proposed-fix checklist items for "Integration scenario to retest" and "Manual verification notes" are themselves marked `[ ]` (deferred) in `issue.md`, consistent with this being intentionally unaddressed follow-up work rather than a gap in this review.

## Cross-Reference to Policy Audit

One Blocking finding independent of AC satisfaction was identified in `policy-audit.2026-07-18T17-42.md`: the new `TesseractOcrTextExtractor.cs` file has 0% line coverage, below the repo's new-code coverage floor. This does not affect any AC verdict above (none of AC1-AC5 as literally worded require a specific coverage percentage on the new file), but is a separate, unresolved policy-compliance gap tracked in `remediation-inputs.2026-07-18T17-42.md`.

## Overall Verdict

**PASS on all acceptance criteria (5/5).** Feature delivery for this `minor-audit` cycle is complete against its own AC set. Merge readiness is gated by the separate Blocking coverage finding documented in the policy audit and remediation inputs, not by any AC gap.
