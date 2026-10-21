# Phase 0 — Instructions Read (Remediation R-1, Issue #752)

- Timestamp: 2026-09-03T23-40
- Task: `[P0-T1]`
- Plan: `docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/remediation-plan.2026-09-03T12-23.md`

## Policy Order

Read in the exact order mandated by `[P0-T1]`, which follows the reading order in
`policy-compliance-order` (CLAUDE.md first, then the cross-language rules, then the
language-specific rule for the only script this loop touches, then the hygiene memory record,
then the remediation input that is the authoritative requirements source for this loop):

1. `CLAUDE.md`
2. `.claude/rules/general-code-change.md`
3. `.claude/rules/general-unit-test.md`
4. `.claude/rules/quality-tiers.md`
5. `.claude/rules/tonality.md`
6. `.claude/rules/powershell.md`
7. `.claude/agent-memory/_shared_no_absolute_host_paths.md`
8. `docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/remediation-inputs.2026-09-03T12-23.md`

## Files read

- `CLAUDE.md` — standing project instructions: policy compliance order, C# toolchain order, tone policy.
- `.claude/rules/general-code-change.md` — cross-language code change policy. The File Size Limit
  section (lines 47-50) carries at line 50 the first named exception, for temporary throwaway scripts
  created and deleted within an agent session; that exception is the recorded basis for the sweep
  helper created in `[P0-T2]` and deleted in `[P2-T5]` step 9.
- `.claude/rules/general-unit-test.md` — cross-language unit test policy, including the uniform
  line-coverage floor and the Coverage Exclusion Policy.
- `.claude/rules/quality-tiers.md` — T1-T4 tier system and the uniform-versus-tier-dependent gate matrix.
- `.claude/rules/tonality.md` — required professional tone for every artifact this loop writes.
- `.claude/rules/powershell.md` — PowerShell toolchain (PoshQC format, PoshQC analyze, Pester) and
  coding standards. Recorded for completeness; this remediation modifies no `.ps1` file that enters
  the change set, so those gates have an empty input set (classified in `[P2-T1]`).
- `.claude/agent-memory/_shared_no_absolute_host_paths.md` — the rule R-1 was raised under. Verified
  against this execution worktree: the file is 92 lines long; the prohibition is at lines 8-13; the
  required-placeholder table is at lines 17-26; the bullet beginning "A sanitisation record must not
  quote" is at line 88 and runs to line 92. These line citations were re-derived against this
  worktree rather than carried over, per hygiene rule 2 of the plan.
- `docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/remediation-inputs.2026-09-03T12-23.md`
  — the authoritative requirements source for this loop: one blocking finding (R-1 from POL-2), six
  findings recorded for disposition only, and the four required-remediation steps.

## Verification

All eight paths listed above were read in full before any Phase 1 edit was made.
