# [P8-T3] #467 fail-before evidence — both menu mnemonics are swallowed

Timestamp: 2026-08-28T01-29
Task: [P8-T3] [expect-fail]
Command: `& "<resolved vstest.console.exe>" "QuickFiler.Test\bin\Debug\QuickFiler.Test.dll" /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation "/TestCaseFilter:FullyQualifiedName~ClaimsAltChord" "/Logger:trx;LogFileName=467-fail.trx" /ResultsDirectory:docs\features\active\efc-controller-surface-defects-464\evidence\regression-testing\p8-t3` under `pwsh -NoProfile` from the worktree root
EXIT_CODE: 1
ExpectedExitCode: 1

## Preceding intermediate build

Command: `& "<resolved MSBuild.exe>" "QuickFiler.Test\QuickFiler.Test.csproj" /t:Build /m /p:Configuration=Debug /p:Platform=AnyCPU /nologo /v:m`
EXIT_CODE: 0

## Result — TRX `<Counters>`, verbatim

```
total="5" executed="5" passed="3" failed="2" error="0" timeout="0" aborted="0"
inconclusive="0" passedButRunAborted="0" notRunnable="0" notExecuted="0" disconnected="0"
warning="0" completed="0" inProgress="0" pending="0"
```

Total executed: **5** (non-zero, per the non-vacuity rule). Failed: **2**.

## Enumerated result names and outcomes

| # | Result name | Outcome | Failure message (verbatim) |
|---|---|---|---|
| 1 | `ClaimsAltChord_WithBareAltAndHandler_ReturnsTrue` | Passed | — |
| 2 | `ClaimsAltChord_WithAltF_ReturnsFalse` | **Failed** | `Expected EfcViewer.ClaimsAltChord(handler.Object, Keys.Alt | Keys.F) to be False because Alt+F is the Filters menu mnemonic and must reach base.ProcessCmdKey, but found True.` |
| 3 | `ClaimsAltChord_WithAltM_ReturnsFalse` | **Failed** | `Expected EfcViewer.ClaimsAltChord(handler.Object, Keys.Alt | Keys.M) to be False because Alt+M is the Move Options menu mnemonic and must not be swallowed, but found True.` |
| 4 | `ClaimsAltChord_WithNonAltChord_ReturnsFalse` | Passed | — |
| 5 | `ClaimsAltChord_WithNullHandler_ReturnsFalse` | Passed | — |

The task requires `ClaimsAltChord_WithAltF_ReturnsFalse` and `ClaimsAltChord_WithAltM_ReturnsFalse` to
be `Failed`. Both are. **This is the fail-before evidence that both mnemonics are currently swallowed**:
the defect-preserving predicate is exactly the pre-change condition
`handler is not null && keyData.HasFlag(Keys.Alt)`, which claims every Alt-modified chord and returns
`true` before `base.ProcessCmdKey` is ever reached.

Rows 1, 4 and 5 pass before the correction because the defect is over-claiming, not under-claiming: bare
Alt is claimed correctly, a non-Alt chord is correctly not claimed, and a null handler correctly claims
nothing. Only the two mnemonic rows discriminate.

## Artifacts

TRX: `docs/features/active/efc-controller-surface-defects-464/evidence/regression-testing/p8-t3/467-fail.trx`,
sanitised (worktree path to `<repo-root>`, account to `<user>`, machine to `<host>`). The `/InIsolation`
`Deploy_*` scratch tree written into that directory was deleted.

Output Summary: EXPECT-FAIL SATISFIED. 5 executed, 2 failed, EXIT_CODE 1 against ExpectedExitCode 1. Both
mnemonic tests are red against the defect-preserving predicate, which claims every Alt-modified chord.
