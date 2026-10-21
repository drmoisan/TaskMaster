# quickfiler-test-uithread-dispatcher (Issue #493)

- Work Mode: full-bug
- Type: bug
- Primary Issue: #493
- Primary Issue URL: https://github.com/drmoisan/TaskMaster/issues/493
- Epic: quickfiler-bug-family
- Integration Branch: epic/quickfiler-bug-family-integration
- Last Updated: 2026-08-24T09-55

> Provenance note. This issue is pre-existing: it was already open on GitHub and its potential
> entry was already promoted before this orchestration run began. Per delegation instructions,
> `mcp__drm-copilot__new_potential_bug_entry` and `mcp__drm-copilot__potential_to_issue` were not
> called in this run (calling either would have duplicated issue #493). Only
> `mcp__drm-copilot__new_active_feature_folder` was called, which scaffolded `spec.md` and a
> template `plan.<timestamp>.md` but produced no `issue.md` because it had no promoted-source
> reference to copy in this run. This file was authored by the orchestrator to consolidate the
> pre-existing GitHub issue body and the richer promoted record, following the same pattern used
> for `docs/features/active/winformspumphost-suite-determinism-511/issue.md`.

> Acceptance-criteria authority. Work Mode is `full-bug`, so per the `acceptance-criteria-tracking`
> skill the authoritative acceptance-criteria source for this feature is `spec.md` only. The
> criteria are not duplicated here.

## Requirements Source

The promoted record is richer than the GitHub issue body and is authoritative:

- `docs/features/potential/promoted/2026-08-07-uithread-dispatcher-static-swap-no-restore.md` (#493)

Issue state was verified against durable GitHub state on 2026-08-24 with
`gh issue view 493 --json number,title,state,labels,url`. It is `OPEN` and carries the `bug`
label. No promotion tool was invoked to create it in this session; see the "Promotion" section.

## Summary

`QfcItemControllerTestSupport.EnsureUiThreadDispatcher` (in
`QuickFiler.Test/Controllers/QfcItemController.TestSupport.cs`) mutates the process-wide static
`UtilitiesCS.Threading.UiThread._dispatcher` (via reflection into the private `_dispatcher`
backing field) and never restores the prior value. Because the static is process-wide and MSTest
runs test classes in parallel by default, one class's mutation is visible to every other class in
the same test host, and the static keeps pointing at whatever dispatcher the last caller installed
for the remainder of the run. This violates the Independence and Environment Stability principles
in `.claude/rules/general-unit-test.md`.

## Impact — this defect has already caused a failure

During execution of issue #230, the Phase 8 toolchain loop failed its first iteration with two
`[Timeout]` expiries, one from each of the two test classes
(`QfcItemController_InitializationTests` / `.Part2.cs` and `QfcItemController_SeamFactoryTests`)
that swap this static via their own `SwapUiThreadDispatcher`/`PumpHarness` fixture. Under
class-level parallelization, one class's restore reverted the process-wide static to a parked,
never-pumped dispatcher while the other class's member under test was still awaiting a dispatcher
operation, producing a deadlock. That failure was diagnosed as a genuine isolation defect and was
fixed *locally* in the #230 fixture (`QfcItemController.InitializationTests.Part2.cs`) with a
private static `SemaphoreSlim(1,1)` gate (`UiThreadDispatcherGate`) held from fixture build
through an idempotent `PumpHarness.Restore()`.

The shared helper `EnsureUiThreadDispatcher` was not changed by that local fix and carries no gate
and no restore. It is already called (discarding its `void` return) from
`QuickFiler.Test/Controllers/QfcItemController.FocusAndThemeTests.cs` (two call sites, lines 452
and 468) — a file this feature does not own and must not edit (see Constraints below). Any future
test that calls `EnsureUiThreadDispatcher` reintroduces the leak hazard for the remainder of the
process's test run.

## Promotion

- Potential entry: pre-existing, `docs/features/potential/promoted/2026-08-07-uithread-dispatcher-static-swap-no-restore.md`.
- GitHub issue: pre-existing, #493, created 2026-08-08.
- Active feature folder: created this run via `mcp__drm-copilot__new_active_feature_folder`
  (`feature_name=quickfiler-test-uithread-dispatcher`, `type=bug`, `work_mode=full-bug`,
  `issue_number=493`).

## Constraints (from delegation)

- Files this feature owns: `QuickFiler.Test/Controllers/QfcItemController.TestSupport.cs`,
  `QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part2.cs`, and — only if the
  fix genuinely requires it — `UtilitiesCS/Threading/UiThread.cs`.
- This feature must not write any QuickFiler production source. Every
  `QuickFiler/Controllers/QfcItemController.*` production partial belongs to sibling epic
  features #484, #444, or #489.
- `QuickFiler.Test/Controllers/QfcItemController.FocusAndThemeTests.cs` is a sibling-owned test
  file that calls `EnsureUiThreadDispatcher()` and discards its return value; it is out of scope
  for this feature and must not be edited. See `spec.md` § Root Cause Analysis / Proposed Fix for
  how the design keeps that call site source-compatible without depending on it to cooperate.
- Base branch: this worktree branched from the epic integration base
  `epic/quickfiler-bug-family-integration` (identical to `origin/main` at `988e819b`).
