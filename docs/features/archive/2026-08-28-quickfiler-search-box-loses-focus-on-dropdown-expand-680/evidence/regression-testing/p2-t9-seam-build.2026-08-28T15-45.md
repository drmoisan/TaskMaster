# P2-T9 — Seam Build (declaration-only seam plus all new tests)

Timestamp: 2026-08-28T15-45

Command: `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`
(run with `/v:m`)

EXIT_CODE: 0

Output Summary:

- Build succeeded with 0 error lines.
- 5 warning lines, all the pre-existing `System.Reactive.PackagesConfigCheck.targets`
  `packages.config` advisory recorded at baseline. Warning count is unchanged from baseline, so the
  seam and the new tests introduce no diagnostic under warnings-as-errors.
- The two additive `IItemViewer` members (`SearchLeave`, `IsFolderDropDownOpen`) compile against
  every implementer in the solution; no implementer required a change.
- Newly compiled test files, both with explicit `<Compile Include>` entries in
  `QuickFiler.Test/QuickFiler.Test.csproj` (DR-2):
  - `QuickFiler.Test/Controllers/QfcItemController.SearchDismissalTests.cs`
  - `QuickFiler.Test/Viewers/ItemViewerSearchDismissalContractTests.cs`
- `QfcItemController.TextBoxSearch_Leave` is still an empty no-op body at this point, so the run in
  P2-T10 is a genuine runtime fail-before against no-op stubs rather than a compile failure.

Acceptance: satisfied — `EXIT_CODE: 0`.
