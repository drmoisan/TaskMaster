# coverage-gaps-test-seams (Issue #236)

- Date captured: 2026-07-04
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/2026-07-04-coverage-gaps-test-seams-236/ (Issue #236)

- Issue: #236
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/236
- Last Updated: 2026-07-04
- Work Mode: full-feature

## Problem / Why

TaskMaster has strong test coverage overall, but several QuickFiler elements still
have uncovered logic because their current implementations couple directly to
Outlook COM, WinForms viewers, static queues, and UI control state. The target
elements are:

- `EfcViewerQueue`
- `ItemViewerQueue`
- `QfcThemeHelper`
- `EfcHomeController`
- `TlpCellStates`

The coverage gap should be closed by adding testable seams or isolating logic
into testable methods. Coverage exemptions are not permitted.

## Proposed Behavior

Refactor the target elements so deterministic unit tests can exercise their
decision logic without launching Outlook, dereferencing live COM objects, or
requiring live WinForms windows. Use narrow seams where external boundaries must
remain, and keep default production behavior compatible with existing callers.

Where seam injection is insufficient, move pure logic into focused methods or
small collaborators so minimal code remains untestable.

## Acceptance Criteria

- [x] AC1 - `EfcViewerQueue` queue creation, cached dequeue, empty dequeue,
      replacement scheduling, cancellation boundaries, and disposal/reset
      behavior are covered by deterministic MSTest tests through a narrow
      factory/dispatcher seam without constructing a live `EfcViewer` in unit
      tests.
- [x] AC2 - `ItemViewerQueue` queue creation, synchronous and dispatched build
      paths, cached dequeue, empty dequeue, chunk dequeue, cancellation
      boundaries, and disposal/reset behavior are covered by deterministic
      MSTest tests through a narrow factory/dispatcher seam without constructing
      a live `ItemViewer` in unit tests.
- [x] AC3 - `QfcThemeHelper` theme construction, theme key selection,
      representative color/control-group mapping, `SetupFormThemes`, and direct
      `SetTheme` extension behavior are covered without requiring live
      QuickFiler form instances beyond test-controlled controls or adapters.
- [x] AC4 - `EfcHomeController` construction and initialization decision logic
      is covered through explicit seams for Outlook selection traversal, data
      model creation, viewer dequeue/construction, keyboard handler creation,
      explorer-controller creation, and form-controller creation.
- [x] AC5 - `TlpCellStates` constructors, raw-list conversion behavior,
      duplicate-key behavior, `TryAddState` outcomes, empty input handling, and
      null-input behavior are covered directly. If production behavior changes,
      null inputs fail fast with a specific exception.
- [x] AC6 - Existing public/static production entry points remain
      source-compatible for current callers, including queue APIs,
      `QfcThemeHelper` production overloads, `EfcHomeController` public
      constructor and factory methods, and `TlpCellStates` constructors.
- [x] AC7 - No coverage exemptions are added for `EfcViewerQueue`,
      `ItemViewerQueue`, `QfcThemeHelper`, `EfcHomeController`, or
      `TlpCellStates`, and no coverage configuration is weakened.
- [ ] AC8 - Repository-wide line coverage remains at or above 80%, and changed
      or newly introduced non-exempt code for issue #236 meets the repository
      policy target of at least 90% coverage.
- [x] AC9 - All implementation evidence, including baselines, QA gates,
      regression results, and coverage artifacts, is stored under
      `docs/features/active/2026-07-04-coverage-gaps-test-seams-236/evidence/<kind>/`.
- [x] AC10 - The final C# toolchain passes in order: CSharpier, .NET analyzers,
      nullable analysis with warnings as errors, and MSTest with code coverage.

## Constraints & Risks

- Preserve existing production behavior for Outlook and QuickFiler workflows.
- Do not add coverage exemptions or weaken coverage policy.
- Keep seams narrow and local to COM, WinForms, and static-construction
  boundaries.
- Avoid broad refactors outside the named coverage targets unless required to
  expose a testable boundary.
- Unit tests must remain deterministic and must not depend on Outlook, external
  services, mutable machine state, or temporary files.
- Tests that use any temporary static override seam must restore defaults in
  cleanup and must prevent parallel interference when required by repository
  test settings.

## Test Conditions to Consider

- [ ] Queue creation and dequeue behavior with injected factories or providers.
- [ ] Cancellation and disposal behavior around viewer acquisition.
- [ ] Theme group construction with representative buttons and panels.
- [ ] `TlpCellStates` constructors from both typed snapshot lists and raw
      snapshot lists.
- [ ] `EfcHomeController` paths that can be covered through injected
      collaborators rather than live COM or live forms.
- [ ] Full MSTest run with code coverage enabled.

## Next Step

- [ ] Complete atomic planning for issue #236 using `spec.md`,
      `user-story.md`, and the research artifact
      `artifacts/research/2026-07-04T13-19-issue-236-coverage-gaps-test-seams-research.md`.
