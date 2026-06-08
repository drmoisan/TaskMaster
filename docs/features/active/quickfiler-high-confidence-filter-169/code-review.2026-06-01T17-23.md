# Code Review — quickfiler-high-confidence-filter (Issue #169)

- Generated: 2026-06-01T17-23 (UTC)
- Base branch: `development` @ `3322bbee6a941eaa05e8388dd78ec3998e542d75`
- Head: `32de29d7748492eb0ec62219f2fe20b3d279142e`
- Scope: full branch diff vs. base

## Executive Summary

The implementation adds a high-confidence filtering mode to QuickFiler through small, well-isolated
changes: a `FolderScorer.TopScore()` accessor, two persisted user settings with interface/impl
plumbing, a post-scoring removal pass (`RemoveBelowThresholdAsync`) with a conditional caller
(`ApplyHighConfidenceFilterAsync`), and a second ribbon entry point with a validated threshold edit
box. The design follows the repository's seam-based DI guidance (injectable `Func<string, Task>`
removal delegate, narrow internal methods for testability), uses MSTest + Moq + FluentAssertions
throughout, and passes CSharpier, analyzer, and nullable builds independently.

One blocking behavioral defect was found: high-confidence mode is a **persisted** user setting that
`LoadQuickFilerHighConfidenceAsync` sets to `true` and never resets. The standard QuickFiler entry
point does not clear it, so after a single high-confidence launch the persisted flag remains `true`
across sessions and causes the standard "QuickFiler" entry point to silently apply the
high-confidence filter. This contradicts AC6 and the spec's documented alternate flow ("the standard
'QuickFiler' entry point is used; `RemoveBelowThresholdAsync` is not called"). The behavior is not
caught by the unit tests because they exercise `ApplyHighConfidenceFilterAsync` with directly-mocked
settings rather than the entry-point/persistence interaction.

A second blocking-class issue is coverage: the canonical `artifacts/csharp/coverage.xml` is absent
(mandatory for the changed language), and the feature's only behaviorally distinct entry-point method
(`LoadQuickFilerHighConfidenceAsync`, which carries the defect) is at 0% coverage.

The remaining findings are minor/non-blocking (input validation precision, file-size pre-existing
condition, and a test-suite flakiness note).

## Findings Table

| Severity | File | Location | Finding | Recommendation | Rationale | Evidence |
|---|---|---|---|---|---|---|
| Blocker | `TaskMaster/Ribbon/RibbonController.cs` | `LoadQuickFilerHighConfidenceAsync` (lines 127–140) vs. `LoadQuickFilerAsync` (107–119) | High-confidence mode is persisted (`HighConfidenceModeEnabled` setter calls `Settings.Default.Save()`) and set to `true` on the high-confidence launch but never reset. The standard `LoadQuickFilerAsync` does not set it to `false`, so once the high-confidence entry point is used, the persisted flag stays `true` across sessions and the standard entry point applies the filter via `ApplyHighConfidenceFilterAsync`. | Make the mode launch-scoped rather than persisted: set `HighConfidenceModeEnabled = false` at the start of `LoadQuickFilerAsync` (and/or after the high-confidence session loads), or carry the mode as a per-launch flag passed into `QfcHomeController.LaunchAsync` instead of a persisted setting. Add a unit test that asserts a standard launch following a high-confidence launch does not invoke `RemoveBelowThresholdAsync`. | Violates AC6 ("With high-confidence mode disabled, QuickFiler behaves exactly as today") and the spec alternate flow that says the standard entry point does not call the removal pass. The default-false claim only holds until the first high-confidence use. | `RibbonController.cs:132` sets the persisted flag; grep shows no code path sets it back to `false`. `QfcFormController.cs:958` reads the persisted flag to decide filtering. |
| Blocker | (coverage) | `artifacts/csharp/coverage.xml` | Canonical C# coverage artifact absent; coverage cannot be independently verified by the workflow's required mechanism. `LoadQuickFilerHighConfidenceAsync` (the entry point carrying the defect above) is at 0% coverage per the narrative comparison. | Produce `artifacts/csharp/coverage.xml` from the instrumented run; add coverage for the entry-point behavior to the extent feasible behind a seam. | Coverage verification is mandatory for every changed language; the one substantive new member at 0% is exactly the method with the AC6 defect. | `evidence/coverage/comparison.2026-06-01T17-12-39Z.md`; `artifacts/csharp/coverage.xml` not present on disk. |
| Minor | `TaskMaster/Ribbon/RibbonController.cs` | `SetHighConfidenceThresholdText` (lines 270–288) | Validation accepts any `double` in [0, 100] including fractional percentages (e.g. "12.5"), which is correct, but `GetHighConfidenceThresholdText` rounds to whole percent on read, so a stored 0.125 renders as "13" and a subsequent no-op re-entry of "13" would silently change the stored value to 0.13. Round-trip is lossy for non-integer percentages. | Either constrain input to whole-number percentages, or render the stored value without rounding so the displayed text round-trips exactly. | Minor data-fidelity issue; only affects users who enter fractional percentages. Not a correctness blocker for the documented [0,100] integer use case. | `RibbonController.cs:262-265` (round on read) vs. `:270-288` (parse on write). |
| Minor | `QuickFiler/Controllers/QfcItemController.cs`, `TaskMaster/Ribbon/RibbonViewer.cs`, `RibbonController.LoadQuickFilerHighConfidenceAsync` | new COM/WinForms boundary members | Thin boundary wrappers at 0% unit coverage. Acceptable as boundary seams, but `LoadQuickFilerHighConfidenceAsync` contains real branching logic (the `_quickFilerLoaded` guard plus the persisted-flag mutation) that is more than a one-line delegate and is where the F1 defect lives. | Extract the flag-setting decision into a testable helper, or pass mode as a parameter, so the entry-point behavior is covered. | Boundary wrappers are reasonably exempt, but the method carrying business logic and a defect should not be exempt. | `evidence/coverage/comparison...md` lists these at 0%. |
| Info | `QuickFiler/Controllers/QfcItemController.cs`, `QfcCollectionController.cs`, `QfcFormController.cs`, `UtilitiesCS/OutlookObjects/Folder/FolderScorer.cs` | whole files | Files exceed the 500-line policy limit (2437 / 2207 / 1080 / 607 lines). Pre-existing at merge-base; not caused by this feature's small additions. | Track a separate refactor to split these VSTO controllers; do not let them grow further. | Policy limit is breached but the breach predates this work. | `final-toolchain.2026-06-01T17-12-39Z.md` file-size note; `git diff` shows additive-only changes. |
| Info | test suite | `UtilitiesCS.Test` timing/concurrency tests | 11 flaky failures under coverage instrumentation, asserted pre-existing and non-regressive. | Continue the existing test-isolation work (commits 384858b8, b160037a); not a gate for this feature. | Same flaky category present in the pre-change baseline (8 failures, passing on re-run). | `final-toolchain...md`; `tests-coverage...txt` baseline. |

## Positive Observations

- `RemoveBelowThresholdAsync` correctly captures EntryIDs before mutating the list, avoiding
  index-drift during removal — a deliberate, commented decision (`QfcCollectionController.cs`).
- The cutoff comparison `TopFolderScore < cutoff` is inclusive of the boundary, matching the spec
  ("a group whose score equals the cutoff is retained"); a dedicated test verifies the boundary case.
- Null guards are present and tested in both `RemoveBelowThresholdAsync` (`_itemGroups is null`) and
  `ApplyHighConfidenceFilterAsync` (`groups is null || _globals?.QfSettings is null`).
- The injectable `Func<string, Task>` removal seam is the minimal DI seam called for by the C# rules
  and keeps the WinForms/COM removal path out of unit tests.
- `SetHighConfidenceThresholdText` uses `CultureInfo.InvariantCulture`, making the ribbon edit box
  locale-independent — a correct, non-obvious choice.
- All new tests use MSTest + Moq + FluentAssertions, follow Arrange-Act-Assert, and document intent.

## Typed Review Note

No TypeScript or Python files changed on this branch; typed-Python review is not applicable.
