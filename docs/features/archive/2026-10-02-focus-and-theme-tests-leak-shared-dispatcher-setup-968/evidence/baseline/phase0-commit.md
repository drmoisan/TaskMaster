# Phase 0 commit (issue #968, task P0-T19)

Timestamp: 2026-10-03T02-54
Command: git -C WORKTREE add -- docs/features/active/2026-10-02-focus-and-theme-tests-leak-shared-dispatcher-setup-968
Canonical command: pathspec-limited git add of FEATURE, then a pathspec-limited git commit of the same pathspec (separate Bash calls)
EXIT_CODE: 0
Output Summary:
- git add exit 0 (LF-to-CRLF working-copy warnings only)
- git -C WORKTREE commit -m "docs(968): record phase 0 baseline evidence" -- docs/features/active/2026-10-02-focus-and-theme-tests-leak-shared-dispatcher-setup-968 -> exit 0; 19 files changed, 586 insertions(+), 18 deletions(-)
- git -C WORKTREE rev-parse HEAD -> exit 0
- PHASE0-COMMIT: 74b7c6bd078535cd439c7c77851eb09abb4f60e7 (observation)
- git -C WORKTREE show --name-status --format= HEAD -> exit 0
- git -C WORKTREE status --porcelain --untracked-files=all -> exit 0, empty output at the time it ran (this artifact and the plan check-off mark were written afterwards)

PHASE0-COMMIT-PATHS:
- A	FEATURE/evidence/baseline/analyzer-alignment.md
- A	FEATURE/evidence/baseline/bootstrap-dotnet-coverage.md
- A	FEATURE/evidence/baseline/bootstrap-nuget-restore.md
- A	FEATURE/evidence/baseline/bootstrap-sdk.md
- A	FEATURE/evidence/baseline/bootstrap-tool-restore.md
- A	FEATURE/evidence/baseline/census-baseline.md
- A	FEATURE/evidence/baseline/concurrent-set-baseline.md
- A	FEATURE/evidence/baseline/coverage-jacoco-projection.md
- A	FEATURE/evidence/baseline/coverage-summary.md
- A	FEATURE/evidence/baseline/csharpier-check-baseline.md
- A	FEATURE/evidence/baseline/datamodel-set-baseline.md
- A	FEATURE/evidence/baseline/fold-census-baseline.md
- A	FEATURE/evidence/baseline/msbuild-analyzer-baseline.md
- A	FEATURE/evidence/baseline/msbuild-nullable-baseline.md
- A	FEATURE/evidence/baseline/phase0-instructions-read.md
- A	FEATURE/evidence/baseline/scope-and-anchor.md
- A	FEATURE/evidence/baseline/stall-probe.md
- A	FEATURE/evidence/baseline/toolchain-baseline.md
- M	FEATURE/plan.2026-10-02T05-42.md

Every committed path is under FEATURE (FEATURE = docs/features/active/2026-10-02-focus-and-theme-tests-leak-shared-dispatcher-setup-968); no COMMIT SWEPT FOREIGN PATH. No porcelain line names a path under QuickFiler/ or QuickFiler.Test/. The commit message omits the attribution trailer because D-10 prohibits angle brackets in commit messages. This artifact is committed in P6-T9.
