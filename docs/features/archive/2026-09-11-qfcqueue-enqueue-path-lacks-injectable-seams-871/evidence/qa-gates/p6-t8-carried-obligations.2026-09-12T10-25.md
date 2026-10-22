# P6-T8 — Carried-forward obligations this item enables but does not discharge

Timestamp: 2026-09-13T17-05
Command: none; this task records two obligations and cites the evidence that their precondition is met
EXIT_CODE: 0

The spec's Rollout and Follow-up section lists two clean-up tasks that depend on this item's coverage
movement but are not performed by any task in this plan. Both are recorded here so that the owning
orchestrator can close them out, and so that a reader of this feature folder does not conclude they were
forgotten.

## Obligation 1 — item 678 acceptance criterion AC20

Obligation1: item 678, acceptance criterion AC20
Obligation1Location: an archived feature folder for item 678, outside this item's Write Set
Obligation1DischargedByThisPlan: no

Item 678 is the prior art for the QfcQueue coverage work and its AC20 waits on a demonstration that the
enqueue path's coverage has moved. No task in this plan edits any document belonging to item 678: its
feature folder is archived and lies outside the seventeen paths of this item's Write Set, so a task that
flipped a checkbox there would be a scope-lock failure under the rule every diff and status gate in this
plan applies. Checking it off is therefore the owning orchestrator's action, taken against that folder,
not this executor's.

## Obligation 2 — issue 727 sub-finding 4

Obligation2: issue 727, sub-finding 4
Obligation2Location: an archived feature folder outside this item's Write Set
Obligation2DischargedByThisPlan: no

Issue 727 records a policy gap whose fourth sub-finding is likewise held open pending the coverage
movement on this path. The same scope reasoning applies: the record lives outside this Write Set and this
plan does not write to it.

## The evidence that the coverage movement both obligations wait on has occurred

The precondition for both is the post-change Cobertura measurement and its comparison against the
baseline. Two artifacts carry it.

**The P5-T5 coverage artifact**, p5-t5-coverage-postchange.2026-09-12T10-25.md in the qa-gates evidence
directory of this feature folder. It records a post-processed Cobertura document produced by a run in
which all 1423 cases of the QuickFiler test assembly passed at `failed=0`, together with the QuickFiler
package figures 10314 covered of 12626 valid at 0.816886 and the four per-file class figures.

**The P6-T1 comparison**, p6-t1-coverage-file-rates.2026-09-12T10-25.md in the same directory. It records
both gate clauses as PASS:

- The enqueue part `QuickFiler/Controllers/QfcQueue.Enqueue.cs` moved from 0.152941 over 13 of 85 covered
  lines to 1 over 85 of 85, strictly greater than both the spec's recorded pre-change rate and the
  P0-T12 measurement.
- The combined post-change rate for the base, Tlp and UiIdle parts is 0.540299 over 181 of 335, at or
  above both the spec's recorded 0.503205 and the P0-T12 pre-split measurement of 0.496795.

Two further artifacts corroborate. P6-T2 derives the genuinely-new line rate as 0.958333 over 23 of 24,
clearing the 0.90 new-code floor, and records zero regressions among relocated statements whose anchor
line is uniquely identifiable. P6-T4 records the QuickFiler package delta as 99 newly covered lines
against 23 added valid lines and projects the repository-wide rate at 0.857898.

## The out-of-scope counter-leak defect already holds a potential-bug entry

CounterLeakEntryExists: yes
CounterLeakEntryPath: docs/features/potential/2026-09-12-qfcqueue-enqueueasync-jobsrunning-counter-leak.md
NewEntryFiledByThisPlan: no

The defect is that the running-jobs increment sits outside the try block whose finally decrements it, so
a throw from the hook loop or from the background-template clone leaks the counter. It was promoted to a
potential-bug entry on 2026-09-12, before this plan's Phase 6 ran, and P6-T7 added a relative Markdown
link to that entry into the spec's Rollout and Follow-up section, verified by a path-existence check. No
new entry is filed by this plan, because filing one would duplicate the existing record.

This item did not fix the defect and did not disturb it. P3-T7 verified both: the increment's line number
is still strictly less than the line number of the `try` keyword that opens the block whose `finally`
holds the sole decrement, and an anchored zero-context content diff of that single path against the
recorded anchor produced no hunk containing the increment line, the `try` line or the decrement line.
P4-T10 confines both of its catch-path throws to the substituted item-group factory, which raises from
inside the try block so the counter decrements normally; no test raises from the background-template
factory or from the hook loop, because a test that did would have to treat the leak as expected
behaviour, which this item forbids.

Output Summary: Two obligations are carried forward and are not discharged by any task in this plan —
item 678 acceptance criterion AC20 and issue 727 sub-finding 4 — because both are recorded in archived
feature folders outside this item's Write Set and writing to them would be a scope-lock failure. The
coverage movement they wait on has occurred and is evidenced by the P5-T5 coverage artifact and the
P6-T1 comparison, both of whose gate clauses read PASS. The out-of-scope counter-leak defect already
holds a potential-bug entry dated 2026-09-12, now linked from the spec by P6-T7, so no new entry is
filed. Acceptance met.
