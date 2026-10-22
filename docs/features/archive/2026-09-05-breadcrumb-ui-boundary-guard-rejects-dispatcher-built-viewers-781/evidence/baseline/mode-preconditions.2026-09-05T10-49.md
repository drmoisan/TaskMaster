# Minor-Audit Mode Preconditions (issue #781)

Timestamp: 2026-09-05T16-15

Task: [P0-T2]

Command: `pwsh -NoProfile -Command` over a block that reads
`docs/features/active/2026-09-05-breadcrumb-ui-boundary-guard-rejects-dispatcher-built-viewers-781/issue.md`
with `Get-Content -LiteralPath`, counts exact-match lines for the work-mode marker and the
acceptance-criteria heading, counts lines beginning `- [ ] AC<n>: ` and `- [x] AC<n>: ` for
`n` in 1 through 8, and tests for the existence of `spec.md` and `user-story.md` in the same
folder. Run from the repository root; no host path is recorded.

EXIT_CODE: 0

## Precondition Results

| # | Required condition | Observed | Verdict |
| --- | --- | --- | --- |
| 1 | `issue.md` contains the exact line `- Work Mode: minor-audit` | exact-match count 1 | PASS |
| 2 | `issue.md` contains a heading line whose text is exactly `## Acceptance Criteria` | exact-match count 1 | PASS |
| 3 | Each of `- [ ] AC1: ` through `- [ ] AC8: ` present exactly once and unchecked | see table below | PASS |
| 4 | Neither `spec.md` nor `user-story.md` exists in the feature folder | both `False` | PASS |

### Acceptance-criteria line state

| Criterion | Lines beginning `- [ ] AC<n>: ` | Lines beginning `- [x] AC<n>: ` |
| --- | --- | --- |
| AC1 | 1 | 0 |
| AC2 | 1 | 0 |
| AC3 | 1 | 0 |
| AC4 | 1 | 0 |
| AC5 | 1 | 0 |
| AC6 | 1 | 0 |
| AC7 | 1 | 0 |
| AC8 | 1 | 0 |

Output Summary: All four minor-audit preconditions pass. The work-mode marker and the
`## Acceptance Criteria` heading are each present exactly once; all eight acceptance criteria
AC1 through AC8 are present exactly once and all eight are unchecked; and neither `spec.md` nor
`user-story.md` exists in the active feature folder. `MODE PRECONDITION FAILED` is not reported.
Execution continues to [P0-T3].
