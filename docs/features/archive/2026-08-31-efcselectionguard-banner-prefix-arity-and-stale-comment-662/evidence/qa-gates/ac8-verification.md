# AC8 Verification (P2-T19)

Timestamp: 2026-09-01T16-56

Sources: the TRX produced by P2-T7
(`evidence/qa-gates/p2-t7/quickfiler-postchange.trx`) and the TRX produced by
P2-T8 (`evidence/qa-gates/p2-t8/utilitiescs-postchange.trx`), compared against
the baseline figures recorded by P0-T11 and P0-T12.

EXIT_CODE: 0 (both post-change runs)

Output Summary:

## The four figures

| Assembly | Baseline `passed` | Baseline `failed` | Post-change `passed` | Post-change `failed` |
|---|---|---|---|---|
| `QuickFiler.Test` | 1286 (P0-T11) | 0 (P0-T11) | 1287 (P2-T7) | 0 (P2-T7) |
| `UtilitiesCS.Test` | 4783 (P0-T12) | 0 (P0-T12) | 4783 (P2-T8) | 0 (P2-T8) |

Post-change `<Counters ... />` lines:

```
QuickFiler.Test:   <Counters total="1287" executed="1287" passed="1287" failed="0" error="0" timeout="0" aborted="0" inconclusive="0" passedButRunAborted="0" notRunnable="0" notExecuted="0" disconnected="0" warning="0" completed="0" inProgress="0" pending="0" />
UtilitiesCS.Test:  <Counters total="4783" executed="4783" passed="4783" failed="0" error="0" timeout="0" aborted="0" inconclusive="0" passedButRunAborted="0" notRunnable="0" notExecuted="0" disconnected="0" warning="0" completed="0" inProgress="0" pending="0" />
```

## The two comparisons

**Comparison 1 — `QuickFiler.Test`.** The P2-T7 TRX carries `failed="0"`, which
satisfies the `failed="0"` requirement. Its `passed` value of 1287 is not lower
than the 1286 recorded for the same assembly in the P0-T11 baseline artifact; it
is higher by exactly one, which is the new test added by P1-T7. **PASS.**

**Comparison 2 — `UtilitiesCS.Test`.** The P2-T8 TRX carries `failed="0"`, which
satisfies the `failed="0"` requirement. Its `passed` value of 4783 is not lower
than the 4783 recorded for the same assembly in the P0-T12 baseline artifact; it
is equal, because this change adds no test to this assembly. **PASS.**

## The pre-existing-failure branch does not arise

P2-T19's closing clause states that if either baseline artifact recorded a
non-zero `failed` value, this gate cannot pass as written. Neither did: P0-T11
recorded `failed="0"` and P0-T12 recorded `failed="0"`. No discrepancy is
recorded, AC8 is not left unchecked, and no `REMEDIATION-REQUIRED` arises from
this task.

## Behavioural claim AC8 makes

AC8 asserts that no behavioural change reaches `FolderSuggestionTree.IsBanner`,
`BreadcrumbRowBuilder`, or `EfcFormController.IsBannerRow`. The evidence is
that both owned assemblies pass in full with zero failures:

- `FolderSuggestionTree.IsBanner` moved from a local `private const string
  BannerPrefix = "===="` to `BreadcrumbRowBuilder.BannerPrefix`, whose value is
  the identical `"===="`. Every `FolderSuggestionTree` test in
  `UtilitiesCS.Test` still passes.
- `BreadcrumbRowBuilder` was not modified at all; AC5b's zero-diff gate is the
  direct evidence, and this run is the behavioural corroboration.
- `EfcFormController.IsBannerRow` was not modified; the only edit to
  `EfcFormController.cs` is the three-line `SelectedFolder` comment. The merged
  test `IsSelectableFolder_AndIsBannerRow_ClassifyThreeAndFourEqualsRowsIdentically`
  still passes, per AC7.

## Note on the P2-T7 re-run

The P2-T7 artifact records two runs. Run 1 hung and was terminated, producing no
TRX and therefore contributing no `failed` figure to this gate; before
termination it showed 15 timeout failures, all in `WinFormsPumpHost` and
`UiThread` dispatcher tests unrelated to the four edited files. Run 2, the run
whose counters are read above, passed all 1287 tests including all 15 of those.
The P0-T11 baseline ran the byte-identical command before any edit existed and
also passed all of them, which is what establishes those failures as an
environmental scheduling flake rather than a regression caused by this change.

**AC8 checked off in `issue.md`.**
