# quickfiler-test-assembly-resolve-self-sufficiency (Bug)

- Date captured: 2026-09-13
- Author: Dan Moisan
- Status: Active
- GitHub Issue: https://github.com/drmoisan/TaskMaster/issues/877
- Work Mode: minor-audit

## Summary

`QuickFiler.Test` is not self-sufficient for assembly resolution. It relies on a process-global
`AppDomain.CurrentDomain.AssemblyResolve` handler installed by a *different* test assembly,
`UtilitiesCS.Test`. Under class-level parallelism a `QuickFiler.Test` class can reach Deedle's static
initializer before that sibling handler is installed, and the Deedle load fails.

## Environment

- OS/version: Windows 11 Pro 10.0.26200
- Toolchain: MSBuild + vstest.console.exe, .NET Framework 4.8 test assemblies
- Command/flags used: `vstest.console.exe QuickFiler.Test.dll /Settings:scripts/vscode/TaskMaster.cli.runsettings /InIsolation /TestCaseFilter:TestCategory!=LiveOutlook`
- Settings file under test: `scripts/vscode/TaskMaster.cli.runsettings` (`<Workers>0</Workers>`, `<Scope>ClassLevel</Scope>`)

## Steps to Reproduce

1. Build the solution in Debug.
2. Run the `QuickFiler.Test` assembly through `vstest.console.exe` passing
   `/Settings:scripts/vscode/TaskMaster.cli.runsettings`.
3. Observe three failures in `QuickFiler.Controllers.Tests.QfcInitEmailQueueZeroBatchTests`.

## Expected Behavior

The full `QuickFiler.Test` suite passes with `Workers=0` and `Scope=ClassLevel` unchanged. An assembly's
tests must not depend on a side effect performed by a different test assembly.

## Actual Behavior

Three tests fail with an assembly-load failure at static-initializer time:

- `InitEmailQueue_ZeroBatchSize_ReturnsEmptyListWithoutThrowing`
- `InitEmailQueue_ZeroBatchSize_StillStartsBackgroundWorker`
- `InitEmailQueue_PositiveBatchSize_RetainsExistingProjectionAndFrameDrop`

Exception chain: `TypeInitializationException` for `Deedle.Reflection`, then for
`<StartupCode$Deedle>.$FrameUtils`, then `FileNotFoundException: netstandard, Version=2.1.0.0`.

## Suspected Cause / Notes

The shared mutable static state is `AppDomain.CurrentDomain.AssemblyResolve` — a process-global event
handler chain, mutated by one test assembly and silently depended upon by another.

- `UtilitiesCS.Test/TestAssemblyInitializer.cs` installs a process-wide `AssemblyResolve` fallback in its
  `[AssemblyInitialize]`, resolving by simple name plus public-key token. Its doc comment records the
  reason: vstest's testhost does not always honour binding redirects from the `.dll.config`, so an
  assembly referenced at one version but deployed at another raises `FileNotFoundException`.
- `QuickFiler.Test/SetupAssemblyInitializer.cs` installs no resolver. It calls only `EnableVisualStyles`
  and `SetCompatibleTextRenderingDefault`.
- Neither project's `app.config` carries a `netstandard` redirect: 73 `bindingRedirect` entries in
  `QuickFiler.Test/app.config`, 76 in `UtilitiesCS.Test/app.config`, zero mentioning `netstandard`.

So the Deedle load in `QuickFiler.Test` is satisfied only by the fallback that `UtilitiesCS.Test`
installs, and success depends on that sibling assembly's initializer having already run in the same
testhost process. Serial ordering happens to hold. The CLR caches the failed static initializer for the
process lifetime, which is why repeated runs look deterministic.

This is test hygiene, not a production thread-safety bug: production resolves these bindings through
`TaskMaster.exe.config`.

## Proposed Fix / Validation Ideas

Make `QuickFiler.Test` self-sufficient by installing the same fallback in its own `[AssemblyInitialize]`.
MSTest runs an assembly's own `[AssemblyInitialize]` before any test in that assembly, including under
parallelism, which removes the cross-assembly ordering dependency.

Share the resolver rather than duplicating it: one source file linked into both test projects with a
`<Compile Include="..." Link="..." />` item, so the two copies cannot drift.

## Out of Scope (non-negotiable)

The ruling is that tests must always run in parallel; a suite requiring serial execution has already
violated unit-test isolation, so anything reducing concurrency masks the defect instead of fixing it.
Explicitly prohibited: any change to `scripts/vscode/TaskMaster.cli.runsettings`; any `Workers` change;
removing or weakening the `Parallelize` block; `[DoNotParallelize]`; dropping or bypassing `/Settings:`;
retries; timing tolerance; sleeps; reordering tests; and any `[ClassInitialize]` hack that merely
front-runs the race.

## Acceptance Criteria

- [ ] A single shared source file containing the `AssemblyResolve` fallback exists in the repository and is the only copy of that resolver logic.
- [ ] The shared file is linked into both `QuickFiler.Test` and `UtilitiesCS.Test` using a `<Compile Include="..." Link="..." />` item in each project file.
- [ ] `QuickFiler.Test/SetupAssemblyInitializer.cs` installs the `AssemblyResolve` fallback in its `[AssemblyInitialize]`, alongside the existing `EnableVisualStyles` and `SetCompatibleTextRenderingDefault` calls.
- [ ] `UtilitiesCS.Test` behaviour is unchanged in effect: it still installs the same resolver from its own `[AssemblyInitialize]`, with the same resolution semantics.
- [ ] The explanatory comment is carried across and states why the resolver exists, naming vstest's testhost not honouring binding redirects.
- [ ] The full `QuickFiler.Test` suite passes with `Workers=0`, `Scope=ClassLevel`, and `/Settings:scripts/vscode/TaskMaster.cli.runsettings` still passed, with the run banner recording the parallelization mode, the totals, and exit code 0.
- [ ] `scripts/vscode/TaskMaster.cli.runsettings` is unmodified, and the diff contains no `[DoNotParallelize]`, no `Workers` change, no retry, no sleep, and no timing tolerance.
- [ ] The C# toolchain passes in CLAUDE.md order: CSharpier format check, .NET analyzers, nullable type-check, and MSTest.

## Next Step

- [x] Promote to GitHub issue (bug-report template) — pre-existing issue #877
- [x] Move to active fix folder / branch
