# Phase 6 — Formatting verification, read mode

Timestamp: 2026-09-09T13-41
Task: [P6-T2]

Command: `dotnet tool run csharpier check .`
EXIT_CODE: 0

Complete verbatim output:

```text
Checked 1622 files in 4404ms.
```

## Acceptance check

| Condition | Required | Observed | Met |
|---|---|---|---|
| Exit code | `0` | **0** | yes |
| Files reported as needing formatting | `0` | **0** | yes |

The run checked 1622 files and printed no per-file line. `csharpier check` is read-only and prints one
line per non-conforming file, exiting 1 when any file needs formatting, so a bare summary line with
exit code 0 is the zero-drift result and the exit code is a discriminating gate here rather than a
write-mode command's uninformative success.

This confirms the `[P6-T1]` write-mode run reached a fixed point: re-running the formatter would
change nothing further, so step 1 of the toolchain loop is stable and the loop may proceed to step 2
without restarting.

Invocation was through `dotnet tool run`, so the manifest-pinned CSharpier 1.2.6 was used rather than
any globally installed version. A different global version would produce diffs disagreeing with
`.github/workflows/_format-check.yml`, which runs the pinned version after `dotnet tool restore`.

Output Summary: `EXIT_CODE: 0` and **0** files reported as needing formatting, across 1622 files
checked. Formatting is clean on the final state of the tree.
