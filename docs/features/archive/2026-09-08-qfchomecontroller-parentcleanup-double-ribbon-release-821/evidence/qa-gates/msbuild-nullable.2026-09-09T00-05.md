# Phase 6 — Type-check (nullable)

Timestamp: 2026-09-09T13-48
Task: [P6-T4]

Command: `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`
EXIT_CODE: 0

`/p:Nullable=enable` was **not** added, per the plan and per `CLAUDE.md`. This is
character-for-character the repository's approved nullable-gate command.

Summary lines, verbatim from the captured log:

```text
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

## Acceptance check

| Condition | Required | Observed | Met |
|---|---|---|---|
| Literal `Build succeeded.` present | yes | **yes** | yes |
| Error integer | `0` | **0** | yes |
| Warning integer | — | 0 | matches the `[P0-T9]` baseline of 0 |

## Proof the Rebuild was not vacuous

Lines containing `CoreCompile:` in the captured log: **62**. A warm `/t:Build` would have returned
exit 0 with `CoreCompile` skipped on every project, and the gate could not have failed.

## Why this gate is the load-bearing proof for Sites B and B'

`UtilitiesCS/Threading/ProgressViewer.cs` and `UtilitiesCS/Threading/ProgressPane.cs` both carry
`#nullable enable` on line 1, and both cancellation-source fields are declared as explicitly nullable
references — `private CancellationTokenSource? _cancelSource;` and
`private CancellationTokenSource? _tokenSource;`. Before this change each dereference was written
`_cancelSource!.Cancel();` and `_tokenSource!.Cancel();`, where the null-forgiving operator suppressed
the compiler's null-state check rather than guarding the call.

This change removed both `!` operators — `[P5-T1]` records the `!.Cancel()` search at 0 matches across
all four production files — and introduced no `#pragma warning disable`, also recorded at 0 matches.
Writing the dereference without the `!` and without a null check the compiler can see would raise
CS8602, "Dereference of a possibly null reference", which `/p:TreatWarningsAsErrors=true` promotes to
a build error. The guard supplied instead is the `?? throw new InvalidOperationException(...)` form,
which yields a provably non-null local before the dereference.

Confirming searches over the captured log:

| Literal searched | Matches |
|---|---|
| `CS8602` | **0** |
| `CS86` (any nullable-flow diagnostic) | **0** |

Zero CS86xx diagnostics of any kind. Had the guard been omitted or ineffective, CS8602 would appear
here and the build would have failed, so this result is a positive observation about the guard rather
than an absence of evidence.

Output Summary: `Build succeeded.` with **0 errors** and 0 warnings, exit code 0, verified
non-vacuous by 62 `CoreCompile:` occurrences. No CS8602 and no CS86xx diagnostic of any kind appears
in the log, proving the two `#nullable enable` progress files no longer need the `!` operator because
a real guard now satisfies the flow analysis.
