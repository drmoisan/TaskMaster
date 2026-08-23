# P5-T8 — Coverage Comparison (AC-19)

Timestamp: 2026-08-08T21-39

Sources:

- Baseline: `<FEATURE>\evidence\baseline\tests-with-coverage.2026-08-08T20-44.md` (P0-T9), raw
  document `coverage\coverage-baseline-505.cobertura.xml`.
- Final: `<FEATURE>\evidence\qa-gates\tests-with-coverage.2026-08-08T21-37.md` (P5-T6), raw
  document `coverage\coverage-final-505.cobertura.xml`.
- Per-type figures: `<FEATURE>\evidence\qa-gates\new-type-coverage.2026-08-08T21-38.md` (P5-T7).

Both documents were produced by the identical command and the identical post-processing pipeline,
and both were read with the identical query method, so the comparison is like-for-like.

## 1. Repo-wide root `<coverage>` attributes

| Attribute | Baseline (P0-T9) | Final (P5-T6) | Delta |
|---|---|---|---|
| `line-rate` | 0.858904 | **0.859190** | **+0.000286** |
| `branch-rate` | 0.793353 | **0.793602** | **+0.000249** |
| `lines-covered` | 95706 | **95993** | **+287** |
| `lines-valid` | 111428 | **111725** | **+297** |
| `branches-covered` | 22225 | 22278 | +53 |
| `branches-valid` | 28014 | 28072 | +58 |
| Test total | 6399 (6399 passed) | 6435 (6435 passed) | +36 |

The denominator grew by 297 lines — the two new non-exempt production files — and 287 of those are
covered. Both rates moved slightly **up**. There is no repo-wide regression.

## 2. Per-file figures for the section 4 scope-locked `.cs` paths

| Path | Baseline | Final | Assessment |
|---|---|---|---|
| `TaskMaster\Ribbon\EngineCommandCatalog.cs` | `line-rate=1`, 24/24 lines | **`line-rate=1`, 37/37 lines** | No regression. The file grew by 13 measured lines (the six new `Map` entries plus the explanatory comment) and every one is covered. |
| `TaskMaster\Ribbon\EngineToggleCatalog.cs` | absent (new file) | **`line-rate=1`, 18/18** | New code, 100%. |
| `TaskMaster\Ribbon\EngineToggleStateCoordinator.cs` | absent (new file) | **`line-rate=0.991489`, 133/135** | New code, above the 0.90 floor. |
| `TaskMaster\Ribbon\RibbonController.EngineCommands.cs` | absent | absent | Partial of the `[ExcludeFromCodeCoverage]` `RibbonController`; see section 3. |
| `TaskMaster\Ribbon\RibbonViewer.EngineCommands.cs` | absent | absent | Partial of the `[ExcludeFromCodeCoverage]` `RibbonViewer`; see section 3. |
| The five scope-locked test files | not in document | not in document | `Invoke-MSTestWithCoverage.ps1` strips test packages from the Cobertura output by design; test code is correctly outside the coverage denominator. |

### Changed-line no-regression statement

**No changed line lost coverage.** Explicitly, for every `.cs` path in the section 4 scope lock:

- `EngineCommandCatalog.cs` was at `line-rate=1` before and is at `line-rate=1` after; its added
  lines are covered by the six new `EngineCommandCatalogTests` data rows and the extended
  `ControlIds_ContainsExactlyTheFourteenEngineBackedControlIds` set.
- `EngineToggleCatalog.cs` and `EngineToggleStateCoordinator.cs` are new; their changed lines are
  the whole file, measured at 1.000000 and 0.991489.
- `RibbonController.EngineCommands.cs` and `RibbonViewer.EngineCommands.cs` were outside the
  measured denominator before the change and remain outside it after; the exemption was neither
  removed nor widened (P4-T4), so no line moved from covered to uncovered.
- The five test files are outside the denominator by pipeline design, before and after.

## 3. The `[ExcludeFromCodeCoverage]` exemption — the plan's caution, and its empirical resolution

**The plan's a-priori caution, recorded as required.** The modified handlers live in type-level
`[ExcludeFromCodeCoverage]` classes (`RibbonViewer.cs:32`, `RibbonController.cs:36`) under the
ratified VSTO/COM ribbon-handler exemption, so a flat repo-wide figure is *expected* and is not a
regression. However, `coverage.config` supplies a custom `<CodeCoverage>` block containing only
`<ModulePaths>`, and a custom block can displace the default `<Attributes>` excludes and silently
stop honoring `[ExcludeFromCodeCoverage]`. That risk means the exemption narrative had to be
treated as **expected-but-unverified** rather than asserted.

**Empirical resolution.** The risk was probed directly against the final document rather than left
open. Querying `//class[@filename]` for the two exempt types returns **absent** for both:

```
TaskMaster/Ribbon/RibbonViewer.cs      in final document: absent
TaskMaster/Ribbon/RibbonController.cs  in final document: absent
```

Both partials of each exempt type (`RibbonController.EngineCommands.cs`,
`RibbonViewer.EngineCommands.cs`) are likewise absent. The exempt types are therefore **not** in
the coverage denominator, so `[ExcludeFromCodeCoverage]` **is** being honored despite the custom
`<CodeCoverage>` block. The full block, for the record:

```xml
<Configuration>
  <CodeCoverage>
    <ModulePaths>
      <Exclude>
        <ModulePath>.*Deedle.*</ModulePath>
        <ModulePath>.*FSharp.*</ModulePath>
        <ModulePath>.*Castle\.Core.*</ModulePath>
        <ModulePath>.*FluentAssertions.*</ModulePath>
        <ModulePath>.*Moq.*</ModulePath>
        <ModulePath>.*Microsoft\.Testing.*</ModulePath>
        <ModulePath>.*MSTest.*</ModulePath>
      </Exclude>
    </ModulePaths>
  </CodeCoverage>
</Configuration>
```

It excludes only third-party modules and adds no `<Attributes>` element, which is consistent with
the observed attribute-honoring behavior.

This uncertainty never affected the P5-T7 binary gate in any case: both target files are
**non-exempt** and are measured directly.

## 4. Repo-wide figure: record-and-report, not an independent floor

The repo-wide figure is a **record-and-report** obligation against the `CLAUDE.md` § UT2 testable
denominator, not an independent numeric floor imposed by this bug fix. Any shortfall against the
80% repo-wide target is pre-existing debt and is non-blocking for this delivery, per the #424
precedent. As measured, the figure is **0.859190**, above the `CLAUDE.md` § UT2 80% target and
slightly improved over the baseline, so no shortfall arises.

The blocking gates for this delivery are:

1. the **changed-line no-regression** requirement — satisfied, section 2; and
2. the **0.90 new-code floor** from P5-T7 — satisfied at 0.991489 and 1.000000.

## 5. Recorded policy conflict (not silently resolved)

Two repository documents state different coverage thresholds:

| Source | Threshold |
|---|---|
| `CLAUDE.md` § UT2 / `.claude/rules/csharp.md` | line coverage **>= 80%** repo-wide (on the testable denominator, with the ratified COM/VSTO/WinForms exemption); **>= 90%** for any new module, class, or method |
| `.claude/rules/general-unit-test.md` / `.claude/rules/quality-tiers.md` | line coverage **>= 85%**, branch coverage **>= 75%**, uniform across tiers T1-T4 |

This is an **unresolved policy conflict** and is recorded here rather than silently resolved in
either direction. It is noted only that the measured figures happen to clear **both** readings:
line-rate 0.859190 >= 0.85 and >= 0.80; branch-rate 0.793602 >= 0.75; both new files >= 0.90.
Reconciling the two documents is outside the scope of #505/#506/#518.

## 6. Evidence hygiene

Neither raw Cobertura document is committed. Both live under the gitignored `coverage\` directory
(`.gitignore` `coverage/*`); the committed record is the numeric headline values and the
class-level summaries in this artifact and in P0-T9, P5-T6, and P5-T7. `artifacts\csharp\coverage.xml`
was deliberately **not** created (plan rule 9).

Binary outcome: PASS.
