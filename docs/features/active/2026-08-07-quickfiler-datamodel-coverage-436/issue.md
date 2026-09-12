# quickfiler-datamodel-coverage (Issue #436)

- Date captured: 2026-08-07
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/quickfiler-datamodel-coverage/ (Issue #436)
- Parent epic issue: [#136](https://github.com/drmoisan/TaskMaster/issues/136)
- Epic: `quickfiler-per-file-coverage` (child F5, wave 1)
- Integration branch: `epic/quickfiler-per-file-coverage-integration`
- Upstream dependency: F1 `quickfiler-coverage-denominator-and-exemption-ledger`

- Issue: #436
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/436
- Last Updated: 2026-08-08
- Work Mode: full-feature

## Problem / Why

Parent epic issue #136 requires every production `.cs` file compiled by `QuickFiler/QuickFiler.csproj`
to reach at least 80% line coverage or to sit on an explicitly ratified exemption ledger. This child
covers the QuickFiler data-model cluster: the `QfcDatamodel` partial-class family, the `EfcDataModel`
class, and the `IQfcDatamodel` contract.

The cluster is the queue/data backbone consumed by the QuickFiler home controller and the collection
controller. It currently blocks the epic goal for two reasons:

1. `QuickFiler/Controllers/QfcDatamodel.cs` carries `[ExcludeFromCodeCoverage]` at line 25. Per the
   epic's ratified policy reconciliation, that attribute is unratified until F1's ledger either
   justifies it against the irreducible-remainder standard or marks it for removal. The qualifier
   "without an injectable seam" in the `CLAUDE.md` § UT2 exemption is a live obligation, not a
   standing permission.
2. Existing tests (`QfcDatamodelTests.cs`, `QfcDatamodelLivenessTests.cs`, `EfcDataModelTests.cs`)
   provide partial coverage, but per-file line coverage has never been measured for these files, so
   the genuine gaps are unknown.

## Proposed Behavior

Raise per-file line coverage to at least 80% for every file in the cluster that F1's ledger
classifies as `testable`, using injectable seams rather than exemptions, with no behavior change to
observable QuickFiler flows.

Scope (5 files, ~1,283 lines, as of `origin/main` at 74be1964):

| File | Lines | `[ExcludeFromCodeCoverage]` |
| --- | --- | --- |
| `QuickFiler/Controllers/QfcDatamodel.cs` | 496 | yes |
| `QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs` | 177 | no |
| `QuickFiler/Controllers/QfcDatamodel.FrameBuilding.cs` | 154 | no |
| `QuickFiler/Controllers/EfcDataModel.cs` | 397 | no |
| `QuickFiler/Interfaces/IQfcDatamodel.cs` | 59 | no |

Tests live in `QuickFiler.Test/Controllers/`, mirroring the production tree.

Per the #136 per-file mandate, research and planning proceed one production file at a time: a
separate research artifact per production file, a separate atomic-plan phase per production file, and
each individual test case as its own atomic task.

## Acceptance Criteria (early draft)

- [ ] Every `testable` file in the scope table reaches at least 80% line coverage, verified with F1's
      per-file coverage harness and recorded as numeric evidence under `<FEATURE>/evidence/qa-gates/`.
- [ ] `QfcDatamodel.cs` has its `[ExcludeFromCodeCoverage]` removed and reaches the floor via seam
      extraction, unless F1's ledger ratifies a specific irreducible remainder.
- [ ] No production file in scope exceeds 500 lines.
- [ ] Tests use MSTest, Moq, and FluentAssertions; deterministic, isolated, no temporary files, no
      external services, no live forms.
- [ ] Coverage per file spans the positive path plus invalid-input, boundary, and error-handling
      behavior.
- [ ] The full C# toolchain passes in final form.
- [ ] No behavior change to observable QuickFiler flows.

## Constraints & Risks

- **Cross-child contract.** `IQfcDatamodel` is consumed by the home controller (sibling F7) at
  `QfcHomeController.cs:163` and `:173`, and — reaching the contract indirectly through
  `IQfcHomeController.DataModel` — by `QfcQueue.cs:476` (sibling F2) and
  `QfcFormController.EventHandlers.cs:196` (sibling F6). Seam introduction must be additive; the
  public shape those consumers rely on must not change. An unavoidable breaking change is recorded in
  `spec.md` as a cross-child contract note rather than by editing sibling files.
  **Correction (research-verified 2026-08-08):** an earlier draft of this section named the collection
  controller (sibling F11) as a consumer. That is false — `QfcCollectionController.cs` (2,349 lines)
  contains zero matches for `DataModel|Datamodel|_datamodel`. F2 and F6 are the real additional
  consumers. See `research/2026-08-08T00-43-iqfcdatamodel.md`.
- **Second cross-child contract.** `SortOptionsEnum` is declared in this child's
  `IQfcDatamodel.cs`, interpreted only by `EmailSorter.cs` (sibling F2), and consumed only from this
  child's `FrameBuilding.cs:114`. It is a contract with F2, not with F7 or F11.
- **Concurrency and ordering.** `QfcDatamodel.QueueProcessing.cs` carries concurrency and ordering
  invariants. Tests must use an injected clock and fake timers; `Thread.Sleep`, `Task.Delay`, and real
  wall-clock waits are prohibited. The established seam is `System.TimeProvider` with
  `FakeTimeProvider` (`Microsoft.Bcl.TimeProvider`); the repo has no `IClock` and no new clock
  abstraction is to be introduced.
- **Deedle data frames, not WinForms.** `QfcDatamodel.FrameBuilding.cs` builds
  `Deedle.Frame<int, string>` data frames. Seam hierarchy is interface seam, then injectable delegate,
  then adapter.
  **Correction (research-verified 2026-08-08):** an earlier draft of this section stated that this
  file "interacts with WinForms layout". That is false — the file contains zero `System.Windows.Forms`
  references and no WinForms type; "Frame" denotes `Deedle.Frame`. Consequently the STA last-resort
  clause does **not** apply anywhere in this child, and no `*.StaTests.cs` file is introduced. See
  `research/2026-08-08T00-43-qfcdatamodel-framebuilding.md`.
- **COM binding.** The cluster is bound to `Microsoft.Office.Interop.Outlook` types (`MailItem`).
  Seams must isolate logic from live Outlook objects.
- **File-size boundary.** `QfcDatamodel.cs` at 496 lines is 4 lines below the 500-line limit. Any
  growth requires a split into the existing partial family.
- **Shared files.** This child must not modify `coverage.config` or any shared build property file;
  those belong to F1.
- **Upstream timing.** F1 is prepared concurrently, so its ledger and harness do not exist on disk at
  planning time. The plan consumes F1's contract as defined in epic.md "Shared Design" section 6.

## Test Conditions to Consider

- [ ] Unit coverage areas: queue admission and dequeue ordering, batch/frame assembly, sort-option
      handling (`SortOptionsEnum`), undo-move stack behavior, cleanup and completion state.
- [ ] Negative and boundary scenarios: zero/negative batch size, timeout expiry, cancellation,
      empty queue, null or invalid inputs.
- [ ] State transitions: `Complete` flag lifecycle, queue drain, cancellation token propagation.
- [ ] Determinism: injected clock and fake timers for all timing-dependent paths.

## Next Step

- [ ] Promote to GitHub issue (feature request template)
- [ ] Create `docs/features/active/quickfiler-datamodel-coverage/` folder from the template
