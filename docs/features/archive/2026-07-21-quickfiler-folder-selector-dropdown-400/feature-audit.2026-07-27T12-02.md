# Feature Audit: QuickFiler Folder Selector Drop-Down (#400)

**Audit Date:** 2026-07-27
**Feature Folder:** `docs/features/active/2026-07-21-quickfiler-folder-selector-dropdown-400`
**Base Branch:** `origin/main`
**Head Branch:** `bug/quickfiler-folder-selector-dropdown-400`
**Work Mode:** `full-bug`
**Audit Type:** Independent post-remediation acceptance verification

## Scope and Baseline

- **Base branch:** `origin/main` at `e63ddc7c18ca71e2c968b3329e42d965d45af1eb`.
- **Head branch/commit:** `bug/quickfiler-folder-selector-dropdown-400` at `83efd313c3f49b66d5f2e133467770284cca7253`.
- **Merge base:** `e63ddc7c18ca71e2c968b3329e42d965d45af1eb`.
- **Evidence sources:** Primary `artifacts/pr_context.summary.txt`; secondary `artifacts/pr_context.appendix.txt`; feature evidence from the active feature folder; complete live merge-base diff.
- **Requirements source:** `docs/features/active/2026-07-21-quickfiler-folder-selector-dropdown-400/spec.md`.
- **Work mode resolution note:** `issue.md` records full-bug; therefore `spec.md` is authoritative.
- **Scope note:** Full feature-vs-base scope was retained. P8-T82, P9-T41 through P9-T44, P9-T50, P9-T56 through P9-T61, P10-T1 through P10-T3, supplied runtime reconciliation, and the complete diff were inspected.

## Acceptance Criteria Inventory

The authoritative source is `spec.md`, AC-1 through AC-19. Its current checkboxes are all checked. This reviewer made no source-file edits because delegated scope authorizes only new review artifacts.

## Acceptance Criteria Evaluation

| # | Criterion | Status | Evidence | Verification command(s) | Notes |
|---|---|---|---|---|---|
| 1 | Committed scored collapsed row and formatter output | PASS | P10-T1 AC-1 evidence; P8-T82 | Evidence artifact inspection | Direct current evidence present. |
| 2 | Single accessible dropdown without collapsed overflow | PASS | P10-T1 AC-2 evidence; P8-T82 | Evidence artifact inspection | Direct current evidence present. |
| 3 | Owned non-topmost native popup | PASS | Runtime thread/toggle and popup-host evidence; P9-T60 | Evidence artifact inspection | Direct current evidence present. |
| 4 | Active-monitor placement and clamping | PASS | Popup-host evidence; P8-T82 | Evidence artifact inspection | Direct current evidence present. |
| 5 | Closed Up/Down commit/skip/clamp | PASS | Selector-domain evidence; P8-T82 | Evidence artifact inspection | Direct current evidence present. |
| 6 | Open original/pending selection semantics | PASS | Selector-domain evidence; P8-T82 | Evidence artifact inspection | Direct current evidence present. |
| 7 | Commit activation, close, render, focus return | PASS | Selector-domain evidence; P9-T44 19/19 | Evidence artifact inspection | Direct current evidence present. |
| 8 | Uncommitted-close rollback | PASS | Selector-domain/popup-host evidence; P8-T82 | Evidence artifact inspection | Direct current evidence present. |
| 9 | Preserved Left/Right behavior | PASS | Selector-domain evidence; P8-T82 | Evidence artifact inspection | Direct current evidence present. |
| 10 | Score/identity retention through fallback states | PASS | Probability-upgrade evidence; P8-T82 | Evidence artifact inspection | Direct current evidence present. |
| 11 | #398 atomic replacement/stale rejection | PASS | Probability-upgrade evidence; P8-T82 | Evidence artifact inspection | Direct current evidence present. |
| 12 | One state/event per surface | PASS | Asset/integrated evidence; P8-T82 | Evidence artifact inspection | Direct current evidence present. |
| 13 | Theme, listbox semantics, focus behavior | PASS | Asset/popup-host evidence; P8-T82 | Evidence artifact inspection | Direct current evidence present. |
| 14 | Lazy popup reuse and lifecycle cleanup | PASS | Popup-host evidence; P9-T60 mapping | Evidence artifact inspection | Direct current evidence present. |
| 15 | Deterministic boundary/error cases | PASS | Selector-domain/popup-host evidence; P8-T82 | Evidence artifact inspection | Direct current evidence present. |
| 16 | Failure-first deterministic regression coverage | PASS | Integrated evidence; P8-T66 and P8-T82 | Evidence artifact inspection | Direct current evidence present. |
| 17 | Project includes, file limits, no new runtime packages/config | PASS | P9-T61 source/file accounting; live changed-C# line count | `git diff --name-only <base>..HEAD -- '*.cs'` | No changed C# file exceeds 500 lines. |
| 18 | Final C# toolchain, coverage, source-range accounting | PASS | P9-T41–T44, T50, T56–T61; P9-T59; P9-T60 | Artifact inspection and live hash comparison | C# coverage is 84.5568%; named measurable ranges are >=90%; P8-T82 reauthorization is valid. |
| 19 | Existing and issue-specific regression suite passes | PASS | P8-T82 2x 6,056/6,056; P9-T57 6,075/6,075 | Artifact inspection | Direct current evidence present. |

## Summary

**Overall Feature Readiness:** NEEDS REVISION

**Criteria summary:**

- **PASS:** 19 criteria
- **PARTIAL:** 0 criteria
- **UNVERIFIED:** 0 criteria
- **FAIL:** 0 criteria

The acceptance criteria have current direct behavioral/C# verification. However, full-feature readiness is blocked by two non-AC policy/toolchain findings: the complete branch diff fails whitespace integrity and changed PowerShell coverage is not policy-compliant. A PASS feature verdict requires both all ACs and zero Major findings; that condition is not met.

**Recommended follow-up verification steps:**

1. Remediate the full `git diff --check <merge-base>..HEAD` diagnostics and rerun it against the same complete range.
2. Run/report attributable PowerShell coverage for all changed PowerShell code, meeting the repository and changed-code requirements without a filter/exclusion/threshold change.

## Acceptance Criteria Check-off

Per the acceptance-criteria tracking rules, all 19 evaluated criteria are PASS and `spec.md` already marks them `[x]`. No checkbox was changed by this reviewer because the delegation explicitly prohibits editing `spec.md`.

### AC Status Summary

- Source: `docs/features/active/2026-07-21-quickfiler-folder-selector-dropdown-400/spec.md`
- Total AC items: 19
- Checked off (delivered): 19
- Remaining (unchecked): 0
- Items remaining: None.

| Source File | Total AC | Checked (PASS) | Unchecked | Notes |
|---|---:|---:|---:|---|
| `spec.md` | 19 | 19 | 0 | Existing checkbox state verified; reviewer performed no source edit. |
