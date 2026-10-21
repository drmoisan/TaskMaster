# P4-T1 SaveCase Test File Final State Census

Timestamp: 2026-10-03T08-57
Command: Write tool rewrote UtilitiesCS.Test\EmailIntelligence\SortEmail_SaveCase_Tests.cs from Listing L-TSC-FINAL (after reading it; four leading spaces stripped); then CMD-CENSUS with PATHS-TSC and TOKENS-TSC (single path; TOTAL lines)
EXIT_CODE: 0 (CMD-CENSUS process exit code)
Output Summary: the three L1 tests are kept and the six SaveCaseAsync tests (seven rows) are added; every TOTAL equals the FINAL column of TOKENS-TSC; the file has 341 lines. This task opens the compile-red span P4-T1 to P4-T7 (the six-argument SaveCaseAsync core lands at P4-T4).

- TOKEN [TestMethod] @ TOTAL = 6
- TOKEN [DataTestMethod] @ TOTAL = 3
- TOKEN [DataRow( @ TOTAL = 6
- TOKEN DisplayName= @ TOTAL = 6
- TOKEN SortEmail.SaveCase( @ TOTAL = 3
- TOKEN SortEmail.SaveCaseAsync( @ TOTAL = 9
- TOKEN newScriptedPrompt( @ TOTAL = 6
- TOKEN RecordingSave(saves) @ TOTAL = 9
- TOKEN newYesNoToAllPromptSession(Prompt) @ TOTAL = 1
- TOKEN Times.Once @ TOTAL = 2
- TOKEN Times.Never @ TOTAL = 3
- TOKEN C:\Sortemail959Sandbox @ TOTAL = 2
- TOKEN DoNotParallelize @ TOTAL = 0
- TOKEN Thread.Sleep @ TOTAL = 0
- TOKEN Task.Delay @ TOTAL = 0
- TOKEN Timeout @ TOTAL = 0
- TOKEN Directory.CreateDirectory @ TOTAL = 0
- TOKEN File. @ TOTAL = 0
- TOKEN MemoryAppender @ TOTAL = 0
- LINES UtilitiesCS.Test\EmailIntelligence\SortEmail_SaveCase_Tests.cs = 341
- SHA256 UtilitiesCS.Test\EmailIntelligence\SortEmail_SaveCase_Tests.cs = EEE8DC3FBB85F854D760B67200780C4236068CAAA9566F18CC17B254E69016E8

Acceptance check: every TOTAL equals the FINAL column of TOKENS-TSC ([TestMethod] 6, [DataTestMethod] 3, [DataRow( 6, SortEmail.SaveCase( 3, SortEmail.SaveCaseAsync( 9, newScriptedPrompt( 6, RecordingSave(saves) 9, banned tokens 0). Holds.
