# File-Size Audit — Issue #553

- Timestamp: 2026-08-14T11-14 (local) / 2026-08-14T15:14:22Z (UTC)
- Task: [P5-T4]
- Governing rule: `.claude/rules/general-code-change.md` § File Size Limit — no
  production code, test code, or reusable script file may exceed **500 lines**.

Command:

```powershell
Get-ChildItem .github/workflows/*.yml, .github/workflows/README.md |
    ForEach-Object { "{0}`t{1}" -f $_.Name, (Get-Content $_.FullName).Count }
```

EXIT_CODE: 0

## Output Summary

| File | Lines | Limit | Margin | Status |
| --- | --- | --- | --- | --- |
| `_actionlint.yml` | 29 | 500 | 471 | PASS |
| `ci.yml` | 32 | 500 | 468 | PASS |
| `_format-check.yml` | 41 | 500 | 459 | PASS |
| `_build-analyzers.yml` | 53 | 500 | 447 | PASS |
| `_build-nullable.yml` | 60 | 500 | 440 | PASS |
| `_mstest-coverage.yml` | 96 | 500 | 404 | PASS |
| `codex-web-setup-test.yml` (untouched) | 110 | 500 | 390 | PASS |
| `README.md` | 195 | 500 | 305 | PASS |

**Every listed file is under 500 lines.** The largest authored file is
`README.md` at 195 lines; the largest workflow file is `_mstest-coverage.yml` at
96 lines. Markdown documentation is exempt from the limit under the rule's stated
exceptions, so `README.md` would pass regardless, but it is measured and reported
rather than assumed.

Measured values against the plan's expectations: each callee < 100 (largest 96),
`ci.yml` ~32 (exactly 32), README < 150 (**195 — exceeds the plan's expectation
but well within the 500-line limit**). The README grew past the planner's rough
estimate because the review-finding F2 fix added the verified five-context list
and the capture guidance. The expectation was an estimate, not a constraint; the
governing 500-line limit is satisfied with 305 lines of margin.

## Decomposition effect

The change replaced one 160-line monolith with an orchestrator plus five focused
callees:

| | Before | After |
| --- | --- | --- |
| `ci.yml` | 160 lines, 2 jobs, all gates inline | 32 lines, 5 jobs, zero inline steps |
| Callee files | none | 5 files, 29–96 lines each, 279 lines total |

Total workflow YAML rose from 160 to 311 lines, which is the expected cost of
per-job setup no longer being shared. Each file now has a single responsibility,
consistent with the module-cohesion guidance in the same rule.

## Acceptance ([P5-T4])

- Every listed file is under 500 lines.
