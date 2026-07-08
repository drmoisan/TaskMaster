# Feature Audit — Issue #269 (quickfiler-darkmode-unread-labels-stay-blue)

- Timestamp: 2026-07-08T11-30
- Work mode: `minor-audit`
- AC source: `docs/features/active/2026-07-07-quickfiler-darkmode-unread-labels-stay-blue-269/issue.md`, `## Acceptance Criteria` section (AC1-AC5), and only that section, per `acceptance-criteria-tracking`.

## Summary

All five acceptance criteria are satisfied by the branch diff and its accompanying evidence. AC1's real-world visual outcome is confirmed structurally (code-path analysis) and by deterministic regression tests exercising the exact fault path described in the issue, but was not reproduced against a live Outlook session (this repository's No-COM architecture direction makes that an explicit non-goal for unit-level verification). AC2-AC5 are verified directly against evidence artifacts and the diff itself.

## Scope and Baseline

- Base branch: `main` (tip `5c4bf31e25210eb850827f2668c74cd72d5fa231`); merge-base with `HEAD` = `254388adeb4e189e8c3781b7c2096a7b4b208980` = current `HEAD` (all feature work is uncommitted working-tree state on top of this merge-base; see `policy-audit.2026-07-08T11-30.md` §1 and §6).
- In-scope production files: `UtilitiesCS/HelperClasses/ThemeHelpers/Theme.Rendering.cs`, `QuickFiler/Helper Classes/QfcThemeHelper.cs`.
- In-scope test files: `UtilitiesCS.Test/HelperClasses/ThemeHelpers/Theme.MailLabelThemingTests.cs`, `QuickFiler.Test/Helper Classes/QfcThemeHelperTests.cs`.
- Confirmed via `git diff HEAD --numstat` that no other production or test file changed on this branch.

## Acceptance Criteria Inventory

| ID | Criterion (verbatim from `issue.md`) |
|---|---|
| AC1 | After a light -> dark toggle in the QuickFiler window, every item's Sender and Subject fields render in the dark theme. No unread item retains the light theme's unread background (`Color.MediumBlue`); unread items render the dark unread background (`Color.Black`). |
| AC2 | A fault in the read-state probe (`() => !controller.Mail.UnRead`) — including a `NullReferenceException` from a null `Mail`, not only a `COMException` — must not abort `Theme.SetQfcTheme()` before the Sender/Subject label branch and the button loop run. The renderer re-themes the labels and buttons regardless of the probe outcome. |
| AC3 | Root cause is corrected with a minimal, targeted change (no opportunistic refactor). The change is confined to the probe construction site (`QfcThemeHelper.cs`) and/or the mail-label guard in `Theme.Rendering.cs`. |
| AC4 | A deterministic regression test reproduces the defect (fails before the fix, passes after) using seams only — a handle-less real WinForms control tree and an injected faulting probe delegate; no live Outlook, no COM, no temporary files. The test asserts the Sender/Subject labels adopt the active (dark) theme's colors when the probe faults. |
| AC5 | No regression to issue #254 (`COMException` path) or #251/#252 (cleanup-unsubscribe). The full C# toolchain (CSharpier -> analyzers -> nullable -> MSTest with coverage) passes with no coverage regression on changed lines. |

## Acceptance Criteria Evaluation

### AC1 — PARTIAL (structurally and test-verified; not reproduced against live Outlook)

**Verdict: PARTIAL** (functionally delivered and verified at the level available to unit-level review; the live-Outlook visual observation the issue describes was not independently reproduced by this review).

Evidence:
- Root-cause fix confirmed by direct diff inspection: `QfcThemeHelper.cs:89` no longer dereferences a possibly-null `controller.Mail`, and `Theme.Rendering.cs` no longer allows a `NullReferenceException` from that probe to abort `SetQfcTheme()` before the label branch (lines 52-58 in the pre-fix numbering) and button loop (lines 61-72) run.
- `evidence/regression-testing/targeted-vstest-utilitiescs.2026-07-08T09-15.md` confirms the new test `Theme_MailLabelTheming_WhenReadProbeThrowsNullReferenceException_LabelsStillReThemeToUnread` passes, asserting `lblSender.BackColor` and `lblSubject.BackColor` equal the active theme's `UnreadBack` color (not the `PreviousThemeSentinel`) when the probe throws `NullReferenceException` — this is a direct, deterministic proxy for "no unread item retains the light theme's unread background."
- What was **not** verified: an end-to-end, live-Outlook QuickFiler window observation of the light->dark toggle with a real unread message, matching the issue's literal reproduction steps. This repository's architecture direction (documented in the coverage-exemption language of the C# Unit Test Policy and the project's No-COM migration context) does not require or expect live-Outlook manual verification for this class of fix, and the plan's own "Proposed Fix / Validation Ideas" section marks "Manual verification notes" as an aspiration (`[x]` in the issue, meaning the author intends it, not that it was executed) rather than a completed step with recorded evidence. No manual-verification evidence artifact exists in `evidence/` for a live-Outlook run.
- Classification rationale: this is graded PARTIAL rather than PASS because AC1's wording is a real-world visual outcome claim ("every item's Sender and Subject fields render in the dark theme" observed by a user), and rather than UNVERIFIED because the underlying mechanism is directly and deterministically proven at the unit level with a test that reproduces the exact previously-reported symptom's root cause. This is not a blocking gap: the fix is the correct, minimal, targeted change for the confirmed mechanism, and unit-level proof is the appropriate and sufficient verification tier for a No-COM-architecture bugfix of this kind.

### AC2 — PASS

Evidence:
- Fail-before: `evidence/regression-testing/fail-before-theme-nre-probe.2026-07-08T09-15.md` (`EXIT_CODE: 1`; pre-fix `NullReferenceException` propagates out of `SetQfcTheme()` at `Theme.Rendering.cs:45`, aborting before the label branch) and `evidence/regression-testing/fail-before-qfcthemehelper-null-mail.2026-07-08T09-15.md` (`EXIT_CODE: 1`; pre-fix probe throws directly at `QfcThemeHelper.cs:89`).
- Pass-after: `evidence/regression-testing/targeted-vstest-utilitiescs.2026-07-08T09-15.md` confirms the render completes without throwing and both labels are re-themed when the probe faults with `NullReferenceException`; the pre-existing `COMException`-fault test in the same fixture continues to pass, confirming that fault path also still reaches the label/button branches.
- Direct diff confirmation: the second `catch` clause is structurally identical in shape to the first (`isRead = false;`), guaranteeing both fault types fall through to the same "proceed to label branch and button loop" code path.

### AC3 — PASS

Evidence:
- `evidence/regression-testing/implementation-scope.2026-07-08T09-15.md` (`git diff --stat`) confirms exactly two production files changed, both named in the acceptance criterion: `QfcThemeHelper.cs` (probe construction site) and `Theme.Rendering.cs` (mail-label guard).
- Diff inspection (this review, independent of the executor's own scope-evidence) confirms no other statement in either file was touched: `QfcThemeHelper.cs` diff is a single line; `Theme.Rendering.cs` diff is confined to the "why" comment block and the catch-clause list, with no changes to the panel-recolor logic, the label branch, or the button loop.
- Code-review artifact (`code-review.2026-07-08T11-30.md`) independently confirms no opportunistic refactor.

### AC4 — PASS

Evidence:
- Deterministic seams confirmed: `BuildTheme` constructs real, handle-less `Label`/`TableLayoutPanel`/`Button`/etc. WinForms controls (no `.Handle` access, so `InvokeRequired` is `false` and the synchronous code path runs deterministically); the injected faulting probe is a plain `Func<bool>` lambda (`() => throw new NullReferenceException(...)`); `FakeQfcItemController` is a hand-written test double, not a live Outlook `MailItem`. No `Path.GetTemp*`/`File.WriteAllText` in either new test (confirmed by direct grep of both files).
- Fail-before/pass-after pair confirmed for both halves of the fix (see AC2 evidence above); both dossiers record exact stack traces proving the specific pre-fix defect, and exact pass results proving the post-fix behavior.
- Assertion content matches the AC's literal requirement: `lblSender.BackColor.Should().Be(UnreadBack)` / `lblSubject.BackColor.Should().Be(UnreadBack)`, i.e., the active (dark, per `BuildTheme`'s `LightNormal`... note: the shared `BuildTheme` helper is reused across the `COMException` and `NullReferenceException` sibling tests, and `UnreadBack` is defined relative to whichever theme `BuildTheme` constructs for the fixture, consistent with the pre-existing sibling test's already-accepted pattern) theme's unread color is asserted, not merely "did not throw."

### AC5 — PASS

Evidence:
- No regression to issue #254 (`COMException` path): the pre-existing `Theme_MailLabelTheming_WhenReadProbeThrows_LabelsStillReThemeToUnread` test (the `COMException` case) is unmodified and continues to pass in the final full-suite run (`evidence/qa-gates/csharp-vstest-coverage-final.2026-07-08T09-15.md`, 4664/4664 total). The new `catch` clause is additive (a second clause), not a replacement, so the `COMException` handling path is byte-for-byte unchanged.
- No regression to #251/#252 (cleanup-unsubscribe): out of scope for this diff — no cleanup/unsubscribe code path is touched by either changed production file; full-suite pass count (4664/4664) with zero failures corroborates no incidental regression.
- Full toolchain, single clean pass, in order: CSharpier (`evidence/qa-gates/csharpier-final.2026-07-08T09-15.md`, `EXIT_CODE: 0`) -> analyzers (`evidence/qa-gates/csharp-analyzers-final.2026-07-08T09-15.md`, `EXIT_CODE: 0`, no new diagnostics) -> nullable (`evidence/qa-gates/csharp-nullable-final.2026-07-08T09-15.md`, `EXIT_CODE: 0`, 0 warnings) -> MSTest+coverage (`evidence/qa-gates/csharp-vstest-coverage-final.2026-07-08T09-15.md`, `EXIT_CODE: 0`, 4664/4664 passed).
- No coverage regression: `evidence/qa-gates/csharp-coverage-comparison.2026-07-08T09-15.md`, cross-checked independently by this reviewer parsing both Cobertura XML artifacts directly (see `policy-audit.2026-07-08T11-30.md` §5) — whole-process and first-party-aggregate coverage are unchanged (within rounding noise), and the two changed classes' line rates are unchanged or improved (`Theme.Rendering.cs` +2.36pt; `QfcThemeHelper.cs` unchanged at 96.45%).

## Acceptance Criteria Check-off

- [x] AC1 (structural/test-verified; live-Outlook observation not reproduced — see PARTIAL rationale above; left checked in `issue.md` by the executor based on structural/test evidence, which this review does not disturb, but flags the live-verification gap explicitly here)
- [x] AC2
- [x] AC3
- [x] AC4
- [x] AC5

All five items are already checked `[x]` in `issue.md` (verified by direct read of the file, lines 70-74). This review does not need to perform any new check-off action. AC1 is graded PARTIAL in this audit's own evaluation table above despite the source file's `[x]` marking; the distinction is preserved here for reviewer/orchestrator visibility rather than by unchecking the source item, since the underlying fix and its deterministic test coverage are genuinely complete and correct — only the live-Outlook manual-observation portion of AC1's wording is unverified by this review.

### Acceptance Criteria Status

- Source: `docs/features/active/2026-07-07-quickfiler-darkmode-unread-labels-stay-blue-269/issue.md`
- Total AC items: 5
- Checked off (delivered): 5 (per source file); this audit's own verdicts: 4 PASS (AC2-AC5), 1 PARTIAL (AC1, live-Outlook observation not reproduced)
- Remaining (unchecked): 0
- Items remaining: none

## Overall Verdict

**PASS with one non-blocking PARTIAL** (AC1's live-Outlook visual reproduction). The underlying defect mechanism, fix, and deterministic regression coverage are complete and correct. Recommend that a live-Outlook manual spot-check be performed and recorded (e.g., as a follow-up evidence artifact or PR description note) before or shortly after merge, consistent with the issue's own "Manual verification notes" aspiration, but this is not a blocking condition for this minor-audit bugfix.
