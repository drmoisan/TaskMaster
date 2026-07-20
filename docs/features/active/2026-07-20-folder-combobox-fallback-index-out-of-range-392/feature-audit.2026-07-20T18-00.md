# Feature Audit — folder-combobox-fallback-index-out-of-range (Issue #392)

- Timestamp: 2026-07-20T18-00
- Reviewer: feature-review (initial audit)
- Work Mode: `minor-audit`

## Scope and Baseline

- Base branch (resolved): `main` @ `bd43572498474be89d80e1f9620dffb132ade377`.
- Head: `8f34f8ef45d188f02ea19caef3c6e2b610f1a4ab`.
- Audit scope: the full branch diff vs the merge-base (feature-vs-base), not any plan/task/phase
  subset. Confirmed via `git diff --numstat bd435724..8f34f8ef`: 2 changed `.cs` files (1 production,
  1 test), 29 added Markdown files (plan, issue, evidence).
- Acceptance-criteria source (per `minor-audit` marker in `issue.md`): the explicit
  `## Acceptance Criteria` section of `issue.md` only (AC-1 through AC-5). No `spec.md` or
  `user-story.md` exist in this feature folder, consistent with `minor-audit` mode.
- Evidence: production/test diffs read directly (`git diff bd435724..8f34f8ef -- <path>`); executor
  QA-gate and regression-testing evidence under `evidence/qa-gates/` and `evidence/regression-testing/`
  read as pre-existing (not re-run); canonical C# coverage artifact `artifacts/csharp/coverage.xml`
  parsed directly.

## Acceptance Criteria Inventory

From `issue.md` `## Acceptance Criteria`:

- AC-1: A deterministic MSTest regression test reproduces the defect (fallback selection with exactly
  one folder suggestion) and fails before the fix; the same test passes after the fix. No temporary
  files or external dependencies are used.
- AC-2: `QfcItemController.AssignFolderComboBox` no longer throws `ArgumentOutOfRangeException` when
  `FolderArray` has exactly one entry and no predetermined folder matches: it selects index 0 (the
  only suggestion) instead of index 1.
- AC-3: Existing multi-suggestion behavior is preserved: with two or more suggestions and no
  predetermined match, index 1 remains selected; with a predetermined folder present in the list, that
  folder remains preselected.
- AC-4: The retained static helper `PopulateAndSelectFolder` applies the same bounds-safe fallback so a
  single-item combo box does not throw.
- AC-5: The full C# toolchain passes in order (CSharpier format, .NET analyzers build, nullable build,
  MSTest via vstest.console.exe) with zero regressions relative to the Phase 0 baseline, and
  new/changed code meets the >= 90% coverage target. Scope note (amended 2026-07-20 by orchestrator,
  before feature review): nullable enforcement is scoped to first-party projects per
  `.claude/rules/csharp.md`; the 34 pre-existing nullable errors in vendored `SVGControl.csproj` are
  byte-identical to the Phase 0 baseline, not enforced by CI, tracked separately, and do not gate this
  bug fix.

## Acceptance Criteria Evaluation

| AC | Verdict | Evidence |
|---|---|---|
| AC-1 | PASS | `evidence/regression-testing/fail-before-392.2026-07-20T14-05.md` (targeted run of the two new tests, EXIT_CODE 1, expected-fail tagged) and `pass-after-392.2026-07-20T14-10.md` (same targeted run after the fix, EXIT_CODE 0). No temp files or external dependencies used (grep of the changed test file confirms no `Path.GetTemp*`, no network/DB calls). |
| AC-2 | PASS | `git diff bd435724..8f34f8ef -- QuickFiler/Controllers/QfcItemController.FolderHandling.cs` shows `_itemViewer.SetFolderSelectedIndex(_folderHandler.FolderArray.Length == 1 ? 0 : 1)` replacing the hardcoded `SetFolderSelectedIndex(1)`. Verified by new test `AssignFolderComboBox_WhenSingleSuggestionNoPredeterminedMatch_SelectsIndexZero`, which asserts `mock.Verify(v => v.SetFolderSelectedIndex(0), Times.Once())` and `mock.Verify(v => v.SetFolderSelectedIndex(1), Times.Never())`. |
| AC-3 | PASS | `evidence/regression-testing/targeted-no-regression-392.2026-07-20T14-13.md` (EXIT_CODE 0) re-runs six pre-existing tests covering exact predetermined match, all-missing-predetermined (index-1 fallback with 2+ suggestions), empty array, no-predetermined-folder, predetermined-folder-present, and null-folder-handler paths; all pass unchanged. |
| AC-4 | PASS | `git diff` shows `PopulateAndSelectFolder`'s `comboBox.SelectedIndex = predeterminedIndex >= 0 ? predeterminedIndex : (folderArray.Length == 1 ? 0 : 1)`. New test `PopulateAndSelectFolder_SingleItemNoPredeterminedMatch_SelectsIndexZeroWithoutThrowing` asserts no `ArgumentOutOfRangeException`, `SelectedIndex == 0`, and the correct selected item text. |
| AC-5 | PASS (on its literal terms; see coverage caveat below) | Toolchain: format PASS, analyzers PASS (0 errors), tests PASS (541/541, 0 regressions per `evidence/qa-gates/regression-check-392.2026-07-20T14-42.md` set-difference check), nullable build reproduces the byte-identical pre-existing 34-error vendored-`SVGControl.csproj` condition (0 new, 0 first-party errors per `evidence/qa-gates/nullable-final-392.2026-07-20T15-10.md`), matching the amended scope note. New/changed-code coverage: 100% line coverage on all 5 reported Cobertura sequence points for the changed lines (`evidence/qa-gates/coverage-delta-392.2026-07-20T14-38.md`), exceeding the >= 90% target. **Caveat**: AC-5's text does not itself require repo-wide or package-level coverage floors; the separate, broader repository coverage policy (uniform 85%/75% floor per `.claude/rules/quality-tiers.md`) is evaluated independently in `policy-audit.2026-07-20T18-00.md` Section 5, where it is FAIL at the package and repo-wide scopes (pre-existing, not caused by this fix). That finding does not fail AC-5 itself but is tracked as a separate policy-audit remediation item. |

## Summary

All five acceptance criteria in `issue.md` are met on their literal terms, backed by concrete
evidence: two new regression tests demonstrably fail before and pass after the fix, the fix is applied
identically at both fallback-selection call sites, existing multi-suggestion and predetermined-match
behavior is unchanged, and the full C# toolchain passes with zero regressions relative to baseline
(the nullable gate's literal EXIT_CODE remains 1, but is confirmed byte-identical to a pre-existing,
out-of-scope, vendored condition per the AC-5 scope note). The residual open item is a
policy-level (not AC-level) coverage-floor gap at the `QuickFiler` package and canonical repo-wide
scopes, confirmed pre-existing and unrelated to this fix, routed to remediation via
`remediation-inputs.2026-07-20T18-00.md`.

### Acceptance Criteria Status
- Source: `issue.md` (`## Acceptance Criteria`, AC-1 through AC-5)
- Total AC items: 5
- Checked off (delivered): 5
- Remaining (unchecked): 0
- Items remaining: none

## Acceptance Criteria Check-off

All five AC items are already checked off (`[x]`) in `issue.md`, authored by the executor during plan
execution. This audit independently confirms each PASS verdict above against the cited evidence; no
checkbox text was modified and no additional check-offs were required (all were already `[x]` prior to
this review).
