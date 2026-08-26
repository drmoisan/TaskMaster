# Final QC stage 1 (verification) — repository-wide CSharpier check

Timestamp: 2026-08-26T13-42
Task: [P7-T2]

Command (read-only, run from the worktree root):

```
dotnet tool run csharpier check .
```

EXIT_CODE: 0

Output (verbatim):

```
Checked 1520 files in 5142ms.
```

## Reported unformatted-file set

**Empty.** CSharpier 1.2.6 reported no unformatted file across the 1520 files it checked.

## Comparison against the `[P0-T9]` baseline

| | Files checked | Unformatted files | Exit code |
|---|---|---|---|
| `[P0-T9]` baseline | 1520 | 0 (empty set) | 0 |
| `[P7-T2]` post-change | 1520 | 0 (empty set) | 0 |

The acceptance condition is satisfied on its first branch: `EXIT_CODE: 0`. The reported set is empty,
so it is trivially equal to the empty baseline set and contains none of the nine owned files.

Because the `[P0-T9]` baseline unformatted set was **empty**, the authorized
pre-existing-unformatted-file exception described in `[P8-T4]` and `[P8-T13]` does **not** apply, and
the `spec.md` criterion beginning "`dotnet tool run csharpier check .` reports no formatting
differences" is checked off unconditionally on this evidence.

Output Summary: EXIT_CODE 0. 1520 files checked, 0 unformatted, matching the empty `[P0-T9]` baseline
set exactly.
