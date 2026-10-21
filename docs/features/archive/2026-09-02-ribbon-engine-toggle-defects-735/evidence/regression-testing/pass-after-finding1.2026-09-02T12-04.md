# Finding 1 — Pass-After Run (P1-T7)

Timestamp: 2026-09-03T01-42
Task: [P1-T7]
Command:

```
& $vstest TaskMaster.Test\bin\Debug\TaskMaster.Test.dll `
  /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation `
  "/TestCaseFilter:FullyQualifiedName~RibbonExplorerXml_EveryCallbackNameResolvesToAPublicRibbonViewerMethod|FullyQualifiedName~RibbonExplorerXml_CheckBoxOnActionCallbacksTakeControlAndPressedParameters" `
  "/Logger:trx;LogFileName=p1-t7.trx" `
  /ResultsDirectory:docs\features\active\2026-09-02-ribbon-engine-toggle-defects-735\evidence\regression-testing\p1-t7
```

EXIT_CODE: 0

## Results directory contents

Exactly one TRX file and no other entry:

```
p1-t7.trx
```

No MSTest deployment scratch directory was produced by this run. The one that appeared under the
P1-T2 results directory is created only on a failing run and was removed there.

## Counts read from the TRX `ResultSummary/Counters` element

| Counter | Value |
|---|---|
| total | 2 |
| executed | 2 |
| passed | 2 |
| failed | 0 |
| notExecuted | 0 |

## Per-test outcomes read from the TRX

| Test | Outcome |
|---|---|
| `RibbonExplorerXml_EveryCallbackNameResolvesToAPublicRibbonViewerMethod` | Passed |
| `RibbonExplorerXml_CheckBoxOnActionCallbacksTakeControlAndPressedParameters` | Passed |

## Fail-before / pass-after pair

| | Unresolved callback names | Unresolvable check-box callbacks |
|---|---|---|
| Pre-fix (P1-T2) | 5 | 4 |
| Post-fix (this run) | 0 | 0 |

The post-fix counts are zero by construction: the enumeration test's assertion is that the
unresolved set is empty, and it passed, so the count of callback names in the document that resolve
to no public instance method on the viewer type is zero. The same holds for the check-box arity
test. This pair is the evidence for F1-AC1, F1-AC5, F1-AC6 and F1-AC7.

Output Summary: Both Finding 1 tests pass after the CustomUI edit. EXIT_CODE 0, TRX counters total
2, passed 2, failed 0. The unresolved-callback count moved from 5 pre-fix to 0 post-fix and the
unresolvable check-box callback count from 4 to 0.
