# Coverage Delta Against the Phase 0 Baseline ([P8-T7])

Timestamp: 2026-08-28T06-28

Command: computed from the two retained Cobertura documents —
`evidence/baseline/coverage-baseline.cobertura.xml` (`[P0-T14]`) and
`evidence/qa-gates/coverage-final.cobertura.xml` (`[P8-T6]`) — using one counting method applied
identically to both. **No new coverage run was started for this task.**
EXIT_CODE: 0

## Counting method

Rates are computed per `<line>` element, deduplicated by `(filename, line number)` so a line emitted
under several `class` elements — async state machines and lambdas are separate `class` elements — is
counted once. The method reproduces each document's own root `line-rate` and `branch-rate` attributes
exactly, which is what validates it.

Two denominators are reported:

- **RAW** — every measured line. Test assemblies are already outside it (`coverage.config` excludes
  them, and the nine Cobertura packages are all production assemblies), and `[ExcludeFromCodeCoverage]`
  members never appear at all.
- **TESTABLE DENOMINATOR** — RAW minus Designer-generated files, the exemption `CLAUDE.md` § UT2 names
  explicitly under category (b) and the one that is mechanically identifiable by filename. Categories
  (a) VSTO add-in lifecycle and (c) Outlook-Interop event handlers are already removed from RAW by their
  in-source `[ExcludeFromCodeCoverage]` attributes, so they are not double-subtracted. **30** Designer
  files holding **3574** lines are excluded, identically on both sides.

## The four required repository-wide figures

| # | Figure | Value |
| --- | --- | --- |
| 1 | **Baseline RAW line rate** | **0.852607** (54670 / 64121) |
| 2 | **Baseline TESTABLE-DENOMINATOR line rate** | **0.850562** (51499 / 60547) |
| 3 | **Post-change RAW line rate** | **0.852830** (54692 / 64130) |
| 4 | **Post-change TESTABLE-DENOMINATOR line rate** | **0.850799** (51521 / 60556) |

### The change does not lower either denominator

| Denominator | Baseline | Post-change | Delta | Not lowered? |
| --- | --- | --- | --- | --- |
| RAW | 0.852607 | **0.852830** | **+0.000223** | **yes — increased** |
| Testable | 0.850562 | **0.850799** | **+0.000237** | **yes — increased** |

Both figures **rose**. The no-regression condition holds for each of the two denominators separately,
which is why both baseline figures had to be recorded: with only a baseline raw figure the
testable-denominator claim could not have been evaluated at all.

### The `>= 80%` floor against the CLAUDE.md testable denominator

**0.850799 = 85.08%**, which is at or above the `>= 80%` floor stated in `CLAUDE.md` § UT2. It also
clears the `>= 85%` uniform line floor in `.claude/rules/quality-tiers.md`.

Branch rates, for completeness: baseline raw 0.791925 and testable 0.794114; post-change raw 0.792255
and testable 0.794446. Both rose, and both clear the `>= 75%` branch floor.

## Per-file rates for the three MEASURED owned production files

| File | Baseline (`[P0-T15]`) | Post-change | Delta | `>= 90%`? |
| --- | --- | --- | --- | --- |
| `BreadcrumbItemViewerLifecycleCoordinator.cs` | 0.905660 (288/318) | **0.909091** (300/330) | **+0.003431** | yes |
| `BreadcrumbPopupUiOperations.cs` | 0.991453 (232/234) | **0.991342** (229/231) | −0.000111 | yes |
| `BreadcrumbDropDownHost.cs` | 0.992883 (279/281) | **0.992883** (279/281) | 0 | yes |

**All three exceed 90%.**

### The `BreadcrumbPopupUiOperations.cs` figure fell by 0.000111, and no line lost coverage

This is an arithmetic artifact of a deletion, not a coverage regression. #475 part 1 deleted
`CaptureCurrentOrTests`, whose body was **fully covered**: covered lines fell 232 → 229 and valid lines
fell 234 → 231, both by exactly 3. Removing three fully-covered lines from a file whose rate is below
1.0 necessarily lowers that ratio slightly.

**The file's uncovered-line count is unchanged at 2** — 234 − 232 = 2 before, 231 − 229 = 2 after. Not
one line that was covered became uncovered. The only edit to this file is the deletion, and the file
received no addition of any kind.

## Coverage for the changed lines is not reduced

Every line this feature **added** to a measured production file was checked individually against the
post-change Cobertura:

| File | Added lines | Measured added lines | Covered | Uncovered |
| --- | --- | --- | --- | --- |
| `BreadcrumbItemViewerLifecycleCoordinator.cs` | 16 | 12 | **12** | **0** |
| `BreadcrumbPopupUiOperations.cs` | 0 (deletion only) | 0 | 0 | **0** |
| `BreadcrumbDropDownHost.cs` | 2 | 2 | **2** | **0** |
| **Total** | 18 | **14** | **14** | **0** |

**All 14 measured added lines are covered — 100%, zero uncovered.** They include D2's retained-theme
guard (`string? retained = _retainedTheme;`, the two-conjunct `if`, and `host.SetTheme(retained);`), the
`_retainedTheme = theme;` assignment in `SetTheme`, and both `CaptureCurrent()` constructor-chain
arguments. The four unmeasured added lines in the coordinator are blank or brace-only lines the
collector does not emit.

Each new or changed **measured** production member therefore reaches at least 90% line coverage, and
coverage for the changed lines is not reduced relative to the Phase 0 baseline.

## `ItemViewer.Breadcrumb.cs` contributes no coverage movement

`QuickFiler/Viewers/ItemViewer.Breadcrumb.cs` matches **zero** `class` elements in the post-change
Cobertura, exactly as at baseline. `QuickFiler/Viewers/ItemViewer.cs:20` carries
`[ExcludeFromCodeCoverage]` on the `ItemViewer` partial **type**, and a type-level attribute on one part
applies to the whole partial type, so every member of that file is excluded from measurement.

**D1, D3, D4, D5, and #475 part 3 are therefore coverage-exempt by construction and move no coverage
number.** Their regression tests are required by the CLAUDE.md Bugfix Workflow and by the acceptance
criteria, not by a coverage delta. A reviewer must not read flat coverage on those five units as a
testing gap, and must not remove the exemption to "fix" it: `ItemViewer.cs` is a forbidden file and its
attribute is assumption D489-2.

Output Summary: **Four repository-wide figures.** Baseline raw **0.852607** and testable-denominator
**0.850562**; post-change raw **0.852830** and testable-denominator **0.850799**. **Both denominators
rose**, so the change lowers neither, and the post-change testable figure of **85.08%** is at or above
the `>= 80%` CLAUDE.md floor. Per-file: the coordinator rose to **0.909091**,
`BreadcrumbPopupUiOperations.cs` moved −0.000111 purely as a deletion artifact with its uncovered-line
count unchanged at 2, and `BreadcrumbDropDownHost.cs` is unchanged at **0.992883**; all three exceed
90%. **All 14 measured added lines are covered, zero uncovered.**
`QuickFiler/Viewers/ItemViewer.Breadcrumb.cs` contributes no coverage movement because
`ItemViewer.cs:20` carries a type-level exclusion.
