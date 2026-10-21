# Issue #609 Remediation Inputs

## Required correction

1. Trace and reproduce the initial Efc startup suggestion path from `FolderPredictor.FolderArray` with a persisted full Outlook value `\\mailbox@example.com\Archive\Clients\North`.
2. Add a deterministic MSTest regression in `UtilitiesCS.Test/OutlookObjects/Folder/FolderPredictorTests.cs` that fails before correction and proves the generated in-root row is `Clients\North`.
3. Keep the projected row text and matching `FolderScore` key aligned. Preserve already-relative values and out-of-root full values.
4. Apply the smallest archive-root-aware production correction at the startup generator boundary only if the test fails. Do not change `BreadcrumbBridgeRouter`, `EmailFilerConfig`, `EfcDataModel`, `EfcFormController`, persistence, COM, filesystem APIs, `Store.FilePath`, or mailbox `@` parsing unless the reproduction proves it essential.
5. Re-run the existing Issue #609 router and filer tests, then the complete C# QA and coverage loop. Check off all eight `spec.md` criteria only with direct evidence.

## Required evidence

- Fail-before and post-fix results under `evidence/regression-testing/`.
- Baseline or remediation-baseline and final QA evidence under canonical feature evidence folders.
- A coverage comparison with numeric repository and changed-file values.

## Do not do

- Do not treat the existing router/configuration tests as proof of the upstream producer.
- Do not silently normalize a full `DestinationOlStem`.
- Do not expand to unrelated source maps, search, recents, or persistence behavior.
- Do not add manual test steps, temporary files, external services, Outlook COM, or UI dependencies.
