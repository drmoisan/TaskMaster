# P3-T14 — Sanitisation of generated evidence artifacts

Timestamp: 2026-09-01T20-10
Command: a run-time-derived, case-insensitive, longest-token-first substitution pass over the generated `.trx` and `.xml` artifacts under the feature evidence tree, followed by two independent verification passes — a fixed-string absence sweep and an XML well-formedness check
EXIT_CODE: 0

## Files in scope, with per-file substitution counts

| File | Substitutions applied |
| --- | --- |
| `evidence/regression-testing/p3-t4-green.trx` | 10 |
| `evidence/regression-testing/p3-t5-red.trx` | 11 |
| `evidence/regression-testing/p3-t10-new-tests.trx` | 19 |
| `evidence/baseline/baseline.cobertura.xml` | 0 |

`evidence/regression-testing/p3-t11-pinned.trx` was also produced in Phase 3 and was swept in the same pass, receiving 16 substitutions.

The Cobertura document required no substitution because the runner's Koverage post-processing step had already rewritten its filenames to repository-relative form before it was copied. Its zero count is therefore a genuine measurement rather than a skipped file: it was read, scanned against every derived token, and found clean.

## Derivation of the substitution tokens

The tokens are derived at run time from `$PWD`, `${env:ProgramFiles}`, `${env:ProgramFiles(x86)}`, `$env:USERPROFILE`, `$env:COMPUTERNAME` and `$env:USERNAME`, per the token list in the plan's section 0. They are not written into the plan and are not recorded here, so neither the plan file nor this artifact quotes the values it removes.

Three properties of the pass are load-bearing:

- **Case-insensitive.** Path casing is not preserved consistently across the tools that wrote these documents, so a case-sensitive pass would leave variants behind.
- **Longest token first.** Both Program Files roots map to the single placeholder `<program-files>`, and the ordering is what makes the `(x86)` root substitute before its shorter prefix. Without it the shorter root would match first and leave a stray ` (x86)` fragment.
- **Both separator spellings.** Each backslash-bearing token is expanded into a forward-slash variant as well, because the documents mix the two.

Both Program Files roots are in the derivation set because the `.trx` documents record the resolved `vstest.console.exe` path, which is an absolute host path under the same obligation as any other.

## Verification 1 — absence sweep

For each file, a case-insensitive fixed-string sweep was run for every derived token and, additionally, for the generic drive-qualified user-profile root and the generic drive-qualified Program Files root, each in both separator spellings. Those four generic patterns are described here by name rather than quoted, because P4-T28 sweeps this artifact and a quoted pattern would match itself and make that condition unsatisfiable.

| File | Sweep hits |
| --- | --- |
| `evidence/baseline/baseline.cobertura.xml` | 0 |
| `evidence/regression-testing/p3-t4-green.trx` | 0 |
| `evidence/regression-testing/p3-t5-red.trx` | 0 |
| `evidence/regression-testing/p3-t10-new-tests.trx` | 0 |
| `evidence/regression-testing/p3-t11-pinned.trx` | 0 |

All zero.

## Verification 2 — XML well-formedness

    [xml](Get-Content -LiteralPath $p -Raw)

| File | Parse |
| --- | --- |
| `evidence/baseline/baseline.cobertura.xml` | OK |
| `evidence/regression-testing/p3-t4-green.trx` | OK |
| `evidence/regression-testing/p3-t5-red.trx` | OK |
| `evidence/regression-testing/p3-t10-new-tests.trx` | OK |
| `evidence/regression-testing/p3-t11-pinned.trx` | OK |

Every document still parses, so no substitution corrupted a document.

## Why both verifications are required

An absence assertion and a validity assertion fail on **disjoint** inputs, so passing one is not passing the other. A document could be swept clean and left unparseable, or left perfectly parseable with a host literal still in it. Both were therefore run over every file.

The specific hazard here is concrete rather than theoretical. The test runner writes host paths into XML **attribute** values, and a raw angle-bracketed placeholder inside an attribute makes the document unparseable. The placeholders were therefore written XML-escaped. That this is a real hazard and not a precaution was verified directly:

    a placeholder written with raw angle brackets inside an attribute   →  parse FAILED
    the same placeholder written XML-escaped inside an attribute        →  parse OK

So the parse check is discriminating: it would have caught a naive substitution, and the escaped form is what makes both checks pass simultaneously.

## Timing

This sanitisation runs **before** P3-T15 stages anything. Sanitisation in place cannot recover a literal that has already been committed, and the only later whole-tree sweep is P4-T28 in Phase 4, which is after the Phase 3 commit. Deferring this pass would therefore have left host identifiers in an intermediate commit.

No pre-substitution value is recorded anywhere in this artifact, because an artifact that quotes what it removed reintroduces the leak it was written to close.
