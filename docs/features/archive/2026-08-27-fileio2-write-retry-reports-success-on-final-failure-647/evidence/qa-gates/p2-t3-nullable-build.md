# P2-T3 — Nullable Build Gate After the Seam Change

Timestamp: 2026-08-31T19-25
Command: msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true
EXIT_CODE: 0
ExpectedExitCode: 0

## Recorded integers from MSBuild's final summary

- Warnings: 5
- Errors: 0

## Acceptance evaluation against the recorded baseline

- Recorded error count 0 is less than or equal to `BASELINE_NULLABLE_ERRORS:` 0 from `evidence/baseline/p0-t14-nullable-build.md`. Holds.
- Recorded warning count 5 is less than or equal to `BASELINE_NULLABLE_WARNINGS:` 5 from the same artifact. Holds.

CARRIED_BASELINE_ERRORS: `evidence/baseline/p0-t14-nullable-build.md` records `BASELINE_NULLABLE_ERRORS: 0` and `BASELINE_NULLABLE_WARNINGS: 5`. The warning baseline is non-zero, so the carried-blocker form applies to it: the 5 warnings this run reports are the same 5 `System.Reactive.PackagesConfigCheck.targets` warnings the baseline recorded, emitted by a targets file rather than by the compiler. They carry no diagnostic identifier and are not promoted by `TreatWarningsAsErrors`, which is why the observed `EXIT_CODE:` is nevertheless 0 rather than non-zero. No non-zero exit was authorized or needed.

The build was verified to be a real compilation: 36 `csc.exe` invocations in the captured log. A scan of the log for lines matching `(warning|error) <LETTERS><DIGITS>` returned zero matches, so the seam introduced no compiler diagnostic of any kind.

Specifically confirmed absent for the seam shape introduced by P2-T1: no CS8602 nullable dereference on either delegate, which the null-coalescing into explicitly typed non-nullable locals before the loop prevents; no CS1998 on either production default, because neither default lambda is written `async`; and no CS0162 unreachable code, because the loop was not yet restructured in this phase.

Output Summary: The seam change compiles clean under the nullable gate with no increase against either recorded baseline integer.
