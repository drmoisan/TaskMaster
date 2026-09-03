# QA Gate — MSTest Regression Re-run (Rx-dependent assemblies)

- Task: [P2-T9]
- Phase: Phase 2 — Verification & Final QC

Timestamp: 2026-09-02T23-16

Command: `& $vstestPath UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /EnableCodeCoverage /InIsolation /Logger:trx /TestCaseFilter:"TestCategory!=LiveOutlook"`

EXIT_CODE: 0

## Tool and assembly resolution

`vstest.console.exe` was resolved with the same resolution `.github/workflows/_mstest-coverage.yml:60-67` uses:

- `$vswherePath = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'` — present.
- `& $vswherePath -latest -products * -find 'Common7\IDE\Extensions\TestPlatform\vstest.console.exe' | Select-Object -First 1` — resolved to the Visual Studio 18 Community Test Platform copy of `vstest.console.exe`.

The two test assemblies were located with the same filter `.github/workflows/_mstest-coverage.yml:70-76` uses (recursive `*.Test.dll` search; path matches `bin\Debug\`, excludes `obj\` and `ref\`). That filter yields 9 assemblies in this worktree; the two named by this task are present in the filtered set:

- `UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll`
- `QuickFiler.Test\bin\Debug\QuickFiler.Test.dll`

The run was scoped to exactly those two Rx-dependent assemblies, as this task's command specifies.

## Output Summary

Literal Passed/Failed/Total counts printed in the console output:

```
Test Run Successful.
Total tests: 6095
     Passed: 6095
 Total time: 47.3269 Seconds
```

- `Total tests: 6095`
- `Passed: 6095`
- `Failed: 0`

Derivation of the `Failed` count: `vstest.console.exe` prints a `Failed:` summary line only when at least one test fails; on a fully passing run it prints `Test Run Successful.` with `Total tests:` and `Passed:` only. Six lines of the captured console output contain the substring `Failed`, and all six are test *names* (for example `Passed TryGetUrlStreamAsync_FailedResponse_ReturnsNull [4 ms]`), each prefixed `Passed`; none is a failure marker. The single line containing `Skipped` is likewise a test name that passed.

Independent confirmation from the run's own TRX result file (`ResultSummary/Counters`, `outcome="Completed"`):

```
total=6095 executed=6095 passed=6095 failed=0 error=0 timeout=0 aborted=0 notExecuted=0
```

Both sources agree: 6095 tests executed, 6095 passed, 0 failed, 0 errored, 0 timed out, 0 aborted, 0 not executed. Process exit code was 0.

TRX and coverage attachment paths are not quoted in this artifact because they embed the operator account name and worktree-root path; they were written under the worktree's `TestResults/` directory, which is excluded by the repository `.gitignore` rule `[Tt]est[Rr]esult*/` and is therefore not part of the committed change set.

## Acceptance

- `EXIT_CODE: 0` recorded: PASS.
- `Failed: 0` recorded: PASS.
- Confirms the Rx-dependent MSTest suites pass unchanged after the `Directory.Build.props` addition.
