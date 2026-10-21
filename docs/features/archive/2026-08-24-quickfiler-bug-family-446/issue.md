# quickfiler-queue-datamodel-defects (Issue #446)

- Issue: #446
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/446
- Also closes: #426, #448
- Advances (must remain open): #427 - only the 427-A producer side is delivered here, so no
  closing keyword may precede #427 in any commit message or pull-request body (see
  `evidence/issue-updates/p4-t17-pr-closing-keyword-constraint.2026-08-26T10-41.md`)
- Type: bug
- Work Mode: full-bug
- Epic: quickfiler-bug-family
- Integration Branch: epic/quickfiler-bug-family-integration
- Owner: drmoisan
- Last Updated: 2026-08-24
- Status: Prepared (preparation mode; execution deferred to epic-orchestrator)

> Acceptance criteria for this work mode live in `spec.md`, not in this file. See
> `.claude/skills/acceptance-criteria-tracking/SKILL.md`: for `full-bug` the AC source is `spec.md` only.

## Summary

This feature closes four pre-existing defects in the QuickFiler queue and datamodel. All four sit on
or beside the same dequeue result path, so they are corrected together rather than as four
independent patches.

| Issue | Defect | Severity |
| --- | --- | --- |
| #446 | `QfcHomeController.IterateQueueAsync` treats a deadline-expired empty dequeue as proof of source exhaustion and irreversibly closes the queue, silently dropping queued items for the rest of the session. | High — silent data loss |
| #448 | `QfcFormController.UndoConsumer()` has a loop that never terminates; past its 10-second threshold it busy-spins on a background thread for the life of the process. | High — hang and CPU burn |
| #426 | Mail items rejected by the high-confidence dequeue gate are removed from the master queue but never unhooked from `EmailMoveMonitor`, retaining a live `MailItem` COM reference and a `BeforeItemMove` subscription per rejected candidate. | Medium |
| #427 | Every accepted mail item is scored twice: the gate computes and discards the top folder, then `QfcItemController` re-runs the identical sequence after `Show()`. The `QfcPreScoredItem` carrier that exists to prevent this is dormant. | Low |

`#446` is the highest-severity item in the whole epic and drives the specification: the queue must
never be closed on any cause other than genuine exhaustion of the mail source.

## Authoritative Requirement Sources

The promoted potential documents are richer than the GitHub issue bodies and carry file:line, the
offending code block, root cause, suggested fix and severity:

- #426 `docs/features/potential/promoted/2026-08-07-emailmovemonitor-rejected-item-hook-retention.md`
- #427 `docs/features/potential/promoted/2026-08-07-quickfiler-post-show-duplicate-scoring.md`
- #446 `docs/features/potential/promoted/2026-08-07-iteratequeueasync-deadline-closes-queue-early.md`
- #448 `docs/features/potential/promoted/2026-08-07-quickfiler-undoconsumer-nonterminating-loop.md`

Line citations in those documents were captured at commit `fb32b923`; this branch is based on
`988e819b`. Citations are re-verified against the current tree in `research/`.

## Files This Feature Owns

No sibling epic child writes these:

- `QuickFiler/Controllers/QfcDatamodel.cs`
- `QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs`
- `QuickFiler/Controllers/QfcStreamingDequeueConfidenceGate.cs`
- `QuickFiler/Controllers/QfcFormController.Actions.cs`
- `QuickFiler/Controllers/QfcHomeController.Iteration.cs`
- `QuickFiler/Interfaces/IQfcDatamodel.cs`
- `QuickFiler/Controllers/QfcItemController.FolderHandling.cs`
- the `EmailMoveMonitor` source under `QuickFiler/Helper Classes/`

Ownership boundaries that must not be crossed:

- `QfcHomeController.Iteration.cs` is ours; the sibling partials `QfcHomeController.cs` and
  `QfcHomeController.Metrics.cs` belong to feature 442.
- `QfcItemController.FolderHandling.cs` is ours; every other `QfcItemController` partial belongs to
  features 484, 444 or 489.

## Constraints

- Per the Bugfix Workflow in `CLAUDE.md`, every defect gets a failing regression test first.
- `#446` and `#448` are ordering and lifetime invariants. `.claude/rules/general-unit-test.md`
  prohibits real wall-clock waits, `Thread.Sleep` and `Task.Delay` in tests and requires a
  controllable clock and fake timers for async tests, so both regression tests must be driven by
  injected time seams.
- Tests use MSTest with Moq and FluentAssertions. No live Outlook COM, no temporary files.
- `#426` must preserve the STA thread-affinity contract established by issues #214 and #420, and must
  not change the drop-on-reject contract pinned by `DequeueAsync_BelowThresholdItemsAreDiscarded`.
- Prefer adding test methods to the existing `QuickFiler.Test/Controllers/` and
  `QuickFiler.Test/Helper Classes/` files, which already carry `Compile Include` entries, so that
  `QuickFiler.Test/QuickFiler.Test.csproj` is not touched.
- C# toolchain in order: CSharpier, msbuild analyzers, msbuild nullable, vstest with coverage.

## Promotion Provenance

The four potential entries were created and promoted before this run and the four GitHub issues are
already open. `new_potential_bug_entry` and `potential_to_issue` were deliberately not called:
`potential_to_issue` has no idempotent path and always creates a new issue, which would have
duplicated all four. Only `new_active_feature_folder` was called for this feature. The checkpoint at
`artifacts/orchestration/orchestrator-state.json` records this truthfully under
`delegation_receipts.promotion`.
