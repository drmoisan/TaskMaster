# Footprint and Scope Boundary (P2-T10)

Timestamp: 2026-09-30T11-27
Task: P2-T10
Command: git diff --name-only origin/main...HEAD; git status --porcelain --untracked-files=all; git diff --name-only origin/main...HEAD -- UtilitiesCS.Test; git diff --name-only origin/main...HEAD -- .claude; git diff --name-only origin/main...HEAD -- .claude config; git ls-files --error-unmatch -- UtilitiesCS TaskMaster.runsettings scripts/vscode/TaskMaster.cli.runsettings coverage.config UtilitiesCS.Test/UtilitiesCS.Test.csproj TaskMaster.Test/Ribbon/RibbonControllerTests.cs TaskMaster.Test/Bootstrap/FSharpCoreHintPathAlignmentTests.cs UtilitiesCS.Test/EmailIntelligence/SortEmail_Tests.cs UtilitiesCS/HelperClasses/FileSystem/PhysicalDirectoryInfoAdapter.cs UtilitiesCS/HelperClasses/FileSystem/PhysicalFileInfoAdapter.cs UtilitiesCS/HelperClasses/FileSystem/DirectoryInfoWrapper.cs (run inside a pwsh payload that printed only its exit code and the listed-path count, because the tracked list under UtilitiesCS is 603 lines); git diff --exit-code ANCHOR-SHA-2 -- UtilitiesCS TaskMaster.runsettings scripts/vscode/TaskMaster.cli.runsettings coverage.config UtilitiesCS.Test/UtilitiesCS.Test.csproj TaskMaster.Test/Ribbon/RibbonControllerTests.cs TaskMaster.Test/Bootstrap/FSharpCoreHintPathAlignmentTests.cs UtilitiesCS.Test/EmailIntelligence/SortEmail_Tests.cs; git diff --exit-code ANCHOR-SHA-2 -- UtilitiesCS/HelperClasses/FileSystem/PhysicalDirectoryInfoAdapter.cs; git diff --exit-code ANCHOR-SHA-2 -- UtilitiesCS/HelperClasses/FileSystem/PhysicalFileInfoAdapter.cs; git diff --exit-code ANCHOR-SHA-2 -- UtilitiesCS/HelperClasses/FileSystem/DirectoryInfoWrapper.cs; git rev-parse origin/main; git merge-base HEAD origin/main; CMD-ADDED-LINES; Get-FileHash -Algorithm SHA256 of the five files; RUNSETTINGS-HASH-NOW (ANCHOR-SHA-2 substituted with 66afa6372fd82fc1ffd7c81f85a1ad65eebc5817, the P2-T7 `ANCHOR-SHA-2:` value)
EXIT_CODE: 0 (scoped to the `OUT-OF-SET-DIFF-EXIT` span)
Output Summary: the footprint is exactly the two Write Set code paths plus paths under the feature folder; no production file, runsettings file, coverage.config, project file or other root-walk site differs from the merged origin/main tip; no .claude or config path is committed; every added-line prohibition count is 0 over 276 added lines; the five hashes equal their anchors; all twelve acceptance items hold.

## Committed list (`git diff --name-only origin/main...HEAD`, verbatim)

UtilitiesCS.Test/HelperClasses/DirectoryInfoWrapper_Tests.cs
UtilitiesCS.Test/HelperClasses/PhysicalFileSystemAdapters_Tests.cs
docs/features/active/2026-09-29-filesystem-wrapper-tests-open-repository-solution-file-940/evidence/baseline/coverage-baseline.md
docs/features/active/2026-09-29-filesystem-wrapper-tests-open-repository-solution-file-940/evidence/baseline/p0-t12-pre-edit-census.2026-09-30T07-22.md
docs/features/active/2026-09-29-filesystem-wrapper-tests-open-repository-solution-file-940/evidence/baseline/p0-t13-phase0-commit.2026-09-30T07-24.md
docs/features/active/2026-09-29-filesystem-wrapper-tests-open-repository-solution-file-940/evidence/baseline/p0-t2-mode-preconditions.2026-09-30T07-11.md
docs/features/active/2026-09-29-filesystem-wrapper-tests-open-repository-solution-file-940/evidence/baseline/p0-t3-worktree-context.2026-09-30T07-13.md
docs/features/active/2026-09-29-filesystem-wrapper-tests-open-repository-solution-file-940/evidence/baseline/p0-t4-channel-and-toolchain.2026-09-30T07-14.md
docs/features/active/2026-09-29-filesystem-wrapper-tests-open-repository-solution-file-940/evidence/baseline/p0-t5-nuget-restore.2026-09-30T07-14.md
docs/features/active/2026-09-29-filesystem-wrapper-tests-open-repository-solution-file-940/evidence/baseline/p0-t6-csharpier-check.2026-09-30T07-16.md
docs/features/active/2026-09-29-filesystem-wrapper-tests-open-repository-solution-file-940/evidence/baseline/p0-t7-msbuild-analyzers.2026-09-30T07-16.md
docs/features/active/2026-09-29-filesystem-wrapper-tests-open-repository-solution-file-940/evidence/baseline/p0-t8-msbuild-nullable.2026-09-30T07-17.md
docs/features/active/2026-09-29-filesystem-wrapper-tests-open-repository-solution-file-940/evidence/baseline/p0-t9-stall-probe.2026-09-30T07-18.md
docs/features/active/2026-09-29-filesystem-wrapper-tests-open-repository-solution-file-940/evidence/baseline/phase0-instructions-read.md
docs/features/active/2026-09-29-filesystem-wrapper-tests-open-repository-solution-file-940/evidence/baseline/test-run-baseline.md
docs/features/active/2026-09-29-filesystem-wrapper-tests-open-repository-solution-file-940/evidence/other/preflight-clearance.2026-09-30T01-26.md
docs/features/active/2026-09-29-filesystem-wrapper-tests-open-repository-solution-file-940/evidence/other/root-walk-site-classification.2026-09-30T07-27.md
docs/features/active/2026-09-29-filesystem-wrapper-tests-open-repository-solution-file-940/evidence/qa-gates/coverage-final.md
docs/features/active/2026-09-29-filesystem-wrapper-tests-open-repository-solution-file-940/evidence/qa-gates/p2-t1-csharpier-format.2026-09-30T08-08.md
docs/features/active/2026-09-29-filesystem-wrapper-tests-open-repository-solution-file-940/evidence/qa-gates/p2-t2-csharpier-check.2026-09-30T08-08.md
docs/features/active/2026-09-29-filesystem-wrapper-tests-open-repository-solution-file-940/evidence/qa-gates/p2-t3-msbuild-analyzers.2026-09-30T08-09.md
docs/features/active/2026-09-29-filesystem-wrapper-tests-open-repository-solution-file-940/evidence/qa-gates/p2-t4-msbuild-nullable.2026-09-30T08-10.md
docs/features/active/2026-09-29-filesystem-wrapper-tests-open-repository-solution-file-940/evidence/regression-testing/fail-before-exception.2026-09-30T07-28.md
docs/features/active/2026-09-29-filesystem-wrapper-tests-open-repository-solution-file-940/evidence/regression-testing/mutation-directory-creationtime-setter-noop.md
docs/features/active/2026-09-29-filesystem-wrapper-tests-open-repository-solution-file-940/evidence/regression-testing/mutation-directory-delete-noop.md
docs/features/active/2026-09-29-filesystem-wrapper-tests-open-repository-solution-file-940/evidence/regression-testing/mutation-directory-exists-inverted.md
docs/features/active/2026-09-29-filesystem-wrapper-tests-open-repository-solution-file-940/evidence/regression-testing/mutation-directory-getfiles-empty.md
docs/features/active/2026-09-29-filesystem-wrapper-tests-open-repository-solution-file-940/evidence/regression-testing/mutation-directory-name-fullname.md
docs/features/active/2026-09-29-filesystem-wrapper-tests-open-repository-solution-file-940/evidence/regression-testing/mutation-file-isreadonly-setter-noop.md
docs/features/active/2026-09-29-filesystem-wrapper-tests-open-repository-solution-file-940/evidence/regression-testing/mutation-file-length-zero.md
docs/features/active/2026-09-29-filesystem-wrapper-tests-open-repository-solution-file-940/evidence/regression-testing/mutation-file-moveto-noop.md
docs/features/active/2026-09-29-filesystem-wrapper-tests-open-repository-solution-file-940/evidence/regression-testing/mutation-wrapper-enumeratefilesysteminfos-empty.md
docs/features/active/2026-09-29-filesystem-wrapper-tests-open-repository-solution-file-940/evidence/regression-testing/mutation-wrapper-getfiles-empty.md
docs/features/active/2026-09-29-filesystem-wrapper-tests-open-repository-solution-file-940/evidence/regression-testing/mutation-wrapper-tostring-empty.md
docs/features/active/2026-09-29-filesystem-wrapper-tests-open-repository-solution-file-940/evidence/regression-testing/p1-t3-pfs-census.2026-09-30T07-30.md
docs/features/active/2026-09-29-filesystem-wrapper-tests-open-repository-solution-file-940/evidence/regression-testing/p1-t31-post-control-clean-tree.2026-09-30T08-04.md
docs/features/active/2026-09-29-filesystem-wrapper-tests-open-repository-solution-file-940/evidence/regression-testing/p1-t4-diw-census.2026-09-30T07-31.md
docs/features/active/2026-09-29-filesystem-wrapper-tests-open-repository-solution-file-940/evidence/regression-testing/p1-t5-csharpier-scoped.2026-09-30T07-32.md
docs/features/active/2026-09-29-filesystem-wrapper-tests-open-repository-solution-file-940/evidence/regression-testing/p1-t6-build-after-fix.2026-09-30T07-33.md
docs/features/active/2026-09-29-filesystem-wrapper-tests-open-repository-solution-file-940/evidence/regression-testing/p1-t7-exception-type-observation.2026-09-30T07-34.md
docs/features/active/2026-09-29-filesystem-wrapper-tests-open-repository-solution-file-940/evidence/regression-testing/p1-t8-fix-commit.2026-09-30T07-35.md
docs/features/active/2026-09-29-filesystem-wrapper-tests-open-repository-solution-file-940/evidence/regression-testing/test-run-final.md
docs/features/active/2026-09-29-filesystem-wrapper-tests-open-repository-solution-file-940/issue.md
docs/features/active/2026-09-29-filesystem-wrapper-tests-open-repository-solution-file-940/plan.2026-09-29T23-02.md
docs/features/active/2026-09-29-filesystem-wrapper-tests-open-repository-solution-file-940/research/2026-09-29T21-10-filesystem-wrapper-tests-open-repository-solution-file-research.md
docs/features/potential/promoted/2026-09-29-filesystem-wrapper-tests-open-repository-solution-file.md

## Porcelain companion (`git status --porcelain --untracked-files=all`, verbatim)

 M docs/features/active/2026-09-29-filesystem-wrapper-tests-open-repository-solution-file-940/evidence/qa-gates/coverage-final.md
 M docs/features/active/2026-09-29-filesystem-wrapper-tests-open-repository-solution-file-940/plan.2026-09-29T23-02.md
?? docs/features/active/2026-09-29-filesystem-wrapper-tests-open-repository-solution-file-940/evidence/qa-gates/p2-t7-msbuild-analyzers.2026-09-30T11-19.md
?? docs/features/active/2026-09-29-filesystem-wrapper-tests-open-repository-solution-file-940/evidence/qa-gates/p2-t7-msbuild-nullable.2026-09-30T11-20.md
?? docs/features/active/2026-09-29-filesystem-wrapper-tests-open-repository-solution-file-940/evidence/qa-gates/p2-t8-post-format-census.2026-09-30T11-25.md
?? docs/features/active/2026-09-29-filesystem-wrapper-tests-open-repository-solution-file-940/evidence/qa-gates/toolchain-pass.md

## Derived sets

- INHERITED-AND-EXCLUDED:
  - Clause A: docs/features/active/2026-09-29-filesystem-wrapper-tests-open-repository-solution-file-940/evidence/other/preflight-clearance.2026-09-30T01-26.md
  - Clause A: docs/features/active/2026-09-29-filesystem-wrapper-tests-open-repository-solution-file-940/issue.md
  - Clause A: docs/features/active/2026-09-29-filesystem-wrapper-tests-open-repository-solution-file-940/plan.2026-09-29T23-02.md
  - Clause A: docs/features/active/2026-09-29-filesystem-wrapper-tests-open-repository-solution-file-940/research/2026-09-29T21-10-filesystem-wrapper-tests-open-repository-solution-file-research.md
  - Clause A: docs/features/potential/promoted/2026-09-29-filesystem-wrapper-tests-open-repository-solution-file.md
  - Clause A: docs/features/active/2026-09-29-filesystem-wrapper-tests-open-repository-solution-file-940/evidence/baseline/p0-t2-mode-preconditions.2026-09-30T07-11.md
  - Clause A: docs/features/active/2026-09-29-filesystem-wrapper-tests-open-repository-solution-file-940/evidence/baseline/phase0-instructions-read.md
  - Clause B: no listed path begins .claude/agent-memory/ (none removed)
- THIS-ITEM-FOOTPRINT:
  - UtilitiesCS.Test/HelperClasses/DirectoryInfoWrapper_Tests.cs
  - UtilitiesCS.Test/HelperClasses/PhysicalFileSystemAdapters_Tests.cs
  - every other path of the two lists above, each under docs/features/active/2026-09-29-filesystem-wrapper-tests-open-repository-solution-file-940/ (37 committed feature-folder paths not in Clause A, plus the two modified and four untracked evidence paths of the porcelain companion)
- UCS-TEST-CHANGED:
  - UtilitiesCS.Test/HelperClasses/DirectoryInfoWrapper_Tests.cs
  - UtilitiesCS.Test/HelperClasses/PhysicalFileSystemAdapters_Tests.cs
- CLAUDE-CHANGED: NONE (empty list)
- COMMITTED-CLAUDE-OR-CONFIG-RAW: NONE (empty list)
- COMMITTED-CLAUDE-OR-CONFIG: NONE
- INHERITED-PROMOTION-RECORD: docs/features/potential/promoted/2026-09-29-filesystem-wrapper-tests-open-repository-solution-file.md
- ORIGIN-MAIN-NOW: 66afa6372fd82fc1ffd7c81f85a1ad65eebc5817
- MERGE-BASE-NOW: 66afa6372fd82fc1ffd7c81f85a1ad65eebc5817 (equals the P2-T7 `ANCHOR-SHA-2:` value)
- PATHSPEC-MATCH-EXIT: 0 (603 tracked paths listed)
- OUT-OF-SET-DIFF-EXIT: 0
- PDA-DIFF-EXIT: 0
- PFA-DIFF-EXIT: 0
- DIWP-DIFF-EXIT: 0

## CMD-ADDED-LINES (anchored at ANCHOR-SHA-2)

- ADDED-LINE-COUNT: 276
- ADDED-DONOTPARALLELIZE: 0
- ADDED-THREAD-SLEEP: 0
- ADDED-TASK-DELAY: 0
- ADDED-TIMEOUT: 0
- ADDED-RETRY: 0
- ADDED-WORKERS: 0
- ADDED-SCOPE: 0
- ADDED-TEMP: 0
- ADDED-CATCH: 0
- ADDED-ROOTWALK: 0

## Hashes

| File | SHA-256 | Anchor | Equal |
| --- | --- | --- | --- |
| UtilitiesCS.Test/HelperClasses/PhysicalFileSystemAdapters_Tests.cs | C88A785C23D8DB2960E9FAA53DF9EF91F6F00359683485BEC7EDFC44F9A2F998 | P2-T1 PFS-HASH-AFTER C88A785C23D8DB2960E9FAA53DF9EF91F6F00359683485BEC7EDFC44F9A2F998 | yes |
| UtilitiesCS.Test/HelperClasses/DirectoryInfoWrapper_Tests.cs | 6650B33204BABCAAB7CD6E97C8B4BA7012ABB1F1320DB6AF7764ECD8714B7910 | P2-T1 DIW-HASH-AFTER 6650B33204BABCAAB7CD6E97C8B4BA7012ABB1F1320DB6AF7764ECD8714B7910 | yes |
| UtilitiesCS/HelperClasses/FileSystem/PhysicalDirectoryInfoAdapter.cs | FBAC7002BE4DF5979624F8AE649D52F1044BC40D54EBDA4618C6374EAB9CB242 | PRE-EDIT-HASH-PDA: FBAC7002BE4DF5979624F8AE649D52F1044BC40D54EBDA4618C6374EAB9CB242 | yes |
| UtilitiesCS/HelperClasses/FileSystem/PhysicalFileInfoAdapter.cs | 0337E5C1FF7A2E3FF5E5D67D383E68583838AFB5E5BA999A30502CF24333D948 | PRE-EDIT-HASH-PFA: 0337E5C1FF7A2E3FF5E5D67D383E68583838AFB5E5BA999A30502CF24333D948 | yes |
| UtilitiesCS/HelperClasses/FileSystem/DirectoryInfoWrapper.cs | F77616271ABB36C9133ADE496550703B391B077AB68C8F5DC652AB73FD177DC3 | PRE-EDIT-HASH-DIWP: F77616271ABB36C9133ADE496550703B391B077AB68C8F5DC652AB73FD177DC3 | yes |

- RUNSETTINGS-HASH-NOW: 98EF03A8D3B0EBB2ED7A765E3B5E1B58E774D20202DF2F294C03A7260B9CEF57 (equals P0-T4 `RUNSETTINGS-HASH:`)

## Acceptance

1. PATHSPEC-MATCH-EXIT: 0 - met.
2. MERGE-BASE-NOW equals ANCHOR-SHA-2 - met.
3. THIS-ITEM-FOOTPRINT is the two Write Set code paths plus feature-folder paths only - met.
4. UCS-TEST-CHANGED is exactly the two Write Set paths - met.
5. every CLAUDE-CHANGED path begins .claude/agent-memory/ - met (list empty).
6. COMMITTED-CLAUDE-OR-CONFIG: NONE - met.
7. OUT-OF-SET-DIFF-EXIT: 0 - met.
8. PDA-DIFF-EXIT, PFA-DIFF-EXIT and DIWP-DIFF-EXIT all 0 - met.
9. the ten ADDED- counts other than ADDED-LINE-COUNT are 0 over ADDED-LINE-COUNT 276 - met.
10. production hashes equal PRE-EDIT-HASH- anchors and test hashes equal P2-T1 after-format hashes - met.
11. RUNSETTINGS-HASH-NOW equals RUNSETTINGS-HASH - met.
12. the porcelain span is present as the name-listing diff's companion - met.
