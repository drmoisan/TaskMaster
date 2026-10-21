Timestamp: 2026-08-31T08:45:00-04:00
WhyFailingRunImpossible: Comment text and XML documentation carry no observable runtime behavior. No deterministic red state exists and no new test can fail before this change and pass after it.
Alternative Proof: `WriteMetricsAsync_FiltersNullDiagnosticLinesBeforeWriting`, `GetMoveDiagnostics_WithOneGroup_ReturnsExactlyOneLine`, `GetMoveDiagnostics_WithThreeGroups_ReturnsThreeLinesAndNoNulls`, and `GetMoveDiagnostics_WithNullItemController_ReturnsUnknownLineWithoutThrowing` remain the behavior guard; no test method was added, removed, or renamed.
SearchScope: FEATURE/evidence/regression-testing/
SearchPatterns: fail-before-exception.*.md
SearchResult: FEATURE/evidence/regression-testing/fail-before-exception.2026-08-29T12-22.md
