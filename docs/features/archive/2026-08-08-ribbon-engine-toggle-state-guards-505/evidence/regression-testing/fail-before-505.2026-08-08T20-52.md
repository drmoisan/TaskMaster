# P1-T4 [expect-fail] — Red Before the Fix (R1, R2, R3, R5)

Timestamp: 2026-08-08T20-52

Command:

```
pwsh -NoProfile -Command "Set-Location '<REPO>'; & '<VSTEST>' 'TaskMaster.Test\bin\Debug\TaskMaster.Test.dll' /InIsolation '/TestCaseFilter:FullyQualifiedName~TaskMaster.Test.Ribbon.RibbonViewerEngineCallbackShapeTests|FullyQualifiedName~TaskMaster.Test.Ribbon.EngineCommandCatalogTests'"
```

EXIT_CODE: **1** (non-zero — the expected red)

## Output Summary

`Test Run Failed.` Total tests: **24** — Passed: **13**, Failed: **11**, Skipped: 0.

### FAILED, with the pre-fix cause

| Test case | R# | Pre-fix cause |
|---|---|---|
| `ToggleGetPressedCallbacks_MatchOfficeCheckBoxGetPressedSignature` | R1 | `Expected type to be System.Boolean ... but found System.Threading.Tasks.Task`1[[System.Boolean...]]` — `SpamBayesEnabled_GetPressed` returns `Task<bool>`; Office cannot bind it. |
| `GetPressedCallbacks_BeforeSetGlobals_ReturnFalseWithoutThrowing` | R2 | `System.NullReferenceException` thrown from `RibbonViewer.<SpamBayesEnabled_GetPressed>d__89.MoveNext()` at `RibbonViewer.EngineCommands.cs:123` — the faulted task from dereferencing the null `Controller.Engines`. |
| `ToggleClickHandlers_AreAsyncVoidAwaitedShape` | R5 | `Expected handler.GetCustomAttribute<AsyncStateMachineAttribute>() not to be <null> because 'SpamBayesEnabled_Click' must await its work` — the handler is a plain `void` that discards the toggle `Task`. |
| `ShowSaveInfoHandlers_AreAsyncVoidAwaitedShape` | R5 | Same assertion for `GetSaveLocation_Click` — a plain `void` handler with no `await`. |
| `TryGetEngineName_ForEachEngineBackedControlId_ReturnsExpectedEngineName ("SpamSaveNetwork","Spam")` | R3 | The id is absent from `EngineCommandCatalog.Map`. |
| `... ("SpamSaveLocal","Spam")` | R3 | Same. |
| `... ("GetSaveState","Spam")` | R3 | Same. |
| `... ("TriageSaveNetwork","Triage")` | R3 | Same. |
| `... ("TriageSaveLocal","Triage")` | R3 | Same. |
| `... ("TriageGetSaveState","Triage")` | R3 | Same. |
| `ControlIds_ContainsExactlyTheFourteenEngineBackedControlIds` | R3 | `ControlIds` has 8 entries; the expected set has 14. |

That is 4 failing shape/behavior tests plus 6 failing catalog data rows plus 1 failing set-equality
test = **11 failures**, matching the reported count exactly.

### Expected pre-fix PASS

- `ToggleOnActionCallbacks_MatchOfficeCheckBoxOnActionSignature` — **PASSED**. The toggle
  `onAction` signatures (`void (IRibbonControl, bool)`) are already correct before the fix; this
  pin exists so the #506 rewrite cannot regress them, not to demonstrate a defect.

The remaining 12 passing cases are the eight pre-existing `EngineCommandCatalog` data rows and the
four unchanged null/empty/unknown/duplicate catalog tests.

## AC-15 red established

This artifact establishes the AC-15 "red before the fix" evidence for **R1, R2, R3, and R5**.
R4's red (the catalog-derived XML set-equality tests) is structurally reachable only after the
catalog is extended, and is captured separately at **P3-T5** in
`<FEATURE>\evidence\regression-testing\fail-before-r4-xml.<TS>.md`.

Binary outcome: PASS (expected failure observed and attributed).
