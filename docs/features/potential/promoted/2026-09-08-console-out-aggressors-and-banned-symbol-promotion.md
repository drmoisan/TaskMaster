# console-out-aggressors-and-banned-symbol-promotion (Issue #826)

- Date captured: 2026-09-08
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/console-out-aggressors-and-banned-symbol-promotion/ (Issue #826)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #826
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/826
- Last Updated: 2026-09-08
## Summary

Residual process-wide console mutation and analyzer-severity work left in place by issue #811.
#811 eliminated the four `Console.Out` capture-and-assert sites in `UtilitiesCS.Test` by adding a
`TextWriter` seam to the four production members they exercised, and removed the propagating
save/restore in `NLogTraceWriter_Test`. It deliberately did not touch the roughly 24 test classes
that replace `Console.Out` and never restore it, the two production `Console.WriteLine`
diagnostics in `OlTableExtensions.TableAccess.cs`, or the RS0030 analyzer severity.

## Environment

- OS/version: Windows 11 Pro 10.0.26200 locally; GitHub Actions windows runner in CI
- Runtime: .NET Framework 4.8.1 test host, VSTest 18.9.0, MSTest 4.4.0, class-level parallelism
- Command/flags used: `vstest.console.exe <nine assemblies> /EnableCodeCoverage /InIsolation`
- Data source or fixture: not applicable; these are static-source observations

## Steps to Reproduce

Read the cited sites. None of these produces a failure today, because after #811 no test asserts
on `Console.Out` content.

## Expected Behavior

1. A test does not mutate process-wide state it never restores.
2. Production code reports diagnostics through the logger, not through the console.
3. Banned timing APIs are enforced by the build rather than by a reviewer's diff search.

## Actual Behavior

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

## Impact / Severity

- [ ] Blocker
- [ ] High
- [ ] Medium
- [x] Low

Low: no current failure depends on any of these. Item 1 is latent risk that returns the moment any
future test captures `Console.Out`; item 3 means the next timing hack will be caught only if a
reviewer looks for it.

## Suspected Cause / Notes

Items 1 and 2 are long-standing conventions that predate the determinism work. Item 3 is a
deliberate staged rollout recorded in `.claude/rules/csharp.md` under the severity-first ordering
invariant: new analyzer severities are set to `suggestion` before the analyzer is wired in, because
the type-check step runs `/p:TreatWarningsAsErrors=true` and would promote a `warning` to an error.

## Proposed Fix / Validation Ideas

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

## Next Step

- [ ] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch
