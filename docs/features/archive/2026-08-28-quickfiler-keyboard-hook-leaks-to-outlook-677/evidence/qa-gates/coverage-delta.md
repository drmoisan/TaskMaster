# Final QA Gate 6 — Coverage Delta and Threshold Verification (P5-T6)

Timestamp: 2026-08-28T16-10
EXIT_CODE: 0

## Inputs

- Baseline Cobertura: `evidence/baseline/coverage-baseline.cobertura.xml` (P0-T9)
- Final Cobertura: `evidence/qa-gates/coverage-final.cobertura.xml` (P5-T5)
- Changed-line source: `git diff 361a49b884a4e3fe192bf04bae05151c598398fa --unified=0 -- <file>`
  (BASELINE_SHA from P0-T2)

### Untracked-file precondition (performed first)

```
git add -N -- QuickFiler/Controllers/QfcFormController.Deactivate.cs
```

`git diff <commit>` never reports an untracked path. Without this intent-to-add the new production
file's changed-line set would have been empty and its >= 90% gate would have passed vacuously
whatever its coverage was. After the intent-to-add, `git diff --stat` against BASELINE_SHA reports
`60 insertions(+)` for that file, confirming it is now visible to the diff.

### Method

Class entries in each Cobertura report are aggregated **by file name**: for each file, a line's hit
count is the maximum across every `<class>` entry carrying that `filename`. This is required
because dotnet-coverage emits one `<class>` per type/nested type (including async state machines),
so several entries can carry the same `filename` and a naive sum double-counts. A changed line is
counted as executable only if it appears as a `<line>` record in the final report; a changed line
that is a comment, brace-only, or declaration-only produces no record and is excluded from both
numerator and denominator.

## (a) Baseline repo-wide line-rate

**0.852721** (85.2721%)

## (b) Final repo-wide line-rate

**0.852804** (85.2804%)

Branch-rate for reference: baseline 0.792255, final 0.792300.

## (c) Per-file changed/new-line coverage

| File | Raw diff lines | Changed executable lines | Floor | Covered | Uncovered | Changed-line coverage |
|---|---|---|---|---|---|---|
| `QuickFiler/Viewers/BreadcrumbDropDownHost.cs` | 37 | **11** | 3 | 11 | 0 | **100.00%** |
| `QuickFiler/Viewers/BreadcrumbDropDownHost.Open.cs` | 4 | **1** | 1 | 1 | 0 | **100.00%** |
| `QuickFiler/Controllers/QfcFormController.Deactivate.cs` | 60 | **24** | 10 | 24 | 0 | **100.00%** |
| `QuickFiler/Controllers/QfcItemController.FolderHandling.cs` | 4 | **1** | 1 | 1 | 0 | **100.00%** |
| `QuickFiler/Controllers/QfcFormController.SetupDisposal.cs` | 2 | **2** | 2 | 2 | 0 | **100.00%** |

Uncovered changed lines, all five files: **none**.

## Acceptance conditions

### (0) Non-vacuity — PASS

Every one of the five named files has a **non-empty** changed-line set, and every file meets its
own stated executable-line floor:

| File | Changed executable lines | Required floor | Meets floor |
|---|---|---|---|
| `QuickFiler/Controllers/QfcFormController.Deactivate.cs` | 24 | >= 10 | yes |
| `QuickFiler/Viewers/BreadcrumbDropDownHost.cs` | 11 | >= 3 | yes |
| `QuickFiler/Controllers/QfcFormController.SetupDisposal.cs` | 2 | >= 2 | yes |
| `QuickFiler/Viewers/BreadcrumbDropDownHost.Open.cs` | 1 | >= 1 | yes |
| `QuickFiler/Controllers/QfcItemController.FolderHandling.cs` | 1 | >= 1 | yes |

The two single-line files match the plan's own predictions exactly: P2-T2 changes exactly one
executable line (`Schedule(FocusPending)`) and P3-T6 adds exactly one expression-bodied line. The
`SetupDisposal.cs` count of 2 is exactly the P3-T9 `+=` and `-=` subscription lines.

### (1) Changed/new-line coverage >= 90% per file — PASS

All five files: 100.00%.

### (2) No regression on changed lines — PASS

Zero changed lines that were covered at baseline are uncovered at final, across all five files.
(Trivially satisfied here, since no changed line is uncovered at final at all.)

### (3) Repo-wide line-rate >= baseline − 0.0010 — PASS

| Quantity | Value |
|---|---|
| Baseline repo-wide line-rate | 0.852721 |
| Gate minimum (baseline − 0.0010) | 0.851721 |
| Final repo-wide line-rate | 0.852804 |
| Delta (final − baseline) | **+0.000083** |

The repo-wide rate did not merely stay within the instrumentation-noise tolerance; it **increased**.

## CLAUDE.md floor (reported figure, ratified-exemption denominator)

Repository-wide line coverage is **85.2804%** against the ratified COM/VSTO/WinForms exemption
denominator, comfortably above the CLAUDE.md `>= 80%` floor and above the
`.claude/rules/quality-tiers.md` `>= 85%` uniform line threshold. Branch coverage is 79.23%, above
the 75% uniform branch threshold.

Exempt-by-existing-attribute files touched by this change (recorded, not gated, per decision D9):
`ItemViewer.*` partials and `QfcFormViewer.cs` carry pre-existing type-level
`[ExcludeFromCodeCoverage]` attributes under the ratified COM/VSTO/WinForms exemption in CLAUDE.md.
No new exclusion attribute was introduced by this plan. The interface files
(`IQfcFormViewer.cs`, `IItemViewer.cs`, `IQfcItemController.cs`) contain no executable lines.

**Verdict: PASS on all four acceptance conditions. Not REMEDIATION-REQUIRED.**
