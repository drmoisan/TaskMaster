# system-reactive-7-packages-config-migration (Issue #395)

- Date captured: 2026-07-20
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/system-reactive-7-packages-config-migration/ (Issue #395)

- Issue: #395
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/395
- Last Updated: 2026-07-21
## Problem / Why

The package update merged in PR #391 moved `System.Reactive` to 7.0.0 in five projects (`QuickFiler`, `TaskMaster`, `ToDoModel`, `UtilitiesCS`, `UtilitiesCS.Test`). System.Reactive 7.0 no longer supports `packages.config`-based projects: its `System.Reactive.PackagesConfigCheck.targets` emits an MSBuild warning on every build of each affected project ("The project contains a packages.config file, which is not supported by System.Reactive v7.0 or later. Please migrate to PackageReference."). The repository is running an explicitly unsupported configuration, and the warning repeats five times per build.

## Proposed Behavior

Resolve the unsupported System.Reactive 7.0 + packages.config combination in one of two ways (decision required):

1. Migrate the five affected projects from `packages.config` to `PackageReference`. Note this conflicts with the current analyzer wiring convention, which deliberately keeps legacy non-SDK VSTO projects on `packages.config` with file-based `<Analyzer Include>` items (see `.claude/rules/csharp.md`, Analyzer Stack mechanism). A migration must preserve analyzer wiring, binding redirects, and VSTO build behavior.
2. Alternatively, pin `System.Reactive` back to the last 6.x release in the affected projects, or set the documented `RxUseUnsupportedPackagesConfig=true` escape hatch with an in-repo rationale, accepting the unsupported status explicitly.

## Acceptance Criteria (early draft)

- [ ] Builds of all five affected projects produce no `System.Reactive.PackagesConfigCheck` warning.
- [ ] The chosen approach is documented (decision record or rule update), including analyzer-wiring implications for legacy projects.
- [ ] Full C# toolchain (CSharpier, analyzer build, TreatWarningsAsErrors rebuild, MSTest suite) passes with no regressions.

## Constraints & Risks

- The affected projects are legacy non-SDK .NET Framework 4.8.1 / VSTO projects; PackageReference migration in such projects is non-trivial and can change restore layout (`packages\` folder disappears), breaking `<HintPath>` and `<Analyzer Include>` items and at least one test fixture that resolves files under `packages\`.
- The long-term direction is to leave VSTO entirely, which may argue for the low-effort pin/escape-hatch option rather than investing in migration.
- Suppressing the warning via `RxUseUnsupportedPackagesConfig=true` leaves the repo on an unsupported package configuration.

## Test Conditions to Consider

- [ ] Full solution build with `/p:TreatWarningsAsErrors=true` remains green.
- [ ] MSTest suite passes for all eight solution test assemblies.
- [ ] Rx-dependent runtime paths in QuickFiler/ToDoModel behave unchanged (binding redirects still resolve System.Reactive 7.0.0.0).

## Next Step

- [x] Promote to GitHub issue (feature request template)
- [ ] Create `docs/features/active/system-reactive-7-packages-config-migration/` folder from the template
