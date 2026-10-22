# File-length audit of the six plan-owned .cs files after formatting (P6-T7)

Timestamp: 2026-09-02T23-58

EXIT_CODE: 0

Command:

```
foreach ($f in @(
  'TaskMaster/AppGlobals/NonBlockingDelay.cs',
  'TaskMaster.Test/AppGlobals/NonBlockingDelayTests.cs',
  'UtilitiesCS.Test/NoLiveFormInTestAssemblyTests.cs',
  'SVGControl.Test/NoLiveFormInTestAssemblyTests.cs',
  'UtilitiesCS.Test/ReusableTypeClasses/StackGeek_Tests.cs',
  'UtilitiesCS.Test/OutlookObjects/Filter DASL/DASLFilterParserTests.cs')) {
    (Get-Content -Path $f).Count
}
```

The counts are taken after the P6-T1 scope-locked `csharpier format` pass, whose final run
recorded `RewrittenFileCount: 0`, so these are the formatted lengths.

## Line counts

| File | Lines | Limit | Result |
|---|---|---|---|
| `TaskMaster/AppGlobals/NonBlockingDelay.cs` | 91 | 500 | PASS |
| `TaskMaster.Test/AppGlobals/NonBlockingDelayTests.cs` | 129 | 500 | PASS |
| `UtilitiesCS.Test/NoLiveFormInTestAssemblyTests.cs` | 56 | 500 | PASS |
| `SVGControl.Test/NoLiveFormInTestAssemblyTests.cs` | 56 | 500 | PASS |
| `UtilitiesCS.Test/ReusableTypeClasses/StackGeek_Tests.cs` | 276 | 500 | PASS |
| `UtilitiesCS.Test/OutlookObjects/Filter DASL/DASLFilterParserTests.cs` | 125 | 500 | PASS |

Six counts recorded. The largest is 276, which is 224 lines below the 500-line ceiling set by
`.claude/rules/general-code-change.md` and by `CLAUDE.md` section 4.

Output Summary: Six file lengths recorded — 91, 129, 56, 56, 276, and 125. Every count is at or
below 500, so the repository file-size limit holds for all six plan-owned `.cs` files.
