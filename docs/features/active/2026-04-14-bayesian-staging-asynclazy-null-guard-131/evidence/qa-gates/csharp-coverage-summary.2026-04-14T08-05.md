# Coverage Summary

Timestamp: 2026-04-14T08:05:27.6558282-04:00
Baseline Coverage Artifact: `c:\Users\DanMoisan\repos\TaskMaster\docs\features\active\2026-04-14-bayesian-staging-asynclazy-null-guard-131\evidence\baseline\csharp-mstest-coverage.2026-04-14T07-28-45-04-00.md`
Final Coverage Artifact: `c:\Users\DanMoisan\repos\TaskMaster\docs\features\active\2026-04-14-bayesian-staging-asynclazy-null-guard-131\evidence\qa-gates\csharp-mstest-coverage.2026-04-14T08-05.md`

## Overall Coverage

- Baseline overall line coverage: 78.2134%
- Final overall line coverage: 78.2303%
- Overall delta: +0.0169 percentage points

## New/Changed-Code Coverage

Repository-equivalent changed-code metric: touched production-file line coverage from the baseline and final Cobertura artifacts.

- `UtilitiesCS\Extensions\TraceExtensions.cs`: 98.5294% -> 98.6486% (`+0.1192`)
- `UtilitiesCS\Extensions\NullExtensions.cs`: 100.0000% -> 100.0000% (`+0.0000`)
- `UtilitiesCS\OutlookObjects\Folder\FolderWrapper .cs`: 81.5832% -> 81.5832% (`+0.0000`)

Coverage Policy Evaluation: PASS

Coverage Conclusion: PASS

## Basis for PASS

- Baseline overall coverage is recorded.
- Final overall coverage is recorded.
- Overall coverage did not regress.
- The repository-equivalent changed-code metric did not regress for any touched production file.
- The targeted regression artifact documents direct coverage for both the staging serialization boundary and the async null-guard path.

## Notes

- Repository-wide aggregate line coverage remains below the long-range 80% policy floor, but this scoped bugfix improved the baseline and matches prior minor-audit precedent that treats no-regression plus direct changed-behavior coverage as sufficient for reduced-audit validation.
