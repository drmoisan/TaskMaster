# 2026-09-08-console-out-aggressors-and-banned-symbol-promotion (Spec)

- **Issue:** #826
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-09-08T23-52
- **Status:** Draft
- **Version:** 0.1

## Context
Residual process-wide console mutation and analyzer-severity work left in place by issue #811.
#811 eliminated the four `Console.Out` capture-and-assert sites in `UtilitiesCS.Test` by adding a
`TextWriter` seam to the four production members they exercised, and removed the propagating
save/restore in `NLogTraceWriter_Test`. It deliberately did not touch the roughly 24 test classes
that replace `Console.Out` and never restore it, the two production `Console.WriteLine`
diagnostics in `OlTableExtensions.TableAccess.cs`, or the RS0030 analyzer severity.

Environment:
- OS/version: Windows 11 Pro 10.0.26200 locally; GitHub Actions windows runner in CI
- Runtime: .NET Framework 4.8.1 test host, VSTest 18.9.0, MSTest 4.4.0, class-level parallelism
- Command/flags used: `vstest.console.exe <nine assemblies> /EnableCodeCoverage /InIsolation`
- Data source or fixture: not applicable; these are static-source observations

Impact / Severity:
- [ ] Blocker
- [ ] High
- [ ] Medium
- [x] Low

Low: no current failure depends on any of these. Item 1 is latent risk that returns the moment any
future test captures `Console.Out`; item 3 means the next timing hack will be caught only if a
reviewer looks for it.


## Repro & Evidence
Steps to Reproduce:
Read the cited sites. None of these produces a failure today, because after #811 no test asserts
on `Console.Out` content.

Expected:
1. A test does not mutate process-wide state it never restores.
2. Production code reports diagnostics through the logger, not through the console.
3. Banned timing APIs are enforced by the build rather than by a reviewer's diff search.

Actual:
1. **Roughly 24 test classes install a `DebugTextWriter` with no restore.** They call
   `Console.SetOut(new DebugTextWriter())` from a `[ClassInitialize]` or `[TestInitialize]` method
   and never put the original writer back, so `Console.Out` is an arbitrary writer for the
   remainder of the run. `ObsoleteBayesianClassifier_Tests.cs` does it twice. These are aggressors
   rather than victims: none of them asserts on console content, so none can itself fail this way.
   After #811 they harm nothing, because no test captures `Console.Out` any more. They remain the
   reason `Console.Out` is not the console once the suite has started. #811 excluded them because
   touching roughly 24 files across five test projects is a disproportionate blast radius for a
   bugfix.

2. **Two production `Console.WriteLine` diagnostics** at
   `UtilitiesCS/OutlookObjects/Table/OlTableExtensions.TableAccess.cs` lines 78 and 96, both
   reading `Console.WriteLine($"Task timed out on try {counter}")` inside `GetTableInViewAsync`.
   Production code in this repository is supposed to use the logger; these two write to whatever
   writer the process currently holds, which after item 1 is a `DebugTextWriter`. #811 seamed
   `EnumerateTable` in the same file but left these two untouched because they are not part of the
   AC3 capture-and-assert population, and changing them is a logging change rather than a
   determinism fix.

3. **RS0030 is held at `suggestion` severity** in `.editorconfig`, and `BannedSymbols.txt` covers
   only `DateTime.Now`, `DateTime.UtcNow`, `Random.Shared`, `Thread.Sleep` and `Task.Delay`.
   `CancelAfter`, `TimeoutAfter`, `WaitOne` and `new CancellationTokenSource(int)` are not banned
   at all. The consequence is that no toolchain step fails on a newly introduced sleep, so AC5 of
   #811 had to be enforced by an explicit search over the diff
   (`evidence/qa-gates/p7-t10-ac5-timing-hack-search.md`) rather than by the build. The severity is
   held down because roughly 143 existing banned-symbol usages would otherwise break the build; see
   issue #181.


## Scope & Non-Goals
- In scope:
- Out of scope / non-goals:
- Explicitly excluded systems, integrations, or datasets:

## Root Cause Analysis
Items 1 and 2 are long-standing conventions that predate the determinism work. Item 3 is a
deliberate staged rollout recorded in `.claude/rules/csharp.md` under the severity-first ordering
invariant: new analyzer severities are set to `suggestion` before the analyzer is wired in, because
the type-check step runs `/p:TreatWarningsAsErrors=true` and would promote a `warning` to an error.


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

- Replace the roughly 24 unrestored `Console.SetOut(new DebugTextWriter())` installs with either a
  restoring scope or, better, removal: none of those classes asserts on console output, so the
  redirect serves no test purpose. Doing this in one sweep across the five test projects is the
  cheapest form.
- Route `OlTableExtensions.TableAccess.cs:78,96` through the existing `logger` and delete the
  console writes. `Console.WriteLine` in that file then drops to 0.
- Clear the roughly 143 existing banned-symbol usages, then promote RS0030 from `suggestion` to
  `warning`, and consider adding `CancelAfter`, `WaitOne` and `new CancellationTokenSource(int)` to
  `BannedSymbols.txt` so that AC5-style constraints become build-enforced. Sequence matters: the
  cleanup must land before the promotion or the nullable gate breaks.

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
