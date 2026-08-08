---
name: project-503-ribbon-readiness-plan-seams
description: '#503 ribbon engine-readiness guard planning facts: RibbonViewer 487/500 forces a partial split, 6+4 csproj Compile entries, compile-time red, #504-#508 already promoted'
metadata:
  type: project
---

Planning facts verified on the `bug/ribbon-engine-readiness-guard-503` worktree (2026-08-08), merge-base `003c5715`:

- `TaskMaster\Ribbon\RibbonViewer.cs` is 487/500 lines and is NOT `partial`. New ribbon callbacks cannot be added in place; the `#region Spam Manager` (250-296) and `#region Triage` (298-347) blocks must relocate into a new partial. The move carries 26 members, not just the 8 defective handlers.
- New files need 6 `<Compile Include>` entries in `TaskMaster\TaskMaster.csproj` (ItemGroup at line 459) and 4 in `TaskMaster.Test\TaskMaster.Test.csproj` (ItemGroup at line 311). Both are packages.config/non-SDK.
- Namespace is flat `TaskMaster` (not `TaskMaster.Ribbon`); tests are `TaskMaster.Test.Ribbon`. `IAppItemEngines`, `IConditionalEngine<T>`, `MailItemHelper` are all in `UtilitiesCS`.
- `coverage.config` excludes only Deedle/FSharp/Castle/FluentAssertions/Moq/MSTest modules, so `TaskMaster.dll` IS instrumented and per-type Cobertura line-rate assertions on new `TaskMaster.*` types are measurable.
- The bugfix red is necessarily a COMPILE-time red: the unit under test does not exist at merge-base, and the defective path lives in `[ExcludeFromCodeCoverage]` COM-bound `RibbonViewer` handlers whose `SB`/`Triage` getters install a real `WindowsFormsSynchronizationContext` side effect. Pair the `[expect-fail]` build artifact with a `fail-before-exception.<TS>.md` dossier.
- The repo has no non-modal notification surface; `MessageBox.Show` + log4net is the established user-facing notice mechanism in `RibbonViewer`/`RibbonController`. Pin this as a recorded decision rather than inventing a status bar.

**Why:** These were each a potential preflight finding (unsatisfiable line cap, files that do not compile, unmeasurable coverage AC, ambiguous red, invented UI surface).

**How to apply:** Reuse directly for any follow-up on #504-#507 (orphan `onAction` callbacks, invalid `getPressed` signatures, fire-and-forget `ToggleEngineAsync`, non-null-safe `RibbonController.Engines`) — those are already promoted issues and must not be fixed inside #503. `#508` is the pre-existing `YieldAsync_WithoutDispatcher_RemainsStrict` order-dependent flake; the merge-base suite is 6293 tests and NOT green. Related: [[legacy-csproj-explicit-compile-include]], [[csharpier-repowide-format-breaks-zero-diff-acs]].
