# [P2-T5] — Test Gate (vstest, coverage-enabled, isolated)

Timestamp: 2026-08-29T23-23
Run performed: 2026-08-30T01-17
Task: [P2-T5]
Working directory: `<repo-root>` (the repository root of this worktree)
EXIT_CODE: 0

Redaction note: no absolute host path, account name, or machine name appears in this artifact.
The repository root is written as `<repo-root>`, the resolved runner is written as
`<vstest-console>`, and the TRX filename is written with `<account>` and `<HOST>` placeholders,
following the convention already used by
`evidence/qa-gates/p4-t5-vstest-final.2026-08-29T08-15.md`. The default `vstest.console.exe` TRX
filename embeds both the account and the machine name and is therefore cited only in redacted
form.

## Runner resolution

Command: `& "C:\Program Files (x86)\Microsoft Visual Studio\Installer\vswhere.exe" -latest -products * -find "Common7\IDE\Extensions\TestPlatform\vstest.console.exe"`
EXIT_CODE: 0

The `vswhere.exe` path is written verbatim above because it is a fixed system location under
`Program Files (x86)`: it is identical on every Windows host, carries no account name and no
machine name, and trips none of the PA-7 sweep patterns.

The runner path this command returned is recorded as the placeholder `<vstest-console>` rather
than verbatim, because that returned path varies by installed Visual Studio edition and
version.

## Test command

Command: `<vstest-console> QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /EnableCodeCoverage /InIsolation /Logger:trx /ResultsDirectory:coverage\trx\remediation-p2-t5 /TestCaseFilter:"TestCategory!=LiveOutlook"`
EXIT_CODE: 0

`/InIsolation` is required in this repository: Moq-based tests fail on assembly load without
it. `/EnableCodeCoverage` satisfies the coverage-mode requirement for a language with mandatory
coverage policy. The results directory `coverage/trx/...` is matched by `.gitignore` line 144
(`coverage/*`), so the TRX and its coverage attachment do not enter the commit.

Console summary:

```
Test Run Successful.
Total tests: 1254
     Passed: 1254
 Total time: 16.9438 Seconds
```

TRX written to `coverage\trx\remediation-p2-t5\<account>_<HOST>_2026-08-30_01_36_48_net481.trx`.

### First invocation and its correction

The gate was first invoked with the results-directory switch unquoted. The shell's
argument-conversion layer swallowed the backslash separators, so the switch arrived as a single
concatenated token and the runner wrote its output to a directory outside the gitignored
`coverage/` tree, where it appeared as an untracked entry in repository status. That run also
reported `Total tests: 1254, Passed: 1254`, but it did not execute the plan's mandated
`/ResultsDirectory:coverage\trx\remediation-p2-t5`.

The misplaced directory was deleted and the gate was re-invoked with the switch quoted, so the
path arrived intact. The run recorded in this artifact is that corrected invocation. Repository
status carries no residue of the first one.

## TRX-derived acceptance evidence

Read directly from the TRX rather than from console text.

`ResultSummary/Counters` element:

```
total=1254 executed=1254 passed=1254 failed=0 error=0 timeout=0 aborted=0 notExecuted=0
```

`ResultSummary/@outcome`: `Completed`

The test carrying the corrected CR-1 documentation block and the corrected assertion
because-message:

- Test name: `UnregisterNavigation_AfterRegisteringAtOneDigitAndGrowingToTen_RemovesTheOneDigitKeys`
- Number of matching `UnitTestResult` entries: `1`
- Outcome: `Passed`
- Duration: `00:00:00.0004156`

Its sibling in the same file, `UnregisterNavigation_AfterRegisteringAtTwoDigitsAndShrinkingToNine_RemovesTheTwoDigitKeys`,
also reports `Passed`.

## Acceptance

| Clause | Required | Measured | Result |
|---|---|---|---|
| 1. TRX `Counters` records `failed="0"` | `failed="0"` | `failed=0` | PASS |
| 2. TRX records the CR-1 test with outcome `Passed` | present, `Passed` | present exactly once, `Passed` | PASS |

Supplementary: `EXIT_CODE` is `0`; `error`, `timeout`, `aborted`, and `notExecuted` are all `0`;
`executed` equals `total` at 1254.

## Output Summary

The full test gate for the touched assembly passed. 1254 of 1254 tests executed and passed,
with `failed=0`, `error=0`, `timeout=0`, `aborted=0`, and `notExecuted=0` in the TRX
`Counters` element and run outcome `Completed`. The specific test carrying this cycle's two
corrected text regions,
`UnregisterNavigation_AfterRegisteringAtOneDigitAndGrowingToTen_RemovesTheOneDigitKeys`,
appears exactly once with outcome `Passed`, confirming the corrected assertion because-message
compiles and the assertion still holds. Both acceptance clauses PASS. The full four-step
toolchain loop is now clean in a single pass: format, check, analyze, type-check, test.
