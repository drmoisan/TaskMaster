# P5-T1 — Final QC step 1: Formatting (remediation cycle 1, issue #614)

Timestamp: 2026-08-26T22-18

Command (1 of 2): `dotnet tool run csharpier format .`

Command (2 of 2): `dotnet tool run csharpier check .`

EXIT_CODE: 0 (the `check` verification; the `format` invocation also exited 0)

## Output Summary

Two loop attempts were required, because the first `format` invocation rewrote files, which under
the plan's restart rule mandates a restart from P5-T1.

| Attempt | `format` output | Files rewritten (SHA-256 compared before/after) | `check` |
| --- | --- | --- | --- |
| 1 | `Formatted 1530 files in 1240ms.` | 2 — `QuickFiler/Controllers/EfcSelectionGuard.cs`, `QuickFiler.Test/Controllers/EfcSelectionGuardTests.cs` | not run; restarted |
| 2 | `Formatted 1530 files in 1162ms.` | **0** — all three hashes byte-identical to attempt 1's output | `Checked 1530 files in 3506ms.` — **exit 0** |

`Formatted N files` is CSharpier's PROCESSED count, not a rewrite count, so rewrites were determined
by hashing the three touched files before and after each invocation rather than by reading that
line. Attempt 2 was verified idempotent: the post-attempt-2 hashes are identical to the
post-attempt-1 hashes.

`QuickFiler/Controllers/EfcFormController.cs` was NOT rewritten by the formatter — its hash is
unchanged across both attempts — because the `IsValidSelection` property line was authored in the
already-wrapped shape CSharpier produces.

## No out-of-scope file was rewritten

`git status --porcelain` after formatting lists exactly the same modified set as the P4-T2 scope
lock:

```
 M QuickFiler.Test/Controllers/EfcSelectionGuardTests.cs
 M QuickFiler/Controllers/EfcFormController.cs
 M QuickFiler/Controllers/EfcSelectionGuard.cs
 M docs/features/.../remediation-plan.2026-08-26T21-00.md
```

plus untracked evidence artifacts under `<FEATURE>/evidence/`. The P0-T6 baseline exited 0, so a
format-clean baseline plus an unchanged modified-file set proves the formatter touched nothing
outside this cycle's scope.

## Post-format line counts

| File | Post-format | Ceiling | Verdict |
| --- | ---: | ---: | --- |
| `QuickFiler/Controllers/EfcFormController.cs` | 1079 | 1084 | pass (5 lines spare) |
| `QuickFiler/Controllers/EfcSelectionGuard.cs` | 147 | 500 | pass |
| `QuickFiler.Test/Controllers/EfcSelectionGuardTests.cs` | 316 | 500 | pass |
| `QuickFiler/Controllers/BreadcrumbBridgeRouter.cs` | 596 | unmodified | unchanged |
| `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue439Tests.cs` | 694 | unmodified | unchanged |
