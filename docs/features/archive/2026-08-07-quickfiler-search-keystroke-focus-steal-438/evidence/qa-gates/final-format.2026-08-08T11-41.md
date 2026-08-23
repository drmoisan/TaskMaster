# [P6-T1] Final QA — Formatting

- **Issue:** #438
- **Task:** [P6-T1]
- **Timestamp:** 2026-08-08T11-41

## Command 1 — format (mutating)

`pwsh -NoProfile -Command "& ./.dotnet-sdk/dotnet.exe tool run csharpier format . ; exit $LASTEXITCODE"`

- **EXIT_CODE:** 0
- **Output:** `Formatted 1500 files in 5548ms.`

CSharpier is the repo-pinned **1.2.6** resolved through `./.dotnet-sdk/dotnet.exe` (verified in P0-T1). The global csharpier 1.3.0 on PATH was not used.

## Command 2 — check (gate)

`pwsh -NoProfile -Command "& ./.dotnet-sdk/dotnet.exe tool run csharpier check . ; exit $LASTEXITCODE"`

- **EXIT_CODE:** 0
- **Output:** `Checked 1500 files in 4668ms.`

Zero formatting violations remain. The file count rose from the 1488 checked at baseline (P0-T4) to 1500, matching the 12 new `.cs` files added by this change.

## Files reformatted

`format` normalized whitespace and line wrapping in the files this change touched (for example collapsing `BreadcrumbSelectionEffects.Handled | BreadcrumbSelectionEffects.RenderRequired` onto one line, and re-wrapping the chained FluentAssertions calls in the two new `UtilitiesCS.Test` suites). No formatter change altered any assertion, expected value, or control flow.

## Scope re-verification after formatting

The formatter must not silently widen the change surface, so the scope-critical files were re-checked after `format`:

| File | `git diff --stat` after format |
|---|---|
| `QuickFiler.Test/Viewers/BreadcrumbDropDownHostTests.cs` | `2 +-` — 1 insertion, 1 deletion (still the single `partial` token line) |
| `QuickFiler/Controllers/QfcItemController.FolderHandling.cs` | no rows (zero diff) |
| `QuickFiler/Controllers/EfcFormController.cs` | no rows (zero diff) |
| `QuickFiler/Controllers/BreadcrumbBridgeRouter.cs` | no rows (zero diff) |

AC-13's zero-diff proof and the D3 one-token constraint both survive formatting.

## Result

- **Output Summary:** `format` completed with EXIT_CODE 0 across 1500 files, and the `check` gate returned EXIT_CODE 0 with zero violations. Because `format` changed files, the remainder of the QA loop (P6-T2 size audit, P6-T3 analyzers, P6-T4 nullable, P6-T5 tests) runs against this post-format tree. The scope-locked files were re-verified after formatting and are unchanged. Accept criteria met.
