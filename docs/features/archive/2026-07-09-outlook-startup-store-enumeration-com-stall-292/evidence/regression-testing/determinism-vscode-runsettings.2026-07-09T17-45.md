# Determinism Verification — VS Code ClassLevel Runsettings (Cycle 2, Issue #292) — POST-FIX

Timestamp: 2026-07-09T17-45

Command (each run): `vstest.console.exe TaskMaster.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /EnableCodeCoverage`

The `TaskMaster.cli.runsettings` force `ClassLevel` parallelization, which is the configuration under which
the shared-static `CurrentStoreContext` race manifested pre-fix (P0-T6: 2 of 251 failed). After marking the
three scope-opener/reader classes `[DoNotParallelize]`, the previously race-prone classes share the serialized
bucket, so no null-baseline reader observes a concurrent writer.

| Run | EXIT_CODE | Result | Total | Passed | Failed |
|---|---|---|---|---|---|
| 1 | 0 | Test Run Successful | 251 | 251 | 0 |
| 2 | 0 | Test Run Successful | 251 | 251 | 0 |
| 3 | 0 | Test Run Successful | 251 | 251 | 0 |
| 4 | 0 | Test Run Successful | 251 | 251 | 0 |
| 5 | 0 | Test Run Successful | 251 | 251 | 0 |

Result: 5/5 green. Combined with the P2-T4 CI-invocation-form pass (5141/5141), determinism is verified under
BOTH the CI invocation and the VS Code `ClassLevel` runsettings, satisfying AC4.
