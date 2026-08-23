# P5 primary rollback and late-cleanup audit

Timestamp: 2026-07-22T07:32:16.5828023Z

## Scope and provenance

The correction remained within the delegated P5-T49 tuple: two production sources and one test source. No fourth source file was edited. The test file reuses the existing creator-thread queue in `BreadcrumbSelectorToggleUiBoundaryTests` without changing that file.

| File | P5-T50 retained baseline | Final state |
| --- | --- | --- |
| `QuickFiler/Viewers/BreadcrumbDropDownHost.cs` | 462 lines; `D67A0E8E407D5FD7BABAB90EAA8643F0CD2748EF075B6AE67379B17613680B5C` | 470 lines; `7B0A2981918DB95A83EEB077AE860EA62B28C8713CDD537EED5C0BECD9BD6F28` |
| `QuickFiler/Viewers/BreadcrumbDropDownOpenLifetime.cs` | 465 lines; `E4D751CF08DAEE45005541DAC0B2325CB559ACFABAED661B4802FE0B65E27C02` | 437 lines; `E53DE9BE76CB7AC3F69B43C12088A7B4B6DA6F3F2455DCF7C6C10F5A010C53F1` |
| `QuickFiler.Test/Viewers/BreadcrumbDropDownCoverageThresholdTests.cs` | 448 lines; `20E000AAF771D89133F48529E2395E56BDA2A5EFABB1DA0CC7DD42B76A21CE74` | 479 lines; `25EE741353DB8CFA625F5783ED7CA17697768FBAB826865F53D72F0DF4BBBD77` |

All three files are readable, documented, and no more than 480 physical lines. Public `IBreadcrumbDropDownHost` signatures and constructors are unchanged. Host anchor-focus ownership remains in `BreadcrumbDropDownHost`; it was not moved into the lifetime helper.

## Failure-first and correction evidence

The retained diagnostic initially reported 5/7 passing: rollback prevented anchor focus, and reset/readiness completed before late surface disposal. The first correction exposed a class-level test deadlock. Live task-state inspection showed the production open task had already cleared `_openTask`; the blocked continuation had captured a WinForms synchronization context installed by prior cases on the adapter STA thread, which was synchronously waiting without a message pump. The harness now uses the existing creator-thread capturing context and explicitly drains open and disposal work on its creator thread.

The strengthened failure-first class then completed with 5/7 passing and exactly two named failures. `OpenAsync_RollbackCallbackFailsOnce_OuterPipelineCompletesRecovery` proved that a later focus failure was discarded, no-space placement repeated rollback and replaced the primary error, and retained popup state was inconsistent. `OpenAsync_ReadyHandlerResetsLifecycle_RejectsInstalledSurface` proved two messenger-disposal invocations. No unrelated test failed.

After correction, `p5-primary-rollback-pass-after.2026-07-22T07-31.md` records 7 discovered, 7 passed, 0 failed, and 0 skipped. CSharpier made no changes; analyzer and nullable solution builds completed successfully.

## Call-order and ownership matrix

| Scenario | Required and verified order |
| --- | --- |
| Factory/open initialization failure with cancel and focus failures | Retain the initiating exception as `LastInitializationException`; do not invoke native close before show; attempt cancel once; attempt anchor focus once even when cancel throws; report both distinct secondaries once; clear the shared open task; complete the caller `false`; permit a fresh retry. |
| No-space placement failure | Throw and retain the no-space exception as primary; route recovery through `HandleOpenFailureAsync` once; keep native close at zero; attempt cancel and focus once; report the placement primary once at the UI boundary and the rollback secondary once; do not overwrite the primary or repeat rollback. |
| Failure after show during focus | Mark the host closed; invoke native close once; attempt cancel once; return anchor focus once; retain/report the focus failure; reuse the valid surface for a successful retry without a second native close. |
| Ready-handler reset | Transfer the installed tuple to Host ownership atomically before publishing readiness; reset takes that ownership and disposes it once; the retention result records that ownership was transferred so the caller does not dispose the same tuple again; complete the shared open task `false` after cleanup. |
| Reset while readiness is pending | Invalidate the generation; complete the cancellation signal; reject the late tuple; dispose messenger and control once; finish cleanup before completing every shared open waiter `false`; expose authoritative closed/null installed state; accept a new surface on retry. |

## Primary and secondary observation proof

`BreadcrumbDropDownHost.CompleteAll` attempts every operation. It retains the first failure for its caller and reports each later failure at the UI boundary, so a failing cancel cannot suppress focus and a failing focus is not discarded. `BreadcrumbDropDownOpenLifetime.HandleOpenFailureAsync` assigns the initiating error before rollback and reports the retained rollback failure without replacing that primary. Placement validation now throws the no-space primary into this single recovery path instead of invoking rollback directly. The observer-only catch remains at the fire-and-forget scheduling boundary with its invariant documented: dispatcher scheduling and action failures are reported before the associated task faults.

The test did not preserve the former contradictory `CancelCount == 2` or secondary-as-primary behavior. It now requires cancel, focus, native close where applicable, disposal, and shared completion at most once in the required order. Disposal trackers count invocations rather than only first effective disposal, so duplicate cleanup cannot pass by relying on idempotent `Dispose` implementations.

## Scope and downstream gates

No test was deleted or weakened. The rollback, placement, disposal, creator-thread dispatch, closed-state, and retry assertions were strengthened. `git diff --check` reports no whitespace error in the three batch files. P6-T9 through P6-T16 remain unchecked and unchanged for the broader pending-open cancellation work. P9-T4 and P9-T6 remain unchecked and preserve the mandatory full-repository coverage run and authoritative coverage comparison.
