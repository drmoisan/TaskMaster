# NonBlockingDelayTests scoped run (P2-T4)

Timestamp: 2026-09-02T23-08

Command: `& $vstest TaskMaster.Test\bin\Debug\TaskMaster.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation /TestCaseFilter:"FullyQualifiedName~NonBlockingDelayTests"`

Tool resolution used the Block K prelude (`MSBUILD_FOUND: True`, `VSTEST_FOUND: True`).

EXIT_CODE: 0

TotalCount: 3
PassedCount: 3
FailedCount: 0

## Per-node outcomes

| Test method | Outcome |
|---|---|
| `WaitAsync_WithNoDispatcher_CompletesAfterInterval` | Passed [58 ms] |
| `WaitAsync_ZeroDelay_CompletesWithoutPump` | Passed [< 1 ms] |
| `WaitAsync_SingleArgumentOverload_CompletesOnSystemTimeProvider` | Passed [1 ms] |

## Output Summary

- `Test Run Successful.` / `Total tests: 3` / `Passed: 3`.
- All three test methods declared by `TaskMaster.Test/AppGlobals/NonBlockingDelayTests.cs`
  after the Block B rewrite passed on the pump-less MSTest host.
- No retry branch was used. The run passed on its first execution.

## Evidence preservation performed by this task

Before this command ran, the round-14 failing-run record that occupied this path was preserved
by renaming it to
`docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/evidence/regression-testing/nonblockingdelay-zero-delay-observation.2026-09-02T10-30.md`.

Branch taken: `Test-Path` on this path returned `True` and `Test-Path` on the preserved path
returned `False`, so the rename was performed exactly once. In the renamed file only, the first
line was changed to the zero-due-time observation heading and the single line
`ExpectedExitCode: 1` was inserted immediately below its `EXIT_CODE: 1` schema field (not below
the second occurrence of that text inside the fenced determinism-confirmation block). Nothing
else in the preserved record was changed.

That preserved file remains the authority for the executed zero-due-time observation cited by
Block B's zero-delay doc comment, by `spec.md`'s Assumptions bullet, by its Test Strategy
Finding 1 paragraph, and by its Risks zero-delay entry.

PreservedRecordPresent: True
