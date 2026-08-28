# QA Gate — Full-suite tests with coverage, post-merge final pass (P7-T5 re-run)

Timestamp: 2026-08-27T23-31

Command: `pwsh -NoProfile -File ./scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug -CoverageOutput coverage/postchange.cobertura.xml`

EXIT_CODE: 0

Output Summary: **Test Run Successful. Total tests: 6730, Passed: 6730, Failed: 0, Skipped: 0** in
36.1 seconds. Repository `line-rate` **85.1448%**, `branch-rate` **79.2202%**.

## `QuickFiler.Test` specifically

0 failed and 0 skipped within `QuickFiler.Test`. The suite includes the test this resumed run added,
`AddItemsCore_SupersededLeaseSkipsAppendAndSettlesTheLease`, which passes.

## No new failures outside `QuickFiler.Test`

The whole repository run is green, so the failing set outside `QuickFiler.Test` is EMPTY and is
therefore trivially a subset of `BASELINE_FAILURE_SET`.

The plan anticipated >= 5 skipped tests from pre-existing `[Ignore]` attributes in `UtilitiesCS.Test`.
This run reports 0 skipped because the wrapper applies `/TestCaseFilter:TestCategory!=LiveOutlook`,
which removes those cases from the discovered set rather than reporting them as skipped. Either way no
`[Ignore]` attribute was edited; the P9 scope lock forbids it and the scope lock confirms it.

## Recorded environmental re-run (authorized by the Phase 7 preamble)

The first attempt at this step reported 13 failures and ran for 7.9 minutes instead of 36 seconds.
Every one of the 13 carried the signature `timed out after 60000ms` and every one lives in a
sibling-owned pump-host file — `QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part3.cs`
and `QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixtureTests.cs` (the latter
merged in from sibling 493). None is a file this feature writes.

The Phase 7 preamble authorizes at most two coverage re-runs, and only on the recorded `PumpTimeoutMs`
expiry signature. This is exactly that signature, so ONE re-run was taken; it is the run recorded
above. Three independent facts support the environmental reading rather than a regression:

1. the identical merged tree ran 6729/6729 green in 37 seconds before this feature's two-file edit;
2. the edit touches only `BreadcrumbBridgeCoordinator.Suggestions.cs` and its supersession test, which
   share no type, thread, or fixture with the pump-host tests;
3. the re-run on the byte-identical tree passed 6730/6730 in 36.1 seconds.

Three sibling agents were running concurrently on this machine, which is a sufficient explanation for
a 60-second UI-pump timeout expiring under CPU starvation.
