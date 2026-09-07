# Phase 0 — Baseline Exemption and Identifier Counts ([P0-T17])

Timestamp: 2026-08-28T05-21

Command:
`grep -c -F 'ExcludeFromCodeCoverage' <the seven owned files>` and
`git grep -n -F 'CaptureCurrentOrTests' -- '*.cs'`, both run from the worktree root.
EXIT_CODE: 0

---

## Breakdown 1 — `ExcludeFromCodeCoverage` occurrences in the SEVEN owned files

Seven files rather than four. The criterion `[P9-T13]` flips states that no new attribute is
introduced **anywhere** by this feature, and the three owned test files are part of this feature, so
restricting the baseline to the four production files would leave the test half of that criterion
unevidenced.

| # | Owned file | Baseline count |
| --- | --- | --- |
| 1 | `QuickFiler/Viewers/ItemViewer.Breadcrumb.cs` | **0** |
| 2 | `QuickFiler/Viewers/BreadcrumbItemViewerLifecycleCoordinator.cs` | **0** |
| 3 | `QuickFiler/Viewers/BreadcrumbPopupUiOperations.cs` | **7** |
| 4 | `QuickFiler/Viewers/BreadcrumbDropDownHost.cs` | **0** |
| 5 | `QuickFiler.Test/Viewers/BreadcrumbItemViewerLifecycleCoordinatorTests.cs` | **0** |
| 6 | `QuickFiler.Test/Viewers/BreadcrumbPopupBoundaryCoverageTests.Part2.cs` | **0** |
| 7 | `QuickFiler.Test/Viewers/ItemViewerBreadcrumbLifecycleRegressionTests.cs` | **0** — the file does not yet exist |

Total across the seven owned files: **7**.

The seven occurrences in `BreadcrumbPopupUiOperations.cs` sit at lines 105, 380, 383, 390, 394, 412,
and 457. `[P8-T10]` compares the post-change per-file counts against this table and requires equality
on every row, which detects both an added attribute and a removed one.

Note that `QuickFiler/Viewers/ItemViewer.Breadcrumb.cs` carries **zero** occurrences of its own. Its
coverage exemption is inherited from the type-level `[ExcludeFromCodeCoverage]` at
`QuickFiler/Viewers/ItemViewer.cs:20`, which is on a **forbidden** file this feature must not touch.
That is why the exemption cannot be evidenced from this file's own count and is evidenced separately
in `[P0-T15]` and `[P8-T10]`.

---

## Breakdown 2 — `CaptureCurrentOrTests` occurrences in tracked `.cs` files

`git grep` was used so that only tracked files are searched, excluding build output under `bin/` and
`obj/`.

### The five production occurrences

| File | Line | Role |
| --- | --- | --- |
| `QuickFiler/Viewers/BreadcrumbPopupUiOperations.cs` | 86 | the declaration: `internal static BreadcrumbPopupUiOperations CaptureCurrentOrTests() =>` |
| `QuickFiler/Viewers/BreadcrumbDropDownHost.cs` | 98 | constructor-chain argument in the `public` seven-parameter `LegacySurfaceFactory` overload |
| `QuickFiler/Viewers/BreadcrumbDropDownHost.cs` | 118 | constructor-chain argument in the `internal` seven-parameter `ReadySurfaceFactory` overload |
| `QuickFiler/Viewers/ItemViewer.Breadcrumb.cs` | 156 | `EnsureBreadcrumbLifecycle` argument in the two-argument `ConfigureBreadcrumbDropDown(environment, initializer)` overload |
| `QuickFiler/Viewers/ItemViewer.Breadcrumb.cs` | 192 | `EnsureBreadcrumbLifecycle` argument in the three-argument `ConfigureBreadcrumbDropDown(host, anchorBounds, workingArea)` overload |

This is exactly the five-occurrence set this task's acceptance names, at exactly the five cited lines.
The citations resolve without drift at this base commit.

### The three test occurrences

All in `QuickFiler.Test/Viewers/BreadcrumbPopupBoundaryCoverageTests.Part2.cs`:

| Line | Role |
| --- | --- |
| 171 | the method-name line: `public void CaptureCurrentOrTests_NullAndControlledContexts_SelectExpectedBoundaries()` |
| 178 | a call line — the method group passed to `WithContext(null, ...)` |
| 186 | a call line — the method group passed to `WithContext(context, ...)` |

That is one method-name line and two call lines, as the acceptance states.

### Totals

| Scope | Occurrence lines |
| --- | --- |
| Production `.cs` | 5 |
| Test `.cs` | 3 |
| **All tracked `.cs`** | **8** |

Per-file line counts from `git grep -c`: `BreadcrumbPopupBoundaryCoverageTests.Part2.cs` 3,
`BreadcrumbDropDownHost.cs` 2, `BreadcrumbPopupUiOperations.cs` 1, `ItemViewer.Breadcrumb.cs` 2. No
other tracked `.cs` file names the identifier.

`[P6-T3]` must retire **all five** production references in one edit set. Retiring only the two
`BreadcrumbDropDownHost` references would leave `ItemViewer.Breadcrumb.cs` naming a deleted member,
which is compile error CS0117, and would make `[P6-T3]`'s own zero-match acceptance unreachable and
`[P6-T4]`'s and `[P6-T5]`'s builds fail for a reason unrelated to the defect under test. The three
test occurrences are retired by deleting and replacing the one test method.

Output Summary: Baseline `ExcludeFromCodeCoverage` counts across the seven owned files are 0, 0, **7**,
0, 0, 0, 0 — total 7, all seven in `BreadcrumbPopupUiOperations.cs` at lines 105, 380, 383, 390, 394,
412, 457. `CaptureCurrentOrTests` occurs on **8** lines in tracked `.cs` files: the five production
sites named in the acceptance (declaration at `BreadcrumbPopupUiOperations.cs:86`; calls at
`BreadcrumbDropDownHost.cs:98` and `:118` and `ItemViewer.Breadcrumb.cs:156` and `:192`), plus one
method-name line and two call lines in `BreadcrumbPopupBoundaryCoverageTests.Part2.cs` at 171, 178,
and 186.
