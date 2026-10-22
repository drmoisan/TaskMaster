# AC12 coverage delta and thresholds (Issue #824, task P5-T11)

Timestamp: 2026-09-09T16-12

Command: comparison of `evidence/baseline/coverage-classes-baseline.2026-09-09T15-16.md` with
`evidence/qa-gates/coverage-classes-post-change.2026-09-09T16-10.md`, plus a changed-line
computation that intersects the added line numbers from
`pwsh -NoProfile -Command 'Set-Location "<worktree-root>"; git diff --unified=0 HEAD -- "UtilitiesCS/NewtonsoftHelpers/SDIL Reader/ILGlobals.cs"'`
with the `line` elements of the matching `class` element in `coverage/post-change.cobertura.xml`.

The diff is anchored on `HEAD` rather than on the merge base, per the adaptation recorded in
`evidence/other/executor-deviations.2026-09-09T15-28.md`. For this particular path the two anchors
are equivalent: `UtilitiesCS/NewtonsoftHelpers/SDIL Reader/ILGlobals.cs` does not appear in the
inherited listing enumerated by `evidence/baseline/base-inertness.2026-09-09T15-19.md`, so every
added line the diff reports is one this run introduced.

EXIT_CODE: 0

## Per-class comparison — the AC12 gate

Comparison is on the per-`class` attributes read directly from the two Cobertura documents, which is
the comparable axis. Raw and post-processed root totals are not compared.

| Class | Metric | Baseline | Post-change | Delta | Post >= baseline |
|---|---|---|---|---|---|
| `SDILReader.ILGlobals` | `line-rate` | 0.9459459459459459 | 0.95 | +0.0040540540540541 | **yes** |
| `SDILReader.ILGlobals` | `branch-rate` | 0.875 | 0.875 | 0 | **yes** |
| `SDILReader.MethodBodyReader` | `line-rate` | 0.9732620320855615 | 0.9732620320855615 | 0 | **yes** |
| `SDILReader.MethodBodyReader` | `branch-rate` | 0.8947368421052632 | 0.8947368421052632 | 0 | **yes** |

All four post-change values are greater than or equal to their baseline values. AC12's
non-regression criterion is satisfied.

`SDILReader.ILGlobals` improved. Its instrumented line count rose from 37 to 40 as the fix added the
static constructor and the `RunClassConstructor` body, and the uncovered count stayed at 2, so the
ratio moved from 35/37 to 38/40. `SDILReader.MethodBodyReader` is unchanged on both metrics, which
is consistent with the file being byte-for-byte unmodified as P4-T6 verified.

### The residual uncovered lines, named rather than summarised

| Document | Uncovered line numbers in `SDILReader.ILGlobals` | Source |
|---|---|---|
| Baseline | 142, 143 | the `{` and the `throw new Exception("Invalid OpCode.");` of the invalid-opcode guard |
| Post-change | 160, 161 | the same two statements, shifted by the edit |

The residual is the same unreachable path on both sides. `spec.md` records that this path is
unreachable over the fixed `System.Reflection.Emit.OpCodes` set and is deliberately not tested; the
exhaustive AC4 test instead asserts the `(value & 0xff00) == 0xfe00` classification for every
multi-byte opcode, which is the same condition that guards the throw. The fix neither introduced nor
removed uncovered lines.

## Changed-code coverage

Added line numbers reported by the anchored diff for
`UtilitiesCS/NewtonsoftHelpers/SDIL Reader/ILGlobals.cs`: **41** lines —

```
6, 115, 116, 117, 118, 119, 120, 121, 122, 123, 124, 125, 126, 127, 128, 129, 130,
133, 134, 135, 136, 137, 138, 139, 141, 142, 155, 163,
167, 168, 169, 170, 171, 172, 173, 174, 175, 176, 177, 178, 179
```

Intersected with the instrumented `line` elements of the `SDILReader.ILGlobals` class element in
`coverage/post-change.cobertura.xml` (40 distinct instrumented line numbers):

| Measure | Value |
|---|---|
| Added lines that are instrumented | **9** |
| Instrumented added lines with `hits` greater than 0 | **9** |
| Instrumented added lines with `hits` equal to 0 | **0** |
| Added lines that are not instrumented | 32 |

**Changed-code coverage: 9 / 9 = 100 %.**

The requirement that every changed line that is instrumented and uncovered be enumerated by line
number with its source text is satisfied with an **empty list**: there is no such line. The count is
stated explicitly as zero rather than left implicit, so the empty enumeration is a recorded
observation rather than an omission.

The 32 non-instrumented added lines are the `using` directive, the XML documentation comment blocks,
the local-variable declarations that the compiler folds, and brace-only lines. Cobertura emits no
`line` element for them, so they are outside the coverage denominator by construction rather than by
exclusion.

## Document-level line-rate against both thresholds, per plan D10

| Figure | Baseline | Post-change |
|---|---|---|
| `/coverage/@line-rate` | 0.856204 (85.6204 %) | 0.856241 (85.6241 %) |
| `/coverage/@branch-rate` | 0.79807 (79.807 %) | 0.798129 (79.8129 %) |

| Threshold | Source | Post-change value | Met |
|---|---|---|---|
| Line coverage >= 80 % | `CLAUDE.md`; enforced by `scripts/vscode/Invoke-MSTestWithCoverage.Threshold.ps1:52-54` | 85.6241 % | **yes** |
| Line coverage >= 85 % | `.claude/rules/general-unit-test.md`, `.claude/rules/quality-tiers.md` | 85.6241 % | **yes** |
| Branch coverage >= 75 % | `.claude/rules/general-unit-test.md`, `.claude/rules/quality-tiers.md` | 79.8129 % | **yes** |

The document-level line-rate is reported against both the 80 figure and the 85 figure without either
being lowered, weakened, or deleted. The two figures disagree with each other in the repository's own
policy documents; plan D10 requires that conflict to be reported rather than silently resolved, and
it is reported here. On this tree the post-change value clears both, so the conflict is not
load-bearing for this change.

Both document-level rates increased slightly, which is consistent with the fix adding covered lines
and the new tests exercising them.

## No threshold, exclusion, or severity was changed

**No coverage threshold, exclusion list, or analyzer severity was lowered, weakened, or deleted
anywhere in this change.** Specifically:

- `scripts/vscode/Invoke-MSTestWithCoverage.Threshold.ps1` was not modified;
- `coverage.config` was not modified, and no production file was added to any exclusion list;
- `.editorconfig` was not modified and no analyzer severity was changed;
- no `[ExcludeFromCodeCoverage]` attribute was added to any type or member;
- no policy document under `.claude/rules/` was modified.

The P4-T8 and P5-T3 scope gates corroborate this independently: the only source files in this run's
change footprint are the two the plan's Scope section names, and neither is a configuration or
policy file.
