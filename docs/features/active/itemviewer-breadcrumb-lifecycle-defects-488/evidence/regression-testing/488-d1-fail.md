# D1 — Fail-Before Evidence ([P1-T4]) `[expect-fail]`

Timestamp: 2026-08-28T05-27

## Step 1 — intermediate build

Command (under `pwsh -NoProfile`, worktree root):

```
MSBuild.exe QuickFiler.Test\QuickFiler.Test.csproj /t:Build /m /p:Configuration=Debug /p:Platform=AnyCPU /nologo
```

GATE: none (intermediate build)
EXIT_CODE: 0 — `0 Error(s)`, 3 warnings, elapsed 00:00:03.26. `QuickFiler.Test\bin\Debug\QuickFiler.Test.dll`
was rewritten by this build, so the test run below exercised the new source rather than a stale
assembly.

### DOCUMENTED DEVIATION — project-level platform name

This task's text specifies `"/p:Platform=Any CPU"`. That value was tried first and **failed**:

```
Microsoft.Common.CurrentVersion.targets(843,5): error : The BaseOutputPath/OutputPath property is
not set for project 'QuickFiler.Test.csproj'. ... Configuration='Debug' Platform='Any CPU'. You may
be seeing this message because you are trying to build a project without a solution file, and have
specified a non-default Configuration or Platform that doesn't exist for this project.
```

`Any CPU` (with a space) is the **solution-level** platform name. The project's own platform name is
`AnyCPU` (no space): `QuickFiler.Test.csproj:12` declares
`<Platform Condition=" '$(Platform)' == '' ">AnyCPU</Platform>` and the output path is set by the
condition at `:32`, `'$(Configuration)|$(Platform)' == 'Debug|AnyCPU'`. A direct project build must
therefore use `AnyCPU`, and the substitution above is the minimal change that lets the stated command
run at all.

This is safe under decision D-2: this build is **not** an analyzer or nullable gate, and its only
purpose is to produce a fresh `QuickFiler.Test.dll`. The two msbuild gates in this plan
(`[P0-T10]`/`[P0-T11]` and `[P8-T3]`/`[P8-T4]`) build `TaskMaster.sln`, where `"Any CPU"` is the
correct name, and they are unaffected. The same substitution applies to every `/t:Build` invocation
in this plan.

## Step 2 — the failing test run

Command (under `pwsh -NoProfile`, worktree root):

```
vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation "/TestCaseFilter:FullyQualifiedName~ConfigureBreadcrumbDropDown_EnvironmentChange_DisposesOutgoingHostBeforeReplacement" "/Logger:trx;LogFileName=488-d1-fail.trx" /ResultsDirectory:docs\features\active\itemviewer-breadcrumb-lifecycle-defects-488\evidence\regression-testing\p1-t4-d1-fail
```

EXIT_CODE: 1
ExpectedExitCode: 1

| Test | Outcome |
| --- | --- |
| `ConfigureBreadcrumbDropDown_EnvironmentChange_DisposesOutgoingHostBeforeReplacement` | **Failed** |

Total tests 1, Failed 1, elapsed 2.1370 seconds. `Test Run Failed.`

## The failing assertion is the FIRST one — the discriminating observation

```
Expected a <System.ObjectDisposedException> to be thrown because the outgoing host must already be
disposed when the replacement is constructed, but no exception was thrown.
```

The stack trace attributes the failure to
`ItemViewerBreadcrumbLifecycleRegressionTests.cs:line 80`, which is the
`theme.Should().Throw<ObjectDisposedException>(...)` statement — the **first** of the three
assertions and the `SetTheme` disposal-guard assertion named as the discriminating observation in
decision D-10a.

This is the required attribution. `SetTheme` on the captured outgoing host reaches the host's
`ThrowIfDisposed()` guard and throws once the host is disposed, and returns silently while it is not.
Against the unfixed code the outgoing host has not been disposed at the moment the replacement is
constructed, so no exception is thrown and the assertion fails. That is the product defect.

Neither of the other two assertions was reached, so neither could be the cause. Had the failure been
attributed to the `Close` assertion or to the post-drain `DropDown.IsDisposed` assertion, that would
have indicated a defect in the test rather than evidence of the product defect, because per decision
D-10a both of those observations hold in the pre-fix state as well and are corroborating rather than
discriminating.

## TRX

`docs/features/active/itemviewer-breadcrumb-lifecycle-defects-488/evidence/regression-testing/p1-t4-d1-fail/488-d1-fail.trx`

Output Summary: EXIT_CODE 1 with `ExpectedExitCode: 1`. The D1 regression test
`ConfigureBreadcrumbDropDown_EnvironmentChange_DisposesOutgoingHostBeforeReplacement` records outcome
**Failed** against the unfixed code, and the failure is attributed to the first assertion — the
discriminating `SetTheme` disposal-guard observation — with the message "no exception was thrown".
The intermediate build that produced the assembly exited 0 and is not a gate.
