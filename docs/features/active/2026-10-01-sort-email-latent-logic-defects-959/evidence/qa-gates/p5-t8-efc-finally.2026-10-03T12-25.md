# P5-T8 EfcDataModel try/finally (D11 Step Two), Census, Scoped Format and Test Build

Timestamp: 2026-10-03T12-25
Command: Edit E-E-FINALLY applied to QuickFiler\Controllers\EfcDataModel.cs; then CMD-CENSUS (PATHS-E with TOKENS-E); then CMD-SCOPED-FORMAT (PATHS-E; TASKID p5-t8); then CMD-BUILD-TEST (PROJECT QuickFiler.Test, TASKID p5-t8): msbuild QuickFiler.Test\QuickFiler.Test.csproj /t:Build /m /p:Configuration=Debug /p:Platform=AnyCPU, resolved through vswhere, plus /nodeReuse:false, plus a normal-verbosity file logger
EXIT_CODE: 0 (scoped to the CMD-BUILD-TEST invocation, the printed MSBUILD_EXIT_CODE)
Output Summary: The Edit anchor (the P5-T6 seam-call block) occurred once. The filer call now runs inside `try`, and `ResetFilerPromptState()` runs in `finally`. Every TOKENS-E total equals the FINAL column, and E has 485 lines (at most 499). The scoped format made no change (BEFORE and AFTER hashes equal; FORMAT_EXIT_CODE 0, CHECK_EXIT_CODE 0). The QuickFiler.Test build is green (MSBUILD_EXIT_CODE 0, ERROR_LINES 0, no CS0165, DLL_ADVANCED True).

## CMD-CENSUS PATHS-E with TOKENS-E (TOTAL lines)

```
TOKEN SortEmail.Cleanup_Files(); @ TOTAL = 1
TOKEN ResetFilerPromptState(); @ TOTAL = 1
TOKEN protectedinternalvirtualvoidResetFilerPromptState() @ TOTAL = 1
TOKEN varresult=awaitInvokeFilerAsync(config,mailHelpers); @ TOTAL = 0
TOKEN result=awaitInvokeFilerAsync(config,mailHelpers); @ TOTAL = 1
TOKEN boolresult; @ TOTAL = 1
TOKEN finally{ @ TOTAL = 1
TOKEN returnresult; @ TOTAL = 1
LINES QuickFiler\Controllers\EfcDataModel.cs = 485
SHA256 QuickFiler\Controllers\EfcDataModel.cs = 772262874673028C7D109263F82A55B6C6B4D2641FED3B186166F30DA9F15FA2
```

## CMD-SCOPED-FORMAT output

```
BEFORE QuickFiler\Controllers\EfcDataModel.cs = 772262874673028C7D109263F82A55B6C6B4D2641FED3B186166F30DA9F15FA2
Formatted 1 files in 1113ms.
FORMAT_EXIT_CODE: 0
AFTER QuickFiler\Controllers\EfcDataModel.cs = 772262874673028C7D109263F82A55B6C6B4D2641FED3B186166F30DA9F15FA2
Checked 1 files in 480ms.
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

## Acceptance (P5-T8, all required)

1. Every TOKENS-E total equals the FINAL column (`finally{` 1, `boolresult;` 1, `varresult=awaitInvokeFilerAsync(config,mailHelpers);` 0, `result=awaitInvokeFilerAsync(config,mailHelpers);` 1, `ResetFilerPromptState();` 1, `returnresult;` 1): met.
2. `FORMAT_EXIT_CODE: 0` and `CHECK_EXIT_CODE: 0`: met.
3. `MSBUILD_EXIT_CODE: 0` and `ERROR_LINES: 0` (no CS0165): met.
4. `DLL_ADVANCED: True`: met.
5. `LINES` for E is 485 (at most 499): met.
