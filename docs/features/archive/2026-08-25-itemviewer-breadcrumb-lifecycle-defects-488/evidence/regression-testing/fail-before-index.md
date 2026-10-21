# Fail-Before Index — the Six Defect Units ([P9-T2])

Timestamp: 2026-08-28T06-33

Command: `grep -H 'EXIT_CODE:|ExpectedExitCode:'` over the seven fail-before artifacts under
`docs/features/active/itemviewer-breadcrumb-lifecycle-defects-488/evidence/regression-testing/`.
EXIT_CODE: 0

This index is produced **before** the criterion that cites it is flipped, because a check-off task may
not cite an artifact a later task has not yet written. `[P9-T14]` flips that criterion and cites this
file.

## One row per defect unit

The CLAUDE.md Bugfix Workflow requires a failing regression test **first**, for each defect unit, with
evidence recording it failing against the unfixed code before the corresponding production change lands.

| # | Unit | Regression test | Fail-before artifact | `EXIT_CODE:` | `ExpectedExitCode:` |
| --- | --- | --- | --- | --- | --- |
| 1 | **D1** | `ConfigureBreadcrumbDropDown_EnvironmentChange_DisposesOutgoingHostBeforeReplacement` | `488-d1-fail.md` | **1** | 1 |
| 2 | **D2** | `ConfigureHostQueued_SetThemeBeforeDrain_ReplaysThemeOntoAdoptedHost` | `488-d2-fail.md` | **1** | 1 |
| 3 | **D3** | `InitializeBreadcrumbPipeline_SecondDifferentProvider_ThrowsInvalidOperationException` | `488-d3-fail.md` | **1** | 1 |
| 4 | **D4** | `InitializeBreadcrumbPipeline_AmbientContextNull_ThrowsBoundaryDiagnostic` and `InitializeBreadcrumbPipeline_DifferentNonNullContext_ThrowsBoundaryDiagnostic` | `488-d4-fail.md` | **1** | 1 |
| 5 | **D5** | `InitializeBreadcrumbPipeline_AfterViewerDisposed_ThrowsObjectDisposedException` | `488-d5-fail.md` | **1** | 1 |
| 6a | **#475** | `LegacySurfaceFactoryConstructor_AmbientContextNull_ThrowsInvalidOperationException` | `475-ctor-fail.md` | **1** | 1 |
| 6b | **#475** | `ConfigureBreadcrumbDropDown_SeededLifecycleNullUiContext_DoesNotThrow` | `475-lazy-fail.md` | **1** | 1 |

All artifacts live under
`docs/features/active/itemviewer-breadcrumb-lifecycle-defects-488/evidence/regression-testing/`.

**Every cited artifact records a non-zero `EXIT_CODE:` of 1 together with `ExpectedExitCode: 1`**, which
was verified by a direct search rather than asserted.

## Why #475 carries two fail-before artifacts

#475 is one defect unit delivered in three parts, and two of those parts required separate red
observations because they fail for different reasons and at different points:

- **`475-lazy-fail.md`** is the ordinary case. After `[P6-T3]` delivered parts 1 and 2, the
  seam-preservation test failed because the eagerly evaluated `EnsureBreadcrumbLifecycle` argument threw
  from `BreadcrumbUiDispatcher.CaptureCurrent()` before the already-initialized early return could
  discard it. Part 3 in `[P6-T7]` made that argument lazy and turned the test green, recorded in
  `475-pass.md`.
- **`475-ctor-fail.md`** required a **temporary revert**. `[P6-T3]` delivers the deletion and the
  constructor swaps as one compile-valid edit set — deleting the selector while a caller still names it
  would not compile — so there is no intermediate state in which the constructor still degrades silently
  and the tree still builds. The observation was taken by temporarily restoring a local ambient-probing
  selector private to `BreadcrumbDropDownHost.cs`, **rebuilding** (exit 0), running the filter, then
  restoring the two swaps and **rebuilding again** (exit 0). Both rebuilds are recorded in that
  artifact, because `vstest.console.exe` runs the compiled assembly rather than the source: reverting
  without rebuilding would have left the fail-fast swaps in the binary and the test would have passed.
  The restored file's SHA-256 matches its pre-revert snapshot exactly.

## Observed failure mode per unit

Each red observation is discriminating, not incidental:

| Unit | What the red run observed |
| --- | --- |
| D1 | The **first** assertion failed — `SetTheme` on the captured host raised no `ObjectDisposedException`, the discriminating observation named by decision D-10a. The two corroborating assertions were never reached. |
| D2 | `ThemesApplied` was an **empty collection**: the theme set while the post was queued was lost outright. |
| D3 | "No exception was thrown" — the second, different provider was silently discarded by the blanket early return. The positive case already passed and is recorded as observed. |
| D4 | Both tests: "no exception was thrown", confirming no boundary check existed. |
| D5 | "No exception was thrown" — the call succeeded against a disposed viewer. |
| #475 lazy | `InvalidOperationException` from `BreadcrumbUiDispatcher.CaptureCurrent()` inside the eager argument. |
| #475 ctor | `ArgumentNullException` instead of `InvalidOperationException` — the restored selector substituted a test dispatcher silently. |

Output Summary: **All six defect units — D1, D2, D3, D4, D5, and #475 — have recorded fail-before
evidence.** Seven artifacts cover them (`488-d1-fail.md`, `488-d2-fail.md`, `488-d3-fail.md`,
`488-d4-fail.md`, `488-d5-fail.md`, and both `475-ctor-fail.md` and `475-lazy-fail.md` for #475), and
**every one records a non-zero `EXIT_CODE: 1` with `ExpectedExitCode: 1`**, verified by direct search.
