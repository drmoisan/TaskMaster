# Policy Audit — utilities-coverage-part-three-87 (2026-03-26T09-40)

- **Feature folder:** `docs/features/active/2026-03-19-utilities-coverage-part-three-87/`
- **Current branch inspected:** `feature/utilities-coverage-part-three-87`
- **Base branch:** `development`
- **Work mode source:** `issue.md` declares `- Work Mode: full-feature`, so acceptance criteria were taken from `spec.md` and `user-story.md`.
- **Feature folder selection rule:** Used the user-specified feature folder `docs/features/active/2026-03-19-utilities-coverage-part-three-87/`; it matches issue `#87` and the scoped feature docs changed in the refreshed PR-context bundle.
- **PR context refresh note:** The canonical `artifacts/pr_context.summary.txt` / `artifacts/pr_context.appendix.txt` bundle was stale for another branch at the start of review and was refreshed during this review using direct git-based evidence because the VS Code `drmCopilotExtension.collectPrContext` command surface was unavailable in this tool environment.

## Verdict

**FAIL — Needs revision before PR / merge review.**

The branch contains valid QA evidence for the `#87` coverage work, but the branch is not isolated relative to `development` and the primary coverage acceptance criterion remains unmet. The final QA loop passes technically, yet `UtilitiesCS` coverage is still below the documented threshold and the refreshed diff includes unrelated `.codex`, `QuickFiler`, `TaskMaster`, and other feature-folder changes.

## Audit summary

| Area | Status | Result | Evidence |
|---|---|---|---|
| Policy reading order | ✅ | PASS | Reviewed `.github/copilot-instructions.md`, `general-code-change.instructions.md`, `general-unit-test.instructions.md`, `csharp-code-change.instructions.md`, and `csharp-unit-test.instructions.md` before finalizing the audit. |
| Work mode / AC source selection | ✅ | PASS | `issue.md` declares `full-feature`; review used `spec.md` + `user-story.md` as the authoritative AC sources. |
| PR context artifact freshness | ✅ | PASS | `artifacts/pr_context.summary.txt` and `artifacts/pr_context.appendix.txt` were refreshed for `feature/utilities-coverage-part-three-87` vs `origin/development`. |
| Feature folder selection | ✅ | PASS | The requested folder matches issue `#87` and the refreshed PR-context scoping-doc list. |
| Branch diff isolation relative to base | ❌ | FAIL | Refreshed `artifacts/pr_context.appendix.txt` shows `220 files changed` relative to `development`, including unrelated `.codex`, `.github`, `QuickFiler`, `TaskMaster`, and other active feature folders. |
| C# toolchain evidence | ✅ | PASS | `final-qc-format.md`, `final-qc-analyzers.md`, `final-qc-nullable.md`, and `final-qc-test-coverage.md` all record `EXIT_CODE: 0`. Live review-time reruns also succeeded. |
| Coverage gate required by scoped feature docs | ❌ | FAIL | `final-qc-test-coverage.md` records `UtilitiesCS line coverage: 69.81%`, and `v1/evidence/qa-gates/final-coverage-verification.md` records `EXIT_CODE: 1` with `94` non-skip files below `80%`. |
| Acceptance-criteria tracking in source files | ⚠️ | PARTIAL | This review checked off the six verified PASS criteria in `user-story.md` and corresponding `Definition of Done` items in `spec.md`; the primary `>= 80%` coverage criterion remains unchecked. |
| Feature docs status alignment | ⚠️ | PARTIAL | `spec.md` still shows `Status: In Progress`, which accurately reflects the unresolved coverage criterion, but the feature is not ready for completion or merge. |

## Key evidence

### Canonical baseline and feature evidence

- `artifacts/pr_context.summary.txt`
- `artifacts/pr_context.appendix.txt`
- `docs/features/active/2026-03-19-utilities-coverage-part-three-87/issue.md`
- `docs/features/active/2026-03-19-utilities-coverage-part-three-87/spec.md`
- `docs/features/active/2026-03-19-utilities-coverage-part-three-87/user-story.md`
- `docs/features/active/2026-03-19-utilities-coverage-part-three-87/plan.2026-03-22T21-00.md`
- `docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/baseline/phase0-instructions-read.md`
- `docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/baseline/baseline-test-coverage.md`
- `docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/qa-gates/final-qc-format.md`
- `docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/qa-gates/final-qc-analyzers.md`
- `docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/qa-gates/final-qc-nullable.md`
- `docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/qa-gates/final-qc-test-coverage.md`
- `docs/features/active/2026-03-19-utilities-coverage-part-three-87/v1/evidence/qa-gates/final-coverage-verification.md`
- `docs/features/active/2026-03-19-utilities-coverage-part-three-87/v1/evidence/other/skip-candidates.md`

### Policy-relevant findings

- **Branch packaging failure:** `artifacts/pr_context.appendix.txt` records unrelated changes under `.codex/`, `.github/`, `QuickFiler/`, `QuickFiler.Test/`, `TaskMaster/`, and other active feature folders.
- **Coverage failure:** `final-qc-test-coverage.md` records `UtilitiesCS line coverage: 69.81%`, and `final-coverage-verification.md` records `Per-file >=80% result (excluding documented skip candidates): FAIL`.
- **Plan evidence reflects the failure:** `plan.2026-03-22T21-00.md` task `P90-T6` is checked but contains a follow-up note explicitly admitting the measured coverage was below threshold.

## Appendix A — commands run during this review

1. `git rev-parse --abbrev-ref HEAD`
2. `git rev-parse --verify HEAD`
3. `git rev-parse --verify origin/development`
4. `git merge-base HEAD origin/development`
5. `git diff --name-status <merge-base> HEAD`
6. `git diff --shortstat <merge-base> HEAD`
7. `dotnet tool run csharpier format .`
8. `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform 'Any CPU' -EnableNETAnalyzers -EnforceCodeStyleInBuild`
9. `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform 'Any CPU' -EnableNullable -TreatWarningsAsErrors`
10. `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug`

## Appendix B — command outcomes

- Formatter: exited `0`; no git-visible working-tree changes after the run.
- Analyzer build: exited `0`; build succeeded.
- Nullable build: exited `0`; build succeeded.
- Coverage-enabled test run: canonical feature artifact records `EXIT_CODE: 0`, `Total tests: 3,409`, `Passed: 3,407`, `Failed: 0`, `Skipped: 2`, and `UtilitiesCS line coverage: 69.81%`.
- Refreshed branch diff: `220 files changed, 23805 insertions(+), 4013 deletions(-)` relative to `development`.

## Recommendation

**Do not open or merge this branch as-is.** Rebase or cherry-pick the intended `#87` work onto a clean branch from `development`, then continue the remaining coverage uplift until the documented `>= 80%` UtilitiesCS threshold is actually reached and the refreshed diff contains only the `#87` scope.