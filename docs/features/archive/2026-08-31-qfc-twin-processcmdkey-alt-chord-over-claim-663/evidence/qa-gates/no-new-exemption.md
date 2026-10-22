# Phase 5 — No new coverage exemption ([P5-T6])

Timestamp: 2026-09-01T23-32

Command 1: `git diff -U0 origin/main...HEAD -- '*.cs'`
Command 2: `git status --porcelain`

EXIT_CODE: 0 for both.

## Why the diff is scoped to `.cs`

The documentation commits already on this branch add twenty lines that quote the attribute name in prose,
so an unscoped diff reports twenty `+` matches before any source edit is made and the gate could never
pass. Scoping to `.cs` preserves the gate's discrimination, because AC-13 is about the change set's C#
content.

## Acceptance reading 1 — no added `.cs` line contains `ExcludeFromCodeCoverage`

Measured by filtering the diff output for lines matching `^\+` and excluding the `+++` file-header form:

```
ADDED_LINE_COUNT=164
ADDED_WITH_EXCLUDE=0
```

The C# change set adds 164 lines and **none of them contains `ExcludeFromCodeCoverage`**. The denominator
is non-zero, so the reading is a real measurement over a populated added-line set rather than a vacuous
pass over an empty diff.

The new predicate was deliberately placed on `QfcFormKeyHandler`, which carries no such attribute, rather
than on `QfcFormViewer`, which does. That placement is what makes the `[P4-T7]` `<method>` element exist
and is why no new exemption was needed to satisfy the coverage requirement.

## Acceptance reading 2 — porcelain reports no `.cs` path

```
PORCELAIN_LINES=4
 M docs/features/active/qfc-twin-processcmdkey-alt-chord-over-claim-663/plan.2026-08-31T20-16.md
?? docs/features/active/qfc-twin-processcmdkey-alt-chord-over-claim-663/evidence/baseline/
?? docs/features/active/qfc-twin-processcmdkey-alt-chord-over-claim-663/evidence/qa-gates/
?? docs/features/active/qfc-twin-processcmdkey-alt-chord-over-claim-663/evidence/regression-testing/
CS_PATHS=0
```

Zero of the four reported paths ends in `.cs`, as required. The porcelain span is scoped to `.cs` paths
rather than required to be empty, because evidence artifacts written by Phases 0 through 4 are still
untracked at this point and `.claude/agent-memory` is a tracked directory that unrelated agent activity
can leave modified. On this reading `.claude/agent-memory` is clean; the four entries are the plan file
carrying this run's check-offs and the three evidence directories this plan writes, all of which
`[P6-T18]` commits.

Output Summary: The `.cs`-scoped anchored diff adds 164 lines and none contains `ExcludeFromCodeCoverage`,
and `git status --porcelain` reports no path ending in `.cs`. No new coverage exemption was introduced
anywhere in the change. AC-13 holds.
