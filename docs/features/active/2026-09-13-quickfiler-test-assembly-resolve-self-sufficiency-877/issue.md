# quickfiler-test-assembly-resolve-self-sufficiency (Bug)

- Date captured: 2026-09-13
- Author: Dan Moisan
- Status: Active
- GitHub Issue: https://github.com/drmoisan/TaskMaster/issues/877
- Work Mode: minor-audit

## Summary

`QuickFiler.Test` is not self-sufficient for assembly resolution. Three of its tests load Deedle, and
loading Deedle requires an assembly bind that nothing in the repository or on the machine can satisfy
on its own: `netstandard, Version=2.1.0.0, PublicKeyToken=cc7b13ffcd2ddd51`. That bind succeeds only
when a process-global `AppDomain.CurrentDomain.AssemblyResolve` fallback matching on simple name plus
public key token is already installed. `QuickFiler.Test` installs no such fallback. It has been
borrowing one by accident from `SVGControl`, whose resolver is installed lazily from the
`SVGControl.SvgRenderer` static constructor and therefore only after some earlier test class has touched
an SVG-bearing control. When the Deedle-using class runs before any such class, the bind fails.

The fix is to install the fallback in `QuickFiler.Test`'s own `[AssemblyInitialize]`, which MSTest runs
before any test in that assembly, so the assembly no longer depends on what ran before it.

## Environment

- OS/version: Windows 11 Pro 10.0.26200
- Toolchain: MSBuild + vstest.console.exe, .NET Framework 4.8 test assemblies
- Command/flags used: `vstest.console.exe QuickFiler.Test.dll /Settings:scripts/vscode/TaskMaster.cli.runsettings /InIsolation /TestCaseFilter:TestCategory!=LiveOutlook`
- Settings file under test: `scripts/vscode/TaskMaster.cli.runsettings` (`<Workers>0</Workers>`, `<Scope>ClassLevel</Scope>`)

## Steps to Reproduce

The deterministic reproduction is the single class run, not the suite run. A suite run can pass or fail
depending on which class the scheduler starts first, so it is not a reliable repro.

1. Build the solution in Debug.
2. Run `QuickFiler.Controllers.Tests.QfcInitEmailQueueZeroBatchTests` alone, with no runsettings file, so
   that no other class has had the opportunity to install a resolver:
   `vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /TestCaseFilter:"FullyQualifiedName~QfcInitEmailQueueZeroBatchTests" /InIsolation`
3. Observe that all three tests fail at Deedle's static initializer.

## Expected Behavior

`QuickFiler.Test` resolves every assembly its own tests need, without depending on a side effect
performed by a different test assembly or by an unrelated control library that a different test class
happens to touch first. Concretely, the Deedle-using class must pass when it is the only class in the
run.

## Actual Behavior

Three tests fail with an assembly-load failure at static-initializer time:

- `InitEmailQueue_ZeroBatchSize_ReturnsEmptyListWithoutThrowing`
- `InitEmailQueue_ZeroBatchSize_StillStartsBackgroundWorker`
- `InitEmailQueue_PositiveBatchSize_RetainsExistingProjectionAndFrameDrop`

Exception chain: `TypeInitializationException` for `Deedle.Reflection`, then for
`<StartupCode$Deedle>.$FrameUtils`, then
`FileNotFoundException: netstandard, Version=2.1.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51`.

## Verified Mechanism

Every link below was measured in this worktree at head `c9590a8b7`.

1. `Deedle.dll` is deployed as `Deedle, Version=3.0.0.0, PublicKeyToken=null`. Its assembly-reference
   table names `FSharp.Core 4.5.0.0` and `netstandard 2.0.0.0`.
2. `QuickFiler.Test/app.config` (line 46) and `UtilitiesCS.Test/app.config` (line 38) both redirect
   FSharp.Core to 11.0.0.0.
3. The deployed `FSharp.Core.dll` is `FSharp.Core, Version=11.0.0.0, PublicKeyToken=b03f5f7f11d50a3a`,
   and its assembly-reference table names `netstandard 2.1.0.0`. The netstandard 2.1.0.0 requirement
   therefore enters the closure through the FSharp.Core redirect, not through Deedle, which asks only
   for netstandard 2.0.0.0.
4. `netstandard, Version=2.1.0.0` exists nowhere on this machine. It is absent from
   `QuickFiler.Test/bin/Debug`, and the GAC holds exactly one netstandard: `v4.0_2.0.0.0__cc7b13ffcd2ddd51`.
5. No `*.config` file anywhere in the repository contains the string `netstandard`, so no binding
   redirect covers the reference in any host.
6. The bind is therefore satisfiable only by an `AssemblyResolve` fallback that matches on simple name
   plus public key token. netstandard 2.0.0.0 in the GAC carries the same token, `cc7b13ffcd2ddd51`, so
   such a handler resolves the request while the default binder cannot.
7. The repository contains exactly two first-party `AssemblyResolve` handlers:
   - `UtilitiesCS.Test/TestAssemblyInitializer.cs`, installed from that assembly's `[AssemblyInitialize]`.
     It cannot run in a process that executes only `QuickFiler.Test`.
   - `SVGControl/SvgAssemblyResolver.cs`, installed by `SvgAssemblyResolver.Install()` from the
     `SVGControl.SvgRenderer` static constructor (`SVGControl/SvgRenderer.cs` lines 25-28). This
     installation is lazy: it happens only when something touches `SvgRenderer`.
8. `QuickFiler.Test` reaches `SvgRenderer` indirectly, through QuickFiler's SVG-bearing WinForms
   viewers - `QuickFiler/Viewers/ItemViewer.Designer.cs` and `QuickFiler/Viewers/EfcViewer.Designer.cs`
   instantiate `SVGControl.ButtonSVG` and `SVGControl.SvgResource`. Any test class that builds one of
   those viewers installs the resolver for the rest of the process.

So `QuickFiler.Test` genuinely lacks a resolver it requires, and has been borrowing one by accident from
whichever earlier class happened to touch `SvgRenderer` first. The CLR caches a failed type initializer
for the lifetime of the process, so once `Deedle.Reflection` has failed once in a testhost, it fails for
every later test in that testhost regardless of what is installed afterwards.

### M-matrix

Three runs in this worktree at head `c9590a8b7`, before any fix.

| Run | Command shape | Result |
|---|---|---|
| M2 | `QuickFiler.Test.dll` alone, `/Settings:scripts/vscode/TaskMaster.cli.runsettings`, `/InIsolation`, `/TestCaseFilter:"TestCategory!=LiveOutlook"` | Total 1395, Passed 1395, exit 0. A prior session ran the identical command shape, also selecting Total 1395, and observed 1392 passed with 3 FAILED and a non-zero exit, so this run shape is NONDETERMINISTIC across runs |
| M3 | `QuickFiler.Test.dll`, `/TestCaseFilter:"FullyQualifiedName~QfcInitEmailQueueZeroBatchTests"`, `/InIsolation`, no runsettings | Total 3, Failed 3, exit 1, `FileNotFoundException: netstandard, Version=2.1.0.0` |
| M6 | `QuickFiler.Test.dll`, `/TestCaseFilter:"FullyQualifiedName~QfcInitEmailQueueZeroBatchTests\|FullyQualifiedName~BreadcrumbDropDownIntegrationTests"`, `/InIsolation`, no runsettings | Total 13, Passed 10, Failed 3, exit 1; the zero-batch class executed first and failed. A prior-session run of a different shape, Total 9 against Total 13, selected a different set of tests and is not comparable with this row |

M3 is the stable discriminator. It is strictly stricter than any suite run, because a suite run lets any
earlier class rescue the bind invisibly. M2 and M3 differ only in how many classes are in the run, and
they disagree, which is what localises the defect to intra-assembly ordering.

M2 has additionally been observed to disagree with itself. Two runs of the identical M2 command each
selected 1395 tests; one reported 1395 passed with a zero exit, and the other reported 1392 passed with
3 FAILED and a non-zero exit. A passing M2 run, and a passing full-suite run, are therefore
non-probative for this fix: neither can confirm it and neither can refute it. The M2 disagreement alone
is sufficient to establish that non-probative status, and no other run shape is required for it.

M6 is a single-run demonstration that the zero-batch class fails when the runner schedules it first,
even though an SVG-bearing class is present later in the same sequential run. Its two recorded
observations have different totals, 9 against 13, so they did not select the same set of tests. They are
not offered as a same-command disagreement, and no claim is made that M6 disagreed with itself.

## Refuted Explanations

Both explanations below were held with confidence and are recorded here with the evidence that killed
them, so that a later reader does not re-derive either one. This blocker has consumed two wrong theories
already.

### Refuted: sibling-assembly initialiser ordering

The original attribution was that `QuickFiler.Test` depends on the `AssemblyResolve` handler installed
by `UtilitiesCS.Test/TestAssemblyInitializer.cs`, and that under class-level parallelism a
`QuickFiler.Test` class can reach Deedle before that sibling handler is installed.

Refuted by M2. `QuickFiler.Test` ran alone in its own testhost. `UtilitiesCS.Test` was not in the run, so
its `[AssemblyInitialize]` never executed and its handler was never installed. All 1395 tests passed,
including the three Deedle-using tests. A handler that is not present cannot be the thing that rescues
the bind. M3 then shows the same assembly, in the same host configuration, failing when only the
Deedle-using class is selected. The variable is therefore ordering *within* `QuickFiler.Test`, not the
presence of a sibling assembly.

### Refuted: Deedle static-initialiser race

The follow-on theory was that the failure is a race on Deedle's own static initialiser, and that the fix
is to force Deedle's initialisation from `QuickFiler.Test`'s `[AssemblyInitialize]` so that it completes
once, before any parallel class can enter it.

Refuted by deduction from M3, before it was implemented. `[AssemblyInitialize]` runs before every test
class in the assembly, so it runs before any class that could touch `SvgRenderer`, which means it runs
in exactly the process state M3 measures: no resolver installed, netstandard 2.1.0.0 unsatisfiable. The
forced initialisation would therefore fail, and because the CLR caches a failed type initialiser for the
process lifetime, every Deedle-touching test in the run would then fail deterministically. The proposed
fix would have converted three order-dependent failures into a deterministic failure of the whole
Deedle-using surface. It was not implemented.

## Production Risk

The same unsatisfiable bind exists in production, and production has only the lazy `SVGControl` handler
available. The add-in appears safe only because the VSTO surface renders SVG before anything reaches
Deedle, which is an ordering accident of the current UI flow rather than a guarantee. The production
path was not investigated as part of this issue. It is tracked separately as
https://github.com/drmoisan/TaskMaster/issues/879 and is explicitly out of scope here.

## Proposed Fix / Validation Ideas

Install the `AssemblyResolve` fallback in `QuickFiler.Test`'s own `[AssemblyInitialize]`, in
`QuickFiler.Test/SetupAssemblyInitializer.cs`, alongside the existing `EnableVisualStyles` and
`SetCompatibleTextRenderingDefault` calls. MSTest runs an assembly's own `[AssemblyInitialize]` before
any test in that assembly, including under parallelism, so this removes the ordering dependence
entirely.

Share the resolver between `QuickFiler.Test` and `UtilitiesCS.Test` from a single source file if that is
clean, so the two copies cannot drift. There is no `Link=` convention anywhere in this repository to
match, so the sharing mechanism is an open choice and must be justified in the change description. If
sharing forces awkwardness, duplicate between exactly those two test projects and record why.

Carry the explanatory comment across, and state both reasons the resolver exists: vstest's testhost does
not honour the binding redirects in the test assembly's `.dll.config`, and the netstandard 2.1.0.0 bind
that nothing on the machine satisfies. Without the second reason a later reader can conclude the handler
is redundant and delete it.

## Out of Scope (non-negotiable)

`SVGControl` must not be modified. Consolidating the three near-identical resolvers into production code
is a separate decision with a different blast radius and must not ride along inside a test-hygiene item.
Three copies of the resolver will exist after this change, and that is accepted for now.

The production risk recorded above is out of scope; it is tracked as issue #879.

The ruling is that tests must always run in parallel; a suite requiring serial execution has already
violated unit-test isolation, so anything reducing concurrency masks the defect instead of fixing it.
Explicitly prohibited: any change to `scripts/vscode/TaskMaster.cli.runsettings`; any `Workers` change;
removing or weakening the `Parallelize` block; `[DoNotParallelize]`; dropping or bypassing `/Settings:`;
retries; timing tolerance; sleeps; reordering tests; and any `[ClassInitialize]` hack that merely
front-runs the race.

## Acceptance Criteria

- [x] `QuickFiler.Test/SetupAssemblyInitializer.cs` installs an `AssemblyResolve` fallback in its own `[AssemblyInitialize]`, alongside the existing `EnableVisualStyles` and `SetCompatibleTextRenderingDefault` calls, and the fallback resolves by simple name plus public key token.
- [x] PRIMARY GUARD: `QuickFiler.Controllers.Tests.QfcInitEmailQueueZeroBatchTests` passes when it is the only class in the run and no runsettings file is passed, with the run recording the totals and exit code 0. This is the criterion that proves self-sufficiency; a suite run cannot, because any earlier class can rescue the bind invisibly.
- [x] The resolver logic is shared between `QuickFiler.Test` and `UtilitiesCS.Test` from a single source file, with the sharing mechanism justified in the change description; or, if sharing forces awkwardness, it is duplicated between exactly those two test projects with the reason recorded. `SVGControl` is not modified and the scope is confined to the two test projects.
- [x] `UtilitiesCS.Test` behaviour is unchanged in effect: it still installs the same resolver from its own `[AssemblyInitialize]`, with the same resolution semantics.
- [x] The explanatory comment states why the resolver exists, naming both vstest's testhost not honouring binding redirects and the netstandard 2.1.0.0 bind that nothing on the machine satisfies.
- [x] The full `QuickFiler.Test` suite passes with `Workers=0`, `Scope=ClassLevel`, and `/Settings:scripts/vscode/TaskMaster.cli.runsettings` still passed, with the run banner recording the parallelization mode, the totals, and exit code 0.
- [x] `scripts/vscode/TaskMaster.cli.runsettings` is unmodified, and the diff contains no `[DoNotParallelize]`, no `Workers` change, no retry, no sleep, and no timing tolerance.
- [x] The C# toolchain passes in CLAUDE.md order: CSharpier format check, .NET analyzers, nullable type-check, and MSTest.

## Next Step

- [x] Promote to GitHub issue (bug-report template) — pre-existing issue #877
- [x] Move to active fix folder / branch
