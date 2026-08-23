# Feature Audit: QuickFiler folder-selector dropdown (Issue #400)

**Audit Date:** 2026-08-04
**Feature Folder:** `docs/features/active/2026-07-21-quickfiler-folder-selector-dropdown-400`
**Base Branch:** `origin/main`
**Head Branch:** `bug/quickfiler-folder-selector-dropdown-400` at `62c4eb1c2b99ae6e9fa7742a31d283ec4a8d7151`, including final worktree remediation
**Work Mode:** `full-bug`
**Audit Type:** Post-remediation acceptance verification

## Scope and Baseline

- **Base branch:** `origin/main`.
- **Head branch/commit:** `bug/quickfiler-folder-selector-dropdown-400` at `62c4eb1c2b99ae6e9fa7742a31d283ec4a8d7151`.
- **Merge base:** `050f7cd52a3b13ec2786c9dafbe9f99620ebf9e8`.
- **Evidence sources:**
  - Primary: direct `git diff`, `git diff --check`, scoped PSScriptAnalyzer, and focused Pester review output.
  - Secondary baseline diff: `artifacts/pr_context.appendix.txt` (historical only; it remains stale after published MCP collection).
  - Feature evidence: `evidence/regression-testing/invoke-mstest-wrapper-focused-coverage.2026-08-04T11-15.md` and `evidence/qa-gates/powershell-*.2026-08-04T11-15.md`.
- **Feature folder used:** `docs/features/active/2026-07-21-quickfiler-folder-selector-dropdown-400`.
- **Requirements source:** `spec.md`.
- **Work mode resolution note:** `issue.md` explicitly records `Work Mode: full-bug`; therefore `spec.md` is the sole authoritative checkbox AC source.
- **Scope note:** The published MCP collector was invoked with `origin/main` but did not update the stale on-disk head value. Live Git state is used for current-range verification. No acceptance source was edited because all passing AC checkboxes were already marked `[x]`.

## Acceptance Criteria Inventory

**Authoritative AC source files for this run:**

- `docs/features/active/2026-07-21-quickfiler-folder-selector-dropdown-400/spec.md` — only source.

### Acceptance criteria

1. AC-1: Scored selected folder renders as one collapsed row with retained normalized percentage and no recomputation.
2. AC-2: Collapsed page has no scrolling affordance and exactly one accessible drop-down button.
3. AC-3: Host opens an owned native popup over sibling controls and never as a system-wide topmost window.
4. AC-4: Popup placement follows active-monitor working-area, below/above/tie/clamp rules.
5. AC-5: Closed Up/Down commits selectable bounded movement without scrolling or duplicate publish.
6. AC-6: Open Up/Down changes pending selection only with original snapshot and active visibility.
7. AC-7: Enter and row activation commit once, close, render committed state, and return focus.
8. AC-8: Escape and uncommitted closes restore original selection without a pending commit.
9. AC-9: Left/Right breadcrumb behavior remains unchanged and does not mutate selector state.
10. AC-10: All immediate/hierarchy fallback paths retain score, identity, and selection.
11. AC-11: Issue #398 atomic replacement and stale-upgrade protections remain intact.
12. AC-12: Both surfaces receive state once and route each inbound event once.
13. AC-13: Theme, accessibility/listbox, focus, and initialization-failure seams are verified.
14. AC-14: Popup WebView lazy initialization, reuse, disposal/reset, and subscription lifecycle are verified.
15. AC-15: Empty/invalid/failure/placement/repeated-close boundaries are deterministic and leak-free.
16. AC-16: Deterministic failure-first automated evidence covers the listed semantic contracts.
17. AC-17: Source/project includes, file-size limits, generated-designer boundary, and dependency limits are satisfied.
18. AC-18: Exact C# format/analyzer/nullable/MSTest coverage toolchain and coverage thresholds are satisfied.
19. AC-19: Existing and new semantic regression tests pass.

## Acceptance Criteria Evaluation

| # | Criterion | Status | Evidence | Verification command(s) | Notes |
|---|-----------|--------|----------|--------------------------|-------|
| 1-17 | AC-1 through AC-17 | PASS | `spec.md` checkboxes and retained feature evidence under `evidence/regression-testing` and `evidence/qa-gates`. | Historical feature toolchain evidence; direct current `git diff --check`. | No final remediation changed the C# or HTML implementation that satisfies these ACs. |
| 18 | AC-18 exact C# toolchain and coverage contract | PASS | Prior recorded C# QA evidence remains applicable; final PowerShell remediation preserves protected C# coverage/config inputs. | Current protected-input hash evidence and `git diff --check 050f7cd52a3b13ec2786c9dafbe9f99620ebf9e8`. | Final changes are two PowerShell paths only; no C# scope/config drift was detected. |
| 19 | AC-19 regression suite and semantic contract | PASS | Focused Pester rerun passed 25/25; historical feature evidence records the broader selector suite. | `Invoke-Pester -Path tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1`. | The final remediation has direct automated regression coverage. |

## Summary

**Overall Feature Readiness:** PASS

**Criteria summary:**

- **PASS:** 19 criteria
- **PARTIAL:** 0 criteria
- **UNVERIFIED:** 0 criteria
- **FAIL:** 0 criteria

**Top gaps preventing PASS:**

1. None.

**Recommended follow-up verification steps:**

1. Run normal PR/CI gates after committing the review and remediation artifacts.
2. Correct the external PR-context collector refresh behavior in its own maintenance scope.

## Acceptance Criteria Check-off

No checkbox source-file change was made. All 19 authoritative `spec.md` acceptance criteria were already checked `[x]`, and this review independently confirmed that final remediation introduces no AC regression.

### AC Status Summary

- Source: `docs/features/active/2026-07-21-quickfiler-folder-selector-dropdown-400/spec.md`
- Total AC items: 19
- Checked off (delivered): 19
- Remaining (unchecked): 0
- Items remaining: None.

| Source File | Total AC | Checked (PASS) | Unchecked | Notes |
|-------------|----------|----------------|-----------|-------|
| `docs/features/active/2026-07-21-quickfiler-folder-selector-dropdown-400/spec.md` | 19 | 19 | 0 | Checkbox-backed and authoritative for `full-bug`. |
