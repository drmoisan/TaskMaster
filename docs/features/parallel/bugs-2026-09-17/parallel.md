---
parallel: bugs-2026-09-17
mode: closed
max_concurrency: 2
created_at: "2026-09-17T00:00:00Z"
items:
  - issue_num: 895
    feature_folder: "docs/features/active/2026-09-14-fsharp-core-hintpath-netstandard21-skew-895"
    kind: bug
    state: prepared
    blast_radius:
      paths:
        - ".../evidence/baseline/analyzer-baseline.2026-09-16T23-27.md"
        - ".../evidence/baseline/channel-probe.2026-09-16T23-27.md"
        - ".../evidence/baseline/format-baseline.2026-09-16T23-27.md"
        - ".../evidence/baseline/nullable-baseline.2026-09-16T23-27.md"
        - ".../evidence/baseline/phase0-instructions-read.2026-09-16T23-27.md"
        - ".../evidence/baseline/test-coverage-baseline.2026-09-16T23-27.md"
        - ".../evidence/baseline/toolchain-bootstrap.2026-09-16T23-27.md"
        - ".../evidence/baseline/tree-baseline.2026-09-16T23-27.md"
        - ".../evidence/issue-updates/issue-895.2026-09-16T23-27.md"
        - ".../evidence/other/ac-status.2026-09-16T23-27.md"
        - ".../evidence/other/hintpath-edits.2026-09-16T23-27.md"
        - ".../evidence/other/post-format-sweep.2026-09-16T23-27.md"
        - ".../evidence/other/remarks-correction.2026-09-16T23-27.md"
        - ".../evidence/other/scope-boundary-diff.2026-09-16T23-27.md"
        - ".../evidence/qa-gates/analyzer-final.2026-09-16T23-27.md"
        - ".../evidence/qa-gates/coverage-delta.2026-09-16T23-27.md"
        - ".../evidence/qa-gates/format-final.2026-09-16T23-27.md"
        - ".../evidence/qa-gates/loop-closure.2026-09-16T23-27.md"
        - ".../evidence/qa-gates/nullable-final.2026-09-16T23-27.md"
        - ".../evidence/qa-gates/test-final.2026-09-16T23-27.md"
        - ".../evidence/regression-testing/expect-fail-build.2026-09-16T23-27.md"
        - ".../evidence/regression-testing/expect-fail-shape-a.2026-09-16T23-27.md"
        - ".../evidence/regression-testing/expect-fail-shape-b.2026-09-16T23-27.md"
        - ".../evidence/regression-testing/pass-after-bootstrap-namespace.2026-09-16T23-27.md"
        - ".../evidence/regression-testing/pass-after-build.2026-09-16T23-27.md"
        - ".../evidence/regression-testing/pass-after-shape-a.2026-09-16T23-27.md"
        - ".../evidence/regression-testing/pass-after-shape-b.2026-09-16T23-27.md"
        - ".../evidence/regression-testing/test-authoring.2026-09-16T23-27.md"
        - ".../research/2026-09-16T23-30-fsharp-core-hintpath-research.md"
        - ".../spec.md"
        - "QuickFiler.Test/QuickFiler.Test.csproj"
        - "QuickFiler/QuickFiler.csproj"
        - "TaskMaster.Test/Bootstrap/FSharpCoreDeployedIdentityTests.cs"
        - "TaskMaster.Test/Bootstrap/FSharpCoreHintPathAlignmentTests.cs"
        - "TaskMaster.Test/Bootstrap/NetstandardBindChildDomainTests.cs"
        - "TaskMaster.Test/Ribbon/RibbonControllerTests.cs"
        - "TaskMaster.Test/TaskMaster.Test.csproj"
        - "TaskMaster/TaskMaster.csproj"
        - "ToDoModel.Test/ToDoModel.Test.csproj"
        - "ToDoModel/ToDoModel.csproj"
        - "UtilitiesCS.Test/UtilitiesCS.Test.csproj"
        - "UtilitiesCS/Bootstrap/AssemblyBindingFallback.cs"
        - "UtilitiesCS/UtilitiesCS.csproj"
        - "coverage/coverage.cobertura.xml"
        - "coverage/logs/p0-t6-nullable.txt"
        - "coverage/logs/p4-t1-build.txt"
        - "coverage/logs/p4-t7-analyzer.txt"
        - "coverage/logs/p4-t8-nullable.txt"
        - "coverage/logs/p4-t9-runner.txt"
        - "docs/features/active/2026-09-14-fsharp-core-hintpath-netstandard21-skew-895/**"
        - "docs/features/active/2026-09-14-fsharp-core-hintpath-netstandard21-skew-895/issue.md"
        - "docs/features/active/2026-09-14-fsharp-core-hintpath-netstandard21-skew-895/plan.2026-09-16T23-27.md"
        - "docs/features/active/2026-09-14-fsharp-core-hintpath-netstandard21-skew-895/spec.md"
        - "evidence/other/preflight-clearance.2026-09-14T01-55.md"
      modules: []
      shared_surfaces: []
      contracts: []
      source: declared
      computed_at: "2026-09-17T00:00:00Z"
  - issue_num: 900
    feature_folder: "docs/features/active/breadcrumb-thread-affinity-tests-assume-taskrun-distinct-thread-900"
    kind: bug
    state: prepared
    blast_radius:
      paths:
        - ".claude/**"
        - ".claude/hooks/**"
        - ".claude/hooks/enforce-orchestration-preimplementation-gate-helpers.ps1"
        - ".claude/hooks/enforce-orchestration-preimplementation-gate.ps1"
        - ".claude/hooks/validate-planner-output.ps1"
        - ".claude/settings.json"
        - ".claude/skills/feature-promotion-lifecycle/SKILL.md"
        - "FEATURE/evidence/baseline/phase0-instructions-read.md"
        - "FEATURE/evidence/other/commit-message-final.txt"
        - "FEATURE/evidence/other/commit-message-fix.txt"
        - "FEATURE/issue.md"
        - "FEATURE/plan.2026-09-16T23-27.md"
        - "FEATURE/research/2026-09-16T23-50-breadcrumb-thread-affinity-tests-taskrun-distinct-thread-research.md"
        - "FEATURE/spec.md"
        - "FEATURE/user-story.md"
        - "QuickFiler.Test/QuickFiler.Test.csproj"
        - "QuickFiler.Test/Viewers/BreadcrumbPopupBoundaryCoverageTests.cs"
        - "QuickFiler.Test/Viewers/BreadcrumbUiThreadDispatchTests.cs"
        - "QuickFiler.Test/Viewers/ItemViewerBreadcrumbThreadAffinityTests.cs"
        - "QuickFiler/Viewers/ItemViewer.Breadcrumb.cs"
        - "QuickFiler/Viewers/ItemViewer.Breadcrumb.cs:80"
        - "QuickFiler/Viewers/ItemViewer.cs"
        - "QuickFiler/Viewers/ItemViewer.cs:20"
        - "UtilitiesCS.Test/Threading/UiThreadInitContract_Tests.cs"
        - "WindowsBase/System/Windows/Threading/Dispatcher.cs"
        - "coverage/final-900.cobertura.xml"
        - "coverage/plan900-helper.ps1"
        - "docs/features/active/breadcrumb-thread-affinity-tests-assume-taskrun-distinct-thread-900/**"
        - "docs/features/active/breadcrumb-thread-affinity-tests-assume-taskrun-distinct-thread-900/evidence/other/commit-message-final.txt"
        - "docs/features/active/breadcrumb-thread-affinity-tests-assume-taskrun-distinct-thread-900/evidence/other/commit-message-fix.txt"
        - "docs/features/active/breadcrumb-thread-affinity-tests-assume-taskrun-distinct-thread-900/research/2026-09-16T23-50-breadcrumb-thread-affinity-tests-taskrun-distinct-thread-research.md"
        - "docs/features/active/breadcrumb-thread-affinity-tests-assume-taskrun-distinct-thread-900/spec.md"
        - "docs/features/archive/2026-07-07-onedrive-writer-timeout-test-determinism-253/evidence/regression-testing/fail-before-exception.2026-07-07T14-05.md"
        - "docs/features/potential/promoted/2026-09-05-breadcrumb-ui-boundary-guard-rejects-dispatcher-built-viewers.md"
        - "packages/**"
      modules: []
      shared_surfaces:
        - ".claude/settings.json"
      contracts: []
      source: declared
      computed_at: "2026-09-17T00:00:00Z"
expected_conflict_components:
  - name: fsharp-core-hintpath
    members:
      - 895
  - name: breadcrumb-thread-affinity
    members:
      - 900
---

# Parallel Run: bugs-2026-09-17

Two thematically unrelated TaskMaster defects, planned to preflight clearance and cohort-seeded on
2026-09-17. Base: `origin/main` at `91746d2e4776a59ee1db1856c5c490a009c4958b`. Plan-home branch:
`parallel/bugs-2026-09-17-plan`. Planner checkpoint:
`artifacts/orchestration/parallel-planner-state.json`.

Both items were filed during run `bugs-2026-09-11` from findings that would otherwise have
evaporated at merge.

## Items

| issue_num | kind | subject | branch |
| --- | --- | --- | --- |
| 895 | bug | FSharp.Core HintPath split between the netstandard2.0 and netstandard2.1 flavours | `bug/fsharp-core-hintpath-netstandard21-skew-895` |
| 900 | bug | Breadcrumb thread-affinity tests assume `Task.Run` yields a distinct thread | `bug/breadcrumb-thread-affinity-tests-assume-taskrun-distinct-thread-900` |

Both items are `state: prepared`, both cleared preflight in 2 rounds with the exact signal
`PREFLIGHT: ALL CLEAR`, and both declared radii passed V1, V2 and V3 with zero findings.

## Cohorts at generation 0

One cohort. The two items do not contend, so they execute concurrently under the
`max_concurrency: 2` cap.

| cohort | item_keys |
| --- | --- |
| 0 | 895, 900 |

`conflict_edges` is empty. Recomputation parity against `compute-cohorts.sh` passed: recomputing
over the recorded keys and the recorded (empty) edge set reproduces `[[895,900]]` exactly.

## Why there is no conflict edge, stated explicitly

The two declared radii intersect on exactly one path, `QuickFiler.Test/QuickFiler.Test.csproj`.
That path is a member of the mechanically-mergeable path class configured in
`config/blast-radius.json` under `mergeable_paths` (`**/*.csproj`), so `Test-BlastRadiusConflict`
contributes no `path_overlap` edge for it. The path remains in item 895's declared radius and is
still read by every audit; only the pairwise overlap comparison is narrowed.

This is recorded here so that the absent edge is not later mistaken for an oversight or for a
narrowed radius. No radius was narrowed to suppress an edge.

## Adjacency: issue 898 is not in this run

Item 895 edits FSharp.Core `HintPath` values in `QuickFiler/QuickFiler.csproj`,
`QuickFiler.Test/QuickFiler.Test.csproj` and `ToDoModel/ToDoModel.csproj`, and adds two
`<Compile Include>` items to `TaskMaster.Test/TaskMaster.Test.csproj`. Issue 898 (Meziantou analyzer
HintPath skew) touches the same class of file and is NOT in this run.

Under the current truth table those two items would still not acquire a `path_overlap` edge, because
`**/*.csproj` is mergeable. That is the correct scheduling answer — two items appending or amending
distinct entries in an additive project-file registry are resolvable by a deterministic union — but
it means the overlap will not be visible as contention if 898 is scheduled later. It is recorded
here instead. Item 895's plan was directed to confine its edits to FSharp.Core `HintPath` values and
not to reformat, reorder or renumber surrounding items, which is what keeps the union mechanical.

## Advisory: item 900's declared radius is materially over-broad

Item 900's approved plan is a test-only change whose write set is the single file
`QuickFiler.Test/Viewers/ItemViewerBreadcrumbThreadAffinityTests.cs`. Its derived radius
nevertheless carries 35 paths and one shared surface. The following declared entries are citations
the plan makes for diagnostic reasons, not write claims, and the extractor cannot currently tell the
difference:

- `.claude/**`, `.claude/hooks/**`, `.claude/settings.json`, `.claude/hooks/enforce-orchestration-preimplementation-gate.ps1`, `.claude/hooks/enforce-orchestration-preimplementation-gate-helpers.ps1`, `.claude/hooks/validate-planner-output.ps1`, `.claude/skills/feature-promotion-lifecycle/SKILL.md` — cited because the plan documents the pre-implementation gate seeding its execution session will need. `config/blast-radius.json` lists `.claude/rules/**` under `mandate_reads` but not `.claude/hooks/**` or `.claude/settings.json`, so these survive the exclusion pass.
- `.claude/settings.json` is additionally resolved as a SHARED SURFACE. Item 895 declares no shared surface, so no `shared_surface_overlap` edge arises in this run; a later item that genuinely writes that file would serialize against 900 for no real reason.
- `FEATURE/issue.md`, `FEATURE/spec.md`, `FEATURE/plan.2026-09-16T23-27.md`, `FEATURE/user-story.md`, `FEATURE/research/...`, `FEATURE/evidence/...` — a literal `FEATURE/` stand-in that names nothing on disk.
- `WindowsBase/System/Windows/Threading/Dispatcher.cs` — a framework source path, not a repository file.
- `QuickFiler/Viewers/ItemViewer.Breadcrumb.cs:80` and `QuickFiler/Viewers/ItemViewer.cs:20` — line-locator tokens, admitted alongside the same paths without the locator.
- `packages/**` — a restore directory.
- `coverage/final-900.cobertura.xml`, `coverage/plan900-helper.ps1` — gitignored per-worktree build output (`.gitignore:144`).

Item 895's radius carries the same gitignored-build-output class: `coverage/coverage.cobertura.xml`
and five `coverage/logs/*` entries (`.gitignore:144` and `.gitignore:348`).

**The over-report was measured, not assumed, and is not load-bearing for this run's schedule.** The
only pairwise verdict in a two-item run is 895 against 900, and it is `False`. A narrower radius for
either item yields the same verdict, so a correction round would change no scheduling decision. No
radius was narrowed: narrowing to suppress an edge is prohibited, and none of these entries
triggered a V1, V2 or V3 finding, so no re-planning round was owed under the planner procedure.

**Two consequences the executing surface should carry forward.** First, drift detection compares the
declared radius against the paths a diff actually touched; an over-broad declared radius fails OPEN,
so a genuine escape into `.claude/**` by item 900 would not be reported as drift. Second, this run is
`mode: closed`. If it is reopened so that `/parallel-add` can admit further items, item 900's radius
should be tightened first, because `.claude/**` and `packages/**` will collide with almost anything
subsequently admitted.

These are defects in the derivation truth table and the extractor, both of which are push-down-owned
from drm-copilot. They are recorded here rather than worked around locally.

## Issue 899 was not admitted to this run

The operator's intake named three items. Issue 899 (two coexisting acceptance-criteria numbering
schemes in one spec) was made conditional on its fix not landing in the push-down-owned `.claude`
tree, with an instruction to stop and report if it did. It does, so 899 was never delegated to a
preparation child and does not appear in `items` above.

**Ownership.** The authoring contract is
`.claude/skills/acceptance-criteria-tracking/SKILL.md` lines 43-50, whose line 44 permissively
allows three different acceptance-criteria headings for every non-`minor-audit` work mode and whose
line 50 forbids reformatting non-checkbox criteria. The only authoring-time gate that parses
criteria out of a `spec.md` is `.claude/hooks/validate-prd-feature-output.ps1` line 66, bound to the
`prd-feature` agent by `.claude/agents/prd-feature.md` line 20. The scaffold that injects stray
checkboxes into every newly promoted `spec.md` is not in this repository at all; it is in the
drm-copilot MCP resources bundle. Both `.claude` files were verified byte-identical to their
drm-copilot copies, so ownership is measured rather than inferred.

**The issue's premise is also factually wrong.**
`docs/features/active/2026-09-11-ci-coverage-threshold-and-pester-gates-869/spec.md` does not carry
two numbering schemes. It carries one `## Acceptance Criteria` section at lines 239-271 holding 31
checkbox entries, all 31 of which carry a `(#NNN)` issue-attribution prefix: 7 for `#561`, 9 for
`#562`, 15 for `#869`. The reported 15 is the `#869` subset of that same 31-entry list. The genuine
placement defect in 869 is 4 checkbox lines outside the section, at lines 24-27, copied in by the
promotion scaffold. The `(#NNN)` form appears in exactly 1 of 86 active specs.

**The real defect is larger than the one filed.** 52 of 86 active `spec.md` files carry checkbox
lines outside their acceptance-criteria section; 5 have zero in-section criteria because the feature
template ships `## Definition of Done` instead of an `## Acceptance Criteria` heading; and a third
label scheme, `AC<n>`, is in use in 41 files and is what the executor greps for.

**Recommended disposition.** Correct the premise recorded on 899, then split it: file the scheme
declaration, the authoring-time gate and the scaffold templates upstream against drm-copilot, and if
the repo-local slice is wanted as a TaskMaster item, re-scope 899 to that slice explicitly. A
repo-local slice is available and would survive push-down, because the push-down engine overwrites
payload paths and never deletes destination-only files. Re-scoping an issue is a caller decision,
not a planner default, so it was not planned here.

## Execution

Execution has NOT started. It begins only when the operator runs `/parallel-run bugs-2026-09-17` or
replays the kickoff prompt in `parallel-kickoff.md` from the main session. Each item resumes at
atomic execution from its committed plan-path on its own pushed feature branch, and each item opens
its own pull request against `main`. There is no integration branch.

Two execution prerequisites were reported by the preparation children and are recorded here because
they will stop a run that does not satisfy them:

1. Both plans contain command tasks. A worktree-isolated execution session may have `pwsh` refused
   by the Bash-tool isolation filter; item 900's plan stops at task P0-T4 with
   `CHANNEL UNAVAILABLE` in that case. Both plans carry a two-rung channel probe rather than
   hard-coding an outcome, but the executing surface should expect to run item children without
   worktree isolation, reusing each item's existing preparation worktree.
2. Each item's execution session needs `artifacts/orchestration/orchestrator-state.json` seeded with
   `issue-num`, a `docs/features/active/...` `feature-folder`, `route_id` and `path_selected`, and
   `lifecycle_ready: true`, or item 900's plan stops at task P0-T3 with
   `PRE-IMPLEMENTATION GATE NOT SEEDED`.
