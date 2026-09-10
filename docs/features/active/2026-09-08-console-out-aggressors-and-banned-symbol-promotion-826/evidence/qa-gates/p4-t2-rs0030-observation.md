# RS0030 positive observation at the five files under test (issue #826, [P4-T2])

Timestamp: 2026-09-09T19-22

CHANNEL: SARIF

The channel is the one certified by [P4-T1]; this task reads the same run's output.

## Exact commands

Line numbers were re-derived at observation time, so that no line number is carried from the plan
document. Run as one `pwsh -NoProfile -Command` block carrying the plan's C2 preamble branch guard:

```
Select-String -LiteralPath "QuickFiler.Test/Controllers/QfcQueueCoverageExpansionTests.cs" -CaseSensitive -SimpleMatch "CancelAfter("
Select-String -LiteralPath "QuickFiler.Test/Viewers/BreadcrumbCoordinatorLifecycleTests.cs" -CaseSensitive -SimpleMatch "WaitOne(0)"
Select-String -LiteralPath "UtilitiesCS/Threading/TimeOutTask.cs" -CaseSensitive -Pattern "new CancellationTokenSource\([^)]"
Select-String -LiteralPath "QuickFiler/Controllers/QfcQueue.cs" -CaseSensitive -Pattern "new CancellationTokenSource\([^)]"
Select-String -LiteralPath "UtilitiesCS/OutlookObjects/Conversation/ConversationHelper.cs" -CaseSensitive -Pattern "new CancellationTokenSource\([^)]"
```

The observed diagnostics were extracted from the three SARIF documents written by the [P4-T1] build
invocations, filtering `runs[0].results` on `ruleId -eq "RS0030"` and reading
`locations[0].resultFile.uri` and `locations[0].resultFile.region.startLine` (SARIF version 1.0.0).

EXIT_CODE: 0

## Per-file reconciliation

| File under test | Re-derived lines | Observed RS0030 lines | Re-derived minus observed | Observed minus re-derived |
|---|---|---|---|---|
| `QuickFiler.Test/Controllers/QfcQueueCoverageExpansionTests.cs` | 169 | 169 | empty | empty |
| `QuickFiler.Test/Viewers/BreadcrumbCoordinatorLifecycleTests.cs` | 57 | 57 | empty | empty |
| `UtilitiesCS/Threading/TimeOutTask.cs` | 53, 119, 200, 274, 358, 436, 506, 588, 670, 752 | 53, 119, 200, 274, 358, 436, 506, 588, 670, 752 | empty | empty |
| `QuickFiler/Controllers/QfcQueue.cs` | 50, 101 | 50, 101 | empty | empty |
| `UtilitiesCS/OutlookObjects/Conversation/ConversationHelper.cs` | 295 | 295 | empty | empty |

For each of the five files the set difference "re-derived minus observed" is **empty**, so every
re-derived site produced a diagnostic. This is AC10's pass condition, which is per site rather than per
file: a run reporting one of the ten `TimeOutTask.cs` sites would satisfy a per-file gate and still fail
AC10.

The "observed minus re-derived" column is recorded rather than asserted, per the plan. It is empty for
all five files in this run.

## Control counts, re-recorded from the same run

Each is the number of RS0030 diagnostics whose own result location is the control file, not the number
of textual occurrences of the file name:

| Control site | RS0030-scoped count | Required |
|---|---|---|
| `UtilitiesCS/Threading/ApplicationIdleTimer.cs` | 9 | at least 1 |
| `QuickFiler/Controllers/EfcHomeControllerDependencies.cs` | 1 | at least 1 |
| `QuickFiler.Test/Helper Classes/MailItemInfoTests.cs` | 1 | at least 1 |

All three fired in the same run that produced the observations above, so the observation is not void.

## Observed totals

Recorded, not asserted:

| SARIF document | RS0030 diagnostics |
|---|---|
| `utilitiescs.sarif` | 37 |
| `quickfiler.sarif` | 5 |
| `quickfilertest.sarif` | 3 |
| **total across the three projects** | **45** |

Of those 45, **15** are at the five files under test, which happens to equal the plan's predicted figure
of 15 new diagnostics. The equality is recorded as an observation and is not the pass condition; the
pass condition is per-site observation with a live control, which holds independently.

The remaining 30 diagnostics are at already-banned symbols (`DateTime.Now`, `DateTime.UtcNow`,
`Random.Shared`, `Thread.Sleep`, `Task.Delay`) elsewhere in the three projects. They are pre-existing,
they were reported before this change as well, and they are unaffected by it.

## Full extracted list of RS0030 result locations

Repository-relative paths only. The raw SARIF documents stay under the gitignored `coverage/826-raw/`
directory and are not committed.

`utilitiescs.sarif` (37):

```
UtilitiesCS/Interfaces/IGenericTimer.cs:19
UtilitiesCS/Extensions/AsyncSerialization.cs:43
UtilitiesCS/Extensions/AsyncSerialization.cs:71
UtilitiesCS/Extensions/AsyncSerialization.cs:102
UtilitiesCS/Extensions/AsyncSerialization.cs:135
UtilitiesCS/Extensions/AsyncSerialization.cs:185
UtilitiesCS/Threading/TimeOutTask.cs:53
UtilitiesCS/Threading/TimeOutTask.cs:119
UtilitiesCS/Threading/TimeOutTask.cs:200
UtilitiesCS/Threading/TimeOutTask.cs:274
UtilitiesCS/ReusableTypeClasses/TimedActions/TimedQueueOfActions.cs:191
UtilitiesCS/Threading/TimeOutTask.cs:358
UtilitiesCS/Threading/TimeOutTask.cs:436
UtilitiesCS/Threading/TimeOutTask.cs:506
UtilitiesCS/Threading/TimeOutTask.cs:588
UtilitiesCS/Threading/TimeOutTask.cs:670
UtilitiesCS/Threading/TimeOutTask.cs:752
UtilitiesCS/ReusableTypeClasses/AsyncLazy/AsyncLazy.cs:89
UtilitiesCS/Threading/ThreadMonitor.cs:151
UtilitiesCS/Threading/ApplicationIdleTimer.cs:140
UtilitiesCS/Threading/ApplicationIdleTimer.cs:141
UtilitiesCS/Threading/ThreadMonitor.cs:211
UtilitiesCS/Threading/ApplicationIdleTimer.cs:209
UtilitiesCS/Threading/ApplicationIdleTimer.cs:236
UtilitiesCS/Threading/ApplicationIdleTimer.cs:240
UtilitiesCS/Threading/ApplicationIdleTimer.cs:292
UtilitiesCS/Threading/ApplicationIdleTimer.cs:309
UtilitiesCS/Threading/ApplicationIdleTimer.cs:330
UtilitiesCS/Threading/ApplicationIdleTimer.cs:60
UtilitiesCS/ReusableTypeClasses/AsyncLazy/AsyncLazy.cs:157
UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.cs:1207
UtilitiesCS/ReusableTypeClasses/TimedActions/TimedDiskWriter.cs:185
UtilitiesCS/EmailIntelligence/ClassifierGroups/ClassifierGroupUtilities.cs:468
UtilitiesCS/OutlookObjects/Attachment/AttachmentHelper.cs:329
UtilitiesCS/OutlookObjects/Attachment/AttachmentSerializable.cs:172
UtilitiesCS/OutlookObjects/Conversation/ConversationHelper.cs:295
UtilitiesCS/EmailIntelligence/EmailParsingSorting/EmailDataMiner.cs:141
```

`quickfiler.sarif` (5):

```
QuickFiler/Controllers/QfcQueue.cs:50
QuickFiler/Controllers/EfcHomeControllerDependencies.cs:77
QuickFiler/Controllers/QfcQueue.cs:101
QuickFiler/Controllers/QfcFormController.EventHandlers.cs:348
QuickFiler/Controllers/QfcItemController.EventWiring.cs:137
```

`quickfilertest.sarif` (3):

```
QuickFiler.Test/Helper Classes/MailItemInfoTests.cs:25
QuickFiler.Test/Viewers/BreadcrumbCoordinatorLifecycleTests.cs:57
QuickFiler.Test/Controllers/QfcQueueCoverageExpansionTests.cs:169
```

Output Summary: all 15 re-derived sites across the five files under test produced an RS0030 diagnostic,
with all three controls firing in the same run. The eight DocIDs added by [P3-T1] therefore resolve to
real symbols; none is a silently unresolvable string. Observed total across the three projects is 45
RS0030 diagnostics, of which 15 are at the enumerated sites. No site is missing, so AC10's positive
observation requirement is met and no discrepancy needs reporting.

Honesty note carried forward from the spec: at `suggestion` severity these diagnostics are info-level and
produce **zero** build enforcement. The addition pre-stages the list so that a future promotion is a
one-line severity change; it does not make any timing-hack constraint build-enforced today.
