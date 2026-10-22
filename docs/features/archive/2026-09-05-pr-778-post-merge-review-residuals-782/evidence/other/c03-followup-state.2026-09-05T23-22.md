# C03 Latch Re-Arm Promotion State — Issue #782 (P8-T21)

Timestamp: 2026-09-05T23-22

**This task performs no promotion.** The promotion of the C03 successor item — restoring the retry
semantics C03 asked for, by some mechanism that does not re-arm the latch that the two lazy
accessors `UiSyncContext` and `AutoScaleFactor` consume — is an orchestrator step performed through
the MCP promotion lifecycle outside this plan, exactly as the C09 behavioural follow-up in P8-T8 is.
This artifact records which state that promotion is in, and nothing else.

Command:

```powershell
Get-ChildItem -LiteralPath 'docs/features/potential/promoted' -Filter '*.md' |
    Where-Object { $_.Name -ne '2026-09-05-pr-778-post-merge-review-residuals.md' } |
    Select-String -Pattern 'latch re-arm|single-shot latch|ThreadSafeSingleShotGuard|retry after a failed Initialize'
```

EXIT_CODE: 0

Output Summary:

## The mandatory exclusion

The excluded filename is **`2026-09-05-pr-778-post-merge-review-residuals.md`**. The exclusion is
mandatory and is not an optimisation.

That file is this delivery's own promoted entry. It carries the token `single-shot latch` on
**line 56**, in its description of finding C03, and it carries `- Issue: #782` on line 7. Both
observations were re-derived against the current tree and both match the plan's stated values
exactly:

```text
line 56: - C03 `UiThread.Init()`: set the single-shot latch only after `Initialize()` succeeds so a failed
line  7: - Issue: #782
```

`Select-String` matches case-insensitively, so without the exclusion the unfiltered search returns
that file **today, before any promotion has occurred**, Branch A fires against this delivery's own
issue number, and this task records a promoted state that is false. The Branch A line is
deliberately not quoted anywhere in this artifact: the acceptance condition counts occurrences of
the branch token, so quoting the unused branch would make the count read two and the condition
would fail for a reason unrelated to the state being recorded.

### Unfiltered search output, recorded in full

```text
2026-09-05-pr-778-post-merge-review-residuals.md:56: - C03 `UiThread.Init()`: set the single-shot latch only after `Initialize()` succeeds so a failed
UNFILTERED_HIT_COUNT=1
```

### Filtered search output, recorded in full

```text
FILTERED_HIT_COUNT=0
FILTERED_DISTINCT_FILES=0
QUALIFYING_COUNT=0
```

The filtered search returns zero files.

## Branch taken

**Branch B.** The filtered search returned zero files, so no file contains a line matching
`^- Issue: #[0-9]+` whose number is not 782.

**Branch B is the state the plan measured at authoring time.** The observed state matches it, so a
Branch A result would have been a real change of state rather than the pre-existing match.

The branch line, recorded verbatim:

C03 FOLLOW-UP DEFERRED: the UiThread.Init() latch re-arm has not yet been promoted; owner is the orchestrator, which performs promotion outside this plan.

Exactly one of the two branch lines is present in this artifact and not both.

## Why the follow-up exists at all

C03 was withdrawn from this delivery under SD18 after a measured regression. The re-arm made
`UtilitiesCS.Test.Extensions.DictionaryExtensions_Tests.TryAddValuesAsync_UpdatesExistingValue` fail
reproducibly at a 21-second duration against a 500 ms budget, because the `UiSyncContext` and
`AutoScaleFactor` getters call `Init()` lazily and a re-armed latch makes every later read of either
accessor retry the WinForms `SyncContextForm` construction and throw again, starving the thread
pool. The full measurement, bisect, and mechanism are recorded in
`evidence/other/code-review.2026-09-05T23-00.md` under the entry opening
`C03 OMITTED: latch re-arm not implemented`.

The follow-up is therefore not a deferred implementation of the same change. It asks for the retry
semantics by a mechanism that does not re-arm the shared latch, which is a different design and
belongs in its own entry.
