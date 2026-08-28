# [P7-T10] #465 D fail-before evidence — divergent-arity banner classification

Timestamp: 2026-08-28T01-22
Task: [P7-T10] [expect-fail]
Command: `& "<resolved vstest.console.exe>" "QuickFiler.Test\bin\Debug\QuickFiler.Test.dll" /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation "/TestCaseFilter:FullyQualifiedName~IsBannerRow_ClassifiesByTheFourCharacterPrefix|FullyQualifiedName~IsBannerRow_NullOrShortRow_ReturnsFalseWithoutThrowing|FullyQualifiedName~IsSelectableFolder_AndIsBannerRow_ClassifyThreeAndFourEqualsRowsIdentically" "/Logger:trx;LogFileName=465d-fail.trx" /ResultsDirectory:docs\features\active\efc-controller-surface-defects-464\evidence\regression-testing\p7-t10` under `pwsh -NoProfile` from the worktree root
EXIT_CODE: 1
ExpectedExitCode: 1

## Preceding intermediate build

Command: `& "<resolved MSBuild.exe>" "QuickFiler.Test\QuickFiler.Test.csproj" /t:Build /m /p:Configuration=Debug /p:Platform=AnyCPU /nologo /v:m`
EXIT_CODE: 0

## Result — TRX `<Counters>`, verbatim

```
total="3" executed="3" passed="1" failed="2" error="0" timeout="0" aborted="0"
inconclusive="0" passedButRunAborted="0" notRunnable="0" notExecuted="0" disconnected="0"
warning="0" completed="0" inProgress="0" pending="0"
```

Total executed: **3** (non-zero, per the non-vacuity rule). Failed: **2**.

## Enumerated result names and outcomes

| # | Result name | Outcome | Failure message (verbatim) |
|---|---|---|---|
| 1 | `IsBannerRow_ClassifiesByTheFourCharacterPrefix` | **Failed** | `Expected EfcFormController.IsBannerRow("===") to be False because a three-equals row is shorter than the producer prefix, but found True.` |
| 2 | `IsBannerRow_NullOrShortRow_ReturnsFalseWithoutThrowing` | **Failed** | `Did not expect any exception because a row of length 0 must not throw, but found System.ArgumentOutOfRangeException: Index and length must refer to a location within the string.` |
| 3 | `IsSelectableFolder_AndIsBannerRow_ClassifyThreeAndFourEqualsRowsIdentically` | Passed | — |

The task requires **at least** rows 1 and 2 to be `Failed`. Both are. This is the fail-before evidence
for the divergent-arity classification: the defect-preserving `IsBannerRow` reproduces the
three-character `Substring` test, so it misclassifies a three-equals row as a banner and throws
`ArgumentOutOfRangeException` on any row shorter than three characters.

## Why row 3 passes before the correction, and why that is expected here

Row 3 asserts `spec.md:977`: that a three-equals row and a four-equals row classify **identically at the
two EFC sites** — the creation path (`IsValidSelection`, now `IsSelectableFolder`) and the guard
expression `ActionOkAsync` composes. Merged sibling #614 already centralized both sites on
`QuickFiler/Controllers/EfcSelectionGuard.cs`, so both rows are already rejected at both sites on this
base. The base-drift addendum records exactly this: "The two EFC sites no longer diverge in arity; a
three-`=` row and a four-`=` row already classify identically at both sites (both rejected)."

Row 3 therefore pins a property that is already delivered upstream and must not regress, rather than one
this feature turns from red to green. It is recorded as `Passed` rather than presented as fail-before
evidence.

Note that the plan's `[P7-T9]` prose describes row 3 as asserting that `IsSelectableFolder` is "the
logical negation of `IsBannerRow`". That formulation is **not satisfiable on this base and is not what
`spec.md:977` requires**: after the correction, `IsSelectableFolder("===")` is `false` (because
`EfcSelectionGuard.IsValidCreationSelection` rejects a three-equals value under its own three-character
prefix) while `!IsBannerRow("===")` is `true`. The test asserts the criterion's actual property —
identical classification at the two sites — because `spec.md` is the acceptance-criteria source and wins
over the plan's paraphrase. The deviation is recorded here rather than absorbed.

The guard expression is reproduced rather than driven through `ActionOkAsync`, because `ActionOkAsync`
shows a `MessageBox` on the rejection path, which the headless test policy prohibits.

## Artifacts

TRX: `docs/features/active/efc-controller-surface-defects-464/evidence/regression-testing/p7-t10/465d-fail.trx`,
sanitised (worktree path to `<repo-root>`, account to `<user>`, machine to `<host>`). The `/InIsolation`
`Deploy_*` scratch tree written into that directory was deleted.

Output Summary: EXPECT-FAIL SATISFIED. 3 executed, 2 failed, EXIT_CODE 1 against ExpectedExitCode 1. Both
tests the task names as required-red are red: the defect-preserving `IsBannerRow` misclassifies a
three-equals row and throws on a short row. The third test pins an upstream-delivered non-regression
property and passes.
