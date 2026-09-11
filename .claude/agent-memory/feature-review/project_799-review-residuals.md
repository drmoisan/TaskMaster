---
name: 799-review-residuals
description: "#799 breadcrumb-lineage review: PASS/0 blocking, 8/8 AC; a spec-authorised escalation whose precondition was verifiable in code; session-root pr_context belonged to a different cohort item"
metadata:
  type: project
---

Issue #799 (breadcrumb lineage below archive root) reviewed 2026-09-07: **PASS, 0 blocking, 7 non-blocking,
8/8 AC**. Artifacts at timestamp `2026-09-07T20-30`.

**Why:** three lessons here generalise beyond this issue.

**How to apply:**

1. **A "pre-authorised escalation" in a spec is checkable, not just quotable.** `spec.md` decision D-B said: take
   the Efc-only fallback *if* the QuickFiler presented row set turns out to be composed only inside a
   sibling-owned file. That precondition is a code fact. It held: the row list is the local `built` list in
   `FolderBreadcrumbBridgeRouter.SetSuggestionsAsync` lines 42-86, while `QfcItemController.FolderHandling.cs`
   lines 212/221 only hand `FolderArray`/`FolderRowArray` to the viewer before any provider resolution. Always
   evaluate the precondition rather than accepting that the deviation was "documented".
2. **A `Mock<IFace>(MockBehavior.Strict)` boundary is what makes a "no hunk needed" claim provable.** The #799
   trim lives in the concrete provider's `GetAncestorChainAsync`; every provider in both #439 router test files
   is a strict mock with `Setup(...GetAncestorChainAsync...).ReturnsAsync(chain)`, so the trim cannot reach them.
   Same mechanism makes a new capability interface inert in old tests: `provider as IFolderLabelAbsenceReport`
   yields null on a `Mock<IFolderHierarchyProvider>`, so suppression is a no-op there. Grep the mock construction
   lines to prove it; do not infer it from a passing suite.
3. **The session-root `artifacts/pr_context.summary.txt` can belong to a DIFFERENT cohort item.** In this
   parallel run it described `bug/quickfiler-crash-...-798` at `d00ab762`, not #799. Extends
   [[pr-context-stale-after-remediation-commit]]: check the *Head ref* line names YOUR branch, not just that the
   SHA is current. Its "Changed files overview" listed only `.md`/`.xml`, so `Get-ChangedLanguageSet` returned
   empty and `validate-feature-review-coverage.ps1` skipped every coverage-row check
   (see [[coverage-hook-skips-when-no-pr-context-summary]] — a foreign summary has the same effect as a missing
   one). Write clean explicit rows anyway.

**Residuals owed (all non-blocking, none remediation-required):**

- CR-1 (Low-Med): `FolderPredictor.AddRecents`/`AddRecentRows` now read `_globals.Ol.ArchiveRootPath`
  unconditionally. That getter *throws* `InvalidOperationException` (`AppOlObjects.cs:260-270`). Reachable when
  suggestions are empty but recents are not — `FolderArray` guards `AddSuggestions` on `Suggestions.Count > 0`
  separately. Ironic because the spec's whole justification for the provider's lazy `Func<string>` accessor was
  avoiding new throw sites for this exact property; the reasoning was not carried to the recents sites.
- CR-2 (Low): the additive AC6 score alias is appended immediately after its original, so a later genuine
  relative-keyed score for the same folder can be shadowed (`BuildProbabilityIndex` assigns through the indexer,
  last write wins). Fix is a second pass rather than interleaving.
- CR-3 (Low): the new AC2 chain-misses-root ERROR has no once-per gate, reintroducing in miniature the per-render
  log spam AC7 was written to fix.
- CR-4 (Low): Efc suppression reaches the WebView2 document but not the parallel `_folderRows` surface
  (`EfcFormController.cs:1107-1108`).
- CR-5/CR-6/CR-7 (Informational): comment says "three construction sites", only two production sites exist;
  relocated `[ExcludeFromCodeCoverage]`; the two committed follow-up promotions (wrapper relative-path loaders)
  have no receipt in `evidence/`.

**Coverage:** first-party index 84.55 -> 84.58 line, 79.24 -> 79.28 branch, `PACKAGES_MATCHED=9` both sides. Both
new types independently confirmed at `line-rate="1" branch-rate="1"` by grepping the class elements in
`artifacts/csharp/coverage.xml`. The raw root element reads 70.47/59.40 — the vendor-inflated denominator, see
[[csharp-repowide-coverage-below-80]]. The executor itself flagged its percentages as a *comparability index*,
not a de-duplicated per-line rate, which is the same trap as
[[cobertura-class-line-double-count-trap]]. 84.58 sits above CLAUDE.md's 80 and below `.claude/rules`' 85; see
[[build-ci-coverage-gate-fidelity-epic-outcome]].
