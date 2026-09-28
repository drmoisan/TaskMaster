---
name: invoke-mstest-with-coverage-three-traps
description: scripts/vscode/Invoke-MSTestWithCoverage.ps1 concatenates an absolute -CoverageOutput onto the repo root, discovers every test assembly under -SearchRoot, and throws outright when dotnet-coverage is absent.
metadata:
  type: project
---

`scripts/vscode/Invoke-MSTestWithCoverage.ps1` is the right runner to name in a C# plan — it already
supplies `/InIsolation` and `/TestCaseFilter:TestCategory!=LiveOutlook` internally, which are the two
flags local runs otherwise omit. It carries three traps a plan must handle. All three verified
2026-09-12 on issue 742.

**1. `-CoverageOutput` must be repository-relative.** The script does
`$resolvedOutputPath = Join-Path $repoRoot $CoverageOutput`. PowerShell's `Join-Path`, unlike
`[IO.Path]::Combine`, does **not** detect an already-rooted child — it concatenates. So an absolute
value produces a malformed path rooted inside the repo. A plan that tries to keep raw coverage output
out of the repository by passing an absolute temp path does not achieve that; it just writes somewhere
broken. Pass a relative path under `coverage/`, which is gitignored (`.gitignore` has `coverage/*`),
and have the task delete it after transcribing the figures.

**2. `-SearchRoot .` runs the WHOLE repository suite.** The script does
`Get-ChildItem -Path $resolvedSearchRoot -Recurse -Filter '*.Test.dll'` filtered to the configuration,
excluding only `\.claude\` paths. With `-SearchRoot .` that is every test project, not the one the plan
is about. Two consequences: the acceptance criterion no longer matches the spec's scope, and the run
risks the known local hang in the UtilitiesCS shell-icon test classes that stall vstest on this machine.
Scope it: `-SearchRoot QuickFiler.Test`.

**3. It THROWS when `dotnet-coverage` is missing.** `dotnet-tools.json` pins only `csharpier`, so
`dotnet tool restore` does not provide `dotnet-coverage`; it needs a global install. The script throws
rather than degrading, so a plan with no branch for it aborts at the baseline task. Either install it in
the bootstrap task or give the coverage task an explicitly authorized skip branch — a bare `SKIPPED` is
invalid under the atomic-plan contract, so the authorization has to be in the task text itself.

**How to apply.** Read the script's own argument construction before naming it in a plan; do not assume
the parameter names behave like `[IO.Path]::Combine` or that a search root scopes the way the flag name
suggests. Related: [[coverage-seam-workaround-for-claude-worktrees]],
[[cobertura-postprocessing-is-a-zero-exit-proxy-not-a-test-result]].
