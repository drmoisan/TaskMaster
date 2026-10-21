# [P8-T5] Terminal commit and clean-tree proof

- Issue: #792
- Timestamp: 2026-09-18T06-34
- Command: `git add -- docs/features/active/2026-09-06-breadcrumb-webview2-init-fails-resource-not-in-correct-state-792` then `git commit -m "chore(792): manual verification evidence and acceptance reconciliation"` (run with `git -C <item worktree>` on branch `bug/breadcrumb-webview2-init-fails-resource-not-in-correct-state-792`; the session's attribution trailer lines were supplied through a second `-m`, so the subject line is the plan's text verbatim); then `git status --porcelain --untracked-files=all -- . ':(exclude).claude/agent-memory'` and `git rev-parse HEAD`; then (after this artifact is written) `git add -- docs/features/active/2026-09-06-breadcrumb-webview2-init-fails-resource-not-in-correct-state-792` and `git commit --amend --no-edit`; then the [P8-T5] check-off in the plan as the final edit
- EXIT_CODE: 0
- Output Summary: `[bug/breadcrumb-webview2-init-fails-resource-not-in-correct-state-792 56ff697f] chore(792): manual verification evidence and acceptance reconciliation`; `7 files changed, 329 insertions(+), 6 deletions(-)`; five evidence files created (`p8-t1-addin-rebuild.md`, `p8-t2-ac-u5-manual-verification.md`, `p8-t3-ac-u5.md`, `p8-t4-ac-status.md` and the Phase 7 residual `p7-t18-commit.md`), `plan.2026-09-17T07-30.md` modified with the [P7-T18] and [P8-T1] through [P8-T4] check-offs, `spec.md` modified with the AC-U5 check-off; the porcelain capture below is empty; no source, project, solution or configuration path was staged or committed.

## Porcelain capture (verbatim, `git status --porcelain --untracked-files=all -- . ':(exclude).claude/agent-memory'`, immediately after the first commit)

```
```

The capture is empty: nothing outside the feature folder is dirty and nothing ending `.cs`, `.csproj`, `.sln` or `packages.config` is dirty. The scoped source porcelain of convention 9 (`git status --porcelain -- '*.cs' '*.csproj' '*.sln' 'packages.config'`) also printed nothing when run before the commit.

## HEAD observation

COMMIT-SHA-OBSERVED (before amend): 56ff697f12ab0805fbccf3751fc7d7c21c07d053

PARENT-SHA: 986ce5aafb5cae63fb9a01ce1d904491ea2b3b95 (the [P7-T18] commit)

The amend that follows (sweeping this artifact into the same commit) rewrites the commit object, so the post-amend head SHA differs from the value above. The post-amend SHA is reported in the executor's completion message; this file cannot record it without a further amend, and the plan authorizes exactly one.

## Paths in the first commit (`git show --name-only --format= HEAD`, 7 paths, verbatim)

```
docs/features/active/2026-09-06-breadcrumb-webview2-init-fails-resource-not-in-correct-state-792/evidence/issue-updates/p8-t3-ac-u5.md
docs/features/active/2026-09-06-breadcrumb-webview2-init-fails-resource-not-in-correct-state-792/evidence/issue-updates/p8-t4-ac-status.md
docs/features/active/2026-09-06-breadcrumb-webview2-init-fails-resource-not-in-correct-state-792/evidence/other/p8-t1-addin-rebuild.md
docs/features/active/2026-09-06-breadcrumb-webview2-init-fails-resource-not-in-correct-state-792/evidence/other/p8-t2-ac-u5-manual-verification.md
docs/features/active/2026-09-06-breadcrumb-webview2-init-fails-resource-not-in-correct-state-792/evidence/qa-gates/p7-t18-commit.md
docs/features/active/2026-09-06-breadcrumb-webview2-init-fails-resource-not-in-correct-state-792/plan.2026-09-17T07-30.md
docs/features/active/2026-09-06-breadcrumb-webview2-init-fails-resource-not-in-correct-state-792/spec.md
```

Every path starts with `docs/features/active/2026-09-06-breadcrumb-webview2-init-fails-resource-not-in-correct-state-792/`. After the amend the commit additionally contains `evidence/other/p8-t5-terminal.md` (this file).

## Residual outside the plan's pathspecs (recorded, not an acceptance clause)

`git status --porcelain --untracked-files=all` without the exclusion lists `.claude/agent-memory/atomic-executor/MEMORY.md` (modified) and `.claude/agent-memory/atomic-executor/project_pwsh_param_name_case_collision_flattens_log_array.md` (untracked), both inherited from an earlier executor and outside the plan's add pathspecs; they were deliberately left uncommitted, as in [P7-T18].

## Expected end state after the amend and the final check-off

- `git diff --numstat HEAD -- <feature>/plan.2026-09-17T07-30.md` prints `1 1` for exactly that one file: the [P8-T5] check-off is the sole permitted residual.
- `git status --porcelain --untracked-files=all -- . ':(exclude).claude/agent-memory' ':(exclude)<feature>/plan.2026-09-17T07-30.md'` prints nothing.

Git printed `LF will be replaced by CRLF` warnings for the Markdown files; this is the repository's autocrlf normalisation notice and does not affect the committed content.
