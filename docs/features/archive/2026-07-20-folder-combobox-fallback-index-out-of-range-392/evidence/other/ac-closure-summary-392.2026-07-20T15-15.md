Timestamp: 2026-07-20T15-15

## AC Closure Summary — Issue #392 (revised, all AC checked)

| AC | Text | Status | Backing evidence path(s) |
|---|---|---|---|
| AC-1 | Deterministic MSTest regression test reproduces the defect and fails before the fix; passes after; no temp files/external dependencies | Checked | `evidence/regression-testing/fail-before-392.2026-07-20T14-05.md`; `evidence/regression-testing/pass-after-392.2026-07-20T14-10.md` |
| AC-2 | `AssignFolderComboBox` no longer throws for single-entry `FolderArray` with no predetermined match; selects index 0 | Checked | `evidence/other/root-cause-392.2026-07-20T13-50.md`; `evidence/regression-testing/fail-before-392.2026-07-20T14-05.md`; `evidence/regression-testing/pass-after-392.2026-07-20T14-10.md` |
| AC-3 | Existing multi-suggestion index-1 behavior and predetermined-preselect behavior preserved | Checked | `evidence/regression-testing/targeted-no-regression-392.2026-07-20T14-13.md` |
| AC-4 | `PopulateAndSelectFolder` applies the same bounds-safe fallback | Checked | `evidence/other/root-cause-392.2026-07-20T13-50.md`; `evidence/regression-testing/fail-before-392.2026-07-20T14-05.md`; `evidence/regression-testing/pass-after-392.2026-07-20T14-10.md` |
| AC-5 (amended 2026-07-20 by orchestrator, first-party-scoped nullable wording) | Full C# toolchain passes in order; zero regressions relative to Phase 0 baseline; new/changed code >= 90% coverage; vendored `SVGControl.csproj` nullable errors byte-identical to baseline are non-blocking (tracked in `docs/features/potential/2026-07-07-ci-nullable-check-skipped-vendored-projects.md`) | Checked | `evidence/qa-gates/csharpier-final-392.2026-07-20T14-20.md`; `evidence/qa-gates/analyzer-final-392.2026-07-20T14-24.md`; `evidence/qa-gates/nullable-final-392.2026-07-20T15-10.md` (error-set comparison: 0 new, 0 first-party); `evidence/qa-gates/vstest-coverage-final-392.2026-07-20T14-32.md`; `evidence/qa-gates/regression-check-392.2026-07-20T14-42.md`; `evidence/qa-gates/coverage-delta-392.2026-07-20T14-38.md`; `evidence/qa-gates/coverage-conversion-392.2026-07-20T14-50.md`; `evidence/issue-updates/ac-status-final-392.2026-07-20T15-15.md` |

All 5 acceptance criteria are fully checked off. AC-5's nullable-build component is satisfied under
the amended, first-party-scoped wording: the P2-T3 error-set comparison against the P0-T11 baseline
confirms zero new errors and zero errors attributable to any first-party project; all 34 errors are
byte-identical-to-baseline and confined to the vendored `SVGControl.csproj`, which is out of this
plan's Scope-Lock and out of the amended AC-5's enforcement scope.
