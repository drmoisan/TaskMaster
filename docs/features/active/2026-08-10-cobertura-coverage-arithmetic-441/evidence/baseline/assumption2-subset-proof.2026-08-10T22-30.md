# Assumption 2 Subset Proof (P0-T18)

Timestamp: 2026-08-10T22-30

Proves `spec.md` § Assumptions item 2 — *method-level line numbers are a subset of the class-level
rollup* — exhaustively on both committed sample documents, **before** any implementation change, so
that document drift fails at baseline rather than after Phase 2.

`spec.md` records this assumption as verified on three spot-checked classes only and explicitly
`UNVERIFIED` across all classes, because exhaustive proof requires script execution. This artifact
supplies that execution.

Method: a **read-only streaming `XmlReader` pass** (not an `[xml]` cast — the inputs are 17.5 MB and
10.4 MB) that applies, per `<class>`, exactly the union and `max(hits)` rule this plan specifies:
enumerate both the class-level `./lines/line` axis and the `./methods/method/lines/line` axis, key
by `[int]number`, resolve repeats by `max(hits)` / `branch=True` if either / `condition-coverage`
from the larger `Total` tie-broken by larger `Covered`. A **method-only line key** is a key observed
on the method axis and never on the class-level axis.

Command:

```powershell
$root = (git rev-parse --show-toplevel) -replace '/', '\'
& <scratchpad>\Test-Assumption2.ps1 -Path (Join-Path $root 'docs\features\active\2026-08-06-quickfiler-high-confidence-queue-init-stall-424\evidence\baseline\coverage-baseline.cobertura.xml')
& <scratchpad>\Test-Assumption2.ps1 -Path (Join-Path $root 'docs\features\active\2026-08-06-quickfiler-high-confidence-queue-init-stall-424\evidence\qa-gates\coverage-final.cobertura.xml')
```

The analysis script is a throwaway agent-session script held outside the repository (scratchpad); it
writes nothing, mutates nothing, and is not part of the change. Its full logic is described above.

EXIT_CODE: 0

Output Summary:

```
FILE=coverage-baseline.cobertura.xml
classes=3169
class-level distinct=79957
union distinct=79957
union covered=56124
union branches valid=23109
union branches covered=13472
method-only line keys=0

FILE=coverage-final.cobertura.xml
classes=534
class-level distinct=62345
union distinct=62345
union covered=53013
union branches valid=15828
union branches covered=12445
method-only line keys=0
```

## `coverage-baseline.cobertura.xml` (raw generator output, 3169 classes)

| Quantity | Required by P0-T18 | Measured | Match |
| --- | --- | --- | --- |
| class-level distinct | 79957 | **79957** | yes |
| union distinct | 79957 | **79957** | yes |
| union covered | 56124 | **56124** | yes |
| union branches valid | 23109 | **23109** | yes |
| union branches covered | 13472 | **13472** | yes |
| **method-only line keys** | **0** | **0** | yes |

## `coverage-final.cobertura.xml` (post-processed, 534 classes)

| Quantity | Required by P0-T18 | Measured | Match |
| --- | --- | --- | --- |
| class-level distinct | 62345 | **62345** | yes |
| union distinct | 62345 | **62345** | yes |
| union covered | 53013 | **53013** | yes |
| **method-only line keys** | **0** | **0** | yes |

Additionally measured (not required by P0-T18, recorded as an observation): union branches valid
**15828** and union branches covered **12445**. This falls inside the range `[15730, 16582]` that the
research derived analytically for class-level `branches-valid` on this document, against the emitted
defective value of 27848. It is an independent forward prediction of the post-change branch figures
for this input.

## Verdict

**Assumption 2 holds exhaustively on both documents.** `union distinct == class-level distinct` and
`method-only line keys == 0` on every one of the 3169 + 534 = 3703 classes, so the union design and
the class-level oracle agree everywhere. The union formulation therefore reproduces the generator's
own arithmetic exactly (79957 / 56124 / 23109 / 13472), which is the primary correctness oracle for
AC-1.

No spec-level finding is raised. No plan or spec revision is required. The Phase 5
return-to-Phase-2 loop is not implicated by this task in any case.
