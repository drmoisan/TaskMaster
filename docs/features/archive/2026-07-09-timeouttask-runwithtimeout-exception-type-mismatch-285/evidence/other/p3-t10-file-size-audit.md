# P3-T10 — File-Size Audit After the Final Format Pass

Timestamp: 2026-09-01T08-29

Command:

```text
(Get-Content -LiteralPath UtilitiesCS/Threading/TimeOutTask.cs).Count
(Get-Content -LiteralPath UtilitiesCS.Test/Threading/TimeOutTask_OverloadCoverageTests.cs).Count
```

Both measured after the P3-T1 repository-wide format pass, so the figures are the formatter's final
output rather than a pre-format intermediate.

EXIT_CODE: 0

## Measurements

| File | At merge base | **After change** | Growth | 500-line ceiling |
| --- | --- | --- | --- | --- |
| `UtilitiesCS/Threading/TimeOutTask.cs` | 993 | **1011** | +18 | **exceeded (pre-existing)** |
| `UtilitiesCS.Test/Threading/TimeOutTask_OverloadCoverageTests.cs` | 387 | **427** | +40 | within ceiling |

## Test File

The recorded test-file line count is **427**, which is **at most 500**. The file remains under the
repository's 500-line ceiling after the change, with 73 lines of headroom.

This is also the reason the new regression test was added to this file rather than to
`UtilitiesCS.Test/Threading/TimeOutTask_AdditionalTests.cs`: that sibling is already 527 lines and
adding to it would have deepened an existing breach.

## Production File — Pre-Existing Deviation Statement

The recorded production-file line count is **1011**.

**`UtilitiesCS/Threading/TimeOutTask.cs` already exceeded the 500-line ceiling in
`.claude/rules/general-code-change.md` at the merge base, with 993 lines.** The growth attributable
to this change is the difference between the two figures, **1011 minus 993 = 18 lines**. **The breach
is a pre-existing condition that cannot be corrected inside this item's scope boundary**, because
splitting the file would create a path outside the three permitted paths — the production file, the
test file, and the feature folder — and any modification outside those three paths is a scope
violation while this item runs concurrently with other items against the same `main`.

The 18 lines consist of the 5-line explanatory comment plus widened clause replacing a 1-line clause
(+4), the 3-line seam construction replacing a 1-line construction (+2), two new parameter
declarations (+2), and the expansion of the wrapper's single-line forwarding call and the recursion's
argument list into multi-line form by CSharpier (+10).

### Disposition

This deviation is recorded, not fixed here. It is a candidate follow-up issue alongside the spec's
existing Non-Goals list, which already carries four other defects resident in this same file plus the
527-line `TimeOutTask_AdditionalTests.cs` breach. The spec's Rollout & Follow-up section directs that
each non-goal be promoted to its own issue through the standard promotion lifecycle after merge; this
file-size deviation belongs with that set.

Output Summary: The test file is 427 lines, within the 500-line ceiling. The production file is 1011
lines and exceeds it, having already been 993 lines at the merge base; the change accounts for 18 of
those lines. The breach is pre-existing and out of scope for this item.

Acceptance: met. The recorded test-file line count (427) is at most 500. The recorded production-file
line count (1011) is recorded together with the statement that the file already exceeded the 500-line
ceiling at the merge base with 993 lines, that the growth attributable to this change is the
difference between the two figures (18 lines), and that the breach is a pre-existing condition that
cannot be corrected inside this item's scope boundary.
