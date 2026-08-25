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

4. **Bounded concurrency.** `max_concurrency` must be an integer from 1 through 32, and must not be a boolean.

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

- **M4 — Concurrency default.** `max_concurrency`, when present, must be an integer from 1 through 32. When absent it defaults to `4`: the accessor `manifest_max_concurrency(mapping)` returns the default and the validator emits no error for absence.

- **M5 — Created-at.** `created_at` must be a non-empty string.

- **M6 — Items.** `items` must be a list. An empty list is valid at authoring time. Each entry must be an object carrying `issue_num` (positive integer, unique across items), `feature_folder` (non-empty string), `kind` in `{feature, bug}`, `state` in the item-state enum, and `blast_radius` in the shape of orchestrator invariant 9.

- **M7 — Prohibited keys.** No `depends_on` key may appear at any level, and no `integration_branch` key may appear at top level. Presence is an explicit rejection.

- **M8 — Expected conflict components (optional assertion).** `expected_conflict_components`, when present, must be a list. Each entry must be an object carrying a required `members` list that is non-empty and holds positive integers, each of which resolves to an `items[].issue_num`, with no `issue_num` appearing in more than one component; and an optional `name` that, when present, must be a non-empty string. When the key is ABSENT the invariant contributes zero errors and the manifest's error list is byte-identical to what it was before M8 existed.

  The value must be authored as a YAML BLOCK sequence. The destination-runtime bash YAML subset parser (`.claude/lib/bash/parallel-yaml-scan.sh`) rejects a non-empty flow collection, so a flow-style value such as `members: [101, 102]` is outside the supported subset and is not accepted on the bash path.

  `expected_conflict_components` is an ASSERTION, not a declaration. It NEVER overrides a derived conflict edge, NEVER feeds `compute_cohorts`, and NEVER influences scheduling. It is consumed by a planner diagnostic (`scripts/dev_tools/parallel_lane_assertion.py`), invoked advisory-only, whose findings never block. Its name deliberately references the DERIVED conflict graph: the field asserts what the operator expects blast-radius derivation to produce, and a mismatch is a signal to re-examine the radii, never a licence to edit the graph. The prohibition on narrowing a radius beyond the configured exclusions to suppress an edge is unaffected, as is the `depends_on` prohibition of invariant 10, P3, and M7 — this key is not a dependency edge and does not express ordering.

  Example, in the mandatory block-sequence form:

  ```yaml
  expected_conflict_components:
    - name: hooks-lane          # optional, diagnostic label only
      members:                  # required, non-empty, positive ints
        - 101
        - 102
  ```

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

`max_concurrency` is bounded at 1 through 32 inclusive and defaults to `4` when absent from the manifest. The design document sets only the default of 4. Booleans are rejected even though `True` and `False` are integers in Python.

The upper bound is derived from a constraint analysis of this surface alone. No other surface's bound is a reason for it. The findings recorded here so that downstream features do not re-litigate them:

- **No constraint binds hard below O(100) concurrent worktrees.** Git worktrees, per-item feature branches, checkpoint size, and the cohort-coloring computation all scale well past a hundred concurrent items; none of them fails, or degrades sharply, anywhere near 32.
- **The first-binding constraint is GitHub Actions job concurrency**, which begins to bite at roughly 10 to 20 concurrent items on a typical plan. It binds by QUEUING, not by failing: excess jobs wait for a runner and the run completes more slowly. A `max_concurrency` above that point is therefore not an error, merely a setting whose marginal throughput is absorbed by the queue.
- **The ceiling of 32 is a SANITY limit, not a capacity limit.** Its purpose is to reject an order-of-magnitude operator typo (`320` for `32`), not to express a supported maximum. Do not read a value at or below 32 as an assurance that the runner pool can serve it.
- **Under the per-edge cohort barrier `max_concurrency` is a pure throughput throttle.** Mutual exclusion inside a conflict component is automatic: a conflicting neighbour in a strictly prior current-generation cohort must be `merged` or `worktree_removed` before an item starts, so raising the cap can never co-schedule two conflicting items. Raising it changes only how many independent lanes advance at once.

The bound is enforced in three places with the same semantics: orchestrator invariant 4, planner invariant P2, and manifest invariant M4.

The epic surface is unaffected. `max_parallel_features` remains bounded at `1..8`; it is a different field on a different surface and is not changed by this bound.

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

## Blast-Radius Contention Doctrine (issue #489)

The conflict graph that seeds cohorts is only as good as the evidence that produces its edges. Two
classes of derivation defect made thematically unrelated items contend, and the corrections below
are part of the landed contract. Enforcement remains prose plus validator logic; no JSON Schema is
authored, imported, or read for any of it.

### Read-by-mandate classification

Every agent in this repository is instructed to read the policy rules, the tier map, and the process
artifacts before doing any work. A plan that cites `.claude/rules/python.md` or `quality-tiers.yml`
is therefore reporting compliance with the reading order, not declaring that its diff will write
those files. Counting such a citation as contention made every well-formed plan collide with every
other well-formed plan.

`config/blast-radius.json` carries an optional `mandate_reads` list enumerating those paths as exact
entries and `**` subtree globs. That list is the mandate-read exclusion set. `derive_blast_radius` removes matching citations from the harvest
before resolving modules and shared surfaces, and `validate_blast_radius` removes them from its
plan-side extraction so V1 and V2 stay self-consistent against a radius derived from the same plan.
The key is optional and fail-closed: a truth table that omits it excludes nothing and reproduces
pre-change behaviour exactly.

Three constraints bound the mandate-read exclusion:

1. **The planner remains obliged to enumerate a genuine write explicitly.** An exclusion describes
   the default reading relationship, not a permanent ban. When an item's plan will actually write an
   excluded path, the planner appends that exact path to the declared radius after normalization.
2. **`quality-tiers.yml` stays a shared surface.** It is listed in both `shared_surfaces` and
   `mandate_reads`: the first governs what happens when an item really writes it, the second governs
   what happens when an item merely cites it.
3. **`detect_escaped_paths` makes the read/write distinction exact at execution time.** The
   derivation heuristic reads intent from plan text and can be wrong in either direction; drift
   detection compares the declared radius against the paths a diff actually touched, so an item that
   wrote an excluded path is caught against observed evidence rather than against prose.

The extractor additionally rejects four token shapes that were never write claims: a wildcard-free
token whose final component names a directory rather than a file, a `docs/features/` glob whose
wildcard occupies or truncates the feature-folder segment, a contract token carrying no ASCII
letter, and a token containing a placeholder or interpolation marker. `artifacts/` is not a known
top-level segment, so a bare `artifacts/**` subtree claim no longer satisfies the shape rules.

### Placeholder-shape rejection (issue #502)

The fourth shape is a token containing any member of the marker set

```text
<    >    ${    $(    %
```

The set is not a configuration key and is not read from `config/blast-radius.json`. It originates in
the checkable-literal placeholder guard defined in `.claude/rules/plan-acceptance-gates.md`, which
uses the identical five markers to decide that a plan operand documents a command *shape* rather
than stating a real assertion. Both subsystems answer the same question about the same text, so the
two vocabularies are pinned equal by test rather than left to convention.

**A marker-bearing token never matches a tracked path.** A placeholder or interpolation form
resolves at run time to text that is not in the token, so the token as written names nothing. For
the two angle brackets the claim is stronger than a heuristic: Windows forbids both characters in a
filename outright, so an angle-bracketed token cannot name a file on the platform this repository is
developed on. Admitting such a token recorded a `paths` entry that no diff could ever touch, and two
items citing the identical shape then acquired a `path_overlap` edge on a string that resolves to
nothing.

**The dominant token is a mandated artifact shape, which is why the exposure was corpus-wide rather
than incidental.** Every agent in this repository is instructed to write its evidence to the
canonical scheme defined in `.claude/skills/evidence-and-timestamp-conventions/SKILL.md`, and that
scheme is non-overridable, so a well-formed plan quotes the same feature-relative artifact shape
that every other well-formed plan quotes. The shape is therefore the most widely shared token in the
corpus, and admitting it made compliance with the evidence-path scheme a source of contention. The
same reasoning applies to the feature-document shapes and to the session-keyed state-file shape.

**The planner remains obliged to enumerate a genuine write explicitly.** This rejection describes a
plan that documents a shape, not a permanent ban on the paths a shape resembles. When an item's plan
will actually write a path it expressed as a shape, the planner appends that exact concrete path to
the declared radius after normalization. This is the same obligation constraint 1 of the
mandate-read exclusion imposes, and `detect_escaped_paths` is the same backstop: drift detection
compares the declared radius against the paths a diff actually touched, so an item that wrote a path
it expressed only as a shape is caught against observed evidence rather than against prose.

**Accepted fail-open trade.** The rejection runs inside the classifier, upstream of shared-surface
resolution, so a marker-bearing token whose shape matches a configured `shared_surface_globs`
pattern is dropped before it can be reported as a touched shared surface. The trade is inherent to
that placement: a guard downstream of surface resolution would leave the token in `paths` and
reintroduce the path-level false edge it exists to remove. Corpus exposure was measured empty — no
plan in the 58-plan corpus examined for issue #502 cited a marker-bearing token whose shape matched a
configured shared-surface glob — and the planner obligation above is what keeps a genuine write
visible. A later change that widens the marker set must re-take that measurement.

**Known residual: the whitespace split.** Extraction harvests whitespace-free inline-code tokens, so
a placeholder form written with internal whitespace is split into fragments before the marker test
ever runs. Each fragment is then judged on its own shape and is normally rejected for an unrelated
reason, most often for carrying no separator. The residual is therefore benign in the accepting
direction but is recorded here because it means the rejection is not a complete guard against every
way a shape can be written; it is a guard against the whitespace-free forms the extractor actually
admits.

Enforcement of this rejection is prose plus validator logic, exactly as for the three shapes above.
No JSON Schema is authored, imported, or read for it, and `config/blast-radius.json` gains no key.

### Module-map granularity criterion

Issue #472 removed the location-bucket modules `docs` and `tests` because a bucket keyed on where a
file lives rather than on which subsystem owns it attaches to nearly every work item. The same
reasoning extends to umbrella buckets keyed on a top-level directory that essentially every item
writes into: an umbrella that matches almost every radius is not a coherent unit of contention,
because a level that always fires carries no information and only suppresses concurrency.

Under that criterion `python-dev-tools`, `vscode-extension`, `claude-runtime`, `copilot-surface`,
and `agents-surface` were removed, leaving the seven subsystem modules `mcp-server`, `benchmarks`,
`poshqc`, `powershell-dev-tools`, `codex-runtime`, `config`, and `schemas`. Removing a module never
weakens the relation below the path level: two items editing the same file still contend on
`path_overlap`, and two items editing a declared shared surface still contend on
`shared_surface_overlap`.

A candidate module belongs in the map when it names a subsystem an item could plausibly not touch.
A candidate that matches the majority of work items belongs nowhere.

### The published truth table is not a copy of this one (issue #500)

The push-down publishes a second truth table into a destination workspace at
`extensions/drm-copilot/resources/claude-customizations/config/blast-radius.json`. That copy stood
stale after issue #489 corrected only the self-hosted one, and correcting it fixed contention in
both directions at once. Three points fix the relation between the two copies so a later maintainer
does not re-synchronise them by hand.

**A destination's module map is DERIVED, so the bundled `modules` key is not consumed.**
`assembleModules` in `extensions/drm-copilot/src/lib/push-down/claude-blast-radius-derive-core.ts`
computes a destination's module map from the destination's OWN layout — the manifest-bearing
directories its scan observes — unioned with `PAYLOAD_MODULES`. It never reads the source document's
`modules` key. The bundled `modules` key is retained rather than deleted only so that a maintainer
reading the file is not told something false, and because
`tests/scripts/dev_tools/test_blast_radius_config.py` calls `load_module_globs` on it and that
helper raises on an absent key. Nothing schedules on it.

**`PAYLOAD_MODULES` carries `config` only.** `claude-runtime` was removed from it by the same
granularity criterion that removed it from this repository's own map. The criterion transfers
without modification: every agent in the runtime is instructed to read the policy rules and process
skills before doing any work, so a `.claude/**` umbrella matches nearly every radius in a
destination exactly as it did here. The no-signal floor is preserved because `config/**` in a
destination holds only the two published files, which makes `config` a subsystem an item can
plausibly not touch and keeps the assembled map non-empty so the forbidden-glob guard has a
non-vacuous input.

**The bundled `shared_surfaces` and `shared_surface_globs` sets are the destination-portable
subset, not a copy of the self-hosted sets.** They were authored narrow when the bundled copy was
created and were never a copy that fell behind, so the correct gate is portable-set equality against
a declared constant plus a subset relation against the self-hosted list — never byte-equality with
the self-hosted file. Only `version`, `over_breadth_fraction`, and `mandate_reads` are byte-equal
across the two copies.

The reason the two key groups take different relations is an asymmetry between surfaces and modules.
An over-matching MODULE glob costs concurrency on every pair of items it touches, because a module
that fires for both radii forces contention whether or not the items are related. A SURFACE or
mandate-read entry naming a path the destination lacks is inert: it matches nothing, so it costs
nothing. Erring wide is therefore free on the surface side and expensive on the module side, which
is why the portable surface set carries ecosystem-standard root filenames a given destination may
not have. A separator-free shared surface carries additional weight: it is the sole gate on whether
the path-token extractor accepts a separator-free token at all, so a published table with no
separator-free surface entry cannot detect two items rewriting the same root build file, whatever
that file is named.

**A directional invariant closes the residual Class 2 gap (issue #500 remediation).** Portable-set
equality against the declared portable-surface constant and the `bundled <= self_hosted` subset
relation together do not observe the self-hosted copy gaining a portable separator-free surface
that never reaches the bundle: both checks are satisfied by a bundled set that stays fixed while
the self-hosted set grows around it. `test_every_separator_free_self_hosted_shared_surface_reaches_the_bundle`
in `tests/scripts/dev_tools/test_blast_radius_config_parity.py`, mirrored in
`tests/scripts/claude-lib/blast-radius/BlastRadius.KeyPartition.Tests.ps1`, closes that gap
structurally by asserting the reverse containment for separator-free entries: every separator-free
self-hosted `shared_surfaces` entry must also appear in the bundled separator-free set.

**The key-partition gate now asserts exhaustiveness (issue #500 remediation, R8).** The three
declared classes each assert a property of the keys they name, but none of them asserted that
the two committed copies' top-level key sets are identical, or that every top-level key
belongs to one of the three declared classes. `test_every_top_level_key_is_classified_and_shared_by_both_copies`
in `tests/scripts/dev_tools/test_blast_radius_config_parity.py`, mirrored in
`tests/scripts/claude-lib/blast-radius/BlastRadius.KeyPartition.Tests.ps1`, closes that gap: the
union of both copies' top-level keys is exhaustively covered by the three declared classes, and
an unclassified key or a key present in only one copy fails loudly and names itself.

## Enforcement

- `scripts/dev_tools/validate_parallel_orchestrator_state.py`, with the helper modules `scripts/dev_tools/_parallel_state_common.py`, `scripts/dev_tools/_parallel_state_structures.py`, and `scripts/dev_tools/_parallel_state_records.py`, appends one error per violated orchestrator invariant. The completion-gate invariants 20 and 21 run only when the caller passes `require_complete=True`.
- `scripts/dev_tools/validate_parallel_planner_state.py` appends one error per violated planner invariant. The ready-gate invariants P6 through P9 run only when the caller passes `require_ready_for_execution=True`.
- `scripts/dev_tools/parallel_manifest_contract.py` appends one error per violated manifest invariant and exposes the default-resolving accessors. Manifest validation is a library call, not an MCP artifact type.
- `scripts/dev_tools/validate_orchestration_artifacts.py` registers the CLI subparsers `parallel-orchestrator-state` (with `--require-complete`) and `parallel-planner-state` (with `--require-ready-for-execution`). An unknown artifact type continues to fail with `Unsupported artifact type: {type}`.
- The TypeScript parity port at `extensions/drm-copilot/src/lib/validate/parallel-state-shared.ts`, `parallel-state-structures.ts`, `parallel-state-records.ts`, `parallel-orchestrator-state-core.ts`, and `parallel-planner-state-core.ts` reproduces the same invariants and is dispatched from `extensions/drm-copilot/src/lib/validate/orchestration-artifacts.ts` for both new `artifact_type` values. Verified scope: 96 of 96 error strings matched across 43 constructed documents, for JSON-representable values that round-trip through both runtimes' native types. Three divergence classes are known outside that verified scope: (1) **`pythonRepr` quote selection** — `parallel-state-shared.ts:112-132` always single-quotes, while Python's `repr` switches to double quotes when the value contains a single quote (recorded repo-wide at `docs/features/potential/2026-08-07-python-repr-quote-selection-divergence.md`); (2) **integral floats** — `JSON.parse` erases Python's `int`/`float` distinction, so an integral float value produces a different Python-side error count than the TypeScript side; (3) **boolean/integer equality** — `parallel-state-structures.ts:228` uses `===`, so a boolean value is not selected the way Python's `True == 1` equality selects it, producing differing error counts.
- Enforcement is therefore Python validator logic, plus the TypeScript parity port, plus this prose file. It is NEVER an imported JSON Schema. No schema file is read at validation time.
- The `parallel` route entry lives in `config/orchestration-routing.json` with `requires_pr_gate: false` (there is no run-level pull request to gate; each child's own route checkpoint enforces its per-item pull-request gate) and is mirrored byte-for-byte in `extensions/drm-copilot/resources/config/orchestration-routing.json`.
- The `PreToolUse` merge gate `.claude/hooks/enforce-epic-merge-gate.ps1` carries a parallel allow-branch that authorizes a per-item `gh pr merge --merge` from the parallel-orchestrator checkpoint when `route_id == "parallel"`, the target item's `merge_status == "ci_green"`, and the command's PR number matches that item's `pr_number`; any other case fails closed with `EPIC_MERGE_GATE_BLOCKED`.
