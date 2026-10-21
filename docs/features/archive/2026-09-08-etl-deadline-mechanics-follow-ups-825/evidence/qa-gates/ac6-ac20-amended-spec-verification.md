# AC6 and AC20 — Amended-Spec Verification (read-only)

Timestamp: 2026-09-09T16-49

Command: Select-String over docs/features/active/2026-09-08-etl-deadline-mechanics-follow-ups-825/spec.md for the five patterns recorded below

EXIT_CODE: 0

AmendmentMarkerCount: 4
NoEditAtAllCount: 0
WriteSetDfDeedleEtlTimeoutTestsCount: 1
TotalAcCount: 35
CheckedAcCount: 1

Output Summary: Every measured value matches its stated expectation, so the working tree carries the
amended spec and Phase 3 may proceed. This task made no edit to spec.md. The AC6, AC20 and AC35
amendments were applied during preparation by the orchestrator, in commits 945659cd (AC6 and AC20)
and 13c27214 (AC35), for the reason recorded in the adjudicated design conflict section of the plan:
acceptance criteria in this repository are authored by planning and scoping agents, and an executor
free to rewrite the criterion it is judged against is not gated by that criterion.

## What each figure establishes

AmendmentMarkerCount is the number of occurrences of the token `Amended 2026-09-09`. The expected
value 4 accounts for the marker on AC6, the marker on AC20, the marker on AC35 and the marker on the
replaced Non-goals bullet.

NoEditAtAllCount is the number of occurrences of the token `no edit at all`. The expected value 0
records that the falsified Non-goals assertion, that the three DfDeedle-path tests become
deterministic with no edit, has been replaced rather than retained.

WriteSetDfDeedleEtlTimeoutTestsCount is the number of lines matching the Write Set's
backticked-path bullet form for UtilitiesCS.Test/Extensions/DfDeedleEtlTimeoutTests.cs. The expected
value 1 records that the Write Set gained that path on 2026-09-09, which is what makes the bounded
timer-ordering update of Phase 3 an in-scope edit rather than a boundary violation.

TotalAcCount counts lines matching the box-state-independent inventory form, so it is the inventory
assertion and is unaffected by check-off progress. The expected value 35 matches the count P0-T11
pinned before Phase 1.

CheckedAcCount is 1, the transition Phase 1 produced: P1-T4 checked off AC8 after the compile-time
proof recorded at evidence/other/ac8-createcancellationtokensource-proof.md returned
`CS1061Count: 0`. The P1-T4 fallback branch was not taken, so the alternative expected value of 0
does not apply.
