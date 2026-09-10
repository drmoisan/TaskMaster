# Coverage comparison and AC15 record (issue #826, [P7-T6])

Timestamp: 2026-09-09T19-43

BRANCH: REACHABLE

The value above is copied from the [P1-T1] artifact.

Command: this task performs no new measurement. It compares the figures recorded by [P0-T9]
(`<FEATURE>/evidence/baseline/baseline-tests-coverage.md`) against those recorded by [P7-T4]
(`<FEATURE>/evidence/qa-gates/p7-t4-tests-coverage.md`) under the plan's D15 comparability rule. Both
figures were produced by the identical command form against the identical nine assemblies with the
identical filter, so the comparison is like for like.

EXIT_CODE: 0

## Root-level figures

| Figure | Baseline ([P0-T9]) | Post-change ([P7-T4]) | Direction |
|---|---|---|---|
| `line-rate` | 0.8611544074577819 | 0.8613286095646392 | up |
| `lines-valid` | 201454 | 201534 | up by 80 |
| `lines-covered` | 173483 | 173587 | up by 104 |

Absolute percentage difference in `lines-valid`: **0.0397 percent** (80 / 201454).

## Comparability verdict

**COMPARABLE.** The `lines-valid` difference of 0.0397 percent is well inside the D15 threshold of 5
percent, so the root `line-rate` comparison is the reported figure and the fallback branch is not taken.

Under that branch the required condition is that the post-change root `line-rate` is not lower than the
baseline root `line-rate`. Observed: 0.8613286095646392 is **higher** than 0.8611544074577819. The
condition holds.

## Line-coverage floor

`.claude/rules/general-unit-test.md` sets a `>= 85%` line-coverage floor. Reported against the measured
post-change figure: **86.1329 percent**, which is above the floor. The baseline figure was 86.1154
percent, so the floor was met before and after and the change moved it upward.

## Per-file figures for `UtilitiesCS/OutlookObjects/Table/OlTableExtensions.TableAccess.cs`

| Figure | Baseline | Post-change |
|---|---|---|
| per-file line rate (pooled across the nine matching `<class>` elements) | 0.846975 (238 of 281) | 0.907473 (255 of 281) |
| `<GetTableInViewAsync>d__32` state-machine class `line-rate` | 0.6533333333333333 | 0.88 |

The distinct-line denominator is 281 in both runs, so the rise is a genuine coverage gain and not a
denominator artefact.

## Changed-line hit counts

Both runs located the changed lines at observation time with
`Select-String -CaseSensitive -SimpleMatch` against the file under measurement, never from a line number
in the plan. Both reported lines 96 and 115. Both runs applied the identical pooling rule, in which each
line contributes once from `class/lines` and once from `method/lines`.

| Changed line | BaselineChangedLineHits | PostChangeChangedLineHits | Decrease? |
|---|---|---|---|
| 96 — the statement in the `else` branch of `catch (TaskCanceledException)` | 0 | 2 | no |
| 115 — the statement in `catch (TimeoutException)` | 2 | 2 | no |

Neither post-change count is lower than its baseline counterpart, so **no changed line lost coverage**,
and **both changed lines carry a non-zero post-change hit count**, which is the expected outcome the plan
states and which AC15's operative demand requires.

The distribution is exactly what plan decision D2 derives. Line 115 was already covered at the baseline
by feature 825's live test `GetTableInViewAsync_TimeoutRetry_UsesCallerTimeoutMsNotLiteral2000`, so
AC15's premise that both changed lines are uncovered before the change is partly false for that line;
the greater-than-or-equal comparison plus the non-zero post-change requirement is what discharges AC15's
real content. Line 96 moved from 0 to 2 through the second test method [P2-T2] authored,
`GetTableInViewAsync_TimeoutSourceThrowsTaskCanceled_EntersCancelCatchElseAndRetriesOnce`.

Neither post-change count is 0, so no gap needs reporting to the orchestrator.

## Item 1 moved no figure, as predicted

The 33 item-1 files all compile into `*.Test.dll` assemblies, which the coverage pipeline excludes from
instrumentation, so neither numerator nor denominator moves on their account. The observed movement in
`lines-valid` is +80 and in `lines-covered` is +104; both are attributable to the item-2 production edit
and the newly covered branch rather than to the test-file deletions, and the net effect on the root rate
is an increase.

## No weakening anywhere in this change

Stated explicitly, as the plan requires:

- **No coverage exclusion was added.** The only exclusion mechanism used anywhere in this feature is the
  repository's own pre-existing, committed `coverage.config`, which names seven third-party module
  patterns (`Deedle`, `FSharp`, `Castle.Core`, `FluentAssertions`, `Moq`, `Microsoft.Testing`, `MSTest`)
  and no first-party production path. It was not created or modified by this feature; it appears in
  neither the anchored diff nor the porcelain span.
- **No coverage threshold was changed.** The `>= 85%` line floor and the `>= 75%` branch floor in
  `.claude/rules/general-unit-test.md` are untouched, as is every figure in
  `.claude/rules/quality-tiers.md`. No file under `.claude/rules/` appears in this feature's diff.
- **No `[ExcludeFromCodeCoverage]` attribute was added anywhere in this change.**
- **No analyzer severity was lowered.** `dotnet_diagnostic.RS0030.severity` remains `suggestion`, proven
  by [P3-T3] and [P4-T3].

Output Summary: verdict `COMPARABLE` at a 0.0397 percent `lines-valid` difference. Root line coverage
rose from 86.1154 to 86.1329 percent, above the 85 percent floor. The per-file rate for the table-access
file rose from 0.846975 to 0.907473. Both changed lines carry a non-zero post-change hit count and
neither decreased. No exclusion, threshold change or `[ExcludeFromCodeCoverage]` attribute was added
anywhere. AC15 is satisfied.
