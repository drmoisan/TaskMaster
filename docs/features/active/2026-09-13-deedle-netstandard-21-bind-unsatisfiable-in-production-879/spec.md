# 2026-09-13-deedle-netstandard-21-bind-unsatisfiable-in-production (Spec)

- **Issue:** #879
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-09-13
- **Status:** Draft
- **Version:** 0.2
- **Work Mode:** full-bug (this file is the sole acceptance-criteria source; no `user-story.md` exists for this item)

> Path convention for this document: the exhaustive set of files this work may create or modify is the
> backticked list under **Proposed Fix -> Files/modules to change (write set, exhaustive)**. Repository
> paths that appear anywhere else in this document are citations for traceability and are not in scope.

## Context
Loading Deedle requires an assembly bind that nothing in this repository or on the build machine can
satisfy on its own: `netstandard, Version=2.1.0.0, PublicKeyToken=cc7b13ffcd2ddd51`. No `netstandard`
binding redirect exists in any `*.config` file in the repository, `netstandard.dll` is not deployed to
any `bin\Debug` output, and the only `netstandard` in the GAC is version 2.0.0.0. The bind currently
succeeds only when a process-global `AppDomain.CurrentDomain.AssemblyResolve` fallback that matches on
simple name plus public key token is already installed. In production that fallback is installed
lazily, from SVGControl.SvgRenderer's static constructor (SVGControl/SvgRenderer.cs:25-28), so it is
present only after the add-in has rendered SVG.

### Update since `issue.md` was written

`issue.md` records this as a latent risk deduced in the test host, with the production path not
investigated. That is no longer the state of knowledge. Four facts supersede or extend it:

1. **The failure is reproduced in production, not deduced.** The maintainer reproduced it on `main` from
   the ribbon entry point `QuickFilerHighConfidence_Click` (TaskMaster/Ribbon/RibbonViewer.cs:159). It
   fails with `FileNotFoundException` for `netstandard, Version=2.1.0.0`, and the inner frame shows the
   chain falling back to `2.0.0.0` and failing there as well. **Any remedy must cover both versions.**
2. **The production FSharp.Core redirect is not test-only.** TaskMaster/app.config lines 69-71 carry
   `FSharp.Core` `oldVersion="0.0.0.0-11.0.0.0" newVersion="11.0.0.0"` in the production add-in config,
   verified by reading the file. Because the observed production failure requests
   `netstandard 2.1.0.0` - a version only `FSharp.Core 11.0.0.0` references - that production redirect
   was demonstrably applied inside the Outlook host. Binding redirects in the add-in config therefore
   **are** honoured at the moment Deedle loads. `issue.md` lines 95 and 113-117 read as though the
   redirect were test-only; that reading is corrected here.
3. **Production Deedle reach is three assemblies, not one.** `QuickFiler`, `ToDoModel` and `UtilitiesCS`
   each carry a `Deedle` reference, centred on UtilitiesCS/Extensions/DfDeedle*.cs. The fix must be
   central, installed once for the process, not applied per call site.
4. **Both existing masked test assemblies are masked by construction.** The PR #880 test-side resolver at
   repository root TestSupport/TestAssemblyResolver.cs is `Compile`-linked into both `QuickFiler.Test`
   (QuickFiler.Test/QuickFiler.Test.csproj:230) and `UtilitiesCS.Test`
   (UtilitiesCS.Test/UtilitiesCS.Test.csproj:80) and installed from `[AssemblyInitialize]` in each. Any
   end-to-end bind assertion placed in either assembly is masked and proves nothing. `TaskMaster.Test`
   has no `[AssemblyInitialize]` (verified by content search over TaskMaster.Test) and no reference to
   any SVGControl type anywhere in the project.

Full derivation of these and the remaining findings is in
`research/2026-09-13T19-05-deedle-netstandard-bind-research.md`, which is authoritative for this spec.

Environment:
- OS/version: Windows 11 Pro 10.0.26200
- Python version: not applicable (C#, .NET Framework 4.8.1, VSTO add-in)
- Command/flags used: `vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /TestCaseFilter:"FullyQualifiedName~QfcInitEmailQueueZeroBatchTests" /InIsolation`
- Data source or fixture: QuickFiler.Test/Controllers/QfcInitEmailQueueZeroBatchTests.cs, which calls
  `Deedle.Reflection.convertRecordSequence`
- Production host: Outlook, VSTO add-in AppDomain created by the VSTO runtime

Impact / Severity:
- [ ] Blocker
- [x] High
- [ ] Medium
- [ ] Low

Severity is High on a reproduced production failure. Every Deedle-backed feature in the affected Outlook
session fails, and because the CLR caches a failed type initializer for the lifetime of the process,
restarting Outlook is the only recovery. The confidence attached to the severity is higher than when
`issue.md` was written, because the failure is no longer deduced.


## Repro & Evidence
Steps to Reproduce (test host, as recorded in `issue.md`):

1. Build the solution in Debug.
2. Run the single class `QfcInitEmailQueueZeroBatchTests` through `vstest.console.exe` with no other test
   class selected and no runsettings file, so that no earlier class has touched `SVGControl.SvgRenderer`.
3. Observe that all three tests fail at Deedle's static initializer with
   `FileNotFoundException: netstandard, Version=2.1.0.0`.

Note: after PR #880 this test-host repro no longer reproduces from `QuickFiler.Test`, because
`[AssemblyInitialize]` there now installs the test-side fallback. That is the masking described in
Context item 4, not a fix of the production defect.

Steps to Reproduce (production, reproduced by the maintainer on `main`):

1. Start Outlook with the add-in loaded, in a fresh process.
2. Do not open any SVG-bearing surface first (no `MyBox` dialog, no config viewer, no
   folder-not-found dialog, no prior QuickFiler session).
3. Click the QuickFiler high-confidence ribbon button, which enters
   `QuickFilerHighConfidence_Click` at TaskMaster/Ribbon/RibbonViewer.cs:159.
4. Observe `FileNotFoundException` for `netstandard, Version=2.1.0.0`, with the inner frame showing the
   chain falling back to `netstandard, Version=2.0.0.0` and failing there as well.

Expected:
Loading Deedle succeeds on its own merits in every host, through deployed assemblies, declared binding
redirects, and a fallback the add-in installs for itself at a deterministic point, without depending on
an `AssemblyResolve` fallback that an unrelated component happens to have installed first.

Actual:
The bind is unsatisfiable. Measured in the test host at head `c9590a8b7`:

```
System.TypeInitializationException: The type initializer for 'Deedle.Reflection' threw an exception.
 ---> System.TypeInitializationException: The type initializer for '<StartupCode$Deedle>.$FrameUtils' threw an exception.
 ---> System.IO.FileNotFoundException: Could not load file or assembly
      'netstandard, Version=2.1.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51'
      or one of its dependencies. The system cannot find the file specified.
```

The CLR caches a failed type initializer for the lifetime of the process. Once `Deedle.Reflection` has
failed once, every later use of Deedle in that process raises the same `TypeInitializationException`,
including uses that occur after a fallback handler has been installed.

Logs / Screenshots:
- [x] Attached minimal logs or screenshot
- Snippet: the exception chain quoted above, captured from a `vstest.console.exe` run of
  `QfcInitEmailQueueZeroBatchTests` alone at head `c9590a8b7`.
- The production stack, including the `2.0.0.0` fallback frame, was observed by the maintainer during the
  `main` reproduction. It is not attached to this folder; it is reported, not archived.


## Scope & Non-Goals

In scope:
- A new host-neutral assembly-binding fallback type in `UtilitiesCS`, with a unit-testable resolution
  seam and a resolution ladder that can supply `netstandard` deterministically.
- Eager installation of that fallback from the `ThisAddIn` static constructor in `TaskMaster`.
- Declarative hardening: a `netstandard` `<dependentAssembly>` entry in `TaskMaster/app.config` covering
  both `2.0.0.0` and `2.1.0.0`.
- Unit tests for the resolution ladder and a child-`AppDomain` acceptance harness with a negative control.

Out of scope / non-goals:
- **Do not modify** SVGControl/SvgAssemblyResolver.cs, SVGControl/SvgRenderer.cs or
  SVGControl/SvgAssemblyProbe.cs. That handler has an independent reason to exist for the `devenv.exe`
  WinForms designer host (issue #418, rationale documented at SVGControl/SvgAssemblyResolver.cs:17-29).
  Changing it widens the blast radius into #418's territory.
- **Do not redo or widen the PR #880 test-side fix.** TestSupport/TestAssemblyResolver.cs,
  QuickFiler.Test/SetupAssemblyInitializer.cs and UtilitiesCS.Test/TestAssemblyInitializer.cs are out of
  scope.
- **Do not modify** QuickFiler.Test/app.config or UtilitiesCS.Test/app.config.
- **Do not** pin, downgrade or otherwise change the `FSharp.Core` version or its binding redirect in any
  config file. It removes the `2.1.0.0` requirement at the root but does not address the reported
  `2.0.0.0` failure, and the redirect appears in several config files.
- **Do not** deploy a `netstandard.dll` facade into any `bin` output, and do not add `NETStandard.Library`
  to any `packages.config`. The framework directory copy is already guaranteed present on any machine
  able to run this add-in, so deployment adds a package, a deployed file and a VSTO application-manifest
  entry for no capability gained.
- **Do not** modify scripts/vscode/TaskMaster.cli.runsettings, TaskMaster.runsettings, coverage.config or
  any file under .github/workflows/.
- **Do not** modify anything under .claude/hooks/, .claude/rules/ or .github/instructions/.
- **Do not** create a new test project. CI runs all `*.Test.dll` in one `vstest.console.exe` invocation
  (.github/workflows/_mstest-coverage.yml:86-99) and `/InIsolation` isolates the test host from
  `vstest.console.exe`, not test assemblies from each other, so a new project buys no isolation
  guarantee that the child-`AppDomain` harness does not already provide, while costing a `.csproj`,
  `packages.config`, `app.config` and `TaskMaster.sln` registration rows.
- **Do not** rely on an existing test assembly alone as the isolation mechanism. TaskMaster.runsettings
  sets class-level parallelisation, so sibling-class ordering within an assembly is nondeterministic and
  any sibling that constructs an SVG-bearing control masks the result.
- **Do not** build a child-process harness. .claude/rules/general-unit-test.md bars unit tests from
  depending on external processes, and there is no console executable in this solution to launch.
- **Do not** return `typeof(object).Assembly` from the resolver. `netstandard.dll` is a pure
  type-forwarding facade; substituting `mscorlib` converts a clean `FileNotFoundException` into a
  `TypeLoadException` at an arbitrary later point.
- **Do not** create or use temporary files in any test. Repository policy prohibits it and the design
  needs none.
- No change to Deedle usage, to `DfDeedle`, or to any QuickFiler/ToDoModel data-model behaviour.

Explicitly excluded systems, integrations, or datasets:
- The `devenv.exe` WinForms designer host scenario (issue #418).
- The test-host masking half of the problem (issue #877 / PR #880).
- ClickOnce / VSTO application-manifest content, since no new deployed file is introduced.


## Root Cause Analysis

Measured facts, verified in the repository at head `c9590a8b7` and re-verified in the item worktree:

- `Deedle.dll` is deployed as `Deedle, Version=3.0.0.0, PublicKeyToken=null` and references FSharp.Core
  4.5.0.0.
- TaskMaster/app.config:69-71 redirects FSharp.Core to 11.0.0.0 **in production**;
  QuickFiler.Test/app.config:46-47 and UtilitiesCS.Test/app.config carry the same redirect.
- The deployed `FSharp.Core.dll` is `FSharp.Core, Version=11.0.0.0, PublicKeyToken=b03f5f7f11d50a3a`,
  and it references `netstandard, Version=2.1.0.0`.
- No `*.config` file anywhere in the repository contains the string `netstandard`, so no binding redirect
  covers that reference in any host, production included.
- `netstandard.dll` is not present in any `bin\Debug` output.
- The GAC on this machine contains exactly one `netstandard`: `v4.0_2.0.0.0__cc7b13ffcd2ddd51`.
- `netstandard 2.0.0.0` and the requested `netstandard 2.1.0.0` share the public key token
  `cc7b13ffcd2ddd51`, which is why a fallback matching on simple name plus public key token can satisfy
  the bind while the default binder cannot. `netstandard 2.1.0.0` does not exist for .NET Framework on
  any machine; the only possible satisfying assembly is the 2.0.0.0 facade, returned through a handler
  that does not version-check.
- The repository contains exactly two first-party `AssemblyResolve` installation sites:
  SVGControl/SvgAssemblyResolver.cs:41 (installed from the `SVGControl.SvgRenderer` static constructor)
  and TestSupport/TestAssemblyResolver.cs:45 (installed from `[AssemblyInitialize]` in two test
  projects). This count is derived twice by independent search strategies in
  `research/2026-09-13T19-05-deedle-netstandard-bind-research.md`, section "Numeric Derivation Evidence".

Chain of causation on the reproduced production path:

1. TaskMaster.csproj does not reference `SVGControl`, and no file under TaskMaster/ names any `SVGControl`
   type. Nothing in add-in startup, `ThisAddIn`, or the ribbon layer can install a fallback.
2. The ribbon click reaches `QfcHomeController.LaunchAsync` -> `InitAsync`. `InitAsync` launches the
   data-model load (QuickFiler/Controllers/QfcHomeController.cs:123) **before** constructing
   `QfcFormViewer` (line 131), and `QfcFormViewer` carries no SVG control in any case.
   `ProgressTracker.Initialize()` creates `ProgressViewer`, which also carries no SVG control.
3. The data-model load reaches `QfcDatamodel.InitDfAsync` -> `GetEmailsInViewDfAsync` ->
   `UtilitiesCS.DfDeedle.GetEmailDataInViewAsync`, the first executable frame that touches a Deedle type.
4. `Deedle.dll` loads, forcing `FSharp.Core` (redirected to 11.0.0.0 by the production config), which
   forces `netstandard 2.1.0.0`. No redirect covers it and no fallback is installed, so the bind fails.
   The first `ButtonSVG` in the QuickFiler flow is created by
   QuickFiler/Helper Classes/ItemViewerQueue.cs:105, strictly after the Deedle load, so the SVG handler
   is never installed on this path.

Correction to `issue.md` lines 111-112: the claim that the viewers instantiate `SVGControl.ButtonSVG`
**and** `SVGControl.SvgResource` is inaccurate for `SvgResource`. `SvgResource`
(SVGControl/ISvgResource.cs:18-22) is a plain data class that names no `SvgRenderer` member; constructing
it does not install the fallback. Only `ButtonSVG`, `PictureBoxSVG`, `SvgImageSelector`, `ToggleSwitch`
and direct `SvgRenderer` use do.

Why the existing handler would not have been enough even if installed early: its ladder is
(1) return an already-loaded assembly with a matching simple name and token; (2) `Assembly.Load` of the
**partial** display name; (3) probe `<dir>\<name>.dll` next to known directories. In a fresh add-in
AppDomain no `netstandard` is known to be loaded, a partial display name is unlikely to reach the GAC,
and `netstandard.dll` is not deployed, so all three rungs are expected to miss. Promoting
`SvgAssemblyResolver.Install()` to public and calling it from `ThisAddIn` is the smallest possible diff
and would probably not fix the reproduced defect; it is rejected for that reason, and because it would
create a production startup dependency on an SVG rendering component.

Why the `2.0.0.0` leg also fails is **unknown**. The GAC holds `2.0.0.0` and a fully-specified strong-name
reference should find it. Nothing in this repository explains it. See Risks & Mitigations.


## Proposed Fix

### Design summary (what changes where):

Install an eager, self-sufficient `AssemblyResolve` fallback in production, from the `ThisAddIn` type
initializer, and harden the configuration declaratively.

- The resolution logic lives in a new host-neutral type in `UtilitiesCS`, so it is unit-testable without
  WinForms, COM or a live Outlook process. `ThisAddIn` is `[ExcludeFromCodeCoverage]`
  (TaskMaster/ThisAddIn.cs:18) and must remain a thin call site containing one statement.
- The installation point is `static ThisAddIn()`. It is the only candidate whose ordering rests on a CLR
  guarantee rather than on documented-but-unguaranteed host behaviour: the VSTO runtime must construct a
  `ThisAddIn` instance before it can call `RequestComAddInAutomationService`,
  `CreateRibbonExtensibilityObject` or raise `Startup`, and the type initializer must run before that
  construction. Declaring an explicit static constructor also clears `beforefieldinit`, making the
  ordering precise rather than "at or before first use".
- The ladder adds the two capabilities SVGControl/SvgAssemblyResolver.cs lacks: `Assembly.Load` of the
  **full** display name `netstandard, Version=2.0.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51`,
  and `Assembly.LoadFrom` of `RuntimeEnvironment.GetRuntimeDirectory()` joined with `netstandard.dll`.
  The latter is guaranteed present on any machine able to run a `net481` VSTO add-in and is independent
  of GAC lookup behaviour, which is the leg that makes the remedy robust to the unexplained `2.0.0.0`
  failure.
- Secondary, declarative hardening only, **not the fix**: a `netstandard` `<dependentAssembly>` in
  `TaskMaster/app.config` covering `0.0.0.0-2.1.0.0 -> 2.0.0.0`. A `<bindingRedirect>` rewrites an
  identity; it cannot manufacture an assembly. After the rewrite the CLR must still locate
  `netstandard 2.0.0.0`, and the reproduced trace reports that leg failing too. The redirect is therefore
  insufficient alone and is included only because it is cheap, structurally uniform with the ten
  redirects already in that file, and verified to be honoured in the Outlook host.

### Boundaries and invariants to preserve:

- **The installer must never throw.** An exception escaping a static constructor becomes a
  `TypeInitializationException` on `ThisAddIn`, which would take the entire add-in down. The installer
  and the handler must absorb all failures at their boundary and return `null` for an unresolvable name.
- **No log4net inside the handler.** Logging through log4net from an `AssemblyResolve` handler can
  re-enter assembly loading. Use `Trace`-based diagnostics only, matching the rationale already recorded
  at SVGControl/SvgAssemblyResolver.cs:98-99 and :140-142.
- **Re-entrance guard.** A `[ThreadStatic]` guard must return `null` on a nested request for the same
  simple name, so the handler cannot recurse into itself.
- **Version is deliberately not compared** when returning an already-loaded or facade assembly. .NET
  Framework does not re-validate the identity of an assembly returned from an `AssemblyResolve` handler;
  this is the mechanism by which a `2.1.0.0` request is satisfied by the `2.0.0.0` facade, and the
  repository already depends on the same behaviour for ExCSS.
- **`ThisAddIn` stays a thin call site.** No resolution logic in `TaskMaster`, which is excluded from
  coverage.
- Existing public APIs are unchanged. No behaviour visible to users changes on a working machine.

### Dependencies or blocked work:

- None blocking. PR #880 (issue #877) has landed and is out of scope; this item is the production half.
- Issue #418 owns the SVGControl designer-host handler and is untouched.

### Implementation strategy (what changes, not sequencing):

#### Files/modules to change (write set, exhaustive):

- `UtilitiesCS/Bootstrap/AssemblyBindingFallback.cs` (new) - the host-neutral installer and resolution
  ladder.
- `UtilitiesCS/UtilitiesCS.csproj` - one new `<Compile Include>` item. This project uses explicit
  `Compile` items; an unregistered file silently does not build.
- `TaskMaster/ThisAddIn.cs` - add `static ThisAddIn()` containing exactly one call.
- `TaskMaster/app.config` - one new `<dependentAssembly>` block for `netstandard`.
- `UtilitiesCS.Test/Bootstrap/AssemblyBindingFallbackTests.cs` (new) - ladder unit tests.
- `UtilitiesCS.Test/UtilitiesCS.Test.csproj` - one new `<Compile Include>` item.
- `TaskMaster.Test/Bootstrap/ChildDomainBindProbe.cs` (new) - the public `MarshalByRefObject` proxy
  executed inside the child domain.
- `TaskMaster.Test/Bootstrap/NetstandardBindChildDomainTests.cs` (new) - the child-`AppDomain` acceptance
  harness, including the negative control.
- `TaskMaster.Test/Bootstrap/AddInEagerInstallShapeTests.cs` (new) - reflection-shape regression guards
  for the static constructor and the config redirect.
- `TaskMaster.Test/TaskMaster.Test.csproj` - new `<Compile Include>` items for the three files above.
- `docs/features/active/2026-09-13-deedle-netstandard-21-bind-unsatisfiable-in-production-879/` - this
  spec, the plan, and evidence artifacts under its `evidence/` subtree.

#### Functions/classes/CLI commands impacted:

- New: `UtilitiesCS.Bootstrap.AssemblyBindingFallback` with a public `Install()` (idempotent; installing
  twice must not attach two handlers) and an internal `Resolve(AssemblyName requested)` seam made visible
  to `UtilitiesCS.Test` through the existing `InternalsVisibleTo` arrangement, or exposed as an internal
  static method on a nested strategy type. The "load by full display name" and "load from path" steps
  must be injectable delegates so unit tests need no real GAC or filesystem access.
- Changed: `TaskMaster.ThisAddIn` gains an explicit static constructor. No instance member changes.
- No CLI commands are affected.

#### Data flow and validation changes:

None. The change affects assembly binding only; no data, no serialization format, no Outlook object
access.

#### Error handling and logging updates:

- The handler returns `null` for any name it cannot resolve, and never propagates an exception to the
  CLR binder.
- Diagnostics are emitted through `System.Diagnostics.Trace` only, with enough context to identify the
  requested display name and which ladder rung satisfied it.

#### Rollback/feature-flag considerations (if applicable):

- Rollback is deleting the `static ThisAddIn()` body, which restores the previous (defective) behaviour
  exactly. No feature flag is introduced: a flag would add a code path in which the defect persists, and
  the installer is unconditional by design.
- The `app.config` entry can be reverted independently of the code change.

### Technical specifications (interfaces/contracts):

Resolution ladder, evaluated in order, first non-null wins:

1. An already-loaded assembly in the current `AppDomain` whose simple name matches case-insensitively and
   whose public key token is equal. Version is not compared.
2. `Assembly.Load` of the **full** display name: the requested simple name, `Culture=neutral`, the
   requested public key token, and `Version=2.0.0.0` for the `netstandard` identity specifically. A fully
   specified strong name is the reference form the GAC is searched for; the existing handler asks by
   partial name, which is materially weaker.
3. `Assembly.LoadFrom(Path.Combine(RuntimeEnvironment.GetRuntimeDirectory(), "netstandard.dll"))` for the
   `netstandard` identity. Verified present on the development machine at
   `%WINDIR%\Microsoft.NET\Framework64\v4.0.30319\netstandard.dll`.
4. A directory probe for `<probe-dir>\<simple-name>.dll` next to the executing assembly, matching the
   existing handler's third strategy.

#### Inputs/outputs and formats:

- Input: the `AssemblyName` carried by `ResolveEventArgs.Name`.
- Output: a loaded `Assembly`, or `null` when no rung applies.

#### Required configuration keys and defaults:

- `TaskMaster/app.config`, inside the existing `<assemblyBinding>` element: `<assemblyIdentity>` with
  `name="netstandard"`, `publicKeyToken="cc7b13ffcd2ddd51"`, `culture="neutral"`, and
  `<bindingRedirect oldVersion="0.0.0.0-2.1.0.0" newVersion="2.0.0.0" />`. The `oldVersion` range covers
  both reported versions.
- The block must be written in the attribute-per-line form CSharpier already produces for that file, or
  step 1 of the toolchain will rewrite it and force a restart of the loop.

#### Backward-compatibility expectations:

- No public API is removed or changed. `SVGControl.SvgAssemblyResolver` continues to install its own
  handler for the designer host; two handlers coexisting is benign, since each returns `null` for names
  it cannot resolve.

#### Performance constraints (latency/throughput/memory):

- The installer runs once per process during add-in type initialization and performs no I/O itself.
  Ladder rungs 2-4 execute only on a bind that would otherwise have failed. No measurable startup cost is
  expected; none is budgeted.


## Assumptions, Constraints, Dependencies
- Assumptions (environment, data, access):
  - Any machine able to load this `net481` add-in has .NET Framework 4.8.1 installed and therefore has
    `netstandard.dll` 2.0.0.0 in both the GAC and the runtime directory. Verified on the development
    machine; treated as a framework guarantee elsewhere.
  - The VSTO runtime sets the add-in AppDomain's `ConfigurationFile` to `TaskMaster.dll.config`. This is
    **not** confirmed from primary Microsoft documentation; it is confirmed empirically by the FSharp.Core
    argument in Context item 2. Stated as inference, not citation.
  - `TaskMaster.Test` build output contains `Deedle.dll` transitively through its `UtilitiesCS` and
    `ToDoModel` references. **Unverified**: no `bin` output exists in this worktree, so it could not be
    checked. See Risks & Mitigations for the named fallback host.
- Constraints (budget, performance, compatibility):
  - Repository policy prohibits temporary files in tests; the child-`AppDomain` harness writes none, as
    its `ApplicationBase` points at an existing build output directory.
  - The new `UtilitiesCS` type is in the coverage denominator and must meet the repository floor; new
    modules target `>= 90%` line coverage per CLAUDE.md.
  - A new file in `UtilitiesCS` carrying `#nullable enable` opts that file into `CS86xx`-as-error under
    the nullable gate.
- External dependencies (services, libraries, releases):
  - None added. No new NuGet package, no new deployed file, no new project.
  - `AppDomain.CreateDomain` / `MarshalByRefObject` have no precedent in this repository (content search
    for `CreateDomain|MarshalByRefObject|AppDomainSetup` over all `*.cs` returned zero matches), so the
    harness is a greenfield technique here.


## Data / API / Config Impact
- User-facing or API changes: none. The only observable change is that Deedle-backed features work on a
  path that previously failed.
- Data or migration considerations: none.
- Logging/telemetry updates: `Trace` output from the fallback handler identifying the requested display
  name and the satisfying ladder rung. No log4net use inside the handler.
- Compatibility notes: one new `<dependentAssembly>` block in `TaskMaster/app.config`, deployed as
  `TaskMaster/bin/Debug/TaskMaster.dll.config` by the existing `_CopyAppConfigFile` target. No change to
  `TaskMaster/TaskMaster.csproj` is required, because `app.config` is already an item there. No VSTO
  application-manifest change, because no new file is deployed.


## Test Strategy

**The central requirement: the acceptance test must exercise a path that reaches the bind with no prior
SVG rendering and no prior `AssemblyResolve` handler.** If any SVG-bearing control is touched first,
`SvgRenderer`'s static constructor installs the fallback and masks the result, which is how this defect
hid inside the test suite. A criterion that cannot distinguish a fixed build from an unfixed one is
worse than none.

Design: a fresh child `AppDomain` created with `AppDomain.CreateDomain` and driven through a
`MarshalByRefObject` proxy, because the parent domain's handler does not propagate into a child domain.
Both child domains set `ApplicationBase` to the directory of the test assembly and `ConfigurationFile` to
that test assembly's own `.dll.config`, which carries no `netstandard` redirect and is out of scope to
change. This keeps the negative control valid after the `TaskMaster/app.config` hardening lands: the
harness measures the resolver, not the redirect. `AppDomain.Unload` runs in `[TestCleanup]`.

- Regression tests to add or update:
  - `TaskMaster.Test/Bootstrap/NetstandardBindChildDomainTests.cs` - positive harness and negative
    control.
  - `TaskMaster.Test/Bootstrap/AddInEagerInstallShapeTests.cs` - static-constructor shape guard and
    `app.config` content guard. `TaskMaster.Test` already performs reflection-shape assertions of this
    kind (TaskMaster.Test/Ribbon/RibbonCommandBoundaryTests.cs:187-201).
- Unit tests for the fixed behavior and boundaries:
  - `UtilitiesCS.Test/Bootstrap/AssemblyBindingFallbackTests.cs` - ladder ordering, token comparison
    including null/empty-token edge cases, re-entrance guard, idempotent `Install()`, and
    never-throws-on-unresolvable. The "load by display name" and "load from path" steps are injected as
    delegates, so these tests touch neither the GAC nor the filesystem. MSTest, Moq and FluentAssertions
    per CLAUDE.md. Note that `UtilitiesCS.Test` is itself masked by the PR #880 handler, which is why
    only pure ladder logic - not the end-to-end bind - belongs there.
- Edge cases and negative scenarios:
  - Unresolvable simple name returns `null` without throwing.
  - Matching simple name with a different public key token is rejected.
  - Requested name with no token, and requested name with an empty token.
  - Nested request for the same simple name returns `null` via the re-entrance guard.
  - `Install()` called twice attaches one handler.
- Error handling and logging verification: assert that a rung which throws internally is absorbed and the
  ladder continues to the next rung, and that the handler still returns `null` when every rung fails.
- Coverage impact and targets: `UtilitiesCS/Bootstrap/AssemblyBindingFallback.cs` is a new module and
  targets `>= 90%` line coverage. The `TaskMaster` half is inside `[ExcludeFromCodeCoverage] class
  ThisAddIn` and contributes no denominator, which is why the logic must not live there.
- Toolchain commands to run (format -> lint -> type-check -> test), in this exact order, restarting from
  step 1 on any failure or auto-fix:
  1. `dotnet tool run csharpier format .` then `dotnet tool run csharpier check .`
  2. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
  3. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`
  4. `vstest.console.exe <test-assembly-paths> /EnableCodeCoverage`
- Manual validation steps (human gate, unchanged from issue.md:130-132): start a fresh Outlook session,
  click the QuickFiler ribbon button without first opening any SVG-bearing surface, and record whether
  Deedle loads and the QuickFiler data model populates.


## Acceptance Criteria

- [ ] `UtilitiesCS/Bootstrap/AssemblyBindingFallback.cs` exists, declares a host-neutral
      `UtilitiesCS.Bootstrap.AssemblyBindingFallback` with a public `Install()` and an internal
      `Resolve` seam, references no WinForms or Outlook Interop type, and is registered as a
      `<Compile Include>` item in `UtilitiesCS/UtilitiesCS.csproj`.
- [ ] `TaskMaster.Test.Bootstrap.AddInEagerInstallShapeTests.ThisAddIn_HasExplicitStaticConstructor`
      passes, asserting that `typeof(ThisAddIn).Attributes.HasFlag(TypeAttributes.BeforeFieldInit)` is
      `false` and `typeof(ThisAddIn).TypeInitializer` is not `null`. This guards the eager install point
      against deletion without depending on a prose phrase search.
- [ ] `TaskMaster/ThisAddIn.cs` declares `static ThisAddIn()` whose body is exactly one call to
      `UtilitiesCS.Bootstrap.AssemblyBindingFallback.Install()`, and `ThisAddIn` retains its
      `[ExcludeFromCodeCoverage]` attribute.
- [ ] `UtilitiesCS.Test.Bootstrap.AssemblyBindingFallbackTests` passes and covers, as separately named
      test methods: already-loaded match wins over a fresh load; the full-display-name rung; the
      runtime-directory `LoadFrom` rung; a mismatched public key token is rejected; null and empty
      requested tokens; the `[ThreadStatic]` re-entrance guard returns `null`; `Install()` is idempotent;
      an internally throwing rung is absorbed and the ladder continues; an unresolvable name returns
      `null` without throwing. All rungs are exercised through injected delegates, touching neither the
      GAC nor the filesystem.
- [ ] `TaskMaster.Test/Bootstrap/ChildDomainBindProbe.cs` declares a public `MarshalByRefObject` proxy
      that performs every assertion listed below **inside** the child domain and marshals the results
      back, and `TaskMaster.Test/Bootstrap/NetstandardBindChildDomainTests.cs` creates each domain with
      `AppDomain.CreateDomain` and unloads it in `[TestCleanup]`.
- [ ] `TaskMaster.Test.Bootstrap.NetstandardBindChildDomainTests.ChildDomain_HasNoSvgControlAssemblyLoaded`
      passes, asserting inside the child domain that `AppDomain.CurrentDomain.GetAssemblies()` contains
      no assembly whose simple name is `SVGControl`. Because `SvgRenderer`'s type initializer cannot have
      run if the assembly is not loaded, this is a complete proof that no SVG rendering occurred.
- [ ] `TaskMaster.Test.Bootstrap.NetstandardBindChildDomainTests.ChildDomain_HasNoAssemblyResolveHandlerBeforeInstall`
      passes, asserting inside the child domain that the `AppDomain` assembly-resolution event has an
      empty invocation list before the installer under test runs. Read the private `_AssemblyResolve`
      instance field by reflection (net481 is a frozen runtime) and assert
      `Delegate.GetInvocationList()` length is zero, or that the field is `null`.
- [ ] `TaskMaster.Test.Bootstrap.NetstandardBindChildDomainTests.ChildDomain_ConfigurationFileDeclaresNoNetstandardRedirect`
      passes, asserting that the configuration file supplied to both child domains contains no
      `netstandard` `<dependentAssembly>` entry. This makes the isolation guarantee checkable rather than
      assumed, and keeps the positive result attributable to the installer rather than to the
      `TaskMaster/app.config` hardening.
- [ ] `TaskMaster.Test.Bootstrap.NetstandardBindChildDomainTests.AfterInstall_BothNetstandardVersionsBind`
      passes, asserting inside the child domain that after
      `UtilitiesCS.Bootstrap.AssemblyBindingFallback.Install()` runs, **both**
      `Assembly.Load("netstandard, Version=2.1.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51")`
      and `Assembly.Load("netstandard, Version=2.0.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51")`
      return a non-null assembly.
- [ ] `TaskMaster.Test.Bootstrap.NetstandardBindChildDomainTests.AfterInstall_DeedleTypeInitializerSucceeds`
      passes, asserting inside the same child domain that a Deedle type initializes without
      `TypeInitializationException` (for example the member already exercised by
      QuickFiler.Test/Controllers/QfcInitEmailQueueZeroBatchTests.cs,
      `Deedle.Reflection.convertRecordSequence`). The test asserts `Deedle.dll` is present in the
      domain's `ApplicationBase` and **fails** rather than skipping if it is not.
- [ ] `TaskMaster.Test.Bootstrap.NetstandardBindChildDomainTests.NegativeControl_WithoutInstall_Netstandard21Throws`
      passes, asserting in a **second** child domain, with the installer **not** run, that
      `Assembly.Load("netstandard, Version=2.1.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51")`
      throws `FileNotFoundException` naming `netstandard`. This is the load-bearing criterion: it is what
      distinguishes a fixed build from an unfixed one. The test carries an in-file comment stating that if
      this negative control ever starts passing without a code change, isolation has been lost and the
      positive tests above are vacuous and must not be trusted.
- [ ] `TaskMaster/app.config` declares, inside the existing `<assemblyBinding>` element, a
      `<dependentAssembly>` with `name="netstandard"`, `publicKeyToken="cc7b13ffcd2ddd51"`,
      `culture="neutral"` and `<bindingRedirect oldVersion="0.0.0.0-2.1.0.0" newVersion="2.0.0.0" />`,
      verified by `TaskMaster.Test.Bootstrap.AddInEagerInstallShapeTests.AppConfig_DeclaresNetstandardRedirect`
      which parses the config as XML and asserts on the element and attribute values, not on a text
      phrase.
- [ ] This spec records that the `app.config` redirect is hardening and not the fix, on the stated ground
      that a `<bindingRedirect>` rewrites an identity and cannot manufacture an assembly.
- [ ] `git diff --name-only` against the merge base with `main` lists none of:
      SVGControl/SvgAssemblyResolver.cs, SVGControl/SvgRenderer.cs, SVGControl/SvgAssemblyProbe.cs,
      TestSupport/TestAssemblyResolver.cs, QuickFiler.Test/SetupAssemblyInitializer.cs,
      UtilitiesCS.Test/TestAssemblyInitializer.cs, QuickFiler.Test/app.config,
      UtilitiesCS.Test/app.config, scripts/vscode/TaskMaster.cli.runsettings, TaskMaster.runsettings,
      coverage.config, any path under .github/, .claude/hooks/, or .claude/rules/, and no
      `packages.config` anywhere.
- [ ] No `FSharp.Core` version or binding-redirect value changes in any `*.config` or `*.csproj`, and no
      `netstandard.dll` is added to any `bin` output or to any project as a deployed item.
- [ ] No test added by this work creates, writes or deletes a file on disk, and no test uses
      `Thread.Sleep`, `Task.Delay` or a wall-clock wait.
- [ ] Line coverage for `UtilitiesCS/Bootstrap/AssemblyBindingFallback.cs` is `>= 90%`, read from the
      Cobertura artifact produced by the final coverage run and recorded under the feature folder's
      `evidence/qa-gates/` directory.
- [ ] A full four-step toolchain pass completes with no failures and no auto-fixes in the final pass,
      using the exact commands listed in Test Strategy, with console logs captured under the feature
      folder's `evidence/qa-gates/` directory. The analyzer and nullable logs must each show zero
      `Skipping target "CoreCompile"` occurrences, proving the gate was not vacuous.
- [ ] Manual live-Outlook gate recorded: a fresh Outlook session, QuickFiler ribbon button clicked
      without first opening any SVG-bearing surface, result written to the feature folder's
      `evidence/other/` directory. This is a human gate; an unrecorded result does not discharge it.


## Risks & Mitigations

Technical or operational risks:

- **Known unknown: why the `netstandard 2.0.0.0` leg also fails in the Outlook add-in AppDomain is
  unexplained.** The GAC holds `2.0.0.0` and a fully-specified strong-name reference should find it.
  Nothing in this repository accounts for it, and this spec does not claim it is resolved. The remedy is
  designed to be robust to it: ladder rung 3 loads the facade from
  `RuntimeEnvironment.GetRuntimeDirectory()` by absolute path, bypassing GAC lookup entirely. The
  cheapest measurement that would settle the question is a Fusion binding log
  (`HKLM\SOFTWARE\Microsoft\Fusion`, `EnableLog=1`, `ForceLog=1`) captured from one reproduced Outlook
  run. This should be done, but the fix does not wait on it.
- **The harness host assembly may not have `Deedle.dll` in its output.** `TaskMaster.Test` references
  `TaskMaster`, `ToDoModel` and `UtilitiesCS`, so `Deedle.dll` is expected transitively, but this could
  not be verified: no `bin` output exists in this worktree. Mitigation: if the Deedle assertion cannot be
  satisfied from `TaskMaster.Test`, host the child-domain harness in `ToDoModel.Test`, which references
  Deedle directly and has no `[AssemblyInitialize]`. That substitution changes the write set and must be
  recorded, not made silently.
- **Reading the private `_AssemblyResolve` field by reflection is implementation-dependent.** It is
  stable on net481, a frozen runtime, and this repository targets only net481. If the field is absent the
  test must fail loudly rather than skip the guarantee, since a silently skipped isolation check makes
  the positive tests vacuous.
- **An exception escaping `static ThisAddIn()` would disable the entire add-in.** Mitigated by the
  non-throwing boundary invariant and by unit tests asserting the installer swallows rung failures.
- **Two `AssemblyResolve` handlers will coexist in a process that renders SVG.** Both return `null` for
  names they cannot resolve, so the composition is benign, but ordering is first-subscribed-first-called
  and the eager handler will now run first. No behaviour depending on SVGControl's handler winning is
  known; if one is found, it belongs to issue #418.
- **`AppDomain.CreateDomain` is a greenfield technique in this repository.** Risk of flakiness from
  unloaded domains or cross-domain marshalling errors. Mitigated by `[TestCleanup]` unload and by keeping
  the proxy surface to primitive return values.
- **CSharpier may rewrite a hand-added `app.config` block.** Mitigated by matching the existing
  attribute-per-line form and by running step 1 of the toolchain first.

Mitigations and rollbacks:

- Rollback of the code fix is deleting the `static ThisAddIn()` body; rollback of the hardening is
  removing the `netstandard` `<dependentAssembly>` block. The two are independent.
- If the manual live-Outlook gate fails despite green automated criteria, capture the Fusion log before
  changing the design; the gap would be between the child-domain environment and the add-in AppDomain,
  which the log would localise.


## Rollout & Follow-up
- Release/rollout steps: merge to `main`, rebuild, and reinstall the add-in registration as usual for a
  VSTO change. No configuration migration, no user action, no new deployed file.
- Post-fix monitoring or clean-up tasks:
  - Capture a Fusion binding log from one Outlook run to close the `2.0.0.0` known unknown, and record
    the result on this issue.
  - Consider a follow-up issue to remove the ordering dependency that QuickFiler viewers still have on
    SVGControl's lazy handler, once the eager handler is proven in production. Out of scope here.
  - QuickFiler/Viewers/Form1.Designer.cs appears to be a leftover scratch form carrying one `ButtonSVG`;
    whether it is reachable in production was not determined and does not affect this fix. Candidate for
    a separate cleanup issue.
- Links:
  - Issue: https://github.com/drmoisan/TaskMaster/issues/879
  - Related: issue #877 and PR #880 (test-side resolver, out of scope), issue #418 (SVGControl designer
    host handler, out of scope).
  - Research: `research/2026-09-13T19-05-deedle-netstandard-bind-research.md`
