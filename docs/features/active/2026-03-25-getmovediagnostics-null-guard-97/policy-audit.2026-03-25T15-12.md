# Policy Audit — getmovediagnostics-null-guard-97 (2026-03-25T15-12)

- **Feature folder:** `docs/features/active/2026-03-25-getmovediagnostics-null-guard-97/`
- **Current branch inspected:** `getmovediagnostics-null-guard-97` @ `66220df0089cc10e6a32f4ed29aa7558f5cc2596`
- **Base branch:** `origin/feature/utilities-coverage-part-three-87` @ `3b472b211b0066000f7b0f6582c5eb977dd2ba69`
- **Comparison source:** `artifacts/pr_context.summary.txt` and `artifacts/pr_context.appendix.txt` as corrected by the user.
- **Supersedes:** the earlier `*.2026-03-25T14-57.md` review set that was written against the wrong base comparison.
- **Work mode source:** `issue.md` declares `- Work Mode: minor-audit`, so `issue.md` is the sole acceptance-criteria source for this audit.
- **Feature folder selection rule:** Used the user-supplied `plan.2026-03-25T12-00.md` in the `#97` feature folder; the branch suffix and `issue.md` both resolve to issue `#97`.
- **Template note:** No repository template matching `docs/features/templates/policy_audit/policy-audit.yyyy-MM-ddTHH-mm.md` was present. This audit uses the repository’s established artifact structure and records the missing template as a process gap.

## Verdict

**NEEDS REVISION — not ready for PR review against `origin/feature/utilities-coverage-part-three-87`.**

The `#97` null-guard implementation is functionally correct and the issue acceptance criteria are satisfied, but the corrected upstream diff still contains unrelated `.codex`/tooling content and the minor-audit evidence set is not canonically synchronized on disk.

## Audit summary

| Area | Status | Result | Evidence |
|---|---|---|---|
| Policy reading order | ✅ | PASS | Shared review policies and required skills were loaded before review, including `.github/copilot-instructions.md`, `general-code-change.instructions.md`, `general-unit-test.instructions.md`, `csharp-code-change.instructions.md`, and `csharp-unit-test.instructions.md`. |
| Work mode / AC source selection | ✅ | PASS | `issue.md` line 9 declares `minor-audit`; this audit used `issue.md` only. |
| Minor-audit integrity: `issue.md` exists | ✅ | PASS | `docs/features/active/2026-03-25-getmovediagnostics-null-guard-97/issue.md` exists. |
| Minor-audit integrity: `spec.md` absent | ✅ | PASS | No `spec.md` exists in the feature folder. |
| Minor-audit integrity: `user-story.md` absent | ✅ | PASS | No `user-story.md` exists in the feature folder. |
| Minor-audit integrity: Phase 0 policy-read artifact | ✅ | PASS | `evidence/baseline/phase0-instructions-read.md` exists and includes `Timestamp:` and `Policy Order:`. |
| Minor-audit integrity: baseline command artifacts | ✅ | PASS | `baseline-format.md`, `baseline-lint.md`, `baseline-nullable.md`, `baseline-test-filter.md`, and `baseline-coverage.md` all exist and include `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:`. |
| Minor-audit integrity: plan checklist vs artifact state | ⚠️ | PARTIAL | The active plan `plan.2026-03-25T12-00.md` still leaves `P2-T3` through `P2-T6` unchecked, while `issue.md` has all AC items checked and the feature folder still lacks `evidence/qa-gates/qc-nullable.md`, `qc-regression-tests.md`, and `qc-coverage.md`. The corrected appendix also still references the legacy committed `plan.md`, while the workspace now contains `plan.2026-03-25T12-00.md` and an unstaged deletion of `plan.md`. |
| PR-context baseline alignment | ✅ | PASS | `artifacts/pr_context.summary.txt` and `artifacts/pr_context.appendix.txt` now agree on the intended base branch: `origin/feature/utilities-coverage-part-three-87`. |
| PR-context internal consistency | ⚠️ | PARTIAL | The corrected summary still claims `Core logic changes: 0 files` and `Docs/templates/agents/tooling: 13 files`, while the corrected appendix lists actual code changes in `QuickFiler/Controllers/QfcCollectionController.cs`, `QuickFiler/Controllers/QfcHomeController.cs`, `QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs`, `QuickFiler.Test/Controllers/QfcHomeControllerTests.cs`, and `QuickFiler.Test/QuickFiler.Test.csproj`. |
| Baseline diff focus relative to intended upstream | ❌ | FAIL | Even against the corrected upstream base, the diff includes unrelated additions such as `.codex/agents/atomic-executor.toml`, `.codex/skills/feature-review/SKILL.md`, and `.github/skills.zip`, which are outside the scope of issue `#97`. |
| C# formatter validation | ✅ | PASS | Review-time `dotnet tool run csharpier format .` exited `0` and did not introduce additional working-tree changes. |
| Analyzer validation | ✅ | PASS | Review-time analyzer build exited `0` via `Invoke-VSBuild.ps1 -EnableNETAnalyzers -EnforceCodeStyleInBuild`. |
| Nullable validation | ✅ | PASS | Review-time nullable build exited `0` via `Invoke-VSBuild.ps1 -EnableNullable -TreatWarningsAsErrors`. |
| Test validation | ✅ | PASS | The repository coverage task completed successfully in the current session and refreshed `coverage/coverage.cobertura.xml`; the feature-specific regression code is covered in the resulting report. |
| Coverage metrics availability | ✅ | PASS | Numeric baseline and post-review coverage values are available from `evidence/baseline/baseline-coverage.md` and `coverage/coverage.cobertura.xml`. |

## Coverage metrics required by policy

| Metric | Value | Evidence |
|---|---:|---|
| Baseline QuickFiler test-assembly line coverage | 86.29% | `evidence/baseline/baseline-coverage.md` |
| Baseline QuickFiler production-module line coverage | 19.44% | `evidence/baseline/baseline-coverage.md` |
| Post-review overall Cobertura line coverage | 61.02% | `coverage/coverage.cobertura.xml` root `line-rate="0.6102342032626481"` |
| Post-review `QuickFiler.Test` package line coverage | 87.17% | `coverage/coverage.cobertura.xml` package `QuickFiler.Test` `line-rate="0.871654501216545"` |
| Post-review `QfcCollectionController.cs` file line coverage | 4.27% | `coverage/coverage.cobertura.xml` class `QuickFiler.Controllers.QfcCollectionController` `line-rate="0.042735042735042736"` |
| Post-review `QfcHomeController.cs` file line coverage | 78.71% | `coverage/coverage.cobertura.xml` class `QuickFiler.Controllers.QfcHomeController` `line-rate="0.7871287128712872"` |
| Post-review `QfcCollectionControllerTests.cs` line coverage | 91.07% | `coverage/coverage.cobertura.xml` class `QuickFiler.Controllers.Tests.QfcCollectionControllerTests` `line-rate="0.9107142857142857"` |
| Post-review `QfcHomeControllerTests.cs` line coverage | 100.00% | `coverage/coverage.cobertura.xml` class `QuickFiler.Controllers.Tests.QfcHomeControllerTests` `line-rate="1"` |

## Key evidence

### Corrected PR-context evidence

- `artifacts/pr_context.summary.txt` lines 15–19: corrected base/head range
- `artifacts/pr_context.summary.txt` lines 97–110: summary-level changed-file classification
- `artifacts/pr_context.appendix.txt` lines 240–244: corrected base/head range
- `artifacts/pr_context.appendix.txt` lines 263–288: actual changed files in range
- `artifacts/pr_context.appendix.txt` lines 324–336: diff-stat evidence for unrelated `.codex` plus relevant QuickFiler code files

### Canonical feature evidence

- `docs/features/active/2026-03-25-getmovediagnostics-null-guard-97/issue.md`
- `docs/features/active/2026-03-25-getmovediagnostics-null-guard-97/plan.2026-03-25T12-00.md`
- `docs/features/active/2026-03-25-getmovediagnostics-null-guard-97/evidence/baseline/phase0-instructions-read.md`
- `docs/features/active/2026-03-25-getmovediagnostics-null-guard-97/evidence/baseline/baseline-format.md`
- `docs/features/active/2026-03-25-getmovediagnostics-null-guard-97/evidence/baseline/baseline-lint.md`
- `docs/features/active/2026-03-25-getmovediagnostics-null-guard-97/evidence/baseline/baseline-nullable.md`
- `docs/features/active/2026-03-25-getmovediagnostics-null-guard-97/evidence/baseline/baseline-test-filter.md`
- `docs/features/active/2026-03-25-getmovediagnostics-null-guard-97/evidence/baseline/baseline-coverage.md`
- `docs/features/active/2026-03-25-getmovediagnostics-null-guard-97/evidence/qa-gates/qc-format.md`
- `docs/features/active/2026-03-25-getmovediagnostics-null-guard-97/evidence/qa-gates/qc-lint.md`
- `docs/features/active/2026-03-25-getmovediagnostics-null-guard-97/evidence/regression-testing/fail-before-evidence.2026-03-25T00-00.md`
- `coverage/coverage.cobertura.xml`

## Commands run for this review

1. `dotnet tool run csharpier format .`
2. `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform "Any CPU" -EnableNETAnalyzers -EnforceCodeStyleInBuild`
3. `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform "Any CPU" -EnableNullable -TreatWarningsAsErrors`
4. `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug`

## Recommendation

**Needs revision before PR review against `origin/feature/utilities-coverage-part-three-87`.**

The implementation for issue `#97` is acceptable, but the branch should be cleaned so that the diff contains only the bug-fix scope, the PR-context summary matches the appendix, and the canonical Phase 2 QA artifacts are present before a PASS-style minor-audit result is issued.
