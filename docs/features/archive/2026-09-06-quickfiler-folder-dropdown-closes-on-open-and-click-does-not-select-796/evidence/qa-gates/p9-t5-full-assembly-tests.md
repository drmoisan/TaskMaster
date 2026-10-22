# P9-T5 — Full QuickFiler.Test assembly run

Timestamp: 2026-09-07T15-57
Task: [P9-T5]
Issue: #796
Channel used: A

Command, the P0-T10 command form with the results directory `TestResults\796\p9-t5`
and the log file name `p9-t5.trx`:

```
pwsh -NoProfile -Command '$vswhere = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"; $vstest = & $vswhere -latest -products * -find "Common7\IDE\Extensions\TestPlatform\vstest.console.exe" | Select-Object -First 1; & $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation "/TestCaseFilter:TestCategory!=LiveOutlook" /ResultsDirectory:TestResults\796\p9-t5 "/Logger:trx;LogFileName=p9-t5.trx"; "EXIT_CODE=$LASTEXITCODE"'
```

EXIT_CODE: 0

The assembly under test is the one the P9-T4 rebuild produced; that rebuild wrote
QuickFiler.Test/bin/Debug/QuickFiler.Test.dll at 2026-09-07T19:55:41.1478330Z, ahead of
this run, so this is not a stale binary.

## Run summary, verbatim

```
Test Run Successful.
Total tests: 1380
     Passed: 1380
 Total time: 13.3662 Seconds
```

| Figure | Value | How obtained |
|---|---|---|
| Total | 1380 | read from the run summary |
| Passed | 1380 | read from the run summary |
| Failed | 0 | NOT PRINTED ON A PASSING RUN |

## Failed set

EMPTY.

The run printed no `Failed:` line. Under the convention P0-T10 records and this task's
acceptance clause repeats, a run printing no `Failed:` line has an EMPTY Failed set
rather than an unread one, so the reading is a measurement and not an omission.

That reading was corroborated against the TRX rather than left on the console summary
alone, because the console summary omits the line entirely on a passing run and an
omitted line cannot be distinguished from an unparsed one by reading the console:

```
pwsh -NoProfile -Command '$x = New-Object System.Xml.XmlDocument; $x.Load((Resolve-Path "TestResults\796\p9-t5\p9-t5.trx").Path); $r = $x.GetElementsByTagName("UnitTestResult"); "TotalResultNodes=" + $r.Count; $bad = @($r | Where-Object { $_.GetAttribute("outcome") -ne "Passed" }); "NonPassedCount=" + $bad.Count'
```

TotalResultNodes=1380
NonPassedCount=0

Every one of the 1380 result nodes carries `outcome="Passed"`. No node carries Failed,
NotExecuted, Aborted, Timeout, or any other outcome.

## Comparison against the P0-T10 baseline

Baseline read from evidence/baseline/p0-t10-quickfiler-test-baseline.md.

| Clause | Baseline | P9-T5 | Verdict |
|---|---|---|---|
| Failed set is a subset of BASELINE_FAILURE_SET | EMPTY | EMPTY | subset; no test outside the set Failed |
| Total no smaller than the baseline Total | 1370 | 1380 | not smaller |

Because the BASELINE_FAILURE_SET is empty, the subset condition permits zero failures,
and zero is what was observed. The Total grew by 10, which is this item's own added
tests; the gate asserts non-shrinkage rather than an absolute total, so growth satisfies
it.

## Expect-fail inventory

INVENTORY COUNT READ: 4 rows.

The count is four rather than five because task P4-T7 took its NO branch. The artifact
evidence/qa-gates/p4-t7-park-focus-decision.md opens its `## Branch taken` section with
`NO branch`, quotes `AC2-PARK-FOCUS-SUPPRESSED: NO` from the Phase 3 decision record as
its basis, and states in its own consequences list that the plan's expect-fail inventory
therefore stands at four rows and that task P9-T5 reads it as four.

A caution on how that condition must be read, recorded because a literal search returns
the opposite answer to the correct one. The plan makes the fifth row conditional on the
P4-T7 artifact RECORDING the line `PARK-FOCUS-SUPPRESSION: IN SCOPE FOR P4-T8`. A search
of that artifact for the token was run in this task and it returns one hit, at line 30.
That hit is not a record of the line. It is the backticked subject of the sentence "The
line ... is deliberately ABSENT from this artifact", so the single occurrence of the
token is the artifact declaring that it does not carry the line. Reading the search hit
count as satisfaction of the condition would invert the decision and produce a
five-row inventory containing a test that was never written. The condition is therefore
resolved on the recorded branch — NO — and not on token presence.

The conditional fifth row, the paired `ParkFocusOffWebView2()` negative test, is
accordingly not in the inventory. Its absence from the suite is corroborated
independently of the artifact, by enumerating every `testName` in the P9-T5 TRX
containing the substring `Park`:

```
ActionCancelAsync_ParksFocusAndCancelsBreadcrumbSelectors
Cleanup_WithParkedConsumer_ReturnsWithoutWaiting
FormDeactivated_NoWebView2Focus_DoesNotPark
FormDeactivated_WebView2Focused_ParksFocusOnce
```

Four names, none of them a paired negative test of
FormDeactivated_WebView2Focused_ParksFocusOnce. The substring `Park` was used rather
than the narrower `ParkFocus` deliberately: `ParkFocus` returns zero matches against
this suite, because the existing test spells the verb `Parks`, so a zero-match result
from it would have proved nothing about whether a paired test exists.

Outcome per inventory row, read from the TRX by `testName`:

```
pwsh -NoProfile -Command '$x = New-Object System.Xml.XmlDocument; $x.Load((Resolve-Path "TestResults\796\p9-t5\p9-t5.trx").Path); $r = $x.GetElementsByTagName("UnitTestResult"); $names = @("FormDeactivated_SelfInflictedByOwnPopup_DoesNotCancelAnySelector","NativeCloseWhileCommitPending_DoesNotCancelSelection","NativeCloseWithNoCommitPending_StillCancelsSelection","SearchLeaveAfterMouseDrivenOpen_DoesNotCloseDropDown"); foreach ($n in $names) { $m = @($r | Where-Object { $_.GetAttribute("testName") -eq $n }); "EXPECTFAIL " + $n + " count=" + $m.Count + " outcome=" + (($m | ForEach-Object { $_.GetAttribute("outcome") }) -join ",") }'
```

| # | Test | Class | Result nodes | Outcome |
|---|---|---|---|---|
| 1 | FormDeactivated_SelfInflictedByOwnPopup_DoesNotCancelAnySelector | QfcFormControllerDeactivateTests | 1 | Passed |
| 2 | NativeCloseWhileCommitPending_DoesNotCancelSelection | BreadcrumbDropDownCloseOrderingTests | 1 | Passed |
| 3 | NativeCloseWithNoCommitPending_StillCancelsSelection | BreadcrumbDropDownCloseOrderingTests | 1 | Passed |
| 4 | SearchLeaveAfterMouseDrivenOpen_DoesNotCloseDropDown | QfcItemController_SearchLeaveLatchTests | 1 | Passed |

Each name resolves to exactly one result node, so no row is satisfied by a
same-named test in a second class, and every one of the four is Passed.

## Acceptance clause by clause

| Clause | Observed | Met |
|---|---|---|
| Failed set a subset of BASELINE_FAILURE_SET, with no test outside that set Failed | Failed set EMPTY; baseline set EMPTY | yes |
| the empty-`Failed:`-line convention applied as P0-T10 records it | applied, and corroborated by 0 non-Passed nodes out of 1380 in the TRX | yes |
| every expect-fail inventory test recorded as Passed | all 4 Passed | yes |
| the artifact names which inventory count it read and why | 4 rows, because P4-T7 took its NO branch | yes |
| recorded Total no smaller than the baseline Total | 1380 against 1370 | yes |

## Raw output

The TRX is written to the gitignored path TestResults/796/p9-t5/p9-t5.trx and is never
committed, because a TRX embeds the host account name and machine name in its `runUser`
and `computerName` attributes. `.gitignore` line 39 ignores TestResults/ through the
bracket class `[Tt]est[Rr]esult*/`.

Output Summary: 1380 tests run under the `TestCategory!=LiveOutlook` filter with
`/InIsolation`; 1380 Passed, Failed set EMPTY, EXIT_CODE 0. The Failed set is a subset
of the empty BASELINE_FAILURE_SET, the Total grew from 1370 to 1380, and all four rows
of the expect-fail inventory — four because P4-T7 took its NO branch — are recorded as
Passed. All five acceptance clauses are met.
