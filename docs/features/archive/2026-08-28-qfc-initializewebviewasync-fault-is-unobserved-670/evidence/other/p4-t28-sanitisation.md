# P4-T28 — Whole-tree sanitisation sweep over the evidence tree

Timestamp: 2026-09-01T20-22
Command: a substitution pass over the generated documents under the feature evidence tree, followed by an absence sweep and an XML well-formedness check over **every file** under `docs/features/active/2026-08-28-qfc-initializewebviewasync-fault-is-unobserved-670/evidence/`
EXIT_CODE: 0

## Scope

This sweep covers **52 files** — the whole evidence tree, not only the Phase 4 artifacts. The wider scope is required rather than tidy, for two distinct reasons.

**First, documents written after P3-T14 exist.** `evidence/qa-gates/postchange.cobertura.xml` was written in Phase 4, after the P3-T14 pass had already run, so no earlier sanitisation covered it. The `evidence/baseline/baseline.normalized.cobertura.xml` and `evidence/qa-gates/postchange.normalized.cobertura.xml` documents that P4-T8 may write fall in the same category; neither exists in this run, because the two `POSTPROCESSED` flags agreed and no normalization was required.

**Second, this is the whole-tree re-sweep over the Phase 0 through Phase 3 artifacts.** Those were gated at capture time precisely because P3-T15 commits them before any Phase 4 task runs, and sanitisation in place cannot recover a literal that is already committed.

## Substitution counts

The substitution pass over the generated documents reported **zero** substitutions for every one:

| File | Substitutions |
| --- | --- |
| `evidence/baseline/baseline.cobertura.xml` | 0 |
| `evidence/qa-gates/postchange.cobertura.xml` | 0 |
| `evidence/regression-testing/p3-t4-green.trx` | 0 |
| `evidence/regression-testing/p3-t5-red.trx` | 0 |
| `evidence/regression-testing/p3-t10-new-tests.trx` | 0 |
| `evidence/regression-testing/p3-t11-pinned.trx` | 0 |

The four `.trx` documents report zero here because P3-T14 already substituted them (10, 11, 19 and 16 substitutions respectively). `postchange.cobertura.xml` reports zero because the runner's Koverage post-processing had already rewritten its filenames to repository-relative form before it was copied, the same reason `baseline.cobertura.xml` needed none.

## The three capture-time-gated artifacts

`evidence/baseline/p0-t2-sdk-bootstrap.md`, `evidence/baseline/p0-t3-nuget-restore.md` and `evidence/baseline/p0-t5-tool-restore.md` each carry a capture-time rewrite instruction and a capture-time zero-sweep gate of their own. This task sweeps all three and **expects zero substitutions in each**.

| File | Substitutions | Sweep hits |
| --- | --- | --- |
| `evidence/baseline/p0-t2-sdk-bootstrap.md` | 0 | 0 |
| `evidence/baseline/p0-t3-nuget-restore.md` | 0 | 0 |
| `evidence/baseline/p0-t5-tool-restore.md` | 0 | 0 |

All three are zero, so **no discrepancy is recorded**. A non-zero count in any of the three would have meant that task's capture-time gate had been recorded as passing while a host literal was still present, and would have been reported here rather than silently absorbed — the literal would by then already be inside the P3-T15 commit and beyond the reach of in-place repair.

## Verification 1 — absence sweep over every file

Each of the 52 files was swept case-insensitively for every run-time-derived token and, additionally, for the generic drive-qualified user-profile root and the generic drive-qualified Program Files root, each in both separator spellings. Those four generic patterns are described by name and are deliberately **not** quoted, because an artifact that quotes a sweep pattern is matched by any later pass over that artifact and would make this condition unsatisfiable.

**Every one of the 52 files returned a sweep count of 0.**

## Verification 2 — XML well-formedness

Every Cobertura and TRX document under the tree still parses under `[xml](Get-Content -LiteralPath $p -Raw)`:

| Document | Parse |
| --- | --- |
| `evidence/baseline/baseline.cobertura.xml` | OK |
| `evidence/qa-gates/postchange.cobertura.xml` | OK |
| `evidence/regression-testing/p3-t4-green.trx` | OK |
| `evidence/regression-testing/p3-t5-red.trx` | OK |
| `evidence/regression-testing/p3-t10-new-tests.trx` | OK |
| `evidence/regression-testing/p3-t11-pinned.trx` | OK |

No `.normalized.cobertura.xml` document is present, because P4-T8 required no normalization.

Both verifications were run over every file rather than one being taken as evidence for the other. An absence assertion and a validity assertion fail on disjoint inputs: a document can be swept clean and left unparseable, or left parseable with a literal still in it. Passing one is not passing the other.

The parse check is discriminating, verified directly in P3-T14: a placeholder written with raw angle brackets inside an XML attribute fails to parse, while the same placeholder written XML-escaped parses. That is why the placeholders are XML-escaped inside the `.trx` and `.xml` documents.

## Timing

This sweep runs **before** P4-T29 stages anything, so no unsanitised content can enter the final commit.

No pre-substitution value and no quoted sweep literal appears anywhere in this artifact.
