# Follow-up promotion handoff (AC-29 input)

Timestamp: 2026-08-26T16-32

Command:

```
gh issue list --state open --limit 200 --json number,title
gh issue view 623 --json number,title,state,body
gh issue view 444 --json number,title,state
gh issue list --state open --search "<term>" --limit 10 --json number,title   # per candidate
ls docs/features/potential/2026-08-26-*.md
```

EXIT_CODE: 0

ExpectedExitCode: 0

## Output Summary

All nine follow-up candidates in
`docs/features/active/qfc-collection-controller-defects-468/spec.md` `## Follow-up Candidates` are
dispositioned below. Two already map to an open GitHub issue; the remaining seven received a new
potential entry under `docs/features/potential/` at task `[P14-T5]`.

**PROMOTION_DEFERRED.** Reason: the potential-to-issue promotion tooling
(`mcp__drm-copilot` promotion lifecycle functions) is **not present in this executor's tool surface**.
The only `drm-copilot` MCP functions available to this session are `run_poshqc_format`,
`run_poshqc_analyze`, `run_poshqc_test`, and `run_poshqc_analyze_autofix`. No potential-to-issue
function is callable from here, and the plan's task text for `[P14-T6]` directs the executor to record
the deferral and continue rather than halt.

Creating the seven issues by hand with `gh issue create` was **not** done. The promotion lifecycle
maintains a receipt and moves the source entry into `docs/features/potential/promoted/`; a hand-made
issue would produce an issue without a receipt and would leave the entry in the unpromoted folder,
which is a worse state than a clean deferral. Per
`.claude/skills/feature-promotion-lifecycle/SKILL.md`, promotion is a lifecycle operation, not an
issue-creation operation.

**Handoff.** The orchestrator owns AC-29. It must run the seven potential entries below through the
potential-to-issue lifecycle and record the resulting issue numbers in this feature folder before
AC-29 can be checked off. AC-29 is therefore left **unchecked** at the end of this plan.

## Disposition table — nine rows

| # | Candidate (abbreviated from `## Follow-up Candidates`) | Disposition | Verified |
|---|---|---|---|
| 1 | `QfcCollectionController.cs` exceeds the 500-line cap | **existing issue #623** — `Feature: quickfiler-500-line-cap-violations`, OPEN. Its body names `QuickFiler/Controllers/QfcCollectionController.cs` at 2349 lines explicitly and carries the acceptance criterion "`QuickFiler/Controllers/QfcCollectionController.cs` is at most 500 lines". | `gh issue view 623` — state OPEN |
| 2 | Remove the `stackMovedItems` parameter entirely | **new potential entry** `docs/features/potential/2026-08-26-qfc-remove-stackmoveditems-parameter.md` | file exists |
| 3 | Relocate the `ReadyForMove` presentation to the caller | **new potential entry** `docs/features/potential/2026-08-26-qfc-relocate-readyformove-presentation-to-caller.md` | file exists |
| 4 | Consolidate `IFilerFormController` and `IQfcFormController` | **new potential entry** `docs/features/potential/2026-08-26-consolidate-ifilerformcontroller-and-iqfcformcontroller.md` | file exists |
| 5 | Remove the orphan `QuickFiler.Interfaces.IQfcFormController` | **new potential entry** `docs/features/potential/2026-08-26-remove-orphan-quickfiler-interfaces-iqfcformcontroller.md` | file exists |
| 6 | Harden `KbdActions(IEnumerable<UClass>)` with the duplicate check both `Add` overloads perform | **existing issue #444** — `Bug: kbdactions-enumerable-ctor-bypasses-duplicate-guard`, OPEN. The title names the exact constructor. Also handed off in detail at `evidence/other/downstream-handoff-444.2026-08-26T16-26.md`. | `gh issue view 444` — state OPEN |
| 7 | File the unsynchronized undo handoff | **new potential entry** `docs/features/potential/2026-08-26-qfc-unsynchronized-undo-handoff-after-batch-move.md` | file exists |
| 8 | Revisit the unsynchronized plain read of `removespecificcontrolgroupcounter` | **new potential entry** `docs/features/potential/2026-08-26-qfc-unsynchronized-plain-read-reentrancy-counter.md` | file exists |
| 9 | Settle the #468 residual risk repository-wide (conditional) | **new potential entry** `docs/features/potential/2026-08-26-issue-468-residual-reflective-caller-risk.md` | file exists |

Exactly nine rows. Every path named in the Disposition column exists on disk; the listing that
verifies this is:

```
docs/features/potential/2026-08-26-consolidate-ifilerformcontroller-and-iqfcformcontroller.md
docs/features/potential/2026-08-26-issue-468-residual-reflective-caller-risk.md
docs/features/potential/2026-08-26-qfc-relocate-readyformove-presentation-to-caller.md
docs/features/potential/2026-08-26-qfc-remove-stackmoveditems-parameter.md
docs/features/potential/2026-08-26-qfc-unsynchronized-plain-read-reentrancy-counter.md
docs/features/potential/2026-08-26-qfc-unsynchronized-undo-handoff-after-batch-move.md
docs/features/potential/2026-08-26-remove-orphan-quickfiler-interfaces-iqfcformcontroller.md
```

Seven files, matching the seven "new potential entry" rows.

## Why candidates 2, 3, 4 and 9 do not map to an existing open issue

Each of these was surfaced by an issue this feature **closes**, so the apparent owner disappears at
merge:

| Candidate | Apparent owner in a search | Why it does not count |
|---|---|---|
| 2 | #469 (`stackMovedItems` search hit) | #469 is closed by this feature's merge; the parameter removal is explicitly deferred by decision D11 |
| 3 | #474 (`ReadyForMove` search hit) | #474 is closed by this feature's merge; the interface-level relocation is explicitly out of scope per `[P13-T1]` |
| 4 | #474 (`IFilerFormController` search hit) | same — #474 closes, and consolidation is a refactor the branch declined |
| 9 | #468 | #468 is closed by this feature's merge; AC-16's search is judged sufficient, and the entry records the condition under which a broader search would be warranted |

Candidate 1 (#623) and candidate 6 (#444) are different: both issues are independent of this feature's
closure set and remain open after the merge.

## Non-vacuity of the search for existing owners

`SearchScope:` open GitHub issues in `drmoisan/TaskMaster`, retrieved with
`gh issue list --state open --limit 200`.
`SearchPatterns:` `stackMovedItems`, `ReadyForMove`, `IFilerFormController`, `IQfcFormController`,
`KbdActions`, `undo`, `removespecificcontrolgroupcounter`, plus a full title scan for the 500-line cap.
`SearchResult:` the scope is non-empty — the listing returned more than 30 open issues, and individual
searches returned non-empty result sets for six of the eight patterns. The two mappings claimed above
(#623, #444) were each additionally confirmed by `gh issue view`, which reported state `OPEN` for
both.
