# Phase 0 — AC23 Baseline (Cycle 4, #177 / INV-1)

Timestamp: 2026-06-16T10-26
Command: `vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /InIsolation /TestCaseFilter:"FullyQualifiedName~LcppnFolderPredictorStore|FullyQualifiedName~LcppnFolderPredictor_Serialization"`
EXIT_CODE: 0

## Cycle-3 workaround call site (INV-1 retention target)
- `UtilitiesCS/EmailIntelligence/ClassifierGroups/OlFolder/LcppnFolderPredictorStore.cs:63`
  `settings.ContractResolver = new DoNotSerializeContractResolver("Config");`
  Present and unchanged at baseline. This is the exclusion that must be RETAINED (not reverted).

## AC23 test pass status (pre-change baseline)
- Total 10, Passed 10, Failed 0.
- Includes `RoundTrip_WithDedicatedConfig_PreservesContentAndFileName` (the AC23 assertion that the
  serialized JSON excludes the runtime-only Config/Disk yet round-trips Version/BeamWidth/Nodes.Keys
  losslessly) and the `LcppnFolderPredictor_Serialization_Tests` round-trip suite.

Output Summary: `DoNotSerializeContractResolver("Config")` present at LcppnFolderPredictorStore.cs
line 63. AC23 tests green (10/10) before any change. Establishes the AC23-green baseline for INV-1
re-verification in Phase 3.
