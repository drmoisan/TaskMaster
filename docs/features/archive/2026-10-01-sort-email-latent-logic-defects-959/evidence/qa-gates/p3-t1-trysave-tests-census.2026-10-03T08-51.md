# P3-T1 Try-Save Tests: Tripwire and T12 Census

Timestamp: 2026-10-03T08-51
Command: Edits E-TST2-TRIPWIRE then E-TST2-T12 on UtilitiesCS.Test\EmailIntelligence\SortEmail_TrySaveAttachment_Tests.cs (insert-only); then CMD-CENSUS with PATHS-TST2 and TOKENS-TST2; git diff --numstat 94287369908cc920b21b0e3256314f988ad7d2f5 -- UtilitiesCS.Test/EmailIntelligence/SortEmail_TrySaveAttachment_Tests.cs; git status --porcelain -- UtilitiesCS.Test/EmailIntelligence/SortEmail_TrySaveAttachment_Tests.cs
EXIT_CODE: 0 (scoped to the git status --porcelain invocation, the last command)
Output Summary: the CreateDirectoryLimit tripwire and T12 were added; the edits only insert lines (numstat 35 added, 0 deleted), so the eleven pre-existing test methods are textually unchanged (AC5).

CMD-CENSUS (single path; TOTAL lines):
- TOKEN [TestMethod] @ TOTAL = 12
- TOKEN CreateDirectoryLimit @ TOTAL = 3
- TOKEN TrySaveAttachmentAsync_WhenYesToAllIsHeldAndRetryIsStillDenied_RethrowsAfterOneClear @ TOTAL = 1
- TOKEN "retryboundexceeded" @ TOTAL = 1
- TOKEN .Throws(denied) @ TOTAL = 1
- TOKEN BeSameAs(denied) @ TOTAL = 1
- TOKEN SetupSequence @ TOTAL = 11
- TOKEN Times.Exactly(2) @ TOTAL = 5
- TOKEN newSeams( @ TOTAL = 12
- TOKEN DoNotParallelize @ TOTAL = 0
- TOKEN Thread.Sleep @ TOTAL = 0
- TOKEN Task.Delay @ TOTAL = 0
- TOKEN Timeout @ TOTAL = 0
- TOKEN MemoryAppender @ TOTAL = 0
- LINES UtilitiesCS.Test\EmailIntelligence\SortEmail_TrySaveAttachment_Tests.cs = 410
- SHA256 UtilitiesCS.Test\EmailIntelligence\SortEmail_TrySaveAttachment_Tests.cs = B6EF9164D8413AA900311CA6118B9F305F32718C5F7F816FFB1CA849604E8ECC

- NUMSTAT: 35	0	UtilitiesCS.Test/EmailIntelligence/SortEmail_TrySaveAttachment_Tests.cs
- PORCELAIN:  M UtilitiesCS.Test/EmailIntelligence/SortEmail_TrySaveAttachment_Tests.cs

Acceptance check: every TOKENS-TST2 total equals the FINAL column; the numstat line reads a positive added count (35), 0 deleted and the path; the porcelain line shows the file modified. All three hold.
