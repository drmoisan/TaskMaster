# Phase 0 — Baseline Full-Suite Test and Coverage Run (issue #440, plan task P0-T13)

Timestamp: 2026-08-29T06-27

Command:

```
pwsh -NoProfile -File scripts\vscode\Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug -CoverageOutput coverage\baseline440.cobertura.xml
```

The coverage filename ends in `.cobertura.xml` per plan Global rule 8, so the P4-T2
formatting verification cannot pick it up.

EXIT_CODE: 0

## Output Summary

### (a) Run summary, verbatim

```
Test Run Successful.
Total tests: 6857
     Passed: 6857
```

There is no `Failed:` line, which is the fully green shape the plan describes. The
runner reported a readable total on the first execution, so no second execution was
required or performed.

### (b) Test counts

- `BaselineTotalTests`: **6857**
- `BaselinePassedTests`: **6857**
- `BaselineFailedTests`: **0**
- `BaselineFailureSet`: **none**

### (c) Repository-wide coverage, from the root `coverage` element

| Attribute | Verbatim value |
| --- | --- |
| `line-rate` | `0.852935` |
| `branch-rate` | `0.792523` |
| `lines-covered` | `54755` |
| `lines-valid` | `64196` |
| `branches-covered` | `13037` |
| `branches-valid` | `16450` |

- `BaselineLineCoveragePercent`: **85.2935**
- `BaselineBranchCoveragePercent`: **79.2523**

### (d) Per-file counters for `UtilitiesCS\OutlookObjects\Folder\BreadcrumbStateModel.cs`

Obtained by dot-sourcing `scripts\vscode\Invoke-MSTestWithCoverage.Helpers.ps1` and
calling `Get-CoberturaClassLineSummary` on the single `class` element of the produced
document whose `filename` attribute equals that path. Exactly one `class` element
matched (`MATCHING_CLASS_NODES=1`), so there is no ambiguity in the lookup. The class
`name` attribute is `UtilitiesCS.OutlookObjects.Folder.BreadcrumbStateModel`.

- `BaselineFileTotalLines`: **121**
- `BaselineFileCoveredLines`: **119**
- `BaselineFileTotalBranches`: **44**
- `BaselineFileCoveredBranches`: **41**

Derived uncovered counts, which are the quantities plan gates P4-T6 (2) and (3)
compare against:

- Baseline uncovered lines: 121 - 119 = **2**
- Baseline uncovered branches: 44 - 41 = **3**

### (d.1) Pre-change coverage of the `LeftArrow()` span, lines 220 to 246

Recorded here so the P4-T6 changed-region gate has a documented before-state. Every
`line` element in the span, keyed by line number with the class-level rollup taking
precedence over the method-level view:

```
line 221: hits=1
line 222: hits=1
line 223: hits=1  condition-coverage=100% (2/2)
line 224: hits=1
line 225: hits=1
line 231: hits=1
line 232: hits=1  condition-coverage=100% (8/8)   <- the #440 transition if
line 233: hits=1
line 234: hits=1
line 235: hits=1
line 236: hits=1
line 237: hits=1
line 238: hits=1
line 239: hits=1
line 241: hits=1  condition-coverage=100% (2/2)
line 242: hits=1
line 243: hits=1
line 244: hits=1
line 245: hits=1
line 246: hits=1
```

Span element count: 20. Every element has `hits` greater than 0.

`BaselineTransitionIfConditionCoverage` (line 232): **100% (8/8)**.

### (e) Discovery

- Discovered test assemblies: **9**.
- The nine discovered paths, expressed relative to the repository root, are
  `QuickFiler.Test\bin\Debug\QuickFiler.Test.dll`,
  `SVGControl.Test\bin\Debug\SVGControl.Test.dll`,
  `Tags.Test\bin\Debug\Tags.Test.dll`,
  `TaskMaster.Test\bin\Debug\TaskMaster.Test.dll`,
  `TaskTree.Test\bin\Debug\TaskTree.Test.dll`,
  `TaskVisualization.Test\bin\Debug\TaskVisualization.Test.dll`,
  `ToDoModel.Test\bin\Debug\ToDoModel.Test.dll`,
  `UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll` and
  `VBFunctions.Test\bin\Debug\VBFunctions.Test.dll`.
- **No** discovered path expressed relative to the repository root contains a
  `.claude` segment. No `CONTAMINATED-DISCOVERY:` record was produced and no rerun
  was required.

### (f) Coverage-threshold branch (Global rule 7)

The threshold branch was **not** taken. The wrapper printed
`Post-processing coverage XML for Koverage compatibility...` followed by
`Done. Coverage artifact: <repo-root>\coverage\baseline440.cobertura.xml`, which only
occurs when `Assert-CoberturaLineCoverageThreshold` did not throw and the write-back
executed. No `COVERAGE-THRESHOLD-THROW:` record was produced. The document read above
is therefore the normal post-processed output, with relative `class/@filename` values,
which is why the `UtilitiesCS\OutlookObjects\Folder\BreadcrumbStateModel.cs` lookup
resolved directly.

## Redaction note

The raw Cobertura document and the captured run log remain under the gitignored
`coverage/` tree and are not copied under this feature folder, per Global rule 8. No
absolute host path, account name, or host name appears in this artifact.
