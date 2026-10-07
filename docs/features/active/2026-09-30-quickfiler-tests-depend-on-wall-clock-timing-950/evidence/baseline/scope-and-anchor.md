# Scope and anchor (P0-T2, P0-T3)

Timestamp: 2026-10-02T00-46
Task: P0-T2

## Documents read in full

- FEATURE/spec.md (302 lines)
- FEATURE/issue.md (67 lines)
- FEATURE/research/2026-10-01T00-00-wall-clock-waits-research.md (194 lines)

## Write Set (code paths, verbatim)

- `QuickFiler/Controllers/QfcDatamodel.cs`
- `QuickFiler.Test/Controllers/QfcDatamodelLivenessTests.cs`
- `QuickFiler.Test/Controllers/QfcDatamodelTeardownTests.cs`
- `QuickFiler.Test/Controllers/QfcInitEmailQueueZeroBatchTests.cs`
- `QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixtureTests.cs`

Feature documents in the Write Set: FEATURE/spec.md (check-off edits only) and FEATURE/plan.2026-10-01T07-11.md (task check-off edits only); evidence files under FEATURE/evidence/.

## Prohibited paths (from the plan Write Set section)

QuickFiler.Test/QuickFiler.Test.csproj, QuickFiler/QuickFiler.csproj, QuickFiler.Test/Controllers/QfcItemController.FocusAndThemeTests.cs, QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs, QuickFiler.Test/Controllers/QfcItemController.TestSupport.cs, UtilitiesCS/Threading/UiThread.cs, TaskMaster.runsettings, scripts/vscode/TaskMaster.cli.runsettings, every file under scripts/, every file under .github/, every file under .claude/ (uncommitted .claude/agent-memory/ files are never staged), every file under docs/features/potential/, and the research document. No raw trx, coverage document or msbuild log is copied into FEATURE.

## Work mode and acceptance-criteria inventory

- issue.md line 12 reads: `- Work Mode: full-bug`
- spec.md lines beginning `- [ ] AC`: 17 (counted with a line-anchored search)
- spec.md lines beginning `- [x] AC`: 0
- Inventory: AC1 to AC17, spec.md lines 266 to 282.

Output Summary: Write Set and prohibited paths recorded; Work Mode full-bug confirmed at issue.md line 12; 17 unchecked and 0 checked acceptance-criteria lines in spec.md.

## Anchor and pre-change tree state (P0-T3)

Timestamp: 2026-10-02T00-47
Command: git -C WORKTREE rev-parse HEAD; git -C WORKTREE rev-parse --abbrev-ref HEAD; git -C WORKTREE merge-base --is-ancestor 34c2ed88cbb009f2f231453db87bc64d45a9bd51 HEAD; git -C WORKTREE merge-base origin/main HEAD; git -C WORKTREE diff --name-status 34c2ed88cbb009f2f231453db87bc64d45a9bd51 HEAD; git -C WORKTREE diff --exit-code 34c2ed88cbb009f2f231453db87bc64d45a9bd51 HEAD -- QuickFiler QuickFiler.Test UtilitiesCS/Threading/UiThread.cs scripts/vscode TaskMaster.runsettings; git -C WORKTREE status --porcelain --untracked-files=all (each a separate call)
EXIT_CODE: 0

HEAD-SHA: 03c0d01eb517c7b4fe3a837bfede5a4fcf16669f
BRANCH: bug/quickfiler-tests-depend-on-wall-clock-timing-950
ANCESTOR-CHECK: exit 0 (BASE 34c2ed88cbb009f2f231453db87bc64d45a9bd51 is an ancestor of HEAD)
MERGE-BASE: 34c2ed88cbb009f2f231453db87bc64d45a9bd51 (equals BASE)

INHERITED-COMMITTED:
- A	docs/features/active/2026-09-30-quickfiler-tests-depend-on-wall-clock-timing-950/evidence/other/preflight-clearance-r2.2026-10-02T12-10.md
- A	docs/features/active/2026-09-30-quickfiler-tests-depend-on-wall-clock-timing-950/evidence/other/preflight-clearance.2026-10-02T04-30.md
- A	docs/features/active/2026-09-30-quickfiler-tests-depend-on-wall-clock-timing-950/issue.md
- A	docs/features/active/2026-09-30-quickfiler-tests-depend-on-wall-clock-timing-950/plan.2026-10-01T07-11.md
- A	docs/features/active/2026-09-30-quickfiler-tests-depend-on-wall-clock-timing-950/research/2026-10-01T00-00-wall-clock-waits-research.md
- A	docs/features/active/2026-09-30-quickfiler-tests-depend-on-wall-clock-timing-950/spec.md
- A	docs/features/potential/promoted/2026-09-30-quickfiler-tests-depend-on-wall-clock-timing.md

Every inherited path is under FEATURE or is exactly the promoted record: in scope.

CODE-TREE-AT-BASE: UNCHANGED (scoped --exit-code diff exited 0 and printed nothing)

PRE-EXISTING-WORKTREE-PATHS:
-  M docs/features/active/2026-09-30-quickfiler-tests-depend-on-wall-clock-timing-950/plan.2026-10-01T07-11.md
- ?? docs/features/active/2026-09-30-quickfiler-tests-depend-on-wall-clock-timing-950/evidence/baseline/phase0-instructions-read.md
- ?? docs/features/active/2026-09-30-quickfiler-tests-depend-on-wall-clock-timing-950/evidence/baseline/scope-and-anchor.md

No porcelain line names a path under QuickFiler/ or QuickFiler.Test/.

Output Summary: anchor verified (merge-base equals BASE, ancestor check exit 0); inherited set is FEATURE plus the promoted record; cited code tree unchanged since BASE; code tree clean at anchor.
