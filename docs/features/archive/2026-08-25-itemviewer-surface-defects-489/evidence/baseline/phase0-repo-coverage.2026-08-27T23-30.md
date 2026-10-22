# Phase 0 — Repository-wide Coverage Baseline (P0-T14)

Timestamp: 2026-08-27T23-30
Command: pwsh -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug -CoverageOutput coverage\coverage.cobertura.xml
EXIT_CODE: 1
ExpectedExitCode: 1

BaselineLineRate: 0.13296151701059677
BaselineBranchRate: 0.10335641981885989
BaselineLinesValid: 8965
BaselineRepoPassed: 75
BaselineRepoFailed: 0
BaselineRepoSkipped: 0

DiscoveredAssemblies:
- `<repo-root>\SVGControl.Test\bin\Debug\SVGControl.Test.dll`

## ACCEPTANCE LITERALLY MET, BASELINE NOT VALID — recorded but NOT checked off

P0-T14's three stated acceptance conjuncts are each literally satisfied: the decimals and integers
are recorded, `BaselineLinesValid:` is the positive integer `8965`, and no entry in
`DiscoveredAssemblies:` contains a path segment equal to `.claude` after the worktree-root prefix is
replaced by `<repo-root>` (the single entry's segments are `SVGControl.Test`, `bin`, `Debug`,
`SVGControl.Test.dll`).

The task is nevertheless **left unchecked**, under the plan's fail-closed evidence rule, because the
figures are not a repository-wide baseline and using them as one would corrupt Phase 11.

## Why the measurement is not repository-wide

The script reported `Discovered 1 test assemblies.` One test assembly exists on disk:
`SVGControl.Test/bin/Debug/SVGControl.Test.dll`. Every other test assembly — `QuickFiler.Test`,
`UtilitiesCS.Test`, `ToDoModel.Test`, `Tags.Test`, `TaskMaster.Test`, `TaskTree.Test`,
`TaskVisualization.Test`, `VBFunctions.Test` — is absent, because both Phase 0 msbuild gates use
`/t:Rebuild`, which emptied every `bin/Debug` directory, and the rebuild then failed at `CoreCompile`
through the inherited analyzer version skew recorded in the P0-T11 artifact. `SVGControl.Test` is the
only assembly that survived, being the one project with no dependency on `UtilitiesCS`.

Three independent signals confirm the run is not repository-wide:

1. `Total tests: 75`, all passed. A repository-wide run is an order of magnitude larger.
2. `BaselineRepoSkipped:` is `0`. The plan states this value is **expected to be non-zero**, because
   three files outside `QuickFiler.Test` carry a live `[Ignore(...)]` attribute —
   `UtilitiesCS.Test/YesNoToAll_Test.cs`, `UtilitiesCS.Test/ResourceTests.cs` and
   `UtilitiesCS.Test/InputBox_Test.cs`. None of those ran, so none was skipped. The observed `0` is
   direct evidence that `UtilitiesCS.Test` was not in the run.
3. The line rate is `13.296%` against a repository figure of roughly `85%`.

## Consequence for Phase 11 if these figures were treated as a baseline

P11-T8 clause (d) asserts **equality** against `BaselineRepoSkipped:`. Recording `0` here would
require a later, healthy, repository-wide run to also report `0` skipped, which it cannot: a run that
includes `UtilitiesCS.Test` reports three. The gate would be unsatisfiable in the wrong direction —
it would fail precisely when the repository is restored to health. Symmetrically, spec AC54 requires
post-change coverage to be not lower than this baseline; a `13.296%` floor would be trivially passed
by any real run and would gate nothing. Both are reasons to reject these numbers as a baseline rather
than to record them and move on.

## Which denominator each figure uses

Two different percentages appear in this run and they are not interchangeable.

- **`13.296%`** is `lines-covered="1192"` over `lines-valid="8965"` on the root `<coverage>` element of
  the on-disk `coverage/coverage.cobertura.xml`, which is the **post-processed, Koverage-compatible**
  document — per-file `<class>` elements pre-merged and test packages stripped. This is the figure
  P0-T14 instructs be read and it is what is recorded in the fields above.
- **`41.3161%`** is the figure in the script's own threshold message. `Assert-CoberturaLineCoverageThreshold`
  (`scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1:458-489`) reads the `line-rate` attribute of
  the root `/coverage` node of the XML string it is handed, which is the **raw dotnet-coverage
  Cobertura** before Koverage post-processing. The two documents have different denominators, so the
  two percentages differ by roughly a factor of three.

Both are recorded so that no later comparison silently mixes them. The document containing 268
`<class>` elements is the post-processed one.

## Which non-zero exit path produced EXIT_CODE 1

The plan names two paths, both compatible with a passing baseline capture. This run took the
**second**: `Assert-CoberturaLineCoverageThreshold` threw at
`scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1:489` with
`Cobertura line coverage 41.3161% is below the required 80% threshold.`

It was **not** the first path. `Test Run Successful.` was reported with `Passed: 75` of
`Total tests: 75`, so no test failed anywhere in the search root. `ExpectedExitCode:` is recorded as
`1` per the plan's rule for a non-zero exit.

## Worktree-nesting note

The plan records that a substring assertion that a discovered assembly path does not contain
`\.claude\` is unsatisfiable by construction, because this worktree root itself lies under
`.claude\worktrees\`. The satisfiable form the plan specifies was used: the worktree-root prefix is
stripped and replaced by `<repo-root>`, and no remaining path segment equals `.claude`. That holds for
the single discovered assembly.

Output Summary: The coverage run **did not measure the repository**. Only `1` test assembly was
discovered, `SVGControl.Test.dll`, because `/t:Rebuild` emptied every `bin/Debug` and the rebuild
failed on `UtilitiesCS` through the inherited analyzer version skew recorded under P0-T11.
`Total tests: 75`, `Passed: 75`, `Failed: 0`, `Skipped: 0`. Root `<coverage>` reports
`line-rate="0.13296151701059677"`, `branch-rate="0.10335641981885989"`, `lines-covered="1192"`,
`lines-valid="8965"` on the post-processed Koverage document; the script's own threshold message
reports `41.3161%` from the raw pre-post-processing document. `EXIT_CODE: 1` came from the sub-80%
threshold throw at `Invoke-MSTestWithCoverage.Helpers.ps1:489`, not from any failing test. All three
literal acceptance conjuncts are satisfied, but the figures are not a repository-wide baseline — a
recorded `BaselineRepoSkipped:` of `0` would make P11-T8 clause (d) unsatisfiable for a healthy run —
so the task is recorded and left unchecked.
