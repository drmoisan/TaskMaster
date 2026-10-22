# [P4-T7] Post-fix file sizes against the 500-line cap (Issue 638)

Timestamp: 2026-08-29T12-34

Command:

```
(Get-Content -LiteralPath 'QuickFiler/Controllers/EfcDataModel.cs').Count
(Get-Content -LiteralPath 'QuickFiler.Test/Controllers/EfcDataModelArchiveRootTests.cs').Count
```

`Measure-Object -Line` was deliberately not used: it reports a different figure for a file
with a trailing newline.

EXIT_CODE: 0

Output Summary:

POSTFIX_EFCDATAMODEL_LINE_COUNT: 485

POSTFIX_ARCHIVEROOTTESTS_LINE_COUNT: 389

Both counts are at or below the 500-line cap in `.claude/rules/general-code-change.md`, so
no tightening was required and no other file absorbed any overflow.

`QuickFiler/Controllers/EfcDataModel.cs` grew from the
`PRECHANGE_EFCDATAMODEL_LINE_COUNT: 423` recorded in [P1-T4] to 485, consuming 62 of the
77 lines of headroom and leaving 15. The growth comprises the [P2-T1] seam and its XML doc
comment, the redacted user-facing message constant, the `TryGetArchiveRoot` helper and its
XML doc comment, and the three guard blocks added by [P4-T2] through [P4-T4].

`QuickFiler.Test/Controllers/EfcDataModelArchiveRootTests.cs` landed at 389 lines, close to
the plan's budget guidance of roughly 385.

Because CSharpier can change line counts, [P8-T21] re-runs both counts after the [P6-T1]
formatting pass before checking off AC19.
