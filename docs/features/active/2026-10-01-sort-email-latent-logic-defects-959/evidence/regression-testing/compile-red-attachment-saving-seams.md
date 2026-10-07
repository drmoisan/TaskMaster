# Compile-Red: F1 Tests Against the Pre-Seam Production Tree (P4-T3)

Timestamp: 2026-10-03T08-59
Command: msbuild UtilitiesCS.Test\UtilitiesCS.Test.csproj /t:Build /m /p:Configuration=Debug /p:Platform=AnyCPU (CMD-BUILD-TEST, TASKID p4-t3; resolved through vswhere; plus /nodeReuse:false; plus a normal-verbosity file logger coverage\logs\p4-t3.msbuild.log; console stream discarded, figures read from the file logger), preceded by CMD-HASH on PATHS-A
EXIT_CODE: 1 (the printed MSBUILD_EXIT_CODE)
ExpectedExitCode: 1
Output Summary: expect-fail build; the final SaveCase and AttachmentSaving test listings call overloads that do not exist yet in the unchanged production file A, so the compiler reports 40 errors, all in the new test files, naming the six-argument SaveAttachmentAsync core, the six-argument SaveCaseAsync, the four-argument SaveAttachment core and the missing RedirectSaveFolder member; the output assembly did not advance.

- A-HASH-BEFORE: 2A145BF2BD43921CF6F9B624501523C0C5AFC9D155BB743DA2487315966BBC79 (equals the AFTER hash of A recorded by P1-T10)
- MSBUILD_EXIT_CODE: 1
- CSC_OUT_LINES: 2
- ZERO_ERRORS_LINES: 0
- ERROR_LINES: 40
- ERROR_LINES_TEST_FILES: 40
- ERROR_LINES_OTHER_FILES: 0
- MISSING_SAVEATTACHMENTASYNC_6: 14
- MISSING_SAVECASEASYNC_6: 18
- MISSING_SAVEATTACHMENT_4: 6
- MISSING_REDIRECTSAVEFOLDER: 2
- ERROR_CODES: CS0117,CS1501
- DLL_ADVANCED: False

Missing overloads named by the compiler output: no overload for method 'SaveAttachmentAsync' takes 6 arguments (CS1501); no overload for method 'SaveCaseAsync' takes 6 arguments (CS1501); no overload for method 'SaveAttachment' takes 4 arguments (CS1501); 'SortEmail' does not contain a definition for 'RedirectSaveFolder' (CS0117). The error lines carry absolute file paths and are therefore transcribed as counts and codes only.

Acceptance check (P4-T3): EXIT_CODE 1 is non-zero and equals ExpectedExitCode; A-HASH-BEFORE equals the P1-T10 AFTER hash of A; ERROR_LINES_TEST_FILES 40 (at least 1) and ERROR_LINES_OTHER_FILES 0; the four MISSING_ counts are each at least 1; ERROR_CODES contains CS1501 and CS0117; DLL_ADVANCED False. All six hold.
