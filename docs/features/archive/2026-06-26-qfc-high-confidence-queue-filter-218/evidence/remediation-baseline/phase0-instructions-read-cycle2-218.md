# Phase 0 Instructions Read — Cycle 2 (Rebased Tree), Issue #218

Timestamp: 2026-06-28T17-31

Policy Order: CLAUDE.md -> .claude/rules/general-code-change.md -> .claude/rules/general-unit-test.md -> .claude/rules/csharp.md -> language/domain-specific (C#) -> review inputs and prior cycle artifacts.

Files read (in order):
1. CLAUDE.md (standing instructions; loaded into session context) — C# toolchain order, 80/90 coverage policy, COM/VSTO testable-denominator exemption.
2. .claude/rules/general-code-change.md (loaded into session context) — 500-line file limit, mandatory toolchain loop, fail-fast error handling.
3. .claude/rules/general-unit-test.md (loaded into session context) — coverage floors, AAA structure, no temp files, no external deps.
4. .claude/rules/csharp.md / C# Code Change Policy + C# Unit Test Policy sections of CLAUDE.md — CSharpier, MSTest, Moq, FluentAssertions, nullable.
5. .claude/skills/policy-compliance-order/SKILL.md (loaded into session context) — policy reading order, hard constraints (no policy edits).
6. .claude/skills/atomic-plan-contract/SKILL.md (loaded into session context) — plan format, Phase 0 evidence schema, final QA loop, no-SKIPPED rule.
7. .claude/skills/evidence-and-timestamp-conventions/SKILL.md (loaded into session context) — canonical evidence paths, ISO-8601 timestamps, machine-checkable schema.
8. docs/features/active/2026-06-26-qfc-high-confidence-queue-filter-218/remediation-inputs.2026-06-28T19-14.md — Ground-Truth Update (2026-06-28T19-34): maintainer commit 2637e4c1 did most of Finding 1; remaining work is test split completion, banned-API sweep, changed-line coverage, repo-wide exception.
9. docs/features/active/2026-06-26-qfc-high-confidence-queue-filter-218/policy-audit.2026-06-26T20-58.md — three findings (file size, repo-wide coverage, changed-line coverage isolation).
10. docs/features/active/2026-06-26-qfc-high-confidence-queue-filter-218/code-review.2026-06-26T20-58.md — no blocker/major code findings; PR readiness gated on policy.
11. docs/features/active/2026-06-26-qfc-high-confidence-queue-filter-218/feature-audit.2026-06-26T20-58.md — all 5 issue #218 AC PASS.
12. docs/features/active/2026-06-26-qfc-high-confidence-queue-filter-218/issue.md — Work Mode: minor-audit; 5 acceptance criteria, all checked.

Output Summary: All required policy, skill, and review inputs read in the mandated order. Confirmed Work Mode = minor-audit (AC source = issue.md only, 5 AC all already checked). Confirmed cycle-2 ground truth: production splits done by 2637e4c1 (verify-only), remaining work is completing the test split, banned-API sweep, changed-line coverage regeneration, and the authority-scoped repo-wide coverage exception. Hard constraints noted: no behavior change beyond mechanical, behavior-preserving completion of the split; original compiled test is canonical if a split copy diverged; no policy-file edits; no temporary files in tests; do not raise repo-wide coverage with out-of-scope tests. Banned-API set: DateTime.Now, DateTime.UtcNow, Random.Shared, Thread.Sleep, Task.Delay.
