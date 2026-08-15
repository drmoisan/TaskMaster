---
name: powershell-coverage-nondeterministic-vsbuild-tests
description: PowerShell repo-wide coverage is NOT a stable constant — Invoke-VSBuild.Tests.ps1 dot-sources the script's top-level body, running vswhere.exe and Sync-PackageReferences.ps1, so figures shift run to run
metadata:
  type: project
---

Never quote a previously-recorded PowerShell repository-wide coverage number as if it were a
constant. Measure it in-session.

**Why:** `tests/scripts/vscode/Invoke-VSBuild.Tests.ps1` `BeforeAll` does
`. $scriptPath -NoExecute`, which executes the ENTIRE top-level body of
`scripts/vscode/Invoke-VSBuild.ps1`, not just its function definitions. `-NoExecute` returns at
line 158 — *after* the script has already run `& $vswherePath` (a real external executable) and
`& $syncScript -SolutionRoot $repoRoot`. `Sync-PackageReferences.ps1:148` writes `.csproj` files
via `[System.IO.File]::WriteAllText` when HintPaths are stale. How far that script gets depends on
ambient repository state, so its own coverage — and the repo-wide total — moves between runs.

Measured on the same source at head `22b5de02`:

| File | Committed evidence | Reviewer run | Analyzed lines |
|---|---|---|---|
| `Sync-PackageReferences.ps1` | 71 covered | 53 covered | 84 (identical) |
| `Invoke-MSTest.ps1` | 27 covered | 23 covered | 36 (identical) |
| repo-wide LINE | 502/702 = 71.51% | 494/717 = 68.90% | — |

Also true and stable: **Pester 5.6.1 emits no `BRANCH` counter** in its JaCoCo output
(`INSTRUCTION`, `LINE`, `METHOD`, `CLASS` only), so no PowerShell branch figure exists from the
repo's designated producer. Do not chase it; record it as FAIL with the producer-limit reason,
split across lines so `UNVERIFIED` never shares a line with a language label plus a coverage
keyword.

**How to apply:** run coverage yourself (`Invoke-Pester` with `CodeCoverage.Path` = all
`scripts/**/*.ps1`, `Run.Path = 'tests/scripts/vscode'`, `OutputFormat = 'JaCoCo'`, output written
outside the repo tree) rather than citing a committed figure. Also: `.csproj` does NOT put C# into
the coverage hook's changed-language set — `Get-ChangedLanguageSet` matches `.cs` only.

Related: [[build-ci-coverage-gate-fidelity-epic-outcome]],
[[epic-fanin-artifact-path-and-hook-regex]], [[powershell-coverage-gate]]
