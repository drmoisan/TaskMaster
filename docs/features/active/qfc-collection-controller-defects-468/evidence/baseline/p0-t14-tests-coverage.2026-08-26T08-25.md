# [P0-T14] Test and coverage baseline

Timestamp: 2026-08-26T08-25

Command:

```
pwsh -NoProfile -File scripts\vscode\Invoke-MSTestWithCoverage.ps1 `
    -SearchRoot . -Configuration Debug `
    -CoverageOutput docs\features\active\qfc-collection-controller-defects-468\evidence\baseline\coverage-baseline.cobertura.xml
```

EXIT_CODE: 0

ExpectedExitCode: 0

Coverage artifact:
`docs/features/active/qfc-collection-controller-defects-468/evidence/baseline/coverage-baseline.cobertura.xml`
(10,602,264 bytes; not matched by any `.gitignore` rule, verified with `git check-ignore -v`).

## Output Summary

**Test Run Successful. Total tests: 6482, Passed: 6482, Failed: 0, Skipped: 0. Total time 52.8255
seconds. Root Cobertura `line-rate` = 84.7703%, `branch-rate` = 78.6876%.**

### Test counts

Aggregate, as reported by vstest:

```
Test Run Successful.
Total tests: 6482
     Passed: 6482
 Total time: 52.8255 Seconds
```

Independently confirmed by counting per-test result lines in the run log: 6482 lines matching
`^  Passed `, **0** matching `^  Failed `, **0** matching `^  Skipped `.

| Metric | Value |
|---|---|
| Total tests | **6482** |
| Passed | **6482** |
| Failed | **0** |
| Skipped | **0** |

### Per-assembly breakdown (nine assemblies, one vstest invocation)

The run log is cleanly segmented by `Test Parallelization enabled for <assembly>` markers, one per
assembly, so a per-assembly passed count is directly derivable.

| Assembly | Passed | Failed |
|---|---|---|
| **`QuickFiler.Test.dll`** | **937** | **0** |
| `SVGControl.Test.dll` | (segment 950-1022) | 0 |
| `Tags.Test.dll` | (segment 1024-1090) | 0 |
| `TaskMaster.Test.dll` | (segment 1092-1455) | 0 |
| `TaskTree.Test.dll` | (segment 1457-1509) | 0 |
| `TaskVisualization.Test.dll` | (segment 1511-1672) | 0 |
| `ToDoModel.Test.dll` | (segment 1674-1797) | 0 |
| `UtilitiesCS.Test.dll` | 4699 | 0 |
| `VBFunctions.Test.dll` | (segment 6499-end) | 0 |
| **Total** | **6482** | **0** |

#### PLAN DEFECT — P1-T8's passed-count comparison is unsatisfiable as written (resolved here)

P1-T8 states: "Run the full `QuickFiler.Test` suite with the **full-suite regression command** ...
Acceptance: `EXIT_CODE: 0`, a failed count of exactly `0`, and **a passed count not lower than the
P0-T14 baseline passed count**." P2-T11 and P3-T6 are worded without that clause and are unaffected.

The two figures are not commensurable:

- The plan's `### Full-suite regression command` (Conventions) runs **one** assembly:
  `QuickFiler.Test\bin\Debug\QuickFiler.Test.dll`.
- The plan's `### Full-suite coverage command`, which P0-T14 uses, discovers and runs **nine**
  assemblies.

A QuickFiler.Test-only passed count can never reach the nine-assembly aggregate of 6482, so the
condition as literally worded can never be satisfied by any correct execution.

**Resolution (recorded, not a plan edit):** the comparable baseline is the QuickFiler.Test-only
subset of this same run: **937 passed, 0 failed**. P1-T8, and every later QuickFiler.Test-only suite
gate, is measured against **937**, not 6482. Both figures are recorded here so a reviewer can audit
the substitution. The plan file is not modified.

### Coverage

Read from the root `<coverage>` element of the post-processed Cobertura XML, verbatim:

```xml
<coverage line-rate="0.847703" branch-rate="0.786876" complexity="25028" version="1.9"
          timestamp="1787747775" lines-covered="53763" lines-valid="63422"
          branches-covered="12675" branches-valid="16108">
```

| Metric | Raw | As a percentage |
|---|---|---|
| **`line-rate`** | 0.847703 | **84.7703%** |
| **`branch-rate`** | 0.786876 | **78.6876%** |
| `lines-covered` / `lines-valid` | 53763 / 63422 | |
| `branches-covered` / `branches-valid` | 12675 / 16108 | |

For reference, the `QuickFiler` package element reports `line-rate="0.768497035318381"` and
`branch-rate="0.7269046742730954"`.

These are the **baseline numbers** the final-QC coverage delta is computed against. Per the plan's
`### Coverage scope note`, no acceptance condition in this plan claims a coverage increase
attributable to this feature; the numbers are captured and the delta reported, and per-defect proof
is carried by named MSTest methods instead.

#### Measured confirmation of the coverage scope note

The plan asserts that `QfcCollectionController` carries `[ExcludeFromCodeCoverage]` at `<CTRL>:21`
and that no test added by this plan can move any coverage number for that file. This was verified
directly against the baseline artifact rather than assumed:

- `grep -c "QfcCollectionController"` over the Cobertura XML returns **9** hits.
- All nine are occurrences of the **interface** `QuickFiler.Interfaces.IQfcCollectionController`
  inside `signature="..."` attributes of other types' methods (`QfcItemController` constructors,
  `Initialize`, `SaveParameters`, and similar).
- There is **no** `<class ... name="QuickFiler.QfcCollectionController" ...>` element anywhere in
  the document.

The class is therefore genuinely absent from the coverage denominator, exactly as the plan states.

### Assembly-discovery assertion (restated form required by the plan's Conventions)

The executing workspace **is** an agent worktree rooted under `.claude\worktrees\`, so the usual "no
discovered assembly path contains `\.claude\`" assertion is unsatisfiable here. The plan's restated
form is asserted instead.

The runner reported `Discovered 9 test assemblies.` and `A total of 9 test files matched the
specified pattern.` The nine discovered paths, with the `<WS>` prefix elided, are:

```
<WS>\QuickFiler.Test\bin\Debug\QuickFiler.Test.dll
<WS>\SVGControl.Test\bin\Debug\SVGControl.Test.dll
<WS>\Tags.Test\bin\Debug\Tags.Test.dll
<WS>\TaskMaster.Test\bin\Debug\TaskMaster.Test.dll
<WS>\TaskTree.Test\bin\Debug\TaskTree.Test.dll
<WS>\TaskVisualization.Test\bin\Debug\TaskVisualization.Test.dll
<WS>\ToDoModel.Test\bin\Debug\ToDoModel.Test.dll
<WS>\UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll
<WS>\VBFunctions.Test\bin\Debug\VBFunctions.Test.dll
```

- **Every discovered `*.Test.dll` path begins with the `<WS>` prefix.** Confirmed: all nine.
- **No discovered path contains a `\.claude\worktrees\` segment after that prefix.** Confirmed: the
  remainder of each path after `<WS>` is `\<Project>.Test\bin\Debug\<Project>.Test.dll`. No stale
  sibling-worktree build was picked up.

### Tooling versions and artifact hygiene

- `vstest.console.exe` resolved through `vswhere` to Visual Studio **18** Community; `VSTest version
  18.8.0 (x64)`.
- `dotnet-coverage v18.5.2.0 [win-x64 - .NET 10.0.10]`.
- Inner runsettings: `scripts/vscode/TaskMaster.cli.runsettings` (MSTest parallelization only, no
  coverage data collector). Outer instrumentation excludes: `coverage.config`.
- The wrapper post-processed the Cobertura XML for Koverage compatibility, rewriting absolute paths
  to workspace-relative form and injecting `<sources><source>.</source></sources>`.
- **Host-path hygiene verified**: the committed coverage XML contains **0** occurrences of
  `C:\Users` and **0** occurrences of the operator's account name. No sanitisation was required.

### Plan-accuracy note — `-SearchRoot` argument form

The plan's `### Full-suite coverage command` writes `-SearchRoot <WS>`, i.e. an absolute path.
`Invoke-MSTestWithCoverage.ps1` computes its search root as `Join-Path $repoRoot $SearchRoot`, and
PowerShell's `Join-Path 'C:\a' 'C:\b'` yields `C:\a\C:\b` (verified directly). Passing an absolute
`<WS>` therefore produces a non-existent path and the script throws `Search root not found: ...`
before running anything.

`-SearchRoot .` was used instead. Because `$repoRoot` is resolved from the script's own location as
`<WS>\scripts\vscode\..\..` = `<WS>`, `Join-Path $repoRoot '.'` resolves to exactly `<WS>` — the
identical discovery scope the plan intends. This is a substitution of argument *form* only, with no
change to scope or semantics. Reported rather than silently worked around.

### Acceptance verification

- `Command:` recorded. `EXIT_CODE: 0` recorded.
- Total / passed / failed test counts recorded: 6482 / 6482 / 0 (plus the QuickFiler.Test-only
  subset, 937 / 0).
- Numeric `line-rate` and `branch-rate` read from the Cobertura root `<coverage>` element and
  expressed as percentages: **84.7703%** and **78.6876%**.
- The assembly-path statement is made in the plan's restated form and both halves are confirmed.

Result: PASS, with two non-blocking plan defects reported (P1-T8's incommensurable passed-count
comparison, resolved by recording the 937 subset baseline; and the `-SearchRoot <WS>` argument form).
