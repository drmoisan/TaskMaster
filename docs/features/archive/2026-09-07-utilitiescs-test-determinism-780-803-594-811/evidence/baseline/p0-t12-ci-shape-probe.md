# P0-T12 — CI-verbatim AC4 command-shape probe

Timestamp: 2026-09-08T09-28
Task: [P0-T12]
Command: <vstest> <nine assemblies> /EnableCodeCoverage /InIsolation "/Logger:trx;LogFileName=p0-t12.trx" /ResultsDirectory:coverage/trx/p0-t12 /Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None "/TestCaseFilter:TestCategory!=LiveOutlook&FullyQualifiedName!~ShellUtilities&FullyQualifiedName!~SysImageListHelper&FullyQualifiedName!~OSBrowser"
EXIT_CODE: 0

AC4_COMMAND_SHAPE: CI-VERBATIM

This probe was run once against the pre-change tree, before any source edit, to establish whether
the CI-verbatim shape is locally viable. No `/Settings:` file is passed, so parallelism comes only
from `[assembly: Parallelize(Workers = 0, Scope = ClassLevel)]` at
`UtilitiesCS.Test/Properties/AssemblyInfo.cs:18-21`, exactly as in
`.github/workflows/_mstest-coverage.yml:99`. The nine assemblies are enumerated explicitly rather
than discovered recursively, so the dot-claude worktree discovery problem cannot arise.

## TRX counters

`coverage/trx/p0-t12/p0-t12.trx` exists.

```
total=7153 executed=7153 passed=7153 failed=0 error=0 timeout=0 aborted=0 notExecuted=0
```

No test produced an outcome other than `Passed`. The failing-test enumeration the acceptance
condition would otherwise require is therefore empty, and the CI-VERBATIM branch applies: the set
of failing tests is a subset of the six named failure modes this item repairs because it is the
empty set.

## Wall clock

PROBE_SECONDS: 62

A P8 pair task runs two such runs back to back, so the estimate for each of P8-T1 through P8-T5 is
about 124 s of test time plus process startup, comfortably inside the 600000 ms tool timeout.

## `.coverage` file cleanup

| Observation | Value |
|---|---|
| `*.coverage` files found under `coverage/trx/p0-t12` | 2 |
| `*.coverage` files remaining after deletion | 0 |

vstest emits two `.coverage` files per run under `/EnableCodeCoverage`. Both were deleted after
the counters were read, as the task requires. They are large binaries and are never committed.

## Consequence for Phase 8

Because `AC4_COMMAND_SHAPE` is `CI-VERBATIM`, the P8-T1 through P8-T5 runs use the base argument
shape with no `/Settings:` argument. The `RUNSETTINGS-FALLBACK` branch does not apply and
`/Settings:TaskMaster.runsettings` is not added.

## Acceptance evaluation

- `coverage/trx/p0-t12/p0-t12.trx` exists. PASS
- Counters recorded. PASS
- `PROBE_SECONDS: 62` recorded. PASS
- Exactly one of the two shape verdicts is recorded: `AC4_COMMAND_SHAPE: CI-VERBATIM`. PASS
- `.coverage` files under the results directory were deleted after the counters were read. PASS

## Output Summary

The CI-verbatim AC4 command shape is locally viable. One probe run: 7153 tests, 7153 passed, 0
failed, 62 s, exit 0. Phase 8 will use the CI-verbatim shape with no runsettings file.
