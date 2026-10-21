# Feature Audit — Issue #677 (quickfiler-keyboard-hook-leaks-to-outlook)

- **Artifact:** `feature-audit.2026-08-28T12-31.md`
- **Work mode:** `full-bug` (persisted `- Work Mode: full-bug` marker in `issue.md`)
- **AC source:** `spec.md` only (v0.3), per the full-bug rule in `acceptance-criteria-tracking`

## Scope and Baseline

- **Base branch (resolved):** `main`
- **Merge base:** `361a49b884a4e3fe192bf04bae05151c598398fa`
- **Head:** `bug/quickfiler-keyboard-hook-leaks-to-outlook-677` @ `59bc263020b6c07678be53102f0c72fab3dd0fcf` (matches the PR-context summary head; artifacts current)
- **Diff scope:** 74 paths — 11 production C# files (+205/-3), 4 test files (+701), 2 csproj files, 55 docs/evidence paths, 11 agent-memory paths. Scope determined from `git diff --name-status` directly; the PR-context summary's "docs-only" classification was a classifier defect and was corrected in place.

## Acceptance Criteria Inventory

`spec.md` `## Acceptance Criteria` contains **10** checkbox items (referred to below as AC-1 … AC-10 in file order):

| # | Criterion (abbreviated) | Checkbox state in spec.md |
|---|---|---|
| AC-1 | Typing into native Outlook windows operates Outlook normally with QuickFiler open in any internal state — verified manually in a live session | `[ ]` (annotated: pending manual live-Outlook verification) |
| AC-2 | Click-back into QuickFiler restores its own keyboard navigation — same manual session | `[ ]` (annotated: pending manual live-Outlook verification) |
| AC-3 | Escape/commit still returns caret to the breadcrumb anchor (#438/#400 preserved) — existing tests green + manual check | `[ ]` (annotated: automated half satisfied; manual half pending) |
| AC-4 | Regression unit tests added and passing for (a) predicate-gated `FinishClose`, (b) predicate-gated late `_focusPending`, (c) deactivate parking + selector cancellation | `[x]` |
| AC-5 | Predicate evaluated at execution time, not scheduling time (asserted by test) | `[x]` |
| AC-6 | `KeyboardHandler` unchanged; no behavior changes outside the focus/activation scope | `[x]` |
| AC-7 | All existing `BreadcrumbDropDownHost`/breadcrumb pipeline tests pass without modification | `[x]` |
| AC-8 | Coverage: new/changed code >= 90% line; no reduction on changed lines | `[x]` |
| AC-9 | Full toolchain pass (format -> analyzers -> nullable -> vstest with coverage) | `[x]` |
| AC-10 | Feature-folder docs updated to match delivered behavior | `[x]` |

## Acceptance Criteria Evaluation

| # | Verdict | Reviewer evidence |
|---|---|---|
| AC-1 | UNVERIFIED — genuinely unautomatable; manual session pending | Requires Outlook's native message pump, real WebView2 runtime child windows, and real Win32 activation transitions (spec Test Strategy "Manual validation steps (required)"; research artifact Automation Feasibility). Cannot be exercised headlessly without violating the determinism/no-external-process test policies. Human-exception handling is complete: status record `evidence/other/manual-verification-pending.md`, runbook `runbooks/manual-live-outlook-verification.runbook.md`, owner assigned (maintainer) in spec Rollout item 1, and `issue.md` Next Step carries the open item. |
| AC-2 | UNVERIFIED — same basis and same handling as AC-1 | Same artifacts; AC-2 has a dedicated step mapping in the pending-verification record. |
| AC-3 | PARTIAL — automated half PASS; manual half UNVERIFIED (same basis as AC-1) | Automated half independently confirmed: whole-assembly TRX parses at 1218/1218 (= 1201 baseline + 17 new; no pre-existing test dropped); the pre-existing breadcrumb test files are byte-unmodified in the branch diff (reviewer-verified — the only pre-existing test-file edit is the sanctioned `FakeQfcItemController` no-op member); `FinishClose_PredicateTrue_FocusAnchorInvoked` and `UnsetPredicate_DefaultsTrue_FocusAnchorStillInvoked` assert the in-form focus return directly and passed in the reviewer's own re-run. |
| AC-4 | PASS | 17 tests exist exactly as enumerated in the spec (names cross-checked against the three files, read in full); committed TRXs show 29/29 and 9/9; reviewer independently re-ran the scoped filter (38 tests including the pre-existing Part1/Part2 host tests) with vstest exit 0 against a freshly rebuilt assembly. Coverage areas (a), (b), (c) each map to named tests. |
| AC-5 | PASS | `FinishClose_PredicateFlipsFalseAfterScheduling_DoesNotFocusAnchor` schedules the close while the predicate is true, asserts `context.PendingCount > 0` (proving queued, not inline), flips the predicate, drains, and asserts `FocusAnchorCount == 0`. Production code reads `MayTakeFocus()` inside `FocusPending()` and `FocusAnchorIfPermitted()` bodies — execution-time by construction (reviewed in diff). |
| AC-6 | PASS | Reviewer-executed `git diff 361a49b8..HEAD -- QuickFiler/Controllers/KeyboardHandler.cs` is empty. The full production change set is the 11 files listed in Scope, all on the focus/activation surface defined by spec Scope & Non-Goals. |
| AC-7 | PASS | `BreadcrumbDropDownHostTests.cs` and `.Part2.cs` absent from the branch diff (byte-unmodified); whole-assembly run 1218/1218 with zero failures; predicate default `() => true` pinned by `UnsetPredicate_DefaultsTrue_FocusAnchorStillInvoked`. |
| AC-8 | PASS | Independently re-parsed from the two committed Cobertura XMLs: new file `QfcFormController.Deactivate.cs` 24/24 (100%); changed-line coverage 100% on all five coverage-bearing files (reviewer mapped every residual uncovered line — `BreadcrumbDropDownHost.cs` 352/394, `FolderHandling.cs` 95–98/169–171 — outside all changed hunks, so zero uncovered and zero regressed changed lines); repo-wide line-rate rose 0.852721 -> 0.852804. Full detail: policy audit section 5, including the non-blocking disposition of the pre-existing `SetupDisposal.cs` whole-file debt (not an AC-8 obligation, which is scoped to new/changed code). |
| AC-9 | PASS | Reviewer independently re-ran three of the four gates this session (CSharpier check: 1558 files/0 violations; analyzer rebuild: exit 0/0 errors/5 pre-existing advisories; nullable rebuild: exit 0/0 CS86xx) and the scoped test run; the full-suite-with-coverage gate is evidenced by `evidence/qa-gates/coverage-final.md` (6838/6838, exit 0) and its committed Cobertura artifact. |
| AC-10 | PASS | All listed docs exist and are consistent: `spec.md` AC annotations, `issue.md` updates, `evidence/issue-updates/issue-677.md`, `evidence/other/manual-verification-pending.md`, `pr-notes.md`, fully checked-off `plan.2026-08-28T08-45.md`, plus the manual-verification runbook. |

### Adjudication of the three manual items (AC-1, AC-2, manual half of AC-3)

The unchecked-with-documented-reason treatment is **correct per repository norms** and is not an incomplete deliverable:

- `acceptance-criteria-tracking` rule 4 requires exactly this: leave unmet items unchecked and document the gap — done in the AC annotations, the pending-verification record, the runbook, and `issue.md`.
- The impossibility claim was verified, not accepted on assertion: the failure exists only in the composition of the live message pump, the real WebView2 runtime (upstream defect WebView2Feedback #951 is a runtime behavior, not reproducible with mocks), and real Win32 activation. A headless MSTest cannot observe any of the three; automating them would violate the no-external-process and determinism test policies.
- No remediation plan is triggered: the outstanding halves are not remediable by code work; they have an authored runbook, an assigned owner, and an open tracking item in `issue.md`. The same session also carries the secondary-contributor reconfirmation measurement (spec Rollout item 1).

## Summary

- **Blocking findings:** 0 (policy audit and code review both conclude 0 blocking).
- **AC results:** 7 PASS (AC-4 … AC-10), 1 PARTIAL (AC-3, automated half proven), 2 UNVERIFIED (AC-1, AC-2, manual live-Outlook only).
- **Recommendation: GO for PR** — merge-ready subject to the documented manual live-Outlook verification residual, which is post-merge-schedulable and owned by the maintainer per spec Rollout item 1. The PR body should state that AC-1/AC-2/AC-3-manual remain open so the PR does not imply full closure semantics beyond the delivered fix.
- **Residuals owed at/after merge (non-blocking):** (1) run the manual runbook session; (2) promote the 16-csproj analyzer HintPath/packages.config version skew to a GitHub issue; (3) promote the pre-existing `QfcFormController.SetupDisposal.cs` coverage debt; (4) if the secondary WinForms modal-menu-mode contributor is confirmed live, promote it via the MCP promotion lifecycle (spec Rollout item 2).

## Acceptance Criteria Check-off

- AC-4 … AC-10 were already checked `[x]` in `spec.md` by the executor; the reviewer re-verified each against independent evidence (table above) and confirms every existing check-off is justified. No reviewer check-off edits were required.
- AC-1, AC-2, AC-3 remain `[ ]` in `spec.md`, correctly, per rule 4 of `acceptance-criteria-tracking` (documented pending manual verification). The reviewer made no changes to the AC source file.

### Acceptance Criteria Status
- Source: docs/features/active/2026-08-28-quickfiler-keyboard-hook-leaks-to-outlook-677/spec.md
- Total AC items: 10
- Checked off (delivered): 7
- Remaining (unchecked): 3
- Items remaining:
  - AC-1: typing into native Outlook windows operates normally with QuickFiler open (manual live-Outlook session; runbook authored)
  - AC-2: click-back restores QuickFiler keyboard navigation (same manual session)
  - AC-3: Escape/commit caret return — manual half only (automated half already proven green)
