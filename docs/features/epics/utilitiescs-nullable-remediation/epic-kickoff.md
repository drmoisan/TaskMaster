# Epic Kickoff: utilitiescs-nullable-remediation

Planned by epic-planner on 2026-07-19T00-40. All 13 child features are prepared: issues promoted,
active folders created, research complete, spec/user-story written, atomic plans approved,
preflight ALL CLEAR. Planning state: `artifacts/orchestration/epic-planner-state.json` (branch:
`epic/utilitiescs-nullable-remediation-integration`).

Objective: remediate the pre-existing nullable-reference-type debt (~2131 CS86xx diagnostics
across ~234 UtilitiesCS files plus SVGControl) that the CI nullable gate was silently failing to
catch, so the repaired gate (post PR #361) can be genuinely enforced without permanently blocking
future PRs. Null-annotation and null-safety remediation only; no behavior changes. Architecture:
per-file `#nullable enable` opt-in (both `UtilitiesCS.csproj` and `SVGControl.csproj` carry no
project-level `<Nullable>` element).

## Invocation Prompt

Run `/epic-run utilitiescs-nullable-remediation` to execute this epic, or paste the prompt below.

> Use the epic-orchestrator subagent to execute the prepared epic at
> `docs/features/epics/utilitiescs-nullable-remediation/epic.md`. The integration branch
> `epic/utilitiescs-nullable-remediation-integration` already contains every prepared feature
> folder and approved atomic plan; child features resume at atomic execution from their committed
> plan-path rather than re-planning. Execute per the epic-orchestrate skill: wave-scheduled child
> orchestrator runs in isolated worktrees, merge-on-green fan-in to the integration branch, and
> the final integration-to-main PR. `model_budget.fable_policy: disabled.`

The `## Invocation Prompt` block is the exact text the user replays (from the main session, never
from an `orchestrator` agent) to launch execution.

## Base-Branch Note (read before final integration-to-main PR)

The integration branch was based on `origin/fix/ci-nullable-gate-masking` (PR #361 head, commit
`20d163ac`), which is `origin/main` plus exactly the one nullable-gate-repair commit — NOT plain
`origin/main`. PR #361 (the `/t:Rebuild` gate repair this epic is premised on) is OPEN, not yet
merged. Children validate against the real repaired gate. The final integration-to-`main` PR is
clean once #361 lands; if #361 is still open at epic-completion, the maintainer must sequence the
merge so the gate repair is not lost. This is a deliberate, documented deviation from the
branch-off-`main` default (see `epic-planner-state.json.integration_base`).

## Feature Summary (13 children, waves 0/1/2)

| issue_num | feature_folder | wave | complexity | plan-path |
| --- | --- | --- | --- | --- |
| 363 | utilitiescs-nullable-extensions | 0 | C3/opus | docs/features/active/2026-07-18-utilitiescs-nullable-extensions-363/plan.2026-07-18T21-20.md |
| 364 | utilitiescs-nullable-helperclasses | 0 | C3/opus | docs/features/active/2026-07-18-utilitiescs-nullable-helperclasses-364/plan.2026-07-18T21-21.md |
| 366 | utilitiescs-nullable-reusabletypes | 0 | C3/opus | docs/features/active/2026-07-18-utilitiescs-nullable-reusabletypes-366/plan.2026-07-18T22-04.md |
| 367 | utilitiescs-nullable-newtonsofthelpers | 0 | C3/opus | docs/features/active/2026-07-18-utilitiescs-nullable-newtonsofthelpers-367/plan.2026-07-18T22-04.md |
| 369 | utilitiescs-nullable-threading | 0 | C3/opus | docs/features/active/2026-07-18-utilitiescs-nullable-threading-369/plan.2026-07-18T22-04.md |
| 368 | utilitiescs-nullable-svgcontrol | 0 | C2/sonnet | docs/features/active/2026-07-18-utilitiescs-nullable-svgcontrol-368/plan.2026-07-18T22-04.md |
| 365 | utilitiescs-nullable-outlook-folder-store | 1 | C2/sonnet | docs/features/active/2026-07-18-utilitiescs-nullable-outlook-folder-store-365/plan.2026-07-18T22-03.md |
| 371 | utilitiescs-nullable-outlook-mailitem-item | 1 | C2/sonnet | docs/features/active/2026-07-18-utilitiescs-nullable-outlook-mailitem-item-371/plan.2026-07-18T22-05.md |
| 370 | utilitiescs-nullable-email-parsing | 1 | C2/sonnet | docs/features/active/2026-07-18-utilitiescs-nullable-email-parsing-370/plan.2026-07-18T22-05.md |
| 372 | utilitiescs-nullable-email-classifier | 1 | C3/opus | docs/features/active/2026-07-18-utilitiescs-nullable-email-classifier-372/plan.2026-07-18T22-06.md |
| 374 | utilitiescs-nullable-dialogs-misc | 1 | C2/sonnet | docs/features/active/2026-07-18-utilitiescs-nullable-dialogs-misc-374/plan.2026-07-18T22-30.md |
| 375 | utilitiescs-nullable-residuals | 1 | C3/opus | docs/features/active/2026-07-18-utilitiescs-nullable-residuals-375/plan.2026-07-18T23-13.md |
| 376 | utilitiescs-nullable-ci-capstone | 2 | C2/sonnet | docs/features/active/2026-07-19-utilitiescs-nullable-ci-capstone-376/plan.2026-07-19T04-25.md |

Dependency edges (keyed by feature_folder in the manifest):
- Wave 0 (no dependencies): 363, 364, 366, 367, 368, 369.
- Wave 1: 365 & 371 & 374 depend on [extensions #363, helperclasses #364]; 370 & 372 depend on
  [extensions #363]; 375 depends on [extensions #363, helperclasses #364, threading #369].
- Wave 2: 376 (capstone) depends on all 12 remediation children.

## Execution-Time Gates and Flags (epic-orchestrator MUST carry these into atomic execution)

These are recorded during preparation and are triggered/enforced at atomic-execution time by
epic-orchestrator (or its child orchestrator runs), not during planning.

1. **reusabletypes #366 — maintainer-ratification STOP (Phase 6, `[P6-T2]`).** The plan contains
   a hard STOP for a public-contract change: adding `where TKey : notnull` on generic dictionary
   base types (resolves CS8714). Execution must halt at that task for maintainer ratification
   before proceeding. Actual in-scope set resolved to 51 files (vs the ~12 estimate).

2. **dialogs-misc #374 — Phase-0 execution-start gate.** Execution must NOT begin until extensions
   #363 **Batch D** (`Extensions/WinFormsExtensions.cs`, the `Clone<T>()` contract) has merged.
   The wave barrier already enforces that #363 is merged before #374 starts; this gate is the
   finer-grained in-child condition. Note: the #364 (helperclasses) depends_on edge for #374 is
   grep-unconfirmed by source (zero HelperClasses/ type refs under Dialogs/); it was retained as
   harmless (both wave-0 upstreams prepared), not dropped.

3. **outlook-folder-store #365 — child checkpoint enum deviation.** The child recorded
   step8/step9/step10 as `not_applicable`, which is outside the MCP step-status enum, so no
   `--require-complete` assertion was made at the preparation terminus. epic-orchestrator must
   treat the preparation terminus as `next_step: S5_atomic_execution` and must NOT rely on a
   `--require-complete`-clean child checkpoint for this feature on resume. Plan is 13 phases; 83
   files with 63 `#nullable enable` opt-in targets.

4. **residuals #375 — effective 37-file set + maintainer decisions.** The effective opt-in set is
   37 files, not 44: `PeopleScoDictionaryNewBackup.cs` is a dead, uncompiled duplicate flagged for
   maintainer exclude/delete, and 6 OlFolderTools Designer files are left null-oblivious (no
   pragma), consistent with the epic-wide Designer exclusion. The 44-file DoD inventory is
   unchanged. Additional items for the maintainer: `MSDemoConv.cs`, the `To Depricate/*` tree, and
   `MailResolution_ToRemove` (surfaced in the residuals spec.md and epic.md). Three pre-existing
   >500-line files are annotated in place, not split (splitting is out of scope for
   null-annotation-only remediation). An undeclared-but-harmless dependency on reusabletypes #366
   exists but was not added as a manifest edge (residuals is already wave 1, so it does not change
   wave layering).

5. **ci-capstone #376 — maintainer-decision items.** (a) **Optional project-level `<Nullable>`
   flip (AC5):** documented as evaluated with current infeasibility; it is a separately-gated,
   maintainer-decision step and is OUT OF SCOPE for default execution — the capstone does not flip
   any project-level `<Nullable>` element. (b) **`.claude/rules/csharp.md` conflict (AC4):** the
   rule documents forcing `/p:Nullable=enable` globally, which conflicts with the codebase's
   per-file opt-in convention. Policy prohibits editing `.claude/rules/*`, so the conflict is
   surfaced as a maintainer-decision item in the capstone spec.md (two options presented, no
   choice made); no rule file is edited. (c) **AC6 consolidation:** the capstone reproduces the
   epic's full source-cited maintainer-decision inventory in one place. The capstone reported
   `fable_policy: disabled` at preparation; the epic-run marker above carries the same policy.

6. **Epic-wide exclusions (not part of any child's opt-in set).** `Interfaces/**` (~62 files,
   near-zero CS86xx risk: pure interface declarations, CS8618 cannot fire) and
   `Properties/Resources.Designer.cs` + `Settings.Designer.cs` (fully generated, left
   null-oblivious). These carry no CS86xx debt under per-file pragma enforcement and do not
   diminish the definition-of-done.

## Wave Layering Summary

- **Wave 0 (6 children, no dependencies):** 363, 364, 366, 367, 368, 369.
- **Wave 1 (6 children):** 365, 370, 371, 372, 374, 375.
- **Wave 2 (1 child, capstone):** 376.

DAG re-validated at planning completion: 13 features, cycle-free, all `depends_on` edges resolve;
longest-path layering gives max wave 2.

## Status

Execution has NOT started. It will begin only when the user runs
`/epic-run utilitiescs-nullable-remediation` or replays the Invocation Prompt above.
