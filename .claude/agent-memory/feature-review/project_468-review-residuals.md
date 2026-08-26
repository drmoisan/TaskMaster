---
name: 468-review-residuals
description: "#468 qfc-collection-controller-defects review (2026-08-26): PASS, 0 blocking; dual-floor coverage reporting pattern hook-verified; residuals #623 stale line count, AC-27/28 deferred to PR/default-merge"
metadata:
  type: project
---

Epic-child review of `bug/qfc-collection-controller-defects-468` vs `origin/epic/quickfiler-bug-family-integration` (merge base equaled the integration tip, so two-dot == three-dot). 27/29 AC PASS; AC-27 (PR accuracy) and AC-28 (issue closure) deferred by design — the branch merges to the INTEGRATION branch, so no closing reference registers; the seven issues (#286 #468 #469 #470 #471 #473 #474) close only at the integration-to-default merge.

**Coverage floor conflict resolution that passed the hook (pattern to reuse):** line rate 84.9435% sits between CLAUDE.md's 80% floor and the rules-files' 85%. Wrote TWO single-line C# rows — PASS vs the 80% floor, FAIL (pre-existing repo-wide, improved +0.1732 pp, non-blocking) vs the 85% floor — plus separate PASS rows for branch (78.9377%) and changed-line/new-file checks. Hook exit 0 from both cwds with `artifacts/csharp/coverage.xml` absent (caller forbade creating it; committed `<FEATURE>/evidence/` Cobertura served as the artifact per [[feature-evidence-cobertura-counts-as-coverage-artifact]]). Applied floor for the blocking decision: CLAUDE.md 80% (position 1 in the compliance order); contradiction tracked by #563.

**Residuals owed downstream:**
- Issue #623 (controller over 500-line cap) records baseline 2,349; post-feature the file is 2,437 (+88, spec-mandated docs/seams/guards; AC-25 prohibited splitting). Update #623 at merge time. Judged NON-BLOCKING because the only lawful remedy was out of the feature's contract.
- CR-1: `GetMoveDiagnostics` still reads `_itemGroupsToMove.Count` unguarded while `TryGetItemGroupByIndex` guards null — pre-existing shape, flag to the #623 decomposition owner.
- Three test files at 500/497/494 lines — future changes must extract first.

**Why:** the caller supplied three "known items" and asked for merits-based adjudication; all three survived scrutiny as non-blocking (file growth, doc-comment-only banned-API hits with two mandated by plan D9, dual-floor shortfall).

**How to apply:** for QuickFiler-family reviews, reuse the dual-floor row pattern verbatim; check whether #623/#563 remain open before repeating the dispositions; expect the stale-summary misclassification ([[pr-context-summary-misclassifies-cs]]) — this session's regenerated summary used correct `- path (+N/-N)` bullets so the hook detected CSharp properly.
