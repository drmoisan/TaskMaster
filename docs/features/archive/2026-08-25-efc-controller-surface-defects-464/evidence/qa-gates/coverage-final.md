# [P10-T7] Post-change repository-wide coverage

Timestamp: 2026-08-28T02-05
Task: [P10-T7]
Command: `& .\scripts\vscode\Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug -CoverageOutput docs\features\active\efc-controller-surface-defects-464\evidence\qa-gates\coverage-final.cobertura.xml` under `pwsh -NoProfile` from the worktree root
EXIT_CODE: 0

Run start (UTC): `2026-08-28T02-00-45`
Run end (UTC): `2026-08-28T02-01-44`

This is the identical command `[P0-T14]` used for the baseline, so the two measurements are produced by
the same instrument.

## Cobertura file

The file was written to
`docs/features/active/efc-controller-surface-defects-464/evidence/qa-gates/coverage-final.cobertura.xml`
(10,750,414 bytes) and its root element was read before the file was deleted. Raw Cobertura XML is not
committed in this repository: it is machine-generated measurement data of order ten megabytes, and the
numbers below are the durable record. The deletion is recorded here so a later reader is not surprised by
the artifact's absence.

## Cobertura root `<coverage>` attributes, verbatim

```xml
<coverage line-rate="0.85252" branch-rate="0.791875" complexity="25349" version="1.9"
          timestamp="1787882488" lines-covered="54667" lines-valid="64124"
          branches-covered="13001" branches-valid="16418">
```

| Attribute | Value |
|---|---|
| `line-rate` | **0.85252** (85.25%) |
| `branch-rate` | **0.791875** (79.19%) |
| `lines-covered` | 54667 |
| `lines-valid` | 64124 |
| `branches-covered` | 13001 |
| `branches-valid` | 16418 |
| `complexity` | 25349 |

## Discovered test assemblies — count 9, verbatim list

The runner log records `Discovered 9 test assemblies.` and `A total of 9 test files matched the
specified pattern.` The nine, with the worktree root rendered as `<repo-root>`:

```
<repo-root>\QuickFiler.Test\bin\Debug\QuickFiler.Test.dll
<repo-root>\SVGControl.Test\bin\Debug\SVGControl.Test.dll
<repo-root>\Tags.Test\bin\Debug\Tags.Test.dll
<repo-root>\TaskMaster.Test\bin\Debug\TaskMaster.Test.dll
<repo-root>\TaskTree.Test\bin\Debug\TaskTree.Test.dll
<repo-root>\TaskVisualization.Test\bin\Debug\TaskVisualization.Test.dll
<repo-root>\ToDoModel.Test\bin\Debug\ToDoModel.Test.dll
<repo-root>\UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll
<repo-root>\VBFunctions.Test\bin\Debug\VBFunctions.Test.dll
```

This is the same set of nine that `[P0-T14]` discovered. No entry resolves outside this worktree; a
search of the log for a `bin\Debug\*.dll` path not under this worktree returns nothing, so no sibling
checkout was pulled in.

## Test tally

```
Test Run Successful.
Total tests: 6789
     Passed: 6789
 Total time: 41.0730 Seconds
```

Zero failures across all nine assemblies. The baseline run of the same command reported 6719 executed
with **15 failed**, all fifteen being the load-driven `QfcItemController.*` WinFormsPumpHost and
dispatcher-fixture flakiness described in `baseline/coverage.md`. None of the fifteen failed here.

Test-count delta: 6789 − 6719 = **+70**, which reconciles with `[P10-T6]`: 26 results brought in by the
mandated integration merge plus the 44 results this feature adds. No other assembly's count changed.

## Coverage instrument

`dotnet-coverage v18.5.2.0 [win-x64 - .NET 10.0.11]`, VSTest 18.9.0 (x64), followed by the script's
Koverage-compatibility post-processing step, which merges per-file `<class>` elements and strips test
packages. The delivered file contains **9 `<package>` elements** — `QuickFiler`, `UtilitiesCS`,
`TaskVisualization`, `SVGControl`, `ToDoModel`, `Tags`, `TaskMaster`, `TaskTree`, `VBFunctions` — with no
duplicate package name and no test package. This is a deduplicated first-party denominator.

Output Summary: EXIT_CODE 0. Post-change repository-wide coverage is `line-rate` **0.85252** (85.25%,
54667 of 64124 lines) and `branch-rate` **0.791875** (79.19%, 13001 of 16418 branches), measured over the
same **9** discovered test assemblies as the Phase 0 baseline, all inside this worktree. The run executed
6789 tests with 6789 passed and **0 failed**; the 15 load-driven failures seen in the baseline run did
not recur. The Cobertura file was read and then deleted rather than committed. `[P10-T8]` records the
comparison against the baseline, including a caveat about the two runs' materially different
denominators.
