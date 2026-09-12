# `quickfiler-qfc-form-explorer-controller-coverage` — User Story

- Issue: #435
- Parent epic: #136 (`quickfiler-per-file-coverage`), child F6, wave 1
- Owner: drmoisan
- Status: Draft
- Last Updated: 2026-08-08

## Story Statement

- As the **maintainer of TaskMaster**, I want the `QfcFormController` partial-class family and
  `QfcExplorerController` to each meet a measured per-file line-coverage floor, so that a regression
  in QuickFiler's form lifecycle, event dispatch, or Outlook explorer navigation is caught by a
  deterministic test run rather than by a user losing filed mail.
- As an **autonomous agent asked to change QuickFiler**, I want every path in these five executable
  files reachable behind an injectable seam and pinned by an assertion, so that I can judge whether a
  proposed edit is safe from the test suite alone, without a live Outlook session and without a human
  clicking through modal dialogs.
- As the **owner of a sibling child in epic #136**, I want F6 to raise its own coverage without
  editing my files or relocating lines into an excluded file, so that fan-in to the integration
  branch is a clean merge and my child's coverage obligation is not silently enlarged.

## Problem / Why

Child F6 of epic #136 owns 10 compiled files, approximately 1,611 lines in
`QuickFiler/QuickFiler.csproj`. This cluster is the seam between the QuickFiler form, the Outlook
explorer, and the item-collection controller — the code path a user exercises on every filing
session. It does not meet the per-file 80% line-coverage floor mandated by issue #136:

- `QuickFiler/Controllers/QfcExplorerController.cs` (323 lines) carries `[ExcludeFromCodeCoverage]`
  at line 20 and has **no tests at all**. Per the epic's Shared Design section 1, that attribute is
  treated as unratified: the `CLAUDE.md` COM/VSTO exemption qualifier "without an injectable seam" is
  a live obligation, not standing permission. The research found no irreducible member in the file —
  every barrier is a constructor COM call, a direct interop read, or a modal `MessageBox.Show`, and
  each is seamable.
- The four `QfcFormController.*` partials (196 + 232 + 399 + 302 lines) have partial coverage, but
  the event-handler, action, and setup/disposal paths cross the form/viewer boundary and are the
  least reachable. Worse, six existing tests execute production lines while asserting nothing — five
  have an empty Assert section with only a placeholder comment, and one is a suppressed tautology —
  so today's measured line rate **overstates** the assurance a maintainer actually has.
- Actual current per-file coverage is unmeasured on this branch. The only prior figures available
  belong to a different feature's branch artifact and are not this child's baseline.

The maintainer-facing consequence is concrete. Three real defects were found by reading this code
during research and none of them is currently detectable by a test: `UndoConsumer()` spins forever
once its ten-second timeout branch fires, `ExplConvView_Cleanup()` throws `NotImplementedException`,
and `OpenQFItem` re-resolves `ActiveExplorer()` instead of reusing the instance it captured. Code with
no assertions around it does not tell you when it breaks; it tells you nothing at all.

## Personas & Scenarios

### Persona — Dan, repository maintainer

- **Who:** sole maintainer of TaskMaster, a VSTO/WinForms Outlook add-in with a long-term goal of
  migrating off VSTO.
- **What he cares about:** being able to delegate QuickFiler changes to an agent and trust the result
  without manually re-filing a mailbox to check nothing broke.
- **Constraints:** the code is COM- and WinForms-bound; a live Outlook session cannot be part of a
  unit test run; fourteen sibling features are modifying the same assembly concurrently.
- **Goals and frustrations:** he wants the coverage number to mean something. A file at 75% where a
  quarter of the "covered" lines are exercised by assertion-free tests is worse than an honest 0%,
  because it invites false confidence.

### Persona — the delegated implementation agent

- **Who:** an autonomous agent given a QuickFiler bug or feature.
- **What it cares about:** a deterministic signal. It cannot click a dialog, cannot see a form, and
  cannot tell from reading alone whether a `Task.Run` it left running will outlive the test host.
- **Constraints:** the repo's unit-test policy forbids temporary files, external services, live
  forms, popups, and wall-clock waits. Any code path that requires one of those is a path the agent
  cannot verify.

### Scenario — a regression in the disposal ordering invariant

1. An agent is asked to tidy `QfcFormController.Cleanup()` and reorders two statements so that
   `_formViewer` is nulled before `UnregisterFormEventHandlers()` runs.
2. The build succeeds. Every analyzer passes. The change looks harmless.
3. **Today:** the existing `Cleanup_ShouldCleanupResources` test executes the whole method and
   asserts nothing, so the suite stays green. The defect ships. In production the keyboard handlers
   remain subscribed to controls on a disposed form, and the next filing session routes key events
   into a dead controller.
4. **After F6:** a test registers a real in-memory child control, calls `Cleanup()`, raises `KeyDown`
   on that control, and asserts the keyboard handler is never invoked. The reorder turns the suite
   red on the first run, naming the exact invariant that broke.

### Scenario — verifying the undo path without a human in the loop

1. An agent is asked to change how undone moves are re-queued.
2. **Today:** `UndoDialog()` past its guard opens three `MessageBox.Show` dialogs and starts a
   background `UndoConsumer` task that never terminates. There is no way to run it in a test — the
   run would block on the first dialog, and if it did not, it would leak a thread spinning at 100%
   for the rest of the session. So the agent changes the code and verifies nothing.
3. **After F6:** the prompts are behind an injectable seam that a test scripts with `DialogResult`
   values, and the consumer start is behind a delegate the test captures without executing. Nine
   cases pin the decision loop — message-is-null, undo-yes, undo-no, repeat-no, nothing-to-undo,
   ordering, and start-once idempotence — so the agent's change is either confirmed or rejected in
   under a second, with no window on screen.

### Scenario — a clean fan-in for the sibling children

1. Fourteen wave-1 children build against `QuickFiler.csproj` and `QuickFiler.Test.csproj`
   concurrently; both are legacy non-SDK projects with explicit `<Compile Include>` lists.
2. **Risk:** every child that adds a file must edit both, and the epic assigns neither file to
   anyone. This is the most likely conflict surface in the epic.
3. **After F6:** this child adds only its own `<Compile Include>` lines, in alphabetical position,
   removing and reordering nothing — so the conflicts that do occur are line-adjacent and
   mechanically resolvable. It edits no sibling-owned production file, adds no member to any of the
   five interface declarations, and relocates no executable line into a file carrying
   `[ExcludeFromCodeCoverage]`.

## Value Delivered

- **Honest measurement.** Numeric per-file line coverage for every file in the set, produced by F1's
  harness and committed as evidence — not an aggregate assembly figure that averages a 0% file
  against a 95% one.
- **A testable explorer controller.** The one file in the set with zero tests and a blanket coverage
  exemption becomes ordinary, deterministically testable code, with its ~139 lines of caller-free
  dead code removed rather than carried.
- **Assertions where there were none.** New behavioral tests supersede the assertion-free legacy
  cases, so the coverage number and the assurance level converge.
- **Latent defects surfaced as tracked work.** Three defects and one pre-existing test-file violation
  become GitHub issues instead of disappearing when this feature folder merges.
- **No cost transfer.** The sibling children's files, contracts, and coverage obligations are
  unchanged.

## Acceptance Criteria

These map one-to-one to AC-1 through AC-7 in `spec.md` and must be read as consistent with it. Where
`spec.md` states an implementation detail, it governs.

- [ ] **US-1 — The coverage number exists and is per-file.** Every file the F1 ledger classifies as
      `testable` reaches >= 80% line coverage, measured with F1's per-file harness and recorded as a
      numeric per-file line rate keyed by repo-relative path under
      `docs/features/active/2026-08-07-quickfiler-qfc-form-explorer-controller-coverage-435/evidence/qa-gates/`.
      Aggregate assembly coverage does not satisfy this. Any new production file added by this child
      reaches >= 90%. Files classified `no executable content` are evidenced by that classification,
      not by a percentage. (spec AC-1)
- [ ] **US-2 — The blanket exemption is gone, not relocated.** `[ExcludeFromCodeCoverage]` no longer
      appears in `QuickFiler/Controllers/QfcExplorerController.cs`, the file is present in the
      coverage report rather than absent from it, and its measured line rate is >= 80%. No new
      exemption attribute is introduced anywhere. Any residual exemption must be ratified by an
      explicit row in F1's ledger naming the specific irreducible remainder; this child cannot ratify
      one itself. (spec AC-2)
- [ ] **US-3 — No file grows past the readable limit.** Every production file in the set, and every
      production and test file this child adds or modifies, is at or under 500 lines, evidenced by a
      line count. The pre-existing 827-line `QfcFormControllerTests.cs` is out of scope and left
      untouched. (spec AC-3)
- [ ] **US-4 — Tests an agent can run unattended.** All new tests use MSTest, Moq, and
      FluentAssertions in Arrange–Act–Assert form. None creates a temporary file, contacts an
      external service, constructs or shows a form, opens a modal dialog, or uses `Thread.Sleep`,
      `Task.Delay`, `DateTime.Now`, `DateTime.UtcNow`, or `Random.Shared`. Ambient
      `SynchronizationContext` is restored in `[TestCleanup]`, no test touches `UiThread` statics,
      and the suite passes with class-level parallelism enabled. (spec AC-4)
- [ ] **US-5 — Coverage that means something.** For each `testable` file, the new test set includes
      at least one positive-path, one invalid-input, one boundary, and one error-handling case, and
      the evidence maps each category to the test method names satisfying it for that file.
      Multi-operand guards get one case per operand; stateful members get an explicit
      state-transition case. (spec AC-5)
- [ ] **US-6 — The full toolchain is green in one final pass.** `csharpier`, the analyzer build, the
      nullable/`TreatWarningsAsErrors` build, and the coverage-enabled MSTest run all complete in
      order with no failures and no file rewrites; the commands and results are recorded under
      `<FEATURE>/evidence/qa-gates/`. (spec AC-6)
- [ ] **US-7 — A user cannot tell anything changed.** No observable QuickFiler flow behaves
      differently: seam defaults reproduce the calls they replace including the existing
      fire-and-forget task discard; the `QfcExplorerController` constructor signature is unchanged so
      the F7 and F8 factory sites compile unmodified; no member is added to, removed from, or renamed
      on any of the five interface files; the two `LoadItemsAsync` signature lines in
      `QfcFormController.Actions.cs` remain single-line and unchanged; no production code is deleted
      by this child; and every pre-existing test in `QfcFormControllerTests.cs` and
      `QfcFormControllerSeamTests.cs` passes without being edited, weakened, or skipped. (spec AC-7)

## Non-Goals

- **Fixing the latent defects found during research.** `UndoConsumer()`'s non-termination,
  `ExplConvView_Cleanup()`'s `NotImplementedException`, and `OpenQFItem`'s duplicate
  `ActiveExplorer()` call are each a behavior change and are promoted to their own GitHub issues.
  This child pins the current behavior in tests and adds the seam that keeps the non-terminating loop
  out of the test host.
- **Splitting the 827-line `QuickFiler.Test/Controllers/QfcFormControllerTests.cs`.** A correct split
  is a four-way concurrent edit to one file inside a single wave. Promoted separately.
- **Deleting the dead `QuickFiler/Interfaces/IQfcFormController.cs`.** Recommended, but routed to
  F16 because it changes the epic's compiled-file denominator and edits a shared build input mid-wave.
- **Correcting the `IQfcFormViewer.cs` namespace/folder inconsistency.** Breaking across four
  siblings, delivers no coverage, and belongs after the epic closes.
- **Adding constructor argument-validation guards.** Changing `NullReferenceException` to
  `ArgumentNullException` on an already-fatal path is a behavior change; separate issue.
- **Editing any sibling-owned file.** `QfcFormViewer.cs` (F15), `KeyboardHandler.cs` (F3),
  `QfcCollectionController.cs` (F11), `EfcFormController.cs` (F9), `FilerQueue.cs` (F2),
  `coverage.config` and shared build property files (F1), and anything in `UtilitiesCS` are all
  outside this child.
- **Raising the coverage number by shrinking the denominator.** Relocating executable lines into a
  file carrying `[ExcludeFromCodeCoverage]` is rejected at plan review, regardless of the resulting
  percentage.
- **Migrating QuickFiler off VSTO/WinForms.** Where a seam choice is open, prefer host-neutral
  extraction a future port can reuse; that is the extent of this child's contribution to the
  migration.
