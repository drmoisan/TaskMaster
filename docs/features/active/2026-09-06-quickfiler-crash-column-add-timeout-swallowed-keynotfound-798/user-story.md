# 2026-09-06-quickfiler-crash-column-add-timeout-swallowed-keynotfound (User Story)

- **Issue:** #798
- **Owner:** drmoisan
- **Last Updated:** 2026-09-07T02-30
- **Work Mode:** full-bug

> **Non-authoritative.** issue.md records `- Work Mode: full-bug`, under which spec.md is the sole
> authoritative acceptance-criteria source. This file exists because the caller requested it and the
> agent's SubagentStop hook requires a user-story artifact; it is narrative context only. It contains
> **zero** checkbox items by design, so no tracker can mistake it for a second acceptance-criteria
> source. Do not add checkboxes here, and do not check off criteria here. The criteria live in
> spec.md under `## Acceptance Criteria`.

> **Formatting convention.** As in spec.md, repository file paths are written as plain prose in this
> document. The change footprint is declared only in the `## Write Set` section of spec.md.

## Who is affected

The Outlook add-in operator — the single maintainer-user who runs the VSTO build day to day, plus any
future user of the shipped add-in. The defect is operator-facing rather than developer-facing: it
manifests as a host crash during ordinary use, not as a build or test failure.

## Story 1 — Launching QuickFiler on a slow folder (AC1, AC6)

As the operator, I select a mail folder and click QuickFiler on the ribbon, expecting the QuickFiler
dialog to open for that folder.

Today, on the folder named "T&E", nothing appears for roughly nine seconds and then Outlook reports
an unhandled exception. The add-in gave no indication that anything was slow and no indication of
which folder or which step failed. The launch had in fact given up on preparing the folder's table
columns three times in a row and continued anyway.

Fixed means: either QuickFiler opens, or I get a dialog that tells me the column-add step timed out
on the folder named "T&E". I know which folder and which step to look at, and Outlook keeps running.

## Story 2 — Diagnosing why a folder is slow (AC2)

As the operator investigating a slow or failing launch, I open the debug log expecting to see where
the time went.

Today the log shows a nine-second gap with nothing in it. There is no way to tell whether the delay
was the folder's user-defined-property enumeration or one of the six column add and remove calls.

Fixed means: the log carries a timing line for the property enumeration and for each individual
column add and remove, in the same format as the timing lines already present, so a repeat occurrence
is attributable to a specific call without attaching a debugger.

## Story 3 — Getting a comprehensible error instead of a dictionary failure (AC3)

As the operator, when the folder's data cannot be prepared, I expect the error to describe what is
missing.

Today the error reads "The given key was not present in the dictionary." That names neither the
column nor the folder, and it surfaces two layers away from the actual cause.

Fixed means: the failure names the missing columns and the folder, so the message is actionable on
its own.

## Story 4 — Reporting a defect with a usable stack (AC4)

As the operator filing a defect report, I expect the reported stack to point at the code that
actually failed.

Today the rethrow resets the stack at the point of the catch, so the origin frame is lost and the
report points at the handler rather than at the fault.

Fixed means: the original stack survives the rethrow and the report identifies the originating frame.
Note that the exception the operator sees is still wrapped by the timeout helper, so the dialog must
show the inner exception detail; otherwise the visible text is only "One or more errors occurred."

## Story 5 — Not losing Outlook to a ribbon click (AC5)

As the operator, I expect a failure inside an add-in command to be reported by the add-in, not to
take down the host.

Today a failure in the QuickFiler, QuickFiler high-confidence, or Sort Email ribbon commands escapes
an unguarded `async void` handler, reaches the thread pool, and Outlook reports it as an unhandled
exception.

Fixed means: those three commands catch failures at the ribbon boundary, write the full detail to the
log, and show me an error dialog. Outlook continues running and I can retry or work elsewhere.

## What "done" looks like from the operator's seat

A QuickFiler launch either opens the dialog or explains, in one dialog, which folder and which step
failed. The log always contains per-step timing for the column-add work. Outlook does not surface an
unhandled exception from any of the three ribbon commands.

## Known trade-off the operator should expect

After this change, a folder whose column preparation genuinely exceeds the existing nine-second
budget will fail to open QuickFiler every time, rather than opening intermittently and crashing
later. That is a deliberate exchange of an unpredictable host crash for a predictable, named,
recoverable error. The new timing logs exist so that the underlying slowness can then be identified
and addressed on evidence.
