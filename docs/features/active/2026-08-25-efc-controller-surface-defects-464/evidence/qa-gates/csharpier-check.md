# [P10-T3] Repository-wide CSharpier check

Timestamp: 2026-08-28T01-56
Task: [P10-T3]
Command: `dotnet tool run csharpier check .` from the worktree root, under `pwsh -NoProfile`
EXIT_CODE: 0

Run start (UTC): `2026-08-28T01-56-02`
Run end (UTC): `2026-08-28T01-56-07`

## Result — complete stdout, verbatim

```
Checked 1549 files in 4470ms.
```

Reported unformatted-file set: **empty**. Cardinality **0**.

## Acceptance

The task's first branch applies directly: **`EXIT_CODE: 0`**. The subset branch is not needed.

For completeness, the subset relation is also recorded and also holds:

| Set | Cardinality | Members |
|---|---|---|
| `BASELINE_UNFORMATTED` from `[P0-T9]` | 0 | (none) |
| Delivered unformatted set from this run | **0** | (none) |

The empty set is a subset of the empty set, and it contains none of the eight files `[P10-T1]` formats,
so the clause that guards the subset relation against concealing a regression is satisfied vacuously and
correctly.

The file count rose from 1543 at `[P0-T9]` to 1549 here. The six additional files are the three test
files this feature created plus files added by merged siblings #476 and #501, which the mandated
integration merge brought in. Every one of the 1549 is formatted.

## Consequence for `[P11-T2]` and `[P11-T15]`

`[P11-T15]`'s **authorised exception 1** is conditioned on `BASELINE_UNFORMATTED` being **non-empty**. It
is empty, so the exception **does not apply**, and the criterion beginning "`dotnet tool run csharpier
check .` reports no formatting differences" is checked off unconditionally under `[P11-T2]`. The pass
outcome for `[P11-T15]` is therefore the 74-of-74 form, not the 73-of-74 form, as far as this gate is
concerned.

## Loop position

No file was rewritten by this read-only command, so the toolchain loop does not restart. This is stage 1
(formatting) of the first and only Phase 10 pass; execution proceeds to `[P10-T4]`.

Output Summary: PASS. `dotnet tool run csharpier check .` exits 0 over 1549 files with an empty
unformatted set, matching the empty `[P0-T9]` baseline. The `[P11-T15]` authorised exception is not
triggered because the baseline set is empty.
