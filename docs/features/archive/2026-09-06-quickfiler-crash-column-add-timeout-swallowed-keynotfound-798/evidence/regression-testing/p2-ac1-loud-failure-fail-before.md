# Phase 2 — AC1 loud-failure regression test (fail-before)

Timestamp: 2026-09-07T01-48
Task: [P2-T2] [expect-fail]
Issue: #798

Host-specific absolute paths, user account names and machine names are redacted to `<repo-root>`,
`<vstest>`, `<user>` and `<machine>` tokens.

## Test added

`UtilitiesCS.Test.Extensions.DfDeedleQfcColumnTimeoutTests.AddQfcColumnsAsync_ThirdDeadlineExpires_ThrowsNamingFolderAndStep`

Arranged as in P2-T1 — a counting, entry-signalling, blocking injected adder and the deterministic
arming barrier — with a folder mock whose `Name` returns the literal `T&E`. The three deadlines are
driven through the P2-T1 arming barrier; a bare sequence of three consecutive `Advance` calls is not
used. The test then awaits the returned call and asserts it throws, and that the thrown exception's
flattened message contains both the literal `T&E` and the literal `column add`.

The flattening helper walks the exception and every exception nested inside it, including the
members of an `AggregateException`, so the message assertion is indifferent to whether the
production failure is wrapped.

## Build

Command: msbuild UtilitiesCS.Test\UtilitiesCS.Test.csproj /t:Build /m /p:Configuration=Debug /p:Platform=AnyCPU
EXIT_CODE: 0
ExpectedExitCode: 0

## Test run

Command: `<vstest> UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /InIsolation /Logger:trx /ResultsDirectory:coverage\trx\p2-t2 /Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None /TestCaseFilter:"TestCategory!=LiveOutlook&FullyQualifiedName~AddQfcColumnsAsync_ThirdDeadlineExpires&FullyQualifiedName!~ShellUtilities&FullyQualifiedName!~SysImageListHelper&FullyQualifiedName!~OSBrowser"`

EXIT_CODE: 1
ExpectedExitCode: 1

The filter carries the shell-icon exclusion extension because P0-T8 recorded
`SHELL_ICON_EXCLUSION: REQUIRED`.

TRX: `coverage\trx\p2-t2\<user>_<machine>_2026-09-07_01_48_22_net481.trx`

- total: 1
- passed: 0
- failed: 1
- duration of the failing test: 319 ms
- total run time: 2.1984 s

## Observed failure

Status: **Failed**, as expected, because the call returned normally instead of throwing.

Failure message, quoted verbatim:

```
Expected a <System.Exception> to be thrown because an exhausted column-add budget must surface to the caller instead of returning normally, but no exception was thrown.
```

The failure is the one the plan predicts. The assertion that failed is the throw assertion itself,
not the subsequent message-content assertion, which confirms the unfixed method swallows the third
`TimeoutException` and returns normally. The test did not hang; the four-minute blame hang timeout
did not fire.

Output Summary: 1 total, 0 passed, 1 failed. `AddQfcColumnsAsync_ThirdDeadlineExpires_ThrowsNamingFolderAndStep`
failed because no exception was thrown after the third deadline expired, which is the AC1 loud-failure
fail-before condition. EXIT_CODE 1 matches ExpectedExitCode 1.
