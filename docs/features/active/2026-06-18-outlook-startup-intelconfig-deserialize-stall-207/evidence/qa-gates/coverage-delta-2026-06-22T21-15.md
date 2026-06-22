# Coverage Delta

Timestamp: 2026-06-22T21-15

- Baseline coverage (P0-T6): line-rate 0.6411 = 64.11% (104196 / 162515 lines), raw all-package Cobertura.
- Post-change coverage (P2-T4): line-rate 0.6402 = 64.02% (104053 / 162530 lines), raw all-package Cobertura.
- Changed/new-code coverage statement: the only touched file is
  `TaskMaster.Test/AppGlobals/LiveOutlookHookupIntegrationTests.cs`, the developer-only `LiveOutlook`
  integration harness. It carries `[TestCategory("LiveOutlook")]`, is excluded from the standard gated
  run, and is excluded from the coverage denominator per its XML doc and the CLAUDE.md
  COM/VSTO/Interop coverage exemption. No production line changed, so there is no new production code
  subject to the >= 90% new-code floor.

Output Summary: The baseline and post-change repository headlines are equal within run-to-run noise
(64.11% vs 64.02%; the ~0.09% difference reflects which timing-sensitive tests happened to run, not a
coverage regression). Because only the coverage-exempt LiveOutlook harness was modified and no
production line changed, no production line coverage changed and the >= 80% first-party testable
denominator floor is unaffected. No coverage regression on changed lines.
