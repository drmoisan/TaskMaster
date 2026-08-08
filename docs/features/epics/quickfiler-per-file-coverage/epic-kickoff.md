# Epic Kickoff: quickfiler-per-file-coverage

Planned by epic-planner on 2026-08-08T14-00. All sixteen child features are prepared: issues
promoted, active folders created, research complete, spec/user-story written, atomic plans approved
and validated, preflight `PREFLIGHT: ALL CLEAR` on every child. Planning state:
`artifacts/orchestration/epic-planner-state.json` (branch:
`epic/quickfiler-per-file-coverage-integration`).

Parent epic issue: [#136](https://github.com/drmoisan/TaskMaster/issues/136) —
*Feature: quickfiler-80-per-file-coverage*.

## Invocation Prompt

Run `/epic-run quickfiler-per-file-coverage` to execute this epic, or paste the prompt below.

> Use the epic-orchestrator subagent to execute the prepared epic at
> `docs/features/epics/quickfiler-per-file-coverage/epic.md`. The integration branch
> `epic/quickfiler-per-file-coverage-integration` already contains every prepared feature folder and
> approved atomic plan; child features resume at atomic execution from their committed plan-path
> rather than re-planning. Execute per the epic-orchestrate skill: wave-scheduled child orchestrator
> runs in isolated worktrees, merge-on-green fan-in to the integration branch, and the final
> integration-to-main PR.

## Feature Summary

| issue_num | feature_folder | wave | complexity | plan-path |
| --- | --- | --- | --- | --- |
| #432 | `2026-08-07-quickfiler-coverage-ledger-432` | 0 | C3 | `docs/features/active/2026-08-07-quickfiler-coverage-ledger-432/plan.2026-08-07T20-41.md` |
| #430 | `2026-08-07-quickfiler-keyboard-actions-coverage-430` | 1 | C3 | `docs/features/active/2026-08-07-quickfiler-keyboard-actions-coverage-430/plan.2026-08-07T20-41.md` |
| #431 | `quickfiler-queue-admission-coverage` | 1 | C2 | `docs/features/active/quickfiler-queue-admission-coverage/plan.2026-08-07T20-41.md` |
| #433 | `2026-08-07-quickfiler-qfc-home-controller-coverage-433` | 1 | C3 | `docs/features/active/2026-08-07-quickfiler-qfc-home-controller-coverage-433/plan.2026-08-07T20-41.md` |
| #434 | `2026-08-07-quickfiler-helper-classes-coverage-434` | 1 | C3 | `docs/features/active/2026-08-07-quickfiler-helper-classes-coverage-434/plan.2026-08-07T20-41.md` |
| #435 | `2026-08-07-quickfiler-qfc-form-explorer-controller-coverage-435` | 1 | C3 | `docs/features/active/2026-08-07-quickfiler-qfc-form-explorer-controller-coverage-435/plan.2026-08-07T20-41.md` |
| #436 | `2026-08-07-quickfiler-datamodel-coverage-436` | 1 | C3 | `docs/features/active/2026-08-07-quickfiler-datamodel-coverage-436/plan.2026-08-07T20-42.md` |
| #437 | `2026-08-07-quickfiler-efc-home-controller-coverage-437` | 1 | C3 | `docs/features/active/2026-08-07-quickfiler-efc-home-controller-coverage-437/plan.2026-08-07T20-42.md` |
| #452 | `2026-08-07-quickfiler-efc-form-item-controller-coverage-452` | 1 | C3 | `docs/features/active/2026-08-07-quickfiler-efc-form-item-controller-coverage-452/plan.2026-08-07T22-35.md` |
| #453 | `2026-08-07-quickfiler-item-controller-coverage-453` | 1 | C3 | `docs/features/active/2026-08-07-quickfiler-item-controller-coverage-453/plan.2026-08-07T22-35.md` |
| #454 | `2026-08-07-quickfiler-collection-controller-coverage-454` | 1 | C3 | `docs/features/active/2026-08-07-quickfiler-collection-controller-coverage-454/plan.2026-08-07T22-35.md` |
| #455 | `2026-08-07-quickfiler-breadcrumb-dropdown-webview-coverage-455` | 1 | C3 | `docs/features/active/2026-08-07-quickfiler-breadcrumb-dropdown-webview-coverage-455/plan.2026-08-07T22-36.md` |
| #456 | `2026-08-07-quickfiler-itemviewer-coverage-456` | 1 | C3 | `docs/features/active/2026-08-07-quickfiler-itemviewer-coverage-456/plan.2026-08-07T22-38.md` |
| #495 | `2026-08-08-quickfiler-breadcrumb-bridge-coverage-495` | 1 | C3 | `docs/features/active/2026-08-08-quickfiler-breadcrumb-bridge-coverage-495/plan.2026-08-08T00-32.md` |
| #496 | `2026-08-08-quickfiler-form-viewers-bayesian-coverage-496` | 1 | C2 | `docs/features/active/2026-08-08-quickfiler-form-viewers-bayesian-coverage-496/plan.2026-08-08T09-52.md` |
| #497 | `2026-08-08-quickfiler-per-file-coverage-capstone-497` | 2 | C3 | `docs/features/active/2026-08-08-quickfiler-per-file-coverage-capstone-497/plan.2026-08-08T00-34.md` |

Waves: **0** = 1 feature (the enabler), **1** = 14 features, **2** = 1 feature (the capstone).
Wave 1 exceeds the parallelism cap of 8, so it executes in two batches.

## What epic-orchestrator Must Know Before Wave 0

These are the rulings and verified facts that preparation established. Every one is recorded in
full in `epic.md`; this is the index, not a substitute for reading it.

**Sequencing.** #432 is a genuine contract dependency, not stylistic ordering — it fixes the
coverage denominator, delivers the line-and-branch measurement harness every sibling reports
evidence with, and settles the exemption policy. Nothing in wave 1 can state its acceptance
criteria until it lands. #497 depends on all fifteen.

**Two harness defects are open and must be verified fixed before any figure is trusted.**
`#441` (coverage scripts double-count `<line>` nodes via a descendant axis) and `#478` (the merge
blends a correct class-level union with a primary-only method subtree). **Fixing #441 alone does not
fix #478.** If #432's harness does not address both, that is a Blocking finding.

**Never read the emitted `line-rate` / `branch-rate` attributes.** They are corrupt:
`MailActions.cs` emits `branch-rate="0.75"`, falsely passing the 75% gate against a true 72.7%.
Recompute from class-level `<line>` elements, unioning classes that share a filename with max-hits
per line.

**Branch coverage is frequently the binding gate**, not line coverage. Twelve files pass the line
floor and fail the branch floor in the corrected baseline. A file with `branches-valid = 0` reports
branch **N/A**, never 0%.

**Absence from a coverage report is never coverage.** An attribute on a partial *type* suppresses
every partial — confirmed on `QfcDatamodel.cs`, `ItemViewer.cs` and `QfcFormViewer.cs`.

**Four dispositions exist**, not three: `testable`, `ratified-exempt` (four grounds, including the
prohibited-to-execute adapter ground ratified for this epic only), `interface-only / not-measured`,
and `measured-not-gated` for generated files.

**A prior maintainer ratification supersedes the epic ledger.** Closed issue **#227** already
adjudicated the `QfcItemController` boundary; open issue **#230** tracks nine deliberately deferred
exemptions and is explicitly **not** a merge condition. Do not report those as gaps, and **no task
may build the #230 seam**.

**Repository-wide coverage is a self-consistent before/after pair** captured on the child's own
branch with identical command and post-processing. The previously cited 70.19% figure is
**withdrawn** — a raw-versus-post-processed comparison produced a fifteen-point phantom improvement
before it was caught.

**Toolchain.** `csharpier` is pinned at 1.2.6 and requires a subcommand:
`dotnet tool run csharpier format .`, not the bare form in `CLAUDE.md`. Every C# child needs a NuGet
restore in Phase 0 — `packages/` is gitignored and msbuild does not restore `packages.config`
projects, so build and test tasks otherwise die at `PrepareForBuild`.

**Committed plans are pure CRLF and the MCP plan validator accepts them.** Three children raised
this as a false alarm; do not spend effort normalizing line endings.

**Two shared files will conflict at fan-in, by design.** `QuickFiler/QuickFiler.csproj` and
`QuickFiler.Test/QuickFiler.Test.csproj` are both non-SDK with explicit compile lists and no
globbing. Every child adds test files, so every child edits the test csproj. Conflicts there are
additive and resolved by keeping both sides; this is not a decomposition defect.

## Known Execution-Time Follow-Ups

- **#435 (F6) needs a plan revision before execution.** epic-planner ruled that the dead
  `#region Email Sorting To Rewrite` in `QfcExplorerController.cs` is deleted inside F6 rather than
  deferred to #449; its approved plan does not yet cover that deletion, and the file cannot reach
  the floor without it.
- **#434 (F4) must promote eight deferred defects** through the MCP promotion lifecycle during its
  execution run rather than leaving them as feature-folder prose.
- **In-flight conflict risks:** #400 (overlaps #455 and #456), #424 (overlaps #433 and #431), #426
  (overlaps #434), and #440 (overlaps #495 and reaches #455/#456). For #440 specifically, tests pin
  **current** behavior and cite #440 so a future break is legible.

## Defects Promoted During Planning

Preparation surfaced and promoted a substantial defect trail rather than leaving findings as prose:
**#439, #441, #442, #443, #444, #445, #446, #447, #448, #449, #450, #451, #457, #458, #459, #460,
#461, #462, #463, #464, #465, #466, #467, #468, #469, #470, #471, #472, #473, #474, #475, #476,
#477, #478, #480, #481, #482, #483, #484, #485.** None is fixed by this epic, which is bound by a
no-behavior-change NFR; each is characterized by tests that pin current behavior.
