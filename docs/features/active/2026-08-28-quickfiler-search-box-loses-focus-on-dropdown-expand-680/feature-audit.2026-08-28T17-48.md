# Feature Audit — Issue #680 (re-audit cycle after remediation)

- Date: 2026-08-28T17-48
- Reviewer: feature-review agent
- Branch: `bug/quickfiler-search-box-loses-focus-on-dropdown-expand-680` @ `c4e96b72b38fc122a8658ecbeff245814eef09bd`
- Base: merge-base `b0c7fa18a3beb073e7b051f49e28f48159f0f179`
- Work mode: `full-bug` (verified: `- Work Mode: full-bug` marker in `issue.md`) — `spec.md` is the sole authoritative AC source
- Prior cycle: `feature-audit.2026-08-28T16-27.md` (7 of 9 AC delivered; AC-1/AC-2 pending live-Outlook HV)

## Summary

The remediation commit changes no acceptance-criteria outcome: the prior cycle's Blocking policy finding (file-size ceiling) was orthogonal to the spec ACs, and its fix is a verbatim relocation with zero behavior change (verified: identical bodies/accessibility, unchanged call sites, scoped suites 36/36 in committed TRX and 64/64 in this reviewer's rerun at head). All seven automatable ACs remain delivered and re-verified; AC-1 and AC-2 remain intentionally unchecked pending the live-Outlook HV runbook, which is the correct treatment under the recorded human-exception route. One evidence-chain caveat (AC-3's red-run TRX overwritten by a remediation task-ID collision) and one stale enumeration (AC-6's twelve-file footprint predates the remediation's thirteenth code file) are noted below; neither changes an AC verdict.

## AC Evaluation

| AC | Verdict | Evidence (re-verified this cycle unless noted) |
|---|---|---|
| AC-1 (keystrokes delivered continuously, live HV) | UNVERIFIED — pending live-Outlook HV, by design | Not dischargeable headlessly (menu-mode engagement, Win32 focus, live message pump). Runbook exists: `runbooks/quickfiler-search-focus-hv-680.runbook.md` (items HV-1/HV-2). Checkbox correctly left unchecked in `spec.md` (line 160). Documented exception carried from the prior cycle; unchanged by the remediation. |
| AC-2 (gesture paths unchanged, live HV) | UNVERIFIED — pending live-Outlook HV, by design | Same runbook, items HV-3 through HV-9. The automatable half is pinned by `TextBoxSearchKeyDown_DownArrow_StillOpensAndFocusesTheDropDown` and the gesture-open host tests — green in the reviewer's 64/64 rerun at head. Checkbox correctly unchecked (line 161). |
| AC-3 (fail-before host-seam regression test) | PASS (checked) | Test population present and green at head (reviewer rerun). Fail-before chain: markdown transcription `p2-t3-red-run-host.2026-08-28T15-30.md` intact (27 total / 25 passed / 2 predicted failures); pass-after `p3-t6` intact. Caveat (non-blocking): the red-run TRX at `evidence/regression-testing/p2-t3/p2-t3.trx` was overwritten by the remediation's green run (task-ID collision); reviewer confirmed the red counters (`total="27" passed="25" failed="2"`) directly from `8e82a2e0:.../p2-t3/p2-t3.trx` in git history. Restore recommended in `remediation-inputs.2026-08-28T17-48.md` item 2. |
| AC-4 (coordinator/controller/contract dismissal tests) | PASS (checked) | Red/green chain (`p2-t10` red, `p3-t9` green 47/47) verified in cycle 16-27; all dismissal, wiring, and contract tests green in this cycle's 64/64 scoped rerun at head. |
| AC-5 (#438/#400 pinned suites pass unmodified) | PASS (checked) | None of the nine pinned files appears in the branch diff at head (re-confirmed via `git diff --name-status b0c7fa18..HEAD`); pinned-suite green run (75/75, `p4-t2.trx`) verified in cycle 16-27; host-suite members re-green in this cycle's rerun. |
| AC-6 (no unintended changes outside scope boundary) | PASS (checked), with a stale-enumeration note | The full code diff at head is 13 files (7 production, 5 test, 1 csproj), all inside the Scope & Non-Goals boundary. Note: the AC's parenthetical enumerates "twelve files" — written before the remediation added `BreadcrumbDropDownHostTests.Part3.cs` (test, in-boundary) to the footprint. The criterion itself remains satisfied; the embedded enumeration is stale by one test file. Recommended: a one-line note in the spec's AC status table at PR time (do not edit the AC text). |
| AC-7 (coverage: members >= 90%, no changed-line regression, repo-wide recorded) | PASS (checked) | Re-verified this cycle from raw Cobertura: repo-wide line-rate 0.852717 (final, same-session baseline 0.852841, no-change rerun 0.852888 — dip adjudicated as instrumentation noise; branch-rate 0.792401 >= baseline 0.79234); relocation-touched files at 99.32% and 100% with zero per-file regression; feature-changed members at 100% per cycle 16-27, relocated members fully covered at their new location (23/23). |
| AC-8 (full toolchain pass in order) | PASS (checked) | Feature-cycle final pass verified in cycle 16-27. Remediation cycle ran its own full loop: format (exit 0 after one formatter-triggered restart, correctly restarted per the loop rule), analyzer rebuild (exit 0, warning set = baseline), nullable rebuild (exit 0), full test run (exit 0, 0 failures). Reviewer independently re-verified format (exit 0, 1560 files) and tests (64/64) at head this session. |
| AC-9 (docs updated, #677 follow-up discharge recorded) | PASS (checked) | Delivery report now carries the Post-Rebase Addendum correcting the two statements the rebase staled (verified append-only, both corrections present verbatim); rollout notes, runbook, and the #677 discharge record are unchanged and remain accurate. |

## Acceptance Criteria Status

- Source: `docs/features/active/2026-08-28-quickfiler-search-box-loses-focus-on-dropdown-expand-680/spec.md`
- Total AC items: 9
- Checked off (delivered): 7 — AC-3, AC-4, AC-5, AC-6, AC-7, AC-8, AC-9
- Remaining (unchecked): 2
- Items remaining: AC-1 (live-HV keystroke continuity), AC-2 (live-HV gesture-path parity) — both intentionally unchecked pending execution of the 9-item HV runbook in a live Outlook session, per the recorded human-exception route. No newly checked-off items this cycle; the source-file checkbox states already match the verdicts above and were left untouched.

## Baseline Comparison

- Merge-base `b0c7fa18` is the current origin/main tip; the branch carries 7 commits.
- Behavior delta vs baseline is confined to the search-box open/dismiss lifecycle: non-capturing (`AutoClose = false`) search-driven opens, controller-owned dismissal (Escape/Leave with the one-shot handoff latch), `AutoClose` restore at close completion and focusing reopen, and the additive `IItemViewer` members. Gesture paths, the #438 managed focus pipeline, and all pinned suites are unchanged.
- Post-remediation coverage baseline for downstream reviews: repo-wide line-rate 0.852717–0.852888 (run-to-run band), branch-rate 0.7924.

## Gate to PR

The feature itself is delivery-complete for its automatable scope. Two items gate the PR:
1. **Blocking**: TRX host-identity sanitization (R2 in `remediation-inputs.2026-08-28T17-48.md`).
2. **Owner action (non-gating for code, gating for AC closure)**: execute the HV runbook and record the outcome under `evidence/other/` before checking AC-1/AC-2.
