## P2-T6: Coverage delta, baseline vs. final

**The root `<coverage line-rate="..." branch-rate="...">` attributes on the final Cobertura document
are unreliable and were NOT used for this comparison.** They report 70.74%/59.35%, a implausible ~15pp
drop for a 5-file, single-parameter-removal change with zero test-count regression (6949/6949 passing
in both runs, identical XML file size to within 13 bytes). This matches the already-filed, still-open
defects `#529` ("Cobertura package rates not recomputed") and `#530` ("Cobertura merged class methods
incomplete") — the coverage merge/post-processing step does not reliably recompute the root summary
rate attributes after a merge, even though the underlying per-line hit data is correct.

**The `lines-covered`/`lines-valid` and `branches-covered`/`branches-valid` totals are reliable** (they
are direct sums over the `<line>` elements, not a separately-maintained rate attribute) and were used
instead, per the same method used elsewhere in this session's parallel-run coverage measurements:

| Metric | Baseline (P0-T7) | Final (P2-T5) | Delta |
|---|---|---|---|
| Line coverage | 55088 / 64506 = 85.3963% | 55093 / 64506 = 85.4041% | **+5 lines, +0.0078pp** |
| Branch coverage | 13173 / 16576 = 79.4703% | 13171 / 16576 = 79.4582% | −2 branches, −0.0121pp |

**Disposition: no regression.** Line coverage improved marginally (the discard statement's removal
plus the test rewrite). The branch delta (−2 out of 16576, −0.012pp) is within normal run-to-run noise
for a 6949-test parallelized suite and is not attributable to this change: no conditional/branching
logic was added, removed, or touched by any of the five edited files — the change is a parameter
removal, three call-site argument drops, and one test rewrite, none of which contain a branch.
