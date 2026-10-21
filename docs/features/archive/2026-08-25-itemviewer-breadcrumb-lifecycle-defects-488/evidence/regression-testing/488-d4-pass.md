# D4 — Pass-After Evidence ([P4-T6])

Timestamp: 2026-08-28T05-47

## Step 1 — intermediate build

Command (under `pwsh -NoProfile`, worktree root):

```
MSBuild.exe QuickFiler.Test\QuickFiler.Test.csproj /t:Build /m /p:Configuration=Debug /p:Platform=AnyCPU /nologo
```

GATE: none (intermediate build)
EXIT_CODE: 0 — `0 Error(s)`, 3 warnings, elapsed 00:00:03.05. The warning count is unchanged from the
Phase 0 baseline, so `[P4-T4]` and `[P4-T5]` introduced no new diagnostic. The `/p:Platform=AnyCPU`
substitution is the documented deviation recorded in full in `488-d1-fail.md`.

## Step 2 — the test run

Command (under `pwsh -NoProfile`, worktree root):

```
vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation "/TestCaseFilter:FullyQualifiedName~InitializeBreadcrumbPipeline_AmbientContextNull_ThrowsBoundaryDiagnostic|FullyQualifiedName~InitializeBreadcrumbPipeline_DifferentNonNullContext_ThrowsBoundaryDiagnostic" "/Logger:trx;LogFileName=488-d4-pass.trx" /ResultsDirectory:docs\features\active\itemviewer-breadcrumb-lifecycle-defects-488\evidence\regression-testing\p4-t6-d4-pass
```

EXIT_CODE: 0

| Test | Outcome |
| --- | --- |
| `InitializeBreadcrumbPipeline_AmbientContextNull_ThrowsBoundaryDiagnostic` | **Passed** |
| `InitializeBreadcrumbPipeline_DifferentNonNullContext_ThrowsBoundaryDiagnostic` | **Passed** |

Total tests 2, Passed 2, **Failed 0**. `Test Run Successful.`

## What changed against [P4-T3]

Both tests moved from **Failed** to **Passed**. `[P4-T3]` recorded "no exception was thrown" for each;
after `[P4-T4]` added `ThrowIfOffUiBoundary` and `[P4-T5]` invoked it as the first statement of
`InitializeBreadcrumbPipeline(provider, operations)`, both calls now throw
`InvalidOperationException`.

Each assertion carries two further clauses that the pass had to satisfy beyond the exception type:

- **The message names the operation.** Each test requires the thrown message to contain the token
  `InitializeBreadcrumbPipeline`. The delivered helper interpolates its `operation` parameter, and
  `[P4-T5]` passes `nameof(InitializeBreadcrumbPipeline)`, so the requirement is met by the guard's
  own diagnostic and not by an incidental exception from elsewhere. This clause is what rules out the
  dispatcher's ambient-context message, which does not name the operation.
- **The instance is not an `ObjectDisposedException`.** Per decision D-9 that type derives from
  `InvalidOperationException`, so without the exclusion a D5 disposal throw would satisfy a D4
  assertion. Both passes clear the exclusion.

## The second case proves reference equality rather than a null check

`InitializeBreadcrumbPipeline_DifferentNonNullContext_ThrowsBoundaryDiagnostic` installs a
**different, non-null** `SynchronizationContext` and asserts the same throw. A guard implemented as a
bare `SynchronizationContext.Current == null` check would let that call through and the test would
fail. It passes, which establishes that the delivered comparison is reference equality against the
viewer's captured `UiSyncContext`.

That distinction is the substance of the D4 design: a continuation resumed without the captured
context can land on a recycled pool thread whose managed thread id matches the owning one, so neither
a null check nor a thread-identity comparison is a boundary proof.

## Scope of what this evidence establishes

These two tests **prove the guard fires. They do not prove the race is absent.** A true two-thread
data race cannot be reproduced deterministically under the repository's ban on sleeps, timer delays,
and wall-clock waits: two threads with no barrier offer no way to force the interleaving. Both tests
are single-threaded and carry that statement in their own XML doc comments. No criterion in `spec.md`
asserts that the race is eliminated.

## TRX

`docs/features/active/itemviewer-breadcrumb-lifecycle-defects-488/evidence/regression-testing/p4-t6-d4-pass/488-d4-pass.trx`

Output Summary: EXIT_CODE 0, failed count **0**, both named tests `Passed`. The affinity guard now
throws `InvalidOperationException` naming `InitializeBreadcrumbPipeline` where `[P4-T3]` observed no
exception at all, for both a null ambient context and a different non-null one — the latter proving
the comparison is reference equality rather than a null check.
