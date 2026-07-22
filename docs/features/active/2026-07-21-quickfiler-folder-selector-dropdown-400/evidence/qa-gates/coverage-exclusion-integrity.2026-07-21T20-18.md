# Coverage Exclusion Integrity

Timestamp: 2026-07-21T20-18Z
Command: Compare reviewed-base `HEAD:QuickFiler/Viewers/BreadcrumbDropDownHost.cs` and current host/helper `ExcludeFromCodeCoverage` locations with `git show`, `Get-Content`, and `Select-String`; run `git diff --exit-code HEAD -- coverage.config`; and scan the complete `.csproj` diff for coverage filters, attributes, or exclusions
EXIT_CODE: 0
Output Summary: Coverage filters and project coverage attributes are unchanged. The reviewed-base host had two method-level exclusions. The current implementation retains `ShowOwnedPopup` in the host and moves the direct WebView2 adapter exclusion to `BreadcrumbWebViewSurfaceFactory.CreateSurfaceAsync`, preserving exactly two method-level exclusions without adding a class-level exclusion.

## Reviewed-Base Exclusion Set

`HEAD:QuickFiler/Viewers/BreadcrumbDropDownHost.cs`:

1. Line 358 — `[ExcludeFromCodeCoverage]` on `CreateProductionSurfaceAsync`.
2. Line 389 — `[ExcludeFromCodeCoverage]` on `ShowOwnedPopup`.

## Current Exclusion Set

1. `QuickFiler/Viewers/BreadcrumbWebViewSurfaceFactory.cs:30` — `[ExcludeFromCodeCoverage]` on `CreateSurfaceAsync`.
2. `QuickFiler/Viewers/BreadcrumbDropDownHost.cs:477` — `[ExcludeFromCodeCoverage]` on `ShowOwnedPopup`.

Both annotations directly precede private static methods. Neither applies to a class or namespace. The direct-adapter exclusion moved from the host to the new helper exactly once, and the `ShowOwnedPopup` exclusion is retained unchanged in purpose and scope.

## Configuration and Attribute Checks

- `git diff --exit-code HEAD -- coverage.config`: exit 0; no filter change.
- Coverage-related matches in the complete `.csproj` diff: 0.
- No `ExcludeByAttribute`, `ExcludeByFile`, class-level coverage annotation, package setting, or coverage threshold was added or changed.
- The only production project change is the required helper Compile include.

P4-T2 result: PASS. The reviewed-base exclusion count and method-level scope are preserved.
