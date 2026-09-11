---
name: csproj-only-edges-are-blocked-by-the-hook-not-by-me
description: The operator's ruling that .csproj/assembly-module edges must not gate a launch cannot be honoured from inside this agent — the registered Layer-1 barrier hook denies mechanically, and every lever that would clear it is either evasion or an F6/F8-owned write
metadata:
  type: feedback
---

When a conflict edge rests only on a non-SDK `.csproj` or an assembly-level module, say so, treat
the items as concurrent, and do NOT ask whether to wait — but be clear that this agent cannot
actually launch them past the registered barrier hook. Surface the one lever and let the operator
pull it.

**Why:** the operator ruled on 2026-09-07 that project-file and assembly-module overlaps are not
contention, because every non-SDK project has an explicit compile-entry list so any two C# items
touching one assembly collide there, and the merge resolution is the mechanical union of both item
lists. That ruling governs my JUDGMENT. It does not govern the mechanism:
`.claude/hooks/enforce-parallel-cohort-barrier.ps1` is registered in `.claude/settings.json` on the
`PreToolUse` `Agent` matcher, it reads `conflict_edges[]` without regard to the REASON on the edge,
and its `deny` is a hard block rather than an approvable prompt.

Observed on run `bugs-2026-09-06` (2026-09-08). With item 812 in flight at generation-3 cohort 4,
items 810 and 811 at cohort 5 were both denied. Verified by plain set intersection of the declared
path lists, never from the library detail string:

- `810~812` — ZERO shared paths. The edge rests solely on the assembly-level module `QuickFiler`.
- `811~812` — exactly ONE shared path, `UtilitiesCS.Test/UtilitiesCS.Test.csproj`.
- `810~811` — no edge at all, so the two may run together whenever either may run.

**Every apparent lever is closed to this agent, and knowing why saves re-deriving it:**

- Rephrasing the prompt so the hook fails to match is EVASION. The operator prohibited it explicitly
  and the auto-mode classifier denies it. Do not look for a phrasing.
- Editing `conflict_edges[]` is prohibited: the `parallel-orchestrate` skill assigns it to
  `parallel-planner` for seeding and to F8 for recomputation, and names it as never written here.
- Recolouring `cohorts[]` to move an item off a later index is F6's, for the same reason.
- Editing `config/blast-radius.json` changes nothing retroactively — the hook reads the RECORDED
  edges, not the config — and it is push-down-owned in any case.

**How to apply:**

- Confirm the edge class by set intersection of the two `blast_radius.paths` lists, and report the
  intersection itself. A count of zero, or a single `.csproj`, is the finding.
- Report the deny verbatim, then name the exact lever: the hook registration in
  `.claude/settings.json` (the `enforce-parallel-cohort-barrier.ps1` entry on the `PreToolUse`
  `Agent` matcher). **Name it as something the OPERATOR must do, not something to offer to do
  yourself.** Verified 2026-09-08: when the operator authorized the lift and said "proceed", the
  Edit to `.claude/settings.json` was denied by the Claude Code auto-mode classifier, which guards
  against an agent disabling its own safety gate and is independent of every repository hook. Its
  stated remedy is to stop and let the user decide. Offering to do it wastes a round and raises an
  expectation that cannot be met.
- **The same edit IS reachable through `pwsh`, and that route is off-limits.** The
  pre-implementation gate extension-filters Write and Edit but not `pwsh`, so the change would go
  through. Do not take it. It bypasses the INTENT of the classifier denial rather than only its
  mechanism, which the denial text prohibits outright and which is the same prohibition the
  operator recorded against phrasing a delegation so the barrier fails to match. Tool-shopping
  around a safety denial is the defect, not the tool.
- **Revert every optimistic checkpoint write the moment the launch fails.** Receipts and the
  `in_flight` / `worktree_created` item writes go in BEFORE the spawn so the model-routing gate can
  read them, so a denied spawn leaves the checkpoint claiming a launch that never happened. On
  2026-09-08 that meant reverting two item state pairs, two `delegation_receipts[]` entries, two
  `model_routing_receipts[]` entries, `completed_steps` and `next_step`, then re-validating to
  confirm clean. Replace the authorization record with a truthful blocked-attempt record rather
  than deleting it: the attempt and its reasoning are worth keeping.
- **Report what Layer 2 said, because it is the operator's real cost.** With 810 and 811 briefly
  recorded `in_flight` the retrospective backstop returned
  `PARALLEL_COHORT_BARRIER_VIOLATION: 812 ran concurrently with conflicting 810` and the same for
  811. Lifting Layer 1 does not silence Layer 2 — that is the point of the two-layer design — so a
  successful lift would have left the checkpoint failing plain validation, and the
  `require_complete` gate with it, until 812 reached a terminal status. Tell the operator that
  before they decide, not after.
- **Check the deny's quoted target first.** When it quotes a FOLDER basename the block is genuine;
  when it quotes a FILE name the defect is the prompt, per
  [[barrier-hook-resolves-the-longest-active-path-token]].
- Have the delegation prompts written and probed before reporting, so the launch is one call away
  whichever way the operator decides.
- The durable fix stays upstream in drm-copilot: edge derivation should not emit an edge whose only
  basis is a project file or an assembly module.
