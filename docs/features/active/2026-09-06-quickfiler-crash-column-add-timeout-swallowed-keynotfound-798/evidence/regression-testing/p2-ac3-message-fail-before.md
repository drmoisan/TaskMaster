# Phase 2 — AC3 message-content, multi-missing, case-variant and positive tests (fail-before)

Timestamp: 2026-09-07T02-04
Task: [P2-T7] [expect-fail]
Issue: #798

Host-specific absolute paths, user account names and machine names are redacted to `<repo-root>`,
`<vstest>`, `<user>` and `<machine>` tokens.

## Tests added

- `UtilitiesCS.Test.Extensions.DfDeedleRequiredColumnValidationTests.ValidateRequiredEmailColumns_MissingTwoKeys_MessageNamesBothAndFolder`
- `UtilitiesCS.Test.Extensions.DfDeedleRequiredColumnValidationTests.ValidateRequiredEmailColumns_CaseVariantEntryid_ReportsEntryIDMissing`
- `UtilitiesCS.Test.Extensions.DfDeedleRequiredColumnValidationTests.ValidateRequiredEmailColumns_AllKeysPresent_DoesNotThrow`
- `UtilitiesCS.Test.Extensions.DfDeedleRequiredColumnValidationTests.ValidateRequiredEmailColumns_MissingSentOn_MessageNamesFolder`

The message-content assertions use the folder-name literal `T&E`. The case-variant test supplies the
key `Entryid` and asserts the required key `EntryID` is still reported missing, pinning ordinal
comparison; the map is built with the default comparer, which is ordinal, and the required names
carry the intentional casing asymmetry of capital D in `EntryID` and lowercase d in
`ConversationId`.

## Build

Command: msbuild UtilitiesCS.Test\UtilitiesCS.Test.csproj /t:Build /m /p:Configuration=Debug /p:Platform=AnyCPU
EXIT_CODE: 0
ExpectedExitCode: 0

## Test run

Command: `<vstest> UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /InIsolation /Logger:trx /ResultsDirectory:coverage\trx\p2-t7 /Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None /TestCaseFilter:"TestCategory!=LiveOutlook&FullyQualifiedName~DfDeedleRequiredColumnValidationTests&FullyQualifiedName!~ShellUtilities&FullyQualifiedName!~SysImageListHelper&FullyQualifiedName!~OSBrowser"`

EXIT_CODE: 1
ExpectedExitCode: 1

The filter carries the shell-icon exclusion extension because P0-T8 recorded
`SHELL_ICON_EXCLUSION: REQUIRED`. The run covers the whole class, so it also re-verifies the five
P2-T6 tests.

TRX: `coverage\trx\p2-t7\<user>_<machine>_2026-09-07_02_04_36_net481.trx`

- total: 9
- passed: 1
- failed: 8
- total run time: 2.0226 s

## Observed status of the four P2-T7 tests

| Test | Observed status | Duration |
|---|---|---|
| `ValidateRequiredEmailColumns_MissingTwoKeys_MessageNamesBothAndFolder` | Failed | 1 ms |
| `ValidateRequiredEmailColumns_CaseVariantEntryid_ReportsEntryIDMissing` | Failed | < 1 ms |
| `ValidateRequiredEmailColumns_AllKeysPresent_DoesNotThrow` | Passed | < 1 ms |
| `ValidateRequiredEmailColumns_MissingSentOn_MessageNamesFolder` | Failed | < 1 ms |

The three throwing tests are recorded as failed and the positive test as passed, matching the plan.

Failure messages, read from the TRX and quoted verbatim:

```
Expected a <System.Exception> to be thrown because two required columns are absent, but no exception was thrown.
```

```
Expected a <System.Exception> to be thrown because a case variant does not satisfy an ordinal comparison, but no exception was thrown.
```

```
Expected a <System.Exception> to be thrown because SentOn is absent, but no exception was thrown.
```

Each of the three failures is the one the plan predicts. In every case the assertion that failed is
the throw assertion, not the message-content assertion that follows it, so the failure is caused by
the validator's empty body and not by an incidental message mismatch. No test hung; the four-minute
blame hang timeout did not fire.

## Re-verification of P2-T6 in the same run

All five P2-T6 negative tests failed again, each with the message shape recorded in
`p2-ac3-negative-fail-before.md`. Their observed statuses are unchanged.

Output Summary: 9 total, 1 passed, 8 failed. The three throwing P2-T7 tests failed because the
validator performs no validation yet, and `ValidateRequiredEmailColumns_AllKeysPresent_DoesNotThrow`
passed. EXIT_CODE 1 matches ExpectedExitCode 1.
