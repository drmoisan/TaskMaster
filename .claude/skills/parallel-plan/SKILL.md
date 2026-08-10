---
name: parallel-plan
description: Prepare a set of thematically unrelated items for concurrent execution before any execution begins - item intake over issue numbers and potential-entry paths, concurrent preparation-mode child orchestrator delegations, blast-radius computation and V1-V3 validation, cohort seeding with a recomputation-parity check, run-manifest and planner-checkpoint authoring, and the parallel-orchestrator kickoff artifact.
argument-hint: "[items: issue numbers and/or potential-entry paths]"
context: fork
agent: parallel-planner
---

# Parallel Plan Skill

A user invocation (`/parallel-plan <slug> <item> [<item> ...]`) forks the `parallel-planner` agent
with this procedure in context. The run slug and item list for this run are:

$ARGUMENTS

This skill frames work for the `parallel-planner` agent, parallel to how
`.claude/skills/epic-plan/SKILL.md` frames work for `epic-planner`. The `parallel` surface
schedules thematically unrelated items by computed blast-radius contention: there is no worthiness
assessment, no operator-supplied ordering, and no shared branch into which item work merges.
Planning ends at preflight clearance and cohort seeding; no atomic execution, PR authoring, or CI
monitoring occurs under this skill.

## Prerequisites

Before proceeding, `parallel-planner` must:

1. Read `CLAUDE.md` for repository tone policy and architectural context.
2. Read applicable `.claude/rules/` files for the languages in scope, including
   `.claude/rules/parallel-orchestration.md`, which is the schema authority for the run manifest,
   both parallel checkpoints, and the nine parallel enums.
3. Read the policy files listed in the compliance reading order section of `CLAUDE.md`.

## Item Intake

Invocation shape: `/parallel-plan <slug> <item> [<item> ...]`, where each `<item>` is either a
GitHub issue number (already-promoted work) or a potential-entry path (unpromoted work). This is
the same intake domain as `/parallel-add`, so initial intake here and F6's add operation accept
identical forms.

- **Promotion.** Unpromoted items are promoted by their own preparation-mode child, not by the
  planner. The `preparation` route already carries the promotion MCP tools (`new_potential_entry`,
  `potential_to_issue`, `new_active_feature_folder`).
- **`issue_num` resolution.** `issue_num` is the primary key for every item reference. It is known
  at intake for issue-number items. For potential-entry items, record negative placeholders in
  intake order (`-1`, `-2`, ...) and back-fill the real number from each child's promotion receipt
  as preparation completes. Ordering is safe by construction: cohort seeding requires declared
  radii, radii require approved plans, and plans require promotion, so every placeholder resolves
  before seeding runs.
- **Fully resolved before kickoff.** The manifest is committed in fully resolved form — no negative
  `issue_num` remaining — before the kickoff artifact is written.
- **`feature_folder`.** Recorded at intake as a resolvable-hint basename, then resolved to a
  concrete `docs/features/active/<basename>` path after promotion with any lifecycle prefix
  stripped, per the `epic-orchestrate` convention. The concrete location is additionally fixed by
  the per-item `branch_name` recorded in the checkpoint.
- **`kind`.** Record `kind: feature | bug` at intake from the item source (issue labels, or the
  potential entry's declared kind). When the source is indeterminate, default to `feature` and note
  the choice `[recommendation, not upstream-constrained]`.

The operator is never asked for ordering edges, cohort assignments, or a worthiness verdict.
Intake proceeds directly to preparation fan-out.

## Preparation Fan-Out

One preparation-mode `Agent(orchestrator)` run per item. Preparation produces documents and plans
rather than code, and items carry no ordering constraint, so launch ALL item preparations
concurrently: one message, N `Agent` calls, each `isolation: "worktree"` and
`run_in_background: true`. Create each preparation worktree's branch from `origin/main`.

Each delegation prompt includes this literal kickoff line, followed by the model-budget marker
line:

> `Preparation mode: true. route_id: preparation. parallel_slug: <slug>. Perform promotion, research, feature documents (spec.md, user-story.md), atomic planning, and preflight clearance only. Atomic execution, PR authoring, and CI monitoring are out of scope for this run and are executed later by parallel-orchestrator. After the atomic-executor preflight returns PREFLIGHT: ALL CLEAR, commit the feature folder and plan to the current branch, push the current branch to origin, set out-of-scope step statuses to not-applicable, set next_step to S5_atomic_execution, and stop, reporting the plan-path and preflight status.`
>
> `model_budget.fable_policy: <disabled|available|preferred>.`

Properties of that line, each individually load-bearing:

- The markers `Preparation mode: true.` and `route_id: preparation.` are reused verbatim. Route
  selection is marker-driven, so no issuer identity is required.
- `parallel_slug: <slug>.` replaces the epic context fields.
- The push instruction is an addition to the planner's own prompt, not to the child contract text
  in `.claude/skills/orchestrate/SKILL.md`. It is required so each item's prepared work is durable
  before its worktree is removed.
- Downstream attribution reads "executed later by parallel-orchestrator". The planner authors its
  own prompt; the `orchestrate` skill text is untouched.
- The `model_budget.fable_policy` marker line is appended per the pattern in
  `.claude/skills/epic-orchestrate/SKILL.md` `## Model Selection`. The default is `disabled` when
  the marker is absent.

**Deliberate omissions.** The kickoff line above carries neither mode marker. It contains no
`Epic mode: true`, so `enforce-epic-wave-barrier.ps1` — which gates execution-phase delegations —
does not apply to preparation. It contains no `Parallel mode: true`, so F7's future cohort-barrier
hook, which matches that marker, will not gate preparation either. Both omissions are intentional
and must be preserved verbatim when the line is emitted.

**No edit to shared surfaces.** No edit is made to `.claude/skills/orchestrate/SKILL.md` or to
`config/orchestration-routing.json`, including the `preparation` route. The route mechanism is
marker-driven; the child contract's "commit ... to the current branch" wording is satisfied by a
branch created off `origin/main`; and `routes.preparation` declares no epic-specific required
agent, skill, or MCP tool.

**Collected per child at termination:** `plan-path`, preflight status, the promotion receipt (the
`issue_num` back-fill source), the model-routing receipt with `logical_agent: "orchestrator"`, the
topology receipt, `branch_name`, and `worktree_path`. There is no fan-in merge step: the planner
fetches and records each pushed item branch, back-fills `issue_num` into the manifest on
`parallel/<slug>-plan`, and updates the checkpoint.

## Artifact Home

**Per-item artifacts.** Each item's prepared feature folder and approved atomic plan live on that
item's own feature branch, created from `origin/main` at preparation time, committed by the
unchanged `route_id: preparation` terminal step, pushed to `origin` before its worktree is removed,
and reused unchanged as the item's execution branch. Record `branch_name` and `worktree_path` per
item in the checkpoint.

**Run-level artifacts.** `docs/features/parallel/<slug>/parallel.md` (the run manifest) and
`docs/features/parallel/<slug>/parallel-kickoff.md` (the durable kickoff copy) live on the
planner-owned branch `parallel/<slug>-plan`, created off `origin/main` and pushed. That branch is
explicitly not an integration branch: no item branch ever merges into it, it never merges into any
item branch, and it holds only `docs/features/parallel/<slug>/**`, a subtree no item's blast radius
may include. Each item opens its own pull request against `main`.

**Read access.** Readers — validators and the future `parallel-orchestrator` — reach both artifact
classes by ref: `git fetch origin <branch>` followed by `git show <ref>:<path>`, without checking
the ref out, per the technique documented in `.claude/skills/epic-run/SKILL.md`.

Three residual risks are recorded rather than eliminated:

1. **Stale execution base.** An item branch created at preparation time is based on an older `main`
   tip by the time its cohort executes. Mitigation is F5-owned (the execution-phase child merges
   `origin/main` at start, and cohort construction keeps peers non-conflicting); real overlap is
   caught by F8 drift detection. This skill records the branch-reuse contract only.
2. **Branch accumulation on withdrawal.** Withdrawn or abandoned items leave pushed preparation
   branches. F6's `--disposition abandon` path is expected to delete the branch; the cleanup
   expectation is recorded here so F6 can implement it.
3. **Per-branch git-integrity requirement — F4-owned.** The readiness gate's git-integrity check
   must verify committed plan blobs against each per-item branch ref plus the
   `parallel/<slug>-plan` ref, rather than against a single shared ref. This obligation belongs to
   the planner surface, not to the schema feature: `.claude/rules/parallel-orchestration.md` states
   that F3's `require_ready_for_execution` gate is structural only and leaves git-integrity checks,
   launch-evidence binding, and kickoff-contract cross-checks to F4.

## Radius Computation and Validation

The blast-radius feature landed as an **import-only Python library with no CLI entry point**,
matching the `scripts/dev_tools/epic_wave_computation.py` precedent. Reach it through the
`"Bash(poetry run *)"` allowlist entry as an importable-library call:

```bash
poetry run python -c "from scripts.dev_tools.compute_blast_radius import derive_blast_radius"
```

Landed contract, consumed as-is and never reimplemented here:

- **Derivation.**
  `derive_blast_radius(plan_text, spec_text, feature_folder, config, *, source, computed_at) -> BlastRadius`
  in `scripts/dev_tools/compute_blast_radius.py`. It takes the approved atomic plan's **document
  text** and the feature `spec.md`'s **document text**, the feature folder name, and the parsed
  `config/blast-radius.json` truth table. It does not take file paths: read the documents and pass
  their text.
- **Serialized shape.** `BlastRadius.to_dict()` yields exactly the key set
  `("paths", "modules", "shared_surfaces", "contracts", "source", "computed_at")`, which is the
  shape both the manifest and the checkpoint record.
- **Validation.** `validate_blast_radius(radius, plan_text, config, *, tracked_file_count)` in
  `scripts/dev_tools/_blast_radius_validation.py` returns findings over three rules:
  `RULE_COVERAGE = "V1"`, `RULE_SHARED_SURFACE = "V2"`, and `RULE_OVER_BREADTH = "V3"`. Each finding
  carries a severity of `Blocking` or `Advisory`.
- **Contention.** `conflicts(a, b, config) -> ConflictResult`, re-exported from
  `compute_blast_radius.py`. The signature takes three arguments; the third is the parsed
  `config/blast-radius.json`. Reasons come from the fixed vocabulary
  `{path_overlap, module_overlap, shared_surface_overlap, contract_dependency}`, and the relation
  fails closed.

**The F1a corrections (issue #452, merged PR #453) are load-bearing.** Derivation now reaches
separator-free repository-root shared surfaces from plan and spec text, admitting such a token only
as an exact ordinal member of the configured `shared_surfaces` list in `config/blast-radius.json`;
and the contention path comparison now honours listed-directory prefixes on both sides, aligning
with `is_path_subsumed`. Both corrections move results in the fail-closed direction — they report
more contention, not less. Do not work around either correction, and do not narrow a radius in
order to suppress a conflict edge they produce.

### Planner procedure

1. After an item's plan is approved and preflight-clear, read the approved plan text and the
   feature `spec.md` text, derive the radius with `source: "declared"`, and record it on the item.
   The `declared` radius is the authoritative input to scheduling.
2. Validate the radius and record the findings under the item's `radius_validation` entry.
3. **V1 (coverage) or V2 (shared-surface enumeration) Blocking failure.** The item does NOT
   transition to `prepared`. Record the findings in the checkpoint and issue a follow-up
   preparation-mode delegation for that item carrying the findings as plan-revision instructions —
   the same iterate-until-clear posture the child already applies to preflight. The item is
   re-planned, not rejected. Withdrawal is a caller decision made through F6's remove operation and
   is never a planner default.
4. **V3 (over-breadth) Advisory.** Record it in the checkpoint and surface it in the completion
   report. It has no state effect.
5. **Readiness conjunction.** `prepared` requires BOTH `preflight_status == "PREFLIGHT: ALL CLEAR"`
   AND a `declared` radius that passed V1 and V2.

Derivation, the V1-V3 rules, and the contention relation are implemented upstream. This skill calls
them; it defines none of them.

## Cohort Seeding

The cohort-scheduler feature likewise landed as an **import-only Python library with no CLI entry
point**, invoked through the same `"Bash(poetry run *)"` allowlist entry:

```bash
poetry run python -c "from scripts.dev_tools.parallel_cohort_computation import compute_cohorts"
```

Landed contract:

- `compute_cohorts(item_keys, conflict_edges) -> list[list[int]]` in
  `scripts/dev_tools/parallel_cohort_computation.py`. The signature accepts exactly two parameters,
  `item_keys: Iterable[int]` and `conflict_edges: Iterable[tuple[int, int]]`. There is no third
  parameter and nothing further to supply at seeding time.
- The return value is a plain list of lists in deterministic Welsh-Powell order: vertices are
  visited by the composite key `(-degree, item_key)` ascending — descending distinct-neighbour
  degree with ties broken by ascending item key — and each vertex takes the lowest cohort index not
  already held by one of its neighbours.
- Recoloring support belongs to F6 and F8, which is why the landed seeding signature needs no
  additional state input.

**Mapping to the recorded shape.** This planner maps the returned list of lists into the F3-owned
`cohorts[]` object shape, supplying `index` from the outer-list position and `generation: 0` itself.
The library returns the partition; the planner supplies the record fields.

### Seeding procedure

1. Invoke `compute_cohorts` exactly once per plan run, over the full conflict graph, after every
   item is `prepared` and radius-validated. Derive the conflict edge set by applying
   `conflicts(a, b, config)` to every unordered pair of `declared` radii.
2. Record `cohorts[]` at `generation: 0`, each cohort's `item_keys[]` sorted ascending.
3. Record `conflict_edges[]` as `{a, b, reason}` entries for auditability.
4. Record `recolor_generation: 0` and `current_cohort: 0`.
5. Record `max_concurrency` — default 4, bounded 1 through 8 by the F3 schema — without enforcing
   it. Enforcement is F5's, through
   `compute_concurrency_batches(cohort_item_keys, max_concurrency)`, which fills slots in ascending
   item-key order. Recoloring under add, remove, or drift mutation is F6 and F8 scope. This skill
   performs seeding only.

### Recomputation parity (planner-owned check)

Planner invariant P5 in `.claude/rules/parallel-orchestration.md` is deliberately absent from the
F3 validator and states that recomputation parity against the cohort-computation module is the
planner-surface feature's check — the analogue of the epic planner's wave-number cross-check. This
skill discharges that obligation as documented procedure, not as a new module:

1. After writing `cohorts[]` into the checkpoint and **before** emitting the kickoff artifact,
   re-invoke `compute_cohorts` over exactly the `item_keys` and `conflict_edges` recorded in the
   checkpoint.
2. Assert that the recomputed list of lists maps to exactly the recorded `cohorts[]` at
   `generation: 0`: the same partition, the same `index` assignment, and the same ascending
   `item_keys[]` ordering.
3. A mismatch is a **Blocking** condition. Stop the run and report the mismatch. Do not
   auto-correct the recorded cohorts, and do not emit the kickoff artifact.

No production module is added for this check; it is a re-invocation of the landed library.

## Manifest Authoring

Write `docs/features/parallel/<slug>/parallel.md` conforming to the F3-owned frontmatter schema
recorded in `.claude/rules/parallel-orchestration.md` (manifest invariants M1-M7):

- `parallel` — the run slug, a non-empty string.
- `mode` — `closed` or `open`; defaults to `closed` when absent.
- `max_concurrency` — an integer from 1 through 8; defaults to `4` when absent.
- `created_at` — a non-empty ISO-8601 string.
- `items[]` — one entry per item, each carrying `issue_num` (a positive integer, unique across
  items), `feature_folder` (a non-empty string), `kind` (`feature` or `bug`), `state`, and
  `blast_radius` carrying `paths`, `modules`, `shared_surfaces`, `contracts`, `source: "declared"`,
  and `computed_at`.

The manifest carries no `depends_on` field at any level and no top-level `integration_branch`
field; both are prohibited-key rejections in the schema. Commit it to `parallel/<slug>-plan` in
fully resolved form — every negative placeholder `issue_num` replaced by its promoted number —
before the kickoff artifact is written.

Manifest validation is a library call to `scripts/dev_tools/parallel_manifest_contract.py`
(`validate_parallel_manifest_text`, with the default-resolving accessors `manifest_mode` and
`manifest_max_concurrency`). It is deliberately not an MCP `artifact_type`; do not attempt to
validate the manifest through `mcp__drm-copilot__validate_orchestration_artifacts`.

## Checkpoint Persistence

Write `artifacts/orchestration/parallel-planner-state.json` after every completed step.

Top-level fields: `objective`, `parallel_slug`, `parallel_manifest_path`, `mode`,
`max_concurrency`, `plan_home_branch` (`parallel/<slug>-plan`), `items[]`, `cohorts[]`,
`conflict_edges[]`, `recolor_generation`, `kickoff_prompt_path`
(`artifacts/orchestration/parallel-kickoff-<slug>.md`), `completed_steps`, `next_step`, and
`last_updated`. F3's landed required-key set (planner invariant P1) is a strict subset of this
list, and `plan_home_branch` is a permitted additional field.

Per item: `issue_num`, `feature_folder`, `kind`, `state`, `complexity_band`, `preparation_status`,
`research_path`, `plan_path`, `preflight_status`, `branch_name`, `worktree_path`, `blast_radius`
(with `source: "declared"`), `radius_validation` (the `v1`, `v2`, and `v3` results with their
severities), `model_routing_receipt`, and `topology_receipt`.

**Deliberately absent:** any `epic_worthiness` analogue, any `depends_on` field, and any `wave`
field. The parallel surface renders no worthiness verdict and expresses ordering only as
blast-radius overlap.

**Readiness contract.** Before reporting completion the checkpoint must satisfy F3's
`require_ready_for_execution` gate: at least two items (invariant P6); every item
`preparation_status: prepared` with `preflight_status` exactly `PREFLIGHT: ALL CLEAR`, non-empty
`research_path` and `plan_path`, and `blast_radius.source == "declared"` (P7); `next_step` exactly
the ready sentinel `PARALLEL_EXECUTION_READY` (P8); and `kickoff_prompt_path` exactly
`artifacts/orchestration/parallel-kickoff-<slug>.md` (P9).

**Git integrity is F4-owned.** F3's `require_ready_for_execution` gate is structural only, so
verifying that each item's committed plan blob exists on that item's pushed branch ref — and that
the manifest and durable kickoff exist on the `parallel/<slug>-plan` ref — is this planner
surface's obligation, layered behind the planner's own check without changing the F3 schema.
Perform it with `git cat-file -e <ref>:<path>` and `git show <ref>:<path>` against each per-item
branch ref plus `parallel/<slug>-plan`.

**Cache doctrine.** The checkpoint is a cache of durable state, not the source of truth. On resume,
re-derive ground truth from `git branch`, `git worktree list --porcelain`, and the pushed refs.
Where the checkpoint and the repository disagree, the repository wins and the checkpoint is
rewritten from it.

Validate the checkpoint through `mcp__drm-copilot__validate_orchestration_artifacts` with
`artifact_type: "parallel-planner-state"` before reporting completion.

## F3 Ownership Boundary

The following surfaces are owned by the schema-and-validator feature (F3). This planner writes
conforming instances and consumes the fixed enums; it never extends or redefines them:

- The manifest schema and the planner-checkpoint schema, recorded as prose invariants in
  `.claude/rules/parallel-orchestration.md`, together with all nine parallel enums.
- `scripts/dev_tools/validate_parallel_planner_state.py` and
  `scripts/dev_tools/validate_parallel_orchestrator_state.py`.
- The two state MCP `artifact_type` values `parallel-planner-state` and
  `parallel-orchestrator-state`.
- The `route_id: parallel` entry in `config/orchestration-routing.json`.

The kickoff contract is **not** F3-owned. `scripts/dev_tools/parallel_kickoff_contract.py` and the
MCP `artifact_type: "parallel-kickoff"` are F4-owned and are **delivered by this feature**, per the
epic-manifest adjudication "Planner Adjudication: the kickoff-contract boundary (F3 / F4)" and the
matching "F3 Scope Boundary — kickoff contract deferred to F4" section of
`.claude/rules/parallel-orchestration.md`. Both are landed modules, not a recommendation carrying a
pending contingency.

The single-item-run floor question is resolved by F3's landed ready-gate invariant P6, which
requires at least two items under `require_ready_for_execution`. A one-item run cannot reach a ready
checkpoint.

## Kickoff Artifact

Write the working copy to `artifacts/orchestration/parallel-kickoff-<slug>.md` (a gitignored tree)
and commit a byte-identical durable copy to `docs/features/parallel/<slug>/parallel-kickoff.md` on
`parallel/<slug>-plan`.

```markdown
# Parallel Kickoff: <slug>

Planned by parallel-planner on <iso8601>. All items are prepared: promoted, active folders created,
research complete, spec and user-story written, atomic plans approved, preflight ALL CLEAR, blast
radii declared and V1/V2-clear. Planning state:
artifacts/orchestration/parallel-planner-state.json (run branch: parallel/<slug>-plan).

## Invocation Prompt

Run `/parallel-run <slug>` to execute this run, or paste the prompt below.

Use the parallel-orchestrator subagent to execute the prepared run whose manifest is
docs/features/parallel/<slug>/parallel.md on the plan-home branch parallel/<slug>-plan. Each item
resumes at atomic execution from its committed plan-path on its own pushed feature branch rather
than re-planning, and each item opens its own pull request against main.

## Item Summary

| issue_num | feature_folder | cohort | complexity | branch | plan-path |
| --- | --- | --- | --- | --- | --- |
| ... | ... | ... | ... | ... | ... |

## Integrity

planning_commit: <hex>

| plan-path | plan-hash |
| --- | --- |
| ... | ... |
```

Structural requirements enforced by the contract module:

- The first line is exactly `# Parallel Kickoff: <slug>`, with the slug matching
  `[a-z0-9][a-z0-9-]*`.
- `## Invocation Prompt` must name `` Run `/parallel-run <slug>` ``, the manifest path
  `docs/features/parallel/<slug>/parallel.md`, the plan-home branch `parallel/<slug>-plan`, and the
  resume-boundary sentence stating that each item resumes at atomic execution from its committed
  plan-path on its own pushed feature branch.
- `## Item Summary` is a strict pipe table with the exact ordered headers
  `issue_num | feature_folder | cohort | complexity | branch | plan-path` and at least one data row.
  `issue_num` and `cohort` are integers; `complexity` is one of `C1`, `C2`, `C3`, `C4`.
- `## Integrity` is optional. When present it carries a `planning_commit:` field holding the head
  commit of `parallel/<slug>-plan` as run-level provenance and a per-item `plan-path | plan-hash`
  table whose hashes are 40 to 64 hex characters with no repeated plan path.

Validate the artifact through `mcp__drm-copilot__validate_orchestration_artifacts` with
`artifact_type: "parallel-kickoff"`. That artifact type dispatches to
`scripts/dev_tools/parallel_kickoff_contract.py`, which this feature delivers.

## Completion Report

The final report to the operator must include:

- The manifest path `docs/features/parallel/<slug>/parallel.md` and the run branch
  `parallel/<slug>-plan`.
- Per item: one `plan-path:` line, the branch name, the preflight status, and the
  radius-validation result, including any V3 Advisory findings.
- The cohort table at `generation 0`, together with the result of the recomputation-parity check.
- Both kickoff artifact paths: `artifacts/orchestration/parallel-kickoff-<slug>.md` and
  `docs/features/parallel/<slug>/parallel-kickoff.md`.

End with the statement that execution has NOT started and begins only when the operator runs
`/parallel-run <slug>` or replays the kickoff prompt from the main session.
