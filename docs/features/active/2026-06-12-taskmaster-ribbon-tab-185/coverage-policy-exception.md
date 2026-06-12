# Coverage Policy Exception — Issue #185 (Taskmaster Ribbon Tab)

- **Exception ID:** 185-COV-001
- **Date:** 2026-06-12
- **Authorizing authority:** Dan Moisan (repository owner)
- **Scope:** This pull request only (issue #185 / branch `TaskMaster-wt-2026-06-12-10-29`).
- **Status:** Approved

## Gate being excepted

`.claude/rules/csharp.md` and the feature-review coverage contract require repository-wide
C# line coverage to remain `>= 80%`. The canonical Cobertura artifact `artifacts/csharp/coverage.xml`
produced in remediation cycle 1 reports a repository-wide line-rate of **58.94%**
(`line-rate="0.5893769565947007"`, lines-covered 101852 / lines-valid 172813), which is below
the 80% floor.

## Authorized decision

For issue #185 only, the `>= 80%` repository-wide C# coverage gate is scoped to **changed/new
code**. The change-scope gates are met and govern the PASS judgment for this feature:

- The in-scope production change (`TaskMaster/Ribbon/RibbonExplorer.xml`) is a single-line,
  content-preserving edit to a non-compiled XML resource with no instrumentable IL, so it
  introduces no new production lines and cannot regress changed-line coverage.
- The in-scope test file (`TaskMaster.Test/Ribbon/RibbonExplorerXmlTests.cs`) is 98.82% covered
  (line-rate 1.00 for authored source; the only uncovered lines are compiler-generated).
- No changed line shows a coverage regression.

## Rationale

The 58.94% repository-wide figure is a **pre-existing, repository-wide condition** driven by
under-covered legacy COM/VSTO/WinForms production assemblies (for example `TaskVisualization`
0.37%, `ToDoModel` 10.8%, `QuickFiler` 25.2%, `TaskMaster` 25.8%) and bundled third-party DLLs.
It is not introduced by issue #185. The first feature-review could not evaluate the gate because
the canonical coverage artifact did not exist; cycle 1 produced it, which made the long-standing
shortfall visible.

This decision is consistent with the precedent recorded for issue #171, where the same
pre-existing condition (57.99% repository-wide) was accepted with a documented
pre-existing-condition justification because the change-scope gates were met.

The repository CI workflow (`.github/workflows/ci.yml`) does not enforce an 80% coverage
threshold as a required check, so this exception affects the feature-review policy judgment only
and does not alter any required CI gate.

## Boundaries (what this exception does NOT do)

- It does not modify, weaken, or reword the `>= 80%` threshold in `.claude/rules/csharp.md` or any
  policy document. The policy text is unchanged.
- It does not apply to any other issue, branch, or future work.
- It does not waive the change-scope coverage gates (>= 90% new code; no changed-line regression),
  which remain in force and are satisfied here.

## Committed follow-up

Raising repository-wide C# coverage toward the 80% floor (resolution option 1 from
`remediation-inputs.2026-06-12T11-37.md`) will be pursued as separate, explicitly scoped and
tracked work on a new branch after this PR merges. It is not folded into the #185 branch.
