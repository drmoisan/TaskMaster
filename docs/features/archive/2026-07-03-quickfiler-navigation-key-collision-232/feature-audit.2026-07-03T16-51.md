# Feature Audit: QuickFiler Navigation-Key Collision Fix (Issue #232)

**Audit Date:** 2026-07-03
**Feature Folder:** `docs/features/active/2026-07-03-quickfiler-navigation-key-collision-232`
**Base Branch:** `main`
**Head Branch:** `TaskMaster-wt-2026-07-03-10-11`
**Work Mode:** `full-bug`
**Audit Type:** Initial acceptance review (full branch diff vs base)

---

## Scope and Baseline

- **Base branch:** `main` (commit `00507b595297c3e6970634a1855f1144c987dbdf`)
- **Head branch/commit:** `TaskMaster-wt-2026-07-03-10-11` (commit `90e75ec19e0d0bb88e6d05168354cac4a66a6a2a`)
- **Merge base:** `00507b595297c3e6970634a1855f1144c987dbdf`
- **Evidence sources:**
  - Primary: `artifacts/pr_context.summary.txt`
  - Secondary baseline diff: `artifacts/pr_context.appendix.txt`
  - Feature evidence: `docs/features/active/2026-07-03-quickfiler-navigation-key-collision-232/evidence/**`
  - Additional evidence: `git diff 00507b59..90e75ec1` (authoritative scope)
- **Feature folder used:** `docs/features/active/2026-07-03-quickfiler-navigation-key-collision-232`
- **Requirements source:** `spec.md` (full-bug work mode → `spec.md` only)
- **Work mode resolution note:** `issue.md` line 12 declares `- Work Mode: full-bug`, which resolves the authoritative AC source to `spec.md` only (per acceptance-criteria-tracking).
- **Scope note:** Full-branch audit against base; scope was verified from `git diff` directly because the PR context summary overview under-reports the C# changes (`Core logic changes: 0 files`). The larger dequeue-time high-confidence rework is intentionally excluded and tracked as feature #233 (AC8 scope confirmation).

---

## Acceptance Criteria Inventory

**Authoritative AC source files for this run:**
- `docs/features/active/2026-07-03-quickfiler-navigation-key-collision-232/spec.md` — only source (full-bug)

### Acceptance criteria

1. AC1: `LoadControlsAndHandlers_01(TableLayoutPanel, List<QfcItemGroup>)` unregisters the outgoing page's `"Collection"` navigation keys and registers the incoming page's navigation keys on every swap (verified via mock-based assertion on `RegisterNavigation()`/`UnregisterNavigation()` call order).
2. AC2: The reported reproduction (1-item page popped out; `RemoveSpecificControlGroupAsync` reaches zero items; `SkipGroupAsync` swaps in a cached page whose key range overlaps a previously-abandoned page's stale keys) no longer throws `ArgumentException`; a regression test encodes this exact scenario and passes.
3. AC3: `RemoveSpecificControlGroupAsync`'s trailing `RegisterNavigation()` does not double-register keys already registered by a swap that occurred earlier in the same call.
4. AC4: `QfcDatamodel.ScoreRemainingQueueMailItemAsync` logs at debug level, for every item scored, the item summary (Subject/EntryID), the computed score, and a caller-context string.
5. AC5: `QfcItemController.LoadFolderHandler`/`LoadFolderHandlerAsync` log at debug level, at all 4 assignment points, the item summary, computed score, and a caller-context string distinguishing the branch.
6. AC6: `QfcHighConfidencePreFilter.FilterAsync` logs at debug level, for every item scored, the item summary, computed score, and topFolder, and a caller-context string; a new `logger` field is added to this file following repo log4net convention.
7. AC7: The logging additions introduce no behavior change — all pre-existing tests covering `ScoreRemainingQueueMailItemAsync`, `LoadFolderHandler(Async)`, and `QfcHighConfidencePreFilterTests.cs` continue to pass unmodified.
8. AC8: No unintended behavior changes outside the defined scope (fixed-batch-without-backfill pattern, dormant #171 pre-filter, and `removespecificcontrolgroupcounter` reentrancy hygiene remain untouched and are documented as follow-up).
9. AC9: Full C# toolchain (csharpier → .NET analyzers → nullable/TreatWarningsAsErrors → MSTest via vstest.console.exe) passes with no regressions, in this exact order, in a single clean pass.
10. AC10: Repository-wide and changed-line coverage obligations are met per the ratified COM/WinForms exemption boundary; `QfcHighConfidencePreFilter.cs` changed lines meet the `>=90%` new/changed-code target.

---

## Acceptance Criteria Evaluation

| # | Criterion | Status | Evidence | Verification command(s) | Notes |
|---|-----------|--------|----------|--------------------------|-------|
| 1 | Swap unregisters outgoing + registers incoming keys | PASS | Diff routes `LoadControlsAndHandlers_01` through `SwapItemGroups`; `LoadControlsAndHandlers_01_SwapsPage_RemovesOutgoingKeysAndAddsIncomingKeys` asserts outgoing key removed and exactly one incoming key. | `git diff 00507b59..HEAD -- QfcCollectionController.cs`; `vstest ... /Tests:LoadControlsAndHandlers_01_SwapsPage_...` | `swap-register-unregister-order.pass.md`. |
| 2 | Reported repro no longer throws | PASS | `SwapItemGroups_ThenSkipGuardedTrailingRegister_LeavesExactlyOneEntryPerIncomingKey` asserts `NotThrow` for the zero-item→swap path; the expect-fail boundary test documents pre/post registration behavior. | `vstest ... /Tests:SwapItemGroups_ThenSkipGuarded...` | `reported-repro.pass-after-fix.md`. |
| 3 | Trailing register does not double-register | PASS | `swapAlreadyRegistered` guard added; `RegisterNavigation_CalledTwiceWithoutInterveningUnregister_...` asserts the throw the guard avoids; guarded-skip test leaves exactly one entry per key. | `vstest ... /Tests:RegisterNavigation_CalledTwice...,SwapItemGroups_ThenSkip...` | `double-registration-guard.pass.md`. |
| 4 | `ScoreRemainingQueueMailItemAsync` debug-logs summary/score/caller | PASS | `logger.Debug` added in `ScoreRemainingQueueMailItemAsync` logging `Subject`, `EntryID`, `Score`, and a caller-context string. | `git show HEAD:QfcDatamodel.cs` (lines 316–329) | Minor: context string reads `LoadRemainingEmailsToQueueAsync` (the logical caller), not the enclosing method name. AC met (a caller-context string is present). |
| 5 | `LoadFolderHandler(Async)` logs at all 4 points | PASS | Four `logger.Debug` calls with `FromField`/`FromArrayOrString` context strings and `TopScore()`. | `git diff ... -- QfcItemController.FolderHandling.cs` | `part-b-logging-no-regression.md`. |
| 6 | `FilterAsync` logs summary/score/topFolder + new `logger` field | PASS | New `private static readonly log4net.ILog logger` field; `logger.Debug` logs Subject/EntryID/Score/TopFolder in the scoring lambda. | `git diff ... -- QfcHighConfidencePreFilter.cs` | Non-exempt file. |
| 7 | Logging introduces no behavior change | PASS | 29/29 pre-existing tests across the three affected files pass unmodified; no assertions altered. | `vstest ... /Tests:<29 names>` | `part-b-logging-no-regression.md`. |
| 8 | No out-of-scope behavior change | PASS | Reentrancy counter byte-identical; `QfcHighConfidencePreFilterLoader` untouched; batch/backfill untouched; follow-ups recorded. | `git diff --stat 00507b59 -- QuickFiler/ QuickFiler.Test/` | `ac8-scope-confirmation.md`, `follow-up-candidates.md`. |
| 9 | Full C# toolchain passes with no regressions | PASS | csharpier EXIT 0 (0 changed); analyzers EXIT 0 (0 errors, no new diagnostics); nullable EXIT 1 with proven zero-delta legacy population; vstest EXIT 0 (4641/4641). | `csharpier format .`; `msbuild ... EnableNETAnalyzers`; `msbuild ... Nullable=enable`; `vstest ...` | Nullable exit-1 is pre-existing legacy VSTO debt; no-regression proven in `msbuild-nullable-final.md`. |
| 10 | Coverage obligations met per exemption boundary | PARTIAL | Transcribed: `QfcHighConfidencePreFilter.cs` changed lines 100% (>= 90%); repo-wide 76.5758% → 76.5712% (flat). Substance supports the claim. | `vstest ... /Settings:<cobertura.runsettings>` | Blocking gap: the machine-readable Cobertura coverage artifact for #232 is absent (`artifacts/csharp/coverage.xml` missing; no `coverage.xml` persisted under `.../evidence/**`). The claim cannot be independently verified from an artifact. See policy audit Section 8. |

---

## Summary

**Overall Feature Readiness:** NEEDS REVISION

**Criteria summary:**
- **PASS:** 9 criteria (AC1–AC9)
- **PARTIAL:** 1 criterion (AC10)
- **UNVERIFIED:** 0 criteria
- **FAIL:** 0 criteria

**Top gaps preventing PASS:**

1. AC10 — the machine-readable C# coverage artifact for Issue #232 is absent. The transcribed coverage numbers (100% changed-line on the non-exempt file; repo-wide flat) support the claim but cannot be independently verified from a coverage artifact as the feature-review Coverage Verification model requires. This is the single blocking item.
2. None.
3. None.

**Recommended follow-up verification steps:**

1. Regenerate the Cobertura coverage run for the touched assemblies and persist the XML at the canonical `artifacts/csharp/coverage.xml` (or `.../evidence/coverage/`); then re-verify that `QfcHighConfidencePreFilter.cs` changed lines are 100% and that repo-wide coverage shows no regression against baseline.
2. Optionally align the `QfcDatamodel` log caller-context string with its enclosing method name (`ScoreRemainingQueueMailItemAsync`) or document the intended logical-caller labeling.

---

## Acceptance Criteria Check-off

Per the acceptance-criteria tracking rules:
- Criteria evaluated as **PASS** may be checked off in the authoritative source file if represented as checkboxes and not already checked.
- Criteria evaluated as **PARTIAL/FAIL/UNVERIFIED** must remain unchecked.

All ten AC items in `spec.md` were already checked off `[x]` by the executor upon delivery. AC1–AC9 are independently confirmed PASS and their existing check-off is correct. AC10 is evaluated **PARTIAL** by this review: the underlying coverage work was delivered (transcribed 100% changed-line, flat repo-wide), but the machine-readable coverage artifact required for independent verification is absent. Because the PARTIAL reflects an evidence-verifiability gap rather than undelivered coverage work, this review does not silently revert the executor's AC10 check-off; instead the gap is recorded here and in `remediation-inputs.2026-07-03T16-51.md`. No `spec.md` checkbox was modified by this review.

### AC Status Summary

- Source: `docs/features/active/2026-07-03-quickfiler-navigation-key-collision-232/spec.md`
- Total AC items: 10
- Checked off (delivered): 10 (executor-set; AC10 flagged PARTIAL by review pending the coverage artifact)
- Remaining (unchecked): 0
- Items remaining: None (checkbox state); AC10 carries an open review-flagged verification gap.

| Source File | Total AC | Checked (PASS) | Unchecked | Notes |
|-------------|----------|----------------|-----------|-------|
| `spec.md` | 10 | 10 | 0 | Checkbox-backed. Review verdict: 9 PASS + 1 PARTIAL (AC10); no checkbox changed by this review. |
