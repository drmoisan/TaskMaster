# [P0-T7] Git base capture

- Issue: #792
- Timestamp: 2026-09-17T18-38
- Command: `git fetch origin`; `git rev-parse HEAD`; `git rev-parse origin/main`; `git merge-base HEAD origin/main`; `git merge-base --is-ancestor origin/main HEAD; $LASTEXITCODE`; `git status --porcelain --untracked-files=all`; `git diff --name-status (git merge-base HEAD origin/main) HEAD` (all run against the item worktree on branch `bug/breadcrumb-webview2-init-fails-resource-not-in-correct-state-792`)
- EXIT_CODE: 0
- Output Summary: fetch exit 0; HEAD `11b107a55fc32078f97e0cd48f893c175be5b6f4`; `origin/main` `e7cbb57229c63a228e7fe0bcbcdbfbc06db8bcd3`; merge base equals `origin/main`; `origin/main` is an ancestor of HEAD (exit 0); porcelain lists 7 lines, all under the feature folder, 0 source paths; base-to-HEAD name-status lists 7 added files, all under the feature folder.

## Base

BASE-SHA: e7cbb57229c63a228e7fe0bcbcdbfbc06db8bcd3

ORIGIN-MAIN-IS-ANCESTOR: true

Note (DIFF BASES block, both forms compared): `git merge-base --is-ancestor origin/main HEAD` exited 0, so `origin/main` is an ancestor of HEAD and the merge base IS the `origin/main` SHA: `git merge-base HEAD origin/main` printed `e7cbb57229c63a228e7fe0bcbcdbfbc06db8bcd3`, identical to `git rev-parse origin/main`. Two-dot and three-dot forms anchored to `origin/main` therefore resolve to the same base. Bare local `main` was not used.

HEAD-OBSERVED: 11b107a55fc32078f97e0cd48f893c175be5b6f4

SPEC-REF-SHA: 11b107a55fc32078f97e0cd48f893c175be5b6f4

Note: `SPEC-REF-SHA` equals the `git rev-parse HEAD` output at capture time. The feature folder does not exist at `BASE-SHA` (every feature-folder path is `A` in the name-status list below), so criterion-text immutability of `spec.md` is checked against `SPEC-REF-SHA`, not `BASE-SHA`.

## Porcelain (verbatim, `git status --porcelain --untracked-files=all`)

```
 M docs/features/active/2026-09-06-breadcrumb-webview2-init-fails-resource-not-in-correct-state-792/plan.2026-09-17T07-30.md
?? docs/features/active/2026-09-06-breadcrumb-webview2-init-fails-resource-not-in-correct-state-792/evidence/baseline/p0-t3-sdk.md
?? docs/features/active/2026-09-06-breadcrumb-webview2-init-fails-resource-not-in-correct-state-792/evidence/baseline/p0-t4-tool-restore.md
?? docs/features/active/2026-09-06-breadcrumb-webview2-init-fails-resource-not-in-correct-state-792/evidence/baseline/p0-t5-nuget-restore.md
?? docs/features/active/2026-09-06-breadcrumb-webview2-init-fails-resource-not-in-correct-state-792/evidence/baseline/p0-t6-dotnet-coverage.md
?? docs/features/active/2026-09-06-breadcrumb-webview2-init-fails-resource-not-in-correct-state-792/evidence/baseline/phase0-instructions-read.md
?? docs/features/active/2026-09-06-breadcrumb-webview2-init-fails-resource-not-in-correct-state-792/evidence/other/p0-t2-outlook-closed.md
```

PORCELAIN-SOURCE-PATHS: 0

(Computed as the count of porcelain lines whose path ends `.cs`, `.csproj`, `.sln` or `packages.config`. The gitignored `coverage/plan792-helper.ps1` and `packages/Meziantou.Analyzer.3.0.203/` do not appear, as expected.)

## BASE-TO-HEAD-NAME-STATUS

```
A	docs/features/active/2026-09-06-breadcrumb-webview2-init-fails-resource-not-in-correct-state-792/issue.md
A	docs/features/active/2026-09-06-breadcrumb-webview2-init-fails-resource-not-in-correct-state-792/plan.2026-09-12T13-21.md
A	docs/features/active/2026-09-06-breadcrumb-webview2-init-fails-resource-not-in-correct-state-792/plan.2026-09-17T07-30.md
A	docs/features/active/2026-09-06-breadcrumb-webview2-init-fails-resource-not-in-correct-state-792/research/2026-09-12T10-30-breadcrumb-webview2-init-research.md
A	docs/features/active/2026-09-06-breadcrumb-webview2-init-fails-resource-not-in-correct-state-792/research/2026-09-17T11-20-breadcrumb-webview2-init-research.md
A	docs/features/active/2026-09-06-breadcrumb-webview2-init-fails-resource-not-in-correct-state-792/spec.md
A	docs/features/active/2026-09-06-breadcrumb-webview2-init-fails-resource-not-in-correct-state-792/user-story.md
```
