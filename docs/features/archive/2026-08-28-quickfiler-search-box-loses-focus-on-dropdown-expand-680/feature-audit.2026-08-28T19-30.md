# Feature Audit — quickfiler-search-box-loses-focus-on-dropdown-expand (Issue #680)

- Review cycle: R4 pass 2
- Timestamp: 2026-08-28T19-30
- Branch: `bug/quickfiler-search-box-loses-focus-on-dropdown-expand-680` @ `4cf822e8` vs `main` @ `b0c7fa18`
- Work mode: `full-bug` — `spec.md` is the sole authoritative AC source (verified from `issue.md`)

## AC Evaluation

| AC | Verdict | Evidence (verified this session unless noted) |
|---|---|---|
| AC-1 | PENDING-HV | Correctly unchecked. Live-Outlook manual verification per the HV runbook (`evidence/other/hv-runbook-680.2026-08-28T16-12.md`, `runbooks/quickfiler-search-focus-hv-680.runbook.md`); not dischargeable by automated tests. Unaffected by both remediation cycles. |
| AC-2 | PENDING-HV | Correctly unchecked. Same runbook (HV-3 through HV-9). The automated half (Down-arrow handoff, gesture-open host tests) is green in this reviewer's fresh scoped rerun. |
| AC-3 | PASS | Fail-before red-run TRX restored at `evidence/regression-testing/p2-t3/p2-t3.trx` and re-verified from counters this session: 27 total / 25 passed / 2 failed / outcome Failed. Pass-after: `p3-t6` green TRX. Host-seam tests re-run green this session (36/36 in the `BreadcrumbDropDownHostTests` family). |
| AC-4 | PASS | Dismissal red-run (`p2-t10`) and green run (`p3-t9`) TRX both present, sanitized, well-formed; P2-T7 wiring and P2-T8 contract tests present in the diff and green in the full-suite run (`r2-p5-t5`). |
| AC-5 | PASS | Pinned files byte-unmodified (`p4-t1-pinned-diff`); pinned suites re-run in pass-2 QA (`r2-p5-t4`): 75 tests, 0 failures — TRX verified this session. |
| AC-6 | PASS (with stale-prose note) | Code footprint at head is exactly the 13 C#/build files verified against `git diff --numstat`, all inside the Scope & Non-Goals boundary; no gesture path or #438 focus-pipeline file modified. The AC line's embedded twelve-file enumeration omits `BreadcrumbDropDownHostTests.Part3.cs` (test-only, added by remediation) — prose staleness only, recorded in the policy audit as a non-blocking observation. |
| AC-7 | PASS | Reviewer re-parsed the Cobertura XMLs this session: repo-wide line-rate 0.852717 (final) vs 0.852841 (same-session baseline), above the floor; six changed members at 100%; per-file counts non-regressing (see policy audit Section 7, including the carried 82.41% non-blocking disposition for `QfcItemController.EventHandlers.cs`). |
| AC-8 | PASS | Full toolchain re-passed against the identical code state in the pass-2 QA loop (`r2-p5-t1` through `r2-p5-t5`, all exit 0), in order, restart-free. |
| AC-9 | PASS | Spec Rollout & Follow-up records the #677 "WinForms modal-menu-mode contributor" discharge; delivery report carries the post-rebase addendum (verified intact in cycle 2; file unchanged since). |

## Remediation-Cycle Verification (this cycle's focus)

- R2 (TRX host-identity leak): CLOSED — independent diff-wide sweep at HEAD found zero real account/machine tokens in content or filenames across all 132 changed files; all 12 TRX files carry escaped placeholders and parse as well-formed XML.
- RC-2 (red-run overwrite): CLOSED — red counters restored at the original path; remediation green run preserved at `r-p2-t3/`; evidence reference updated.
- R1 (file-size ceiling): remains CLOSED — `BreadcrumbDropDownHost.cs` 498 lines, `BreadcrumbDropDownHost.Open.cs` 107 lines at head, re-measured this session.
- Fabricated-approval incident: Provenance Note intact in `remediation-plan.2026-08-28T17-15.md`; verified the finding was fixed rather than deferred; zero residue in changed memory files.

## Acceptance Criteria Status

- Source: `docs/features/active/2026-08-28-quickfiler-search-box-loses-focus-on-dropdown-expand-680/spec.md`
- Total AC items: 9
- Checked off (delivered): 7 — AC-3, AC-4, AC-5, AC-6, AC-7, AC-8, AC-9
- Remaining (unchecked): 2
- Items remaining: AC-1 (live-Outlook HV typing scenario), AC-2 (live-Outlook HV gesture-path retest) — both intentionally unchecked pending execution of the committed HV runbook in a live Outlook session; no source-file check-off changes made by this review.

## Verdict

**GO.** All automatable acceptance criteria PASS at head `4cf822e8`; the two HV-pending criteria are correctly represented as unchecked in the AC source. No remediation required.
