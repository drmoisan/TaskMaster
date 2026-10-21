# [P0-T6] Provenance of the retained baseline coverage document

Timestamp: 2026-09-06T01-31

Command:

```powershell
Get-Item -LiteralPath 'coverage\782-p0-baseline.cobertura.xml'
Get-Item -LiteralPath 'coverage\782-p0-cov.txt'
Select-String -SimpleMatch 'Total tests:' -Path 'coverage\782-p0-cov.txt'
git check-ignore -v -- coverage/782-p0-baseline.cobertura.xml
```

EXIT_CODE: 0

Output Summary: the retained document and its companion log were both last written at
`2026-09-05 19:26:55`, which precedes the `Timestamp: 2026-09-05T21-59` carried by
`evidence/baseline/p0-t7-coverage.md`; the companion log records `Total tests: 6992`, which is the
superseded-base count and not the re-anchored `6997`; and the document is git-ignored by
`.gitignore:144`.

## File timestamps

```text
coverage\782-p0-baseline.cobertura.xml  CreationTime=2026-09-05 19:26:55  LastWriteTime=2026-09-05 19:26:55  Length=18144506
coverage\782-p0-cov.txt                 CreationTime=2026-09-05 19:26:25  LastWriteTime=2026-09-05 19:26:55  Length=521849
```

Both files were last written at the same instant, which is consistent with their being the two
outputs of one collection run.

## The companion log's recorded test count

```text
coverage\782-p0-cov.txt:7013:Total tests: 6992
```

Exactly one `Total tests:` line is present in that log.

## Git-ignore status

```text
.gitignore:144:coverage/*	coverage/782-p0-baseline.cobertura.xml
```

The `git check-ignore -v` line is non-empty and names `.gitignore` at line 144 with the pattern
`coverage/*`. No document under `coverage/` can be cited as committed evidence.

## Which recorded baseline test count the retained collection matches

Two baseline test counts are on record for this delivery:

- the superseded count **6992**, taken at the orphaned base; and
- the re-anchored count **6997**, recorded at `evidence/baseline/p0-t6-vstest.md:71` as
  `BASELINE_TOTAL_TESTS: 6997`.

The retained collection's companion log `coverage/782-p0-cov.txt` records **6992**. It therefore
matches the superseded count and does not match the re-anchored count.

**This is the discriminating observation for which collection wrote the retained document.** The
file-timestamp comparison also points the same way — the document was last written at
`2026-09-05 19:26:55`, before the `2026-09-05T21-59` timestamp the coverage artifact carries — but a
file timestamp is mutable ambient state and a recorded test count inside the log is not. The test
count is therefore the observation this record rests on, and the timestamp is corroboration.

The consequence carried into Phase 3 is that `coverage/782-p0-baseline.cobertura.xml` is the earlier,
superseded collection's output rather than the re-measurement's, so an artifact that names it as the
input for the re-measured figures 112355 and 26500 has named the wrong input. That is the R4 defect,
and [P3-T2] records the correction.
