<!-- markdownlint-disable-file -->

# Task Research Notes: issue-87-branch-isolation-remediation

## Research Executed

### File Analysis

- `docs/features/active/2026-03-19-utilities-coverage-part-three-87/remediation-plan.2026-03-26T09-40.md`
  - Phase 1 requires a branch diff relative to `origin/development` that contains only `UtilitiesCS`, `UtilitiesCS.Test`, and `docs/features/active/2026-03-19-utilities-coverage-part-three-87/`, plus refreshed PR-context artifacts.
- `docs/features/active/2026-03-19-utilities-coverage-part-three-87/remediation-inputs.2026-03-26T09-40.md`
  - REQ-1 explicitly forbids unrelated `.codex`, `.github`, `QuickFiler`, `TaskMaster`, and other feature-folder changes in the `origin/development...HEAD` diff.
- `artifacts/pr_context.summary.txt`
  - Refreshed review context resolved base `origin/development`, merge-base `27f999cfe4a34cd6db03280f85ab13518d84c743`, and recorded issue `#87` as the intended outcome.
- `artifacts/pr_context.appendix.txt`
  - Verified the current branch is contaminated relative to `origin/development`: `220 files changed`, with top-level changes in `.codex`, `.github`, `QuickFiler`, `QuickFiler.Test`, `TaskMaster`, `UtilitiesSwordfish`, `UtilitiesCS`, `UtilitiesCS.Test`, and multiple feature folders.
- `.git/branch_analysis_issue87.txt`
  - Per-commit path analysis proved the branch history is interleaved: some commits are pure `#87` coverage commits, while others mix `#87` work with unrelated paths or belong entirely to unrelated QuickFiler / tooling work.

### Code Search Results

- `origin/development...HEAD diff scope`
  - `artifacts/pr_context.appendix.txt` reports `220 files changed`, with top-level counts: `UtilitiesCS.Test 96`, `docs 74`, `UtilitiesCS 20`, `.codex 9`, `QuickFiler.Test 5`, `QuickFiler 4`, `.github 3`, `TaskMaster 3`, `UtilitiesSwordfish 1`.
- `issue-87 feature-folder scope`
  - The current branch already contains the intended `#87` feature docs and evidence under `docs/features/active/2026-03-19-utilities-coverage-part-three-87/`, but they are accompanied by unrelated feature folders `#96` and `#97` in the same branch range.
- `commit ownership by top-level path`
  - Per-commit evidence from `.git/branch_analysis_issue87.txt` identified clean candidates (`078fd77`, `3206593`, `cce7c5a`, `fff20c7`, `d65320b`, `2326734`, `5f90762`, `27639bf`, `5afe10d`, `ee9e4d9`, `4009d1c`, `5661a47`, `4830958`, `6e5d01d`) and mixed / contaminated commits requiring exclusion or file bootstrap (`ee92dd6`, `a8d24b2`, `5fb07f7`, `221e76f`, `4634ac5`).

### External Research

- #githubRepo:"not-used (tool unavailable in current environment)"
  - No GitHub repository search tool was available in this session; local git evidence plus official Git and GitHub documentation were sufficient.
- #fetch:https://git-scm.com/docs/git-cherry-pick
  - Verified that `git cherry-pick` requires a clean worktree, supports `-n/--no-commit` for partial application, and supports `--abort`, `--continue`, and `--skip` for conflict handling.
- #fetch:https://git-scm.com/docs/git-branch
  - Verified that a new branch can be created directly from `origin/development`, and that branch tracking/upstream can be set explicitly when establishing the clean remediation branch.
- #fetch:https://git-scm.com/docs/git-rebase
  - Verified that rebasing replays branch commits on top of a new base; `git rebase origin/development` is the correct mechanism to refresh a clean remediation branch after `development` moves.
- #fetch:https://docs.github.com/en/pull-requests/collaborating-with-pull-requests/proposing-changes-to-your-work-with-pull-requests/about-pull-requests
  - Verified that PR diffs use a merge-base relationship and that PR review is centered on the Files changed / Commits / Checks views.
- #fetch:https://docs.github.com/en/pull-requests/collaborating-with-pull-requests/proposing-changes-to-your-work-with-pull-requests/creating-a-pull-request
  - Verified the standard PR creation flow and the option to open a draft PR while remediation is still in progress.
- #fetch:https://docs.github.com/en/pull-requests/collaborating-with-pull-requests/proposing-changes-to-your-work-with-pull-requests/keeping-your-pull-request-in-sync-with-the-base-branch
  - Verified that a PR branch can be updated from the base branch either with a merge commit or by rebasing; rebasing preserves a linear history and is the cleaner fit for an isolated remediation branch.

### Project Conventions

- Standards referenced: `origin/development` is the required base for remediation; `artifacts/pr_context.summary.txt` and `artifacts/pr_context.appendix.txt` are the canonical PR-context artifacts; issue `#87` remediation must stay limited to `UtilitiesCS`, `UtilitiesCS.Test`, and `docs/features/active/2026-03-19-utilities-coverage-part-three-87/`.
- Instructions followed: `policy-compliance-order`, `pr-base-branch-merge-base`, `pr-context-artifacts`, plus the active remediation plan and remediation inputs.

## Key Discoveries

### Project Structure

The current published branch `feature/utilities-coverage-part-three-87` is not salvageable as a clean PR by ordinary file deletion alone because the contamination is in branch history, not just in the working tree. The branch tip is `6e5d01d617f4811ae2b9c196783051e91df13589`, and the branch range from merge-base `27f999cfe4a34cd6db03280f85ab13518d84c743` contains 36 commits. Those commits mix three categories:

1. `#87`-relevant `UtilitiesCS` / `UtilitiesCS.Test` / feature-doc work.
2. Mixed commits that combine `#87` work with unrelated paths such as `TaskMaster/TaskMaster.csproj`, `TaskMaster/Ribbon/RibbonExplorer.xml`, `QuickFiler/*`, `.codex/*`, and historical feature docs.
3. Entirely unrelated commits for issues `#96`, `#97`, Codex tooling, QuickFiler, and `UtilitiesSwordfish`.

That means the Phase 1 goal is best achieved by creating a **new clean branch from `origin/development`**, replaying only the relevant history, and leaving the current branch as a source/archive branch.

### Implementation Patterns

The evidence supports a two-mechanism recovery pattern:

- **Mechanism A — direct cherry-pick for clean commits**
  - Use this only for commits whose changed paths are entirely within `UtilitiesCS`, `UtilitiesCS.Test`, and `docs/features/active/2026-03-19-utilities-coverage-part-three-87/`.
- **Mechanism B — file bootstrap for mixed commits**
  - For commits that mix wanted and unwanted paths, do not cherry-pick the whole commit. Instead, create the clean branch from `origin/development`, then bootstrap only the specific wanted files from the current branch or from the specific mixed commit SHA, and commit those file selections as new focused commits.

This is lower risk than interactive history surgery on the already-published mixed branch, because it avoids rewriting the contaminated branch and makes the new PR reviewable from a clean base.

**Mandatory unachievable objective callout**:
- **It is not realistically achievable to satisfy Phase 1 on the current published branch without history rewriting or branch replacement.** The contamination is commit-level and already pushed to `origin/feature/utilities-coverage-part-three-87`. A clean PR scope requires a new branch from `origin/development` or a full interactive rebase/rewrite of the published branch. Based on the evidence, the new clean branch is the safer and clearer route.

### Complete Examples

```powershell
# Verified clean-branch recovery sequence for issue #87

# 0. Preserve the current mixed branch as an immutable source/reference
git fetch origin
git switch feature/utilities-coverage-part-three-87
git branch archive/feature-util-coverage-87-mixed-20260326

# 1. Start the clean remediation branch directly from origin/development
git switch -c feature/utilities-coverage-part-three-87-clean origin/development

# 2. Cherry-pick clean issue-87 commits in chronological order
git cherry-pick 078fd77 3206593 cce7c5a fff20c7 d65320b 2326734 5f90762 27639bf 5afe10d ee9e4d9 4009d1c 5661a47 4830958 6e5d01d

# 3. Bootstrap wanted files from mixed commits instead of cherry-picking whole commits
git restore --source ee92dd6 -- UtilitiesCS UtilitiesCS.Test docs/features/active/2026-03-19-utilities-coverage-part-three-87
git restore --source a8d24b2 -- UtilitiesCS UtilitiesCS.Test docs/features/active/2026-03-19-utilities-coverage-part-three-87
git restore --source 5fb07f7 -- UtilitiesCS UtilitiesCS.Test docs/features/active/2026-03-19-utilities-coverage-part-three-87
git restore --source 221e76f -- UtilitiesCS.Test docs/features/active/2026-03-19-utilities-coverage-part-three-87

# 4. Inspect and remove any bootstrapped unrelated files before committing
git status --short
git restore --source HEAD --staged --worktree -- .codex .github QuickFiler QuickFiler.Test TaskMaster UtilitiesSwordfish docs/features/active/2026-03-25-getmovediagnostics-null-guard-97 docs/features/active/2026-03-25-quickfiler-gui-not-expanding-96 docs/features/active/2026-03-13-utilities-coverage-65 change-plan.md missing-serializable-list.json
git add UtilitiesCS UtilitiesCS.Test docs/features/active/2026-03-19-utilities-coverage-part-three-87
git commit -m "chore(issue-87): salvage mixed UtilitiesCS coverage changes onto clean branch"

# 5. Refresh PR-context artifacts from the clean branch against origin/development
# (Use the repo collector if available; otherwise direct git inspection)

# 6. Open a draft PR from feature/utilities-coverage-part-three-87-clean to development
# Keep it draft until Phase 2 coverage reaches >= 80%.

# 7. When development moves forward later, refresh the clean branch linearly
git fetch origin
git switch feature/utilities-coverage-part-three-87-clean
git rebase origin/development

# 8. If the branch is already published, update the remote safely
git push --force-with-lease origin feature/utilities-coverage-part-three-87-clean
```

### API and Schema Documentation

- `git cherry-pick --no-commit` can apply a commit’s changes without committing them, but the cleaner repo-specific recovery here is bootstrapping only wanted files from mixed SHAs rather than partially cherry-picking unwanted paths and cleaning them afterward.
- `git rebase origin/development` replays the clean branch commits on top of the latest `development`; this is the preferred update strategy after the base branch changes because the target branch should remain linear and focused.
- GitHub PRs can be opened as **draft PRs**, which is the correct state while coverage is still below threshold.
- GitHub distinguishes compare diffs and PR diffs by merge-base timing, so after branch recreation or rebasing, PR-context artifacts must be regenerated to avoid stale scope evidence.

### Configuration Examples

```text
Recommended branch roles

archive/feature-util-coverage-87-mixed-20260326
  Purpose: immutable backup/reference for the currently published mixed branch.

feature/utilities-coverage-part-three-87-clean
  Base: origin/development
  Purpose: only issue #87 paths; the branch used for the new draft PR and all further remediation work.

origin/development
  Purpose: sole comparison base for Phase 1, Phase 3 review refresh, and PR creation.
```

### Technical Requirements

1. **Do not continue remediation on the current mixed branch.**
   - Its history already includes unrelated commits for `.codex`, `.github`, QuickFiler, TaskMaster, issue `#96`, and issue `#97`.
2. **Create a new clean branch from `origin/development`.**
   - This is the only reliable way to satisfy the Phase 1 acceptance criterion without carrying unrelated history into the PR.
3. **Use two commit-selection buckets.**
   - **Safe direct cherry-picks:** `078fd77`, `3206593`, `cce7c5a`, `fff20c7`, `d65320b`, `2326734`, `5f90762`, `27639bf`, `5afe10d`, `ee9e4d9`, `4009d1c`, `5661a47`, `4830958`, `6e5d01d`.
   - **Mixed commits requiring file bootstrap, not full cherry-pick:** `ee92dd6`, `a8d24b2`, `5fb07f7`, `221e76f`; optionally `4634ac5` only if the clean branch later proves that `SerializableList` still needs that exact fix.
4. **Explicitly exclude these commits / scopes from the clean branch.**
   - `52742b8`, `4d5f476`, `60408b0`, `16d7d5d`, `0c9a045`, `ea0206e`, `bd8fc03`, `a19ac86`, `66220df`, `ad4ae95`, `c448819`, plus all paths under `.codex`, `.github`, `QuickFiler`, `QuickFiler.Test`, `TaskMaster`, `UtilitiesSwordfish`, and other active feature folders unless a later clean-branch verification proves a specific file is required and can be reintroduced in-scope.
5. **PR strategy.**
   - Open a **new draft PR** from `feature/utilities-coverage-part-three-87-clean` to `development`.
   - Use `Fixes #87` only when the branch has passed REQ-2 and REQ-4; before that, describe it as remediation for `#87` but keep the PR draft.
   - Refresh `artifacts/pr_context.summary.txt` and `artifacts/pr_context.appendix.txt` from the clean branch immediately before PR creation and again after any rebase.
6. **Update strategy after `development` changes.**
   - Update only the clean branch: `git fetch origin` then `git rebase origin/development`.
   - If the PR is already open, force-push with `--force-with-lease` and rerun the PR-context refresh plus the required QA evidence refresh.
   - Do **not** try to keep the original mixed branch current. After the clean PR merges, either leave the original branch archived or delete it; if more remediation work is needed, create another fresh descendant from the updated `development`.
7. **Rejected alternatives (brief summary).**
   - **In-place file deletion on the mixed branch:** rejected because PR scope is determined by branch history, not just the final worktree.
   - **Interactive rebase of the published mixed branch:** possible, but higher-risk because the branch is already pushed and interleaves unrelated issue work; a new clean branch is simpler to audit and safer to review.
   - **Single large partial cherry-pick sweep from mixed commits:** rejected in favor of targeted file bootstrap because the mixed commits touch unrelated top-level paths and other feature folders.

## Recommended Approach

Use a **clean replacement branch** off `origin/development`, not the current mixed branch, and rebuild the intended `#87` history with a hybrid of direct cherry-picks and targeted file bootstrap.

Recommended sequence:

1. Freeze the current branch as an archive/reference branch.
2. Create `feature/utilities-coverage-part-three-87-clean` from `origin/development`.
3. Replay the safe, in-scope `UtilitiesCS` / `UtilitiesCS.Test` / `#87` docs commits directly in chronological order.
4. For the mixed commits, bootstrap only the wanted files from the mixed SHA or from the current branch tip into the clean branch, then commit those as fresh focused commits.
5. Refresh PR-context artifacts and verify the new diff contains only `UtilitiesCS`, `UtilitiesCS.Test`, and `docs/features/active/2026-03-19-utilities-coverage-part-three-87/`.
6. Open a **draft PR** from the clean branch to `development`.
7. Continue Phase 2 coverage remediation only on the clean branch.
8. If `development` advances before merge, rebase the clean branch on `origin/development`, force-push with lease, and refresh PR-context + QA evidence again.

This approach minimizes history surgery, isolates review scope, preserves the current branch as forensic backup, and gives the cleanest path to satisfying P1-T1 through P1-T3.

## Implementation Guidance

- **Objectives**: isolate issue `#87` from unrelated branch history; produce a PR whose diff is limited to `UtilitiesCS`, `UtilitiesCS.Test`, and the `#87` feature folder; keep a safe strategy for subsequent rebases onto `development`.
- **Key Tasks**: archive the mixed branch; create a clean branch from `origin/development`; cherry-pick pure issue-87 commits; bootstrap selected files from mixed commits; refresh PR-context artifacts; open a draft PR; continue remediation only on the clean branch; rebase that clean branch whenever `development` changes.
- **Dependencies**: current source branch `feature/utilities-coverage-part-three-87`; base branch `origin/development`; canonical PR-context artifacts `artifacts/pr_context.summary.txt` and `artifacts/pr_context.appendix.txt`; per-commit evidence in `.git/branch_analysis_issue87.txt`.
- **Success Criteria**: refreshed appendix shows no `.codex`, `.github`, `QuickFiler`, `QuickFiler.Test`, `TaskMaster`, `UtilitiesSwordfish`, or unrelated feature folders; the new PR is opened from the clean branch to `development`; future updates happen by rebasing the clean branch on `origin/development`, not by extending the original mixed branch.