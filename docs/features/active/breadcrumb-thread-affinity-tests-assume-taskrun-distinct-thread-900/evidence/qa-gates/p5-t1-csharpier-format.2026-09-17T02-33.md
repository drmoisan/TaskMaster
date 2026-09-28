# P5-T1 — Repository-Wide Formatter (loop iteration 2)

Timestamp: 2026-09-17T02-33

Command: `dotnet tool run csharpier format .` run from the worktree root, with
`git diff 66b65a4626095ade5a01643aee4a43c90cc58cbf -- QuickFiler QuickFiler.Test` captured
immediately before and immediately after it.

EXIT_CODE: 0

CHANNEL: COMMAND

## Output Summary

Tool summary line, verbatim:

    Formatted 1641 files in 2719ms.

That line is a processed-file count, not a rewrite count.

FORMAT_CHANGED_TREE: False

PATCH-HASH-BEFORE: 929E200CE7116D4EFC5B9CA4D3FDABC64D7F3E0D50D9631CD2BEE53C3A3E6A98

PATCH-HASH-AFTER: 929E200CE7116D4EFC5B9CA4D3FDABC64D7F3E0D50D9631CD2BEE53C3A3E6A98

The two values are the SHA-256 of the anchored patch text captured before and after the command,
taken by writing the `git diff` output to a git-ignored file under `coverage/` and hashing it with
the same `Get-FileHash -Algorithm SHA256 -LiteralPath` form used everywhere else in this plan. They
are equal, so the formatter rewrote nothing under `QuickFiler` or `QuickFiler.Test`.

Both hashes are also identical to the values recorded in iteration 1, which confirms the tree was
not altered between iterations by anything the loop restart did.

## Acceptance

Both conditions hold: `EXIT_CODE: 0`, and `FORMAT_CHANGED_TREE:` is recorded as `False`. No
`FORMAT SCOPE BREACH` analysis is required.

Supporting observations:

    CHANGED_NAME_COUNT: 1
    CHANGED_NAME QuickFiler.Test/Viewers/ItemViewerBreadcrumbThreadAffinityTests.cs
    PORCELAIN_LINE_COUNT: 0
    COVERAGE_XML_EXISTS: False

`artifacts/csharp/coverage.xml` still does not exist at this point in the loop, as D-12 requires.

## Loop context

This is iteration 2 of the P5-T1 through P5-T7 loop. Iteration 1 reached P5-T5 and failed there on
an environmental file-contention failure in an unrelated `UtilitiesCS.Test` test; the plan's rule is
that any failing step restarts the loop from P5-T1. The iteration 1 record, including the values
this step produced then, is
`evidence/other/p5-t5-iteration-1-environmental-failure.2026-09-17T02-32.md`.

## Build lock

This task ran inside a held shared build lock for item 900.
