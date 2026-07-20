# folder-combobox-fallback-index-out-of-range (Issue #392)

- Date captured: 2026-07-20
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/folder-combobox-fallback-index-out-of-range/ (Issue #392)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #392
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/392
- Last Updated: 2026-07-20
- Work Mode: minor-audit

## Summary

`QfcItemController.AssignFolderComboBox` unconditionally selects folder-suggestion index 1 when no predetermined folder matches. When the folder suggestion list contains exactly one entry, the breadcrumb pipeline (`BreadcrumbBridgeCoordinator.SelectRow` -> `BreadcrumbStateModel.SelectRow`) rejects the out-of-range index and throws `System.ArgumentOutOfRangeException`, aborting the QuickFiler load sequence.

## Environment

- OS/version: Windows 11 Pro 10.0.26200
- Runtime: .NET Framework 4.8 VSTO add-in (TaskMaster / QuickFiler)
- Command/flags used: Outlook ribbon action "QuickFiler High Confidence" (`RibbonViewer.QuickFilerHighConfidence_Click`)
- Data source or fixture: Live mailbox item whose folder predictor returns a single folder suggestion

## Steps to Reproduce

1. Launch QuickFiler in high-confidence mode from the TaskMaster ribbon.
2. Load an email whose `FolderPredictor` produces a `FolderArray` with exactly one entry and no predetermined folder match.
3. `QfcCollectionController.LoadSecondaryAsync` invokes `QfcItemController.AssignFolderComboBox`, which calls `_itemViewer.SetFolderSelectedIndex(1)`.

## Expected Behavior

With a single available suggestion, the top (index 0) suggestion is selected and loading completes without error.

## Actual Behavior

`System.ArgumentOutOfRangeException` is thrown from `BreadcrumbStateModel.SelectRow` (UtilitiesCS) and propagates up through `QfcCollectionController.LoadSecondaryAsync` / `QfcFormController.LoadItemsAsync` / `QfcHomeController.LaunchAsync`, aborting the QuickFiler load:

```
Message=Row selection requires -1 or an index in [0, 0].
Parameter name: index
Actual value was 1.
  at UtilitiesCS.OutlookObjects.Folder.BreadcrumbStateModel.SelectRow(Int32 index) in BreadcrumbStateModel.cs:line 237
  at QuickFiler.Viewers.BreadcrumbBridgeCoordinator.SelectRow(Int32 index) in BreadcrumbBridgeCoordinator.cs:line 127
  at QuickFiler.Controllers.QfcItemController.AssignFolderComboBox() in QfcItemController.FolderHandling.cs:line 204
```

## Logs / Screenshots

- [x] Attached minimal logs or screenshot
- Snippet: full stack trace captured in the exception details above (source: user-supplied crash report, 2026-07-20).

## Impact / Severity

- [ ] Blocker
- [x] High
- [ ] Medium
- [ ] Low

High: the exception aborts the entire QuickFiler high-confidence load path whenever any loaded item has exactly one folder suggestion.

## Suspected Cause / Notes

- `QuickFiler/Controllers/QfcItemController.FolderHandling.cs` line 202: `_itemViewer.SetFolderSelectedIndex(1)` assumes at least two suggestion rows exist.
- The same unguarded index-1 fallback exists in the retained static helper `PopulateAndSelectFolder` (same file, line 228): `comboBox.SelectedIndex = predeterminedIndex >= 0 ? predeterminedIndex : 1;` — a WinForms `ComboBox` with one item also rejects `SelectedIndex = 1`.
- `BreadcrumbStateModel.SelectRow` validation is correct defensive behavior; the defect is in the callers.

## Proposed Fix / Validation Ideas

- [x] Unit coverage areas: clamp the fallback index to the available row count (`rows > 1 ? 1 : 0`; `-1`/no-op when empty) in `AssignFolderComboBox` and mirror the guard in `PopulateAndSelectFolder`; add MSTest regression tests for single-entry and empty suggestion lists.
- [ ] Integration scenario to retest: QuickFiler high-confidence load with a single-suggestion item.
- [x] Manual verification notes: re-run the ribbon "QuickFiler High Confidence" action against an item with one suggestion after the fix.

## Acceptance Criteria

- [x] AC-1: A deterministic MSTest regression test reproduces the defect (fallback selection with exactly one folder suggestion) and fails before the fix; the same test passes after the fix. No temporary files or external dependencies are used.
- [x] AC-2: `QfcItemController.AssignFolderComboBox` no longer throws `ArgumentOutOfRangeException` when `FolderArray` has exactly one entry and no predetermined folder matches: it selects index 0 (the only suggestion) instead of index 1.
- [x] AC-3: Existing multi-suggestion behavior is preserved: with two or more suggestions and no predetermined match, index 1 remains selected; with a predetermined folder present in the list, that folder remains preselected.
- [x] AC-4: The retained static helper `PopulateAndSelectFolder` applies the same bounds-safe fallback so a single-item combo box does not throw.
- [x] AC-5: The full C# toolchain passes in order (CSharpier format, .NET analyzers build, nullable build, MSTest via vstest.console.exe) with zero regressions relative to the Phase 0 baseline, and new/changed code meets the >= 90% coverage target. Scope note (amended 2026-07-20 by orchestrator, before feature review): nullable enforcement is scoped to first-party projects per `.claude/rules/csharp.md` (analyzers/nullable are wired to first-party projects only; vendored projects are excluded). The 34 pre-existing nullable errors in vendored `SVGControl.csproj` are byte-identical to the Phase 0 baseline, are not enforced by CI (MSBuild incremental skip), and are tracked separately in `docs/features/potential/2026-07-07-ci-nullable-check-skipped-vendored-projects.md`; they do not gate this bug fix.

Remediation Cycle 1 closure note (2026-07-20, no AC checkbox changes; all five ACs above were already `[x]` before this cycle and are unaffected): R1 closed `QfcItemController.FolderHandling.cs`'s class-level branch-coverage gap (73.81% -> 76.19%, clearing the uniform >= 75% floor in `.claude/rules/quality-tiers.md`) by adding one new test exercising a previously-uncovered, pre-existing branch, with zero production-code change and zero regression (542/542 tests passing, up from 541). R2 (the `QuickFiler` package-wide and canonical repo-wide coverage gaps) is dispositioned `SCOPE_CHANGE`, tracked in open GitHub issue #136 (*Feature: quickfiler-80-per-file-coverage*). Full evidence index: `evidence/issue-updates/remediation-cycle1-note.2026-07-20T18-48.md`.

## Next Step

- [x] Promote to GitHub issue (bug-report template)
- [x] Move to active fix folder / branch
