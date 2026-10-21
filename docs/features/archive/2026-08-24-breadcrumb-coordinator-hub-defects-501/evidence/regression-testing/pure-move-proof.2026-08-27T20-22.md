# Pure-Move Proof — SR-1 Partial Split Is Behaviour-Neutral (P2-T4)

Timestamp: 2026-08-27T20-22

Command:

```
& $vstest 'QuickFiler.Test\bin\Debug\QuickFiler.Test.dll' /InIsolation
    '/Settings:scripts\vscode\TaskMaster.cli.runsettings'
    '/TestCaseFilter:FullyQualifiedName~BreadcrumbBridgeCoordinatorTests|FullyQualifiedName~BreadcrumbBridgeCoordinatorProbabilityTests|FullyQualifiedName~BreadcrumbCoordinatorLifecycleTests|FullyQualifiedName~BreadcrumbSelectorCoordinatorTests'
    '/Logger:trx;LogFileName=p2-t4.trx'
    '/ResultsDirectory:docs/features/active/breadcrumb-coordinator-hub-defects-501/evidence/regression-testing/trx/p2-t4'
```

EXIT_CODE: 0

Output Summary:

```
Test Run Successful.
Total tests: 42
     Passed: 42
 Total time: 2.3426 Seconds
```

| Metric | Value |
| --- | ---: |
| Total | 42 |
| Passed | 42 |
| Failed | 0 |
| Skipped | 0 |

## Why this is a pure-move proof

P2-T1 relocated `SetSuggestions`, the `SuggestionsUpgrade` property, `PopulateSuggestionsAsync` and
`AddItems` from `QuickFiler/Viewers/BreadcrumbBridgeCoordinator.cs` into the new partial part
`QuickFiler/Viewers/BreadcrumbBridgeCoordinator.Suggestions.cs` with no behavioural edit: every moved
member body is byte-identical to its pre-move text. The only textual difference introduced anywhere in
the moved code is the new file's own header (`#nullable enable`, usings, namespace, doc comment,
partial-class declaration).

The four test classes selected here are exactly the classes that exercise those four members: the
research document's sections 3.7 and 3.8 enumerate 20-plus call sites of `SetSuggestions` and
`AddItems` across them. All 42 tests pass unchanged, so the split altered no observable behaviour. This
is the state the #502 call-site change in Phase 4 will be applied on top of.

TRX artifact: `FF/evidence/regression-testing/trx/p2-t4/p2-t4.trx`, post-processed so it carries no
absolute host path, no account name and no machine name.

Acceptance: `EXIT_CODE: 0`, 0 failed, 0 skipped, and a passed count greater than 30 (42). PASS.
