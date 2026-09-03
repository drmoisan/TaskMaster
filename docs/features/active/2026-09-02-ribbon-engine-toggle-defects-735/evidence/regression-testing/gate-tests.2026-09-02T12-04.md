# Finding 2 — Gate Fixture Run (P2-T8)

Timestamp: 2026-09-03T02-11
Task: [P2-T8]
Command:

```
& $vstest TaskMaster.Test\bin\Debug\TaskMaster.Test.dll `
  /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation `
  "/TestCaseFilter:FullyQualifiedName~SpamManagerResetGateTests" `
  "/Logger:trx;LogFileName=p2-t8.trx" `
  /ResultsDirectory:docs\features\active\2026-09-02-ribbon-engine-toggle-defects-735\evidence\regression-testing\p2-t8
```

EXIT_CODE: 0

## Results directory contents

Exactly one TRX file and no other entry:

```
p2-t8.trx
```

No MSTest deployment scratch directory was produced (the run passed).

## Counts read from the TRX `ResultSummary/Counters` element

| Counter | Value |
|---|---|
| total | 9 |
| executed | 9 |
| passed | 9 |
| failed | 0 |
| notExecuted | 0 |

Console summary agreed: `Test Run Successful. Total tests: 9  Passed: 9`.

## Per-test outcomes read from the TRX

### The three constructor null-argument cases (evidence for F2-AC1)

| Test | Outcome | Asserted parameter name |
|---|---|---|
| `Constructor_WithNullAutoFileAccessor_ThrowsArgumentNullException` | Passed | `autoFileAccessor` |
| `Constructor_WithNullEnginesAccessor_ThrowsArgumentNullException` | Passed | `enginesAccessor` |
| `Constructor_WithNullNotifyDelegate_ThrowsArgumentNullException` | Passed | `notifyNotReady` |

Each asserts `ArgumentNullException` naming the offending parameter, so the gate's construction
contract is pinned for all three dependencies.

### The six `RunAsync` contract cases (evidence for F2-AC2)

| Test | Outcome | Contract clause covered |
|---|---|---|
| `RunAsync_WithNullReset_ThrowsArgumentNullExceptionBeforeProbingAccessors` | Passed | throws for a null reset delegate BEFORE invoking any accessor |
| `RunAsync_WhenAutoFileAccessorReturnsNull_NotifiesOnceAndDoesNotInvokeReset` | Passed | notifies exactly once, returns a completed task, never invokes reset |
| `RunAsync_WhenManagerIsNull_NotifiesOnceAndDoesNotInvokeReset` | Passed | same, for an unset classifier manager |
| `RunAsync_WhenEnginesAccessorReturnsNull_NotifiesOnceAndDoesNotInvokeReset` | Passed | same, for an absent engines facade |
| `RunAsync_WhenAllDependenciesAvailable_InvokesResetWithResolvedManagerAndEngines` | Passed | returns the reset invocation, passing both resolved dependencies by identity, with no notification |
| `RunAsync_WhenResetFaults_PropagatesUnchangedAndDoesNotNotify` | Passed | no await and no catch, so the fault propagates as the same exception instance and no notice is emitted |

The null-reset case uses strict accessors that throw `InvalidOperationException` if invoked. The
test asserts `ArgumentNullException` with parameter name `reset`, so an ordering regression that
moved the argument check after the accessors would surface as the wrong exception type rather than
passing silently.

The success case asserts `BeSameAs` on both lambda arguments against the exact instances the
accessors returned, so a gate that resolved the dependencies and then passed different ones would
fail.

The faulting case asserts `BeSameAs` on the exception instance, so a gate that caught and re-wrapped
would fail.

## Test-policy compliance

No test in this fixture sleeps, polls, reads the wall clock, touches the filesystem, creates a
temporary file, or starts a message pump. MSTest is the framework, Moq the mocking library and
FluentAssertions the assertion library. The one concrete type on the gate's boundary,
`ManagerAsyncLazy`, is constructed over a mocked `IApplicationGlobals`; its constructor performs a
field assignment and an async-lazy assignment that does not execute its factory, so construction
reaches no disk and no COM.

Output Summary: All nine gate tests pass. EXIT_CODE 0, TRX counters total 9, passed 9, failed 0.
The three constructor cases, the null-reset ordering case, the three not-ready cases, the
pass-through-by-identity success case and the fault-propagation case are each recorded as Passed.
