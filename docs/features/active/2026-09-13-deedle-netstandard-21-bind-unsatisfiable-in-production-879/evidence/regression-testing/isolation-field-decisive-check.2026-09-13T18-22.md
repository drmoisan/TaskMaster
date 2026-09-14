# Phase 2 — Decisive net481 Isolation Check

Timestamp: 2026-09-13T23-38

Command: (read of the [P2-T11] artifact)

EXIT_CODE: 0

Output Summary:

```
ChildDomain_HasNoSvgControlAssemblyLoaded OUTCOME=Passed
ChildDomain_HasNoAssemblyResolveHandlerBeforeInstall OUTCOME=Passed
ChildDomain_ConfigurationFileDeclaresNoNetstandardRedirect OUTCOME=Passed
NegativeControl_WithoutInstall_Netstandard21Throws OUTCOME=Passed
NegativeControl_HasNoUtilitiesCsAssemblyLoaded OUTCOME=Passed
```

The private AppDomain assembly-resolution field is present and readable on net481; the isolation
assertion did not skip.

All five required outcomes are `Passed`, so the halt condition in `[P2-T12]` is not triggered.

This check is taken before the production fix exists, so it cannot be confounded by it.

## What Each Outcome Establishes

- `ChildDomain_HasNoSvgControlAssemblyLoaded` proves no SVG rendering occurred in the positive
  domain. `SvgRenderer`'s type initializer cannot have run if its assembly is not loaded, so a
  zero count is a complete proof rather than an indication. The same test read
  `CountLoadedAssembliesNamed("UtilitiesCS")` after the installer call and required it to be
  greater than zero, which is the positive control proving the counting helper can report a
  non-zero value.
- `ChildDomain_HasNoAssemblyResolveHandlerBeforeInstall` proves the invocation list is empty
  before the installer runs and non-empty after it. The second reading is what makes the first
  an observation: a reflected field that were null in every state would report zero
  unconditionally. Both readings passed, so the field lookup succeeded and the field value did
  become non-null. That is the decisive net481 observation the `[P0-T12]` artifact deferred to
  this task. The `[P0-T12]` pwsh probe read `PWSH_HOST_FIELD_PRESENT=False` on
  `.NET 10.0.11`, which it recorded as indicative only; the net481 reading here is the
  opposite and is the one that governs.
- `ChildDomain_ConfigurationFileDeclaresNoNetstandardRedirect` proves the configuration file
  supplied to both child domains declares no `netstandard` entry, so a later positive bind is
  attributable to the installer and not to the Phase 3 hardening.
- `NegativeControl_WithoutInstall_Netstandard21Throws` proves the environment is genuinely
  unsatisfiable without the installer. This is the load-bearing criterion and the only one that
  distinguishes a fixed build from an unfixed one.
- `NegativeControl_HasNoUtilitiesCsAssemblyLoaded` proves that keeping the installer call in
  its own probe method prevents `UtilitiesCS` from being JIT-resolved in the installer-free
  domain. The count is taken after that domain has performed a real bind attempt, not on an
  untouched domain, and the same helper returns a non-zero count in the positive domain, so the
  zero is an observation rather than a constant.

## Host and Resolver Provenance

Host: `TaskMaster.Test/bin/Debug/TaskMaster.Test.dll`, run under
`scripts/vscode/TaskMaster.cli.runsettings`, which carries the MSTest parallelisation block
only and no data collector.

The process-wide `AppDomain.CurrentDomain.AssemblyResolve` handler that issue #877 and PR #880
install from `[AssemblyInitialize]` is **not present in this host**. `TestSupport/TestAssemblyResolver.cs`
is `Compile`-linked by exactly two projects, `QuickFiler.Test` and `UtilitiesCS.Test`;
`TaskMaster.Test` links neither that file nor any `[AssemblyInitialize]`.

Independently of that, every observation above is taken through the child domain's own
`AppDomain` instance, inside a domain created by `AppDomain.CreateDomain`, which a
parent-domain handler does not reach. No measurement recorded here depends on handler
ordering, and none would change if a parent-domain handler had been present.

## Scope Note

This task's acceptance condition concerns only the five outcomes above and is met. The
separate failure of `[P2-T11]`'s second required line,
`AfterInstall_DeedleTypeInitializerSucceeds OUTCOME=Failed`, is recorded in the `[P2-T11]`
artifact and does not bear on the isolation result: that test passed rather than failed, which
is a fail-before coverage gap in the probe's specified mechanism, not a loss of isolation. The
negative control did not start passing, so the `ISOLATION-LOST-INVARIANT` condition is not
triggered.
