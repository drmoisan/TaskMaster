# P6-T1 — Built Debug Test Assembly Inventory

Timestamp: 2026-09-13T06-45

Command: `pwsh -NoProfile -Command 'Set-Location -LiteralPath <worktree-root>; & <worktree-root>/scripts/vscode/Invoke-MSTestWithCoverage.ps1 -NoExecute'`

EXIT_CODE: 0

DISCOVERED_ASSEMBLY_COUNT: 9

Output Summary: The coverage entry point was run in discovery-only mode (`-NoExecute`), which returns after the discovery report and before any collection. It resolved `vstest.console.exe` through vswhere and printed `Discovered 9 test assemblies.` The count is greater than 0, so the Phase 0 task T8 solution rebuild did not need to be re-run.

## Entry-Point Output (verbatim, host path elided)

```
Using vstest.console: <visual-studio-install>\Common7\IDE\Extensions\TestPlatform\vstest.console.exe
Discovered 9 test assemblies.
Coverage output: <worktree-root>\coverage\coverage.cobertura.xml
```

## Independent Re-derivation of the Same Filter

A second command applied the entry point's own discovery predicate (`*.Test.dll` under a `bin/Debug` segment, excluding `obj`, `ref` and the editor-agent directory) and enumerated the matches repository-relative. It returned the same count.

Command: `pwsh -NoProfile -Command 'Set-Location -LiteralPath <worktree-root>; ... Get-ChildItem -Recurse -Filter "*.Test.dll" | Where-Object { ... } ...'`

EXIT_CODE: 0

REDERIVED_ASSEMBLY_COUNT: 9

```
QuickFiler.Test/bin/Debug/QuickFiler.Test.dll
SVGControl.Test/bin/Debug/SVGControl.Test.dll
Tags.Test/bin/Debug/Tags.Test.dll
TaskMaster.Test/bin/Debug/TaskMaster.Test.dll
TaskTree.Test/bin/Debug/TaskTree.Test.dll
TaskVisualization.Test/bin/Debug/TaskVisualization.Test.dll
ToDoModel.Test/bin/Debug/ToDoModel.Test.dll
UtilitiesCS.Test/bin/Debug/UtilitiesCS.Test.dll
VBFunctions.Test/bin/Debug/VBFunctions.Test.dll
```

## Acceptance

The recorded count is the bare integer 9, which is greater than 0. The rebuild contingency does not apply.

## Tooling Note

The Bash tool collapses a doubled backslash before a native executable receives it, so the re-derivation expressed its path predicates against a forward-slash normalisation of each full name rather than against backslash-escaped regular expressions. The first attempt at the re-derivation returned 0 for that reason alone; the entry point's own in-process discovery, which is the load-bearing observation for this task, was unaffected.
