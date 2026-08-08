# Feature Audit — QuickFiler Search Keystroke Focus Steal (Issue #438) — Cycle 2 (Post-Remediation Re-Audit)

- **Date:** 2026-08-08T15-34 (clock-read)
- **Reviewer:** feature-review agent (cycle 2)

## Scope and Baseline

- **Base branch:** `main` (resolved per `pr-base-branch-merge-base`; independently recomputed this session via `git merge-base HEAD origin/main` → `003c5715055d7d1933db68a742531332756e30b2`, matching the caller-supplied value)
- **Branch head:** `bug/quickfiler-search-keystroke-focus-steal-438` @ `2134fa7be5a3638a3dd34c4182c2f118f0fc3d90` (matches the refreshed PR-context artifacts' recorded head; artifacts confirmed fresh, generated 2026-08-08 19:27:53 UTC against this head)
- **Diff scope:** 97 files (30 `.cs`, 4 `.csproj`, remainder docs/evidence/agent-memory). Only C# has changed files.
- **Cycle delta:** since cycle-1 head `ff9d14ab`, the only non-documentation change is the additive two-test extension of `QuickFiler.Test/Viewers/BreadcrumbItemViewerLifecycleCoordinatorTests.cs` (remediation R1). Production code is bit-identical to what cycle 1 audited, so cycle-1 AC verifications carry forward by construction and were re-anchored at the new head.
- **Work mode:** `full-bug` (persisted `- Work Mode: full-bug` marker in `issue.md`). AC source: `spec.md` only — AC-1..AC-14 gating, HV-1 a documented non-gating human-verification exception. `user-story.md` intentionally absent for this mode.
- **Primary evidence:** refreshed `artifacts/pr_context.summary.txt` / `artifacts/pr_context.appendix.txt` (summary's C# misclassification corrected in place; appendix used for true scope); executor and remediation evidence trees under `evidence/`; independent reviewer verification recorded in `policy-audit.2026-08-08T15-34.md`.

## Acceptance Criteria Inventory

| ID | Summary | Gating |
|---|---|---|
| AC-1 | Controller seam: presentation intent exactly once; no `SetFolderDroppedDown` / `FocusFolderDropDown` / `SetFolderSelectedIndex`; fails before, passes after | Yes |
| AC-2 | Search-driven open: zero `FocusPending`/`FocusAnchor`; default (gesture) open: `FocusPending` exactly once | Yes |
| AC-3 | Two consecutive search refreshes: exactly one host `OpenAsync`, zero host `Close` | Yes |
| AC-4 | Highlight changes only `PendingIdentity`; no `SelectionChanged`; committed selection untouched | Yes |
| AC-5 | Escape during search restores pre-search committed identity; controller cache not stale | Yes |
| AC-6 | Multi-character string through the viewer seam delivers the full string; row set reflects the complete query | Yes |
| AC-7 | Explicit-gesture behavior unchanged and pinned; named existing tests pass unmodified | Yes |
| AC-8 | Session-preserving row replacement emits exactly one render per surface per state update (#400 AC-12 preserved) | Yes |
| AC-9 | Empty and banner-only result sets are deterministic no-ops for the highlight step | Yes |
| AC-10 | Contract changes additive only: one `IItemViewer` member, one 4-parameter `OpenAsync` overload; contract tests pass unmodified | Yes |
| AC-11 | #400 reconciliation holds; single sanctioned test-method rewrite; enumerated structural edits only | Yes |
| AC-12 | Final uninterrupted toolchain pass; new/changed members >= 90%; changed-line coverage no regression; repo figure >= baseline; figures recorded in evidence | Yes |
| AC-13 | EfcViewer search path has zero diff | Yes |
| AC-14 | `<Compile Include>` wiring for every added `.cs`; no file over 500 lines; no new package/config | Yes |
| HV-1 | Live-Outlook eight-character typing check per runbook | No (documented human-verification exception; not a merge gate) |

## Acceptance Criteria Evaluation

Cycle-1 evaluated all 14 gating criteria PASS at `ff9d14ab` on independent evidence (see `feature-audit.2026-08-08T13-25.md` for the per-criterion verification detail). This cycle re-verifies each at head `2134fa7b`, where the only code delta is the additive test-only remediation.

| ID | Verdict | Cycle-2 verification |
|---|---|---|
| AC-1 | PASS | Production handler untouched since cycle 1; regression suite (8 tests) green in the 6350/6350 final run; fail-before evidence unchanged and still on-branch. |
| AC-2 | PASS | Host/lifetime focus-guard files bit-identical since cycle 1; delegate-count tests green in final run. |
| AC-3 | PASS | Coordinator/router files bit-identical; open-once/never-close tests green in final run. |
| AC-4 | PASS | Session `HighlightRow` bit-identical; pending-only tests green in final run. |
| AC-5 | PASS | Escape-path tests green in final run; no related file touched since cycle 1. |
| AC-6 | PASS | Full-string seam tests green in final run. |
| AC-7 | PASS | No gesture-path file or test touched since cycle 1; pinning tests green in final run. |
| AC-8 | PASS | Router/session render-count tests green in final run. |
| AC-9 | PASS | Edge-case no-op tests green in final run; the two new remediation tests additionally pin the unwired-coordinator no-op configuration. |
| AC-10 | PASS | Interface files bit-identical since cycle 1 (re-verified via `ff9d14ab..HEAD` diff: no production file present); additive-only conclusion stands; contract tests green in final run. |
| AC-11 | PASS | Amended AC-11 still protects test integrity at the new head: the remediation added two methods with zero removed lines (verified: diff hunk contains no `^-` content lines); no existing test modified anywhere on the branch beyond the cycle-1 sanctioned set. |
| AC-12 | PASS | Remediation-cycle uninterrupted final pass: csharpier EXIT 0 → analyzer EXIT 0 → nullable-as-errors EXIT 0 → coverage-enabled run 6350/6350 EXIT 0. Member floor: R1 target file now 100%/100%; member minimum across new/changed members unchanged at 95.24% (>= 90). Changed-line no-regression holds (remediation changed only test lines). Repo figure vs baseline: line 0.858512 → 0.858620 and branch 0.792359 → 0.792860 against the same-session baseline — both improved; the 0.000045 shortfall against the cycle-1 cross-session constant is adjudicated measurement variance in three untouched legacy files (policy audit §1.3), not a regression, and the applicable policy floors (85%/75%) are cleared. Figures recorded in `evidence/qa-gates/` and `evidence/remediation-baseline/`. |
| AC-13 | PASS | Re-verified at head: `git diff --name-only 003c5715..HEAD` contains no EfcViewer search-path file; zero diff. The gesture-scoped qualification of #400 AC-13 remains the sanctioned, documented refinement assessed in cycle 1. |
| AC-14 | PASS | Re-verified at head: no new `.cs` file added by the remediation (extension in place), zero `.csproj` diffs since cycle 1; maximum changed-file length 499 (`BreadcrumbDropDownHostTests.cs`, unchanged), extended test file 382; no new package/config anywhere on the branch. |
| HV-1 | UNVERIFIED (non-gating by spec) | Deliberately outstanding; runbook at `runbooks/verify-search-focus-retention.runbook.md`; negative outcome to be promoted as a new issue post-merge. |

## Summary

All 14 gating acceptance criteria remain **PASS** at the remediated head. The cycle-1 blocking coverage finding (R1) is resolved: the null-coordinator fallback arms of `PresentSearchResults` are now pinned by two observable-outcome tests, and the file measures 100% line / 100% branch reproducibly. The two items submitted for adjudication — the literal repo-wide line-floor miss on plan task `[P2-T7]` and the estimated evidence timestamps — are both dispositioned non-blocking on independent evidence (policy audit §1.3 and §7 CQ-2). Zero blocking findings remain. Outstanding non-gating items: R2 maintainer disposition, promotion of the recurring PR-context classifier defect, and HV-1 post-merge execution. Recommendation: **go** for PR creation.

## Acceptance Criteria Check-off

All 14 gating criteria were checked `[x]` in `spec.md` during execution and re-verified PASS by cycle 1; `spec.md` is byte-unmodified since (verified at P2-T8 and re-verified this session via `git diff ff9d14ab..HEAD -- .../spec.md` → empty). Each criterion was re-verified PASS at the new head this session, so all existing check-offs stand; no source-file edits were required and none were made. HV-1 remains `[ ]` (correctly unchecked; non-gating). No phantom criteria were added.

### Acceptance Criteria Status
- Source: `docs/features/active/2026-08-07-quickfiler-search-keystroke-focus-steal-438/spec.md`
- Total AC items: 15 (14 gating + HV-1 non-gating)
- Checked off (delivered): 14
- Remaining (unchecked): 1
- Items remaining: HV-1 (documented human-verification exception — not a merge gate; execute post-merge per `runbooks/verify-search-focus-retention.runbook.md`)
