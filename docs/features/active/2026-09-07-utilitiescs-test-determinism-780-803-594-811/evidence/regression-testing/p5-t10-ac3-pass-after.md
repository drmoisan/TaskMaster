# P5-T10 — AC3 pass-after evidence

Timestamp: 2026-09-08T09-59
Task: [P5-T10]
Command: msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true /nologo /v:q "/flp:LogFile=coverage/msbuild-p5-t10.log;Verbosity=normal" ; then <vstest> UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /InIsolation "/Logger:trx;LogFileName=p5-t10.trx" /ResultsDirectory:coverage/trx/p5-t10 /Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None "/TestCaseFilter:FullyQualifiedName~StackGeek_Tests|FullyQualifiedName~PrettyPrint_Tests|FullyQualifiedName~DASLFilterParserTests|FullyQualifiedName~OlTableExtensions_Tests|FullyQualifiedName~NLogTraceWriter_Test|FullyQualifiedName~Triage_OlLogicTests"
EXIT_CODE: 0

## Build

| Observation | Value |
|---|---|
| Build exit code | `0` |
| Count of lines exactly equal to `    0 Error(s)` | `1` |
| Warning count | `0` (baseline ceiling from P0-T8 is 0) |
| Diagnostic lines matching `: (warning\|error) <id>` | none |

```
    0 Warning(s)
    0 Error(s)
```

This proves the two out-of-project callers of the seamed members compile unchanged:
`TaskMaster/Ribbon/TryFunctionalityInConstruction.cs:51,66` (which calls `EnumerateTable()` and
`df.PrettyPrint()`) and
`UtilitiesCS/EmailIntelligence/ClassifierGroups/Triage/Triage_OlLogic.cs:74` (which calls
`parser.PrintTree(logicTree, 0)`). All three new parameters are optional and trailing, so the
existing call sites bind to the same members with the writer defaulted to `Console.Out`.

## D10 substitution decision

D10_SUBSTITUTION_TRIGGERED: False

The build log contains no `CS0103` or `CS0246` diagnostic naming `PrettyPrint.cs`, so the D10
conditional substitution did not apply: `using Svg;` stays deleted and
`using System.Windows.Input;` is retained. The three removals chosen by D10
(`using System.Threading.Tasks;` at old line 6, the duplicate `using System.Text;` at old line 12,
and `using Svg;` at old line 18) were each verified by a token probe before deletion — `Task`
occurred only on its own using line, `Svg` only on its own using line, and `System.Text` appeared
at both line 5 and line 12 — and all three were confirmed unused by this clean build.

PRETTYPRINT_LINECOUNT: 680

`PrettyPrint.cs` is exactly 680 lines, unchanged from baseline. The zero-net-growth budget held:
`+1` for `using System.IO;`, `+2` for the two overloads wrapping to two lines each under
CSharpier's 100-column width, and `-3` for the three removals.

## Test run

| Observation | Value |
|---|---|
| Test run exit code | `0` |

```
total=139 executed=139 passed=139 failed=0 error=0 timeout=0 aborted=0 notExecuted=0
```

`failed` = 0. No result carried an outcome other than `Passed`.

## The six named outcomes

| Method | Outcome |
|---|---|
| `PrintTree_WritesIndentedTreeToSuppliedWriter` (renamed) | `Passed` |
| `DataFramePrettyHelpers_RenderRowsMarkdownAndWriterOutput` (renamed) | `Passed` |
| `EnumerateTable_WritesFormattedOutputToSuppliedWriterAndMovesToStart` (renamed) | `Passed` |
| `Main_RunsSampleScenarioWithoutThrowing` | `Passed` |
| `Run_WritesScenarioToSuppliedWriter` (new) | `Passed` |
| `PrettyPrint_NullWriter_WritesToConsoleWithoutThrowing` (new) | `Passed` |

None returned `ABSENT`. `Triage_OlLogicTests` is included in the filter because
`Triage_OlLogicTests.cs:142` already exercises the null-writer `PrintTree` path, which is why no
extra null-writer test was added for DASL; it passed as part of the 139.

## Console.SetOut elimination

CONSOLE_SETOUT_TOTAL: 0

`Select-String -SimpleMatch 'Console.SetOut'` over the five test files this phase touched
(`StackGeek_Tests.cs`, `PrettyPrint_Tests.cs`, `DASLFilterParserTests.cs`,
`OlTableExtensions_Tests.cs`, `NLogTraceWriter_Test.cs`) totals 0. Every capture-and-assert site
now writes to a test-owned `StringWriter` passed as an argument, so no test in this assembly
depends on process-wide `Console.Out`, and the roughly 24 unrestored `DebugTextWriter` aggressors
can no longer break any assertion.

Three of the four victim classes lost `[DoNotParallelize]` entirely
(`StackGeek_Tests`, `PrettyPrint_Tests`, `DASLFilterParserTests`), so the AC4 ten-run gate now
exercises the seam rather than the serialization stopgap. `OlTableExtensions_Tests` retains the
attribute under D8 and spec Mitigation 4, with its comment rewritten so it no longer claims the
console as the reason; that retention is filed as a follow-up in P8-T9.

`NLogTraceWriter_Test`, the propagator, lost its `Console.Out` save/install/restore altogether: it
asserts on a mock logger and never on console text, so the `DebugTextWriter` install had no purpose
and its `[TestCleanup]` could install a foreign writer as the process-wide `Console.Out` for the
remainder of the run.

## Acceptance evaluation

- Build exit 0 with `    0 Error(s)`. PASS
- Test `EXIT_CODE: 0`. PASS
- `failed` = 0. PASS
- `Read-TrxOutcome` is `Passed` for all six named tests. PASS
- `Select-String -SimpleMatch 'Console.SetOut'` over the five test files totals 0. PASS

## Output Summary

Full-solution rebuild clean (0 warnings, 0 errors) and 139 tests across the six filtered classes
all passing. The four `TextWriter` seams are in place, the shared-console dependency is eliminated
from every capture-and-assert site, and `PrettyPrint.cs` held at exactly 680 lines with no D10
substitution required.
