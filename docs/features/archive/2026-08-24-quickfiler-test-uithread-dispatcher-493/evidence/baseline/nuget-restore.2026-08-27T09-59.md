# NuGet Package Restore (P0-T5)

Timestamp: 2026-08-27T09-59
Task: [P0-T5]
Command: `pwsh -NoProfile -File scripts/vscode/Invoke-Restore.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform "Any CPU"` (run from `<repo-root>`)
EXIT_CODE: 0
Output Summary: Restore succeeded with 0 warnings and 0 errors, installing 172 packages to
packages.config projects in 2.77 s. The marker path
`packages/Meziantou.Analyzer.3.0.174/build/Meziantou.Analyzer.props` — named by the
`EnsureNuGetPackageBuildImports` error target of `QuickFiler.Test/QuickFiler.Test.csproj` — exists
under `<repo-root>`.

## Redacted tail of the restore log

```
         Installed:
             172 package(s) to packages.config projects
     1>Done Building Project "<repo-root>/TaskMaster.sln" (Restore target(s)).

Build succeeded.
    0 Warning(s)
    0 Error(s)

Time Elapsed 00:00:02.77
```

Raw log: `TestResults/plan-logs/p0-t5/restore.log` (git-ignored; not committed).

## Acceptance verification

| Check | Result |
| --- | --- |
| `EXIT_CODE` | `0` |
| `packages/Meziantou.Analyzer.3.0.174/build/Meziantou.Analyzer.props` exists under `<repo-root>` | `True` (886 bytes) |

## Invocation note

A first attempt launched the script through `Start-Process -PassThru -NoNewWindow` with
`-ArgumentList` supplied as a PowerShell array. That form strips the quoting around the two-word
value `Any CPU`, so the script received `Any` and `CPU` as separate arguments and failed with
`A positional parameter cannot be found that accepts argument 'CPU'` (exit code 1). The command was
re-run with a direct `&` invocation, which preserves the quoted argument, and succeeded with exit
code 0. Only the successful run's result is recorded as the baseline; the failed attempt is recorded
here for the audit trail and was an invocation-form defect, not a restore failure.
