# `quickfiler-datamodel-coverage` — User Story

- Issue: [#436](https://github.com/drmoisan/TaskMaster/issues/436)
- Parent epic issue: [#136](https://github.com/drmoisan/TaskMaster/issues/136) (`quickfiler-per-file-coverage`, child F5, wave 1)
- Owner: drmoisan
- Status: Ready for planning
- Last Updated: 2026-08-08
- Work Mode: `full-feature` — `spec.md` and `user-story.md` are the authoritative acceptance-criteria sources.
- Companion: `spec.md` (engineering contract; section references below point there)

## Story Statement

- As the **maintainer of TaskMaster**, I want the QuickFiler data-model cluster covered per file at 80% or
  above with deterministic tests, so that a regression in the queue and data backbone fails a test instead
  of reaching a user's mailbox.
- As the **maintainer**, I want `[ExcludeFromCodeCoverage]` removed from
  `QuickFiler/Controllers/QfcDatamodel.cs` and replaced by injectable seams, so that the largest blind spot
  in this cluster stops being invisible to the coverage gate.
- As an **autonomous coding agent working in this repository**, I want the queue, frame-building, and filing
  paths pinned by tests that need no live Outlook, no modal dialog, and no wall-clock wait, so that I can
  change this code and get a trustworthy pass/fail signal in a single local toolchain run.
- As a **QuickFiler user**, I want nothing about filing behavior to change, so that this work is invisible
  to me.

## Problem / Why

Parent epic issue #136 requires every production `.cs` file compiled by `QuickFiler/QuickFiler.csproj` to
reach at least 80% line coverage or to sit on an explicitly ratified exemption ledger. This child covers the
QuickFiler data-model cluster: the `QfcDatamodel` partial-class family, the `EfcDataModel` class, and the
`IQfcDatamodel` contract — five files, 1,283 lines (verified on disk).

The cluster is the queue and data backbone of the QuickFiler filing loop. It admits mail into the master
queue, builds and shapes the Deedle data frame the UI iterates over, gates dequeue by classifier confidence,
and executes the actual folder move. Two conditions make it the epic's highest-value blind spot:

1. **A single attribute hides three files.** `QuickFiler/Controllers/QfcDatamodel.cs` carries
   `[ExcludeFromCodeCoverage]` at line 25. The attribute is **type-scoped**, so it removes all three
   `QfcDatamodel` partials (496 + 177 + 154 lines) from measurement at once. Their measured coverage today
   is not a low number — the files are **absent** from the coverage report entirely, so no gate can see them
   and no trend can flag them. Under the epic's ratified reconciliation, that attribute is unratified until
   it is justified against the irreducible-remainder standard or removed.
2. **The fourth file is measurably far short.** `EfcDataModel.cs` is measured at `line-rate="0.55618"` and
   `branch-rate="0.457143"` in the committed Cobertura report, with 126 uncovered lines concentrated in the
   folder-prediction, folder-move, and folder-open paths — the parts that actually move a user's mail.

There is a third, quieter problem. Existing coverage is partly **mislabelled**: five tests named
`TryQueueRemainingMailItemAsync_*` in `QfcDatamodelTests.cs` do not touch `QfcDatamodel` at all — they
construct `QfcRemainingQueueAdmission` directly. The datamodel method of that name is genuinely untested. A
maintainer reading the test file today would reasonably conclude the opposite.

The business outcome the epic states is directly served here: fewer regression escapes in QuickFiler, and a
project that is safe for autonomous agentic maintenance. Coverage is the mechanism, but the goal is trust —
an agent (or a person) changing dequeue ordering or the frame pipeline should learn immediately whether the
change was safe.

## Personas & Scenarios

### Persona — Dan, maintainer of TaskMaster

- **Who:** the sole maintainer, working primarily through delegated coding agents.
- **Cares about:** not shipping a filing regression into his own live mailbox; being able to accept an
  agent's change without reading every line of it.
- **Constraints:** the code is VSTO/COM-bound and WinForms-adjacent, so naive tests either need a live
  Outlook or pop modal dialogs; the long-term direction is away from VSTO, so effort spent on host-neutral
  logic keeps its value and effort spent on host-bound scaffolding does not.
- **Frustration today:** the three most important files in this cluster produce no coverage number at all,
  so "coverage is fine" and "this code is untested" are indistinguishable from the report.

### Persona — an autonomous coding agent assigned to a QuickFiler change

- **Who:** a delegated agent with a change budget and a mandatory toolchain loop.
- **Cares about:** a deterministic, fast, local pass/fail signal.
- **Constraints:** may not create temporary files, may not depend on external services, may not use
  wall-clock waits, and must never produce a test that requires a human to dismiss a dialog.
- **Frustration today:** touching `QfcDatamodel` gives no test feedback, so the only verification available
  is manual inspection.

### Scenario A — a dequeue-ordering change

An agent is asked to adjust how the confidence gate scans candidates. It edits `QueueProcessing.cs` and runs
the toolchain. Today: the file is outside the denominator, few of its ordering invariants are pinned, and the
suite passes whether or not the change silently reordered the batch. After this feature: the delivered tests
pin the FIFO prefix, the accept/reject/discard behavior, the 12-second first-batch deadline, the exact 200 ms
poll interval, the progress-sink contract, and cancellation mid-scan — all driven by `FakeTimeProvider`, so
the suite fails in milliseconds rather than passing by luck.

### Scenario B — a change to the frame pipeline

An agent reorders the frame-shaping steps in `FrameBuilding.cs` while cleaning up duplication. Today nothing
distinguishes *filter → dedup → sort* from *dedup → filter*, and the difference is user-visible: the wrong
order can keep a calendar item as a conversation's representative row and drop the mail the user expected to
see. After this feature, one test exists specifically to distinguish those two orders.

### Scenario C — the maintainer reviews an agent's PR

Dan opens a PR touching `EfcDataModel.MoveToFolderAsync`. Today he must reason manually about whether the
`EmailFilerConfig` is still built correctly and whether the `"Trash to Delete"` attachment suppression still
applies. After this feature, those are assertions, and the per-file coverage evidence committed under
`evidence/qa-gates/` gives him a number for the exact file that changed rather than an assembly average.

## Observable-behavior guarantee

**This feature changes no QuickFiler behavior that any user can observe.** That is AC7, and it is a hard
constraint that shapes several decisions rather than a hope:

- **The public contract is untouched.** `QuickFiler/Interfaces/IQfcDatamodel.cs` receives **zero production
  edits**. All nine interface members keep byte-identical signatures. `SortOptionsEnum` is unchanged,
  including `Default = 42`. Every verified consumer call site — F7's `QfcHomeController`, F2's
  `QfcQueue.cs:476`, F6's `QfcFormController.EventHandlers.cs:196` — compiles and behaves unchanged
  (`spec.md` §4).
- **Every seam is additive and defaults to production.** Each is an `internal` member, an additive `internal`
  constructor, or an additive `internal static` overload on a concrete class, with a null-means-production
  default. Nothing a user exercises takes a different path.
- **Known defects are pinned, not fixed.** Twelve latent defects and observations were found during research
  — a non-idempotent `Cleanup()`, a null-vs-empty return asymmetry that can NRE the iteration loop, mail
  items that leave the queue still hooked to the move monitor, a stack-trace-resetting rethrow, and others.
  Fixing any of them would change observable behavior, so every one is promoted to its own GitHub issue and
  the tests **characterize current behavior** instead (`spec.md` §12). A future fix then becomes a
  deliberate, visible change with a test that must be updated on purpose.
- **Deleted code is verified dead.** ~123 lines are removed from `QfcDatamodel.cs`: an unused logger field,
  an empty region, and three members whose only remaining references are commented-out subscriptions and
  `nameof(...)` uses inside their own bodies. Two of them exist only to show `MessageBox` dialogs that no
  live path reaches. Their removal also retires a `CS0618` suppression.
- **No new dialogs, no new threads, no new clocks.** No `*.StaTests.cs` file is introduced anywhere in this
  child, and no new clock abstraction is added — `System.TimeProvider` with `FakeTimeProvider` is the
  established seam.

## Non-Goals

- Fixing any defect in the `spec.md` §12 register. They are promoted to issues, not repaired here.
- Editing any sibling-owned file, `UtilitiesCS`, or `coverage.config`.
- Changing the repository-wide coverage thresholds, or the `IQfcDatamodel` / `SortOptionsEnum` contracts.
- Converting QuickFiler away from VSTO/WinForms. Where a seam choice is open this feature prefers
  host-neutral extraction that a future WebView2/Office.js port can reuse, but the port itself is a separate
  effort.
- Coverage work on `QuickFiler/Legacy/**` or `QuickFiler/Notes/**` — neither is compiled.
- Adding `[ExcludeFromCodeCoverage]` anywhere, including to the declaration-only `IQfcDatamodel.cs`, where it
  would exclude nothing.

## Acceptance Criteria

- [ ] **AC1 — Per-file coverage floor.** Every file in the §2 scope table that F1's ledger classifies as
      `testable` reaches at least 80% line coverage, verified with F1's per-file coverage harness and
      recorded as numeric per-file evidence under
      `docs/features/active/2026-08-07-quickfiler-datamodel-coverage-436/evidence/qa-gates/`. Projected or
      read-derived figures are not acceptable evidence and must be replaced by harness output before this
      criterion is checked off. `QuickFiler/Interfaces/IQfcDatamodel.cs` is outside this numeric gate only
      if F1's ledger classifies it `not-measurable (declaration-only)` per §5.2; if F1 classifies it
      `testable`, escalate with the measured evidence in §5.2 rather than attempting to comply.
- [ ] **AC2 — Exemption removal.** `[ExcludeFromCodeCoverage]` at `QuickFiler/Controllers/QfcDatamodel.cs:25`
      is removed, and all three `QfcDatamodel` partials it currently suppresses reach the 80% floor through
      seam extraction rather than exemption — unless F1's ledger ratifies a specific irreducible remainder,
      in which case that remainder is recorded member-level alongside the ledger entry ratifying it. The
      removal is the last production task of the feature (§7). No new `[ExcludeFromCodeCoverage]` is added
      to any file in scope.
- [ ] **AC3 — File size.** No production file in scope exceeds 500 lines after the change, including every
      new production file this feature adds (`QfcDatamodel.Construction.cs`, and `QfcEmailFrameShaper.cs` /
      `EfcDataModel.Seams.cs` if taken).
- [ ] **AC4 — Test conventions and determinism.** All new and modified tests use MSTest, Moq, and
      FluentAssertions with Arrange–Act–Assert, and are independent, isolated, and deterministic: timing is
      driven exclusively by `System.TimeProvider` / `FakeTimeProvider`, with no `Thread.Sleep`, no
      `Task.Delay`, no real wall-clock wait, no temporary file, no external service or process, no live
      form, no modal dialog, and no STA-apartment test.
- [ ] **AC5 — Scenario completeness.** For each file in scope, the delivered tests span the positive path
      plus invalid-input, boundary, and error-handling behavior, and state-transition and ordering behavior
      where the file exhibits such behavior.
- [ ] **AC6 — Toolchain.** The full C# toolchain passes in its final form, in order and with no intervening
      file change: `csharpier .`; the analyzer msbuild; the nullable msbuild; and `vstest.console.exe` with
      `/EnableCodeCoverage`.
- [ ] **AC7 — No behavior change.** No observable QuickFiler flow changes.
      `QuickFiler/Interfaces/IQfcDatamodel.cs` receives zero production edits, `SortOptionsEnum` is unchanged
      including `Default = 42`, all nine `IQfcDatamodel` members keep byte-identical signatures, and every
      consumer call site in §4.1 compiles and behaves unchanged.
- [ ] **AC8 — Defects promoted, not fixed.** Every latent defect and promote-to-issue observation in the §12
      register is promoted to a GitHub issue through the MCP promotion lifecycle rather than fixed in this
      child, and each resulting issue number is recorded in this spec.
