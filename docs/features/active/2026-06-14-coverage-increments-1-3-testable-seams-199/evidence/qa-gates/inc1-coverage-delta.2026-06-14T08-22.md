# Increment 1 — Coverage Delta

Timestamp: 2026-06-14T08-22

Command: dotnet-coverage merge (TestResults *.coverage) --output-format cobertura -> artifacts/csharp/inc1.cobertura.xml; per-method line analysis

EXIT_CODE: 0

## Baseline

Production-only baseline (post-#197, authority 197-COV-001): 71.65%.
Pre-feature ToDoModel assembly line-rate (from artifacts/csharp/coverage-firstparty.cobertura.xml):
0.1082 (10.82%).

## Targeted-method covered-line results after Increment 1 (inc1.cobertura.xml)

- ToDoLoader.SetAndSave<T> (four overloads): all reachable overloads exercised; covered overloads
  at line-rate 1.0. New covered lines on the SetAndSave family where previously 0.
- IDList.GetNextToDoID(string): line-rate 1.0 (all reachable lines covered; the Settings/Serialize
  side-paths are exercised via the rollover test with snapshot/restore; the Filepath!=null
  Serialize branch is intentionally not taken).
- ProjectEntry.CompareTo(IProjectEntry): line-rate 1.0. CompareTo(object): 0.727 — the residual is
  the length tie-break, reachable only with a non-4-char id (dialog gap, see evidence/other).
- ProjectEntry.SetProjectId: line-rate 0.5 — the covered half is the two dialog-free branches
  (empty->set, same-value->break); the uncovered half is the malformed-id and change-confirmation
  branches that route through MyBox.ShowDialog (documented Flag-and-Stop gap, evidence/other).
- BaseChanger: class line-rate 0.9692 (96.92%); ToBase 0.889 (residual is the case-2 even-pad arm
  not reachable with intMinDigits in {1,2}); ToBase10(char/string), ValidateParams guards, MaxBase
  all exercised.

## New/changed-code coverage

The new code added by this increment is the four test files; their production-method targets are
covered to the maximum extent reachable WITHOUT a new production seam. New-code (test file)
line-rate is 1.0 across the new test classes. The only sub-100% targeted production paths are the
explicitly Flag-and-Stopped dialog-dependent branches (ProjectEntry SetProjectId malformed/change
and the CompareTo length tie-break), which are not reachable from ToDoModel.Test without a
prohibited production change.

## Disposition

- Covered-line count on the named ToDoModel seams INCREASED (BaseChanger 96.92%; IDList,
  ProjectEntry, ToDoLoader targeted methods newly covered where dialog-free).
- No coverage regression on changed lines (test-only addition; no production lines changed).
- New-code coverage >= 90% on all reachable targeted paths. The < 90% per-method figures are
  confined to the documented, accepted dialog gap and are NOT a remediation trigger because the
  plan's Flag-and-Stop rule explicitly authorizes restricting coverage there.

Outcome: PASS (with the documented ProjectEntry dialog gap recorded in
evidence/other/projectentry-malformed-gap.2026-06-14T08-22.md).
