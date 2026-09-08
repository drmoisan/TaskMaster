# P8-T4 — AC4 runs 7 and 8

Timestamp: 2026-09-08T10-34
Task: [P8-T4]
Command: <vstest> <nine assemblies> /EnableCodeCoverage /InIsolation "/Logger:trx;LogFileName=p8-t4-run<k>.trx" /ResultsDirectory:coverage/trx/p8-t4 /Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None "/TestCaseFilter:TestCategory!=LiveOutlook&FullyQualifiedName!~ShellUtilities&FullyQualifiedName!~SysImageListHelper&FullyQualifiedName!~OSBrowser"
EXIT_CODE: 0
EXIT_CODE (run 7): 1

## VERDICT: ACCEPTANCE NOT MET

Run 7 reported one failing test. The pair acceptance requires `failed`=0 on both runs, so this
task does not pass and is left unchecked in the plan. No re-run was performed: re-running until
green is what AC4 exists to prevent, and doing it here would destroy the evidence.

## Tree state

| Observation | Value |
|---|---|
| `git rev-parse HEAD` | `03b7bd57cacfc902a8b9f4e917ace627ffcac464` |
| Equals `SOURCE-HEAD` from P7-T12 | `True` |
| `git status --porcelain -- "*.cs" "*.csproj"` entry count | 0 |

The tree is unchanged from the source commit, so the failure is not attributable to an edit
between runs.

## Counters

| Run | total | executed | passed | failed | error | aborted | timeout | notExecuted | seconds |
|---|---|---|---|---|---|---|---|---|---|
| 7 | 7162 | 7162 | 7161 | **1** | 0 | 0 | 0 | 0 | 73.9 |
| 8 | 7162 | 7162 | 7162 | 0 | 0 | 0 | 0 | 0 | 69.4 |

## The failing test

```
UtilitiesCS.Test.NewtonsoftHelpers.SDILReader.MethodBodyReader_Tests.GetBodyCode_ReturnsConcatenatedInstructions
```

Failure message, verbatim after redaction:

```
Expected bodyCode "0000 : nop
0001 :  1879067923
0006 : stloc.0
0007 : br.s 0009
0009 : ldloc.0
0010 : ret
" to contain "ldstr".
   at UtilitiesCS.Test.NewtonsoftHelpers.SDILReader.MethodBodyReader_Tests.GetBodyCode_ReturnsConcatenatedInstructions() in <worktree>\UtilitiesCS.Test\NewtonsoftHelpers\SDILReader\MethodBodyReader_Tests.cs:line 74
```

Duration 0.12 s. All nine C4 names still read `Passed` on this run.

## This is a different defect, in files outside the write set

The failing test is not one of the nine C4 names, and it is not one of the three failure modes
this item repairs. Neither the test file
(`UtilitiesCS.Test/NewtonsoftHelpers/SDILReader/MethodBodyReader_Tests.cs`) nor the production
file it exercises (`UtilitiesCS/NewtonsoftHelpers/SDIL Reader/MethodBodyReader.cs` and
`ILGlobals.cs`) appears anywhere in the 20-path write set, and neither was touched by this change.

## Root cause, established by reading

`ILGlobals` holds unsynchronised process-wide mutable statics:

```
UtilitiesCS/NewtonsoftHelpers/SDIL Reader/ILGlobals.cs:112  public static Dictionary<int, object> Cache = new Dictionary<int, object>();
UtilitiesCS/NewtonsoftHelpers/SDIL Reader/ILGlobals.cs:117  public static OpCode[] multiByteOpCodes = null!;
UtilitiesCS/NewtonsoftHelpers/SDIL Reader/ILGlobals.cs:118  public static OpCode[] singleByteOpCodes = null!;
UtilitiesCS/NewtonsoftHelpers/SDIL Reader/ILGlobals.cs:121  public static void LoadOpCodes()
UtilitiesCS/NewtonsoftHelpers/SDIL Reader/ILGlobals.cs:123      singleByteOpCodes = new OpCode[0x100];
```

`LoadOpCodes()` first reassigns `singleByteOpCodes` to a freshly allocated, all-default array and
then fills it in a loop. There is no lock, no `Lazy<T>`, and no `volatile`.

Two test classes call it, and **neither carries `[DoNotParallelize]`**, so under the assembly's
class-level parallel scope they can execute concurrently on different workers:

- `UtilitiesCS.Test/NewtonsoftHelpers/SDILReader/ILGlobals_Tests.cs` calls `ILGlobals.LoadOpCodes()`
  at lines 15, 26, 37 and 47.
- `UtilitiesCS.Test/NewtonsoftHelpers/SDILReader/MethodBodyReader_Tests.cs` calls it at line 364.

When `MethodBodyReader.ConstructInstructions` reads
`ILGlobals.singleByteOpCodes[(int)value]` (line 105) while a sibling class is between the
reassignment and the fill, it receives `default(OpCode)`. `default(OpCode)` has an empty `Name`
and `OperandType` 0, which is `InlineBrTarget`, so the switch at line 119 takes the branch
`metadataToken = ReadInt32(...); metadataToken += position; instruction.Operand = metadataToken;`
and stores a raw integer instead of resolving the string through
`module.ResolveString(metadataToken)` at line 192.

The observed output is exactly that signature: the line reads `0001 :  1879067923` — two spaces
where the opcode name should be (the empty `Name` of `default(OpCode)`) followed by a raw token
value rather than `ldstr "hello"`.

## Attribution

This is a **pre-existing latent race**, not one introduced by this change:

- Both racing classes were already parallel-eligible before this change; neither is in the write
  set and neither gained or lost a `[DoNotParallelize]` attribute here.
- The static state involved is `ILGlobals`, which this change does not touch.

Stated conservatively: this change did move three classes (`StackGeek_Tests`, `PrettyPrint_Tests`,
`DASLFilterParserTests`) from the sequential phase into the parallel set, which raises the parallel
density of the run and can therefore alter the probability that any latent race surfaces. It does
not create this race, and it does not make these two classes newly concurrent with each other.

The defect was not observed in the P0-T10 or P0-T12 baseline runs, but those are two single runs of
an intermittently failing test and do not establish absence.

## Acceptance evaluation

- Both TRX files exist. PASS
- Both runs report `failed`=0. **FAIL** — run 7 reported `failed`=1.
- Both `total` values equal the P7-T5 `total` (7162). PASS
- Both exit codes are 0. **FAIL** — run 7 exited 1.

## Output Summary

AC4 runs 7 and 8 of 10. Run 8 was clean; run 7 failed one test,
`MethodBodyReader_Tests.GetBodyCode_ReturnsConcatenatedInstructions`, through an unsynchronised
static-state race in `ILGlobals` between two test classes that are both outside this item's write
set. The task's acceptance is not met and it remains unchecked. Consequence: the AC4 ten-run gate
cannot report zero failures on ten consecutive runs, so AC4 is not checked off. Recorded for
follow-up triage.
