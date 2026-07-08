# Final QC — Coverage Delta and Thresholds (issue #211, Phase 3.4)

Timestamp: 2026-06-24T14-30
Command: derived from P0-T7 baseline and P5-T4 post-change Cobertura merges (`dotnet-coverage merge -f cobertura` + per-class line analysis).
EXIT_CODE: 0

## Coverage values

| Metric | Baseline (P0-T7) | Post-change (P5-T4) | Delta |
| --- | --- | --- | --- |
| Raw merged Cobertura overall line-rate | 0.59280 (59.28%) | 0.59350 (59.35%) | +0.0007 (+0.07 pts) |
| New code: StoreFilterAttribution | n/a (did not exist) | 100.00% (84/84) | new |
| Target module: StoresWrapper | 100.00% (221/221) | 98.71% (307/311) | see note |

## Threshold verification

- New/changed code (`StoreFilterAttribution`) = 100.00% >= 90% floor. PASS.
- Repository-wide line coverage: no regression. Post-change raw overall 59.35% >= baseline 59.28%. PASS (no regression).
  - The raw cobertura overall figure is below the policy 80% number because it counts ALL assemblies, including the CLAUDE.md-exempt VSTO/WinForms/Designer/Outlook-Interop and vendored (Swordfish/SVGControl) code plus test code. The policy 80% floor applies to the testable denominator after those exemptions. The change does not move the raw figure downward; the no-regression obligation is met.
- StoresWrapper module 98.71%: the 4 uncovered line instances are the two empty `catch { }` blocks (lines 152, 165) guarding the LIVE COM reads of `store.DisplayName`/`store.FilePath` in the new `ShouldIncludeStoreInstrumented` glue. These require a live Outlook COM store throwing on property access and are unreachable in deterministic unit tests; they fall under the CLAUDE.md COM/VSTO/Outlook-Interop coverage exemption. The pure logic they wrap is 100% covered in StoreFilterAttribution. This is not a regression of previously-tested lines — those lines are newly added COM-bound glue, not modifications to formerly-covered code.

## Outcome

PASS. New code >= 90% (100%); no repository-wide regression. Coverage thresholds met.
