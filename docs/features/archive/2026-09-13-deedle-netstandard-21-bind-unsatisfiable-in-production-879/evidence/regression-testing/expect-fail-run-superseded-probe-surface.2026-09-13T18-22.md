# Phase 2 — Fail-Before Harness Run (expect-fail)

Timestamp: 2026-09-13T23-36
Superseded: yes - revision R2 repointed the probe member that [P2-T5] specifies, and [P2-T11] was re-run against the repointed probe.

Build lock: ACQUIRED 879 at 2026-09-13T23:32:22, RELEASED after this run returned.

Command:

```
vstest.console.exe "TaskMaster.Test/bin/Debug/TaskMaster.Test.dll" /Settings:scripts/vscode/TaskMaster.cli.runsettings /InIsolation /TestCaseFilter:"FullyQualifiedName~TaskMaster.Test.Bootstrap" /Logger:trx /ResultsDirectory:TestResults/p2-expect-fail
```

resolved through `vswhere -latest -products * -find "Common7\IDE\Extensions\TestPlatform\vstest.console.exe"`.

EXIT_CODE: 1

ExpectedExitCode: 1

Output Summary:

```
AfterInstall_BothNetstandardVersionsBind OUTCOME=Failed
ChildDomain_HasNoAssemblyResolveHandlerBeforeInstall OUTCOME=Passed
NegativeControl_HasNoUtilitiesCsAssemblyLoaded OUTCOME=Passed
NegativeControl_WithoutInstall_Netstandard21Throws OUTCOME=Passed
ChildDomain_ConfigurationFileDeclaresNoNetstandardRedirect OUTCOME=Passed
ChildDomain_HasNoSvgControlAssemblyLoaded OUTCOME=Passed
ThisAddIn_HasExplicitStaticConstructor OUTCOME=Failed
NegativeControl_Netstandard20Observation_IsRecorded OUTCOME=Passed
AppConfig_DeclaresNetstandardRedirect OUTCOME=Failed
AfterInstall_DeedleTypeInitializerSucceeds OUTCOME=Passed
```

Run totals: 10 discovered, 7 passed, 3 failed, 1.4862 seconds.

## Acceptance Condition: NOT MET

`[P2-T11]` requires this artifact to record both of the following:

| Required line | Observed |
|---|---|
| `AfterInstall_BothNetstandardVersionsBind OUTCOME=Failed` | met |
| `AfterInstall_DeedleTypeInitializerSucceeds OUTCOME=Failed` | NOT met; the observed outcome is `Passed` |

The task is therefore not complete and its checklist entry is left unchecked. The run's own
exit code of 1 matches `ExpectedExitCode: 1` and is not itself the gate.

## Diagnosis of the Unmet Line

`AfterInstall_BothNetstandardVersionsBind` failed exactly as the fail-before design predicts:
the `2.1.0.0` identity returned `FileNotFoundException` against an expected `LOADED`, in a
domain where the behaviour-empty installer had already run. That confirms the harness reaches
an unsatisfiable bind and that the seam resolves nothing yet.

`AfterInstall_DeedleTypeInitializerSucceeds` nonetheless returned `OK` and passed. Two
candidate causes were considered and one is ruled out by measurement.

Ruled out: the chain that requires `netstandard 2.1.0.0` being absent from the harness output
directory. Measured in `TaskMaster.Test/bin/Debug`:

```
FSharp.Core.dll PRESENT=True VERSION=11.0.0.0
Deedle.dll PRESENT=True VERSION=3.0.0.0
netstandard.dll PRESENT=False
```

`FSharp.Core` is at the redirected `11.0.0.0`, which is the version that references
`netstandard 2.1.0.0`, and no `netstandard` facade is deployed beside it. The preconditions
for the defect are present.

Remaining and likely cause: the probe method that `[P2-T5]` specifies does not reach the
failing bind. The plan fixes the mechanism as "obtains the type `Deedle.Reflection` and forces
its class constructor through `RuntimeHelpers.RunClassConstructor`", and that is what was
implemented. The reported production trace, reproduced in `issue.md` and in `spec.md`, shows
the failure arising one frame deeper:

```
System.TypeInitializationException: The type initializer for 'Deedle.Reflection' threw an exception.
 ---> System.TypeInitializationException: The type initializer for '<StartupCode$Deedle>.$FrameUtils' threw an exception.
 ---> System.IO.FileNotFoundException: Could not load file or assembly 'netstandard, Version=2.1.0.0, ...'
```

Running the class constructor of `Deedle.Reflection` in isolation does not appear to force the
`<StartupCode$Deedle>.$FrameUtils` initializer, which is where the binding requirement is
raised. The original reproduction recorded in `issue.md` and the wording of acceptance
criterion AC10 in `spec.md` both name a member *invocation* rather than a class-constructor
run: `Deedle.Reflection.convertRecordSequence`, the member already exercised by
`QuickFiler.Test/Controllers/QfcInitEmailQueueZeroBatchTests.cs`. This is stated as the likely
cause rather than a verified one: it was inferred from the trace and from the two measurements
above, and no additional probe was added to confirm it, because the probe surface is fixed by
`[P2-T5]` and changing it is a plan revision rather than an executor action.

No adaptation was made and the task was not forced. The observation is recorded and reported.

## Open `2.0.0.0` Risk: A Material New Observation

`NegativeControl_Netstandard20Observation_IsRecorded` wrote this line to the TRX standard
output:

```
NETSTANDARD_2_0_0_0_NEGATIVE_DOMAIN_RESULT=LOADED
```

In an installer-free child domain on this machine, with no binding redirect for `netstandard`
in the configuration file supplied to that domain, the `2.0.0.0` identity binds successfully
while the `2.1.0.0` identity raises `FileNotFoundException`. The plan's `## R2` records the
unexplained `2.0.0.0` frame in the production trace as an open risk and expects `[P4-T10]` to
take this measurement. It is available here already, and it narrows the question in the
direction `## R2` anticipated: the difference is localised to the add-in AppDomain rather than
to the machine, because on this machine the `2.0.0.0` leg is satisfiable outside that domain.
This remains an observation and not a gate.

## The Two Other Failures

`ThisAddIn_HasExplicitStaticConstructor` and `AppConfig_DeclaresNetstandardRedirect` both
failed, which is the expected Phase 2 state: `[P3-T3]` adds the static constructor and
`[P3-T4]` adds the `netstandard` redirect, and neither exists yet. Neither is named in
`[P2-T11]`'s acceptance condition nor in `[P2-T12]`'s five isolation outcomes.

## Test Host and Resolver Provenance

The run host is `TaskMaster.Test/bin/Debug/TaskMaster.Test.dll`. `TaskMaster.Test` does not
link `TestSupport/TestAssemblyResolver.cs`; that file is `Compile`-linked by exactly two
projects, `QuickFiler.Test` and `UtilitiesCS.Test`. The process-wide resolver installed from
`[AssemblyInitialize]` by issue #877 and PR #880 is therefore **not present in this host at
all**. Independently of that, every observation above is taken through the child domain's own
`AppDomain` instance inside a domain created by `AppDomain.CreateDomain`, which a
parent-domain handler does not reach. No measurement in this run depends on handler ordering.

## Evidence Hygiene

The raw TRX is left under `TestResults/p2-expect-fail`, which `.gitignore` line 39 excludes
via the `[Tt]est[Rr]esult*/` pattern. Only the projected outcome list above is committed.
