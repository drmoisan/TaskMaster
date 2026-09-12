# `quickfiler-helper-classes-coverage` — User Story

- Issue: #434
- Owner: drmoisan
- Status: Draft
- Last Updated: 2026-08-07T23-10

## Story Statement

- As the maintainer of QuickFiler, I want every testable file under `QuickFiler/Helper Classes/` to
  carry at least 80% per-file line coverage, so that a regression in a theme helper, a conversation
  loader, a viewer queue, or the move monitor fails a test on my machine instead of reaching a user's
  Outlook session.
- As an autonomous agent maintaining QuickFiler, I want the ordering, async-loading, and
  static-state invariants of these files expressed as deterministic assertions rather than as prose
  comments, so that I can change this code without silently breaking a contract that no test guards.
- As the epic owner for issue #136, I want each of the 14 F4 files to be either measurably above the
  80% floor or explicitly classified in F1's ledger with an accurate classification, so that the
  capstone F16 can account for all 121 compiled files without misreporting a declaration-only file as
  a 0% failure.

## Problem / Why

Issue #136 requires every production `.cs` file compiled by `QuickFiler/QuickFiler.csproj` to reach
at least 80% line coverage, or to sit on an explicitly ratified classification in the epic's ledger.
This feature is child F4: the thirteen files under `QuickFiler/Helper Classes/` plus
`QuickFiler/Interfaces/IEmailMoveMonitor.cs` — 14 files, roughly 2,860 lines.

Aggregate assembly coverage hides which individual files are short of the floor. Eight test files
already exist in `QuickFiler.Test/Helper Classes/` (2,425 lines, 58 `[TestMethod]` declarations), but
their reach is uneven and, in places, illusory. `QuickFiler.Test/Helper Classes/MailItemInfoTests.cs`
looks like coverage for the helper directory but targets `UtilitiesCS.MailItemHelper`
(`MailItemInfoTests.cs:120-123`) and has both of its test-method bodies commented out
(`:125-138`, `:140-168`), so it contributes nothing to any F4 file. Five files —
`EfcThemeHelper.cs` (499 lines), `EfcViewerQueue.cs`, `ItemViewerQueue.cs`, `QfcThemeControlSet.cs`,
and `cInfoMail.cs` — have no dedicated test file at all, and `EfcThemeHelper.cs` in particular has
100% of its executable lines currently unreached.

The specific risk this creates is that the helper directory is where QuickFiler's least visible
contracts live. `ConversationResolver`'s two loaders order the same property in opposite directions
(`ConversationResolver.Loading.cs:62` descending, `:109` ascending) and consumers read the result
positionally (`EfcItemController.cs:1103`, `QfcItemController.Conversation.cs:121`) — a "cleanup" that
aligned them would silently reorder a user's conversation list. `EmailMoveMonitor` leaks a
`BeforeItemMove` subscription when a mail's parent folder changes between hook and unhook
(`EmailMoveMonitor.cs:75` vs `:82`). The viewer queues mutate an unsynchronised `Queue<T>` across the
WPF dispatcher boundary. None of these is guarded by an assertion today.

What is **not** the problem: these files do not, on the whole, need architectural rework. Nine of the
ten testable files already have the seam they need — `QfcThemeControlSet` and the `internal
SetupThemes(QfcThemeControlSet)` overload from the issue #236 refactor, the
`ApplyState(IContainerControlLocal)` seam from a prior de-exemption cycle, `EmailMoveMonitor`'s
injectable `_marshalToSta` delegate, and the four constructor-injected delegates on
`ViewerQueueCore`. The remaining gap is test authoring, plus three small additive seams on
`ConversationResolver` to replace two static dependencies and make one `async void` handler awaitable.

## Personas & Scenarios

### Persona — Dan, maintainer of QuickFiler

- **Who:** the repository owner and sole maintainer of the QuickFiler VSTO add-in, working on a
  Windows workstation with Outlook installed, alongside a fleet of autonomous coding agents that make
  changes on his behalf.
- **What he cares about:** that a change an agent makes to a helper class does not reach his own
  Outlook session as a silent behaviour change. Coverage is the mechanism by which he delegates
  safely; it is a quality-control design choice, not a metric to satisfy.
- **Constraints:** the code is a legacy non-SDK .NET Framework 4.8.1 VSTO project; unit tests must
  never construct a live form, never show a popup, and never depend on the UI thread, so many of these
  helpers cannot be exercised through their production entry points. Thirteen sibling features are
  running against the same integration branch, so any change that touches a file he does not own
  becomes a merge conflict.
- **Goals and frustrations:** he wants the ordering and lifetime contracts in `ConversationResolver`,
  `EmailMoveMonitor`, and the viewer queues to be executable rather than documented in comments. His
  frustration is that aggregate coverage numbers look acceptable while individual files sit at zero,
  and that a "harmless cleanup" in a helper can change what a user sees in their inbox.
- **Context and motivation:** this is child F4 of a 16-child epic. He is not asking for new features
  here; he is buying the ability to let agents work on QuickFiler unsupervised.

### Scenario — an agent "fixes" the conversation ordering divergence

- **Who is acting:** an autonomous coding agent assigned an unrelated refactor in
  `QuickFiler/Helper Classes/`.
- **What triggered the action:** the agent notices that `LoadConversationInfo` orders the expanded
  conversation list with `OrderByDescending(x => x.ConversationID)`
  (`ConversationResolver.Loading.cs:62`) while `LoadConversationInfoAsync` orders the same property
  with `OrderBy` (`:109`). It reads as an obvious inconsistency.
- **Steps taken:** the agent changes `:109` to `OrderByDescending` for consistency, runs the C#
  toolchain, and sees it pass.
- **Obstacle that should occur but does not today:** nothing fails. No test asserts either ordering.
  The change ships. In the user's Outlook session, the conversation list in the item viewer now
  renders in reverse, because `EfcItemController.cs:1103` and
  `QfcItemController.Conversation.cs:121` read `ConversationInfo.Expanded` positionally.
- **What this feature changes:** after F4, two tests pin the divergence explicitly — one asserting the
  synchronous loader returns `c3, c2, c1` for inputs `c1, c3, c2`, and one asserting the async loader
  returns `c1, c2, c3` — with an XML doc on each recording that the divergence is intentional
  contract, not a defect. The agent's change now fails a named test with an actionable message, and
  the test's documentation tells it why the two loaders differ.
- **Expected outcome:** the regression is caught at the toolchain step, on the agent's own machine,
  before review — which is the whole point of the 80% per-file floor.

### Scenario — a test leaves the viewer-queue statics poisoned

- **Who is acting:** an agent adding a test in a different feature that happens to touch
  `ItemViewerQueue`.
- **What triggered the action:** the agent needs a queued viewer and calls `SetCoreForTesting(core)`
  without restoring state afterwards.
- **Obstacle:** `ItemViewerQueue` holds five pieces of mutable process-global state
  (`ItemViewerQueue.cs:11-29`). Today `ViewerQueueStaticWrapperTests.cs` protects itself with
  `[TestCleanup]` only, which is a post-condition guarantee — it does not guarantee that the class
  *starts* from a known state. A later test in the same assembly can therefore drive the previous
  test's core and its recorder lists, or, worse, hit the production default and construct a live
  `ItemViewer` (`ItemViewerQueue.cs:105`) or dereference a null `UiThread.Dispatcher`.
- **What this feature changes:** each wrapper gains one additive `internal static ResetForTesting()`
  that restores the production delegates and then rebuilds the core in the correct order, and every
  test class touching either type carries `[DoNotParallelize]` plus both `[TestInitialize]` and
  `[TestCleanup]` calling it. Order-independence becomes a pre-condition guarantee, as
  `.claude/rules/general-unit-test.md` § Core Principles requires.
- **Expected outcome:** the tests in this area run correctly in any order, and the failure mode where
  a unit test constructs a live WinForms control is closed off by construction.

## Acceptance Criteria

- [ ] Each of the 10 testable files (`EfcThemeHelper.cs`, `QfcThemeHelper.cs`, `QfcThemeControlSet.cs`, `TlpCellSnapShot.cs`, `ConversationResolver.cs`, `ConversationResolver.Loading.cs`, `ViewerQueueCore.cs`, `ItemViewerQueue.cs`, `EfcViewerQueue.cs`, `EmailMoveMonitor.cs`) reaches >= 80% line coverage, verified with F1's per-file harness, with the numeric per-file result committed under `<FEATURE>/evidence/qa-gates/`.
- [ ] Each of the 4 zero-coverable-line files (`IConversationResolver.cs`, `IEmailMoveMonitor.cs`, `QfEnums.cs`, `cInfoMail.cs`) is recorded in F1's `docs/features/epics/quickfiler-per-file-coverage/coverage-ledger.md` with a zero-coverable-line classification distinct from `ratified-exempt`, and none receives an `[ExcludeFromCodeCoverage]` attribute.
- [ ] No production file in scope exceeds 500 lines, and `EfcThemeHelper.cs` remains at 499 lines with no partial split.
- [ ] All new and modified tests use MSTest, Moq, and FluentAssertions, are deterministic, isolated, and order-independent, and use no temporary files, external services, live forms, or popups.
- [ ] Per-file coverage spans the positive, invalid-input, boundary, and error-handling categories for each of the 10 testable files.
- [ ] The full C# toolchain passes in final form in a single pass: csharpier, the analyzer build, the nullable build, and coverage-enabled vstest.
- [ ] No observable behaviour change to QuickFiler flows: every seam is additive and every existing call site compiles unchanged.
- [ ] No edit is made to `coverage.config`, to `QuickFiler/QuickFiler.csproj`, or to any sibling-owned file, and the only shared-file change is `<Compile Include>` additions inside the `Helper Classes\` block of `QuickFiler.Test/QuickFiler.Test.csproj` at lines 158-165.

## Non-Goals

- Deleting `cInfoMail.cs`. Deletion is behaviour-neutral but buys zero coverage — an empty denominator
  is empty either way — and would require editing the shared `QuickFiler/QuickFiler.csproj` (line 342)
  that all thirteen siblings are editing concurrently. Deferred to a post-fan-in hygiene issue.
- Deleting or widening `IConversationResolver.cs`, or removing the two `[Obsolete(..., true)]` members
  at `ConversationResolver.cs:301` and `:333`.
- Fixing the latent defects the research identified: `EmailMoveMonitor`'s leaked `BeforeItemMove`
  subscription and its live-COM predicate read at `:213`; the unsynchronised `Queue<T>` across the
  dispatcher boundary; `Reset`'s double-dispose; `DequeueChunk`'s replenish-by-`originalCount`
  unbounded growth; the missing `[Flags]` on `QfEnums.InitTypeEnum`; and
  `MailItemInfoTests.cs:25`'s banned `DateTime.Now`. Each is promoted as its own issue.
- Any edit to `coverage.config`, `QuickFiler/QuickFiler.csproj`, or any sibling-owned production or
  test file, including `QuickFiler.Test/Helper Classes/EmailMoveMonitorTests.cs`, which is left
  untouched so issue #426 can extend it later without a rebase conflict.
- Introducing STA test infrastructure, a QuickFiler-specific `.runsettings`, a new clock abstraction,
  or any new package dependency. No STA test is required anywhere in F4; the epic's STA last-resort
  clause is available but is not invoked.
- Normalising the three test namespaces in `QuickFiler.Test/Helper Classes/`, or splitting
  `TlpCellSnapShot.cs` into one file per declared type.
