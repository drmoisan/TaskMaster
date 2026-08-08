# Attribution of the Two Full-Suite Failures — Controlled Experiment

Timestamp: 2026-08-08T16-52

Context: [P2-T5] failed with `Failed: 2` on two consecutive toolchain passes. This artifact
determines whether those failures are caused by this change. Conclusion: **they are not.** They
reproduce identically at merge-base with the change fully reverted.

## The two failing tests

Both in `QuickFiler.Test`, class `QuickFiler.Controllers.Tests.QfcItemController_InitializationTests`:

- `InitializeBool_ThroughThePumpHost_CompletesAndInitializesState`
- `InitializeNineArgOverload_ThroughThePumpHost_SavesParametersAndDelegates`

Both fail with the same exception:

```
System.InvalidOperationException: Invoke or BeginInvoke cannot be called on a control
until the window handle has been created.
   at System.Windows.Forms.Control.MarshaledInvoke(...)
   at QuickFiler.Controllers.QfcItemController.InvokeBeginInvoke(Boolean async, Action action)
        in QuickFiler\Controllers\QfcItemController.FocusAndTheme.cs:line 256
   at QuickFiler.Controllers.QfcItemController.ToggleTips(Boolean async, ToggleState desiredState)
        in QuickFiler\Controllers\QfcItemController.FocusAndTheme.cs:line 204
   at QuickFiler.Controllers.QfcItemController.Initialize(Boolean async)
        in QuickFiler\Controllers\QfcItemController.Initialization.cs:line 185
   at ... QuickFiler.Test\TestSupport\WinFormsPumpHost.cs:line 95
```

This is a WinForms window-handle-creation race in a test pump harness. It involves no WPF
`Dispatcher`, no `WpfDispatcherYield`, and no code in the scoped diff.

## Experiment design

Four runs, varying only the presence of the change:

| # | Configuration | Scope | Command |
|---|---|---|---|
| A | change present | `QfcItemController_InitializationTests` only | `vstest QuickFiler.Test.dll /InIsolation /TestCaseFilter:FullyQualifiedName~QfcItemController_InitializationTests` |
| B | change present | full `QuickFiler.Test` assembly alone | `vstest QuickFiler.Test.dll /InIsolation` |
| C | change present | full 9-assembly instrumented suite | `Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug` |
| D | **change reverted to merge-base** | full 9-assembly instrumented suite | `Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug` |

## Results

| # | EXIT_CODE | Total | Passed | Failed | Failing tests |
|---|---|---|---|---|---|
| A | 0 | 9 | 9 | 0 | none |
| B | 0 | 867 | 867 | 0 | none |
| C (pass 1) | 1 | 6295 | 6293 | 2 | the two above |
| C (pass 2) | 1 | 6295 | 6293 | 2 | the two above |
| **D (baseline)** | **1** | **6293** | **6291** | **2** | **the two above** |

## Conclusion: pre-existing, not caused by this change

Run D is the decisive one. With `UtilitiesCS/OutlookObjects/Folder/WpfDispatcherYield.cs` and
`UtilitiesCS.Test/OutlookObjects/Folder/WpfDispatcherYieldTests.cs` restored to merge-base
`003c5715` (`git status --porcelain -- '*.cs' '*.csproj' '*.sln'` empty) and the solution rebuilt,
the combined suite produced:

```
Total tests: 6293
     Passed: 6291
     Failed: 2
  Failed InitializeBool_ThroughThePumpHost_CompletesAndInitializesState [326 ms]
  Failed InitializeNineArgOverload_ThroughThePumpHost_SavesParametersAndDelegates [118 ms]
```

The same two tests, the same exception. The change is therefore exonerated.

This figure — `Total tests: 6293, Passed: 6291, Failed: 2` — is a **byte-for-byte match** with the
"Run 1" baseline already recorded in `<FEATURE>/issue.md:53`, captured before this work began. The
issue's own evidence documents this exact failure count at this exact merge-base.

Runs A and B further localize the defect: the two tests pass in class isolation (9/9) and pass in
their own full assembly (867/867). They fail only inside the combined, `dotnet-coverage`-instrumented
9-assembly run, which is the signature of a timing/ordering-sensitive WinForms handle race amplified
by instrumentation overhead — not a functional regression.

## Why the P0-T10 baseline was green

`<FEATURE>/evidence/baseline/tests-coverage.2026-08-08T16-22.md` recorded 6293/6293 with 0 failures.
That was a single sample of an intermittently-failing pair. It does not contradict run D; it
confirms that these two QuickFiler tests are themselves nondeterministic, which is precisely the
class of defect issue #508 exists to address (in a different test). The suite is not reliably green
at baseline — `<FEATURE>/issue.md:45-54` states this explicitly as the motivation for the issue.

## Experiment integrity

The change was saved and restored by file copy rather than `git stash`, and verified by hash:

| File | SHA-256 before experiment | SHA-256 after restore |
|---|---|---|
| `WpfDispatcherYield.cs` | `02986C1CDEC194DCEC4EA56852EF7EC03B74F0AF4729009568C8D924C352A364` | `02986C1CDEC194DCEC4EA56852EF7EC03B74F0AF4729009568C8D924C352A364` |
| `WpfDispatcherYieldTests.cs` | `4374A608616CA16767384DC69518A7E1EBD8C07F2E4189CA81C4EF3F2FE28701` | `4374A608616CA16767384DC69518A7E1EBD8C07F2E4189CA81C4EF3F2FE28701` |

Both hashes are identical, so the change was restored byte-for-byte with no drift. The temporary
attribution coverage report written to `coverage/attribution-baseline.cobertura.xml` was deleted; no
evidence artifact was written outside `<FEATURE>/evidence/`.

## Scope position

Fixing the QuickFiler handle race would require editing
`QuickFiler/Controllers/QfcItemController.FocusAndTheme.cs` or
`QuickFiler.Test/TestSupport/WinFormsPumpHost.cs`. Both are outside this plan's two-file scope
boundary, and neither is `TaskMaster/Ribbon/**`. It is a separate defect deserving its own issue,
and it is escalated rather than absorbed. No `[Ignore]`, `[DoNotParallelize]`, test-case filter, or
retry was added to route around it.

Output Summary: The two `QuickFiler.Test` failures blocking the P2-T5 gate are PRE-EXISTING and NOT
caused by this change. A controlled four-run experiment shows they pass in class isolation (9/9) and
in their own assembly (867/867) but fail in the combined instrumented suite both with the change
(6295/6293/2, twice) and — decisively — with the change fully reverted to merge-base
(6293/6291/2), which byte-for-byte matches the "Run 1" baseline already recorded at `issue.md:53`.
The change was restored with SHA-256 verification. Root cause is a WinForms window-handle race in
`QfcItemController`, outside the scope boundary; escalated, not worked around.
