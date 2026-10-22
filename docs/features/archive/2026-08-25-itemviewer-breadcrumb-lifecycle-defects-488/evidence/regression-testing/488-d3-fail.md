# D3 — Fail-Before Evidence ([P3-T2]) `[expect-fail]`

Timestamp: 2026-08-28T05-38

## Step 1 — intermediate build

Command (under `pwsh -NoProfile`, worktree root):

```
MSBuild.exe QuickFiler.Test\QuickFiler.Test.csproj /t:Build /m /p:Configuration=Debug /p:Platform=AnyCPU /nologo
```

GATE: none (intermediate build)
EXIT_CODE: 0 — `0 Error(s)`, 3 warnings, elapsed 00:00:02.14. The `/p:Platform=AnyCPU` substitution is
the documented deviation recorded in full in `488-d1-fail.md`.

## Step 2 — the failing test run

Command (under `pwsh -NoProfile`, worktree root):

```
vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation "/TestCaseFilter:FullyQualifiedName~InitializeBreadcrumbPipeline_SecondDifferentProvider_ThrowsInvalidOperationException|FullyQualifiedName~InitializeBreadcrumbPipeline_RepeatSameProvider_DoesNotThrowAndKeepsCoordinator" "/Logger:trx;LogFileName=488-d3-fail.trx" /ResultsDirectory:docs\features\active\itemviewer-breadcrumb-lifecycle-defects-488\evidence\regression-testing\p3-t2-d3-fail
```

EXIT_CODE: 1
ExpectedExitCode: 1

| Test | Outcome |
| --- | --- |
| `InitializeBreadcrumbPipeline_SecondDifferentProvider_ThrowsInvalidOperationException` | **Failed** |
| `InitializeBreadcrumbPipeline_RepeatSameProvider_DoesNotThrowAndKeepsCoordinator` | **Passed** |

Total tests 2, Passed 1, Failed 1. `Test Run Failed.`

## The negative case — the required failing observation

```
Expected a <System.InvalidOperationException> to be thrown because a second, different provider must
be refused rather than silently discarded, but no exception was thrown.
```

Against the unfixed code, `InitializeBreadcrumbPipeline(provider, operations)` opens with a plain
already-initialized early return:

```csharp
if (BreadcrumbCoordinator != null)
{
    return;
}
```

That return does not inspect the supplied provider at all, so a second call carrying a genuinely
different `IFolderHierarchyProvider` returns silently and the new provider is **discarded without any
diagnostic**. No exception is thrown, the assertion fails, and that is the defect.

## The positive case already passes, and its outcome is recorded as observed

`InitializeBreadcrumbPipeline_RepeatSameProvider_DoesNotThrowAndKeepsCoordinator` records **Passed**
against the unfixed code. This task's acceptance explicitly anticipates that: "the positive case may
already pass and its outcome is recorded as observed."

It passes for a reason that will change under the fix. Today it passes because the blanket early
return refuses *every* second call, including one carrying the same provider, so nothing throws and
the coordinator is left in place. After `[P3-T3]` it will pass because the guard's reference
comparison finds the supplied provider reference-equal to the retained one and takes the
return-without-effect branch deliberately. The observable outcome is identical either way, which is
precisely why this case cannot discriminate and why the negative case is the one that carries the
fail-before evidence.

## TRX

`docs/features/active/itemviewer-breadcrumb-lifecycle-defects-488/evidence/regression-testing/p3-t2-d3-fail/488-d3-fail.trx`

Output Summary: EXIT_CODE 1 with `ExpectedExitCode: 1`.
`InitializeBreadcrumbPipeline_SecondDifferentProvider_ThrowsInvalidOperationException` records outcome
**Failed** against the unfixed code, with "no exception was thrown" — the second, different provider is
silently discarded by the blanket already-initialized early return. The positive case
`InitializeBreadcrumbPipeline_RepeatSameProvider_DoesNotThrowAndKeepsCoordinator` records **Passed**,
as observed.
