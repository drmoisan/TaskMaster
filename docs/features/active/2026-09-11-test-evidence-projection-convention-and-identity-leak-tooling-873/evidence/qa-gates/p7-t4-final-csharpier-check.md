# P7-T4 — Final C# Format Verification

Timestamp: 2026-09-13T07-04
Task: [P7-T4]

Command: dotnet tool run csharpier check .
Working directory: the worktree root.

EXIT_CODE: 0

Build lock: acquired for item 873 immediately before this command and released immediately after it
returned. The lock was held across this one command only.

## Verbatim output

```
Checked 1626 files in 4737ms.
```

The exit code was read in the same invocation and printed as `CSHARPIER_EXIT_CODE: 0`.

## Why verify mode rather than write mode

Verify mode is used deliberately. This delivery changes no C# source file; the repository formatter
ignore file already excludes project files from the check; and a repository-wide write-mode format
would rewrite source outside this delivery's footprint. Verify mode exits non-zero on drift, so its
exit code is a real discriminating observation rather than the always-zero result a write-mode run
would produce. The `Checked 1626 files` line is recorded alongside the exit code so the observation
is not the exit code alone: it shows the check actually examined the tree.

## Comparison against the P0-T7 baseline

| Figure | P0-T7 baseline | P7-T4 final |
|---|---|---|
| EXIT_CODE | 0 | 0 |

The recorded exit code is 0, which is equal to and therefore no worse than the baseline exit code of
0. The gate passes.

## Pass 2 — re-run after the P7-T7 toolchain restart

Timestamp: 2026-09-13T07-12

The toolchain loop restarted because P7-T7's coverage gate failed on its first measurement and the
remediation edited `tests/scripts/vscode/Invoke-MSTestWithCoverage.Projection.Tests.ps1`. That edit
touches no C# file, so this gate's pass-1 result was not invalidated by it; the command was
nonetheless re-run so the final result is one consecutive clean pass across every gate rather than a
set of results taken at different tree states.

Command: dotnet tool run csharpier check .
EXIT_CODE: 0

```
Checked 1626 files in 4881ms.
```

Build lock: acquired before the command and released after the last C# command of this pass returned.

## Output Summary

EXIT_CODE: 0 with `Checked 1626 files in 4737ms.` printed. No formatting drift exists anywhere in the
C# tree, and the result is no worse than the P0-T7 baseline exit code of 0. Pass 2 reproduced the
same result: EXIT_CODE: 0 with `Checked 1626 files in 4881ms.`
