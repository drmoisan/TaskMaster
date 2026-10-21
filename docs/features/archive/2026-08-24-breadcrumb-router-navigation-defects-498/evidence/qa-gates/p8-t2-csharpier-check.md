# P8-T2 — Toolchain Step 1, Read-Only Verification at CI Parity

Timestamp: 2026-08-26T11-23

Pass number: **3** — the final pass.

Command: `pwsh -NoProfile -Command 'dotnet tool run csharpier check .; "EXIT_CODE: $LASTEXITCODE"'`

EXIT_CODE: 0

## Output Summary

- Exit code: **0** — the primary acceptance condition, met absolutely.
- Raw output, verbatim: `Checked 1525 files in 6128ms.`
- **Unformatted files: none.** CSharpier listed no file, which is what an exit code of 0 means for the
  `check` subcommand.

The file count rose from the `P0-T12` baseline's 1520 to 1525, accounted for by the five new source
files this feature added (`BreadcrumbBridgeRouter.Selection.cs`, `BreadcrumbBridgeRouter.Arrows.cs`,
`BreadcrumbBridgeRouterQueueTests.Part2.cs`, `BreadcrumbBridgeRouterTests.Selection.cs`,
`BreadcrumbStateModel.Row.cs`). All five are formatted.

Evidence artifacts under the feature folder did not enter the check: `.csharpierignore` already excludes
`**/evidence/**`, `*.trx` and `*.cobertura.xml`.

Pass 1 produced the identical result (`EXIT_CODE: 0`, `Checked 1525 files`). Pass 2 did not reach this
step, because `P8-T1` rewrote a file and the loop restarted immediately.

### Degradation status

The conditional degradation in this task is permitted ONLY IF the `P0-T12` baseline artifact recorded a
non-zero exit code for the identical command. `p0-t12-csharpier-check.md` records `EXIT_CODE: 0` with an
EMPTY unformatted set, so the degradation branch is **unavailable**. The gate stood at its primary
condition and met it. No `ExpectedExitCode:` is declared, because the observed exit code is 0.

The command is character-for-character the one CI runs (`dotnet tool run csharpier check .`), invoked
through `dotnet tool run` so the manifest-pinned CSharpier 1.2.6 is used rather than any global install.
