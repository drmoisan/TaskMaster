# Final QC — MSTest + Coverage (issue #211, Phase 3.4)

Timestamp: 2026-06-24T14-30
Command: `vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /EnableCodeCoverage /InIsolation /TestCaseFilter:"TestCategory!=LiveOutlook"`
(VS18 Community vstest.console.exe; /InIsolation per repo memory for Moq assemblies.)
EXIT_CODE: 0

Output Summary:
- Test Run Successful. Total tests: 3929; Passed: 3929; Failed: 0. Total time ~41.3 s.
- 13 new StoreFilterAttributionTests all pass (verified individually in P3-T7); 3916 pre-existing tests pass (no regression vs baseline count of 3916).
- Coverage merged to Cobertura via `dotnet-coverage merge -f cobertura`.

Coverage figures (post-change):
- Raw merged Cobertura overall line-rate (ALL assemblies): 0.59350 (59.35%) — vs baseline 0.59280 (59.28%). No regression; +0.07 pts.
- NEW CODE: `UtilitiesCS.OutlookObjects.Store.StoreFilterAttribution` = 100.00% (84/84 lines). Exceeds the >= 90% new-code floor.
- Target module `UtilitiesCS.OutlookObjects.Store.StoresWrapper` = 98.71% (307/311 lines), vs baseline 100% (221/221).
  - The 4 uncovered line instances (lines 152 and 165, each counted per coverage segment) are the two empty `catch { }` blocks in the new `ShouldIncludeStoreInstrumented` helper guarding the LIVE COM reads `store.DisplayName` and `store.FilePath`. These execute only when a live Outlook COM property access throws and are unreachable in deterministic unit tests (no live Outlook host). They fall under the CLAUDE.md COM/VSTO coverage exemption (Outlook-Interop-bound glue without an injectable seam) and were intentionally placed in the COM-bound StoresWrapper rather than the coverable helper, per the plan's decomposition mandate. The pure decision/format logic they wrap is fully covered (100%) in StoreFilterAttribution.

Acceptance: all tests pass; numeric post-change coverage recorded. New code (StoreFilterAttribution) = 100% >= 90%; no repo-wide regression (59.35% >= 59.28% baseline, both above the testable-denominator interpretation of the policy floor).
