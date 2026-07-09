# Baseline Tests — VS Code ClassLevel Runsettings (Cycle 2, Issue #292) — PRE-FIX

Timestamp: 2026-07-09T17-45

Command: `vstest.console.exe TaskMaster.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /EnableCodeCoverage`
EXIT_CODE: 1 (Test Run Failed)

Output Summary: Total tests 251; Passed 249; Failed 2. The two failures are both in the target class
`StoresWrapperEnumerationScopeTests`:

- `Init_HealthyMultiStore_PreservesIncludedSetAndOrder_AndClearsContextAfterReturn`
- `Init_EnumerationThrowsMidStream_LeavesCurrentStoreContextNull`

Both failures are the shared-static `CurrentStoreContext` parallel race: the target class is an unmarked
`CurrentStoreContext` scope-opener and null-baseline reader, and the `TaskMaster.cli.runsettings` force
`ClassLevel` parallelization, so a concurrent writer in another class pollutes the null baseline these tests
read. A second pre-fix run reproduced the identical 2 failures. Under the CI invocation form (P0-T5) the same
251 tests pass because `TaskMaster.Test` runs sequentially. This is the fail-before condition the P1 fix removes.
