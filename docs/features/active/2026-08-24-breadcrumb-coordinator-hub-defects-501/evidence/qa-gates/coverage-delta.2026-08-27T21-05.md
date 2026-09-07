# Final QA — Coverage Delta (P7-T6, AC-32 second half) — TWO CONDITIONS FAILED, TASK LEFT UNCHECKED

Timestamp: 2026-08-27T21-05

Baseline artifact: `FF/evidence/baseline/baseline.cobertura.2026-08-27T20-01.xml`
Post-change artifact: `FF/evidence/qa-gates/postchange.cobertura.2026-08-27T21-00.xml`

**Outcome: sections (a) through (d) are all reported below with numeric values. Two of the four
acceptance conditions FAIL, both from one root cause: three new lines in the `AddItems` `false` branch
are not reachable by any test the plan authorizes. The task box is left UNCHECKED and escalated in the
final report. No figure in this artifact has been rounded, reframed, or omitted to make the gate pass.**

---

## (a) Baseline repository rates

Read from the baseline Cobertura root element:

| Metric | Raw | Absolute | Percentage |
| --- | ---: | --- | ---: |
| `line-rate` | 0.85138 | 54387 / 63881 | **85.13790%** |
| `branch-rate` | 0.792096 | 12927 / 16320 | **79.20956%** |

## (b) Post-change repository rates

Read from the post-change Cobertura root element:

| Metric | Raw | Absolute | Percentage |
| --- | ---: | --- | ---: |
| `line-rate` | 0.851369 | 54411 / 63910 | **85.13691%** |
| `branch-rate` | 0.792075 | 12933 / 16328 | **79.20749%** |

**Repository line-rate delta: 85.13691% - 85.13790% = -0.00099 percentage points.**

Required: at or above 0.00 percentage points. **CONDITION FAILED** by 0.001 pp.

Arithmetic of the shortfall: covered lines rose by 24 (54387 to 54411) while coverable lines rose by 29
(63881 to 63910). The 5-line gap is what pushes the ratio down. Three of those five are the uncovered
`AddItems` `false` branch identified in section (d).

## (c) Per-file rates

Counting method identical to P0-T15: aggregate every `<class>` element sharing a `filename`; line rate is
per-`<line>`-element; branch rate sums the `condition-coverage` numerators and denominators. Each of the
five owned files resolves to exactly one `<class>` element in both artifacts.

### Three single files not part of the SR-1 split

| File | Baseline line-rate | Post line-rate | Delta (pp) | Verdict |
| --- | ---: | ---: | ---: | --- |
| `QuickFiler/Viewers/BreadcrumbDropDownOpenCoordinator.cs` | 98.39080% (428/435) | 98.39080% (428/435) | **0.00** | at or above -0.50 |
| `QuickFiler/Viewers/BreadcrumbCoordinatorUpgradeLifetime.cs` | 99.05660% (210/212) | 100.00000% (217/217) | **+0.94340** | at or above -0.50 |
| `QuickFiler/Viewers/BreadcrumbMessengerHub.cs` | 100.00000% (449/449) | 100.00000% (473/473) | **0.00** | at or above -0.50 |

Branch rates for the same three, recorded for completeness:

| File | Baseline branch-rate | Post branch-rate | Delta (pp) |
| --- | ---: | ---: | ---: |
| `BreadcrumbDropDownOpenCoordinator.cs` | 92.39130% (170/184) | 91.48936% (172/188) | -0.90194 |
| `BreadcrumbCoordinatorUpgradeLifetime.cs` | 91.07143% (51/56) | 93.10345% (54/58) | +2.03202 |
| `BreadcrumbMessengerHub.cs` | 97.72727% (172/176) | 97.72727% (172/176) | 0.00 |

### COMBINED row for the SR-1 split pair

The pair is combined because P2-T1 relocated members between the two files, so a per-file comparison
across the split has no common denominator and the new file has no baseline row by construction.

- Baseline (the `QuickFiler/Viewers/BreadcrumbBridgeCoordinator.cs` row from P0-T15):
  **100.00000%** (504 covered / 504 coverable).
- Post-change, summed across both files:
  - `QuickFiler/Viewers/BreadcrumbBridgeCoordinator.cs`: 448 covered / 448 coverable
  - `QuickFiler/Viewers/BreadcrumbBridgeCoordinator.Suggestions.cs`: 74 covered / 80 coverable
  - Combined: (448 + 74) / (448 + 80) = 522 / 528 = **98.86364%**

**Combined split-pair line-rate delta: 98.86364% - 100.00000% = -1.13636 percentage points.**

Required: at or above -0.50 percentage points. **CONDITION FAILED** by 0.64 pp.

Both raw rates are cited above as the task requires when a delta is negative. Combined branch rate for the
pair, for completeness: baseline 87.50000% (147/168) to post 86.93182% (153/176), a delta of -0.56818 pp.

## (d) New and changed-code coverage

Method: for every line added or modified in the five owned production files according to
`git diff -U0 BASELINE_SHA -- <path>`, look up that line number in the post-change Cobertura and record
whether `hits` is greater than 0. Lines the collector does not instrument (blank lines, comments, XML doc
comments, declarations, braces) are excluded from both numerator and denominator, since coverage is
undefined for them.

| File | Added or modified new-file lines | Instrumented | Covered |
| --- | ---: | ---: | ---: |
| `QuickFiler/Viewers/BreadcrumbDropDownOpenCoordinator.cs` | 39 | 12 | 12 |
| `QuickFiler/Viewers/BreadcrumbCoordinatorUpgradeLifetime.cs` | 52 | 12 | 12 |
| `QuickFiler/Viewers/BreadcrumbMessengerHub.cs` | 36 | 14 | 14 |
| `QuickFiler/Viewers/BreadcrumbBridgeCoordinator.cs` | 0 (members removed only) | 0 | 0 |
| `QuickFiler/Viewers/BreadcrumbBridgeCoordinator.Suggestions.cs` | 111 | 48 | 45 |
| **TOTAL** | **238** | **86** | **83** |

**New/changed-line coverage: 83 / 86 = 96.51%.**

Required: at or above 90.00 percent. **CONDITION SATISFIED.**

### The three uncovered changed lines

```
UNCOVERED: QuickFiler/Viewers/BreadcrumbBridgeCoordinator.Suggestions.cs line 106
UNCOVERED: QuickFiler/Viewers/BreadcrumbBridgeCoordinator.Suggestions.cs line 107
UNCOVERED: QuickFiler/Viewers/BreadcrumbBridgeCoordinator.Suggestions.cs line 108
```

Source at those lines:

```csharp
105:            if (!ran)
106:            {
107:                _upgradeLifetime.Abandon(lease);
108:            }
```

This is the `AddItems` `false` branch — the I-502.4 / AC-14 skip path.

---

## Root cause of both failures: the `AddItems` skip path has no injectable seam

All three uncovered lines, and therefore both failing conditions, come from one gap. `AddItems` creates
its own lease and immediately guards on it:

```csharp
BreadcrumbUpgradeLease lease = _upgradeLifetime.BeginPopulation();
bool ran = _upgradeLifetime.RunSynchronous(lease, () => { ... });
```

The two statements are adjacent and nothing is injectable between them, so the lease is ALWAYS current
when `RunSynchronous` is entered and the `false` branch cannot be reached from any single-threaded test.
Every alternative route was checked and none reaches it:

| Attempted route | Why it does not reach the `false` branch |
| --- | --- |
| pass an already-cancelled token to `BeginPopulation` | `AddItems` calls the parameterless overload; the token is not injectable from a caller |
| `Invalidate()` the lifetime before calling `AddItems` | `AddItems` then calls `BeginPopulation`, which makes a NEW current lease; the guard passes |
| dispose the lifetime first | `BeginPopulation` throws `ObjectDisposedException` at `:62`; control never reaches `RunSynchronous` |
| make `_router.AddItems(items)` throw | `RunSynchronous`'s `catch` fires (Abandon + rethrow), not the `false` branch |
| reflect `_current` / `_generation` between the two statements | no seam exists between two adjacent statements; this is precisely the case research section 5.4 option 3 records as "does not actually work" |

This is exactly the seam problem SR-5 solved for `SetSuggestions` by adding
`internal void SetSuggestionsCore(...)`. **No equivalent `AddItemsCore` seam is authorized by any task in
this plan.** Adding one would be a production API change outside the plan's scope, and it would also
change `Suggestions.cs`'s line count and invalidate the already-recorded P4-T6, P6-T1 and P6-T4 evidence.
The atomic-execution contract forbids performing work no task describes, so no seam was added.

## What would fix it, for the plan revision

Covering those three lines flips BOTH failing conditions in one step:

- Combined split pair would become (448 + 77) / (448 + 80) = 525 / 528 = **99.43182%**, a delta of
  -0.56818 pp. Still marginally outside -0.50, so the split-pair threshold would additionally need either
  the two remaining uncovered `Suggestions.cs` lines covered or an explicit tolerance for the relocation.
- Repository line-rate would become 54414 / 63910 = **85.14161%**, a delta of **+0.00371 pp**, satisfying
  the at-or-above-0.00 requirement.

The minimal plan delta is a task adding `internal void AddItemsCore(IReadOnlyList<string> items, BreadcrumbUpgradeLease lease)`
alongside the existing `SetSuggestionsCore`, plus one test in the already-owned
`QuickFiler.Test/Viewers/BreadcrumbBridgeCoordinatorSupersessionTests.cs` (140 lines, 360 lines of
headroom, no new file and no new project-file line required) driving it with a superseded lease.

## Summary of the four acceptance conditions

| Condition | Required | Observed | Verdict |
| --- | --- | --- | --- |
| all four sections recorded with numeric values | yes | yes | SATISFIED |
| repository line-rate delta | at or above 0.00 pp | **-0.00099 pp** | **FAILED** |
| each of four per-file deltas | at or above -0.50 pp | 0.00, +0.94340, 0.00, **-1.13636** | **FAILED** (the combined split pair) |
| new/changed-line coverage | at or above 90.00% | **96.51%** | SATISFIED |
