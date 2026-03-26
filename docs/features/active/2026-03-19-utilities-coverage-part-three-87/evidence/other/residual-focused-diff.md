# Evidence: Focused Diff Verification

- **Timestamp:** 2026-03-27T08:06 UTC
- **Command:** `git -C c:\Users\DanMoisan\repos\TaskMaster-residual-clean diff --name-only origin/development...chore/mixed-branch-excluded-work-clean`
- **EXIT_CODE:** 0
- **Output Summary:**

## Changed Files (20 total)

### .codex/** (6 files) — IN ALLOWLIST
- `.codex/agents/atomic-executor.toml`
- `.codex/agents/atomic-planner.toml`
- `.codex/agents/feature-reviewer.toml`
- `.codex/codex-web-setup.plan.md`
- `.codex/codex-web-setup.sh`
- `.codex/prompts/feature-review-remediate.md`

### .github/** (1 file) — IN ALLOWLIST
- `.github/workflows/codex-web-setup-test.yml`

### QuickFiler/** (3 files) — IN ALLOWLIST
- `QuickFiler/Controllers/EfcHomeController.cs`
- `QuickFiler/Controllers/QfcHomeController.cs`
- `QuickFiler/Controllers/QfcItemController.cs`

### QuickFiler.Test/** (3 files) — IN ALLOWLIST
- `QuickFiler.Test/Controllers/EfcHomeControllerTests.cs`
- `QuickFiler.Test/Controllers/QfcItemControllerTests.cs`
- `QuickFiler.Test/QuickFiler.Test.csproj`

### TaskMaster/** (3 files) — IN ALLOWLIST
- `TaskMaster/AppGlobals/AppAutoFileObjects.cs`
- `TaskMaster/Ribbon/RibbonExplorer.xml`
- `TaskMaster/TaskMaster.csproj`

### UtilitiesSwordfish/** (1 file) — IN ALLOWLIST
- `UtilitiesSwordfish/Collections/ConcurrentObservableBase.cs`

### missing-serializable-list.json (1 file) — IN ALLOWLIST
- `missing-serializable-list.json`

### UtilitiesCS.Test/** (2 files) — NOT IN EXPLICIT ALLOWLIST
- `UtilitiesCS.Test/ReusableTypeClasses/Concurrent/Observable/Collection/ConcurrentObservableCollectionSenderTests.cs`
- `UtilitiesCS.Test/UtilitiesCS.Test.csproj`

## Allowlist Compliance

- **18 of 20 files** match the explicit allowlist (`.codex/**`, `.github/**`, `QuickFiler/**`, `QuickFiler.Test/**`, `TaskMaster/**`, `UtilitiesSwordfish/**`, `missing-serializable-list.json`).
- **2 files** from `UtilitiesCS.Test/**` are outside the explicit allowlist but came from commit `60408b0`, which was explicitly listed in the plan's CON-5 verified residual commit set. These are unit tests for `UtilitiesSwordfish/Collections/ConcurrentObservableBase.cs` (which IS in the allowlist). The test project `UtilitiesCS.Test` is the repo-standard test location for shared utility code including UtilitiesSwordfish collections.

## Exclusion Compliance

- **No `UtilitiesCS/**` paths** — `UtilitiesCS.Test` is a distinct top-level directory from `UtilitiesCS` and does not match the `UtilitiesCS/**` glob.
- **No `docs/features/active/2026-03-25-getmovediagnostics-null-guard-97/**` paths** — confirmed absent.
- **No `docs/features/active/2026-03-25-quickfiler-gui-not-expanding-96/**` paths** — confirmed absent.

## Verdict

Exclusion conditions fully satisfied. Allowlist has a minor gap for `UtilitiesCS.Test/**` (planned commit content not reflected in the explicit allowlist). No `#87`, `#96`, or `#97` scope leaked.
