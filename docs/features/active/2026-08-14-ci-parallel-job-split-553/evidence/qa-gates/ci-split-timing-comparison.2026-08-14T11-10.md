# Post-Split Timing vs Measured Baseline — Issue #553

- Timestamp: 2026-08-14T11-10 (local) / 2026-08-14T15:10Z (UTC)
- Task: [P4-T6]
- Measurement of record: run **31812508684** (the [P4-T4] post-probe green run)
- Comparison denominator: **444s**, from
  `evidence/baseline/ci-sequential-baseline.2026-08-14T13-05.md`

Command (identical collection method to the baseline capture):

```
gh api repos/drmoisan/TaskMaster/actions/runs/31812508684/jobs
```

EXIT_CODE: 0

## Per-job timings — measurement of record (run 31812508684, head `ad28ea81`)

| Job | Runner | Started | Completed | Duration |
| --- | --- | --- | --- | --- |
| `actionlint / actionlint` | ubuntu-latest | 15:02:37Z | 15:03:11Z | 34s |
| `format-check / Verify formatting` | windows-latest | 15:02:38Z | 15:05:03Z | 145s |
| `build-nullable / Build with nullable warnings treated as errors` | windows-latest | 15:02:37Z | 15:05:29Z | 172s |
| `build-analyzers / Build with analyzers and code style enforcement` | windows-latest | 15:02:38Z | 15:06:03Z | 205s |
| `mstest-coverage / Run MSTest suite with coverage` | windows-latest | 15:02:38Z | 15:09:50Z | 432s |

**Measured pipeline wall clock: 433s** — latest `completed_at` (15:09:50Z) minus
earliest `started_at` (15:02:37Z).

## Comparison against the 444s baseline

| Metric | Baseline (measured) | Measurement of record | Absolute delta | Percentage delta |
| --- | --- | --- | --- | --- |
| Pipeline wall clock | 444s | 433s | −11s | **−2.5%** |
| Billed `windows-latest` seconds | 444s | 954s | +510s | **+215%** (2.15x) |

Billed `windows-latest` seconds are the sum of the four Windows jobs
(145 + 172 + 205 + 432 = 954s). The `ubuntu-latest` actionlint job is excluded
from the Windows total; it ran concurrently in the baseline as well.

## Second green sample, and why both are reported

A single post-split run is a single sample, and so is the baseline. Two green
post-split runs of **byte-identical workflow files** were captured during this
work, and they differ substantially:

| Run | Head | actionlint | format | nullable | analyzers | mstest | Wall clock | Windows billed |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 31809697953 ([P3-T4] first green) | `0b016c81` | 36s | 131s | 188s | 186s | 259s | **259s** | 764s |
| 31812508684 ([P4-T4], measurement of record) | `ad28ea81` | 34s | 145s | 172s | 205s | 432s | **433s** | 954s |

Reduction against the 444s baseline is **41.7%** on the first sample and **2.5%**
on the second. Both are reported because selecting only the favourable sample
would misrepresent the result.

### Diagnosis of the variance: uniform runner slowness, not a structural change

Step-level comparison of the critical-path `mstest-coverage` job across the two
runs isolates where the additional 173s went:

| Step | Run 31809697953 | Run 31812508684 | Ratio |
| --- | --- | --- | --- |
| Set up job | 2s | 2s | 1.0x |
| Checkout repository | 40s | 44s | 1.1x |
| Setup MSBuild | 0s | 2s | — |
| Setup NuGet | 1s | 1s | 1.0x |
| Cache NuGet packages | 18s | 18s | 1.0x |
| Restore solution | 12s | 63s | 5.3x |
| Build solution | 91s | 149s | 1.6x |
| Run MSTest suite with coverage | 84s | 138s | 1.6x |
| Upload test results | 2s | 3s | 1.5x |
| Post-steps | 7s | 9s | 1.3x |

Every compute-bound step scaled by a similar factor (~1.6x for both the build and
the test execution), while the fixed-cost steps (job setup, cache restore,
checkout) were essentially unchanged. That signature is characteristic of a
slower hosted-runner instance or contended host I/O, not of a workflow-structure
difference: the two runs executed identical YAML, and the `Restore solution`
outlier (5.3x) points to NuGet network variance on top of the general slowdown.

**Conclusion drawn:** the split's structural benefit is real and is visible in
both samples — four gates that were strictly serial now run concurrently, so
wall clock is bounded by the single slowest gate rather than by their sum. The
*magnitude* of the wall-clock reduction is dominated by hosted-runner variance in
the MSTest job, which is the critical path in every sample. On a fast runner the
reduction approaches the spec's ~38% estimate; on a slow runner it can approach
zero, because the critical-path job alone can grow to the size of the entire
former serial pipeline. No numeric threshold is gated on this measurement, as the
plan specifies.

## Comparison against the spec's estimates

The spec's Expected Outcomes were explicitly labelled estimates, not
measurements. Recorded here for completeness:

| Metric | Spec target (estimate) | Spec worst case (estimate) | Measured, run A | Measured, run B |
| --- | --- | --- | --- | --- |
| Wall clock | ~277s | ~333s | 259s | 433s |
| Billed Windows seconds | ~763s | ~962s | 764s | 954s |

The billed-seconds estimates proved accurate to within ~1%: 764s measured against
a ~763s estimate on the first run, 954s against a ~962s worst case on the second.
The wall-clock estimates bracketed the first sample well (259s, slightly better
than the ~277s target) but did not anticipate the second sample's runner
variance. **This artifact is the measurement of record; the spec's figures remain
estimates.**

## Runner-environment parity (`.claude/rules/benchmark-baselines.md`)

Both sides of the comparison were captured on GitHub-hosted runners of the same
class, so the parity requirement is satisfied:

| | Baseline run 31749877507 | Measurement run 31812508684 |
| --- | --- | --- |
| Gate job runner label | `windows-latest` | `windows-latest` (×4) |
| Lint job runner label | `ubuntu-latest` | `ubuntu-latest` |
| Runner group | GitHub Actions (GitHub-hosted) | GitHub Actions (GitHub-hosted) |
| Collection method | `gh api .../runs/<id>/jobs` | `gh api .../runs/<id>/jobs` |

Neither measurement was taken on a developer workstation, so the rule's
prohibition on workstation-versus-runner comparison is not engaged. The rule's
`"Unknown processor"` rejection condition applies to BenchmarkDotNet baseline JSON
carrying a `HostEnvironmentInfo` block; this artifact is a workflow-run timing
record whose provenance is the run URL and collection command, and a sibling
provenance file is recorded alongside it
(`post-split-timing.provenance.json`) per the rule's provenance requirement.

## Cross-run contention caveat (research Q7)

Realized speedup can be eroded by account-level runner contention, which is not
determinable from repository data. Per-run demand is 5 concurrent jobs (4 ×
`windows-latest`, 1 × `ubuntu-latest`), below every GitHub plan's concurrency
ceiling, and in both samples all five jobs started within 2 seconds of each other
— so **neither sample was queued**, and queueing is excluded as the cause of the
run-B slowdown. The slowdown is within-job execution time.

## Addendum (2026-08-14T11-23): third green sample reclassifies run B as an outlier

The [P5-T15] pre-migration green run produced a third sample of the same
byte-identical pipeline, and it changes the interpretation above:

| Sample | Run | Head | Wall clock | vs 444s baseline | Windows billed |
| --- | --- | --- | --- | --- | --- |
| A | 31809697953 | `0b016c81` | 259s | −41.7% | 764s |
| B (measurement of record) | 31812508684 | `ad28ea81` | 433s | −2.5% | 954s |
| C | 31813885124 | `df49d208` | **245s** | **−44.8%** | 747s |

Sample C per-job: actionlint 30s, format-check 113s, build-nullable 193s,
build-analyzers 196s, mstest-coverage 245s.

**Two of three samples cluster at 245–259s (−41.7% to −44.8%); run B at 433s is
the outlier.** The median of the three is 259s, a 41.7% reduction. Sample C is
also the fastest of the three and slightly better than the spec's ~277s target
estimate.

Run B remains labelled the measurement of record because the plan designates the
[P4-T4] run as such, and it is retained rather than discarded: it is a real
observation of what this pipeline does on a slow runner, and it bounds the
downside honestly. But the central estimate of the split's benefit is better
represented by the cluster than by that single sample. The step-level diagnosis
above stands — run B's slowdown was uniform across compute-bound steps and is
attributable to hosted-runner variance, not to workflow structure.

Caveat retained: three samples is still a small number, all drawn within roughly
one hour, and the baseline remains a single sample. No numeric threshold is gated
on any of these figures.

## Acceptance ([P4-T6])

- Artifact exists with the comparison table populated from live API data; no
  placeholder values.
- Spec seeded-condition checkbox 7 ("Total wall-clock duration of the reworked
  pipeline is measured against the current sequential baseline and recorded as
  evidence") is checked off with this artifact as the evidence pointer.
