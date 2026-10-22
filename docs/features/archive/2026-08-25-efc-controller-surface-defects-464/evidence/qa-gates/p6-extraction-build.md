# [P6-T3] Intermediate build after the RC3 defect-preserving extraction

Timestamp: 2026-08-28T01-04
Task: [P6-T3]
Command: `& "<resolved MSBuild.exe>" TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU" /nologo /v:m` under `pwsh -NoProfile` from the worktree root, output redirected to a log file outside the repository
EXIT_CODE: 0

## This is an intermediate build, not a gate

Per decision D3, this task only needs to confirm that the five extracted `internal async Task` members
compile. It uses `/t:Build`, so it is **not** cited as an analyzer gate or a nullable gate and proves
nothing about analyzer diagnostics. The analyzer and nullable gates use `/t:Rebuild` and are run in
Phase 10.

## Result

Zero lines matching `: error` in the build log. The solution builds with the extraction in place.

## What was extracted (defect-preserving; no behaviour change in this task)

| `async void` handler | Wrapper line | Extracted `internal async Task` member | Declaration line |
|---|---|---|---|
| `ButtonCancel_Click` | `:460-461` | `ButtonCancelClickAsync` | `:463` |
| `ButtonOK_Click` | `:479` | `ButtonOkClickAsync` | `:481` |
| `ButtonRefresh_Click` | `:497-498` | `ButtonRefreshClickAsync` | `:500` |
| `ButtonCreate_Click` | `:516-517` | `ButtonCreateClickAsync` | `:519` |
| `ButtonDelete_Click` | `:579-580` | `ButtonDeleteClickAsync` | `:582` |

Each extracted member carries its handler's original body verbatim, **including its `throw;`**, with the
`logger.Error` call in the `catch` replaced by a `BoundaryErrorSink` call carrying the same message and
exception. Count of `throw;` in the file after this task: **5**, unchanged from the Phase 0 baseline.
Count of `BoundaryErrorSink(ex.Message, ex);` calls: **5**.

Each `async void` handler's body is now a single `await` of its extracted member. No extracted member
appears on any interface: a search of `QuickFiler/Interfaces/` for the five names returns 0 matching
lines.

Delivered line count of `QuickFiler/Controllers/EfcFormController.cs`: **1134**, within the derived size
gate of 1193.

Output Summary: PASS. EXIT_CODE 0, zero compile errors. All five `async void` handlers are reduced to
one-line wrappers over new `internal async Task` members that preserve the rethrow, so the boundary
defect is intact and observable by the fail-before test in [P6-T5].
