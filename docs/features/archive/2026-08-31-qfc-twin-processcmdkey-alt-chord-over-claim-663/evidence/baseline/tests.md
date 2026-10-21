# Phase 0 — Baseline repository-wide test state ([P0-T12])

Timestamp: 2026-09-01T22-10

Command: `pwsh -NoProfile -File scripts/vscode/Invoke-MSTest.ps1 -SearchRoot . -Configuration Debug`

EXIT_CODE: 0

The exit code is recorded but is not the gate, because the wrapper throws at line 130 on any failure. It
is 0 here because the run reported no failure.

The command was run twice, identically. The first invocation was launched detached and its exit code was
not captured; it reported the same discovered-assembly count, the same `Total tests:` figure, the same
`Passed:` figure and the same empty failing list. The second invocation was run with the exit code
captured and is the run recorded here. Both runs agree on every figure below.

## Discovered assemblies

```
Discovered 9 test assemblies.
```

The count is greater than zero, so this is not a discovery defect.

## Runner summary block, transcribed verbatim

```
Test Run Successful.
Total tests: 6927
     Passed: 6927
 Total time: 27.7039 Seconds
```

That is the complete summary block. The runner printed no `Failed:` line and no `Skipped:` line, and it
printed no single-line `Failed! - Failed: N, Passed: N, Skipped: N, Total: N` summary, because the run
succeeded.

## Figures

- `Total tests:` = **6927**
- `Passed:` = **6927**
- `Failed:` = **0** (no `Failed:` line was printed; the run is reported as successful and the failing-test
  list is empty)

## BASELINE_NOT_RUN

The runner printed **no skipped or inconclusive figure under any label** — there is no `Skipped:` line of
its own in the summary, and there is no single-line `Failed! - ...` summary carrying a `Skipped:` field.
The `[P0-T12]` fallback route therefore applies:

**BASELINE_NOT_RUN is derived arithmetically as `Total tests:` minus `Passed:` minus `Failed:`
= 6927 − 6927 − 0 = 0.**

**Route used: the arithmetic derivation, not a printed label.** Every later run that reads a not-run
figure — `[P3-T3]` and `[P4-T5]` — must read it the same way: if the run prints a `Skipped:` figure under
some label it is read there and the divergence is recorded; if it prints none, the same subtraction is
used.

## BASELINE_FAILURE_SET

Verbatim list of every failing test name reported by this run:

```
(empty)
```

**BASELINE_FAILURE_SET = { } (the empty set).**

Zero console lines match the vstest failing-test line form `^\s*Failed\s+\S`. The standard error stream
was empty (0 bytes).

This is a measured reading. The spec's "Reference points to re-measure" subsection recorded a prior
observation of 15 pre-existing load-driven failures concentrated in three `QfcItemController` test files;
that figure was explicitly labelled a prior observation to be re-measured and not quoted as current state.
On this uninstrumented run in this worktree, no such failure appeared. The spec's own instruction is
followed: the current measurement supersedes the prior observation.

The consequence for the later gates is that they become stricter rather than weaker. `[P2-T3]`,
`[P3-T3]` and `[P4-T5]` compare against an empty baseline set, so any failing name other than the three
expected Phase 2 reds fails the gate outright.

`[P0-T13]` captures BASELINE_COVERAGE_FAILURE_SET separately from the instrumented run, because
instrumentation adds load-driven failures and the two sets are not interchangeable.

## Arithmetic pinned for later phases

`[P2-T3]`, `[P3-T3]` and `[P4-T5]` each require `Total tests:` to equal this baseline plus seven:
**6927 + 7 = 6934**.

Output Summary: The baseline uninstrumented repository-wide run discovered 9 test assemblies, ran 6927
tests, passed 6927, and reported no failing test. BASELINE_FAILURE_SET is the empty set. The runner
printed no skipped or inconclusive figure under any label, so BASELINE_NOT_RUN was derived arithmetically
as 6927 − 6927 − 0 = 0 and the arithmetic route is pinned for every later run. The expected post-change
total for Phases 2 through 4 is 6934.
