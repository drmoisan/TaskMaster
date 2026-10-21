# Phase 5 — AC13 suppression scan

Timestamp: 2026-09-09T13-21
Task: [P5-T1]

Scope: the four production files in the Write Set.

- `QuickFiler/Controllers/QfcHomeController.cs`
- `QuickFiler/Controllers/EfcHomeController.cs`
- `UtilitiesCS/Threading/ProgressViewer.cs`
- `UtilitiesCS/Threading/ProgressPane.cs`

## Search 1 — the null-forgiving dereference

Command:

```text
pwsh -NoProfile -Command 'Select-String -SimpleMatch -Path <the four production files> -Pattern "!.Cancel()"'
```

EXIT_CODE: 0
Result: **0 matches** across all four files.

The two occurrences that existed at baseline — `UtilitiesCS/Threading/ProgressViewer.cs` line 75
(`_cancelSource!.Cancel();`) and `UtilitiesCS/Threading/ProgressPane.cs` line 57
(`_tokenSource!.Cancel();`) — are both gone. Each was replaced by a guarded call inside
`RequestCancel`, where the `?? throw new InvalidOperationException(...)` form gives the compiler a
provably non-null local. The suppression was removed by supplying a real guard, not by relocating it.

## Search 2 — the suppression directive

Command:

```text
pwsh -NoProfile -Command 'Select-String -SimpleMatch -Path <the four production files> -Pattern "#pragma warning disable"'
```

EXIT_CODE: 0
Result: **0 matches** across all four files.

No `#pragma warning disable` was introduced anywhere. The CS8602 that removing the `!` would
otherwise raise is satisfied by the guard rather than suppressed; `[P6-T4]`, the nullable gate run
with `/p:TreatWarningsAsErrors=true`, is the check that proves this, because CS8602 would be
promoted to a build error there if the guard were absent or ineffective.

Output Summary: both searches return **0 matches** across all four production files. The
null-forgiving operator is gone and no suppression directive replaced it. The second half of AC13 —
that no analyzer or compiler severity was lowered — is recorded separately by `[P5-T2]`.
