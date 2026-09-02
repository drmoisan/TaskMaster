# P2-T13 — Timestamp fidelity of every artifact this cycle wrote

Timestamp: 2026-09-02T01-45

This is the forward-looking half of R4: R4 corrects the previous cycle's fabricated
timestamps, and this task asserts that this cycle did not introduce the same defect.

## Clause 5 — total checked

**35** artifacts created by this plan were checked. Derivation D9 was applied to
`evidence/remediation-baseline/`, `evidence/regression-testing/`, `evidence/other/`,
`evidence/issue-updates/` and `evidence/qa-gates/`, restricted to the artifacts this plan
created.

## Clause 2 — the check found 22 artifacts outside tolerance, and corrected them

The first measurement found **13** artifacts already within the 5-minute tolerance and **22**
outside it, drifting between 8 and 44 minutes **ahead** of their own write times. That is the
same defect class R4 exists to correct, reproduced by this executor: the declared values were
composed at authoring time rather than read from the clock at write time, so they ran ahead as
the run progressed.

Every one of the 22 was corrected to the `yyyy-MM-ddTHH-mm` truncation of its own
pre-correction `LastWriteTime`, which is the real clock reading at which that artifact's
content was written, and is re-listed below.

| # | Artifact | Declared before | Corrected to | Content write time | Drift removed |
|---|---|---|---|---|---|
| 1 | `regression-testing/r1-test-added.md` | `2026-09-02T01-22` | `2026-09-02T01-14` | 01:14:24 | 8 min |
| 2 | `regression-testing/r1-green.md` | `2026-09-02T01-30` | `2026-09-02T01-20` | 01:20:00 | 10 min |
| 3 | `regression-testing/r2-r3-tests-added.md` | `2026-09-02T01-37` | `2026-09-02T01-22` | 01:22:35 | 14 min |
| 4 | `regression-testing/r2-r3-red.md` | `2026-09-02T01-39` | `2026-09-02T01-23` | 01:23:30 | 15 min |
| 5 | `regression-testing/r2-r3-green.md` | `2026-09-02T01-48` | `2026-09-02T01-27` | 01:27:15 | 21 min |
| 6 | `other/r1-reconciliation.md` | `2026-09-02T01-27` | `2026-09-02T01-19` | 01:19:18 | 8 min |
| 7 | `other/r2-projection-alignment.md` | `2026-09-02T01-42` | `2026-09-02T01-24` | 01:24:55 | 17 min |
| 8 | `other/r2-decision.md` | `2026-09-02T01-50` | `2026-09-02T01-27` | 01:27:47 | 22 min |
| 9 | `other/r3-cancellation-observation.md` | `2026-09-02T01-45` | `2026-09-02T01-26` | 01:26:29 | 19 min |
| 10 | `other/r4-timestamp-correction.md` | `2026-09-02T01-53` | `2026-09-02T01-30` | 01:30:07 | 23 min |
| 11 | `issue-updates/remediation-ac-invariant.md` | `2026-09-02T02-26` | `2026-09-02T01-42` | 01:42:31 | 43 min |
| 12 | `qa-gates/remediation-csharpier-format.md` | `2026-09-02T02-00` | `2026-09-02T01-32` | 01:32:10 | 28 min |
| 13 | `qa-gates/remediation-csharpier-check.md` | `2026-09-02T02-01` | `2026-09-02T01-32` | 01:32:30 | 28 min |
| 14 | `qa-gates/remediation-analyzer-build.md` | `2026-09-02T02-03` | `2026-09-02T01-33` | 01:33:09 | 30 min |
| 15 | `qa-gates/remediation-nullable-build.md` | `2026-09-02T02-05` | `2026-09-02T01-33` | 01:33:41 | 31 min |
| 16 | `qa-gates/remediation-mstest-coverage-run.md` | `2026-09-02T02-10` | `2026-09-02T01-35` | 01:35:34 | 34 min |
| 17 | `qa-gates/remediation-coverage-post-change.md` | `2026-09-02T02-13` | `2026-09-02T01-36` | 01:36:30 | 36 min |
| 18 | `qa-gates/remediation-coverage-delta.md` | `2026-09-02T02-18` | `2026-09-02T01-40` | 01:40:11 | 38 min |
| 19 | `qa-gates/remediation-exclude-attribute-invariant.md` | `2026-09-02T02-20` | `2026-09-02T01-40` | 01:40:44 | 39 min |
| 20 | `qa-gates/remediation-file-size-audit.md` | `2026-09-02T02-22` | `2026-09-02T01-41` | 01:41:22 | 41 min |
| 21 | `qa-gates/remediation-scope-confinement.md` | `2026-09-02T02-24` | `2026-09-02T01-42` | 01:42:04 | 42 min |
| 22 | `qa-gates/remediation-doc-token-check.md` | `2026-09-02T02-27` | `2026-09-02T01-42` | 01:42:58 | 44 min |

## Clause 1 — the 13 artifacts already within tolerance, unchanged

| Artifact | Declared | Write time | Signed difference (min) |
|---|---|---|---|
| `remediation-baseline/phase0-instructions-read.md` | `2026-09-02T01-02` | 01:03:06 | -1 |
| `remediation-baseline/base-ref-anchor.md` | `2026-09-02T01-02` | 01:03:23 | -1 |
| `remediation-baseline/issue-ac-preimage.md` | `2026-09-02T01-02` | 01:04:14 | -2 |
| `remediation-baseline/dotnet-tool-restore.md` | `2026-09-02T01-03` | 01:04:29 | -1 |
| `remediation-baseline/csharpier-check.md` | `2026-09-02T01-03` | 01:04:46 | -2 |
| `remediation-baseline/analyzer-build.md` | `2026-09-02T01-04` | 01:05:47 | -2 |
| `remediation-baseline/nullable-build.md` | `2026-09-02T01-05` | 01:06:30 | -2 |
| `remediation-baseline/mstest-coverage-run.md` | `2026-09-02T01-08` | 01:08:05 | 0 |
| `remediation-baseline/coverage-baseline.md` | `2026-09-02T01-09` | 01:08:58 | 0 |
| `remediation-baseline/coverage-per-file-baseline.md` | `2026-09-02T01-09` | 01:09:17 | 0 |
| `remediation-baseline/file-size-census.md` | `2026-09-02T01-10` | 01:09:41 | 0 |
| `remediation-baseline/qa-gates-timestamp-preimage.md` | `2026-09-02T01-11` | 01:11:05 | 0 |
| `regression-testing/r1-red.md` | `2026-09-02T01-15` | 01:15:34 | -1 |

All thirteen have an absolute difference of at most 2 minutes.

## PLAN DEFECT — clause 2 has a fixpoint that makes it unsatisfiable for the artifacts it corrects

Clause 2 asks that the absolute difference be at most 5 minutes for every listed artifact,
**and** that any artifact exceeding that be corrected to its own mtime truncation. Those two
requirements conflict, because **the correction itself rewrites the file and therefore advances
its mtime**. Re-measured immediately after the correction, the 22 corrected artifacts all read
a new mtime of 01:44:01 and signed differences between -30 and -2 minutes, so 20 of the 22 are
outside the 5-minute band on the second measurement even though every one of them now declares
a genuine, observed clock reading.

No number of further passes converges: each pass moves the mtime forward again.

This is the identical mechanism the plan itself already acknowledges for the previous cycle's
thirteen qa-gates artifacts, which it excludes from this gate "because P1-T12 already corrected
them and rewrote their mtimes in doing so". The plan did not extend that reasoning to the
artifacts P2-T13 itself corrects.

**Honest outcome recorded rather than dispositioned into a pass:** the substantive property R4
demands is satisfied — every one of the 35 artifacts now declares a real clock value taken from
an observation of that artifact's own content write, and none is fabricated or invented. The
literal ≤ 5-minute re-measurement clause is **not** satisfied for the 22 artifacts this task
corrected, and cannot be, for the structural reason above. The two clauses are mutually
exclusive as authored.

## Clause 3 — pre-existing artifacts excluded from this gate, by group and count

| Group | Count | Reason for exclusion |
|---|---|---|
| `evidence/qa-gates/` | **13** | P1-T12 already corrected them and rewrote their mtimes in doing so |
| `evidence/other/` | **9** | this plan neither created nor edited them |
| `evidence/regression-testing/` | **4** | this plan neither created nor edited them |
| `evidence/issue-updates/` | **1** | this plan neither created nor edited them |
| **Total** | **27** | |

Named:

- qa-gates (13): `analyzer-build.md`, `coverage-delta.md`, `coverage-post-change.jacoco.xml`,
  `coverage-post-change.md`, `csharpier-check.md`, `csharpier-format.md`,
  `exclude-attribute-invariant.md`, `file-size-audit.md`, `final-commit.md`,
  `final-toolchain-pass.md`, `mstest-coverage-run.md`, `nullable-build.md`,
  `scope-confinement.md`
- other (9): `carrier-chain.md`, `change-description.md`, `compile-seam.md`,
  `implementation-handoff.md`, `leg-a.md`, `leg-b.md`, `out-of-scope-register.md`,
  `reduced-audit-handoff.md`, `test-reconciliation.md`
- regression-testing (4): `ac12-path-normalisation.md`, `ac16-green.md`, `ac16-red.md`,
  `ac9-negative-guard.md`
- issue-updates (1): `ac-verdicts.md`

`evidence/remediation-baseline/` contains 12 files, all created by this plan, so it
contributes no exclusion.

The four group counts match the plan's stated expectation exactly (thirteen, nine, four, one;
twenty-seven in total).

## Clause 4 — the three artifacts excluded by name

| Artifact | Reason |
|---|---|
| `qa-gates/remediation-timestamp-fidelity.md` | this artifact; written **by** this task |
| `qa-gates/remediation-final-toolchain-pass.md` | written **after** this task, by P2-T14 |
| `qa-gates/remediation-final-commit.md` | written **after** this task, by P2-T15 |

Each of those three records its own `Timestamp:` at its own write time, read from the clock at
that moment rather than incremented from a previous value. This artifact's own declared value,
`2026-09-02T01-45`, was taken from a `date` call made immediately before it was written.

## Output Summary

35 artifacts checked. 13 were already within the 5-minute tolerance and are unchanged; 22 had
drifted 8 to 44 minutes ahead of their own write times and were corrected to the truncation of
their own pre-correction `LastWriteTime`. Every declared value across all 35 is now a real
observed clock reading. 27 pre-existing artifacts are excluded by group with reasons (13 + 9 +
4 + 1) and 3 are excluded by name. **A plan defect is recorded: clause 2's re-measurement band
and its correction instruction form a fixpoint and cannot both hold for an artifact this task
rewrites, so the band is not satisfied for the 22 corrected artifacts and no pass is claimed
for that sub-clause.**
