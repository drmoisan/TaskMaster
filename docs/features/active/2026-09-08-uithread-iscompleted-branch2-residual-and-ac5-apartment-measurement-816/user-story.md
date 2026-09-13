# 2026-09-08-uithread-iscompleted-branch2-residual-and-ac5-apartment-measurement (User Story)

- **Issue:** #816
- **Work Mode:** full-bug
- **Last Updated:** 2026-09-12
- **Status:** Non-normative context

## Normative status of this document

> **This document is non-normative and contains no acceptance criteria.** The specification file in
> this same feature folder remains the SOLE acceptance-criteria source for this item, per
> `acceptance-criteria-tracking`, which resolves `full-bug` to the specification only. Nothing here
> is to be checked off, counted in an acceptance-criteria tally, or treated as a delivery
> obligation. Where this document and the specification differ, the specification governs.

## Why this document exists at all

The `feature-promotion-lifecycle` skill states that a `full-bug` feature folder normally carries a
specification and no user story. Two things override that default for this item, and both are
recorded here so the deviation is not mistaken for a scoping error:

1. The run directive for this item explicitly requested both feature documents.
2. The prd-feature agent definition carries a SubagentStop hook that unconditionally requires an
   existing user-story document in the feature folder, so the agent cannot complete without one.

Because the acceptance-criteria source rule is unchanged by either of those, this document is
deliberately written as checkbox-free narrative.

## Narrative

**As** a maintainer of the UtilitiesCS threading layer, **I want** every exit of the
`SynchronizationContextAwaiter.IsCompleted` predicate that can report "already on the UI thread" to
prove that claim by two independent mechanisms, **so that** an await of the captured UI context
cannot complete inline on a thread-pool thread that merely happens to carry a recycled managed
thread id.

Today four of the five exits satisfy that standard. The exit that matches the persistent captured
UI context does not: once the reference match succeeds, its only remaining guard is a managed
thread-id comparison, which is exactly the bare owning-thread-identity shape the surrounding code
base already documents as unsafe. When that comparison is a false positive, the continuation runs
synchronously on a non-UI thread with no marshalling, and work already queued through the captured
context runs after it instead of before it.

**As** the same maintainer, **I also want** the apartment state of the thread a measurement runs on
to be read from that thread at runtime rather than inferred from a settings file, a parallelization
attribute, or documented test-framework behaviour, **so that** the one unmet acceptance criterion
carried by the issue #809 delivery is settled by an observation instead of by an assumption that a
later finding can withdraw. That is precisely what happened before: the earlier probe inferred its
apartment, the premise behind the inference was falsified by the same delivery, and its conclusion
had to be withdrawn, leaving the status of the issue #782 scenario recorded as unknown.

## Who is affected

- The two WebView2 viewer setup paths in QuickFiler that await the viewer's UI context and then
  capture a task scheduler from the current synchronization context. They are the call sites whose
  behaviour depends on the predicate being correct.
- Maintainers reading the issue #809 acceptance-criteria list, which currently shows one criterion
  open with no way to tell from the list alone what remains outstanding.

## What success looks like, in plain terms

A reviewer can point at one test that fails on the current code and passes after the change, and at
a second test that passes in both states, and conclude from the pair that the change closed the
recycled-thread-id case and left the genuine-UI-thread case alone. A reviewer can also open one
evidence artifact and read the apartment value that was actually observed on the executing thread,
rather than an argument about what it probably was.

## Explicitly not part of this story

The case where a caller is genuinely on the captured UI thread inside a WPF dispatcher operation is
out of scope. On that leg the predicate returns true today and must continue to return true, because
the dispatcher exit of the same predicate already reports true in that state. Ordering assertions at
the production await sites are also out of scope and remain an open residual from issue #809.
