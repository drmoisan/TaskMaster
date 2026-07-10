# Coverage Delta (P7-T5)

Timestamp: 2026-07-09T17-56

## Baseline (pre-change)
- TaskTree.dll had **no test project**. Baseline line coverage of TaskTree.dll = **0%**
  (no TaskTree.Test assembly existed; the project was untested).

## Post-change
- TaskTree.dll line coverage: **94.04%** (>= 80% assembly floor for the testable denominator).
- New / refactored production files (>= 90% requirement for new modules):
  - TaskTree/TaskTreeController.cs: **95.65% (66/69)** — refactored partial.
  - TaskTree/TaskTreeController.MoveLogic.cs: **93.29% (139/149)** — new file.
  - TaskTree/ITaskTreeForm.cs — interface only (no executable lines; not in coverage denominator).
  - TaskTree/TreeListViewVisual.cs — `[ExcludeFromCodeCoverage]` (E2, COM/WinForms wrapper).

## Changed-line regression
- No changed line lost coverage: TaskTree.dll had 0% coverage prior, so every covered line is a net gain.
  There is no prior-covered line whose coverage was reduced.

## Conclusion
- Assembly floor (>= 80%): PASS (94.04%).
- New-file floor (>= 90%): PASS (controller 95.65%, move-logic 93.29%).
- No-regression-on-changed-lines: PASS (no prior coverage to regress).
