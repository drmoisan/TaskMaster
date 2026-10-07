# P2-T4 Scoped Format of U and TUL

Timestamp: 2026-10-03T08-46
Command: dotnet tool run csharpier format then dotnet tool run csharpier check over UtilitiesCS\EmailIntelligence\EmailParsingSorting\SortEmail.UndoAndMoveLog.cs and UtilitiesCS.Test\EmailIntelligence\SortEmail_UndoAndMoveLog_Tests.cs (CMD-SCOPED-FORMAT, TASKID p2-t4)
EXIT_CODE: 0 (scoped to the csharpier check invocation; the printed CHECK_EXIT_CODE)
Output Summary: U was already in CSharpier form; the new test file was rewritten (line endings); the read-only check passed.

- BEFORE UtilitiesCS\EmailIntelligence\EmailParsingSorting\SortEmail.UndoAndMoveLog.cs = 9477B5A0D1B28524FBE3CC87394B6C95474218AA103A780C31BF3C686DAA0C28
- BEFORE UtilitiesCS.Test\EmailIntelligence\SortEmail_UndoAndMoveLog_Tests.cs = 4F4A0928CD70D27542ED39B2A8853F2633E997E20E85026696553824AAD65002
- Formatter summary (observation): Formatted 2 files in 2138ms.
- FORMAT_EXIT_CODE: 0
- AFTER UtilitiesCS\EmailIntelligence\EmailParsingSorting\SortEmail.UndoAndMoveLog.cs = 9477B5A0D1B28524FBE3CC87394B6C95474218AA103A780C31BF3C686DAA0C28
- AFTER UtilitiesCS.Test\EmailIntelligence\SortEmail_UndoAndMoveLog_Tests.cs = CAE210E9A9983A8C8BB843A3E80789248D2BF3EBB6A81E097482AE71C7F06C59
- Check summary: Checked 2 files in 759ms.
- CHECK_EXIT_CODE: 0

Acceptance check: FORMAT_EXIT_CODE 0 and CHECK_EXIT_CODE 0. Holds.
