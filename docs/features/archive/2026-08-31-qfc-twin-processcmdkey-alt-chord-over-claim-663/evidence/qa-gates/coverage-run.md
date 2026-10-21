# Phase 4 — Post-change coverage collection ([P4-T6])

Timestamp: 2026-09-01T23-18

Command: `pwsh -NoProfile -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug -CoverageOutput docs/features/active/qfc-twin-processcmdkey-alt-chord-over-claim-663/evidence/qa-gates/coverage.cobertura.xml`

That output path is the one AC-11 names.

EXIT_CODE: 0

The exit code is recorded but is not the gate, for the reason stated in the plan's reading guide: the
wrapper throws at line 236 when the inner run reports a non-zero exit. It is 0 here because the run
reported no failure; the standard error stream is 0 bytes and the wrapper ran past its throw site to its
terminal `Done.` line.

## Acceptance reading 1 — the Cobertura document exists at the AC-11 path

Path:
`docs/features/active/qfc-twin-processcmdkey-alt-chord-over-claim-663/evidence/qa-gates/coverage.cobertura.xml`

Exists: **True**. Byte size: **10,792,221 bytes**.

Wrapper output, verbatim, with the worktree root rendered as `<repo-root>`:

```
Code coverage results: <repo-root>\docs\features\active\qfc-twin-processcmdkey-alt-chord-over-claim-663\evidence\qa-gates\coverage.cobertura.xml.
Post-processing coverage XML for Koverage compatibility...
Done. Coverage artifact: <repo-root>\docs\features\active\qfc-twin-processcmdkey-alt-chord-over-claim-663\evidence\qa-gates\coverage.cobertura.xml
```

Because this run had no failing test, the wrapper reached its own Koverage post-processing block and
post-processed the document in place. `[P4-T7]` applies the same transform out of band as the plan
requires, which is safe because the transform is idempotent.

## Acceptance reading 2 — the failing-test list

```
Discovered 9 test assemblies.
Test Run Successful.
Total tests: 6934
     Passed: 6934
```

Verbatim failing-test list:

```
(empty)
```

Zero console lines match the vstest failing-test line form `^\s*Failed\s+\S`. The list therefore contains
no name outside BASELINE_COVERAGE_FAILURE_SET from `[P0-T13]`, which is the empty set.

The comparison uses the instrumented baseline rather than the `[P0-T12]` uninstrumented one because
instrumentation adds load-driven failures, so the two sets are not interchangeable. On this run the
instrumented suite completed on the first attempt, unlike the `[P0-T13]` baseline, which required one
retry after a diagnosed hang.

The `Total tests:` figure of 6934 also equals the `[P0-T12]` baseline of 6927 plus seven, so the seven new
methods were discovered and executed under instrumentation as well.

Output Summary: The instrumented post-change run completed on its first attempt with 6934 of 6934 tests
passed and an empty failing list, so no failing name lies outside BASELINE_COVERAGE_FAILURE_SET. The
Cobertura document was written to the AC-11 path at 10,792,221 bytes and post-processed in place by the
wrapper. The exit code is 0 and is recorded but not used as the gate.
