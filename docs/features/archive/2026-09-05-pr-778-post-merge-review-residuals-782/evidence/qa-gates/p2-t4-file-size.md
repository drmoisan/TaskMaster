# QA Gate — Post-Split File Sizes (P2-T4)

Timestamp: 2026-09-05T22-04

Command:

```powershell
(Get-Content -LiteralPath 'UtilitiesCS.Test/Threading/ProgressTracker_Tests.cs').Count
(Get-Content -LiteralPath 'UtilitiesCS.Test/Threading/ProgressTracker_ReportAndViewerTests.cs').Count
```

EXIT_CODE: 0

Output Summary:

| File | Counting command | Projected | Observed | Deviation |
|---|---|---|---|---|
| `UtilitiesCS.Test/Threading/ProgressTracker_Tests.cs` | `(Get-Content -LiteralPath 'UtilitiesCS.Test/Threading/ProgressTracker_Tests.cs').Count` | 271 | 272 | +1 |
| `UtilitiesCS.Test/Threading/ProgressTracker_ReportAndViewerTests.cs` | `(Get-Content -LiteralPath 'UtilitiesCS.Test/Threading/ProgressTracker_ReportAndViewerTests.cs').Count` | 260 | 260 | 0 |

Both observed counts are strictly less than 500 and strictly less than 300, and each is within 5
lines of its projection, so the gate passes on every condition.

The +1 deviation on `ProgressTracker_Tests.cs` is accounted for and is not an anomaly. Deleting the
moved region left the file at exactly the projected 271 lines; the C15 attribute split then replaced
the single combined line `    [TestClass, DoNotParallelize]` with the two separate lines
`    [TestClass]` and `    [DoNotParallelize]`, which adds one line. The projection was taken before
that split.

The source file was 514 lines before the split. This gate is deliberately placed after the split:
before it, the 500-line condition could not pass.

## Formatting of the new file at creation time (P2-T1)

`UtilitiesCS.Test/Threading/ProgressTracker_ReportAndViewerTests.cs` was formatted immediately after
creation, so that it cannot be the file that rewrites the tree during P7-T1 and forces a second
Phase 7 pass.

Exact format command:

```powershell
$env:DOTNET_ROOT = (Resolve-Path '.dotnet-sdk').Path
$env:PATH = "$env:DOTNET_ROOT;$env:PATH"
dotnet tool run csharpier format UtilitiesCS.Test/Threading/ProgressTracker_ReportAndViewerTests.cs
```

| Field | Value |
|---|---|
| Pre-format line count | 260 |
| Post-format line count | 260 |
| CSharpier exit code | 0 |
| CSharpier printed line | `Formatted 1 files in 1072ms.` |

The pre-format and post-format counts are identical, which indicates CSharpier made no line-count
change to the extracted region. The region was moved verbatim: it was extracted mechanically from
lines 270-512 of the source file rather than retyped, so no test method name, attribute, or
assertion was altered by the move.
