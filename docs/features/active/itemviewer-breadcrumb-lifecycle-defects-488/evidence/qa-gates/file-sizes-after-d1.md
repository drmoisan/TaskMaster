# File Sizes After D1 ([P1-T9])

Timestamp: 2026-08-28T05-30

Command: `wc -l` over the three named files, plus
`git diff --name-only 12465043e052fce66a1861bf1ddd037a1aa81afc -- QuickFiler.Test/Viewers/BreadcrumbDropDownIntegrationTests.cs`
EXIT_CODE: 0

## Line counts

| File | Baseline | Now | Delta | Limit | Result |
| --- | --- | --- | --- | --- | --- |
| `QuickFiler/Viewers/ItemViewer.Breadcrumb.cs` | 319 | **331** | +12 | at most 500 | pass |
| `QuickFiler.Test/Viewers/ItemViewerBreadcrumbLifecycleRegressionTests.cs` | 0 (new) | **188** | +188 | at most 480 | pass |
| `QuickFiler.Test/Viewers/BreadcrumbDropDownIntegrationTests.cs` | 500 | **500** | 0 | exactly 500 | pass |

`ItemViewer.Breadcrumb.cs` grew by 12 lines against a planned D1 delta of +4. The difference is the
eight-line explanatory comment block `[P1-T5]` placed above the new statement, recording why the type
test names the concrete `BreadcrumbDropDownHost` rather than the interface and why a fresh pattern
variable is required. The file has 169 lines of headroom remaining against the 500-line ceiling, which
comfortably absorbs the +8 (D3), +14 (D4), +4 (D5), and +3 (#475 part 3) still budgeted for it in
constraint C2.

## Byte-identity of `BreadcrumbDropDownIntegrationTests.cs`

Command:

```
git diff --name-only 12465043e052fce66a1861bf1ddd037a1aa81afc -- QuickFiler.Test/Viewers/BreadcrumbDropDownIntegrationTests.cs
```

**Output: no lines.**

This — and not the line count — is what establishes the **byte-identity** the criterion `[P1-T13]`
flips demands. A file can be edited without changing its line count, so the "exactly 500" figure in
the table above establishes only that the file is still at the ceiling; it does not establish that the
file is unmodified. The empty diff output establishes that it is byte-identical to its state at
`BASE_SHA` `12465043e052fce66a1861bf1ddd037a1aa81afc`.

The separate criterion that `[P9-T4]` flips does assert the exact line count of 500 alongside the
unmodified claim, and `[P8-T8]` records both conjuncts for it after the final formatting pass.

Output Summary: `ItemViewer.Breadcrumb.cs` is **331** lines (at most 500, pass),
`ItemViewerBreadcrumbLifecycleRegressionTests.cs` is **188** lines (at most 480, pass), and
`BreadcrumbDropDownIntegrationTests.cs` is **exactly 500** lines and produces **no output lines** from
`git diff --name-only <BASE_SHA>`, establishing byte-identity.
