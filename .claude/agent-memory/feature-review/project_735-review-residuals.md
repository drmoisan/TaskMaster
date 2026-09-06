---
name: 735-review-residuals
description: "#735 ribbon-engine-toggle-defects closed PASS / 0 blocking; residuals: a latent prime-marker TryRemove race outside _primeGate, an evidence test-count reconciliation that papered over an off-by-one, and one AC left open for a live-Outlook operator"
metadata:
  type: project
---

Review of `bug/ribbon-engine-toggle-defects-735` (head `30e66833`, base `b13d5b7b`) closed **PASS,
0 blocking, 24/25 AC**. Work mode `full-bug`, so `spec.md` was the sole AC source.

**Why keep this:** three residuals outlive the PR and one of them is a reusable review technique.

**How to apply:** when #735 or `TaskMaster/Ribbon/EngineToggleStateCoordinator.cs` comes back.

- **Latent race still open (recommended as a follow-up issue).** `CompletePrime` calls
  `_primeTasks.TryRemove` at `:348` *outside* `lock (_primeGate)`, while `StartPrimeIfNeeded`
  registers the marker *inside* it at `:276` — and `StartObservedPrime(...)` is fully evaluated
  (continuation already scheduled) before its result is assigned. When `ApplyPrimeAsync` never
  suspends, the continuation can `TryRemove` before the assignment lands, re-registering a marker for
  an already-failed prime and blocking re-prime for the session. Pre-existing; CR-2 made it reachable
  on one more path (cancellation). Not flaky for the shipped tests, which conclude from prime-handle
  identity rather than counts.
- **Evidence reconciliation covering an arithmetic error.**
  `evidence/qa-gates/vstest-coverage-run.md` counted "9 cache tests" (actual 10), then explained the
  resulting off-by-one by attributing the 27th test to a pre-existing baseline-filter miss, naming a
  real test. The delta was in fact 27 new tests exactly. **Technique:** when an evidence artifact
  explains a residual test-count delta, recount `[TestMethod]` at base and head
  (`git ls-tree`/`grep -c`) instead of accepting the narrative — a fabricated reconciliation reads
  exactly like a real one.
- **F2-AC8 open, correctly.** The Clear Spam Manager manual verification needs a live Outlook host;
  the executor recorded `ManualVerificationStatus: OPERATOR-ACTION-REQUIRED` with both observation
  fields blank rather than asserting a result. Accept this disposition; do not treat it as a
  remediation trigger. Same shape as the `fail-before-exception` dossier, which correctly declared a
  pre-fix failing run *structurally impossible* (modal dialog + message pump + disk) and substituted a
  named one-to-one mapping from three gate tests onto three null states.
- Coverage on this branch was healthy and independently recomputed: repo-wide 85.41%/79.50%; new
  `SpamManagerResetGate.cs` 100%/100%; new `EngineTogglePressedStateCache.cs` 94.87%/80% with the two
  uncovered lines being exactly the CAS retry paths (`:109`, `:127`), unreachable deterministically;
  modified `EngineToggleStateCoordinator.cs` 98.52% -> 100%. `RibbonController.Intelligence.cs` has
  zero measurable lines in both Cobertura documents because `RibbonController.cs:36` carries a
  pre-existing type-level exemption — same shape as [[storewrapper-controller-absent-from-cobertura]].
- Anchor trap encountered here is written up separately:
  [[three-dot-degenerates-when-base-is-ancestor]]. `artifacts/pr_context.*` in the worktree again
  described another item (#730), per [[pr-context-artifacts-are-tracked-not-gitignored]].
