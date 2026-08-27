# [P5-T25] Deferral of AC-472-10

Timestamp: 2026-08-27T20-16
Command: none — this artifact records a deferral decision
EXIT_CODE: 0
Output Summary: AC-472-10's promotion and issue-creation clauses are satisfied (potential entry plus
GitHub issue #644); its PR-body clause is not, because this preparation run does not create the
integration pull request. The criterion is conjunctive, so the checkbox is deliberately left
unchecked.

DEFERRED-TO-ORCHESTRATOR: AC-472-10 promotion and issue creation are outside this feature's scope per decision D-472-B

PostedAs: unknown

No GitHub issue comment or body update was posted by this task. This artifact is a local deferral
record, not a mirrored issue update; it lives under `evidence/issue-updates/` because it concerns the
disposition of an issue-level criterion.

## The criterion, verbatim from `spec.md`

> The unbracketed-removal count-mismatch defect described in `### Downstream notes` item 3 is
> promoted through the feature-promotion lifecycle into a new potential entry **and** a new GitHub
> issue, and the issue number is recorded in this feature's PR body. Prose in this folder alone does
> not satisfy this criterion.

## Clause-by-clause status

| Clause | Status | Evidence |
| --- | --- | --- |
| promoted into a new potential entry | **satisfied** | `docs/features/potential/promoted/2026-08-27-qfc-unregister-navigation-count-mismatch-orphan.md`, present in the branch diff at `[P5-T1]` |
| promoted into a new GitHub issue | **satisfied** | issue **#644**, `https://github.com/drmoisan/TaskMaster/issues/644`, created by commit `12256da4 docs(444): promote count-mismatch follow-up defect as issue #644` |
| the issue number is recorded in this feature's PR body | **outstanding** | no pull request exists yet; this preparation run does not create one |

Because the criterion joins its clauses with **and**, two of three satisfied is not satisfaction. The
checkbox stays `- [ ]` with its text unmodified.

## Why the underlying defect was not fixed here — decision D-472-B

The count-mismatch defect is distinct from the filed width mismatch this feature does fix.
`UnregisterNavigation` bounds its unregister loop with the *current* `_itemGroups.Count`, while
`RemoveSpecificControlGroup(int)` mutates `_itemGroups` with no unregister/register bracket around
the mutation, reachable from `RemoveBelowThresholdAsync` via the `RemoveGroupByEntryId` seam and from
the `'R'` char action. Fixing it requires the key-ledger design, which breaks the characterisation
tests in `QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs` — a file at exactly 500 lines
whose `[TestMethod]` count upstream #468 freezes.

`CLAUDE.md`'s **Bugfix Workflow step 2** governs this directly:

> Change only what is needed to make the failing test pass; keep boundaries intact and avoid
> opportunistic refactors. If you uncover deeper design problems, open a new issue instead of
> widening scope.

Decision **D-472-B** of the atomic plan applies that rule: the defect is promoted, not absorbed. This
feature's #472 regression test asserts the residual orphan explicitly — exactly one `"10"` entry
remains — and carries an XML documentation comment attributing that residual to the follow-up defect
and stating that it is out of this feature's scope, so the assertion does not silently absorb the
second defect.

## What the orchestrator must do

`docs/features/active/quickfiler-keyboard-action-defects-444/evidence/other/p5-t9-pr-body-inputs.2026-08-27T20-13.md`
carries the outstanding item to the orchestrator under its heading
`## Item 4 — Outstanding promotion of the UnregisterNavigation count-mismatch defect`. That section
states the issue number **#644** and its URL in the exact form the PR body needs. Once the
integration pull request records #644, this criterion becomes satisfiable and may be checked off by
whoever authors that body.

## Acceptance

- The artifact carries the required `DEFERRED-TO-ORCHESTRATOR:` line — met; it appears above,
  verbatim.
- That criterion in `spec.md` remains `- [ ]` with its text unmodified — met; the checkbox was not
  touched.
