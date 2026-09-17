# Phase 1 — [expect-fail] Shape A Against the UNFIXED Tree (Issue #895)

Timestamp: 2026-09-17T01-20
Task: [P1-T6] [expect-fail]
WORKTREE-LEAF: agent-a8bc4dc5978785885
BUILD-LOCK: held from `[P1-T5]` (`ACQUIRED 895`, exit 0).

Tree state: unfixed. This is AC1's required pre-fix observation. A failing run is the expected
outcome of this task.

Commands (inside a WT-PREAMBLE payload, with a pre-run removal of any earlier TRX in this task's own
results directory, then VSTEST-RESOLVE, then SCOPED-RUN):

```
& $vstest "TaskMaster.Test/bin/Debug/TaskMaster.Test.dll" /Settings:scripts/vscode/TaskMaster.cli.runsettings /InIsolation /TestCaseFilter:"FullyQualifiedName~FSharpCoreHintPathAlignmentTests" "/Logger:trx;LogFileName=p1-t6.trx" /ResultsDirectory:TestResults/p1-t6
$LASTEXITCODE
```

EXIT_CODE: 1
ExpectedExitCode: 1

## Output Summary:

Console summary from the run:

```
Test Parallelization enabled for ...\TaskMaster.Test\bin\Debug\TaskMaster.Test.dll (Workers: 24, Scope: ClassLevel)
  Passed SolutionHasExactlySixFSharpCoreHintPaths [188 ms]
  Failed EveryFSharpCoreHintPath_SelectsNetstandard20 [219 ms]
Test Run Failed.
Total tests: 2
     Passed: 1
     Failed: 1
```

The runsettings in force reported `Workers: 24, Scope: ClassLevel`, which is the repository's
standard `Workers=0` (auto) / `ClassLevel` configuration resolved on this machine. Nothing was
serialised for this run.

TRX-READ output:

```
PRERUN_TRX_COUNT=0
TRX_MATCH_COUNT=1
COUNTERS_TOTAL=2 EXECUTED=2 PASSED=1 FAILED=1
EveryFSharpCoreHintPath_SelectsNetstandard20 OUTCOME=Failed
SolutionHasExactlySixFSharpCoreHintPaths OUTCOME=Passed
```

FAILED_MESSAGE, verbatim:

```
FAILED_MESSAGE[EveryFSharpCoreHintPath_SelectsNetstandard20]=Expected offenders to be empty because the other flavour of this package references netstandard 2.1.0.0, an identity that does not exist on .NET Framework, so any copy deployed from it is unloadable wherever it lands; misaligned entries: QuickFiler.Test\QuickFiler.Test.csproj: ..\packages\FSharp.Core.11.0.100\lib\netstandard2.1\FSharp.Core.dll | QuickFiler\QuickFiler.csproj: ..\packages\FSharp.Core.11.0.100\lib\netstandard2.1\FSharp.Core.dll | ToDoModel\ToDoModel.csproj: ..\packages\FSharp.Core.11.0.100\lib\netstandard2.1\FSharp.Core.dll, but found at least one item {"QuickFiler.Test\QuickFiler.Test.csproj: ..\packages\FSharp.Core.11.0.100\lib\netstandard2.1\FSharp.Core.dll"}.
```

Token presence, measured with ordinal `Contains` over that message:

```
TOKEN QuickFiler.csproj PRESENT=True
TOKEN QuickFiler.Test.csproj PRESENT=True
TOKEN ToDoModel.csproj PRESENT=True
TOKEN UtilitiesCS.csproj PRESENT=False
TOKEN UtilitiesCS.Test.csproj PRESENT=False
TOKEN ToDoModel.Test.csproj PRESENT=False
```

## Observation on the failure-message shape

The assertion library's own rendering of a non-empty collection is
`but found at least one item {<single element>}`: it names one representative element, not the whole
collection. That is visible in the message above, where the brace-delimited portion carries only the
`QuickFiler.Test` entry. An acceptance condition that reads three distinct project names out of that
rendered portion alone would be unsatisfiable regardless of how many entries the collection holds.

The three names are present because the test projects the offending entries into the reason argument
itself, under the label `misaligned entries:`. The behaviour was verified against this run rather
than assumed: the rendered portion and the projected portion are both present in the recorded
message and can be told apart.

## Acceptance

- `PRERUN_TRX_COUNT=0`: yes, so the TRX read is this run's.
- `TRX_MATCH_COUNT=1`: yes.
- `COUNTERS_TOTAL=2 EXECUTED=2 PASSED=1 FAILED=1`: yes.
- `SolutionHasExactlySixFSharpCoreHintPaths OUTCOME=Passed`: yes. The count is six on the unfixed
  tree, so the enumeration is proven live and the flavour assertion is measured over the whole set.
- `EveryFSharpCoreHintPath_SelectsNetstandard20 OUTCOME=Failed`: yes. The test can see the defect.
- The `FAILED_MESSAGE` line contains `QuickFiler.csproj`, `QuickFiler.Test.csproj` and
  `ToDoModel.csproj`: yes, all three.
- It contains none of `UtilitiesCS.csproj`, `UtilitiesCS.Test.csproj`, `ToDoModel.Test.csproj`:
  confirmed, all three absent. The three already-correct HintPaths are not reported as offenders.
- `EXIT_CODE: 1` with `ExpectedExitCode: 1`: yes.

Neither blocking branch was taken: the run did not report `PASSED=2`, and `TOTAL` is 2 as expected.
