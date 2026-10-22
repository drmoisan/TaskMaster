# Settle the issue #468 residual reflective-caller risk repository-wide (Issue #635)

- Date captured: 2026-08-26
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/Settle_the_issue_468_residual_reflective-caller_risk_repository-wide/ (Issue #635)
- Captures: **follow-up candidate 9** of `## Follow-up Candidates` in
  `docs/features/active/qfc-collection-controller-defects-468/spec.md`
- Origin: issue **#468**, task `[P14-T5]`
- Origin feature folder: `docs/features/active/qfc-collection-controller-defects-468`

- Issue: #635
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/635
- Last Updated: 2026-08-26
- Work Mode: full-bug

## Summary

Issue #468 removed twelve dead members from `QuickFiler/Controllers/QfcCollectionController.cs`.
Compilation proves no compile-time caller survived, but it cannot prove the absence of a caller that
reaches a member by name at runtime. AC-16 required a residual-risk search to close that gap, and
that search was performed and recorded:
`docs/features/active/qfc-collection-controller-defects-468/evidence/other/p1-t1-reflective-caller-search.2026-08-26T08-25.md`.

Candidate 9 asks for a broader search **if the AC-16 search is judged insufficient**. This entry
records the condition and the scope a broader search would take, so the judgment can be made
deliberately rather than by omission.

## What the AC-16 search actually covered

- **Search (a)** — `*.csproj`, `*.resx`, `*.config`, `*.xaml`, `*.json`, `*.settings` across 398
  build-input files, for all twelve identifiers, excluding `docs/`, `.claude/`, `packages/`, and
  `TestResults/`. Result: **zero hits**. The artifact also records the measured non-vacuity of the
  scope, so the zero is not an artefact of an empty search set.
- **Search (b)** — all 42 `GetMethod(` call sites and all 0 `InvokeMember(` call sites in first-party
  C#. Result: **none passes any of the twelve identifiers**.

## What a broader search would add

- Every non-`.cs` file type in the repository, not only the six build-input extensions — for example
  `*.txt`, `*.md` outside `docs/`, `*.ps1`, and any embedded resource.
- `GetMember(`, `GetProperty(`, `GetMethods(` followed by a name filter, and `Type.GetType(` +
  `Activator` paths, in addition to `GetMethod(` and `InvokeMember(`.
- The `QuickFiler` tree specifically, including designer and resource files.

## Assessment

The residual risk is low on its own terms. The twelve removed members are ordinary instance methods
on a controller with no serialization surface and no data-binding surface, so there is no mechanism by
which a name-based caller would be expected to exist. The AC-16 search is judged **sufficient** for
the merge of issue #468; this entry exists so that judgment is recorded and reversible rather than
implicit.

Promote this entry only if a name-resolution failure is later observed in the QuickFiler tree, or if a
subsequent removal in the same file wants a stronger baseline than AC-16 provides.

## Acceptance ideas (for the promoted entry to refine)

- A repository-wide search over all file types for the twelve identifiers, with the scope size
  measured and recorded so a zero result is demonstrably non-vacuous.
- An enumeration of every reflection entry point in the `QuickFiler` tree, not only `GetMethod(` and
  `InvokeMember(`.
- A recorded decision either closing the risk or naming the specific caller found.
