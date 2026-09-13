# Red Before Fix (P1-T2) [expect-fail]

- Timestamp: 2026-09-13T01-15
- Command (build): MSBuild.exe (VS18) TaskMaster.sln /t:Build /m /p:Configuration=Debug
  "/p:Platform=Any CPU"
- Command (test): <resolved vstest executable>
  QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /InIsolation
  "/TestCaseFilter:FullyQualifiedName=QuickFiler.Controllers.Tests.KaStringAsyncTests.KeyEquals_ContainsMatchAtNonPrefixIndex_InvokesUpdateWithLastMatchedCharacter"
- EXIT_CODE: 1
- ExpectedExitCode: 1

## Verbatim failure detail

```
Failed KeyEquals_ContainsMatchAtNonPrefixIndex_InvokesUpdateWithLastMatchedCharacter [142 ms]
Error Message:
 Expected updateArg to be "1" because Update receives the last character of the matched span
(Key.IndexOf("1", StringComparison.Ordinal) + other.Length - 1 = 1), not the pre-fix
prefix-only offset that yielded "0", but "0" differs near "0" (index 0).
```

```
Total tests: 1
     Failed: 1
Test Run Failed.
 Total time: 1.2507 Seconds
```

## Output Summary

Build (plain incremental) succeeded, exit 0, compiling the P1-T1 test into the QuickFiler.Test
build output assembly. The scoped vstest run against the new test method, run against
unmodified production code (Phase 2 has not yet run), observed EXIT_CODE 1, matching the
declared ExpectedExitCode of 1. Verdict "Test Run Failed."; exactly 1 test ran and it Failed;
the captured assertion shows an observed value of "0" against an expected "1" — the same
failing-run shape (exit code 1, ExpectedExitCode 1, "Test Run Failed.") recorded at
docs/features/archive/2026-08-07-quickfiler-keyboard-action-contract-defects-445/evidence/regression-testing/red-before-fix.2026-08-22T09-38.md.
