# Final QA Gate 4 — Nullable / Type-Check Build (P5-T4)

Timestamp: 2026-08-28T16-07
Command (CR-MSBUILD then CR-NULLABLE, fully expanded):

```
pwsh -NoProfile -Command '$vswhere = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"; $msbuild = & $vswhere -latest -products * -requires Microsoft.Component.MSBuild -find "MSBuild\**\Bin\MSBuild.exe" | Select-Object -First 1; & $msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true'
```

EXIT_CODE: 0

## Output Summary

```
    5 Warning(s)
    0 Error(s)

Time Elapsed 00:00:18.85
```

- Errors: **0**. Zero `CS86xx` nullable-flow diagnostics anywhere in the output, so the two files
  this plan edits that carry `#nullable enable` —
  `QuickFiler/Viewers/BreadcrumbDropDownHost.cs` (the new `Func<bool> MayTakeFocus` property, the
  `FocusAnchorIfPermitted` method, and the guarded `FocusPending`) and
  `QuickFiler/Viewers/BreadcrumbDropDownHost.Open.cs` — introduce no null-state defect. The
  non-annotated files (`QfcFormController.Deactivate.cs`, `QfcFormViewer.cs`,
  `ItemViewer.Breadcrumb.cs`, the interfaces) are outside nullable analysis by the repository's
  per-file opt-in model and are unaffected.
- Warnings: **5**, identical in count and source to the P0-T8 baseline — the uncoded
  `System.Reactive.PackagesConfigCheck.targets(31,5)` packages.config advisory. It carries no
  `CSxxxx` identifier, so `/p:TreatWarningsAsErrors=true` does not promote it.

**Delta versus baseline: zero.**

Two properties of this command are load-bearing and were preserved exactly, per CLAUDE.md and
`.claude/rules/csharp.md`:

- `/p:Nullable=enable` is **not** passed. Nullable enforcement here is per-file opt-in via
  `#nullable enable`; forcing the solution-wide property conscripts every file that has never
  adopted the pragma and is not what CI runs.
- `/t:Rebuild`, not `/t:Build`. MSBuild's up-to-date check does not invalidate on a command-line
  `/p:` change, so a warm `/t:Build` returns exit 0 having skipped `CoreCompile` on every project
  and the gate could not fail.
