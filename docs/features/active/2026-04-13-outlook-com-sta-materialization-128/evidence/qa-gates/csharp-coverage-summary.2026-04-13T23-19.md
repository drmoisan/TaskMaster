# Coverage Summary

Timestamp: 2026-04-13T23-19
Baseline Coverage Artifact: `docs/features/active/2026-04-13-outlook-com-sta-materialization-128/evidence/baseline/csharp-mstest-coverage.2026-04-13T22-58.md`
Final Coverage Artifact: `docs/features/active/2026-04-13-outlook-com-sta-materialization-128/evidence/qa-gates/csharp-mstest-coverage.2026-04-13T23-19.md`

## Overall Coverage

- Baseline overall line coverage: 78.1782%
- Final overall line coverage: 78.2134%
- Overall delta: +0.0352 percentage points

## New/Changed-Code Coverage

Repository-equivalent changed-code metric: touched production-file line coverage from the baseline and final Cobertura artifacts.

- `UtilitiesCS/EmailIntelligence/EmailParsingSorting/EmailDataMiner.cs`: 89.8305% -> 90.2883% (`+0.4578`)
- `UtilitiesCS/OutlookObjects/MailItem/MailItemHelper.cs`: 82.9496% -> 82.9741% (`+0.0245`)
- `UtilitiesCS/OutlookObjects/Recipient/RecipientStatic.cs`: 83.4430% -> 83.7972% (`+0.3542`)

Coverage Policy Evaluation: PASS

Coverage Conclusion: PASS

## Basis for PASS

- Baseline overall coverage is recorded.
- Final overall coverage is recorded.
- Overall coverage did not regress.
- The repository-equivalent changed-code metric for each touched production file did not regress.
- The added regression tests exercised the new STA materialization path and the COM-safe sender/recipient fallbacks.
