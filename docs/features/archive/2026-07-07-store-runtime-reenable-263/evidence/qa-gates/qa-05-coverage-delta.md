# QA Gate 05 — Coverage Delta (P6-T5)

Timestamp: 2026-07-08T01-27

Comparison of P0-T13 baseline against P6-T4 post-change.

(a) No regression on previously-covered lines:
- Baseline overall (2-assembly, ci runsettings-style): 61.94% (102707 / 165825).
- Post-change overall (2-assembly): 62.12% (103436 / 166510).
- Delta: +0.18 pp. Coverage INCREASED; no regression on previously-covered lines. The added lines are the well-covered F3 code; no previously-covered production line lost coverage.
- Verdict: PASS.

(b) New-code coverage on the F3 decision logic >= 90%:
- New decision logic (StoreRehookCoordinator, AddOrRestoreStore, AppEvents inbox idempotency guards, sink folder idempotency guards, IsReady(Store), StoreRehookResult): ~252/253 = 99.6%.
- All touched files aggregate: 94.9%.
- Verdict: PASS (>= 90%).

(c) Repository line coverage for the testable denominator >= 80%:
- First-party PRODUCTION aggregate across all 7 *.Test.dll (ci.yml style, per the delegation's "measure by running ALL *.Test.dll" instruction): 83.23% (87889 / 105600), before COM/VSTO/WinForms attribute exemptions.
- Verdict: PASS (>= 80%).

All three checks PASS. Plan outcome for coverage: PASS (not remediation-required).
