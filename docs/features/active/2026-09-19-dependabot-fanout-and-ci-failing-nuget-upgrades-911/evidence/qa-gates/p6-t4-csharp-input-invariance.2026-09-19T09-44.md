# P6-T4 — Batch C changed no C# compilation input

Timestamp: 2026-09-19T09-44

Commands:

```
git -C "<execution-worktree-root>" diff --name-only 596e7a70c78443861576f21a572bd2a919f02c66 -- .
git -C "<execution-worktree-root>" status --porcelain --untracked-files=all
```

`596e7a70c78443861576f21a572bd2a919f02c66` is the head SHA P4-T7 recorded, which is the
anchor this task names. The diff is anchored to a ref rather than left to compare against
the index, and it is paired with a porcelain companion because the two are complementary:
the anchored diff enumerates tracked changes only and can never report a file this batch
creates, and porcelain status goes empty once the change is committed.

EXIT_CODE: 0

## Capture 1 — anchored diff, 2 paths

```
docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/plan.2026-09-19T09-44.md
docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/spec.md
```

Both are tracked files this batch modified: the plan carries the Phase 5 and Phase 6
check-offs, and `spec.md` carries the acceptance check-offs for AC8, AC11, AC12, AC13,
AC14, AC16, AC21, AC22 and AC23.

## Capture 2 — porcelain, 35 entries

Two modified, both the tracked files above, and 33 untracked: 27 evidence artifacts written
by Phase 5 and Phase 6, and the six Batch C PowerShell files. The six are the only entries
outside `docs/`:

```
scripts/dependencies/AnalyzerItemRepair.psm1
scripts/dependencies/ConsistencyVerifier.psm1
scripts/dependencies/ProjectConsistency.psm1
tests/scripts/dependencies/AnalyzerItemRepair.Tests.ps1
tests/scripts/dependencies/ConsistencyVerifier.Tests.ps1
tests/scripts/dependencies/ProjectConsistency.Tests.ps1
```

Nothing under `coverage/` appears in either capture, because `.gitignore:144` covers it.

## Output Summary

```
UNION_COUNT=35
CSHARP_INPUT_COUNT=0
```

## Acceptance

| Clause | Required | Measured |
|---|---|---|
| Union of the two captures contains at least 4 paths | >= 4 | 35 |
| Paths matching `*.cs`, `*.csproj`, `*.sln`, `packages.config` or `app.config` | exactly 0 | 0 |

The at-least-4 clause is the non-vacuity guard: an empty union would also satisfy the zero
and would prove nothing. The union is 35, so the zero is a measurement over a populated set
rather than over an empty one.

## What this establishes

Batch C changed no C# compilation input, so the green analyzer and nullable builds P2-T5
and P2-T6 recorded still describe the current tree. No solution-wide `/t:Rebuild` is run in
this phase, and CMD-OUTLOOK therefore does not bind here: its scope is the two solution-wide
rebuild commands only, which are the seven tasks P0-T11, P0-T12, P1-T14, P2-T5, P2-T6,
P9-T5 and P9-T6.
