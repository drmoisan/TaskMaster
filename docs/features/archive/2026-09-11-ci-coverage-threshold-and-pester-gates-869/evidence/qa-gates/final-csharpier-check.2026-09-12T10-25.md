# Final C# formatter verification (P10-T7)

Timestamp: 2026-09-14T21-15

Command: `pwsh -NoProfile -Command '<worktree prologue>; dotnet tool run csharpier check .'`
EXIT_CODE: 0

Issued through `pwsh` because the Bash allowlist grants no bare `dotnet` entry, and through `dotnet tool run` so the manifest-pinned CSharpier 1.2.6 is used rather than the globally installed 1.3.0.

## Output, verbatim

```
Checked 1639 files in 5417ms.
```

## Recorded observations

- Count of files the tool reported as checked: **1639**.
- Full list of files the tool reported as unformatted: **empty**.

## Comparison against the P0-T17 baseline

| Observation | P0-T17 baseline | This run |
| --- | --- | --- |
| Files checked | 1639 | 1639 |
| Unformatted list | empty | empty |
| Exit code | 0 | 0 |

The unformatted list is **identical** to the list recorded in the P0-T17 baseline artifact: both are empty. The delivery therefore introduced no new formatting difference, and the list gained no entry, so the loop does not restart at P10-T1.

The file count is unchanged at 1639. That is the expected result: the only file this delivery could have added to CSharpier's input set was the contingency fixture `tests/scripts/vscode/fixtures/sync-package-references/packages.config`, which CSharpier processes, and the P6-T7 contingency recorded `Decision: NOT REQUIRED`, so it was not created. No other file this delivery wrote is of a type CSharpier checks: the PowerShell files are not, the YAML workflow files are not, and the markdown documents and evidence artifacts are excluded by the `**/evidence/**` and markdown handling in `.csharpierignore`.

This step is read-only and rewrites no file, so its exit code together with its printed file list is a sufficient observation and no before-and-after tree comparison is required.

Output Summary: CSharpier 1.2.6 checked 1639 files and reported no unformatted file, exiting 0. Both the file count and the empty unformatted list are identical to the P0-T17 baseline, so the delivery introduced no C# formatting difference.
