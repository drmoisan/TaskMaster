# [P0-T7] CSharpier baseline check

Timestamp: 2026-09-06T14-27

Command: `dotnet tool run csharpier check .`

EXIT_CODE: 0

Verbatim printed line:

```
Checked 1583 files in 4650ms.
```

BASELINE-CSHARPIER-CHECKED-FILES: 1583

Output Summary: The check is read-only and returns non-zero on drift, so exit 0 with the single
success line is the clean-tree observation. No drifting path was reported, so there is no
pre-existing drift set to disclose. The tree is formatter-clean at `BASE-SHA`.

This is the number [P3-T2] compares against. Four new `.cs` files are added by this plan
([P1-T6], [P1-T12], [P1-T13], [P1-T14]), so the expected final count is 1587, a delta of 4.
