# P7-T8 — File-size audit

Timestamp: 2026-09-08T10-19
Task: [P7-T8]
Command: `(Get-Content -LiteralPath <path>).Count` for the 19 write-set `.cs` files, after the final format pass
EXIT_CODE: 0

## Measured line counts

| File | Lines | Cap | Verdict |
|---|---|---|---|
| `UtilitiesCS/Extensions/DictionaryExtensions.cs` | 281 | 500 | OK |
| `UtilitiesCS/OutlookObjects/Table/OlTableExtensions.Etl.cs` | 476 | 500 | OK |
| `UtilitiesCS/Extensions/DfDeedle.cs` | 319 | 500 | OK |
| `UtilitiesCS/Extensions/DfDeedle.FrameUtilities.cs` | 279 | 500 | OK |
| `UtilitiesCS/OutlookObjects/Filter DASL/DASLFilterParser.cs` | 123 | 500 | OK |
| `UtilitiesCS/HelperClasses/PrettyPrint.cs` | 680 | at most 680 (pre-existing violation) | OK |
| `UtilitiesCS/OutlookObjects/Table/OlTableExtensions.TableAccess.cs` | 432 | 500 | OK |
| `UtilitiesCS/ReusableTypeClasses/Other/StackGeek.cs` | 206 | 500 | OK |
| `UtilitiesCS.Test/Extensions/DictionaryExtensions_Tests.cs` | 318 | 500 | OK |
| `UtilitiesCS.Test/Extensions/DfDeedle_COM_Tests.cs` | 848 | strictly below 869 (pre-existing violation) | OK |
| `UtilitiesCS.Test/Extensions/DfDeedleEtlTimeoutTests.cs` | 228 | 500 | OK |
| `UtilitiesCS.Test/OutlookObjects/Table/OlTableExtensionsEtlClockTests.cs` | 234 | 500 | OK |
| `UtilitiesCS.Test/Extensions/DfDeedleQfcColumnTimeoutTests.cs` | 453 | strictly below 500 | OK |
| `UtilitiesCS.Test/TestHelpers/ArmingBarrierTimeProvider.cs` | 55 | 500 | OK |
| `UtilitiesCS.Test/ReusableTypeClasses/StackGeek_Tests.cs` | 279 | 500 | OK |
| `UtilitiesCS.Test/HelperClasses/PrettyPrint_Tests.cs` | 418 | 500 | OK |
| `UtilitiesCS.Test/OutlookObjects/Filter DASL/DASLFilterParserTests.cs` | 109 | 500 | OK |
| `UtilitiesCS.Test/OutlookObjects/Table/OlTableExtensions_Tests.cs` | 1846 | at most 1848 (pre-existing violation) | OK |
| `UtilitiesCS.Test/HelperClasses/NLogTraceWriter_Test.cs` | 110 | 500 | OK |

## Not touched

| File | Lines | Expected |
|---|---|---|
| `UtilitiesCS/Threading/TimeOutTask.cs` | 1011 | exactly 1011 (untouched) |

`TimeOutTask.cs` is byte-identical to the base commit; P7-T9's numstat over it is empty.

## Project file

`UtilitiesCS.Test/UtilitiesCS.Test.csproj` is 988 lines. It is recorded as exempt: the 500-line
cap in the General Code Change Policy governs production code, test code and reusable script
files, and does not reach `*.csproj`. Its growth is exactly three `<Compile Include>` items, from
985 in the merged base tree to 988.

## Acceptance evaluation

- Every file other than the three pre-existing violations is at most 500 lines. The largest is
  `OlTableExtensions.Etl.cs` at 476. PASS
- `UtilitiesCS/HelperClasses/PrettyPrint.cs` is at most 680 (it is exactly 680, unchanged). PASS
- `UtilitiesCS.Test/Extensions/DfDeedle_COM_Tests.cs` is strictly below 869 (848, a reduction of
  21). PASS
- `UtilitiesCS.Test/OutlookObjects/Table/OlTableExtensions_Tests.cs` is at most 1848 (1846, a
  reduction of 9). PASS
- `UtilitiesCS.Test/Extensions/DfDeedleQfcColumnTimeoutTests.cs` is strictly below 500 (453, a
  reduction of 47 achieved by moving `ArmingBarrierTimeProvider` out). PASS
- `UtilitiesCS/Threading/TimeOutTask.cs` is exactly 1011. PASS
- The project file is recorded as exempt from the cap. PASS

## Output Summary

No new cap violation was introduced. All three pre-existing violations shrank or held: 
`DfDeedle_COM_Tests.cs` 869 to 848, `OlTableExtensions_Tests.cs` 1855 to 1846, `PrettyPrint.cs`
680 to 680. `DfDeedleQfcColumnTimeoutTests.cs`, which was at exactly the 500-line cap and could not
grow, dropped to 453.
