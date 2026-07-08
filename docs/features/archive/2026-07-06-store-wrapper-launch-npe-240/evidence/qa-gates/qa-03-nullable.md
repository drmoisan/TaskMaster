# QA Gate 03 — Nullable / TreatWarningsAsErrors (Issue #240)

Timestamp: 2026-07-06T07-45

## Solution-wide command (as specified by the plan)

Command: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`
(Invoked as `-t:Rebuild` for the reason recorded in the P0-T10 baseline: an incremental `-t:Build` skips `CoreCompile` for up-to-date outputs.)

EXIT_CODE: 1

This non-zero exit code reproduces the exact pre-existing condition recorded in the P0-T10 baseline (`evidence/baseline/nullable-baseline.md`): forcing `Nullable=enable` across the whole solution fails on the vendored `SVGControl.csproj` and `UtilitiesSwordfish.NET.General.csproj` projects, which are dependencies of `UtilitiesCS.csproj` and therefore block `UtilitiesCS`/`UtilitiesCS.Test` from even reaching `CoreCompile` in a full-solution run. This is unrelated to and pre-dates issue #240.

## Scoped verification (touched-file diagnostics)

To satisfy the acceptance clause "confirm 0 warnings/errors on the touched files," `UtilitiesCS.csproj` and `UtilitiesCS.Test.csproj` were rebuilt directly with `-p:BuildProjectReferences=false` (using the already-built vendored-project outputs) so `CoreCompile` actually runs against `StoreWrapperController.cs` and `StoreWrapperController_Tests.cs`.

Command: `msbuild TaskMaster.sln -t:UtilitiesCS:Rebuild,UtilitiesCS_Test:Rebuild -p:Configuration=Debug -p:Platform="Any CPU" -p:Nullable=enable -p:TreatWarningsAsErrors=true -p:BuildProjectReferences=false`
EXIT_CODE: 1 (2089 pre-existing errors across the wider `UtilitiesCS.csproj`, unrelated to this issue — the project has no `<Nullable>` project setting and was never nullable-annotated; forcing the flag surfaces the whole project's latent debt, not a regression from this change)

Output Summary: `StoreWrapperController_Tests.cs` contributes zero diagnostics. `StoreWrapperController.cs` contributes diagnostics only on pre-existing, unmodified code (constructor property initialization at line 77 — `CS8618` x8, unchanged since before this fix; and `SelectFolder`/`SelectFsFolder`'s pre-existing `return null;` statements at lines 345/352/371 — `CS8603` x3); these lines are unmodified original code, merely shifted by the new type declarations inserted above the class. A first attempt at this fix introduced 2 new `CS8625` diagnostics (`StoreLaunchReadiness.NotReady` passing `null` literals to non-nullable-typed constructor parameters, at line 50). This was fixed with a narrowly-scoped `#pragma warning disable CS8625` / `#pragma warning restore CS8625` around the two-argument `null, null` construction, documented in-code with a `why:` comment, per repo policy C#7 ("keep suppression as narrow as possible and document the rationale in-code"). A `?`-nullable-annotation approach was rejected because this project has no `<Nullable>` setting (implicit disable for normal builds), so declaring `StoresWrapper?`/`IList<string>?` would introduce new `CS8632` warnings during ordinary (non-forced) builds — confirmed empirically not to occur with the pragma approach (re-verified against the P3-T2 analyzer gate: 70 warnings, 0 errors, no new diagnostics on either touched file). After the fix, the scoped rebuild's total error count for `UtilitiesCS.csproj` dropped from 2091 to 2089 — exactly the 2 diagnostics eliminated, confirming the fix is isolated and introduces zero new nullable diagnostics.

## Verdict

No warnings or errors on `StoreWrapperController.cs` or `StoreWrapperController_Tests.cs` are attributable to the code introduced or modified by this issue. Both the solution-wide `EXIT_CODE: 1` and the scoped rebuild's `EXIT_CODE: 1` are pre-existing, unrelated conditions (vendored-project nullable debt and whole-project nullable debt, respectively), identical in kind to the P0-T10 baseline. This is recorded transparently rather than reported as a false `EXIT_CODE: 0`, per the fail-closed evidence rule.
