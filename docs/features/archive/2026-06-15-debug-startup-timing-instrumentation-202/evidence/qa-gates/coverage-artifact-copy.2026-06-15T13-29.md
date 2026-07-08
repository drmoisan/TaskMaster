# Coverage Artifact Copy (Finding 2, Non-Blocking) (Issue #202, P3-T1/P3-T2)

Timestamp: 2026-06-15T13-29

Command: `cp TestResults/remed-final.cobertura.xml artifacts/csharp/coverage.xml`

EXIT_CODE: 0

Output Summary:

- Source: `TestResults/remed-final.cobertura.xml` (the merged Cobertura produced from the P2-T4
  test run via `dotnet-coverage merge -f cobertura`).
- Destination: `artifacts/csharp/coverage.xml` (workflow process artifact consumed by the
  feature-review-workflow contract; `artifacts/` is gitignored, so this file is for local /
  process use only and is not committed).
- Figure parity confirmed: the destination parses to the same figures recorded in P2-T4 —
  raw overall line-rate 76.37%, `TaskMaster.ApplicationGlobals` 77.63%,
  `TaskMaster.StartupTimingRecorder` 100%, `TaskMaster.NullStartupTimingRecorder` 100%.

Note on evidence path: `artifacts/csharp/coverage.xml` is a workflow report file, not an
evidence artifact under the `<FEATURE>/evidence/` scheme; per the plan's Evidence Location
Notice it is written to its workflow-named location. This recording of the copy action is the
canonical evidence artifact and resides under `<FEATURE>/evidence/qa-gates/`.

Finding 2 (non-blocking) is closed.
