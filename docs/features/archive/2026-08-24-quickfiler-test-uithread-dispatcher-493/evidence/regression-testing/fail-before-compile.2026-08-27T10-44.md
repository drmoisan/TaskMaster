# Fail-Before Compile Demonstration — Part 2 of the AC-10 Evidence (P1-T4) [expect-fail]

Timestamp: 2026-08-27T10-44
Task: [P1-T4]
Command: `& $MSBUILD TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
ExpectedExitCode: 1
EXIT_CODE: 1
Output Summary: With the fixture and the six regression tests in place but
`QfcItemControllerTestSupport.EnsureUiThreadDispatcher` still declared `void`, the analyzer msbuild
step fails as expected. Six log lines contain both the simple string
`QfcItemController.UiThreadDispatcherFixtureTests.cs` and the simple string `error CS`, carrying
three distinct `CS0029` diagnostics — one for each of R1, R2, and R3 — each reported twice by
MSBuild (once in the interleaved node output prefixed `9>`, once in the error summary).

FailBeforeErrorLineCount: 6

## The three distinct diagnostics (redacted)

```
<repo-root>\QuickFiler.Test\Controllers\QfcItemController.UiThreadDispatcherFixtureTests.cs(56,47): error CS0029: Cannot implicitly convert type 'void' to 'System.IDisposable' [<repo-root>\QuickFiler.Test\QuickFiler.Test.csproj]
<repo-root>\QuickFiler.Test\Controllers\QfcItemController.UiThreadDispatcherFixtureTests.cs(114,43): error CS0029: Cannot implicitly convert type 'void' to 'System.IDisposable' [<repo-root>\QuickFiler.Test\QuickFiler.Test.csproj]
<repo-root>\QuickFiler.Test\Controllers\QfcItemController.UiThreadDispatcherFixtureTests.cs(160,43): error CS0029: Cannot implicitly convert type 'void' to 'System.IDisposable' [<repo-root>\QuickFiler.Test\QuickFiler.Test.csproj]
```

At least one such line is quoted verbatim in redacted form, as the acceptance condition requires;
all three distinct diagnostics are quoted.

## Mapping the three diagnostics to the regression tests

| Source line | Test | Statement that cannot compile |
| --- | --- | --- |
| 56 | R1 `EnsureDispatcher_WhileATransactionHoldsALiveDispatcher_DoesNotReplaceIt` | `IDisposable ensureScope = QfcItemControllerTestSupport.EnsureUiThreadDispatcher();` |
| 114 | R2 `EnsureDispatcher_WhenTheFieldIsNull_InstallsAndRestoresOnDispose` | the same assignment |
| 160 | R3 `EnsureDispatcher_ScopeDisposedTwice_IsIdempotent` | the same assignment |

`CS0029: Cannot implicitly convert type 'void' to 'System.IDisposable'` is exactly the compile-level
fail-before signal spec § Test Strategy predicts: the helper returns `void` at the base branch, so a
test that binds its result to an `IDisposable` cannot build, and therefore no red *test run* for this
defect can exist.

## Why a failing exit code is the correct outcome here

This is the plan's only `[expect-fail]` task. The failure is the evidence: it demonstrates that the
regression tests are genuinely coupled to the fix rather than passing vacuously against the
unmodified helper. `P2-T3` records the pass-after counterpart, asserting `EXIT_CODE: 0` and zero
lines containing both of the same two simple strings.

The three diagnostics all name `QfcItemController.UiThreadDispatcherFixtureTests.cs`, the file this
plan created. No diagnostic names a file outside the Scope Lock, which is the condition `P1-T1`
preserved by deliberately not editing `QfcItemController.TestSupport.cs`.

Log path: `TestResults/plan-logs/p1-t4/msbuild-failbefore.log` (git-ignored; not committed).

## Companion artifact

`P0-T14` supplies part 1 of the AC-10 evidence — the verbatim pre-change source excerpt and the
`WhyFailingRunImpossible:` statement — at
`<FEATURE>/evidence/regression-testing/fail-before-exception.2026-08-27T10-27.md`.
