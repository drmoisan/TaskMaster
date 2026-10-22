# P4-T21 — scoped format of the two new test files

Timestamp: 2026-09-13T16-36

Command: dotnet tool run csharpier format QuickFiler.Test/Controllers/QfcQueueEnqueueTests.cs QuickFiler.Test/Controllers/QfcQueueEnqueueTests.Harness.cs

EXIT_CODE: 0

Output Summary:
- Console output: `Formatted 2 files in 1708ms.`
- The formatter rewrote both files. The rewrite is not visible in the two porcelain captures below,
  because both files are new and therefore appear as untracked in both captures; it is visible in
  the measured line counts. Measured immediately after this command, the test-class part stands at
  425 lines and the harness part at 343, against the P4-T20 measurements of 419 and 339. The
  formatter therefore added 6 physical lines to the test-class part and 4 to the harness part,
  which is the line-adding behaviour the 470 trigger in P4-T20 exists to absorb.
- Both post-format counts remain below both the 470 trigger and the 500-line ceiling.

Command: dotnet tool run csharpier check QuickFiler.Test/Controllers/QfcQueueEnqueueTests.cs QuickFiler.Test/Controllers/QfcQueueEnqueueTests.Harness.cs

EXIT_CODE: 0

Output Summary:
- Console output: `Checked 2 files in 544ms.` with no file reported, which is this tool's
  success-case output for a scoped check.

PorcelainBefore:

```
 M QuickFiler.Test/QuickFiler.Test.csproj
 M docs/features/active/2026-09-11-qfcqueue-enqueue-path-lacks-injectable-seams-871/plan.2026-09-12T10-25.md
?? QuickFiler.Test/Controllers/QfcQueueEnqueueTests.Harness.cs
?? QuickFiler.Test/Controllers/QfcQueueEnqueueTests.cs
?? docs/features/active/2026-09-11-qfcqueue-enqueue-path-lacks-injectable-seams-871/evidence/baseline/coverage-baseline.2026-09-12T10-25.cobertura.xml
?? docs/features/active/2026-09-11-qfcqueue-enqueue-path-lacks-injectable-seams-871/evidence/qa-gates/p4-t20-test-file-size.2026-09-12T10-25.md
?? docs/features/active/2026-09-11-qfcqueue-enqueue-path-lacks-injectable-seams-871/evidence/qa-gates/p4-t3-analyze.2026-09-12T10-25.md
?? docs/features/active/2026-09-11-qfcqueue-enqueue-path-lacks-injectable-seams-871/evidence/regression-testing/p4-t10-catch-paths.2026-09-12T10-25.md
?? docs/features/active/2026-09-11-qfcqueue-enqueue-path-lacks-injectable-seams-871/evidence/regression-testing/p4-t11-collection-changed.2026-09-12T10-25.md
?? docs/features/active/2026-09-11-qfcqueue-enqueue-path-lacks-injectable-seams-871/evidence/regression-testing/p4-t12-move-monitor.2026-09-12T10-25.md
?? docs/features/active/2026-09-11-qfcqueue-enqueue-path-lacks-injectable-seams-871/evidence/regression-testing/p4-t13-digits.2026-09-12T10-25.md
?? docs/features/active/2026-09-11-qfcqueue-enqueue-path-lacks-injectable-seams-871/evidence/regression-testing/p4-t14-carrier.2026-09-12T10-25.md
?? docs/features/active/2026-09-11-qfcqueue-enqueue-path-lacks-injectable-seams-871/evidence/regression-testing/p4-t15-controller-passthrough.2026-09-12T10-25.md
?? docs/features/active/2026-09-11-qfcqueue-enqueue-path-lacks-injectable-seams-871/evidence/regression-testing/p4-t16-index-mapping.2026-09-12T10-25.md
?? docs/features/active/2026-09-11-qfcqueue-enqueue-path-lacks-injectable-seams-871/evidence/regression-testing/p4-t17-addasync-body.2026-09-12T10-25.md
?? docs/features/active/2026-09-11-qfcqueue-enqueue-path-lacks-injectable-seams-871/evidence/regression-testing/p4-t18-dispatcher-shapes.2026-09-12T10-25.md
?? docs/features/active/2026-09-11-qfcqueue-enqueue-path-lacks-injectable-seams-871/evidence/regression-testing/p4-t19-background-template.2026-09-12T10-25.md
?? docs/features/active/2026-09-11-qfcqueue-enqueue-path-lacks-injectable-seams-871/evidence/regression-testing/p4-t4-seam-contracts.2026-09-12T10-25.md
?? docs/features/active/2026-09-11-qfcqueue-enqueue-path-lacks-injectable-seams-871/evidence/regression-testing/p4-t5-headless-construction.2026-09-12T10-25.md
?? docs/features/active/2026-09-11-qfcqueue-enqueue-path-lacks-injectable-seams-871/evidence/regression-testing/p4-t6-viewer-factory-identity.2026-09-12T10-25.md
?? docs/features/active/2026-09-11-qfcqueue-enqueue-path-lacks-injectable-seams-871/evidence/regression-testing/p4-t7-guards.2026-09-12T10-25.md
?? docs/features/active/2026-09-11-qfcqueue-enqueue-path-lacks-injectable-seams-871/evidence/regression-testing/p4-t8-success-path.2026-09-12T10-25.md
?? docs/features/active/2026-09-11-qfcqueue-enqueue-path-lacks-injectable-seams-871/evidence/regression-testing/p4-t9-counter-bookkeeping.2026-09-12T10-25.md
```

PorcelainAfter:

```
 M QuickFiler.Test/QuickFiler.Test.csproj
 M docs/features/active/2026-09-11-qfcqueue-enqueue-path-lacks-injectable-seams-871/plan.2026-09-12T10-25.md
?? QuickFiler.Test/Controllers/QfcQueueEnqueueTests.Harness.cs
?? QuickFiler.Test/Controllers/QfcQueueEnqueueTests.cs
?? docs/features/active/2026-09-11-qfcqueue-enqueue-path-lacks-injectable-seams-871/evidence/baseline/coverage-baseline.2026-09-12T10-25.cobertura.xml
?? docs/features/active/2026-09-11-qfcqueue-enqueue-path-lacks-injectable-seams-871/evidence/qa-gates/p4-t20-test-file-size.2026-09-12T10-25.md
?? docs/features/active/2026-09-11-qfcqueue-enqueue-path-lacks-injectable-seams-871/evidence/qa-gates/p4-t3-analyze.2026-09-12T10-25.md
?? docs/features/active/2026-09-11-qfcqueue-enqueue-path-lacks-injectable-seams-871/evidence/regression-testing/p4-t10-catch-paths.2026-09-12T10-25.md
?? docs/features/active/2026-09-11-qfcqueue-enqueue-path-lacks-injectable-seams-871/evidence/regression-testing/p4-t11-collection-changed.2026-09-12T10-25.md
?? docs/features/active/2026-09-11-qfcqueue-enqueue-path-lacks-injectable-seams-871/evidence/regression-testing/p4-t12-move-monitor.2026-09-12T10-25.md
?? docs/features/active/2026-09-11-qfcqueue-enqueue-path-lacks-injectable-seams-871/evidence/regression-testing/p4-t13-digits.2026-09-12T10-25.md
?? docs/features/active/2026-09-11-qfcqueue-enqueue-path-lacks-injectable-seams-871/evidence/regression-testing/p4-t14-carrier.2026-09-12T10-25.md
?? docs/features/active/2026-09-11-qfcqueue-enqueue-path-lacks-injectable-seams-871/evidence/regression-testing/p4-t15-controller-passthrough.2026-09-12T10-25.md
?? docs/features/active/2026-09-11-qfcqueue-enqueue-path-lacks-injectable-seams-871/evidence/regression-testing/p4-t16-index-mapping.2026-09-12T10-25.md
?? docs/features/active/2026-09-11-qfcqueue-enqueue-path-lacks-injectable-seams-871/evidence/regression-testing/p4-t17-addasync-body.2026-09-12T10-25.md
?? docs/features/active/2026-09-11-qfcqueue-enqueue-path-lacks-injectable-seams-871/evidence/regression-testing/p4-t18-dispatcher-shapes.2026-09-12T10-25.md
?? docs/features/active/2026-09-11-qfcqueue-enqueue-path-lacks-injectable-seams-871/evidence/regression-testing/p4-t19-background-template.2026-09-12T10-25.md
?? docs/features/active/2026-09-11-qfcqueue-enqueue-path-lacks-injectable-seams-871/evidence/regression-testing/p4-t4-seam-contracts.2026-09-12T10-25.md
?? docs/features/active/2026-09-11-qfcqueue-enqueue-path-lacks-injectable-seams-871/evidence/regression-testing/p4-t5-headless-construction.2026-09-12T10-25.md
?? docs/features/active/2026-09-11-qfcqueue-enqueue-path-lacks-injectable-seams-871/evidence/regression-testing/p4-t6-viewer-factory-identity.2026-09-12T10-25.md
?? docs/features/active/2026-09-11-qfcqueue-enqueue-path-lacks-injectable-seams-871/evidence/regression-testing/p4-t7-guards.2026-09-12T10-25.md
?? docs/features/active/2026-09-11-qfcqueue-enqueue-path-lacks-injectable-seams-871/evidence/regression-testing/p4-t8-success-path.2026-09-12T10-25.md
?? docs/features/active/2026-09-11-qfcqueue-enqueue-path-lacks-injectable-seams-871/evidence/regression-testing/p4-t9-counter-bookkeeping.2026-09-12T10-25.md
```

Every path in both captures satisfies the Scope-lock rule: two Write Set paths, one Write Set
document, evidence artifacts under two of the three Write Set evidence directories, and the raw
Cobertura baseline document that P0-T12 deliberately left untracked.
