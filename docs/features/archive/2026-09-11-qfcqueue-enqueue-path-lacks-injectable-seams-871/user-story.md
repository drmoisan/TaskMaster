# 2026-09-11-qfcqueue-enqueue-path-lacks-injectable-seams (User Story)

- **Issue:** #871
- **Work Mode:** full-bug
- **Last Updated:** 2026-09-12T10-35
- **Status:** Narrative context only

> **Why this file exists.** Work mode for issue #871 is `full-bug`, and under the repository's
> acceptance-criteria tracking protocol the spec document in this folder is the sole authoritative
> acceptance-criteria source for that mode. This user story exists because the delivery tooling
> requires the artifact, and because the defect is maintainer-facing rather than purely internal: the
> people who pay for it are the engineers who must change the enqueue path without any automated
> proof that they did not break it. This document therefore carries **zero checkboxes** and must not
> be used as an acceptance-criteria source. Every criterion lives in the spec document, identified
> AC1 through AC22.

> **Formatting note.** This document deliberately contains no backticked repository paths. The
> change footprint is declared once, in the spec document's Write Set section. Do not add paths here.

## Personas

- **Maintainer** — an engineer or agent modifying the QuickFiler enqueue path. Cannot currently
  verify any change to it except by loading the add-in into a live Outlook session.
- **Reviewer** — an engineer or agent auditing a pull request that touches the enqueue path. Has no
  automated signal that the change preserved behaviour.
- **End user** — an Outlook user running high-confidence filing. Should observe no change whatsoever
  from this work; that absence of change is itself a deliverable.

## Story 1 — Substitute the move monitor without a live Outlook session

- **Given** a maintainer writing a unit test for the enqueue path,
- **When** the test assigns a strict mock through the internal move-monitor seam on the queue,
- **Then** the hook call for each mail item is observable and verifiable, the production default
  remains the single per-owner monitor instance the field initializer creates, and the three existing
  QfcQueue tests that reflect on the private move-monitor field continue to pass unmodified.

Covers AC1 and AC15.

## Story 2 — Run the UI idle path synchronously in a headless test host

- **Given** a test host with no WPF message pump and no initialized process-wide dispatcher,
- **When** the test assigns a hand-written synchronous dispatcher fake through the queue's internal
  UI idle seam,
- **Then** every UI-marshalled call on the enqueue path executes inline and deterministically,
- **And** production still marshals all three call shapes at `ContextIdle` priority with the existing
  double-await and yield preserved, so background page construction is scheduled exactly as before.

Covers AC2 and AC7.

## Story 3 — Cover viewer construction and row placement rather than relocate them

- **Given** a maintainer who wants the viewer-construction region covered, not merely moved behind a
  seam,
- **When** the test substitutes the viewer factory and the row placer while leaving the coarse
  item-group factory at its default,
- **Then** the production body of `AddAsync` actually executes, the substituted factory receives the
  queue's own cancellation token, and the substituted placer receives the same three arguments the
  previous direct call passed.

Covers AC3, AC4, AC5 and AC17.

## Story 4 — Reach the enqueue body past the background template clone

- **Given** that everything after the template clone was previously gated on an unproven,
  reflection-driven copy of a WinForms control,
- **When** the test substitutes the background template factory with one returning an identifiable
  sentinel panel,
- **Then** the guard branches, the success path, both catch paths, the running-jobs bookkeeping and
  the collection-changed notification all become reachable deterministically,
- **And** the production default still evaluates the identical clone expression with the identical
  named argument.

Covers AC6, AC10, AC11, AC12, AC13 and AC14.

## Story 5 — Verify the per-row controller construction contract

- **Given** a page of items enqueued through the seams,
- **When** the test captures every argument passed to the item-controller factory for each row,
- **Then** both arms of the item-number-digit calculation are exercised, the index mapping survives a
  non-zero starting offset, the carried folder handler is resolved for both the carrier-present and
  carrier-absent cases, and initialization is awaited exactly once per row.

Covers AC16.

## Story 6 — Keep every file under the size ceiling

- **Given** that the base queue file already exceeds the repository's 500-line hard ceiling before
  any seam is added,
- **When** the Tlp-manipulation region and the UI helper region are relocated into two new partial
  parts and the new interface is added,
- **Then** every production file in the change footprint measures under 500 lines after the edit, the
  new test file does too, and the figures are re-measured rather than estimated,
- **And** each new file is registered in its project's compile manifest, because these legacy
  non-SDK projects have no implicit source glob and a missing entry would present as a missing member
  rather than a missing file.

Covers AC8 and AC9.

## Story 7 — See no change as a reviewer or an end user

- **Given** a reviewer auditing the diff,
- **When** they compare the relocated members against their previous text,
- **Then** every relocated member moved verbatim apart from the named seam substitutions, no nullable
  pragma was added to relocated code, the obsolete-API suppression is intact, the log call and its
  message are unchanged, every region closes on the same side of the split, and no public member of
  the queue type was added, removed, retyped or resigned,
- **And** an end user running high-confidence filing over more than one page sees background pages
  render exactly as before.

Covers AC18 and AC22.

## Story 8 — Read an honest coverage account, including what is still not covered

- **Given** that a testability item can appear successful while merely relocating an untestable
  region,
- **When** the work reports its coverage,
- **Then** the new code meets the 90% floor and the repository stays at or above the 80% floor as
  stated in the repository's top-level instruction file, the measured file-level rate for the enqueue
  part rises strictly above its recorded baseline, and both the pre-change and post-change coverage
  artifacts are committed under this feature's evidence directories,
- **And** every region on the enqueue path that remains uncovered is named explicitly in a committed
  document with the reason and a citation, rather than being left silently uncovered.

Covers AC19 and AC20.

## Story 9 — Leave the separately promoted defect alone

- **Given** that research on this item uncovered an unbalanced job-counter control flow on the same
  path, promoted separately as its own potential bug entry,
- **When** this item is implemented,
- **Then** that control flow is unchanged, no test written here asserts the leaking behaviour as
  correct, and the separate entry is linked from the spec document's follow-up section,
- **Because** mixing a behavioural fix into a testability change would make both unreviewable.

Covers AC21.

## Out of scope for this story set

Stated in plain prose deliberately, with no paths: the UtilitiesCS threading types are not modified;
the three existing QuickFiler QfcQueue test files are not modified; the dead, entirely commented-out
template-activation member is relocated but not deleted; no coverage exemption attribute or
assembly-level exclusion is introduced; and the reflection-based control clone that the background
template seam bypasses is not itself brought under test by this item.
