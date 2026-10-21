# P4-T8 — Analyzer Build After the Defect Fix and Call-Site Updates

Timestamp: 2026-08-31T19-58
Command: msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true
EXIT_CODE: 0
ExpectedExitCode: 0

## Recorded integers from MSBuild's final summary

- Warnings: 5
- Errors: 0

## Acceptance evaluation against the recorded baseline

- Recorded error count 0 is less than or equal to `BASELINE_ANALYZER_ERRORS:` 0 from `evidence/baseline/p0-t13-analyzer-build.md`. Holds.
- Recorded warning count 5 is less than or equal to `BASELINE_ANALYZER_WARNINGS:` 5 from the same artifact. Holds.

CARRIED_BASELINE_ERRORS: `evidence/baseline/p0-t13-analyzer-build.md` records `BASELINE_ANALYZER_ERRORS: 0` and `BASELINE_ANALYZER_WARNINGS: 5`. The warning baseline is non-zero, so the carried-blocker form applies to it: the 5 warnings this run reports are the same `System.Reactive.PackagesConfigCheck.targets` warnings recorded at baseline, emitted once per affected project by a targets file rather than by the compiler. They carry no diagnostic identifier and none originates in this change's footprint, which is why the observed `EXIT_CODE:` is 0 rather than non-zero.

36 `csc.exe` invocations in the captured log confirm a real compilation rather than a skipped incremental pass.

## The two inferred conversion behaviors are now compiled

This run is the task's stated purpose: confirming the two behaviors the research file marked as inferred rather than compiled. Both are confirmed, and the confirmation is what makes the deliberate call-site edits necessary rather than optional.

1. **A `Task<bool>`-returning method group converts to `Func<..., Task>` through return-type covariance.** Confirmed indirectly and decisively: the property initializer `= FileIO2.WriteTextFileAsync;` at `QuickFiler/Controllers/QfcHomeController.Metrics.cs` line 34 compiled unchanged both before P4-T3 changed the property's declared result type and after. Had the conversion been illegal, the pre-P4-T3 tree would have failed to compile the moment P4-T1 changed the method's return type. It did not. The property would therefore have kept compiling while silently discarding the new failure signal, which is exactly the hazard the research file names and the reason P4-T3 changes the declaration deliberately.

2. **An `await`-expression-bodied async lambda returning `Task<bool>` converts to `Action<T>`.** Same confirmation: the original expression-bodied `writer.DiskWriter = async (items) => await FileIO2.WriteTextFileAsync(...)` in `TaskMaster/AppGlobals/AppOlObjects.cs` would have continued to compile against the new signature, discarding the result. P4-T5 replaced it with a block body deliberately rather than leaving it to compile by accident.

A clean analyzer build is therefore not evidence that the fix reached the callers. That evidence is P7-T12 through P7-T16, which read the tree.

## One error was raised and remediated, with a loop restart

The first invocation of this task exited 1 with a single error:

```
TaskMaster\AppGlobals\AppOlObjects.cs(324,28): error CS0104: 'Exception' is an ambiguous reference between 'Microsoft.Office.Interop.Outlook.Exception' and 'System.Exception'
```

This is a genuine finding produced by this gate, not a pre-existing condition: the token `catch (Exception ex)` occurs zero times in the pre-change file, so the ambiguity could only have been introduced by P4-T5. The remediation, its rationale, and the resulting toolchain-loop restart are recorded in `evidence/qa-gates/p4-t7-format.md`. The run recorded above is the post-remediation run.

Output Summary: The analyzer gate passes with no increase against either recorded baseline integer, and both previously inferred conversion behaviors are confirmed by compilation.
