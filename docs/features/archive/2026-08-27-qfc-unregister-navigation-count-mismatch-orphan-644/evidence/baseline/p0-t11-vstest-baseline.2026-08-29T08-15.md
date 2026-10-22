# Baseline — Test gate for the touched assembly ([P0-T11])

- Issue: #644
- Task: `[P0-T11]`
- Timestamp: 2026-08-29T08-15

## Runner resolution

Command: `& "C:\Program Files (x86)\Microsoft Visual Studio\Installer\vswhere.exe" -latest -products * -find "Common7\IDE\Extensions\TestPlatform\vstest.console.exe"`
EXIT_CODE: 0

Resolved runner path:

```
C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe
```

This is the Visual Studio 18 Community test platform. It is a machine-wide product installation
path rather than a user or repository path, so it carries no account, host, or worktree identity
and is recorded verbatim.

## Test run

Command: `<resolved-runner> QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /EnableCodeCoverage /InIsolation /Logger:trx /ResultsDirectory:coverage\trx\p0-t11 /TestCaseFilter:"TestCategory!=LiveOutlook"`
Working directory: repository root (`<repo-root>`)
EXIT_CODE: 0

TRX written to `coverage\trx\p0-t11\<account>_<HOST>_2026-08-29_13_42_18_net481.trx`. The default
`vstest.console.exe` TRX filename embeds the account and machine name, so the filename is
redacted here. `coverage/*` is matched by `.gitignore` line 144, so neither the TRX nor the
binary `.coverage` attachment dirties the tree.

## TRX `Counters` element

```
total=1248 executed=1248 passed=1248 failed=0 error=0 timeout=0 aborted=0 notExecuted=0
```

- **total: 1248**
- **passed: 1248**
- **failed: 0**

## BASELINE FAILURE SET

```
none
```

Derived by enumerating every `UnitTestResult` in the TRX whose `outcome` attribute is not
`Passed`. The enumeration returned a count of 0, so no test in this assembly is failing before
any edit is made.

## Gate outcome

The baseline failure set is empty, so the `REMEDIATION-REQUIRED` reporting branch this task
authorizes was **not** taken and Phase 1 may proceed. AC-15 admits no pre-existing failure in this
assembly and there is none.

Output Summary: Baseline test gate green. **total 1248 / passed 1248 / failed 0**, zero errors,
zero timeouts, zero aborted, zero not-executed. `BASELINE FAILURE SET` is empty. This is the
pre-change comparison point for `[P4-T5]`, whose acceptance requires `failed="0"` after the fix
plus the six new ledger tests and the seven named reconciliation tests all passing.
