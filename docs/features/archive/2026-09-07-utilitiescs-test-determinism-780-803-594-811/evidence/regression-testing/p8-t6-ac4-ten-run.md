# P8-T6 — AC4 ten-run aggregate

Timestamp: 2026-09-08T10-39
Task: [P8-T6]
Command: PROC-TRX `Read-TrxCounters` and `Times/@start`,`@finish` over the ten TRX files under `coverage/trx/p8-t1` through `coverage/trx/p8-t5`
EXIT_CODE: 0

## VERDICT: 9 of 10 runs clean. AC4 IS NOT SATISFIED.

AC4 requires zero failures on ten consecutive runs. Run 7 reported one failure, so the criterion
is not met and AC4 is left unchecked in spec.md. P8-T7, the AC4 check-off task, is therefore not
performed.

## The ten runs

| Run | total | executed | passed | failed | error | aborted | timeout | seconds |
|---|---|---|---|---|---|---|---|---|
| 1 | 7162 | 7162 | 7162 | 0 | 0 | 0 | 0 | 70.9 |
| 2 | 7162 | 7162 | 7162 | 0 | 0 | 0 | 0 | 54.2 |
| 3 | 7162 | 7162 | 7162 | 0 | 0 | 0 | 0 | 55.0 |
| 4 | 7162 | 7162 | 7162 | 0 | 0 | 0 | 0 | 51.9 |
| 5 | 7162 | 7162 | 7162 | 0 | 0 | 0 | 0 | 51.3 |
| 6 | 7162 | 7162 | 7162 | 0 | 0 | 0 | 0 | 70.7 |
| 7 | 7162 | 7162 | **7161** | **1** | 0 | 0 | 0 | 73.9 |
| 8 | 7162 | 7162 | 7162 | 0 | 0 | 0 | 0 | 69.4 |
| 9 | 7162 | 7162 | 7162 | 0 | 0 | 0 | 0 | 54.2 |
| 10 | 7162 | 7162 | 7162 | 0 | 0 | 0 | 0 | 58.8 |

Exactly ten rows. All ten `total` values are identical at 7162, matching the P7-T5 `total`.
Wall-clock times range from 51.3 s to 73.9 s. Total gate duration about 10 minutes.

## The one failure

Run 7 (`coverage/trx/p8-t4/p8-t4-run1.trx`):

```
UtilitiesCS.Test.NewtonsoftHelpers.SDILReader.MethodBodyReader_Tests.GetBodyCode_ReturnsConcatenatedInstructions
Expected bodyCode "0000 : nop
0001 :  1879067923
...
" to contain "ldstr".
```

It is caused by an unsynchronised process-wide static in
`UtilitiesCS/NewtonsoftHelpers/SDIL Reader/ILGlobals.cs`: `LoadOpCodes()` reassigns
`singleByteOpCodes` to a fresh all-default array before filling it, and two test classes that both
call it (`ILGlobals_Tests` and `MethodBodyReader_Tests`) carry no `[DoNotParallelize]`, so a reader
can observe the array mid-initialisation. Full derivation in `p8-t4-ac4-runs.md`.

This is a **different defect** from the three this item repairs, in files outside the 20-path write
set, and it is pre-existing: neither racing class was touched by this change and neither gained or
lost a parallelism attribute. It is recorded for follow-up triage rather than fixed here, because
fixing it would require editing files outside the declared write set.

## The three repaired failure modes did not recur

This was checked exhaustively rather than sampled: 13 tracked tests were read from each of the ten
TRX files by `Read-TrxOutcome`, giving 130 assertions, and **all 130 returned `Passed`**. None
returned `ABSENT`, `NO-RESULT`, `Failed` or `NotExecuted`. The 13 are the nine C4 names plus the
four AC3 conversions (`DataFramePrettyHelpers_RenderRowsMarkdownAndWriterOutput`,
`PrintTree_WritesIndentedTreeToSuppliedWriter`,
`EnumerateTable_WritesFormattedOutputToSuppliedWriterAndMovesToStart`,
`Main_RunsSampleScenarioWithoutThrowing`).

Across all ten runs, every one of the nine C4 names read `Passed`, including the two sentinels that
motivated this item:

- `DictionaryExtensions_Tests.TryAddValuesAsync_UpdatesExistingValue` (#780)
- `DfDeedle_COM_Tests.GetEmailDataInViewAsync_SeparatesTableSnapshotFromDataFrameTransform` (#803, #594 item 1)

and the four `Console.Out` victim tests (#594 items 2 and 3), three of which now run without
`[DoNotParallelize]`. On the evidence of ten consecutive runs, the three defects this change
targets did not recur.

## Provenance and load

| Observation | Value |
|---|---|
| `SOURCE-HEAD` recorded by all five pair tasks | `03b7bd57cacfc902a8b9f4e917ace627ffcac464` (identical in all five) |
| `AC4_COMMAND_SHAPE` | CI-VERBATIM |
| Local worker count (`[Environment]::ProcessorCount`) | 24 |

`Workers = 0` in `UtilitiesCS.Test/Properties/AssemblyInfo.cs:18-21` resolves to the processor
count, so these runs used 24 class-level workers.

## Filter deviation, stated honestly

The filter used is:

```
TestCategory!=LiveOutlook&FullyQualifiedName!~ShellUtilities&FullyQualifiedName!~SysImageListHelper&FullyQualifiedName!~OSBrowser
```

The three `FullyQualifiedName!~` clauses beyond `TestCategory!=LiveOutlook` are a documented local
environmental exclusion, not a property of this change: one of the 23 shell-icon tests fails per
run on this workstation with an invalid Win32 icon handle, independently of this fix, per the
`SHELL_ICON_EXCLUSION: REQUIRED` verdict in the #798 baseline evidence. Those classes are covered
by CI, which runs them unfiltered.

The CI run on the pull request supplies the unfiltered single-run form. One CI run is not ten, and
ten local runs are not the unfiltered form; both facts are stated here because neither alone
discharges AC4.

## Acceptance evaluation

- Exactly ten rows. PASS
- Every `failed`, `error`, `aborted`, `timeout` is 0. **FAIL** — run 7 has `failed`=1.
- Every `passed` equals its `total`. **FAIL** — run 7 has `passed`=7161, `total`=7162.
- All ten `total` values are identical and equal the P7-T5 `total`. PASS
- The five `SOURCE-HEAD` values are identical. PASS
- The artifact contains no account name, machine name or absolute path. Verified by P8-T8. PASS
- `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` present. PASS

## Output Summary

Ten consecutive full nine-assembly `/InIsolation` runs on the unchanged source commit
`03b7bd57`, 7162 tests each, 24 workers. Nine runs were completely clean; run 7 failed one test
through a pre-existing unsynchronised-static race in `ILGlobals`, which lies outside this item's
write set. The three defects this item repairs did not recur in any of the ten runs. AC4's literal
condition of zero failures on ten consecutive runs is not met, so AC4 remains unchecked.
