# Raw vstest Artifact Deletion — Remediation Cycle 1, Final Task

Timestamp: 2026-08-23T19-42

Command:
```bash
E=docs/features/active/winformspumphost-suite-determinism-511/evidence
find $E -name '*.trx'      -type f            # enumerate
find $E -name '*.coverage' -type f            # enumerate
find $E -name '*.trx'      -type f -delete
find $E -name '*.coverage' -type f -delete
find $E -type d -empty -not -path "*/.*" -print -delete   # prune emptied scratch directories
find $E -name '*.trx'      | wc -l            # verify
find $E -name '*.coverage' | wc -l            # verify
```

EXIT_CODE: 0

Output Summary:

### Deleted-file counts by extension

| Extension | Deleted by this task | Recursive count under `evidence/` after deletion | Required |
| --- | --- | --- | --- |
| `*.trx` | **1** | **0** | exactly 0 |
| `*.coverage` | **2** | **0** | exactly 0 |
| **Total** | **3** | **0** | — |

### What was deleted

All three files were produced by this cycle's own P3-T6 suite run and lived under the per-run scratch
directory `evidence/qa-gates/r1-p3-t6/`. Account and host name segments in the default
`vstest.console.exe` filenames are redacted as `<account>` and `<host>` per the repository's
host-identifier hygiene rule.

| # | Path (relative to `evidence/`) | Extension |
| --- | --- | --- |
| 1 | `qa-gates/r1-p3-t6/<account>_<host>_2026-08-23_19_20_16_net481.trx` | `.trx` |
| 2 | `qa-gates/r1-p3-t6/1fb74b86-ee3f-4956-baa8-eef44537b3b4/<account>_<host>_2026-08-23.19_21_00.coverage` | `.coverage` |
| 3 | `qa-gates/r1-p3-t6/<account>_<host>_2026-08-23_19_20_16/In/<host>/<account>_<host>_2026-08-23.19_21_00.coverage` | `.coverage` |

### Emptied scratch directories pruned

Five directories, deepest-first:

```
qa-gates/r1-p3-t6/1fb74b86-ee3f-4956-baa8-eef44537b3b4
qa-gates/r1-p3-t6/<account>_<host>_2026-08-23_19_20_16/In/<host>
qa-gates/r1-p3-t6/<account>_<host>_2026-08-23_19_20_16/In
qa-gates/r1-p3-t6/<account>_<host>_2026-08-23_19_20_16
qa-gates/r1-p3-t6
```

After pruning, the evidence tree contains zero empty directories and exactly five subdirectories:
`baseline/`, `other/`, `qa-gates/`, `regression-testing/`, and `remediation-baseline/`.

### Relationship to the earlier maintainer deletion

The 2026-08-23 maintainer deletion recorded in
`docs/features/active/winformspumphost-suite-determinism-511/evidence/other/raw-vstest-artifact-disposition.2026-08-23T21-40.md`
had already removed the 56 pre-existing `.trx` and 42 `.coverage` files (roughly 1,180.6 MB) and
pruned 188 empty scratch directories. This task therefore deleted only the raw artifacts newly
produced by the Phase 3 loop, principally the P3-T6 TRX.

### Why deletion is safe

All three deleted files were untracked or ignored — the `r1-p*-t*/` line appended to
`evidence/.gitignore` by P0-T9 covers the whole `r1-p3-t6/` subtree, and the repository-root
`.gitignore` already excludes `*.coverage`. None was ever staged or committed, so no committed
content is lost.

The distilled Markdown records are the evidence of record, per remediation-inputs Part 1 row 8, which
verified the committed distillation faithful against the raw TRX before their deletion. The P3-T6 run
is fully recorded in
`docs/features/active/winformspumphost-suite-determinism-511/evidence/qa-gates/remediation-suite-run.2026-08-23T20-57.md`,
including the `ResultSummary/Counters` block verbatim, the per-assembly pass and fail counts for all
nine assemblies, and the outcome of each of the four owned named tests. Repository policy rejects
committed raw machine test and coverage artifacts.

The post-processed Cobertura XML remains available outside the evidence tree for the downstream
review gate, at the gitignored producer paths `coverage\remediation.cobertura.xml` and
`artifacts/csharp/coverage.xml`; neither is under `evidence/` and neither is affected by this
deletion.
