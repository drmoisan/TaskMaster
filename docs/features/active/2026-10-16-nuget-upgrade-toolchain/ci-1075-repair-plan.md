# CI 1075 Hygiene Repair Plan

## Objective
Correct the five absolute user-profile paths reported by CI run 1075 on PR 993, validate locally, and update the existing PR branch so CI can verify the correction.

## Scope and authorization
- One production file: TaskMaster/TaskMaster.csproj (PublishUrl only).
- Four existing task evidence/report files named in the failed hygiene job.
- Supporting plan and QA evidence in this task folder.
- Preserve the user's publish destination in the existing ignored TaskMaster.csproj.user.
- Keep the hygiene guard and all validation thresholds unchanged.

## Tasks
- [x] Reproduce the existing hygiene failure before edits using the repository guard; use the CI failure as initial regression evidence.
- [x] Set the tracked PublishUrl to publish\; verify the existing ignored user file retains the personal override.
- [x] Replace the four tracked machine-specific validation paths with portable repository-relative paths. Use ../TaskMaster-wt/2026-10-16T16-48-toolchain for the worktree and paths relative to that worktree for coverage.
- [x] Confirm the original five findings are gone; verify relevant JSON/XML remain parseable.
- [x] Run CSharpier, full analyzer rebuild, warnings-as-errors compiler rebuild, and the complete MSTest/coverage suite in order. Use the existing isolated validation worktree to avoid Outlook file locks, copying the corrected project into it and verifying identity.
- [x] Run the repository hygiene guard again over the final tracked files and review the diff for unintended changes.
- [x] Record QA results; the root session will commit/push the fix and observe every PR check on the new head.
