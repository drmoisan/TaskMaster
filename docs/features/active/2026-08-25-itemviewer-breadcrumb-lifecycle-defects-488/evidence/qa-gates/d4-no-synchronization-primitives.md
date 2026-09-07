# D4 — No Synchronization Primitive Was Introduced ([P4-T7])

Timestamp: 2026-08-28T05-48

Command:

```
git add -N QuickFiler.Test/Viewers/ItemViewerBreadcrumbLifecycleRegressionTests.cs
git diff 12465043e052fce66a1861bf1ddd037a1aa81afc -- QuickFiler/Viewers/ItemViewer.Breadcrumb.cs QuickFiler/Viewers/BreadcrumbItemViewerLifecycleCoordinator.cs QuickFiler/Viewers/BreadcrumbPopupUiOperations.cs QuickFiler/Viewers/BreadcrumbDropDownHost.cs QuickFiler.Test/Viewers/BreadcrumbItemViewerLifecycleCoordinatorTests.cs QuickFiler.Test/Viewers/BreadcrumbPopupBoundaryCoverageTests.Part2.cs QuickFiler.Test/Viewers/ItemViewerBreadcrumbLifecycleRegressionTests.cs
```

then a whole-word search of the diff's **added** lines for each of the five banned tokens.
EXIT_CODE: 0

## Scope — all SEVEN owned files were inspected

| # | File | Category |
| --- | --- | --- |
| 1 | `QuickFiler/Viewers/ItemViewer.Breadcrumb.cs` | owned production |
| 2 | `QuickFiler/Viewers/BreadcrumbItemViewerLifecycleCoordinator.cs` | owned production |
| 3 | `QuickFiler/Viewers/BreadcrumbPopupUiOperations.cs` | owned production |
| 4 | `QuickFiler/Viewers/BreadcrumbDropDownHost.cs` | owned production |
| 5 | `QuickFiler.Test/Viewers/BreadcrumbItemViewerLifecycleCoordinatorTests.cs` | owned test |
| 6 | `QuickFiler.Test/Viewers/BreadcrumbPopupBoundaryCoverageTests.Part2.cs` | owned test |
| 7 | `QuickFiler.Test/Viewers/ItemViewerBreadcrumbLifecycleRegressionTests.cs` | owned test (new) |

Seven files rather than the four production files, because the criterion `[P4-T13]` flips bans those
constructs "by this feature" without restricting the ban to production code, and the three owned test
files are part of this feature.

`git add -N` was run on the new test file first. Without it that file is untracked and contributes no
added lines to the diff, so all of its content would have escaped the search — the file most likely to
reach for a threading construct would have been the one file not examined.

## Result

The diff carries **496 added lines** across the seven files. Matching is **whole-word**.

| Token | Count on added lines |
| --- | --- |
| `Interlocked` | **0** |
| `lock` | **0** |
| `Monitor` | **0** |
| `Volatile` | **0** |
| `Mutex` | **0** |
| **Total** | **0** |

No added line in the diff contains any of the five tokens as a whole word.

## Why whole-word matching is the correct predicate here

Two near-misses in the added text would produce false positives under a substring search and are
correctly excluded:

- **`wall-clock`** appears on **4** added lines, in doc comments recording that no wall-clock wait is
  used. A substring search for `lock` matches inside `clock`; a whole-word search does not, because
  `clock` is bounded by word characters on the left.
- **`Interlocked`** likewise contains `lock` as a substring. It is searched for as its own token and
  found zero times, and it cannot inflate the `lock` count under whole-word matching.

The distinction matters because a false positive here would be indistinguishable from a real violation
without re-reading the diff by hand.

## Consistency with the delivered design

This is the expected result rather than a lucky one. D4's design deliberately rejects atomic
initialization: making the three pipeline fields atomic would require `Interlocked.CompareExchange` or
a lock plus a disposal path for the loser of each race, **and it would not solve the underlying
problem**, because `ItemViewer` is a `UserControl`, `components` is WinForms state, and the breadcrumb
anchor is a `Control`. Making those fields atomic would legitimise off-thread access to control state
that is not thread-safe at all. The delivered guard instead **declares and enforces** the affinity
contract with a reference comparison and a throw, which needs no synchronization primitive.

Output Summary: All **seven** owned files were inspected, with the new test file intent-added first so
its content appears in the diff. Across **496 added lines**, the whole-word counts for `Interlocked`,
`lock`, `Monitor`, `Volatile`, and `Mutex` are **0, 0, 0, 0, and 0** respectively. `wall-clock` appears
on 4 added lines and is correctly not counted as `lock` under whole-word matching.
