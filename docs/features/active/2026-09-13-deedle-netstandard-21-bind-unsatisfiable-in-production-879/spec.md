# 2026-09-13-deedle-netstandard-21-bind-unsatisfiable-in-production (Spec)

- **Issue:** #879
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-09-13T18-22
- **Status:** Draft
- **Version:** 0.1

## Context
Loading Deedle requires an assembly bind that nothing in this repository or on the build machine can
satisfy on its own: `netstandard, Version=2.1.0.0, PublicKeyToken=cc7b13ffcd2ddd51`. No `netstandard`
binding redirect exists in any `*.config` file in the repository, `netstandard.dll` is not deployed to
any `bin\Debug` output, and the only `netstandard` in the GAC is version 2.0.0.0. The bind currently
succeeds only when a process-global `AppDomain.CurrentDomain.AssemblyResolve` fallback that matches on
simple name plus public key token is already installed. In production that fallback is installed
lazily, from `SVGControl.SvgRenderer`'s static constructor, so it is present only after the add-in has
rendered SVG. **The production path was not investigated.** This entry records a latent risk deduced
from measurements taken in the test host during issue #877, not a reproduced production failure.

Environment:
- OS/version: Windows 11 Pro 10.0.26200
- Python version: not applicable (C#, .NET Framework 4.8, VSTO add-in)
- Command/flags used: `vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /TestCaseFilter:"FullyQualifiedName~QfcInitEmailQueueZeroBatchTests" /InIsolation`
- Data source or fixture: `QuickFiler.Test\Controllers\QfcInitEmailQueueZeroBatchTests.cs`, which calls
  `Deedle.Reflection.convertRecordSequence`

Impact / Severity:
- [ ] Blocker
- [x] High
- [ ] Medium
- [ ] Low

Severity is assessed as High on the deduced failure mode, not on an observed production incident. If a
production path reaches Deedle first, every Deedle-backed feature in that Outlook session fails, and a
restart of Outlook is the only recovery. The confidence attached to the severity is correspondingly
lower than it would be for a reproduced defect.


## Repro & Evidence
Steps to Reproduce:
The reproduction below is in the test host. No production reproduction was attempted.

1. Build the solution in Debug.
2. Run the single class `QfcInitEmailQueueZeroBatchTests` through `vstest.console.exe` with no other test
   class selected and no runsettings file, so that no earlier class has touched `SVGControl.SvgRenderer`.
3. Observe that all three tests fail at Deedle's static initializer with
   `FileNotFoundException: netstandard, Version=2.1.0.0`.

The equivalent production sequence, which has not been executed or observed, is: start the add-in and
reach any code path that loads Deedle before any code path that touches `SVGControl.SvgRenderer`.

Expected:
Loading Deedle should succeed on its own merits in every host, through deployed assemblies and declared
binding redirects, without depending on an `AssemblyResolve` fallback that an unrelated component happens
to have installed first.

Actual:
The bind is unsatisfiable without a fallback handler. Measured in the test host at head `c9590a8b7`:

```
System.TypeInitializationException: The type initializer for 'Deedle.Reflection' threw an exception.
 ---> System.TypeInitializationException: The type initializer for '<StartupCode$Deedle>.$FrameUtils' threw an exception.
 ---> System.IO.FileNotFoundException: Could not load file or assembly
      'netstandard, Version=2.1.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51'
      or one of its dependencies. The system cannot find the file specified.
```

The CLR caches a failed type initializer for the lifetime of the process. Once `Deedle.Reflection` has
failed once, every later use of Deedle in that process raises the same `TypeInitializationException`,
including uses that occur after the fallback handler has been installed. A production process that
reaches Deedle before it renders SVG would therefore be permanently unable to use Deedle, not
transiently.

Logs / Screenshots:
- [x] Attached minimal logs or screenshot
- Snippet: the exception chain quoted above, captured from a `vstest.console.exe` run of
  `QfcInitEmailQueueZeroBatchTests` alone at head `c9590a8b7`.


## Scope & Non-Goals
- In scope:
- Out of scope / non-goals:
- Explicitly excluded systems, integrations, or datasets:

## Root Cause Analysis
Measured facts, all verified in the repository at head `c9590a8b7`:

- `Deedle.dll` is deployed as `Deedle, Version=3.0.0.0, PublicKeyToken=null` and references FSharp.Core
  4.5.0.0.
- `QuickFiler.Test\app.config` and `UtilitiesCS.Test\app.config` both redirect FSharp.Core to 11.0.0.0.
- The deployed `FSharp.Core.dll` is `FSharp.Core, Version=11.0.0.0, PublicKeyToken=b03f5f7f11d50a3a`,
  and it references `netstandard, Version=2.1.0.0`.
- No `*.config` file anywhere in the repository contains the string `netstandard`, so no binding redirect
  covers that reference in any host, production included.
- `netstandard.dll` is not present in `QuickFiler.Test\bin\Debug`.
- The GAC on this machine contains exactly one `netstandard`: `v4.0_2.0.0.0__cc7b13ffcd2ddd51`.
- netstandard 2.0.0.0 and the requested netstandard 2.1.0.0 share the public key token
  `cc7b13ffcd2ddd51`, which is why a fallback that matches on simple name plus public key token
  satisfies the bind while the default binder does not.
- The repository contains exactly two first-party `AssemblyResolve` handlers:
  `UtilitiesCS.Test\TestAssemblyInitializer.cs`, which is a test-only handler installed from that
  assembly's `[AssemblyInitialize]`, and `SVGControl\SvgAssemblyResolver.cs`, which is installed by
  `SvgAssemblyResolver.Install()` from the `SVGControl.SvgRenderer` static constructor.

Production therefore has exactly one handler available, and it is installed lazily. The add-in appears
safe today only because the VSTO surface renders SVG - the QuickFiler viewers instantiate
`SVGControl.ButtonSVG` and `SVGControl.SvgResource` - before anything reaches Deedle. That is an
ordering accident of the current UI flow, not a guarantee. Nothing in the code declares or enforces
the ordering, so any new or reordered production path that reaches Deedle first hits the same
unsatisfiable bind.

The FSharp.Core redirect to 11.0.0.0 is the proximate trigger: it is what makes a netstandard 2.1.0.0
reference reachable at all. Whether the correct remedy is a `netstandard` binding redirect, deploying a
`netstandard` facade, pinning FSharp.Core to a version that does not reference netstandard 2.1, or
installing the resolver eagerly in production startup, is open and should be settled by the
investigation this entry requests.


## Proposed Fix

### Design summary (what changes where):

### Boundaries and invariants to preserve:

### Dependencies or blocked work:

### Implementation strategy (what changes, not sequencing):
	
#### Files/modules to change:

#### Functions/classes/CLI commands impacted:

#### Data flow and validation changes:

#### Error handling and logging updates:

#### Rollback/feature-flag considerations (if applicable):

### Technical specifications (interfaces/contracts):

#### Inputs/outputs and formats:

#### Required configuration keys and defaults:

#### Backward-compatibility expectations:

#### Performance constraints (latency/throughput/memory):

## Assumptions, Constraints, Dependencies
- Assumptions (environment, data, access):
- Constraints (budget, performance, compatibility):
- External dependencies (services, libraries, releases):

## Data / API / Config Impact
- User-facing or API changes:
- Data or migration considerations:
- Logging/telemetry updates (if any):
- Compatibility notes (CLI flags, config schemas, versioning):

## Test Strategy
Seeded from issue:

- [ ] Unit coverage areas: a host-neutral test that loads Deedle in a process with no `AssemblyResolve`
      fallback installed and asserts the load succeeds.
- [ ] Integration scenario to retest: establish whether any production code path can reach Deedle before
      `SVGControl.SvgRenderer` is touched. This is the investigation that was not performed and is the
      first task of any work on this entry.
- [ ] Manual verification notes: a live-Outlook check is a manual human gate. Exercise a Deedle-backed
      feature in a freshly started Outlook session without first opening any SVG-bearing QuickFiler
      surface, and record whether Deedle loads.
- [ ] Decide the remedy only after the reachability question above is answered; do not install a
      workaround for a path that may not exist.

- Regression tests to add or update:
- Unit tests (pytest) for the fixed behavior and boundaries:
- Edge cases and negative scenarios (invalid inputs, missing data, boundary values):
- Error handling and logging verification:
- Coverage impact and targets for changed lines/modules:
- Toolchain commands to run (format → lint → type-check → test):
- Manual validation steps (if required):


## Acceptance Criteria
- [ ] Repro steps now produce the expected behavior in all documented environments.
- [ ] Regression test(s) added and passing (list file path and test name).
- [ ] Edge cases and invalid inputs are handled with correct errors or fallbacks.
- [ ] No unintended behavior changes outside the defined scope.
- [ ] Required logs/telemetry updated and validated (if applicable).
- [ ] Performance constraints met or explicitly waived with rationale.
- [ ] Full toolchain pass completed (format → lint → type-check → test).
- [ ] Docs/config references updated to match the new behavior.

## Risks & Mitigations
- Technical or operational risks:
- Mitigations and rollbacks:

## Rollout & Follow-up
- Release/rollout steps:
- Post-fix monitoring or clean-up tasks:
- Links: issue, PRs, related docs
