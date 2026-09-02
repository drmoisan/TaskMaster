# 2026-08-26-qfc-remove-stackmoveditems-parameter — Spec

- **Issue:** #629
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-08-30
- **Status:** Draft
- **Version:** 0.2
- **Work Mode:** full-feature

## Overview

`IQfcCollectionController.MoveEmailsAsync(SloStack<IMovedMailInfo> stackMovedItems)` declares a parameter
its implementation does not use to populate the undo stack. Issue #469 defect 4 established that the
undo record is actually written by `EmailFiler.PushToUndoStack`
(`UtilitiesCS/EmailIntelligence/EmailParsingSorting/EmailFiler.cs:185-189`) onto the same global
`SloStack` instance the caller already holds a reference to — the parameter is redundant, not wrong.
Issue #468's branch documented this and added an explicit `_ = stackMovedItems;` discard rather than
removing the parameter, because removal reaches `QuickFiler/Controllers/QfcFormController.EventHandlers.cs`,
a file outside that feature's owned set. This feature removes the now-documented-redundant parameter.

- Target users/personas: repository maintainers reading/maintaining `QfcCollectionController` and its
  interface; no end-user-facing behavior changes.
- Success metric: the interface and implementation compile with zero parameters on `MoveEmailsAsync`,
  the sole call site is updated, and no test regression occurs.

## Behavior

- Main flow: `QfcFormController.EventHandlers.cs` calls `_groups.MoveEmailsAsync()` with no arguments;
  `QfcCollectionController.MoveEmailsAsync()` runs exactly as it does today (undo-stack population is
  unaffected, since it never depended on the removed parameter).
- Edge flows: none — this is a pure signature simplification with no behavioral branch depending on
  the removed parameter's value.
- Error handling: unchanged. No new failure mode is introduced or removed.

## Inputs / Outputs

- Inputs: none added or removed at the behavioral level; the C# method signature loses one parameter.
- Outputs: unchanged.
- Config keys: none.
- Backward compatibility: `IQfcCollectionController` is an internal, first-party interface with a
  single production implementation and a single production call site (both in this repo); this is not
  a published/public API, so the signature change is not a breaking change for any external consumer.

## API / CLI Surface

- `Task MoveEmailsAsync(SloStack<IMovedMailInfo> stackMovedItems)` → `Task MoveEmailsAsync()` on both
  `IQfcCollectionController` (`QuickFiler/Interfaces/IQfcCollectionController.cs:63`) and
  `QfcCollectionController` (`QuickFiler/Controllers/QfcCollectionController.cs:2253`).
- Sole call site: `QuickFiler/Controllers/QfcFormController.EventHandlers.cs:225` —
  `await _groups.MoveEmailsAsync(_movedItems);` → `await _groups.MoveEmailsAsync();`.

## Data & State

- No data flow, storage, or state changes. The undo stack continues to be populated exactly as it is
  today, by `EmailFiler.PushToUndoStack`, independent of this parameter.

## Constraints & Risks

- **Risk:** a test double (`Mock<IQfcCollectionController>`) somewhere in `QuickFiler.Test` sets up the
  old signature and would fail to compile or silently stop matching after the change.
  **Mitigation:** grep `QuickFiler.Test` for `MoveEmailsAsync` and `Mock<IQfcCollectionController>`
  before editing; update every matching `Setup`/`Verify` call.
- **Risk:** `MoveEmailsAsync_WithNullStack_BehavesIdenticallyToAnEmptyStack`
  (`QuickFiler.Test/Controllers/QfcCollectionControllerDefects468MoveTests.cs`) exercises exactly the
  argument shape being removed.
  **Mitigation:** retire or rewrite that test so it no longer asserts on an argument that no longer
  exists; confirm the underlying behavior it was pinning (safe operation regardless of undo-stack
  state) is still covered another way, or explicitly document why it is not needed.
- No security, performance, or rollout risk — this is a same-process signature simplification with an
  unchanged call graph.

## Implementation Strategy

- Implementation scope: `QuickFiler/Interfaces/IQfcCollectionController.cs`,
  `QuickFiler/Controllers/QfcCollectionController.cs`,
  `QuickFiler/Controllers/QfcFormController.EventHandlers.cs`, and any `QuickFiler.Test` file whose
  mocks/tests reference the removed parameter.
- No new classes/functions. No dependency changes. No logging/telemetry changes.
- Rollout: standard PR merge to `main`; no feature flag, no staged rollout — this is an internal
  signature cleanup.

## Definition of Done

- [x] Acceptance criteria documented and mapped to tests (see `## Acceptance Criteria` below)
- [ ] Behavior matches acceptance criteria (verified in the atomic plan's QA gates)
- [ ] Tests updated/added
- [ ] Edge cases covered (mock/setup sweep)
- [x] Docs updated (this spec; issue.md already documents the prior deferral decision)
- [ ] Telemetry/logging: not applicable
- [ ] Toolchain pass completed (CSharpier → analyzers → nullable → MSTest)

## Acceptance Criteria

- [ ] AC1. `IQfcCollectionController.MoveEmailsAsync` declares zero parameters.
- [ ] AC2. `QfcCollectionController.MoveEmailsAsync` declares zero parameters and its body contains no
  `stackMovedItems` reference (the `_ = stackMovedItems;` discard at `:2260` is removed along with the
  parameter), while its XML doc comment continues to state how the undo stack is actually populated
  (via `EmailFiler.PushToUndoStack` onto the shared `SloStack` instance).
- [ ] AC3. The sole call site (`QfcFormController.EventHandlers.cs:225`) invokes
  `await _groups.MoveEmailsAsync();` with no argument.
- [ ] AC4. No `QuickFiler.Test` file contains a `Mock<IQfcCollectionController>` `Setup`/`Verify` that
  still names the old single-parameter overload.
- [ ] AC5. `MoveEmailsAsync_WithNullStack_BehavesIdenticallyToAnEmptyStack` is retired or rewritten to
  no longer assert on the removed parameter's shape, with the disposition (retire vs. rewrite) recorded
  and justified.
- [ ] AC6. The full `QuickFiler.Test` suite passes with no regression, including an assertion that
  undo-after-batch-move is still exercised by at least one surviving test.
- [ ] AC7. A single clean toolchain pass completes in order: `dotnet tool run csharpier check .`,
  the analyzer build, the nullable build, and the full MSTest suite with coverage — each exiting 0.
- [ ] AC8. The diff touches only the files listed under "Implementation Strategy" above, plus this
  feature folder's own evidence and documentation.
