# Baseline — `R_TIMESTAMP_PREIMAGE` for the R4 correction

- Timestamp: 2026-09-02T01-11
- Issue: #678
- Task: [P0-T12]
- Derivation: D9, applied to
  `docs/features/active/2026-08-28-quickfiler-carry-folder-predictor-to-item-controller-678/evidence/qa-gates`

This capture was taken **before any task in this cycle wrote to `evidence/qa-gates/`**.
Writing a file replaces the `LastWriteTime` the correction is derived from, so this ordering
constraint is load-bearing: after P1-T12 or P2-T1 runs, the mtimes below are no longer
recoverable.

## Clause 1 and 2 — the 13 files, their mtimes and their truncations

`Get-ChildItem -File` reported exactly **13** files, which is the complete directory
listing. Sorted by name:

| # | File | `LastWriteTime` (to the second) | `yyyy-MM-ddTHH-mm` truncation | Declared `Timestamp:` |
|---|---|---|---|---|
| 1 | `analyzer-build.md` | 2026-09-01T22:43:19 | `2026-09-01T22-43` | `2026-09-01T23-48` |
| 2 | `coverage-delta.md` | 2026-09-01T23:17:45 | `2026-09-01T23-17` | `2026-09-02T00-02` |
| 3 | `coverage-post-change.jacoco.xml` | 2026-09-01T23:17:18 | `2026-09-01T23-17` | `NONE` |
| 4 | `coverage-post-change.md` | 2026-09-01T23:17:07 | `2026-09-01T23-17` | `2026-09-01T23-58` |
| 5 | `csharpier-check.md` | 2026-09-01T22:42:34 | `2026-09-01T22-42` | `2026-09-01T23-46` |
| 6 | `csharpier-format.md` | 2026-09-01T22:42:12 | `2026-09-01T22-42` | `2026-09-01T23-45` |
| 7 | `exclude-attribute-invariant.md` | 2026-09-01T23:18:20 | `2026-09-01T23-18` | `2026-09-02T00-14` |
| 8 | `file-size-audit.md` | 2026-09-01T23:19:15 | `2026-09-01T23-19` | `2026-09-02T00-18` |
| 9 | `final-commit.md` | 2026-09-01T23:25:27 | `2026-09-01T23-25` | `2026-09-02T00-46` |
| 10 | `final-toolchain-pass.md` | 2026-09-01T23:20:42 | `2026-09-01T23-20` | `2026-09-02T00-28` |
| 11 | `mstest-coverage-run.md` | 2026-09-01T23:03:33 | `2026-09-01T23-03` | `2026-09-01T23-12` |
| 12 | `nullable-build.md` | 2026-09-01T22:43:33 | `2026-09-01T22-43` | `2026-09-01T23-49` |
| 13 | `scope-confinement.md` | 2026-09-01T23:20:03 | `2026-09-01T23-20` | `2026-09-02T00-24` |

## Clause 3 — the artifact that declares no top-level `Timestamp:`

Row 3, `coverage-post-change.jacoco.xml`, declares `NONE`. It is a generated Cobertura/JaCoCo
XML document and carries no Markdown `Timestamp:` field. It is not edited by P1-T12 and is
excluded from the ordering check in clause 4. Editing it is additionally prohibited on its own
grounds: angle-bracket redaction inside an XML attribute value would produce invalid XML.

The other twelve are Markdown artifacts and each declares exactly one top-level
`Timestamp:` at the head of the file.

## Clause 4 — the five nested `- Timestamp:` declarations in `final-toolchain-pass.md`

Each nested declaration sits in a per-command section whose `Output Summary:` ends with a
`Detail:` reference naming the per-command artifact it summarises. The `Detail:` reference is
written inline at the end of the summary prose rather than on its own list line.

| # | Line | Nested declared value | Command | `Detail:` reference | Corrected value to copy |
|---|---|---|---|---|---|
| 1 | 9 | `2026-09-02T00-05` | `dotnet tool run csharpier format .` | `evidence/qa-gates/csharpier-format.md` | `2026-09-01T22-42` |
| 2 | 20 | `2026-09-02T00-06` | `dotnet tool run csharpier check .` | `evidence/qa-gates/csharpier-check.md` | `2026-09-01T22-42` |
| 3 | 29 | `2026-09-02T00-07` | analyzer build | `evidence/qa-gates/analyzer-build.md` | `2026-09-01T22-43` |
| 4 | 39 | `2026-09-02T00-08` | nullable build | `evidence/qa-gates/nullable-build.md` | `2026-09-01T22-43` |
| 5 | 48 | `2026-09-02T00-10` | MSTest with coverage | `evidence/qa-gates/mstest-coverage-run.md` | `2026-09-01T23-03` |

Each corrected value in the last column is the clause-1 truncation of the referenced
artifact's own mtime, taken from the table above and not derived by any other means.

## Clause 5 — total declarations in scope for R4

```
12 top-level declarations (one per Markdown artifact; the .jacoco.xml declares none)
+ 5 nested declarations inside final-toolchain-pass.md
= 17 declarations in scope for R4
```

`R_TIMESTAMP_PREIMAGE` is the union of the 13-row table and the 5-row nested table above.

## The drift R4 exists to correct

Every declared value runs ahead of its own file's mtime, by between 9 and 81 minutes, and
the six latest land on the following calendar date. The largest single drift is
`final-commit.md` at 81 minutes; the smallest is `mstest-coverage-run.md` at 9 minutes.

The remediation-inputs statement that "relative ordering is correct" does not hold. Sorting
the twelve Markdown artifacts by their **declared** value and reading their **mtimes** in
that order produces four inverting pairs, all of them involving `mstest-coverage-run.md`:

| Earlier by declared value | Later by declared value | Earlier mtime | Later mtime |
|---|---|---|---|
| `mstest-coverage-run.md` (`2026-09-01T23-12`) | `csharpier-format.md` (`2026-09-01T23-45`) | 2026-09-01T23:03:33 | 2026-09-01T22:42:12 |
| `mstest-coverage-run.md` (`2026-09-01T23-12`) | `csharpier-check.md` (`2026-09-01T23-46`) | 2026-09-01T23:03:33 | 2026-09-01T22:42:34 |
| `mstest-coverage-run.md` (`2026-09-01T23-12`) | `analyzer-build.md` (`2026-09-01T23-48`) | 2026-09-01T23:03:33 | 2026-09-01T22:43:19 |
| `mstest-coverage-run.md` (`2026-09-01T23-12`) | `nullable-build.md` (`2026-09-01T23-49`) | 2026-09-01T23:03:33 | 2026-09-01T22:43:33 |

No assignment of real clock values can preserve both real-clock fidelity and the declared
relative ordering, because the two genuinely disagree. P1-T12 records which property is
preserved and why.

## Output Summary

13 files enumerated, which is the complete directory listing. 12 declare a top-level
`Timestamp:`; `coverage-post-change.jacoco.xml` declares `NONE`. 5 nested `- Timestamp:`
declarations inside `final-toolchain-pass.md` are enumerated with the per-command artifact
each `Detail:` line references. Total declarations in scope for R4: **17**. Declared values
run 9 to 81 minutes ahead of their own mtimes, and the declared relative ordering is
falsified by four inverting pairs involving `mstest-coverage-run.md`.
