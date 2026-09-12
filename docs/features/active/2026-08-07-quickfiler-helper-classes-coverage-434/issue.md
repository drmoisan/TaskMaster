# quickfiler-helper-classes-coverage (Issue #434)

- Date captured: 2026-08-07
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/quickfiler-helper-classes-coverage/ (Issue #434)
- Parent epic issue: [#136](https://github.com/drmoisan/TaskMaster/issues/136)
- Epic: `quickfiler-per-file-coverage` (child F4, wave 1, complexity band C3)
- Depends on: F1 `quickfiler-coverage-denominator-and-exemption-ledger`

- Issue: #434
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/434
- Last Updated: 2026-08-08
- Work Mode: full-feature

## Problem / Why

Issue #136 requires every production `.cs` file compiled by `QuickFiler/QuickFiler.csproj` to reach
at least 80% line coverage, or to sit on an explicitly ratified exemption ledger. This entry covers
child F4 of the `quickfiler-per-file-coverage` epic: the whole `QuickFiler/Helper Classes/` directory
plus the one interface that belongs to it, 14 files and roughly 2,860 lines.

The directory is a mixed surface. `QfcThemeHelper.cs`, `ConversationResolver.cs`,
`ConversationResolver.Loading.cs`, `EmailMoveMonitor.cs`, `TlpCellSnapShot.cs`, `ViewerQueueCore.cs`,
and `cInfoMail.cs` already have some test presence in `QuickFiler.Test/Helper Classes/`, but the
per-file line coverage each currently attains is unmeasured — aggregate assembly coverage hides
which files are actually short of 80%. Five files have no dedicated test file at all:
`EfcThemeHelper.cs` (499 lines), `EfcViewerQueue.cs`, `ItemViewerQueue.cs`, `QfcThemeControlSet.cs`,
and `cInfoMail.cs`.

Two structural obstacles stand between the current state and the target:

1. The theme helpers (`EfcThemeHelper.cs`, `QfcThemeHelper.cs`, `QfcThemeControlSet.cs`,
   `TlpCellSnapShot.cs`) manipulate WinForms controls directly. Their colour/layout decision logic
   is pure but is currently entangled with control mutation.
2. The viewer queues (`ViewerQueueCore.cs`, `ItemViewerQueue.cs`, `EfcViewerQueue.cs`),
   `ConversationResolver` (both partials), `EmailMoveMonitor.cs`, and `cInfoMail.cs` touch Outlook
   Interop types (`MailItem`, `MAPIFolder`, `Store`) and carry ordering and async-loading
   invariants. Without injectable seams and an injected clock these cannot be exercised
   deterministically.

`EfcThemeHelper.cs` sits at 499 lines against the repository's 500-line hard limit, so any growth in
that file forces a partial split.

## Proposed Behavior

Raise every `testable` file in the F4 file set to at least 80% line coverage without changing any
observable QuickFiler behavior. Where a file cannot be reached by a deterministic unit test in its
current shape, introduce a seam — interface seam first, injectable delegate second, adapter third —
and cover the extracted logic. Where the epic's F1 ledger classifies a file as `ratified-exempt`,
accept that classification as authoritative and record it rather than forcing coverage.

Work proceeds strictly one production file at a time, per the #136 mandate: one research artifact
per production file, one atomic-plan phase per production file, and each individual test case as its
own atomic task.

## Acceptance Criteria (early draft)

- [ ] Every `testable` file in the F4 set reaches >= 80% line coverage, verified with F1's per-file
      coverage harness and recorded as numeric evidence under `<FEATURE>/evidence/qa-gates/`.
- [ ] Any file not brought to 80% is `ratified-exempt` in F1's
      `docs/features/epics/quickfiler-per-file-coverage/coverage-ledger.md`, cited by name.
- [ ] No production file in scope exceeds 500 lines; `EfcThemeHelper.cs` (499) is split if it grows.
- [ ] New and modified tests use MSTest, Moq, and FluentAssertions, are deterministic and isolated,
      and use no temporary files, external services, live forms, or popups.
- [ ] Coverage per file spans the positive path plus invalid-input, boundary, and error-handling
      behavior.
- [ ] The full C# toolchain passes in final form: csharpier, analyzer build, nullable build, and
      coverage-enabled vstest.
- [ ] No behavior change to observable QuickFiler flows.

## Constraints & Risks

- **Concurrency isolation.** Thirteen sibling children of epic #136 run against the same integration
  branch. This child must touch only the 14 files listed plus `QuickFiler.Test/Helper Classes/**` and
  its own feature folder. It must not modify `coverage.config` or any shared build property file.
- **Upstream dependency on F1.** F1 delivers both the per-file coverage harness and the ratified
  exemption ledger. This child consumes both; it does not define its own coverage measurement
  mechanism and does not unilaterally decide exemptions.
- **COM and WinForms boundaries.** Tests must never construct live forms, never show popups, and
  never depend on the UI thread. The STA last-resort clause (epic `Shared Design` section 3) permits
  never-shown in-memory controls in dedicated `*.StaTests.cs` files only after a seam has been shown
  infeasible, with the rationale documented per test.
- **Determinism.** `ConversationResolver` and the viewer queues carry ordering and async-loading
  invariants. Tests must use an injected clock and fake timers; `Thread.Sleep`, `Task.Delay`, and
  real wall-clock waits are prohibited.
- **Duplication risk.** Eight test files already exist in `QuickFiler.Test/Helper Classes/`. Research
  must establish actual current per-file coverage and target genuine gaps rather than re-testing
  what is already covered.
- **File-size risk.** `EfcThemeHelper.cs` at 499 lines cannot absorb any seam scaffolding without a
  partial split.

## Test Conditions to Consider

- [ ] Per-file line coverage measurement for each of the 14 files, before and after.
- [ ] Theme-helper colour and layout decision logic exercised without control mutation.
- [ ] Viewer-queue enqueue/dequeue ordering, capacity boundaries, and empty-queue behavior.
- [ ] `ConversationResolver` async loading ordering under an injected clock and fake timers.
- [ ] `EmailMoveMonitor` move-event handling including null and error paths.
- [ ] `cInfoMail` construction from a mocked `MailItem` including missing-property paths.
- [ ] `TlpCellSnapShot` capture/restore round-trip and boundary cell indices.
- [ ] Invalid-input, boundary, and error-handling behavior for every covered file.

## Next Step

- [ ] Promote to GitHub issue (feature request template)
- [ ] Create `docs/features/active/quickfiler-helper-classes-coverage/` folder from the template
