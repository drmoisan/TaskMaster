# Remediation Plan — 2026-09-02-ci-build-infra-debt-730

- Timestamp: 2026-09-02T23-56
- Issue: #730
- Branch: `bug/ci-build-infra-debt-730` @ `e80f74c9`
- Base: `origin/main` @ `8be5a6aa`
- Work Mode: full-bug
- Scope: R-1 (mandatory) and R-4 (mandatory delivery hygiene); R-2 and R-3 omitted per directive.

---

## Context

Feature-review pass (2026-09-02T23-47) identified one blocking artifact-hygiene defect (R-1): the research document `research/research.2026-09-02T09-15.md` line 8 embeds an absolute host path containing the operator account name. This violates the repository rule: no committed artifact may embed absolute host paths or the operator account name. All eight specification acceptance criteria PASS; the workflow and build-property changes require no correction.

A preflight review of round 1 of this remediation plan (remediation cycle 1, round 2) additionally found that this plan document itself reproduced the same absolute path/account-name literal at four locations. Round 2 of this plan redacts those four locations and tightens the verification and commit tasks below; the redaction is folded into items 1–2 and 4 below rather than tracked as a separate scope item, because it is a correction to this plan's own compliance with the same R-1 rule it enforces on `research.md`.

Remediation scope:
1. Replace the absolute path on line 8 of `research.md` with the `<repo-root>` placeholder (R-1), and redact the same literal from this plan document's own prose (Preflight Validation and Self-Review sections).
2. Verify via case-insensitive sweeps that the account-name token is gone from the feature folder with no exclusions, and that no `C:\Users` or `C:/Users` strings remain in the feature folder outside three files that reference the search-pattern text only as methodology description, never as an embedded absolute host path — two pre-existing, out-of-scope feature-review artifacts (`policy-audit.2026-09-02T23-47.md`, `remediation-inputs.2026-09-02T23-47.md`) and this plan document itself (`remediation-plan.2026-09-02T23-56.md`, which necessarily names the pattern it instructs the executor to search for) (R-1).
3. Confirm the four sanitized `.log` files remain unchanged (regression gate).
4. Stage and commit the full review-and-remediation record: `research.md`, this remediation plan (once redacted), the plan file's outstanding checkbox residue, and the four untracked feature-review artifacts (R-4).

---

## Plan

### Phase 0 — Remediation: Sanitize Research Document and Commit Residual

- [x] [P0-T1] Read the current line 8 of `docs/features/active/2026-09-02-ci-build-infra-debt-730/research/research.2026-09-02T09-15.md` to verify its exact current state. The line's raw literal value is not reproduced in this plan, per the repository's no-absolute-host-path rule; the fenced block below uses `<repo-root>` as a symbolic placeholder standing in for whatever the actual absolute path is, not as an assertion that the line already reads that placeholder text pre-edit. Acceptance: the line currently consists of exactly one backticked absolute host path occupying the full line, embedding the operator account name, symbolically represented as:
  ```
    `<repo-root>`
  ```
  Record the pre-edit baseline in `evidence/other/research-line8-pre-edit.2026-09-02T23-56.md`, describing the line's structure only (one backticked absolute host path spanning the full line, embedding the operator account name) and explicitly not quoting the raw path or account-name value, consistent with the redaction convention P2-T10 of `plan.2026-09-02T08-57.md` uses for `evidence/qa-gates/log-sanitization.2026-09-02T23-31.md`.

- [x] [P0-T2] In `docs/features/active/2026-09-02-ci-build-infra-debt-730/research/research.2026-09-02T09-15.md`, replace line 8's backticked absolute path with `<repo-root>`, so the bullet reads:
  ```markdown
  - All evidence below was read directly from the working tree at
    `<repo-root>`
    on branch `bug/ci-build-infra-debt-730` (based on `origin/main`) on 2026-09-02.
  ```
  Acceptance: line 8 (and only line 8) changes, from the backticked absolute host path to the single line `` `<repo-root>` `` with its existing two-space list-continuation indentation preserved (verified directly against the current file: line 8's leading-whitespace run is 2 spaces, not 4); line 7 (`- All evidence below was read directly from the working tree at`) and line 9 (`` on branch `bug/ci-build-infra-debt-730` (based on `origin/main`) on 2026-09-02. ``) remain byte-for-byte unchanged.

- [x] [P0-T3] Verify the remediation via case-insensitive sweeps across the entire feature folder `docs/features/active/2026-09-02-ci-build-infra-debt-730/` (the same folder scope as this task's opening sentence and sub-item 3 below — not "committed files in the branch diff", which would silently skip untracked files including this plan document itself):
  1. Run `$accountToken = Split-Path -Leaf $env:USERPROFILE; @(Get-ChildItem -Recurse -File -LiteralPath docs/features/active/2026-09-02-ci-build-infra-debt-730 | Select-String -Pattern "(?i)$([regex]::Escape($accountToken))").Count`. The account-name token is derived at run time from the current session's own `$env:USERPROFILE`, matching the convention P2-T10 of `plan.2026-09-02T08-57.md` already uses, rather than being hardcoded in this task. Expect the printed count to equal `0`.
  2. Run `@(Get-ChildItem -Recurse -File -LiteralPath docs/features/active/2026-09-02-ci-build-infra-debt-730 | Where-Object { $_.Name -notin @('remediation-inputs.2026-09-02T23-47.md', 'policy-audit.2026-09-02T23-47.md', 'remediation-plan.2026-09-02T23-56.md') } | Select-String -Pattern 'C:\\Users|C:/Users').Count`. Expect the printed count to equal `0`. The three named files are excluded from this sweep because each references the search-pattern text only as methodology description — a table column header in `policy-audit.2026-09-02T23-47.md`, sweep-description prose in `remediation-inputs.2026-09-02T23-47.md`, and this plan's own sweep-instruction and citation prose in `remediation-plan.2026-09-02T23-56.md` (this document necessarily names the pattern it instructs the executor to search for) — never as an embedded absolute host path. All three are confirmed, by direct read in this revision pass, to contain zero occurrences of the operator account-name value itself; that value remains covered, for this document too, by sub-item 1's unexcluded sweep.
  3. Confirm the four sanitized `.log` files remain unchanged: verify their line counts are still 11878, 12030, 11906, and 11742 respectively, and that 0-match sweeps on all four still hold.

  Record the verification results (command output and pass/fail status) in `evidence/qa-gates/research-sanitization-verification.2026-09-02T23-56.md`. Acceptance: sub-item 1 and sub-item 2 each print a count of `0`, and sub-item 3 confirms all four `.log` line counts are unchanged.

- [ ] [P0-T4] Stage and commit the full review-and-remediation record. Before staging, run `git status --porcelain` and confirm every reported path is prefixed by `docs/features/active/2026-09-02-ci-build-infra-debt-730/`. Re-derived in this revision pass (a live `git status --porcelain` could not be run directly by the planner in this pass; this enumeration was cross-checked against a full directory listing of the feature folder instead), the expected reported paths are: `plan.2026-09-02T08-57.md` (modified — checkbox residue), `research/research.2026-09-02T09-15.md` (modified by P0-T2), `code-review.2026-09-02T23-47.md` (untracked), `feature-audit.2026-09-02T23-47.md` (untracked), `policy-audit.2026-09-02T23-47.md` (untracked), `remediation-inputs.2026-09-02T23-47.md` (untracked), `remediation-plan.2026-09-02T23-56.md` (untracked, this plan file), plus two new evidence artifacts P0-T1 and P0-T3 create under `evidence/other/` and `evidence/qa-gates/` respectively. Treat the executor's own live `git status --porcelain` output, not this enumeration, as authoritative: if it differs (for example because an evidence filename's timestamp changed), stage exactly what it reports. If any reported path lies outside `docs/features/active/2026-09-02-ci-build-infra-debt-730/`, halt this task and report the discrepancy instead of staging it. Then run:
  ```powershell
  git add docs/features/active/2026-09-02-ci-build-infra-debt-730/
  git commit -m "fix: sanitize absolute host path and commit full review-and-remediation record (remediation cycle 1, round 2)

  Remove operator account name and absolute host path from
  research.2026-09-02T09-15.md line 8 and from remediation-plan.2026-09-02T23-56.md,
  replacing each occurrence with the <repo-root> placeholder / a non-identifying
  description per repository rule (no committed artifact may embed absolute host
  paths or the operator account name).

  Also commit the full feature-review-and-remediation record: plan.2026-09-02T08-57.md
  checkbox residue, the four feature-review artifacts (code-review, feature-audit,
  policy-audit, remediation-inputs, all dated 2026-09-02T23-47), this remediation
  plan, and its sanitization-verification evidence.

  Co-Authored-By: Claude Fable 5.1 <noreply@anthropic.com>
  Claude-Session: https://claude.ai/code/session_0195CL93aqgKF19nj9h6trbt"

  git status --porcelain
  ```
  Acceptance: the post-commit `git status --porcelain` output is empty (clean worktree), and the commit message references the remediation cycle, the specific rule being enforced, and the full set of files committed (not only two).

---

## Preflight Validation

PLANNER-INTERNAL-REVIEW: PASS

### Citation-to-Tree

CITATION-TO-TREE: PASS — All citations re-derived directly against current worktree state; round-1 citations re-verified and round-2 (this revision) citations newly derived (2026-09-03):

- `docs/features/active/2026-09-02-ci-build-infra-debt-730/research/research.2026-09-02T09-15.md` line 8 currently contains an absolute host path embedding the operator account name, embedded in backticks (root-relative form: `<repo-root>`; the literal value is not reproduced here per the no-absolute-host-path rule), confirmed by direct file read.
- `docs/features/active/2026-09-02-ci-build-infra-debt-730/plan.2026-09-02T08-57.md` carries P3-T9 task marked `[x]` (completed), with documented residual checklist metadata expected post-task-execution per plan's own note.
- Four `.log` files verified at canonical locations: `evidence/baseline/msbuild-analyzers-pre.log` (11878 lines), `evidence/baseline/msbuild-nullable-pre.log` (12030 lines), `evidence/qa-gates/msbuild-analyzers-post.log` (11906 lines), `evidence/qa-gates/msbuild-nullable-post.log` (11742 lines); all four re-counted directly in this pass and confirmed to contain zero occurrences of the operator account-name value.
- This plan document (`remediation-plan.2026-09-02T23-56.md`) was found, by direct grep in this pass, to reproduce the same absolute path/account-name literal at 4 locations (former lines ~30, ~43, ~77, ~121); all 4 are redacted in this revision, and a full-document re-grep after redaction confirmed no remaining occurrence of the account-name value anywhere in the document. The document does still contain the generic `C:\Users`/`C:/Users` prefix as sweep-instruction and citation prose (it necessarily names the pattern it instructs P0-T3 to search for); P0-T3 sub-item 2 excludes this document from its own generic-prefix sweep for that reason, while sub-item 1's account-name-value sweep still covers it with no exclusion.
- `docs/features/active/2026-09-02-ci-build-infra-debt-730/policy-audit.2026-09-02T23-47.md` (line ~127, table column header `` `C:\Users\` matches ``) and `docs/features/active/2026-09-02-ci-build-infra-debt-730/remediation-inputs.2026-09-02T23-47.md` (lines 49–50, sweep-description prose) both re-verified by direct read in this pass to reference the `C:\Users`/`C:/Users` search-pattern text only as methodology description, and to contain zero occurrences of the operator account-name value; both are out of scope for this remediation per its own scope-limiting directive.
- A live `git status --porcelain` could not be executed directly by the planner in this session (no shell tool was available); the untracked/modified file set asserted in P0-T4 was instead cross-checked via a full recursive directory listing of the feature folder, which returned exactly the 7 paths P0-T4 enumerates. P0-T4 itself re-derives the authoritative list via its own `git status --porcelain` call at execution time and treats that live output, not this cross-check, as authoritative.

### AC-Traceability

AC-TRACEABILITY: PASS — N/A: remediation cycle, no new acceptance criteria. This plan addresses an artifact-hygiene defect identified by feature-review in the approved delivery of issue #730, plus a preflight-identified defect in this plan document's own compliance with that same rule. The eight specification acceptance criteria (AC1–AC8) all PASS and are not re-evaluated by this remediation. The plan is scoped to two delivery-stabilization tasks: (1) remove the absolute host path from research documentation and from this plan document itself (R-1), (2) stage and commit the full review-and-remediation record (R-4).

### Scope-Boundary

SCOPE-BOUNDARY: PASS — All tasks are confined to documentation remediation and git hygiene:
- P0-T1: read and verify current state.
- P0-T2: edit research.md line 8 only; no change to any workflow file, `.csproj`, `packages.config`, `Directory.Build.props`, or `Directory.Build.targets`.
- P0-T3: sweep verification, scoped to the feature folder with a documented, justified carve-out (for the generic-prefix sub-item only) for two pre-existing out-of-scope files plus this plan document itself; no code execution, no toolchain re-run.
- P0-T4: stage and commit the full review-and-remediation record; no reformat, rebuild, or re-test.
- This revision additionally redacts 4 self-referential literal occurrences in this plan document's own Preflight Validation and Self-Review sections (P0-T1's acceptance block, P0-T3 sub-item 1, and the two sections' own citation text); no task boundary changes as a result.

Out-of-scope (explicitly excluded per directive): R-2 (sanitization workflow expansion), R-3 (archive-fragile path references), re-running msbuild or vstest gates, any property/package change, and editing `remediation-inputs.2026-09-02T23-47.md`, `policy-audit.2026-09-02T23-47.md`, `code-review.2026-09-02T23-47.md`, or `feature-audit.2026-09-02T23-47.md` (staged and committed as-is in P0-T4; not redacted).

### Citation

CITATION: docs/features/active/2026-09-02-ci-build-infra-debt-730/remediation-inputs.2026-09-02T23-47.md | section "R-1 — Remove the absolute host path from the research document (REMEDIATION REQUIRED)" and "R-4 — Commit the residual checklist edit (OPTIONAL)"

CITATION: docs/features/active/2026-09-02-ci-build-infra-debt-730/research/research.2026-09-02T09-15.md | line 8

CITATION: docs/features/active/2026-09-02-ci-build-infra-debt-730/plan.2026-09-02T08-57.md | line 145 (P3-T9 task and note on post-commit checklist residue)

CITATION: docs/features/active/2026-09-02-ci-build-infra-debt-730/remediation-plan.2026-09-02T23-56.md | former lines ~30, ~43, ~77, ~121 (this plan document's own pre-redaction literal occurrences, corrected in this revision)

CITATION: docs/features/active/2026-09-02-ci-build-infra-debt-730/policy-audit.2026-09-02T23-47.md | line ~127 (table column header referencing the search-pattern text, not an embedded absolute path)

CITATION: docs/features/active/2026-09-02-ci-build-infra-debt-730/remediation-inputs.2026-09-02T23-47.md | lines 49–50 (sweep-description prose referencing the search-pattern text, not an embedded absolute path)

### AC-Inventory

AC-INVENTORY: None. Remediation cycle; no new AC. Existing AC1–AC8 in spec.md all verified PASS by prior feature-review (policy-audit.2026-09-02T23-47.md, code-review.2026-09-02T23-47.md, feature-audit.2026-09-02T23-47.md).

### AC-Mapping

N/A (no new AC).

### Unresolved-Gaps

UNRESOLVED-GAPS: NONE

---

## SELF-REVIEW: RE-DERIVED THIS PASS

**Citations re-derived in this pass (remediation cycle 1, round 2):**

- `docs/features/active/2026-09-02-ci-build-infra-debt-730/research/research.2026-09-02T09-15.md` line 8: current state re-verified via direct Read tool call in this pass; confirmed to still contain one backticked absolute host path embedding the operator account name (literal value not reproduced here). Lines 7 and 9 re-read and confirmed as the correct, unchanged preamble/postamble.
- `docs/features/active/2026-09-02-ci-build-infra-debt-730/remediation-plan.2026-09-02T23-56.md` (this document): re-grepped in this pass for the operator account-name value and for the `C:\Users`/`C:/Users` literal; the 4 pre-existing account-name/full-path occurrences (former lines ~30, ~43, ~77, ~121) were located and redacted, and this pass's own edits were re-read end-to-end afterward to confirm zero remaining occurrences of the account-name value anywhere in the document, that no code fence, heading, or list nesting was corrupted by the redaction, and that the document's own remaining generic `C:\Users`/`C:/Users` mentions (sweep-instruction and citation prose naming the search pattern) are correctly self-excluded from P0-T3 sub-item 2's sweep for the same methodology-description reason as the two audit-artifact carve-outs.
- `docs/features/active/2026-09-02-ci-build-infra-debt-730/plan.2026-09-02T08-57.md`: P3-T9 task status and residual-checklist note re-verified via Read tool at line 145 and surrounding context; unchanged since round 1.
- `docs/features/active/2026-09-02-ci-build-infra-debt-730/policy-audit.2026-09-02T23-47.md` and `docs/features/active/2026-09-02-ci-build-infra-debt-730/remediation-inputs.2026-09-02T23-47.md`: re-read in this pass and confirmed to contain the `C:\Users`/`C:/Users` search-pattern text only as methodology description (table header; sweep-description prose), with zero occurrences of the operator account-name value, justifying their exclusion from P0-T3 sub-item 2's sweep.
- The four sanitized `.log` files: line counts re-counted directly in this pass (11878, 12030, 11906, 11742) and confirmed unchanged from the values sub-item 3 already asserted, with zero occurrences of the operator account-name value in any of the four.
- Feature-folder file listing re-derived in this pass via a full recursive directory enumeration (no live `git status --porcelain` was available to the planner this session): confirmed the file set P0-T4 enumerates matches what exists on disk; P0-T4 itself re-derives the authoritative tracked/untracked state via its own live `git status --porcelain` call at execution time.

**Sibling region check:**
- Research file lines 7–9: line 7 ("All evidence below...") and line 9 ("on branch") re-confirmed as correct preamble/postamble requiring no change.
- Plan file lines 144–146: P3-T8 (AC8 check-off) is separate and unaffected; P3-T9 is scoped to this remediation; no sibling invalidation.
- This document's own Context, PLANNER-INTERNAL-REVIEW, and Self-Review sections were re-read end-to-end after all redaction edits to confirm no sibling prose (headings, other citations, the AC-Traceability and Scope-Boundary text) retained a stale reference to the pre-redaction two-task framing or to the raw literal.
