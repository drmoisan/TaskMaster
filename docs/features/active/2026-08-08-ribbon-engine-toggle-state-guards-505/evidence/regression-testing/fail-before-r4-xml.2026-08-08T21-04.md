# P3-T5 [expect-fail] — Red Before the XML Change (R4)

Timestamp: 2026-08-08T21-04

Command:

```
pwsh -NoProfile -Command "Set-Location '<REPO>'; & '<VSTEST>' 'TaskMaster.Test\bin\Debug\TaskMaster.Test.dll' /InIsolation '/TestCaseFilter:FullyQualifiedName~TaskMaster.Test.Ribbon.RibbonExplorerXmlTests'"
```

EXIT_CODE: **1** (non-zero — the expected red)

State of the tree at capture: `EngineCommandCatalog.Map` extended to 14 entries (P3-T4, built at
`/t:Build` EXIT 0 immediately before this run); `RibbonExplorer.xml` **not yet** updated. This is
the intended mid-change red that proves the catalog and the XML must land atomically.

## Output Summary

`Test Run Failed.` Total tests: **8** — Passed: **6**, Failed: **2**, Skipped: 0.

### FAILED

| Test | Failure message |
|---|---|
| `RibbonExplorerXml_EveryEngineBackedControlDeclaresGetEnabledCallback` | `Expected getEnabled not to be <null> because control 'SpamSaveNetwork' is engine-backed and must declare a getEnabled callback.` — the six new catalog ids carry no `getEnabled` attribute in the XML. |
| `RibbonExplorerXml_GetEnabledIsDeclaredOnlyOnEngineBackedControls` | `Expected declaringIds to contain exactly 14 items in any order because only the engine-backed controls may be disabled by the readiness callback, but it misses {"SpamSaveNetwork", "SpamSaveLocal", "GetSaveState", "TriageSaveNetwork", "TriageSaveLocal", "TriageGetSaveState"}` — set equality between the XML's `getEnabled` declarations (8) and `ControlIds` (14). |

Neither test's source was modified; both derive their expectations from
`EngineCommandCatalog.ControlIds`, which is exactly why extending the catalog turns them red until
the XML matches.

### PASSED — the load-bearing negative result

- `RibbonExplorerXml_EngineBackedControlsAreSchemaLegalForGetEnabled` — **PASSED**. Every one of
  the six newly catalogued ids resolves to a `button` element, so the schema-legality assertion
  holds. This confirms the design constraint that the two `checkBox` toggles must stay outside
  `EngineCommandCatalog`: had they been added, this test would have failed.
- `RibbonExplorerXml_GetEnabledCallbackMatchesOfficeSignatureOnRibbonViewer` — PASSED.
- The four structural ribbon tests (well-formedness, menu-legal children, Taskmaster tab grouping,
  `TabMail` emptiness) — PASSED.

## AC-15 red established

This artifact establishes the AC-15 "red before the fix" evidence for **R4**, completing the
red-before-green set begun in
`<FEATURE>\evidence\regression-testing\fail-before-505.2026-08-08T20-52.md` (R1, R2, R3, R5).

Binary outcome: PASS (expected failure observed and attributed).
