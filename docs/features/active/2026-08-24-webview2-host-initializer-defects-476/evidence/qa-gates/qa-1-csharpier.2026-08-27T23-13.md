# QA Gate 1 of 4 — CSharpier ([P4-T1], post-base-merge re-run)

Timestamp: 2026-08-27T23-13

Command:
```
dotnet tool run csharpier format QuickFiler/Viewers/WebView2BreadcrumbHost.cs QuickFiler/Viewers/WebView2CoreInitializer.cs QuickFiler/Viewers/IWebViewCoreInitializer.cs QuickFiler.Test/Controllers/WebView2CoreInitializerTests.cs QuickFiler.Test/Viewers/WebView2BreadcrumbHostTests.cs QuickFiler.Test/Viewers/WebView2BreadcrumbHostContractTests.cs
dotnet tool run csharpier check .
```
(both run through `pwsh -NoProfile` from the workspace root)

EXIT_CODE: 0

## Output Summary

- Scoped format exit code **0**; output `Formatted 6 files in 2472ms.` That figure is CSharpier's
  **processed** count, not a rewrite count. Rewrite was measured independently by hashing all six
  files before and after: every MD5 was unchanged and `git status --porcelain` was empty after the
  apply, so **no file was rewritten** by this step.
- Repository-wide `check .` exit code **0**; output `Checked 1545 files in 4538ms.` CSharpier
  reported **no file** as unformatted.
- Acceptance part 1: the `check .` output names none of the six touched files. Satisfied — it names
  no file at all.
- Acceptance part 2: the `check .` output names no file that was not already recorded in the Phase 0
  baseline `baseline-1-csharpier-check.2026-08-27T19-59.md`. That baseline recorded an empty
  reported-file list; this run's reported-file list is also empty, so the containment holds.
- File count moved from 1540 (Phase 0 baseline) to 1545. The five additional files are the merged
  siblings' and this feature's new `*.cs` files, not new formatting debt.

## Deviation record (Decisions Record item 9)

The mandated command in `CLAUDE.md` §C#1.1 is `dotnet tool run csharpier format .`. This task applies
the formatter file-scoped to the six files this feature touches and then verifies read-only at full
repository scope. A repository-wide apply could rewrite a pre-existing deviation in a file on this
feature's forbidden list, manufacturing a scope violation from the toolchain rather than from the
change. The read-only `check .` at full scope is the gate CI enforces, and it passed with zero
reported files. `[P5-T38]` records this reconciliation at criterion check-off.

## Pre-run scan for banned APIs in modified files

`grep -n 'DateTime\.Now|Random\.Shared|Thread\.Sleep|Task\.Delay'` over all six files returned zero
matches, so no banned-API remediation was required.
