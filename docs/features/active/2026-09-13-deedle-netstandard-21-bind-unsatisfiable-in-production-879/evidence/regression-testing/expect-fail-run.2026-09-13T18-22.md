# Phase 2 — Fail-Before Harness Run (expect-fail), Revision R2 re-run

Timestamp: 2026-09-14T10-04

Build lock: ACQUIRED 879 at 2026-09-14T10:03:26, RELEASED by 879 at 2026-09-14T10:04:28.

Outlook gate: `@(Get-Process -Name OUTLOOK -ErrorAction SilentlyContinue).Count` returned `0` before
the build that produced the assembly under test.

This run overwrites the version 1.0 artifact at this path. The superseded artifact was preserved by
`[P1-T5]` at
`evidence/regression-testing/expect-fail-run-superseded-probe-surface.2026-09-13T18-22.md`, and its
presence was confirmed before this task ran.

Command:

```
pwsh -NoProfile -Command '
$vswhere = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"
$vstest = @(& $vswhere -latest -products * -find "Common7\IDE\Extensions\TestPlatform\vstest.console.exe")[0]
& $vstest "TaskMaster.Test/bin/Debug/TaskMaster.Test.dll" /Settings:scripts/vscode/TaskMaster.cli.runsettings /InIsolation /TestCaseFilter:"FullyQualifiedName~TaskMaster.Test.Bootstrap" "/Logger:trx;LogFileName=p2-expect-fail.trx" /ResultsDirectory:TestResults/p2-expect-fail
$LASTEXITCODE
'
```

TRX reader span, and the failure-class extraction span, were run exactly as the task specifies, both
pinned to the file name `p2-expect-fail.trx`. All three payloads additionally carry a leading
`Set-Location` to the item worktree, because the executor was launched without worktree isolation.

Pinning was load-bearing and is confirmed to have worked. Before this run,
`TestResults/p2-expect-fail` contained exactly one TRX,
`DanMoisan_MEGALODON4_2026-09-13_23_35_52_net481.trx`, written by the superseded version 1.0 run, and
zero files matching `p2-expect-fail.trx`. `TRX_MATCH_COUNT=1` after the run therefore reads this
run's TRX and not the superseded one.

EXIT_CODE: 1

ExpectedExitCode: 1

Output Summary:

```
TRX_MATCH_COUNT=1
ChildDomain_HasNoSvgControlAssemblyLoaded OUTCOME=Passed
NegativeControl_Netstandard20Observation_IsRecorded OUTCOME=Passed
AppConfig_DeclaresNetstandardRedirect OUTCOME=Failed
AfterInstall_BothNetstandardVersionsBind OUTCOME=Failed
ThisAddIn_HasExplicitStaticConstructor OUTCOME=Failed
ChildDomain_HasNoAssemblyResolveHandlerBeforeInstall OUTCOME=Passed
ChildDomain_ConfigurationFileDeclaresNoNetstandardRedirect OUTCOME=Passed
AfterInstall_DeedleTypeInitializerSucceeds OUTCOME=Passed
NegativeControl_HasNoUtilitiesCsAssemblyLoaded OUTCOME=Passed
NegativeControl_WithoutInstall_Netstandard21Throws OUTCOME=Passed
DEEDLE_RECORD_CONVERSION_OUTCOME=INVOKED-NO-EXCEPTION
```

Acceptance Condition: NOT MET.

Condition-by-condition:

- `TRX_MATCH_COUNT=1` — MET.
- `AfterInstall_BothNetstandardVersionsBind OUTCOME=Failed` — MET.
- `AfterInstall_DeedleTypeInitializerSucceeds OUTCOME=Failed` — NOT MET. The recorded outcome is
  `Passed`.
- exactly one `DEEDLE_RECORD_CONVERSION_OUTCOME=` line whose value begins with
  `NETSTANDARD-BIND-FAILURE:` — NOT MET. Exactly one such line is present, and its value is
  `INVOKED-NO-EXCEPTION`, which is the completion token rather than either failure class.

The task is left unchecked in the plan and the executor halts here, which is the branch the task
text and the delegation both require.

The recorded value is neither failure class, so the `OTHER-FAILURE:` halt branch the task describes
is not the branch taken. The probe did not reach an unrelated exception; it reached no exception at
all.

## Why this is the same defect class as version 1.0, on a different member

Revision R2 repointed the probe from `RuntimeHelpers.RunClassConstructor` on `Deedle.Reflection` to an
invocation of `Deedle.Reflection.convertRecordSequence` closed over a property-only record type,
because the version 1.0 member returned its success token against a build carrying no fix. The
repointed member returns its success token against the same unfixed build. The measurement is
therefore still not discriminating between a fixed build and an unfixed one, which is the property
`[P2-T11]` exists to establish.

The run rules out the two explanations that would have made this an artefact rather than an
observation:

- Isolation is intact. `NegativeControl_WithoutInstall_Netstandard21Throws` passed, so the
  installer-free domain still cannot bind the `2.1.0.0` identity, and
  `NegativeControl_HasNoUtilitiesCsAssemblyLoaded` passed, so that domain never loaded
  `UtilitiesCS`. All five isolation outcomes `[P2-T12]` gates are `Passed`.
- The bind really is unsatisfiable in the positive domain where the Deedle observation was taken.
  `AfterInstall_BothNetstandardVersionsBind` failed in that same domain with
  `"FileNotFoundException"` observed for the `2.1.0.0` identity against an expected `"LOADED"`.

So in one and the same child domain, the `netstandard 2.1.0.0` identity is unbindable, and
`Deedle.Reflection.convertRecordSequence` closed over a record type completes without raising. The
member is reachable and the harness is sound; what the member does not do is transit the code path
whose `netstandard 2.1.0.0` dependency the reported production trace names.

This is consistent with, rather than contradicted by, the preflight observation the plan relies on.
Preflight invoked the same member closed over a property-only type on an unfixed tree and recorded
`INVOKED-NO-EXCEPTION`, and read that as evidence the post-fix gate is reachable. The same
measurement is also evidence that the pre-fix gate is not discriminating, because the tree preflight
measured carried no fix. The plan's acceptance condition and the preflight observation it cites
disagree, and this run resolves the disagreement in favour of the preflight observation.

## What this does not establish

It does not establish that the production defect is absent. `AfterInstall_BothNetstandardVersionsBind`
independently demonstrates the unsatisfiable `2.1.0.0` bind that the issue reports. What is
unestablished is a Deedle-level observation that changes value when the fix is applied. Selecting
that observation is a planning decision, not an execution one, so the probe was not adapted and the
acceptance condition was not weakened.
