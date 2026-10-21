# Feature Audit — Issue #680: QuickFiler search box loses focus on drop-down expand

- Date: 2026-08-28T16-27
- Reviewer: feature-review agent

## Scope and Baseline

- Base branch: `main`, resolved per `pr-base-branch-merge-base`; merge-base `b0c7fa18a3beb073e7b051f49e28f48159f0f179` (the branch was rebased onto this exact commit, so the merge-base equals origin/main's current tip).
- Head: `bug/quickfiler-search-box-loses-focus-on-dropdown-expand-680` @ `79a8500a2ffffc6449ffc0bbabe9acc66558f91f` (5 commits).
- Work mode: `full-bug` (persisted `- Work Mode: full-bug` marker in `issue.md`) → AC source is `spec.md` only, per `acceptance-criteria-tracking`.
- Audit scope: full branch diff vs merge-base — 12 C#/build files plus 44 docs/evidence/agent-memory files. Verified directly via `git diff --numstat`; the PR-context summary's docs-only classification was a generator defect and was corrected in place.
- Rebase context: execution completed against the pre-#677 base; the branch was then rebased onto main (which had merged #677/PR #684). One manual conflict resolution in `BreadcrumbDropDownHost.Open.cs` composed both fixes; verified correct in this review (see code-review Composition Verification) and re-tested at head (55/55 scoped rerun by this reviewer).

## Acceptance Criteria Inventory

Source: `spec.md` `## Acceptance Criteria` (sole authoritative source; 9 items, AC-1 through AC-9).

| AC | Summary | Spec checkbox state at review |
|---|---|---|
| AC-1 | Continuous typing with non-capturing auto-open, verified manually per HV runbook | `[ ]` unchecked (pending HV) |
| AC-2 | Gesture paths unchanged per #400/#438, verified in the same HV session | `[ ]` unchecked (pending HV) |
| AC-3 | Fail-before host-seam regression test added and passing | `[x]` |
| AC-4 | Coordinator/controller/contract-seam tests incl. dismissal edge cases | `[x]` |
| AC-5 | #438/#400 suites pass unmodified | `[x]` |
| AC-6 | No unintended behavior changes outside the scoped lifecycle | `[x]` |
| AC-7 | Coverage: changed members >= 90%, no changed-line regression, repo-wide recorded | `[x]` |
| AC-8 | Full toolchain pass in order, all steps passing in the final pass | `[x]` |
| AC-9 | Docs updated incl. #677 follow-up discharge | `[x]` |

## Acceptance Criteria Evaluation

| AC | Verdict | Evidence |
|---|---|---|
| AC-1 | UNVERIFIED (pending manual HV — expected state, non-blocking) | Genuinely unautomatable: menu-mode engagement and live keyboard retargeting need a real message pump, popup window, and WebView2. The human-exception route is properly executed: checkpoint `human_interaction` records an `exception` response with the runbook path; `runbooks/quickfiler-search-focus-hv-680.runbook.md` items HV-1/HV-2 cover this AC with concrete pass conditions and a defined negative-outcome fallback (promote the borderless-Form rewrite; never amend in place). Spec documents the unchecked state and the check-off precondition. This is the correct treatment under `acceptance-criteria-tracking` (leave unmet items unchecked with the gap documented), not an incomplete deliverable. |
| AC-2 | UNVERIFIED (pending manual HV — expected state, non-blocking) | Same route; runbook items HV-3 through HV-9, including both DR-8 composition risks (HV-7 post-handoff outside-click, HV-9 row-click on a non-capturing popup). The automated half is pinned and green: `TextBoxSearchKeyDown_DownArrow_StillOpensAndFocusesTheDropDown` and the gesture-open host tests pass at head in the reviewer's rerun. |
| AC-3 | PASS | Red run `p2-t3.trx`: exactly the 2 predicted failures (`ShowPopup_NonFocusingOpen_...AutoCloseFalse`, `ShowPopup_TwoConsecutiveNonFocusingOpens_...`), 25/27 pass, reviewer-parsed. Green run `p3-t6.trx`: 27/27, same filter/population. All four required `AutoClose` state assertions exist by name in `BreadcrumbDropDownHostTests.Part2.cs` (false at show for non-focusing; true for 3-parameter gesture; restored after close; restored after `takeFocus: true` reopen). Test names and file path listed in the delivery report as required. Reviewer re-ran at head: green. |
| AC-4 | PASS | Red run `p2-t10.trx`: exactly the 3 predicted Moq failures, 9/12 pass. Green run `p3-t9.trx`: 47/47. Dismissal-ownership edge cases each have a named test (exactly-one intent per Escape/leave, no spurious intent when closed, latch consumed exactly once); the "no un-dismissable state" condition is additionally backstopped at the host by the `FinishClose` restore (code-review Info finding). Wiring (P2-T7) and additive-contract (P2-T8) tests present and green. |
| AC-5 | PASS | Reviewer independently re-ran the pinned-file diff against the merge-base: all nine pinned files byte-identical at head (empty diff). `p4-t2.trx`: 75/75. The pinned controller suite also passed in the reviewer's post-rebase scoped rerun. |
| AC-6 | PASS | The complete code footprint is exactly the twelve files enumerated on the AC-6 line (verified against `git diff --numstat`); all are inside the Scope & Non-Goals boundary. No gesture-path file, no #438 focus-pipeline file, and no unrelated QuickFiler subsystem is touched. Remaining diff entries are feature-folder docs/evidence and version-controlled agent-memory, which carry no behavior. |
| AC-7 | PASS | Reviewer re-parsed both raw Cobertura files and reproduced the executor's figures exactly: all six changed members at 100% (>= 90% floor); five of five measured changed files with final covered-count >= baseline (zero changed-line regression); repo-wide line-rate 0.85269 → 0.85279 recorded and assessed per § UT2 (above the floor; no pre-existing-shortfall clause needed). Note: the per-file 82.41% figure on `QfcItemController.EventHandlers.cs` versus the rules-file 85% floor is a policy-audit matter (dispositioned non-blocking there); AC-7's own conditions are met in full. |
| AC-8 | PASS | Final restart-free pass artifacts: p6-t1 (PRE_FORMAT_CHECK_EXIT 0), p6-t2 (exit 0), p6-t3 (exit 0), p6-t4 (exit 0, 6839/6839). The first pass's formatter rewrite and loop restart are honestly recorded. Post-rebase: checkpoint records csharpier/analyzer/nullable exit 0 and 1236/1236; reviewer independently re-verified format (csharpier check exit 0 at head) and the scoped suites (55/55). |
| AC-9 | PASS | Spec Rollout & Follow-up carries the dated discharge literal `Discharged by #680 on 2026-08-28 — see delivery-report` (verified present), the no-#677-folder cross-issue record note, and the re-confirmed "no config updates" statement; delivery report and rollout notes carry the discharge record. Minor: two delivery-report statements are stale post-rebase (code-review CR-1, non-gating for this AC — the discharge record itself is accurate). |

## Summary

- 7 of 9 acceptance criteria PASS with reviewer-independent verification (re-parsed TRX and Cobertura evidence, re-run pinned-file diff, fresh build + 55/55 scoped test rerun at head, format check at head).
- AC-1 and AC-2 are UNVERIFIED pending the live-Outlook HV runbook — the documented, expected state for genuinely unautomatable criteria under this repository's human-exception route; both remain correctly unchecked in `spec.md` with a complete 9-item runbook and recorded exception response. Not treated as delivery gaps.
- One Blocking policy finding gates PR readiness: `BreadcrumbDropDownHost.cs` at 514 lines (> 500), introduced by this branch's additions composing with #677 after the rebase. Carried in `policy-audit.2026-08-28T16-27.md` § 8 and `remediation-inputs.2026-08-28T16-27.md`.
- Recommendation: **NO-GO for PR until the file-size remediation lands** (small partial-class relocation plus gate re-run); GO in all other respects. The HV runbook execution remains an owner action at or promptly after merge.

### Acceptance Criteria Status
- Source: docs/features/active/2026-08-28-quickfiler-search-box-loses-focus-on-dropdown-expand-680/spec.md
- Total AC items: 9
- Checked off (delivered): 7 (AC-3, AC-4, AC-5, AC-6, AC-7, AC-8, AC-9 — all checked by the executor; reviewer confirms each PASS)
- Remaining (unchecked): 2
- Items remaining: AC-1 (continuous-typing HV), AC-2 (gesture-path HV) — both intentionally unchecked pending the live-Outlook runbook, per the documented human-exception route.

## Acceptance Criteria Check-off

- No new check-offs performed by this review: every PASS criterion (AC-3 through AC-9) was already checked `[x]` in `spec.md` by the executor with evidence citations, and this review confirms each. AC-1 and AC-2 are left unchecked per the check-off protocol (evaluated UNVERIFIED; check-off is permitted only after the HV runbook outcome is recorded under `evidence/other/`).
