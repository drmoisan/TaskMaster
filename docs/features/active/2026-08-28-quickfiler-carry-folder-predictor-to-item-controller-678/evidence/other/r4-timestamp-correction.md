# R4 — Evidence timestamp correction

- Timestamp: 2026-09-02T01-30
- Issue: #678
- Tasks: [P1-T12] (the correction) and [P1-T13] (the no-other-field proof)

## Derivation method, in one sentence

Each corrected value is the `yyyy-MM-ddTHH-mm` truncation of that artifact's own filesystem
`LastWriteTime`, captured by Derivation D9 at P0-T12 **before any edit touched the
directory**, and the five nested values inside `final-toolchain-pass.md` are copied from the
corrected values of the artifacts their own `Detail:` lines reference.

## Clause 1 and 2 — the corrected values, with the originals and the source mtimes

Every value in the "Corrected" column is the exact third-column value P0-T12 recorded for
that file. No value is chosen by any other means.

| # | File | Source mtime | Original declared | Corrected | Drift removed |
|---|---|---|---|---|---|
| 1 | `analyzer-build.md` | 2026-09-01T22:43:19 | `2026-09-01T23-48` | `2026-09-01T22-43` | 65 min |
| 2 | `coverage-delta.md` | 2026-09-01T23:17:45 | `2026-09-02T00-02` | `2026-09-01T23-17` | 45 min |
| 3 | `coverage-post-change.md` | 2026-09-01T23:17:07 | `2026-09-01T23-58` | `2026-09-01T23-17` | 41 min |
| 4 | `csharpier-check.md` | 2026-09-01T22:42:34 | `2026-09-01T23-46` | `2026-09-01T22-42` | 64 min |
| 5 | `csharpier-format.md` | 2026-09-01T22:42:12 | `2026-09-01T23-45` | `2026-09-01T22-42` | 63 min |
| 6 | `exclude-attribute-invariant.md` | 2026-09-01T23:18:20 | `2026-09-02T00-14` | `2026-09-01T23-18` | 56 min |
| 7 | `file-size-audit.md` | 2026-09-01T23:19:15 | `2026-09-02T00-18` | `2026-09-01T23-19` | 59 min |
| 8 | `final-commit.md` | 2026-09-01T23:25:27 | `2026-09-02T00-46` | `2026-09-01T23-25` | 81 min |
| 9 | `final-toolchain-pass.md` | 2026-09-01T23:20:42 | `2026-09-02T00-28` | `2026-09-01T23-20` | 68 min |
| 10 | `mstest-coverage-run.md` | 2026-09-01T23:03:33 | `2026-09-01T23-12` | `2026-09-01T23-03` | 9 min |
| 11 | `nullable-build.md` | 2026-09-01T22:43:33 | `2026-09-01T23-49` | `2026-09-01T22-43` | 66 min |
| 12 | `scope-confinement.md` | 2026-09-01T23:20:03 | `2026-09-02T00-24` | `2026-09-01T23-20` | 64 min |

The five nested `- Timestamp:` declarations inside `final-toolchain-pass.md`:

| # | Line | Original declared | Corrected | Copied from the corrected value of |
|---|---|---|---|---|
| 1 | 9 | `2026-09-02T00-05` | `2026-09-01T22-42` | `csharpier-format.md` (row 5) |
| 2 | 20 | `2026-09-02T00-06` | `2026-09-01T22-42` | `csharpier-check.md` (row 4) |
| 3 | 29 | `2026-09-02T00-07` | `2026-09-01T22-43` | `analyzer-build.md` (row 1) |
| 4 | 39 | `2026-09-02T00-08` | `2026-09-01T22-43` | `nullable-build.md` (row 11) |
| 5 | 48 | `2026-09-02T00-10` | `2026-09-01T23-03` | `mstest-coverage-run.md` (row 10) |

`coverage-post-change.jacoco.xml` declares no `Timestamp:` and was **not edited**. It is a
generated Cobertura/JaCoCo XML document; it carries no Markdown field to correct, and
angle-bracket redaction inside an XML attribute value would produce invalid XML.

## Clause 4 — the ordering check

The twelve Markdown artifacts sorted by their **original declared** value, with the corrected
value each takes:

| Order by declared value | File | Declared | Corrected |
|---|---|---|---|
| 1 | `mstest-coverage-run.md` | `2026-09-01T23-12` | `2026-09-01T23-03` |
| 2 | `csharpier-format.md` | `2026-09-01T23-45` | `2026-09-01T22-42` |
| 3 | `csharpier-check.md` | `2026-09-01T23-46` | `2026-09-01T22-42` |
| 4 | `analyzer-build.md` | `2026-09-01T23-48` | `2026-09-01T22-43` |
| 5 | `nullable-build.md` | `2026-09-01T23-49` | `2026-09-01T22-43` |
| 6 | `coverage-post-change.md` | `2026-09-01T23-58` | `2026-09-01T23-17` |
| 7 | `coverage-delta.md` | `2026-09-02T00-02` | `2026-09-01T23-17` |
| 8 | `exclude-attribute-invariant.md` | `2026-09-02T00-14` | `2026-09-01T23-18` |
| 9 | `file-size-audit.md` | `2026-09-02T00-18` | `2026-09-01T23-19` |
| 10 | `scope-confinement.md` | `2026-09-02T00-24` | `2026-09-01T23-20` |
| 11 | `final-toolchain-pass.md` | `2026-09-02T00-28` | `2026-09-01T23-20` |
| 12 | `final-commit.md` | `2026-09-02T00-46` | `2026-09-01T23-25` |

`coverage-post-change.jacoco.xml` is excluded from this sort because it declares no
top-level value.

**The corrected sequence is NOT non-decreasing in that order.** Positions 2 through 12 are
non-decreasing among themselves; position 1 inverts against four of them.

Every inverting pair, enumerated by both file names, both mtimes and both original values:

| # | Earlier by declared value | Later by declared value | Earlier mtime | Later mtime |
|---|---|---|---|---|
| 1 | `mstest-coverage-run.md`, declared `2026-09-01T23-12` | `csharpier-format.md`, declared `2026-09-01T23-45` | 2026-09-01T23:03:33 | 2026-09-01T22:42:12 |
| 2 | `mstest-coverage-run.md`, declared `2026-09-01T23-12` | `csharpier-check.md`, declared `2026-09-01T23-46` | 2026-09-01T23:03:33 | 2026-09-01T22:42:34 |
| 3 | `mstest-coverage-run.md`, declared `2026-09-01T23-12` | `analyzer-build.md`, declared `2026-09-01T23-48` | 2026-09-01T23:03:33 | 2026-09-01T22:43:19 |
| 4 | `mstest-coverage-run.md`, declared `2026-09-01T23-12` | `nullable-build.md`, declared `2026-09-01T23-49` | 2026-09-01T23:03:33 | 2026-09-01T22:43:33 |

All four involve `mstest-coverage-run.md` and no other pair inverts.

## Clause 5 — the ordering sub-clause is superseded, and why

R4 acceptance clause 1 asks for values that are both real clock values **and** preserve the
existing relative ordering. **Those two properties are not jointly satisfiable here**, so the
ordering sub-clause is superseded by real-clock fidelity, which is the property R4 exists to
restore.

The reason is that the declared ordering and the filesystem ordering genuinely disagree, for
at least the pair `mstest-coverage-run.md` (declared `2026-09-01T23-12`, mtime
`2026-09-01 23:03`) and `csharpier-format.md` (declared `2026-09-01T23-45`, mtime
`2026-09-01 22:42`): the first is declared *earlier* but was written *later*. No assignment
of real clock values can preserve both properties, because preserving the declared ordering
would require assigning `csharpier-format.md` a value later than `mstest-coverage-run.md`'s,
which its own mtime contradicts.

The remediation-inputs statement that "relative ordering is correct" is therefore itself
inaccurate for that file. Real-clock fidelity is chosen because it is the stated defect R4
names — that the values are "neither local time nor UTC" — and because the mtimes are
recoverable evidence while the declared ordering is not.

## Clause 6 — the count of corrected declarations

```
12 top-level declarations + 5 nested declarations = 17
```

This equals the total P0-T12 recorded as in scope for R4 (17).

## Clause 7 — no other field was altered

No `Command:`, `EXIT_CODE:`, `ExpectedExitCode:` or `Output Summary:` value is altered
anywhere. Proved mechanically in the next section.

An implementation note, recorded because it produced a transient wrong state. The first pass
of the correction script keyed its nested-value table by line number in a PowerShell
`[ordered]` dictionary. An `OrderedDictionary` indexed with an integer performs **positional**
lookup rather than key lookup, so all five nested lookups returned null and the five nested
values were written empty. The blanking was detected by reading the five lines back
immediately afterwards and was repaired in the same task with a table of explicit pairs that
is never indexed by an integer key. The diff below is taken after the repair and shows the
five nested lines carrying their correct values, so the transient state did not reach any
committed artifact.

## No-other-field proof

Command, run before any commit of the P1-T12 edit, so the comparison is against the last
committed state of these artifacts rather than against the base ref, at which they did not
yet exist:

```
git diff HEAD -- docs/features/active/2026-08-28-quickfiler-carry-folder-predictor-to-item-controller-678/evidence/qa-gates
```

### Clause 1 — every added line is a `Timestamp:` line

The 17 added lines, verbatim:

```
+Timestamp: 2026-09-01T22-43
+Timestamp: 2026-09-01T23-17
+Timestamp: 2026-09-01T23-17
+Timestamp: 2026-09-01T22-42
+Timestamp: 2026-09-01T22-42
+Timestamp: 2026-09-01T23-18
+Timestamp: 2026-09-01T23-19
+Timestamp: 2026-09-01T23-25
+Timestamp: 2026-09-01T23-20
+- Timestamp: 2026-09-01T22-42
+- Timestamp: 2026-09-01T22-42
+- Timestamp: 2026-09-01T22-43
+- Timestamp: 2026-09-01T22-43
+- Timestamp: 2026-09-01T23-03
+Timestamp: 2026-09-01T23-03
+Timestamp: 2026-09-01T22-43
+Timestamp: 2026-09-01T23-20
```

Each begins, after leading whitespace and an optional `- ` list marker, with the literal
`Timestamp:`.

### Clause 2 — every removed line is a `Timestamp:` line

The 17 removed lines, verbatim:

```
-Timestamp: 2026-09-01T23-48
-Timestamp: 2026-09-02T00-02
-Timestamp: 2026-09-01T23-58
-Timestamp: 2026-09-01T23-46
-Timestamp: 2026-09-01T23-45
-Timestamp: 2026-09-02T00-14
-Timestamp: 2026-09-02T00-18
-Timestamp: 2026-09-02T00-46
-Timestamp: 2026-09-02T00-28
-- Timestamp: 2026-09-02T00-05
-- Timestamp: 2026-09-02T00-06
-- Timestamp: 2026-09-02T00-07
-- Timestamp: 2026-09-02T00-08
-- Timestamp: 2026-09-02T00-10
-Timestamp: 2026-09-01T23-12
-Timestamp: 2026-09-01T23-49
-Timestamp: 2026-09-02T00-24
```

### Clause 3 — added count equals removed count equals the declaration count

`git diff HEAD --numstat` over the same path:

```
1  1  analyzer-build.md
1  1  coverage-delta.md
1  1  coverage-post-change.md
1  1  csharpier-check.md
1  1  csharpier-format.md
1  1  exclude-attribute-invariant.md
1  1  file-size-audit.md
1  1  final-commit.md
6  6  final-toolchain-pass.md
1  1  mstest-coverage-run.md
1  1  nullable-build.md
1  1  scope-confinement.md
```

Totals: **17 added, 17 removed**, equal to each other and equal to the declaration count of
17 recorded in clause 6. `final-toolchain-pass.md` accounts for 6 of each: its own top-level
declaration plus the five nested ones.

### Clause 4 — the diff touches no other file

The diff names exactly the twelve Markdown artifacts above and no path outside
`evidence/qa-gates/`. `coverage-post-change.jacoco.xml` does not appear in the diff at all,
so it was not touched.

Every hunk header is `@@ -1,6 +1,6 @@` except `final-toolchain-pass.md`, whose hunks are
`@@ -1,12 +1,12 @@`, `@@ -17,7 +17,7 @@`, `@@ -26,7 +26,7 @@`, `@@ -36,7 +36,7 @@` and
`@@ -45,7 +45,7 @@`. Every hunk has equal before and after line counts, so no line was added
or deleted anywhere, only replaced. The context lines visible in each hunk show the adjacent
`Command:`, `EXIT_CODE:` and `Output Summary:` fields unchanged.

## Output Summary

17 timestamp declarations corrected across 12 Markdown artifacts: 12 top-level and 5 nested
inside `final-toolchain-pass.md`. Every corrected value is the `yyyy-MM-ddTHH-mm` truncation
of that artifact's own pre-edit `LastWriteTime`, from P0-T12. The declared relative ordering
could not be preserved and is superseded by real-clock fidelity; four inverting pairs are
enumerated, all involving `mstest-coverage-run.md`. The anchored diff shows 17 added and 17
removed lines, every one of them a `Timestamp:` line, in 12 files, with no other field and no
other file touched.
