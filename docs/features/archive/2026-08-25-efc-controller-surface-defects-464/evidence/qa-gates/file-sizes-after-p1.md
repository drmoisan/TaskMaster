# Phase 1 — file sizes after the EfcItemController deletions

Timestamp: 2026-08-28T00-15
Task: [P1-T14]
Command: `wc -l` over `QuickFiler/Controllers/EfcItemController.cs` and the three test files created by `[P1-T1]`
EXIT_CODE: 0

## `QuickFiler/Controllers/EfcItemController.cs`

| Measurement | Lines |
|---|---|
| `[P0-T15]` baseline at `BASELINE_SHA` | 1170 |
| Delivered after Phase 1 | **1054** |
| Net change | **−116** |

**1054 is strictly fewer than the 1170-line baseline**, which is the only size gate this plan applies to
this file.

**No task in this plan asserts a count under 500 for `EfcItemController.cs`.** The file remains a
pre-existing 500-line-ceiling violation; splitting it is explicitly out of scope per `spec.md`
§`Out of scope / non-goals`, and no acceptance criterion asserts otherwise.

Itemised net delta:

| Change | Lines |
|---|---|
| `[P1-T6]` delete `ToggleExpansion()` and its trailing blank | −12 |
| `[P1-T6]` delete `ToggleExpansion(Enums.ToggleState)` and its trailing blank | −45 |
| `[P1-T7]` delete `RegisterActions` and its trailing blank | −14 |
| `[P1-T8]` delete `InitializeWebView()` and its trailing blank | −33 |
| `[P1-T9]` delete the seven-parameter constructor and its trailing blank | −15 |
| `[P1-T10]` delete the `_selectorsCtrls` field | −1 |
| `[P1-T10]` replace two `_selectorsCtrls` arguments with `null` plus a two-line explanatory comment at each site | +4 |
| **Total** | **−116** |

## Test files created by this feature

| File | Lines | Ceiling | Within? |
|---|---|---|---|
| `QuickFiler.Test/Controllers/EfcItemControllerTests.cs` | 285 | 500 | yes |
| `QuickFiler.Test/Controllers/EfcItemController.CleanupTests.cs` | 18 | 500 | yes |
| `QuickFiler.Test/Controllers/EfcViewerTests.cs` | 22 | 500 | yes |

Each is at most 500 lines. The two files still at their `[P1-T1]` size carry their `[TestClass]`
declaration and file-header documentation only; their test methods are added in later phases.

All four files above are CSharpier-formatted; the counts are post-format and will not shift under
`[P10-T1]`'s pass unless later phases change them.

Output Summary: `EfcItemController.cs` is 1054 lines, 116 fewer than its 1170-line baseline and therefore
strictly below it. The three created test files are 285, 18 and 22 lines, all at most 500.
