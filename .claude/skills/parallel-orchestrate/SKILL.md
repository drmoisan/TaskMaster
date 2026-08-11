---
name: parallel-orchestrate
description: Execute a prepared parallel run for the parallel-orchestrator agent — cohort scheduling under a max_concurrency cap, per-item fan-out onto isolated worktrees branched from origin/main, per-item merge to main after durably confirming CI green, worktree cleanup, and the generated parallel-status.md projection. There is no integration branch and no final integration pull request.
argument-hint: "[parallel-manifest-path | parallel-slug]"
context: fork
agent: parallel-orchestrator
---

# Parallel Orchestrate Skill

A user invocation (`/parallel-orchestrate <parallel-manifest-path>`) forks the
`parallel-orchestrator` agent with this procedure in context. The parallel manifest path (or
parallel slug) for this run is:

$ARGUMENTS

This skill frames work for the `parallel-orchestrator` agent, parallel to how
`.claude/skills/orchestrate/SKILL.md` frames work for `orchestrator`. It documents manifest
consumption, cohort consumption and ordering, the cohort barrier and `max_concurrency` slot
filling, the per-item branch and worktree lifecycle, the child kickoff parameter, model selection,
per-item merge to `main`, per-item merge-conflict handling, worktree cleanup,
`parallel-status.md` maintenance, checkpoint persistence, and completion, so that the procedure is
not re-derived ad hoc on each parallel run.

Each item on this surface ships independently: an item's pull request targets `main` directly.
There is no integration branch, no final integration pull request, and no fan-in path anywhere in
this procedure. Scheduling order comes only from computed blast-radius contention recorded in the
cohort table; there is no `depends_on` field on this surface.

Every section below is self-contained. A cross-reference names another section by its exact
heading text, never by position or number, so a section can be added or extended without
reflowing, reordering, or renumbering anything else in this file. The sections
`## Mutation Protocol (F6)`, `## Enforcement Hooks (F7)`, and `## Radius Drift Detection (F8)`
are reserved for the wave-4 features named in them: each is appended to by its own feature and
must not be relocated.

## Prerequisites

Before proceeding, `parallel-orchestrator` must:

1. Read `CLAUDE.md` for repository tone policy and architectural context.
2. Read the applicable `.claude/rules/` files for the languages in scope, including
   `.claude/rules/parallel-orchestration.md`, which is the schema authority for this surface.
3. Read the policy files listed in the compliance reading order section of `CLAUDE.md`.

## Parallel Manifest Consumption

The run manifest is the YAML frontmatter of `docs/features/parallel/<slug>/parallel.md`, authored
by `parallel-planner` on the planner-owned branch `parallel/<slug>-plan`. That branch is not an
integration branch: no item branch merges into it, it merges into no item branch, and it holds
only run-level artifacts under `docs/features/parallel/<slug>/`.

**This section is not a schema authority.** The manifest schema, the checkpoint schema, and all
nine parallel enums are defined once as prose invariants in
`.claude/rules/parallel-orchestration.md` (manifest invariants M1 through M7) and are enforced by
the F3-owned validators `scripts/dev_tools/parallel_manifest_contract.py` and
`scripts/dev_tools/validate_parallel_orchestrator_state.py`, with the manifest half reachable on the
destination-runtime path as `bash .claude/lib/bash/validate-parallel-manifest.sh`. This is a deliberate delta from
`.claude/skills/epic-orchestrate/SKILL.md`, whose manifest section carries its schema inline. Read
the schema from the rule file and the validators; consume it here and never redefine or extend it.

Consumption rules:

- `issue_num` is the primary key for every item reference. It is the stable GitHub issue number,
  so it does not drift when an item's folder moves from `docs/features/active/` to
  `docs/features/completed/`.
- Read `mode` (`closed` or `open`, defaulting to `closed`), `max_concurrency` (an integer from 1
  through 8, defaulting to 4), and each item's identity and state: `feature_folder`, `kind`,
  `state`, and `blast_radius`.
- The manifest is read-only to `parallel-orchestrator`. It is static input authored by
  `parallel-planner`: never write it, rewrite it, or back-fill a field into it.
- The manifest carries no `depends_on` key at any level and no top-level `integration_branch` key.
  Presence of either is an explicit rejection, not a tolerated extra field.
- A malformed manifest is rejected before any kickoff, recorded as a synthetic Blocking finding in
  the checkpoint. Do not guess a repair, do not silently skip the offending item, and do not launch
  a partial cohort. Validate with the destination-runtime bash entry point, which needs no Python
  interpreter and is published by push-down alongside `.claude`:
  `bash .claude/lib/bash/validate-parallel-manifest.sh docs/features/parallel/<slug>/parallel.md`.
  Its non-zero exit is the rejection signal and its printed error list, one error per line on
  stdout, is the content of the Blocking finding; exit 1 means the manifest is invalid and exit 2
  means the file is unreadable or uses a YAML construct outside the supported subset. Consume
  `mode` and `max_concurrency` through the same entry point's `--print-mode` and
  `--print-max-concurrency` subcommands rather than reading the frontmatter directly.
  `validate_parallel_manifest_text` in `scripts/dev_tools/parallel_manifest_contract.py` remains the
  repository authority and the parity reference. Manifest validation is deliberately not an MCP
  artifact type.

## Cohort Consumption and Ordering

`cohorts[]` is an input, not a computation. Each entry has the shape
`{ index, generation, item_keys[] }` and is seeded by `parallel-planner` at `generation: 0` from
the deterministic Welsh-Powell coloring in `scripts/dev_tools/parallel_cohort_computation.py`.

`parallel-orchestrator` consumes the recorded partition and never computes or recolors one:

- Cohort computation belongs to the cohort-scheduler library that `parallel-planner` calls.
- Recoloring after a membership change belongs to F6; recoloring after a drift event belongs to F8.
- Read `recolor_generation` and schedule only from the cohorts whose `generation` equals it.

Items within a cohort are non-conflicting by construction — a cohort is an independent set in the
conflict graph — so they may branch from the same `main` tip and may merge in any order. After one
same-cohort item merges, another same-cohort item's pull request, based on the older tip, remains
mergeable, and GitHub produces a merge commit.

The conflict graph the cohorts project was computed by a contention relation that fails closed.
Since the blast-radius corrections of issue #452 (merged as PR #453), that relation reports more
contention rather than less: separator-free repository-root shared surfaces are reached from plan
and specification text, and path comparison honours listed-directory prefixes on both sides. Build
on that corrected behaviour. Never narrow a radius, re-derive a partition, or reinterpret an edge
in order to combine two cohorts or widen a launch batch.

## Cohort Barrier and Max-Concurrency Slot Filling

Two independent controls govern every launch. The cohort barrier governs when a cohort may start.
`max_concurrency` governs how many items of a started cohort run at once. Neither substitutes for
the other.

**Cohort barrier.** Cohort `N+1` branches from `main` only after every cohort-`N` item is `merged`
or `worktree_removed`. Increment `current_cohort` only on durable confirmation from
`git worktree list --porcelain`, `git branch`, and
`gh pr view --json state,mergedAt,headRefOid` — never from an in-memory completion notification. A
blocked item (`blocked_ci_loop_limit` or `blocked_drift`) is neither `merged` nor
`worktree_removed`, so a blocked item holds the barrier and cohort `N+1` does not start.

**`max_concurrency` slot filling.** `max_concurrency` caps the number of simultaneously in-flight
items independently of cohort size: a cohort of twelve items executes at most `max_concurrency`
items at a time. Fill slots in ascending item-key order, keyed on `issue_num`, and refill each
freed slot with the next unstarted item of the current cohort in that same ascending item-key
order. A cohort larger than `max_concurrency` therefore launches in several batches from the same
recorded `main` tip. The batching is a pure function, reached on the destination-runtime path as
`bash .claude/lib/bash/compute-concurrency-batches.sh --keys "<k1> <k2> ..." --max-concurrency <n>`.
It prints a compact JSON array of arrays, returns the batches in order, and sorts the keys itself,
so determinism does not depend on caller ordering.
`compute_concurrency_batches(cohort_item_keys, max_concurrency)` in
`scripts/dev_tools/parallel_cohort_computation.py` remains the repository authority and the parity
reference.

**Mechanical enforcement of the barrier is F7 scope, not this feature's.** F7 delivers a two-layer
design, because no single `PreToolUse` hook can validate a batch of concurrent `Agent` calls: hooks
fire per call with no cross-call state visibility.

- Layer 1, per-call deterrent: `.claude/hooks/enforce-parallel-cohort-barrier.ps1`, a `PreToolUse`
  hook on the `Agent` matcher. It fires when `subagent_type == "orchestrator"` and the serialized
  prompt carries the parallel kickoff marker, resolves the target item, reads
  `artifacts/orchestration/parallel-orchestrator-state.json`, and denies with reason
  `PARALLEL_COHORT_BARRIER_BLOCKED` unless every conflicting item in a prior cohort is `merged` or
  `worktree_removed`.
- Layer 2, retrospective backstop: a cohort-ordering invariant inside
  `validate_parallel_orchestrator_state_text`, enforced at `parallel-orchestrator` `SubagentStop`
  time, appending `PARALLEL_COHORT_BARRIER_VIOLATION` when the ordering invariant is violated.

Neither layer is shipped by this feature; both are named here so the obligation is legible to an
operator and to the F7 planner. Until F7 lands, the barrier is enforced by this procedure alone.

## Per-Item Branch and Worktree Lifecycle

1. Run one `git fetch origin main` immediately before each cohort launch, so every item in that
   cohort branches from the same current remote `main` tip rather than from a stale local ref.
   Record the fetched tip.
2. Each item's worktree is created by that item's delegation spawn,
   `Agent(orchestrator, isolation: "worktree", run_in_background: true)`, branched from
   `origin/main`. Do not create or check out item worktrees by hand.
3. `parallel-planner` created and pushed each item's feature branch from `origin/main` at
   preparation time, and that branch is reused unchanged as the item's execution branch, so the
   spawn checks out an existing branch whose base is `origin/main`. Because that base may be older
   than the fetched tip, the item's own run reconciles its branch against the fetched `origin/main`
   tip at execution start. Real path overlap that survives that reconciliation is drift; the
   conflict outcome is handled per `## Per-Item Merge-Conflict Handling`, and drift recording
   itself belongs to F8.
4. Each item's pull request base branch is `main`. Record `worktree_path`, `branch_name`,
   `pr_number`, and `pr_url` for the item, and set `merge_status: worktree_created` at spawn.
5. Worktree removal is the terminal step of this lifecycle and is specified in
   `## Worktree Cleanup`.

No integration branch is created, fetched, pushed, or referenced at any point in this lifecycle.
There is no final integration pull request and no fan-in path: the epic surface's
integration-branch lifecycle has no counterpart on this surface, and its absence is structural
rather than an omission.

## Parallel-Mode Kickoff Parameter

When `parallel-orchestrator` delegates an item to `Agent(orchestrator)`, the delegation prompt
carries exactly these five elements.

1. The literal marker line:

   > `Parallel mode: true. parallel_slug: <slug>. parallel_checkpoint_path: artifacts/orchestration/parallel-orchestrator-state.json. cohort_index: <n>. PR base branch MUST be main; pass --base main to gh pr create.`

   The token `Parallel mode: true` must appear exactly: it is the marker F7's Layer 1 barrier hook
   matches on. The clause `PR base branch MUST be main` is the child's explicit base-branch
   instruction, recorded as prompt text rather than left to a base-branch ancestry heuristic.
2. The item's active feature folder path, written literally as `docs/features/active/<basename>`.
   The child needs it for its own operation, and F7's Layer 1 hook resolves the target item by
   scanning the prompt for exactly that path shape, so the path is emitted as a bare path token.
3. The canonical issue number line, which is the item key.
4. The item's committed `plan-path`, together with the resume instruction: resume at atomic
   execution from that plan rather than re-running promotion, research, or planning. The item's
   prepared feature folder and approved atomic plan are already committed and preflight-clear on the
   item's own pushed feature branch.
5. The model-budget marker line `model_budget.fable_policy: <disabled|available|preferred>.`

Spawn parameters, passed on the `Agent` call and never written into the prompt text:
`isolation: "worktree"`, `run_in_background: true`, branch base `origin/main`, and `model` equal to
that item's model routing receipt's resolved model.

Negative obligations on the prompt:

- It never carries `Preparation mode: true`. Preparation fan-out belongs to `parallel-planner` and
  to F6's admission path; an item that reaches this procedure is already prepared.
- It never carries the epic-mode marker line that `.claude/skills/epic-orchestrate/SKILL.md`
  emits — the marker whose text is `Epic mode` followed by the value `true` — so the epic
  wave-barrier hook does not fire on a parallel child. That marker's value is deliberately not
  written out anywhere in this file, so no file this feature delivers carries an epic-mode marker
  string.
- It contains no instruction for the child to merge its own pull request. The parent performs each
  item's merge, as specified in `## Per-Item Merge to Main (Merge-on-Green)`.

Excluded from the prompt as parent-side concerns: the item's declared blast radius,
`max_concurrency`, and `mode`. Keeping the prompt minimal preserves the child contract unchanged.

## Model Selection

When `parallel-orchestrator` delegates an item to `Agent(orchestrator)`, the prompt appends the
session model-budget kickoff marker line, following the existing kickoff-marker pattern:

> `model_budget.fable_policy: <disabled|available|preferred>.`

The item's own `orchestrator` reads that line and applies the two-axis model-selection mechanism
documented in `.claude/skills/orchestrate/SKILL.md` (`## Model Selection`): it assesses a
judgment-based `complexity_band`, records `complexity_assessments[]` and
`model_routing_receipts[]`, and resolves each of its own delegations' model tier under the given
`fable_policy`. The canonical, tested reference implementations are `Get-ComplexityFloor` and
`Resolve-DelegationModel` in `.claude/lib/model-routing/ModelRouting.psm1`. Default `fable_policy`
is `disabled` when the marker is absent.

`parallel-orchestrator` spawns exactly one delegation channel, `Agent(orchestrator)`, one
delegation per item. It applies the same per-delegation resolution to that channel and passes
`model` equal to the routing receipt's `model` on the spawn call. It MUST NOT omit `model` — an
omitted `model` falls back to the delegate's frontmatter default, `opus`, which suppresses a
`fable` resolution — and MUST NOT hard-code `model=opus` in a way that overrides the resolved
routing model.

`route` is never an input to model selection. `route` remains file-count driven and governs only
agents, skills, and MCP tools. A skill whose frontmatter `context` field holds the value `fork`
inherits the parent model and ignores a model override, so model selection applies to agent
delegations, not to fork-routed skill invocations.

## Per-Item Merge to Main (Merge-on-Green)

The parent — `parallel-orchestrator` — performs each item's merge to `main`. The child contract is
unchanged: `.claude/skills/orchestrate/SKILL.md` is **not modified by this feature**. There is no
`parallel_mode` clause in its step 9, no `parallel_merge` object in the child checkpoint, and no
additional condition on the child's PR Creation Gate.

Procedure, per item:

1. The item's child orchestration runs unmodified, with `epic_mode` `false` or absent, and finishes
   at its own DONE. The child's PR Creation Gate condition 6 already requires
   `ci_gate.conclusion == "success"`, so at child DONE the item's pull request is open against
   `main` and its checks are green. The child is never instructed to merge its own pull request.
2. On child completion, durably confirm pull-request state and check conclusion with
   `gh pr view --json state,mergedAt,headRefOid`, and with `gh pr checks` when the check conclusion
   must be re-read — never from an in-memory completion notification — then record
   `merge_status: ci_green`.
3. Execute `gh pr merge --merge <PR>` for that item's pull request, whose base is `main`.
4. On success, record `merge_commit_sha`, `merged_at`, and `merge_status: merged`, then regenerate
   `docs/features/parallel/<slug>/parallel-status.md`.
5. On a merge failure caused by a conflict, follow `## Per-Item Merge-Conflict Handling`.

**F7 dependency.** `.claude/hooks/enforce-epic-merge-gate.ps1` is a project-wide `PreToolUse`
Bash-matcher hook that denies any `gh pr merge --merge` unless an epic-shaped checkpoint satisfies
its allow conditions; its block reason is `EPIC_MERGE_GATE_BLOCKED`. A parallel run has no
epic-shaped checkpoint, so step 3 above is denied until F7 scopes or extends that gate's allow
conditions for the parallel case. This feature modifies no file under `.claude/hooks/` and does not
change `.claude/settings.json`, so the parallel surface is not executable end-to-end before F7
lands. That limitation is documented, not worked around.

Branch protection on `main` affects only the pacing of step 3, not its ownership: if `main`
requires branches to be up to date, an automated `gh pr update-branch` plus re-green cycle is
inserted between same-cohort merges, which serializes those merges in practice while remaining
unattended.

## Per-Item Merge-Conflict Handling

Remediation is child-owned and parent-initiated. The conflict is always between one item's own
branch and `origin/main`; there is no integration branch and therefore no fan-in conflict path on
this surface.

1. On a conflicted `gh pr merge --merge`, the parent detects the failure and re-delegates that item's
   child orchestration, passing the conflict signal and the instruction to resolve against
   `origin/main`. The conflict capture and the finding write both belong to the child's
   `atomic-executor`, which works inside the item's own worktree, the only working tree holding the
   item's branch. It runs `git fetch origin main`, then `git merge --no-commit origin/main`, and on
   non-zero exit captures `git diff --name-only --diff-filter=U` for the conflicted-file list
   together with the raw conflict-marker content of each conflicted file. The child's
   `atomic-executor` then writes that evidence as a synthetic Blocking finding to the item's own
   `remediation-inputs.<timestamp>.md` in the item's active feature folder under
   `docs/features/active/`, not to the run's parallel folder. Assigning both the capture and the
   finding write to the child's chain matches `.claude/skills/epic-orchestrate/SKILL.md`, whose
   equivalent capture and finding write also belong to the child's `atomic-executor`.
2. The parent re-delegates that item's child orchestration. The child processes the finding through
   its unmodified R1 through R5 remediation loop exactly as it processes any local Blocking
   finding. No new remediation loop is introduced by this procedure.
3. The child's `remediation_pass` counter is shared with its local-finding and CI-failure passes,
   with the cap of 3, unmodified.
4. Each remediated pass ends again at child DONE with the pull request open and CI green, after
   which the parent retries the merge per `## Per-Item Merge to Main (Merge-on-Green)`. During
   remediation the item's `merge_status` legitimately remains `pr_open` or `ci_green`: the
   `merge_status` enum is F3-owned, carries no conflict member, and is not extended by this
   feature.
5. On loop exhaustion, the parent records the terminal `merge_status: blocked_ci_loop_limit` for the
   item; the child's own checkpoint retains its precise blocked status. A blocked item is neither
   `merged` nor `worktree_removed`, so it holds the cohort barrier defined in
   `## Cohort Barrier and Max-Concurrency Slot Filling`.

Boundary with F8: a merge conflict between two same-cohort items is evidence that the declared
blast radius under-reported, and this feature records the child's blocked or remediated outcome
only, leaving drift recording in `drift_events[]`, quiesce of admission, conflict recomputation
against the observed radius, and requeue of the later-started item to F8.

## Worktree Cleanup

After an item reaches `merge_status: merged` and that state is durably confirmed by
`gh pr view --json state,mergedAt,headRefOid`, the parent — running from the main repository
checkout, never from inside a child worktree — issues `git worktree remove <worktree_path>`. On
success it records `merge_status: worktree_removed` and `worktree_removed_at`, then regenerates
`docs/features/parallel/<slug>/parallel-status.md`.

Mechanical gating of this command for parallel worktrees is F7 scope.
`.claude/hooks/enforce-epic-worktree-removal-gate.ps1` is a project-wide `PreToolUse` Bash-matcher
hook that denies any `git worktree remove` unless the epic checkpoint carries a matching
`features[]` record whose `merge_status` is `merged` or `worktree_removed`; an unreadable checkpoint
or an absent record also denies. Its block reason is `EPIC_WORKTREE_REMOVAL_BLOCKED`. A parallel run
has no epic checkpoint record for its worktrees, so removal is denied until F7 both delivers
`enforce-parallel-worktree-removal-gate.ps1` and coordinates the epic gate's allow conditions:
`PreToolUse` denials are conjunctive, so a new allow-hook alone cannot override the existing deny.
This feature ships no hook file and makes no `.claude/settings.json` change.

## Documentation Maintenance Boundaries

`docs/features/parallel/<slug>/parallel-status.md` is a generated projection of
`artifacts/orchestration/parallel-orchestrator-state.json`. It is regenerated in full, is never
hand-authored, and is never treated as an input. It is never the source of the cohort table and
never the source of the schedule: the run manifest and the checkpoint are authoritative. Generate it
from the template `docs/features/templates/parallel/parallel-status.md`.

Header block fields: `parallel_slug`, `mode`, `max_concurrency`, `current_cohort`,
`recolor_generation`, `last_updated`.

Item table: one row per `items[]` entry, carrying `issue_num`, `feature_folder`, cohort index,
lifecycle `state`, `merge_status`, `pr_url`, `merge_commit_sha`, and the item's lifecycle
timestamps. The cohort column takes the place of the epic status document's wave column.

Cohort table: a projection of `cohorts[] { index, generation, item_keys[] }`, so a recolored
schedule stays traceable by `generation`.

Read-only projections of F3-owned arrays, rendered by this feature but never written by it: section
`## Conflict Edges` projects `conflict_edges[]`; section `## Mutations` projects `mutations[]`,
whose rows appear only once F6 populates that array; section `## Drift Events` projects
`drift_events[]`, which only F8 populates. An empty array renders an empty section rather than an
omitted one.

Regeneration boundaries — regenerate at each of the following, not only at final completion:

- Run kickoff, seeding the initial projection from the manifest and the seeded cohorts.
- Every item `state` or `merge_status` transition.
- Every cohort transition, meaning every `current_cohort` increment.
- Every `recolor_generation` increment.
- Every append to `mutations[]`.
- Every append to `drift_events[]`.
- Run completion in `closed` mode, or run close in `open` mode.

Defining the `mutations[]` and `drift_events[]` appends as regeneration boundaries here means F6 and
F8 need no amendment to these projection rules.

## Parallel-Level Checkpoint

This section is consumption documentation only. The checkpoint schema is owned by F3, defined once
as prose invariants in `.claude/rules/parallel-orchestration.md`, and enforced by
`scripts/dev_tools/validate_parallel_orchestrator_state.py`. Consume that schema; add no field to it
and extend no enum in it.

Fields `parallel-orchestrator` writes to `artifacts/orchestration/parallel-orchestrator-state.json`:
`objective`, `route_id: "parallel"`, `parallel_slug`, `parallel_manifest_path`,
`parallel_status_doc_path`, `mode`, `max_concurrency`, `completed_steps`, `next_step`,
`last_updated`, `current_cohort`, `recolor_generation`, `cohorts[]`, `items[]` — each item entry
carrying `issue_num`, `feature_folder`, `state`, `blast_radius`, `worktree_path`, `branch_name`,
`pr_number`, `pr_url`, `merge_status`, `merge_commit_sha`, and the lifecycle timestamps — and the
three receipt arrays `delegation_receipts[]`, `skill_receipts[]`, and `mcp_call_receipts[]`,
populated with the `parallel` route's required names from `config/orchestration-routing.json`.

The `merge_status` enum has exactly eight members: `not_started`, `worktree_created`, `pr_open`,
`ci_green`, `merged`, `worktree_removed`, `blocked_drift`, and `blocked_ci_loop_limit`. An absent
`merge_status` is treated as `not_started`.

The transition chain this feature writes: `not_started` to `worktree_created` at the item's
delegation spawn; `worktree_created` to `pr_open` when the child reports its pull request open;
`pr_open` to `ci_green` on durable confirmation after child DONE; `ci_green` to `merged` when the
parent's merge succeeds; and `merged` to `worktree_removed` after gated removal.
`blocked_ci_loop_limit` is the mapped terminal for an exhausted remediation loop.

Never written by this feature: `blocked_drift`, which only F8 writes; `conflict_edges[]`, seeded by
`parallel-planner` and recomputed only by F8; `mutations[]`, which only F6 appends to; and
`drift_events[]`, which only F8 appends to. These are read for projection and for scheduling context
and are otherwise untouched.

The checkpoint is a cache of durable state, not the source of truth. Every recorded field is
re-derivable on resume from `git worktree list --porcelain` for worktree existence and path,
`git branch` for branch existence and name, and `gh pr view --json state,mergedAt,headRefOid` for
pull-request state, merge time, and merge commit. Where the checkpoint disagrees with those three
commands, the commands win and the checkpoint is rewritten from them.

Validate through `mcp__drm-copilot__validate_orchestration_artifacts` with
`artifact_type: "parallel-orchestrator-state"`, or the equivalent CLI invocation
`poetry run python -m scripts.dev_tools.validate_orchestration_artifacts parallel-orchestrator-state <path>`,
adding `--require-complete` at the completion gate.

## Completion Requirements

Completion is mode-dependent. Read `mode` from the manifest: it is `closed` or `open` and defaults
to `closed`.

In `closed` mode, `parallel-orchestrator` must not report completion until all of the following
hold:

1. Every non-withdrawn item has `merge_status` of `merged` or `worktree_removed`, each durably
   confirmed by `git` and `gh` rather than by an in-memory completion notification.
2. `docs/features/parallel/<slug>/parallel-status.md` has been regenerated a final time and reflects
   the completed state.
3. The parallel checkpoint passes validation with `require_complete` for
   `artifact_type: "parallel-orchestrator-state"`.
4. Each item's acceptance criteria have been checked off in that item's own acceptance-criteria
   source files by that item's own run, per the `acceptance-criteria-tracking` skill.

In `open` mode there is no automatic completion. The run is a standing queue and terminates only via
`/parallel-close`, which is owned by F6 and is neither specified nor shipped by this feature. Do not
synthesize a completion condition for an `open`-mode run.

No completion condition involves a run-level pull request. There is no final integration pull
request on this surface, so completion keys on the per-item terminal states above and on nothing
else.

## Mutation Protocol (F6)

A parallel run is mutable while it executes. Three slash commands mutate it, and each one appends
exactly one `mutations[]` entry to the parallel-orchestrator checkpoint on success:

- `/parallel-add <issue|potential-entry>` — `.claude/skills/parallel-add/SKILL.md`. Admits one new
  item: the item enters `proposed`, is prepared through a preparation-mode child
  `Agent(orchestrator)` run reusing the `route_id: preparation` contract unchanged, its conflict
  edges are computed against ALL items including in-flight ones, and the admission decision places
  it in the current cohort only when it conflicts with no member of that cohort, pinned or
  unstarted, and otherwise defers it and recolors the unstarted subgraph.
- `/parallel-remove <item> [--disposition detach|abandon]` —
  `.claude/skills/parallel-remove/SKILL.md`. Removes one item per the state-dependent behavior
  table: an unstarted item is withdrawn and the unstarted subgraph is recolored; an in-flight item
  requires an explicit disposition and is rejected without one; a merged item is rejected.
- `/parallel-close <parallel-slug>` — `.claude/skills/parallel-close/SKILL.md`. Terminates an
  `open`-mode run. Rejected while any item is `in_flight`.

Every mutation re-derives durable state (`git worktree list --porcelain`, `git branch`,
`gh pr view`) before it is applied, because the checkpoint is a cache of durable state and not the
source of truth. A rejected mutation appends no entry and changes no state.

The decision logic for all three commands is the pure engine
`scripts/dev_tools/parallel_mutation_protocol.py`. It decides; it never applies. Item keys are
integers (`items[].issue_num`) everywhere on this surface.

### Pinning invariant

**In-flight items are pinned. Scheduling is recomputed only over the not-yet-started subgraph, and
recoloring is a pure function of `(remaining subgraph, pinned set, pinned cohort index)`.**

The recolor function takes the induced subgraph of unstarted items (states `proposed`, `admitted`,
`prepared`, `scheduled`), the pinned set (state `in_flight`), the current generation, and — as a
third scheduling input — the current cohort index `current_cohort` that the pinned items occupy. It
returns cohort assignments for unstarted items ONLY: the returned mapping's key set equals the
unstarted set exactly and contains no pinned key. A pinned item is therefore absent from the result
rather than reassigned, and that absence IS the guarantee that a mutation never moves work already
running.

**Pinned-barrier offset.** The returned indices are ABSOLUTE checkpoint cohort indices at or above
`current_cohort`, and strictly above `current_cohort` whenever any conflict edge joins an unstarted
item to a pinned item. When no such edge exists the lowest returned index equals `current_cohort`
exactly, so unstarted items may share the running cohort and `max_concurrency` slot filling is
preserved. The offset is a single uniform shift applied to every color class, so F2's distinct color
classes remain distinct cohort indices and independence within the unstarted set is preserved
exactly.

Write the returned indices VERBATIM into `cohorts[].index`; never re-base them to zero. `cohorts[]`
carries exactly ONE current-generation entry per index, so returned keys landing on index
`current_cohort` JOIN the pinned members of that one entry instead of forming a second entry with
the same index, which F3 invariant 13 rejects.

Coloring is delegated in full to the Welsh-Powell entry point
`bash .claude/lib/bash/compute-cohorts.sh --keys "<k1> ..." --edges "<a>:<b> ..."`, the
destination-runtime port of `compute_cohorts` in
`scripts/dev_tools/parallel_cohort_computation.py`, which remains the repository authority and the
parity reference. No part of the coloring, the vertex ordering, or
the tie-break is reimplemented by the mutation engine, and the offset is applied entirely inside the
mutation engine's own recolor function.

**Two design corrections (spec 1.2).** Admission previously checked only the `in_flight` subset, but
`max_concurrency` caps simultaneously in-flight items independently of cohort size and refills each
freed slot from the same current cohort — see
`## Cohort Barrier and Max-Concurrency Slot Filling` — so the current cohort durably holds
not-yet-launched `scheduled` members that a candidate can contend with. Recoloring previously
dropped the candidate-to-pinned edges together with the pinned vertices, which discarded the pinned
CONSTRAINT as well as the pinned VERTICES and returned a deferred candidate to cohort 0, the current
cohort, whenever the cohort barrier held `current_cohort` at 0.

### Recompute boundary

`recolor_generation` increments by exactly one on a recompute and is stamped unchanged on every
other operation. The boundary is normative:

Operations that RECOMPUTE (`recolor_generation` increments by exactly one):

1. **Deferred add** — the candidate conflicts with a member of the current cohort — pinned or
   not-yet-launched — so the unstarted subgraph, including the new item, is recolored.
2. **Remove of an unstarted item** — the vertex is dropped and the remaining unstarted subgraph is
   recolored.
3. **Drift-induced requeue** — the later-started item of a newly conflicting pair is halted and
   requeued into a future cohort.

Operations that DO NOT recompute (generation stamped unchanged):

1. **Admission into the current cohort with no conflict against any current-cohort member** — no
   cohort assignment changes.
2. **`detach`** — the detached item was pinned and was never a vertex of the unstarted subgraph, so
   its departure cannot change the induced subgraph.
3. **`abandon`** — same rationale as `detach`. An unstarted item previously deferred because of a
   conflict with the now-abandoned item keeps its deferred cohort assignment: the assignment remains
   valid and is at most conservative, and no opportunistic recompute is performed. This keeps
   generation accounting minimal and deterministic.
4. **`close`** — run termination changes no cohort assignment.

A non-recompute operation still appends exactly one `mutations[]` entry, stamping the current,
unchanged `recolor_generation`. A sequence of N operations from generation `g` therefore ends at
exactly `g` plus the number of recompute operations.

### Per-op mutation-log entry contents

| Op case | `op` | `item_key` | `prior_state` | `new_state` | `disposition` | `recolor_generation` |
| --- | --- | --- | --- | --- | --- | --- |
| Add, no-conflict admit | `add` | item key | null | `scheduled` | null | `g` (unchanged) |
| Add, deferred | `add` | item key | null | `scheduled` | null | `g` + 1 |
| Remove, unstarted | `remove` | item key | prior state (`proposed`/`admitted`/`prepared`/`scheduled`) | `withdrawn` | null | `g` + 1 |
| Remove, `detach` | `remove` | item key | `in_flight` | `withdrawn` | `detach` | `g` (unchanged) |
| Remove, `abandon` | `remove` | item key | `in_flight` | `withdrawn` | `abandon` | `g` (unchanged) |
| Close | `close` | null (run-scoped) | null | null | null | `g` (unchanged) |
| Drift-induced requeue | `requeue` | item key | `in_flight` | `blocked` | null | `g` + 1 |

`prior_state` is null on BOTH add rows. The accompanying `prepared` -> `scheduled` transition is not
lost and is not recorded in the mutation entry: it is recorded as an item-state update in `items[]`
with the checkpoint's lifecycle timestamps, the same mechanism that records
`proposed` -> `admitted` -> `prepared` during preparation. `new_state` is null for `close` only, and
`item_key` is null for `close` only. `disposition` is non-null only on a `remove` entry whose
`prior_state` is `in_flight`. Every `at` timestamp comes from the engine's injected clock seam; the
engine never reads the wall clock. No field and no enum member is added to `mutations[]`; the nine
parallel enums of `.claude/rules/parallel-orchestration.md` are consumed, never extended.

Retrospective validation of the log is the helper
`scripts/dev_tools/_parallel_orchestrator_state_mutations.py`, wired into
`scripts/dev_tools/validate_parallel_orchestrator_state.py`. It requires every entry to carry all
seven fields and no eighth, and requires `recolor_generation` to be monotonically non-decreasing in
append order, which is what makes a lost update detectable after the fact.

### Mode-dependent completion semantics

- **`closed` mode (the default).** The completion gate fires when every non-withdrawn item is
  `merged` or `worktree_removed`, evaluated over per-item `merge_status`. Mid-execution mutation
  remains permitted. The predicate is `is_closed_mode_complete` in the mutation engine.
- **`open` mode.** The run NEVER auto-completes. It is a standing queue and terminates only via
  `/parallel-close`. The close record is the run's final mutation; nothing may be appended to
  `mutations[]` after it. Do not synthesize a completion condition for an `open`-mode run.

A withdrawn item is exempt from the predicate in both modes: it left the run before reaching a merge
outcome, so requiring a terminal merge status of it would make every run that dropped an item
permanently incompletable. This is also why `detach` records `withdrawn` — the run does not wait for
a detached item.

### Abandon confirmation-marker contract

The `abandon` disposition is destructive: it closes the item's pull request and removes its
worktree. Both side effects run through ONE deterministic CLI invocation of
`scripts/dev_tools/parallel_mutation_abandon_cli.py`, documented in full in
`.claude/skills/parallel-remove/SKILL.md`. Executing the abandon disposition through ad hoc `gh` or
`git` commands is prohibited, because an ad hoc command is not matchable and would bypass the
confirmation contract.

The contract is enforced by the PreToolUse hook
`.claude/hooks/enforce-parallel-abandon-gate.ps1` on the `Bash` matcher:

- A Bash command carrying the abandon disposition token MUST also carry the explicit confirmation
  marker `--confirm-abandon` in the SAME command.
- Without the marker, the command is DENIED with a deny reason prefixed
  `PARALLEL_ABANDON_BLOCKED`.
- With the marker, the command is allowed.
- A command carrying no abandon disposition token is out of scope and is allowed unchanged.

When the gate denies a command, add the confirmation marker deliberately. Do not reformulate the
command to evade the match: that defeats the only mechanism protecting a destructive operation.

### Drift-requeue append contract

Radius drift detection consumes this protocol rather than reimplementing it. When drift halts the
later-started item of a newly conflicting pair, the requeue is recorded through the mutation
engine's `build_requeue_entry` constructor and the recolor through `recolor_unstarted`:

- Item state becomes `blocked` and per-item `merge_status` becomes `blocked_drift`.
- Exactly one `mutations[]` entry is appended with `op: requeue`, `prior_state: in_flight`,
  `new_state: blocked`, `disposition: null`, and `recolor_generation` equal to `g` + 1 — the requeue
  is a recompute.
- The recolor runs over the unstarted subgraph only, so no other in-flight item moves. Its call
  shape is the five-argument form
  `recolor_unstarted(unstarted_items, conflict_edges, pinned, current_generation, current_cohort=current_cohort)`,
  where `current_cohort` is required and keyword-only.

The drift event itself is recorded in `drift_events[]`, which this protocol does not write. See
`## Radius Drift Detection (F8)`.

## Enforcement Hooks (F7)

Three hooks enforce this surface mechanically: two new, one an additive extension of an existing epic
hook. All three fail closed — once a call is in scope, every unresolvable condition (missing or
malformed checkpoint, unresolvable target item, missing `items[]` record, missing `merge_status`)
denies. Out-of-scope calls allow without reading the checkpoint.

**Layer 1, per-call deterrent.** `.claude/hooks/enforce-parallel-cohort-barrier.ps1`, a `PreToolUse`
hook on the `Agent` matcher. It activates only when `subagent_type` is `orchestrator` and the
serialized delegation prompt carries the marker `Parallel mode: true` (emitted per
`## Parallel-Mode Kickoff Parameter`). It then resolves the target from the
`docs/features/active/<folder>` token in the prompt matched against `items[].feature_folder`, reads
`artifacts/orchestration/parallel-orchestrator-state.json`, projects cohorts to the rows whose
`generation` equals `recolor_generation`, and denies with a reason prefixed
`PARALLEL_COHORT_BARRIER_BLOCKED` unless every `conflict_edges[]` neighbour in a strictly prior
current-generation cohort has `merge_status` of `merged` or `worktree_removed`. `ci_green` does not
satisfy the barrier. Same-cohort and later-cohort neighbours do not block Layer 1.

**Layer 2, retrospective backstop.** `validate_cohort_barrier_ordering` in
`scripts/dev_tools/_parallel_orchestrator_state_cohort_barrier.py`, invoked from
`validate_parallel_orchestrator_state_text` and reached at `parallel-orchestrator` `SubagentStop` time
by the `.claude/settings.json` matcher that runs `.claude/hooks/validate-orchestrator-output.ps1` with
`-ArtifactType parallel-orchestrator-state`. It appends exactly one message per violated edge, in the
form `PARALLEL_COHORT_BARRIER_VIOLATION: <a> ran concurrently with conflicting <b>`. The check is
key-gated on `conflict_edges` and `cohorts`, so a checkpoint lacking either key yields zero errors,
and it adds no checkpoint fields.

**Why both layers are required.** A `PreToolUse` hook fires once per tool call with no cross-call or
conversation-state visibility, so it cannot see the rest of a batch of concurrent `Agent` calls and
cannot reject the batch as a whole. The retrospective validator does see the recorded batch, but only
after those calls executed. Layer 1 deters the individual out-of-order launch; Layer 2 detects the
concurrent batch Layer 1 structurally cannot. Removing either layer reopens the gap, so neither
substitutes for the other and neither substitutes for the procedure in
`## Cohort Barrier and Max-Concurrency Slot Filling`.

**Worktree removal gate.** `.claude/hooks/enforce-parallel-worktree-removal-gate.ps1`, a `PreToolUse`
hook on the `Bash` matcher, intercepts `git worktree remove` and matches the normalized target path
against `items[].worktree_path`. Removal is allowed only when that item's `merge_status` is `merged`
or `worktree_removed`; anything else — including an unreadable checkpoint or no matching record —
denies with a reason prefixed `PARALLEL_WORKTREE_REMOVAL_BLOCKED`. Commands that are not
`git worktree remove` always allow. This is the mechanical counterpart to `## Worktree Cleanup`.

**Invocation-origin extension.** `.claude/hooks/enforce-epic-invocation-origin.ps1` was extended
additively so `$script:GatedSubagentTypes` lists `epic-planner`, `epic-orchestrator`,
`parallel-planner`, and `parallel-orchestrator`. An `Agent(parallel-planner)` or
`Agent(parallel-orchestrator)` call whose caller `agent_type` is `orchestrator` is denied with a
reason prefixed `PARALLEL_INVOCATION_ORIGIN_BLOCKED`, because both parallel personas delegate to
`Agent(orchestrator)` and an orchestrator-originated invocation would nest `orchestrator` inside its
own delegation chain; invoke either persona from the main session instead. Main-thread invocations
(absent or blank caller `agent_type`) and non-orchestrator callers continue to allow. Epic behaviour
is unchanged: the `EPIC_INVOCATION_ORIGIN_BLOCKED` reason string is byte-identical for epic targets.

## Radius Drift Detection (F8)

### Radius Drift Detection and Drift Gate

An in-flight item whose actual diff escapes its declared `blast_radius.paths` invalidates the
concurrency guarantee for every item running beside it, which is the dominant failure mode of this
surface and the compensating control for heuristically derived radii. Detection is the
execution-time half of a paired mitigation: F1's plan-time coverage validation bounds
under-reporting when the radius is derived, and this procedure bounds it while the item runs.
Neither half eliminates the risk. Nothing in this section re-derives F1's matcher or F1's
contention relation; both are imported by the implementation.

#### Seven-Step Procedure

1. Compare the observed changed-path list against the item's declared `blast_radius.paths`.
2. On escape, record one `drift_events[]` entry and raise a synthetic Blocking finding in the
   child's own `remediation-inputs.<yyyy-MM-ddTHH-mm>.md`.
3. Quiesce: suspend admission of new items into the current cohort.
4. Recompute conflicts using the observed radius in place of the drifting item's declared radius.
5. If the escape newly conflicts with a concurrently in-flight item, halt the later-started item of
   that pair, record `merge_status: blocked_drift` with item `state: blocked`, and requeue it into a
   future cohort.
6. The child's existing R1 through R5 remediation loop processes the finding unmodified.
7. **Resolve the recorded drift.** Actor: the `parallel-orchestrator`. Trigger: the consuming
   remediation cycle exiting with `blocking_count == 0`. The parent then performs exactly one of two
   writes to the item's `items[].blast_radius`. Either it re-records the radius from the
   post-remediation diff — the library-built value the detecting invocation already emitted as the
   `observed_radius` payload key, carrying `source: observed` and a `computed_at` that must be
   strictly later than the event's `at` — or, when remediation widened the declared radius instead of
   narrowing the diff, it extends `blast_radius.paths` so every `escaped_paths` entry of the latest
   event is covered. The radius is never hand-constructed; it is the value the command line emitted,
   so the module and shared-surface levels are resolved by F1's library. **No other write clears the
   derived unresolved state**: appending an event cannot, because the action enum has no `resolved`
   member and a zero-escape event is rejected, and changing `merge_status` or item `state` cannot,
   because the derivation reads only `blast_radius`.

Deferral of admission into a **future** cohort remains allowed during quiesce; only admission into
the current cohort is suspended.

#### Child-Side Evaluation Point

Detection is evaluated at each child's pre-feature-review commit: the moment inside the child
orchestrator's Pre-Feature-Review Commit step between the successful commit and the `feature-review`
delegation. Evaluating after the commit is what makes the observed diff complete; evaluating before
the delegation is what lets the synthetic finding reach the same review pass.

The evaluation is active only for a child whose delegation prompt carries the `Parallel mode: true`
marker emitted by `## Parallel-Mode Kickoff Parameter`. A child running outside a parallel run
carries no such marker, evaluates no drift, and is unaffected.

#### CLI Invocation

Detection logic is pure and lives in `scripts/dev_tools/parallel_drift_detection.py` (escape
detection, `drift_events[]` construction, the derived quiesce predicate, and conflict recomputation)
and `scripts/dev_tools/parallel_drift_halt.py` (halt selection and the requeue seam). All I/O is
confined to the thin wrapper `scripts/dev_tools/parallel_drift_detection_cli.py`, invoked as:

```
poetry run python -m scripts.dev_tools.parallel_drift_detection_cli \
  --item-key <issue_num> \
  [--checkpoint artifacts/orchestration/parallel-orchestrator-state.json] \
  [--config config/blast-radius.json] \
  [--at <yyyy-MM-ddTHH-mm>] [--computed-at <yyyy-MM-ddTHH-mm>] \
  <CHANGED_PATH>...
```

Argument surface: `--item-key` is the only required argument and is the item's `issue_num`;
`--checkpoint` and `--config` default to the two paths shown; `--at` is the timestamp recorded on
the `drift_events[]` entry and `--computed-at` the timestamp recorded on the observed radius, each
defaulting at the I/O boundary so the pure functions never read a clock; and the changed paths are
positional and variadic. An empty changed-path list is legal and yields `no_escape`.

The changed-path list is an argument, not something the module derives: the caller produces it with
`git diff --name-only <merge-base(origin/main, HEAD)> HEAD` at the child's pre-review commit, and
the module executes no git command of its own. Merge-base semantics matter — comparing against the
merge base rather than against the current `origin/main` tip keeps a concurrently merged peer item's
files from appearing as spurious drift. A rename lists both the old and the new path, and both must
be covered, which is the fail-closed direction.

Stdout carries exactly one JSON object; errors go to stderr only, so stdout is unconditionally
parseable:

```json
{
  "result": "no_escape | no_new_conflict | halt_required",
  "item_key": 0,
  "at": "",
  "computed_at": "",
  "escaped_paths": [],
  "newly_conflicting_pairs": [],
  "halted_item_keys": [],
  "drift_event": null,
  "observed_radius": null
}
```

`result` is the verdict: `no_escape` means the diff stayed inside the declared radius;
`no_new_conflict` means paths escaped but the observed radius introduced no contention, so nothing
is halted; `halt_required` means at least one pair newly conflicts and `halted_item_keys` names the
later-started item of each. `newly_conflicting_pairs` holds ascending canonical `[a, b]` item-key
pairs, the same edge identity `conflict_edges[]` records, so a recomputed pair is comparable with a
recorded one without normalization. `drift_event` is `null` exactly when `result` is `no_escape`,
because an event with zero
escaped paths is not a drift event. `observed_radius` is the serialized observed `blast_radius` the
parent writes back in step 7 of `#### Seven-Step Procedure`: it carries the six invariant-9 keys with
`source: observed`, is built by F1's library rather than by hand, and is `null` exactly when `result`
is `no_escape`, on the same precondition as `drift_event`. Exit status is `0` on success, `1` on
missing or malformed input, and argparse's `2` on a usage error.

#### Synthetic Blocking Finding

The finding is written to `docs/features/active/<child-slug>/remediation-inputs.<yyyy-MM-ddTHH-mm>.md`
— the flat form directly in the item's own active feature-folder root, matching the existing
merge-conflict finding precedent rather than any folder-per-cycle variant.

It is written by `parallel-orchestrator`, which detects the escape and owns the checkpoint, reaching
the child's checkout through the item's recorded `items[].worktree_path`. That field is optional in
the schema and may be null; an absent value means the finding cannot be written, which the drift
gate treats as unwritten.

The file must contain the literal line `- Severity: Blocking`, matched case-sensitively by the child
orchestrator's post-review outcome evaluation, together with the escaped paths, the declared
patterns, and the required action.

#### Halt the Later-Started Item

The later-started item of a newly conflicting pair is halted, and **the drifting item is never the
one halted**. Halting the drifting item is not an option: it must not be implemented, and it must not
be offered as a configuration. The prohibition is unconditional and holds even when the drifting item
is the later starter by either tie-break. Two reasons make it necessary: the drifting item's work is
already broader than planned and is more expensive to unwind, and the drifting item is mid-remediation
on its own R1 through R5 loop for the drift finding, so halting it would deadlock the very remediation
that resolves the drift.

The exclusion is applied **at the call site, before the later-started comparator runs**:
`halted_item_keys` in `scripts/dev_tools/parallel_drift_detection_cli.py` drops the drifting key from
each pair's candidate list, then halts the single remaining candidate, or applies the comparator when
two remain. Because a recomputed pair holds two distinct canonical keys, the candidate list is always
one or two entries and can never be empty. It remains true, and remains a real structural guarantee,
that the selection function itself receives only the two start markers and no drift information at
all, so a caller cannot invert the comparator; the exclusion lives one level up because that is where
the drifting key is known.

Selection among two candidates is `argmax` over `(start_unknown, worktree_created_at, item_key)`, where
`worktree_created_at` is the adopted start-of-execution marker and `item_key` is the integer
`issue_num`. The three tie-breaks:

- Equal timestamps — the normal case for a same-minute cohort fan-out — deem the larger `issue_num`
  later-started, so the smaller key survives, consistent with the ascending-item-key determinism
  convention used in `## Cohort Barrier and Max-Concurrency Slot Filling`.
- A start timestamp present on exactly one item makes that timestamped item earlier-started, so the
  item of unknown start is halted.
- Both timestamps absent falls through to the item-key tie-break.

Identical inputs produce identical halt decisions; every timestamp is a function input and no clock
is read inside the pure functions.

#### Quiesce Is Derived State

Quiesce is derived, never stored. Admission into the current cohort is suspended while
`has_unresolved_drift(events, items)` returns `True`. **No quiesce field is written anywhere** — not
to the checkpoint, not to the manifest, not to the status document. Deriving quiesce from the event
log and the item records makes it self-clearing on resolution and keeps the F3-owned checkpoint
contract untouched, consistent with `## Parallel-Level Checkpoint`. That exported predicate is the
single seam F6's admission control consults.

#### Requeue Through the Single Recolor Seam

The requeue passes through exactly one seam, `request_requeue_via_recolor`, which **requests** the
mutation and never performs one. No second recolor implementation exists in this feature: the seam
contains no coloring, no cohort assignment, and no graph logic. F6 owns the recolor engine that pins
in-flight items, recolors the unstarted subgraph, and writes the checkpoint.

The requested intent is:

- the joint item write `merge_status: blocked_drift` **and** item `state: blocked`. Both are
  required together: the schema requires a `blocked_drift` merge status to accompany item state
  `blocked`, so writing the merge status alone produces a checkpoint that fails validation;
- exactly one `mutations[]` entry
  `{op: "requeue", item_key, at, prior_state: "in_flight", new_state: "blocked", disposition: null,
  recolor_generation: <prior + 1>}`;
- `recolor_generation` incremented by exactly one.

`new_state` is the item-state value `blocked`, **not** `blocked_drift`. `mutations[].new_state` is
validated against the item-state enum, and `blocked_drift` is a `merge_status`, so it would be
rejected in that slot. `disposition` is null because a non-null disposition is permitted only on a
`remove` entry whose `prior_state` is `in_flight`.

#### Drift-Event Recording (A8)

Exactly one `drift_events[]` entry is appended per drift occurrence, carrying the strongest action
taken. `halted_later_started_item` subsumes `raised_blocking_finding`, so an occurrence that halted a
later-started item records that one event and does **not** additionally record a
`raised_blocking_finding` event for the same occurrence. The entry shape is the six F3-owned fields
`{item_key, declared, observed, escaped_paths, at, action}`; `escaped_paths` is non-empty by
definition. `drift_events[]` is append-only, and each append is a regeneration boundary for the
status document per `## Documentation Maintenance Boundaries`.

#### Two-Layer Drift Gate

A child's transition to review, and the item's progression toward merge, are gated while an
unresolved drift event exists for that item. The gate is implemented at both enforcement layers,
because a `PreToolUse` hook fires per call with no cross-call state visibility and can be bypassed.

- **Layer 1 — per-call deterrent.** `.claude/hooks/enforce-parallel-drift-gate.ps1`, registered on
  the `PreToolUse` `Agent` matcher. It fires only when `subagent_type == "feature-review"` and the
  prompt carries the `Parallel mode: true` marker, resolves the target item by scanning the prompt
  for a `docs/features/active/<basename>` path token, reads
  `artifacts/orchestration/parallel-orchestrator-state.json`, and denies with
  `PARALLEL_DRIFT_GATE_BLOCKED` while the item's latest drift event is unresolved **and** its
  synthetic finding has not been written. Requiring both conditions is what keeps the R4 review of
  the remediation loop from being deadlocked: resolution itself requires a review, so once the
  finding file exists, review proceeds. Allowed outright: a non-`feature-review` target, a prompt
  without the marker, and a resolved or never-drifted item. Denied fail-closed: a missing or
  unreadable checkpoint, an unresolvable target item, and an unreadable drift-event log. The hook
  performs presence gating only — checkpoint-state reads plus one finding-file existence check. It
  runs no git command and no path-glob matching, so all path-matching semantics stay in the single
  Python implementation.
- **Layer 2 — retrospective backstop.** A key-gated invariant in
  `scripts/dev_tools/validate_parallel_orchestrator_state.py`, implemented in the helper
  `scripts/dev_tools/_parallel_orchestrator_state_drift.py`. It emits one
  `PARALLEL_DRIFT_GATE_VIOLATION:` error per item whose latest drift event is unresolved while its
  `merge_status` is in `{pr_open, ci_green, merged, worktree_removed}`. It is additive: a checkpoint
  with no `drift_events` key produces zero new errors. An item resting at `blocked_drift` produces no
  error, which is what makes the gate compatible with the state the halt path writes.

The Layer-1 finding-presence check is narrowed to the **current** drift event. A matched
`remediation-inputs.<yyyy-MM-ddTHH-mm>.md` file opens the gate only when the timestamp embedded in
its name is ordinally greater than or equal to the item's latest drift event's `at`; a finding
written by an earlier, unrelated remediation cycle is therefore ignored and does not open the gate
for drifted, unsurfaced work. The check remains **presence gating only**: the timestamp is taken
with a fixed-offset substring of the directory entry's name and compared ordinally, and the hook
performs no path-glob match, no git command, and no read of any file's content. A name whose
embedded substring is absent or not canonically formatted, and a latest-event `at` that is itself
not canonically formatted, both leave the gate closed, so an unreadable timestamp on either side
denies rather than allows. The canonical `yyyy-MM-ddTHH-mm` shape is required on both sides of every
timestamp comparison the gate makes, in both runtimes: an ungated ordinal comparison fails open,
because `-` sorts below `:` and a colon-bearing value therefore compares greater than a
hyphen-bearing value naming the same instant.

#### Resolution Semantics

This is the least obvious part of the feature and is stated here in full. `drift_events[].action` has
exactly two members, `raised_blocking_finding` and `halted_later_started_item`, and there is **no
`resolved` member**. `escaped_paths` must be non-empty, so a clean re-evaluation cannot be recorded
as a drift event at all. Resolution is therefore **derived** from fields that already exist, and this
feature adds no schema field and extends no enum.

The latest event for an item key is the one with the greatest `at`, ties broken by append order so
the later-appended record wins. A latest event for item K is **unresolved** unless at least one
disjunct holds against K's currently recorded `blast_radius`:

- **(a) Radius widened to cover the escape.** Every `escaped_paths` entry of K's latest event is
  subsumed by the item's current `blast_radius.paths` under F1's corrected path-subsumption
  predicate, which honours exact match, listed-directory prefix, and glob match symmetrically with
  the contention relation.
- **(b) Radius re-recorded from a later observed diff.** The item's `blast_radius.source` is
  `observed` **and** its `blast_radius.computed_at` is strictly later than the event's `at`. This
  covers remediation that narrowed the diff instead of widening the radius.

The derivation is **fail-closed**: absent an affirmative parent write, neither disjunct holds, drift
stays unresolved, and the gate keeps denying. A malformed event log is likewise reported as
unresolved.

Each disjunct has a named producer, so the gate has a release path and nothing deadlocks. The
producer is step 7 of `#### Seven-Step Procedure`: the `parallel-orchestrator` performs one of the two
writes when the consuming remediation cycle exits with `blocking_count == 0`. Non-deadlock is
therefore a consequence of that named producer rather than a property asserted without one. For
disjunct (b) the value written is not hand-constructed: the command line emits it as the
`observed_radius` key of its stdout payload, documented under `#### CLI Invocation`, and the parent
applies that library-built value verbatim with a `computed_at` strictly later than the event's `at`.
For disjunct (a) the parent extends `blast_radius.paths` to cover every escaped path. The R1 through
R5 loop that drives the remediation preceding either write is **reused
unmodified**: `atomic-planner` plans the resolution, `atomic-executor` performs preflight then
resolves, `feature-review` re-audits, and the loop exits on zero blocking findings. No new
remediation loop is authored, no line of the existing loop is modified, and the shared
`remediation_pass` cap of 3 applies. `.claude/skills/orchestrate/SKILL.md` is not modified by this
feature.

#### Layer-1 Narrowing — a Documented Limitation

The Layer-1 PowerShell hook implements only disjunct **(b)**. Disjunct (a) requires F1's glob
matcher, and duplicating that matcher in PowerShell would create exactly the divergent-matcher
failure this feature exists to prevent, so the hook omits it and evaluates the ordinal
`computed_at > at` comparison alone.

The hook is therefore strictly more conservative than the Python derivation: omitting disjunct (a)
can only report unresolved where Python reports resolved. The direction of the narrowing is
**deny-only**, never allow-only, and the finding-file allowance above keeps that conservatism from
deadlocking review. Python remains the single authority on resolution, and a cross-runtime seam test
in `tests/scripts/claude-hooks/enforce-parallel-drift-gate-helpers.Tests.ps1` runs both runtimes
over one shared checkpoint-state table and fails when they diverge. This is a real limitation,
recorded here rather than omitted.

Recovery action for a spurious Layer-1 deny — one the operator can always take: re-record the item's
`blast_radius` from the later observed diff, which satisfies both runtimes because it is disjunct
(b), the one disjunct the hook evaluates.
