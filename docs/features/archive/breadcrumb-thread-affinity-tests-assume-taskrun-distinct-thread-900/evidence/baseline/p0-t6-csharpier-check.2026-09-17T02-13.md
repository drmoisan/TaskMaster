# P0-T6 — Baseline Formatting State

Timestamp: 2026-09-17T02-13

Command: `dotnet tool run csharpier check .` run from the worktree root.

EXIT_CODE: 0

CHANNEL: COMMAND

## Output Summary

Final summary line, verbatim:

    Checked 1641 files in 5389ms.

CHECKED-FILES: 1641

The count is a positive integer, satisfying the second acceptance condition. The `check` subcommand
is read-only and never prints a `Formatted` line, so the summary line above is the whole of the
tool's success-case output; the count is read from it rather than inferred.

FORMAT-BASELINE: CLEAN

`FORMAT BASELINE NOT CLEAN` was not reached. The repository has no pre-existing CSharpier drift at
the merge base, so the repository-wide `dotnet tool run csharpier format .` run in P5-T1 cannot fold
a pre-existing repair into this branch. That is what makes P5-T1's `FORMAT_CHANGED_TREE:`
observation meaningful: any path the formatter rewrites there was clean at this baseline, so a
rewrite of any path other than the Write Set file is a `FORMAT SCOPE BREACH` rather than inherited
debt.

CSHARPIER-VERSION: 1.2.6, the version pinned by `dotnet-tools.json` and restored in P0-T4. The tool
was invoked through `dotnet tool run` so the manifest-pinned version was used, not a global install.

## Scope note

CSharpier 1.2.6 processes `*.cs`, `*.xml` and `packages.config`, and `.csharpierignore` excludes
`**/evidence/**`, `*.cobertura.xml`, `*.coverage`, `*.coveragexml`, `*.trx`, `*.csproj`, `*.props`
and `*.targets`. `artifacts/csharp/coverage.xml` matches none of those patterns and does not exist
at this point in the run; P5-T8 writes it only after the final clean iteration of the QC loop, so no
`format .` pass ever runs while that file is present. The git-ignored helper
`coverage/plan900-helper.ps1` written by the P0-T4 channel probe is a `.ps1` file and is outside
every extension CSharpier processes.

## Build lock

This task ran inside a held shared build lock for item 900, acquired before the check and released
immediately after it completed.
