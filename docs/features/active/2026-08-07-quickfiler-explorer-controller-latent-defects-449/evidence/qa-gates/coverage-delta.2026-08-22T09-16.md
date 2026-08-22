# Coverage Delta and Threshold Report (Issue #449, [P7-T9], [P7-T10])

Timestamp: 2026-08-22T09-16
WORKTREE: `C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a5600546d71e73061`

Command: coverage figures read from the two Cobertura reports produced by the identical
`dotnet-coverage collect --settings coverage.config -- vstest.console.exe ... /InIsolation` invocation
recorded in [P0-T12] and [P7-T6]:
- baseline: `coverage\baseline-p0t12.cobertura.xml`
- post-change: `coverage\postchange-p7t6.cobertura.xml`

EXIT_CODE: 0

Source artifacts: `../baseline/step5-vstest-coverage.2026-08-22T09-16.md` and
`step5-vstest-coverage.2026-08-22T09-16.md`.

---

## 1. Repo-wide line rate

| | Value |
| --- | --- |
| **Baseline** | `0.8532899236682991` = **85.3290 %** (155,943 / 182,755) |
| **Post-change** | `0.8535709020220277` = **85.3571 %** (156,317 / 183,133) |
| **Delta** | **+0.0281 percentage points** (improved) |

## 2. `QuickFiler` package line rate

| | Value |
| --- | --- |
| **Baseline** | `0.8091631603553062` = **80.9163 %** |
| **Post-change** | `0.8098982423681776` = **80.9898 %** |
| **Delta** | **+0.0735 percentage points** (improved) |

## 3. `QfcExplorerController` line rate

| | Value |
| --- | --- |
| **Baseline** | **ABSENT FROM THE REPORT** — not 0 % |
| **Post-change** | **87.8261 %** (101 covered / 115 total lines) |

The baseline value is recorded as **absent**, per [P0-T13], and deliberately not as zero. The
class-level `[ExcludeFromCodeCoverage]` at merge-base line 20 suppressed every member — including the
compiler-generated `async` state machine and the lambda display classes — so the class contributed no
`<class>` element and no lines at all. A rate requires a denominator, and the denominator did not
exist. "0 %" would be a fabricated figure implying the class was measured and found uncovered, which
is a materially different claim.

The post-change figure aggregates **all four** `<class>` elements whose `filename` ends with
`QuickFiler\Controllers\QfcExplorerController.cs`:

```
QuickFiler.Controllers.QfcExplorerController
QuickFiler.Controllers.QfcExplorerController.<>c                     (lambda cache)
QuickFiler.Controllers.QfcExplorerController.<>c__DisplayClass24_0   (closure)
QuickFiler.Controllers.QfcExplorerController.<OpenQFItem>d__24       (async state machine)
```

Reading only the first would have reported a figure for a fragment of the file.

## 4. Changed-code coverage

The production lines this change added or modified, measured against the post-change report by
aggregating per-line hits across all four `<class>` elements:

| Line | Content | Hits | State |
| --- | --- | --- | --- |
| 63 | `(text, caption, buttons, icon) => MessageBox.Show(text, caption, buttons, icon);` (seam default initialiser) | 1 | **COVERED** |
| 139 | `_activeExplorer.CurrentFolder = (MAPIFolder)mailItem.Parent;` (the D2 fix) | 1 | **COVERED** |
| 167 | `DialogResult result = NotInViewDialogInvoker(` (the routed dialog call) | 1 | **COVERED** |

| | Value |
| --- | --- |
| Changed executable lines | **3** |
| Changed lines covered | **3** |
| **Changed-code coverage** | **100.0000 %** |

Lines 1 and 49-62 of the diff are a `using` directive, comment lines, and the multi-line property
type declaration. They emit no sequence point, so they appear in no `<class>`'s `<lines>` collection
and are correctly excluded from the changed-code denominator rather than counted as uncovered.

Line 63 registers a hit because an auto-property initialiser executes during construction; the lambda
BODY is never invoked by any test, since every test that reaches the not-in-view branch substitutes the
seam first. That is the intended design: the production dialog is never displayed under test.

**No coverage regression on changed lines.** All three changed executable lines are covered.

## 5. Comparison against the 80 % threshold — and why it is DELTA-BASED

The only machine-enforced numeric coverage gate in this repository is the repo-wide Cobertura root
`line-rate` compared against 80 % at
`scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1:487-489` (established in
`../baseline/environment-preconditions.2026-08-22T09-16.md`, finding (c); there is no per-file,
per-assembly, or branch-coverage gate anywhere under `scripts/`).

Post-change repo-wide figure **85.3571 %** is above 80 %.

**This comparison is recorded as DELTA-BASED, not as a direct pass of that gate.** The absolute
repo-wide figure produced here is **not directly comparable** to the helper's gated figure, because
this plan's direct `dotnet-coverage collect --settings coverage.config` invocation does **not** apply
the effective-config test-assembly `ModulePath` exclusion that `scripts/vscode/Invoke-MSTestWithCoverage.ps1`
derives at runtime. The nine `*.Test` packages are therefore inside this denominator — visible in the
per-package table, where `QuickFiler.Test` (96.39 %), `UtilitiesCS.Test` (97.82 %), `TaskTree.Test`
(100 %), and `VBFunctions.Test` (100 %) all contribute — and test assemblies typically carry higher
coverage than production code, which biases the absolute figure upward relative to the gated one.

What IS sound is the delta: the baseline and post-change runs use the **identical** collection method,
the identical settings file, and the identical assembly set, so the two figures are commensurable with
each other even though neither is the helper's gated number. On that basis the repo-wide rate
**improved by 0.0281 pp** and no regression occurred.

---

# [P7-T10] — Epic NFR outcome

**The NFR:** "Coverage of `QuickFiler.csproj` is retained or improved at every child merge."

| | `QuickFiler` package line rate |
| --- | --- |
| **Baseline (before)** | **80.9163 %** |
| **Post-change (after)** | **80.9898 %** |
| **Delta** | **+0.0735 percentage points** |

## VERDICT: the NFR is **MET**.

The post-change `QuickFiler` package figure of **80.9898 %** is **above** the baseline figure of
**80.9163 %**, so coverage of `QuickFiler.csproj` is not merely retained but **improved**.

There is **no shortfall to report**.

### Why the improvement occurred despite the class entering the denominator for the first time

This outcome was not guaranteed, and the plan explicitly provided for the opposite. Under D5 the
class-level `[ExcludeFromCodeCoverage]` was removed, which brings `QfcExplorerController` into the
`QuickFiler` coverage **denominator** for the first time. A previously invisible file contributing
uncovered lines would ordinarily depress the package figure, and [P7-T10] required that such a
shortfall be reported honestly with its numeric size rather than concealed.

The figure rose instead because two effects worked together:

1. **The 15 new tests cover the newly visible class well.** `QfcExplorerController` enters the
   denominator at **87.8261 %** (101 / 115), which is materially ABOVE the `QuickFiler` package's
   80.9163 % baseline. Adding a well-covered file to a less-covered package raises the package
   average.
2. **The dead-region deletion removed 139 lines of unreachable code** that could never have been
   covered by any test. Those lines were suppressed by the same attribute and so were not in the
   baseline denominator either, but their deletion means they can never enter it.

### No attribute was restored to manufacture a better number

**No blanket class-level `[ExcludeFromCodeCoverage]` is restored**, and no member-level
`[ExcludeFromCodeCoverage]` was added anywhere, at any point, for any reason. `ac9-attribute-removed.2026-08-22T09-16.md`
records the verifying search:
`git grep -n -F "ExcludeFromCodeCoverage" -- QuickFiler/Controllers/QfcExplorerController.cs`
returns **zero** matching lines. The improved figure is the result of genuine test coverage of
genuinely measured code, not of re-suppressing the measurement.

## Output Summary

Every required value is recorded as a number, with the single documented exception of the baseline
`QfcExplorerController` value, which is recorded as **absent from the report**. Repo-wide line rate
**85.3290 % -> 85.3571 %** (+0.0281 pp). `QuickFiler` package **80.9163 % -> 80.9898 %** (+0.0735 pp).
`QfcExplorerController` **absent -> 87.8261 %** (101/115, aggregated across four `<class>` elements).
Changed-code coverage **100 %** (3 of 3 changed executable lines covered). The post-change repo-wide
figure of 85.3571 % exceeds the 80 % threshold at
`scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1:487-489`, recorded explicitly as a **delta-based**
comparison because this plan's collection method leaves test assemblies in the denominator and so does
not reproduce that helper's gated figure. **The epic NFR is MET: coverage of `QuickFiler.csproj`
improved by 0.0735 percentage points**, with no shortfall and no restoration of any
`[ExcludeFromCodeCoverage]` attribute.
