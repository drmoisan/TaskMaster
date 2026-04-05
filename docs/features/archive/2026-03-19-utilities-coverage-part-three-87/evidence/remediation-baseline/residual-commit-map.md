# P0-T3: Residual Commit Map

Source: `.git/branch_analysis_issue87.txt`

## Direct Cherry-Pick Commits (residual excluded work)

| Commit | Date | Subject | Top-level paths |
|---|---|---|---|
| `52742b8` | 2026-03-21 | fix: align codex web workflow with linux setup | `.codex`, `.github` |
| `4d5f476` | 2026-03-21 | ci: run codex web setup test on branch updates | `.codex`, `.github` |
| `60408b0` | 2026-03-22 | (various residual) | `.codex`, `.github`, `QuickFiler.Test` |
| `16d7d5d` | 2026-03-22 | (various residual) | `.codex`, `.github`, `QuickFiler.Test` |
| `0c9a045` | 2026-03-24 | (various residual) | `.codex`, `.github`, `QuickFiler`, `QuickFiler.Test` |
| `66220df` | 2026-03-24 | (various residual) | `.codex`, `.github`, `UtilitiesSwordfish` |
| `ea0206e` | 2026-03-25 | (various residual) | `.codex`, `.github`, `QuickFiler`, `QuickFiler.Test` |

## Bootstrap File Sources (selected files from mixed commits)

| Commit | File(s) to restore | Rationale |
|---|---|---|
| `ee92dd6` | `QuickFiler/Controllers/QfcHomeController.cs`, `missing-serializable-list.json` | Mixed #87 commit containing these non-#87 files |
| `a8d24b2` | `TaskMaster/TaskMaster.csproj` | Mixed commit containing TaskMaster csproj changes |
| `4634ac5` | `TaskMaster/AppGlobals/AppAutoFileObjects.cs` | Mixed commit containing AppAutoFileObjects changes |
