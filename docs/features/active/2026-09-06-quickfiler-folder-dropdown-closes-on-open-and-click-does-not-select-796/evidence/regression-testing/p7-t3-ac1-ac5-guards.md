# P7-T3 — AC1 and AC5 managed-seam guard run

Timestamp: 2026-09-07T14-52
Task: [P7-T3]
Issue: #796
Channel used: A

Command:

```
pwsh -NoProfile -Command '$vswhere = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"; $vstest = & $vswhere -latest -products * -find "Common7\IDE\Extensions\TestPlatform\vstest.console.exe" | Select-Object -First 1; & $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation "/TestCaseFilter:FullyQualifiedName~BreadcrumbDropDownCloseOrderingTests|FullyQualifiedName~BreadcrumbPendingOpenCloseTests" /ResultsDirectory:TestResults\796\p7-t3 "/Logger:trx;LogFileName=p7-t3.trx"; "EXIT_CODE=$LASTEXITCODE"'
```

EXIT_CODE: 0

Raw results (gitignored, never committed): TestResults/796/p7-t3/p7-t3.trx

## Run summary

```
Test Run Successful.
Total tests: 11
     Passed: 11
```

The combined Total is 11, which is the figure this gate requires: the 4 `[TestMethod]` members
BreadcrumbDropDownCloseOrderingTests holds after task P7-T1, plus the 7
BreadcrumbPendingOpenCloseTests holds after task P7-T2.

vstest.console.exe prints one combined Total for a multi-class filter and prints no per-class
subtotal. The two per-class figures below were therefore DERIVED, by attributing each recorded
per-test result line to the class that declares it, rather than read from the runner.

| Class | Derived count |
|---|---|
| BreadcrumbDropDownCloseOrderingTests | 4 |
| BreadcrumbPendingOpenCloseTests | 7 |

The derived figures agree with the independently measured `[TestMethod]` counts recorded below.

## Per-test results

| Test | Class | Result |
|---|---|---|
| FormatDropDownClosedDiagnostics_IncludesEveryDiscriminatingField | BreadcrumbDropDownCloseOrderingTests | Passed |
| NativeCloseWhileCommitPending_DoesNotCancelSelection | BreadcrumbDropDownCloseOrderingTests | Passed |
| NativeCloseWithNoCommitPending_StillCancelsSelection | BreadcrumbDropDownCloseOrderingTests | Passed |
| GestureOpen_ResolvesOpenAndLeavesHostOpenWithoutClose | BreadcrumbDropDownCloseOrderingTests | Passed |
| CloseWhileFactoryPending_InvalidatesOpenAndRepeatedCloseIsIdempotent | BreadcrumbPendingOpenCloseTests | Passed |
| CloseWhileReadinessPending_RejectsLateReadyAttachShowAndFocus | BreadcrumbPendingOpenCloseTests | Passed |
| CloseCanceledFactory_AllowsOneFreshReopenWithoutLateMutation | BreadcrumbPendingOpenCloseTests | Passed |
| CloseWhilePendingOpenAndCommitPending_DoesNotCancelSelection | BreadcrumbPendingOpenCloseTests | Passed |
| ToggleAndEscapeWhileOpenIsPending_EachClosesHostExactlyOnce | BreadcrumbPendingOpenCloseTests | Passed |
| AutomaticSelectorCloseWhileOpenIsPending_ClosesHostExactlyOnce | BreadcrumbPendingOpenCloseTests | Passed |
| RowSetRefreshWhileOpen_NeverClosesHost | BreadcrumbPendingOpenCloseTests | Passed |

No test in either class is recorded as Failed. Every expect-fail test in these classes was made to
pass by task P5-T4, which precedes this gate.

## Companion measurements for tasks P7-T1 and P7-T2

Neither of those two tasks names an artifact of its own, so their measurements are recorded here.

`[TestMethod]` counts, measured with the P1-T7 command form applied to each file:

```
pwsh -NoProfile -Command '(Select-String -Path <file> -SimpleMatch "[TestMethod]").Count'
```

EXIT_CODE: 0

| File | Measured | Required |
|---|---|---|
| QuickFiler.Test/Viewers/BreadcrumbDropDownCloseOrderingTests.cs | 4 | 4 (P7-T1) |
| QuickFiler.Test/Viewers/BreadcrumbPendingOpenCloseTests.cs | 7 | 7 (P7-T2) |

Physical line count for the file P7-T2 constrains, measured with the idiom recorded on the
`LINE-COUNT-IDIOM:` line of evidence/baseline/p0-t12-file-size-baseline.md and no other:

| File | Measured | Ceiling | Verdict |
|---|---|---|---|
| QuickFiler.Test/Viewers/BreadcrumbPendingOpenCloseTests.cs | 458 | 470 | within |

## The pinned scoping assertions, re-verified after the Phase 7 insertions

Task P5-T6 recorded the two spec-pinned `CancelCount.Should().Be(1)` assertions at lines 48 and 79.
The Phase 7 insertion was placed below them and the anchor and working-area values it needs were
declared as locals inside the new test rather than as class fields, specifically so those two
assertions keep their recorded positions. Re-measured after P7-T2:

```
L48: harness.CancelCount.Should().Be(1);
L79: harness.CancelCount.Should().Be(1);
L113: harness.CancelCount.Should().Be(1);
L49: harness.FocusAnchorCount.Should().Be(1);
L80: harness.FocusAnchorCount.Should().Be(1);
L114: harness.FocusAnchorCount.Should().Be(1);
```

All six are unchanged in text and unchanged in position.

## What AC1 does and does not assert here

GestureOpen_ResolvesOpenAndLeavesHostOpenWithoutClose asserts at the managed seam that the open
task resolves true, that the host reports open, that the selection session reports the selector
open, and that no `Close` reaches the mocked host across the gesture open path. The part of AC1 that
is not automatable is that no FRAMEWORK close occurs: no framework drop-down is shown in a headless
test, so there is no `ToolStripDropDown` to raise `Closed`. That limit is stated in the test's own
doc comment rather than asserted, and it is covered by the Phase 2 manual observation.

Output Summary: 11 total, 11 passed, 0 failed; combined Total matches the required 11; the two
per-class counts were derived and agree with the independently measured `[TestMethod]` counts.
