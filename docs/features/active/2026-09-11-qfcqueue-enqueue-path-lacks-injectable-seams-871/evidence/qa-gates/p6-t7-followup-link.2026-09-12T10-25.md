# P6-T7 — Follow-up link to the separately promoted out-of-scope defect

Timestamp: 2026-09-13T17-04
Command: regular-expression extraction of every Markdown link in the Rollout & Follow-up section of the spec, followed by a path-existence check of each resolved target
EXIT_CODE: 0

## What was added

One relative Markdown link was appended to the existing Links bullet of the
`## Rollout & Follow-up` section of
`docs/features/active/2026-09-11-qfcqueue-enqueue-path-lacks-injectable-seams-871/spec.md`. The link text
is the file name of the target and the target is a relative path from the spec's own directory.

The bullet previously ended with the research-artifact clause. The appended clause is:

```
; and the separately promoted
job-counter defect, whose potential-bug entry is
[2026-09-12-qfcqueue-enqueueasync-jobsrunning-counter-leak.md](../../potential/2026-09-12-qfcqueue-enqueueasync-jobsrunning-counter-leak.md).
```

No other sentence in that section was altered. The two preceding bullets — the release-and-rollout steps
and the post-fix monitoring list, including the clause directing the counter defect to be tracked to its
own fix — are unchanged, as is every other section of the spec apart from the Phase 7 acceptance-criteria
check-offs, which are a separate matter.

## Path-existence check

LinkCountInSection: 1
LinkText: 2026-09-12-qfcqueue-enqueueasync-jobsrunning-counter-leak.md
LinkTarget: ../../potential/2026-09-12-qfcqueue-enqueueasync-jobsrunning-counter-leak.md
ResolvedTargetExists: True

The target resolves, from the spec's directory, to
docs/features/potential/2026-09-12-qfcqueue-enqueueasync-jobsrunning-counter-leak.md, which exists in the
tree. The extraction was run over the section text from its heading to the end of the file and matched
exactly one link, so the acceptance condition's "exactly one link" clause is satisfied by measurement
rather than by inspection.

## Why this link is needed

The spec's Rollout and Follow-up section already named the separately promoted counter-leak defect in
prose but carried no link to it. Acceptance criterion AC21 has three clauses: that the defect was not
fixed here, that no test asserts the leaking behaviour as correct, and that the item points at the
separately filed follow-up. Phase 4 correctly left AC21 unchecked because the third clause had no
satisfying evidence: a prose mention is not a pointer a reader can follow, and the entry lives outside
this feature folder where a reader would not find it.

The defect itself is the running-jobs increment sitting outside the try block whose finally decrements
it, so a throw from the hook loop or from the background-template clone leaks the counter. Fixing it
requires widening the try block, which is a behaviour change and is explicitly forbidden to this item.
P3-T7 verified the region was not disturbed: the increment's line number is still strictly less than the
line number of the `try` keyword, and an anchored zero-context content diff of that path produced no hunk
containing the increment line, the `try` line or the decrement line.

No new potential entry was filed by this task. The entry already exists, captured on 2026-09-12, and
filing a second would duplicate it.

Output Summary: One relative Markdown link to the promoted counter-leak potential-bug entry was appended
to the existing Links bullet of the spec's Rollout & Follow-up section. A regular-expression extraction
over that section finds exactly one link, and its target resolves to an existing file. No other sentence
in the section was altered and no new potential entry was filed. This satisfies the third clause of AC21,
which Phase 4 left outstanding. Acceptance met.
