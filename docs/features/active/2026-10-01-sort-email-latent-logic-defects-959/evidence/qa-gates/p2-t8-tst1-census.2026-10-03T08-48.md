# P2-T8 SortEmail_Tests After the SanitizeArray Test Deletion

Timestamp: 2026-10-03T08-48
Command: Edit E-TST1-DEL-SANITIZE on UtilitiesCS.Test\EmailIntelligence\SortEmail_Tests.cs; then CMD-CENSUS with PATHS-TST1 and TOKENS-TST1 (single path; TOTAL lines)
EXIT_CODE: 0 (CMD-CENSUS process exit code)
Output Summary: the reflection test of the deleted SanitizeArray member is removed; fourteen test methods remain; the SanitizeArrayLineTSV test and the two try-save pins are unchanged.

- TOKEN [TestMethod] @ TOTAL = 14
- TOKEN [DataTestMethod] @ TOTAL = 0
- TOKEN [DataRow( @ TOTAL = 0
- TOKEN DisplayName= @ TOTAL = 0
- TOKEN SanitizeArray_WhenOutputArrayIsInitialized_WritesSanitizedRows @ TOTAL = 0
- TOKEN "SanitizeArray" @ TOTAL = 0
- TOKEN "SanitizeArrayLineTSV" @ TOTAL = 1
- TOKEN saveAttachments:false,savePictures:true @ TOTAL = 1
- TOKEN saveAttachments:saveAttachments,savePictures:savePictures @ TOTAL = 0
- TOKEN "photo.jpg,report.pdf" @ TOTAL = 0
- TOKEN TrySaveAttachmentAsync_WhenSaveSucceeds_ReturnsTrueAndCallsSaveAsFile @ TOTAL = 1
- TOKEN TrySaveAttachmentAsync_WhenDirectoryCreationThrowsIOException_PropagatesAndDoesNotSave @ TOTAL = 1
- TOKEN C:\Sortemail945Sandbox @ TOTAL = 1
- LINES UtilitiesCS.Test\EmailIntelligence\SortEmail_Tests.cs = 432
- SHA256 UtilitiesCS.Test\EmailIntelligence\SortEmail_Tests.cs = 8F34C44EA2C8F44382B1290B1AFA4733B180FDF031CC6234AF202A5205590D2A

Acceptance check: every TOKENS-TST1 total equals the MID column ([TestMethod] 14, SanitizeArray_WhenOutputArrayIsInitialized_WritesSanitizedRows 0, "SanitizeArray" 0, "SanitizeArrayLineTSV" 1, the two try-save test names 1 each). Holds.
