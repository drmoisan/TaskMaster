# Baseline — SpamBayes.cs File Size and Exempt Status (issue #211)

Timestamp: 2026-06-24T15-10

Command: `wc -l UtilitiesCS/EmailIntelligence/ClassifierGroups/SpamBayes/SpamBayes.cs` and `grep -c "ExcludeFromCodeCoverage" UtilitiesCS/EmailIntelligence/ClassifierGroups/SpamBayes/SpamBayes.cs`

EXIT_CODE: 0

Output Summary:
- `SpamBayes.cs` line count: 705 lines.
- `[ExcludeFromCodeCoverage]` present: NO (grep returned 0 occurrences).
- CONSTRAINT-A overage: 705 - 500 = 205 lines OVER the repo 500-line ceiling BEFORE any edit.
- Resolution per plan: extract three self-contained regions into sibling partial-class files
  (`SpamBayes.Conditions.cs`, `SpamBayes.Actions.cs`, `SpamBayes.Classify.cs`) to bring
  `SpamBayes.cs` to <= 500 lines (expected ~402) before Phase 3 instrumentation.
