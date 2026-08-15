# AC12 — consolidated nullable-debt record for the follow-on burn-down epic ([P5-T16])

Timestamp: 2026-08-11T00-30
Command: (none — analysis artifact)
EXIT_CODE: (none — analysis artifact)

Sourced from `FEATURE/evidence/baseline/baseline-nullable-debt.2026-08-10T22-57.md` ([P0-T13]), where
the measurement was executed.

## The measurement command

```
msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true /p:Nullable=enable /nologo /v:m /fl "/flp:logfile=coverage/baseline-nullable-debt.log;verbosity=normal"
```

This is the adopted TYPECHECK command **plus** `/p:Nullable=enable`. It is a measurement probe only.
It is **not** a command this feature adopts at any documented site.

`EXIT_CODE: 1`.

## Measured figure

**MSBuild's own `N Error(s)` summary line: `195 Error(s)`.**

Corroborated by a count restricted to node-prefixed diagnostic lines (`^\s*\d+>.*error CS\d+`), which
also returns **195**.

**Counting method matters.** A naive `Select-String 'error CS'` over the same log returns **390** —
exactly twice 195 — because each error prints once inline with a node-id prefix and once again in the
terminal error-summary block. The naive figure is **not** used.

## Per-diagnostic `CS86xx` breakdown

| Diagnostic | Count |
|---|---|
| CS8766 (nullability mismatch in an implemented member's return type) | 130 |
| CS8618 (non-nullable field/property uninitialized) | 23 |
| CS8625 (null literal to a non-nullable reference) | 12 |
| CS8600 (converting null literal or possible null to a non-nullable type) | 9 |
| CS8601 (possible null reference assignment) | 8 |
| CS8604 (possible null reference argument) | 7 |
| CS8602 (dereference of a possibly null reference) | 3 |
| CS8603 (possible null reference return) | 2 |
| CS8714 (type cannot be used as a `notnull`-constrained type parameter) | 1 |
| **Total** | **195** |

## Owning-project attribution

| Project | Errors |
|---|---|
| **`UtilitiesCS.csproj`** | **195** |
| every other project | 0 |

Representative diagnostic, verbatim:

```
9>...\UtilitiesCS\EmailIntelligence\EmailParsingSorting\MovedMailInfo.cs(45,13): error CS8766: Nullability of reference types in return type of 'string? MovedMailInfo.EntryId.get' doesn't match implicitly implemented member 'string IMovedMailInfo.EntryId.get' (possibly because of nullability attributes). [...\UtilitiesCS\UtilitiesCS.csproj]
```

## Explicit lower-bound qualification

**195 is a lower bound, not a solution-wide total. The solution-wide figure is `>= 195` and
unmeasured.**

| Run | `CoreCompile:` headers executed |
|---|---|
| DEBT-PROBE ([P0-T13], failed) | **22** |
| ANALYZE full green rebuild ([P0-T12]) | **73** |

The build **aborts before dependents compile**. `UtilitiesCS` is a foundational dependency; once it
failed, its dependents were never compiled and their nullable diagnostics were never counted. The
`Skipping target "CoreCompile"` count for the DEBT-PROBE log is **0**, so the truncation is caused by
the failure, not by an incremental skip.

This qualification also explains the historical disagreement in the record (195 vs 220 vs ~414
errors; `UtilitiesCS.csproj` vs `TaskMaster.csproj` attribution): sessions whose builds aborted at
different points report different totals and attribute them to different projects, and a session
counting naively reports double.

**Consequence for sizing the follow-on epic.** Sizing must begin by measuring the solution-wide
figure, which requires either fixing `UtilitiesCS` first or building with per-project
`ContinueOnError` semantics. That measurement is deliberately **not** performed here.

## These diagnostics are NOT fixed in this feature

Fixing the nullable diagnostics is an explicit **Non-Goal** of this feature and of the parent epic.
Issue #492 states the separation: first make the gate report truthfully, then decide how to burn down
the debt. Only the first half is delivered here. The figure above is **recorded, not resolved**, and
is the input to the follow-on burn-down epic.

The adopted TYPECHECK command omits `/p:Nullable=enable` and therefore does not enforce against these
195 diagnostics. That is not a loss of enforcement relative to the merge-base state: the documented
gate never compiled at all ([P0-T11]: exit 0 in 1.8 s, 18 of 18 skips), and the corrected gate still
catches nullable violations in files that have opted in via `#nullable enable` ([P5-T5]: `EXIT_CODE:
1`, `error CS8603`).

## Output Summary

The corrected gate leaves **195** `CS86xx` diagnostics un-enforced, measured 2026-08-10 at this
branch head from MSBuild's own `195 Error(s)` summary line, **all attributed to
`UtilitiesCS.csproj`**, distributed CS8766 x130 / CS8618 x23 / CS8625 x12 / CS8600 x9 / CS8601 x8 /
CS8604 x7 / CS8602 x3 / CS8603 x2 / CS8714 x1. The figure is an explicit **lower bound** (`>= 195`,
solution-wide total unmeasured) because the build aborted after 22 of 73 `CoreCompile` executions.
They are **not fixed in this feature**; the figure is handed to the follow-on burn-down epic.
