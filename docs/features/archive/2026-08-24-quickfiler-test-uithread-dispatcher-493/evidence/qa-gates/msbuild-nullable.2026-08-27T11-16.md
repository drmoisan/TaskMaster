# Type-Check Gate — Final Pass (P3-T4)

Timestamp: 2026-08-27T11-16
Task: [P3-T4]
Command: `& $MSBUILD TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`
EXIT_CODE: 0
Output Summary: `Build succeeded.` with 5 warnings and 0 errors — identical counts to the `P0-T9`
Phase 0 baseline. Zero lines match `warning CS` or `error CS` anywhere in the log, so no compiler or
nullable-flow diagnostic was introduced and none could be promoted to an error by
`/p:TreatWarningsAsErrors=true`.

## MSBuild summary counts

| Metric | Value | Phase 0 baseline (`P0-T9`) |
| --- | --- | --- |
| Build result | `Build succeeded.` | `Build succeeded.` |
| Total warnings | 5 | 5 |
| Total errors | 0 | 0 |
| Lines matching `warning CS` or `error CS` | 0 | 0 |

Log path: `TestResults/plan-logs/p3-t4/msbuild-nullable.log`

That log path is named here for consumption by `P4-T2`.

## Why this gate is not vacuous

- `/t:Rebuild` was used, not `/t:Build`. MSBuild's up-to-date check does not invalidate on a
  command-line `/p:` change, so a warm `/t:Build` would exit 0 with `CoreCompile` skipped on every
  project and the gate could not fail.
- `/p:Nullable=enable` was **not** added. The command is character-for-character the one in
  `.github/workflows/ci.yml`. The property is a solution-wide opt-in that would conscript every file
  that never adopted the `#nullable enable` pragma, and CI omits it deliberately.

## Bearing on the fields this plan declares

`/p:TreatWarningsAsErrors=true` promotes `CS0649` (field never assigned) and `CS0169` (field never
used) to build errors. Every field declared by
`QfcItemController.UiThreadDispatcherFixture.cs` carries an initializer at the declaration or is
definitely assigned in the single constructor named in § Fixture Contract, which is why this gate —
and not `P2-T3`, which omits `/p:TreatWarningsAsErrors=true` — is the one that proves it.
