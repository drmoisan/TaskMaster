# P3-T8 — scoped CSharpier format and check for Phase 3

Timestamp: 2026-09-13T15-43

Command: dotnet tool run csharpier format QuickFiler/Controllers/QfcQueue.Tlp.cs QuickFiler/Controllers/QfcQueue.Enqueue.cs

EXIT_CODE: 0

Output Summary:
- CMD-FORMAT-SCOPED printed `Formatted 2 files in 1668ms.` and exited 0.
- CMD-CHECK-SCOPED printed `Checked 2 files in 501ms.` and exited 0.
- The formatter rewrote the substituted item-group statement, which measured 102 columns before the
  run and therefore exceeded CSharpier's 100-column default. The re-wrap was anticipated by P3-T5
  and is not a defect.
- The two porcelain captures below are identical. Both already listed the two code paths as
  modified before the run, because P3-T1 through P3-T6 had edited them, so the capture pair does not
  by itself distinguish a run that rewrote from one that did not; the formatter's own
  `Formatted 2 files` line and the changed statement shape recorded below are what establish that
  this run rewrote.

## Porcelain capture immediately before CMD-FORMAT-SCOPED

```
 M QuickFiler/Controllers/QfcQueue.Enqueue.cs
 M QuickFiler/Controllers/QfcQueue.Tlp.cs
 M docs/features/active/2026-09-11-qfcqueue-enqueue-path-lacks-injectable-seams-871/plan.2026-09-12T10-25.md
?? docs/features/active/2026-09-11-qfcqueue-enqueue-path-lacks-injectable-seams-871/evidence/baseline/coverage-baseline.2026-09-12T10-25.cobertura.xml
?? docs/features/active/2026-09-11-qfcqueue-enqueue-path-lacks-injectable-seams-871/evidence/qa-gates/p3-t5-itemgroup-callsite.2026-09-12T10-25.md
?? docs/features/active/2026-09-11-qfcqueue-enqueue-path-lacks-injectable-seams-871/evidence/qa-gates/p3-t7-out-of-scope-untouched.2026-09-12T10-25.md
```

## Porcelain capture immediately after CMD-FORMAT-SCOPED

```
 M QuickFiler/Controllers/QfcQueue.Enqueue.cs
 M QuickFiler/Controllers/QfcQueue.Tlp.cs
 M docs/features/active/2026-09-11-qfcqueue-enqueue-path-lacks-injectable-seams-871/plan.2026-09-12T10-25.md
?? docs/features/active/2026-09-11-qfcqueue-enqueue-path-lacks-injectable-seams-871/evidence/baseline/coverage-baseline.2026-09-12T10-25.cobertura.xml
?? docs/features/active/2026-09-11-qfcqueue-enqueue-path-lacks-injectable-seams-871/evidence/qa-gates/p3-t5-itemgroup-callsite.2026-09-12T10-25.md
?? docs/features/active/2026-09-11-qfcqueue-enqueue-path-lacks-injectable-seams-871/evidence/qa-gates/p3-t7-out-of-scope-untouched.2026-09-12T10-25.md
```

## CMD-CHECK-SCOPED

Command: dotnet tool run csharpier check QuickFiler/Controllers/QfcQueue.Tlp.cs QuickFiler/Controllers/QfcQueue.Enqueue.cs

```
Checked 2 files in 501ms.
```

CheckExitCode: 0

## Post-format form of the substituted statement

The formatter split the single-line lambda into a multi-line one. The line carrying the seam call is
line 176 and measures 81 columns; the re-wrapped lambda spans lines 175 through 177 of
`QuickFiler/Controllers/QfcQueue.Enqueue.cs`, inside the fluent chain that begins at line 172.

PostFormatStatement:                 .SelectAwait(async i =>                     (i: i, grp: await ItemGroupFactory(tlp, items[i - start], i))                 )

PostFormatSeamCallLine: 176
PostFormatSeamCallColumnWidth: 81

Reproduced with its physical line breaks preserved:

```
                .SelectAwait(async i =>
                    (i: i, grp: await ItemGroupFactory(tlp, items[i - start], i))
                )
```

Measured column widths of the three physical lines: 39, 81 and 17.

This is the form that AC5 evidence cites. The `PreFormatStatement:` line in the P3-T5 artifact,
which measured 102 columns on one physical line, is retained only to make the re-wrap auditable.
