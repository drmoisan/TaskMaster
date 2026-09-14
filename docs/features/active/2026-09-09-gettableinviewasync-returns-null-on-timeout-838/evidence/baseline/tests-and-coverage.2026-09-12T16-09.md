# P0-T20 — Baseline test run with coverage

Timestamp: 2026-09-13T02-45

Command: the plan's fixed full-run payload from the test-population section with `SUBDIR` replaced by `baseline-tests`, so the result file is named `baseline-tests.trx`. Assembly discovery, test-case filter, derived settings file, isolation switch and run settings are exactly as that section fixes them.

EXIT_CODE: 0

## Delivered-pattern correction, recorded because it changed the observed result

The first invocation of this payload printed `ASSEMBLY_COUNT=0` and the run was void: `No test source files were specified.` The cause was measured, not assumed. The four regular expressions in the plan's discovery expression contain doubled backslashes, and the argv-to-command-line conversion layer between the Bash tool and a native Windows executable de-doubles them even inside a single-quoted argument. A pattern echo proved it: the literal written as `"\\bin\\Debug\\"` arrived as `\bin\Debug\\`, length 12 against the canonical length 14, which as a regular expression is a word boundary followed by `in`, a non-digit class and `ebug`, and matches no path.

The four patterns were therefore constructed from `[char]92`, which no shell layer can alter, and each was echoed and compared against the plan's canonical text before the run. The delivered patterns are:

```
\\bin\\Debug\\
\\obj\\
\\ref\\
(^|\\)\.claude\\
```

These are byte-identical to the four patterns the plan's discovery expression states. The discovery semantics are therefore unchanged: the correction restores the plan's expression rather than altering it. The identical construction must be used by P4-T5, or the final run would discover nothing and the two runs would not be comparable.

## Assembly discovery

ASSEMBLY_COUNT=9, which is at least 2, so the run is not void. The nine discovered assemblies, as worktree-relative paths:

```
QuickFiler.Test\bin\Debug\QuickFiler.Test.dll
SVGControl.Test\bin\Debug\SVGControl.Test.dll
Tags.Test\bin\Debug\Tags.Test.dll
TaskMaster.Test\bin\Debug\TaskMaster.Test.dll
TaskTree.Test\bin\Debug\TaskTree.Test.dll
TaskVisualization.Test\bin\Debug\TaskVisualization.Test.dll
ToDoModel.Test\bin\Debug\ToDoModel.Test.dll
UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll
VBFunctions.Test\bin\Debug\VBFunctions.Test.dll
```

## Result-file selection and counters

TRX_FILE_COUNT=1. The newest file selected by the fixed selection rule is `baseline-tests.trx`, last written 2026-09-13T02-39-10.

```
total    = 7192
executed = 7192
passed   = 7192
failed   = 0
not-run  = 0
```

The not-run count is `total` minus `executed`. The counters are read from the result file's `ResultSummary` `Counters` element rather than from the console, because a green run prints no failed or skipped line at all.

BASELINE-FAILED: NONE

## Denominator R (repository-wide, report-only)

The document-level `line-rate` attribute of the raw Cobertura document is `0.7062982158637655`. This figure is recorded and reported and gates nothing, because no merge-base coverage baseline exists for this feature and a repository-wide blocking threshold therefore cannot be shown satisfiable.

## Per-file baseline for the file under fix

For `UtilitiesCS/OutlookObjects/Table/OlTableExtensions.TableAccess.cs`, computed by the plan's fixed per-file aggregation rule (select every class element whose `filename` attribute ends with the target file name, union their line numbers, take the maximum `hits` per line number, treat a line as covered when that maximum exceeds 0):

```
class nodes matching the file = 9
covered lines = 255
total lines   = 281
```

The nine class nodes are the outer static class, a closure display class, a lambda holder, and six async state machines, one of which is the state machine of `GetTableInViewAsync`. This is why the aggregation rule is mandatory: reading any single class node would under-count the member under fix.

Output Summary: the baseline run discovered nine test assemblies and exited 0 with 7192 tests executed and 7192 passed, zero failed and zero not run, so `BASELINE-FAILED:` is `NONE`. No `ExpectedExitCode:` field is carried, because the observed exit code is 0. The raw Cobertura document exists and its root carries a `line-rate` attribute of 0.7062982158637655, the report-only denominator-R figure. The per-file baseline for the file under fix is 255 covered of 281 total lines, which is the figure P4-T9's no-regression comparison reads. The run did not stall, so nothing is recorded as `STALLED`. No result file and no Cobertura file was copied into the repository; both remain at the out-of-repository scratch root.
