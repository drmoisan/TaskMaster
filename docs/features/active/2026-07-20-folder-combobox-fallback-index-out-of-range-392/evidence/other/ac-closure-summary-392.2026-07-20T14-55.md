Timestamp: 2026-07-20T14-55

## AC Closure Summary — Issue #392

| AC | Text | Status | Backing evidence path(s) |
|---|---|---|---|
| AC-1 | Deterministic MSTest regression test reproduces the defect and fails before the fix; passes after; no temp files/external dependencies | Checked | `evidence/regression-testing/fail-before-392.2026-07-20T14-05.md`; `evidence/regression-testing/pass-after-392.2026-07-20T14-10.md` |
| AC-2 | `AssignFolderComboBox` no longer throws for single-entry `FolderArray` with no predetermined match; selects index 0 | Checked | `evidence/other/root-cause-392.2026-07-20T13-50.md`; `evidence/regression-testing/fail-before-392.2026-07-20T14-05.md`; `evidence/regression-testing/pass-after-392.2026-07-20T14-10.md` |
| AC-3 | Existing multi-suggestion index-1 behavior and predetermined-preselect behavior preserved | Checked | `evidence/regression-testing/targeted-no-regression-392.2026-07-20T14-13.md` |
| AC-4 | `PopulateAndSelectFolder` applies the same bounds-safe fallback | Checked | `evidence/other/root-cause-392.2026-07-20T13-50.md`; `evidence/regression-testing/fail-before-392.2026-07-20T14-05.md`; `evidence/regression-testing/pass-after-392.2026-07-20T14-10.md` |
| AC-5 | Full C# toolchain passes in order; zero regressions; new/changed code >= 90% coverage | **NOT fully checked (partial)** | `evidence/qa-gates/csharpier-final-392.2026-07-20T14-20.md`; `evidence/qa-gates/analyzer-final-392.2026-07-20T14-24.md`; `evidence/qa-gates/nullable-final-392.2026-07-20T14-28.md` (FAIL, pre-existing out-of-scope `SVGControl` vendored nullable debt, no regression); `evidence/qa-gates/vstest-coverage-final-392.2026-07-20T14-32.md`; `evidence/qa-gates/regression-check-392.2026-07-20T14-42.md`; `evidence/qa-gates/coverage-delta-392.2026-07-20T14-38.md`; `evidence/qa-gates/coverage-conversion-392.2026-07-20T14-50.md`; `evidence/issue-updates/ac-status-final-392.2026-07-20T14-55.md` (decision rationale) |

4 of 5 acceptance criteria are fully checked off. AC-5 is left unchecked because its nullable-build
component fails due to a pre-existing, out-of-scope, vendored-project (`SVGControl.csproj`)
nullable-reference-type debt condition, confirmed byte-for-byte identical to the P0-T11 baseline (no
regression introduced by this plan). This plan's Scope-Lock explicitly forbids modifying
`SVGControl.csproj` or any file other than `QuickFiler/Controllers/QfcItemController.FolderHandling.cs`
and `QuickFiler.Test/Controllers/QfcItemController.FolderHandlingTests.cs`, so this gap cannot be
resolved within this plan's authorized scope. All other AC-5 components (CSharpier format on
in-scope files, .NET analyzers build, MSTest execution, zero test regressions, and >= 90%
new/changed-code coverage) pass.
