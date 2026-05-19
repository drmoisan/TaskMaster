# Current Mixed-Branch Diff Scope

Timestamp: 2026-03-26T16:05:00-04:00
Command: git diff --name-status $(git merge-base HEAD origin/development) HEAD
EXIT_CODE: 0

Output Summary:
The current mixed-branch diff relative to `origin/development` spans the following top-level scopes:

- `.codex` — Codex agent/workflow files (agents, skills, prompts, setup scripts)
- `.github` — Agent definitions, skills archive, workflow changes
- `QuickFiler` — Controllers (EfcHomeController, QfcCollectionController, QfcHomeController, QfcItemController)
- `QuickFiler.Test` — Controller test files (EfcHomeControllerTests, QfcCollectionControllerTests, QfcHomeControllerTests, QfcItemControllerTests)
- `TaskMaster` — AppAutoFileObjects.cs, RibbonExplorer.xml, TaskMaster.csproj
- `UtilitiesCS` — Production code changes (Dialogs, EmailIntelligence, Extensions, NewtonsoftHelpers, ReusableTypeClasses, deleted deprecated files, csproj)
- `UtilitiesCS.Test` — Test coverage uplift files (~80+ new/modified test files, csproj, test data)
- `UtilitiesSwordfish` — ConcurrentObservableBase.cs fix
- `docs/features/active/2026-03-19-utilities-coverage-part-three-87/` — Issue #87 feature folder (plans, evidence, audits, remediation)
- `docs/features/active/2026-03-25-getmovediagnostics-null-guard-97/` — Issue #97 feature folder
- `docs/features/active/2026-03-25-quickfiler-gui-not-expanding-96/` — Issue #96 feature folder
- `docs/features/potential/` — Potential feature docs for issues #96 and #97
- `change-plan.md`, `missing-serializable-list.json` — Planning and data files
- Deleted: `.merge_file_gXWn2v`, `interop_inspect.txt`, `test-output.txt`

This confirms the mixed-branch contains `.codex`, `.github`, `QuickFiler`, `TaskMaster`, issue `#96`, and issue `#97` scope alongside the intended issue `#87` scope.
