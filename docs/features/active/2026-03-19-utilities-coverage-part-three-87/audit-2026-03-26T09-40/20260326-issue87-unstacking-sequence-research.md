<!-- markdownlint-disable-file -->

# Task Research Notes: issue-87-unstacking-sequence

## Research Executed

### File Analysis

- `artifacts/research/20260326-issue87-branch-isolation-remediation-research.md`
  - The prior research established that issue `#87` cannot be cleanly salvaged on the current branch and recommended a replacement branch from `origin/development`.
- `.git/branch_analysis_issue87.txt`
  - Per-commit path evidence identifies which commits belong to issue `#97`, issue `#96`, pure residual excluded work, and mixed `#87` commits that require file bootstrap.
- `docs/features/active/2026-03-25-getmovediagnostics-null-guard-97/issue.md`
  - Issue `#97` is already fully specified and its acceptance criteria are checked off.
- `docs/features/active/2026-03-25-quickfiler-gui-not-expanding-96/issue.md`
  - Issue `#96` is fully scoped and mapped to a distinct QuickFiler bugfix area.
- `artifacts/pr_context.appendix.txt`
  - Confirms all of these scopes are still outside `origin/development`, so separate remediation branches are still needed.

### Code Search Results

- `issue 97 commit set`
  - `.git/branch_analysis_issue87.txt` shows issue `#97` work is centered on `a19ac86` and `ad4ae95`, with `c448819` as a merge commit that should not be replayed directly onto a fresh branch.
- `issue 96 commit set`
  - `.git/branch_analysis_issue87.txt` shows issue `#96` work is centered on `bd8fc03` and `3b472b2`.
- `residual excluded changes`
  - Pure non-`#87`, non-`#96`, non-`#97` commits still outside `origin/development` include `52742b8`, `4d5f476`, `60408b0`, `16d7d5d`, `0c9a045`, `66220df`, and `ea0206e`.
- `commit containment check`
  - Verified by direct git inspection that none of these candidate split-out commits are yet contained in `origin/development`.

### External Research

- #githubRepo:"not-used (tool unavailable in current environment)"
  - No repository search tool was available; local git evidence and official documentation were sufficient.
- #fetch:https://git-scm.com/docs/git-cherry-pick
  - Cherry-pick is the correct replay primitive for direct commit transplantation onto a fresh branch; merge commits should generally not be replayed directly when the underlying linear commits are available.
- #fetch:https://git-scm.com/docs/git-branch
  - Fresh branches should be cut directly from `origin/development` for each separated PR.
- #fetch:https://git-scm.com/docs/git-rebase
  - After each branch is published, rebasing onto updated `origin/development` is the cleanest way to keep the PR current.
- #fetch:https://docs.github.com/en/pull-requests/collaborating-with-pull-requests/proposing-changes-to-your-work-with-pull-requests/creating-a-pull-request
  - A separate PR per isolated branch is the correct review structure.
- #fetch:https://docs.github.com/en/pull-requests/collaborating-with-pull-requests/proposing-changes-to-your-work-with-pull-requests/keeping-your-pull-request-in-sync-with-the-base-branch
  - GitHub supports keeping a PR in sync either by merge or rebase; rebase is a better fit for these replacement branches because the goal is a linear, auditable unstacking sequence.

### Project Conventions

- Standards referenced: use `origin/development` as the only base for remediation branches; refresh `artifacts/pr_context.summary.txt` and `artifacts/pr_context.appendix.txt` whenever branch composition changes materially.
- Instructions followed: `policy-compliance-order`, `pr-context-artifacts`, `pr-base-branch-merge-base`, and the active remediation plan constraints.

## Key Discoveries

### Project Structure

The most reliable way to repair the mixed branch is to **unstack** it into four replacement branches, in this order:

1. clean branch and PR for issue `#97`
2. clean branch and PR for issue `#96`
3. clean branch and PR for all other excluded non-`#87` work
4. clean branch and PR for issue `#87`

This order matters because it progressively moves unrelated changes back onto `development`, reducing both cherry-pick pressure and bootstrap overhead for the final `#87` branch.

### Implementation Patterns

The replacement strategy works best as a **serial unstacking flow** rather than parallel branch creation:

- branch `#97` from the current `origin/development`
- merge PR `#97`
- refresh from `origin/development`
- branch `#96`
- merge PR `#96`
- refresh from `origin/development`
- branch residual excluded work
- merge residual PR
- refresh from `origin/development`
- create the final clean `#87` branch from that updated base

This ensures the final `#87` branch is built on the smallest possible delta and minimizes repeated conflict resolution.

**Mandatory unachievable objective callout**:
- **It is not technically sound to create the final clean `#87` branch first if the plan is also to preserve the unrelated fixes in separate PRs.** Doing `#87` first would force `#87` either to carry prerequisite non-`#87` code temporarily or to re-resolve the same conflicts later when those prerequisite branches merge. The verified lower-risk sequence is `#97` → `#96` → residual excluded work → `#87`.

### Complete Examples

```powershell
# Ordered unstacking strategy from the current mixed branch

git fetch origin
git switch feature/utilities-coverage-part-three-87
git branch archive/feature-util-coverage-87-mixed-20260326

# ------------------------------------------------------------
# 1. Issue #97 clean branch
# ------------------------------------------------------------
git switch -c bug/getmovediagnostics-null-guard-97-clean origin/development
git cherry-pick a19ac86 ad4ae95
# Do not cherry-pick c448819 (merge commit)
# Refresh PR context, verify only issue-97 files are present, then open PR to development

# ------------------------------------------------------------
# 2. Issue #96 clean branch (after #97 merges)
# ------------------------------------------------------------
git fetch origin
git switch -c bug/quickfiler-gui-not-expanding-96-clean origin/development
git cherry-pick bd8fc03 3b472b2
# Refresh PR context, verify only issue-96 files are present, then open PR to development

# ------------------------------------------------------------
# 3. Residual excluded-work branch (after #96 merges)
# ------------------------------------------------------------
git fetch origin
git switch -c chore/mixed-branch-excluded-work-clean origin/development
git cherry-pick 52742b8 4d5f476 60408b0 16d7d5d 0c9a045 66220df ea0206e

# Bootstrap wanted residual files from mixed commits instead of replaying whole commits
git restore --source ee92dd6 -- QuickFiler/Controllers/QfcHomeController.cs missing-serializable-list.json
git restore --source a8d24b2 -- TaskMaster/TaskMaster.csproj
git restore --source 221e76f -- TaskMaster/Ribbon/RibbonExplorer.xml
git restore --source 4634ac5 -- TaskMaster/AppGlobals/AppAutoFileObjects.cs

# Remove any issue-87 or other feature-folder material before committing residual bootstrap
git restore --source HEAD --staged --worktree -- UtilitiesCS UtilitiesCS.Test docs/features/active/2026-03-19-utilities-coverage-part-three-87 docs/features/active/2026-03-25-getmovediagnostics-null-guard-97 docs/features/active/2026-03-25-quickfiler-gui-not-expanding-96
git add .codex .github QuickFiler QuickFiler.Test TaskMaster UtilitiesSwordfish missing-serializable-list.json
git commit -m "chore(remediation): recover excluded non-issue-87 changes from mixed branch"

# ------------------------------------------------------------
# 4. Final clean issue #87 branch (after residual PR merges)
# ------------------------------------------------------------
git fetch origin
git switch -c feature/utilities-coverage-part-three-87-clean origin/development
git cherry-pick 078fd77 3206593 cce7c5a fff20c7 d65320b 2326734 5f90762 27639bf 5afe10d ee9e4d9 4009d1c 5661a47 4830958 6e5d01d

# Bootstrap only the issue-87 side of the mixed commits
git restore --source ee92dd6 -- UtilitiesCS UtilitiesCS.Test docs/features/active/2026-03-19-utilities-coverage-part-three-87
git restore --source a8d24b2 -- UtilitiesCS UtilitiesCS.Test docs/features/active/2026-03-19-utilities-coverage-part-three-87
git restore --source 5fb07f7 -- UtilitiesCS UtilitiesCS.Test docs/features/active/2026-03-19-utilities-coverage-part-three-87
git restore --source 221e76f -- UtilitiesCS.Test docs/features/active/2026-03-19-utilities-coverage-part-three-87
git add UtilitiesCS UtilitiesCS.Test docs/features/active/2026-03-19-utilities-coverage-part-three-87
git commit -m "chore(issue-87): reconstruct clean coverage branch from mixed history"
```

### API and Schema Documentation

- `git cherry-pick` is appropriate for **linear, single-purpose commits**.
- Merge commits such as `c448819` should be avoided when the underlying logical commits already exist (`a19ac86`, `ad4ae95`), because replaying the linear commits produces a cleaner replacement branch.
- For mixed commits, `git restore --source <sha> -- <path>` is the most precise recovery primitive because it lets the replacement branch pull forward only the intended files from a mixed commit.
- Each replacement branch should become its **own PR** against `development`, not a stacked PR against another temporary branch.

### Configuration Examples

```text
Recommended replacement branches and PRs

bug/getmovediagnostics-null-guard-97-clean
  PR target: development
  PR title: Bug: getmovediagnostics-null-guard (#97)
  Source commits: a19ac86, ad4ae95

bug/quickfiler-gui-not-expanding-96-clean
  PR target: development
  PR title: Bug: quickfiler-gui-not-expanding (#96)
  Source commits: bd8fc03, 3b472b2

chore/mixed-branch-excluded-work-clean
  PR target: development
  PR title: Chore: recover excluded changes split out of feature/utilities-coverage-part-three-87
  Source commits: 52742b8, 4d5f476, 60408b0, 16d7d5d, 0c9a045, 66220df, ea0206e
  Bootstrap files: selected non-87 files from ee92dd6, a8d24b2, 221e76f, 4634ac5

feature/utilities-coverage-part-three-87-clean
  PR target: development
  PR title: Feature: utilities-coverage-part-three (#87)
  Source commits: clean issue-87 list plus issue-87-only bootstrap from mixed commits
```

### Technical Requirements

1. **Issue #97 branch and PR — first**
   - Create `bug/getmovediagnostics-null-guard-97-clean` from `origin/development`.
   - Cherry-pick `a19ac86` and `ad4ae95`.
   - Do not cherry-pick merge commit `c448819`.
   - Before opening the PR, verify the branch diff is limited to:
     - `QuickFiler/Controllers/QfcCollectionController.cs`
     - `QuickFiler/Controllers/QfcHomeController.cs`
     - corresponding `QuickFiler.Test/*` files
     - `docs/features/active/2026-03-25-getmovediagnostics-null-guard-97/`
     - optional issue-97 potential doc if intentionally preserved.
   - Open a dedicated PR to `development`.

2. **Issue #96 branch and PR — second**
   - After `#97` merges, create `bug/quickfiler-gui-not-expanding-96-clean` from the new `origin/development`.
   - Cherry-pick `bd8fc03` and `3b472b2`.
   - Verify the branch diff is limited to:
     - `QuickFiler/Controllers/QfcItemController.cs`
     - corresponding `QuickFiler.Test/*` files
     - `docs/features/active/2026-03-25-quickfiler-gui-not-expanding-96/`
     - optional issue-96 potential doc if intentionally preserved.
   - Open a dedicated PR to `development`.

3. **Residual excluded-changes branch and PR — third**
   - After `#96` merges, create `chore/mixed-branch-excluded-work-clean` from the new `origin/development`.
   - Direct-cherry-pick the pure residual commits:
     - `52742b8` `.codex/.github workflow setup`
     - `4d5f476` `.codex/.github workflow trigger`
     - `60408b0` `UtilitiesSwordfish` sender contract fix
     - `16d7d5d` unrelated `QfcItemController` cancellation fix
     - `0c9a045` unrelated `EfcHomeController` cleanup / metrics fix
     - `66220df` Codex feature-review tooling conversion
     - `ea0206e` `TaskMaster/Ribbon/RibbonExplorer.xml` change
   - Then bootstrap residual non-87 files from mixed commits where needed:
     - from `ee92dd6`: `QuickFiler/Controllers/QfcHomeController.cs`, `missing-serializable-list.json`
     - from `a8d24b2`: `TaskMaster/TaskMaster.csproj`
     - from `221e76f`: latest intended `TaskMaster/Ribbon/RibbonExplorer.xml` if not already superseded by `ea0206e`
     - from `4634ac5`: `TaskMaster/AppGlobals/AppAutoFileObjects.cs` if that fix is still desired outside `#87`
   - Exclude all issue-87, issue-96, and issue-97 folders from this branch before committing the bootstrap.
   - Open a third PR to `development`.

4. **Final clean issue #87 branch — fourth**
   - Only after the first three PRs are merged, create `feature/utilities-coverage-part-three-87-clean` from the updated `origin/development`.
   - Cherry-pick the clean `#87` commits:
     - `078fd77`, `3206593`, `cce7c5a`, `fff20c7`, `d65320b`, `2326734`, `5f90762`, `27639bf`, `5afe10d`, `ee9e4d9`, `4009d1c`, `5661a47`, `4830958`, `6e5d01d`
   - Bootstrap only the `#87` side of mixed commits:
     - `ee92dd6`, `a8d24b2`, `5fb07f7`, `221e76f`
   - Rebuild PR-context artifacts and verify the final branch diff contains only:
     - `UtilitiesCS/**`
     - `UtilitiesCS.Test/**`
     - `docs/features/active/2026-03-19-utilities-coverage-part-three-87/**`
     - any repo-root artifact files that are unquestionably issue-87-specific and intentionally retained.
   - Open the new draft PR for `#87`.

5. **Update strategy once `development` changes**
   - Treat each PR branch independently.
   - If a PR branch is still open when `development` moves:
     - `git fetch origin`
     - `git switch <branch>`
     - `git rebase origin/development`
     - rerun the branch-specific QA / evidence refresh
     - `git push --force-with-lease origin <branch>`
   - Do this for `#97`, `#96`, the residual branch, and later `#87`.
   - Once a PR is merged, delete only the replacement clean branch; keep the original mixed archive branch until all four replacement PRs are safely merged.

6. **PR state guidance**
   - `#97` and `#96` can likely be opened **ready for review** because their issue docs already indicate completed acceptance criteria.
   - The residual branch should be opened as **draft** unless its scope is trivially obvious and already validated.
   - `#87` should remain **draft** until the `UtilitiesCS >= 80%` gate and refreshed review pass.

7. **Rejected alternatives (brief summary)**
   - **Create `#87` first, then peel off `#96`/`#97`:** rejected because the final `#87` branch would either carry non-87 prerequisites temporarily or require another large rebase after the supporting PRs merge.
   - **Put all excluded changes into one branch before isolating `#96` and `#97`:** rejected because `#96` and `#97` are already coherent issue-scoped units and deserve dedicated review surfaces.
   - **Cherry-pick merge commit `c448819`:** rejected because the underlying linear issue-97 commits are available and produce a cleaner replacement branch.

## Recommended Approach

Expand the original recommendation into an explicit **four-stage unstacking program**:

1. Recover issue `#97` to its own clean bugfix branch and PR.
2. Recover issue `#96` to its own clean bugfix branch and PR.
3. Recover all remaining excluded non-`#87` work to a third branch and PR.
4. Only then rebuild issue `#87` as a clean feature branch from the newly-updated `origin/development`.

This sequence uses `development` as the canonical integration spine and progressively drains unrelated history out of the mixed branch before reconstructing `#87`.

## Implementation Guidance

- **Objectives**: turn one contaminated branch into four reviewable branches; preserve issue boundaries; minimize conflicts for the final `#87` remediation.
- **Key Tasks**: split issue `#97`; split issue `#96`; create a residual excluded-work PR; then create final clean issue `#87`; after each merge, recreate the next branch from the latest `origin/development`.
- **Dependencies**: current mixed source branch `feature/utilities-coverage-part-three-87`; commit map in `.git/branch_analysis_issue87.txt`; issue docs for `#96` and `#97`; canonical PR-context artifacts.
- **Success Criteria**: four dedicated PRs exist (or are ready to create) with diffs scoped to their intended issue/chore areas; the final clean `#87` branch is built from an updated `development` that already contains the unrelated work moved out earlier.