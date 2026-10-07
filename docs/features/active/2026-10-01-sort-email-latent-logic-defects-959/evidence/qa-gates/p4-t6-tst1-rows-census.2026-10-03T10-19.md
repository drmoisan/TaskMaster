# P4-T6 TST1 GetAttachmentsInfo Data Rows Census

Timestamp: 2026-10-03T10-19
Command: Edit E-TST1-ROWS-SYNC then Edit E-TST1-ROWS-ASYNC on UtilitiesCS.Test/EmailIntelligence/SortEmail_Tests.cs; then CMD-CENSUS with PATHS-TST1 and TOKENS-TST1 (pwsh -NoProfile -Command payload)
EXIT_CODE: 0 (scoped to the CMD-CENSUS payload, its process exit code)
Output Summary: Both Edits applied once each. Every TOKENS-TST1 total equals the FINAL column: [TestMethod] 12, [DataTestMethod] 2, [DataRow( 6, DisplayName= 6, saveAttachments:false,savePictures:true 0, saveAttachments:saveAttachments,savePictures:savePictures 2, "photo.jpg,report.pdf" 2, both try-save test names 1 each, C:\Sortemail945Sandbox 1. LINES 484 (prediction before the P4-T7 format: 484). P4-T6 acceptance met.

## CMD-CENSUS output

```
TOKEN [TestMethod] @ UtilitiesCS.Test\EmailIntelligence\SortEmail_Tests.cs = 12
TOKEN [TestMethod] @ TOTAL = 12
TOKEN [DataTestMethod] @ UtilitiesCS.Test\EmailIntelligence\SortEmail_Tests.cs = 2
TOKEN [DataTestMethod] @ TOTAL = 2
TOKEN [DataRow( @ UtilitiesCS.Test\EmailIntelligence\SortEmail_Tests.cs = 6
TOKEN [DataRow( @ TOTAL = 6
TOKEN DisplayName= @ UtilitiesCS.Test\EmailIntelligence\SortEmail_Tests.cs = 6
TOKEN DisplayName= @ TOTAL = 6
TOKEN SanitizeArray_WhenOutputArrayIsInitialized_WritesSanitizedRows @ UtilitiesCS.Test\EmailIntelligence\SortEmail_Tests.cs = 0
TOKEN SanitizeArray_WhenOutputArrayIsInitialized_WritesSanitizedRows @ TOTAL = 0
TOKEN "SanitizeArray" @ UtilitiesCS.Test\EmailIntelligence\SortEmail_Tests.cs = 0
TOKEN "SanitizeArray" @ TOTAL = 0
TOKEN "SanitizeArrayLineTSV" @ UtilitiesCS.Test\EmailIntelligence\SortEmail_Tests.cs = 1
TOKEN "SanitizeArrayLineTSV" @ TOTAL = 1
TOKEN saveAttachments:false,savePictures:true @ UtilitiesCS.Test\EmailIntelligence\SortEmail_Tests.cs = 0
TOKEN saveAttachments:false,savePictures:true @ TOTAL = 0
TOKEN saveAttachments:saveAttachments,savePictures:savePictures @ UtilitiesCS.Test\EmailIntelligence\SortEmail_Tests.cs = 2
TOKEN saveAttachments:saveAttachments,savePictures:savePictures @ TOTAL = 2
TOKEN "photo.jpg,report.pdf" @ UtilitiesCS.Test\EmailIntelligence\SortEmail_Tests.cs = 2
TOKEN "photo.jpg,report.pdf" @ TOTAL = 2
TOKEN TrySaveAttachmentAsync_WhenSaveSucceeds_ReturnsTrueAndCallsSaveAsFile @ UtilitiesCS.Test\EmailIntelligence\SortEmail_Tests.cs = 1
TOKEN TrySaveAttachmentAsync_WhenSaveSucceeds_ReturnsTrueAndCallsSaveAsFile @ TOTAL = 1
TOKEN TrySaveAttachmentAsync_WhenDirectoryCreationThrowsIOException_PropagatesAndDoesNotSave @ UtilitiesCS.Test\EmailIntelligence\SortEmail_Tests.cs = 1
TOKEN TrySaveAttachmentAsync_WhenDirectoryCreationThrowsIOException_PropagatesAndDoesNotSave @ TOTAL = 1
TOKEN C:\Sortemail945Sandbox @ UtilitiesCS.Test\EmailIntelligence\SortEmail_Tests.cs = 1
TOKEN C:\Sortemail945Sandbox @ TOTAL = 1
LINES UtilitiesCS.Test\EmailIntelligence\SortEmail_Tests.cs = 484
SHA256 UtilitiesCS.Test\EmailIntelligence\SortEmail_Tests.cs = 752DDF312C4DACBF0626D62F0B50460FEBD06B6EB12CF5DB1F98F1F2E9912028
```

Compile-red span note: P4-T6 lies inside the P4-T1 to P4-T7 compile-red span; no build has run since P4-T3. COMPILE-RED SPAN OPEN applies to the per-task checkpoint commit the delegation requires.
