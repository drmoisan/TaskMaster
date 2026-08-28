# P6-T7 — #489 D1 standing regression verified, closed by citation with no work item

Timestamp: 2026-08-28T01-03
Command: & $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /InIsolation "/Logger:trx;LogFileName=p6-t7.trx" "/TestCaseFilter:FullyQualifiedName~MenuDropDown_ShowsMoveOptionsMenuThroughDispatcher" /ResultsDirectory:<temp>
EXIT_CODE: 0
ExpectedExitCode: 0

Ran: 1
Passed: 1
Failed: 0
Skipped: 0

## Acceptance

`Test Run Successful. / Total tests: 1 / Passed: 1`.

```
Passed MenuDropDown_ShowsMoveOptionsMenuThroughDispatcher [222 ms]
```

`ExpectedExitCode: 0` is the correct declaration here: P0-T13's `BaselineNamedPins:` block (in the
superseding `phase0-vstest-quickfiler.2026-08-28T00-14.md` artifact) records
`MenuDropDown_ShowsMoveOptionsMenuThroughDispatcher = passed` at baseline. No named test in that
block was recorded `failed`, so the conditional branch for a pre-existing sibling failure does not
apply and the absolute pass count governs.

## Why #489 D1 needs no work item

#489 D1 is closed by citation. The three cited facts were each re-verified against the current branch
head rather than taken from the plan text:

- `_uiDispatcher` is declared `private UtilitiesCS.Threading.IUiDispatcher _uiDispatcher;` at
  `QuickFiler/Controllers/QfcItemController.cs:66` — the plan's line number is exact.
- It is assigned at `QuickFiler/Controllers/QfcItemController.Initialization.cs:383`, which reads
  `_uiDispatcher ??= new UtilitiesCS.Threading.WpfUiDispatcher();` — the plan's line number is exact.
  The surrounding comment records that this is the single construction path every public constructor
  and both factory routes funnel through, and that a seam supplied via the constructor is preserved.
- `MenuDropDown()` is already covered by the test run above.

The UI-thread seam is therefore already `UtilitiesCS.Threading.IUiDispatcher`, the binding upstream
constraint this feature works under, and no third dispatch shape was introduced anywhere in Phase 6.
P6-T1's guard reuses the existing `_itemViewer.InvokeRequired`/`Invoke` pair rather than adding a new
mechanism.

## The test method body, verbatim, as it stands at this point

```csharp
        [TestMethod]
        public async Task MenuDropDown_ShowsMoveOptionsMenuThroughDispatcher()
        {
            var (controller, viewer) = BuildWithDispatcher(out var dispatcher);

            await controller.MenuDropDown();

            dispatcher.Verify(d => d.InvokeAsync(It.IsAny<Action>()), Times.Once());
            viewer.Verify(v => v.ShowMoveOptionsMenu(), Times.Once());
        }
```

The body occupies `QuickFiler.Test/Controllers/QfcItemController.SeamDispatcherTests.cs:99-107`, the
range the plan names, with the `[TestMethod]` attribute at `:98`.

## Byte-identity to `BASELINE_SHA`

`git diff --numstat <BASELINE_SHA> -- QuickFiler.Test/Controllers/QfcItemController.SeamDispatcherTests.cs`
produces **no output row**. The entire file, not merely the cited method, is byte-identical to its
state at `BASELINE_SHA`, which is a strictly stronger result than the acceptance condition requires.
This feature added no test to that file, and in particular did not duplicate the three theme-routing
tests it already contains.

The file's only permitted later change is the unrelated `AddFolderItems` invocation rename at `:193`,
made by P8-T7 inside a different test method. The
`MenuDropDown_ShowsMoveOptionsMenuThroughDispatcher` body at `:99-107` must remain byte-identical
through the end of the plan and is re-verified by P11-T7's full-assembly run.

## TRX artifact

`evidence/regression-testing/p6-t7.trx`, sanitised with the same case-insensitive, XML-entity
substitution scheme used throughout this batch. After redaction the file parses as XML, its
`<UnitTestResult>` count is **1** — matching the `Ran: 1` recorded above — its `ResultSummary`
counters read `total=1 passed=1 failed=0`, and a case-insensitive search for the account name, the
short 8.3 account name and the machine name returns **0** residual occurrences.

Output Summary: The #489 D1 standing regression passes. `Total tests: 1 / Passed: 1`, 0 failed,
`EXIT_CODE: 0` against `ExpectedExitCode: 0`, which is the correct expectation because P0-T13 records
this pin as `passed` at baseline. The three D1 citation facts were re-verified against the branch
head and all are exact: `_uiDispatcher` is `UtilitiesCS.Threading.IUiDispatcher` at
`QfcItemController.cs:66`, assigned at `QfcItemController.Initialization.cs:383`, and `MenuDropDown()`
is covered. The test method body at `:99-107` is recorded verbatim, and the entire containing file
shows no diff row against `<BASELINE_SHA>`, so it is byte-identical rather than merely equivalent.
