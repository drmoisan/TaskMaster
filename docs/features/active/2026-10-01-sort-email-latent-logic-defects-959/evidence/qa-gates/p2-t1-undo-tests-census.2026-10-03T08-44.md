# P2-T1 UndoAndMoveLog Test File Census

Timestamp: 2026-10-03T08-44
Command: Write tool created UtilitiesCS.Test\EmailIntelligence\SortEmail_UndoAndMoveLog_Tests.cs from Listing L-TUL (four leading spaces stripped); then CMD-CENSUS with PATHS-TUL and TOKENS-TUL
EXIT_CODE: 0 (CMD-CENSUS process exit code)
Output Summary: every TOTAL equals the TOKENS-TUL expectation; the file has 82 lines. Each per-file TOKEN line equals its TOTAL line (single path).

- TOKEN [TestMethod] @ TOTAL = 2
- TOKEN SortEmail.WriteCSV_StartNewFileIfDoesNotExist( @ TOTAL = 2
- TOKEN Path.Combine(LogFolder,LogFileName) @ TOTAL = 1
- TOKEN HaveCount(13) @ TOTAL = 1
- TOKEN "MovedMails.txt" @ TOTAL = 1
- TOKEN Triage\tFolderName\tSent_On @ TOTAL = 1
- TOKEN C:\Sortemail959Sandbox @ TOTAL = 1
- TOKEN DoNotParallelize @ TOTAL = 0
- TOKEN Thread.Sleep @ TOTAL = 0
- TOKEN Task.Delay @ TOTAL = 0
- TOKEN Timeout @ TOTAL = 0
- TOKEN Directory.CreateDirectory @ TOTAL = 0
- TOKEN File. @ TOTAL = 0
- TOKEN MemoryAppender @ TOTAL = 0
- LINES UtilitiesCS.Test\EmailIntelligence\SortEmail_UndoAndMoveLog_Tests.cs = 82
- SHA256 UtilitiesCS.Test\EmailIntelligence\SortEmail_UndoAndMoveLog_Tests.cs = 4F4A0928CD70D27542ED39B2A8853F2633E997E20E85026696553824AAD65002

Acceptance check: [TestMethod] 2, SortEmail.WriteCSV_StartNewFileIfDoesNotExist( 2, Path.Combine(LogFolder,LogFileName) 1, HaveCount(13) 1, Triage\tFolderName\tSent_On 1, banned tokens 0. Holds.
