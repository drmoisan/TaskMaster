# Phase 2 File-Size Audit — Remediation Cycle 1, Issue #911

- Timestamp: 2026-09-20T08-54-54
- Task: [P2-T10]
- Finding: R9d
- EXIT_CODE: 0

## Measurements

```
([System.IO.File]::ReadAllLines((Resolve-Path <path>).ProviderPath)).Count
```

| # | Path | [P0-T5] baseline | After Phase 2 | Delta | At most 500 |
|---|---|---|---|---|---|
| 1 | `scripts/dependencies/Repair-PackageManifestConsistency.ps1` | 498 | **470** | **-28** | yes, 30 spare |
| 2 | `scripts/dependencies/ConsistencyVerifier.psm1` | 493 | **499** | +6 | yes, 1 spare |
| 3 | `scripts/dependencies/ProjectConsistency.psm1` | 331 | **373** | **+42** | yes, 127 spare |
| 4 | `tests/scripts/dependencies/ConsistencyVerifier.Tests.ps1` | 275 | **312** | +37 | yes, 188 spare |
| 5 | `tests/scripts/dependencies/DependabotConfig.Tests.ps1` | 335 | **364** | +29 | yes, 136 spare |

Exactly 5 counts recorded. Every one is at most 500.

## The Two-Sided Check on the Move

| Clause | Required | Measured | Result |
|---|---|---|---|
| Composition root strictly **less** than its [P0-T5] value | < 498 | **470** | PASS |
| `ProjectConsistency.psm1` strictly **greater** than its [P0-T5] value | > 331 | **373** | PASS |

The pair is the check. A size audit that only bounds above is satisfied by a move that never
happened: a file that stayed at 498 and a file that stayed at 331 both pass a `<= 500` clause.
The composition root fell by 28 and the module rose by 42, so the function left one file and
arrived in the other.

The composition root's net `-28` is the 36-line function removal against the 8 lines Phase 2 added
to it: the two-line R9b call-site comment from [P2-T5] and the six-line verbose record and its
comment from [P2-T6].

## R9d Status After Phase 2

R9d reported two files "within two and seven lines of the 500-line cap" and asked that the next
addition **extract rather than append**.

| File | At the review | Now | R9d state |
|---|---|---|---|
| `Repair-PackageManifestConsistency.ps1` | 498, 2 lines spare | **470**, 30 spare | improved by the extraction |
| `ConsistencyVerifier.psm1` | 493, 7 lines spare | **499**, 1 spare | tighter |

The composition root is the file decision D1 extracted from, and it now has 30 lines of headroom
where it had 2.

**`ConsistencyVerifier.psm1` is tighter than the review found it**, at 499 of 500. The six lines
are the R5 fix: one resolver call, one rationale comment and four `.DESCRIPTION` lines, all of
which [P2-T3] had to compress from a first form that reached 510. This is recorded as a standing
observation: the next addition to that file must extract rather than append, and extracting a
second function is a new independent outcome this plan does not describe and this cycle does not
perform.

## Output Summary

Five counts, all at most 500. The composition root moved from 498 down to 470 and
`ProjectConsistency.psm1` from 331 up to 373, so the two-sided move check holds.
`ConsistencyVerifier.psm1` sits at 499 with one line of headroom, which is recorded as an
observation for the next author.
