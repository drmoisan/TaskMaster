# P4-T2 — Scope Lock (remediation cycle 1, issue #614)

Timestamp: 2026-08-26T22-14

Command (1 of 2): `git status --porcelain`

Command (2 of 2): `git diff --name-only HEAD`

EXIT_CODE: 0 (both)

Output Summary: the modified-file set is exactly the three permitted source/test files plus
`<FEATURE>/**` docs and evidence. `BreadcrumbBridgeRouter.cs` and
`BreadcrumbBridgeRouterIssue439Tests.cs` are both ABSENT from the diff. No out-of-scope path
appears.

## `git status --porcelain`

```
 M QuickFiler.Test/Controllers/EfcSelectionGuardTests.cs
 M QuickFiler/Controllers/EfcFormController.cs
 M QuickFiler/Controllers/EfcSelectionGuard.cs
 M docs/features/.../remediation-plan.2026-08-26T21-00.md
?? docs/features/.../evidence/regression-testing/cr1-expect-fail.2026-08-26T21-46.md
?? docs/features/.../evidence/regression-testing/cr1-pass-after.2026-08-26T21-50.md
?? docs/features/.../evidence/regression-testing/cr2-expect-fail.2026-08-26T21-56.md
?? docs/features/.../evidence/regression-testing/cr2-pass-after.2026-08-26T22-02.md
?? docs/features/.../evidence/regression-testing/p1-t4-seam-prep.2026-08-26T21-40.md
?? docs/features/.../evidence/regression-testing/p4-t1-integration.2026-08-26T22-10.md
?? docs/features/.../evidence/remediation-baseline/
```

(`docs/features/...` abbreviates
`docs/features/active/2026-08-26-efc-store-root-selection-leaks-full-outlook-path-into-filing-boundary-614`.)

## `git diff --name-only HEAD`

```
QuickFiler.Test/Controllers/EfcSelectionGuardTests.cs
QuickFiler/Controllers/EfcFormController.cs
QuickFiler/Controllers/EfcSelectionGuard.cs
docs/features/.../remediation-plan.2026-08-26T21-00.md
```

## Gate evaluation

| Required | Observed | Verdict |
| --- | --- | --- |
| `QuickFiler/Controllers/EfcSelectionGuard.cs` present | present | pass |
| `QuickFiler/Controllers/EfcFormController.cs` present | present | pass |
| `QuickFiler.Test/Controllers/EfcSelectionGuardTests.cs` present | present | pass |
| `QuickFiler/Controllers/BreadcrumbBridgeRouter.cs` ABSENT | absent | pass |
| `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue439Tests.cs` ABSENT | absent | pass |
| Every other path under `<FEATURE>/**` or `.claude/agent-memory/**` | all remaining paths are under `<FEATURE>/**` | pass |
| No other path | none | pass |

No project file was modified: no `.csproj`, `.props`, `.targets`, or `packages.config` appears in
either output. No new `.cs` file was created, so no `<Compile Include>` edit was needed.

## Diff magnitude and encoding

```
QuickFiler.Test/Controllers/EfcSelectionGuardTests.cs  | 276 ++++++++++--
QuickFiler/Controllers/EfcFormController.cs            |  11 +-
QuickFiler/Controllers/EfcSelectionGuard.cs            | 129 +++++-
docs/features/.../remediation-plan.2026-08-26T21-00.md |  44 +--
4 files changed, 407 insertions(+), 53 deletions(-)
```

`EfcFormController.cs` shows an 11-line diff, matching the two edit sites the plan authorises and no
incidental churn. Byte-order-mark state is unchanged for all three files, verified by comparing the
first three bytes of the `HEAD` blob against the working-tree file: `EfcSelectionGuard.cs` and
`EfcSelectionGuardTests.cs` had and still have no BOM; `EfcFormController.cs` retains its BOM.
