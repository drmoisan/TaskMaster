# Test-Stack and Isolation Audit — hierarchical-lcppn-folder-prediction (#177)

- Timestamp: 2026-06-12T15-26 (UTC)
- Scope: all new and modified test files delivered by this plan (AC17).

## Files audited

| Test file | MSTest | Moq | FluentAssertions | Temp files | External deps |
|---|---|---|---|---|---|
| `EmailIntelligence/Bayesian/IFolderPredictor_Tests.cs` | yes | n/a | yes | none | none |
| `EmailIntelligence/Bayesian/BayesianClassifierGroup_FlatPathUnchanged_Tests.cs` | yes | n/a | yes | none | none |
| `EmailIntelligence/Bayesian/FolderHierarchyTree_Tests.cs` | yes | n/a | yes | none | none |
| `EmailIntelligence/Bayesian/PerParentClassifier_Tests.cs` | yes | n/a | yes | none | none |
| `EmailIntelligence/Bayesian/LcppnFolderPredictor_Tests.cs` | yes | yes | yes | none | none |
| `EmailIntelligence/Bayesian/LcppnFolderPredictor_Serialization_Tests.cs` | yes | n/a | yes | none | none |
| `EmailIntelligence/FolderPredictorSeam_Tests.cs` | yes | yes | yes | none | none |
| `EmailIntelligence/Evaluation/FolderPredictorEvaluator_Tests.cs` | yes | yes | yes | none | none |

## Findings

- Framework: every file uses `Microsoft.VisualStudio.TestTools.UnitTesting` (MSTest) with
  `[TestClass]`/`[TestMethod]`/`[DataTestMethod]`. No xUnit or NUnit.
- Mocking: Moq is used where an external boundary must be isolated — `IApplicationGlobals` /
  `IAppAutoFileObjects` (seam tests), `IFolderWrapper` (evaluator/Build tests, with only
  `RelativePath` configured so no Outlook `MAPIFolder` is touched), and `IFolderPredictor`
  (abstention/wrong-prediction accounting).
- Assertions: FluentAssertions is the assertion library throughout.
- Isolation: a grep for `Path.GetTempFile`, `File.Create`, `File.WriteAllText`, `new FileStream`,
  `Directory.CreateDirectory`, `HttpClient`, `WebRequest`, and `Process.Start` across the new test
  files returns no matches. No temporary files are created; no network, filesystem, or external
  process is used.
- Determinism: all tests are in-memory and order-independent; serialization tests use in-memory
  JSON round-trips; the evaluator split is index-based (deterministic). The feature suite
  (77 tests across Phases 1-7) passes consistently across repeated full-suite runs.
- No Outlook COM: the new prediction/evaluation production namespaces contain no
  `Microsoft.Office.Interop.Outlook` types; the tests construct no Outlook objects.

## Pre-existing flaky test (out of scope)

`AddEntry_UseUiThreadTrue_DequeuesEntryAndSuppressesDispatcherException` (a UI-thread/dispatcher
test outside this feature) intermittently fails under full-suite parallel load and passes in
isolation. It is unrelated to this feature and is the subject of the active
`ci-flaky-test-isolation-176` work. It does not affect this feature's tests or coverage collection.

## Verdict

AC17 satisfied: all new tests use MSTest + Moq + FluentAssertions, are independent, deterministic,
create no temporary files, and depend on no external services.
