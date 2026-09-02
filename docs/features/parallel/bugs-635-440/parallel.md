---
parallel: bugs-635-440
mode: closed
max_concurrency: 2
created_at: "2026-08-29T06:30:00Z"
items:
  - issue_num: 440
    feature_folder: docs/features/active/2026-08-07-breadcrumb-left-right-arrow-parent-child-navigation-440
    kind: bug
    state: prepared
    blast_radius:
      paths:
        - UtilitiesCS/OutlookObjects/Folder/BreadcrumbStateModel.cs
        - UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbStateModelSequenceTests.cs
        - UtilitiesCS.Test/OutlookObjects/Folder/FolderBreadcrumbBridgeRouterTests.cs
        - docs/features/active/2026-08-07-breadcrumb-left-right-arrow-parent-child-navigation-440/**
        - docs/features/active/2026-08-07-breadcrumb-left-right-arrow-parent-child-navigation-440/research/arrow-navigation-contract.2026-08-29T00-52.md
        - docs/features/active/breadcrumb-router-navigation-defects-498/spec.md
        - docs/features/active/breadcrumb-router-navigation-defects-498/evidence/qa-gates/p7-t7-ac21-supersession-record.md
      modules:
        - UtilitiesCS
        - UtilitiesCS.Test
      shared_surfaces: []
      contracts: []
      source: declared
      computed_at: "2026-08-29T06:30:00Z"
  - issue_num: 635
    feature_folder: docs/features/active/2026-08-26-issue-468-residual-reflective-caller-risk-635
    kind: bug
    state: prepared
    blast_radius:
      paths:
        - docs/features/active/2026-08-26-issue-468-residual-reflective-caller-risk-635/**
        - docs/features/active/2026-08-26-issue-468-residual-reflective-caller-risk-635/issue.md
        - docs/features/active/2026-08-26-issue-468-residual-reflective-caller-risk-635/spec.md
        - docs/features/active/2026-08-26-issue-468-residual-reflective-caller-risk-635/plan.2026-08-29T00-23.md
        - docs/features/active/2026-08-26-issue-468-residual-reflective-caller-risk-635/research/reflective-caller-closure.md
        - docs/features/active/2026-08-26-issue-468-residual-reflective-caller-risk-635/evidence/baseline/phase0-instructions-read.2026-08-29T04-55.md
        - docs/features/active/2026-08-26-issue-468-residual-reflective-caller-risk-635/evidence/baseline/p0-t2-requirements-inputs.2026-08-29T04-55.md
        - docs/features/active/2026-08-26-issue-468-residual-reflective-caller-risk-635/evidence/baseline/p0-t3-worktree-baseline.2026-08-29T04-55.md
        - docs/features/active/2026-08-26-issue-468-residual-reflective-caller-risk-635/evidence/baseline/p0-t4-identifier-derivation.2026-08-29T04-55.md
        - docs/features/active/2026-08-26-issue-468-residual-reflective-caller-risk-635/evidence/baseline/p0-t5-scope-census.2026-08-29T04-55.md
        - docs/features/active/2026-08-26-issue-468-residual-reflective-caller-risk-635/evidence/other/p1-t1-partition-a-sweep.2026-08-29T04-55.md
        - docs/features/active/2026-08-26-issue-468-residual-reflective-caller-risk-635/evidence/other/p1-t2-partition-a-control.2026-08-29T04-55.md
        - docs/features/active/2026-08-26-issue-468-residual-reflective-caller-risk-635/evidence/other/p1-t3-partition-b-classification.2026-08-29T04-55.md
        - docs/features/active/2026-08-26-issue-468-residual-reflective-caller-risk-635/evidence/other/p1-t4-partition-c-enumeration.2026-08-29T04-55.md
        - docs/features/active/2026-08-26-issue-468-residual-reflective-caller-risk-635/evidence/other/p1-t5-untracked-pass.2026-08-29T04-55.md
        - docs/features/active/2026-08-26-issue-468-residual-reflective-caller-risk-635/evidence/other/p2-t1-reflection-inventory.2026-08-29T04-55.md
        - docs/features/active/2026-08-26-issue-468-residual-reflective-caller-risk-635/evidence/other/p2-t2-production-reflection-classification.2026-08-29T04-55.md
        - docs/features/active/2026-08-26-issue-468-residual-reflective-caller-risk-635/evidence/other/p2-t3-variable-argument-closure.2026-08-29T04-55.md
        - docs/features/active/2026-08-26-issue-468-residual-reflective-caller-risk-635/evidence/other/p2-t4-binding-serialization-surface.2026-08-29T04-55.md
        - docs/features/active/2026-08-26-issue-468-residual-reflective-caller-risk-635/evidence/other/p3-t1-ac16-corrections.2026-08-29T04-55.md
        - docs/features/active/2026-08-26-issue-468-residual-reflective-caller-risk-635/evidence/other/p3-t3-decision-record.2026-08-29T04-55.md
        - docs/features/active/2026-08-26-issue-468-residual-reflective-caller-risk-635/evidence/other/p3-t4-zero-result-audit.2026-08-29T04-55.md
        - docs/features/active/2026-08-26-issue-468-residual-reflective-caller-risk-635/evidence/qa-gates/p4-t2-no-modification-proof.2026-08-29T04-55.md
        - docs/features/active/2026-08-26-issue-468-residual-reflective-caller-risk-635/evidence/qa-gates/p4-t3-toolchain-gate.2026-08-29T04-55.md
        - docs/features/active/2026-08-26-issue-468-residual-reflective-caller-risk-635/evidence/qa-gates/p4-t4-host-identity-scan.2026-08-29T04-55.md
        - docs/features/active/2026-08-26-issue-468-residual-reflective-caller-risk-635/evidence/qa-gates/p4-t7-ac-reconciliation.2026-08-29T04-55.md
        - docs/features/active/2026-08-26-issue-468-residual-reflective-caller-risk-635/evidence/regression-testing/fail-before-exception.2026-08-29T04-55.md
      modules: []
      shared_surfaces: []
      contracts: []
      source: declared
      computed_at: "2026-08-29T05:45:00Z"
expected_conflict_components:
  - name: breadcrumb-state-model
    members:
      - 440
  - name: reflective-caller-audit
    members:
      - 635
---

# Parallel run bugs-635-440

Two thematically unrelated open bugs, planned as one parallel run on 2026-08-29.

## Items

| issue | kind | complexity | branch | summary |
| --- | --- | --- | --- | --- |
| 440 | bug | C3 | `bug/breadcrumb-left-right-arrow-parent-child-navigation-440` | Breadcrumb Left-arrow parent selection stops after one level on the Qfc surface. |
| 635 | bug | C3 | `bug/issue-468-residual-reflective-caller-risk-635` | Settle the issue 468 residual reflective-caller risk with a repository-wide search. |

## Derived contention

The two declared blast radii do not conflict. `Test-BlastRadiusConflict` reports
`conflict = False` with zero reasons, in both argument orders. The radii are disjoint on all four
axes:

- **paths** — item 440 writes `UtilitiesCS/OutlookObjects/Folder/BreadcrumbStateModel.cs` and two
  `UtilitiesCS.Test` files; item 635 writes only inside its own feature folder.
- **modules** — item 440 carries `UtilitiesCS` and `UtilitiesCS.Test`; item 635 carries none,
  because its plan modifies no production source and cites its search targets as reads rather than
  writes.
- **shared_surfaces** — both empty.
- **contracts** — both empty.

The conflict graph therefore has no edges, and generation-0 cohort seeding places both items in
cohort 0.

## Scope note on item 440

Most of the original statement of issue 440 already landed on `main` as a secondary payload of
feature 498, whose `spec.md` records that it also closes 440 and 499. The residual defect this run
addresses is Qfc-only: `BreadcrumbStateModel.LeftArrow()` gates its parent-select on the active
index equalling the last chain position, so Left walks up exactly one level, while
`BreadcrumbBridgeRouter.Arrows.cs` already walks to the root. The eight surfaces the issue body
names are not all in the residual radius.

## Contention this run cannot see

Cohort scheduling covers only the items in this manifest. At planning time the following branches
held in-flight work on adjacent QuickFiler and coverage surfaces and are invisible to this run:

- `feature/quickfiler-breadcrumb-bridge-coverage-r2`
- `feature/quickfiler-per-file-coverage-capstone-r2`
- `bug/winformspumphost-suite-determinism-511`

Re-check these before executing the run. Item 440 is the exposed item.

## Merge-time overlap outside the blast radius

Both item branches modify `.claude/agent-memory/atomic-planner/MEMORY.md` and
`.claude/agent-memory/task-researcher/MEMORY.md`, which the two preparation children wrote as
bookkeeping side effects. `.claude/agent-memory/**` is a configured `mandate_reads` exclusion, so
derivation drops it and it contributes no conflict edge. That exclusion is correct for scheduling —
every agent-driven branch in this repository writes those append-only index files, so treating them
as contention would serialize every run — but it means the second of the two pull requests to merge
may need a trivial append-order conflict resolution in those two files. This is a merge-time cost,
not a scheduling constraint.

## Known environment defect affecting execution

A Dependabot bump moved `packages.config` to Meziantou 3.0.174 and Roslynator 4.16.1 but left 80
analyzer-include paths on the previous versions, so a fresh worktree fails an msbuild rebuild with
CS0006. The item 440 plan carries a gitignored bootstrap step as a workaround. The solution-wide fix
is out of scope for both items and is not yet promoted to an issue.
