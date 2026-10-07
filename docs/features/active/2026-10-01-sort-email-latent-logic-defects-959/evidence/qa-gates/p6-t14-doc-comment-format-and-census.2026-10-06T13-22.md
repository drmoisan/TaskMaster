# P6-T14 TST1 Doc-Comment Format, Repository Check and Census

Timestamp: 2026-10-06T13-22
Command: (1) CMD-SCOPED-FORMAT with PATHS-TST1, TASKID p6-t14 (canonical `dotnet tool run csharpier format` then `dotnet tool run csharpier check` over UtilitiesCS.Test\EmailIntelligence\SortEmail_Tests.cs); (2) CMD-CHECK-REPO with TASKID p6-t14 (canonical command `dotnet tool run csharpier check .`); (3) CMD-CENSUS with PATHS-TST1 and TOKENS-TST1; each run as pwsh -NoProfile -Command with Set-Location to the item worktree
EXIT_CODE: 0 (scoped to the CMD-CENSUS payload, the last invocation, its process exit code)
ITERATION: 1
Output Summary: the scoped formatter changed nothing (BEFORE and AFTER hashes equal); the scoped and the repository-wide read-only checks both exit 0 (Checked 1639 files); every TOKENS-TST1 total equals its FINAL column and LINES is 488.

- FORMAT_EXIT_CODE: 0
- BEFORE UtilitiesCS.Test\EmailIntelligence\SortEmail_Tests.cs = 0949CB13E7B9E76E263F3C18192C65DAEE011BE122DA8DAEE31803BCC945B331
- AFTER UtilitiesCS.Test\EmailIntelligence\SortEmail_Tests.cs = 0949CB13E7B9E76E263F3C18192C65DAEE011BE122DA8DAEE31803BCC945B331
- Formatter summary line (observation, not gated): `Formatted 1 files in 1583ms.`
- SCOPED-CHECK_EXIT_CODE: 0 (scoped check summary `Checked 1 files in 508ms.`)
- REPO-CHECK_EXIT_CODE: 0
- CHECKED-LINE: Checked 1639 files in 6002ms.

## CMD-CENSUS PATHS-TST1 (TOKENS-TST1, FINAL column)

Single path, so each per-path TOKEN line equals its TOTAL line; the TOTAL lines are transcribed.

```
TOKEN [TestMethod] @ TOTAL = 12
TOKEN [DataTestMethod] @ TOTAL = 2
TOKEN [DataRow( @ TOTAL = 6
TOKEN DisplayName= @ TOTAL = 6
TOKEN SanitizeArray_WhenOutputArrayIsInitialized_WritesSanitizedRows @ TOTAL = 0
TOKEN "SanitizeArray" @ TOTAL = 0
TOKEN "SanitizeArrayLineTSV" @ TOTAL = 1
TOKEN saveAttachments:false,savePictures:true @ TOTAL = 0
TOKEN saveAttachments:saveAttachments,savePictures:savePictures @ TOTAL = 2
TOKEN "photo.jpg,report.pdf" @ TOTAL = 2
TOKEN TrySaveAttachmentAsync_WhenSaveSucceeds_ReturnsTrueAndCallsSaveAsFile @ TOTAL = 1
TOKEN TrySaveAttachmentAsync_WhenDirectoryCreationThrowsIOException_PropagatesAndDoesNotSave @ TOTAL = 1
TOKEN C:\Sortemail945Sandbox @ TOTAL = 1
LINES UtilitiesCS.Test\EmailIntelligence\SortEmail_Tests.cs = 488
SHA256 UtilitiesCS.Test\EmailIntelligence\SortEmail_Tests.cs = 0949CB13E7B9E76E263F3C18192C65DAEE011BE122DA8DAEE31803BCC945B331
```

## Acceptance (P6-T14, all four required)

1. FORMAT_EXIT_CODE: 0 with both hash rows recorded (equal; the formatter did not rewrite the file): met.
2. SCOPED-CHECK_EXIT_CODE: 0 and REPO-CHECK_EXIT_CODE: 0 with CHECKED-LINE matching `Checked <N> files` (N = 1639): met.
3. Every TOKENS-TST1 total equals the FINAL column (unchanged from p6-t10-post-format-census): met.
4. LINES UtilitiesCS.Test\EmailIntelligence\SortEmail_Tests.cs = 488 (SHA256 recorded, not gated): met.

The AFTER hash equals BEFORE, so under the task's commit rule SortEmail_Tests.cs is not staged by this task; only FEATURE/ is staged.
