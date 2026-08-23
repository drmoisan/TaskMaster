# P0-T3 — Feature Inputs Read

Timestamp: 2026-08-08T20-40

Absolute paths read:

1. `C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a406ae4b7a2ce151f\docs\features\active\2026-08-08-ribbon-engine-toggle-state-guards-505\spec.md`
2. `C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a406ae4b7a2ce151f\docs\features\active\2026-08-08-ribbon-engine-toggle-state-guards-505\issue.md`
3. `C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a406ae4b7a2ce151f\docs\features\active\2026-08-08-ribbon-engine-toggle-state-guards-505\research\2026-08-08T19-30-ribbon-engine-toggle-state-guards-research.md`

## Resolved work mode and AC source

Work mode: **`full-bug`**, resolved from the persisted marker `- Work Mode: full-bug` in
`issue.md` line 11 (and restated in `spec.md` line 9). Per
`.claude/skills/acceptance-criteria-tracking/SKILL.md`, `full-bug` resolves the acceptance-criteria
source to **`spec.md` only**. No `user-story.md` exists in this feature folder and none is to be
created.

AC count: **23** criteria, `AC-1` through `AC-23`, in the `## Acceptance Criteria` section of
`spec.md`. **AC-22 is MANUAL-ONLY** and must remain `- [ ]` at the end of this delivery; it
requires recorded live-Outlook verification and must never be checked off on the strength of unit
tests, source inspection, or any automated artifact.

`issue.md` carries a separate issue-level restatement labelled `AC1`-`AC17` under its own
`## Acceptance Criteria` heading (the `iAC1`-`iAC17` tags used in `spec.md` are the spec's
shorthand for those items; `issue.md` contains no literal `iAC` token). Those seventeen items are
updated in P6-T25. Non-AC checkboxes (`## Impact / Severity`, `## Next Step`,
`## Proposed Fix / Validation Ideas`, `## Logs / Screenshots`) are markers, not criteria, and are
not modified.

## The two-guard-shape split (load-bearing design decision)

The ten unguarded `Controller.Engines.<member>` sites divide into two semantic groups, each with a
different guard shape:

- **Four toggle/getPressed sites** (pre-change lines 120, 123, 189, 192) are backed by engine
  *configuration*, not `InboxEngines`. They route through the new host-neutral
  `EngineToggleStateCoordinator` and must **not** route through `RunEngineCommandAsync`: the
  readiness predicate is wrong for them (an engine configured off is filtered out of
  `InboxEngines` at `AppItemEngines.cs:63-64`, so a readiness-gated toggle could never re-enable
  it), `getPressed` is a synchronous poll for which a notification-per-blocked-call is
  unacceptable, and the two `checkBox` ids cannot join `EngineCommandCatalog` because
  `RibbonExplorerXmlTests.cs:253-273` requires every catalog id to resolve to a `button` element.
- **Six save/info command sites** (pre-change lines 126, 129, 132, 195, 198, 201) are backed by
  `InboxEngines` and no-op when the key is absent, so the existing
  `RibbonController.RunEngineCommandAsync` readiness gate (#503) is semantically exact. They gain
  six `EngineCommandCatalog` entries and matching `getEnabled="EngineCommand_GetEnabled"` XML
  attributes, which must land atomically because the existing set-equality tests derive their
  expectations from `ControlIds`.

This is the read/command asymmetry: reads get a cached-state answer with a silent `false`
default; commands get gated invocation with one user-facing notification.

Binary outcome: PASS.
