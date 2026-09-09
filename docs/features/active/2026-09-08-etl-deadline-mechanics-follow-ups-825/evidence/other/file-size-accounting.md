# AC18 — File-Size Accounting for TimeOutTask.cs

Timestamp: 2026-09-09T17-02

LinesBefore: 1011
LinesAfter: 966
CapLimit: 500
CapStillViolated: true

This is a reduction, not a resolution, of the 500-line cap violation.

## What was removed and what remains

Deleting the two inert integer-repeat TimeoutAfter overloads took
UtilitiesCS/Threading/TimeOutTask.cs from 1011 lines to 966, a reduction of 45 lines. The removal
covers the generic overload and its non-generic counterpart together with the blank separator lines
that surrounded them. Neither overload could ever execute its catch clause: all three exits of the
inner provider overload return a Task and none throws synchronously, so the assignment inside the
try never threw, `repeatAttempts` was never read, and the "attempts remaining" warning was
unreachable.

The repository cap defined in .claude/rules/general-code-change.md is 500 lines, and the file
remains 466 lines above it. Bringing the file under the cap requires splitting it, which needs a new
production compile item in UtilitiesCS/UtilitiesCS.csproj; that project file is deliberately outside
this feature's Write Set, so the split is a separate change with its own review surface. It is
recorded in spec.md Rollout as the second deferred follow-up, to be filed by the epic after this
feature merges.

No acceptance criterion in this feature claims the cap violation is resolved. No other artifact,
code comment, commit message or PR body produced by this feature states that it is resolved.
