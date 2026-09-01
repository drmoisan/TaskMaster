# P6-T7 — Coverage Delta and Threshold Verification

Timestamp: 2026-08-31T20-52
Iteration: 1

This task runs no command; it reconciles the figures produced by P0-T16, P0-T17 and P6-T6. Every figure below is on the single governing denominator — the Koverage project-allowlist denominator that `ConvertTo-KoverageCoberturaXml` produces — and none is taken from any runner's console output. Both the baseline and the post-change derivation took the same branch: the on-disk document already carried a `<sources>` element in each case.

## Repository-wide figures

| Figure | Baseline (P0-T16) | Post-change (P6-T6) | Delta |
|---|---|---|---|
| Line rate | 0.853296 | 0.852919 | -0.000377 |
| Lines covered | 54820 | 54835 | +15 |
| Lines valid | 64245 | 64291 | +46 |
| Branch rate | 0.793089 | 0.792754 | -0.000335 |
| Branches covered | 13059 | 13063 | +4 |
| Branches valid | 16466 | 16478 | +12 |

Discovered assembly count, baseline run (P0-T15): 9.
Discovered assembly count, post-change run (P6-T6): 9.

**No-regression evaluation.** The post-change repository line rate is lower than the baseline line rate by 0.000377, which is not more than the 0.005 allowance. The gate holds.

The observed shortfall is recorded together with both assembly counts, as the plan's execution rule requires, because a changed assembly count changes the denominator and is the first cause to rule out. Both counts are 9, so the denominator was not changed by a discovery difference; the 46-line increase in `lines-valid` is the change's own new source lines entering the denominator, which is the intended behavior of a whole-file denominator.

Cause of the shortfall, stated concretely rather than attributed to the tolerance: the change adds 46 lines to the denominator and 15 to the numerator, so the ratio falls very slightly even though absolute coverage rose. The plan's stated purpose for the 0.005 allowance is to absorb numerator nondeterminism across runs of a class-level-parallel suite; the observed 0.000377 is an order of magnitude below that allowance and is fully explained by the denominator growth without needing the nondeterminism argument at all.

## Per-file figures for `UtilitiesCS/To Depricate/FileIO2.cs`

| Figure | Baseline (P0-T17) | Post-change | Delta |
|---|---|---|---|
| Lines covered | 106 | 121 | +15 |
| Lines valid | 126 | 137 | +11 |
| Line rate | 0.841270 | 0.883212 | +0.041942 |

**Changed-file no-regression evaluation.** The post-change covered-line count for this file, 121, is not lower than the baseline covered-line count of 106. The gate holds. Every one of the 15 additional repository-wide covered lines is in this file, which is the expected result of replacing one ~10-second locked-fixture test with six seam-driven tests that reach branches no test previously executed.

## Changed-method figures for `WriteTextFileAsync`

| Figure | Baseline (P0-T17) | Post-change | Delta |
|---|---|---|---|
| Lines covered | 23 | 38 | +15 |
| Lines valid | 29 | 40 | +11 |
| Line rate | 0.793103 | 0.950000 | +0.156897 |

Derivation, identical to the one fixed in P0-T17: the subset of the class-level `<line>` entries whose `number` falls inside the source-line span of a `WriteTextFileAsync` declaration, with spans located by scanning for the declaration form and brace-matching forward. Against post-change source the scan locates both overloads, spans 69 through 74 for the public forwarder and 83 through 150 for the seam overload.

Recorded for continuity with P0-T17, where it was 0: `METHOD_ELEMENT_UNION_COUNT` is now 1. The public overload is no longer `async`, so it is emitted as an ordinary named method and does appear as a `<method>` element. The seam overload still does not, because its state machine's lines are merged into the parent class's class-level list without a named method entry. The span-based derivation covers both and is therefore the one used at both ends.

## Zero-hit lines in the changed method

Exactly two lines inside the two spans carry `hits="0"`:

```
line  74 | ) => WriteTextFileAsync(filename, strOutput, folderpath, token, null, null);
line 101 |     writerFactory ?? (p => new StreamWriter(p, true, System.Text.Encoding.UTF8));
```

Both are permitted lines. The three permitted lines the plan enumerates, with line numbers and source text:

1. **Public overload's forwarding expression** — line 74, `) => WriteTextFileAsync(filename, strOutput, folderpath, token, null, null);`. Observed zero-hit.
2. **Production-default writer-factory delegate expression** — line 101, `writerFactory ?? (p => new StreamWriter(p, true, System.Text.Encoding.UTF8));`. Observed zero-hit.
3. **Production-default delay delegate expression** — line 102, `Func<int, CancellationToken, Task> delayAsync = delay ?? ((ms, t) => Task.Delay(ms, t));`. Observed **covered**, hits greater than zero.

The observed zero-hit set is therefore a strict subset of the permitted set, so the condition that every enumerated zero-hit line is one of the three permitted lines holds.

Why the third permitted line is covered while the second is not, recorded so the asymmetry is not mistaken for a measurement error: both are coalescing expressions whose lambda operand is never invoked when a test supplies its own delegate. CSharpier fits the whole `delayAsync` declaration onto one line, so line 102 carries the declaration statement itself, which every test executes, and the line registers a hit. The `createWriter` declaration is too long for one line, so it wraps, and line 101 carries only the coalescing expression's right operand. That line is reached only when `writerFactory` is null, which no test does. The difference is one of line layout, not of reachability: the `new StreamWriter(...)` lambda body and the `Task.Delay(ms, t)` lambda body are equally unreached by the suite.

UNCOVERED_PUBLIC_FORWARDER: line 74, `) => WriteTextFileAsync(filename, strOutput, folderpath, token, null, null);`

This line is permitted because P5-T7 deleted the only test that called the public overload and P7-T16 requires every remaining `WriteTextFileAsync` call in `UtilitiesCS.Test/HelperClasses/FileIO2_Tests.cs` to bind the seam overload through the `writerFactory:` named argument. No test in the suite invokes the public overload, so its forwarding expression is unreachable from the tests by construction rather than by omission.

## Summary of gate outcomes

| Gate | Required | Observed | Holds |
|---|---|---|---|
| Repository line-rate shortfall | at most 0.005 | 0.000377 | Yes |
| `FileIO2.cs` covered lines | at least 106 | 121 | Yes |
| Zero-hit lines in changed method | all among the 3 permitted | 2 of the 3 | Yes |
