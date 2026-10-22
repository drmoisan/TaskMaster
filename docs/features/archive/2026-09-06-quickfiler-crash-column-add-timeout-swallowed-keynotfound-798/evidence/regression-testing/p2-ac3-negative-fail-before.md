# Phase 2 — AC3 negative regression tests (fail-before)

Timestamp: 2026-09-07T02-03
Task: [P2-T6] [expect-fail]
Issue: #798

Host-specific absolute paths, user account names and machine names are redacted to `<repo-root>`,
`<vstest>`, `<user>` and `<machine>` tokens.

## Tests added

All five sit under `UtilitiesCS.Test.Extensions.DfDeedleRequiredColumnValidationTests`:

- `UtilitiesCS.Test.Extensions.DfDeedleRequiredColumnValidationTests.ValidateRequiredEmailColumns_MissingEntryID_Throws`
- `UtilitiesCS.Test.Extensions.DfDeedleRequiredColumnValidationTests.ValidateRequiredEmailColumns_MissingMessageClass_Throws`
- `UtilitiesCS.Test.Extensions.DfDeedleRequiredColumnValidationTests.ValidateRequiredEmailColumns_MissingSentOn_Throws`
- `UtilitiesCS.Test.Extensions.DfDeedleRequiredColumnValidationTests.ValidateRequiredEmailColumns_MissingConversationId_Throws`
- `UtilitiesCS.Test.Extensions.DfDeedleRequiredColumnValidationTests.ValidateRequiredEmailColumns_MissingTriage_Throws`

Each builds a `Dictionary<string, int>` containing the other four required keys and asserts the call
throws. The dictionary uses the default comparer, which is ordinal, matching the map the table
utility produces.

## Build

Command: msbuild UtilitiesCS.Test\UtilitiesCS.Test.csproj /t:Build /m /p:Configuration=Debug /p:Platform=AnyCPU
EXIT_CODE: 0
ExpectedExitCode: 0

## Test run

Command: `<vstest> UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /InIsolation /Logger:trx /ResultsDirectory:coverage\trx\p2-t6 /Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None /TestCaseFilter:"TestCategory!=LiveOutlook&FullyQualifiedName~DfDeedleRequiredColumnValidationTests&FullyQualifiedName!~ShellUtilities&FullyQualifiedName!~SysImageListHelper&FullyQualifiedName!~OSBrowser"`

EXIT_CODE: 1
ExpectedExitCode: 1

The filter carries the shell-icon exclusion extension because P0-T8 recorded
`SHELL_ICON_EXCLUSION: REQUIRED`.

TRX: `coverage\trx\p2-t6\<user>_<machine>_2026-09-07_02_03_28_net481.trx`

- total: 5
- passed: 0
- failed: 5
- total run time: 2.4305 s

## Observed status

| Test | Observed status | Duration |
|---|---|---|
| `ValidateRequiredEmailColumns_MissingEntryID_Throws` | Failed | 258 ms |
| `ValidateRequiredEmailColumns_MissingMessageClass_Throws` | Failed | 1 ms |
| `ValidateRequiredEmailColumns_MissingSentOn_Throws` | Failed | < 1 ms |
| `ValidateRequiredEmailColumns_MissingConversationId_Throws` | Failed | < 1 ms |
| `ValidateRequiredEmailColumns_MissingTriage_Throws` | Failed | 1 ms |

All five failed with the same shape of message, quoted verbatim for the first:

```
Expected a <System.Exception> to be thrown because the projection indexes EntryID unconditionally, but no exception was thrown.
```

Each failure is the one the plan predicts: `ValidateRequiredEmailColumns` exists with an empty body
after P1-T4, so it validates nothing and no exception is raised. None is an incidental failure. No
test hung; the four-minute blame hang timeout did not fire.

Output Summary: 5 total, 0 passed, 5 failed. All five AC3 negative tests failed because the validator
performs no validation yet, which is the AC3 fail-before condition. EXIT_CODE 1 matches
ExpectedExitCode 1.
