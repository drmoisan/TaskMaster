# D4 — Fail-Before Evidence ([P4-T3]) `[expect-fail]`

Timestamp: 2026-08-28T05-45

## Step 1 — intermediate build

Command (under `pwsh -NoProfile`, worktree root):

```
MSBuild.exe QuickFiler.Test\QuickFiler.Test.csproj /t:Build /m /p:Configuration=Debug /p:Platform=AnyCPU /nologo
```

GATE: none (intermediate build)
EXIT_CODE: 0 — `0 Error(s)`, 3 warnings, elapsed 00:00:02.17. The `/p:Platform=AnyCPU` substitution is
the documented deviation recorded in full in `488-d1-fail.md`.

## Step 2 — the failing test run

Command (under `pwsh -NoProfile`, worktree root):

```
vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation "/TestCaseFilter:FullyQualifiedName~InitializeBreadcrumbPipeline_AmbientContextNull_ThrowsBoundaryDiagnostic|FullyQualifiedName~InitializeBreadcrumbPipeline_DifferentNonNullContext_ThrowsBoundaryDiagnostic" "/Logger:trx;LogFileName=488-d4-fail.trx" /ResultsDirectory:docs\features\active\itemviewer-breadcrumb-lifecycle-defects-488\evidence\regression-testing\p4-t3-d4-fail
```

EXIT_CODE: 1
ExpectedExitCode: 1

| Test | Outcome |
| --- | --- |
| `InitializeBreadcrumbPipeline_AmbientContextNull_ThrowsBoundaryDiagnostic` | **Failed** |
| `InitializeBreadcrumbPipeline_DifferentNonNullContext_ThrowsBoundaryDiagnostic` | **Failed** |

Total tests 2, **Failed 2**. `Test Run Failed.`

## No exception was thrown — the guard does not yet exist

Both failures report the same shape. For the different-context case:

```
Expected a <System.InvalidOperationException> to be thrown because a different non-null context is off
the viewer's owning boundary too, but no exception was thrown.
```

**"No exception was thrown" is the load-bearing observation.** Against the unfixed code
`InitializeBreadcrumbPipeline(provider, operations)` performs no boundary check at all: it opens with
the D3 provider guard and proceeds straight to building the lifecycle coordinator, regardless of which
synchronization context is current. Both calls therefore succeed silently from off the viewer's owning
boundary, which is exactly the undeclared, unenforced affinity that D4 exists to close.

Both cases failing, rather than one, is itself informative: the ambient-null case and the
different-non-null-context case fail identically today because neither is distinguished from the
correct-boundary case by any code.

## Why the two-argument overload is used

Per decision D-8, both tests call the **two-argument** overload with injected operations. The
one-argument overload evaluates `BreadcrumbPopupUiOperations.CaptureCurrent()` as an **eager
argument**, and that method already throws `InvalidOperationException` under a null ambient context. A
D4 test written against the one-argument overload would therefore have thrown here — and passed —
**before the guard existed**, for a reason entirely unrelated to the guard. The observation
"no exception was thrown" is only available because the operations object is injected rather than
captured.

For the same reason each assertion additionally requires the exception message to contain the token
`InitializeBreadcrumbPipeline`. The dispatcher's own ambient-context message does not name the
operation, so that clause keeps a future accidental capture from satisfying the assertion.

## TRX

`docs/features/active/itemviewer-breadcrumb-lifecycle-defects-488/evidence/regression-testing/p4-t3-d4-fail/488-d4-fail.trx`

Output Summary: EXIT_CODE 1 with `ExpectedExitCode: 1`. Both D4 regression tests —
`InitializeBreadcrumbPipeline_AmbientContextNull_ThrowsBoundaryDiagnostic` and
`InitializeBreadcrumbPipeline_DifferentNonNullContext_ThrowsBoundaryDiagnostic` — record outcome
**Failed** against the unfixed code, both with **no exception thrown**, which confirms the affinity
guard does not yet exist.
