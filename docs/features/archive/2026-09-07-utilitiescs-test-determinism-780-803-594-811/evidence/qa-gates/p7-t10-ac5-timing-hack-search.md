# P7-T10 — AC5 timing-hack search over the anchored diff

Timestamp: 2026-09-08T10-21
Task: [P7-T10]
Command: `git diff bb1c7d4b60f7b782227956f36859314d5c47bb03 -- "*.cs" ":(exclude).claude"`, filtered to added lines (`^\+` and not `^\+\+\+`), then counted with `Select-String -SimpleMatch` and the stated regexes
EXIT_CODE: 0

AC5 has no automated enforcement in this repository: `BannedSymbols.txt` bans `Thread.Sleep` and
`Task.Delay` but not `CancelAfter`, `TimeoutAfter` or `WaitOne`, and `.editorconfig` holds RS0030
at `suggestion`, so no toolchain step fails on a newly introduced sleep. This explicit search over
the diff is therefore the enforcement mechanism, as spec.md records under "AC5 enforcement, stated
honestly".

## Scope of the search

| Observation | Value |
|---|---|
| Total diff lines over `*.cs` (dot-claude excluded) | 1530 |
| Added lines examined | 737 |

## Fixed-string counts over added lines

| Pattern | Count | Verdict |
|---|---|---|
| `Thread.Sleep` | 0 | OK |
| `Task.Delay` | 0 | OK |
| `CancelAfter` | 0 | OK |
| `WaitOne` | 0 | OK |
| `Returns(120)` | 0 | OK |
| `SpinWait` | 0 | OK |
| `Stopwatch` | 0 | OK |

`Stopwatch` counts 0 as the plan predicts: the production files' existing stopwatches
(`OlTableExtensions.Etl.cs`, `DfDeedle.cs`) are pre-existing lines, not added ones, so they do not
enter this search.

## Regex shape counts over added lines

| Shape | Meaning | Count | Verdict |
|---|---|---|---|
| `new CancellationTokenSource\(\s*\d` | a numeric constructor timeout | 0 | OK |
| `\.Wait\(\s*(\d\|TimeSpan)` | a timed wait | 0 | OK |
| `catch \(`, restricted to added lines in hunks of files under `UtilitiesCS.Test/` | a retry loop needs a catch | 0 | OK |
| `Returns\(\s*\d{2,}\s*\)` on a line that also contains `GetRowCount` | a mock value chosen to outrun a deadline | 0 | OK |

The `catch (` restriction was applied by tracking the `diff --git` header of each file section and
only counting added lines while inside a section whose post-image path is under `UtilitiesCS.Test/`.
The two new test classes use `finally` only, to release their gates; neither catches anything.

## RETIRED

The existing timing tolerance is gone from the final tree, not merely absent from the added lines:

| Token in `UtilitiesCS.Test/OutlookObjects/Table/OlTableExtensions_Tests.cs` | Final count |
|---|---|
| `Returns(120)` | 0 |
| `cannot fire under test-host` | 0 |

Both were 1 at the base commit (measured by P0-T6). The mock row count that was set to 120 so the
250 ms deadline could not fire is replaced by an un-advanced `FakeTimeProvider`, which controls the
clock rather than widening the deadline.

## RETAINED_GATES

Two occurrences of a `.Wait` member reference survive in the two new test files, and both are
release-by-test gates rather than timed waits:

| File | Occurrence | Form |
|---|---|---|
| `DfDeedleEtlTimeoutTests.cs` | `BuildExplorer(BuildFolderWithTriageUdp(), gateA.Wait, gateB.Wait)` | two method-group references, passed as delegates |
| `OlTableExtensionsEtlClockTests.cs` | `gate.Wait,` | one method-group reference, passed as a delegate |

Count: 2 lines, carrying 3 method-group references. Each is `ManualResetEventSlim.Wait()` with no
timeout argument, invoked inside a mock callback so that the production code blocks until the test
itself releases the gate in a `finally`. A wait with no timeout cannot mask a timing defect: it
does not bound how long the code under test may take, and the test's outcome is decided by the
injected clock, not by elapsed wall time. The `\.Wait\(\s*(\d|TimeSpan)` regex above is what would
catch a timed variant, and it counts 0. This is the same pattern the pre-existing
`DfDeedleQfcColumnTimeoutTests` already uses.

## Acceptance evaluation

- Every one of the eleven counts is 0. PASS
- Each pattern is listed with its count. PASS
- A separate `RETIRED:` record shows `Returns(120)` and `cannot fire under test-host` both count 0
  in the final tree of `OlTableExtensions_Tests.cs`. PASS
- A `RETAINED_GATES:` record identifies the surviving `gate.Wait` occurrences as untimed
  release-by-test gates, with their count. PASS

## Output Summary

No sleep, delay, retry, timed wait, spin, numeric token-source timeout, or deadline-outrunning
mock value appears anywhere in the 737 added lines. The one pre-existing timing tolerance in the
touched files was retired rather than retained. AC5 is satisfied on the technique, not only on the
outcome.
