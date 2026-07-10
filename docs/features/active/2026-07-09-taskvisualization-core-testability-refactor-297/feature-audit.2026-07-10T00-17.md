# Feature Acceptance Audit — taskvisualization-core-testability-refactor (#297)

- Timestamp: 2026-07-10T00-17
- Branch: `feature/taskvisualization-core-testability-refactor-297` @ `5f8eea31`
- Work Mode: `full-feature`; AC source: `spec.md` (Definition of Done + Seeded Test Conditions). `user-story.md` intentionally absent per spec §User Story Applicability.

## Scope and Baseline

Baseline = merge-base `3f04d50f` with `epic/winforms-testability-refactor-integration`. Baseline `TaskController.cs` = 1861 lines, class-level `[ExcludeFromCodeCoverage]`, 0.00% measured coverage. This audit evaluates the full branch diff against that baseline. The 14 acceptance criteria below (10 Definition-of-Done + 4 Seeded-Test-Condition) were each independently verified against source, tests, and toolchain evidence.

## Acceptance Criteria Inventory

Definition of Done (spec.md lines 435-455):
1. `TaskController.cs` decomposed; no in-scope production file exceeds 500 lines.
2. `ITaskViewer` exists, derives from `IForm`, `TaskViewer` implements it; `TaskController` depends on `ITaskViewer`, not the concrete form.
3. Host-neutral logic separated; class-level `[ExcludeFromCodeCoverage]` on `TaskController` removed.
4. No unit test constructs a live form/window or triggers a popup; seams injected.
5. Refactored core >= 80% line coverage; new helper classes >= 90%; exemption inventory listed for ratification.
6. Control-identity regions measured via STA last-resort; no file-level exemption on the two partials; only PostMessage/handle/focus residue exempt; no Form-derived type constructed.
7. Edge cases and error handling verified (positive/negative/edge per unit).
8. Tests, linting, type checks clean.
9. Docs updated (spec/plan/epic as needed).
10. Full C# toolchain pass (format -> lint -> type-check -> test) with no regression.

Seeded Test Conditions (spec.md lines 465-468):
11. Business-logic units covered with pure inputs.
12. Dialog-driven paths covered via seams intercepting MessageBox/input dialogs.
13. Event handler logic covered via a mocked `ITaskViewer`.
14. Outlook Interop boundaries mocked behind seams.

## Acceptance Criteria Evaluation

| # | Criterion | Verdict | Evidence |
|---|---|---|---|
| 1 | Decomposed, all in-scope production files <= 500 lines | PASS | Independent count: Accelerator.cs 500, Actions.cs 490, ControlMaps.cs 296, ControlRelationships.cs 259, Flags.cs 181, TaskController.cs 312, helpers 61/60. No hidden oversize file (all changed + existing in-scope `.cs` counted). |
| 2 | `ITaskViewer : IForm`; TaskViewer implements; controller depends on interface | PASS | `ITaskViewer.cs` `: IForm`, primitives-only; `TaskViewer.cs:19 : Form, ITaskViewer, ITaskViewerControls`; `_viewer` typed `ITaskViewer`; concrete cast confined to `Form` accessor + one guarded exempt method. |
| 3 | Host-neutral logic separated; class-level exemption removed | PASS | Helpers extracted; `TaskController.cs:20` = `public partial class TaskController` (no attribute); grep confirms 0 class-level exemptions on controller partials. |
| 4 | No live form/popup in tests; seams injected | PASS | Banned-API scan clean; no `Form`-derived construction; `Mock<ITaskViewer>`, `Mock<ITagPromptService>`, capturing `Action<string>` used. |
| 5 | Core >= 80%, helpers >= 90%, inventory listed | PASS | Refactored core 88.95%; helpers 100%; `exemption-inventory.2026-07-10T00-01.md` lists every exemption with named dependency and reducibility note. |
| 6 | STA last-resort; no file-level exemption on the two partials; only residue exempt; no Form-derived | PASS | 0 file-level exemptions on Accelerator/ControlMaps/ControlRelationships; only 5 method-level residue exemptions (handle/pump/focus) + Flags.ApplyChanges; STA tests use real never-shown controls, disposed. |
| 7 | Edge/error cases verified | PASS | Parser/mapper positive/zero/negative/non-integer/empty/both-directions/unknown-fallback; AreCollectionsEqual null/disjoint/duplicate; Assign cancel-vs-select. |
| 8 | Tests/lint/type-check clean | PASS | csharpier exit 0; analyzer exit 0 (0 errors); nullable/TWAE exit 0. |
| 9 | Docs updated | PASS | spec.md DoD/Seeded checked; plan.md phases checked; evidence artifacts written. |
| 10 | Full toolchain pass, no regression | PASS | Four gates green in a single pass; baseline coverage 0.00% -> 88.95% (no regression). |
| 11 | Business-logic units covered (pure inputs) | PASS | `TaskDurationParserTests`, `TaskPriorityMapperTests`, `TaskControllerFlagsTests`, `TaskControllerMergeTests`. |
| 12 | Dialog paths covered via seams | PASS | `Assign*` tested against `Mock<ITagPromptService>`; `CaptureDuration` notifier asserted. |
| 13 | Event-handler logic via mocked `ITaskViewer` | PASS | `Today_Change`/`Bullpin_Change`/`FlagAsTask_Change`/`Assign_KB`/`Assign_Priority`/`OK_Action`/`Cancel_Action` verified on the mock. |
| 14 | Outlook Interop boundaries mocked behind seams | PASS | `MoqOlToDo` builders; `Func<MailItem,Task<MailItemHelper>>` factory stub for `AutoAssignAllAsync`. |

All 14 acceptance criteria are satisfied (evidence-verified).

## Relationship to the Blocking Finding

The Blocking finding (§6b in policy-audit/code-review) is orthogonal to the 14 spec acceptance criteria: the spec never required coverage of `SetFlag(Taskname)`/`Shortcut_ReadingNews`, and the coverage AC (#5) is met at aggregate (88.95% >= 80%). The Blocking finding is a review-gate testability-completeness item (a feasible seam left uncovered without a ratified exemption), not an unmet spec AC. It is tracked separately in remediation inputs.

## Evidence Gaps (PARTIAL/UNVERIFIED notes)

- Branch coverage (>= 75%): the branch-coverage percentage is not recorded in committed evidence and the raw Cobertura is gitignored; the numeric branch figure could not be confirmed from committed artifacts. Line-coverage evidence is strong (88.95% aggregate, 100% helpers). Recorded as an evidence gap, not an AC failure.

## Summary

- Acceptance criteria: 14/14 PASS (evidence-verified).
- Toolchain: green (csharpier, analyzers, nullable/TWAE, 104/104 MSTest incl. 41 STA).
- File-size, decoupling, exemption-narrowing, coverage-exclusion, STA-discipline, evidence-location: PASS.
- One Blocking review-gate finding (feasible seam left uncovered) — see remediation inputs.
- Overall: NOT READY TO MERGE pending the single seam-based remediation.

### Acceptance Criteria Status
- Source: `docs/features/active/2026-07-09-taskvisualization-core-testability-refactor-297/spec.md`
- Total AC items: 14
- Checked off (delivered): 14
- Remaining (unchecked): 0
- Items remaining: none

## Acceptance Criteria Check-off

All 14 acceptance criteria are already marked `[x]` in `spec.md` (Definition of Done + Seeded Test Conditions) and are confirmed PASS by this audit; no check-off change was required.
