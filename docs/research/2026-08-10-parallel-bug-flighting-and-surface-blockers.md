# Parallel Bug Flighting and Parallel-Surface Blockers

**Date:** 2026-08-10
**Status:** Blocked — analysis complete, execution cannot start
**Purpose:** Resumable plan for fixing the open bug backlog via `/parallel-plan` + `/parallel-run`, plus the infrastructure gaps that currently prevent that surface from producing a valid run.
**Resume trigger:** All four items in [Unblock Checklist](#unblock-checklist) are closed.

---

## 1. Executive Summary

A `/parallel-plan` run was attempted against the open bug backlog on 2026-08-10. Planning halted
before any preparation delegation was launched. No manifest, checkpoint, kickoff artifact, feature
folder, or branch was created, and no execution started.

The halt was correct: the scheduling half of the parallel surface is not implemented in this
repository, so preparation delegations would have produced feature folders and atomic plans that
could never be scheduled.

The sequencing analysis that the run was intended to produce is recorded here instead, as analysis
rather than as a machine-consumable manifest. When the blockers close, this document supplies the
lane structure, the ordering rationale, and the revalidation steps needed to resume.

---

## 2. Blocking Findings

Each finding below was verified directly in this worktree on 2026-08-10.

### 2.1 The cohort-computation library does not exist — in either repository

`compute_cohorts` (Welsh-Powell graph coloring) is specified in
`.claude/skills/parallel-plan/SKILL.md` as a landed, import-only library at
`scripts/dev_tools/parallel_cohort_computation.py`.

- It is absent from TaskMaster. `scripts/dev_tools/` does not exist at all.
- It is absent from the upstream governance repository. `git grep -in "compute_cohorts|welsh"`
  across all tracked files in `C:\Users\DanMoisan\repos\drm-copilot` returns nothing.
- The upstream branch `feature/parallel-cohort-scheduler-445` and its feature folder exist, but no
  code landed. The upstream parallel epic merged F7/F8 (hooks, drift detection, schemas, validators)
  without F2.

This blocks two mandatory completion requirements: seeding `cohorts[]`, and the P5 recomputation-parity
check. P5 is defined as re-invoking the landed library. Self-implementing the coloring would make the
parity check compare an implementation against itself, certifying nothing.

**Severity: hard blocker. This is new work, not a port.**

### 2.2 `config/blast-radius.json` is absent and the upstream copy is not portable

`config/` in this repository contains only `orchestration-routing.json`. The blast-radius truth table
is a required argument to both `Get-BlastRadius` and `Test-BlastRadiusConflict`.

The upstream copy at `C:\Users\DanMoisan\repos\drm-copilot\config\blast-radius.json` enumerates
modules `scripts/dev_tools`, `packages/mcp-server`, `extensions/drm-copilot` and shared surfaces
`poetry.lock`, `package-lock.json`. None of those exist in a C#/VSTO repository. Applied here it
would attribute zero modules and miss every real TaskMaster shared surface.

TaskMaster's actual shared surfaces, for whoever authors the replacement:

- `TaskMaster.sln`
- `Directory.Build.props` / `Directory.Build.targets`
- `.editorconfig` / `.globalconfig`
- `quality-tiers.yml`
- `coverage.config`
- `.github/workflows/**`

Copying the upstream file would under-report contention. That is the opposite of the fail-closed
direction established by the F1a corrections (upstream issue #452, PR #453), and the skill explicitly
forbids narrowing radii to suppress conflict edges.

**Severity: hard blocker. Authoring it is a design decision about which surfaces are shared in a
C#/VSTO repository, not a mechanical port.**

### 2.3 Supporting gaps

| Gap | Detail |
|---|---|
| `.claude/rules/parallel-orchestration.md` | Absent. Exists upstream only. It is the schema authority named by the planner startup protocol. |
| `route_id: parallel` | Absent from `config/orchestration-routing.json`. Routes are `small`, `large`, `remediation`, `preparation`, `epic`. Orchestrator invariant 2 would reject any execution checkpoint even if planning completed. |
| Invocation form | The skill's `poetry run python -c "from scripts.dev_tools..."` form does not apply. TaskMaster has no `scripts/dev_tools/`, no `pyproject.toml`, no `poetry.lock`. The port here is PowerShell at `.claude/lib/blast-radius/*.psm1`. |

### 2.4 What does work

Do not re-investigate these when resuming:

- MCP validators `parallel-planner-state` and `parallel-kickoff` dispatch correctly (verified by probe).
- Enforcement hooks `.claude/hooks/enforce-parallel-*.ps1` are wired in `.claude/settings.json`.
- The PowerShell blast-radius port is complete, including the contention relation
  `Test-BlastRadiusConflict`: `.claude/lib/blast-radius/{BlastRadius,BlastRadiusConfig,BlastRadiusExtraction,BlastRadiusGlob,BlastRadiusValidation}.psm1`.
- Both `.claude/agents/parallel-*.md` and all six `.claude/skills/parallel-*` skills are present.

---

## 3. Backlog Snapshot (2026-08-10)

- **89 open issues** total: **69 bug**, 17 feature, 3 refactor.
- None assigned.
- The five bugs with live worktrees at the time of analysis (503, 505, 507, 508, 438) were already
  closed and are not among the 69.

Counts verified with:

```
gh issue list --label bug --state open --limit 300 --json number --jq 'length'
gh issue list --state open --limit 300 --json number --jq 'length'
```

---

## 4. Lane Structure

Parallelism is bounded by file-level contention, not by issue count. The 69 bugs collapse into 13
lanes. Items **within** a lane touch the same files and must serialize. Lanes are mutually
independent except where noted in [§6](#6-cross-lane-couplings).

Realistic concurrency is therefore `max_concurrency` (1–8, default 4) across ~13 independent chains,
not 69 simultaneous items.

> **Lane assignments below are provisional.** They were derived from issue titles and subject-matter
> clustering, not from computed blast radii. When the surface is unblocked, the authoritative grouping
> comes from `Get-BlastRadius` + `Test-BlastRadiusConflict`. Treat this table as the expected shape
> to sanity-check the computed result against, not as a substitute for it.

### Lane A — Build / CI / coverage tooling (10)

| # | Title |
|---|---|
| 394 | utilitiescs-test-cs2002-duplicate-compile-entry |
| 441 | Cobertura post-processing double-counts `<line>` nodes, inflating lines-valid and every coverage rate |
| 457 | excludefromcodecoverage-does-not-suppress-nested-lambdas |
| 478 | merge-cobertura-classes-blends-union-with-primary-methods |
| 492 | nullable-gate-masked-by-incremental-build |
| 494 | conflicting-coverage-thresholds-across-policy-docs |
| 509 | csharpier-documented-command-incompatible-with-pinned-version |
| 512 | nullable-gate-cannot-fail-incremental-build |
| 513 | collect-pr-context-misclassifies-csharp-as-documentation |
| 522 | claudemd-nullable-gate-diverges-from-ci |

### Lane B — Test determinism / flakiness (7)

| # | Title |
|---|---|
| 285 | timeouttask-runwithtimeout-exception-type-mismatch |
| 446 | iteratequeueasync-deadline-closes-queue-early |
| 491 | quickfiler-test-form1-live-form |
| 493 | uithread-dispatcher-static-swap-no-restore |
| 511 | winformspumphost-tests-load-flaky-visible-window |
| 516 | timeoutafter-tests-race-real-wall-clock-deadline |
| 520 | console-setout-races-under-class-parallelism |

### Lane C — Ribbon (3)

| # | Title |
|---|---|
| 504 | ribbon-dead-callback-names |
| 524 | ribbon-controller-intelligence-unguarded-globals-deref |
| 525 | engine-toggle-prime-last-writer-race |

### Lane D — Breadcrumb router / hub / dropdown (9)

| # | Title |
|---|---|
| 439 | efcviewer-missing-lineage-and-segment-navigation |
| 440 | breadcrumb-left-right-arrow-parent-child-navigation |
| 462 | breadcrumb-dropdown-coordinator-stale-closepending-drops-reopen |
| 475 | breadcrumb-capturecurrentortests-silently-degrades-in-production |
| 498 | breadcrumb-router-segment-index-unvalidated-host-crash |
| 499 | breadcrumb-router-stale-selectedfolderpath-after-rebind |
| 500 | breadcrumb-webview-post-executes-under-upgrade-lifetime-lock |
| 501 | breadcrumb-hub-postjson-caches-before-broadcast-starves-attachments |
| 502 | breadcrumb-suggestions-upgrade-silently-stale-on-superseded-lease |

### Lane E — WebView2 host / initializer (4)

| # | Title |
|---|---|
| 458 | webview2breadcrumbhost-handler-retention-pooled-viewer |
| 463 | quickfiler-webview2-incognito-arg-en-dash |
| 476 | webview2breadcrumbhost-unmarshalled-sdk-call-and-unsynchronized-state |
| 477 | iwebviewcoreinitializer-contract-defects |

### Lane F — QuickFiler item controller (6)

| # | Title |
|---|---|
| 480 | qfc-item-controller-togglenavigation-double-toggle |
| 481 | qfc-item-controller-no-event-unwiring-path |
| 482 | qfc-item-controller-expansion-registry-divergence |
| 483 | qfc-item-controller-mailactions-error-handling-defects |
| 484 | qfc-item-controller-cleanup-timer-and-stale-field-defects |
| 485 | qfc-item-controller-webview-handler-unguarded-inputs |

### Lane G — QuickFiler collection controller (8)

| # | Title |
|---|---|
| 286 | qfc-collectioncontroller-removespecificcontrolgroup-counter-leak |
| 468 | qfc-collection-controller-unreachable-load-paths |
| 469 | qfc-collection-move-diagnostics-defects |
| 470 | qfc-collection-conversation-index-defects |
| 471 | qfc-collection-eliminate-space-sign-error |
| 472 | qfc-collection-navigation-digits-desync |
| 473 | qfc-collection-background-task-and-catch-defects |
| 474 | qfc-collection-controller-coupling-and-modal-getter |

### Lane H — QuickFiler home controller (2)

| # | Title |
|---|---|
| 442 | qfc-home-controller-metrics-never-flushed |
| 443 | qfc-home-controller-metrics-duration-misread |

### Lane I — EFC controllers (8)

| # | Title |
|---|---|
| 451 | efc-home-controller-metrics-inert-duration |
| 459 | efc-item-controller-keyboard-registration-defects |
| 460 | efc-item-controller-cleanup-nre-and-timer-leak |
| 461 | efc-item-controller-dead-conversation-expanded-handler |
| 464 | efc-controllers-null-guard-and-async-void-boundary-defects |
| 465 | efc-form-controller-lifecycle-and-selection-defects |
| 466 | efc-dead-code-and-latent-nre-traps |
| 467 | efc-viewer-processcmdkey-swallows-alt-mnemonics |

### Lane J — ItemViewer (5)

| # | Title |
|---|---|
| 486 | itemviewer-move-option-menu-defects |
| 487 | itemviewer-parentchanged-console-and-cast |
| 488 | itemviewer-breadcrumb-pipeline-lifecycle |
| 489 | itemviewer-ui-thread-marshalling-divergence |
| 490 | itemviewer-display-and-folder-contract-defects |

### Lane K — Keyboard actions (2)

| # | Title |
|---|---|
| 444 | kbdactions-enumerable-ctor-bypasses-duplicate-guard |
| 445 | quickfiler-keyboard-action-contract-defects |

### Lane L — QuickFiler miscellaneous (3)

| # | Title |
|---|---|
| 427 | quickfiler-post-show-duplicate-scoring |
| 448 | quickfiler-undoconsumer-nonterminating-loop |
| 449 | quickfiler-explorer-controller-latent-defects |

### Lane M — Singletons (2)

| # | Title |
|---|---|
| 287 | storewrapper-dialog-imprecise-for-genuine-failure |
| 426 | emailmovemonitor-rejected-item-hook-retention |

---

## 5. Flight Order

Three flights. The ordering is driven by evidence trustworthiness, not by lane size.

### Flight 0 — Lane A alone (10 issues)

**Lane A must run first and must run alone.**

Issues 512 and 492 report that the nullable gate cannot fail under incremental build. Issues 441,
478, and 457 report that coverage figures are inflated or wrongly merged. Issues 494 and 522 report
that the documented thresholds and gate commands disagree with what CI actually runs.

Until these land, every other bug's QA evidence is unverifiable. Running Flight 1 or 2 first would
certify 59 fixes against gates that cannot fail. Lane A is also the lane most likely to change
`Directory.Build.props`, `coverage.config`, `.github/workflows/**`, and `CLAUDE.md` — the exact
shared surfaces that would force serialization against everything else anyway.

Related prior finding, worth reading before starting Lane A: memory entry
`project_nullable_gate_diverges_from_ci.md` records that `CLAUDE.md`'s `/p:Nullable=enable` step does
not match `ci.yml` and can never pass (~200–414 errors red on `main`). Issues 492, 512, and 522 are
the tracked form of that finding; resolve them as a set rather than individually.

### Flight 1 — Lane B alone (7 issues)

Flaky tests make pass/fail non-deterministic. With Lane A's gates fixed but Lane B's flakiness
unresolved, a red result still cannot be distinguished from noise. Lane B also touches shared test
infrastructure (`UiThread` dispatcher, `WinFormsPumpHost`, console redirection under class
parallelism), which contends broadly across the C, D, E, F, G, H, I, J lanes' test projects.

### Flight 2 — Lanes C through M (52 issues)

The genuinely parallel body of the run. Eleven independent chains, subject to the couplings in §6.
Expected wall-clock is bounded by the longest chain — Lane G (8) or Lane I (8) — not by the total.

---

## 6. Cross-Lane Couplings

These will appear as conflict edges and will force the coupled lanes into distinct cohorts. They are
the reason Flight 2 cannot simply be launched as eleven fully independent streams.

| Coupling | Issues | Effect |
|---|---|---|
| J ↔ D | 488 (`itemviewer-breadcrumb-pipeline-lifecycle`) | ItemViewer and breadcrumb pipeline share lifecycle code. |
| E ↔ D | 458, 476 (`webview2breadcrumbhost-*`) | The WebView2 breadcrumb host bridges the host lane and the router lane. |
| I ↔ D (provisional) | 439 (`efcviewer-missing-lineage-and-segment-navigation`) | Titled as an EFC viewer defect but scoped to breadcrumb segment navigation. Lane placement must be confirmed by computed blast radius. |

Lanes D, E, and J must therefore land in three different cohorts, or the coupled items must be
serialized within a merged D∪E∪J chain.

---

## 7. Unblock Checklist

Work these in order. Items 1 and 2 are the hard blockers.

- [ ] **1. Implement `compute_cohorts`.** The algorithm is fully specified in
      `.claude/skills/parallel-plan/SKILL.md`: visit vertices by `(-degree, item_key)` ascending;
      each vertex takes the lowest cohort index not held by a neighbour. This is genuinely missing
      upstream as well, so it is new work. **It should land in `drm-copilot` first** — if it is
      written only in TaskMaster, the P5 recomputation-parity check compares the implementation
      against itself and certifies nothing. Note the TaskMaster port is PowerShell
      (`.claude/lib/`), not Python (`scripts/dev_tools/`), so either the library or the skill's
      invocation contract needs reconciling.
- [ ] **2. Author `config/blast-radius.json` for TaskMaster's layout.** Enumerate real modules
      (the `.csproj` set) and real shared surfaces (§2.2). Fail closed: when in doubt, widen the
      radius. Narrowing radii to suppress conflict edges is explicitly forbidden by the skill.
- [ ] **3. Copy `.claude/rules/parallel-orchestration.md` from `drm-copilot`,** confirming that
      nothing in it is drm-copilot-specific before committing.
- [ ] **4. Add `route_id: parallel` to `config/orchestration-routing.json`,** including its
      `language_budgets` and `complexity_to_model` entries, matching the shape of the existing
      `epic` route.

---

## 8. Resume Procedure

Once §7 is complete:

1. **Revalidate the backlog.** Issue state will have drifted. Re-run the §3 counts and diff the
   result against the §4 lane tables. Closed issues drop out; new bugs need lane assignment.
2. **Confirm Flight 0 is still needed.** If Lane A has been fixed in the interim (for example via
   the fallback path in §9), Flight 0 is already satisfied and the run starts at Flight 1.
3. **Verify the surface end to end** before committing 69 items to it. Run `/parallel-plan` against a
   deliberately small set first — two or three items from a single lane — and confirm it reaches a
   valid ready checkpoint, that `cohorts[]` is seeded, and that the P5 parity check passes.
4. **Recompute lanes from blast radii, not from this document.** Use §4 as a sanity check on the
   computed grouping. Investigate any large divergence before proceeding — it likely indicates an
   incomplete `blast-radius.json`.
5. **Run the flights in §5 order.** Do not collapse Flight 0 or Flight 1 into Flight 2.

---

## 9. Fallback Path (available today)

Lane A can be run now as a conventional epic, without any of the parallel surface:

```
/epic-plan   # scope Lane A: issues 394, 441, 457, 478, 492, 494, 509, 512, 513, 522
/epic-orchestrate
```

Ten issues with genuine shared-file contention fit the epic surface's explicit dependency graph
better than blast-radius cohorts do, and Lane A is the prerequisite for trusting any of the other
59 fixes regardless of which surface eventually runs them. Doing this first also means that when the
parallel surface is unblocked, the run starts at Flight 1 with trustworthy gates already in place.

Lane B is a reasonable second epic on the same rationale.

---

## 10. Provenance

- Analysis performed 2026-08-10 by the `parallel-planner` agent via `/parallel-plan`; blocking
  findings independently re-verified in the session worktree.
- Issue numbers and titles retrieved via `gh issue list --label bug --state open --limit 300`.
- Agent-scoped findings recorded at `.claude/agent-memory/parallel-planner/`:
  - `project_parallel_surface_partial_port.md`
  - `reference_drm_copilot_upstream.md`
- Upstream governance repository: `C:\Users\DanMoisan\repos\drm-copilot`.
- No manifest, checkpoint, kickoff artifact, feature folder, or branch was created by the halted run.
