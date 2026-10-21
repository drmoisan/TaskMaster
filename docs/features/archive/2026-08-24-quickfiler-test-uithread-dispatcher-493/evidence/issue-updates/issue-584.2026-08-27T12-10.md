# Issue #584 Comment Mirror (P5-T11)

Timestamp: 2026-08-27T12-10
Task: [P5-T11]
Command: `gh issue comment 584 --body-file <path>`
EXIT_CODE: 0
Output Summary: The comment was posted successfully. `PostedAs: comment`. The comment URL is
recorded below and the exact posted text is mirrored verbatim.

PostedAs: comment
CommentUrl: https://github.com/drmoisan/TaskMaster/issues/584#issuecomment-5440802535
IssueUrl: https://github.com/drmoisan/TaskMaster/issues/584
IssueState: OPEN
IssueTitle: `Bug: uithread-dispatcher-null-race-progresstrackerasync`

The issue's number, state, title, and URL were verified with
`gh issue view 584 --json number,title,state,url` immediately before posting.

## Invocation form

The `gh issue comment` form was used, not `gh api ... -X POST` against the issues endpoint: the
`PreToolUse` hook `.claude/hooks/enforce-promotion-mcp-only.ps1` denies the latter.

## Exact posted text

## Injectable-seam conversion scope, measured while fixing #493

Recording this here rather than opening a third issue against the same static, per the overlap
assessment in `docs/features/active/quickfiler-test-uithread-dispatcher-493/spec.md`
§ Rollout & Follow-up item 2.

### Why this is being posted on #584

#584 is about the *null* state of `UtilitiesCS.UiThread._dispatcher` — an NRE at consumers when
nothing has initialised it. #493 is about *unrestored and unsynchronized mutation* of the same
static by tests. The two are adjacent and materially overlapping but not identical: they share a
root object and they would share a remedy, because replacing the static with the existing
`IUiDispatcher` seam would dissolve both. #584's recorded structural analysis already names the
static itself as the defect, which is the same target the seam conversion would address.

### The seam already exists and is partially adopted

- `UtilitiesCS/Threading/IUiDispatcher.cs` and `UtilitiesCS/Threading/WpfUiDispatcher.cs`, whose
  default constructor is literally `: this(() => UiThread.Dispatcher)` at `WpfUiDispatcher.cs:25`.
- `QfcItemController._uiDispatcher`, which the QuickFiler pump test fixture already injects.

### Measured remaining scope

Approximately **62 references across 29 first-party production files** still read the static
directly. Measured by repo-wide grep excluding `*Test*`, docs, and `.claude`. Concentrations:

| File | References |
| --- | --- |
| `QuickFiler/Controllers/QfcCollectionController.cs` | 8 |
| `QuickFiler/Controllers/QfcQueue.cs` | 4 |
| `QuickFiler/Helper Classes/ItemViewerQueue.cs` | 4 |
| `TaskMaster/Ribbon/RibbonViewer.EngineCommands.cs` | 4 |
| ~25 other files | remainder |

Converting those is a multi-phase production refactor across three assemblies with a live VSTO
surface, no behavioural defect of its own, and no bounded blast radius. It was explicitly deferred
out of #493, which is a test-isolation fix confined to the `QuickFiler.Test` assembly.

### What #493 delivered instead

#493 funnels every mutation of the static made from `QuickFiler.Test`'s owned files through one new
test fixture with two locks — one making a single read-modify-write atomic, one serializing long
install-to-restore transactions — and changes
`QfcItemControllerTestSupport.EnsureUiThreadDispatcher` from `void` to `IDisposable` so its seeding
can be reverted. No production assembly changed and `UtilitiesCS/Threading/UiThread.cs` is
untouched. That closes the test-side isolation defect without touching the 62 production call sites.

Two residual mutators remain outside #493's owned set and are unaffected by it:
`QuickFiler.Test/Controllers/WpfUiDispatcherTests.cs`, and the `UtilitiesCS.Test` sites
(`ProgressTracker_Tests.cs`, `ProgressTrackerAsync_Tests.cs`, `IdleAsyncQueue_Tests.cs`) — the last
group being where #584's own reported flake originates.

### Suggested disposition, for the maintainer to decide

Either widen #584's title and body to cover "replace the `UiThread.Dispatcher` static with the
existing `IUiDispatcher` seam", citing #493 as a second motivating defect, or keep #584 scoped to
the null race specifically and promote the seam conversion as a separate issue cross-linked to both.
No new issue has been opened for the seam conversion pending that decision.

Cross-reference: #493.

## End of posted text

The body was 60 lines. No `spec.md` or `plan.md` mirror update is required, because `PostedAs` is
`comment` rather than `body`.

## What this discharges

Spec § Rollout & Follow-up item 2, which asked that the seam-conversion scope be recorded as a
comment on the existing #584 rather than promoted as a duplicate third issue tracking the same
static. The disposition decision (widen #584 versus promote a separate cross-linked issue) is left
to the maintainer and is stated as such in the comment; no new issue was opened for the seam
conversion.
