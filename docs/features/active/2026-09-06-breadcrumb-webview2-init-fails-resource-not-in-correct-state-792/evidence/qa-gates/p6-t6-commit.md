# [P6-T6] Phase 6 commit: structural gates and parity evidence

- Issue: #792
- Timestamp: 2026-09-17T20-57
- Command: `git add -- docs/features/active/2026-09-06-breadcrumb-webview2-init-fails-resource-not-in-correct-state-792` then `git commit -m "chore(792): structural gates and parity evidence"` (run with `git -C <item worktree>` on branch `bug/breadcrumb-webview2-init-fails-resource-not-in-correct-state-792`; the session's attribution trailer lines were supplied through a second `-m`, so the subject line is the plan's text verbatim), then `git show --name-only --format= HEAD` and `git status --porcelain -- '*.cs' '*.csproj' '*.sln' 'packages.config'`
- EXIT_CODE: 0
- Output Summary: `[bug/breadcrumb-webview2-init-fails-resource-not-in-correct-state-792 c9b457bd] chore(792): structural gates and parity evidence`; `7 files changed, 367 insertions(+), 6 deletions(-)`; five Phase 6 evidence files and the Phase 5 residual `p5-t9-commit.md` created, the plan file modified with the [P6-T1] through [P6-T5] check-offs; every committed path is under the feature folder; scoped porcelain printed nothing.

COMMIT-SHA-OBSERVED: c9b457bda44ef856306a1bc96c94683dc528993c

PARENT-SHA: 9ac987a969d1f4786d296fee25817c8e5dde9233 (the [P5-T9] commit)

## Paths in the commit (`git show --name-only --format= HEAD`, 7 paths, verbatim)

```
docs/features/active/2026-09-06-breadcrumb-webview2-init-fails-resource-not-in-correct-state-792/evidence/qa-gates/p6-t1-ac-u6-structural-pass.md
docs/features/active/2026-09-06-breadcrumb-webview2-init-fails-resource-not-in-correct-state-792/evidence/qa-gates/p6-t2-line-counts-advisory.md
docs/features/active/2026-09-06-breadcrumb-webview2-init-fails-resource-not-in-correct-state-792/evidence/qa-gates/p6-t3-compile-item-parity.md
docs/features/active/2026-09-06-breadcrumb-webview2-init-fails-resource-not-in-correct-state-792/evidence/qa-gates/p6-t4-popout-ordering.md
docs/features/active/2026-09-06-breadcrumb-webview2-init-fails-resource-not-in-correct-state-792/evidence/qa-gates/p6-t5-literal-sweep.md
docs/features/active/2026-09-06-breadcrumb-webview2-init-fails-resource-not-in-correct-state-792/evidence/regression-testing/p5-t9-commit.md
docs/features/active/2026-09-06-breadcrumb-webview2-init-fails-resource-not-in-correct-state-792/plan.2026-09-17T07-30.md
```

Every path starts with `docs/features/active/2026-09-06-breadcrumb-webview2-init-fails-resource-not-in-correct-state-792/`; no source, project, solution or configuration path is in the commit. The `p5-t9-commit.md` artifact is the Phase 5 residual left uncommitted by construction after [P5-T9], swept here as that commit swept the Phase 4 residual.

## Acceptance observations

- Commit exit code 0.
- `git show --name-only --format= HEAD` lists feature-folder paths only (7 of 7).
- `git status --porcelain -- '*.cs' '*.csproj' '*.sln' 'packages.config'` (convention 9) printed nothing.

## Residual (recorded, not an acceptance clause)

`git status --porcelain --untracked-files=all` immediately after the commit listed only `.claude/agent-memory/atomic-executor/MEMORY.md` (modified) and `.claude/agent-memory/atomic-executor/project_pwsh_param_name_case_collision_flattens_log_array.md` (untracked), both inherited from an earlier executor and outside the plan's add pathspecs; they were deliberately left uncommitted.

This artifact and the [P6-T6] check-off in the plan file are written after the commit they describe, so they remain uncommitted feature-folder changes at the end of Phase 6 (convention 9 treats docs and evidence as expected-dirty). No source path is dirty. The next feature-folder commit ([P7-T17] or later) will sweep them.

Git printed seven `LF will be replaced by CRLF` warnings for Markdown files; this is the repository's autocrlf normalisation notice and does not affect the committed content.
