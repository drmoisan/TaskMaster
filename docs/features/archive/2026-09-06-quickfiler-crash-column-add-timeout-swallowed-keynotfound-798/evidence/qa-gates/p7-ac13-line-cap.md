# Phase 7 — AC13 500-line-cap audit over the eleven write-set `.cs` files

Timestamp: 2026-09-07T03-21
Task: [P7-T4]
Issue: #798

Host-specific absolute paths are redacted to a `<worktree>` token. Counts were taken at this position
in the task sequence, immediately after the P7-T1 commit and before the P7-T8 formatting pass. This
task is not deferred: P7-T8 re-runs it if its format pass rewrites a file, and P8-T9 re-verifies it
after the final formatting pass.

## Command

Command: a single `pwsh -NoProfile -Command` invocation that, for each of the eleven write-set `.cs`
paths, reports `(Get-Content -LiteralPath <worktree>/<path>).Count`.
EXIT_CODE: 0

## Observed counts, one `path=count` line per file

```
UtilitiesCS/Extensions/DfDeedle.cs=314
UtilitiesCS/Extensions/DfDeedle.QfcColumns.cs=297
QuickFiler/Controllers/QfcDatamodel.FrameBuilding.cs=154
QuickFiler/Controllers/QfcDatamodel.cs=483
TaskMaster/Ribbon/RibbonCommandBoundary.cs=176
TaskMaster/Ribbon/RibbonViewer.cs=432
UtilitiesCS.Test/Extensions/DfDeedleQfcColumnTimeoutTests.cs=500
UtilitiesCS.Test/Extensions/DfDeedleRequiredColumnValidationTests.cs=218
UtilitiesCS.Test/Extensions/DfDeedle_COM_Tests.cs=869
TaskMaster.Test/Ribbon/RibbonCommandBoundaryTests.cs=249
QuickFiler.Test/Controllers/QfcDatamodelRethrowTests.cs=157
```

## Verdict per file

| Path | Count | Rule applied | Result |
|---|---|---|---|
| `UtilitiesCS/Extensions/DfDeedle.cs` | 314 | at or below 500 | PASS |
| `UtilitiesCS/Extensions/DfDeedle.QfcColumns.cs` | 297 | at or below 500 | PASS |
| `QuickFiler/Controllers/QfcDatamodel.FrameBuilding.cs` | 154 | at or below 500 | PASS |
| `QuickFiler/Controllers/QfcDatamodel.cs` | 483 | at or below 500 | PASS |
| `TaskMaster/Ribbon/RibbonCommandBoundary.cs` | 176 | at or below 500 | PASS |
| `TaskMaster/Ribbon/RibbonViewer.cs` | 432 | at or below 500 | PASS |
| `UtilitiesCS.Test/Extensions/DfDeedleQfcColumnTimeoutTests.cs` | 500 | at or below 500 | PASS (at the cap, zero headroom) |
| `UtilitiesCS.Test/Extensions/DfDeedleRequiredColumnValidationTests.cs` | 218 | at or below 500 | PASS |
| `UtilitiesCS.Test/Extensions/DfDeedle_COM_Tests.cs` | 869 | strictly below the base-commit value of 882 | PASS (869 < 882) |
| `TaskMaster.Test/Ribbon/RibbonCommandBoundaryTests.cs` | 249 | at or below 500 | PASS |
| `QuickFiler.Test/Controllers/QfcDatamodelRethrowTests.cs` | 157 | at or below 500 | PASS |

Ten of the eleven files are at or below the absolute 500-line cap. The eleventh,
`UtilitiesCS.Test/Extensions/DfDeedle_COM_Tests.cs`, is held to the strictly-decreasing rule.

## Restatement of the AC13 condition recorded in P0-T12

P0-T12 recorded that `UtilitiesCS.Test/Extensions/DfDeedle_COM_Tests.cs` stood at 882 lines at the
base commit c431dc32 and was therefore already over the repository's 500-line cap before this change
touched it. The violation is pre-existing and is not created by this work.

AC13 requires only that this one file's line count **strictly decrease** relative to its base-commit
value of 882, rather than meeting the absolute cap. The reason is that bringing the file under 500
lines would require splitting it into at least one additional file, which would be a seventeenth path
outside the sixteen-path write set that spec.md fixes and that AC13 pins, and which would break the
write-set gate at P7-T2. The repository's bugfix workflow prohibits that kind of opportunistic
refactor within a targeted defect fix.

The observed count of 869 is strictly below 882, a decrease of 13 lines, delivered by the P1-T11
reflection-test repair that removed the `GetAddQfcColumnsAsyncMethod` helper and its two local
bindings and replaced two reflective invocations with direct calls.

**This satisfies AC13. No deviation is recorded.** The pre-existing violation is promoted as a
follow-up by P9-T16, per AC13's final clause.

## Deviations

None.

Output Summary: All eleven write-set `.cs` files pass the AC13 line-count rule. Ten files are at or
below the absolute 500-line cap, with `UtilitiesCS.Test/Extensions/DfDeedleQfcColumnTimeoutTests.cs`
sitting exactly at the cap at 500 lines and `QuickFiler/Controllers/QfcDatamodel.cs` next closest at
483. `UtilitiesCS.Test/Extensions/DfDeedle_COM_Tests.cs` stands at 869, strictly below its
base-commit value of 882, which is the condition AC13 sets for that one pre-existing over-cap file.
This satisfies AC13 and records no deviation.

---

## Second observation — post-final-formatting-pass re-verification

Timestamp: 2026-09-07T05-50
Task: [P8-T9]

Re-run after the final Phase 8 formatting pass recorded in `final-csharpier.md`, which is the last
event in this plan capable of changing a line count.

Command: a single `pwsh -NoProfile -File` invocation that, for each of the eleven write-set `.cs`
paths, reports `(Get-Content -LiteralPath <worktree>/<path>).Count`.
EXIT_CODE: 0

### Observed counts, one `path=count` line per file

```
UtilitiesCS/Extensions/DfDeedle.cs=314
UtilitiesCS/Extensions/DfDeedle.QfcColumns.cs=297
QuickFiler/Controllers/QfcDatamodel.FrameBuilding.cs=154
QuickFiler/Controllers/QfcDatamodel.cs=483
TaskMaster/Ribbon/RibbonCommandBoundary.cs=176
TaskMaster/Ribbon/RibbonViewer.cs=432
UtilitiesCS.Test/Extensions/DfDeedleQfcColumnTimeoutTests.cs=500
UtilitiesCS.Test/Extensions/DfDeedleRequiredColumnValidationTests.cs=218
UtilitiesCS.Test/Extensions/DfDeedle_COM_Tests.cs=869
TaskMaster.Test/Ribbon/RibbonCommandBoundaryTests.cs=249
QuickFiler.Test/Controllers/QfcDatamodelRethrowTests.cs=157
```

### Comparison against the P7-T4 observation

Every one of the eleven counts is **unchanged**. This is the expected result: neither the P7-T8
formatting pass nor the P8-T1 formatting pass rewrote any file, each confirmed by a
`dotnet tool run csharpier check .` that exited 0 before the format command ran.

### Verdict

- Every file other than `UtilitiesCS.Test/Extensions/DfDeedle_COM_Tests.cs` is at or below 500 lines.
  The closest to the cap is `UtilitiesCS.Test/Extensions/DfDeedleQfcColumnTimeoutTests.cs` at exactly
  500, which satisfies "at or below 500" with zero headroom remaining.
- `UtilitiesCS.Test/Extensions/DfDeedle_COM_Tests.cs` stands at 869, strictly below its base-commit
  value of 882, which is the rule AC13 applies to that one pre-existing over-cap file.

The P8-T9 line-cap condition is satisfied. No deviation is recorded.
