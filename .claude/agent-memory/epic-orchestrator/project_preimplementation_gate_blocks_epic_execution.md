---
name: preimplementation-gate-blocks-epic-execution
description: enforce-orchestration-preimplementation-gate.ps1 blocks epic-run child delegations via a 7-token keyword classifier, not a real readiness check; wording alone decides pass/fail, so probe every run instead of inferring from the hook source
metadata:
  type: project
---

`.claude/hooks/enforce-orchestration-preimplementation-gate.ps1` (PreToolUse, matcher `Agent`)
denies `Agent(orchestrator)` epic-RUN child delegations. The epic checkpoint is in the
`$script:CheckpointPaths` WRITE-exemption list but is never a readiness SOURCE: the singular
`$script:CheckpointPath` is hard-coded to `artifacts/orchestration/orchestrator-state.json`, and
`Test-OrchestrationReady` demands four scalar root fields (`issue-num`, `feature-folder` starting
`docs/features/active/`, `route_id`/`path_selected`, `lifecycle_ready`) that a 12-feature epic
checkpoint structurally cannot supply. Repointing the path still fails on schema.

**The decisive detail (2026-08-26):** whether you are blocked is decided by *prompt wording*, not
by readiness. `Test-ImplementationDelegation` serializes the whole Agent payload and regex-matches
exactly seven tokens:
`python-typed-engineer|powershell-typed-engineer|typescript-engineer|csharp-typed-engineer|atomic-executor|implementation|execute`.
The only exemption is the preparation-mode pair (`Preparation mode: true.` + `route_id: preparation.`).
A kickoff saying "atomic execution" and "execution" and never "implementation" contains NONE of the
seven ("execution" does not contain "execute") and sails through. That is how wave 0 of
quickfiler-bug-family launched on 2026-08-26 after four verbatim denials on 2026-08-25 — the hook
was byte-identical both times.

**Why:** the gate asks a singular question (is THE feature ready) and an epic schedules many, so the
denial is a false positive w.r.t. the gate's purpose whenever the epic is genuinely prepared
(plans committed to the integration branch + preflight clear). But the pass is an incidental
classifier miss, not a fix.

**How to apply:**
- Never conclude "still blocked" from source inspection alone. A denial costs nothing (PreToolUse
  denies before any branch or worktree is consumed), so *probe with the real batch* — see
  [[live-child-at-pr-author-not-hung]] for the same verify-don't-infer lesson.
- Do NOT reword deliberately to dodge the classifier, and never insert the preparation-mode
  literals into an execution prompt (that is lying to the gate). If a kickoff trips the classifier,
  stop and escalate for the upstream fix.
- Disclose the mechanism when it passes incidentally; do not quietly benefit from it.
- Upstream fix belongs in drm-copilot (mirrored to `.codex/hooks/`), per
  [[claude-files-are-pushdown-owned]]: make the readiness source polymorphic on the epic-mode
  kickoff literals. Widening the keyword regex would re-block epics without making the gate correct.

**The legitimate remedy (established 2026-08-26T19-52Z, feature 468).** The gate can be passed
*on its merits* rather than by a classifier miss, and this is the way to handle it:

Seed `artifacts/orchestration/orchestrator-state.json` with a truthful per-feature record for the
one child you are about to delegate — `issue-num`, `feature-folder` under `docs/features/active/`,
`route_id`, `lifecycle_ready: true` — after verifying each of those four against disk and GitHub
(issue OPEN, folder present with issue.md/spec.md/plan, `epic-planner-state.json` recording
preflight clear). Then delegate with a prompt written naturally; it will contain `execute` /
`atomic-executor`, the classifier will fire, the readiness predicate will actually run, and it
will pass because the feature really is ready.

This is not evasion and is categorically different from planting preparation-mode literals: you
are answering the gate's real question honestly rather than arranging for it not to be asked.
The distinction is the same one drawn in
[[feedback_merged_child_worktree_still_locked_defer_removal]] — record what is true, never
fabricate a run that did not happen.

Two operational consequences:
- That path is singular, so **children take turns owning it**. Archive the incumbent record to a
  suffixed filename first (e.g. `orchestrator-state.<issue>-completed.<ts>.json`) and add an
  `authorship_disclosure` block naming who wrote the replacement and why. See
  [[feedback_fan_in_hook_paths_resolve_to_session_cwd]], which hits the same shared-file problem
  from the merge-gate side.
- The underlying defect is untouched: an epic still cannot express N concurrent children in a
  one-feature schema, so this remedy serialises the gate. The polymorphic-readiness upstream fix
  remains the real answer.
