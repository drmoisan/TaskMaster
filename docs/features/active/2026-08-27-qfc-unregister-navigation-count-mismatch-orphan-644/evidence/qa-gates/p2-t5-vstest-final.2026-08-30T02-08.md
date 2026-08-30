# [P2-T5] — vstest gate for the touched assembly

- Timestamp: 2026-08-30T02-08
- Task: `[P2-T5]`
- Issue: #644
- Branch: `bug/qfc-unregister-navigation-count-mismatch-orphan-644`
- Working directory: repository root of the branch worktree, recorded as a generic
  repository-root placeholder. No absolute host path, account name, or machine name is
  written to this artifact.

## Runner resolution

- Command: `& "C:\Program Files (x86)\Microsoft Visual Studio\Installer\vswhere.exe" -latest -products * -find "Common7\IDE\Extensions\TestPlatform\vstest.console.exe"`
- EXIT_CODE: 0

This `vswhere.exe` location under `Program Files (x86)` is a fixed system path,
identical on every Windows host, and carries no account name and no machine name, so it
is written verbatim. The resolved `vstest.console.exe` path it returns varies by
installed Visual Studio edition and is recorded here as `<VSTEST_CONSOLE>` rather than
reproduced.

## Test command

- Command: `<VSTEST_CONSOLE> QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /EnableCodeCoverage /InIsolation /Logger:trx /ResultsDirectory:coverage\trx\remediation-2026-08-30T02-08-p2-t5 /TestCaseFilter:"TestCategory!=LiveOutlook"`
- EXIT_CODE: 0

## Attempts

**One attempt.** The run completed without stalling, so the hang-detection branch this
task authorizes was not entered: no CPU-time sampling was performed, no
`vstest.console.exe` or `testhost` process was terminated, and no retry was executed.
No shared MSBuild or `VBCSCompiler` worker was touched at any point.

For completeness the SHA-256 of the edited source file was recorded on both sides of
the run and is unchanged:

| Point | SHA-256 |
|---|---|
| Before the run | `972BCD8F142E50099783C4F92BDA624639E13DFE5A2767ED4AC189E2679D3DAB` |
| After the run | `972BCD8F142E50099783C4F92BDA624639E13DFE5A2767ED4AC189E2679D3DAB` |

## Console summary

```
Test Run Successful.
Total tests: 1254
     Passed: 1254
```

## TRX artifact

The default `vstest.console.exe` TRX filename embeds both the account name and the
machine name, so it is cited only in redacted form:

```
coverage/trx/remediation-2026-08-30T02-08-p2-t5/<ACCOUNT>_<MACHINE>_2026-08-30_03_31_03_net481.trx
```

Two `.coverage` attachments were produced under the same results directory by
`/EnableCodeCoverage`. No coverage figure is read from them: AC-16 is not re-opened by
this cycle and no coverage comparison is run.

## Acceptance

### Clause 1 — the TRX `Counters` element records `failed="0"`

Measured `Counters` attributes read from the TRX `ResultSummary`:

| Attribute | Value |
|---|---|
| `total` | 1254 |
| `executed` | 1254 |
| `passed` | 1254 |
| `failed` | **0** |
| `error` | 0 |
| `notExecuted` | 0 |
| `ResultSummary outcome` | `Completed` |

Required: `failed="0"`. Measured: **`failed="0"`**. PASS.

### Clause 2 — the corrected test is recorded with outcome `Passed`

The TRX `UnitTestResult` entry for the test carrying both corrected regions:

```
UnregisterNavigation_AfterRegisteringAtTwoDigitsAndShrinkingToNine_RemovesTheTwoDigitKeys => Passed (00:00:00.0015463)
```

Required: present with outcome `Passed`. Measured: **present, outcome `Passed`**. PASS.

Both clauses hold, so the test gate passes and no toolchain-loop restart is required.

## Output Summary

`vstest.console.exe` exited 0 on a single attempt with `Test Run Successful.`,
`Total tests: 1254`, `Passed: 1254`. The TRX `Counters` element records `failed="0"`
(and `error="0"`, `notExecuted="0"`), and the test carrying both corrected regions,
`UnregisterNavigation_AfterRegisteringAtTwoDigitsAndShrinkingToNine_RemovesTheTwoDigitKeys`,
is recorded with outcome `Passed`. The Phase 2 toolchain loop completed in one clean
pass: format, format-check, analyzer build, nullable build, tests.
