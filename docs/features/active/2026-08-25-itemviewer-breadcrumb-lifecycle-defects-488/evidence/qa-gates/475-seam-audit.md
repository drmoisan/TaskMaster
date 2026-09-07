# #475 — Seam-Preservation Audit ([P6-T9])

Timestamp: 2026-08-28T06-09

Command:
`git grep -c -E 'new BreadcrumbPopupUiOperations\(|CreateForCurrentThreadTests' -- 'QuickFiler.Test/*.cs'`
and `git diff --name-only 12465043e052fce66a1861bf1ddd037a1aa81afc -- QuickFiler.Test`, the latter
after `git add -N` on the new test file so it appears in the diff.
EXIT_CODE: 0

## Test files that inject a `BreadcrumbPopupUiOperations` seam

Seventeen files construct operations through the injectable constructor or through
`CreateForCurrentThreadTests`, with the injection-site counts below.

| # | File | Injection sites | Modified by this feature? |
| --- | --- | --- | --- |
| 1 | `Viewers/BreadcrumbBridgeCoordinatorProbabilityTests.cs` | 1 | no |
| 2 | `Viewers/BreadcrumbDropDownCoverageThresholdTests.cs` | 1 | no |
| 3 | `Viewers/BreadcrumbDropDownOpenCoordinatorTests.Part2.cs` | 1 | no |
| 4 | `Viewers/BreadcrumbDropDownOpenCoordinatorTests.cs` | 1 | no |
| 5 | `Viewers/BreadcrumbDropDownReadinessTests.cs` | 3 | no |
| 6 | `Viewers/BreadcrumbDuplicateIdentityIntegrationTests.cs` | 1 | no |
| 7 | `Viewers/BreadcrumbItemViewerLifecycleCoordinatorTests.cs` | 1 | **yes** — owned; D2 only |
| 8 | `Viewers/BreadcrumbPopupBoundaryCoverageTests.Part2.cs` | 1 | **yes** — owned; the one mandatory test edit |
| 9 | `Viewers/BreadcrumbPopupBoundaryCoverageTests.cs` | 1 | no |
| 10 | `Viewers/BreadcrumbPopupControlDispatchTests.cs` | 3 | no |
| 11 | `Viewers/BreadcrumbPopupUiOperationsDirectAdapterTests.cs` | 1 | no |
| 12 | `Viewers/BreadcrumbSelectorCoordinatorTests.cs` | 1 | no |
| 13 | `Viewers/BreadcrumbSelectorOpenRetryTests.cs` | 4 | no |
| 14 | `Viewers/BreadcrumbSelectorToggleUiBoundaryTests.cs` | 3 | no |
| 15 | `Viewers/BreadcrumbUiThreadDispatchTests.cs` | 1 | no |
| 16 | `Viewers/FolderBreadcrumbAssetContractTests.cs` | 1 | no |
| 17 | `Viewers/ItemViewerBreadcrumbLifecycleRegressionTests.cs` | 7 | **yes** — this feature's new file |

Fifteen of the seventeen are untouched. **No existing test's injected
`BreadcrumbPopupUiOperations` seam was removed or replaced.** The seam itself — the injectable
constructor at `BreadcrumbPopupUiOperations` and the `CreateForCurrentThreadTests` factories on both
`BreadcrumbPopupUiOperations` and `BreadcrumbUiDispatcher` — is intact: `[P6-T3]` deleted only the
*ambient-probing selector* `CaptureCurrentOrTests`, and `[P3-T5]`'s and `[P6-T3]`'s diff evidence
confirms both `CreateForCurrentThreadTests` declarations are unchanged from `BASE_SHA`.

## Changed-file set under `QuickFiler.Test`

Command:

```
git diff --name-only 12465043e052fce66a1861bf1ddd037a1aa81afc -- QuickFiler.Test
```

Output:

```
QuickFiler.Test/QuickFiler.Test.csproj
QuickFiler.Test/Viewers/BreadcrumbItemViewerLifecycleCoordinatorTests.cs
QuickFiler.Test/Viewers/BreadcrumbPopupBoundaryCoverageTests.Part2.cs
QuickFiler.Test/Viewers/ItemViewerBreadcrumbLifecycleRegressionTests.cs
```

**Exactly the four paths this task's acceptance names, and no others.** Three are the owned test files
of constraint C1 and the fourth is the project file, which receives exactly one added `Compile Include`
line.

`git add -N` was run on the new test file first; without it that untracked file would not appear in the
diff and the enumeration would have been silently short by one.

## Attribution of the two modified existing test files

Neither was modified "in service of #475" beyond the single mandatory edit:

- `BreadcrumbItemViewerLifecycleCoordinatorTests.cs` was modified by **D2 only** — the `ThemesApplied`
  recorder on `RecordingHost` and the one new D2 regression test. It has one injection site, inside
  `LifecycleFixture`, which is untouched.
- `BreadcrumbPopupBoundaryCoverageTests.Part2.cs` carries the **one mandatory test edit in the whole
  change-set**: `CaptureCurrentOrTests_NullAndControlledContexts_SelectExpectedBoundaries` is deleted
  and `CaptureCurrent_NullAndControlledContexts_FailFastAndCapture` replaces it. That file was the only
  place in the repository referencing the deleted selector from a test, so the edit was unavoidable.
  Its own injection site and its `WithContext` helper are unchanged; the replacement test reuses that
  helper rather than introducing a new arrangement.

Output Summary: **Seventeen** test files inject a `BreadcrumbPopupUiOperations` seam, with per-file
site counts listed above; **fifteen are untouched** and no existing test's injected seam was removed or
replaced. `git diff --name-only <BASE_SHA> -- QuickFiler.Test` reports exactly
`QuickFiler.Test/QuickFiler.Test.csproj`,
`QuickFiler.Test/Viewers/BreadcrumbItemViewerLifecycleCoordinatorTests.cs`,
`QuickFiler.Test/Viewers/BreadcrumbPopupBoundaryCoverageTests.Part2.cs`, and
`QuickFiler.Test/Viewers/ItemViewerBreadcrumbLifecycleRegressionTests.cs`.
