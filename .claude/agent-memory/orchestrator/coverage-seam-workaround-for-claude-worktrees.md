---
name: coverage-seam-workaround-for-claude-worktrees
description: How to get real C# coverage from an agent worktree despite issue #752 — dot-source TWO files and call Invoke-DotnetCoverageCollection with an explicit -TestAssembly list
metadata:
  type: project
---

`scripts/vscode/Invoke-MSTestWithCoverage.ps1` cannot be run as a script from any worktree
under `.claude/worktrees/`. Its assembly-discovery predicate ends with
`$_.FullName -notmatch '\\\.claude\\'`, so every assembly in such a worktree is filtered
out, `$testAssemblies` is empty, and it throws `"No test assemblies found ... Build first."`
**producing no Cobertura document at all**. Tracked as issue #752. Do not work around it by
running the script from the main checkout — that measures the wrong tree.

**Working recipe (validated end to end 2026-09-03, 65 tests, document produced):**

1. Dot-source **two** files, not one:
   - `scripts/vscode/Invoke-MSTestWithCoverage.ps1` → `Invoke-DotnetCoverageCollection`,
     `Get-DotnetCoverageArgumentList`, `Resolve-RunSettingsPath`. Its entry point is guarded
     by `if ($MyInvocation.InvocationName -ne '.')`, so dot-sourcing does not run main.
   - `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1` → the post-processing set. It
     dot-sources `ClosureFilter.ps1`, `PackageRate.ps1` and `Threshold.ps1` itself, so this
     one line also gives you `Assert-CoberturaLineCoverageThreshold` and
     `Remove-CoberturaExemptClosureCoverage`.

   The main script dot-sources Helpers **inside** its main function body, so dot-sourcing
   the main script alone gives you the collection seam but none of the post-processing.

2. Call `Invoke-DotnetCoverageCollection` with an explicit `-TestAssembly` string array.
   That bypasses the broken discovery entirely. Use this seam rather than hand-rolling a
   `vstest.console.exe` command line: `Get-DotnetCoverageArgumentList` is what appends
   `/InIsolation` and `/TestCaseFilter:TestCategory!=LiveOutlook`, and a bare vstest call
   omits the LiveOutlook filter (see [[bare-vstest-omits-liveoutlook-filter]]).

3. It **throws** when the inner run exits non-zero, but only *after* dotnet-coverage has
   written the document. Wrap in try/catch, record the exit code, then post-process anyway.

4. Post-process yourself with `ConvertTo-KoverageCoberturaXml`, writing to a path you chose.
   Because you control this step, the raw/processed document state is deterministic — which
   removes the need for any raw-vs-processed branching in a plan.

**Trap: post-processing does NOT strip every absolute path.** On the validated run the raw
document had 1442 `filename=` attributes carrying a drive letter and the processed document
still had **410**. `ConvertTo-KoverageRelativePath` strips only the repo-root prefix, so any
source outside the repo root keeps its absolute path. Never copy a `filename` value into a
committed evidence artifact on the assumption that post-processing made it relative — apply
the path-hygiene reduction in your own extraction code
(see [[../_shared_no_absolute_host_paths]]).

**Consequence: a document-wide raw-vs-processed discriminator always reports `raw`.** A plan
that decides "the document is raw if ANY `class/@filename` carries a drive letter" is true on
every run, including a correctly processed one, so a gate demanding `processed` self-blocks
permanently. Restrict any such scan to the `class` elements the filename match condition
already selected — for first-party paths under the repo root the count is zero — and record
the whole-document residual count as an informational figure only, never as a gate.

**Trap: `lines-valid` is not a stable denominator.** A single small test assembly still
reported `lines-valid: 45521`, because dotnet-coverage instruments every loaded assembly.
Two runs of an identical tree can differ, so a repository-wide `line-rate` comparison needs
a denominator-comparability branch before it can be a gate
(see [[coverage-mode-raw-vs-processed-is-flake-sensitive]]).

Related: [[coverage-script-now-excludes-claude-worktrees]] records the exclusion itself;
this note records the way through it.
