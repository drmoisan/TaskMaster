# Host Fix Scope

Timestamp: 2026-07-21T20-04Z
Command: `git status --short -- QuickFiler QuickFiler.Test`; `Get-Content` line counts for both authorized production files and the five affected test files; exact `Select-String` counts for host/helper Compile includes, host constructors, and `ExcludeFromCodeCoverage`; `git show 'HEAD:QuickFiler/Viewers/BreadcrumbDropDownHost.cs'` baseline exclusion count; SHA-256 over ordered exact assertion-bearing lines in the four P0-T9-protected test files; `git diff --name-only HEAD -- QuickFiler`; `git ls-files --others --exclude-standard -- QuickFiler`; forbidden-path diff-name filtering; and `git diff --check`
EXIT_CODE: 0
Output Summary: Phase 2 production/project changes are limited to the authorized host, new helper, and one adjacent helper Compile include. The host is 484 lines, the helper is 118 lines, both are below 500, the exclusion was moved exactly once, all protected assertion hashes match P0-T9, forbidden production/configuration paths are unchanged, and the diff has no whitespace errors.

## Authorized Production and Project Diff

Tracked `QuickFiler` changes:

- `QuickFiler/QuickFiler.csproj`
- `QuickFiler/Viewers/BreadcrumbDropDownHost.cs`

Untracked `QuickFiler` production files:

- `QuickFiler/Viewers/BreadcrumbWebViewSurfaceFactory.cs`

No other `QuickFiler` production or project path is modified or untracked. The project change adds one helper Compile include. The added end-of-file newline is non-semantic and accompanies that single project edit.

## Size and Contract Checks

| Check | Result |
|---|---:|
| `BreadcrumbDropDownHost.cs` lines | 484 |
| Required host range | 475–485 |
| `BreadcrumbWebViewSurfaceFactory.cs` lines | 118 |
| Helper target | Approximately 105; 13-line variance |
| Production files above 500 lines | 0 |
| Exact host Compile includes | 1 at line 394 |
| Exact helper Compile includes | 1 at line 395 |
| Helper immediately follows host include | Yes |
| Public host constructors | 2 |
| Internal readiness constructors | 1 |

The helper variance consists of the required correlated navigation handlers and deterministic cleanup paths; it remains a focused internal static adapter and is below the hard 500-line limit.

## Exclusion Movement

- Reviewed base host exclusions: 2.
- Current host exclusions: 1, on `ShowOwnedPopup` at line 477.
- Current helper exclusions: 1, on `CreateSurfaceAsync` at line 30.
- Total method-level exclusion count remains 2.
- No class-level exclusion was added.

The direct WebView2 adapter exclusion moved from the host to the corresponding helper method exactly once. The existing `ShowOwnedPopup` exclusion remains in the host.

## Protected Assertion Integrity

The P0-T9 hashing method was rerun over ordered exact lines matching `\.Should\(` or `\bAssert\.`.

| Protected file | Assertions | P0-T9 SHA-256 | Current SHA-256 | Result |
|---|---:|---|---|---|
| `BreadcrumbDropDownReadinessTests.cs` | 51 | `58cff79fb67b5a6d95f60e961adedba7492691fdd9ffe16036ea467417bfda6d` | `58cff79fb67b5a6d95f60e961adedba7492691fdd9ffe16036ea467417bfda6d` | MATCH |
| `BreadcrumbDropDownLifecycleConcurrencyTests.cs` | 81 | `a38135b5a39844c4a4f1a420773d54dac6cff6c87c0dcc979a8edd4ebce3e84a` | `a38135b5a39844c4a4f1a420773d54dac6cff6c87c0dcc979a8edd4ebce3e84a` | MATCH |
| `BreadcrumbDropDownHostTests.cs` | 52 | `8d9b16ed5d5e2ca21217e4e4c6653415f7fb7c13c105119f7a2182cac418f3dc` | `8d9b16ed5d5e2ca21217e4e4c6653415f7fb7c13c105119f7a2182cac418f3dc` | MATCH |
| `BreadcrumbDropDownLifecycleTests.cs` | 34 | `fc9370c70b339dd99251e43385d82e7c04c2ac779a17546c3ae64e0a7c4fd5ce` | `fc9370c70b339dd99251e43385d82e7c04c2ac779a17546c3ae64e0a7c4fd5ce` | MATCH |

`BreadcrumbDropDownReadinessTests.cs` is 305 lines rather than its 307-line rebaseline because two non-assertion `Task.Yield` calls were removed after they were shown to deadlock under an installed `WindowsFormsSynchronizationContext` without a message loop. No assertion was changed, removed, or weakened.

## Forbidden-Scope Checks

`git diff --name-only HEAD --` returned no match for:

- `QuickFiler/Viewers/ItemViewer.Breadcrumb.cs`
- `QuickFiler/Viewers/BreadcrumbMessengerHub.cs`
- `QuickFiler/Resources/FolderBreadcrumb.html`
- `QuickFiler/Viewers/IItemViewer.cs`
- any `packages.config`
- any settings file
- any `.props` or `.targets` file
- `coverage.config`

No package, settings, coverage-filter, coverage-configuration, or public `IItemViewer` signature change is present. `git diff --check` exited 0.

P2-T8 result: PASS. The implementation did not require a broader architectural change.
