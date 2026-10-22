# P1-T6 Fail-before dossier (compile-red regression tests) [expect-fail]

Timestamp: 2026-10-01T20-50
Command: msbuild UtilitiesCS.Test\UtilitiesCS.Test.csproj /t:Build /m /p:Configuration=Debug /p:Platform=AnyCPU (resolved through vswhere; plus /nodeReuse:false; plus a normal-verbosity file logger coverage\logs\p1-t6.msbuild.log, git-ignored; console stream discarded with Out-Null). Immediately before it: CMD-HASH on UtilitiesCS\EmailIntelligence\EmailParsingSorting\SortEmail.cs and git diff --exit-code MERGE-BASE -- UtilitiesCS (MERGE-BASE = f5b46df637de81a0f4a856152095544f859718cc).
EXIT_CODE: 1
ExpectedExitCode: 1
Output Summary:
MSBUILD_EXIT_CODE: 1
CSC_OUT_LINES: 2
ZERO_ERRORS_LINES: 0
ERROR_LINES: 2
ERROR_LINES_NEW_TEST_FILES: 2
ERROR_LINES_OTHER_FILES: 0
MISSING_SESSION_TYPE_LINES: 2
CS1501_LINES: 0
ERROR_CODES: CS0246
DLL_ADVANCED: False
SRC SHA-256 (UtilitiesCS\EmailIntelligence\EmailParsingSorting\SortEmail.cs): 195BABDB966DFB24CEF1C8F7681B59B8CDBA84C1C045B57FB8F8060FE048708B (equals PRE-EDIT-HASH-SRC of P0-T4)
PRODUCTION-DIFF-EXIT: 0
Note: the error lines carry absolute file paths and are not transcribed; only counts and codes are recorded. CS1501 is 0 because the compiler reports the unresolved type (CS0246) for the new test files; the missing five-argument overload is masked by that earlier binding failure.
WhyFailingRunImpossible: The new tests call a five-argument TrySaveAttachmentAsync overload and a YesNoToAllPromptSession type that do not exist before the fix, so they cannot be executed red at run time; the fail-before is therefore this compile-red build. Reaching the merge-base handler branch at run time would require the real modal YesNoToAll dialog and a real read-only directory, both of which the unit-test policy prohibits.
Alternative proof: The negative control of plan tasks P3-T10 to P3-T13 is the runtime fail-before equivalent: after the fix, the statement `clearReadOnly(directory);` is removed, the predicted six tests (T2, T3, T4, T8, T9, T11) must fail and the other five must pass, and the restore must return the suite to eleven passes.
Acceptance: EXIT_CODE is non-zero (1) and equals ExpectedExitCode (1); ERROR_LINES_NEW_TEST_FILES 2 (at least 1); ERROR_LINES_OTHER_FILES 0; MISSING_SESSION_TYPE_LINES 2 (at least 1); the SRC SHA-256 equals PRE-EDIT-HASH-SRC and PRODUCTION-DIFF-EXIT is 0; DLL_ADVANCED False (all six hold). Outcome: FAIL-BEFORE OBSERVED.
