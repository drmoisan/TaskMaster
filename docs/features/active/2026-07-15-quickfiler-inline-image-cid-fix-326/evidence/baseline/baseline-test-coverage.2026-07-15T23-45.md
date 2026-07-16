# Baseline Full Test Pass with Coverage — P0-T10

- **Timestamp:** 2026-07-15T23-45
- **Command:** `vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /EnableCodeCoverage`
  (invoked in git-bash with `MSYS_NO_PATHCONV=1` and forward-slash relative DLL paths to avoid
  git-bash backslash-argument mangling; semantically identical command and identical assemblies.)
- **EXIT_CODE:** 0
- **Output Summary:** `Test Run Successful. Total tests: 4703. Passed: 4703. Failed: 0. Total time:
  48.7459 Seconds.` No pre-existing test failures at baseline.

## Coverage Conversion

The `.coverage` binary output
(`TestResults/65053df9-8568-497e-a4ea-806d85f5e840/DanMoisan_MEGALODON4_2026-07-15.23_32_41.coverage`)
was converted to Cobertura XML via
`Microsoft.CodeCoverage.Console.exe merge <file> --output <out>.cobertura.xml --output-format cobertura`
(Microsoft.CodeCoverage.Console v18.7.0.0) to extract numeric per-assembly line-coverage percentages.

Per-package (`<package line-rate="..." name="...">`) line-coverage rates relevant to this feature's
production files, read directly from the converted Cobertura XML:

- **`UtilitiesCS` (contains `IAttachment.cs`, `AttachmentSerializable.cs`, `MailItemHelper.Html.cs`,
  and the not-yet-created `CidImageResolver.cs`): line-rate = 0.88403943110843552 -> 88.40%**
- **`QuickFiler` (contains `QfcItemController.ViewerSetup.cs`): line-rate = 0.72515827140104594 ->
  72.52%**

These are the baseline figures for the Phase 4 (P4-T5) coverage-delta comparison.
