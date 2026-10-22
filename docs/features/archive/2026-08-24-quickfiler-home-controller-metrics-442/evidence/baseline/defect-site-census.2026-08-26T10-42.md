# Phase 0 — Pre-Fix Defect-Site Census

Timestamp: 2026-08-26T10-42
Task: [P0-T11]
Command: seven `git grep -n` invocations, each listed with its own result below
EXIT_CODE: 0

This artifact is the pre-fix half of the grep-based acceptance criteria AC-7, AC-10, AC-12,
AC-14, and AC-15. Each search below is repeated after the corresponding fix by [P2-T10],
[P4-T11], and [P5-T14]; the pairing of a non-zero pre-fix count with a zero post-fix count is
what makes those criteria falsifiable.

## Output Summary

| # | Search | Scope | Hits |
| --- | --- | --- | --- |
| 1 | `Elapsed.Seconds` | `QuickFiler/Controllers/` | **4** |
| 2 | `int elapsedSeconds` | `QuickFiler/` | **2** |
| 3 | `NotImplementedException` | `QuickFiler/Controllers/EfcHomeController.Metrics.cs` | **1** |
| 4 | `volatile` | `QuickFiler/Controllers/EfcHomeController.cs` | **1** |
| 5 | `Stopwatch.StartNew` | `QuickFiler/Controllers/EfcHomeController.cs` | **1** |
| 6 | `RecipientSender` | `QuickFiler.Test/` | **1** |
| 7 | `NonBlockingProducer|TimedConsumerAsync|_metricsConsumers|_lockObject|_fileName` | `QuickFiler/Controllers/` | **13** |

Every count required to be non-zero is non-zero, and search 5 returns exactly one hit as the
task requires.

### 1. `git grep -n "Elapsed.Seconds" -- QuickFiler/Controllers/`

```
QuickFiler/Controllers/EfcHomeController.Metrics.cs:23:            QuickFileMetrics_WRITE(filename, selectedFolder, moved, _stopWatch.Elapsed.Seconds);
QuickFiler/Controllers/QfcHomeController.Metrics.cs:42:            double duration = _stopWatchMoved.Elapsed.Seconds;
QuickFiler/Controllers/QfcHomeController.Metrics.cs:120:            //Duration = _stopWatchMoved.Elapsed.Seconds;
QuickFiler/Controllers/QfcHomeController.Metrics.cs:121:            Duration = StopWatch.Elapsed.Seconds;
```

Line 120 is the commented-out occurrence the plan's "Planner-identified gate correction" section
names. AC-7 asserts this search returns no match after the fix, and a commented occurrence is
still a match, so [P4-T7] must delete the comment as well as redirect the live read on line 121.

### 2. `git grep -n "int elapsedSeconds" -- QuickFiler/`

```
QuickFiler/Controllers/EfcHomeController.Metrics.cs:35:            int elapsedSeconds
QuickFiler/Controllers/EfcHomeController.Metrics.cs:57:            int elapsedSeconds,
```

Both are the parameters [P2-T4] widens to `double elapsedSeconds`. Both declaring members are
`internal`, so the widening breaks no public API.

### 3. `git grep -n "NotImplementedException" -- QuickFiler/Controllers/EfcHomeController.Metrics.cs`

```
QuickFiler/Controllers/EfcHomeController.Metrics.cs:28:            throw new NotImplementedException();
```

This is the body of the single-argument `QuickFileMetrics_WRITE(string filename)` that
[P2-T8] implements as guarded delegation.

### 4. `git grep -n "volatile" -- QuickFiler/Controllers/EfcHomeController.cs`

```
QuickFiler/Controllers/EfcHomeController.cs:389:        private volatile bool _isExecuting;
```

[P3-T5] changes this declaration to `private int`.

### 5. `git grep -n "Stopwatch.StartNew" -- QuickFiler/Controllers/EfcHomeController.cs`

```
QuickFiler/Controllers/EfcHomeController.cs:176:            var selectionStopwatch = Stopwatch.StartNew();
```

Exactly one hit, at line 176, as the task requires. This is the pre-existing
`selectionStopwatch` call inside the selection-change path and is unrelated to `_stopWatch`. The
two `_stopWatch` construction sites at lines 76 and 225 currently read `_stopWatch = new
Stopwatch();`, which allocates a stopwatch that is never started; that is root cause RC-5.
After [P2-T2] this search returns three hits, at lines 76, 176, and 225.

### 6. `git grep -n "RecipientSender" -- QuickFiler.Test/`

```
QuickFiler.Test/Controllers/EfcHomeControllerMetricsTests.cs:59:                    "07/04/2026,01:05,Quarterly Update,SingleSorted,120,2.00,RecipientSender,Email,Archive/Target,06/30/2026,09:45:10"
```

The concatenated `RecipientSender` substring in the expected literal is the pinned form of the
missing CSV separator defect RC-7. The whole expected line carries 11 comma-separated fields
rather than 12. [P1-T1] replaces the concatenated substring with `,Recipient,Sender,`.

### 7. `git grep -nE "NonBlockingProducer|TimedConsumerAsync|_metricsConsumers|_lockObject|_fileName" -- QuickFiler/Controllers/`

```
QuickFiler/Controllers/QfcHomeController.Metrics.cs:153:            _fileName = filename;
QuickFiler/Controllers/QfcHomeController.Metrics.cs:154:            await NonBlockingProducer(strOutput, Token);
QuickFiler/Controllers/QfcHomeController.Metrics.cs:190:        private async Task NonBlockingProducer(string[] lines, CancellationToken ct)
QuickFiler/Controllers/QfcHomeController.Metrics.cs:197:                await NonBlockingProducer(line, ct);
QuickFiler/Controllers/QfcHomeController.Metrics.cs:201:        private async Task NonBlockingProducer(string line, CancellationToken ct)
QuickFiler/Controllers/QfcHomeController.Metrics.cs:226:            if (Interlocked.CompareExchange(ref _metricsConsumers, 0, 2) == 2)
QuickFiler/Controllers/QfcHomeController.Metrics.cs:228:                Interlocked.Decrement(ref _metricsConsumers);
QuickFiler/Controllers/QfcHomeController.Metrics.cs:230:                timer.Elapsed += TimedConsumerAsync;
QuickFiler/Controllers/QfcHomeController.cs:356:        private int _metricsConsumers = 0;
QuickFiler/Controllers/QfcHomeController.cs:357:        private static object _lockObject = new object();
QuickFiler/Controllers/QfcHomeController.cs:358:        private static string _fileName;
QuickFiler/Controllers/QfcHomeController.cs:362:        private async void TimedConsumerAsync(object source, ElapsedEventArgs e)
QuickFiler/Controllers/QfcHomeController.cs:366:            Interlocked.Decrement(ref _metricsConsumers);
```

Thirteen hits across the two QFC files. AC-3 asserts this same alternation returns no match after
the fix, proving no part of the flush was left on a timer, a background consumer, or residual
controller state. [P5-T14] records the post-fix count against this pre-fix count of 13.

Note that the `Interlocked.CompareExchange(ref _metricsConsumers, 0, 2) == 2` guard at line 226
can never be true: `_metricsConsumers` is initialized to `0` at `QfcHomeController.cs:356` and is
only ever decremented, so it never holds the comparand `2`. The `System.Timers.Timer` created at
line 229 is consequently unreachable, and even if reached it is never started. That is root cause
RC-1.
