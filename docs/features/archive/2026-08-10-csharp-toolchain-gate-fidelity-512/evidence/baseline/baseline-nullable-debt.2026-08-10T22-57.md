# Baseline — DEBT-PROBE: the nullable debt the corrected gate leaves un-enforced ([P0-T13], AC12)

Timestamp: 2026-08-10T22-57
Command: `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true /p:Nullable=enable /nologo /v:m /fl "/flp:logfile=coverage/baseline-nullable-debt.log;verbosity=normal"`
EXIT_CODE: 1

DEBT-PROBE is TYPECHECK **plus** `/p:Nullable=enable`. It is a **measurement only**. It is not a
command this feature adopts, and the diagnostics it reports are **not fixed here** — the burn-down is
issue #492 and a follow-on epic, explicitly out of scope.

Invoked via `pwsh -NoProfile -ExecutionPolicy Bypass -File coverage/run-nullable-debt.ps1`.

## Headline figure

| Metric | Value |
|---|---|
| `EXIT_CODE` | **1** |
| MSBuild `N Error(s)` summary line | **`195 Error(s)`** |
| MSBuild `N Warning(s)` summary line | `0 Warning(s)` |
| Elapsed | 4.3 s |
| `Skipping target "CoreCompile"` count | **0** (the failure is genuine, not vacuous) |
| `CoreCompile:` header-line count | **22** |

**The authoritative figure is MSBuild's own `195 Error(s)` summary line**, corroborated by a count
restricted to node-prefixed diagnostic lines (`^\s*\d+>.*error CS\d+`), which also returns **195**.

**Counting trap avoided.** A naive `Select-String 'error CS'` over the same log returns **390** —
exactly twice 195 — because each error prints once inline with a node-id prefix and once again in the
terminal error-summary block. That double count is recorded here explicitly so it is not repeated:

```
NAIVE_ERROR_CS_LINE_COUNT: 390
NODE_PREFIXED_ERROR_COUNT: 195
MSBuild summary:            195 Error(s)
```

## Per-diagnostic `CS86xx` breakdown

| Diagnostic | Count |
|---|---|
| CS8766 | 130 |
| CS8618 | 23 |
| CS8625 | 12 |
| CS8600 | 9 |
| CS8601 | 8 |
| CS8604 | 7 |
| CS8602 | 3 |
| CS8603 | 2 |
| CS8714 | 1 |
| **Total** | **195** |

This reproduces issue #492's per-diagnostic breakdown exactly.

## Owning `.csproj` attribution

| Project | Errors |
|---|---|
| `UtilitiesCS.csproj` | **195** |
| all other projects | 0 |

Every one of the 195 diagnostics is attributed to `UtilitiesCS.csproj`. Sample diagnostic, quoted
verbatim from the log:

```
9>...\UtilitiesCS\EmailIntelligence\EmailParsingSorting\MovedMailInfo.cs(45,13): error CS8766: Nullability of reference types in return type of 'string? MovedMailInfo.EntryId.get' doesn't match implicitly implemented member 'string IMovedMailInfo.EntryId.get' (possibly because of nullability attributes). [...\UtilitiesCS\UtilitiesCS.csproj]
```

## The figure is a LOWER BOUND, not a solution-wide total

**195 is `>=`, not `=`.** This run executed **22** `CoreCompile` headers before aborting, against
**73** in the fully green ANALYZE rebuild recorded in
`baseline-analyze-rebuild.2026-08-10T22-54.md`. `UtilitiesCS` is a foundational dependency; once it
failed, its dependents were never compiled and their nullable diagnostics were never counted. The
solution-wide figure under `/p:Nullable=enable` is therefore **`>= 195` and unmeasured**.

Sizing the follow-on burn-down epic must begin by measuring the solution-wide figure, which requires
either fixing `UtilitiesCS` first or building with per-project `ContinueOnError` semantics. That
measurement is deliberately **not** performed here.

This lower-bound qualification also explains the historical disagreement in the record (195 vs 220 vs
~414, and `UtilitiesCS.csproj` vs `TaskMaster.csproj`): sessions whose builds aborted at different
points report different totals and attribute them to different projects, and a session counting
naively reports double.

## Why this measurement does not change the adopted gate

The adopted TYPECHECK command omits `/p:Nullable=enable`, matching `.github/workflows/ci.yml`
character-for-character. Removing the flag loses no enforcement over any file that carries a
`#nullable enable` pragma — proven independently by the negative control in [P5-T5], where the
corrected command still returns exit 1 on a nullable violation introduced into an opted-in file. The
195 diagnostics above originate in `UtilitiesCS` files that have **never** opted in.

## Build-output consequence

This run used `/t:Rebuild` and failed, so MSBuild issued `Clean` to every project before the first
`CoreCompile`. Every project's `bin`/`obj` is now deleted or incomplete. [P0-T14] is the mandatory
restoration build and runs immediately after this task.

## Output Summary

DEBT-PROBE returns `EXIT_CODE: 1` with MSBuild reporting **195 Error(s)**, all attributed to
`UtilitiesCS.csproj`, distributed CS8766 x130 / CS8618 x23 / CS8625 x12 / CS8600 x9 / CS8601 x8 /
CS8604 x7 / CS8602 x3 / CS8603 x2 / CS8714 x1, with a zero `CoreCompile` skip count (the failure is
genuine) and 22 of 73 `CoreCompile` headers executed before the build aborted. The figure is recorded
as a **lower bound** for the follow-on burn-down epic and is **not fixed in this feature**.
