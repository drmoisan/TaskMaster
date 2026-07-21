# Feature Audit — folder-combobox-fallback-index-out-of-range (Issue #392)

- Timestamp: 2026-07-20T19-30
- Reviewer: feature-review (remediation re-audit, cycle 1, R4)
- Work Mode: `minor-audit`

## Scope and Baseline

- Base branch (resolved): `main` @ `bd43572498474be89d80e1f9620dffb132ade377`.
- Head: `8a1b7b98b7d12dac69fd1bee5d5f109d4095c3c6` (two commits ahead of merge-base: `8f34f8ef` fix,
  `8a1b7b98` remediation).
- Audit scope: the full branch diff vs the merge-base (feature-vs-base), not any plan/task/phase
  subset. Confirmed via `git diff --numstat bd435724..8a1b7b98`: 2 changed `.cs` files (1 production,
  1 test; unchanged count from the original audit), 53 added/modified Markdown files.
- Acceptance-criteria source (per `minor-audit` marker in `issue.md`): the explicit
  `## Acceptance Criteria` section of `issue.md` only (AC-1 through AC-5). No `spec.md` or
  `user-story.md` exist in this feature folder.
- Evidence: production/test diffs read directly; remediation-cycle QA-gate evidence under
  `evidence/qa-gates/remediation-*` and `evidence/remediation-baseline/`; canonical C# coverage
  artifact `artifacts/csharp/coverage.xml` (regenerated this cycle) parsed directly; the
  `human_interaction` `scope_change` record in `artifacts/orchestration/orchestrator-state.json`.

## Acceptance Criteria Inventory

Unchanged from the original audit (`feature-audit.2026-07-20T18-00.md`). From `issue.md`
`## Acceptance Criteria`:

- AC-1: A deterministic MSTest regression test reproduces the defect and fails before the fix; the
  same test passes after the fix. No temporary files or external dependencies are used.
- AC-2: `QfcItemController.AssignFolderComboBox` no longer throws `ArgumentOutOfRangeException` when
  `FolderArray` has exactly one entry and no predetermined folder matches: it selects index 0 instead
  of index 1.
- AC-3: Existing multi-suggestion behavior is preserved.
- AC-4: The retained static helper `PopulateAndSelectFolder` applies the same bounds-safe fallback.
- AC-5: The full C# toolchain passes with zero regressions relative to the Phase 0 baseline, and
  new/changed code meets the >= 90% coverage target, with the amended first-party-scoped nullable
  note.

## Acceptance Criteria Evaluation

| AC | Verdict | Evidence |
|---|---|---|
| AC-1 | PASS (unchanged) | `evidence/regression-testing/fail-before-392.2026-07-20T14-05.md` / `pass-after-392.2026-07-20T14-10.md`. Not affected by this remediation cycle (production code is byte-identical to cycle 1). |
| AC-2 | PASS (unchanged) | `git diff` shows the same clamp introduced in cycle 1, unmodified. |
| AC-3 | PASS (unchanged) | Pre-existing tests re-verified again in this cycle's full 542-test run with zero regressions. |
| AC-4 | PASS (unchanged) | Same clamp in `PopulateAndSelectFolder`, unmodified. |
| AC-5 | PASS | Toolchain: format PASS (independently reproduced 0 errors via `csharpier check .` on this review host), analyzers PASS (0 errors), tests PASS (542/542, 0 regressions — up from 541 per `evidence/qa-gates/remediation-regression-check.2026-07-20T18-46.md`), nullable reproduces the same byte-identical pre-existing 34-error vendored condition (0 new, 0 first-party). New/changed-code coverage: 100% on the original fix's lines (unchanged from cycle 1). **Additionally resolved this cycle**: the touched class's branch-coverage floor (73.81% -> 76.19%, per `evidence/qa-gates/remediation-coverage-delta.2026-07-20T18-44.md`, independently re-verified against the regenerated canonical artifact), which was an open policy-audit finding (not an AC gap) in the original cycle. The separate, broader `QuickFiler` package-wide/repo-wide coverage gap remains below floor but is now formally ratified as a `scope_change` exception (see `policy-audit.2026-07-20T19-30.md` Section 5.4); this does not affect AC-5's own literal PASS status, which was already met in the original cycle on its own terms. |

## Summary

All five acceptance criteria in `issue.md` remain PASS, unchanged from the original audit and
unaffected by this coverage-only remediation cycle (no production code was modified). The one
policy-level (not AC-level) finding open after the original audit — the touched class's marginal
branch-coverage floor gap (73.81%, floor 75%) — is now closed (76.19%), independently re-verified
against the regenerated canonical `artifacts/csharp/coverage.xml`. The separate, broader `QuickFiler`
package-wide/canonical repo-wide coverage gap remains below floor but is now properly ratified via a
`human_interaction` `scope_change` record citing open GitHub issue #136, the `#328` `StoreWrapper`
precedent, and CLAUDE.md's COM/VSTO testable-denominator exemption — consistent with established repo
practice for pre-existing, unrelated-to-the-fix coverage shortfalls. **Zero blocking findings remain
in this cycle.**

### Acceptance Criteria Status
- Source: `issue.md` (`## Acceptance Criteria`, AC-1 through AC-5)
- Total AC items: 5
- Checked off (delivered): 5
- Remaining (unchecked): 0
- Items remaining: none

## Acceptance Criteria Check-off

All five AC items remain checked off (`[x]`) in `issue.md`; no checkbox state changed in this
remediation cycle (verified: this coverage-only cycle did not touch any AC checkbox, per
`evidence/issue-updates/remediation-cycle1-note.2026-07-20T18-48.md`, and this audit's independent
re-verification confirms all five remain PASS). No checkbox text was modified.
