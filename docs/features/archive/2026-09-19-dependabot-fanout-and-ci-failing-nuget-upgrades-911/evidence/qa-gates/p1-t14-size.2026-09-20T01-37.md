# Phase 1 File-Size Audit — Remediation Cycle 1, Issue #911

- Timestamp: 2026-09-20T08-47-21
- Task: [P1-T14]
- Finding: R9d
- EXIT_CODE: 0

## Measurement

File: `tests/scripts/vscode/Sync-PackageReferences.Tests.ps1`

```
([System.IO.File]::ReadAllLines((Resolve-Path <path>).ProviderPath)).Count
```

| Measurement | Value |
|---|---|
| [P0-T5] baseline | **185** |
| After Phase 1 | **393** |
| Difference | **+208** |

## Acceptance

| Clause | Required | Measured | Result |
|---|---|---|---|
| The count is an integer | yes | 393 | PASS |
| At most 470 | <= 470 | 393 | PASS, 77 lines of headroom |
| Strictly greater than the [P0-T5] value | > 185 | 393 | PASS |

The two-sided check is the point. A ceiling alone is satisfied by a phase that added nothing; the
strictly-greater clause fails if the eight `It` blocks were not in fact added to this file. The
measured `+208` is consistent with eight tests carrying Arrange-Act-Assert structure, seam
delegates and rationale comments.

The 470 ceiling leaves 30 lines below the 500-line cap in `.claude/rules/general-code-change.md`.
The file sits 107 lines under that cap.

No split was required. Had the count exceeded 470 this task would have halted and reported rather
than splitting the file, because a new test file is outside the spec `## Write Set`.

## Output Summary

`tests/scripts/vscode/Sync-PackageReferences.Tests.ps1` measures **393** lines, up from 185, under
both the 470 phase ceiling and the 500-line repository cap.
