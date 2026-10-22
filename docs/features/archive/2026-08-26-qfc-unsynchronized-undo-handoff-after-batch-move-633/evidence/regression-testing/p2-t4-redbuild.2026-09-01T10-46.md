# Fail-before test compilation (P2-T4)

Timestamp: 2026-09-01T10-46
Task: [P2-T4]
Working directory: WORKTREE

## Prerequisite step

Command: `New-Item -ItemType Directory -Force -Path FEATURE/evidence/regression-testing`
EXIT_CODE: 0

Required, not decorative: MSBuild's file logger does not create intermediate directories and terminates
the build with MSB1029 when the directory part of `/flp:logfile=` does not exist.

## Build step

Command:

```
msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true /fl "/flp:logfile=FEATURE/evidence/regression-testing/p2-t4-redbuild.msbuild.txt;verbosity=normal"
```

EXIT_CODE: 0

File log: `FEATURE/evidence/regression-testing/p2-t4-redbuild.msbuild.txt` (11843 lines).

Verbatim summary lines:

```
Build succeeded.
    5 Warning(s)
    0 Error(s)
```

Count of `Skipping target "CoreCompile"` occurrences: 0.
Count of `CS`, `CA`, `IDE`, `SA`, `MA`, `RCS`, or `S`-prefixed diagnostic lines: 0.

Output Summary: The two fail-before tests compile. The warning count is unchanged from the P0-T8
baseline at 5, all of them the same pre-existing System.Reactive `packages.config` warnings, so the new
test file and the new project compile item introduced no diagnostic.

This is a compile gate, not the fail-before witness itself. A compile failure here would have been a
defect in P2-T2 or P2-T3 rather than evidence of the queue defect, which is why the acceptance condition
is `EXIT_CODE: 0` rather than a non-zero code. The witness is P2-T5, which runs the two tests against
this tree and requires them to fail.

The compile confirms several bindings that a text search could not: `QfcFormController` and its
`internal BackGroundMoveAsync` are reachable from `QuickFiler.Test` through the existing
`InternalsVisibleTo` attribute; `UiThreadDispatcherFixture` and `QfcItemControllerTestSupport` resolve
from the shared `QuickFiler.Controllers.Tests` namespace without a new using directive; and the
`Delegate.CreateDelegate` install path compiles without naming the private `WriteMetricsDelegate` type.
