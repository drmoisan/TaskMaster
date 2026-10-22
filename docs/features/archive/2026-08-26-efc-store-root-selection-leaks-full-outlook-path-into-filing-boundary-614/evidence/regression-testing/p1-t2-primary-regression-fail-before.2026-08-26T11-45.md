# [P1-T2] Primary Regression Test — FAIL-BEFORE Evidence

Timestamp: 2026-08-26T11-45
Task: [P1-T2] `[expect-fail]`
Issue: #614
AC advanced: AC17 (fail-before half)

Command: `& $vstest UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /InIsolation "/TestCaseFilter:FullyQualifiedName~Issue614_ResolvePaths_WithStoreRootStem_RejectsNonRelativeStemWithoutLeakingIdentifiers" "/Logger:trx;LogFileName=p1-t2.trx" "/ResultsDirectory:coverage\trx\p1-t2"`

`$vstest` resolved via vswhere to
`<vs-install>\Common7\IDE\Extensions\TestPlatform\vstest.console.exe` (VSTest version 18.8.0 x64).
Working directory: `<repo-root>`. Shell: `pwsh -NoProfile`.

EXIT_CODE: 1
ExpectedExitCode: 1

## Result

```
A total of 1 test files matched the specified pattern.
  Failed Issue614_ResolvePaths_WithStoreRootStem_RejectsNonRelativeStemWithoutLeakingIdentifiers [161 ms]
Total tests: 1
     Failed: 1
Test Run Failed.
 Total time: 1.4899 Seconds
```

## Failure signal (redaction-safe; every identifier below is a fabricated placeholder)

```
Expected ArgumentException. "fsPathExDividers has a value of
UserstestuserOneDrive - Contosomailbox@example.com which contains illegal characters .
Parameter name: fsPath" to contain "DestinationOlStem" because the diagnostic must identify the
offending parameter.
```

Failing assertion site: `UtilitiesCS.Test/EmailIntelligence/EmailFilerConfig_Tests.cs:321`.

## Interpretation — this is the D4 + D5 defect chain observed live

The pre-fix tree produces exactly the failure the plan predicted:

1. `EmailFilerConfig.ResolvePaths()` (`:203-204`) concatenates without validating that
   `DestinationOlStem` is archive-relative, so the store-root stem `\\mailbox@example.com` is
   accepted and `DestinationOlPath` becomes a doubly-rooted nonsense path.
2. Control therefore reaches `FolderConverter.ToFsFolderpath`, whose whole-path
   `IllegalFolderCharacters` check (D5b: `.` is banned) trips on the `.` in the mailbox domain and
   throws an `ArgumentException` from the WRONG layer with the WRONG parameter name (`fsPath`).
3. Both of the test's required assertions consequently fail:
   - **Assertion 1 fails.** The thrown message names neither `DestinationOlStem` nor the
     archive-relative rule; it names `fsPathExDividers` / `fsPath`. This is the assertion that
     actually reported (FluentAssertions short-circuits on the first failure).
   - **Assertion 2 would also fail.** The same message embeds `mailbox@example.com` verbatim
     (D5e identifier leak, issue #602), which the `NotContain` assertion prohibits. It is visible
     in the quoted failure text above, so the second failure is demonstrated by the same output
     rather than merely asserted.

No production file was modified in Phase 1; this run is against the unmodified pre-fix production
tree at baseline commit `f602410674a20f8b5aa988847ba6d055b008ca11`.

## Artifact locations

- TRX: `coverage\trx\p1-t2\p1-t2.trx` (gitignored `coverage/` tree; the explicit
  `/Logger:...LogFileName=` and `/ResultsDirectory:` prevent vstest's default
  `<account>_<HOST>_<timestamp>.trx` naming, so no host or account identifier is created).

Output Summary: The primary regression test FAILS on the pre-fix tree exactly as required, exit
code 1 against the declared `ExpectedExitCode: 1`. 1 test run, 1 failed, 0 passed. The recorded
failure text demonstrates both halves of the assertion pair failing: the exception originates in
`FolderConverter` rather than the `EmailFilerConfig` stem contract, and it leaks the mailbox
address. This is the fail-before half of AC17; the pass-after half is captured at P4-T3.
