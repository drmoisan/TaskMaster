# P6-T5 — Coverage Delta and Per-Member Thresholds (spec AC-7)

Timestamp: 2026-08-28T16-35

Method: PowerShell XML read of `coverage\coverage-baseline-680.cobertura.xml` (P0-T10) and
`coverage\coverage-final-680.cobertura.xml` (P6-T4). `<class>` entries are aggregated by their
`filename` attribute, because async and lambda compiler-generated classes are emitted as separate
`<class>` nodes that share the containing file's `filename`; filename aggregation is therefore the
correct denominator. Where the same line number appears under more than one `<class>` for a file,
the maximum `hits` value is taken, so a line covered through a compiler-generated class is not
double-counted as uncovered. Every `filename` attribute is normalized to repo-relative form with
`/` separators before joining the two documents.

## (a) Repo-wide line-rate, baseline versus final

| Figure | Baseline (P0-T10) | Final (P6-T4) | Delta |
|---|---|---|---|
| Root `line-rate` | **0.85269** | **0.85279** | **+0.00010** |
| Root `branch-rate` | 0.792133 | 0.792235 | +0.000102 |
| `lines-covered` | 54683 | 54715 | +32 |
| `lines-valid` | 64130 | 64160 | +30 |

The repo-wide figure is a raw, whole-solution number. It is assessed against the CLAUDE.md § UT2
**testable denominator** — production-only first-party code after the ratified COM/VSTO/WinForms
exemptions (VSTO lifecycle classes, WinForms form-derived and Designer-generated code, and the
Outlook-Interop event-handler classes without an injectable seam). The raw figure is above the 80%
floor and moved **upward** with this change, so no pre-existing shortfall clause is invoked and this
change demonstrably does not lower coverage.

## (b) Filename-aggregated covered-line COUNT, per changed production file

Counter-based no-regression: each final covered-line count must be `>=` its baseline covered-line
count. A counter comparison cannot be failed by deletion arithmetic, and this plan deletes at most
one line per file.

| File | Baseline covered | Baseline total | Final covered | Final total | `final >= baseline` |
|---|---|---|---|---|---|
| `QuickFiler/Viewers/BreadcrumbDropDownHost.cs` | 279 | 281 | **287** | 289 | **PASS** |
| `QuickFiler/Viewers/BreadcrumbDropDownHost.Open.cs` | 14 | 14 | **18** | 18 | **PASS** |
| `QuickFiler/Viewers/BreadcrumbDropDownOpenLifetime.cs` | 317 | 320 | **317** | 320 | **PASS** |
| `QuickFiler/Controllers/QfcItemController.EventHandlers.cs` | 73 | 92 | **89** | 108 | **PASS** |
| `QuickFiler/Controllers/QfcItemController.EventWiring.cs` | 319 | 375 | **321** | 377 | **PASS** |

Found-count: **5/5** — every file in (b) is present in both documents.

Excluded from (b), with the stated reason:

- `QuickFiler/Viewers/ItemViewer.FolderSearch.cs` — the `ItemViewer` type is `[ExcludeFromCodeCoverage]`
  via its primary partial (`ItemViewer.cs`), so this partial contributes no coverage obligation. The
  two members added there are thin forwarding lines.
- `QuickFiler/Viewers/IItemViewer.cs` — an interface file with no executable lines.

## (c) Per-member new/changed-code coverage, floor `>= 0.90`

Each member's line span was derived by reading the post-format source file directly at P6 time; no
earlier task recorded spans. Coverage is computed as covered/total over that span within the
filename-aggregated line data of the final document.

| Member | File | Span | Covered / total | Rate | `>= 0.90` |
|---|---|---|---|---|---|
| `BreadcrumbDropDownHost.ShowPopup` | `QuickFiler/Viewers/BreadcrumbDropDownHost.cs` | 276-280 | 4 / 4 | **1.0000** | **PASS** |
| `BreadcrumbDropDownHost.FinishClose` | `QuickFiler/Viewers/BreadcrumbDropDownHost.cs` | 421-436 | 15 / 15 | **1.0000** | **PASS** |
| `BreadcrumbDropDownHost.OpenWithFocusIntentAsync` (including its scheduled lambda) | `QuickFiler/Viewers/BreadcrumbDropDownHost.Open.cs` | 52-81 | 16 / 16 | **1.0000** | **PASS** |
| `BreadcrumbDropDownOpenLifetime.ShowCurrentSurface` | `QuickFiler/Viewers/BreadcrumbDropDownOpenLifetime.cs` | 258-279 | 17 / 17 | **1.0000** | **PASS** |
| `QfcItemController.TextBoxSearch_KeyDown` | `QuickFiler/Controllers/QfcItemController.EventHandlers.cs` | 190-210 | 16 / 16 | **1.0000** | **PASS** |
| `QfcItemController.TextBoxSearch_Leave` | `QuickFiler/Controllers/QfcItemController.EventHandlers.cs` | 217-228 | 9 / 9 | **1.0000** | **PASS** |

Uncovered lines within any of the six spans: **none**.

Found-count: **6/6** — every member in (c) resolves to measured lines in both documents.

Every branch of every one of these members has a named Phase 2 test, which is why the `0.90` floor is
satisfiable by this plan's own tests and would fail if the fix code or its tests were absent.

## Verdict

All (b) comparisons pass, all (c) comparisons pass, found-counts are `5/5` and `6/6`, and every
numeric value above is present. No row is missing. **No REMEDIATION-REQUIRED.**
