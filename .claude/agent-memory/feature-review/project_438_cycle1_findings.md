---
name: 438-cycle1-findings
description: 'Issue #438 review cycle 1 (2026-08-08T13-25): all 14 ACs PASS; one blocking finding R1 (new file BreadcrumbItemViewerLifecycleCoordinator.Search.cs branch 50% — null-conditional null-arms untested); R2 = pre-existing EventHandlers.cs floor miss dispositioned non-blocking.'
metadata:
  type: project
---

Issue #438 (quickfiler-search-keystroke-focus-steal) cycle-1 review at head `ff9d14ab` vs merge-base `003c5715`: implementation verified sound, all 14 gating ACs PASS, 6348/6348 tests, repo coverage 85.87%/79.25% (both improved vs baseline).

Blocking R1: new file `QuickFiler/Viewers/BreadcrumbItemViewerLifecycleCoordinator.Search.cs` measures 100% line but 50% branch (2/4) — the null-arms of `_openCoordinator?.` (line 40) and `_bridgeCoordinator?.` (line 42) are unexercised, i.e., the file's own documented no-open-coordinator fallback has no test. Remediation is test-only (construct the lifecycle coordinator without open/bridge coordinator, call `PresentSearchResults`).

Non-blocking R2: `QfcItemController.EventHandlers.cs` 78.65%/61.11% vs floor 85/75 — pre-existing (baseline 79.57%/65.00%), every changed line covered, uncovered-line set identical (19 both sides); ratio drop is pure denominator shrinkage from deleting 4 covered defective lines. Dispositioned non-blocking; maintainer disposition requested (follow-up coverage issue or recorded exemption).

**Why:** the re-audit cycle should verify R1 closed (branch 4/4 or >= 75%) without re-litigating the already-verified attribution work (BeginOpenCore line 221 = baseline line 187 pre-existing; CS2002 duplicate at merge-base lines 302/354; D10 filters all non-vacuous).

**How to apply:** on the #438 cycle-2 reaudit, check only: R1 file branch >= 75%, no existing test weakened, repo figures not lower than 0.858665/0.792502, `BreadcrumbDropDownHostTests.cs` still <= 500 lines (it is at 499 — new tests must go in Part2), and re-apply the pr_context summary correction if the artifacts were regenerated (see [[pr-context-summary-misclassifies-cs]]).
