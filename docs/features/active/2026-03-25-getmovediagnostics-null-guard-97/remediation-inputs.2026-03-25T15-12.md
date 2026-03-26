# Remediation Inputs — getmovediagnostics-null-guard-97 (2026-03-25T15-12)

## Required fixes

1. **Remove or split unrelated `.codex` / tooling changes from the `#97` branch diff**
   - **Files / locations:**
     - `.codex/agents/atomic-executor.toml`
     - `.codex/agents/atomic-planner.toml`
     - `.codex/agents/feature-reviewer.toml`
     - `.codex/prompts/feature-review-remediate.md`
     - `.codex/skills/atomic-executor/SKILL.md`
     - `.codex/skills/atomic-planner/SKILL.md`
     - `.codex/skills/feature-review/SKILL.md`
     - `.github/skills.zip`
   - **Expected behavior:** The branch diff relative to `origin/feature/utilities-coverage-part-three-87` must contain only the files needed to deliver issue `#97`.
   - **Acceptance criteria:**
     - `artifacts/pr_context.appendix.txt` no longer lists the `.codex/*` files or `.github/skills.zip` in the corrected `#97` diff, or those changes are explicitly moved to another branch/stacked child.
   - **Verification commands / tasks:**
     - Refresh `artifacts/pr_context.summary.txt` and `artifacts/pr_context.appendix.txt` against `origin/feature/utilities-coverage-part-three-87`.
     - Confirm `git diff --name-status origin/feature/utilities-coverage-part-three-87...HEAD` only shows the `QuickFiler`, `QuickFiler.Test`, and `docs/features/active/2026-03-25-getmovediagnostics-null-guard-97/` files required for issue `#97`.

2. **Repair the corrected PR-context summary so it matches the corrected appendix**
   - **Files / locations:**
     - `artifacts/pr_context.summary.txt`
     - `artifacts/pr_context.appendix.txt`
   - **Expected behavior:** The summary must describe the same branch range and changed-file categories that the appendix reports.
   - **Acceptance criteria:**
     - The summary no longer reports `Core logic changes: 0 files` when the appendix lists `QuickFiler/Controllers/QfcCollectionController.cs`, `QuickFiler/Controllers/QfcHomeController.cs`, `QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs`, `QuickFiler.Test/Controllers/QfcHomeControllerTests.cs`, and `QuickFiler.Test/QuickFiler.Test.csproj`.
   - **Verification commands / tasks:**
     - Regenerate the PR-context bundle using the correct upstream branch.
     - Manually compare the summary changed-file overview against appendix `Changed files (name-status)` before finalizing the review.

3. **Synchronize the active feature plan filename, checklist state, and canonical Phase 2 evidence files**
   - **Files / locations:**
     - `docs/features/active/2026-03-25-getmovediagnostics-null-guard-97/plan.2026-03-25T12-00.md`
     - `docs/features/active/2026-03-25-getmovediagnostics-null-guard-97/issue.md`
     - `docs/features/active/2026-03-25-getmovediagnostics-null-guard-97/evidence/qa-gates/qc-nullable.md`
     - `docs/features/active/2026-03-25-getmovediagnostics-null-guard-97/evidence/qa-gates/qc-regression-tests.md`
     - `docs/features/active/2026-03-25-getmovediagnostics-null-guard-97/evidence/qa-gates/qc-coverage.md`
   - **Expected behavior:** The feature folder must contain the canonical QA artifacts required by the approved plan, and the plan checklist must match those artifacts on disk.
   - **Acceptance criteria:**
     - `P2-T3`, `P2-T4`, `P2-T5`, and `P2-T6` are only checked after their corresponding evidence files exist and include `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:`.
     - The active plan filename used by the feature folder is consistent with the file referenced by the PR-context artifacts and the current workspace state.
     - The baseline sync task checks off any already-delivered plan items that remain unchecked.
   - **Verification commands / tasks:**
     - Re-run the nullable build, targeted regression test command, and full QuickFiler coverage command as needed to create the missing evidence files.
     - Reconcile `plan.md` vs `plan.2026-03-25T12-00.md` and then refresh PR context.

4. **Re-run the feature review after scope and evidence cleanup**
   - **Files / locations:**
     - `docs/features/active/2026-03-25-getmovediagnostics-null-guard-97/policy-audit.*.md`
     - `docs/features/active/2026-03-25-getmovediagnostics-null-guard-97/code-review.*.md`
     - `docs/features/active/2026-03-25-getmovediagnostics-null-guard-97/feature-audit.*.md`
   - **Expected behavior:** A refreshed review should be able to issue a PASS-style result once the corrected upstream diff is focused and the canonical evidence set is complete.
   - **Acceptance criteria:**
     - The refreshed review no longer reports unrelated `.codex` / tooling files in the `#97` diff.
     - The refreshed review no longer reports summary/appendix inconsistency.
     - The refreshed review no longer reports missing canonical QA evidence for the active plan.
   - **Verification commands / tasks:**
     - Re-run the feature review workflow using the corrected upstream base `origin/feature/utilities-coverage-part-three-87`.

## Unmet acceptance criteria

There are **no unmet issue acceptance criteria** in `issue.md`. All six issue-specific bug-fix criteria remain satisfied.

Remediation is still required because PR-readiness blockers remain outside the issue AC list:
- unrelated `.codex` / tooling content in the corrected upstream diff,
- a corrected PR-context summary that still does not match the appendix,
- and canonical feature-plan / evidence synchronization drift.

## Do not do

- Do not revert to a `main`-based comparison for this branch.
- Do not expand scope beyond issue `#97` while remediating review blockers.
- Do not weaken repo policy requirements to accommodate missing evidence.
- Do not mark plan tasks complete without the corresponding canonical evidence files on disk.
- Do not silently change the acceptance-criteria text in `issue.md`.
- Do not keep unrelated `.codex` or tooling changes in the final `#97` diff if they are not part of this bug fix.
