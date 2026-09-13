# Green After Fix (P4-T1)

- Timestamp: 2026-09-13T01-30
- Command (build): MSBuild.exe (VS18) TaskMaster.sln /t:Build /m /p:Configuration=Debug
  "/p:Platform=Any CPU"
- Command (test): <resolved vstest executable>
  QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /InIsolation
  "/TestCaseFilter:FullyQualifiedName~KaStringAsyncTests"
- EXIT_CODE: 0

## Verbatim result

```
Test Run Successful.
Total tests: 13
     Passed: 13
 Total time: 1.5036 Seconds
```

All 13 tests in KaStringAsyncTests passed, individually including:
- KeyEquals_ContainsMatchAtNonPrefixIndex_InvokesUpdateWithLastMatchedCharacter (new AC3 test):
  Passed
- KeyEquals_ContainsMatchWhileActivated_InvokesUpdateAndReturnsTrue (reworded prefix-case
  test, AC4): Passed

## Output Summary

Build (plain incremental) exit 0, compiling the P2-T1 production fix and the P3-T1/P3-T2
prose rewords into both the QuickFiler and QuickFiler.Test output assemblies. Test run exit
code 0; verdict "Test Run Successful."; 0 Failed among all 13 tests in KaStringAsyncTests; both
the new regression method and the reworded prefix-case test are individually reported Passed —
matching the passing-run shape recorded at
docs/features/archive/2026-08-07-quickfiler-keyboard-action-contract-defects-445/evidence/baseline/vstest-baseline.2026-08-22T09-30.md.
