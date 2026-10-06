# P5-T6 EfcDataModel Seam Extraction (D11 Step One), Census, Scoped Format and Test Build

Timestamp: 2026-10-03T12-23
Command: Edits E-E-SEAM-CALL then E-E-SEAM-MEMBER applied to QuickFiler\Controllers\EfcDataModel.cs; then CMD-CENSUS (PATHS-E with TOKENS-E); then CMD-SCOPED-FORMAT (PATHS-E, PATHS-TEF; TASKID p5-t6); then CMD-BUILD-TEST (PROJECT QuickFiler.Test, TASKID p5-t6): msbuild QuickFiler.Test\QuickFiler.Test.csproj /t:Build /m /p:Configuration=Debug /p:Platform=AnyCPU, resolved through vswhere, plus /nodeReuse:false, plus a normal-verbosity file logger
EXIT_CODE: 0 (scoped to the CMD-BUILD-TEST invocation, the printed MSBUILD_EXIT_CODE)
Output Summary: Both Edit anchors occurred once. The `MoveToFolderAsync` body now calls the new `protected internal virtual void ResetFilerPromptState()` seam, which calls `SortEmail.Cleanup_Files()`, with no `finally` yet (D11 step one). Every TOKENS-E total equals the SEAM column; E has 475 lines. The scoped format left E unchanged and rewrote TEF (line endings; FORMAT_EXIT_CODE 0, CHECK_EXIT_CODE 0). The QuickFiler.Test build is green (MSBUILD_EXIT_CODE 0, ERROR_LINES 0, no CS0507 or CS0115, DLL_ADVANCED True). The compile-red span P5-T4 to P5-T6 is closed.

## CMD-CENSUS PATHS-E with TOKENS-E (TOTAL lines)

```
TOKEN SortEmail.Cleanup_Files(); @ TOTAL = 1
TOKEN ResetFilerPromptState(); @ TOTAL = 1
TOKEN protectedinternalvirtualvoidResetFilerPromptState() @ TOTAL = 1
TOKEN varresult=awaitInvokeFilerAsync(config,mailHelpers); @ TOTAL = 1
TOKEN result=awaitInvokeFilerAsync(config,mailHelpers); @ TOTAL = 1
TOKEN boolresult; @ TOTAL = 0
TOKEN finally{ @ TOTAL = 0
TOKEN returnresult; @ TOTAL = 1
LINES QuickFiler\Controllers\EfcDataModel.cs = 475
SHA256 QuickFiler\Controllers\EfcDataModel.cs = 4F80CF7F40816A3814EAE1BDE6425E45B582312CAA1709F497F97C821B4ED3A6
```

## CMD-SCOPED-FORMAT output

```
BEFORE QuickFiler\Controllers\EfcDataModel.cs = 4F80CF7F40816A3814EAE1BDE6425E45B582312CAA1709F497F97C821B4ED3A6
BEFORE QuickFiler.Test\Controllers\EfcDataModelFilerCleanupTests.cs = 95B0CA83059042DD81279CE370FE0FD2D3AF9A533A2FC3A34261F5698D614B3F
Formatted 2 files in 1958ms.
FORMAT_EXIT_CODE: 0
AFTER QuickFiler\Controllers\EfcDataModel.cs = 4F80CF7F40816A3814EAE1BDE6425E45B582312CAA1709F497F97C821B4ED3A6
AFTER QuickFiler.Test\Controllers\EfcDataModelFilerCleanupTests.cs = F5E5FB79715C806B8D98216E4FDD32335D60593D1EE025DD69A3A0510ABFDD04
Checked 2 files in 791ms.
CHECK_EXIT_CODE: 0
```

## CMD-BUILD-TEST labelled output

```
    0 Warning(s)
    0 Error(s)
MSBUILD_EXIT_CODE: 0
CSC_OUT_LINES: 2
ZERO_ERRORS_LINES: 1
ERROR_LINES: 0
ERROR_LINES_TEST_FILES: 0
ERROR_LINES_OTHER_FILES: 0
MISSING_SAVEATTACHMENTASYNC_6: 0
MISSING_SAVECASEASYNC_6: 0
MISSING_SAVEATTACHMENT_4: 0
MISSING_REDIRECTSAVEFOLDER: 0
ERROR_CODES: 
DLL_ADVANCED: True
```

## Acceptance (P5-T6, all four required)

1. Every TOKENS-E total equals the SEAM column (`ResetFilerPromptState();` 1, `protectedinternalvirtualvoidResetFilerPromptState()` 1, `SortEmail.Cleanup_Files();` 1, `varresult=awaitInvokeFilerAsync(config,mailHelpers);` 1, `finally{` 0, `boolresult;` 0): met.
2. `FORMAT_EXIT_CODE: 0` and `CHECK_EXIT_CODE: 0`: met.
3. `MSBUILD_EXIT_CODE: 0` and `ERROR_LINES: 0` (no CS0507 or CS0115): met.
4. `DLL_ADVANCED: True`: met.
