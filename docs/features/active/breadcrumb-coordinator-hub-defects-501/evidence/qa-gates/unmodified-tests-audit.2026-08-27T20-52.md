# QA Gate — The Three Must-Pass Tests Were Never Edited (P6-T5; AC-20, AC-21, AC-22)

Timestamp: 2026-08-27T20-52

## The gating command

Command:

```
git diff --numstat -- QuickFiler.Test/Viewers/BreadcrumbDropDownOpenCoordinatorTests.cs QuickFiler.Test/Viewers/BreadcrumbDropDownOpenCoordinatorTests.Part2.cs QuickFiler.Test/Viewers/BreadcrumbMessengerHubTests.cs
```

EXIT_CODE: 0

Output, verbatim:

```
74	0	QuickFiler.Test/Viewers/BreadcrumbDropDownOpenCoordinatorTests.Part2.cs
78	0	QuickFiler.Test/Viewers/BreadcrumbMessengerHubTests.cs
```

`git diff --numstat` emits no row for an unchanged path, so the absence of a
`BreadcrumbDropDownOpenCoordinatorTests.cs` row means that file has 0 added and 0 deleted lines.

## Results against the required bounds

| File | Added | Deleted | Required bound | Verdict | AC |
| --- | ---: | ---: | --- | --- | --- |
| `QuickFiler.Test/Viewers/BreadcrumbDropDownOpenCoordinatorTests.cs` | 0 | 0 | 0 added AND 0 deleted | SATISFIED | AC-20 |
| `QuickFiler.Test/Viewers/BreadcrumbDropDownOpenCoordinatorTests.Part2.cs` | 74 | **0** | 0 deleted | SATISFIED | AC-21 |
| `QuickFiler.Test/Viewers/BreadcrumbMessengerHubTests.cs` | 78 | **0** | 0 deleted | SATISFIED | AC-22 |

A deleted-line count of 0 on the latter two files proves every change to them was purely ADDITIVE. No
existing line was edited, reordered, or removed, so the three named must-pass tests are byte-identical
to their `BASELINE_SHA` text:

| Named test | Home file | Status |
| --- | --- | --- |
| `PendingToggleClose_HostOwnershipSuppressesFallbackAndRepeatedClose` (AC-20) | `BreadcrumbDropDownOpenCoordinatorTests.cs` | file entirely unchanged |
| `SelectorStateTransitions_RequestOpenThenCloseOnlyWhenRequired` (AC-21) | `BreadcrumbDropDownOpenCoordinatorTests.Part2.cs` | file additively changed only |
| `Attach_ReplayFailureRollsBackSubscriptionAndAllowsRetry` (AC-22) | `BreadcrumbMessengerHubTests.cs` | file additively changed only |

## Corroborating behavioural evidence

Byte-identity alone would not prove the tests still PASS; the following runs do.

| Named test | Pre-change green | Post-change green |
| --- | --- | --- |
| `PendingToggleClose_HostOwnershipSuppressesFallbackAndRepeatedClose` | P0-T17 (3/3 passed) | P1-T7 (48/48 passed) |
| `SelectorStateTransitions_RequestOpenThenCloseOnlyWhenRequired` | P0-T17 (3/3 passed) | P1-T7 (48/48 passed) |
| `Attach_ReplayFailureRollsBackSubscriptionAndAllowsRetry` | P0-T17 (3/3 passed) | P5-T9 (41/41 passed; named as passed in that artifact) |

This is the constraint that ruled out the naive #462 fix. Research section 6.1 option A — clearing the
close flag on the successful-close path — would have made the first two of these tests fail by letting a
second `CloseCore` reach `_host.Close`. Option D passes all three with no test edit, which is what these
two independent lines of evidence together establish.
