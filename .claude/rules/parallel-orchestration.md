# Parallel Orchestration Artifact Invariants

This rule governs the three artifacts of the `parallel` orchestration surface: the parallel-run manifest at `docs/features/parallel/<slug>/parallel.md`, the parallel-orchestrator checkpoint at `artifacts/orchestration/parallel-orchestrator-state.json`, and the parallel-planner checkpoint at `artifacts/orchestration/parallel-planner-state.json`. It records the invariants those artifacts must satisfy as numbered prose so that downstream features consume a fixed schema and add behavior only.

The `parallel` surface schedules thematically unrelated items concurrently by computed blast-radius contention rather than by a human-authored dependency graph. There is no `depends_on` field anywhere, `issue_num` is the primary key for every item reference, and there is no integration branch: each item opens its own pull request against `main`.

## Foreign Schema Warning (do not copy verbatim)

The prohibition recorded in `.claude/rules/orchestrator-state.md` is restated here for the parallel artifacts. A hardened snapshot from another repository contains a JSON Schema for the orchestrator-state artifact whose `$id` references a foreign origin (`drmoisan.github.io/mix-calculator/`). That schema MUST NOT be copied verbatim into this repository, and it MUST NOT be adapted into a parallel-artifact schema: its `$id`, its top-level required-field set, and its cycle-level `additionalProperties: false` do not match this repository's checkpoint contract.

No JSON Schema file is authored, imported, or read for the parallel manifest or for either parallel checkpoint. The invariants above are expressed as prose in this file and enforced by validator logic. A schema whose `$id` is repo-local is not the disqualified foreign artifact, but the repository's enforcement mechanism remains prose-and-validator-logic regardless.

## Scope and Backward Compatibility

These invariants apply only to the three parallel artifacts named above. They do not apply to, and do not change, the epic artifacts (`artifacts/orchestration/epic-orchestrator-state.json`, `artifacts/orchestration/epic-planner-state.json`) or the standard orchestrator-state checkpoint governed by `.claude/rules/orchestrator-state.md`. The parallel validators are additive: the existing epic validators, their helper modules, their TypeScript cores, and their tests are unmodified.

Every validator named below returns a list of error strings, never mutates its input, returns a single-element list for text that is not parseable, and rejects a non-object (non-mapping) root. Error strings use the literal context-prefixed style with the prefixes `Parallel checkpoint`, `Parallel planner checkpoint`, and `Parallel manifest`.

## Invariants (parallel orchestrator checkpoint)

Enforced by `validate_parallel_orchestrator_state_text(text, *, require_complete=False)` in `scripts/dev_tools/validate_parallel_orchestrator_state.py`. Invariants 1 through 19 are enforced unconditionally; invariants 20 and 21 are enforced only under `require_complete`.

1. **Required keys.** The checkpoint must carry `objective`, `completed_steps`, `next_step`, `last_updated`, `route_id`, `parallel_slug`, `parallel_manifest_path`, `parallel_status_doc_path`, `mode`, `max_concurrency`, `current_cohort`, `recolor_generation`, `cohorts`, `items`, `conflict_edges`, `mutations`, and `drift_events`. One error is emitted per missing key.

2. **Route identity.** `route_id` must be exactly `'parallel'`.

3. **Mode enum.** `mode` must be `closed` or `open`.

4. **Bounded concurrency.** `max_concurrency` must be an integer from 1 through 8, and must not be a boolean.

5. **Item uniqueness and shape.** Each `items[]` entry must be an object whose `issue_num` is a positive integer unique across items and whose `feature_folder` is a non-empty string.

6. **Item state enum.** Each item's `state` must be one of the eight item-state values `proposed | admitted | prepared | scheduled | in_flight | merged | withdrawn | blocked`.

7. **Merge-status enum.** Each item's `merge_status`, when present, must be one of the eight merge-status values `not_started | worktree_created | pr_open | ci_green | merged | worktree_removed | blocked_drift | blocked_ci_loop_limit`. An absent `merge_status` is treated as `not_started` and contributes zero errors.

8. **State/merge-status consistency.** An item whose `merge_status` is `merged` or `worktree_removed` must have `state == 'merged'`. An item whose `merge_status` is `blocked_drift` or `blocked_ci_loop_limit` must have `state == 'blocked'`.

9. **Blast-radius shape.** Each item's `blast_radius` must be an object carrying `paths`, `modules`, `shared_surfaces`, and `contracts` as lists of non-empty strings, a `source` in `{derived, declared, observed}`, and a non-empty `computed_at` string. `modules`, `shared_surfaces`, and `contracts` may be empty lists.

10. **Prohibited dependency edges.** No object anywhere in the checkpoint may carry a `depends_on` key, and no top-level `depends_on` key may be present. Ordering is expressed only as blast-radius overlap. Presence is an explicit rejection, not mere absence.

11. **Prohibited integration-branch fields.** The checkpoint must not carry `integration_branch` or `epic_merge_pr` at any level. Each parallel item opens its own pull request against `main`, so there is no integration branch and no final integration pull request.

12. **Cohort shape and resolution.** Each `cohorts[]` entry must be an object with a non-negative integer `index`, a non-negative integer `generation` that is `<= recolor_generation`, and an `item_keys` list in which every entry resolves to an `items[].issue_num`.

13. **Current-generation cohort uniqueness.** Among the `cohorts[]` entries whose `generation` equals `recolor_generation`, `index` values must be unique and every non-withdrawn item must appear in exactly one such cohort's `item_keys`. An item that appears in no current-generation cohort is permitted only in state `withdrawn`, `merged`, or `blocked`. The strictness is "exactly one", following the pinning model in which recoloring is a pure function over the unstarted subgraph and therefore implies full coverage.

14. **Current-cohort bound.** `current_cohort` must be a non-negative integer. When any current-generation cohort exists, `current_cohort` must not exceed the maximum current-generation `index`.

15. **Conflict-edge shape.** Each `conflict_edges[]` entry must be an object whose `a` and `b` resolve to distinct `items[].issue_num` values with `a < b` (numeric normalization, so edge identity is canonical and recomputation is deterministic), and whose `reason` is in `{path_overlap, module_overlap, shared_surface_overlap, contract_dependency}`. A self-edge, an unresolved endpoint, an unnormalized pair, a duplicate `(a, b)` pair, or an out-of-enum reason is a malformed edge.

16. **Mutation shape.** Each `mutations[]` entry must satisfy the mutation table: `op` in `{add, remove, close, requeue}`; `item_key` resolving to an `items[].issue_num` for `add`, `remove`, and `requeue`, and null for `close` (a run-level operation); a non-empty `at`; `prior_state` and `new_state` either null or in the item-state enum, with `prior_state` null for `add` and `close` and `new_state` null for `close`; and `recolor_generation` a non-negative integer that is `<=` the top-level `recolor_generation`. Transition legality — which state may follow which — is downstream behavior, not schema; this validator checks shape, enum membership, and the null rules only.

17. **In-flight removal requires a disposition.** A `mutations[]` entry with `op == 'remove'` and `prior_state == 'in_flight'` must carry `disposition` exactly `'detach'` or `'abandon'`. A `disposition` on any other entry must be null.

18. **Drift-event shape.** Each `drift_events[]` entry must carry an `item_key` that resolves to an `items[].issue_num`, `declared` and `observed` as lists of non-empty strings, an `escaped_paths` list that is non-empty and holds non-empty strings (an event with zero escaped paths is not a drift event), a non-empty `at`, and an `action` in `{raised_blocking_finding, halted_later_started_item}`.

19. **Receipt arrays.** `delegation_receipts`, `skill_receipts`, and `mcp_call_receipts`, when present, must each be a list. Absent receipt arrays contribute zero errors. Per-receipt content validation follows the loose tolerance of the standard checkpoint validators.

20. **Completion gate, closed mode.** Under `require_complete` with `mode == 'closed'`, every item whose `state` is not `withdrawn` must have `merge_status` in `{merged, worktree_removed}`.

21. **Completion gate, open mode.** Under `require_complete` with `mode == 'open'`, the checkpoint must additionally record a `mutations[]` entry with `op == 'close'` (the run-close record). Invariant 20's per-item condition applies in open mode as well.

When `require_complete` is not passed, invariants 20 and 21 contribute zero errors and the validation result is byte-identical to a plain call.

## Invariants (parallel planner checkpoint)

Enforced by `validate_parallel_planner_state_text(text, *, require_ready_for_execution=False)` in `scripts/dev_tools/validate_parallel_planner_state.py`. P1 through P4 are enforced unconditionally; P6 through P9 are enforced only under `require_ready_for_execution`.

- **P1 — Required keys.** The checkpoint must carry `objective`, `parallel_slug`, `parallel_manifest_path`, `mode`, `max_concurrency`, `items`, `cohorts`, `conflict_edges`, `recolor_generation`, `completed_steps`, `next_step`, and `last_updated`. One error is emitted per missing key. `kickoff_prompt_path` is optional outside the ready gate.

- **P2 — Route-consistent identity.** `parallel_slug` and `parallel_manifest_path` must be non-empty strings; `mode` and `max_concurrency` satisfy orchestrator invariants 3 and 4.

- **P3 — Item shape.** Each `items[]` entry must carry `issue_num`, `feature_folder`, `kind`, `state`, `blast_radius`, `preparation_status`, `research_path`, `plan_path`, and `preflight_status`. `issue_num` must be a positive integer unique across items; `kind` must be in `{feature, bug}`; `state` must be in the item-state enum; `blast_radius` must satisfy orchestrator invariant 9; `complexity_band`, when present, must be in `{C1, C2, C3, C4}`. The prohibited-key rejections of orchestrator invariants 10 and 11 apply.

- **P4 — Cohort and edge shape.** `cohorts[]`, `conflict_edges[]`, and `recolor_generation` satisfy orchestrator invariants 12 through 15.

- **P5 — Deterministic recoloring seam (deliberately absent).** This feature does not recompute the cohort coloring. Recomputation parity against the cohort-computation module is the planner-surface feature's check (the analogue of the epic planner's wave-number cross-check). The omission is recorded here explicitly so a later reader does not mistake it for an oversight. There is no P5 check in the validator.

- **P6 — Ready gate, cardinality.** Under `require_ready_for_execution`, `items` must contain at least two entries.

- **P7 — Ready gate, preparation.** Under `require_ready_for_execution`, each item must have `preparation_status == 'prepared'`, `preflight_status == 'PREFLIGHT: ALL CLEAR'`, non-empty `research_path` and `plan_path`, and `blast_radius.source == 'declared'`. Only the planner-computed radius is authoritative for scheduling.

- **P8 — Ready gate, sentinel.** Under `require_ready_for_execution`, `next_step` must be exactly `'PARALLEL_EXECUTION_READY'`.

- **P9 — Ready gate, kickoff path.** Under `require_ready_for_execution`, `kickoff_prompt_path` must be exactly `artifacts/orchestration/parallel-kickoff-<parallel_slug>.md`.

The planner checkpoint carries no `epic_worthiness` analogue and no `NON_EPIC_RECOMMENDED` branch; the parallel surface has no worthiness verdict, and scale assessment happens before parallel planning is invoked. When `require_ready_for_execution` is not passed, P6 through P9 contribute zero errors.

## Invariants (parallel run manifest)

Enforced by `validate_parallel_manifest_text(text)` in `scripts/dev_tools/parallel_manifest_contract.py`, with the default-resolving accessors `manifest_mode(mapping)` and `manifest_max_concurrency(mapping)`. Manifest validation is a library call; it is deliberately not a third MCP `artifact_type`.

- **M1 — Frontmatter block.** The document must open with a `---` YAML frontmatter block terminated by `---`, parseable by `yaml.safe_load` into a mapping. Extraction is tolerant of LF, CRLF, and CR line endings. A missing, unterminated, unparseable, or non-mapping frontmatter block is malformed.

- **M2 — Slug.** `parallel` must be a non-empty string.

- **M3 — Mode default.** `mode`, when present, must be `closed` or `open`. When absent it defaults to `closed`: the accessor `manifest_mode(mapping)` returns the default and the validator emits no error for absence.

- **M4 — Concurrency default.** `max_concurrency`, when present, must be an integer from 1 through 8. When absent it defaults to `4`: the accessor `manifest_max_concurrency(mapping)` returns the default and the validator emits no error for absence.

- **M5 — Created-at.** `created_at` must be a non-empty string.

- **M6 — Items.** `items` must be a list. An empty list is valid at authoring time. Each entry must be an object carrying `issue_num` (positive integer, unique across items), `feature_folder` (non-empty string), `kind` in `{feature, bug}`, `state` in the item-state enum, and `blast_radius` in the shape of orchestrator invariant 9.

- **M7 — Prohibited keys.** No `depends_on` key may appear at any level, and no `integration_branch` key may appear at top level. Presence is an explicit rejection.

## Cache Doctrine — the checkpoint is not the source of truth

The parallel-orchestrator checkpoint is a CACHE of durable state, not the source of truth. Every field it records is re-derivable from the repository and from GitHub:

- `git worktree list --porcelain` — worktree existence and path (`items[].worktree_path`, `worktree_created_at`, `worktree_removed_at`).
- `git branch` — branch existence and name (`items[].branch_name`).
- `gh pr view --json state,mergedAt,headRefOid` — pull-request state, merge time, and merge commit (`items[].pr_number`, `pr_url`, `merge_status`, `merged_at`, `merge_commit_sha`).

No downstream feature may treat the checkpoint as authoritative. When the checkpoint disagrees with those three commands, the commands win and the checkpoint is rewritten from them. The validators in this rule check the checkpoint's structural shape only; they never assert that the cached values agree with the repository, because that reconciliation is a runtime concern of the orchestrator surface, not a schema concern.

## Omitted Epic Schema Fields (S8)

There is no integration branch for a parallel run: each item opens its own pull request against `main`. The parallel schema therefore carries no integration-branch and no final-integration-pull-request fields. The disposition of every relevant epic field is fixed as follows.

| Epic field | Disposition in the parallel schema |
| --- | --- |
| `integration_branch` (top level) | OMITTED — no integration branch; its presence is a prohibited-key violation (invariant 11, M7) |
| `epic_merge_pr` / `epic_merge_pr.merge_commit_sha` | OMITTED — no final integration pull request; the completion gate checks per-item terminal states instead (invariants 20-21) |
| `features[].depends_on` | OMITTED everywhere — ordering is derived from blast-radius overlap; presence is a prohibited-key violation (invariant 10, P3, M7) |
| `waves[]`, `features[].wave_number`, `current_wave` | REPLACED by `cohorts[]`, `current_cohort`, and `recolor_generation` |
| `max_parallel_features` | REPLACED by `max_concurrency` |
| `epic_feature_folder` | REPLACED by `parallel_slug`, plus `parallel_manifest_path` and `parallel_status_doc_path` |
| merge-status values `merge_conflict`, `blocked_conflict_loop_limit` | REPLACED by `blocked_drift` and `blocked_ci_loop_limit` — the fan-in merge-conflict path does not exist; drift and per-item CI loops are the parallel failure modes |
| planner `epic_worthiness`, `NON_EPIC_RECOMMENDED` branch | OMITTED — the parallel planner contract carries no worthiness verdict |

Per-item `merge_commit_sha` is retained; only the run-level merge-pull-request block is omitted.

## Concurrency Bound (A7)

`max_concurrency` is bounded at 1 through 8 inclusive and defaults to `4` when absent from the manifest. The design document sets only the default of 4; the upper bound of 8 is adopted here for symmetry with the epic surface, whose `max_parallel_features` is validated as `1..8`. The bound is recorded in this rule file so that downstream features do not re-litigate it. Booleans are rejected even though `True` and `False` are integers in Python.

The bound is enforced in three places with the same semantics: orchestrator invariant 4, planner invariant P2, and manifest invariant M4.

## Drift-Event Recording Rule (A8)

`drift_events[].action` is the two-member enum `{raised_blocking_finding, halted_later_started_item}`. The recording rule is: one event per drift occurrence, carrying the STRONGEST action taken. `halted_later_started_item` subsumes `raised_blocking_finding`, so an occurrence that halted a later-started item records exactly one event with `action == 'halted_later_started_item'` and does not additionally record a `raised_blocking_finding` event for the same occurrence.

The drift-detection feature consumes this enum and this rule without extending either.

## Enum Ownership (F6/F7/F8 consume, never extend)

All nine enums of the parallel surface are owned by the schema-and-validator feature (F3) and are fixed by this rule file:

| Enum | Members |
| --- | --- |
| `mode` | `closed`, `open` (default `closed`) |
| item `state` | `proposed`, `admitted`, `prepared`, `scheduled`, `in_flight`, `merged`, `withdrawn`, `blocked` |
| `merge_status` | `not_started`, `worktree_created`, `pr_open`, `ci_green`, `merged`, `worktree_removed`, `blocked_drift`, `blocked_ci_loop_limit` |
| `blast_radius.source` | `derived`, `declared`, `observed` |
| `items[].kind` | `feature`, `bug` |
| `conflict_edges[].reason` | `path_overlap`, `module_overlap`, `shared_surface_overlap`, `contract_dependency` |
| `mutations[].op` | `add`, `remove`, `close`, `requeue` |
| `mutations[].disposition` | `detach`, `abandon`, or null |
| `drift_events[].action` | `raised_blocking_finding`, `halted_later_started_item` |

The wave-4 features — F6 (mutation protocol), F7 (enforcement hooks), and F8 (drift detection) — CONSUME these member sets and NEVER extend them. A wave-4 feature that needs a new member must amend this rule file and the validators at spec review, not add the member at implementation time. This constraint exists because the wave-4 features are prepared concurrently and would otherwise add fields to the same files at the same time.

## F7 Seam

The retrospective cohort-ordering invariant `PARALLEL_COHORT_BARRIER_VIOLATION` (design section 9, Layer 2) is F7's explicitly assigned addition to the orchestrator validator. It is NOT implemented here. The entry point of `scripts/dev_tools/validate_parallel_orchestrator_state.py` contains a clearly delimited, appendable helper-invocation block, marked with explicit begin and end comments that name F7 and the invariant token, so that F7's edit is one appended helper call with no reflow of existing code. The TypeScript core `extensions/drm-copilot/src/lib/validate/parallel-orchestrator-state-core.ts` carries the matching comment-delimited seam. Existing helper calls sit outside the block.

## F3 Scope Boundary — kickoff contract deferred to F4

F3 deliberately excludes the kickoff-prompt contract module `scripts/dev_tools/parallel_kickoff_contract.py` and the `parallel-kickoff` `artifact_type`. Both are F4's scope, and F3 neither creates the module nor registers the artifact type on the CLI or MCP surfaces. The MCP surface grows by exactly two `artifact_type` values: `parallel-orchestrator-state` and `parallel-planner-state`.

F3's `require_ready_for_execution` gate is STRUCTURAL ONLY. It enforces the kickoff-PATH invariant (P9: `kickoff_prompt_path` must equal `artifacts/orchestration/parallel-kickoff-<parallel_slug>.md`) and does not parse or cross-check kickoff CONTENT. The deeper readiness-integrity machinery of the epic surface — git-integrity checks, launch-evidence binding, and kickoff-contract cross-checks — is left to F4, which may layer repository-aware checks behind an additional keyword without changing the schema. F3 likewise does not recompute the cohort coloring (planner invariant P5).

## Enforcement

- `scripts/dev_tools/validate_parallel_orchestrator_state.py`, with the helper modules `scripts/dev_tools/_parallel_state_common.py`, `scripts/dev_tools/_parallel_state_structures.py`, and `scripts/dev_tools/_parallel_state_records.py`, appends one error per violated orchestrator invariant. The completion-gate invariants 20 and 21 run only when the caller passes `require_complete=True`.
- `scripts/dev_tools/validate_parallel_planner_state.py` appends one error per violated planner invariant. The ready-gate invariants P6 through P9 run only when the caller passes `require_ready_for_execution=True`.
- `scripts/dev_tools/parallel_manifest_contract.py` appends one error per violated manifest invariant and exposes the default-resolving accessors. Manifest validation is a library call, not an MCP artifact type.
- `scripts/dev_tools/validate_orchestration_artifacts.py` registers the CLI subparsers `parallel-orchestrator-state` (with `--require-complete`) and `parallel-planner-state` (with `--require-ready-for-execution`). An unknown artifact type continues to fail with `Unsupported artifact type: {type}`.
- The TypeScript parity port at `extensions/drm-copilot/src/lib/validate/parallel-state-shared.ts`, `parallel-state-structures.ts`, `parallel-state-records.ts`, `parallel-orchestrator-state-core.ts`, and `parallel-planner-state-core.ts` reproduces the same invariants and is dispatched from `extensions/drm-copilot/src/lib/validate/orchestration-artifacts.ts` for both new `artifact_type` values. Verified scope: 96 of 96 error strings matched across 43 constructed documents, for JSON-representable values that round-trip through both runtimes' native types. Three divergence classes are known outside that verified scope: (1) **`pythonRepr` quote selection** — `parallel-state-shared.ts:112-132` always single-quotes, while Python's `repr` switches to double quotes when the value contains a single quote (recorded repo-wide at `docs/features/potential/2026-08-07-python-repr-quote-selection-divergence.md`); (2) **integral floats** — `JSON.parse` erases Python's `int`/`float` distinction, so an integral float value produces a different Python-side error count than the TypeScript side; (3) **boolean/integer equality** — `parallel-state-structures.ts:228` uses `===`, so a boolean value is not selected the way Python's `True == 1` equality selects it, producing differing error counts.
- Enforcement is therefore Python validator logic, plus the TypeScript parity port, plus this prose file. It is NEVER an imported JSON Schema. No schema file is read at validation time.
- The `parallel` route entry lives in `config/orchestration-routing.json` with `requires_pr_gate: false` (there is no run-level pull request to gate; each child's own route checkpoint enforces its per-item pull-request gate) and is mirrored byte-for-byte in `extensions/drm-copilot/resources/config/orchestration-routing.json`.
