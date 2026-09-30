# P4-T1 Repository-Wide CSharpier Format

Timestamp: 2026-09-29T09-33
Command: dotnet tool run csharpier format . (inside one pwsh payload whose first statement set the location to the worktree root; the payload captured Get-FileHash SHA256 of the five Write Set .cs files and the SHA-256 of the text printed by git diff MERGE-BASE -- QuickFiler.Test UtilitiesCS.Test immediately before and after the command, then ran git status --porcelain -- QuickFiler QuickFiler.Test UtilitiesCS UtilitiesCS.Test)
EXIT_CODE: 0
ITERATION: 1

Output Summary:
- Formatter summary line (verbatim): Formatted 1625 files in 5533ms.
- MERGE-BASE used: 177b6d78e1b2408e5aedbd794cef3aad6b7fb372
- BEFORE QuickFiler.Test/Viewers/ItemViewerBreadcrumbThreadAffinityTests.cs = BAD939900B47AE0AC963D203F621D28C959CC7F3E492119F1BD8272197EF5940
- BEFORE QuickFiler.Test/Viewers/ItemViewerBreadcrumbThreadAffinityTests.Part2.cs = D6A35090996E48283CA04D8EE2C441F8ED8946BAECC1D06D9F16931FF767976E
- BEFORE QuickFiler.Test/TestSupport/DedicatedWorkerThread.cs = 986838E4FD72A7E233B26A1BE03B5912DFBB4016980F515F5C01680A0ABF688E
- BEFORE QuickFiler.Test/Viewers/BreadcrumbPopupBoundaryCoverageTests.cs = 4F8B8AFFA9387043AAE3A66EF2028D96AB4F0FA77AFCE244C9044E67B9B8E0FE
- BEFORE UtilitiesCS.Test/HelperClasses/FileInfoWrapper_Tests.cs = 142BCB8E95D710890A71F325A65801ED8EB53A87A339B10018D889491ECEF596
- AFTER QuickFiler.Test/Viewers/ItemViewerBreadcrumbThreadAffinityTests.cs = BAD939900B47AE0AC963D203F621D28C959CC7F3E492119F1BD8272197EF5940
- AFTER QuickFiler.Test/Viewers/ItemViewerBreadcrumbThreadAffinityTests.Part2.cs = D6A35090996E48283CA04D8EE2C441F8ED8946BAECC1D06D9F16931FF767976E
- AFTER QuickFiler.Test/TestSupport/DedicatedWorkerThread.cs = 986838E4FD72A7E233B26A1BE03B5912DFBB4016980F515F5C01680A0ABF688E
- AFTER QuickFiler.Test/Viewers/BreadcrumbPopupBoundaryCoverageTests.cs = 4F8B8AFFA9387043AAE3A66EF2028D96AB4F0FA77AFCE244C9044E67B9B8E0FE
- AFTER UtilitiesCS.Test/HelperClasses/FileInfoWrapper_Tests.cs = 142BCB8E95D710890A71F325A65801ED8EB53A87A339B10018D889491ECEF596
- PATCH-BEFORE = 0D10D18AFF2F225DBE4A7EF5B9081B913933091C2EB07CA6F2BAAFCD0330D31F
- PATCH-AFTER = 0D10D18AFF2F225DBE4A7EF5B9081B913933091C2EB07CA6F2BAAFCD0330D31F
- REWRITTEN: 0
- FORMAT_CHANGED_TREE: NO (the two anchored-patch hashes are equal)
- Porcelain output (git status --porcelain -- QuickFiler QuickFiler.Test UtilitiesCS UtilitiesCS.Test): EMPTY
- Fact 23 prediction (no line-ending rewrite of a Write Set file): MET (REWRITTEN: 0)
- FORMAT SCOPE BREACH: not fired. FORMAT NOT IDEMPOTENT: not applicable (no rewrite).

Acceptance: EXIT_CODE 0; FORMAT_CHANGED_TREE and REWRITTEN recorded; porcelain lists no path outside the Write Set (it is empty). All three hold. No loop restart is required.
