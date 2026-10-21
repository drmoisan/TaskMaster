# [P1-T1] Fail-Closed Reflective Gate Helper

Timestamp: 2026-08-26T09-15

Task: [P1-T1]
Feature: docs/features/active/quickfiler-bug-family-446

## Change

`QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.cs` — the four-step descending
`GetConstructor` fallback chain (8-type, 7-type, 6-type, 5-type) was replaced by a single exact
`GetConstructor` lookup for the eight-parameter constructor declared at
`QuickFiler/Controllers/QfcStreamingDequeueConfidenceGate.cs:57-66`, guarded by a
FluentAssertions `Should().NotBeNull()` assertion.

Defect closed: the chain succeeded on the 5-type lookup whenever the wider lookups missed,
silently constructing a gate with `sourceActive` null, the default deadline and no progress
callback across every consuming test method in the class. The helper now fails closed.

## Verification

### Intra-phase compile

Command: `& $msbuild TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU"`
EXIT_CODE: 0

### `GetConstructor` count

Command: `git grep -c "GetConstructor" -- "QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.cs"`
EXIT_CODE: 0
Output: `QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.cs:1`

The output line ends in `:1`, so the count for that file is 1 as required. `[P0-T15]` recorded the
pre-change count as 4, so this is a real change and not a vacuous match.

### Scoped test run

Command: `& $vstest "QuickFiler.Test\bin\Debug\QuickFiler.Test.dll" /InIsolation "/Settings:scripts\vscode\TaskMaster.cli.runsettings" "/TestCaseFilter:FullyQualifiedName~QfcStreamingDequeueConfidenceGateTests" "/Logger:trx;LogFileName=p1-t1.trx" "/ResultsDirectory:docs\features\active\quickfiler-bug-family-446\evidence\regression-testing\p1-t1"`
EXIT_CODE: 0

- Total: `23`
- Passed: `23`
- Failed: `0`

The passed count of 23 equals the `[TestMethod]` total recorded by `[P0-T15]` (23 on the tree that
carries PR #610).

TRX: `docs/features/active/quickfiler-bug-family-446/evidence/regression-testing/p1-t1/p1-t1.trx`

## Note on TRX File Naming

`/Logger:trx` alone names the output `<account>_<HOST>_<timestamp>_net481.trx`, which would commit
a Windows account name and a machine name into the repository as a file name. Every TRX in this
plan is therefore produced with `/Logger:"trx;LogFileName=<task-id>.trx"` in addition to the
task-private `/ResultsDirectory:`, so the committed file name is deterministic and carries no
host identity. The logger is still `trx` and the file still lands in the directory the plan's
acceptance conditions name, so no acceptance condition is affected.

The TRX *payload* retains the `runUser` and `computerName` attributes that the VSTest TRX schema
writes unconditionally; no `vstest.console.exe` switch suppresses them, and editing them out would
falsify the evidence. They are left as produced. No artifact authored by this executor reproduces
them.

## Line Count

`QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.cs`: 424 -> 348 lines
(76 lines freed for the gate tests `[P1-T2]` through `[P1-T4]` add).

## Output Summary

Helper is fail-closed with exactly one `GetConstructor` lookup. Compile exit 0; scoped run 23/23
passed, 0 failed.
