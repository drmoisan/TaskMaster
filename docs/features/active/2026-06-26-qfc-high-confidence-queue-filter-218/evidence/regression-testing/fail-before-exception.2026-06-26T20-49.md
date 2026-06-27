Timestamp: 2026-06-26T20-49
WhyFailingRunImpossible: The fail-before command for `RunAsync_HighConfidenceEnabled_DoesNotPreFilterInitialGuiBatch` was not captured before implementation edits were applied. Reverting production files solely to manufacture a failing run would modify the workspace outside the approved forward-only execution path.
Alternative Proof:
- Pre-change `QfcHomeController.RunAsync` contained the high-confidence branch that invoked `HighConfidencePreFilterLoader` and loaded `IList<QfcPreScoredItem>`.
- The new regression test asserts the opposite behavior: no prefilter invocation and plain `IList<MailItem>` loading.
- The pre-change branch is visible in the working diff removal from `QuickFiler/Controllers/QfcHomeController.cs`.
SearchScope: docs/features/active/2026-06-26-qfc-high-confidence-queue-filter-218/evidence/regression-testing
SearchPatterns: fail-before-exception.*.md; runasync-highconfidence-initial-load-expect-fail-218.md
SearchResult: fail-before-exception.2026-06-26T20-49.md
