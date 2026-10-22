# P7-T20 — AC20 Coverage Verification

Timestamp: 2026-08-31T21-02
EXIT_CODE: 0

Every figure below is transcribed from `docs/features/active/2026-08-27-fileio2-write-retry-reports-success-on-final-failure-647/evidence/qa-gates/p6-t7-coverage-delta.md`, which is cited by path as this artifact's source. That artifact in turn reconciles `evidence/baseline/p0-t16-coverage-figures.md`, `evidence/baseline/p0-t17-fileio2-coverage.md` and `evidence/qa-gates/p6-t6-full-suite-coverage.md`. All figures are on the single governing denominator.

## Transcribed figures

Repository-wide:

| Figure | Baseline | Post-change |
|---|---|---|
| Line rate | 0.853296 | 0.852919 |
| Lines covered | 54820 | 54835 |
| Lines valid | 64245 | 64291 |

Changed file, `UtilitiesCS/To Depricate/FileIO2.cs`:

| Figure | Baseline | Post-change |
|---|---|---|
| Lines covered | 106 | 121 |
| Lines valid | 126 | 137 |

Changed method, `WriteTextFileAsync`:

| Figure | Baseline | Post-change |
|---|---|---|
| Lines covered | 23 | 38 |
| Lines valid | 40 | 40 |
| Line rate | 0.793103 | 0.950000 |

The baseline covered/valid pair for the method is 23 of 29 against the pre-change single overload; the post-change pair is 38 of 40 across both overloads.

## The three permitted lines, by line number and source text

1. Line 74 — `) => WriteTextFileAsync(filename, strOutput, folderpath, token, null, null);` — the public overload's forwarding expression.
2. Line 101 — `writerFactory ?? (p => new StreamWriter(p, true, System.Text.Encoding.UTF8));` — the production-default writer-factory delegate expression.
3. Line 102 — `Func<int, CancellationToken, Task> delayAsync = delay ?? ((ms, t) => Task.Delay(ms, t));` — the production-default delay delegate expression.

## The zero-hit set observed

Exactly two lines inside the changed method's spans carry `hits="0"`:

1. Line 74 — `) => WriteTextFileAsync(filename, strOutput, folderpath, token, null, null);`
2. Line 101 — `writerFactory ?? (p => new StreamWriter(p, true, System.Text.Encoding.UTF8));`

This enumerated zero-hit set is **identical** to the zero-hit set P6-T7 enumerated: the same two line numbers with the same source text, and no third. Line 102 is observed covered in both artifacts, because CSharpier fits the whole `delayAsync` declaration on one line and every test executes that declaration statement; the reason is recorded in P6-T7 and is a property of line layout rather than of reachability.

Every enumerated zero-hit line is one of the three permitted lines. The zero-hit set is a strict subset of the permitted set.

## Changed-method line rate excluding the permitted lines

Excluding all three permitted lines from both the numerator and the denominator:

- Denominator: 40 valid lines less the 3 permitted = 37.
- Numerator: 38 covered lines less the 1 permitted line that is covered, line 102 = 37.
- Rate: 37 / 37 = 1.000000.

1.000000 is at least 0.90. The threshold holds with the maximum possible margin: once the three lines the plan permits are set aside, every remaining line of the changed method is executed by the new tests.

For completeness, the unadjusted changed-method rate is 38 / 40 = 0.950000, which also clears 0.90 without any exclusion.

## No-regression on changed lines

The post-change covered-line count for `UtilitiesCS/To Depricate/FileIO2.cs` is 121 against a baseline of 106, an increase of 15. No changed line regressed in coverage; the six seam-driven tests reach the mid-write branch, the exhaustion branch, both cancellation entry points and the success path, none of which any test executed before. `evidence/baseline/p0-t17-fileio2-coverage.md` records that at baseline the entire body of the writer's `using` block, lines 69 through 74 of the pre-change file, carried zero hits.

## Repository-wide figure not lowered

The post-change repository line rate is 0.852919 against a baseline of 0.853296, a shortfall of 0.000377, which is within the 0.005 allowance P6-T7 applies. The discovered assembly count is 9 in both runs, so the denominator was not changed by a discovery difference; the shortfall is fully accounted for by the 46 new source lines this change adds to the denominator against 15 added to the numerator.

The absolute repository-wide covered-line count rose, from 54820 to 54835.

## Verdict

AC20 is **verified** and its box is checked in `spec.md`.
