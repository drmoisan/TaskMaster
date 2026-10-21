# Feature Audit: Issue #439 EfcViewer lineage and segment navigation

**Audit Date:** 2026-08-24
**Feature Folder:** `docs/features/active/2026-08-07-efcviewer-missing-lineage-and-segment-navigation-439`
**Base Branch:** `main`
**Head Branch:** `bug/efcviewer-missing-lineage-and-segment-navigation-439`
**Work Mode:** `full-bug`
**Audit Type:** initial acceptance review

## Scope and Baseline

- **Base branch:** `main` at `988e819b3bf3d31d6bbe523a2ce6c66189ce718d`.
- **Head branch/commit:** `bug/efcviewer-missing-lineage-and-segment-navigation-439` at `f1b8e504d9d84f2327c919cb27bdb7b076424a6b`.
- **Merge base:** `988e819b3bf3d31d6bbe523a2ce6c66189ce718d`.
- **Evidence sources:** primary `artifacts/pr_context.summary.txt`; secondary `artifacts/pr_context.appendix.txt`; feature QA, regression, coverage, commit, and reconciliation records in this feature folder.
- **Feature folder used:** the Issue #439 branch suffix and material changed `spec.md` identify this active folder.
- **Requirements source:** `spec.md` only, as required by `issue.md` work mode `full-bug`.
- **Scope note:** review covers the complete merge-base-to-HEAD range. No C# or test drift occurred between implementation commit `c39db103` and `HEAD`.

## Acceptance Criteria Inventory

**Authoritative AC source files for this run:**

- `docs/features/active/2026-08-07-efcviewer-missing-lineage-and-segment-navigation-439/spec.md` — only source.

### Acceptance criteria

1. Given an archive-relative suggestion or search target and `ArchiveRootPath`, the Efc boundary sends the correctly root-expanded full path to `IFolderHierarchyProvider.ResolveLeafKeyAsync` while retaining the original target as the row's filing target.
2. Given a target already rooted at `ArchiveRootPath` under ordinal-ignore-case comparison, the boundary sends that full path unchanged and does not duplicate the root.
3. Given a resolved three-node ancestor chain for a suggestion or search row, the Efc-generated renderer displays those nodes in root-to-leaf order with exactly one `→` between each adjacent pair.
4. Given a resolved row with a `FolderScore` keyed by its original archive-relative target, the row displays that score after hierarchy resolution and normal row selection returns that original target.
5. Given a null resolution key, empty ancestor chain, or hierarchy-provider failure, the row remains selectable as one segment and the fallback cause is sent to the existing logging boundary.
6. Given an ordinary archive-relative suggestion or search target that resolves after root expansion, the row does not use the one-segment fallback.
7. Given a malformed, missing, banner, pseudo-row, out-of-range row, out-of-range segment, or invalid child activation message, the codec/router rejects it without changing selected or expanded state.
8. Given activation of a valid non-leaf segment, the router selects that ancestor's archive-relative target and prevents the row-level handler from reselecting the original leaf.
9. Given a valid activated non-leaf ancestor, expansion requests its immediate children using that ancestor's stable `FolderTreeNodeKey`, not the original leaf key.
10. Given rendered immediate children for an expanded ancestor, activation of a valid child selects that child or sibling's archive-relative target.
11. Existing segment double-click collapses trailing segments, while keyboard Left/Right behavior remains unchanged.
12. `====` banner rows and `Trash to Delete` retain their existing behavior and do not gain lineage, hierarchy resolution, or child activation.
13. No ItemViewer `FolderBreadcrumb.html` behavior, Issue #400 behavior, score-model calculation, public configuration, or external API changes are included.
14. The required C# formatter, analyzer, nullable, MSTest, and coverage comparison pass in one final ordered toolchain pass, with canonical Issue #439 evidence artifacts present.

## Acceptance Criteria Evaluation

| # | Criterion | Status | Evidence | Verification command(s) | Notes |
|---:|---|---|---|---|---|
| 1 | Root expansion and preserved filing target | PASS | Router/controller diff; binding and path/row regression evidence | focused `vstest` recorded in `issue-439-post-fix-regression.md` | Provider receives hierarchy path; row retains original target. |
| 2 | Already-rooted case-insensitive input | PASS | `Issue439AlreadyRootedTargetRemainsUnchangedWithCaseInsensitiveArchiveMatch` | focused recorded `vstest` | Separator-aware root checks prevent duplication. |
| 3 | Root-first arrows | PASS | renderer test and typed-navigation evidence | focused recorded `vstest` | Renderer requires exactly two arrows for three segments. |
| 4 | Original-key score and normal selection | PASS | row-builder/router tests | focused recorded `vstest` | `FilingTarget` remains distinct from full segment paths. |
| 5 | Selectable fallback and logging | PASS | router fallback/queue tests | focused recorded `vstest` | Null key, empty chain, cancellation, and exception paths are covered. |
| 6 | Ordinary rooted target avoids fallback | PASS | Issue #439 router tests | focused recorded `vstest` | Archive-root expansion is tested before provider resolution. |
| 7 | Invalid typed input preserves state | PASS | codec and router tests | focused recorded `vstest` | Required indices and valid row/segment/child state are enforced. |
| 8 | Ancestor selection and stopped propagation | PASS | router and generated-document tests | focused recorded `vstest` | Document assets call `stopPropagation`; router selects relative ancestor. |
| 9 | Stable ancestor key for expansion | PASS | router and modified queue tests | focused recorded `vstest` | Expansion uses provider-bound active-segment key. |
| 10 | Child/sibling activation | PASS | Issue #439 router test | focused recorded `vstest` | Child full path is mapped to archive-relative selection. |
| 11 | Collapse and keyboard behavior | PASS | typed-navigation evidence and unchanged router controls | focused recorded `vstest` | Double-click remains separate from activation. |
| 12 | Banner/trash behavior | PASS | invalid-navigation tests and row classification | focused recorded `vstest` | No hierarchy key/action applies to non-suggestion rows. |
| 13 | Scope/API exclusions | PASS | complete PR appendix and code diff | `git diff --name-status 988e819b..HEAD` | No ItemViewer resource, score-model, configuration, or public API change. |
| 14 | Final C# QA and coverage | FAIL | final QA loop and normalized comparison | documented CSharpier, MSBuild, vstest/coverage commands | Toolchain and aggregate metrics pass, but modified `EfcFormController.cs` is 81/721 = 11.234397%, below the mandatory 80% modified-file floor. |

## Summary

**Overall Feature Readiness:** NEEDS REVISION

**Criteria summary:**

- **PASS:** 13 criteria
- **PARTIAL:** 0 criteria
- **UNVERIFIED:** 0 criteria
- **FAIL:** 1 criterion

**Top gaps preventing PR readiness:**

1. The policy audit finds a 596-line modified production file and a 531-line new test file, each above the required 500-line limit.
2. Modified `EfcFormController.cs` coverage is 81/721 = 11.234397%, below the required 80% modified-file floor.

**Recommended follow-up verification steps:**

1. Execute the remediation plan to split the router and Issue #439 test file into cohesive units within the line limit and remediate EfcForm per-file coverage with headless seams.
2. Re-run the final C# QA loop and feature review against the resulting commit.

## Acceptance Criteria Check-off

All criteria were already checked in the authoritative source. No acceptance-checkbox edit was made during this review.

### AC Status Summary

- Source: `docs/features/active/2026-08-07-efcviewer-missing-lineage-and-segment-navigation-439/spec.md`
- Total AC items: 14
- Checked off (delivered): 13
- Remaining (unchecked): 1
- Items remaining: The required C# formatter, analyzer, nullable, MSTest, and coverage comparison pass in one final ordered toolchain pass, with canonical Issue #439 evidence artifacts present.

| Source File | Total AC | Checked (PASS) | Unchecked | Notes |
|---|---:|---:|---:|---|
| `spec.md` | 14 | 13 | 1 | Checkbox-backed full-bug authoritative source; criterion 14 was returned to unchecked because the modified-file coverage floor is not met. |
