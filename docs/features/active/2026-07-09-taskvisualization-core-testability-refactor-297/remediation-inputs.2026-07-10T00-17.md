# Remediation Inputs — taskvisualization-core-testability-refactor (#297)

- Timestamp: 2026-07-10T00-17
- Branch: `feature/taskvisualization-core-testability-refactor-297` @ `5f8eea31`
- Source artifacts: `policy-audit.2026-07-10T00-17.md`, `code-review.2026-07-10T00-17.md`, `feature-audit.2026-07-10T00-17.md`
- Blocking findings: 1

## Blocking-1: `SetFlag(Taskname)` / `Shortcut_ReadingNews` left uncovered despite a feasible seam

- Files / locations:
  - `TaskVisualization/TaskController.Actions.cs` — `SetFlag` Taskname case, lines 384-389.
  - `TaskVisualization/TaskController.Actions.cs` — `Shortcut_ReadingNews`, lines 299-306 (calls `SetFlag(..., Taskname)`).
  - `TaskVisualization/TaskController.cs` — `InitializeSeams` (lines 150-161) and both constructors (lines 32-141) for the new seam parameter.
  - `TaskVisualization.Test/TaskControllerActionsTests.cs` — add the two tests (currently skipped at lines 75-88 and 185-187).
- Violated rule / criterion:
  - Review-gate adjudication (task 6b): a testable seam left uncovered without a ratified exemption is Blocking.
  - Spec §Coverage Exemption Constraint principle (issue.md line 63): "testable seams are not exempt from the coverage floor" — extended here to "a feasibly-seamable path should not be left uncovered."
  - CLAUDE.md General Unit Test Policy UT2: "untested critical behavior is not acceptable even if the overall percentage looks good" (the methods are model-mutating controller actions, not pure glue).
- Root cause (evidence-first): `_active` is a concrete `readonly ToDoItem` (TaskController.cs:271). `ToDoItem.TaskSubject`'s non-virtual setter routes to Outlook-interop reflection and raises `MissingMethodException` on a Moq proxy, so the write cannot be observed via a mocked `ToDoItem`. ToDoItem is owned by ToDoModel and out of #297 scope to refactor, so the only in-scope observation mechanism is a controller-level write-interception seam.
- Required seam-based fix:
  1. Add an optional-with-default seam parameter `Action<string> setActiveTaskSubject = null` to both `TaskController` constructors (after the existing optional seam params, preserving `FlagTasks.cs` zero-edit compatibility).
  2. In `InitializeSeams`, add `_setActiveTaskSubject = setActiveTaskSubject ?? (v => _active.TaskSubject = v);` (declare `private Action<string> _setActiveTaskSubject;`). Note the default closure captures `_active` by variable, resolved at invocation time — valid because `InitializeSeams` runs before `_active` is assigned in the constructor.
  3. In `SetFlag`'s `Taskname` case replace `_active.TaskSubject = value;` with `_setActiveTaskSubject(value);` (leave `_viewer.TaskNameText = value;` unchanged).
  4. Add `SetFlag_Taskname_WritesSubjectAndFacade` (inject a capturing `Action<string>`; assert captured value and `_viewer.TaskNameText`) and `Shortcut_ReadingNews_SetsAllFlagsAndFocusesDuration` (assert Context/Projects on `_active`, the captured Taskname `"READ: ..."`, the Worktime facade write, and `mock.Verify(v => v.FocusDuration())`).
- Verification after fix: run the full C# toolchain (csharpier -> analyzer -> nullable/TWAE -> vstest with coverage); confirm both new tests pass, Actions.cs coverage rises, and `FlagTasks.cs` remains unchanged (0 lines in diff).

## Non-blocking follow-ups (not required to merge)

- Extract `ApplyActiveFieldsTo(ToDoItem)` from `ApplyChanges` (Flags.cs 51-114) to shrink the whole-method exemption to genuine COM-iteration wiring (candidate for #298).
- Record the branch-coverage percentage in `evidence/qa-gates/coverage-comparison.*.md` (and consider committing a trimmed Cobertura/lcov digest) so the branch >= 75% gate is numerically evidenced for this zero-CI child PR.
