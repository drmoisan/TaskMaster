# Preflight round 2 delta, adjudicated by the orchestrator

Timestamp: 2026-09-07T00-20

Reviewer signal: `PREFLIGHT: REVISIONS REQUIRED` with `CONVERGENCE: FURTHER ROUNDS LIKELY`.
Four blocking findings, all local corrections to acceptance clauses and one task body. No phase, task
ordering, Write Set entry or design decision changes.

## Adjudication

All four blocking findings are ACCEPTED. Unlike round 1, none is rejected and none is narrowed. The
reviewer's round-1 runtime claims needed adjudication because it could not execute anything; these
four are all decidable by reading, and the orchestrator verified the two most consequential premises
directly against the tree.

- B13 verified. `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs` contains
  `EnsureBreadcrumbPipeline();` at line 112 and `internal void EnsureBreadcrumbPipeline()` at line
  138. The relocation task moves lines 132 to 163 only, so the call at line 112 survives and an
  acceptance demanding zero matches for the bare identifier can never be satisfied.
- B14 verified. `UtilitiesCS/OutlookObjects/Folder/FolderRow.cs` opens with `#nullable enable` at
  line 1 and declares `public FolderRow(string text, FolderRowKind kind, FolderScore? score)` at line
  42, a non-nullable first parameter. Passing the newly nullable projection result there is CS8604,
  which the nullable gate promotes to an error.
- B15 accepted. The third acceptance conjunct of the assets task cannot fail: the plan's own scope
  rule restricts the enumeration it reads to a source pathspec, and the resources directory contains
  no file matching that pathspec, so the conjunct is true for every possible execution.
- B16 accepted, and it is a defect the orchestrator introduced. The clause was appended in round 1 as
  part of the accepted B2 replacement text. It asserts an observation that no scheduled command in
  that phase produces, because the only build in that phase deliberately runs without the gate
  switches. The correction moves the proof onto two diagnostic counts recorded by that build and
  re-proved under enforcement in the final phase.

## Non-blocking items

m5 and m6 are accepted as written. m8 requires no change: it reports a count in the forwarded prose,
not in the plan.

m7 is accepted with a different remedy than the reviewer proposed. Rather than rewording the trailing
signal line, the plan file should carry no line matching the preflight signal vocabulary at all.
Clearance is the executor's return recorded in the orchestrator checkpoint; a signal-shaped line
inside the plan file is a second, unmaintained assertion of the same fact and will be stale the moment
clearance is granted.

## Round count

This is the third preflight round against a two-round target. The overrun is legitimate rather than a
review failure: each round returned a complete enumeration rather than one defect at a time, and the
round-2 findings are in regions that only came into existence when the round-1 delta was applied. No
prior pass could have observed them. B16 in particular is a defect created by the round-1 delta
itself.
