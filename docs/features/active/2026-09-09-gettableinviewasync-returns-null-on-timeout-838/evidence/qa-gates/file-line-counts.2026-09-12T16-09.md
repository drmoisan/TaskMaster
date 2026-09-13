# P4-T10 — File-size audit after the final formatter pass

Timestamp: 2026-09-13T03-18

Command: a single pwsh payload printing `(Get-Content -LiteralPath <file>).Count` for the four C# files in the Write Set. This task runs after P4-T1 and P4-T2, because a size measured before the formatter has run is not the delivered size.

EXIT_CODE: 0

| File | Lines | Limit |
|---|---|---|
| `UtilitiesCS/OutlookObjects/Table/OlTableExtensions.TableAccess.cs` | 473 | 500 |
| `UtilitiesCS/OutlookObjects/Table/OlTableExtensions.TableAccess.Failures.cs` | 33 | 500 |
| `UtilitiesCS.Test/OutlookObjects/Table/GetTableInViewAsyncFailureContractTests.cs` | 301 | 500 |
| `UtilitiesCS.Test/OutlookObjects/Table/GetTableInViewAsyncClockTests.cs` | 289 | 500 |

OVER_LIMIT_COUNT=0

Output Summary: both acceptance clauses hold. Every one of the four counts is at most 500. The file under fix grew from the 452 lines P0-T22 recorded to 473, an increase of 21 lines, and remains 27 lines below the limit; the growth is the XML documentation block, the corrected comment and the guard. The clock test file is unchanged at 289 lines, consistent with P3-T5 having replaced one comment line with one comment line. Placing the failure-construction helper in its own partial file rather than in the file under fix is part of what keeps that file under the limit. The two project files and the Markdown documents in the Write Set are outside this criterion, which the general code-change policy exempts.
