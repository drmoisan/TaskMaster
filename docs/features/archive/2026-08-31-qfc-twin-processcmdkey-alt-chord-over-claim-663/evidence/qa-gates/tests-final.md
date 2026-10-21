# Phase 4 — Final repository-wide test gate ([P4-T5])

Timestamp: 2026-09-01T23-09

Command: `pwsh -NoProfile -File scripts/vscode/Invoke-MSTest.ps1 -SearchRoot . -Configuration Debug`

EXIT_CODE: 0

This run was taken after the Phase 4 format, analyzer and type-check stages, so it exercises the
post-format source: the two `/t:Rebuild` gates recompiled every project from the reformatted files before
this run started.

## Discovered assemblies

```
Discovered 9 test assemblies.
```

## Runner summary block, transcribed verbatim

```
Test Run Successful.
Total tests: 6934
     Passed: 6934
 Total time: 26.8927 Seconds
```

The standard error stream is 0 bytes.

## Acceptance reading 1 — the total-count arithmetic

`Total tests:` = **6934**. `[P0-T12]` baseline total 6927 plus seven = **6934**. The figures are equal.

## Acceptance reading 2 — every failing name is in BASELINE_FAILURE_SET

Verbatim failing-test list:

```
(empty)
```

Zero console lines match the vstest failing-test line form `^\s*Failed\s+\S`. The condition holds
vacuously: there is no failing name, so none lies outside BASELINE_FAILURE_SET, which `[P0-T12]` recorded
as the empty set.

## Acceptance reading 3 — no failing name belongs to `QfcFormKeyHandlerTests`, and none is `ExecutingAssembly_ContainsNoFormDerivedType`

Both hold: the failing list is empty. The run transcript separately carries

```
Passed ExecutingAssembly_ContainsNoFormDerivedType [1 ms]
```

which is the structural guard AC-12 names. `[P5-T5]` cites this artifact for that reading.

## Acceptance reading 4 — the not-run figure

The runner printed **no** `Skipped:` line of its own and **no** single-line
`Failed! - Failed: N, Passed: N, Skipped: N, Total: N` summary, so the `[P0-T12]` fallback rule applies,
read the same way it was read there:

not-run = `Total tests:` − `Passed:` − `Failed:` = 6934 − 6934 − 0 = **0**.

BASELINE_NOT_RUN from `[P0-T12]` = 0. The two figures are **equal**.

Output Summary: The final uninstrumented repository-wide run exited 0 with 6934 of 6934 tests passed over
the same 9 discovered assemblies. The `Total tests:` figure equals the `[P0-T12]` baseline of 6927 plus
seven. The failing list is empty, so no failing name lies outside BASELINE_FAILURE_SET, none belongs to
`QfcFormKeyHandlerTests`, and none is `ExecutingAssembly_ContainsNoFormDerivedType`, which is separately
reported as passed. The not-run figure, derived by the arithmetic route `[P0-T12]` pinned, is 0 and equals
BASELINE_NOT_RUN. The test stage of the Phase 4 toolchain loop passes.
