---
name: koverage-cobertura-postprocessing-shape
description: Invoke-MSTestWithCoverage.ps1 post-processes the Cobertura dump - backslash filenames, classes already merged per file, test packages stripped, root attributes recomputed - so per-file coverage queries pinned to forward slashes or to summing sibling <class> nodes silently match nothing
metadata:
  type: project
---

`scripts/vscode/Invoke-MSTestWithCoverage.ps1` does not emit a raw `dotnet-coverage` Cobertura
dump. Before writing `-CoverageOutput` it calls `ConvertTo-KoverageCoberturaXml`
(`Invoke-MSTestWithCoverage.Helpers.ps1`) with **no `-PathSeparator` argument**, so four
transformations apply:

1. **`filename` uses `\`, not `/`.** `-PathSeparator` defaults to
   `[System.IO.Path]::DirectorySeparatorChar`, which is `\` on Windows. Attributes read
   `TaskMaster\Ribbon\EngineToggleStateCoordinator.cs`. A query pinned to
   `TaskMaster/Ribbon/...` matches **zero** rows and a coverage gate built on it reports 0 or
   nothing while appearing to run.
2. **`<class>` nodes are already merged per file.** `Merge-CoberturaClassesByFilename` collapses
   the `<Method>d__N` async state-machine classes and `<>c` closure classes into one `<class>` per
   filename and rewrites its `line-rate`/`branch-rate`. Read that attribute directly; the advice to
   "sum `lines-covered`/`lines-valid` across all `<class>` elements sharing the filename" applies
   only to an unprocessed raw dump.
3. **Test packages are stripped.** `Get-KoverageProjectAllowlist` enumerates every non-`*.Test`
   `*.csproj`/`*.vbproj`/`*.fsproj` assembly name under the repo root; packages outside that set are
   removed from `<packages>`. First-party production projects (including `TaskMaster`) are in.
4. **Root `<coverage>` attributes are recomputed** after stripping (`line-rate`, `branch-rate`,
   `lines-covered`, `lines-valid`, `branches-covered`, `branches-valid`), so the headline figure is
   already a production-only denominator — do not re-derive it.

Also: `$repoRoot` is `Resolve-Path "$PSScriptRoot\..\.."`, i.e. the **worktree** root, and the
`*.Test.dll` glob applies no `\.claude\` filter, so the script is safe to run from an agent
worktree rooted under `.claude/worktrees/`. It hard-fails if `vswhere.exe`, the vswhere-resolved
`Common7\IDE\Extensions\TestPlatform\vstest.console.exe`, or a global `dotnet-coverage` is missing —
note that vstest path differs from the `CommonExtensions\Microsoft\TestWindow` one used for direct
`vstest.console.exe` calls.

Related: [[project_coverage_delta_reproduce_baseline_counting_method]],
[[project_csharp_canonical_coverage_artifact_conversion]],
[[project_dotnet_coverage_denominator_nondeterminism]].
