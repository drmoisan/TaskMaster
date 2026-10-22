# AC32 — Non-Vacuity of Both MSBuild Gates

Timestamp: 2026-09-09T17-17

Source logs: evidence/qa-gates/qc-build-analyzers.txt and evidence/qa-gates/qc-build-nullable.txt

## qc-build-analyzers.txt

LogLines: 70228
SkippingCoreCompileCount: 0
CscInvocationsForWriteSetProjects: 4

## qc-build-nullable.txt

LogLines: 70653
SkippingCoreCompileCount: 0
CscInvocationsForWriteSetProjects: 4

## How each figure is counted

SkippingCoreCompileCount is the number of log lines containing the token
`Skipping target "CoreCompile"`. A zero count means MSBuild's incremental up-to-date check did not
short-circuit compilation on any project.

CscInvocationsForWriteSetProjects is the number of log lines containing the token
`/out:obj\Debug\UtilitiesCS.dll` plus the number containing `/out:obj\Debug\UtilitiesCS.Test.dll`.
Each of the two projects in the Write Set contributes 2, for a total of 4 in each log. Those are the
csc.exe command lines MSBuild echoes under each project's CoreCompile heading at detailed verbosity,
and they are the only single-line evidence that names both the compiler invocation and the project
it compiled.

A count of lines carrying both `Task "Csc"` and a project file name was deliberately not
substituted. MSBuild prefixes a task-start line with the project instance id and never with the
project path, so those two tokens never appear on one line and that count would be zero whatever the
build did.

## Why the second figure is the load-bearing one

A zero count of skip messages proves nothing on its own: a build that compiled nothing at all also
emits no skip message. The csc invocation count is what makes the zero non-vacuous, because it shows
compilation actually ran for both projects in the Write Set. Both figures are therefore recorded
together, and the acceptance requires the second to be at least 2. It is 4 in each log.

Both figures were read before the D7 sanitisation rewrote the absolute paths in these logs. The four
counted tokens carry no absolute path and are unaffected by that rewrite, so the counts remain
reproducible from the committed logs.
