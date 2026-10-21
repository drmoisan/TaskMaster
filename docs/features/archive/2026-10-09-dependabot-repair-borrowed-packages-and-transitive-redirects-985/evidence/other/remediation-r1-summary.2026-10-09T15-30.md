# Remediation R1 Summary (review 2026-10-09T14-55, issue #985)

Timestamp: 2026-10-09T15-30
Command: Grep tool `^- \[[ x]\] AC` over FEATURE/spec.md (content, -o); artifact paths cross-checked against the evidence tree
EXIT_CODE: 0
Output Summary:
- Six remediation items mapped below, each to existing artifacts.
- spec.md AC state: AC1 to AC6 `[x]` (lines 193-198), AC7 `[ ]` (line 199); unchanged from the start of the cycle. No AC checkbox or AC text changed in R1.
- AC7 remains pending CI (post-merge `@dependabot recreate` on PR #984), checked off by the item's orchestrator run after CI is observed.
- D1 residual: the workflow comment drift in `.github/workflows/dependabot-repair.yml` lines 87-105 stays out of scope (AC4) and is promoted to issue #986 (promoted record `docs/features/potential/promoted/2026-10-09-dependabot-repair-workflow-comments-predate-redirect-sync.md`, committed without content change).

| Item | Fail-before | Pass-after | QA-gate artifacts |
|---|---|---|---|
| B-1 (account name in committed plan) | `evidence/regression-testing/fail-before-identity-sweep.2026-10-09T15-20.md` | `evidence/regression-testing/pass-after-identity-sweep.2026-10-09T15-20.md` | `evidence/qa-gates/identity-sweep-final-r1.<TS>.md` (P6-T2) |
| CR-1 (Direction on repair records and report lines) | `evidence/regression-testing/fail-before-sync-module.2026-10-09T15-22.md`, `evidence/regression-testing/fail-before-repair-written-path.2026-10-09T15-22.md` (N7) | `evidence/regression-testing/pass-after-sync-module.2026-10-09T15-23.md`, `evidence/regression-testing/pass-after-repair-written-path.2026-10-09T15-23.md` | `evidence/qa-gates/ps-coverage-r1.2026-10-09T15-29.md` |
| CR-2 (WrittenPath lists each path once) | `evidence/regression-testing/fail-before-repair-written-path.2026-10-09T15-22.md` (N9) | `evidence/regression-testing/pass-after-repair-written-path.2026-10-09T15-23.md` | `evidence/qa-gates/ps-coverage-r1.2026-10-09T15-29.md`, `evidence/other/whatif-live-tree-r1.2026-10-09T15-26.md` |
| CR-3 (case-insensitive handled-name check) | `evidence/regression-testing/fail-before-sync-module.2026-10-09T15-22.md` (N5) | `evidence/regression-testing/pass-after-sync-module.2026-10-09T15-23.md` | `evidence/qa-gates/ps-test-mcp-r1.2026-10-09T15-27.md` |
| AC4-RECHECK (no workflow change) | not applicable (scope guard) | `evidence/qa-gates/workflows-untouched-r1.2026-10-09T15-30.md` | `evidence/qa-gates/workflows-untouched-r1.2026-10-09T15-30.md` |
| AC5-RECHECK (in-memory tests, coverage, toolchain) | not applicable | `evidence/qa-gates/ps-coverage-r1.2026-10-09T15-29.md` | `evidence/qa-gates/ps-format-r1.2026-10-09T15-26.md`, `evidence/qa-gates/ps-analyze-r1.2026-10-09T15-26.md`, `evidence/qa-gates/ps-test-mcp-r1.2026-10-09T15-27.md`, `evidence/qa-gates/ps-temp-file-audit-r1.2026-10-09T15-29.md`, `evidence/qa-gates/ps-line-counts-r1.2026-10-09T15-29.md`, `evidence/qa-gates/coverage-comparison-powershell-r1.2026-10-09T15-29.md`, `evidence/qa-gates/csharp-not-applicable-r1.2026-10-09T15-30.md` |

Coverage headline (PowerShell, direct Pester): aggregate 94.98 to 94.99; `BindingRedirectSync.psm1` 100.00 to 100.00; `Repair-PackageManifestConsistency.ps1` 94.14 to 94.17.
