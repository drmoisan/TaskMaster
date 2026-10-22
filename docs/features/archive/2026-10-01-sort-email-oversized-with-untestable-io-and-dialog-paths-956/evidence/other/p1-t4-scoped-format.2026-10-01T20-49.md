# P1-T4 Scoped CSharpier format of the two new test files

Timestamp: 2026-10-01T20-49
Command: CMD-SCOPED-FORMAT with PATHS-TESTS and TASKID p1-t4: dotnet tool run csharpier format <two files>; then dotnet tool run csharpier check <two files> (logs coverage\logs\p1-t4.csharpier-format.log and coverage\logs\p1-t4.csharpier-check.log, git-ignored)
EXIT_CODE: 0
Output Summary:
BEFORE UtilitiesCS.Test\EmailIntelligence\SortEmail_TrySaveAttachment_Tests.cs = C7272EDD4327ADE35E2D3F56C7645EE76CB5EA405F7A147AFFB637030FB2C530
BEFORE UtilitiesCS.Test\Dialogs\YesNoToAllPromptSession_Tests.cs = 278E1DB45ED29EB02E4473FE4A9EBDD6E7DEA7C73DC1865A23C28696455CB2A7
Formatter summary line (observation, not gated): Formatted 2 files in 2076ms.
FORMAT_EXIT_CODE: 0
AFTER UtilitiesCS.Test\EmailIntelligence\SortEmail_TrySaveAttachment_Tests.cs = BFACBB41EEEC99935D7FB00B4AC94BFD2C810BAFE4C27F9D4B40DFE85F9B0645
AFTER UtilitiesCS.Test\Dialogs\YesNoToAllPromptSession_Tests.cs = 0A0FBBD8F51CFEE972E60718EA619F9A0FFB1670FA5A76B7635FC74AC4F40C8B
Check summary line: Checked 2 files in 801ms.
CHECK_EXIT_CODE: 0
Observation: both hashes changed; the plan's Line endings convention predicts this (the Write tool writes LF and CSharpier normalizes to CRLF per .editorconfig); P1-T5 re-proves the whitespace-insensitive content.
Acceptance: FORMAT_EXIT_CODE 0 and CHECK_EXIT_CODE 0 (both hold).
