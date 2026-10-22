# P0-T3 spec verification (issue #973; read-only)

Timestamp: 2026-10-03T10-42
Command: Grep tool over docs/features/active/2026-10-02-remaining-stale-binding-redirect-pairs-973/spec.md, twelve patterns listed below (count mode; the ConditionalEngine pattern additionally in content mode with -n)
EXIT_CODE: 0
Output Summary: all twelve counts equal the plan's stated values; spec.md is at version 1.1 with 23 unchecked criteria, Planner Amendments 4 and 5 present; no SPEC MISMATCH.

| # | Pattern | Expected | Observed |
|---|---|---|---|
| 1 | `^- \[[ x]\] AC[0-9]+ ` | 23 | 23 |
| 2 | `csproj Reference map` | at least 2 | 3 |
| 3 | `0.10 percentage points` | at least 1 | 2 |
| 4 | `^## Planner Amendments$` | 1 | 1 |
| 5 | `^4\. AC14 route` | 1 | 1 |
| 6 | `^- \[x\] AC` | 0 | 0 |
| 7 | `^## Scope Amendment Log$` | 1 | 1 |
| 8 | `^- \[ \] AC23 ` | 1 | 1 |
| 9 | `CategoryClassifierGroup\.ConditionalEngine\.cs` | 4 | 4 (lines 72, 161, 271, 379) |
| 10 | `^- Part F, unused Graph directives` | 1 | 1 |
| 11 | `^- \*\*Version:\*\* 1\.1` | 1 | 1 |
| 12 | `^5\. AC16, AC23` | 1 | 1 |

Note: pattern 9 matches at line 379 for AC23 (the plan's round-3 locator lists 378); the gated value is the count, which is 4 as stated.
