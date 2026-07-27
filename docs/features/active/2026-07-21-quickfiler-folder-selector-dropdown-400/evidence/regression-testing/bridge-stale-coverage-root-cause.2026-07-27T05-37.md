# P8-T76 aggregate root-cause analysis

`ROOT_CAUSE: UNCLASSIFIED`

## Evidence analyzed

- P8-T73 retained aggregate evidence: `bridge-stale-coverage-all-eight-blame.2026-07-27T05-18.md` and `bridge-stale-coverage-orphan-cleanup.2026-07-27T05-18.md`.
- P8-T75 bounded diagnostic: `bridge-stale-coverage-aggregate-blame.2026-07-27T05-34.md`, its TRX, detailed console output, verbose VSTest diagnostics, recorder events, and process-tree records.
- Isolated stale-lease evidence: `member-coverage-bridge-stale-hang-diagnosis.2026-07-27T05-17.trx` and its diagnostic logs.
- Prior P8-T67 direct determinism TRXs: `member-coverage-all-eight-determinism-run-1.2026-07-27T04-43.trx` and `member-coverage-all-eight-determinism-run-2.2026-07-27T04-44.trx`.

## Findings

1. P8-T73 exceeded the parent shell boundary without output or its requested TRX and left an orphaned VSTest/testhost tree that was later verified and targeted for cleanup. It does not identify a running test, class, or source path.
2. P8-T75 did not reproduce that hang. It completed in 59.1 seconds with no residual VSTest descendants and without reaching the 180-second debugger boundary.
3. P8-T75 reported one failure: `QuickFiler.Test.Viewers.BreadcrumbSelectorCoordinatorTests.TransitionPublicationsAndEvents_RunAfterRouterLockIsReleased`, at `QuickFiler.Test/Viewers/BreadcrumbSelectorCoordinatorTests.cs:172`, where two expected messenger posts were absent. That same test passed in both P8-T67 TRXs. One non-repeated failure does not prove a deterministic root cause or corrective behavior.
4. The isolated `PostRenderAndSelectorAsync_StaleLeaseReturnsCompletedWithoutPublishing` test passed under VSTest blame diagnostics. It is insufficient evidence to identify that test as causal.
5. P8-T75 resolved the same CommonExtensions VSTest path used by P8-T67. The `18.8.0` console product label and `18.0.11829.241` file version describe the same executable; no engine variance was established.

## Required disposition

No reproducible implicated assembly/test or harness/process-lifetime source, exact source file, and deterministic corrective behavior were established. P8-T75, P8-T76, and P8-T77 remain unchecked. Do not modify source, add a file-specific correction plan, retry the aggregate command, or begin Phase 9 without a revised plan supported by additional deterministic diagnostic evidence.
