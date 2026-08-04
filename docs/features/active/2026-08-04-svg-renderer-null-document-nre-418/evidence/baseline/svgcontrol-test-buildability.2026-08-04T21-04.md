# [P0-T10] Baseline Buildability State of `SVGControl.Test` — re-capture on VSTO-enabled host

Timestamp: 2026-08-04T21-04

Issue: #418
Plan: `docs/features/active/2026-08-04-svg-renderer-null-document-nre-418/plan.2026-08-04T14-36.md`
Task: `[P0-T10]`
Branch: `bug/svg-renderer-null-document-nre-418`
HEAD: `a5695656e711f98a8ae6ad334115c0f8666c509f`
Base: `ce0c91e6` (PR #419 repository-wide NuGet package update)

## Command

```
pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath SVGControl.Test/SVGControl.Test.csproj -Configuration Debug -Platform AnyCPU
```

EXIT_CODE: 0

## Output Summary

**Build succeeded. 0 warnings, 0 errors.** Elapsed 00:00:00.28 (incremental; outputs already current
from the full solution recompile recorded in `analyzer-build.2026-08-04T21-04.md`, which performed a
genuine `csc` compile of `SVGControl.Test`).

Output produced:
`SVGControl.Test -> C:\Users\DanMoisan\repos\TaskMaster\SVGControl.Test\bin\Debug\SVGControl.Test.dll`
(28672 bytes, present on disk).

### `EnsureNuGetPackageBuildImports` error text

**The `EnsureNuGetPackageBuildImports` `<Error>` did not fire. There is no error text to record.**
Zero occurrences of `EnsureNuGetPackageBuildImports`, `MSB3073`, or any `error MSB` appear in the
build log. All 71 distinct `..\packages\`-rooted paths referenced by
`SVGControl.Test/SVGControl.Test.csproj` resolve on disk (verified independently: 71 distinct paths,
0 missing).

### `SVGControl.Test` present in `TaskMaster.sln`

**`SVGControl.Test present in TaskMaster.sln: true`**

- Exactly 1 `Project(...) = "SVGControl.Test"` entry, at `TaskMaster.sln:42`, GUID
  `{13AC39E6-DE06-4337-8EB0-41CE674A4C3B}`.
- Exactly 12 `{13AC39E6-DE06-4337-8EB0-41CE674A4C3B}.` configuration-mapping lines.

## Divergence from the plan's expected baseline — recorded, not corrected

The plan's task P0-T10 acceptance text expects a **non-zero** `EXIT_CODE`, verbatim
`EnsureNuGetPackageBuildImports` error text, and `SVGControl.Test present in TaskMaster.sln: false`.
None of those conditions holds in this tree. Two independent reasons:

1. **Phase 1 prerequisite tasks are already complete.** HEAD `a5695656` includes commit `0162567d`
   ("add feature folder and wire SVGControl.Test into solution"), which carried tasks P1-T1 through
   P1-T5 from the prior host. `SVGControl.Test` is therefore already a solution member with all
   twelve configuration mappings, already has its `Svg` compile-time reference, and already builds.
   This Phase 0 re-capture necessarily observes the post-P1-T5 tree; re-creating the original broken
   state would require reverting committed work, which is out of scope for this delegation.
2. **The branch was rebased onto `ce0c91e6`.** The package pins the plan enumerated no longer exist.

This artifact records the real observed state. It does not fabricate a broken baseline and it does
not repair anything.

## Package pin readings — both requested readings, clearly labelled

### Reading (a): the seven folders the plan literally names

| Folder named by plan task P0-T10 | On disk |
|---|---|
| `packages/Castle.Core.5.1.1` | **absent** |
| `packages/FluentAssertions.6.12.0` | **absent** |
| `packages/Moq.4.20.69` | **absent** |
| `packages/MSTest.TestAdapter.3.1.1` | **absent** |
| `packages/MSTest.TestFramework.3.1.1` | **absent** |
| `packages/System.Runtime.CompilerServices.Unsafe.6.0.0` | **absent** |
| `packages/System.Threading.Tasks.Extensions.4.5.4` | **absent** |

All seven are absent. This result is literally true but misleading: it does not indicate a restore
failure. Every one of those versions was superseded by PR #419, so no restore of the current
`packages.config` would ever create those folders.

### Reading (b): the current pins in `SVGControl.Test/packages.config` for the same seven package IDs

| Package ID | Current pin in `SVGControl.Test/packages.config` | `packages/<id>.<current-version>/` on disk |
|---|---|---|
| `Castle.Core` | `5.2.1` | `packages/Castle.Core.5.2.1` — **present** |
| `FluentAssertions` | `8.10.0` | `packages/FluentAssertions.8.10.0` — **present** |
| `Moq` | `4.20.72` | `packages/Moq.4.20.72` — **present** |
| `MSTest.TestAdapter` | `4.3.3` | `packages/MSTest.TestAdapter.4.3.3` — **present** |
| `MSTest.TestFramework` | `4.3.3` | `packages/MSTest.TestFramework.4.3.3` — **present** |
| `System.Runtime.CompilerServices.Unsafe` | `6.1.2` | `packages/System.Runtime.CompilerServices.Unsafe.6.1.2` — **present** |
| `System.Threading.Tasks.Extensions` | `4.6.3` | `packages/System.Threading.Tasks.Extensions.4.6.3` — **present** |

All seven current pins are present on disk, which is why the build succeeds and the
`EnsureNuGetPackageBuildImports` guard does not fire.

### Explicit staleness note

**The version list in plan task P0-T10 is stale relative to the rebased base `ce0c91e6`.** All seven
named versions were superseded by the repository-wide package update in PR #419. A literal check of
the plan's list reports seven absences that carry no defect signal. The orchestrator has been
informed. The plan text was **not** edited by this executor; plan revision is the planner's
responsibility.

## Supporting configuration observations

Recorded because they were read while verifying buildability. No file was modified.

- `SVGControl.Test/SVGControl.Test.csproj:278-280` contains exactly one `<Reference Include="Svg, ...>`
  item with `<HintPath>..\packages\Svg.3.4.8\lib\net481\Svg.dll</HintPath>`, which resolves.
  `SVGControl.Test/packages.config` pins `Svg 3.4.8`. Plan task P1-T4 specified `Svg 3.4.7`; the pin
  moved to `3.4.8` with PR #419. Functionally equivalent — both bind the same
  `Svg, Version=3.4.0.0, PublicKeyToken=12a0bac221edeae2` identity.
- `SVGControl.Test/app.config` `ExCSS` `dependentAssembly` now reads
  `<bindingRedirect oldVersion="0.0.0.0-4.3.2.0" newVersion="4.3.2.0" />`. Plan task P1-T2 specified
  `4.3.1.0`; PR #419 moved ExCSS to `4.3.2`, so the redirect tracks the newer assembly version. The
  AC-10 intent (the `SVGControl.Test` ExCSS redirect matches the `SVGControl` redirect rather than
  remaining at the stale `4.2.4.0`) is satisfied by the current value. Recorded as an observation for
  the planner; not changed here.
- One `MSB3277` warning for `System.Runtime.CompilerServices.Unsafe` was recorded against this project
  on the originating host. It does not reproduce: see the explicit `MSB3277 count: 0` finding in
  `analyzer-build.2026-08-04T21-04.md`.
- `SVGControl.Test` emits exactly one baseline diagnostic under a full-recompile
  `Nullable=enable` + `TreatWarningsAsErrors` build: `CS8630` (`Invalid 'nullable' value: 'Enable' for
  C# 7.3`). See `nullable-build.2026-08-04T21-04.md`.
