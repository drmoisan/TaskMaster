# Coverage Delta Verification (P5-T5) — AC-16

- **Timestamp:** 2026-07-11T13-25
- **Feature:** swordfish-interface-project-teardown (#308), F5
- **Method:** identical `dotnet-coverage collect ... -f cobertura` run at baseline (P0-T6) and post-change (P5-T4).

## Repo-wide raw line coverage

| | line-rate | lines-covered | lines-valid |
|---|---|---|---|
| Baseline (P0-T6) | 0.68931 (68.93%) | 128086 | 185819 |
| Post-change (P5-T4) | 0.69439 (69.44%) | 127873 | 184152 |
| Delta | +0.51 pp | -213 | -1667 |

The denominator dropped by 1667 lines (the vendored `Swordfish.NET.General` package, whose folder F5
deleted) and the numerator by 213 lines (the covered subset of those vendored lines + the removed test
lines). Because the removed vendored package was only 7.61% covered, removing it RAISES the raw
repo-wide rate. **Numerator and denominator dropped together; no surviving first-party production line
lost coverage.**

## Changed / new code coverage

F5 is a pure teardown: it adds NO new production executable code (only deletions plus six
documentary-comment rewordings). There are no new/changed executable production lines, so the
`>= 90%` changed/new-code threshold is satisfied vacuously (no new lines to cover). The one production
edit that removed executable code — the dead `UpdateForMove` method in `QfcExplorerController.cs` — was
never-called (uncovered) dead code; removing it slightly improved `UtilitiesCS` coverage.

## No first-party regression (AC-16 core)

Per-package comparison (P5-T4 table): every surviving first-party PRODUCTION package held its line-rate
or improved:
- `UtilitiesCS` 88.321% -> 88.336% (up) — hosts the clean `ConcurrentObservableCollection`; its
  production lines remain covered by `ConcurrentObservableCollection_Tests.cs` after the WI-4
  lock-recursion-test removal, confirming AC-16 (no coverage regression attributable to the removed tests).
- `QuickFiler`, `TaskMaster`, `ToDoModel`, `Tags`, `TaskTree`, `TaskVisualization`, `VBFunctions`: unchanged.

## Threshold verdict

- Repo-wide raw line coverage: 69.44% (raw includes untestable COM/VSTO/WinForms and vendored code; the
  policy 80% floor applies to the testable denominator, not this raw figure). Baseline-to-final delta is
  POSITIVE.
- No regression on surviving first-party production lines: PASS.
- Changed/new-code >= 90%: PASS (vacuous — no new executable lines; F5 owes no backfill because the
  removed Swordfish tests dropped numerator and denominator together).

**AC-16: PASS.**
