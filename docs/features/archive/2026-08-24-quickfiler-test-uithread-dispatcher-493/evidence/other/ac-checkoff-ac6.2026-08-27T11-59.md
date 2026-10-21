# AC-6 Check-Off (P5-T6)

Timestamp: 2026-08-27T11-59
Task: [P5-T6]
Command: `git diff -- docs/features/active/quickfiler-test-uithread-dispatcher-493/spec.md`
EXIT_CODE: 0
Output Summary: AC-6 ("`QfcItemController.FocusAndThemeTests.cs` is unmodified and unregressed") is
verified on every clause of its own text and is checked off in `spec.md`. `PairsN: 6`,
`PairsNMinus1: 5`, so exactly one further checkbox changed state. One sub-condition inside the
`P4-T2` comparison did **not** hold literally and is stated in full below; it is a defect in a
plan-chosen proxy measurement, not a failure of any clause AC-6 states.

PairsN: 6
PairsNMinus1: 5

`pairs(6) - pairs(5) == 1`. `pairs(5)` is the value recorded by `P5-T5` in
`<FEATURE>/evidence/other/ac-checkoff-ac5.2026-08-27T11-57.md`.

## Cited artifacts, resolved per § Conventions

| Stem | Resolved filename | Purpose |
| --- | --- | --- |
| `unowned-file-identity` | `<FEATURE>/evidence/qa-gates/unowned-file-identity.2026-08-27T11-26.md` | byte-identity |
| `unowned-file-diagnostics-comparison` | `<FEATURE>/evidence/qa-gates/unowned-file-diagnostics-comparison.2026-08-27T11-30.md` | the `P4-T2` comparison |
| `quickfiler-test-run` | `<FEATURE>/evidence/qa-gates/quickfiler-test-run.2026-08-27T11-19.md` | the two named theme tests |

All three exist.

## Baseline diagnostic counts P4-T2 compared against

Repeated here as this task's acceptance condition requires, so a reviewer can see which of the two
cases in § Notes rule 2 held.

| Log | Token | Baseline total match count | Baseline **diagnostic-bearing** count |
| --- | --- | --- | --- |
| analyzer step | `QfcItemController.FocusAndThemeTests.cs` | 2 | **0** |
| analyzer step | `UiThread.cs` | 2 | **0** |
| type-check step | `QfcItemController.FocusAndThemeTests.cs` | 2 | **0** |
| type-check step | `UiThread.cs` | 2 | **0** |

Baseline `AnalyzerStepMatchCount: 4` and `NullableStepMatchCount: 4`, of which **zero** were
diagnostics.

**Which case held: the absolute case.** Because the baseline diagnostic-bearing count is zero and
the post-change diagnostic-bearing count is also zero, set equality against the baseline *is* the
absolute condition AC-6's final sentence states. AC-6's diagnostic clause is therefore discharged
absolutely, not as non-regression, which is the stronger of the two cases § Notes rule 2 anticipates.

## Clause-by-clause verification of AC-6 as written

| AC-6 clause | Evidence | Satisfied |
| --- | --- | --- |
| "The file is byte-identical to its base-branch version (still 497 lines)" | `unowned-file-identity`: recomputed SHA-256 `a3c35259…` equals the `P0-T11` value; line count 497 | **yes** |
| "both call sites at `:452` and `:468` compile unchanged against the new `IDisposable` return type" | `<FEATURE>/evidence/regression-testing/pass-after-compile.2026-08-27T10-58.md` and `<FEATURE>/evidence/qa-gates/msbuild-analyzers.2026-08-27T11-13.md`: zero `error CS` lines anywhere, with the file unmodified | **yes** |
| "both `SetThemeDark_FromNormal_SelectsDarkNormalTheme` and `SetThemeLight_FromNormal_SelectsLightNormalTheme` pass" | `quickfiler-test-run`: both listed as Passed | **yes** |
| "No analyzer diagnostic is raised at either call site under toolchain steps 2 and 3" | `unowned-file-diagnostics-comparison`: diagnostic-bearing count 0 in both logs, both before and after; and both Phase 3 logs contain zero `warning CS` / `error CS` lines anywhere | **yes** |

Every clause AC-6 states is satisfied by evidence produced in this run.

## The sub-condition that did not hold literally, stated in full

`P4-T2`'s acceptance condition asks that, for each of the two source logs, "the final match count
equals the corresponding baseline count **and** the final line set is identical to the baseline line
set after redaction". The first half held on all four token-and-log combinations. The second half —
byte-exact line-set identity — held for the `UiThread.cs` token in both logs but **did not hold** for
the `QfcItemController.FocusAndThemeTests.cs` token in either log.

The reason is structural rather than behavioural, and it was recorded as a predicted hazard in the
`P0-T10` baseline artifact **before** any code change was made, so it is a disclosed outcome. At
MSBuild's default verbosity, every line in the log that contains the string
`QfcItemController.FocusAndThemeTests.cs` is a compiler invocation line — the `csc.exe` command line
and its `BuildResponseFile` echo — and each such line enumerates the compiling project's entire
source-file set. `P1-T2` adds two `<Compile Include>` entries to `QuickFiler.Test.csproj`, so the line
that happens to contain the token necessarily grows, without the file the token names being touched
in any way.

`P4-T2` computed the symmetric difference at token granularity to establish exactly that:

```
TOKEN_DIFF_COUNT=2
=> Controllers\QfcItemController.UiThreadDispatcherFixture.cs
=> Controllers\QfcItemController.UiThreadDispatcherFixtureTests.cs
```

The difference is exactly the two source files this feature added and nothing else. No token was
removed, and no third token was added. The 123-character length delta on each of the four affected
lines is accounted for entirely by those two path strings.

## Why AC-6 is checked off despite that

The failed sub-condition is a comparison of **compiler-invocation text**. AC-6 makes no claim about
compiler-invocation text; its claims are about the file's bytes, its call sites compiling, its two
tests passing, and diagnostics. All four are independently satisfied and each is evidenced above.
Three separate facts establish that no diagnostic regression occurred:

1. The diagnostic-bearing subset is empty on both sides of all four comparisons.
2. Both Phase 3 logs contain zero lines matching `error CS` or `warning CS` anywhere, and both report
   5 warnings and 0 errors — the same counts as the Phase 0 baselines.
3. `P4-T1` proves the file is byte-identical to its Phase 0 state, so no diagnostic could have been
   introduced into it.

The byte-exact line-set comparison is a proxy the plan chose for the diagnostic condition. It is a
poor proxy for the reason above, and this artifact records the shortfall explicitly rather than
treating it as satisfied. The check-off rests on the criterion's own text being met, not on the proxy.

## Result

`- [ ] **AC-6 …` changed to `- [x] **AC-6 …` in
`docs/features/active/quickfiler-test-uithread-dispatcher-493/spec.md`. Only the checkbox changed.
