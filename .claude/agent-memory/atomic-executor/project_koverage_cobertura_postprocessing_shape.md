---
name: koverage-cobertura-postprocessing-shape
description: Invoke-MSTestWithCoverage.ps1 post-processes the Cobertura dump ONLY when every test passes - backslash filenames, classes merged per file, test packages stripped, root attributes recomputed - so a baseline taken from a failing run has a different denominator (~70% raw vs ~85% processed) and per-file queries pinned to forward slashes match nothing
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

## Post-processing is conditional on the run passing — this silently changes the denominator

The script throws at `Invoke-MSTestWithCoverage.ps1:236` the moment **any** test in the search root
fails, which is *before* `ConvertTo-KoverageCoberturaXml` at `:340`,
`Assert-CoberturaLineCoverageThreshold` at `:341` and the `Set-Content` at `:343`. So:

- **Run with a failing test** → `coverage/coverage.cobertura.xml` holds the **raw** dump: absolute
  `class filename` attributes, no `<sources>`, third-party packages still present.
- **Run with everything passing** → the file holds the **post-processed** document.

Measured on one unchanged tree (2026-08-28, feature 489): raw `line-rate=0.7051` at
`lines-valid=82070`; processed `line-rate=0.8516` at `lines-valid=63901`. That is a 22 percent
denominator shift and a 14.6-point rate shift produced by **nothing but whether a test failed**.
Comparing across the two shapes manufactures a false regression or a false pass. A single flaky
test in the baseline run is enough to poison every later comparison.

**Remedy — recover a shape-matched baseline.** Before running the coverage script, copy the existing
`coverage/coverage.cobertura.xml` somewhere outside the repo. Afterwards, dot-source
`Invoke-MSTestWithCoverage.Helpers.ps1` and call `ConvertTo-KoverageCoberturaXml -XmlContent $raw
-RepoRoot <worktree root>` on the saved baseline. That is the identical function the script applies
at `:340`, so it converts the old raw baseline into the new shape and the comparison becomes
like-for-like using the repo's own code rather than hand arithmetic. On feature 489 this turned an
unusable "0.8516 vs 0.7051" into "0.8516 vs 0.8512 at `lines-valid` 63901 vs 63905" — a 0.006
percent denominator delta, well inside a 5 percent gate.

The final run's raw figure is **not** recoverable after the fact: `:343` overwrites the raw document
in place, and no switch retains it. Report the denominator each number belongs to, and never print a
delta across shapes.

Note also that `Assert-CoberturaLineCoverageThreshold` asserts against the **processed** content, so
its 80 percent floor is a first-party floor, not a raw-merge floor.

Also: `$repoRoot` is `Resolve-Path "$PSScriptRoot\..\.."`, i.e. the **worktree** root, and the
`*.Test.dll` glob applies no `\.claude\` filter, so the script is safe to run from an agent
worktree rooted under `.claude/worktrees/`. It hard-fails if `vswhere.exe`, the vswhere-resolved
`Common7\IDE\Extensions\TestPlatform\vstest.console.exe`, or a global `dotnet-coverage` is missing —
note that vstest path differs from the `CommonExtensions\Microsoft\TestWindow` one used for direct
`vstest.console.exe` calls.

Related: [[project_coverage_delta_reproduce_baseline_counting_method]],
[[project_csharp_canonical_coverage_artifact_conversion]],
[[project_dotnet_coverage_denominator_nondeterminism]],
[[project_utilitiescs_test_parallelism_flakiness]].
