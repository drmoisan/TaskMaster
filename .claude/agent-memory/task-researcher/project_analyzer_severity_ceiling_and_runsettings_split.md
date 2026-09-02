---
name: analyzer-severity-ceiling-and-runsettings-split
description: MSTEST0032 is the ONLY rule above suggestion in .editorconfig, no .globalconfig exists, and Invoke-MSTest.ps1's docstring names the wrong runsettings file (verified 2026-08-31 during issue #648)
metadata:
  type: project
---

Four cross-file facts verified on 2026-08-31 while researching issue #648
(`WpfUiDispatcherTests` ungated static swap). Each one repeatedly changes the answer to
"will this change trip an analyzer / which runsettings actually apply".

1. **`.editorconfig` has a hard severity ceiling.** `dotnet_analyzer_diagnostic.severity = suggestion`
   (line 27) is a catch-all, and a grep for `severity = error` returns ZERO matches while
   `severity = warning` returns exactly ONE (`MSTEST0032`, line 29). So MSTEST0032 is the only
   analyzer rule in the whole repo that can be promoted by `/p:TreatWarningsAsErrors=true`.
   CA2007 / MA0004 / RCS1090 / AsyncFixer01-06 / all of Sonar are unreachable as gate failures.

2. **`.globalconfig` does not exist**, despite CLAUDE.md §C#1.2 naming it as an analyzer-config input.
   `.editorconfig` at repo root is the only one.

3. **`scripts/vscode/Invoke-MSTest.ps1` and `Invoke-MSTestWithCoverage.ps1` use
   `scripts/vscode/TaskMaster.cli.runsettings`, not the repo-root `TaskMaster.runsettings`** —
   see `Resolve-RunSettingsPath`. The `Get-VsTestArgumentList` `.DESCRIPTION` block still says
   "repo-root TaskMaster.runsettings" and is stale. Both files carry `Workers=0` /
   `Scope=ClassLevel`; only the repo-root one carries the Code Coverage data collector.
   `.github/workflows/_mstest-coverage.yml` passes NO `/Settings:` at all, so CI runs sequentially.

4. **Legacy test csproj files vary on `LangVersion`.** `QuickFiler.Test.csproj` declares none (and no
   `GenerateDocumentationFile`, so IDE0005 is never reported in a command-line build); thirteen other
   projects pin `latest`/`preview`/`12.0`. Prefer block-scoped `using (...)` / `try`-`finally` in
   `QuickFiler.Test` rather than C# 8 `using` declarations.

**Why:** these determine whether a proposed C# change needs analyzer remediation at all, and which
parallelization a local run actually exercises versus CI.

**How to apply:** before claiming a rewrite "will trigger diagnostic X", check the severity ceiling in
(1) first. Before writing a test-run command into a plan or evidence artifact, resolve the runsettings
path from the script body, not from its docstring. See also
[[project_local_vstest_exclude_claude_worktrees]] for the worktree-exclusion half of the run command.
