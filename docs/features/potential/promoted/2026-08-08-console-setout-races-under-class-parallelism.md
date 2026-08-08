# console-setout-races-under-class-parallelism (Issue #520)

- Date captured: 2026-08-08
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/console-setout-races-under-class-parallelism/ (Issue #520)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #520
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/520
- Last Updated: 2026-08-08
## Summary

`UtilitiesCS.Test` runs under `[assembly: Parallelize(Workers = 0, Scope = ExecutionScope.ClassLevel)]`, and 29 test files in that assembly redirect the **process-global** `Console.Out` via `Console.SetOut(...)` and then restore it in a `finally`. Because `Console.Out` is process-global and the redirecting classes are not serialized against one another, two such classes running concurrently on different workers interleave their set/restore pairs: one class's restore clobbers the other's redirect, so the second class's `StringWriter` captures nothing and its content assertion fails.

Observed failure: `PrintTree_WritesIndentedTreeToConsole` failed once in a full-suite run and passed on an immediate re-run of the same assembly with no code change.

This is the same defect class as issue #508 (a test whose precondition is unarranged process/thread-global ambient state under class-level parallelism), but with a different global (`Console.Out` rather than a WPF `Dispatcher`).

## Environment

- OS/version: Windows 11 Pro 10.0.26200
- Runtime: .NET Framework 4.8.1, MSTest via `vstest.console.exe` (VS18 test platform)
- Command/flags used: `pwsh -NoProfile -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -Configuration Debug`
- Data source or fixture: none

## Steps to Reproduce

1. Build the solution in Debug.
2. Run the full MSTest suite with coverage (class-level parallelization, 24 workers).
3. Repeat. The failure is probabilistic, not every-run.

Observed on a branch whose only source change was two unrelated files in `UtilitiesCS/OutlookObjects/Folder/`:

- Full instrumented suite: `Total tests: 6397, Passed: 6396, Failed: 1` — `PrintTree_WritesIndentedTreeToConsole`.
- Immediate re-run of `UtilitiesCS.Test` alone: `Total tests: 4688, Passed: 4688, Failed: 0`.

## Expected Behavior

Tests produce the same result on every run regardless of which other tests execute concurrently, per `.claude/rules/general-unit-test.md` Core Principles 1 (Independence) and 4 (Determinism). A test must not depend on, or mutate, process-global state that a concurrently-executing test also mutates.

## Actual Behavior

The exact method name `PrintTree_WritesIndentedTreeToConsole` is defined **twice**, in two different classes in two different namespaces, and both bodies redirect `Console.Out`:

- `UtilitiesCS.Test/OutlookObjects/DASLFilterParser_Tests.cs:95` — class `DASLFilterParser_Tests`, namespace `UtilitiesCS.Test.OutlookObjects`
- `UtilitiesCS.Test/OutlookObjects/Filter DASL/DASLFilterParserTests.cs:95` — class `DASLFilterParserTests`, namespace `UtilitiesCS.Test.OutlookObjects.FilterDASL`

Both use this shape:

```csharp
using var writer = new StringWriter();
var originalOut = Console.Out;
Console.SetOut(writer);
try { parser.PrintTree(tree, 0); }
finally { Console.SetOut(originalOut); }
writer.ToString().Should().Contain("AND").And.Contain("  A").And.Contain("  B");
```

Neither class carries `[DoNotParallelize]`. Under class-level parallelism the two classes are eligible to run at the same time, so this interleaving is reachable:

1. Class A captures `originalOut` (the real console) and sets `writer_A`.
2. Class B captures `originalOut` (now `writer_A`) and sets `writer_B`.
3. Class A finishes and restores the real console — `writer_B` is now detached.
4. Class B's `PrintTree` output goes to the real console; `writer_B.ToString()` is empty; the `Contain("AND")` assertion fails.

The duplicate method name is a symptom of a broader duplication: `DASLFilterParser_Tests.cs` and `Filter DASL/DASLFilterParserTests.cs` appear to be mirrored copies of the same suite.

## Scope beyond the observed failure

The two DASL classes are the pair that happened to collide, but the hazard is assembly-wide. 29 files in `UtilitiesCS.Test` call `Console.SetOut`:

```
UtilitiesCS.Test/EmailIntelligence/Bayesian/*.cs (7 files)
UtilitiesCS.Test/EmailIntelligence/... (4 more)
UtilitiesCS.Test/NewtonsoftHelpers/*.cs (7 files)
UtilitiesCS.Test/OutlookObjects/DASLFilterParser_Tests.cs
UtilitiesCS.Test/OutlookObjects/Filter DASL/DASLFilterParserTests.cs
UtilitiesCS.Test/HelperClasses/PrettyPrint*.cs (2 files)
... and others
```

Some (for example `PrettyPrint_Tests`, `OlTableExtensions_Tests`) already carry `[DoNotParallelize]`; most do not. Any two non-serialized ones can collide.

## Logs / Screenshots

- [x] Attached minimal logs or snippet
- Snippet:

```text
  Failed PrintTree_WritesIndentedTreeToConsole [215 ms]
Test Run Failed.
Total tests: 6397
     Passed: 6396
     Failed: 1
```

Immediate re-run of the same assembly, no code change:

```text
Total tests: 4688
     Passed: 4688
```

## Impact / Severity

- [ ] Blocker
- [x] High
- [ ] Medium
- [ ] Low

Same rationale as #508 and #511: a suite that is not reliably green at baseline prevents anyone from distinguishing a real regression from noise, and trains reviewers and agents to re-run until green. This instance is worse than a single flaky test because the mechanism is shared by 29 files, so the failure can surface in any of them and will be attributed to whichever one loses the race.

## Suspected Cause / Notes

`Console.Out` is process-global mutable state. The set/restore idiom used here is only safe if no other concurrently-running test touches `Console.Out`.

Two candidate fixes, in the repository's preferred order (`.claude/rules/csharp.md` "DI Seams"):

1. **Seam the output boundary (preferred).** Give `DASLFilterParser.PrintTree` an overload accepting a `TextWriter` (defaulting to `Console.Out`), so tests pass their own writer and never touch the global. This removes the shared state rather than serializing access to it, and matches how #508 was resolved.
2. **Serialize as a stopgap.** Apply `[DoNotParallelize]` to every class that redirects `Console.Out`. This masks rather than removes the coupling and costs suite wall-clock time; acceptable only as an interim step.

Also worth resolving as part of this work: `DASLFilterParser_Tests.cs` and `Filter DASL/DASLFilterParserTests.cs` are mirrored duplicates. Deduplicating them removes one collision pair outright. Note the directory name `Filter DASL` contains a space, which is known to break some tooling regexes in this repo.

Related: #508 (WPF `Dispatcher` ambient precondition), #511 (WinForms pump-host handle race), #516 (TimeoutAfter wall-clock race), #394 (duplicate Compile entry in `UtilitiesCS.Test.csproj`). This is the fourth distinct nondeterminism defect in the same test assembly family.

Found while verifying the #508 fix against a tree merged with current `main`. Both DASL files predate that work (present at merge-base `003c5715`) and neither is in the #508 diff, so it is pre-existing and out of scope for #508.

## Proposed Fix / Validation Ideas

- [x] Unit coverage areas: add a `TextWriter` parameter to `PrintTree` and assert against the injected writer; no global mutation.
- [x] Integration scenario to retest: run the full suite repeatedly (at least 5 runs) and confirm a stable pass count.
- [x] Manual verification notes: audit all 29 `Console.SetOut` call sites; confirm no remaining test mutates `Console.Out` without either a seam or explicit serialization.

## Next Step

- [ ] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch
