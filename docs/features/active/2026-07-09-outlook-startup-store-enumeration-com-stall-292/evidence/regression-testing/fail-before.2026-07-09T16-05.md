# Fail-Before Evidence (issue #292, remediation cycle 1)

- Timestamp: 2026-07-09T16-05
- Task: [P1-T1]

## Authoritative CI red evidence

- CI workflow: `Format, build, analyze, and test` (CI), run 29046195330.
- Job URL: https://github.com/drmoisan/TaskMaster/actions/runs/29046195330/job/86215357832
- PR #294 head under remediation: `9ae5c0e3952f9ff29febd825b8def21a1981caff`.
- CI result: `Total tests: 5141 / Failed: 10`, all 10 in `UtilitiesCS.Test`.

## The 10 failing tests (from CI and reproduced locally)

`UtilitiesCS.Test/Threading/CurrentStoreContextTests.cs` (9):
1. `Begin_SetsCurrent_ReadableInsideScope`
2. `Dispose_RestoresPreviousValue`
3. `NestedScopes_RestoreInnerThenOuter`
4. `SequentialScopes_EachRestoreToNull`
5. `Begin_NormalizedInnerScope_RestoresRealOuterValue`
6. `Begin_NormalizesUnavailableIdentity_ToNoContext (null)`
7. `Begin_NormalizesUnavailableIdentity_ToNoContext ("")`
8. `Begin_NormalizesUnavailableIdentity_ToNoContext ("   ")`
9. `Begin_NormalizesUnavailableIdentity_ToNoContext ("<unavailable>")`

`UtilitiesCS.Test/Threading/ThreadMonitorTests.cs` (1):
10. `EvaluatePoll_NoContext_CarriesNullIdentity`

Representative assertion: `Expected CurrentStoreContext.Current to be <null> ..., but found "<Stores-enumeration>".`

## Local reproduction outcome

The failure DID reproduce locally, on the full CI-equivalent 7-assembly set:

- P0-T6 CI-equivalent run (`/EnableCodeCoverage /InIsolation /Logger:trx /TestCaseFilter:"TestCategory!=LiveOutlook"`): `Total 5141 / Passed 5131 / Failed 10` — the exact 10 tests above.
- P0-T6 instrumented coverage run (`dotnet-coverage collect` wrapping the same 7-assembly set with the same filter): `Total 5141 / Passed 5130 / Failed 11` — the same 9 `CurrentStoreContextTests` + `ThreadMonitorTests` plus one additional store test observing the polluted global on that schedule.

The count fluctuated (10 vs 11) between the two passes because the race is schedule-dependent: the `[DoNotParallelize]` reader classes run concurrently with parallel-bucket store-test writers that hold `_current == "<Stores-enumeration>"`. This is a real, recurring race, not a one-off flake.

- SearchScope: `<FEATURE>/evidence/remediation-baseline/` and the P0-T6 scratchpad trx.
- SearchPatterns: trx `outcome="Failed"` entries; console `Failed:` summary.
- SearchResult: 10 failing tests reproduced (CI-equivalent run) and 11 (instrumented run); a fail-before exception dossier is NOT required because a genuine local failing run exists.
