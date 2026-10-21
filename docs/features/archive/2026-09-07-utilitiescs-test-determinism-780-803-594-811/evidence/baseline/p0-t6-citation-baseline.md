# P0-T6 — Citation baseline against the merged tree

Timestamp: 2026-09-08T09-20
Task: [P0-T6]
Command: pwsh -NoProfile -File coverage/plan811-helper.ps1 — `(Get-Content -LiteralPath <path>).Count` for line counts, `@(Select-String -LiteralPath <path> -CaseSensitive -SimpleMatch '<token>').Count` for token counts, `Test-Path` for the three new files
EXIT_CODE: 0

MISMATCH_COUNT: 0

No `CITATION-MISMATCH:` line was produced. Execution proceeds; the plan's one permitted halt did
not fire. All measurements were taken against the merged base tree
`bb1c7d4b60f7b782227956f36859314d5c47bb03` (decision D16), before any source edit.

## Line counts (17 of 17 match)

| Path | Expected | Observed |
|---|---|---|
| `UtilitiesCS/Extensions/DictionaryExtensions.cs` | 282 | 282 |
| `UtilitiesCS/OutlookObjects/Table/OlTableExtensions.Etl.cs` | 474 | 474 |
| `UtilitiesCS/Extensions/DfDeedle.cs` | 314 | 314 |
| `UtilitiesCS/Extensions/DfDeedle.FrameUtilities.cs` | 276 | 276 |
| `UtilitiesCS/OutlookObjects/Filter DASL/DASLFilterParser.cs` | 122 | 122 |
| `UtilitiesCS/HelperClasses/PrettyPrint.cs` | 680 | 680 |
| `UtilitiesCS/OutlookObjects/Table/OlTableExtensions.TableAccess.cs` | 430 | 430 |
| `UtilitiesCS/ReusableTypeClasses/Other/StackGeek.cs` | 199 | 199 |
| `UtilitiesCS.Test/Extensions/DictionaryExtensions_Tests.cs` | 296 | 296 |
| `UtilitiesCS.Test/Extensions/DfDeedle_COM_Tests.cs` | 869 | 869 |
| `UtilitiesCS.Test/Extensions/DfDeedleQfcColumnTimeoutTests.cs` | 500 | 500 |
| `UtilitiesCS.Test/ReusableTypeClasses/StackGeek_Tests.cs` | 276 | 276 |
| `UtilitiesCS.Test/HelperClasses/PrettyPrint_Tests.cs` | 408 | 408 |
| `UtilitiesCS.Test/OutlookObjects/Filter DASL/DASLFilterParserTests.cs` | 125 | 125 |
| `UtilitiesCS.Test/OutlookObjects/Table/OlTableExtensions_Tests.cs` | 1855 | 1855 |
| `UtilitiesCS.Test/HelperClasses/NLogTraceWriter_Test.cs` | 119 | 119 |
| `UtilitiesCS/Threading/TimeOutTask.cs` | 1011 | 1011 |

## New files absent (3 of 3 match)

| Path | Expected | Observed |
|---|---|---|
| `UtilitiesCS.Test/Extensions/DfDeedleEtlTimeoutTests.cs` | ABSENT | ABSENT |
| `UtilitiesCS.Test/OutlookObjects/Table/OlTableExtensionsEtlClockTests.cs` | ABSENT | ABSENT |
| `UtilitiesCS.Test/TestHelpers/ArmingBarrierTimeProvider.cs` | ABSENT | ABSENT |

## Token counts (42 of 42 match)

All counts are case-sensitive `-SimpleMatch` line counts, per C2.

| File | Token | Expected | Observed |
|---|---|---|---|
| `DictionaryExtensions.cs` | `CancelAfter(` | 1 | 1 |
| `DictionaryExtensions.cs` | `CreateLinkedTokenSource` | 1 | 1 |
| `OlTableExtensions.Etl.cs` | `var attempts = 3;` | 2 | 2 |
| `OlTableExtensions.Etl.cs` | `TimeoutAfter(milliseconds, attempts)` | 2 | 2 |
| `OlTableExtensions.Etl.cs` | `TimeoutAfter(timeout, attempts)` | 2 | 2 |
| `OlTableExtensions.Etl.cs` | `timed out {attempts} times` | 2 | 2 |
| `OlTableExtensions.Etl.cs` | `DateTime.Now` | 2 | 2 |
| `OlTableExtensions.Etl.cs` | `TimeProvider` | 0 | 0 |
| `DfDeedle.cs` | `TableEtlInvoker` | 3 | 3 |
| `DfDeedle.cs` | `TimeoutAfter(1000, 2)` | 2 | 2 |
| `DfDeedle.cs` | `TimeProvider` | 0 | 0 |
| `DfDeedle.cs` | `was not produced` | 0 | 0 |
| `DfDeedle.FrameUtilities.cs` | `StoreTableEtlInvoker` | 1 | 1 |
| `DASLFilterParser.cs` | `Console.WriteLine` | 1 | 1 |
| `DASLFilterParser.cs` | `TextWriter` | 0 | 0 |
| `PrettyPrint.cs` | `Console.WriteLine` | 2 | 2 |
| `PrettyPrint.cs` | `TextWriter` | 0 | 0 |
| `OlTableExtensions.TableAccess.cs` | `Console.WriteLine` | 5 | 5 |
| `OlTableExtensions.TableAccess.cs` | `TextWriter` | 0 | 0 |
| `StackGeek.cs` | `Console.WriteLine` | 7 | 7 |
| `StackGeek.cs` | `public static void Run(` | 0 | 0 |
| `DfDeedle_COM_Tests.cs` | `TableEtlInvoker` | 14 | 14 |
| `DfDeedle_COM_Tests.cs` | `FakeTimeProvider` | 0 | 0 |
| `DfDeedleQfcColumnTimeoutTests.cs` | `class ArmingBarrierTimeProvider` | 1 | 1 |
| `StackGeek_Tests.cs` | `Console.SetOut` | 3 | 3 |
| `StackGeek_Tests.cs` | `DoNotParallelize` | 1 | 1 |
| `PrettyPrint_Tests.cs` | `Console.SetOut` | 3 | 3 |
| `PrettyPrint_Tests.cs` | `DoNotParallelize` | 2 | 2 |
| `DASLFilterParserTests.cs` | `Console.SetOut` | 3 | 3 |
| `DASLFilterParserTests.cs` | `DoNotParallelize` | 1 | 1 |
| `OlTableExtensions_Tests.cs` | `Console.SetOut` | 2 | 2 |
| `OlTableExtensions_Tests.cs` | `DoNotParallelize` | 1 | 1 |
| `OlTableExtensions_Tests.cs` | `Returns(120)` | 1 | 1 |
| `OlTableExtensions_Tests.cs` | `cannot fire under test-host` | 1 | 1 |
| `OlTableExtensions_Tests.cs` | `redirects Console.Out` | 1 | 1 |
| `OlTableExtensions_Tests.cs` | `FakeTimeProvider` | 0 | 0 |
| `NLogTraceWriter_Test.cs` | `Console.SetOut` | 2 | 2 |
| `NLogTraceWriter_Test.cs` | `originalOut` | 3 | 3 |
| `NLogTraceWriter_Test.cs` | `TestCleanup` | 2 | 2 |
| `UtilitiesCS.Test.csproj` | `Extensions\DfDeedleEtlTimeoutTests.cs` | 0 | 0 |
| `UtilitiesCS.Test.csproj` | `OutlookObjects\Table\OlTableExtensionsEtlClockTests.cs` | 0 | 0 |
| `UtilitiesCS.Test.csproj` | `TestHelpers\ArmingBarrierTimeProvider.cs` | 0 | 0 |

## Output Summary

62 assertions evaluated (17 line counts, 3 absence checks, 42 token counts). All 62 matched the
plan's stated values exactly. `MISMATCH_COUNT: 0`. The re-anchor recorded in D16 did not move any
value this task asserts: the merge changed only `UtilitiesCS.Test/UtilitiesCS.Test.csproj`, whose
line count is not asserted here, and the three project-file token counts it does assert are all
zero-hit checks for items the plan has yet to add.
