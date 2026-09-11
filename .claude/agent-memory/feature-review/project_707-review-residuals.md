---
name: 707-review-residuals
description: narrow-fileio2-retryable-exception-set (#707) — PASS/0 blocking; executor self-corrected a stale merge-base, disclosed an AC-wording gap; catch-clause ordering pattern for future FileIO2/retry-loop reviews
metadata:
  type: project
---

Issue #707 (bug/narrow-fileio2-retryable-exception-set-707, reviewed 2026-09-03): PASS, 0 blocking
findings across policy-audit/code-review/feature-audit. Two-file additive fix (one
`catch (DirectoryNotFoundException ex)` block ahead of the existing `catch (IOException ex)` in
`FileIO2.WriteTextFileAsync`, plus one regression test). All 9 spec.md AC boxes independently
corroborated (8 unconditional PASS, AC9 PASS-with-disclosed-deviation).

**Self-corrected stale merge-base, not a caller narrowing attempt.** The plan's P0-T7 task ran a
bare `git merge-base HEAD main` inside a worktree whose local `main` lagged `origin/main` after a
same-day reconciliation merge (67c2e3b0 merging origin/main@87cb4df3 into the feature branch before
Phase 0 began). This resolved to a stale ancestor (687f15fb) and inflated later diffs to ~360-407
paths instead of the correct 50. Unlike prior cases in this line of work ([[project_stale-caller-merge-base]],
#244), here the *executor itself* caught and transparently disclosed the discrepancy in
`evidence/qa-gates/p7-t2-commit-verification.md`, independently re-derived the correct
reconciliation-relative diff, and reasoned explicitly that the stale-base diff was a safe superset
for a caller-absence check. One earlier AC-verification artifact (`p6-t8-ac8-caller-scope.md`,
timestamped before the discrepancy was caught) used the stale base without the disclosure note;
verified via `git merge-base --is-ancestor <stale> <correct>` (exit 0) that the stale-base diff is
a strict superset of the correct scope, so the AC's negative-match conclusion was unaffected. Lesson:
when a plan spans a same-day reconciliation merge, check every task that computes its own base ref
independently — a self-caught staleness in one place does not guarantee all sibling artifacts inherited
the fix.

**AC-literal-text gap, correctly disclosed rather than silently checked off.** spec.md's AC9 said
"vstest.console.exe against UtilitiesCS.Test with all tests green," but the full suite carries 17
pre-existing Deedle/F#-reflection `VerificationException` failures (dotnet-coverage IL-instrumentation
incompatibility) identical in both baseline and post-change runs. The executor's own evidence
(`p6-t9-ac9.md`) named the literal-text gap explicitly and grounded the check-off in the plan's
narrower task-level acceptance text instead of silently checking the box against the broader AC
prose. This reviewer independently cross-checked both 17-name failure lists (byte-identical) and
accepted the same PASS-with-disclosed-deviation disposition. Pattern worth reusing: when an AC's
literal wording is broader than what a plan's own tasks verify, look for (and credit) executor
self-disclosure of the gap rather than trusting only the checked box.

**Cobertura new-code delta line count can exceed the raw diff line count for async methods.**
The `catch (DirectoryNotFoundException ex)` block is 8 raw source lines, but the Cobertura-derived
new-code delta reported 14/14 new valid/covered lines. Evidence attributed this to
`Merge-CoberturaClassesByFilename` combining multiple compiler-generated async-state-machine classes
onto one source-file entry, producing more than one sequence point per source line. Baseline and
post-change both went through the identical transform, so the 100% new-code coverage conclusion held,
but the underlying per-class breakdown wasn't attached to make the 8-vs-14 gap independently
reproducible — flagged as a non-blocking traceability note, not a defect. Relevant if a future async
C# method's coverage delta looks larger than its diff and the file uses the same merge-by-filename
tooling.
