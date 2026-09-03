# P0-T17 — Numeric Baseline Coverage Capture (Issue #751)

Timestamp: 2026-09-03T14-31

Rung: 3

Command: `Get-ChildItem -Path 'coverage\trx\P0-T14' -Recurse -Filter '*.coverage'`

EXIT_CODE: 0

The recorded `Command:` is the locate command, because rung 3 was reached on the attachment count and **no
conversion command was attempted**. The output directory was created first, as the task requires:
`New-Item -ItemType Directory -Path 'coverage\numeric' -Force | Out-Null`.

## Output Summary

```
COVERAGE_CAPTURE_BLOCKED
```

Observed `.coverage` file count under `coverage\trx\P0-T14`: **2**

The task requires the locate search to return exactly one file. It returned two. Per the rung-3 rule, an
arbitrary member of the set was **not** converted and **no coverage number is stated**.

No error text is transcribed, because rung 3 was reached on the file count rather than on a command failure:
no conversion command was run, so no command produced an error. The recorded count is the whole of the
required evidence for this outcome.

The two file paths are deliberately not transcribed, because the vstest results directory names embed the
account name. Both files carry an identical byte length of 21356385. Their structural relationship, recorded
without paths: one is the attachment published into the run's results directory, and the other is the
in-run copy that vstest retains under its `In\<machine>\` subtree. vstest produced both from the single
P0-T14 run; they are not two separate collections.

## Converter availability at the time of this task

| Rung | Converter | Available |
|---|---|---|
| 1 | `dotnet-coverage` (`Get-Command dotnet-coverage`) | Yes |
| 2 | `CodeCoverage.exe` via `vswhere` | Not probed — rung 1's converter was present, so rung 2 was not reached on converter grounds |

Converter availability is recorded for completeness. It is **not** what selected rung 3. Rung 3's trigger
condition is a disjunction — neither converter available, **or** the located `.coverage` file count is not
exactly one, **or** the conversion command exits non-zero — and the second disjunct is satisfied here. The
presence of a working converter does not license converting one of two candidate files, because the task
explicitly forbids converting an arbitrary member of the set.

## Acceptance

The task defines exactly two acceptance outcomes and requires the artifact to state which one it recorded.

**This artifact records the rung-3 outcome.** Its `Output Summary:` begins with the header
`COVERAGE_CAPTURE_BLOCKED`, records the observed `.coverage` file count as the integer 2, and states no
coverage number. The required `Rung:` field is recorded as `Rung: 3` at the head of this artifact, which
P4-T12 reads when selecting its own outcome.

## Consequence for P4-T12

P4-T12 compares its own rung against this value. Because this artifact carries `Rung: 3` and
`COVERAGE_CAPTURE_BLOCKED` with no numeric values, P4-T12 cannot record its **Numeric** outcome (which
requires both artifacts to carry real numeric values from the same rung) and cannot record its
**Denominator-shift** outcome (which also requires both artifacts to carry real numeric values).

If the P4-T5 attachment search likewise does not return exactly one file, P4-T12 records the **Blocked**
outcome, and the completion report must state the coverage criterion as remediation-required rather than
PASS. That is a defined outcome of this plan, not a failure of this task: this task's acceptance is
satisfied by recording the rung-3 outcome faithfully.
