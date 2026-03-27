# Current Mixed-Branch Diff Scope (Remediation Baseline)

Timestamp: 2026-03-26T15:32:00Z

Command: git diff --name-status $(git merge-base HEAD origin/development) HEAD

EXIT_CODE: 0

## Output Summary

The current `feature/utilities-coverage-part-three-87` branch contains changes spanning multiple issue scopes relative to `origin/development`:

### Issue #96 scope (present)
- `M QuickFiler/Controllers/QfcItemController.cs`
- `A QuickFiler.Test/Controllers/QfcItemControllerTests.cs`
- `docs/features/active/2026-03-25-quickfiler-gui-not-expanding-96/` (multiple added/modified evidence files)

### Issue #97 scope (present)
- `M QuickFiler/Controllers/QfcCollectionController.cs`
- `M QuickFiler/Controllers/QfcHomeController.cs`
- `A QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs`
- `M QuickFiler.Test/Controllers/QfcHomeControllerTests.cs`
- `docs/features/active/2026-03-25-getmovediagnostics-null-guard-97/` (added)

### Issue #87 scope (present)
- Extensive `UtilitiesCS/` and `UtilitiesCS.Test/` modifications and additions
- `UtilitiesSwordfish/Collections/ConcurrentObservableBase.cs`
- `docs/features/active/2026-03-19-utilities-coverage-part-three-87/` (extensive evidence artifacts)

### Other mixed scope (present)
- `.codex/` agent and skill files
- `.github/` workflow and agent files
- `TaskMaster/` project and ribbon files
- `QuickFiler/Controllers/EfcHomeController.cs` and `QuickFiler.Test/Controllers/EfcHomeControllerTests.cs`
- `change-plan.md`
- `artifacts/` research and PR context files

This confirms the branch is a mixed bag requiring unstacking. The remediation plan for this pass targets only the issue #96 commits (bd8fc03 and 3b472b2) for clean-branch recovery.
