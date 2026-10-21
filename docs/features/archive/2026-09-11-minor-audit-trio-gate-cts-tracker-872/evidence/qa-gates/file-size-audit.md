# File-Size Audit After The Final Formatter Pass

Timestamp: 2026-09-13T15-47
Task: [P2-T17]

Verdict: PASS

Command: pwsh -Command 'foreach ($p in @("QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.Part4.cs","UtilitiesCS/Threading/ProgressPackage.cs","UtilitiesCS.Test/Threading/ProgressPackage_Tests.cs","UtilitiesCS/EmailIntelligence/SubjectMap/SubjectMapSco.Orchestration.cs","UtilitiesCS/UtilitiesCS.csproj","UtilitiesCS.Test/UtilitiesCS.Test.csproj")) { $p + ": " + (Get-Content -Path $p).Count }'
EXIT_CODE: 0

## Measured Line Counts

| Path | P0-T13 base | Post-change | Limit | Within limit |
|---|---|---|---|---|
| QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.Part4.cs | 347 | 450 | 500 | yes |
| UtilitiesCS/Threading/ProgressPackage.cs | 150 | 187 | 500 | yes |
| UtilitiesCS.Test/Threading/ProgressPackage_Tests.cs | 120 | 220 | 500 | yes |
| UtilitiesCS/EmailIntelligence/SubjectMap/SubjectMapSco.Orchestration.cs | 274 | 274 | 500 | yes |
| UtilitiesCS/UtilitiesCS.csproj | not pinned | 1321 | not applicable | measured for completeness |
| UtilitiesCS.Test/UtilitiesCS.Test.csproj | not pinned | 996 | not applicable | measured for completeness |

All four C# source files report a line count of 500 or fewer. The two project files are recorded for
completeness; the repository's 500-line limit does not apply to them.

## Timing

This audit ran after the P2-T1 formatter pass of the completing loop, which is required because a size
measured before the formatter runs is not the size the repository limit applies to. The measurement is
not hypothetical on this run: the superseded pass 1 of P2-T1 rewrote
`UtilitiesCS.Test/Threading/ProgressPackage_Tests.cs`, moving it from 216 to 220 lines. Had this audit
run before that pass it would have recorded a stale figure for that file.

## Margin On The Largest File

The QuickFiler test part file is the file nearest the limit at 450 lines, 50 below it. That falls
inside the projected band of 448 to 458 against a base of 347. No file exceeds 500, so the BLOCKED
condition does not arise and no new part file is needed. A new part file would have required editing
the QuickFiler test project file to add a Compile item, and that project file is outside the Write Set.
