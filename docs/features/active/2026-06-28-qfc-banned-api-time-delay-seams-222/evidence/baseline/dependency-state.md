# Baseline — Dependency State (P0-T4)

Timestamp: 2026-06-28T19-04

Grep results for `Microsoft.Bcl.TimeProvider` and `Microsoft.Extensions.TimeProvider.Testing`:

| Config file | Bcl.TimeProvider | TimeProvider.Testing |
|-------------|------------------|----------------------|
| QuickFiler/QuickFiler.csproj | ABSENT | ABSENT |
| QuickFiler/packages.config | ABSENT | ABSENT |
| QuickFiler.Test/QuickFiler.Test.csproj | ABSENT | ABSENT |
| QuickFiler.Test/packages.config | ABSENT | ABSENT |

On-disk DLL state (post P0-T1 restore):

| DLL | State |
|-----|-------|
| packages\Microsoft.Bcl.TimeProvider.10.0.7\lib\net462\Microsoft.Bcl.TimeProvider.dll | PRESENT (restored transitively via UtilitiesCS declaration) |
| packages\Microsoft.Extensions.TimeProvider.Testing.9.0.0\lib\net462\Microsoft.Extensions.TimeProvider.Testing.dll | ABSENT — RESTORE-REQUIRED |

Assessment:
- `Microsoft.Extensions.TimeProvider.Testing` is not yet declared by any project, so its pre-restore absence is EXPECTED and recorded as RESTORE-REQUIRED. It is wired in Phase 1 (P1-T3/P1-T4) and restored in P1-T5.
- This is NOT a DEPENDENCY-BLOCKED condition. DEPENDENCY-BLOCKED is reserved for confirmed feed-unavailability after the Phase 1 restore (P1-T5).
- QuickFiler needs its own explicit `<package>`/`<Reference>` for Bcl.TimeProvider (Phase 1 P1-T1/P1-T2); the transitive DLL presence does not flow a build-time reference to QuickFiler.
