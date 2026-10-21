# Preflight Clearance — Issue #879

Timestamp: 2026-09-14T01-55
Issue: #879
Work Mode: full-bug
Route: preparation
Directive issued to the validating agent: `DIRECTIVE: PREFLIGHT VALIDATION ONLY`

PREFLIGHT: ALL CLEAR

CONVERGENCE: NO FURTHER ROUNDS EXPECTED

## What cleared

Plan path: `docs/features/active/2026-09-13-deedle-netstandard-21-bind-unsatisfiable-in-production-879/plan.2026-09-13T18-22.md`

Plan blob SHA at clearance: `e04cbb90b818bd40532cb9fc4caf83b4c8511308`
Branch head at clearance: `e90e009db90cc1ecdb4ce9a36db26c9bd9328814`
Branch: `bug/deedle-netstandard-21-bind-unsatisfiable-in-production-879`

The plan was revised in place at that single canonical path across every round. No timestamped
sibling plan file was created at any point, per the Plan-Path Continuity Contract.

## Round count

Three preflight rounds. Each round was a separate `atomic-executor` invocation under
`DIRECTIVE: PREFLIGHT VALIDATION ONLY`; each revision between rounds was made by `atomic-planner`.
The orchestrator applied no plan edit itself.

| Round | Plan blob reviewed | Signal | Defects | Convergence line returned |
|---|---|---|---|---|
| 1 | `e99caf6b8221fec2a0ee7ffb3549447922067d72` (commit `8764ec822`) | PREFLIGHT: REVISIONS REQUIRED | 15 | FURTHER ROUNDS LIKELY |
| 2 | commit `4776cfc7` | PREFLIGHT: REVISIONS REQUIRED | 9 | NO FURTHER ROUNDS EXPECTED |
| 3 | `e04cbb90b818bd40532cb9fc4caf83b4c8511308` (commit `e90e009d`) | PREFLIGHT: ALL CLEAR | 0 blocking | NO FURTHER ROUNDS EXPECTED |

The two-round target in `.claude/skills/atomic-plan-contract/SKILL.md` was exceeded by one round.
The reason is recorded rather than excused: **two of the nine round-2 defects were introduced by the
round-1 delta itself**, and both were blocking. Round 1's G8b fix at `[P0-T15]` added a porcelain
companion whose pathspec included the feature folder and an acceptance demanding the output name only
two files, which the fourteen tasks preceding it make impossible; and round 1's fallback-host
amendment scoped a path substitution to two task IDs when nine name the prefix being substituted.
Round 3 exists because the round-2 revision carried five planner-initiated changes that no reviewer
had seen, the largest being a change to the Phase 2 fail-before seam.

## Validator gate

`mcp__drm-copilot__validate_orchestration_artifacts` with `artifact_type: plan` was run by the
orchestrator after every planner round, because neither `atomic-planner` nor `atomic-executor` has
that tool in its session surface.

- After round 1 authoring: `ok`, with one Warning — rule G8b at `[P0-T15]`, a name-listing diff with
  no staging or porcelain companion. That warning was passed to preflight round 1, confirmed as its
  defect D4, and closed.
- After the round-1 revision: `ok`, no warnings.
- After the round-2 revision: `ok`, no warnings.

## Review coverage of the clearing round

- Phases reviewed: 7 of 7
- Tasks reviewed: 86 of 86
- Acceptance clauses reviewed: 87 across 86 tasks
- Spec acceptance criteria traced: 19 of 19, each line-exact against `spec.md`
- Repository citations re-derived against the tree in the clearing pass: 41

Plan structure at clearance: 86 tasks across 7 phases, distributed 16 / 4 / 12 / 5 / 12 / 10 / 27,
matching the plan's own `## Task Counts` table.

## The load-bearing property, as measured by the clearing round

The acceptance test must reach the Deedle bind with no prior SVG rendering and no prior
`AssemblyResolve` handler. All eight harness methods now carry a pinned domain, and every method whose
result can depend on the installer carries a pinned order relative to `InstallProductionFallback()`.
Vacuity is closed in both directions by paired positive controls: `CountLoadedAssembliesNamed`
asserted greater than zero in the positive domain and zero in the installer-free one, and
`CountAssemblyResolveHandlers` asserted zero before the install and greater than zero after it. The
negative control `NegativeControl_WithoutInstall_Netstandard21Throws` retains its own halting task.

The clearing round additionally verified a property no earlier round stated: the positive domain is
created and unloaded per test method via `[TestCleanup]`, so the pre-install zero reading is
observable in every method. A class-scoped domain would have made that reading order-dependent.

## Two non-blocking observations recorded, not applied

The clearing round returned `ALL CLEAR` together with two observations, each carrying an optional
delta. The orchestrator did not re-open the plan for them. The reason is stated rather than implied:
the reviewer classified both as non-blocking, established that neither makes a task unsatisfiable and
neither admits a false green, and identified the later gate in the same plan that contains each; and
two of the three preceding delta rounds introduced new blocking defects, so a further revision round
carries a measured regression risk that these two observations do not justify. Both are reproduced
here so the executor and the coordinator can act on them with full information.

**O1 — the `Cobertura Document State:` discriminator at `[P0-T8]` may not discriminate.** The task
determines `POSTPROCESSED` versus `RAW-COLLECTOR-OUTPUT` by whether the Cobertura document contains
the literal `<sources>`. `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1` line 444 injects that
element only when the document does not already carry one, and the collector's own Cobertura writer
emits it, so the test may return `POSTPROCESSED` on both paths. Impact is confined to the fidelity of
one recorded field: the acceptance admits either value, and a mislabel routes `[P5-T10]` into its
`DENOMINATORS DIFFER` branch, which produces the same terminal judgment as the
`BASELINE DENOMINATOR NOT COMPARABLE` branch. The reviewer's proposed replacement discriminator is
whether any `class` element carries a `filename` attribute beginning with a drive-letter prefix, since
post-processing rewrites every `filename` to a repository-relative path.

**O2 — under the `HOST=ToDoModel.Test` fallback, three porcelain acceptance spans do not name
`ToDoModel.Test`.** `[P6-T1]`, `[P6-T26]` and `[P6-T27]` assert over a span scoped to `UtilitiesCS`,
`TaskMaster`, `UtilitiesCS.Test` and `TaskMaster.Test`. `[P0-T14]`'s path-prefix substitution rule does
not reach a bare `TaskMaster.Test` pathspec. This is unreachable unless the fallback fires, which
requires `Deedle.dll` to be absent from `TaskMaster.Test/bin/Debug`. It is contained: `[P6-T5]`'s
porcelain span is repository-wide and runs before `[P6-T26]` and `[P6-T27]`, so the uncommitted work
fails loudly there and the recovery is a micro-action inside the failing task. The reviewer's proposed
amendment adds `ToDoModel.Test` as an additional pathspec operand on those three tasks rather than
replacing `TaskMaster.Test`, because under the substitution both projects hold work.

## The unexplained netstandard 2.0.0.0 frame

The clearing round re-verified all five checks on the open risk. The plan states in plain words that
it cannot explain why the `2.0.0.0` leg also fails; no path through the task set reaches a green
terminal state with only the `2.1.0.0` leg demonstrated; the completion condition carrying the
sentence `Issue 879 must not be reported as closed on the strength of a 2.1.0.0 result alone.` is
enforced at `[P6-T2]` and propagated verbatim to the issue update at `[P6-T25]`; the Fusion binding
log task `[P6-T3]` is real, with a named artifact and an explicit `Blocking:` value of
`NO - the fix does not wait on this measurement`; and no acceptance condition gates on a `2.0.0.0`
value nobody has measured.

## Scope

Atomic execution, pull-request authoring and CI monitoring are out of scope for this preparation run
and were not performed. No plan task was executed. The only files this run changed on the item branch
are the plan, one directory reference inside one acceptance criterion of `spec.md`, and this artifact.
