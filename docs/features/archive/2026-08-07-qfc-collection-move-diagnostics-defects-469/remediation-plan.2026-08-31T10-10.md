# Remediation Plan: Issue #469 command-evidence reconciliation

- Status: Ready for executor preflight
- Remediation pass: 3 of 3
- Work mode: full-bug evidence-only remediation
- Requirements source: `docs/features/active/2026-08-07-qfc-collection-move-diagnostics-defects-469/remediation-inputs.2026-08-31T10-07.md`
- Plan of record: `docs/features/active/2026-08-07-qfc-collection-move-diagnostics-defects-469/plan.2026-08-29T12-22.md`
- Current head to corroborate: `d69a572b2f1ce3d65866fd9e09c8028b55545ee7`

## Objective and constraints

Create current-head corroboration for exactly nine historical command-evidence artifacts. Eight
historical P5/P7 artifacts lack `Timestamp:` only; the historical P6-T9 clean-pass artifact lacks
`Timestamp:`, `Command:`, and `EXIT_CODE:`. The original artifacts remain unmodified historical
records. This plan authorizes only the explicitly named new Markdown evidence and audit records under this
feature folder, plus the canonical non-evidence PR-context files `artifacts/pr_context.summary.txt` and
`artifacts/pr_context.appendix.txt`. It does not authorize changes to source, tests, projects, configuration, policies,
the retained detached baseline worktree, the Git index, commits, pushes, merges, or GitHub state.

Every new command-evidence file named below must contain `Timestamp:` in ISO-8601 format, the exact
`Command:` that was executed, integer `EXIT_CODE:`, `Output Summary:`, `Corroborates:`, and
`CurrentHead:`. A current result may corroborate historical evidence only when it is expressly
identified as current-head verification; it must not claim to reconstruct the historical execution.

### Phase 0 — Policy and bounded-baseline capture

- [x] [P0-T1] Read `AGENTS.md` standing instructions, its cross-language code-change policy, its
  cross-language unit-test policy, and `.agents/skills/csharp/SKILL.md` in that order. Record the
  ordered list and a tone-policy acknowledgement in
  `docs/features/active/2026-08-07-qfc-collection-move-diagnostics-defects-469/evidence/remediation-baseline/phase0-policy-read.2026-08-31T10-10.md`.
  Acceptance: the record includes `Timestamp:`, `Policy Order:`, and `Distinct Files Read:`.

- [x] [P0-T2] Verify the immutable input scope without changing it: confirm these nine historical
  artifacts exist: `evidence/qa-gates/p5-t1-ac12-forbidden-file.2026-08-29T12-22.md`,
  `evidence/qa-gates/p5-t2-ac12-parameter-retained.2026-08-29T12-22.md`,
  `evidence/qa-gates/p5-t3-filter-retained.2026-08-29T12-22.md`,
  `evidence/qa-gates/p5-t4-ac8-file-sizes.2026-08-29T12-22.md`,
  `evidence/qa-gates/p5-t5-ac7-changed-line-classification.2026-08-29T12-22.md`,
  `evidence/qa-gates/p5-t6-ac9-testmethod-counts.2026-08-29T12-22.md`,
  `evidence/qa-gates/p6-t9-clean-pass.2026-08-29T12-22.md`,
  `evidence/qa-gates/p7-t15-no-closing-keyword.2026-08-29T12-22.md`, and
  `evidence/qa-gates/p7-t16-final-footprint.2026-08-29T12-22.md`; confirm the eight P5/P7 artifacts
  lack `Timestamp:`; confirm P6-T9 lacks all of `Timestamp:`, `Command:`, and `EXIT_CODE:`; and
  record `git rev-parse HEAD`. Write the results to
  `docs/features/active/2026-08-07-qfc-collection-move-diagnostics-defects-469/evidence/remediation-baseline/p0-t2-historical-gap-inventory.2026-08-31T10-10.md`.
  Acceptance: the inventory identifies exactly the nine target paths and records no non-target
  historical artifact as a remediation target.

### Phase 1 — Current-head command-evidence corroboration

- [x] [P1-T1] Re-run the read-only P5-T1 `git diff origin/main --name-only -- QuickFiler QuickFiler.Test docs`
  and `git status --porcelain -- QuickFiler QuickFiler.Test docs` commands. Write current results to
  `docs/features/active/2026-08-07-qfc-collection-move-diagnostics-defects-469/evidence/qa-gates/p1-t1-p5-t1-command-evidence-reconciliation.2026-08-31T10-10.md`.
  Acceptance: the forbidden `QuickFiler/Controllers/QfcFormController.EventHandlers.cs` path is absent
  from both results, and the record corroborates, without editing,
  `evidence/qa-gates/p5-t1-ac12-forbidden-file.2026-08-29T12-22.md`.

- [x] [P1-T2] Re-run the read-only case-sensitive `Select-String` count for `StackMovedItems` in
  `QuickFiler/Interfaces/IQfcCollectionController.cs`. Write the exact invocation and result to
  `docs/features/active/2026-08-07-qfc-collection-move-diagnostics-defects-469/evidence/qa-gates/p1-t2-p5-t2-command-evidence-reconciliation.2026-08-31T10-10.md`.
  Acceptance: the count is at least 2 and the record corroborates, without editing,
  `evidence/qa-gates/p5-t2-ac12-parameter-retained.2026-08-29T12-22.md`.

- [x] [P1-T3] Re-run the two read-only `Select-String` counts for `strOutput.Where(line` and
  `IsNullOrWhiteSpace(line)).ToArray();` in `QuickFiler/Controllers/QfcHomeController.Metrics.cs`.
  Write the exact invocations and results to
  `docs/features/active/2026-08-07-qfc-collection-move-diagnostics-defects-469/evidence/qa-gates/p1-t3-p5-t3-command-evidence-reconciliation.2026-08-31T10-10.md`.
  Acceptance: each count is exactly 1 and the record corroborates, without editing,
  `evidence/qa-gates/p5-t3-filter-retained.2026-08-29T12-22.md`.

- [x] [P1-T4] Re-run the five read-only `(Get-Content -LiteralPath <path>).Count` commands specified
  by P5-T4. Write exact commands and all values to
  `docs/features/active/2026-08-07-qfc-collection-move-diagnostics-defects-469/evidence/qa-gates/p1-t4-p5-t4-command-evidence-reconciliation.2026-08-31T10-10.md`.
  Acceptance: the values are no greater than 2446, 497, 215, and 453 for the first four plan paths,
  and exactly 499 for `QfcCollectionControllerTests.cs`; the record corroborates, without editing,
  `evidence/qa-gates/p5-t4-ac8-file-sizes.2026-08-29T12-22.md`.

- [x] [P1-T5] Re-run the read-only P5-T5 `git diff origin/main -- QuickFiler QuickFiler.Test` and
  `git diff origin/main --numstat -- QuickFiler QuickFiler.Test` commands, then classify each changed
  C# diff line using the plan-of-record prefixes. Write the exact commands, per-file numstat, and
  classification result to
  `docs/features/active/2026-08-07-qfc-collection-move-diagnostics-defects-469/evidence/qa-gates/p1-t5-p5-t5-command-evidence-reconciliation.2026-08-31T10-10.md`.
  Acceptance: 28 changed C# lines are all comments, XML documentation, or `because:` strings, and the
  record corroborates, without editing,
  `evidence/qa-gates/p5-t5-ac7-changed-line-classification.2026-08-29T12-22.md`.

- [x] [P1-T6] Re-run the two read-only `[TestMethod]` `Select-String` counts in the P5-T6 test files.
  Write exact invocations and results to
  `docs/features/active/2026-08-07-qfc-collection-move-diagnostics-defects-469/evidence/qa-gates/p1-t6-p5-t6-command-evidence-reconciliation.2026-08-31T10-10.md`.
  Acceptance: the counts are 9 and 11 and the record corroborates, without editing,
  `evidence/qa-gates/p5-t6-ac9-testmethod-counts.2026-08-29T12-22.md`.

- [x] [P1-T7] Re-run the read-only P7-T15 commit-message scan over `origin/main..HEAD`, including all
  nine closing-keyword tokens. Write the exact scan and individual counts to
  `docs/features/active/2026-08-07-qfc-collection-move-diagnostics-defects-469/evidence/qa-gates/p1-t7-p7-t15-command-evidence-reconciliation.2026-08-31T10-10.md`.
  Acceptance: every token count is 0 and the record corroborates, without editing,
  `evidence/qa-gates/p7-t15-no-closing-keyword.2026-08-29T12-22.md`.

- [x] [P1-T8] Re-run the read-only P7-T16 `git diff origin/main --name-only -- QuickFiler QuickFiler.Test docs`
  and `git status --porcelain -- QuickFiler QuickFiler.Test docs` commands. Write exact invocations,
  output classification, and the allowed-footprint verdict to
  `docs/features/active/2026-08-07-qfc-collection-move-diagnostics-defects-469/evidence/qa-gates/p1-t8-p7-t16-command-evidence-reconciliation.2026-08-31T10-10.md`.
  Acceptance: no `.csproj`, `.props`, `.targets`, `app.config`, `packages.config`, or coverage
  configuration path is attributable to the #469 deliverable footprint, and the record corroborates,
  without editing, `evidence/qa-gates/p7-t16-final-footprint.2026-08-29T12-22.md`.

- [x] [P1-T9] Reconcile the P6-T9 clean-toolchain declaration from the existing P6-T1 through P6-T7
  current artifacts without running formatter, build, or test commands. Write the exact read-only
  metadata-extraction command and the reconciled command/exit-code matrix to
  `docs/features/active/2026-08-07-qfc-collection-move-diagnostics-defects-469/evidence/qa-gates/p1-t9-p6-t9-command-evidence-reconciliation.2026-08-31T10-10.md`.
  Acceptance: the matrix names P6-T1 through P6-T7, preserves the documented P6-T2 baseline-relative
  non-zero result, records the AC10 four-step mapping, and corroborates, without editing,
  `evidence/qa-gates/p6-t9-clean-pass.2026-08-29T12-22.md`.

### Phase 2 — Audit reconciliation and terminal loop decision

- [x] [P2-T1] Re-run the read-only P6-T2 baseline/current CSharpier set comparison using existing
  evidence lists only; do not invoke CSharpier. Write the exact parsing/comparison command and both
  set-difference results to
  `docs/features/active/2026-08-07-qfc-collection-move-diagnostics-defects-469/evidence/qa-gates/p2-t1-p6-t2-set-reconfirmation.2026-08-31T10-10.md`.
  Acceptance: each list contains 35 configuration paths, both differences are empty, and no #469 C#
  path appears in either list.

- [x] [P2-T2] Invoke `mcp__drm-copilot__collect_pr_context` with
  `workspace_root: <current-workspace-root>` and `base: origin/main`; it may write only
  `artifacts/pr_context.summary.txt` and `artifacts/pr_context.appendix.txt`. Then delegate the
  fresh policy, code, and feature re-review of the complete `origin/main...HEAD` range to the
  repository `feature-review` workflow, using those refreshed PR-context files and the nine
  current-head corroboration records as inputs. Write the new audit outputs only to
  `docs/features/active/2026-08-07-qfc-collection-move-diagnostics-defects-469/policy-audit.2026-08-31T10-10.md`,
  `docs/features/active/2026-08-07-qfc-collection-move-diagnostics-defects-469/code-review.2026-08-31T10-10.md`,
  and `docs/features/active/2026-08-07-qfc-collection-move-diagnostics-defects-469/feature-audit.2026-08-31T10-10.md`.
  Write the cross-audit record to
  `docs/features/active/2026-08-07-qfc-collection-move-diagnostics-defects-469/audit-reconciliation.2026-08-31T10-10.md`.
  Acceptance: all six named outputs exist; each audit identifies `origin/main...HEAD` and the
  refreshed PR-context files as review inputs; the cross-audit record maps all nine historical
  artifacts to their current-head corroboration records; the independent CI format-check status is
  recorded separately from the command-metadata finding; and no historical audit or evidence artifact
  is modified.

- [x] [P2-T3] Make the terminal remediation-loop decision in
  `docs/features/active/2026-08-07-qfc-collection-move-diagnostics-defects-469/evidence/other/p2-t3-remediation-loop-decision.2026-08-31T10-10.md`.
  Acceptance: if the reconciliation finds no remaining review blocker attributable to missing command
  metadata, record `REMEDIATION_CLEARED`; if review remains blocking for any reason, record
  `REMEDIATION_LOOP_LIMIT_REACHED` with the exact blocker and stop without creating a fourth
  remediation plan. No manual action or repository mutation may be introduced.

## Execution boundary

This plan is evidence-only. The executor may create only the files explicitly named in this plan.
It must not modify historical evidence, source, tests, projects, configuration, policy files, Git
state, remote state, or the retained detached baseline worktree. Any command outside the stated
read-only commands is a plan violation.
