# Phase 0 — Numeric Coverage Baseline (Issue #445)

Timestamp: 2026-08-22T09-34

Command:
```powershell
& dotnet-coverage collect --output coverage\baseline.cobertura.xml --output-format cobertura --settings coverage\effective-coverage.config -- 'C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe' @assemblies '/Settings:scripts\vscode\TaskMaster.cli.runsettings' /InIsolation '/TestCaseFilter:TestCategory!=LiveOutlook'
$cov = ([xml](Get-Content -Raw coverage\baseline.cobertura.xml)).coverage
$cov.'line-rate'; $cov.'branch-rate'; $cov.'lines-covered'; $cov.'lines-valid'; $cov.'branches-covered'; $cov.'branches-valid'
```
with `@assemblies` the 9-element relative-path list from P0-T14. Run from `WS` via `pwsh -NoProfile`. The outer `dotnet-coverage` supplies instrumentation, so the inner vstest invocation deliberately omits `/EnableCodeCoverage`; `/InIsolation` is still present.

EXIT_CODE: 0

## Inner test-run result

```
ASSEMBLY_COUNT=9
Test Run Successful.
Total tests: 6437
     Passed: 6437
```

Failed 0, Skipped 0. The instrumented run reproduces the P0-T15 totals exactly, so instrumentation did not perturb any test.

## Repository-wide numeric figures (verbatim attribute values)

| Field | Raw value | As percentage |
|---|---|---|
| `line-rate` | `0.7059714463066419` | **70.60%** |
| `branch-rate` | `0.5874059746400172` | **58.74%** |
| `lines-covered` | `56866` | — |
| `lines-valid` | `80550` | — |
| `branches-covered` | `13666` | — |
| `branches-valid` | `23265` | — |

## Per-file covered/total line counts for the four in-scope production files

Aggregated across EVERY Cobertura `class` element whose `filename` ends with the target name, then deduplicated by line number (taking the maximum `hits` per line). Aggregation by `filename` is required because a compiler-generated state machine or closure appears as a separate `class` element; measuring one element alone would understate the file. Deduplication is required because dotnet-coverage emits each line both under its `method` and, where present, under the class-level `lines` block.

| File | `class` elements | Class names | Covered | Total | Percentage |
|---|---|---|---|---|---|
| `KaStringAsync.cs` | 1 | `QuickFiler.Controllers.KaStringAsync` | 49 | 49 | **100.00%** |
| `KaChar.cs` | 2 | `QuickFiler.Controllers.KaChar`, `QuickFiler.Controllers.KaCharAsync` | 28 | 33 | **84.85%** |
| `KaKey.cs` | 2 | `QuickFiler.Controllers.KaKey`, `QuickFiler.Controllers.KaKeyAsync` | 28 | 33 | **84.85%** |
| `IKbdAction.cs` | 0 | (none) | 0 | 0 | not measurable |

### Uncovered lines, and why they matter to this issue

`KaStringAsync.cs`: no uncovered line. The file is fully covered at baseline, which sets the no-regression bar at 100 percent for every pre-existing line.

`KaChar.cs` uncovered lines: `45, 53, 54, 95, 96`
`KaKey.cs` uncovered lines: `45, 53, 54, 95, 96`

Mapped against the source as read in this worktree, these are exactly the dead members Phase 3 deletes:

- `:45` — the `DelegateType` getter body (`get => typeof(Action<Keys>);`), deleted by P3-T1 / P3-T5.
- `:53`, `:54` — the first type's dead `Update` getter and setter, deleted by P3-T3 / P3-T6.
- `:95`, `:96` — the `*Async` type's dead `Update` getter and setter, deleted by P3-T4 / P3-T7.

Every uncovered line in both files is a line this plan removes. Deleting an uncovered line raises the file's rate without adding a test, so `KaChar.cs` and `KaKey.cs` are each expected to reach 33 - 5 = 28 valid lines with 28 covered, i.e. 100 percent, at P5-T7. This is a genuine improvement in the metric and not a measurement artefact: the deleted members had zero read sites repository-wide.

`IKbdAction.cs` produces zero `class` elements. It is an interface-only file with no executable body, so it legitimately contributes nothing to the coverage denominator. `.claude/rules/general-unit-test.md` explicitly recognises "C# interface-only files" as reporting 0 percent executable coverage legitimately. This is a measurement fact, not an exclusion: no `[ExcludeFromCodeCoverage]` attribute was added and `coverage.config` was not modified.

## Threshold position at baseline (reported, not adjudicated)

- CLAUDE.md UT2 / `.claude/rules/csharp.md`: repository-wide line coverage `>= 80%`. Baseline 70.60% is **below** this figure.
- `.claude/rules/general-unit-test.md` / `.claude/rules/quality-tiers.md`: line `>= 85%`, branch `>= 75%`. Baseline 70.60% line and 58.74% branch are **below** both.

Both shortfalls are **pre-existing repository state that this bugfix does not create**, measured before any edit in this plan. Per the plan's Coverage Policy Position the repository-wide figure is reported and tracked but is not a blocking gate for this issue. The blocking gates are the no-regression-on-changed-lines condition and the `>= 90%` figure on newly added production lines, both evaluated in P5-T8.

## Settings-file note

`coverage\effective-coverage.config` (P0-T17) was used, not the canonical `coverage.config`. It preserves the seven third-party module exclusions verbatim and adds one entry, `.*\.Test\.dll$`, so the nine test assemblies are outside the denominator per CLAUDE.md UT2. `coverage.config` itself reports 0 dirty lines under `git status --porcelain`. `scripts\vscode\Invoke-MSTestWithCoverage.ps1` was deliberately NOT used as the runner, because its `Assert-CoberturaLineCoverageThreshold` helper throws when repository-wide line coverage is below 80 percent — which, as measured above, it is — and that throw happens before the Cobertura output is written, which would have left this baseline with no numeric figure at all.

Output Summary: Repository-wide baseline is line-rate 0.7059714463066419 (70.60%), branch-rate 0.5874059746400172 (58.74%), lines-covered 56866, lines-valid 80550, branches-covered 13666, branches-valid 23265. Per-file baselines aggregated by Cobertura `filename` are `KaStringAsync.cs` 49/49 (100.00%), `KaChar.cs` 28/33 (84.85%), `KaKey.cs` 28/33 (84.85%), and `IKbdAction.cs` 0/0 (interface-only, no executable line, legitimately not measurable). All ten uncovered lines across the two `Ka*` files (`45, 53, 54, 95, 96` in each) are precisely the dead `DelegateType` and `Update` members Phase 3 deletes, so both files are expected to reach 28/28 (100%) after the change. The instrumented run reproduced the P0-T15 result exactly: 6437 Passed, 0 Failed, 0 Skipped. Repository-wide figures are below both threshold sets (80% and 85%/75%); that shortfall is pre-existing, is reported rather than resolved, and is not a blocking gate for this bugfix. No value in this artifact is `UNVERIFIED`.
