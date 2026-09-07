# P8-T7 — Coverage Delta

Timestamp: 2026-08-26T11-28

Command: `pwsh -NoProfile -Command 'git diff 61edc19befcf6c4e95b5acd32542f2dcdab41b78 HEAD --unified=0 -- <each owned production file>'` for the changed-line sets, intersected against the per-line `hits` attributes of `docs/features/active/breadcrumb-router-navigation-defects-498/evidence/qa-gates/p8-t5-coverage.cobertura.xml` and `docs/features/active/breadcrumb-router-navigation-defects-498/evidence/baseline/p0-t15-coverage.cobertura.xml`, aggregating every Cobertura `<class>` element that shares the same `filename` value.

EXIT_CODE: 0

## Output Summary

**PASS.** Repository-wide line coverage rose from **84.78%** to **84.83%**. Changed-line coverage over
newly authored code is **94.90%**. Every per-file changed-line figure is at or above the 90.00 percent
floor, is a `NOT APPLICABLE` row, or is the single stated relocation exception — which shows exactly
zero regression.

### The three headline numeric values

| Value | Percent |
|---|---:|
| Baseline repository-wide line coverage (`P0-T15`, root `line-rate="0.847813"`) | **84.78%** |
| Post-change repository-wide line coverage (`P8-T5`, root `line-rate="0.84831"`) | **84.83%** |
| Changed-line coverage (owned production changed and newly authored lines) | **94.90%** |

Post-change (84.83%) is at or above the baseline (84.78%), so the no-regression condition is met with a
**+0.05 pp** improvement. Supporting counts: `lines-covered` 53770 → 53933 (+163), `lines-valid` 63422 →
63577 (+155); branch rate 78.70% → 78.79%.

Changed-line coverage is stated two ways for transparency. The headline 94.90% is **372 covered of 392
measurable** changed or newly authored lines across the owned production files, EXCLUDING the
relocation-exempt `BreadcrumbBridgeRouter.Selection.cs`. Including that file the figure is **95.60%**
(500 of 523). Both are above the 90.00 percent floor.

### Per-file changed-line figures

"Measurable" is the subset of changed lines that Cobertura reports as executable; non-executable lines
(braces the compiler does not sequence-point, comments, blank lines, declarations) carry no hit record
and are excluded from both numerator and denominator.

| File | Changed lines | Measurable | Covered | Changed-line coverage | Gate |
|---|---:|---:|---:|---:|---|
| `QuickFiler/Controllers/BreadcrumbBridgeRouter.cs` | 34 | 20 | 20 | **100.00%** | >= 90.00 — PASS |
| `QuickFiler/Controllers/BreadcrumbBridgeRouter.Selection.cs` | 204 | 131 | 128 | **97.71%** | REPORTED FOR THE RECORD; non-regression — PASS (see below) |
| `UtilitiesCS/OutlookObjects/Folder/OutlookFolderHierarchyProvider.cs` | 44 | 25 | 25 | **100.00%** | >= 90.00 — PASS |
| `UtilitiesCS/OutlookObjects/Folder/BreadcrumbRow.cs` | 0 | 0 | 0 | **NOT APPLICABLE** | not gated (see below) |
| `UtilitiesCS/OutlookObjects/Folder/BreadcrumbStateModel.cs` | 49 | 27 | 25 | **92.59%** | >= 90.00 — PASS |
| `UtilitiesCS/OutlookObjects/Folder/FolderBreadcrumbBridgeRouter.cs` | 8 | 5 | 5 | **100.00%** | >= 90.00 — PASS |

Two further owned production files carry newly authored code and are therefore included in the
changed-line set, though the task's reporting list does not name them:

| File | Changed lines | Measurable | Covered | Changed-line coverage | Gate |
|---|---:|---:|---:|---:|---|
| `QuickFiler/Controllers/BreadcrumbBridgeRouter.Arrows.cs` (new) | 211 | 133 | 128 | **96.24%** | >= 90.00 — PASS |
| `UtilitiesCS/OutlookObjects/Folder/BreadcrumbStateModel.Row.cs` (new) | 384 | 182 | 169 | **92.86%** | >= 90.00 — PASS |

### `UtilitiesCS/OutlookObjects/Folder/BreadcrumbRow.cs` — NOT APPLICABLE

Its changed-line set is EMPTY: `git diff` against the `P0-T10` baseline commit lists no hunk for this
file, because `P6-T3` explicitly forbids adding to it and no task as written does. A changed-line figure
over a zero denominator is undefined, so this row is reported as `NOT APPLICABLE` and NEVER as a
percentage. No conditional branch wrote the file, so the alternative reporting path in the task text
does not apply. This is the expected instance the task text names.

### `QuickFiler/Controllers/BreadcrumbBridgeRouter.Selection.cs` — the one stated exception

This file is gated only on NOT REGRESSING against the coverage of the corresponding relocated lines in
the baseline, because `P1-T2` created it as a byte-for-byte relocation of twelve pre-existing private
members and `P1-T3` proved the relocation behavior-neutral.

The relocated block occupied `QuickFiler/Controllers/BreadcrumbBridgeRouter.cs:410-594` at the
`P0-T10` baseline commit. Its baseline coverage, read from
`evidence/baseline/p0-t15-coverage.cobertura.xml`, is **128 of 131 measurable lines = 97.71%**.
Post-relocation, `BreadcrumbBridgeRouter.Selection.cs` measures **128 of 131 = 97.71%**.

**Regression: zero. The figures are identical**, which is the arithmetic signature of a pure relocation.

`git diff --numstat 8c255ac1 HEAD -- QuickFiler/Controllers/BreadcrumbBridgeRouter.Selection.cs`
(where `8c255ac1` is the Phase 0-1 commit that created the file) returns **no output**, confirming the
file received NO newly authored lines under any later task. The clause "where the file also receives
newly authored lines under a later task, those lines are subject to the 90.00 percent floor in the
normal way" therefore has no application here. The 97.71% figure is stated for the record only.

### Remediation performed under this task, and the re-run it forced

The FIRST computation of this task, against the pass-1 coverage artifact, measured
`UtilitiesCS/OutlookObjects/Folder/BreadcrumbStateModel.Row.cs` at **89.56%** (163 of 182), below the
90.00 percent floor. The task text directs: "Otherwise the executor adds tests and re-runs `P8-T5` and
this task." That path was taken.

Diagnosis of the 19 uncovered lines in that file, before any test was written:

- **13 of the 19 were relocated pre-existing lines that were ALSO uncovered at the `P0-T15` baseline**,
  in the identical code, in `BreadcrumbStateModel.cs` before the Phase 6 split: the two
  constructor-chaining overloads (baseline `:23` and `:56-62`, now `:25` and `:101-107`) and the
  `RequireIdentity` argument-exception throw (baseline `:255-259`, now `:375-379`). This feature neither
  caused nor worsened those.
- **6 were newly authored and genuinely uncovered** — the negative guard branches of three methods added
  by this feature:
  - `WithFilingTarget` (decision D7) early return when the chain is null or empty (`:83`, `:85`)
  - `GetActiveChild` (#440) invalid-request `return null` (`:220`, `:221`)
  - `TryExpandActiveSegment` (#440) no-affordance `return false` (`:234`, `:235`)

Three deterministic MSTest tests were appended to the OWNED test file
`UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbStateModelTests.cs` (395 to 454 lines, still under the
500-line limit) to exercise exactly those three branches:
`SuggestionRowWithFilingTarget_EmptyChain_DefersValidationToChainedConstructor`,
`GetActiveChild_WithoutAnOpenExpansion_ReturnsNull`, and
`TryExpandActiveSegment_WithoutAnAffordance_ReturnsFalseAndLeavesTheRowClosed`. They use Moq-free
in-memory state, FluentAssertions, no timers, no filesystem and no Outlook.

Editing a source file restarted the toolchain loop from `P8-T1`; the full pass history is in
`p8-t6-clean-pass.md`. After the re-run, `BreadcrumbStateModel.Row.cs` measures **92.86%** (169 of 182),
above the floor. The remaining 13 uncovered lines are the inherited ones listed above.

### Per-class corroboration (baseline vs post-change)

Read by Cobertura `<class>` name across all files contributing to each type:

| Class | Baseline | Post-change | Change |
|---|---:|---:|---:|
| `UtilitiesCS.OutlookObjects.Folder.BreadcrumbStateRow` | 216/229 = 94.32% | 169/182 = 92.86% | see note |
| `UtilitiesCS.OutlookObjects.Folder.BreadcrumbStateModel` | not reported separately at baseline | 119/121 = 98.35% | new row |
| `QuickFiler.Controllers.BreadcrumbBridgeRouter` | 368/376 = 97.87% | 423/431 = 98.14% | +0.27 pp |
| `UtilitiesCS.OutlookObjects.Folder.OutlookFolderHierarchyProvider` | 39/41 = 95.12% | 63/65 = 96.92% | +1.80 pp |
| `UtilitiesCS.OutlookObjects.Folder.FolderBreadcrumbBridgeRouter` | 332/337 = 98.52% | 335/339 = 98.82% | +0.30 pp |

Note on `BreadcrumbStateRow`: the two rows are not directly comparable, because at baseline the
Cobertura output attributed the whole 457-line `BreadcrumbStateModel.cs` to a single class row of 229
measurable lines, whereas post-change the Phase 6 split produced two separately reported rows whose
combined measurable count is 303. Taken together, `BreadcrumbStateRow` + `BreadcrumbStateModel` cover
288 of 303 = **95.05%**, above the 94.32% baseline row. The uncovered-line diagnosis above is the
denominator-independent statement and is the one to rely on: this feature contributed **zero** net
uncovered lines to the type after remediation.

### Diff basis, and the `git add -N` step

The task specifies `git diff HEAD --unified=0` on the premise that no task in this plan creates a commit,
so `HEAD` would still be the `P0-T10` baseline commit. In this execution the epic orchestrator committed
each phase as it completed, so `HEAD` advanced from the `P0-T10` baseline
`61edc19befcf6c4e95b5acd32542f2dcdab41b78`. The diff was therefore taken as
`git diff 61edc19befcf6c4e95b5acd32542f2dcdab41b78 HEAD --unified=0`, which is the same line set the
task intends — the task itself names the `P0-T10` baseline commit as the basis.

**`git add -N` was NOT needed and was NOT run.** Its stated purpose is to make untracked new partial
siblings visible to `git diff`; because every new sibling is already committed and therefore tracked, a
commit-to-commit diff enumerates their lines in full without it. No intent-to-add index entry was
created, and `git status --porcelain` is unaffected.

Added and modified lines were taken from the `+` side of each `@@ -a,b +c,d @@` hunk header. New files
appear as one hunk covering the whole file, so all 211 lines of `BreadcrumbBridgeRouter.Arrows.cs`, 204
of `BreadcrumbBridgeRouter.Selection.cs` and 384 of `BreadcrumbStateModel.Row.cs` enter the changed-line
set, which is what the task requires for a new partial sibling containing newly authored code.

### Cobertura aggregation

Every `<class>` element sharing the same `filename` value was aggregated into one line-number-to-hits
map, taking the maximum hit count per line, so that compiler-generated async state machines and lambda
closure classes are counted in the same denominator as their source file. In this artifact the
post-processing performed by `scripts/vscode/Invoke-MSTestWithCoverage.ps1` had already merged them:
each of the eight target filenames resolves to exactly one `<class>` element. The aggregation is
implemented regardless, so the figures do not depend on that post-processing behavior. Cobertura
`filename` values use backslash separators and were normalized to forward slashes before matching.

### Inherited condition recorded

Pull request #605 changed the coverage denominator by removing an `[ExcludeFromCodeCoverage]` attribute
from the unowned `QuickFiler/Controllers/EfcFormController.cs` (1084 lines). Both figures compared above
were MEASURED in this execution worktree after that change landed, so the comparison is like for like.
No figure is inherited from version 1.0 of this plan. Remedying the denominator change is not this
feature's obligation.

### Verdict

- Post-change repository-wide line coverage 84.83% >= baseline 84.78% — **PASS**
- Every per-file changed-line figure other than the `NOT APPLICABLE` row and the stated relocation
  exception is at or above 90.00 percent (100.00, 100.00, 92.59, 100.00, 96.24, 92.86) — **PASS**
- Relocation exception `BreadcrumbBridgeRouter.Selection.cs`: 97.71% post-change against 97.71%
  baseline, zero regression — **PASS**
