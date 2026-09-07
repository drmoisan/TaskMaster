# #475 Part 2 — Fail-Before Evidence for the Constructor Test ([P6-T6]) `[expect-fail]`

Timestamp: 2026-08-28T06-07

## Why a temporary revert was required

`[P6-T3]` delivers #475 parts 1 and 2 as **one compile-valid edit set**: the `CaptureCurrentOrTests`
declaration is deleted and all five production references are repointed in the same change. There is no
intermediate state in which the constructor still degrades silently and the tree still compiles, so the
fail-before observation for `LegacySurfaceFactoryConstructor_AmbientContextNull_ThrowsInvalidOperationException`
cannot be taken by simply running the test at an earlier point in plan order.

The observation was therefore taken by temporarily reverting **only the two `BreadcrumbDropDownHost`
identifier swaps**. The deletion itself was not reverted, because the replacement boundary test in
`BreadcrumbPopupBoundaryCoverageTests.Part2.cs` would then not compile.

The reverted state was spelled as a **restored local selector private to `BreadcrumbDropDownHost.cs`**,
which is the first of the two forms this task permits:

```csharp
        // TEMPORARY ([P6-T6] fail-before observation only). Reproduces the deleted ambient-probing
        // selector locally so the two seven-parameter constructor chains stop failing fast. Removed
        // immediately after the observation is taken.
        private static BreadcrumbPopupUiOperations TemporaryRevertCaptureCurrentOrTests() =>
            System.Threading.SynchronizationContext.Current == null
                ? BreadcrumbPopupUiOperations.CreateForCurrentThreadTests()
                : BreadcrumbPopupUiOperations.CaptureCurrent();
```

The two seven-parameter constructor chains at lines 98 and 118 were pointed at it. **A third site at
line 54 was deliberately left alone**: it already read `BreadcrumbPopupUiOperations.CaptureCurrent()`
at `BASE_SHA` and is not one of the two swaps `[P6-T3]` made, so reverting it would have gone beyond
the two identifier swaps this task authorizes. That was verified against
`git show <BASE_SHA>:QuickFiler/Viewers/BreadcrumbDropDownHost.cs`.

## Step 1 — reverted-state rebuild (load-bearing)

Command (under `pwsh -NoProfile`, worktree root):

```
MSBuild.exe QuickFiler.Test\QuickFiler.Test.csproj /t:Build /m /p:Configuration=Debug /p:Platform=AnyCPU /nologo
```

GATE: none (intermediate build)
**Reverted-state rebuild EXIT_CODE: 0** — `0 Error(s)`, 3 warnings, elapsed 00:00:03.04.

The rebuild is load-bearing. `vstest.console.exe` runs the compiled assembly, not the source, so
reverting the source without rebuilding would have left the fail-fast swaps in the binary, the test
would have passed, and this task's `[expect-fail]` acceptance could never have been met. The exit code
0 is what makes the `Failed` outcome below attributable to the reverted behaviour rather than to a
stale or broken assembly.

## Step 2 — the failing test run, in the reverted state

Command (under `pwsh -NoProfile`, worktree root):

```
vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation "/TestCaseFilter:FullyQualifiedName~LegacySurfaceFactoryConstructor_AmbientContextNull_ThrowsInvalidOperationException" "/Logger:trx;LogFileName=475-ctor-fail.trx" /ResultsDirectory:docs\features\active\itemviewer-breadcrumb-lifecycle-defects-488\evidence\regression-testing\p6-t6-475-ctor-fail
```

EXIT_CODE: 1
ExpectedExitCode: 1

| Test | Outcome (reverted state) |
| --- | --- |
| `LegacySurfaceFactoryConstructor_AmbientContextNull_ThrowsInvalidOperationException` | **Failed** |

Total tests 1, Failed 1. `Test Run Failed.`

```
Expected a <System.InvalidOperationException> to be thrown because the public constructor must refuse
to run without an owning boundary, but found <System.ArgumentNullException>:
System.ArgumentNullException: Value cannot be null.
```

With the ambient-probing selector restored, the constructor does **not** fail fast: under a null
ambient context the selector silently substitutes a test dispatcher, construction proceeds, and the
only exception raised is an unrelated `ArgumentNullException` from a null argument further along. That
silent substitution is precisely the degradation #475 exists to remove.

## Step 3 — restore, and restore-state rebuild

The two identifier swaps were **restored immediately** after the observation, by overwriting the file
with a byte-exact snapshot taken before the revert. The restored file's SHA-256 is
`990f4f6a5d55094a9266020396bc9368025ab30062154b8c3de0a9e46001889e`, identical to the snapshot taken
before the revert, and a search for `TemporaryRevertCaptureCurrentOrTests` in the file returns **0**
occurrences, so the temporary helper is gone.

**Restore-state rebuild EXIT_CODE: 0** — `0 Error(s)`, 3 warnings, elapsed 00:00:02.79.

## Step 4 — post-restore diff state

| Command | Output |
| --- | --- |
| `git diff --name-only <BASE_SHA> -- QuickFiler/Viewers/BreadcrumbDropDownHost.cs` | `QuickFiler/Viewers/BreadcrumbDropDownHost.cs` — reported as **changed**, with the swaps in place |
| `git diff --numstat <BASE_SHA> -- QuickFiler/Viewers/BreadcrumbDropDownHost.cs` | `2	2` — exactly two added and two deleted lines, one pair per constructor chain |
| `git diff --name-only <BASE_SHA> -- QuickFiler/Viewers/BreadcrumbPopupUiOperations.cs QuickFiler/Viewers/ItemViewer.Breadcrumb.cs` | both files reported as changed, in their `[P6-T3]` state, restored unchanged by the revert cycle |

The `2 2` numstat confirms the restore left exactly the two identifier swaps and nothing else: the
temporary helper added no residual line, and no constructor argument was reordered.

## TRX

`docs/features/active/itemviewer-breadcrumb-lifecycle-defects-488/evidence/regression-testing/p6-t6-475-ctor-fail/475-ctor-fail.trx`

Output Summary: EXIT_CODE 1 with `ExpectedExitCode: 1`. In the reverted state
`LegacySurfaceFactoryConstructor_AmbientContextNull_ThrowsInvalidOperationException` records outcome
**Failed**, finding `ArgumentNullException` instead of the expected `InvalidOperationException` because
the restored ambient-probing selector silently substituted a test dispatcher. The reverted-state
rebuild exited **0** and the restore rebuild exited **0**, so the failure is attributable to the
reverted behaviour and not to a stale assembly. The two identifier swaps were restored immediately, the
file's SHA-256 matches its pre-revert snapshot, and the post-restore diff is exactly `2 2`.
