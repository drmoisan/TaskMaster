# Phase 0 Instructions Read (Cycle 2) — Issue #218

Timestamp: 2026-06-28T15-34

Policy Order:
1. `CLAUDE.md` (standing instructions, always loaded)
2. `.claude/rules/general-code-change.md` (cross-language code change policy)
3. `.claude/rules/general-unit-test.md` (cross-language unit test policy)
4. `.claude/rules/csharp.md` (C#-specific toolchain and standards)
5. `.claude/skills/policy-compliance-order/SKILL.md`
6. `.claude/skills/atomic-plan-contract/SKILL.md`
7. `.claude/skills/evidence-and-timestamp-conventions/SKILL.md`

Files Read:
- `CLAUDE.md`
- `.claude/rules/general-code-change.md`
- `.claude/rules/general-unit-test.md`
- `.claude/rules/csharp.md`
- `.claude/skills/policy-compliance-order/SKILL.md`
- `.claude/skills/atomic-plan-contract/SKILL.md`
- `.claude/skills/evidence-and-timestamp-conventions/SKILL.md`
- `.claude/skills/acceptance-criteria-tracking/SKILL.md`
- `docs/features/active/2026-06-26-qfc-high-confidence-queue-filter-218/remediation-inputs.2026-06-28T19-14.md`
- `docs/features/active/2026-06-26-qfc-high-confidence-queue-filter-218/policy-audit.2026-06-26T20-58.md`
- `docs/features/active/2026-06-26-qfc-high-confidence-queue-filter-218/code-review.2026-06-26T20-58.md`
- `docs/features/active/2026-06-26-qfc-high-confidence-queue-filter-218/feature-audit.2026-06-26T20-58.md`
- `docs/features/active/2026-06-26-qfc-high-confidence-queue-filter-218/evidence/qa-gates/line-count-remediation-blocker-218.md`
- `docs/features/active/2026-06-26-qfc-high-confidence-queue-filter-218/evidence/qa-gates/changed-line-coverage-218.md`
- `docs/features/active/2026-06-26-qfc-high-confidence-queue-filter-218/issue.md`

Output Summary:
- All required policy files and cycle-entry review artifacts were read in the prescribed order.
- Cycle 2 scope confirmed: bring three oversized touched files under the 500-line limit via behavior-preserving extraction — `QfcDatamodel.cs` (790), `QfcHomeController.cs` (739), `QfcHomeControllerTests.cs` (1370).
- Three blocking findings carried into cycle 2: Finding 1 (file-size FAIL, expanded user-approved extraction), Finding 2 (changed-production-line coverage must be isolated as a numeric percentage, PARTIAL), Finding 3 (repo-wide coverage 62.04% below raw 80% — authority-scoped exception, not in-scope uplift).
- Hard constraints noted: no behavior change beyond mechanical extraction; preserve `IQfcDatamodel` and home-controller public surfaces, cancellation propagation, `ConfigureAwait(false)`, and logging; preserve every existing test name/assertion; no policy-file edits; no temporary files in tests; do not raise repo-wide coverage with out-of-scope tests.
- Issue #218 acceptance criteria (5 of 5) are checked and pass; they must not be weakened.
- Banned-API set (BannedSymbols.txt): `DateTime.Now`, `DateTime.UtcNow`, `Random.Shared`, `Thread.Sleep`, `Task.Delay`.
- Toolchain order confirmed: CSharpier check -> analyzer build -> nullable build -> MSTest coverage; restart from CSharpier on any change/failure.
