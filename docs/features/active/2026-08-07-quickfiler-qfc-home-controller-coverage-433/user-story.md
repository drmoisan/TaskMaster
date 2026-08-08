# `quickfiler-qfc-home-controller-coverage` — User Story

- Issue: #433
- Parent: Epic `quickfiler-per-file-coverage` (issue #136) — child **F7**, wave 1
- Owner: drmoisan
- Status: Draft
- Last Updated: 2026-08-07T21-15
- Work Mode: `full-feature` (this file and `spec.md` are together the authoritative acceptance-criteria source)

## Story Statement

This is an **enabler** feature. It delivers no new end-user capability. Its beneficiaries are the
people and agents who maintain QuickFiler, and its value is measured in regression escapes that do not
happen.

- As the **maintainer of QuickFiler**, I want the `QfcHomeController` session-lifecycle, metrics, and
  queue-refill code covered by deterministic tests that pin its error paths and mode-selection guards,
  so that I can change how a filing session starts, refills, or reports without discovering the
  regression from a user report weeks later.
- As an **autonomous agent assigned work in QuickFiler**, I want the `QfcHomeController` file set to
  have a test suite that runs unattended — no live Outlook, no forms, no modal dialogs, no files, no
  elapsed wall-clock waits — so that I can verify my own change in a loop and get a truthful
  pass/fail signal without a human in the loop.
- As a **reviewer of a QuickFiler pull request**, I want per-file coverage figures produced by one
  shared, reproducible harness and committed as evidence, so that I can judge whether a change to
  these files is adequately tested without re-deriving the numbers myself.
- As an **end user of QuickFiler**, I want filing to behave exactly as it does today, so that a
  testability change costs me nothing.

## Problem / Why

`QfcHomeController` coordinates the whole QuickFiler filing session. It wires the data model, the
explorer controller, the form viewer, the keyboard handler, and the UI queue; starts the session in
either normal or high-confidence mode; drains the background refill queue; rotates the session
stopwatches; and accumulates metrics. Its ordering and state-transition invariants are the ones that
determine whether a filing session works.

Today the paths most likely to break in an unexpected way are the paths no test executes:

- **Nothing exercises the error arm of the background-worker completion handler.** The only way a user
  learns of that failure is a modal dialog, and the code that produces it has never been run by a test.
- **Nothing exercises any of the three exception handlers in the background refill.** Cancellation,
  cancellation-adjacent failure, and genuine failure are handled by three different branches with three
  different outcomes — swallow, swallow, rethrow — and the difference between them is currently
  unrecorded and unverified.
- **Nothing exercises the metrics drain method at all.** Its 22 lines are the largest single unexecuted
  block in the file set.
- **Nothing exercises the null-guard short-circuits in the mode selection.** Whether a session falls
  back to the full initialization batch when settings are absent is currently a matter of reading the
  source, not of running a test.

The consequence for a maintainer is that a plausible, well-intentioned edit to any of these areas
produces a green build and a green test run while changing behavior. The consequence for an autonomous
agent is worse: it has no signal at all, so it cannot self-verify, and every change in this area
requires human review of the diff rather than review of a test result.

This child closes those gaps for the five files the epic assigns it, using the epic's shared
measurement harness and classification ledger so that the result is comparable across all fifteen
children and can be closed once at the capstone.

## Personas & Scenarios

### Persona: the QuickFiler maintainer

- **Who:** the developer responsible for QuickFiler's behavior in the VSTO add-in, working in a legacy
  .NET Framework 4.8.1 / WinForms / Outlook Interop codebase.
- **What they care about:** that a change they make to session startup or queue refill does not break
  filing for a user in a way that surfaces days later and cannot be reproduced.
- **Their constraints:** the code is COM- and UI-bound; much of it historically could not be tested
  without a live Outlook process; the repository forbids temporary files, live forms, modal dialogs,
  and timing hacks in tests; and the project is on a long-term path away from VSTO, so investment must
  favor host-neutral extraction over new host-bound machinery.
- **Their frustrations:** a green build that proves nothing about the branch they just edited; test
  suites that pass locally and fail in CI because they depend on process-global state; and defects
  found by reading code rather than by running it.

### Persona: the autonomous agent working in QuickFiler

- **Who:** an agent executing an atomic plan against this repository, running the toolchain itself.
- **What they care about:** a deterministic, self-contained verification signal. A test that needs a
  human to dismiss a dialog, or that depends on machine state, is worse than no test — it converts a
  correctness question into an infrastructure question.
- **Their constraints:** cannot interact with a UI; cannot judge whether an unpinned behavior change is
  intended; must not edit files owned by a sibling feature running in parallel.
- **Their goal:** make a change, run format → analyze → type-check → test, and know from the result
  whether the change is safe.

### Scenario 1 — a mode-selection change that used to be invisible

A maintainer is asked to change how QuickFiler decides between normal and high-confidence startup. The
decision is a chain of null-conditional accesses on the settings object, and the maintainer replaces it
with a simpler expression that behaves differently when the settings object is absent.

Today: the build is green, the existing suite is green, and the change ships. A user whose settings
have not yet been populated gets a session that starts down the wrong path.

After this child: two named tests — one for absent globals, one for absent settings, on each of the
three affected members — fail immediately and name exactly which fallback was lost. The maintainer sees
the failure before the commit.

### Scenario 2 — an agent changes the background refill's error handling

An agent is assigned a change to the refill loop. It edits the exception handling, believing the two
`catch` blocks are redundant.

Today: no test executes either handler, so the suite stays green and a genuine failure that should have
propagated is now silently swallowed — or a cancellation that should have been absorbed now escapes to
the ribbon.

After this child: four tests distinguish the four outcomes — cancelled before the refill starts,
cancelled during the dequeue, failed after cancellation was requested, and failed without cancellation
— and the last one pins that the original exception type and message are preserved by the bare rethrow.
The agent gets a specific failure naming the path it changed, and can correct without human input.

### Scenario 3 — a reviewer judging whether a QuickFiler change is adequately tested

A reviewer opens a pull request touching `QfcHomeController.Metrics.cs`.

Today: the reviewer can see the assembly-level coverage number, which says nothing about this file, and
must read the diff and the test project to form a judgment.

After this child: the feature folder carries a per-file coverage figure for each file in the set,
produced by the epic's shared harness with the command and exit code recorded, so the reviewer can
re-run it and compare. The reviewer can also see, per file, whether it is measured at all or is an
interface-only declaration with no executable lines — and why.

### Scenario 4 — a user files email during a high-confidence session

Nothing changes. The session starts, the queue refills, items are filed, progress is reported, and the
"Email Time" calendar appointment and session CSV are produced exactly as before. Every production
change in this child is a redirection through a delegate whose default is the expression it replaced,
a pure-function extraction that reproduces current arithmetic including its rounding, or a relocation
of source text between two files of the same partial class.

## Acceptance Criteria

These are outcome-level criteria. The implementation-level criteria — coverage figures, seam shapes,
file sizes, frozen-file hashes, toolchain results — are in `spec.md` and are not repeated here.

- [ ] **US1 — Mode selection is pinned.** A maintainer who changes the high-confidence mode decision so
      that an absent globals object or an absent settings object no longer falls back to the full
      initialization batch gets a named, failing test, on each of the three members that make that
      decision, rather than a green run.
- [ ] **US2 — The four refill outcomes are distinguishable.** A maintainer or agent who alters the
      background refill's exception handling gets a distinct failing test for each of: cancellation
      before the refill begins (which propagates), cancellation during the dequeue (which is absorbed),
      failure after cancellation was requested (which is absorbed), and failure without cancellation
      (which is rethrown with its original type and message preserved).
- [ ] **US3 — Session completion is pinned on both non-success paths.** A change to the
      background-worker completion handler that alters what happens when the work was cancelled, or
      what message a user is shown when it failed, produces a failing test — and no modal dialog
      appears at any point during the test run.
- [ ] **US4 — Metrics accumulation and the session file hand-off are pinned.** A change to how
      diagnostic lines are produced, ordered, or handed to the writer produces a failing test, and no
      test writes a file to disk to prove it.
- [ ] **US5 — Stopwatch rotation is pinned as a state transition, not just a field move.** A change
      that stops preserving the outgoing measurement, or that stops starting the incoming stopwatch,
      produces a failing test.
- [ ] **US6 — The suite runs unattended.** The full set of tests this child adds completes with no
      human interaction, no live Outlook process, no WinForms form, no modal dialog, no file created on
      disk, no network or external process, and no elapsed wall-clock wait. An agent can run it in a
      verification loop.
- [ ] **US7 — The suite is order-independent and repeatable.** Running the tests in a different order,
      or repeatedly in the same session, produces identical results, because no test depends on
      mutable process-global state, the UI thread, the machine clock, or the ambient culture in a way
      that another test can perturb.
- [ ] **US8 — Coverage for this file set is reproducible on demand.** A reviewer can re-run the epic's
      shared per-file harness using the exact command recorded in the evidence artifact and obtain the
      same per-file figures, without reading the production source to reconstruct them.
- [ ] **US9 — Every file in the set has a stated, evidenced disposition.** A reader of the feature
      folder can determine, for each of the five in-scope files plus the new partial, whether it is
      measured and at what figure, or whether it is an interface-only declaration with no executable
      lines — and can see the evidence behind that classification rather than an assertion.
- [ ] **US10 — Filing behavior is unchanged for the end user.** Normal and high-confidence sessions
      start, refill, file, and report progress exactly as they did before this change, and the session
      CSV and "Email Time" calendar appointment are produced with the same content. No behavior
      difference is observable from outside the assembly.
- [ ] **US11 — Defects found are visible, not silently fixed and not silently endorsed.** Every latent
      defect encountered during this work is a tracked GitHub issue (#442, #443, #446, #447), and every
      test that pins one of those behaviors says in its own summary comment that it is a
      characterization test and which issue tracks the defect — so a future reader cannot mistake the
      test for an endorsement of the behavior.
- [ ] **US12 — Parallel work in QuickFiler is not disturbed.** An agent working on any sibling feature
      in the same wave is neither blocked nor broken by this change: neither shared home-controller
      interface is modified, no sibling-owned file is modified, and the shared project file receives a
      single-line addition placed to minimize the merge-conflict region.
- [ ] **US13 — The next change to this file is easier, not harder.** After this child, a maintainer
      adding a member to `QfcHomeController` has substantial headroom under the 500-line limit instead
      of a handful of lines, and does not have to perform an emergency file split as a precondition of
      their own work.

## Non-Goals

- **No new end-user capability.** This child adds no feature, no setting, no command, and no UI.
- **No fix for the latent defects it documents.** #442 (metrics never flushed), #443 (metrics duration
  misread), #446 (empty-batch inference closes the UI queue irreversibly), and #447 (dead
  `Iterate`/`Iterate2` removal) are tracked separately and must not be fixed here. Nor may any
  additional report-only research finding be fixed.
- **No removal of dead production code.** `Iterate()` and `Iterate2()` are covered, not deleted;
  removal is a breaking interface change requiring a sibling-owned file edit.
- **No change to either home-controller interface**, including the narrowing that would remove three
  `NotImplementedException` implementations from the sibling EFC controller. That is a coordinated
  cross-child change and is out of scope.
- **No migration away from VSTO or WinForms.** Where a seam choice is open, host-neutral extraction is
  preferred so a future port can reuse it, but no port is attempted.
- **No change to any repository coverage threshold**, and no re-scoping of the repository-wide floor.
- **No coverage work on `QuickFiler/Legacy/**` or `QuickFiler/Notes/**`**, which are not compiled and
  are outside the epic denominator.
- **No work on files assigned to sibling children**, even where this child's tests exercise them
  indirectly through their interfaces.
