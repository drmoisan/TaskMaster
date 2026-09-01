# P4-T9 — Nullable Build Gate After the Defect Fix

Timestamp: 2026-08-31T20-00
Command: msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true
EXIT_CODE: 0
ExpectedExitCode: 0

## Recorded integers from MSBuild's final summary

- Warnings: 5
- Errors: 0

## Acceptance evaluation against the recorded baseline

- Recorded error count 0 is less than or equal to `BASELINE_NULLABLE_ERRORS:` 0 from `evidence/baseline/p0-t14-nullable-build.md`. Holds.
- Recorded warning count 5 is less than or equal to `BASELINE_NULLABLE_WARNINGS:` 5 from the same artifact. Holds.

CARRIED_BASELINE_ERRORS: `evidence/baseline/p0-t14-nullable-build.md` records `BASELINE_NULLABLE_ERRORS: 0` and `BASELINE_NULLABLE_WARNINGS: 5`. The warning baseline is non-zero and the carried-blocker form applies to it: the 5 warnings are the `System.Reactive.PackagesConfigCheck.targets` warnings recorded at baseline, which carry no diagnostic identifier and are therefore not promoted to errors by `TreatWarningsAsErrors`. The observed `EXIT_CODE:` is 0.

36 `csc.exe` invocations confirm a real compilation. A scan of the captured log for lines matching `(warning|error) <LETTERS><DIGITS>` returned zero matches.

## The four watch items named in this task

`TreatWarningsAsErrors` promotes every compiler warning, not only the nullable family, and `UtilitiesCS/To Depricate/FileIO2.cs` line 1 carries `#nullable enable`, so every line the fix added to that file participates in nullable flow analysis. Each of the four diagnostics this task names was watched for and none was raised.

- **Nullable dereference on the two seam delegates (CS8602).** Not raised. Both delegates are null-coalesced exactly once, before the loop, into explicitly typed non-nullable locals: `Func<string, TextWriter> createWriter = writerFactory ?? (...)` and `Func<int, CancellationToken, Task> delayAsync = delay ?? (...)`. The explicit type is required because a coalescing expression whose right operand is a lambda has no natural type, and the placement before the loop is what avoids a conditional dereference inside it.
- **An async method without an await (CS1998).** Not raised. The seam overload is `async` and retains `await sw.WriteLineAsync(output)` and `await delayAsync(100, token)`. Neither production default is written as an `async` lambda; both return a task directly, which is why the hazard the spec names under Constraints does not materialize.
- **Unreachable code after the loop restructure (CS0162).** Not raised. The loop is now `while (true)` and every exit is a `return`, so there is no statement after the loop for the compiler to find unreachable. The method has no trailing statement at all.
- **A bound but unused exception variable (CS0168).** Not raised. `catch (IOException ex)` binds `ex` and both `logger.Error` calls pass it to the two-argument overload, so the binding is used on both paths through the handler.

Output Summary: The type-check gate passes with no increase against either recorded baseline integer, and none of the four anticipated diagnostics appeared.
