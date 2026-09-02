# Code Review — Issue #647 (FileIO2 write retry reports success on final failure)

- Timestamp: 2026-08-31T19-44
- Branch: `bug/fileio2-write-retry-reports-success-on-final-failure-647`
- Head reviewed: `8e773f350671c29f2ff34803df63ac60d70ed648`
- Base: `9b6aff2e886eb86af5dfc131ebee7a2ebe1a5b6c`
- Review basis: full branch diff, read line by line, plus the head state of all five changed files

## Verdict

**Blocking findings: 0.** Non-blocking findings recorded in this artifact: 7 (C-1 through C-7).

## Files Reviewed

| Path | Diff | Lines at head | Limit |
|---|---|---|---|
| `UtilitiesCS/To Depricate/FileIO2.cs` | +85/-24 | 293 | 500 |
| `QuickFiler/Controllers/QfcHomeController.Metrics.cs` | +16/-4 | 227 | 500 |
| `TaskMaster/AppGlobals/AppOlObjects.cs` | +39/-6 | 494 | 500 |
| `UtilitiesCS.Test/HelperClasses/FileIO2_Tests.cs` | +245/-32 | 335 | 500 |
| `QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs` | +19/-11 | 454 | 500 |

## Correctness Assessment

The control-flow restructure in `FileIO2.WriteTextFileAsync` is correct and it fixes both defects the spec identifies.

**Defect 1 (exhaustion reports success).** The loop is now `while (true)` with explicit `return` statements, so the flag that ended the loop and the value that reports success are no longer the same variable. The exhaustion branch at lines 137–144 increments `attempts`, and on reaching 100 logs and returns `false`. Retry arithmetic is preserved exactly: the pre-change form incremented then took `attempts < 100`; the new form increments then takes `attempts >= 100`, producing the same 100 open attempts and 99 delays. The exhaustion test asserts those two counts directly, which is the right way to pin an invariant that is easy to break by an off-by-one during a refactor.

**Defect 2 (mid-write failure reports success).** `bool opened` is declared inside the loop body and set immediately after the writer is constructed. An `IOException` raised by `WriteLineAsync` or by the flush inside the implicit `Dispose` reaches the catch with `opened == true` and returns `false` without consuming retry budget. This is the correct treatment for an append-mode writer: retrying after a partial flush would duplicate already-written lines, which would be a new data-corruption mode. The zero-delay assertion in the mid-write test is the observable proof that no retry occurred, and it is exactly the assertion that failed against pre-fix source with `found 1`.

**Success establishment.** `return true` at line 124 sits after the `using` block closes, so the writer has been disposed without error before the method reports success. A flush failure during disposal is caught and returns `false`. This is the specific behavior the issue asked for and it is implemented at the only point where it can be true.

**Note for future readers:** the assignment `opened = true` at line 117 does sit between the writer's creation and the writes, but it does not establish success. It marks the attempt as post-open so that failures become terminal. Confusing it with the deleted `success = true` would be a misreading; the comment at lines 108–110 says so, which is appropriate use of a "why" comment.

**Exception contract.** The catch is not widened: it remains `catch (IOException ex)`, so `UnauthorizedAccessException` and `NotSupportedException` still propagate. `token.ThrowIfCancellationRequested()` still runs before each attempt, and the delay now routes through the caller's token, so `TaskCanceledException` (a subclass of `OperationCanceledException`) is the only new fault shape and it is already inside the documented contract.

**Caller inventory.** A repository-wide grep for `WriteTextFileAsync` across `*.cs` returns the property default at `QfcHomeController.Metrics.cs:34`, the invocation at `:179`, the `AppOlObjects.cs:315` call, and the test file. All are updated. The method-group conversion at `:34` binds unambiguously because the two overloads differ in arity and the `internal` overload is invisible outside `UtilitiesCS`; the risk the spec flagged as decision 10 (silent discard through a reference conversion) is closed at every site by an explicit edit, and the analyzer and nullable builds confirm both inferred conversions compile.

## Design and Policy Assessment

| Dimension | Assessment |
|---|---|
| Simplicity first | The `bool` return is the smallest surface that makes the failure observable. The rejection of a dedicated result type is argued in the spec and the argument holds: no caller differentiates the extra information |
| Separation of concerns | The writer factory and the delay are injected as parameters rather than static state. This is the right call for an assembly running `Parallelize(Scope = ExecutionScope.ClassLevel)`; a static seam would be a genuine cross-class race |
| Extensibility | `Func<string, TextWriter>` rather than `Func<string, StreamWriter>` is a deliberate and correct widening relative to the `SmartSerializableBase.CreateStreamWriter` precedent, and it is what makes an in-memory success-path test possible at all |
| Error handling | Fails explicitly rather than silently. The exhaustion path now carries its causing exception to the appender, which it previously discarded; the mid-write path gains a log entry where it previously produced none |
| Logging | Two textually distinct messages with different operational meanings (contention versus a partially appended file), both through the existing log4net logger, both using the two-argument `Error(object, Exception)` overload. Each caller logs at its own boundary, so a failed write is attributable to the caller as well as to the writer |
| Naming | `opened`, `createWriter`, `delayAsync`, `metricsWritten`, `movedMailsWritten` are all descriptive and behavior-named |
| Documentation | The public method's `<returns>` clause states both outcomes and explicitly says the method does not throw on a failed write, which is the one thing a caller most needs to know. The seam's summary records why parameters were chosen over static state |
| Public API compatibility | Source compatibility is preserved; binary compatibility is broken by the return-type change. All consumers are in-repo and rebuild together, and `MetricsFileWriter` is `internal`. Called out in the spec rather than left implicit |
| Test quality | Six tests, each with a doc comment stating the scenario, explicit Arrange/Act/Assert sections, and assertions on counts rather than on timing. Positive, negative, boundary (99/100), error-handling and both cancellation entry points are covered |

## Non-blocking Findings

**C-1 — `TaskMaster/AppGlobals/AppOlObjects.cs` is at 494 of the 500-line limit.** This change consumed 27 of the 33 lines of headroom the file previously had: 23 for the block-bodied lambda and 4 for the `using Exception = System.Exception;` alias and its comment. Six lines remain. The next edit of any size to this file will breach `.claude/rules/general-code-change.md`. Recommendation: extract the `TimedDiskWriter<string>` construction, including its `DiskWriter` lambda, into a small private factory method in a partial or a dedicated type before the next change lands here. Severity: Minor. Not a violation today.

**C-2 — The public `WriteTextFileAsync` forwarder at `FileIO2.cs:74` has zero test coverage, so nothing verifies that the production defaults are actually selected.** Line 74 measures `hits="0"` in the Cobertura document, as does line 101, the wrapped right operand of the writer-factory coalescing expression. Together these mean no test observes that a null `writerFactory` yields `new StreamWriter(p, true, System.Text.Encoding.UTF8)` in append mode with UTF-8, nor that the public overload forwards with both delegates null. A regression that changed the append flag or the encoding in the default lambda would pass the entire suite. Covering it directly would require filesystem I/O, which `.claude/rules/general-unit-test.md` prohibits, so the omission is defensible rather than careless. Recommendation: either accept it explicitly (the spec already anticipates one such line) or add a seam-parity assertion that constructs the default factory expression once and asserts on the resulting writer's type and encoding without writing to disk. Severity: Minor.

**C-3 — The new failure branch at `QfcHomeController.Metrics.cs:185-191` has no test.** All six `MetricsFileWriter` doubles in `QfcHomeControllerMetricsTests.cs` now return `Task.FromResult(true)` or `return true`, so the `if (!metricsWritten)` arm is never entered; lines 186 through 191 measure `hits="0"`. This is the one place in the change where the new failure signal is consumed by a caller with a testable seam, which makes it the most valuable untested line in the diff. The spec itself anticipated it: "a test asserting that `WriteMetricsAsync` logs when the writer returns `false` is a reasonable addition once that logging exists." Adding a double that returns `Task.FromResult(false)` would execute the branch, but it could assert nothing, because `logger` is a static log4net field on the class and the spec correctly rules an injectable logger out of this change's scope. Recommendation: promote a follow-up issue introducing a logging seam on `QfcHomeController` so the failure-path log becomes assertable, rather than adding a coverage-only test that executes the line without checking it. Severity: Minor.

**C-4 — `using Exception = System.Exception;` in `AppOlObjects.cs` is a file-scope alias and is broader than the problem it solves.** It was added to resolve CS0104 against `Microsoft.Office.Interop.Outlook.Exception` for the single new `catch` clause. Verified safe at head: a grep of the file finds exactly one unqualified `Exception` token in code, the new catch at line 328; the other matches are `COMException`, `InvalidOperationException`, or prose in comments and XML docs. The latent hazard is that the alias silently rebinds every future unqualified `Exception` in a 494-line file that is otherwise saturated with Outlook Interop types, so a later edit intending the Outlook `Exception` type would compile against the BCL type with no diagnostic. A fully qualified `catch (System.Exception ex)` would have been the narrower fix and would have cost 4 fewer lines against the file-size margin in C-1. Severity: Minor.

**C-5 — The broad `catch (Exception ex)` in the async-void `DiskWriter` lambda now swallows every exception type after logging.** Before this change, a non-`IOException` failure inside that lambda escaped an async void body on a `System.Timers.Timer` callback and terminated the Outlook host process. That is the crash the spec's decision 2 argues must not be introduced, and preventing it here is correct: `.claude/rules/general-code-change.md` permits a broad catch at a clear boundary when it propagates with added context, and this one logs the exception with the target filename. The behavior change worth recording is that failures which previously produced a loud process termination now produce only a log entry, so an operator who was previously alerted by a crash will now only see it in the log. The in-code comment at lines 305–310 explains the reasoning, which is the correct treatment. Severity: Advisory.

**C-6 — `Interlocked.Increment(ref attempts)` is retained on a method-local that is never touched concurrently.** The counter is captured by the async state machine and mutated only on the single logical thread of execution, so the interlocked call buys nothing. Retention is deliberate: the spec lists its removal under non-goals and `evidence/qa-gates/p8-t3-promotion-requests.md` entry 3 records it for promotion as a `minor-audit`. Recorded here so the promotion is not lost. Severity: Advisory.

**C-7 — Fourteen `QuickFiler.Test` pump-host and dispatcher tests are load-sensitive under `/EnableCodeCoverage`.** The first P6-T5 invocation reported all fourteen failing at a duration of approximately one minute, which is a fixed timeout rather than an assertion failure; a byte-identical re-run passed 6899 of 6899, and a run of `QuickFiler.Test` without the coverage collector also passed. The characterization is sound and the attribution is convincing: the tests drive a real message pump on a dedicated thread and contend for a process-wide static dispatcher field, and none of them touches `FileIO2` or any file in this footprint. This is pre-existing determinism debt against `.claude/rules/general-unit-test.md`, surfaced by this change's full-suite run rather than caused by it. Recommendation: promote it; a timeout that is reachable under normal collector overhead will keep producing false regressions in every future full-suite gate. Severity: Minor, pre-existing.

## Positive Observations

Recorded because they are load-bearing and should survive into future reviews of this area.

1. The mid-write regression test is genuine fail-before evidence, not a reconstruction. Landing the seam ahead of the loop restructure made it possible to drive unfixed control flow deterministically, and the recorded failure message (`Expected midWriteDelayCalls to be 0, but found 1`) is the exact defect signature. The assertion ordering that makes that message readable is documented in the evidence and must not be reordered.
2. Deleting `WriteTextFileAsync_WhenTargetIsLocked_ShouldRetryAndExitWithoutThrowing` removed a real hazard, not merely a slow test. It held `UtilitiesCS.Test/TestData/FileIO2/sample.csv` open with `FileShare.None` while an append-mode writer attempted to write to it; a sibling test in the same class asserts that fixture's exact contents. The suite was safe only because the write was guaranteed to fail, which the fix now changes the semantics of. Retiring it was necessary, not optional.
3. The exhaustion test asserts factory and delay invocation counts rather than elapsed time, so the 99-delay retry window executes in 2 ms with no wall-clock dependency.
4. The `<returns>` documentation states that the method does not throw on a failed write. That sentence is what prevents the next caller from repeating the original mistake.

## Summary

- Blocking findings: **0**
- Non-blocking findings: **7** (C-1 through C-7)
- No correctness or safety defect was found in the change
- Recommended follow-ups: promote C-3 (logging seam on `QfcHomeController`) and C-7 (pump-host test timeouts); address C-1 before the next edit to `AppOlObjects.cs`
