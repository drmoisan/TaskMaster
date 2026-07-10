# Feature Acceptance Audit — #297 TaskVisualization Core Testability Refactor (Remediation Pass 1 Re-Audit)

- Timestamp: 2026-07-10T00-43
- Branch: `feature/taskvisualization-core-testability-refactor-297` (head `8587ae92`)
- Base: `epic/winforms-testability-refactor-integration` (merge-base `3f04d50f`)
- Work mode: `full-feature`. AC sources: `spec.md` (Definition of Done + Seeded Test Conditions). `user-story.md` is intentionally absent for this refactor child, documented in `spec.md` (User Story Applicability).

## Scope and Baseline

Baseline is the merge-base `3f04d50f` on `epic/winforms-testability-refactor-integration`. The feature decomposes the 1861-line `TaskController.cs` into partial classes plus extracted pure-logic helpers, introduces the `ITaskViewer` / `ITaskViewerControls` / `ITagPromptService` seams, and adds MSTest + STA coverage. This re-audit confirms the remediation delta (the `setActiveTaskSubject` seam and two tests) and that all spec acceptance criteria remain satisfied.

## Acceptance Criteria Inventory

Fourteen acceptance criteria from `spec.md`: ten Definition-of-Done items (DoD-1..DoD-10) and four Seeded Test Conditions (STC-1..STC-4).

Definition of Done:
- DoD-1: Structure matches spec; `TaskController.cs` decomposed so no in-scope production file exceeds 500 lines.
- DoD-2: `ITaskViewer` exists, derives from `IForm`, `TaskViewer` implements it; `TaskController` depends on `ITaskViewer`, not the concrete form.
- DoD-3: Host-neutral logic separated from COM/WinForms; class-level `[ExcludeFromCodeCoverage]` on `TaskController` removed.
- DoD-4: No unit test constructs a live form/window or triggers a popup; seams injected.
- DoD-5: Refactored core >= 80% line; new helpers >= 90%; exemption inventory listed.
- DoD-6: Control-identity regions measured via STA last-resort; no file-level exemption on `ControlMaps.cs` / measured `Accelerator.cs`; only PostMessage/handle/focus residue exempt at method/branch level; no Form-derived construction.
- DoD-7: Edge cases and error handling verified (positive/negative/edge per unit).
- DoD-8: Tests, linting, type checks clean.
- DoD-9: Docs updated (spec/plan/epic manifest).
- DoD-10: Full C# toolchain pass (format -> lint -> type-check -> test) with no regression.

Seeded Test Conditions:
- STC-1: Business-logic units covered with pure inputs.
- STC-2: Dialog-driven paths covered via seams intercepting MessageBox/input dialogs.
- STC-3: Event-handler logic covered via a mocked `ITaskViewer`.
- STC-4: Outlook Interop boundaries mocked behind seams.

## Acceptance Criteria Evaluation

| AC | Verdict | Evidence |
|---|---|---|
| DoD-1 | PASS | Independent `wc -l` count: all in-scope production files <= 500; `TaskController.Accelerator.cs` exactly 500 (at limit, untouched by remediation). See policy-audit Section 5. |
| DoD-2 | PASS | `ITaskViewer.cs:22` `interface ITaskViewer : IForm`; both `TaskController` constructors take `ITaskViewer formInstance` (`TaskController.cs:33`, `:91`). |
| DoD-3 | PASS | No class-level exemption on the `TaskController` partials; grep of `TaskController.cs`/`.Actions.cs` shows no `[ExcludeFromCodeCoverage]`. Helpers `TaskDurationParser`/`TaskPriorityMapper` are host-neutral. |
| DoD-4 | PASS | New tests use `MoqTaskViewer` (Moq-backed `ITaskViewer`, `InvokeRequired => false`, non-`Form`); no `ShowDialog`/`Show`/`MessageBox` popup; the notifier and `setActiveTaskSubject` seams intercept COM/dialog writes. |
| DoD-5 | PASS | Refactored-core aggregate 88.95% line (>= 80%); new helpers 100% line (>= 90%); exemption inventory recorded in `evidence/other/exemption-inventory.2026-07-10T00-01.md`. Remediation raised the changed partial to 98.39% line / 91.30% branch. |
| DoD-6 | PASS | STA tests present (`*.StaTests.cs`, `[STATestClass]`/`[STATestMethod]`); `ControlMaps.cs` measured 96.05%, `ControlRelationships.cs` 99.28%; only Accelerator PostMessage/handle/focus residue exempt at method level (`Accelerator.cs` lines 45/60/284/292/301). No Form-derived construction in tests. |
| DoD-7 | PASS | Positive/negative/edge tests present across `TaskControllerActionsTests`, `TaskDurationParserTests`, `TaskPriorityMapperTests`, `TaskControllerFlagsTests`; remediation added the `SetFlag(Taskname)` and `Shortcut_ReadingNews` positive paths. |
| DoD-8 | PASS | csharpier check clean, analyzers 0 errors, nullable/TWAE incremental gate 0 errors (evidence). |
| DoD-9 | PASS | `spec.md` DoD/Seeded boxes checked; plan updated; exemption inventory and remediation evidence added. |
| DoD-10 | PASS | Full toolchain single clean pass recorded; 106/106 tests pass incl. STA (`evidence/qa-gates/remediation-297-setactivetasksubject-seam.2026-07-10T00-36.md`). |
| STC-1 | PASS | `TaskDurationParserTests` / `TaskPriorityMapperTests` exercise pure inputs (100% helper coverage). |
| STC-2 | PASS | Assign dialog paths covered via `Mock<ITagPromptService>`; `CaptureDuration` invalid path via injected notifier. |
| STC-3 | PASS | Controller event-handler logic covered through `Mock<ITaskViewer>` (`MoqTaskViewer`), including the two remediation tests. |
| STC-4 | PASS | Outlook Interop boundaries mocked behind the `ITagPromptService`, notifier, and `Func<MailItem, Task<MailItemHelper>>` factory seams; the `setActiveTaskSubject` seam now also intercepts the get-only `MailItem.TaskSubject` write. |

All 14 acceptance criteria evaluate PASS.

## Acceptance Criteria Check-off

All DoD and Seeded Test Condition checkboxes in `spec.md` are already marked `[x]` (checked by the executor during delivery and re-confirmed here). No checkbox required a state change in this re-audit. The `issue.md` "Acceptance Criteria (early draft)" checkboxes are not the authoritative AC source under `full-feature` mode (superseded by `spec.md`) and are left unchanged; each of those early-draft items is nonetheless satisfied by the corresponding DoD PASS above.

## Acceptance Criteria Status

- Source: `docs/features/active/2026-07-09-taskvisualization-core-testability-refactor-297/spec.md`
- Total AC items: 14
- Checked off (delivered and verified): 14
- Remaining (unchecked): 0
- Items remaining: none

## Summary

The remediation resolves the sole prior Blocking finding via the prescribed `setActiveTaskSubject` seam and two new tests, with no regression to any acceptance criterion. All 14 spec ACs are PASS. #298 contract interfaces (`ITaskViewer`, `ITaskViewerControls`, `ITagPromptService`) are present and unchanged by the remediation. The feature is acceptance-complete relative to baseline.
