# Phase 0 — Resolution of the Two Unverified Points Carried from Research

Timestamp: 2026-08-26T10-42
Task: [P0-T12]
Command: `git grep -n` over `QuickFiler.Test/Controllers/EfcHomeControllerTests.cs` and
`QuickFiler/Controllers/QfcHomeController.cs`, plus a full read of both files
EXIT_CODE: 0

## A-10 — the two `EfcHomeControllerTests` guard tests survive the `int` to `double` widening

**Finding: neither test would break. A-10 is confirmed. This is not a blocker and Phase 2 may
proceed.**

`QuickFiler.Test/Controllers/EfcHomeControllerTests.cs` is a forbidden-to-write file. It was read
in full for this determination and is not modified by this feature.

The two tests are declared at:

- `QuickFiler.Test/Controllers/EfcHomeControllerTests.cs:81`
  `QuickFileMetrics_WRITE_WithEmptyList_SkipsBodyAndDoesNotThrow`
- `QuickFiler.Test/Controllers/EfcHomeControllerTests.cs:100`
  `QuickFileMetrics_WRITE_WithNullList_SkipsBodyAndDoesNotThrow`

Both call the **three-argument** public overload
`QuickFileMetrics_WRITE(string, string, List<MailItemHelper>)`, whose body begins with the guard
at `QuickFiler/Controllers/EfcHomeController.Metrics.cs:18-21`:

```csharp
if (moved is null || moved.Count == 0)
{
    return;
}
```

The empty-list test passes `new List<MailItemHelper>()`, satisfying the `moved.Count == 0`
disjunct. The null-list test passes `null`, satisfying the `moved is null` disjunct. Both return
at line 20 and never reach line 23, which is the only line in that overload that supplies the
`elapsedSeconds` argument. Neither test observes the parameter's declared type, and neither
asserts anything about a duration value: each asserts only `.Should().NotThrow(...)`.

Widening the parameter at `:35` and `:57` from `int` to `double` therefore cannot change either
test's outcome. The widening also cannot introduce a compile break at their call sites, because
neither test names the four-argument overload.

A secondary confirmation: both tests build their controller through `CreateMinimalController()`,
whose `_stopWatch` is null. Their in-file comments state that entering the body would throw
`NullReferenceException` on `_stopWatch.Elapsed` before reaching any arithmetic. That the tests
pass today is itself evidence that the guard returns before line 23.

## A-11 — members of the `QfcHomeController.cs` partial that consume `System.Collections.Concurrent` or `System.Timers`

**Finding: the only consumers are members deleted by [P5-T10]. A-11 is confirmed.**

`QuickFiler/Controllers/QfcHomeController.cs` was read in full. The complete consumer list is:

### `System.Collections.Concurrent` (imported at line 2)

| Site | Consuming member | Type consumed | Disposition |
| --- | --- | --- | --- |
| `:353-355` | field `_metrics` | `BlockingCollection<string>`, `ConcurrentQueue<string>` | deleted by [P5-T10] |

No other member of the partial names any type from that namespace.

### `System.Timers` (imported at line 11)

| Site | Consuming member | Type consumed | Disposition |
| --- | --- | --- | --- |
| `:362` | method `TimedConsumerAsync` (parameter `e`) | `ElapsedEventArgs` | deleted by [P5-T10] |

No other member of the partial names any type from that namespace. The
`new System.Timers.Timer(2000)` at `QuickFiler/Controllers/QfcHomeController.Metrics.cs:229` is
fully namespace-qualified, sits in a different file of the partial, and is deleted by [P5-T9]; it
does not depend on the `using System.Timers;` directive in `QfcHomeController.cs`.

Both directives are therefore safe to remove in [P5-T12] once [P5-T10] has run.

### `System.Linq` (imported at line 7) — the re-evaluation [P5-T12] also requires

| Site | Consuming member | Expression | Disposition |
| --- | --- | --- | --- |
| `:367` | method `TimedConsumerAsync` | `_metrics.GetConsumingEnumerable().ToArray()` | deleted by [P5-T10] |

This is the only LINQ extension-method call anywhere in `QfcHomeController.cs`. Once
`TimedConsumerAsync` is deleted the directive is expected to be unused as well. [P5-T12] treats
the compiler and the analyzer as the authority and reverts the removal within that task if it
produces a diagnostic.

## Blocker status

No blocker. A-10 resolves to "would not break", so the plan's stop condition before Phase 2 is
not triggered.
