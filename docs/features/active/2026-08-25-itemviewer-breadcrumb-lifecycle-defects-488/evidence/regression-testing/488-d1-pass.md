# D1 — Pass-After Evidence ([P1-T6])

Timestamp: 2026-08-28T05-29

## Step 1 — intermediate build

Command (under `pwsh -NoProfile`, worktree root):

```
MSBuild.exe QuickFiler.Test\QuickFiler.Test.csproj /t:Build /m /p:Configuration=Debug /p:Platform=AnyCPU /nologo
```

GATE: none (intermediate build)
EXIT_CODE: 0 — `0 Error(s)`, 3 warnings, elapsed 00:00:03.21.

The `/p:Platform=AnyCPU` substitution for the task's stated `"/p:Platform=Any CPU"` is the documented
deviation recorded in full in `488-d1-fail.md`: `Any CPU` is the solution-level platform name and a
direct project build requires the project's own name `AnyCPU`, declared at
`QuickFiler.Test.csproj:12`. This build is not an analyzer or nullable gate.

## Step 2 — the test run

Command (under `pwsh -NoProfile`, worktree root):

```
vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation "/TestCaseFilter:FullyQualifiedName~ConfigureBreadcrumbDropDown_EnvironmentChange_DisposesOutgoingHostBeforeReplacement|FullyQualifiedName~ItemViewerDisposal_OwnsHostAndDetachesBothSurfaces|FullyQualifiedName~ConfigureBreadcrumbDropDown_RepeatedSameEnvironmentReusesPopupHost" "/Logger:trx;LogFileName=488-d1-pass.trx" /ResultsDirectory:docs\features\active\itemviewer-breadcrumb-lifecycle-defects-488\evidence\regression-testing\p1-t6-d1-pass
```

EXIT_CODE: 0

| Test | Outcome |
| --- | --- |
| `ConfigureBreadcrumbDropDown_EnvironmentChange_DisposesOutgoingHostBeforeReplacement` | **Passed** |
| `ItemViewerDisposal_OwnsHostAndDetachesBothSurfaces` | **Passed** |
| `ConfigureBreadcrumbDropDown_RepeatedSameEnvironmentReusesPopupHost` | **Passed** |

Total tests 3, Passed 3, **Failed 0**. `Test Run Successful.`

## The discriminating assertion now holds

`[P1-T4]` observed the **first** assertion failing, with the message "Expected a
`<System.ObjectDisposedException>` to be thrown ... but no exception was thrown", attributed to the
`theme.Should().Throw<ObjectDisposedException>(...)` statement. That is the `SetTheme` disposal-guard
observation identified by decision D-10a as the discriminating one.

After `[P1-T5]`'s fix the same assertion **passes**: `SetTheme("dark")` on the captured outgoing host
now reaches the host's `ThrowIfDisposed()` guard and throws `ObjectDisposedException`, because the
outgoing host is disposed by statement order before the replacement is constructed. The two
corroborating assertions — `Close` returning `false` and, after the drain, `DropDown.IsDisposed` being
`true` — also hold, as they did before the fix.

The post-drain `DropDown` disposal assertion was **not** the sole failing assertion at any point, so
the `d1-drain-blocker.md` branch of this task was not triggered and no wait, sleep, or delay was
added.

## Unmodified-file check (required by the plan's unmodified-file rule)

Command:

```
git diff --name-only 12465043e052fce66a1861bf1ddd037a1aa81afc -- QuickFiler.Test/Viewers/BreadcrumbDropDownIntegrationTests.cs QuickFiler.Test/Controllers/QfcItemControllerBreadcrumbDropDownTests.cs
```

**Output: no lines.**

This is what establishes the second conjunct of the two criteria `[P1-T13]` and `[P1-T14]` flip. Both
say the named test passes **unmodified**, and a `Passed` outcome alone would also be produced by a
test that had been edited into passing. The empty diff establishes that neither
`QuickFiler.Test/Viewers/BreadcrumbDropDownIntegrationTests.cs` nor
`QuickFiler.Test/Controllers/QfcItemControllerBreadcrumbDropDownTests.cs` differs by a single byte
from its state at `BASE_SHA` `12465043e052fce66a1861bf1ddd037a1aa81afc`.

In particular, `ItemViewerDisposal_OwnsHostAndDetachesBothSurfaces` passes with its
`host.Dispose()` `Times.Once()` assertion intact. That assertion is on a
`Mock<IBreadcrumbDropDownHost>`, which is not idempotent, and it stays green because `[P1-T5]`'s type
test names the **concrete** `BreadcrumbDropDownHost`: a mock host installed by an earlier
three-argument `ConfigureBreadcrumbDropDown` call fails that type test and is not disposed by the new
statement.

## TRX

`docs/features/active/itemviewer-breadcrumb-lifecycle-defects-488/evidence/regression-testing/p1-t6-d1-pass/488-d1-pass.trx`

Output Summary: EXIT_CODE 0, failed count **0**, all three named tests `Passed`. The discriminating
`SetTheme` assertion now holds where `[P1-T4]` observed it failing. `git diff --name-only <BASE_SHA>`
over `BreadcrumbDropDownIntegrationTests.cs` and `QfcItemControllerBreadcrumbDropDownTests.cs`
produces **no output lines**, establishing byte-identity for both.
